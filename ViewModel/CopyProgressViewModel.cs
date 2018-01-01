using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.ViewModel {
  public class CopyProgressViewModel : ObservableObject {
    private string _currentFilePath;
    private double _totalProgressFraction;
    private string _messageText;
    private double _currentFileProgressFraction;
    private string _currentFileProgressText;
    private string _totalProgressText;
    private string _elapsedTime;
    private string _remainingTime;
    private string _percentCompleteText;
    private string _itemsRemainingText;
    private string _speed;

    public string MessageText {
      get { return _messageText; }
      set {
        _messageText = value;
        RaisePropertyChangedEvent(nameof(MessageText));
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

    public string CurrentFilePath {
      get { return _currentFilePath; }
      set {
        if (_currentFilePath != value) {
          _currentFilePath = value;
          RaisePropertyChangedEvent(nameof(CurrentFilePath));
        }
      }
    }

    public double CurrentFileProgressFraction {
      get { return _currentFileProgressFraction; }
      set {
        _currentFileProgressFraction = value;
        RaisePropertyChangedEvent(nameof(CurrentFileProgressFraction));
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

    public double TotalProgressFraction {
      get { return _totalProgressFraction; }
      set {
        _totalProgressFraction = value;
        RaisePropertyChangedEvent(nameof(TotalProgressFraction));
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