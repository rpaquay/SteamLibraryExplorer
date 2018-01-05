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
using System.Threading;

namespace mtsuite.shared.Tasks.Experimental {
  public abstract class CustomTask : ITask {
    public static int GlobalId;
    private readonly CustomTaskFactory _factory;
    private readonly int _id;

    protected CustomTask(CustomTaskFactory factory) {
      _factory = factory;
      _id = Interlocked.Increment(ref GlobalId);
    }

    public CustomTaskFactory Factory {
      get { return _factory; }
    }

    public int Id {
      get { return _id; }
    }

    public CustomTask ContinuationTask { get; set; }

    public bool IsCompleted { get; set; }

    public Exception Error { get; set; }

    public bool Wait(TimeSpan duration) {
      if (IsCompleted)
        return true;

      var start = DateTime.UtcNow;
      while (DateTime.UtcNow - start < duration) {
        if (IsCompleted)
          return true;

        Thread.Sleep(5);
      }
      return false;
    }

    protected void CheckContinuation() {
      if (ContinuationTask != null)
        throw new InvalidOperationException("Task already has a continuation task");
    }

    /// <summary>
    /// Returns a task that completes after this task and <paramref
    /// name="continuation"/>
    /// </summary>
    public ITask ContinueWith(Action<ITask> continuation) {
      var task = new CustomActionTask(_factory, () => continuation(this));
      SetContinuation(task);
      return task;
    }

    public ITask<TResult> ContinueWith<TResult>(Func<ITask, TResult> continuation) {
      var task = new CustomTask<TResult>(_factory, () => continuation(this));
      SetContinuation(task);
      return task;
    }

    /// <summary>
    /// Returns a task that completes after this task and after
    /// that task produced by <paramref name="taskFactory"/>.
    /// </summary>
    public ITask Then(Func<ITask, ITask> taskFactory) {
      var task = new CustomThenTask(_factory, this, taskFactory);
      SetContinuation(task);
      return task;
    }

    public ITask<TResult> Then<TResult>(Func<ITask, ITask<TResult>> taskFactory) {
      throw new NotImplementedException();
      //var task = new CustomThenTask<TResult>(Factory, this, taskFactory);
      //SetContinuation(task);
      //return task;
    }

    /// <summary>
    /// Set <paramref name="task"/> as the continuation task. Return
    /// <code>true</code> if the current task was completed.
    /// </summary>
    public bool SetContinuation(CustomTask task) {
      lock (this) {
        CheckContinuation();
        if (IsCompleted) {
          _factory.Queue.Enqueue(task);
          return true;
        } else {
          ContinuationTask = task;
          return false;
        }
      }
    }

    public void Complete() {
      CustomTask nextTask;
      lock (this) {
        IsCompleted = true;
        nextTask = ContinuationTask;
        ContinuationTask = null;
      }
      if (nextTask != null) {
        _factory.Queue.Enqueue(nextTask);
      }
    }

#if false
    protected void EnqueueNextTask(Func<CustomTask> factory) {
      try {
        var newTask = factory();
        newTask.SetContinuation(ContinuationTask);
        ContinuationTask = null;
      } catch (Exception e) {
        Error = e;
      }
      Complete();
    }
#endif
    public abstract void Run();
  }

  public class CustomTask<TResult> : CustomTask, ITask<TResult> {
    private readonly Func<TResult> _func;

    public CustomTask(CustomTaskFactory factory, Func<TResult> func)
      : base(factory) {
      _func = func;
    }

    public TResult Result { get; set; }

    public ITask ContinueWith(Action<ITask<TResult>> continuation) {
      var task = new CustomActionTask(Factory, () => continuation(this));
      SetContinuation(task);
      return task;
    }

    //public ITask<TNewResult> WithContinuation<TNewResult>(Func<ITask<TResult>, TNewResult> continuation) {
    //  var task = new CustomTask<TNewResult>(Factory, () => continuation(this));
    //  SetContinuation(task);
    //  return task;
    //}

    public ITask<TNewResult> ContinueWith<TNewResult>(Func<ITask<TResult>, TNewResult> continuation) {
      var task = new CustomTask<TNewResult>(Factory, () => continuation(this));
      SetContinuation(task);
      return task;
    }

    public ITask Then(Func<ITask<TResult>, ITask> taskFactory) {
      var task = new CustomThenTask<TResult>(Factory, this, taskFactory);
      SetContinuation(task);
      return task;
    }

    public ITask<TNewResult> Then<TNewResult>(Func<ITask<TResult>, ITask<TNewResult>> taskFactory) {
      var task = new CustomThenTask<TResult, TNewResult>(Factory, this, taskFactory);
      SetContinuation(task);
      return task;
    }

    public override void Run() {
      try {
        Result = _func();
      } catch (Exception e) {
        Error = e;
      }
      Complete();
    }
  }
}