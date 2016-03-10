using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace WPFDoist.Model {
	public enum SETTING_TYPE {STRING,BOOL};
	
	
	public enum SET_NAMES { MinimizeOnExit, HideWhenMinimized, AdditionalJS, AdditionalCSS, DisableRecurringTaskFullComplete, HideTodoistSettings, DisableContextMenu, RemovePeopleAssign, HideOptions, HotKeyUseAlt,HotKeyUseShift,HotKeyUseControl,HotKeyKey,
		JSDebug, OldSearchBehavior
	};
	public static class Settings {
		public class SaveSetting {
			public string name { get; set; }
			public string value { get; set; }
		}
		public class Setting {
			public SET_NAMES name { get; set; }
			public string desc { get; set; }
			public SETTING_TYPE type { get; set; }

			public bool is_multiline {get {
					return name == SET_NAMES.AdditionalCSS || name == SET_NAMES.AdditionalJS;
				}
			}
			public int height {
				get {
					return name == SET_NAMES.AdditionalCSS || name == SET_NAMES.AdditionalJS ? 60 : 30;
				}
			}

			public string default_value { get; set; }
			public string cur_value { get; set; }
			public bool bool_value {
				get { return bool.Parse(cur_value); }
				set { cur_value = value.ToString(); }
			}

			public Setting Clone() {
				return (Setting)this.MemberwiseClone();
			}
		}
		public static IEnumerable<Setting> GetSettingsCopy() {
			
			return settings.Select(a=>a.Value.Clone()).ToArray();
        }
		private static Dictionary<SET_NAMES, Setting> settings = new Dictionary<SET_NAMES, Setting>();
		private static void AddSetting(SET_NAMES name, String desc, bool default_value) {
			AddSetting(name, desc, SETTING_TYPE.BOOL, default_value.ToString());
		}
		private static void AddSetting(SET_NAMES name, String desc, string default_value) {
			AddSetting(name, desc, SETTING_TYPE.STRING, default_value);
		}
		private static void AddSetting(SET_NAMES name, String desc,SETTING_TYPE type, string default_value) {
			settings[name] = new Setting { name = name, cur_value = default_value, desc = desc, default_value = default_value, type = type };
		}
		public static string GetUserAppDataPath() {
			var assm = Assembly.GetEntryAssembly();
			var at = typeof(AssemblyCompanyAttribute);
			var r = assm.GetCustomAttributes(at, false);
			var ct = ((AssemblyCompanyAttribute)(r[0]));
			var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			path += @"\" + ct.Company +  @"\WPFDoist";
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\WPFDoist\", false);
			if (key != null && ! Debugger.IsAttached) {//if debugging we dont need to use network folder
				var dir_obj = key.GetValue("DataDir");
				var dir = dir_obj == null ? null : dir_obj.ToString();
				if (! String.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
					path = dir;
			}
			return path;
		}
		private static string file_path = GetUserAppDataPath() + "\\program_settings.xml";
		public static async Task ObjDumpToFile(Object obj, StreamWriter file) {
			System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(obj.GetType());
			String text = "";

			if (obj.GetType() != Type.GetType("System.String")) {
				MemoryStream stream = new MemoryStream();
				x.Serialize(stream, obj);
				stream.Seek(0, SeekOrigin.Begin);
				using (StreamReader rd = new StreamReader(stream)) {
					text = rd.ReadToEnd();
				}
			} else
				text = (string)obj;
			await file.WriteAsync(text);
		}
		public  static void LoadSettings() {
			var sets = new SaveSetting[0];
			var serializer = new XmlSerializer(typeof(SaveSetting[]));
			if (File.Exists(file_path)) {
				try {
					using (StreamReader reader = new StreamReader(file_path)) {
						sets = (SaveSetting[])serializer.Deserialize(reader);
					}
					foreach (var set in sets) {
						SET_NAMES type;
						if (SET_NAMES.TryParse(set.name, out type)) {
							SetSetting(type, set.value);
						}
					}
				}catch(Exception){}
			}
		}

		public async static Task SaveSettings() {
			using (var writer = new StreamWriter(file_path)) {
				var objs = settings.Keys.Select(a => new SaveSetting { name = a.ToString(), value = GetSettingS(a) }).ToArray();
				await ObjDumpToFile(objs, writer);
			}
		}
		public static void SetSetting(SET_NAMES name, String value) {
			settings[name].cur_value = value;
		}
		public static void SetSetting(SET_NAMES name, bool value) {
			settings[name].bool_value = value;
		}
		public static string GetSettingS(SET_NAMES name) {
			return settings[name].cur_value;
		}
		public static bool GetSettingB(SET_NAMES name) {
			return settings[name].bool_value;
		}
		static Settings() {
			AddSetting(SET_NAMES.MinimizeOnExit,"Minimize don't close on close button",  false);
			AddSetting(SET_NAMES.HideWhenMinimized,"Hide in systray when minimized",  false);
			
//			AddSetting("KeyboardShortcut", SETTING_TYPE.STRING, "control+shift+s");
			AddSetting(SET_NAMES.AdditionalJS, "Additional javascript to inject on page", "");
			AddSetting(SET_NAMES.AdditionalCSS,"Additional css to inject on page", "");
			AddSetting(SET_NAMES.DisableRecurringTaskFullComplete,"Hitting shift while clicking a recurring task can complete it forever, this disables this from occuring", false);
			AddSetting(SET_NAMES.HideTodoistSettings, "Hide todoist settings icon",  false);
			AddSetting(SET_NAMES.OldSearchBehavior, "Search on enter for search bar (don't autocomplete)", false);
			AddSetting(SET_NAMES.DisableContextMenu,"Disable browser right click (still gets todoist context menu)",  true);
			AddSetting(SET_NAMES.RemovePeopleAssign, "Remove the people assignment option on tasks",  false);
			AddSetting(SET_NAMES.HideOptions,"Hide WPFDoist options (only way to re-enable is delete/edit settings file)", false);
			AddSetting(SET_NAMES.HotKeyUseAlt, "", false);
			AddSetting(SET_NAMES.HotKeyUseControl, "", true);
			AddSetting(SET_NAMES.HotKeyUseShift, "",  true);
			AddSetting(SET_NAMES.HotKeyKey, "", "T");
			AddSetting(SET_NAMES.JSDebug, @"Javascript/Plugin Debugging (alerts on errors, and writes c:\temp\js_debug.js)", false);
			LoadSettings();
		}
		//  KeyboardShortcut AdditionalJS AdditionalCSS DisableRepeatTaskFullComplete HideSettings DisableContextMenu
		//RemovePeopleAssign HideOptions
		
	}
}
