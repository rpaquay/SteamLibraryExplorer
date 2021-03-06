using System;
using System.Windows.Controls;

namespace SteamLibraryExplorer.ViewModel {
  public class ListViewColumnClickEventArgs : EventArgs {
    public ListViewColumnClickEventArgs(ListView listView, GridViewColumnHeader columnHeader) {
      ListView = listView;
      ColumnHeader = columnHeader;
    }

    public ListView ListView { get; }
    public GridViewColumnHeader ColumnHeader { get; }
  }
}