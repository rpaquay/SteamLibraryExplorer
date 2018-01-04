using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using SteamLibraryExplorer.ViewModel;

namespace SteamLibraryExplorer.UserInterface {
  /// <summary>
  /// Interaction logic for SteamLibraryUserControl.xaml
  /// </summary>
  public partial class SteamLibraryUserControl {
    public SteamLibraryUserControl() {
      InitializeComponent();
      Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs routedEventArgs) {
      ViewModel.ListView = ListView;
    }

    public bool DisableMouseWheelScrolling { get; set; }

    public SteamLibraryViewModel ViewModel => (SteamLibraryViewModel)DataContext;

    private void ListViewColumnHeader_Click(object sender, RoutedEventArgs e) {
      ViewModel.OnGamesListViewColumnsHeaderClick(new ListViewColumnClickEventArgs(ListView, (GridViewColumnHeader)sender));
    }

    private void SteamGamesCollectionViewSource_OnFilter(object sender, FilterEventArgs e) {
      ViewModel.OnFilterGameEntry(e);
    }

    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
      if (DisableMouseWheelScrolling) {
        if (sender is ListView && !e.Handled) {
          e.Handled = true;
          var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
          eventArg.RoutedEvent = MouseWheelEvent;
          eventArg.Source = sender;
          ScrollViewer topMostViewer = null;
          for (var parent = ((FrameworkElement)sender).Parent; parent != null; ) {
            if (parent is ScrollViewer) {
              topMostViewer = (ScrollViewer)parent;
            }

            if (parent is FrameworkElement) {
              parent = VisualTreeHelper.GetParent(parent);
            }
            else {
              parent = null;
            }
          }
          topMostViewer?.RaiseEvent(eventArg);
        }
      }
    }
  }
}
