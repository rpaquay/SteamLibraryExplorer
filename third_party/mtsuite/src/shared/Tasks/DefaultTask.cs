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
using System.Threading.Tasks;
using mtsuite.shared.Utils;

namespace mtsuite.shared.Tasks {
  public class DefaultTask : ITask {
    private readonly Task _task;

    public DefaultTask(Task task) {
      _task = task;
    }

    public Task Task {
      get { return _task; }
    }

    public void Start() {
      _task.Start();
    }

    public bool Wait(TimeSpan duration) {
      return _task.Wait(duration);
    }

    public ITask ContinueWith(Action<ITask> continuation) {
      return Wrap(_task.ContinueWith(_ => continuation(this)));
    }

    public ITask<TResult> ContinueWith<TResult>(Func<ITask, TResult> continuation) {
      return Wrap(_task.ContinueWith(_ => continuation(this)));
    }

    public ITask Then(Func<ITask, ITask> taskFactory) {
      return Wrap(_task.ContinueWithTask(_ => Unwrap(taskFactory(this))));
    }

    public ITask<TResult> Then<TResult>(Func<ITask, ITask<TResult>> taskFactory) {
      return Wrap(_task.ContinueWithTask(_ => Unwrap(taskFactory(this))));
    }

    public static ITask Wrap(Task task) {
      return new DefaultTask(task);
    }

    public static ITask<TResult> Wrap<TResult>(Task<TResult> task) {
      return new DefaultTask<TResult>(task);
    }

    public static Task Unwrap(ITask task) {
      return ((DefaultTask)task)._task;
    }

    public static Task<TResult> Unwrap<TResult>(ITask<TResult> task) {
      return ((DefaultTask<TResult>)task).TypedTask;
    }
  }
}