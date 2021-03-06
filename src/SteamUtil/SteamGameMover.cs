﻿using JetBrains.Annotations;
using mtsuite.CoreFileSystem;
using SteamLibraryExplorer.SteamModel;
using SteamLibraryExplorer.Utils;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileSystem = SteamLibraryExplorer.Utils.FileSystem;

namespace SteamLibraryExplorer.SteamUtil {
  public class SteamGameMover : ISteamGameMover {
    public event EventHandler<FileCopyEventArgs> CopyingFile;
    public event EventHandler<PathEventArgs> DeletingFile;
    public event EventHandler<PathEventArgs> CreatingDirectory;
    public event EventHandler<PathEventArgs> DeletingDirectory;

    /// <summary>
    /// Move a steam game from one library to another.
    /// </summary>
    [NotNull]
    public Task<MoveGameResult> MoveSteamGameAsync(
      [NotNull]SteamGame game, [NotNull]FullPath destinationLibraryLocation,
      [NotNull]Action<MoveDirectoryInfo> progress, CancellationToken cancellationToken) {

      return RunAsync(() => {
        try {
          MoveSteamGame(game, destinationLibraryLocation, progress, cancellationToken);
          return MoveGameResult.CreateOk();
        } catch (Exception e) {
          return MoveGameResult.CreateError(e);
        }
      });
    }

    [NotNull]
    public Task DeleteAppCacheAsync([NotNull]SteamConfiguration configuration) {
      return RunAsync(() => DeleteAppCache(configuration));
    }

    private void MoveSteamGame([NotNull]SteamGame game, [NotNull]FullPath destinationLibraryLocation, [NotNull]Action<MoveDirectoryInfo> progress, CancellationToken cancellationToken) {

      if (game.AcfFile != null && !FileSystem.FileExists(game.AcfFile.Path)) {
        throw new ArgumentException($"ACF file \"{game.AcfFile.Path.FullName}\" does not exist");
      }
      if (!FileSystem.DirectoryExists(game.Location)) {
        throw new ArgumentException($"Source directory \"{game.Location.FullName}\" does not exist");
      }

      if (game.WorkshopFile != null) {
        if (!FileSystem.FileExists(game.WorkshopFile.Path)) {
          throw new ArgumentException($"Workshop ACF file \"{game.WorkshopFile.Path.FullName}\" does not exist");
        }
      }

      // Note: The workshop diractory may not exist if there are no workshop files stored for the game
      //if (game.WorkshopLocation != null) {
      //  if (!game.WorkshopLocation.DirectoryExists) {
      //    throw new ArgumentException($"Workshop source directory \"{game.WorkshopLocation.FullName}\" does not exist");
      //  }
      //}

      FullPath? destinationAcfFile = null;
      if (game.AcfFile != null) {
        destinationAcfFile = destinationLibraryLocation.Combine("steamapps", game.AcfFile.Path.Name);
        if (FileSystem.FileExists(destinationAcfFile.Value)) {
          throw new ArgumentException($"Game ACF file \"{destinationAcfFile.Value.FullName}\" already exist");
        }
      }

      var destinationGameLocation = destinationLibraryLocation.Combine("steamapps", "common", game.Location.Name);
      if (FileSystem.DirectoryExists(destinationGameLocation)) {
        if (FileSystem.EnumerateEntries(destinationGameLocation).Any()) {
          throw new ArgumentException(
            $"Destination directory \"{destinationGameLocation}\" already exists and is not empty");
        }
      }

      FullPath? destinationWorkshopFile = null;
      if (game.WorkshopFile != null) {
        destinationWorkshopFile = destinationLibraryLocation.Combine("steamapps", "workshop", game.WorkshopFile.Path.Name);
        if (FileSystem.FileExists(destinationWorkshopFile.Value)) {
          throw new ArgumentException($"Workshop ACF file \"{destinationWorkshopFile.Value.FullName}\" already exist");
        }
      }

      FullPath? destinatioWorkshopLocation = null;
      if (game.WorkshopFile != null && game.WorkshopFile.AppId != null) {
        destinatioWorkshopLocation = destinationLibraryLocation.Combine("steamapps", "workshop", "content", game.WorkshopFile.AppId);
        if (FileSystem.DirectoryExists(destinatioWorkshopLocation.Value)) {
          if (FileSystem.EnumerateEntries(destinatioWorkshopLocation.Value).Any()) {
            throw new ArgumentException(
              $"Destination directory \"{destinatioWorkshopLocation}\" already exists and is not empty");
          }
        }
      }

      var destinationInfo = new DestinationInfo(
        destinationAcfFile, destinationGameLocation,
        destinationWorkshopFile, destinatioWorkshopLocation);

      var moveInfo = new MoveDirectoryInfo(game.Location, destinationGameLocation) {
        StartTime = DateTime.UtcNow,
        CurrentTime = DateTime.UtcNow,
      };

      try {
        MoveSteamGameWorker(game, destinationInfo, progress, moveInfo, cancellationToken);
      } catch (OperationCanceledException) {
        RollbackPartialMove(destinationInfo, moveInfo, progress);
        throw;
      }
    }

