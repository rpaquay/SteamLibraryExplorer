using System.Collections.ObjectModel;

namespace SteamLibraryExplorer.ViewModel {
  /// <summary>
  /// View Model for the main window
  /// </summary>
  public class MainPageViewModel : ObservableObject {
    private string _statusText;

    public string StatusText {
      get { return _statusText; }
      set {
        _statusText = value;
        RaisePropertyChangedEvent(nameof(StatusText));
      }
    }

    public ObservableCollection<SteamGameViewModel> SteamGames { get; } = new ObservableCollection<SteamGameViewModel>();
  }
}
