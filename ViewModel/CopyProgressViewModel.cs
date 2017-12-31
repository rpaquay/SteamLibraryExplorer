using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.ViewModel {
  public class CopyProgressViewModel : ObservableObject {
    private string _currentFilePath;
    private double _totalProgressPercent;
    private string _messageText;
    private double _currentFileProgressPercent;
    private string _currentFileProgressText;
    private string _totalProgressText;
    private string _elapsedTime;
    private string _remainingTime;
    private string _remaingFileCount;
    private string _percentCompleteText;
    private string _itemsRemainingText;
    private string _sourcePath;
    private string _destinationPath;
    private string _speed;

    public string MessageText {
      get { return _messageText; }
      set {
        _messageText = value;
        RaisePropertyChangedEvent(nameof(MessageText));
      }
    }

    public string SourcePath {
      get { return _sourcePath; }
      set {
        if (_sourcePath != value) {
          _sourcePath = value;
          RaisePropertyChangedEvent(nameof(SourcePath));
        }
      }
    }

    public string DestinationPath {
      get { return _destinationPath; }
      set {
        if (_destinationPath != value) {
          _destinationPath = value;
          RaisePropertyChangedEvent(nameof(DestinationPath));
        }
      }
    }

    public string SpeedText {
      get { return _speed; }
      set {
        if (_speed != value) {
          _speed = value;
          RaisePropertyChangedEvent(nameof(SpeedText));
        }
      }
    }

    public string RemainingFileCount {
      get { return _remaingFileCount; }
      set {
        _remaingFileCount = value;
        RaisePropertyChangedEvent(nameof(RemainingFileCount));
      }
    }

    public string CurrentFilePath {
      get { return _currentFilePath; }
      set {
        if (_currentFilePath != value) {
          _currentFilePath = value;
          RaisePropertyChangedEvent(nameof(CurrentFilePath));
        }
      }
    }

    public double CurrentFileProgressPercent {
      get { return _currentFileProgressPercent; }
      set {
        _currentFileProgressPercent = value;
        RaisePropertyChangedEvent(nameof(CurrentFileProgressPercent));
      }
    }

    public string CurrentFileProgressText {
      get { return _currentFileProgressText; }
      set {
        if (_currentFileProgressText != value) {
          _currentFileProgressText = value;
          RaisePropertyChangedEvent(nameof(CurrentFileProgressText));
        }
      }
    }

    public string ItemsRemainingText {
      get { return _itemsRemainingText; }
      set {
        _itemsRemainingText = value;
        RaisePropertyChangedEvent(nameof(ItemsRemainingText));
      }
    }

    public double TotalProgressPercent {
      get { return _totalProgressPercent; }
      set {
        _totalProgressPercent = value;
        RaisePropertyChangedEvent(nameof(TotalProgressPercent));
      }
    }

    public string PercentCompleteText {
      get { return _percentCompleteText; }
      set {
        if (_percentCompleteText != value) {
          _percentCompleteText = value;
          RaisePropertyChangedEvent(nameof(PercentCompleteText));
        }
      }
    }

    public string TotalProgressText {
      get { return _totalProgressText; }
      set {
        _totalProgressText = value;
        RaisePropertyChangedEvent(nameof(TotalProgressText));
      }
    }

    public string ElapsedTime {
      get { return _elapsedTime; }
      set {
        _elapsedTime = value;
        RaisePropertyChangedEvent(nameof(ElapsedTime));
      }
    }

    public string RemainingTime {
      get { return _remainingTime; }
      set {
        _remainingTime = value;
        RaisePropertyChangedEvent(nameof(RemainingTime));
      }
    }
  }
}