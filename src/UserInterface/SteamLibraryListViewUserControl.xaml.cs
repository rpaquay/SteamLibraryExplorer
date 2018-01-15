using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using SteamLibraryExplorer.ViewModel;

namespace SteamLibraryExplorer.UserInterface {
  /// <summary>
  /// Interaction logic for SteamLibraryListViewUserControl.xaml
  /// </summary>
  public partial class SteamLibraryListViewUserControl {
    public SteamLibraryListViewUserControl() {
      InitializeComponent();
      Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs routedEventArgs) {
      if (!(DataContext is SteamLibraryViewModel)) {
        Debug.Assert(false, $"Data context is {DataContext}");
        return;
      }
      ViewModel.ListView = ListView;
    }

    public SteamLibraryViewModel ViewModel => (SteamLibraryViewModel)DataContext;

    public bool DisableMouseWheelScrolling { get; set; }

    public bool HideListViewColumnHeaders { get; set; }

    public bool HideListViewContent { get; set; }

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
          for (var parent = ((FrameworkElement)sender).Parent; parent != null;) {
            if (parent is ScrollViewer) {
              topMostViewer = (ScrollViewer)parent;
            }

            if (parent is FrameworkElement) {
              parent = VisualTreeHelper.GetParent(parent);
            } else {
              parent = null;
            }
          }
          topMostViewer?.RaiseEvent(eventArg);
        }
      }
    }
  }
}
