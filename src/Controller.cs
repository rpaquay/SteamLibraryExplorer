using mtsuite.CoreFileSystem;
using SteamLibraryExplorer.SteamModel;
using SteamLibraryExplorer.SteamUtil;
using SteamLibraryExplorer.Utils;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FileSystem = SteamLibraryExplorer.Utils.FileSystem;

namespace SteamLibraryExplorer {
  public class Controller {
    private static readonly ILoggerFacade Logger = LoggerManagerFacade.GetLogger(typeof(Controller));

    private readonly Model _model;
    private readonly MainView _mainView;
    private readonly ISteamDiscovery _steamDiscovery;
    private readonly ISteamGameMover _steamGameMover;
    private readonly DispatcherTimer _lookForSteamTimer;
    private readonly TaskAwaiter _awaiter;
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private CopyProgressView _currentGameMoveOperationView;

    public Controller(Model model, MainView mainView) {
      _model = model;
      _mainView = mainView;
      _awaiter = new TaskAwaiter(TaskErrorHandler);
      _steamDiscovery = new SteamDiscovery();
      _steamGameMover = new SteamGameMover();
      _steamGameMover.CopyingFile += (sender, args) => { Logger.Info("Copying file \"{0}\" to \"{1}\"", args.SourcePath.FullName, args.DestinationPath.FullName); };
      _steamGameMover.DeletingFile += (sender, args) => { Logger.Info("Deleting file \"{0}\"", args.Path.FullName); };
      _steamGameMover.CreatingDirectory += (sender, args) => { Logger.Info("Creating directory \"{0}\"", args.Path.FullName); };
      _steamGameMover.DeletingDirectory += (sender, args) => { Logger.Info("Deleting directory \"{0}\"", args.Path.FullName); };
      _lookForSteamTimer = new DispatcherTimer();
    }

    private void TaskErrorHandler(Exception obj) {
      Logger.Error(obj, "Error executing asynchronous operation");
    }

    public void Run() {
      _lookForSteamTimer.Interval = TimeSpan.FromSeconds(2);
      _lookForSteamTimer.Tick += LookForSteamTimerOnTick;
      _lookForSteamTimer.Start();
      _mainView.RefreshView += (sender, args) => FetchSteamConfigurationAsync(false);
      _mainView.CloseView += (sender, args) => Application.Current.Shutdown();
      _mainView.CopyGameInvoked += (sender, game) => MoveGameToOtherLibrary(game);
      _mainView.Run();
      FetchSteamConfigurationAsync(true);
    }

    private void LookForSteamTimerOnTick(object o, EventArgs eventArgs) {
      if (_model.SteamConfiguration.Location.Value == null) {
        var steamLocationTask = _steamDiscovery.LocateSteamFolderAsync();
        _awaiter.Await(steamLocationTask, steamLocation => {
          if (steamLocation != null) {
            FetchSteamConfigurationAsync(true);
          }
        });
      }
    }

    private void FetchSteamConfigurationAsync(bool useCache) {
      // Cancel previous operation
      _cancellationTokenSource.Cancel();
      _cancellationTokenSource = new CancellationTokenSource();


      _awaiter.TryFinally(
        () => _mainView.StartProgress(),
        () => FetchSteamConfigurationAsyncBody(useCache),
        () => _mainView.StopProgress());
    }

    private Task FetchSteamConfigurationAsyncBody(bool useCache) {
      var cancellationToken = _cancellationTokenSource.Token;

      // Clear model and start a new collection process
      _model.SteamConfiguration.SteamLibraries.Clear();

      // Find steam location (installation directory)
      return _awaiter.CombineWith(_steamDiscovery.LocateSteamFolderAsync(), steamLocation => {
        // If no location found, re-use previously found one
        if (steamLocation == null) {
          steamLocation = _model.SteamConfiguration.Location.Value;
        }

        // Store location for re-use
        if (steamLocation != null) {
          Logger.Info("Found Steam installation directory: {0}", steamLocation.Value.FullName);
          _model.SteamConfiguration.Location.Value = steamLocation;
        }

        if (steamLocation == null) {
          _mainView.ShowError(
            "Cannot locate Steam installation folder.\r\n\r\n" +
            "Try starting the Steam application and select \"File > Refresh\".");
          throw new OperationCanceledException();
        }

        var mainLibraryTask = _steamDiscovery.LoadMainLibraryAsync(steamLocation.Value, cancellationToken);
        return _awaiter.CombineWith(mainLibraryTask, mainLibrary => {
          cancellationToken.ThrowIfCancellationRequested();

          _model.SteamConfiguration.SteamLibraries.Add(mainLibrary);

          var librariesTask = _steamDiscovery.LoadAdditionalLibrariesAsync(steamLocation.Value, cancellationToken);
          return _awaiter.CombineWith(librariesTask, libraries => {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var library in libraries) {
              _model.SteamConfiguration.SteamLibraries.Add(library);
            }

            // Start background tasks of discovering game size on disk
            return _steamDiscovery.DiscoverSizeOnDiskAsync(_model.SteamConfiguration.SteamLibraries, useCache, cancellationToken);
          });
        });
      });
    }