    private void MoveSteamGameWorker([NotNull]SteamGame game, [NotNull]DestinationInfo destinationInfo,
      [NotNull]Action<MoveDirectoryInfo> progress, [NotNull]MoveDirectoryInfo moveInfo, CancellationToken cancellationToken) {

      // Gather info about all files and directories
      ReportProgess(MovePhase.DiscoveringSourceFiles, progress, moveInfo);
      DiscoverSourceDirectoryFiles(game.Location, progress, moveInfo, cancellationToken);
      DiscoverSourceDirectoryFiles(game.WorkshopLocation, progress, moveInfo, cancellationToken);
      cancellationToken.ThrowIfCancellationRequested();

      // Account for ACF File(s)
      if (game.AcfFile != null) {
        moveInfo.TotalFileCount++;
        moveInfo.TotalBytes += FileSystem.GetFileSize(game.AcfFile.Path);
      }
      if (game.WorkshopFile != null) {
        moveInfo.TotalFileCount++;
        moveInfo.TotalBytes += FileSystem.GetFileSize(game.WorkshopFile.Path);
      }

      // Copy game (and workshop) files
      CopyDirectoryRecurse(game.Location, destinationInfo.GameDirectory, progress, moveInfo, cancellationToken);
      CopyDirectoryRecurse(game.WorkshopLocation, destinationInfo.WorkshopDirectory, progress, moveInfo, cancellationToken);

      // Copy ACF file(s)
      if (game.AcfFile != null && destinationInfo.AcfFile != null) {
        CopySingleFile(game.AcfFile.Path, destinationInfo.AcfFile.Value, progress, moveInfo, cancellationToken);
      }
      if (game.WorkshopFile != null && destinationInfo.WorkshopFile != null) {
        CopySingleFile(game.WorkshopFile.Path, destinationInfo.WorkshopFile.Value, progress, moveInfo, cancellationToken);
      }
      cancellationToken.ThrowIfCancellationRequested();

      // Delete source directory and source acf file
      DeleteDirectoryRecurse(MovePhase.DeletingSourceDirectory, game.Location, progress, moveInfo);
      DeleteDirectoryRecurse(MovePhase.DeletingSourceDirectory, game.WorkshopLocation, progress, moveInfo);

      if (game.AcfFile != null) {
        DeleteSingleFile(MovePhase.DeletingSourceDirectory, game.AcfFile.Path, progress, moveInfo);
      }
      if (game.WorkshopFile != null) {
        DeleteSingleFile(MovePhase.DeletingSourceDirectory, game.WorkshopFile.Path, progress, moveInfo);
      }
    }

    private void RollbackPartialMove([NotNull]DestinationInfo destinationInfo, [NotNull]MoveDirectoryInfo moveInfo, Action<MoveDirectoryInfo> progress) {
      // Delete game files
      DeleteDirectoryRecurse(MovePhase.DeletingDestinationAfterCancellation, destinationInfo.GameDirectory, progress, moveInfo);
      DeleteSingleFile(MovePhase.DeletingDestinationAfterCancellation, destinationInfo.AcfFile, progress, moveInfo);

      // Delete workshop files
      DeleteDirectoryRecurse(MovePhase.DeletingDestinationAfterCancellation, destinationInfo.WorkshopDirectory, progress, moveInfo);
      DeleteSingleFile(MovePhase.DeletingDestinationAfterCancellation, destinationInfo.WorkshopFile, progress, moveInfo);
    }

