using System;
using System.Windows.Input;
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
    private CopyGameCommandImpl _copyGameCommand;

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

    public CopyGameCommandImpl CopyGameCommand {
      get { return _copyGameCommand ?? (_copyGameCommand = new CopyGameCommandImpl(OnCopyGameInvoked));}
    }

    public event EventHandler CopyGameInvoked;

    public class CopyGameCommandImpl : ICommand {
      private readonly Action _callback;

      public CopyGameCommandImpl(Action callback) {
        _callback = callback;
      }

      public bool CanExecute(object parameter) {
        //return parameter is SteamGameViewModel;
        return true;
      }

      public void Execute(object parameter) {
        _callback();
      }

      public event EventHandler CanExecuteChanged;

      protected virtual void OnCanExecuteChanged() {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
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

    protected virtual void OnCopyGameInvoked() {
      CopyGameInvoked?.Invoke(this, EventArgs.Empty);
    }
  }
}