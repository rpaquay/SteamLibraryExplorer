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
using System.Linq;
using System.Threading;

namespace mtsuite.shared.Tasks.Experimental {
  public class CollectionThenTask : CustomTask {
    private readonly CustomTaskCollection _collection;
    private readonly Func<ITaskCollection, ITask> _taskFactory;
    private int _runCount;
    private CustomTask _newTask;

    public CollectionThenTask(CustomTaskFactory factory, CustomTaskCollection collection, Func<ITaskCollection, ITask> taskFactory)
      : base(factory) {
      _collection = collection;
      _taskFactory = taskFactory;
    }

    public override void Run() {
      //TODO: Use a counter to avoid looping through all tasks?
      bool allCompleted = _collection.Cast<CustomTask>().All(x => x.IsCompleted);
      if (!allCompleted)
        return;

      // Ensure we "run" only once.
      if (Interlocked.Increment(ref _runCount) != 1)
        return;

      if (_newTask == null) {
        _newTask = (CustomTask)_taskFactory(_collection);
        _newTask.SetContinuation(this);
      } else {
        Complete();
      }

      Complete();
    }
  }
}