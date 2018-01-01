using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using SteamLibraryExplorer.SteamModel;
using SteamLibraryExplorer.SteamUtil;
using SteamLibraryExplorer.UserInterface;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer {
  class Controller {
    private readonly Model _model;
    private readonly MainView _mainView;
    private readonly SteamDiscovery _steamDiscovery;
    private readonly SteamGameMover _steamGameMover;
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private CopyProgressView _currentGameMoveOperationView;

    public Controller(Model model, MainView mainView) {
      _model = model;
      _mainView = mainView;
      _steamDiscovery = new SteamDiscovery();
      _steamGameMover = new SteamGameMover();
      _steamGameMover.CopyingFile += (sender, args) => {
        Trace.WriteLine($"Copying file \"{args.SourceFile.FullName}\" to \"{args.DestinationFile.FullName}\"");
      };
      _steamGameMover.DeletingFile += (sender, args) => {
        Trace.WriteLine($"Deleting file \"{args.File.FullName}\"");
      };
    }

    public void Run() {
      _mainView.RefreshView += (sender, args) => FetchSteamConfigurationAsync();
      _mainView.CloseView += (sender, args) => Application.Current.Shutdown();
      _mainView.CopyGameInvoked += (sender, game) => MoveGameToOtherLibrary(game);
      _mainView.Run();
      FetchSteamConfigurationAsync();
    }

    private async void MoveGameToOtherLibrary(MoveGameEventArgs e) {
      if (_currentGameMoveOperationView != null) {
        _mainView.ShowError("Cannot move game because another game is currently being move.\r\n" +
          "Please wait for the operation to finish, or cancel it.");
        return;
      }

      if (e.Game.Location == null) {
        _mainView.ShowError("Cannot move game because it does not have a valid location.");
        return;
      }

      if (e.Game.AcfFile == null) {
        _mainView.ShowError("Cannot move game because it does not have a valid ACF file.");
        return;
      }

      var cancellationTokenSource = new CancellationTokenSource();
      _currentGameMoveOperationView = new CopyProgressView(new CopyProgressWindow());
      try {
        _currentGameMoveOperationView.Cancel += (sender, args) => cancellationTokenSource.Cancel();
        Action<MoveDirectoryInfo> progress = info => {
          _currentGameMoveOperationView.ReportProgress(info);
        };

        _currentGameMoveOperationView.Show();

        var destinationLibrary = new DirectoryInfo(e.DestinationLibraryPath);

        var result = await _steamGameMover.MoveSteamGameAsync(e.Game, destinationLibrary, progress, cancellationTokenSource.Token);

        if (result.Kind == SteamGameMover.MoveGameResultKind.Error) {
          _mainView.ShowError(string.Format("Error moving steam game to library \"{0}\":\r\n\r\n{1}", 
            destinationLibrary, result.Error.Message));
        } else if (result.Kind == SteamGameMover.MoveGameResultKind.Ok) {
          await _steamGameMover.DeleteAppCacheAsync(_model.SteamConfiguration);
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
          _model.SteamConfiguration.Location.Value = steamLocation;
        }

        if (_model.SteamConfiguration.Location.Value == null) {
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
