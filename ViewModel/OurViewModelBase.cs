using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace WPFDoist.ViewModel {
	public class OurRelayCommand<T> : ICommand {
		public OurRelayCommand(Action<T> execute, T tag) {
			cmd = new RelayCommand<T>(execute, OurCanExecute);
			cmd.CanExecuteChanged += cmd_CanExecuteChanged;
			this.tag = tag;
		}
		private T tag;
		void cmd_CanExecuteChanged(object sender, EventArgs e) {
			if (CanExecuteChanged != null)
				CanExecuteChanged(sender, e);
		}
		private RelayCommand<T> cmd;

		private bool OurCanExecute(T tag) {
			return can_execute;
		}

		public bool can_execute {
			get { return _can_execute; }
			set {
				if (value == _can_execute)
					return;
				_can_execute = value;
				cmd.RaiseCanExecuteChanged();
			}
		}
		private bool _can_execute = true;

		public bool CanExecute(object parameter) {
			return cmd.CanExecute(tag);
		}

		public void Execute(object parameter) {
			cmd.Execute(tag);
		}

		public event EventHandler CanExecuteChanged;
	}

	public class OurViewModelBase :ViewModelBase {
		protected Dictionary<Action, OurRelayCommand<Action>> func_to_cmd;
		protected void NormalExecute(Action func) {
			func();
		}

		protected ICommand GetCmd(Action func, bool trash = false) {
			OurRelayCommand<Action> ret;
			if (func_to_cmd == null)
				func_to_cmd = new Dictionary<Action, OurRelayCommand<Action>>();
			if (func_to_cmd.TryGetValue(func, out ret))
				return ret;
				ret = new OurRelayCommand<Action>(NormalExecute, func);
			func_to_cmd[func] = ret;
			return ret;
		}

	}
}
