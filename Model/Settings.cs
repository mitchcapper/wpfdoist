using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WPFDoist.Model {
	public enum SETTING_TYPE {STRING,BOOL};
	
	
	public enum SET_NAMES { MinimizeOnExit, AdditionalJS, AdditionalCSS, DisableRecurringTaskFullComplete, HideTodoistSettings, DisableContextMenu, RemovePeopleAssign, HideOptions,SearchForNumbersGreaterThan, HotKeyUseAlt,HotKeyUseShift,HotKeyUseControl,HotKeyKey };
	public static class Settings {
		public class SaveSetting {
			public string name { get; set; }
			public string value { get; set; }
		}
		class Setting {
			public string desc;
			public SETTING_TYPE type;
			public string default_value;
			public string cur_value;
		}
		private static Dictionary<SET_NAMES, Setting> settings = new Dictionary<SET_NAMES, Setting>();
		private static void AddSetting(SET_NAMES name, String desc, SETTING_TYPE type, bool default_value) {
			AddSetting(name, desc, type, default_value.ToString());
		}
		private static void AddSetting(SET_NAMES name, String desc, SETTING_TYPE type, string default_value) {
			settings[name] = new Setting { cur_value = default_value, desc=desc,default_value = default_value, type = type };
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
			settings[name].cur_value = value.ToString();
		}
		public static string GetSettingS(SET_NAMES name) {
			return settings[name].cur_value;
		}
		public static bool GetSettingB(SET_NAMES name) {
			return bool.Parse(settings[name].cur_value);
		}
		static Settings() {
			AddSetting(SET_NAMES.MinimizeOnExit,"Minimize don't close on close button", SETTING_TYPE.STRING, false);
//			AddSetting("KeyboardShortcut", SETTING_TYPE.STRING, "control+shift+s");
			AddSetting(SET_NAMES.AdditionalJS, "Additional javascript to inject on page", SETTING_TYPE.STRING, "");
			AddSetting(SET_NAMES.AdditionalCSS,"Additional css to inject on page", SETTING_TYPE.STRING, "");
			AddSetting(SET_NAMES.DisableRecurringTaskFullComplete,"Hitting shift while clicking a recurring task can complete it forever, this disables this from occuring", SETTING_TYPE.BOOL, false);
			AddSetting(SET_NAMES.HideTodoistSettings, "Hide todoist settings icon", SETTING_TYPE.BOOL, false);
			AddSetting(SET_NAMES.DisableContextMenu,"Disable browser right click (still gets todoist context menu)", SETTING_TYPE.BOOL, true);
			AddSetting(SET_NAMES.RemovePeopleAssign, "Remove the people assignment option on tasks",SETTING_TYPE.STRING, false);
			AddSetting(SET_NAMES.HideOptions,"Hide WPFDoist options (only way to re-enable is delete/edit settings file", SETTING_TYPE.STRING, false);
			AddSetting(SET_NAMES.SearchForNumbersGreaterThan, "If a number is specified here you can search for numbers greater than this number (normally it treats numbers as days)", SETTING_TYPE.STRING, "999");
			AddSetting(SET_NAMES.HotKeyUseAlt, "", SETTING_TYPE.BOOL, false);
			AddSetting(SET_NAMES.HotKeyUseControl, "", SETTING_TYPE.BOOL, true);
			AddSetting(SET_NAMES.HotKeyUseShift, "", SETTING_TYPE.BOOL, true);
			AddSetting(SET_NAMES.HotKeyKey, "", SETTING_TYPE.BOOL, "T");
			LoadSettings();
#pragma warning disable 4014
			SaveSettings();
#pragma warning restore 4014
		}
		//  KeyboardShortcut AdditionalJS AdditionalCSS DisableRepeatTaskFullComplete HideSettings DisableContextMenu
		//RemovePeopleAssign HideOptions

	}
}
