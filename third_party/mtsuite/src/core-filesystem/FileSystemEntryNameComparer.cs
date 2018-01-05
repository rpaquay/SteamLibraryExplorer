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

namespace mtsuite.CoreFileSystem {
  public class FileSystemEntryNameComparer : IEqualityComparer<FileSystemEntry> {
    public static FileSystemEntryNameComparer Instance = new FileSystemEntryNameComparer();

    public bool Equals(FileSystemEntry x, FileSystemEntry y) {
      return
        (x.IsFile == y.IsFile) &&
        (x.IsReparsePoint == y.IsReparsePoint) &&
        StringComparer.OrdinalIgnoreCase.Equals(x.Name, y.Name);
    }

    public int GetHashCode(FileSystemEntry obj) {
      return obj.Name.GetHashCode();
    }
  }
}