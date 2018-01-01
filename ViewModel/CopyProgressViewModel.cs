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
    private string _speedText;

    public string MessageText {
      get { return _messageText; }
      set { UpdateProperty(ref _messageText, value, nameof(MessageText)); }
    }

    public string SpeedTextText {
      get { return _speedText; }
      set { UpdateProperty(ref _speedText, value, nameof(SpeedTextText)); }
    }

    public string CurrentFilePath {
      get { return _currentFilePath; }
      set { UpdateProperty(ref _currentFilePath, value, nameof(CurrentFilePath)); }
    }

    public double CurrentFileProgressFraction {
      get { return _currentFileProgressFraction; }
      set { UpdateProperty(ref _currentFileProgressFraction, value, nameof(CurrentFileProgressFraction)); }
    }

    public string CurrentFileProgressText {
      get { return _currentFileProgressText; }
      set { UpdateProperty(ref _currentFileProgressText, value, nameof(CurrentFileProgressText)); }
    }

    public string ItemsRemainingText {
      get { return _itemsRemainingText; }
      set { UpdateProperty(ref _itemsRemainingText, value, nameof(ItemsRemainingText)); }
    }

    public double TotalProgressFraction {
      get { return _totalProgressFraction; }
      set { UpdateProperty(ref _totalProgressFraction, value, nameof(TotalProgressFraction)); }
    }

    public string PercentCompleteText {
      get { return _percentCompleteText; }
      set { UpdateProperty(ref _percentCompleteText, value, nameof(PercentCompleteText)); }
    }

    public string TotalProgressText {
      get { return _totalProgressText; }
      set { UpdateProperty(ref _totalProgressText, value, nameof(TotalProgressText)); }
    }

    public string ElapsedTime {
      get { return _elapsedTime; }
      set { UpdateProperty(ref _elapsedTime, value, nameof(ElapsedTime)); }
    }

    public string RemainingTime {
      get { return _remainingTime; }
      set { UpdateProperty(ref _remainingTime, value, nameof(RemainingTime)); }
    }
  }
}