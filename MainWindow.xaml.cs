using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPFDoist.Model;
using WPFDoist.ViewModel;

namespace WPFDoist {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		[DllImport("user32")]
		public static extern bool SetForegroundWindow(IntPtr hWnd);
		public MainWindow() {
			Icon = Properties.Resources.todoist.ToImageSource();

			InitializeComponent();
			vm = DataContext as MainViewModel;
			Loaded += MainWindow_Loaded;
			Closing += MainWindow_Closing;

		}

		private MainViewModel vm;
		void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			if (Settings.GetSettingB(SET_NAMES.MinimizeOnExit)) {
				e.Cancel = true;
				last_max = WindowState == WindowState.Maximized;
				if (Settings.GetSettingB(SET_NAMES.MinimizeOnExit))
					Hide();
				else
					WindowState = WindowState.Minimized;
				
			}
		}
		void MainWindow_Loaded(object sender, RoutedEventArgs e) {
			vm.OnLoad(browserMain);
			vm.UnhideWindow += (o, ex) => unhide();
		}
		private void MainWindow_StateChanged(object sender, System.EventArgs e) {
			if (WindowState == WindowState.Maximized)
				last_max = true;
			if (WindowState == WindowState.Normal)
				last_max = false;
		}
		private bool last_max = true;
		private void unhide() {
			Show();
			WindowState = last_max ? WindowState.Maximized : WindowState.Normal;
			SetForegroundWindow(new WindowInteropHelper(this).Handle);
		}
	}
}
