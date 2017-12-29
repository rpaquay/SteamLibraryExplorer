﻿using System.Threading;
using SteamLibraryExplorer.SteamUtil;

namespace SteamLibraryExplorer {
  class Controller {
    private readonly Model _model;
    private readonly View _view;
    private readonly SteamDiscovery _steamDiscovery;
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public Controller(Model model, View view) {
      _model = model;
      _view = view;
      _steamDiscovery = new SteamDiscovery();
    }

    public void Run() {
      _view.FirstShow += (sender, args) => FetchSteamConfigurationAsync();
      _view.RefreshView += (sender, args) => FetchSteamConfigurationAsync();
      _view.Run();
    }

    private async void FetchSteamConfigurationAsync() {
      // Cancel previous operation
      _cancellationTokenSource.Cancel();
      _cancellationTokenSource = new CancellationTokenSource();
      var cancellationToken = _cancellationTokenSource.Token;

      // Clear model and start a new collection process
      _model.SteamConfiguration.SteamLibraries.Clear();

      // Use new location (or previous valid location if there was one)
      var steamLocation = await _steamDiscovery.LocateSteamFolderAsync();
      if (steamLocation != null) {
        _model.SteamConfiguration.Location.Value = steamLocation;
      }

      if (_model.SteamConfiguration.Location.Value == null) {
        _view.ShowError(
          "Cannot locate Steam installation folder.\r\n\r\nTry starting the Steam application and select \"File > Refresh\".");
        return;
      }

      var mainLibrary = await _steamDiscovery.LoadMainLibraryAsync(steamLocation, cancellationToken);
      _model.SteamConfiguration.SteamLibraries.Add(mainLibrary);

      var libraries = await _steamDiscovery.LoadAdditionalLibrariesAsync(steamLocation, cancellationToken);
      foreach (var library in libraries) {
        _model.SteamConfiguration.SteamLibraries.Add(library);
      }

      // Start background tasks of discovering game size on disk
      await _steamDiscovery.DiscoverSizeOnDiskAsync(_model.SteamConfiguration.SteamLibraries, cancellationToken);
    }
  }
}
