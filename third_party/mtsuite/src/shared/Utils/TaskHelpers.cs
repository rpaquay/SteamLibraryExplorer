// Copyright 2015 Renaud Paquay All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace mtsuite.shared.Utils {
  public static class TaskHelpers {
    public static Task ContinueWithWhenAll(this IEnumerable<Task> tasks, Action<IList<Task>> continuationAction) {
      var taskArray = tasks.ToArray();
      if (taskArray.Length == 0) {
        return Task.Factory.StartNew(() => continuationAction(taskArray));
      }
      return Task.Factory.ContinueWhenAll(taskArray, t => {
        continuationAction(taskArray);
      });
    }

    public static Task ContinueWithWhenAll<T>(this IEnumerable<Task<T>> tasks, Action<IList<Task<T>>> continuationAction) {
      var taskArray = tasks.ToArray();
      if (taskArray.Length == 0) {
        return Task.Factory.StartNew(() => continuationAction(taskArray));
      }
      return Task.Factory.ContinueWhenAll(taskArray, t => {
        continuationAction(taskArray);
      });
    }

    public static Task<TNewResult> ContinueWithWhenAll<T, TNewResult>(this IEnumerable<Task<T>> tasks, Func<IList<Task<T>>, TNewResult> continuationAction) {
      var taskArray = tasks.ToArray();
      if (taskArray.Length == 0) {
        return Task.Factory.StartNew(() => continuationAction(taskArray));
      }
      return Task.Factory.ContinueWhenAll(taskArray, t => {
        return continuationAction(taskArray);
      });
    }

    public static Task<TNewResult> ContinueWithTask<T, TNewResult>(this Task<T> task, Func<Task<T>, Task<TNewResult>> continuationFunction) {
      return ContinueWithTask(task, continuationFunction, new CancellationToken());
    }

    public static Task<TNewResult> ContinueWithTask<T, TNewResult>(this Task<T> task, Func<Task<T>, Task<TNewResult>> continuationFunction,
      CancellationToken cancellationToken) {
      var tcs = new TaskCompletionSource<TNewResult>();
      task.ContinueWith(t => {
        if (cancellationToken.IsCancellationRequested) {
          tcs.SetCanceled();
        }
        continuationFunction(t).ContinueWith(t2 => {
          if (cancellationToken.IsCancellationRequested || t2.IsCanceled) {
            tcs.TrySetCanceled();
          } else if (t2.IsFaulted) {
            tcs.TrySetException(t2.Exception);
          } else {
            tcs.TrySetResult(t2.Result);
          }
        });
      });
      return tcs.Task;
    }

    public static Task ContinueWithTask<T>(this Task<T> task, Func<Task<T>, Task> continuationFunction) {
      return ContinueWithTask(task, continuationFunction, new CancellationToken());
    }

    public static Task ContinueWithTask<T>(this Task<T> task, Func<Task<T>, Task> continuationFunction, CancellationToken cancellationToken) {
      var tcs = new TaskCompletionSource<bool>();
      task.ContinueWith(t => {
        if (cancellationToken.IsCancellationRequested) {
          tcs.SetCanceled();
        }
        continuationFunction(t).ContinueWith(t2 => {
          if (cancellationToken.IsCancellationRequested || t2.IsCanceled) {
            tcs.TrySetCanceled();
          } else if (t2.IsFaulted) {
            tcs.TrySetException(t2.Exception);
          } else {
            tcs.TrySetResult(default(bool));
          }
        });
      });
      return tcs.Task;
    }

    public static Task ContinueWithTask(this Task task, Func<Task, Task> continuationFunction) {
      return ContinueWithTask(task, continuationFunction, new CancellationToken());
    }

    public static Task ContinueWithTask(this Task task, Func<Task, Task> continuationFunction, CancellationToken cancellationToken) {
      var tcs = new TaskCompletionSource<bool>();
      task.ContinueWith(t => {
        if (cancellationToken.IsCancellationRequested) {
          tcs.SetCanceled();
        }
        continuationFunction(t).ContinueWith(t2 => {
          if (cancellationToken.IsCancellationRequested || t2.IsCanceled) {
            tcs.TrySetCanceled();
          } else if (t2.IsFaulted) {
            tcs.TrySetException(t2.Exception);
          } else {
            tcs.TrySetResult(default(bool));
          }
        });
      });
      return tcs.Task;
    }

    public static Task<TResult> ContinueWithTask<TResult>(this Task task, Func<Task, Task<TResult>> continuationFunction) {
      return ContinueWithTask(task, continuationFunction, new CancellationToken());
    }

    public static Task<TResult> ContinueWithTask<TResult>(this Task task, Func<Task, Task<TResult>> continuationFunction, CancellationToken cancellationToken) {
      var tcs = new TaskCompletionSource<TResult>();
      task.ContinueWith(t => {
        if (cancellationToken.IsCancellationRequested) {
          tcs.SetCanceled();
        }
        continuationFunction(t).ContinueWith(t2 => {
          if (cancellationToken.IsCancellationRequested || t2.IsCanceled) {
            tcs.TrySetCanceled();
          } else if (t2.IsFaulted) {
            tcs.TrySetException(t2.Exception);
          } else {
            tcs.TrySetResult(t2.Result);
          }
        });
      });
      return tcs.Task;
    }

    public static Task CompletedTask {
      get {
        var tcs = new TaskCompletionSource<bool>();
        tcs.SetResult(false);
        return tcs.Task;
      }
    }
  }
}