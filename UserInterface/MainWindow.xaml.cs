using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using SteamLibraryExplorer.SteamModel;
using SteamLibraryExplorer.ViewModel;

namespace SteamLibraryExplorer.UserInterface {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow {
    public MainWindow() {
      InitializeComponent();

      // Set DataContext *before* OnLoaded
      Debug.Assert(DataContext is MainPageViewModel);
      Loaded += OnLoaded;
    }

    public event EventHandler<GridViewColumnHeader> GamesListViewColumnsHeaderClick;
    public event EventHandler<FilterEventArgs> FilterGameEntry;
    public event EventHandler<TextChangedEventArgs> SearchTextChanged;

    private void OnLoaded(object sender, RoutedEventArgs routedEventArgs) {
      var model = new Model();
      var view = new MainView(this, model);
      var controller = new Controller(model, view);
      controller.Run();
    }

    private void ListViewColumnHeader_Click(object sender, RoutedEventArgs e) {
      OnGamesListViewColumnsHeaderClick((GridViewColumnHeader)sender);
    }

    private void SteamGamesCollectionViewSource_OnFilter(object sender, FilterEventArgs e) {
      OnFilterGameEntry(e);
    }

    private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e) {
      OnSearchTextChanged(e);
    }

    protected virtual void OnGamesListViewColumnsHeaderClick(GridViewColumnHeader e) {
      GamesListViewColumnsHeaderClick?.Invoke(this, e);
    }

    protected virtual void OnFilterGameEntry(FilterEventArgs e) {
      FilterGameEntry?.Invoke(this, e);
    }

    protected virtual void OnSearchTextChanged(TextChangedEventArgs e) {
      SearchTextChanged?.Invoke(this, e);
    }
  }
}
