using System.Collections.ObjectModel;

namespace SteamLibraryExplorer.ViewModel {
  public class MainPageViewModel : ObservableObject {
    private string _statusText;

    public string StatusText {
      get { return _statusText; }
      set {
        _statusText = value;
        RaisePropertyChangedEvent(nameof(StatusText));
      }
    }

    public ObservableCollection<SteamGameViewModel> Games { get; } = new ObservableCollection<SteamGameViewModel>();
  }
}
