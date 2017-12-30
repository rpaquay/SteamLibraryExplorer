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

      var dataContext = new MainPageViewModel();
      DataContext = dataContext;

      var library1 = new SteamLibraryViewModel {
        Location = "D:\\Steam",
      };

      var library2 = new SteamLibraryViewModel {
        Location = "E:\\Steam",
      };

      var group1 = string.Format("Main Library ({0}) : {1} of {2} ", library1.Location, 100, 200);
      var group2 = string.Format("Library ({0}) : {1} of {2} ", library2.Location, 100, 200);
      dataContext.Games.Add(new SteamGameViewModel {
        DisplayName = "Game 1",
        AcfFile = "file.acf",
        FileCount = "10",
        SizeOnDisk = "5",
        ListViewGroupLabel = group1,
      });

      dataContext.Games.Add(new SteamGameViewModel {
        DisplayName = "Game 2",
        AcfFile = "file.acf",
        FileCount = "10",
        SizeOnDisk = "5",
        ListViewGroupLabel = group1,
      });

      dataContext.Games.Add(new SteamGameViewModel {
        DisplayName = "Game 2",
        AcfFile = "file.acf",
        FileCount = "10",
        SizeOnDisk = "5",
        ListViewGroupLabel = group2,
      });

      //List<Ur> items = new List<User>();
      //items.Add(new User() { Name = "John Doe", Age = 42, Sex = SexType.Male });
      //items.Add(new User() { Name = "Jane Doe", Age = 39, Sex = SexType.Female });
      //items.Add(new User() { Name = "Sammy Doe", Age = 13, Sex = SexType.Male });
      //lvUsers.ItemsSource = items;


      // Create grouping in list view
      CollectionView collectionView = (CollectionView)CollectionViewSource.GetDefaultView(ListView.ItemsSource);
      PropertyGroupDescription groupDescription = new PropertyGroupDescription(nameof(SteamGameViewModel.ListViewGroupLabel));
      collectionView.GroupDescriptions.Add(groupDescription);

      var model = new Model();
      var view = new View(this, model);
      var controller = new Controller(model, view);
      controller.Run();
    }
  }
}
