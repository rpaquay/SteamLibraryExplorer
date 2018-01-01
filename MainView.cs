using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using SteamLibraryExplorer.SteamModel;
using SteamLibraryExplorer.UserInterface;
using SteamLibraryExplorer.Utils;
using SteamLibraryExplorer.ViewModel;

namespace SteamLibraryExplorer {
  public class MainView {
    private readonly MainWindow _mainForm;
    private readonly Model _model;
    private readonly MainPageViewModel _viewModel;
    private readonly ThrottledDispatcher _throttledDispatcher = new ThrottledDispatcher();
    private readonly ListViewColumnSorter _listViewColumnSorter = new ListViewColumnSorter();
    private int _gameLibraryCount;
    private int _progressCount;

    public MainView(MainWindow mainForm, Model model) {
      _model = model;
      _mainForm = mainForm;
      _viewModel = (MainPageViewModel)_mainForm.DataContext;
    }

    public event EventHandler RefreshView;
    public event EventHandler CloseView;
    public event EventHandler<MoveGameEventArgs> CopyGameInvoked;

    public void Run() {
      _mainForm.CloseCommand.CanExecute += (sender, args) => args.CanExecute = true;
      _mainForm.RefreshCommand.CanExecute += (sender, args) => args.CanExecute = true;
      _mainForm.CloseCommand.Executed += (sender, args) => OnCloseView();
      _mainForm.RefreshCommand.Executed += (sender, args) => OnRefreshView();

      _mainForm.GamesListViewColumnsHeaderClick += (sender, header) => _listViewColumnSorter.SortColumn(_mainForm.ListView, header);
      _model.SteamConfiguration.Location.ValueChanged += (sender, arg) => ShowSteamLocation(arg.NewValue);
      _model.SteamConfiguration.SteamLibraries.CollectionChanged += SteamLibraries_CollectionChanged;

      _throttledDispatcher.Start(TimeSpan.FromMilliseconds(200));
    }

    private void SteamLibraries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
      switch (e.Action) {
        case NotifyCollectionChangedAction.Add:
          AddGameLibrary((SteamLibrary)e.NewItems[0]);
          break;
        case NotifyCollectionChangedAction.Reset:
          ClearGameLibraries();
          break;
        default:
          throw new ArgumentOutOfRangeException("e", @"Unsupported collection change");
      }
    }

    private void AddGameLibrary(SteamLibrary library) {
      // Add "Move To" entry to existing games
      foreach (var gameViewModel in _viewModel.SteamGames) {
        gameViewModel.MoveToLibraries.Add(library.Location.FullName);
      }

      // Add games of new library
      _gameLibraryCount++;
      var gamesViewModel = new List<SteamGameViewModel>();

      // Note: The order is important for concurrency correctness: we want to register to
      //       the "ValueChanged" event before we initialize the value of the ViewModel.
      library.FreeDiskSize.ValueChanged += (sender, arg) => {
        _throttledDispatcher.Enqeue(library.Location.FullName, () => {
          RefreshGameLibraryGroups(library, gamesViewModel);
        });
      };
      library.TotalDiskSize.ValueChanged += (sender, arg) => {
        _throttledDispatcher.Enqeue(library.Location.FullName, () => {
          RefreshGameLibraryGroups(library, gamesViewModel);
        });
      };

      foreach (var game in library.Games.OrderBy(x => x.DisplayName)) {
        var gameViewModel = new SteamGameViewModel {
          ListViewGroupHeader = GetGroupHeaderText(library),
          ListViewGroupHeaderSortIndex = _gameLibraryCount,
          DisplayName = game.DisplayName,
          AcfFile = game.AcfFile == null ? "<Missing>" : game.AcfFile.Path.GetRelativePathTo(library.Location),
          AcfFileColor = game.AcfFile == null ? Brushes.Red : null,
          Location = game.Location == null ? "<Not found>" : game.Location.GetRelativePathTo(library.Location),
          LocationColor = game.Location == null ? Brushes.Red : null,
        };

        // Add existing libraries to list of "MoveToLibraries" destination
        foreach (var otherLibrary in _model.SteamConfiguration.SteamLibraries) {
          if (!Object.Equals(library, otherLibrary)) {
            gameViewModel.MoveToLibraries.Add(otherLibrary.Location.FullName);
          }
        }

        // Note: The order is important for concurrency correctness: we want to register to
        //       the "ValueChanged" event before we initialize the value of the ViewModel.
        game.SizeOnDisk.ValueChanged += (sender, arg) => {
          _throttledDispatcher.Enqeue(game.Location.FullName + "-" + nameof(gameViewModel.SizeOnDisk), () => {
            gameViewModel.SizeOnDisk = arg.NewValue;
          });
        };
        gameViewModel.SizeOnDisk = game.SizeOnDisk.Value;
        gameViewModel.SizeOnDiskColor = game.Location == null ? Brushes.Red : null;

        // Note: The order is important for concurrency correctness: we want to register to
        //       the "ValueChanged" event before we initialize the value of the ViewModel.
        game.FileCount.ValueChanged += (sender, arg) => {
          _throttledDispatcher.Enqeue(game.Location.FullName + "-" + nameof(gameViewModel.FileCount), () => {
            gameViewModel.FileCount = arg.NewValue;
          });
        };
        gameViewModel.FileCount = game.FileCount.Value;
        gameViewModel.FileCountColor = game.Location == null ? Brushes.Red : null;

        gameViewModel.CopyGameInvoked += (sender, args) => OnCopyGameInvoked(new MoveGameEventArgs(game, args));

        _viewModel.SteamGames.Add(gameViewModel);
        gamesViewModel.Add(gameViewModel);
      }
    }

