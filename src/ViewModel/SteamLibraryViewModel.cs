using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.ViewModel {
  public class SteamLibraryViewModel : ObservableObject {
    private string _displayName;
    private bool _showListViewColumnHeader;

    public event EventHandler<ListViewColumnClickEventArgs> GamesListViewColumnsHeaderClick;
    public event EventHandler<FilterEventArgs> FilterGameEntry;

    public string DisplayName {
      get { return _displayName; }
      set { UpdateProperty(ref _displayName, value, nameof(DisplayName)); }
    }

    public bool ShowListViewColumnHeader {
      get { return _showListViewColumnHeader; }
      set { UpdateProperty(ref _showListViewColumnHeader, value, nameof(ShowListViewColumnHeader)); }
    }

    public SteamLibraryListViewColumnsModel SteamLibraryListViewColumnsModel { get; }
      = new SteamLibraryListViewColumnsModel();

    public ObservableCollection<SteamGameViewModel> SteamGames { get; } =
      new ObservableCollection<SteamGameViewModel>();

    /// <summary>
    /// TODO: Find a way to remove this!
    /// </summary>
    public ListView ListView { get; set; }

    public virtual void OnGamesListViewColumnsHeaderClick(ListViewColumnClickEventArgs e) {
      GamesListViewColumnsHeaderClick?.Invoke(this, e);
    }

    public virtual void OnFilterGameEntry(FilterEventArgs e) {
      FilterGameEntry?.Invoke(this, e);
    }
  }
}