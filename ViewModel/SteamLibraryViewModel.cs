using System.Collections.ObjectModel;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.ViewModel {
  public class SteamLibraryViewModel : ObservableObject {
    private string _displayName;
    private bool _hideListViewColumnHeader;

    public string DisplayName {
      get { return _displayName; }
      set { UpdateProperty(ref _displayName, value, nameof(DisplayName)); }
    }

    public bool HideListViewColumnHeader {
      get { return _hideListViewColumnHeader; }
      set { UpdateProperty(ref _hideListViewColumnHeader, value, nameof(HideListViewColumnHeader)); }
    }

    public ObservableCollection<SteamGameViewModel> SteamGames { get; } =
      new ObservableCollection<SteamGameViewModel>();
  }
}