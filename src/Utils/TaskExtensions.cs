using System;
using System.Threading;
using System.Threading.Tasks;

namespace SteamLibraryExplorer.Utils {
  public static class TaskUtils {
    public static Task Run(Action someAction) {
      return Task.Factory.StartNew(someAction, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
    }
    public static Task<T> Run<T>(Func<T> someAction) {
      return Task.Factory.StartNew(someAction, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
    }
  }
}
