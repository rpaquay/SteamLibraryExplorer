using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
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

    public event EventHandler<TextChangedEventArgs> SearchTextChanged;

    private void OnLoaded(object sender, RoutedEventArgs routedEventArgs) {
      var model = new Model();
      var view = new MainView(this, model);
      var controller = new Controller(model, view);
      controller.Run();
    }

    private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e) {
      OnSearchTextChanged(e);
    }

    protected virtual void OnSearchTextChanged(TextChangedEventArgs e) {
      SearchTextChanged?.Invoke(this, e);
    }
  }
}
