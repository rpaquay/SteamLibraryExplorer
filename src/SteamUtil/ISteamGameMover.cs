using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SteamLibraryExplorer.SteamModel;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.SteamUtil {
  public interface ISteamGameMover {
    event EventHandler<FileCopyEventArgs> CopyingFile;
    event EventHandler<PathEventArgs> DeletingFile;
    event EventHandler<PathEventArgs> CreatingDirectory;
    event EventHandler<PathEventArgs> DeletingDirectory;

    /// <summary>
    /// Move a steam game from one library to another.
    /// </summary>
    [NotNull]
    [ItemNotNull]
    Task<MoveGameResult> MoveSteamGameAsync(
      [NotNull]SteamGame game, [NotNull]FullPath destinationLibraryLocation,
      [NotNull]Action<MoveDirectoryInfo> progress, CancellationToken cancellationToken);

    [NotNull]
    Task DeleteAppCacheAsync([NotNull]SteamConfiguration configuration);
  }
}