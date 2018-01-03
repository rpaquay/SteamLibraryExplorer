using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.ViewModel {
  public class SteamGameViewModel : ObservableObject {
    private string _listViewGroupHeader;
    private int _listViewGroupHeaderSortIndex;
    private string _displayName;
    private string _relativePath;
    private bool _isRelativePathValid;
    private string _acfFile;
    private long _sizeOnDisk;
    private long _fileCount;
    private Brush _relativePathColor;
    private Brush _acfFileColor;
    private Brush _sizeOnDiskColor;
    private Brush _fileCountColor;
    private RelayExecuteCommand _moveGameToLibraryCommand;
    private string _wokrshopAcfFile;
    private Brush _wokrshopAcfFileColor;
    private string _workshopRelativePath;
    private Brush _workshopRelativePathColor;
    private long _workshopSizeOnDisk;
    private long _workshopFileCount;

    /// <summary>
    /// Event raised when a "Move Game To Library" command is invoked
    /// The parameter is the library path.
    /// </summary>
    public event EventHandler<string> MoveGameToLibraryInvoked;

    /// <summary>
    /// The command invoked when a "Move To Library" menu item is clicked.
    /// </summary>
    public RelayExecuteCommand MoveGameToLibraryCommand =>
      _moveGameToLibraryCommand ?? (_moveGameToLibraryCommand = new RelayExecuteCommand(OnMoveGameToLibraryInvoked));

    /// <summary>
    /// The label of group header in the livet view
    /// </summary>
    public string ListViewGroupHeader {
      get { return _listViewGroupHeader; }
      set {
        _listViewGroupHeader = value;
        RaisePropertyChangedEvent(nameof(ListViewGroupHeader));
      }
    }

    /// <summary>
    /// The sort index of group headers (since display name is not a valid sorting key).
    /// </summary>
    public int ListViewGroupHeaderSortIndex {
      get { return _listViewGroupHeaderSortIndex; }
      set { UpdateProperty(ref _listViewGroupHeaderSortIndex, value, nameof(ListViewGroupHeaderSortIndex)); }
    }

    /// <summary>
    /// The list of libraries the game can be moved to.
    /// The string is the path of the destination library.
    /// </summary>
    public ObservableCollection<string> MoveToLibraries { get; } = new ObservableCollection<string>();

    /// <summary>
    /// The game label in the list view. Usually the name of the game, or the directory name
    /// if the ACF file is missing.
    /// </summary>
    public string DisplayName {
      get { return _displayName; }
      set { UpdateProperty(ref _displayName, value, nameof(DisplayName)); }
    }

    public string RelativePath {
      get { return _relativePath; }
      set { UpdateProperty(ref _relativePath, value, nameof(RelativePath)); }
    }

    public Brush RelativePathColor {
      get { return _relativePathColor ?? Brushes.Black; }
      set { UpdateProperty(ref _relativePathColor, value, nameof(RelativePathColor)); }
    }

    public bool IsRelativePathValid {
      get { return _isRelativePathValid; }
      set { UpdateProperty(ref _isRelativePathValid, value, nameof(IsRelativePathValid)); }
    }

    public string AcfFile {
      get { return _acfFile; }
      set { UpdateProperty(ref _acfFile, value, nameof(AcfFile)); }
    }

    public Brush AcfFileColor {
      get { return _acfFileColor ?? Brushes.Black; }
      set { UpdateProperty(ref _acfFileColor, value, nameof(AcfFileColor)); }
    }

    public long SizeOnDisk {
      get { return _sizeOnDisk; }
      set { UpdateProperty(ref _sizeOnDisk, value, nameof(SizeOnDisk)); }
    }

    public Brush SizeOnDiskColor {
      get { return _sizeOnDiskColor ?? Brushes.Black; }
      set { UpdateProperty(ref _sizeOnDiskColor, value, nameof(SizeOnDiskColor)); }
    }

    public long FileCount {
      get { return _fileCount; }
      set { UpdateProperty(ref _fileCount, value, nameof(FileCount)); }
    }

    public Brush FileCountColor {
      get { return _fileCountColor ?? Brushes.Black; }
      set { UpdateProperty(ref _fileCountColor, value, nameof(FileCountColor)); }
    }

    public string WorkshopAcfFile {
      get { return _wokrshopAcfFile; }
      set { UpdateProperty(ref _wokrshopAcfFile, value, nameof(WorkshopAcfFile)); }
    }

    public Brush WorkshopAcfFileColor {
      get { return _wokrshopAcfFileColor ?? Brushes.Black; }
      set { UpdateProperty(ref _wokrshopAcfFileColor, value, nameof(WorkshopAcfFileColor)); }
    }

    public string WorkshopRelativePath {
      get { return _workshopRelativePath; }
      set { UpdateProperty(ref _workshopRelativePath, value, nameof(WorkshopRelativePath)); }
    }

    public Brush WorkshopRelativePathColor {
      get { return _workshopRelativePathColor ?? Brushes.Black; }
      set { UpdateProperty(ref _workshopRelativePathColor, value, nameof(WorkshopRelativePathColor)); }
    }

    public long WorkshopSizeOnDisk {
      get { return _workshopSizeOnDisk; }
      set { UpdateProperty(ref _workshopSizeOnDisk, value, nameof(WorkshopSizeOnDisk)); }
    }

    public long WorkshopFileCount {
      get { return _workshopFileCount; }
      set { UpdateProperty(ref _workshopFileCount, value, nameof(WorkshopFileCount)); }
    }

    protected virtual void OnMoveGameToLibraryInvoked(object libraryPath) {
      MoveGameToLibraryInvoked?.Invoke(this, (string)libraryPath);
    }
  }
}