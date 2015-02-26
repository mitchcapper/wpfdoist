using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using Hardcodet.Wpf.TaskbarNotification;
using WPFDoist.Model;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace WPFDoist.ViewModel {
	public class MainViewModel : OurViewModelBase {

		private TaskbarIcon icon;
		private void SetupSysTray() {
			icon = new TaskbarIcon();
			icon.Icon = Properties.Resources.todoist;
			icon.Visibility = Visibility.Visible;
			icon.ContextMenu = new ContextMenu();
			icon.LeftClickCommand = GetCmd(unhide);
			icon.DoubleClickCommand = GetCmd(unhide);
			var itm = new MenuItem { Header = "Open" };
			itm.Click += (o, e) => unhide();
			icon.ContextMenu.Items.Add(itm);
			itm = new MenuItem { Header = "Sync" };
			itm.Click += (o, e) => browser.Sync();
			icon.ContextMenu.Items.Add(itm);
			itm = new MenuItem { Header = "Reload" };
			itm.Click += (o, e) => browser.Reload();
			icon.ContextMenu.Items.Add(itm);

			if (!Settings.GetSettingB(SET_NAMES.HideOptions)) {
				itm = new MenuItem { Header = "Options" };
				itm.Click += (o, e) => ShowOptions();
				icon.ContextMenu.Items.Add(itm);
			}
			itm = new MenuItem { Header = "Exit" };
			itm.Click += (o, e) => { icon.Visibility = Visibility.Collapsed; icon.Dispose(); Environment.Exit(0); };
			icon.ContextMenu.Items.Add(itm);
		}

		private void ShowOptions() {
			var win = new SettingsWindow();
			win.ShowDialog();
		}

		public string title {
			get { return _title; }
			set { Set(() => title, ref _title, value); }
		}
		private string _title="WPFDoist";
		
		public MainViewModel() {
		}
		private BrowserManager browser;
		public readonly string content_oulook_indent = "[[outlook=id3=";
		public string GetEmailIdFromContentStr(String content) {
			var itm = content;
			var end_pos = itm.IndexOf(",");
			if (end_pos == -1)
				return null;
			var id_str = itm.Substring(content_oulook_indent.Length, end_pos - content_oulook_indent.Length);
			while (id_str.Length % 4 != 0)
				id_str += "=";
			var str = Convert.FromBase64String(id_str.Replace("-", "+").Replace("_", "/"));
			var decoded = Encoding.UTF8.GetString(str, 0, str.Length);
			if (!decoded.StartsWith("id="))
				return null;
			var id_end = decoded.IndexOf(";");
			if (id_end == -1)
				return null;
			return decoded.Substring(3, id_end - 3);
		}
		private void EmailClicked(object sender, string link) {
			String id = GetEmailIdFromContentStr("[[outlook=" + link + ", ");
			OpenEmail(id);

		}
		public static void OpenEmail(String id) {
			var app = new Outlook.Application();
			var ns = app.GetNamespace("MAPI");
			var item = ns.GetItemFromID(id) as Outlook.MailItem;
			item.Display(false);
		}

		public void OnLoad(WebBrowser web_browser) {
			
			browser = new BrowserManager(web_browser);
			browser.FixBrowserMode();
			browser.TitleChanged += (o, e) => title = e;
			browser.EmailClicked += EmailClicked;
			RegShortcutKey();

			browser.Loaded();
			SetupSysTray();
		}

		public EventHandler UnhideWindow;
		private void unhide() {
			if (UnhideWindow != null)
				UnhideWindow(this, null);
		}
		private void handle_keyboard_shortcut(object sender, KeyPressedEventArgs e) {
			unhide();
			browser.AddTask();
		}

		private HotKeyboardHook kb_shortcut;
		private void RegShortcutKey() {
			kb_shortcut = new HotKeyboardHook();
			kb_shortcut.KeyPressed += handle_keyboard_shortcut;
			var setting_key = Settings.GetSettingS(SET_NAMES.HotKeyKey);
			if (String.IsNullOrWhiteSpace(setting_key))
				return;

			try {
				var key = (System.Windows.Forms.Keys)System.Windows.Forms.Keys.Parse(typeof(System.Windows.Forms.Keys), setting_key[0].ToString().ToUpper());
				HotKeyboardHook.ModifierKeys m_keys = 0;
				if (Settings.GetSettingB(SET_NAMES.HotKeyUseAlt))
					m_keys |= HotKeyboardHook.ModifierKeys.Alt;
				if (Settings.GetSettingB(SET_NAMES.HotKeyUseShift))
					m_keys |= HotKeyboardHook.ModifierKeys.Shift;
				if (Settings.GetSettingB(SET_NAMES.HotKeyUseControl))
					m_keys |= HotKeyboardHook.ModifierKeys.Control;

				kb_shortcut.RegisterHotKey(m_keys, key);
			} catch (Exception e) {
				var res = MessageBox.Show("Unable to register hot key its probably already running (or regged to another program), are you sure you want to continue launch\n" + e.Message, "Already Running Continue?", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
				if (res != MessageBoxResult.Yes)
					Environment.Exit(1);
			}

		}

	}
}