    private void RefreshGameLibraryGroups(SteamLibrary library, List<SteamGameViewModel> gamesViewModel) {
      foreach (var game in gamesViewModel) {
        game.ListViewGroupHeader = GetGroupHeaderText(library);
      }

      RefreshListView();
    }

    private void RefreshListView() {
      // The group collection does not listen to propery change events, so we need
      // to explicitly refresh it.
      var view = CollectionViewSource.GetDefaultView(_mainForm.ListView.ItemsSource);
      view.Refresh();
    }

    private static string GetGroupHeaderText(SteamLibrary library) {
      if (library.TotalDiskSize.Value > 0 && library.FreeDiskSize.Value > 0) {
        return string.Format("{0} - {1} of {2} available",
          library.DisplayName,
          HumanReadableDiskSize(library.FreeDiskSize.Value),
          HumanReadableDiskSize(library.TotalDiskSize.Value));
      }
      return library.DisplayName;
    }

    public static string HumanReadableFileCount(long value) {
      return string.Format("{0:n0}", value);
    }

    public static string HumanReadableDiskSize(long value) {
      if (value <= 0) {
        return "0 KB";
      }
      if (value <= 1024) {
        return "1 KB";
      }
      if (value < 1024 * 1024) {
        return string.Format("{0:n0} KB", value / 1024);
      }
      if (value < 1024 * 1024 * 1024) {
        return string.Format("{0:n2} MB", (double)value / 1024 / 1024);
      }
      return string.Format("{0:n2} GB", (double)value / 1024 / 1024 / 1024);
    }

    private void ClearGameLibraries() {
      _viewModel.SteamGames.Clear();
      _gameLibraryCount = 0;
    }

    public void ShowSteamLocation(FullPath directoryInfo) {
      //_mainForm.steamLocationTextBox.Text = directoryInfo.FullName;
      if (directoryInfo == null) {
        _viewModel.StatusText = "Steam location: <Not found>";
        return;
      }

      _viewModel.StatusText = string.Format("Steam location: {0}", directoryInfo.FullName);
    }

    protected virtual void OnRefreshView() {
      RefreshView?.Invoke(this, EventArgs.Empty);
    }

    public void ShowError(string text) {
      MessageBox.Show(_mainForm, text, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
    }

    protected virtual void OnCloseView() {
      CloseView?.Invoke(this, EventArgs.Empty);
    }

    public void StartProgress() {
      if (_progressCount == 0) {
        _viewModel.IsDiscoveringSteamFiles = true;
      }
      _progressCount++;
    }

    public void StopProgress() {
      _progressCount--;
      if (_progressCount == 0) {
        _viewModel.IsDiscoveringSteamFiles = false;
        // We need to refresh so that sorting of column is up-to-date
        _throttledDispatcher.Enqeue(nameof(RefreshListView), RefreshListView);
      }
    }

    protected virtual void OnCopyGameInvoked(MoveGameEventArgs e) {
      CopyGameInvoked?.Invoke(this, e);
    }
  }
}