    private static void ReportProgess(MovePhase phase, [NotNull]Action<MoveDirectoryInfo> progress, [NotNull]MoveDirectoryInfo info) {
      info.CurrentPhase = phase;
      info.CurrentTime = DateTime.UtcNow;

      var clone = info.Clone();
      progress(clone);
    }

    private void CopyDirectoryRecurse([CanBeNull]FullPath? sourceDirectory, [CanBeNull]FullPath? destinationDirectory, [NotNull]Action<MoveDirectoryInfo> progress, [NotNull]MoveDirectoryInfo info, CancellationToken cancellationToken) {
      if (sourceDirectory == null || destinationDirectory == null || !FileSystem.DirectoryExists(sourceDirectory.Value)) {
        return;
      }
      cancellationToken.ThrowIfCancellationRequested();

      info.CurrentDirectory = sourceDirectory.Value;
      ReportProgess(MovePhase.CopyingFiles, progress, info);

      // Create destination directory
      if (!FileSystem.DirectoryExists(destinationDirectory.Value)) {
        OnCreatingDirectory(new PathEventArgs(destinationDirectory.Value));
        FileSystem.CreateDirectory(destinationDirectory.Value);
      }

      // Copy files
      var entries = FileSystem.EnumerateEntries(sourceDirectory.Value).ToList();
      foreach (var childFile in entries.Where(x => x.IsFile)) {
        CopySingleFile(childFile, destinationDirectory.Value.Combine(childFile.Name), progress, info, cancellationToken);
      }

      // Copy sub-directories
      foreach (var childDirectory in entries.Where(x => x.IsDirectory)) {
        CopyDirectoryRecurse(childDirectory.Path, destinationDirectory.Value.Combine(childDirectory.Name), progress, info, cancellationToken);
      }
      info.MovedDirectoryCount++;
    }

    private void CopySingleFile([NotNull] FullPath sourceFile, [NotNull] FullPath destinationFile, [NotNull] Action<MoveDirectoryInfo> progress, [NotNull] MoveDirectoryInfo info, CancellationToken cancellationToken) {
      CopySingleFile(FileSystem.GetEntry(sourceFile), destinationFile, progress, info, cancellationToken);
    }

    private void CopySingleFile(FileSystemEntry sourceFile, [NotNull]FullPath destinationFile, [NotNull]Action<MoveDirectoryInfo> progress, [NotNull]MoveDirectoryInfo info, CancellationToken cancellationToken) {
      OnCopyingFile(new FileCopyEventArgs(sourceFile.Path, destinationFile));

      info.CurrentFile = sourceFile.Path;
      ReportProgess(MovePhase.CopyingFiles, progress, info);

      var lastBytes = 0L;
      var options = CopyFileOptions.Default;

      // Note: Unbuffered slows down dramatically on SSD drives
      //if (sourceFile.FileSize >= 100 * 1024 * 1024) {
      //  options |= CopyFileOptions.Unbuffered;
      //}

      FileSystem.CopyFile(sourceFile, destinationFile, options, (totalBytesTransferred, totalBytes) => {
        cancellationToken.ThrowIfCancellationRequested();

        info.TotalBytesOfCurrentFile = totalBytes;
        info.MovedBytesOfCurrentFile = totalBytesTransferred;

        // Note: The progress callback report transferred bytes vs total
        // We need to transfer that into a delta to add to the total
        var deltaBytes = totalBytesTransferred - lastBytes;
        lastBytes = totalBytesTransferred;
        info.MovedBytes += deltaBytes;
        ReportProgess(MovePhase.CopyingFiles, progress, info);
      });

      info.MovedFileCount++;
    }

