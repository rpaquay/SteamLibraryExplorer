using System.Collections.ObjectModel;
using System.Reflection;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.ViewModel {
  /// <summary>
  /// View Model for the main window
  /// </summary>
  public class MainPageViewModel : ObservableObject {
    private string _statusText;
    private bool _isDiscoveringSteamFiles;
    private string _searchText;

    public string WindowTitle {
      get {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        if (version.Build == 0) {
          return $"Steam Library Explorer {version.Major}.{version.Minor}";
        }
        else {
          return $"Steam Library Explorer {version.Major}.{version.Minor}.{version.Build}";
        }
      }
    }

    public string StatusText {
      get { return _statusText; }
      set { UpdateProperty(ref _statusText, value, nameof(StatusText)); }
    }

    public bool IsDiscoveringSteamFiles {
      get { return _isDiscoveringSteamFiles; }
      set { UpdateProperty(ref _isDiscoveringSteamFiles, value, nameof(IsDiscoveringSteamFiles)); }
    }

    public string SearchText {
      get { return _searchText; }
      set { UpdateProperty(ref _searchText, value, nameof(SearchText)); }
    }

    public ObservableCollection<SteamLibraryViewModel> SteamLibraries { get; } =
      new ObservableCollection<SteamLibraryViewModel>();
  }
}