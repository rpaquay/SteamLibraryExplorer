using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.ViewModel {
  public class SteamLibraryListViewColumnsModel : ObservableObject {
    private double _gameDisplayNameColumnWidth = 220;
    private bool _showWorkshopFiles;

    public double GameDisplayNameColumnWidth {
      get { return _gameDisplayNameColumnWidth; }
      set { UpdateProperty(ref _gameDisplayNameColumnWidth, value, nameof(GameDisplayNameColumnWidth)); }
    }

    public bool ShowWorkshopFiles {
      get { return _showWorkshopFiles; }
      set { UpdateProperty(ref _showWorkshopFiles, value, nameof(ShowWorkshopFiles)); }
    }
  }
}