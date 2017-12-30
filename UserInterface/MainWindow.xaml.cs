using System;
using System.Windows;
using System.Windows.Data;
using SteamLibraryExplorer.SteamUtil;
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
        new PropertyGroupDescription(nameof(SteamGameViewModel.ListViewGroupLabel));
      collectionView.GroupDescriptions.Add(groupDescription);

      Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs routedEventArgs) {
      var model = new Model();
      var view = new View(this, model);
      var controller = new Controller(model, view);
      controller.Run();
    }
  }
}
