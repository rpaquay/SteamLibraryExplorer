using System;
using SteamLibraryExplorer.SteamUtil;
using SteamLibraryExplorer.UserInterface;
using SteamLibraryExplorer.Utils;
using SteamLibraryExplorer.ViewModel;

namespace SteamLibraryExplorer {
  public class CopyProgressView {
    private readonly CopyProgressWindow _progressWindow;
    private readonly ThrottledDispatcher _throttledDispatcher = new ThrottledDispatcher();
    private CopyProgressViewModel _viewModel;

    public CopyProgressView(CopyProgressWindow progressWindow) {
      _progressWindow = progressWindow;
      _viewModel = (CopyProgressViewModel)_progressWindow.DataContext;
    }

    public event EventHandler Cancel;

    public void Show() {
      _throttledDispatcher.Start(TimeSpan.FromMilliseconds(25));
      _progressWindow.Show();
    }

    public void Close() {
      _progressWindow.Close();
    }

    protected virtual void OnCancel() {
      Cancel?.Invoke(this, EventArgs.Empty);
    }

    public void ReportProgress(SteamMove.MoveDirectoryInfo info) {
      _throttledDispatcher.Enqeue(nameof(ReportProgress), () => {
        UpdateViewModel(info);
      });
    }

    private void UpdateViewModel(SteamMove.MoveDirectoryInfo info) {
      switch (info.CurrentPhase) {
        case SteamMove.MovePhase.DiscoveringSourceFiles:
          _viewModel.MessageText = "Discovering files and directories";
          break;
        case SteamMove.MovePhase.CopyingFiles:
          _viewModel.MessageText = "Copying files to destination directory";
          break;
        case SteamMove.MovePhase.DeletingSourceDirectory:
          _viewModel.MessageText = "Deleting source directory";
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }

      _viewModel.CurrentFilePath = info.CurrentFile?.Name + string.Format(" ({0:n0} of {1:n0})", info.MovedFileCount, info.TotalFileCount) ?? "";

      _viewModel.CurrentFileProgressPercent = ProgressValue(info.MovedBytesOfCurrentFile, info.TotalBytesOfCurrentFile);
      _viewModel.CurrentFileProgressText = string.Format("{0:n0} of {1:n0} bytes", info.MovedBytesOfCurrentFile, info.TotalBytesOfCurrentFile);

      _viewModel.TotalProgressPercent = ProgressValue(info.MovedBytes, info.TotalBytes);
      _viewModel.TotalProgressText = string.Format("{0:n0} of {1:n0} bytes", info.MovedBytes, info.TotalBytes);

      _viewModel.ElapsedTime = TimeSpanText(info.ElapsedTime);
      _viewModel.RemainingTime = TimeSpanText(info.EstimatedRemainingTime);
    }

    private string TimeSpanText(TimeSpan span) {
      if (span == TimeSpan.MaxValue)
        return "-";

      string formatted = string.Format("{0}{1}{2}{3}",
        span.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? String.Empty : "s") : string.Empty,
        span.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? String.Empty : "s") : string.Empty,
        span.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", span.Minutes, span.Minutes == 1 ? String.Empty : "s") : string.Empty,
        span.Duration().Seconds > 0 ? string.Format("{0:0} second{1}", span.Seconds, span.Seconds == 1 ? String.Empty : "s") : string.Empty);

      if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

      if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

      return formatted;
    }

    public double ProgressValue(long current, long total) {
      if (total == 0) {
        return 0;
      }

      return (double)current / total;
    }
  }
}