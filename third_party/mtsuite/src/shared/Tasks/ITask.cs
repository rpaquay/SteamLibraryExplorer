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

namespace mtsuite.shared.Tasks {
  public interface ITask {
    /// <summary>
    /// Wait for the task to complete for <paramref name="duration"/>. Return
    /// <code>true</code> if the task completed within the <paramref
    /// name="duration"/>.
    /// </summary>
    bool Wait(TimeSpan duration);

    /// <summary>
    /// Returns a task that completes when this task and <paramref
    /// name="continuation"/> complete.
    /// </summary>
    ITask ContinueWith(Action<ITask> continuation);

    /// <summary>
    /// Returns a task that completes when this task and <paramref
    /// name="continuation"/> complete. The returned task result is the result
    /// of <paramref name="continuation"/>.
    /// </summary>
    ITask<TResult> ContinueWith<TResult>(Func<ITask, TResult> continuation);

    /// <summary>
    /// Returns a task that completes when this task and the task returned by
    /// <paramref name="taskFactory"/> complete..
    /// </summary>
    ITask Then(Func<ITask, ITask> taskFactory);

    /// <summary>
    /// Returns a task that completes when this task and the task returned by
    /// <paramref name="taskFactory"/> complete. The returned task result is the
    /// result of task returned by <paramref name="taskFactory"/>.
    /// </summary>
    ITask<TResult> Then<TResult>(Func<ITask, ITask<TResult>> taskFactory);
  }

  public interface ITask<TResult> : ITask {
    /// <summary>
    /// The task result. Only available when the task has completed.
    /// </summary>
    TResult Result { get; }

    /// <summary>
    /// Returns a task that completes when this task and <paramref
    /// name="continuation"/> complete.
    /// </summary>
    ITask ContinueWith(Action<ITask<TResult>> continuation);

    /// <summary>
    /// Returns a task that completes when this task and <paramref
    /// name="continuation"/> complete. The returned task result is the result
    /// of <paramref name="continuation"/>.
    /// </summary>
    ITask<TNewResult> ContinueWith<TNewResult>(Func<ITask<TResult>, TNewResult> continuation);

    /// <summary>
    /// Returns a task that completes when this task and the task returned by
    /// <paramref name="taskFactory"/> complete..
    /// </summary>
    ITask Then(Func<ITask<TResult>, ITask> taskFactory);

    /// <summary>
    /// Returns a task that completes when this task and the task returned by
    /// <paramref name="taskFactory"/> complete. The returned task result is the
    /// result of task returned by <paramref name="taskFactory"/>.
    /// </summary>
    ITask<TNewResult> Then<TNewResult>(Func<ITask<TResult>, ITask<TNewResult>> taskFactory);
  }
}