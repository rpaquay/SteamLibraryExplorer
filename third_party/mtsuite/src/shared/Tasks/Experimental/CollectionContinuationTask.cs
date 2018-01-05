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
  public class CollectionContinuationTask : CustomTask {
    private readonly CustomTaskCollection _collection;
    private readonly Action<ITaskCollection> _action;
    private int _runCount;

    public CollectionContinuationTask(CustomTaskFactory factory, CustomTaskCollection collection, Action<ITaskCollection> action)
      : base(factory) {
      _collection = collection;
      _action = action;
    }

    public override void Run() {
      //TODO: Use a counter to avoid looping through all tasks?
      bool allCompleted = _collection.Cast<CustomTask>().All(x => x.IsCompleted);
      if (!allCompleted)
        return;
      if (Interlocked.Increment(ref _runCount) != 1)
        return;

      try {
        _action(_collection);
      } catch (Exception e) {
        Error = e;
      }
      Complete();
    }
  }
  public class CollectionContinuationTask<TResult> : CustomTask {
    private readonly CustomTaskCollection<TResult> _collection;
    private readonly Action<ITaskCollection<TResult>> _action;
    private int _runCount;

    public CollectionContinuationTask(CustomTaskFactory factory, CustomTaskCollection<TResult> collection, Action<ITaskCollection<TResult>> action)
      : base(factory) {
      _collection = collection;
      _action = action;
    }

    public override void Run() {
      //TODO: Use a counter to avoid looping through all tasks?
      bool allCompleted = _collection.Cast<CustomTask>().All(x => x.IsCompleted);
      if (!allCompleted)
        return;
      if (Interlocked.Increment(ref _runCount) != 1)
        return;

      try {
        _action(_collection);
      } catch (Exception e) {
        Error = e;
      }
      Complete();
    }
  }

  public class CollectionContinuationTask<TResult, TNewResult> : CustomTask<TNewResult> {
    private readonly CustomTaskCollection<TResult> _collection;
    private readonly Func<ITaskCollection<TResult>, TNewResult> _function;
    private int _runCount;

    public CollectionContinuationTask(CustomTaskFactory factory, CustomTaskCollection<TResult> collection, Func<ITaskCollection<TResult>, TNewResult> function)
      : base(factory, null) {
      _collection = collection;
      _function = function;
    }

    public override void Run() {
      //TODO: Use a counter to avoid looping through all tasks?
      bool allCompleted = _collection.Cast<CustomTask>().All(x => x.IsCompleted);
      if (!allCompleted)
        return;
      if (Interlocked.Increment(ref _runCount) != 1)
        return;

      try {
        Result = _function(_collection);
      } catch (Exception e) {
        Error = e;
      }
      Complete();
    }
  }
}