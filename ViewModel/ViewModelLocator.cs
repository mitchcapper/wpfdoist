using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace WPFDoist.ViewModel {
	class ViewModelLocator {
		private static ViewModelLocator _instance;
		public static ViewModelLocator instance {
			get { return _instance ?? (_instance = new ViewModelLocator()); }
		}
		public ViewModelLocator()
		{
			ServicePointManager.DefaultConnectionLimit = 25;
			bool first = _instance == null;
			_instance = this;
			ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
			if (first) {
				SimpleIoc.Default.Register<MainViewModel>();
				SimpleIoc.Default.Register<SettingsViewModel>();
			}
		}

		public MainViewModel Main {
			get {
				return ServiceLocator.Current.GetInstance<MainViewModel>();
			}
		}
		public SettingsViewModel Settings {
			get {
				return ServiceLocator.Current.GetInstance<SettingsViewModel>();
			}
		}
	}
}
