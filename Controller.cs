using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using SteamLibraryExplorer.SteamModel;
using SteamLibraryExplorer.SteamUtil;
using SteamLibraryExplorer.UserInterface;

namespace SteamLibraryExplorer {
  class Controller {
    private readonly Model _model;
    private readonly MainView _mainView;
    private readonly SteamDiscovery _steamDiscovery;
    private readonly SteamMove _steamMove;
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public Controller(Model model, MainView mainView) {
      _model = model;
      _mainView = mainView;
      _steamDiscovery = new SteamDiscovery();
      _steamMove = new SteamMove();
    }

    public void Run() {
      _mainView.RefreshView += (sender, args) => FetchSteamConfigurationAsync();
      _mainView.CloseView += (sender, args) => Application.Current.Shutdown();
      _mainView.CopyGameInvoked += (sender, game) => CopyGame(game);
      _mainView.Run();
      FetchSteamConfigurationAsync();
    }

    private async void CopyGame(SteamGame game) {
      var t = new CancellationTokenSource();
      var view = new CopyProgressView(new CopyProgressWindow());
      try {
        view.Cancel += (sender, args) => t.Cancel();
        Action<SteamMove.MoveDirectoryInfo> progress = info => {
          view.ReportProgress(info);
        };

        view.Show();

        //TODO: Remove this
        var destinationDirectory = new DirectoryInfo(Path.Combine("e:\\", "test"));

        var result = await _steamMove.MoveSteamGameAsync(
          game.AcfFile.FileInfo,
          game.Location,
          destinationDirectory,
          progress,
          t.Token);

        if (result.Kind == SteamMove.MoveGameResultKind.Error) {
          _mainView.ShowError(string.Format("Error copying steam game:\r\n\r\n{0}", result.Error.Message));
        }
      }
      finally {
        view.Close();
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

        // Use new location (or previous valid location if there was one)
        var steamLocation = await _steamDiscovery.LocateSteamFolderAsync();
        if (cancellationToken.IsCancellationRequested) {
          return;
        }
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
