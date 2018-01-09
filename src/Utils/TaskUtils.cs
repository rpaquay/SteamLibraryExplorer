using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace SteamLibraryExplorer.Utils {
  public static class TaskUtils {
    public static Task Run(Action someAction) {
      return Task.Factory.StartNew(someAction, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
    }
    public static Task<T> Run<T>(Func<T> someAction) {
      return Task.Factory.StartNew(someAction, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
    }

    public static Task CompletedTask {
      get {
        var tcs = new TaskCompletionSource<object>();
        tcs.SetResult(null);
        return tcs.Task;
      }
    }
  }

  public class TaskAwaiter {
    private readonly Action<Exception> _errorHandler;
    private readonly TaskScheduler _scheduler = TaskScheduler.FromCurrentSynchronizationContext();

    public TaskAwaiter(Action<Exception> errorHandler) {
      _errorHandler = errorHandler;
    }

    public Task Await<T>(Task<T> task, Action<T> action) {
      return task.ContinueWith(t => {
        if (t.IsCanceled) {
          return;
        }
        if (t.IsFaulted) {
          _errorHandler(t.Exception);
          return;
        }
        action(t.Result);
      }, _scheduler);
    }

    public Task<TNewResult> CombineWith<T, TNewResult>(Task<T> task, Func<T, Task<TNewResult>> continuationFunction) {
      var tcs = new TaskCompletionSource<TNewResult>();
      task.ContinueWith(t => {
        if (t.IsCanceled) {
          tcs.TrySetCanceled();
          return;
        }
        if (t.IsFaulted) {
          _errorHandler(t.Exception);
          tcs.TrySetException(t.Exception);
          return;
        }
        continuationFunction(t.Result).ContinueWith(t2 => {
          if (t2.IsCanceled) {
            tcs.TrySetCanceled();
          } else if (t2.IsFaulted) {
            _errorHandler(t2.Exception);
            tcs.TrySetException(t2.Exception);
          } else {
            tcs.TrySetResult(t2.Result);
          }
        });
      }, _scheduler);
      return tcs.Task;
    }

    public Task CombineWith<T>(Task<T> task, Func<T, Task> continuationFunction) {
      var tcs = new TaskCompletionSource<object>();
      task.ContinueWith(t => {
        if (t.IsCanceled) {
          tcs.TrySetCanceled();
          return;
        }
        if (t.IsFaulted) {
          _errorHandler(t.Exception);
          tcs.TrySetException(t.Exception);
          return;
        }
        continuationFunction(t.Result).ContinueWith(t2 => {
          if (t2.IsCanceled) {
            tcs.TrySetCanceled();
          } else if (t2.IsFaulted) {
            _errorHandler(t2.Exception);
            tcs.TrySetException(t2.Exception);
          } else {
            tcs.TrySetResult(null);
          }
        });
      }, _scheduler);
      return tcs.Task;
    }

    public Task CombineWith(Task task, Func<Task> continuationFunction) {
      var tcs = new TaskCompletionSource<object>();
      task.ContinueWith(t => {
        if (t.IsCanceled) {
          tcs.TrySetCanceled();
          return;
        }
        if (t.IsFaulted) {
          _errorHandler(t.Exception);
          tcs.TrySetException(t.Exception);
          return;
        }
        continuationFunction().ContinueWith(t2 => {
          if (t2.IsCanceled) {
            tcs.TrySetCanceled();
          } else if (t2.IsFaulted) {
            _errorHandler(t2.Exception);
            tcs.TrySetException(t2.Exception);
          } else {
            tcs.TrySetResult(null);
          }
        });
      }, _scheduler);
      return tcs.Task;
    }

    public Task TryFinally(Action tryAction, Func<Task> task, Action finallyAction) {
      tryAction();
      return task().ContinueWith(t => finallyAction(), _scheduler);
    }
  }
}
