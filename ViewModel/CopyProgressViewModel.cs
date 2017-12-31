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

    public string MessageText {
      get { return _messageText; }
      set {
        _messageText = value;
        RaisePropertyChangedEvent(nameof(MessageText));
      }
    }

    public string CurrentFilePath {
      get { return _currentFilePath; }
      set {
        _currentFilePath = value;
        RaisePropertyChangedEvent(nameof(CurrentFilePath));
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
        _currentFileProgressText = value;
        RaisePropertyChangedEvent(nameof(CurrentFileProgressText));
      }
    }

    public double TotalProgressPercent {
      get { return _totalProgressPercent; }
      set {
        _totalProgressPercent = value;
        RaisePropertyChangedEvent(nameof(TotalProgressPercent));
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