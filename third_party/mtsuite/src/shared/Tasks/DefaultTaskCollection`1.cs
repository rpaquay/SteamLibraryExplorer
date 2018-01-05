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
  public class DefaultTaskCollection<TResult> : ITaskCollection<TResult>, ITaskCollection {
    private readonly DefaultTaskFactory _factory;
    private readonly List<DefaultTask<TResult>> _tasks = new List<DefaultTask<TResult>>();

    public DefaultTaskCollection(DefaultTaskFactory factory) {
      _factory = factory;
    }

    public DefaultTaskCollection(DefaultTaskFactory factory, IEnumerable<ITask<TResult>> source)
      : this(factory) {
      AddRange(source);
    }

    public void Add(ITask<TResult> task) {
      _tasks.Add((DefaultTask<TResult>)task);
    }

    public void AddRange(IEnumerable<ITask<TResult>> tasks) {
      _tasks.AddRange(tasks.Cast<DefaultTask<TResult>>());
    }

    public ITask ContinueWith(Action<ITaskCollection<TResult>> continuation) {
      if (_tasks.Count == 0)
        return _factory.StartNew(() => continuation(this));

      var tasks = _tasks.Select(x => x.TypedTask).ToArray();
      return new DefaultTask(Task.Factory.ContinueWhenAll(tasks, _ => continuation(this)));
    }

    public ITask<TNewResult> ContinueWith<TNewResult>(Func<ITaskCollection<TResult>, TNewResult> continuation) {
      if (_tasks.Count == 0)
        return _factory.StartNew(() => continuation(this));

      var tasks = _tasks.Select(x => x.TypedTask).ToArray();
      return new DefaultTask<TNewResult>(Task.Factory.ContinueWhenAll(tasks, _ => continuation(this)));
    }

    public ITask Then(Func<ITaskCollection<TResult>, ITask> taskFactory) {
      return ContinueWith(_ => { }).Then(t => taskFactory(this));
    }

    public ITask<TNewResult> Then<TNewResult>(Func<ITaskCollection<TResult>, ITask<TNewResult>> taskFactory) {
      return ContinueWith(_ => default(TNewResult)).Then(t => taskFactory(this));
    }

    public IEnumerator<ITask<TResult>> GetEnumerator() {
      return _tasks.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    void ITaskCollection.Add(ITask task) {
      Add((ITask<TResult>)task);
    }

    void ITaskCollection.AddRange(IEnumerable<ITask> tasks) {
      AddRange(tasks.Cast<ITask<TResult>>());
    }

    ITask ITaskCollection.ContinueWith(Action<ITaskCollection> continuation) {
      if (_tasks.Count == 0)
        return _factory.StartNew(() => continuation(this));

      var tasks = _tasks.Select(x => x.TypedTask).ToArray();
      return new DefaultTask(Task.Factory.ContinueWhenAll(tasks, _ => continuation(this)));
    }

    ITask<TNewResult> ITaskCollection.ContinueWith<TNewResult>(Func<ITaskCollection, TNewResult> continuation) {
      if (_tasks.Count == 0)
        return _factory.StartNew(() => continuation(this));

      var tasks = _tasks.Select(x => x.TypedTask).ToArray();
      return new DefaultTask<TNewResult>(Task.Factory.ContinueWhenAll(tasks, _ => continuation(this)));
    }

    ITask ITaskCollection.Then(Func<ITaskCollection, ITask> taskFactory) {
      return ContinueWith(_ => { }).Then(t => taskFactory(this));
    }

    ITask<TNewResult> ITaskCollection.Then<TNewResult>(Func<ITaskCollection, ITask<TNewResult>> taskFactory) {
      return ContinueWith(_ => { }).Then(t => taskFactory(this));
    }

    IEnumerator<ITask> IEnumerable<ITask>.GetEnumerator() {
      return GetEnumerator();
    }
  }
}