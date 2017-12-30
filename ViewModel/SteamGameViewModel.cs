namespace SteamLibraryExplorer.ViewModel {
  public class SteamGameViewModel : ObservableObject {
    private string _listViewGroupLabel;
    private string _displayName;
    private string _location;
    private string _acfFile;
    private string _sizeOnDisk;
    private string _fileCount;

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

    public string AcfFile {
      get { return _acfFile; }
      set {
        _acfFile = value;
        RaisePropertyChangedEvent(nameof(AcfFile));
      }
    }

    public string SizeOnDisk {
      get { return _sizeOnDisk; }
      set {
        _sizeOnDisk = value;
        RaisePropertyChangedEvent(nameof(SizeOnDisk));
      }
    }

    public string FileCount {
      get { return _fileCount; }
      set {
        _fileCount = value;
        RaisePropertyChangedEvent(nameof(FileCount));
      }
    }
  }
}