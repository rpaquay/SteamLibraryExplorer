using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using SteamLibraryExplorer.SteamModel;
using SteamLibraryExplorer.SteamUtil;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer {
  class Controller {
    private static readonly ILoggerFacade Logger = LoggerManagerFacade.GetLogger(typeof(Controller));

    private readonly Model _model;
    private readonly MainView _mainView;
    private readonly SteamDiscovery _steamDiscovery;
    private readonly SteamGameMover _steamGameMover;
    private readonly DispatcherTimer _lookForSteamTimer;
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private CopyProgressView _currentGameMoveOperationView;

    public Controller(Model model, MainView mainView) {
      _model = model;
      _mainView = mainView;
      _steamDiscovery = new SteamDiscovery();
      _steamGameMover = new SteamGameMover();
      _steamGameMover.CopyingFile += (sender, args) => {
        Logger.Info("Copying file \"{0}\" to \"{1}\"", args.SourcePath.FullName, args.DestinationPath.FullName);
      };
      _steamGameMover.DeletingFile += (sender, args) => {
        Logger.Info("Deleting file \"{0}\"", args.Path.FullName);
      };
      _steamGameMover.CreatingDirectory += (sender, args) => {
        Logger.Info("Creating directory \"{0}\"", args.Path.FullName);
      };
      _steamGameMover.DeletingDirectory += (sender, args) => {
        Logger.Info("Deleting directory \"{0}\"", args.Path.FullName);
      };
      _lookForSteamTimer = new DispatcherTimer();
    }

    public void Run() {
      _lookForSteamTimer.Interval = TimeSpan.FromSeconds(2);
      _lookForSteamTimer.Tick += LookForSteamTimerOnTick;
      _lookForSteamTimer.Start();
      _mainView.RefreshView += (sender, args) => FetchSteamConfigurationAsync();
      _mainView.CloseView += (sender, args) => Application.Current.Shutdown();
      _mainView.CopyGameInvoked += (sender, game) => MoveGameToOtherLibrary(game);
      _mainView.Run();
      FetchSteamConfigurationAsync();
    }

    private async void LookForSteamTimerOnTick(object o, EventArgs eventArgs) {
      if (_model.SteamConfiguration.Location.Value == null) {
        var steamLocation = await _steamDiscovery.LocateSteamFolderAsync();
        if (steamLocation != null) {
          FetchSteamConfigurationAsync();
        }
      }
    }

    private async void MoveGameToOtherLibrary(MoveGameEventArgs e) {
      if (_currentGameMoveOperationView != null) {
        _mainView.ShowError("Cannot move game because another game is currently being move.\r\n" +
          "Please wait for the operation to finish, or cancel it.");
        return;
      }

      if (!FileSystem.DirectoryExists(e.Game.Location)) {
        _mainView.ShowError($"Cannot move game because the directory \"{e.Game.Location}\" does not exist");
        return;
      }

      if (e.Game.AcfFile == null) {
        _mainView.ShowError("Cannot move game because it does not have a valid ACF file.");
        return;
      }

      var cancellationTokenSource = new CancellationTokenSource();
      _currentGameMoveOperationView = new CopyProgressView(_mainView.CreateCopyPropgressWindow());
      try {
        _currentGameMoveOperationView.Cancel += (sender, args) => cancellationTokenSource.Cancel();
        Action<MoveDirectoryInfo> progress = info => {
          _currentGameMoveOperationView.ReportProgress(info);
        };

        _currentGameMoveOperationView.Show();

        var destinationLibrary = new FullPath(e.DestinationLibraryPath);

        var result = await _steamGameMover.MoveSteamGameAsync(e.Game, destinationLibrary, progress, cancellationTokenSource.Token);

        if (result.Kind == MoveGameResultKind.Error) {
          Debug.Assert(result.Error != null);
          _mainView.ShowError(
            $"Error moving steam game to library \"{destinationLibrary}\":\r\n\r\n" +
            $"{result.Error.Message}");
        } else if (result.Kind == MoveGameResultKind.Ok) {
          await _steamGameMover.DeleteAppCacheAsync(_model.SteamConfiguration);
          var steamProcess = await _steamDiscovery.FindSteamProcessAsync();
          _currentGameMoveOperationView.Close();

          var successMessage =
            $"\"{e.Game.DisplayName}\" has been successfully moved to the library located at \"{e.DestinationLibraryPath}\".";
          if (steamProcess == null) {
            _mainView.ShowInfo(successMessage);
          }
          else {
            var yes = _mainView.ShowYesNo(successMessage + "\r\n\r\n" +
              "Steam should be restarted now to make sure the new game location is taken into account.\r\n\r\n" +
              "Do you want to automatically restart Steam now?");

            if (yes) {
              var success = await _steamDiscovery.RestartSteamAsync();
              if (success) {
                _mainView.ShowInfo("Steam was successfully restarted");
              }
              else {
                _mainView.ShowInfo("Steam could not be restarted. Please restart manually.");
              }
            }
          }
        }
      }
      finally {
        _currentGameMoveOperationView.Close();
        _currentGameMoveOperationView = null;

        // Refresh view since things have changed
        FetchSteamConfigurationAsync();
      }
    }

    private async void FetchSteamConfigurationAsync() {
      // Cancel previous operation
      _cancellationTokenSource.Cancel();
      _cancellationTokenSource = new CancellationTokenSource();
      var cancellationToken = _cancellationTokenSource.Token;

      _mainView.StartProgress();
      try {

        // Clear model and start a new collection process
        _model.SteamConfiguration.SteamLibraries.Clear();


        // Find steam location (installation directory)
        var steamLocation = await _steamDiscovery.LocateSteamFolderAsync();
        if (cancellationToken.IsCancellationRequested) {
          return;
        }

        // If no location found, re-use previously found one
        if (steamLocation == null) {
          steamLocation = _model.SteamConfiguration.Location.Value;
        }

        // Store location for re-use
        if (steamLocation != null) {
          Logger.Info("Found Steam installation directory: {0}", steamLocation.FullName);
          _model.SteamConfiguration.Location.Value = steamLocation;
        }

        if (steamLocation == null) {
          _mainView.ShowError(
            "Cannot locate Steam installation folder.\r\n\r\n" +
            "Try starting the Steam application and select \"File > Refresh\".");
          return;
        }

        var mainLibrary = await _steamDiscovery.LoadMainLibraryAsync(steamLocation, cancellationToken);
        if (cancellationToken.IsCancellationRequested) {
          return;
        }
        _model.SteamConfiguration.SteamLibraries.Add(mainLibrary);

        var libraries = await _steamDiscovery.LoadAdditionalLibrariesAsync(steamLocation, cancellationToken);
        if (cancellationToken.IsCancellationRequested) {
          return;
        }
        foreach (var library in libraries) {
          _model.SteamConfiguration.SteamLibraries.Add(library);
        }

        // Start background tasks of discovering game size on disk
        await _steamDiscovery.DiscoverSizeOnDiskAsync(_model.SteamConfiguration.SteamLibraries, cancellationToken);
      }
      finally {
        _mainView.StopProgress();
      }
    }
  }
}
