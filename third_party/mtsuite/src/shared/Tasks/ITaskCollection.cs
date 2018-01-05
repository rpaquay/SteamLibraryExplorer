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

namespace mtsuite.shared.Tasks {
  public interface ITaskCollection : IEnumerable<ITask> {
    void Add(ITask task);
    void AddRange(IEnumerable<ITask> tasks);

    ITask ContinueWith(Action<ITaskCollection> continuation);
    ITask<TResult> ContinueWith<TResult>(Func<ITaskCollection, TResult> continuation);

    ITask Then(Func<ITaskCollection, ITask> taskFactory);
    ITask<TResult> Then<TResult>(Func<ITaskCollection, ITask<TResult>> taskFactory);
  }

  public interface ITaskCollection<TResult> : IEnumerable<ITask<TResult>> {
    void Add(ITask<TResult> task);
    void AddRange(IEnumerable<ITask<TResult>> tasks);

    ITask ContinueWith(Action<ITaskCollection<TResult>> continuation);
    ITask<TNewResult> ContinueWith<TNewResult>(Func<ITaskCollection<TResult>, TNewResult> continuation);

    ITask Then(Func<ITaskCollection<TResult>, ITask> taskFactory);
    ITask<TNewResult> Then<TNewResult>(Func<ITaskCollection<TResult>, ITask<TNewResult>> taskFactory);
  }
}