    private void MoveGameToOtherLibrary(MoveGameEventArgs e) {
      if (_currentGameMoveOperationView != null) {
        _mainView.ShowError("Cannot move game because another game is currently being moved.\r\n\r\n" +
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

      var destinationLibrary = new FullPath(e.DestinationLibraryPath);
      if (!FileSystem.DirectoryExists(destinationLibrary)) {
        _mainView.ShowError($"Cannot move game because the destination library path \"{destinationLibrary.FullName}\" does not exist.");
        return;
      }

      _awaiter.TryFinally(
        () => _currentGameMoveOperationView = new CopyProgressView(_mainView.CreateCopyPropgressWindow()),
        () => MoveGameToOtherLibraryAsync(e.Game, destinationLibrary),
        () => {
          _currentGameMoveOperationView.Close();
          _currentGameMoveOperationView = null;

          // Refresh view since things have changed
          FetchSteamConfigurationAsync(true);
        }
      );

    }

    private Task MoveGameToOtherLibraryAsync(SteamGame game, FullPath destinationLibrary) {
      var cancellationTokenSource = new CancellationTokenSource();

      // Show modeless progess dialog
      _currentGameMoveOperationView.Cancel += (sender, args) => cancellationTokenSource.Cancel();
      Action<MoveDirectoryInfo> progress = info => { _currentGameMoveOperationView.ReportProgress(info); };
      _currentGameMoveOperationView.Show();

      // Perform the (long running) move operation
      var moveSteamGameAsync = _steamGameMover.MoveSteamGameAsync(game, destinationLibrary, progress, cancellationTokenSource.Token);
      return _awaiter.CombineWith(moveSteamGameAsync, result => {
        // Check the result
        switch (result.Kind) {
          case MoveGameResultKind.Cancelled:
            // If user cancelled, there is nothing else to do
            return TaskUtils.CompletedTask;

          case MoveGameResultKind.Error:
            // If there was an error, display error to the user
            Debug.Assert(result.Error != null);
            _mainView.ShowError(
              $"Error moving steam game to library \"{destinationLibrary.FullName}\":\r\n\r\n" +
              $"{result.Error.Message}");
            return TaskUtils.CompletedTask;

          case MoveGameResultKind.Ok:
            //
            // If the move is complete, show success message and restart steam (if it is running)
            //

            // Delete the steam application cache so that Steam is aware things have changed
            // wrt to the location of games on disk. Steam will automatically rebuild the cache
            // next time is starts. It only takes a couple of seconds.
            return _awaiter.CombineWith(_steamGameMover.DeleteAppCacheAsync(_model.SteamConfiguration), () => {
              return _awaiter.CombineWith(_steamDiscovery.FindSteamProcessAsync(), steamProcess => {

                // Hide the progress dialog now, since we are about to show info messages
                _currentGameMoveOperationView.Close();

                // Display sucess message and restart Steam
                var successMessage =
                  $"The game \"{game.DisplayName}\" has been successfully moved to " +
                  $"the library located at \"{destinationLibrary.FullName}\".";
                if (steamProcess == null) {
                  _mainView.ShowInfo(successMessage);
                }
                else {
                  var steamExePath = new FullPath(steamProcess.MainModule.FileName);
                  var yes = _mainView.ShowYesNo(
                    successMessage + "\r\n\r\n" +
                    "Steam should be restarted now to make sure the new game location is taken into account.\r\n\r\n" +
                    "Do you want to automatically restart Steam now?");

                  if (yes) {
                    return _awaiter.CombineWith(_steamDiscovery.RestartSteamAsync(steamExePath), success => {
                      if (success) {
                        _mainView.ShowInfo("Steam was successfully restarted");
                      }
                      else {
                        _mainView.ShowInfo("Steam could not be restarted. Please restart manually.");
                      }
                      return TaskUtils.CompletedTask;
                    });
                  }
                }
                return TaskUtils.CompletedTask;
              });
            });
          default:
            throw new ArgumentOutOfRangeException();
        }
      });
    }
  }
}