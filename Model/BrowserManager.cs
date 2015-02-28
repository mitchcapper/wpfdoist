using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using mshtml;
using WPFDoist.ViewModel;

namespace WPFDoist.Model {
	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	[ComVisible(true)]
	public class ObjectForScriptingHelper {
		private BrowserManager manager;
		public ObjectForScriptingHelper(BrowserManager manager) {
			this.manager = manager;
		}
		public void logq(Object msg) {
			var msg_str = "JS Plugin log: " + msg.ToString();
			Debug.WriteLine(DateTime.Now + ": " + msg_str);
		}

		public void log(Object msg) {
			var msg_str = "JS Plugin log: " + msg.ToString();
			Debug.WriteLine(DateTime.Now + ": " + msg_str);
			if (Settings.GetSettingB(SET_NAMES.JSDebug))
				MessageBox.Show(msg_str);
		}
		public bool plugin_link(Object id, Object link) {
			try {
				var int_id = int.Parse(id.ToString());
				var link_str = Encoding.UTF8.GetString(Convert.FromBase64String(link.ToString()));
				var ext = ViewModelLocator.instance.Extensions.extensions.Single(a => a.id == int_id);
				ext.ext_as_link.HandleLink(link_str);
			} catch (Exception e) {
				if (Settings.GetSettingB(SET_NAMES.JSDebug))
					MessageBox.Show("Error running plugin of: " + e.Message);
			}
			return false;
		}
		public void openmail(Object link) {
			manager.FireEmailEvt(link.ToString());
		}
		public void onError(Object arg1, Object arg2, Object arg3) {
			var err_msg = "javascript err:  " + arg1 + " - " + arg2 + " - " + arg3;
			Debug.WriteLine(err_msg);
			if (Settings.GetSettingB(SET_NAMES.JSDebug))
				MessageBox.Show(err_msg);
			//m_Window.onError(arg1, arg2, arg3);
		}
	}

