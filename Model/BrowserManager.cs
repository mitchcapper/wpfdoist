using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Win32;
using mshtml;

namespace WPFDoist.Model {
	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	[ComVisible(true)]
	public class ObjectForScriptingHelper {
		private BrowserManager manager;
		public ObjectForScriptingHelper(BrowserManager manager) {
			this.manager = manager;
		}
		public void openmail(Object link) {
			manager.FireEmailEvt(link.ToString());
		}
		public void onError(Object arg1, Object arg2, Object arg3) {
			Debug.WriteLine("javascript err:  " + arg1 + " - " + arg2 + " - " + arg3);
			//m_Window.onError(arg1, arg2, arg3);
		}
	}

	public class BrowserManager {
		public BrowserManager(WebBrowser browser) {
			browserMain = browser;
            browserMain.Navigated += wb_browser_Navigated;
			helper = new ObjectForScriptingHelper(this);
			browserMain.ObjectForScripting = helper;
		}
		public void AddTask() {
			browserMain.InvokeScript("quickAdd");
		}
		public void Sync() {
			browserMain.InvokeScript("sync");
        }
		public void Loaded() {
			browserMain.Navigate("https://todoist.com/app");
			
		}
		public async void DoSearch(String search_str, bool allow_retry = true) {
			var err = false;
			try {
				browserMain.InvokeScript("do_search", search_str);
			} catch (Exception) {
				err = true;
			}
			if (err && allow_retry) {
					await Task.Delay(500);
					DoSearch(search_str, false);
				}
		}
        public async void Reload() {
			browserMain.Navigate("about:blank");
			await Task.Delay(300);
			browserMain.Navigate("https://todoist.com/app");

		}
		private string last_base_url;

		public EventHandler<string> TitleChanged;
		public EventHandler<string> EmailClicked;
		private WebBrowser browserMain;
		private ObjectForScriptingHelper helper;
		public void FireEmailEvt(String link) {
			if (EmailClicked != null)
				EmailClicked(this,link);
        }
		private void wb_browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e) {
			HTMLDocument doc2 = browserMain.Document as HTMLDocument;
			if (TitleChanged != null)
				TitleChanged(this, doc2.title);
			String key = browserMain.Source.PathAndQuery;
			if (last_base_url == key)
				return;
			last_base_url = key;
			string prevent_recurring_force_complete = Settings.GetSettingB(SET_NAMES.DisableRecurringTaskFullComplete) ? @"
//Can't use Agenda.completeItem as reference to function is bound early on so renaming doesnt work
	if (ItemsModel.complete2 == undefined){
		ItemsModel.complete2 = ItemsModel.complete;
		ItemsModel.complete = function (c, h) {
			h=undefined;
			return ItemsModel.complete2(c,h);
		};
	}
" + "\n" : "";
			string remove_people_js = Settings.GetSettingB(SET_NAMES.RemovePeopleAssign) ? "PeopleAssigner.render = function(){return null;};" + "\n" : "";
			string numbers_greater_than_js = "";
			var greater_str = Settings.GetSettingS(SET_NAMES.SearchForNumbersGreaterThan);
			if (! String.IsNullOrWhiteSpace(greater_str)) {
				numbers_greater_than_js = @"
		DateBocks.magicDate2 = DateBocks.magicDate;
		DateBocks.magicDate = function (j,a) {
			if (! isNaN(j) ){
				var num = parseInt(j);
				if (! isNaN(num) && num > " + greater_str + @" )
					return -1;
			}
			return DateBocks.magicDate2(j,a);
		};" + "\n";
			}
            string js_str = @"
function externalError(errorMsg, document, lineNumber) {
  window.external.onError(errorMsg, document, lineNumber);
  return true;
 }
 window.onerror = externalError;
function do_search(str){
	//str = str.replace(/ /g,'__'); 
    document.location.hash='#agenda/' + str;
}
function sync(){
	document.location.hash='#sync';
}
function quickAdd(){
	var add = '';
	add= (document.location.hash == '#quickAdd') ? '2' : '';
		
	document.location.hash='#quickAdd'+add;
}
function HandleUrl(url){
	window.external.HandleLink(url);
	return false;
}
function OurLoaded(){
     //disable the right mouse click menu
	setTimeout( ReplaceFuncs, 3000 );
     document.oncontextmenu = function() {return false;}" + 
 @"
	ReplaceFuncs();
	if (window.UserOnLoaded){
		window.UserOnLoaded();
	}
};
function ReplaceFuncs(){
	if (DateBocks.magicDate2 == undefined){
" + remove_people_js + numbers_greater_than_js + @"
		
	} " + prevent_recurring_force_complete
	+ @"
}
window.addEventListener('load', OurLoaded, false);
" + Settings.GetSettingS(SET_NAMES.AdditionalJS) + "\n";
			var disable_setting_icon = Settings.GetSettingB(SET_NAMES.HideTodoistSettings) ? ".cmp_gear {display: none !important;}\n" : "";
			string css_str = disable_setting_icon + @"
#search_bar .input_q {
	width: 300px !important;
}

" + Settings.GetSettingS(SET_NAMES.AdditionalCSS) + "\n";
			if (doc2.body == null && false)
				return;

			var script = (IHTMLScriptElement)doc2.createElement("SCRIPT");
			script.type = "text/javascript";
			script.text = js_str;
			var css = (IHTMLElement)doc2.createElement("STYLE");
			css.innerHTML = css_str;
			IHTMLElementCollection nodes = doc2.getElementsByTagName("head");
			foreach (var elem in nodes) {
				var head = (HTMLHeadElement)elem;
				head.appendChild((IHTMLDOMNode)script);
				head.appendChild((IHTMLDOMNode)css);
			}
		}

		public void FixBrowserMode() {
			try {
				RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true);
				if (key == null)
					key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION");
				key.SetValue("WPFDoist.exe", 11000, RegistryValueKind.DWord);
			} catch (Exception) { }
		}
		
	}
}
