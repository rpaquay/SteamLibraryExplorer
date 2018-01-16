using System;
using System.Threading;
using System.Threading.Tasks;

namespace SteamLibraryExplorer.Utils {
  public static class TaskUtils {
    //private static readonly TaskScheduler Scheduler = new LimitedConcurrencyLevelTaskScheduler(8);
    private static readonly TaskScheduler Scheduler = TaskScheduler.Default;

    /// <summary>
    /// Create a new task for <paramref name="someAction"/> and run in on the default scheduler.
    /// </summary>
    public static Task Run(Action someAction) {
      return Run(someAction, CancellationToken.None);
    }

    /// <summary>
    /// Create a new task for <paramref name="someAction"/> and run in on the default scheduler.
    /// </summary>
    public static Task Run(Action someAction, CancellationToken token) {
      return Task.Factory.StartNew(someAction, token, TaskCreationOptions.None, Scheduler);
    }

    /// <summary>
    /// Create a new task for <paramref name="someAction"/> and run in on the default scheduler.
    /// </summary>
    public static Task<T> Run<T>(Func<T> someAction) {
      return Run(someAction, CancellationToken.None);
    }

    public static Task<T> Run<T>(Func<T> someAction, CancellationToken token) {
      return Task.Factory.StartNew(someAction, token, TaskCreationOptions.None, Scheduler);
    }

    public static Task CompletedTask {
      get {
        var tcs = new TaskCompletionSource<object>();
        tcs.SetResult(null);
        return tcs.Task;
      }
    }
  }
}
