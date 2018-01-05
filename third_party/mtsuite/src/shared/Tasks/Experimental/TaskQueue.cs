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

using System.Collections.Generic;
using System.Threading;

namespace mtsuite.shared.Tasks.Experimental {
  public class TaskQueue {
    private readonly CustomTaskFactory _factory;
    private readonly Queue<CustomTask> _queue = new Queue<CustomTask>();
    //private readonly AutoResetEvent _signal = new Semaphore(0, Int32.MaxValue);
    private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);

    public TaskQueue(CustomTaskFactory factory) {
      _factory = factory;
    }

    public void Enqueue(CustomTask task) {
      //Logger.WriteLine("Enqueue task {0} of type {1}", task.Id, task.GetType().Name);
      lock (_queue) {
        _queue.Enqueue(task);
      }
      //_signal.Set();
      _signal.Release(1);
    }

    public CustomTask Dequeue() {
      lock (_queue) {
        if (_queue.Count == 0)
          return null;
        return _queue.Dequeue();
      }
      //Logger.WriteLine("Dequeue task {0} of type {1}", task.Id, task.GetType().Name);
    }

    public void Wait() {
      _signal.Wait();
    }
  }
}