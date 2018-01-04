using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace SteamLibraryExplorer.UserInterface {
  /// <summary>
  /// Interaction logic for SteamLibraryUserControl.xaml
  /// </summary>
  public partial class SteamLibraryUserControl : UserControl {
    public SteamLibraryUserControl() {
      InitializeComponent();
    }

    public event EventHandler<GridViewColumnHeader> GamesListViewColumnsHeaderClick;
    public event EventHandler<FilterEventArgs> FilterGameEntry;

    public bool DisableMouseWheelScrolling { get; set; }

    private void ListViewColumnHeader_Click(object sender, RoutedEventArgs e) {
      OnGamesListViewColumnsHeaderClick((GridViewColumnHeader)sender);
    }

    private void SteamGamesCollectionViewSource_OnFilter(object sender, FilterEventArgs e) {
      OnFilterGameEntry(e);
    }

    protected virtual void OnGamesListViewColumnsHeaderClick(GridViewColumnHeader e) {
      GamesListViewColumnsHeaderClick?.Invoke(this, e);
    }

    protected virtual void OnFilterGameEntry(FilterEventArgs e) {
      FilterGameEntry?.Invoke(this, e);
    }

    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
      if (DisableMouseWheelScrolling) {
        if (sender is ListView && !e.Handled) {
          e.Handled = true;
          var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
          eventArg.RoutedEvent = UIElement.MouseWheelEvent;
          eventArg.Source = sender;
          var parent = ((Control) sender).Parent as UIElement;
          parent.RaiseEvent(eventArg);
        }
      }
    }
  }
}
