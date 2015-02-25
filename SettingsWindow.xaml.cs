using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WPFDoist.Model;
using WPFDoist.ViewModel;

namespace WPFDoist {
	/// <summary>
	/// Interaction logic for SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window {
		public SettingsWindow() {
			InitializeComponent();
			vm = DataContext as SettingsViewModel;
			vm.reset();
		}
		SettingsViewModel vm;
		

		private void ButtonSave_Click(object sender, RoutedEventArgs e) {
			vm.save();
			Close();
		}

		private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
			Close();
		}
	}
}
