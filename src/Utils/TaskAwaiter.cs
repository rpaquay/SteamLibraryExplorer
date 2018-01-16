using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SteamLibraryExplorer.Utils {
  public class TaskAwaiter {
    private readonly Action<Exception> _errorHandler;
    private readonly TaskScheduler _scheduler;

    public TaskAwaiter(Action<Exception> errorHandler)
      : this(errorHandler, TaskScheduler.FromCurrentSynchronizationContext()) {
    }

    public TaskAwaiter(Action<Exception> errorHandler, TaskScheduler scheduler) {
      _errorHandler = errorHandler;
      _scheduler = scheduler;
    }

    /// <summary>
    /// Returns a task that executes <paramref name="action"/> after <paramref name="task"/> completes.
    /// </summary>
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

    private void WrapAction<TNewResult>(TaskCompletionSource<TNewResult> tcs, Action action) {
      try {
        action();
      } catch (OperationCanceledException) {
        tcs.TrySetCanceled();
      } catch (Exception e) {
        tcs.TrySetException(e);
      }
    }

    private void ProcessCompletion<TNewResult>(Task t, TaskCompletionSource<TNewResult> tcs, Action action) {
      if (t.IsCanceled) {
        tcs.TrySetCanceled();
      } else if (t.IsFaulted) {
        Debug.Assert(t.Exception != null);
        _errorHandler(t.Exception);
        tcs.TrySetException(t.Exception);
      } else {
        WrapAction(tcs, action);
      }
    }

    public Task<TNewResult> CombineWith<T, TNewResult>(Task<T> task, Func<T, Task<TNewResult>> continuationFunction) {
      var tcs = new TaskCompletionSource<TNewResult>();
      task.ContinueWith(t => {
        ProcessCompletion(t, tcs, () => {
          continuationFunction(t.Result).ContinueWith(t2 => {
            ProcessCompletion(t2, tcs, () => tcs.TrySetResult(t2.Result));
          });
        });
      }, _scheduler);
      return tcs.Task;
    }

    public Task CombineWith<T>(Task<T> task, Func<T, Task> continuationFunction) {
      var tcs = new TaskCompletionSource<object>();
      task.ContinueWith(t => {
        ProcessCompletion(t, tcs, () => {
          continuationFunction(t.Result).ContinueWith(t2 => {
            ProcessCompletion(t2, tcs, () => tcs.TrySetResult(null));
          });
        });
      }, _scheduler);
      return tcs.Task;
    }

    public Task CombineWith(Task task, Func<Task> continuationFunction) {
      var tcs = new TaskCompletionSource<object>();
      task.ContinueWith(t => {
        ProcessCompletion(t, tcs, () => {
          continuationFunction().ContinueWith(t2 => {
            ProcessCompletion(t2, tcs, () => tcs.TrySetResult(null));
          });
        });
      }, _scheduler);
      return tcs.Task;
    }

    public Task TryFinally(Action tryAction, Func<Task> taskProvider, Action finallyAction) {
      tryAction();
      return taskProvider().ContinueWith(t => finallyAction(), _scheduler);
    }
  }
}