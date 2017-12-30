using System.Windows.Media;

namespace SteamLibraryExplorer.ViewModel {
  public class SteamGameViewModel : ObservableObject {
    private string _listViewGroupLabel;
    private string _displayName;
    private string _location;
    private string _acfFile;
    private string _sizeOnDisk;
    private string _fileCount;
    private Brush _locationColor;
    private Brush _acfFileColor;
    private Brush _sizeOnDiskColor;
    private Brush _fileCountColor;

    public string ListViewGroupLabel {
      get { return _listViewGroupLabel; }
      set {
        _listViewGroupLabel = value;
        RaisePropertyChangedEvent(nameof(ListViewGroupLabel));
      }
    }

    public string DisplayName {
      get { return _displayName; }
      set {
        _displayName = value;
        RaisePropertyChangedEvent(nameof(DisplayName));
      }
    }

    public string Location {
      get { return _location; }
      set {
        _location = value;
        RaisePropertyChangedEvent(nameof(Location));
      }
    }

    public Brush LocationColor {
      get { return _locationColor ?? Brushes.Black; }
      set {
        _locationColor = value;
        RaisePropertyChangedEvent(nameof(LocationColor));
      }
    }

    public string AcfFile {
      get { return _acfFile; }
      set {
        _acfFile = value;
        RaisePropertyChangedEvent(nameof(AcfFile));
      }
    }

    public Brush AcfFileColor {
      get { return _acfFileColor ?? Brushes.Black; }
      set {
        _acfFileColor = value;
        RaisePropertyChangedEvent(nameof(AcfFileColor));
      }
    }

    public string SizeOnDisk {
      get { return _sizeOnDisk; }
      set {
        _sizeOnDisk = value;
        RaisePropertyChangedEvent(nameof(SizeOnDisk));
      }
    }

    public Brush SizeOnDiskColor {
      get { return _sizeOnDiskColor ?? Brushes.Black; }
      set {
        _sizeOnDiskColor = value;
        RaisePropertyChangedEvent(nameof(SizeOnDiskColor));
      }
    }

    public string FileCount {
      get { return _fileCount; }
      set {
        _fileCount = value;
        RaisePropertyChangedEvent(nameof(FileCount));
      }
    }

    public Brush FileCountColor {
      get { return _fileCountColor ?? Brushes.Black; }
      set {
        _fileCountColor = value;
        RaisePropertyChangedEvent(nameof(FileCountColor));
      }
    }

  }
}