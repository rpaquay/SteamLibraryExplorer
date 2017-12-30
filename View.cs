using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using SteamLibraryExplorer.SteamUtil;
using SteamLibraryExplorer.UserInterface;
using SteamLibraryExplorer.Utils;
using SteamLibraryExplorer.ViewModel;

namespace SteamLibraryExplorer {
  public class View {
    private readonly MainWindow _mainForm;
    private readonly Model _model;
    private readonly MainPageViewModel _viewModel;
    private readonly IDictionary<string, Action> _refreshActions = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase);
    private readonly object _refreshActionsLock = new object();
    private readonly DispatcherTimer _timer = new DispatcherTimer();

    public View(MainWindow mainForm, Model model) {
      _model = model;
      _mainForm = mainForm;
      _viewModel = (MainPageViewModel)_mainForm.DataContext;
    }

    public event EventHandler RefreshView;
    public event EventHandler CloseView;

    public void Run() {
      _mainForm.CloseCommand.CanExecute += (sender, args) => args.CanExecute = true;
      _mainForm.RefreshCommand.CanExecute += (sender, args) => args.CanExecute = true;
      _mainForm.CloseCommand.Executed += (sender, args) => OnCloseView();
      _mainForm.RefreshCommand.Executed += (sender, args) => OnRefreshView();
      _model.SteamConfiguration.Location.ValueChanged += (sender, arg) => ShowSteamLocation(arg.NewValue);
      _model.SteamConfiguration.SteamLibraries.CollectionChanged += SteamLibraries_CollectionChanged;

      _timer.Interval = TimeSpan.FromMilliseconds(200);
      _timer.Tick += RefreshListViewTimerOnTick;
      _timer.Start();
    }

    private void RefreshListViewTimerOnTick(object o, EventArgs eventArgs) {
      List<Action> refreshActions;
      lock (_refreshActionsLock) {
        if (_refreshActions.Count == 0) {
          return;
        }

        refreshActions = _refreshActions.Values.ToList();
        _refreshActions.Clear();
      }

      foreach (var action in refreshActions) {
        action();
      }
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
      var groupLabel = GetGroupHeaderText(library);

      var gamesViewModel = new List<SteamGameViewModel>();
      library.FreeDiskSize.ValueChanged += (sender, arg) => {
        lock (_refreshActionsLock) {
          _refreshActions[library.Location.FullName] = () => {
            foreach (var game in gamesViewModel) {
              game.ListViewGroupLabel = GetGroupHeaderText(library);
            }
            ICollectionView view = CollectionViewSource.GetDefaultView(_mainForm.ListView.ItemsSource);
            view.Refresh();
          };
        }
      };
      library.TotalDiskSize.ValueChanged += (sender, arg) => {
        lock (_refreshActionsLock) {
          _refreshActions[library.Location.FullName] = () => {
            foreach (var game in gamesViewModel) {
              game.ListViewGroupLabel = GetGroupHeaderText(library);
            }
            ICollectionView view = CollectionViewSource.GetDefaultView(_mainForm.ListView.ItemsSource);
            view.Refresh();
          };
        }
      };

      foreach (var game in library.Games) {
        var vm = new SteamGameViewModel {
          ListViewGroupLabel = groupLabel,
          DisplayName = game.DisplayName,
          AcfFile = game.AcfFile == null ? "<Missing>" : game.AcfFile.FileInfo.GetRelativePathTo(library.Location),
          Location = game.Location == null ? "<Not found>" : game.Location.GetRelativePathTo(library.Location),
          SizeOnDisk = HumanReadableDiskSize(game.SizeOnDisk.Value),
          FileCount = HumanReadableFileCount(game.FileCount.Value),
        };
        //TODO: Make color RED if missing acf file or location

        game.SizeOnDisk.ValueChanged += (sender, arg) => {
          lock (_refreshActionsLock) {
            _refreshActions[game.Location.FullName + "-size"] = () => vm.SizeOnDisk = HumanReadableDiskSize(arg.NewValue);
          }
        };
        game.FileCount.ValueChanged += (sender, arg) => {
          lock (_refreshActionsLock) {
            _refreshActions[game.Location.FullName + "-count"] = () => vm.FileCount = HumanReadableFileCount(arg.NewValue);
          }
        };

        _viewModel.SteamGames.Add(vm);
        gamesViewModel.Add(vm);
      }
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

    private static string HumanReadableFileCount(long value) {
      return string.Format("{0:n0}", value);
    }

    private static string HumanReadableDiskSize(long value) {
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

      //if (value < 1024 * 1024) {
      //  return string.Format("{0:n0} KB ({1:n0} Bytes)", value / 1024, value);
      //}
      //if (value < 1024 * 1024 * 1024) {
      //  return string.Format("{0:n0} MB ({1:n0} Bytes)", value / 1024 / 1024, value);
      //}
      //return string.Format("{0:n2} GB ({1:n0} Bytes)", (double)value / 1024 / 1024 / 1024, value);
    }

    private void ClearGameLibraries() {
      _viewModel.SteamGames.Clear();
    }

    public void ShowSteamLocation(DirectoryInfo directoryInfo) {
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
  }
}
