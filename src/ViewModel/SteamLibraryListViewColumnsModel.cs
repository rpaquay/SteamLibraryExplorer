using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.ViewModel {
  public class SteamLibraryListViewColumnsModel : ObservableObject {
    private bool _showWorkshopFiles;
    private double _gameDisplayNameColumnWidth = 260;
    private double _gameAcfFileColumnWidth = 240;
    private double _gameLocationColumnWidth = 240;
    private double _gameSizeOnDiskColumnWidth = 80;
    private double _gameFileCountColumnWidth = 80;
    private double _workshopAcfFileColumnWidth = 240;
    private double _workshopLocationColumnWidth = 240;
    private double _workshopSizeOnDiskColumnWidth = 80;
    private double _workshopFileCountColumnWidth = 80;

    public bool ShowWorkshopFiles {
      get { return _showWorkshopFiles; }
      set { UpdateProperty(ref _showWorkshopFiles, value, nameof(ShowWorkshopFiles)); }
    }

    public double GameDisplayNameColumnWidth {
      get { return _gameDisplayNameColumnWidth; }
      set { UpdateProperty(ref _gameDisplayNameColumnWidth, value, nameof(GameDisplayNameColumnWidth)); }
    }

    public double GameAcfFileColumnWidth {
      get { return _gameAcfFileColumnWidth; }
      set { UpdateProperty(ref _gameAcfFileColumnWidth, value, nameof(GameAcfFileColumnWidth)); }
    }

    public double GameLocationColumnWidth {
      get { return _gameLocationColumnWidth; }
      set { UpdateProperty(ref _gameLocationColumnWidth, value, nameof(GameLocationColumnWidth)); }
    }

    public double GameSizeOnDiskColumnWidth {
      get { return _gameSizeOnDiskColumnWidth; }
      set { UpdateProperty(ref _gameSizeOnDiskColumnWidth, value, nameof(GameSizeOnDiskColumnWidth)); }
    }

    public double GameFileCountColumnWidth {
      get { return _gameFileCountColumnWidth; }
      set { UpdateProperty(ref _gameFileCountColumnWidth, value, nameof(GameFileCountColumnWidth)); }
    }
    public double WorkshopAcfFileColumnWidth {
      get { return _workshopAcfFileColumnWidth; }
      set { UpdateProperty(ref _workshopAcfFileColumnWidth, value, nameof(WorkshopAcfFileColumnWidth)); }
    }

    public double WorkshopLocationColumnWidth {
      get { return _workshopLocationColumnWidth; }
      set { UpdateProperty(ref _workshopLocationColumnWidth, value, nameof(WorkshopLocationColumnWidth)); }
    }

    public double WorkshopSizeOnDiskColumnWidth {
      get { return _workshopSizeOnDiskColumnWidth; }
      set { UpdateProperty(ref _workshopSizeOnDiskColumnWidth, value, nameof(WorkshopSizeOnDiskColumnWidth)); }
    }

    public double WorkshopFileCountColumnWidth {
      get { return _workshopFileCountColumnWidth; }
      set { UpdateProperty(ref _workshopFileCountColumnWidth, value, nameof(WorkshopFileCountColumnWidth)); }
    }
  }
}