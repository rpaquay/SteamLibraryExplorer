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

namespace mtsuite.CoreFileSystem {
  public class ReparsePointInfo {
    public bool IsJunctionPoint { get; set; }
    public bool IsSymbolicLink { get; set; }
    public string Target { get; set; }
    public bool IsTargetRelative { get; set; }
    public DateTime CreationTimeUtc { get; set; }
    public DateTime LastAccessTimeUtc { get; set; }
    public DateTime LastWriteTimeUtc { get; set; }
  }
}