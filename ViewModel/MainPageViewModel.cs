using System.Collections.ObjectModel;
using System.Reflection;

namespace SteamLibraryExplorer.ViewModel {
  /// <summary>
  /// View Model for the main window
  /// </summary>
  public class MainPageViewModel : ObservableObject {
    private string _statusText;

    public string WindowTitle {
      get {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return string.Format("Steam Library Explorer {0}.{1}", version.Major, version.Minor);
      }
    }

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
