using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.ViewModel {
  public class SteamLibraryViewModel : ObservableObject {
    private string _displayName;
    private bool _hideListViewColumnHeader;

    public event EventHandler<ListViewColumnClickEventArgs> GamesListViewColumnsHeaderClick;
    public event EventHandler<FilterEventArgs> FilterGameEntry;


    public string DisplayName {
      get { return _displayName; }
      set { UpdateProperty(ref _displayName, value, nameof(DisplayName)); }
    }

    public bool HideListViewColumnHeader {
      get { return _hideListViewColumnHeader; }
      set { UpdateProperty(ref _hideListViewColumnHeader, value, nameof(HideListViewColumnHeader)); }
    }

    public ObservableCollection<SteamGameViewModel> SteamGames { get; } =
      new ObservableCollection<SteamGameViewModel>();

    public virtual void OnGamesListViewColumnsHeaderClick(ListViewColumnClickEventArgs e) {
      GamesListViewColumnsHeaderClick?.Invoke(this, e);
    }

    public virtual void OnFilterGameEntry(FilterEventArgs e) {
      FilterGameEntry?.Invoke(this, e);
    }
  }
}