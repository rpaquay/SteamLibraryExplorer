namespace SteamLibraryExplorer.ViewModel {
  public class SteamLibraryViewModel : ObservableObject {
    private string _location;

    public string Location {
      get { return _location; }
      set {
        _location = value;
        RaisePropertyChangedEvent(nameof(Location));
      }
    }

    //public ObservableCollection<SteamGameViewModel> SteamGames { get; } = new ObservableCollection<SteamGameViewModel>();
  }
}