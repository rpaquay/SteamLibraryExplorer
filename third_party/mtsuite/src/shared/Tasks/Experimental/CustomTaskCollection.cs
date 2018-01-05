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

namespace mtsuite.shared.Tasks.Experimental {
  public class CustomTaskCollection : ITaskCollection {
    private readonly CustomTaskFactory _factory;
    private readonly List<ITask> _tasks = new List<ITask>();

    public CustomTaskCollection(CustomTaskFactory factory) {
      _factory = factory;
    }

    public CustomTaskCollection(CustomTaskFactory factory, IEnumerable<ITask> source)
      : this(factory) {
      AddRange(source);
    }

    public IEnumerator<ITask> GetEnumerator() {
      return _tasks.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    public void Add(ITask task) {
      _tasks.Add(task);
    }

    public void AddRange(IEnumerable<ITask> tasks) {
      _tasks.AddRange(tasks);
    }

    public ITask ContinueWith(Action<ITaskCollection> continuation) {
      var task = new CollectionContinuationTask(_factory, this, continuation);
      return _factory.EnqueueTaskForCollection(task, this);
    }

    public ITask<TResult> ContinueWith<TResult>(Func<ITaskCollection, TResult> continuation) {
      throw new NotImplementedException();
    }

    public ITask Then(Func<ITaskCollection, ITask> taskFactory) {
      var task = new CollectionThenTask(_factory, this, taskFactory);
      return _factory.EnqueueTaskForCollection(task, this);
    }

    public ITask<TResult> Then<TResult>(Func<ITaskCollection, ITask<TResult>> taskFactory) {
      throw new NotImplementedException();
    }
  }

  public class CustomTaskCollection<TResult> : ITaskCollection<TResult> {
    private readonly CustomTaskFactory _factory;
    private readonly List<ITask<TResult>> _tasks = new List<ITask<TResult>>();

    public CustomTaskCollection(CustomTaskFactory factory) {
      _factory = factory;
    }

    public CustomTaskCollection(CustomTaskFactory factory, IEnumerable<ITask<TResult>> source)
      : this(factory) {
      AddRange(source);
    }

    public IEnumerator<ITask<TResult>> GetEnumerator() {
      return _tasks.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    public void Add(ITask<TResult> task) {
      _tasks.Add(task);
    }

    public void AddRange(IEnumerable<ITask<TResult>> tasks) {
      _tasks.AddRange(tasks);
    }

    public ITask ContinueWith(Action<ITaskCollection<TResult>> continuation) {
      var task = new CollectionContinuationTask<TResult>(_factory, this, continuation);
      return _factory.EnqueueTaskForCollection(task, this);
    }

    public ITask<TNewResult> ContinueWith<TNewResult>(Func<ITaskCollection<TResult>, TNewResult> continuation) {
      var task = new CollectionContinuationTask<TResult, TNewResult>(_factory, this, continuation);
      return _factory.EnqueueTaskForCollection(task, this);
    }

    public ITask Then(Func<ITaskCollection<TResult>, ITask> taskFactory) {
      throw new NotImplementedException();
    }

    public ITask<TNewResult> Then<TNewResult>(Func<ITaskCollection<TResult>, ITask<TNewResult>> taskFactory) {
      throw new NotImplementedException();
    }
  }
}