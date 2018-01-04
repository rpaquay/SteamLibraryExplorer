using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.ViewModel {
  public class SteamLibraryViewModel : ObservableObject {
    private string _displayName;
    private bool _hideListViewColumnHeader;
    private double _gameDisplayNameColumnWidth = 220;
    private bool _showWorkshopFiles;

    public event EventHandler<ListViewColumnClickEventArgs> GamesListViewColumnsHeaderClick;
    public event EventHandler<FilterEventArgs> FilterGameEntry;


    public string DisplayName {
      get { return _displayName; }
      set { UpdateProperty(ref _displayName, value, nameof(DisplayName)); }
    }

    public double GameDisplayNameColumnWidth {
      get { return _gameDisplayNameColumnWidth; }
      set { UpdateProperty(ref _gameDisplayNameColumnWidth, value, nameof(GameDisplayNameColumnWidth)); }
    }

    public bool HideListViewColumnHeader {
      get { return _hideListViewColumnHeader; }
      set { UpdateProperty(ref _hideListViewColumnHeader, value, nameof(HideListViewColumnHeader)); }
    }

    public bool ShowWorkshopFiles {
      get { return _showWorkshopFiles; }
      set { UpdateProperty(ref _showWorkshopFiles, value, nameof(ShowWorkshopFiles)); }
    }

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