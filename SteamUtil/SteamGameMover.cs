using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SteamLibraryExplorer.SteamModel;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.SteamUtil {
  public class SteamGameMover {
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
        } catch (Exception e) {
          return MoveGameResult.CreateError(e);
        }
      });
    }

    public event EventHandler<FileCopyEventArgs> CopyingFile;
    public event EventHandler<FileDeleteEventArgs> DeletingFile;

    public class FileCopyEventArgs {
      public FileCopyEventArgs(FileInfo sourceFile, FileInfo destinationFile) {
        SourceFile = sourceFile;
        DestinationFile = destinationFile;
      }

      public FileInfo SourceFile { get; }
      public FileInfo DestinationFile { get; }
    }

    public class FileDeleteEventArgs {
      public FileDeleteEventArgs(FileInfo file) {
        File = file;
      }

      public FileInfo File { get; }
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
      if (game.WorkshopLocation != null) {
        game.WorkshopLocation.Refresh();
        if (!game.WorkshopLocation.Exists) {
          throw new ArgumentException($"Workshop source directory \"{game.WorkshopLocation.FullName}\" does not exist");
        }
      }

      var destinationAcfFile = destinationLibraryLocation.CombineDirectory("steamapps").CombineFile(game.AcfFile.FileInfo.Name);
      if (destinationAcfFile.Exists) {
        throw new ArgumentException($"Game ACF file \"{destinationAcfFile.FullName}\" already exist");
      }

      var destinationGameLocation = destinationLibraryLocation.CombineDirectory("steamapps").CombineDirectory("common").CombineDirectory(game.Location.Name);
      if (destinationGameLocation.Exists) {
        if (destinationGameLocation.EnumerateFileSystemInfos().Any()) {
          throw new ArgumentException($"Destination directory \"{destinationGameLocation}\" already exists and is not empty");
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
        destinatioWorkshopLocation = destinationLibraryLocation.CombineDirectory("steamapps").CombineDirectory("workshop")
          .CombineDirectory("content").CombineDirectory(game.WorkshopFile.AppId);
        if (destinatioWorkshopLocation.Exists) {
          if (destinatioWorkshopLocation.EnumerateFileSystemInfos().Any()) {
            throw new ArgumentException(
              $"Destination directory \"{destinatioWorkshopLocation}\" already exists and is not empty");
          }
        }
      }

      var info = new MoveDirectoryInfo {
        SourceDirectory = game.Location,
        DestinationDirectory = destinationGameLocation,
        StartTime = DateTime.UtcNow,
        CurrentTime = DateTime.UtcNow,
      };

      try {
        MoveSteamGameWorker(game, destinationAcfFile, destinationGameLocation,
          destinationWorkshopFile, destinatioWorkshopLocation,
          info, progress, cancellationToken);
      } catch (OperationCanceledException) {
        RollbackPartialMove(destinationAcfFile, destinationGameLocation, destinationWorkshopFile, destinatioWorkshopLocation, info, progress, cancellationToken);
      }
    }

    private void MoveSteamGameWorker(SteamGame game, FileInfo destinationAcfFile, DirectoryInfo destinationDirectory,
      FileInfo destinationWorkshopFile, DirectoryInfo destinatioWorkshopLocation,
      MoveDirectoryInfo info, Action<MoveDirectoryInfo> progress, CancellationToken cancellationToken) {

      // Gather info about all files and directories
      ReportProgess(MovePhase.DiscoveringSourceFiles, progress, info);
      DiscoverSourceDirectoryFiles(game.Location, progress, info, cancellationToken);
      DiscoverSourceDirectoryFiles(game.WorkshopLocation, progress, info, cancellationToken);
      cancellationToken.ThrowIfCancellationRequested();

      // Account for ACF File(s)
      info.TotalFileCount++;
      info.TotalBytes += game.AcfFile.FileInfo.Length;
      if (game.WorkshopFile != null) {
        info.TotalFileCount++;
        info.TotalBytes += game.WorkshopFile.FileInfo.Length;
      }

      // Copy game (and workshop) files
      CopyDirectoryRecurse(game.Location, destinationDirectory, progress, info, cancellationToken);
      CopyDirectoryRecurse(game.WorkshopLocation, destinatioWorkshopLocation, progress, info, cancellationToken);

      // Copy ACF file(s)
      CopySingleFile(game.AcfFile.FileInfo, destinationAcfFile, progress, info, cancellationToken);
      if (game.WorkshopFile != null) {
        CopySingleFile(game.WorkshopFile.FileInfo, destinationWorkshopFile, progress, info, cancellationToken);
      }
      cancellationToken.ThrowIfCancellationRequested();

      // Delete source directory and source acf file
      DeleteDirectoryRecurse(MovePhase.DeletingSourceDirectory, game.Location, progress, info);
      DeleteDirectoryRecurse(MovePhase.DeletingSourceDirectory, game.WorkshopLocation, progress, info);
      DeleteSingleFile(MovePhase.DeletingSourceDirectory, game.AcfFile.FileInfo, progress, info);
      if (game.WorkshopFile != null) {
        DeleteSingleFile(MovePhase.DeletingSourceDirectory, game.WorkshopFile.FileInfo, progress, info);
      }
    }

    private void RollbackPartialMove(FileInfo destinationAcfFile, DirectoryInfo destinationDirectory, FileInfo destinationWorkshopFile, DirectoryInfo destinatioWorkshopLocation, MoveDirectoryInfo info, Action<MoveDirectoryInfo> progress, CancellationToken cancellationToken) {
      DeleteSingleFile(MovePhase.DeletingDestinationAfterCancellation, destinationAcfFile, progress, info);
      DeleteDirectoryRecurse(MovePhase.DeletingDestinationAfterCancellation, destinationDirectory, progress, info);

      DeleteSingleFile(MovePhase.DeletingDestinationAfterCancellation, destinationWorkshopFile, progress, info);
      DeleteDirectoryRecurse(MovePhase.DeletingDestinationAfterCancellation, destinatioWorkshopLocation, progress, info);
    }

    private static void ReportProgess(MovePhase phase, Action<MoveDirectoryInfo> progress, MoveDirectoryInfo info) {
      info.CurrentPhase = phase;
      info.CurrentTime = DateTime.UtcNow;

      var clone = info.Clone();
      progress(clone);
    }

    private void CopyDirectoryRecurse(DirectoryInfo sourceDirectory, DirectoryInfo destinationDirectory,
      Action<MoveDirectoryInfo> progress, MoveDirectoryInfo info, CancellationToken cancellationToken) {

      if (sourceDirectory == null) {
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
        CopySingleFile(sourceFile, destinationDirectory.CombineFile(sourceFile.Name), progress, info, cancellationToken);
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

    private void DeleteDirectoryRecurse(MovePhase phase, DirectoryInfo directory, Action<MoveDirectoryInfo> progress, MoveDirectoryInfo info) {
      if (directory == null) {
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

    private void DeleteSingleFile(MovePhase phase, FileInfo file, Action<MoveDirectoryInfo> progress, MoveDirectoryInfo info) {
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
      if (sourceDirectory == null) {
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

    public enum MovePhase {
      DiscoveringSourceFiles,
      CopyingFiles,
      DeletingSourceDirectory,
      DeletingDestinationAfterCancellation,
    }

    public class MoveDirectoryInfo {
      public DirectoryInfo SourceDirectory { get; set; }
      public DirectoryInfo DestinationDirectory { get; set; }
      public MovePhase CurrentPhase { get; set; }

      public long MovedFileCount { get; set; }
      public long TotalFileCount { get; set; }
      public long RemainingFileCount => TotalFileCount - MovedFileCount;
      public long RemainingBytes => TotalBytes - MovedBytes;

      public long MovedDirectoryCount { get; set; }
      public long TotalDirectoryCount { get; set; }

      public long MovedBytes { get; set; }
      public long TotalBytes { get; set; }

      public DirectoryInfo CurrentDirectory { get; set; }
      public FileInfo CurrentFile { get; set; }
      public long MovedBytesOfCurrentFile { get; set; }
      public long TotalBytesOfCurrentFile { get; set; }

      public DateTime StartTime { get; set; }
      public DateTime CurrentTime { get; set; }

      public long DeletedDirectoryCount { get; set; }
      public long DeletedFileCount { get; set; }

      public long RemainingFileToDeleteCount {
        get { return MovedFileCount - DeletedFileCount; }
      }

      public MoveDirectoryInfo Clone() {
        return new MoveDirectoryInfo {
          SourceDirectory = SourceDirectory,
          DestinationDirectory = DestinationDirectory,
          CurrentPhase = CurrentPhase,
          MovedFileCount = MovedFileCount,
          TotalFileCount = TotalFileCount,
          MovedDirectoryCount = MovedDirectoryCount,
          TotalDirectoryCount = TotalDirectoryCount,
          MovedBytes = MovedBytes,
          TotalBytes = TotalBytes,
          CurrentDirectory = CurrentDirectory,
          CurrentFile = CurrentFile,
          MovedBytesOfCurrentFile = MovedBytesOfCurrentFile,
          TotalBytesOfCurrentFile = TotalBytesOfCurrentFile,
          StartTime = StartTime,
          CurrentTime = CurrentTime,
          DeletedDirectoryCount = DeletedDirectoryCount,
          DeletedFileCount = DeletedFileCount,
        };
      }


      public TimeSpan ElapsedTime => CurrentTime - StartTime;

      public TimeSpan EstimatedRemainingTime {
        get {
          var elapsedSeconds = ElapsedTime.TotalSeconds;
          if (TotalBytes == 0 || MovedBytes == 0 || elapsedSeconds <= 1) {
            return TimeSpan.MaxValue;
          }

          // 0.8 means there is 80% of the work done
          var progressRatio = (double)MovedBytes / TotalBytes;
          Debug.Assert(progressRatio >= 0.0);
          Debug.Assert(progressRatio <= 1.0);

          // If total seconds is 10 and progress is 0.8, then total time is 10 / 0.8 (12.5), and
          // remaining time is 12.5 - 10 = 2.5
          double estimatedTotalSeconds = elapsedSeconds / progressRatio;
          double estimatedSecondsRemaining = estimatedTotalSeconds - elapsedSeconds;
          return TimeSpan.FromSeconds(estimatedSecondsRemaining);
        }
      }
    }

    private static Task<T> RunAsync<T>(Func<T> func) {
      return Task.Run(() => func.Invoke());
    }

    private static Task RunAsync(Action func) {
      return Task.Run(func);
    }

    public class MoveGameResult {
      public MoveGameResult(MoveGameResultKind kind, Exception error) {
        Kind = kind;
        Error = error;
      }

      public static MoveGameResult CreateError(Exception e) {
        if (e is OperationCanceledException) {
          return new MoveGameResult(MoveGameResultKind.Cancelled, null);
        }
        return new MoveGameResult(MoveGameResultKind.Error, e);
      }

      public static MoveGameResult CreateOk() {
        return new MoveGameResult(MoveGameResultKind.Ok, null);
      }

      public MoveGameResultKind Kind { get; }
      public Exception Error { get; }
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
  }
}
