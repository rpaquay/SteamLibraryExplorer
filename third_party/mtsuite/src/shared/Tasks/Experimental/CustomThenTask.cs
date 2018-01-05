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

namespace mtsuite.shared.Tasks.Experimental {
  public class CustomThenTask : CustomTask {
    private readonly CustomTask _customTask;
    private readonly Func<ITask, ITask> _taskFactory;
    private CustomTask _newTask;

    public CustomThenTask(CustomTaskFactory factory, CustomTask customTask, Func<ITask, ITask> taskFactory)
      : base(factory) {
      _customTask = customTask;
      _taskFactory = taskFactory;
    }

    public override void Run() {
      if (_newTask == null) {
        _newTask = (CustomTask)_taskFactory(_customTask);
        _newTask.SetContinuation(this);
      } else {
        Complete();
      }
    }
  }

  public class CustomThenTask<TResult> : CustomTask {
    private readonly ITask<TResult> _customTask;
    private readonly Func<ITask<TResult>, ITask> _taskFactory;
    private CustomTask _newTask;

    public CustomThenTask(CustomTaskFactory factory, ITask<TResult> customTask, Func<ITask<TResult>, ITask> taskFactory)
      : base(factory) {
      _customTask = customTask;
      _taskFactory = taskFactory;
    }

    public override void Run() {
      if (_newTask == null) {
        _newTask = (CustomTask)_taskFactory(_customTask);
        _newTask.SetContinuation(this);
      } else {
        Complete();
      }
    }
  }

  public class CustomThenTask<TResult, TNewResult> : CustomTask<TNewResult> {
    private readonly ITask<TResult> _customTask;
    private readonly Func<ITask<TResult>, ITask<TNewResult>> _taskCreator;
    private CustomTask<TNewResult> _newTask;

    public CustomThenTask(CustomTaskFactory factory, ITask<TResult> customTask, Func<ITask<TResult>, ITask<TNewResult>> taskCreator)
      : base(factory, null /*TODO*/) {
      _customTask = customTask;
      _taskCreator = taskCreator;
    }

    public override void Run() {
      if (_newTask == null) {
        _newTask = (CustomTask<TNewResult>)_taskCreator(_customTask);
        _newTask.SetContinuation(this);
      } else {
        Result = _newTask.Result;
        Complete();
      }
    }
  }
}