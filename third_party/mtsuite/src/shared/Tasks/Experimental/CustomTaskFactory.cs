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

namespace mtsuite.shared.Tasks.Experimental {
  public class CustomTaskFactory : ITaskFactory {
    private readonly TaskQueue _queue;
    private readonly List<TaskThread> _threads = new List<TaskThread>();
    private readonly CustomTask _completedTask;

    public CustomTaskFactory() {
      _queue = new TaskQueue(this);
      _completedTask = new CustomActionTask(this, () => { });
      _completedTask.Run(); // Ensure "IsCompleted" is "true".
      for (var i = 0; i < Environment.ProcessorCount; i++) {
        _threads.Add(new TaskThread(this));
      }
    }

    public ITask CompletedTask {
      get { return _completedTask; }
    }

    public TaskQueue Queue {
      get { return _queue; }
    }

    public void Dispose() {
      foreach (var thread in _threads) {
        thread.Terminate();
      }
      foreach (var thread in _threads) {
        _queue.Enqueue(new CustomActionTask(this, () => { }));
      }
      foreach (var thread in _threads) {
        thread.Wait();
      }
    }

    public ITask StartNew(Action action) {
      var task = new CustomActionTask(this, action);
      _queue.Enqueue(task);
      return task;
    }

    public ITask<T> StartNew<T>(Func<T> func) {
      var task = new CustomTask<T>(this, func);
      _queue.Enqueue(task);
      return task;
    }

    public ITaskCollection CreateCollection() {
      return new CustomTaskCollection(this);
    }

    public ITaskCollection CreateCollection(IEnumerable<ITask> source) {
      return new CustomTaskCollection(this, source);
    }

    public ITaskCollection<TResult> CreateCollection<TResult>() {
      return new CustomTaskCollection<TResult>(this);
    }

    public ITaskCollection<TResult> CreateCollection<TResult>(IEnumerable<ITask<TResult>> source) {
      return new CustomTaskCollection<TResult>(this, source);
    }

    public ITask EnqueueTaskForCollection(CustomTask task, ITaskCollection collection) {
      bool allComplete = true;
      foreach (CustomTask item in collection) {
        bool wasCompleted = item.SetContinuation(task);
        allComplete &= wasCompleted;
      }
      // If all tasks of the collections were complete, start this one
      // imediately.
      if (allComplete) {
        _queue.Enqueue(task);
      }
      return task;
    }

    public ITask EnqueueTaskForCollection<TResult>(CustomTask task, ITaskCollection<TResult> collection) {
      bool allComplete = true;
      foreach (CustomTask<TResult> item in collection) {
        bool wasCompleted = item.SetContinuation(task);
        allComplete &= wasCompleted;
      }
      // If all tasks of the collections were complete, start this one
      // imediately.
      if (allComplete) {
        _queue.Enqueue(task);
      }
      return task;
    }

    public ITask<TNewResult> EnqueueTaskForCollection<TResult, TNewResult>(CustomTask<TNewResult> task, ITaskCollection<TResult> collection) {
      bool allComplete = true;
      foreach (CustomTask<TResult> item in collection) {
        bool wasCompleted = item.SetContinuation(task);
        allComplete &= wasCompleted;
      }
      // If all tasks of the collections were complete, start this one
      // imediately.
      if (allComplete) {
        _queue.Enqueue(task);
      }
      return task;
    }
  }
}
