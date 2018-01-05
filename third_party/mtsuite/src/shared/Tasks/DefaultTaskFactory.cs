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
using System.Threading.Tasks;
using mtsuite.shared.Utils;

namespace mtsuite.shared.Tasks {
  public class DefaultTaskFactory : ITaskFactory {
    private readonly DefaultTask _completedTask;

    public DefaultTaskFactory() {
      _completedTask = new DefaultTask(TaskHelpers.CompletedTask);
    }

    public ITask CompletedTask {
      get { return _completedTask; }
    }

    public void Dispose() {
      // Nothing to do.
    }

    public ITask StartNew(Action action) {
      return new DefaultTask(Task.Factory.StartNew(action));
    }

    public ITask<T> StartNew<T>(Func<T> func) {
      return new DefaultTask<T>(Task<T>.Factory.StartNew(func));
    }

    public ITaskCollection CreateCollection() {
      return new DefaultTaskCollection(this);
    }

    public ITaskCollection CreateCollection(IEnumerable<ITask> source) {
      return new DefaultTaskCollection(this, source);
    }

    public ITaskCollection<TResult> CreateCollection<TResult>() {
      return new DefaultTaskCollection<TResult>(this);
    }

    public ITaskCollection<TResult> CreateCollection<TResult>(IEnumerable<ITask<TResult>> source) {
      return new DefaultTaskCollection<TResult>(this, source);
    }
  }
}