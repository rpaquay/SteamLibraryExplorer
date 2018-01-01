using System;
using System.Windows.Input;

namespace SteamLibraryExplorer.ViewModel {
  public class RelayExecuteCommand : ICommand {
    private readonly Action<object> _callback;

    public RelayExecuteCommand(Action<object> callback) {
      _callback = callback;
    }

    public bool CanExecute(object parameter) {
      //return parameter is T;
      return true;
    }

    public void Execute(object parameter) {
      _callback(parameter);
    }

    public event EventHandler CanExecuteChanged;

    protected virtual void OnCanExecuteChanged() {
      CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
  }
}