using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WPFDoist.Model;

namespace WPFDoist.ViewModel {
	public class SettingsViewModel : OurViewModelBase {

		public SettingsViewModel() {
			
		}
		IEnumerable<Settings.Setting> sets;
		public void reset() {
			sets = Settings.GetSettingsCopy().Where(a => a.name.ToString().Contains("HotKey") == false);
			string_settings = sets.Where(a => a.type == SETTING_TYPE.STRING).ToArray();
			bool_settings = sets.Where(a => a.type == SETTING_TYPE.BOOL).ToArray();
			hot_key_alt = Settings.GetSettingB(SET_NAMES.HotKeyUseAlt);
			hot_key_cntrl = Settings.GetSettingB(SET_NAMES.HotKeyUseControl);
			hot_key_shift = Settings.GetSettingB(SET_NAMES.HotKeyUseShift);
			hot_key_char = Settings.GetSettingS(SET_NAMES.HotKeyKey);
		}
		public string hot_key_char {
			get { return _hot_key_char; }
			set { Set(() => hot_key_char, ref _hot_key_char, value); }
		}
		private string _hot_key_char;

		public bool hot_key_alt {
			get { return _hot_key_alt; }
			set { Set(() => hot_key_alt, ref _hot_key_alt, value); }
		}
		private bool _hot_key_alt;

		public bool hot_key_cntrl {
			get { return _hot_key_cntrl; }
			set { Set(() => hot_key_cntrl, ref _hot_key_cntrl, value); }
		}
		private bool _hot_key_cntrl;

		public bool hot_key_shift {
			get { return _hot_key_shift; }
			set { Set(() => hot_key_shift, ref _hot_key_shift, value); }
		}
		private bool _hot_key_shift;

		
		public IEnumerable<Settings.Setting> bool_settings {
			get { return _bool_settings; }
			set { Set(() => bool_settings, ref _bool_settings, value); }
		}
		private IEnumerable<Settings.Setting> _bool_settings;

		public IEnumerable<Settings.Setting> string_settings {
			get { return _string_settings; }
			set { Set(() => string_settings, ref _string_settings, value); }
		}
		private IEnumerable<Settings.Setting> _string_settings;

		public async void save() {
			Settings.SetSetting(SET_NAMES.HotKeyUseAlt, hot_key_alt);
			Settings.SetSetting(SET_NAMES.HotKeyUseControl, hot_key_cntrl);
			Settings.SetSetting(SET_NAMES.HotKeyUseShift, hot_key_shift);
			Settings.SetSetting(SET_NAMES.HotKeyKey, hot_key_char.ToUpper());
			foreach (var set in sets)
				Settings.SetSetting(set.name, set.cur_value);
			await Settings.SaveSettings();
			var res = MessageBox.Show("Most settings will only take effect on restart, do you want to exit now?", "Exit Now?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
			if (res == MessageBoxResult.Yes)
				Environment.Exit(0);
		}
	}
}
