using System.Collections.ObjectModel;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.ViewModel {
  public class SteamLibraryViewModel : ObservableObject {
    private string _displayName;

    public string DisplayName {
      get { return _displayName; }
      set { UpdateProperty(ref _displayName, value, nameof(DisplayName)); }
    }

    public ObservableCollection<SteamGameViewModel> SteamGames { get; } =
      new ObservableCollection<SteamGameViewModel>();
  }
}