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
using System.Diagnostics;

namespace mtsuite.shared.Tasks.Experimental {
  public class CustomActionTask : CustomTask {
    private readonly Action _action;

    public CustomActionTask(CustomTaskFactory factory, Action action)
      : base(factory) {
      _action = action;
    }

    public override void Run() {
      Debug.Assert(!IsCompleted);

      try {
        _action();
      } catch (Exception e) {
        Error = e;
      }
      Complete();
    }
  }
}