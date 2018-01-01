using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.ViewModel {
  public class SteamGameViewModel : ObservableObject {
    private string _listViewGroupHeader;
    private int _listViewGroupHeaderSortIndex;
    private string _displayName;
    private string _location;
    private string _acfFile;
    private long _sizeOnDisk;
    private long _fileCount;
    private Brush _locationColor;
    private Brush _acfFileColor;
    private Brush _sizeOnDiskColor;
    private Brush _fileCountColor;
    private RelayExecuteCommand _copyGameCommand;

    public string ListViewGroupHeader {
      get { return _listViewGroupHeader; }
      set {
        _listViewGroupHeader = value;
        RaisePropertyChangedEvent(nameof(ListViewGroupHeader));
      }
    }

    public int ListViewGroupHeaderSortIndex {
      get { return _listViewGroupHeaderSortIndex; }
      set {
        _listViewGroupHeaderSortIndex = value;
        RaisePropertyChangedEvent(nameof(ListViewGroupHeaderSortIndex));
      }
    }

    public RelayExecuteCommand CopyGameCommand {
      get { return _copyGameCommand ?? (_copyGameCommand = new RelayExecuteCommand(OnCopyGameInvoked));}
    }

    public event EventHandler<string> CopyGameInvoked;

    public ObservableCollection<string> MoveToLibraries { get; } = new ObservableCollection<string>();

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

    public long SizeOnDisk {
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

    public long FileCount {
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

    protected virtual void OnCopyGameInvoked(object libraryPath) {
      CopyGameInvoked?.Invoke(this, (string)libraryPath);
    }
  }
}