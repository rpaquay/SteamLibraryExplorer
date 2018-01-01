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
      FileInfo sourceAcfFile, DirectoryInfo sourceDirectory,
      FileInfo destinationAcfFile, DirectoryInfo destinationDirectory,
      Action<MoveDirectoryInfo> progress, CancellationToken cancellationToken) {

      return RunAsync(() => {
        try {
          MoveSteamGame(sourceAcfFile, sourceDirectory, destinationAcfFile, destinationDirectory, progress, cancellationToken);
          return MoveGameResult.CreateOk();
        } catch (Exception e) {
          return MoveGameResult.CreateError(e);
        }
      });
    }

    public Task DeleteAppCacheAsync(SteamConfiguration configuration) {
      return RunAsync(() => DeleteAppCache(configuration));
    }

    private void DeleteAppCache(SteamConfiguration configuration) {
      if (configuration.Location.Value == null) {
        throw new ArgumentException("Steam location is not known, cannot delete appcache file");
      }
      var file = configuration.Location.Value.CombineDirectory("appcache").CombineFile("appinfo.vdf");
      file.Delete();
    }

    private void MoveSteamGame(
      FileInfo sourceAcfFile, DirectoryInfo sourceDirectory,
      FileInfo destinationAcfFile, DirectoryInfo destinationDirectory,
      Action<MoveDirectoryInfo> progress, CancellationToken cancellationToken) {

      var info = new MoveDirectoryInfo {
        SourceDirectory = sourceDirectory,
        DestinationDirectory = destinationDirectory,
        StartTime = DateTime.UtcNow,
        CurrentTime = DateTime.UtcNow,
      };
      ReportProgess(MovePhase.DiscoveringSourceFiles, progress, info);

      sourceAcfFile.Refresh();
      if (!sourceAcfFile.Exists) {
        throw new ArgumentException($"ACF file \"{sourceAcfFile.FullName}\" does not exist");
      }
      sourceDirectory.Refresh();
      if (!sourceDirectory.Exists) {
        throw new ArgumentException($"Source directory \"{sourceDirectory.FullName}\" does not exist");
      }

      destinationAcfFile.Refresh();
      if (destinationAcfFile.Exists) {
        throw new ArgumentException($"ACF file \"{destinationAcfFile.FullName}\" already exist");
      }
      destinationDirectory.Refresh();
      if (destinationDirectory.Exists) {
        if (destinationDirectory.EnumerateFiles().Any()) {
          throw new ArgumentException($"Destination directory \"{sourceDirectory.FullName}\" already exists and is not empty");
        }
      }

      try {
        MoveSteamGameWorker(sourceAcfFile, sourceDirectory, destinationAcfFile, destinationDirectory, info, progress, cancellationToken);
      }
      catch (OperationCanceledException) {
        RollbackPartialMove(destinationDirectory, info, progress, cancellationToken);
      }
    }

    private void MoveSteamGameWorker(FileInfo sourceAcfFile, DirectoryInfo sourceDirectory, FileInfo destinationAcfFile,
      DirectoryInfo destinationDirectory, MoveDirectoryInfo info, Action<MoveDirectoryInfo> progress, CancellationToken cancellationToken) {

      ReportProgess(MovePhase.DiscoveringSourceFiles, progress, info);
      DiscoverSourceDirectoryFiles(sourceDirectory, progress, info, cancellationToken);
      cancellationToken.ThrowIfCancellationRequested();

      // Account for ACF File
      info.TotalFileCount++;
      info.TotalBytes += sourceAcfFile.Length;

      ReportProgess(MovePhase.CopyingFiles, progress, info);
      CopyDirectoryRecurse(sourceDirectory, destinationDirectory, progress, info, cancellationToken);
      cancellationToken.ThrowIfCancellationRequested();

      // Copy ACF file
      CopySingleFile(sourceAcfFile, destinationAcfFile, progress, info, cancellationToken);
      cancellationToken.ThrowIfCancellationRequested();

      // Delete source directory and source acf file
      DeleteDirectoryRecurse(MovePhase.DeletingSourceDirectory, sourceDirectory, progress, info);
      DeleteSingleFile(MovePhase.DeletingSourceDirectory, sourceAcfFile, progress, info);
    }

    private void RollbackPartialMove(DirectoryInfo destinationDirectory, MoveDirectoryInfo info, Action<MoveDirectoryInfo> progress, CancellationToken cancellationToken) {
      DeleteDirectoryRecurse(MovePhase.DeletingDestinationAfterCancellation, destinationDirectory, progress, info);
    }

    private static void ReportProgess(MovePhase phase, Action<MoveDirectoryInfo> progress, MoveDirectoryInfo info) {
      info.CurrentPhase = phase;
      info.CurrentTime = DateTime.UtcNow;

      var clone = info.Clone();
      progress(clone);
    }

    private void CopyDirectoryRecurse(DirectoryInfo sourceDirectory, DirectoryInfo destinationDirectory,
      Action<MoveDirectoryInfo> progress, MoveDirectoryInfo info, CancellationToken cancellationToken) {
      cancellationToken.ThrowIfCancellationRequested();

      // Create destination directory
      info.MovedDirectoryCount++;
      info.CurrentDirectory = sourceDirectory;
      if (!destinationDirectory.Exists) {
        destinationDirectory.Create();
      }
      ReportProgess(MovePhase.CopyingFiles, progress, info);

      // Copy files
      foreach (var sourceFile in sourceDirectory.EnumerateFiles()) {
        CopySingleFile(sourceFile, destinationDirectory.CombineFile(sourceFile.Name), progress, info, cancellationToken);
      }

      // Copy sub-directories
      foreach (var sourceChild in sourceDirectory.EnumerateDirectories()) {
        var destinationChild = destinationDirectory.CombineDirectory(sourceChild.Name);
        CopyDirectoryRecurse(sourceChild, destinationChild, progress, info, cancellationToken);
      }
    }

    private static void CopySingleFile(FileInfo sourceFile, FileInfo destinationFile, Action<MoveDirectoryInfo> progress,
      MoveDirectoryInfo info, CancellationToken cancellationToken) {

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

    private static void DeleteSingleFile(MovePhase phase, FileInfo file, Action<MoveDirectoryInfo> progress, MoveDirectoryInfo info) {
      info.CurrentFile = file;
      ReportProgess(phase, progress, info);

      file.Delete();

      info.DeletedFileCount++;
    }

    private void DiscoverSourceDirectoryFiles(DirectoryInfo sourceDirectory,
      Action<MoveDirectoryInfo> progress, MoveDirectoryInfo info, CancellationToken cancellationToken) {
      cancellationToken.ThrowIfCancellationRequested();

      info.TotalDirectoryCount++;
      info.CurrentDirectory = sourceDirectory;
      ReportProgess(MovePhase.DiscoveringSourceFiles, progress, info);

      foreach (var file in sourceDirectory.EnumerateFiles()) {
        info.TotalFileCount++;
        info.TotalBytes += file.Length;
      }
      foreach (var dir in sourceDirectory.EnumerateDirectories()) {
        DiscoverSourceDirectoryFiles(dir, progress, info, cancellationToken);
      }
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
  }
}
