using System;
using System.Windows.Input;

namespace SteamLibraryExplorer.ViewModel {
  public class RelayExecuteCommand<T> : ICommand {
    private readonly Action<T> _callback;

    public RelayExecuteCommand(Action<T> callback) {
      _callback = callback;
    }

    public bool CanExecute(object parameter) {
      //return parameter is T;
      return true;
    }

    public void Execute(object parameter) {
      _callback((T)parameter);
    }

    public event EventHandler CanExecuteChanged;

    protected virtual void OnCanExecuteChanged() {
      CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
  }
}