using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using SteamLibraryExplorer.ViewModel;

namespace SteamLibraryExplorer.UserInterface {
  /// <summary>
  /// Interaction logic for SteamLibraryUserControl.xaml
  /// </summary>
  public partial class SteamLibraryUserControl : UserControl {
    public SteamLibraryUserControl() {
      InitializeComponent();
    }

    public bool DisableMouseWheelScrolling { get; set; }

    public SteamLibraryViewModel ViewModel => (SteamLibraryViewModel) DataContext;

    private void ListViewColumnHeader_Click(object sender, RoutedEventArgs e) {
      ViewModel.OnGamesListViewColumnsHeaderClick(new ListViewColumnClickEventArgs(ListView, (GridViewColumnHeader)sender));
    }

    private void SteamGamesCollectionViewSource_OnFilter(object sender, FilterEventArgs e) {
      ViewModel.ListView = ListView;
      ViewModel.OnFilterGameEntry(e);
    }

    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
      if (DisableMouseWheelScrolling) {
        if (sender is ListView && !e.Handled) {
          e.Handled = true;
          var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
          eventArg.RoutedEvent = UIElement.MouseWheelEvent;
          eventArg.Source = sender;
          var parent = ((Control) sender).Parent as UIElement;
          parent?.RaiseEvent(eventArg);
        }
      }
    }
  }
}