	public class BrowserManager {
		public BrowserManager(WebBrowser browser) {
			var exts = ViewModelLocator.instance.Extensions.extensions;//load the extensions now rather than when we need them
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
				EmailClicked(this, link);
		}
		private void wb_browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e) {
			HTMLDocument doc2 = browserMain.Document as HTMLDocument;
			if (TitleChanged != null)
				TitleChanged(this, doc2.title);
			String key = browserMain.Source.PathAndQuery;
			if (last_base_url == key)
				return;
			last_base_url = key;
			if (doc2.body == null && false)
				return;

			var script = (IHTMLScriptElement)doc2.createElement("SCRIPT");
			script.type = "text/javascript";
			script.text = GenerateJavascript();
			var css = (IHTMLStyleElement)doc2.createElement("STYLE");
			css.type = "text/css";
			css.styleSheet.cssText = GenerateCSS();
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





		private string GenerateJavascript() {
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
			if (!String.IsNullOrWhiteSpace(greater_str)) {
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
			string replace_str = @"
function wpf_replace_func_norm(ext_id){
	var args = get_call_arr(arguments,2);
	return wpf_funcs[ext_id].apply(null,args) + "" "";
}
function get_call_arr(args,start_at){
	var call_arr = new Array();//slice does't work on arguments
	for (var x = start_at; x < args.length;x++){
		call_arr.push(args[x]);
	}
	return call_arr;
}
function wpf_replace_func_link(ext_id,trash,full_link){
	var args = get_call_arr(arguments,2);//if you call another func args changes to that
	full_link = btoa(full_link);
	var display_str = (wpf_funcs[ext_id]) ? wpf_funcs[ext_id].apply(null,args) : args[0];
	var ret_str = ""<a href='#' onclick=\""return window.external.plugin_link("" + ext_id + "",'"" +full_link+""');\"">"" + display_str + ""</a> "";
	//window.external.logq(ret_str);
	return ret_str;
}
function wpf_replace_func_proto(ext_id,trash,full_link){
	var args = get_call_arr(arguments,2);
	full_link = full_link.replace(""&amp;"",""&"");
	if (wpf_proto_funcs[ext_id])
		full_link = wpf_proto_funcs[ext_id](full_link);
	full_link = btoa(full_link);
	return ""<a href='#' onclick=\""return window.location=atob('"" + full_link + ""');return false;\"">"" + wpf_funcs[ext_id].apply(null,args) + ""</a> "";
}
var wpf_funcs = new Object();
var wpf_proto_funcs = new Object();
";
			var func_str = "";
			var tag_str = "";
			var itms = ViewModelLocator.instance.Extensions.extensions;
			foreach (var itm in itms) {
				if (! String.IsNullOrWhiteSpace(itm.ext.regexp_replace_with_func_body))
					func_str += "wpf_funcs[" + itm.id + "] = function(){" + itm.ext.regexp_replace_with_func_body + "};\n";
				if (itm.type == EXT_TYPE.PROTO && !String.IsNullOrWhiteSpace(itm.ext_as_proto.override_url_func_body))
					func_str += "wpf_proto_funcs[" + itm.id + "] = function(){" + itm.ext_as_proto.override_url_func_body + "};\n";
				var func_name = "wpf_replace_func_norm";
				if (itm.type == EXT_TYPE.LINK)
					func_name = "wpf_replace_func_link";
				if (itm.type == EXT_TYPE.PROTO)
					func_name = "wpf_replace_func_proto";
				tag_str += "Formatter.tags_to_enable.push([/" + itm.ext.regexp_to_find + " /g," + func_name + ".bind(Formatter," + itm.id + "),/" + itm.ext.regexp_to_find + "$/g," + func_name + ".bind(Formatter," + itm.id + ")]);\n";//must have a space following or be the end of the task
			}
			replace_str += func_str;
			var right_click_disable = Settings.GetSettingB(SET_NAMES.DisableContextMenu) ? "document.oncontextmenu = function() {return false;}" : "";
			string js_str = @"
function externalError(errorMsg, document, lineNumber) {
  window.external.onError(errorMsg, document, lineNumber);
  return true;
 }
function obj_str(obj){
	return JSON.stringify(obj);
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
function LoadTest(){
	if (! window.Formatter){
		setTimeout(LoadTest, 300);
		return;
	}
	setTimeout( ReplaceFuncs, 3000 );" + "\n" + right_click_disable + "\n" + tag_str +
@"
	ReplaceFuncs();
	if (window.UserOnLoaded){
		window.UserOnLoaded();
	}
	window.external.logq('Fully Loaded finished');
}
function OurLoaded(){
     LoadTest();
};

function ReplaceFuncs(){
	if (DateBocks.magicDate2 == undefined){
" + remove_people_js + numbers_greater_than_js + @"
		
	} " + prevent_recurring_force_complete
+ @"
}
if (document.readyState === 'complete') {
	OurLoaded();
}else{
	if (window.addEventListener)
		window.addEventListener('load', OurLoaded, false);
	else
		 window.attachEvent('onload', OurLoaded);
}
" + replace_str + Settings.GetSettingS(SET_NAMES.AdditionalJS) + "\n";
			if (Settings.GetSettingB(SET_NAMES.JSDebug)) {
				try {
					File.WriteAllText(@"c:\temp\js_debug.js", js_str);
				} catch (Exception) { }
			}
			return js_str;
		}
		private string GenerateCSS() {
			var disable_setting_icon = Settings.GetSettingB(SET_NAMES.HideTodoistSettings) ? ".cmp_gear {display: none !important;}\n" : "";
			string css_str = disable_setting_icon + @"
#search_bar .input_q {
	width: 300px !important;
}
" + Settings.GetSettingS(SET_NAMES.AdditionalCSS) + "\n";
			return css_str;
		}

	}
}
