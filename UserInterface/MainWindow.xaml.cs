using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using SteamLibraryExplorer.SteamModel;
using SteamLibraryExplorer.ViewModel;

namespace SteamLibraryExplorer.UserInterface {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {
    public MainWindow() {
      InitializeComponent();

      DataContext = new MainPageViewModel();

      // Create grouping in list view
      CollectionView collectionView = (CollectionView)CollectionViewSource.GetDefaultView(ListView.ItemsSource);
      PropertyGroupDescription groupDescription =
        new PropertyGroupDescription(nameof(SteamGameViewModel.ListViewGroupHeader));
      collectionView.GroupDescriptions.Add(groupDescription);

      Loaded += OnLoaded;
    }

    public event EventHandler<GridViewColumnHeader> GamesListViewColumnsHeaderClick;

    private void OnLoaded(object sender, RoutedEventArgs routedEventArgs) {
      var model = new Model();
      var view = new View(this, model);
      var controller = new Controller(model, view);
      controller.Run();
    }

    private void ListViewColumnHeader_Click(object sender, RoutedEventArgs e) {
      OnGamesListViewColumnsHeaderClick((GridViewColumnHeader)sender);
    }

    protected virtual void OnGamesListViewColumnsHeaderClick(GridViewColumnHeader e) {
      GamesListViewColumnsHeaderClick?.Invoke(this, e);
    }
  }
}
