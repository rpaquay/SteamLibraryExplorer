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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mtsuite.shared.Tasks {
  public class DefaultTaskCollection : ITaskCollection {
    private readonly DefaultTaskFactory _factory;
    private readonly List<DefaultTask> _tasks = new List<DefaultTask>();

    public DefaultTaskCollection(DefaultTaskFactory factory) {
      _factory = factory;
    }

    public DefaultTaskCollection(DefaultTaskFactory factory, IEnumerable<ITask> source) : this(factory) {
      AddRange(source);
    }

    public void Add(ITask task) {
      _tasks.Add((DefaultTask)task);
    }

    public void AddRange(IEnumerable<ITask> tasks) {
      _tasks.AddRange(tasks.Cast<DefaultTask>());
    }

    public ITask ContinueWith(Action<ITaskCollection> continuation) {
      if (_tasks.Count == 0)
        return _factory.StartNew(() => continuation(this));

      var tasks = _tasks.Select(x => x.Task).ToArray();
      return new DefaultTask(Task.Factory.ContinueWhenAll(tasks, _ => continuation(this)));
    }

    public ITask<TResult> ContinueWith<TResult>(Func<ITaskCollection, TResult> continuation) {
      if (_tasks.Count == 0)
        return _factory.StartNew(() => continuation(this));

      var tasks = _tasks.Select(x => x.Task).ToArray();
      return new DefaultTask<TResult>(Task.Factory.ContinueWhenAll(tasks, _ => continuation(this)));
    }

    public ITask Then(Func<ITaskCollection, ITask> taskFactory) {
      return ContinueWith(_ => { }).Then(t => taskFactory(this));
    }

    public ITask<TResult> Then<TResult>(Func<ITaskCollection, ITask<TResult>> taskFactory) {
      return ContinueWith(_ => { }).Then(t => taskFactory(this));
    }

    public IEnumerator<ITask> GetEnumerator() {
      return _tasks.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }
  }
}