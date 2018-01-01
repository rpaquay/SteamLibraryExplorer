using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SteamLibraryExplorer.SteamModel;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.SteamUtil {
  public class SteamGameMover {
    public event EventHandler<FileCopyEventArgs> CopyingFile;
    public event EventHandler<FileDeleteEventArgs> DeletingFile;

    /// <summary>
    /// Move a steam game from one library to another.
    /// </summary>
    public Task<MoveGameResult> MoveSteamGameAsync(
      SteamGame game, DirectoryInfo destinationLibraryLocation,
      Action<MoveDirectoryInfo> progress, CancellationToken cancellationToken) {

      return RunAsync(() => {
        try {
          MoveSteamGame(game, destinationLibraryLocation, progress, cancellationToken);
          return MoveGameResult.CreateOk();
        }
        catch (Exception e) {
          return MoveGameResult.CreateError(e);
        }
      });
    }

    public Task DeleteAppCacheAsync(SteamConfiguration configuration) {
      return RunAsync(() => DeleteAppCache(configuration));
    }

    private void MoveSteamGame(
      SteamGame game, DirectoryInfo destinationLibraryLocation,
      Action<MoveDirectoryInfo> progress, CancellationToken cancellationToken) {

      game.AcfFile.FileInfo.Refresh();
      if (!game.AcfFile.FileInfo.Exists) {
        throw new ArgumentException($"ACF file \"{game.AcfFile.FileInfo.FullName}\" does not exist");
      }
      game.Location.Refresh();
      if (!game.Location.Exists) {
        throw new ArgumentException($"Source directory \"{game.Location.FullName}\" does not exist");
      }

      if (game.WorkshopFile != null) {
        game.WorkshopFile.FileInfo.Refresh();
        if (!game.WorkshopFile.FileInfo.Exists) {
          throw new ArgumentException($"Workshop ACF file \"{game.WorkshopFile.FileInfo.FullName}\" does not exist");
        }
      }

      // Note: The workshop diractory may not exist if there are no workshop files stored for the game
      //if (game.WorkshopLocation != null) {
      //  game.WorkshopLocation.Refresh();
      //  if (!game.WorkshopLocation.Exists) {
      //    throw new ArgumentException($"Workshop source directory \"{game.WorkshopLocation.FullName}\" does not exist");
      //  }
      //}

      var destinationAcfFile = destinationLibraryLocation.CombineDirectory("steamapps")
        .CombineFile(game.AcfFile.FileInfo.Name);
      if (destinationAcfFile.Exists) {
        throw new ArgumentException($"Game ACF file \"{destinationAcfFile.FullName}\" already exist");
      }

      var destinationGameLocation = destinationLibraryLocation.CombineDirectory("steamapps").CombineDirectory("common")
        .CombineDirectory(game.Location.Name);
      if (destinationGameLocation.Exists) {
        if (destinationGameLocation.EnumerateFileSystemInfos().Any()) {
          throw new ArgumentException(
            $"Destination directory \"{destinationGameLocation}\" already exists and is not empty");
        }
      }

      FileInfo destinationWorkshopFile = null;
      if (game.WorkshopFile != null) {
        destinationWorkshopFile = destinationLibraryLocation.CombineDirectory("steamapps").CombineDirectory("workshop")
          .CombineFile(game.WorkshopFile.FileInfo.Name);
        if (destinationWorkshopFile.Exists) {
          throw new ArgumentException($"Workshop ACF file \"{destinationWorkshopFile.FullName}\" already exist");
        }
      }

      DirectoryInfo destinatioWorkshopLocation = null;
      if (game.WorkshopFile != null) {
        destinatioWorkshopLocation = destinationLibraryLocation.CombineDirectory("steamapps")
          .CombineDirectory("workshop")
          .CombineDirectory("content").CombineDirectory(game.WorkshopFile.AppId);
        if (destinatioWorkshopLocation.Exists) {
          if (destinatioWorkshopLocation.EnumerateFileSystemInfos().Any()) {
            throw new ArgumentException(
              $"Destination directory \"{destinatioWorkshopLocation}\" already exists and is not empty");
          }
        }
      }

      var destinationInfo = new DestinationInfo(
        destinationAcfFile, destinationGameLocation,
        destinationWorkshopFile, destinatioWorkshopLocation);

      var moveInfo = new MoveDirectoryInfo {
        SourceDirectory = game.Location,
        DestinationDirectory = destinationGameLocation,
        StartTime = DateTime.UtcNow,
        CurrentTime = DateTime.UtcNow,
      };

      try {
        MoveSteamGameWorker(game, destinationInfo, progress, moveInfo, cancellationToken);
      }
      catch (OperationCanceledException) {
        RollbackPartialMove(destinationInfo, moveInfo, progress);
      }
    }

    private void MoveSteamGameWorker(SteamGame game, DestinationInfo destinationInfo,
      Action<MoveDirectoryInfo> progress, MoveDirectoryInfo moveInfo, CancellationToken cancellationToken) {

      // Gather info about all files and directories
      ReportProgess(MovePhase.DiscoveringSourceFiles, progress, moveInfo);
      DiscoverSourceDirectoryFiles(game.Location, progress, moveInfo, cancellationToken);
      DiscoverSourceDirectoryFiles(game.WorkshopLocation, progress, moveInfo, cancellationToken);
      cancellationToken.ThrowIfCancellationRequested();

      // Account for ACF File(s)
      moveInfo.TotalFileCount++;
      moveInfo.TotalBytes += game.AcfFile.FileInfo.Length;
      if (game.WorkshopFile != null) {
        moveInfo.TotalFileCount++;
        moveInfo.TotalBytes += game.WorkshopFile.FileInfo.Length;
      }

      // Copy game (and workshop) files
      CopyDirectoryRecurse(game.Location, destinationInfo.GameDirectory, progress, moveInfo, cancellationToken);
      CopyDirectoryRecurse(game.WorkshopLocation, destinationInfo.WorkshopDirectory, progress, moveInfo,
        cancellationToken);

      // Copy ACF file(s)
      CopySingleFile(game.AcfFile.FileInfo, destinationInfo.AcfFile, progress, moveInfo, cancellationToken);
      if (game.WorkshopFile != null) {
        CopySingleFile(game.WorkshopFile.FileInfo, destinationInfo.WorkshopFile, progress, moveInfo, cancellationToken);
      }
      cancellationToken.ThrowIfCancellationRequested();

      // Delete source directory and source acf file
      DeleteDirectoryRecurse(MovePhase.DeletingSourceDirectory, game.Location, progress, moveInfo);
      DeleteDirectoryRecurse(MovePhase.DeletingSourceDirectory, game.WorkshopLocation, progress, moveInfo);
      DeleteSingleFile(MovePhase.DeletingSourceDirectory, game.AcfFile.FileInfo, progress, moveInfo);
      if (game.WorkshopFile != null) {
        DeleteSingleFile(MovePhase.DeletingSourceDirectory, game.WorkshopFile.FileInfo, progress, moveInfo);
      }
    }

    private void RollbackPartialMove(DestinationInfo destinationInfo, MoveDirectoryInfo moveInfo,
      Action<MoveDirectoryInfo> progress) {
      DeleteSingleFile(MovePhase.DeletingDestinationAfterCancellation, destinationInfo.AcfFile, progress, moveInfo);
      DeleteDirectoryRecurse(MovePhase.DeletingDestinationAfterCancellation, destinationInfo.GameDirectory, progress,
        moveInfo);

      DeleteSingleFile(MovePhase.DeletingDestinationAfterCancellation, destinationInfo.WorkshopFile, progress,
        moveInfo);
      DeleteDirectoryRecurse(MovePhase.DeletingDestinationAfterCancellation, destinationInfo.WorkshopDirectory,
        progress, moveInfo);
    }

    private static void ReportProgess(MovePhase phase, Action<MoveDirectoryInfo> progress, MoveDirectoryInfo info) {
      info.CurrentPhase = phase;
      info.CurrentTime = DateTime.UtcNow;

      var clone = info.Clone();
      progress(clone);
    }

    private void CopyDirectoryRecurse(DirectoryInfo sourceDirectory, DirectoryInfo destinationDirectory,
      Action<MoveDirectoryInfo> progress, MoveDirectoryInfo info, CancellationToken cancellationToken) {

      if (sourceDirectory == null || !sourceDirectory.Exists) {
        return;
      }
      cancellationToken.ThrowIfCancellationRequested();

      info.CurrentDirectory = sourceDirectory;
      ReportProgess(MovePhase.CopyingFiles, progress, info);

      // Create destination directory
      if (!destinationDirectory.Exists) {
        destinationDirectory.Create();
      }

      // Copy files
      foreach (var sourceFile in sourceDirectory.EnumerateFiles()) {
        CopySingleFile(sourceFile, destinationDirectory.CombineFile(sourceFile.Name), progress, info,
          cancellationToken);
      }

      // Copy sub-directories
      foreach (var sourceChild in sourceDirectory.EnumerateDirectories()) {
        var destinationChild = destinationDirectory.CombineDirectory(sourceChild.Name);
        CopyDirectoryRecurse(sourceChild, destinationChild, progress, info, cancellationToken);
      }
      info.MovedDirectoryCount++;
    }

    private void CopySingleFile(FileInfo sourceFile, FileInfo destinationFile, Action<MoveDirectoryInfo> progress,
      MoveDirectoryInfo info, CancellationToken cancellationToken) {

      OnCopyingFile(new FileCopyEventArgs(sourceFile, destinationFile));

      info.CurrentFile = sourceFile;
      ReportProgess(MovePhase.CopyingFiles, progress, info);

      var lastBytes = 0L;
      FileUtils.CopyFile(
        sourceFile.FullName,
        destinationFile.FullName,
        sourceFile.Length >= 100 * 1024 * 1024,
        copyProgress => {
          info.TotalBytesOfCurrentFile = copyProgress.TotalFileSize;
          info.MovedBytesOfCurrentFile = copyProgress.TotalBytesTransferred;

          // Note: The progress callback report transferred bytes vs total
          // We need to transfer that into a delta to add to the total
          var deltaBytes = copyProgress.TotalBytesTransferred - lastBytes;
          lastBytes = copyProgress.TotalBytesTransferred;
          info.MovedBytes += deltaBytes;
          ReportProgess(MovePhase.CopyingFiles, progress, info);
        },
        cancellationToken);

      info.MovedFileCount++;
    }

    private void DeleteDirectoryRecurse(MovePhase phase, DirectoryInfo directory, Action<MoveDirectoryInfo> progress,
      MoveDirectoryInfo info) {
      if (directory == null || !directory.Exists) {
        return;
      }

      info.CurrentDirectory = directory;
      ReportProgess(phase, progress, info);

      // Delete files
      foreach (var sourceFile in directory.EnumerateFiles()) {
        DeleteSingleFile(phase, sourceFile, progress, info);
      }

      // Delete sub-directories
      foreach (var sourceChild in directory.EnumerateDirectories()) {
        DeleteDirectoryRecurse(phase, sourceChild, progress, info);
      }

      // Delete directory
      directory.Delete();

      info.DeletedDirectoryCount++;
    }

    private void DeleteSingleFile(MovePhase phase, FileInfo file, Action<MoveDirectoryInfo> progress,
      MoveDirectoryInfo info) {
      if (file == null) {
        return;
      }

      OnDeletingFile(new FileDeleteEventArgs(file));
      info.CurrentFile = file;
      ReportProgess(phase, progress, info);

      file.Delete();

      info.DeletedFileCount++;
    }

    private void DiscoverSourceDirectoryFiles(DirectoryInfo sourceDirectory,
      Action<MoveDirectoryInfo> progress, MoveDirectoryInfo info, CancellationToken cancellationToken) {
      cancellationToken.ThrowIfCancellationRequested();
      if (sourceDirectory == null || !sourceDirectory.Exists) {
        return;
      }

      info.CurrentDirectory = sourceDirectory;
      ReportProgess(MovePhase.DiscoveringSourceFiles, progress, info);

      foreach (var file in sourceDirectory.EnumerateFiles()) {
        info.TotalFileCount++;
        info.TotalBytes += file.Length;
      }
      foreach (var dir in sourceDirectory.EnumerateDirectories()) {
        DiscoverSourceDirectoryFiles(dir, progress, info, cancellationToken);
      }

      info.TotalDirectoryCount++;
    }

    private void DeleteAppCache(SteamConfiguration configuration) {
      if (configuration.Location.Value == null) {
        throw new ArgumentException("Steam location is not known, cannot delete appcache file");
      }
      var file = configuration.Location.Value.CombineDirectory("appcache").CombineFile("appinfo.vdf");
      file.Delete();
    }

    private static Task<T> RunAsync<T>(Func<T> func) {
      return Task.Run(() => func.Invoke());
    }

    private static Task RunAsync(Action func) {
      return Task.Run(func);
    }

    public enum MoveGameResultKind {
      Ok,
      Error,
      Cancelled
    }

    protected virtual void OnCopyingFile(FileCopyEventArgs e) {
      CopyingFile?.Invoke(this, e);
    }

    protected virtual void OnDeletingFile(FileDeleteEventArgs e) {
      DeletingFile?.Invoke(this, e);
    }

    private class DestinationInfo {
      public DestinationInfo(FileInfo acfFile, DirectoryInfo gameDirectory, FileInfo workshopFile, DirectoryInfo workshopDirectory) {
        AcfFile = acfFile;
        GameDirectory = gameDirectory;
        WorkshopFile = workshopFile;
        WorkshopDirectory = workshopDirectory;
      }

      public FileInfo AcfFile { get; }
      public DirectoryInfo GameDirectory { get; }
      public FileInfo WorkshopFile { get; }
      public DirectoryInfo WorkshopDirectory { get; }
    }
  }
}