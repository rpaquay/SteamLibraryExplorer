using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.SteamUtil {
  public class SteamMove {
    public Task<MoveGameResult> MoveSteamGameAsync(
      FileInfo acfFile, DirectoryInfo sourceDirectory, DirectoryInfo destinationDirectory,
      Action<MoveDirectoryInfo> progress, CancellationToken cancellationToken) {

      return RunAsync(() => {
        try {
          MoveSteamGame(acfFile, sourceDirectory, destinationDirectory, progress, cancellationToken);
          return MoveGameResult.CreateOk();
        } catch (Exception e) {
          return MoveGameResult.CreateError(e);
        }
      });
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

    private void MoveSteamGame(FileInfo acfFile, DirectoryInfo sourceDirectory, DirectoryInfo destinationDirectory, Action<MoveDirectoryInfo> progress, CancellationToken cancellationToken) {
      var info = new MoveDirectoryInfo {
        StartTime = DateTime.UtcNow,
        CurrentTime = DateTime.UtcNow,
      };
      ReportProgess(MovePhase.DiscoveringSourceFiles, progress, info);

      //TODO: REmove this
      if (destinationDirectory.Exists) {
        destinationDirectory.Delete(true);
      }

      acfFile.Refresh();
      if (!acfFile.Exists) {
        throw new ArgumentException($"ACF file \"{acfFile.FullName}\" does not exist");
      }
      sourceDirectory.Refresh();
      if (!sourceDirectory.Exists) {
        throw new ArgumentException($"Steam game directory \"{sourceDirectory.FullName}\" does not exist");
      }
      destinationDirectory.Refresh();
      if (destinationDirectory.Exists) {
        if (destinationDirectory.EnumerateFiles().Any()) {
          throw new ArgumentException($"Steam game destination directory \"{sourceDirectory.FullName}\" exists and is not empty");
        }
      }

      CollectDirectoryInfo(sourceDirectory, progress, info, cancellationToken);

      ReportProgess(MovePhase.CopyingFiles, progress, info);
      CopyDirectoryRecurse(sourceDirectory, destinationDirectory, progress, info, cancellationToken);
    }

    private static void ReportProgess(MovePhase phase, Action<MoveDirectoryInfo> progress, MoveDirectoryInfo info) {
      info.CurrentPhase = phase;
      info.CurrentTime = DateTime.UtcNow;

      var clone = new MoveDirectoryInfo {
        CurrentPhase = info.CurrentPhase,
        MovedFileCount = info.MovedFileCount,
        TotalFileCount = info.TotalFileCount,
        MovedDirectoryCount = info.MovedDirectoryCount,
        TotalDirectoryCount = info.TotalDirectoryCount,
        MovedBytes = info.MovedBytes,
        TotalBytes = info.TotalBytes,
        CurrentDirectory = info.CurrentDirectory,
        CurrentFile = info.CurrentFile,
        MovedBytesOfCurrentFile = info.MovedBytesOfCurrentFile,
        TotalBytesOfCurrentFile = info.TotalBytesOfCurrentFile,
        StartTime = info.StartTime,
        CurrentTime = info.CurrentTime,
      };
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
        info.MovedFileCount++;
        info.CurrentFile = sourceFile;
        var lastBytes = 0L;
        FileUtils.CopyFile(
          sourceFile.FullName,
          Path.Combine(destinationDirectory.FullName, sourceFile.Name),
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
      }

      // Copy sub-directories
      foreach (var sourceChild in sourceDirectory.EnumerateDirectories()) {
        var destinationChild = new DirectoryInfo(Path.Combine(destinationDirectory.FullName, sourceChild.Name));
        CopyDirectoryRecurse(sourceChild, destinationChild, progress, info, cancellationToken);
      }
    }

    private void CollectDirectoryInfo(DirectoryInfo sourceDirectory,
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
        CollectDirectoryInfo(dir, progress, info, cancellationToken);
      }
    }

    public enum MovePhase {
      DiscoveringSourceFiles,
      CopyingFiles,
      DeletingSourceDirectory,
    }

    public class MoveDirectoryInfo {
      public MovePhase CurrentPhase { get; set; }
      public long MovedFileCount { get; set; }
      public long TotalFileCount { get; set; }

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

      public TimeSpan ElapsedTime {
        get { return CurrentTime - StartTime; }
      }

      public TimeSpan EstimatedRemainingTime {
        get {
          if (TotalBytes == 0 || MovedBytes == 0 || ElapsedTime.TotalSeconds <= 1) {
            return TimeSpan.MaxValue;
          }
          // 0.8 means there is 80% of the work done
          double progressRatio = (double)MovedBytes / TotalBytes;
          Debug.Assert(progressRatio > 0);
          Debug.Assert(progressRatio <= 1.0);
          // If total seconds is 10 and progress is 0.8, then total time is 10 / 0.8 (12.5), and
          // remaining time is 12.5 - 10 = 2.5
          double estimatedTotalSeconds = ElapsedTime.TotalSeconds / progressRatio;
          double secondsRemaining = estimatedTotalSeconds - ElapsedTime.TotalSeconds;
          return TimeSpan.FromSeconds(secondsRemaining);
        }
      }
    }

    private static Task<T> RunAsync<T>(Func<T> func) {
      return Task.Run(() => func.Invoke());
    }
  }
}
