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

using System.Threading;

namespace mtsuite.shared.Tasks.Experimental {
  public class TaskThread {
    private readonly CustomTaskFactory _factory;
    private readonly ManualResetEvent _terminateEvent = new ManualResetEvent(false);
    private bool _terminateRequest;

    public TaskThread(CustomTaskFactory factory) {
      _factory = factory;
      CreateThread();
    }

    private void CreateThread() {
      var thread = new Thread(ThreadFunc);
      thread.IsBackground = true;
      thread.Start();
    }

    private void ThreadFunc() {
      //Logger.WriteLine("Starting thread {0}", Thread.CurrentThread.ManagedThreadId);
      while (true) {
        var task = _factory.Queue.Dequeue();
        if (task == null) {
          _factory.Queue.Wait();
          if (_terminateRequest)
            break;
          continue;
        }

        //Logger.WriteLine("Thread {0}: running task {1} of type {2}", Thread.CurrentThread.ManagedThreadId, task.Id, task.GetType().Name);
        task.Run();
      }

      //Logger.WriteLine("Terminating thread {0}", Thread.CurrentThread.ManagedThreadId);
      _terminateEvent.Set();
    }

    public void Terminate() {
      _terminateRequest = true;
    }
    public void Wait() {
      _terminateEvent.WaitOne();
      //Logger.WriteLine("Done waiting for thead termination.");
    }
  }
}