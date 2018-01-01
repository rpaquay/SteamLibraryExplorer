using System;
using System.Diagnostics;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.SteamUtil {
  public class MoveDirectoryInfo {
    public FullPath SourceDirectory { get; set; }
    public FullPath DestinationDirectory { get; set; }
    public MovePhase CurrentPhase { get; set; }

    public long MovedFileCount { get; set; }
    public long TotalFileCount { get; set; }
    public long RemainingFileCount => TotalFileCount - MovedFileCount;
    public long RemainingBytes => TotalBytes - MovedBytes;

    public long MovedDirectoryCount { get; set; }
    public long TotalDirectoryCount { get; set; }

    public long MovedBytes { get; set; }
    public long TotalBytes { get; set; }

    public FullPath CurrentDirectory { get; set; }
    public FullPath CurrentFile { get; set; }
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
        var progressRatio = (double) MovedBytes / TotalBytes;
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
}