    private void DeleteDirectoryRecurse(MovePhase phase, [CanBeNull]FullPath? directory, [NotNull]Action<MoveDirectoryInfo> progress, [NotNull]MoveDirectoryInfo info) {
      if (directory == null || !FileSystem.DirectoryExists(directory.Value)) {
        return;
      }

      info.CurrentDirectory = directory.Value;
      ReportProgess(phase, progress, info);

      var entries = FileSystem.EnumerateEntries(directory.Value).ToList();
      // Delete files
      foreach (var sourceFile in entries.Where(x => x.IsFile)) {
        DeleteSingleFile(phase, sourceFile.Path, progress, info);
      }

      // Delete sub-directories
      foreach (var sourceChild in entries.Where(x => x.IsDirectory)) {
        DeleteDirectoryRecurse(phase, sourceChild.Path, progress, info);
      }

      // Delete directory
      OnDeletingDirectory(new PathEventArgs(directory.Value));
      FileSystem.DeleteDirectory(directory.Value);

      info.DeletedDirectoryCount++;
    }

    private void DeleteSingleFile(MovePhase phase, [CanBeNull]FullPath? file, [NotNull]Action<MoveDirectoryInfo> progress, [NotNull]MoveDirectoryInfo info) {
      if (file == null) {
        return;
      }

      OnDeletingFile(new PathEventArgs(file.Value));
      info.CurrentFile = file.Value;
      ReportProgess(phase, progress, info);

      FileSystem.DeleteFile(file.Value);

      info.DeletedFileCount++;
    }

    private void DiscoverSourceDirectoryFiles([CanBeNull]FullPath? sourceDirectory, [NotNull]Action<MoveDirectoryInfo> progress, [NotNull]MoveDirectoryInfo info, CancellationToken cancellationToken) {
      cancellationToken.ThrowIfCancellationRequested();
      if (sourceDirectory == null || !FileSystem.DirectoryExists(sourceDirectory.Value)) {
        return;
      }

      info.CurrentDirectory = sourceDirectory.Value;
      ReportProgess(MovePhase.DiscoveringSourceFiles, progress, info);

      foreach (var entry in FileSystem.EnumerateEntries(sourceDirectory.Value)) {
        if (entry.IsFile) {
          info.TotalFileCount++;
          info.TotalBytes += entry.FileSize;
        }

        if (entry.IsDirectory) {
          DiscoverSourceDirectoryFiles(entry.Path, progress, info, cancellationToken);
        }
      }

      info.TotalDirectoryCount++;
    }

    private void DeleteAppCache([NotNull]SteamConfiguration configuration) {
      if (configuration.Location.Value == null) {
        throw new ArgumentException("Steam location is not known, cannot delete appcache file");
      }
      var file = configuration.Location.Value?.Combine("appcache").Combine("appinfo.vdf");
      FileSystem.DeleteFile(file.Value);
    }

    [NotNull]
    private static Task<T> RunAsync<T>([NotNull]Func<T> func) {
      return TaskUtils.Run(() => func.Invoke());
    }

    [NotNull]
    private static Task RunAsync([NotNull]Action func) {
      return TaskUtils.Run(func);
    }

    protected virtual void OnCopyingFile([NotNull]FileCopyEventArgs e) {
      CopyingFile?.Invoke(this, e);
    }

    protected virtual void OnDeletingFile([NotNull]PathEventArgs e) {
      DeletingFile?.Invoke(this, e);
    }

    private class DestinationInfo {
      public DestinationInfo([CanBeNull]FullPath? acfFile, [NotNull]FullPath gameDirectory, [CanBeNull]FullPath? workshopFile, [CanBeNull]FullPath? workshopDirectory) {
        AcfFile = acfFile;
        GameDirectory = gameDirectory;
        WorkshopFile = workshopFile;
        WorkshopDirectory = workshopDirectory;
      }

      [NotNull]
      public FullPath GameDirectory { get; }
      [CanBeNull]
      public FullPath? AcfFile { get; }
      [CanBeNull]
      public FullPath? WorkshopFile { get; }
      [CanBeNull]
      public FullPath? WorkshopDirectory { get; }
    }

    protected virtual void OnCreatingDirectory([NotNull]PathEventArgs e) {
      CreatingDirectory?.Invoke(this, e);
    }

    protected virtual void OnDeletingDirectory([NotNull]PathEventArgs e) {
      DeletingDirectory?.Invoke(this, e);
    }
  }
}