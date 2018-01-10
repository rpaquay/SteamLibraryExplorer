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
using System.IO;

namespace mtsuite.CoreFileSystem {
  public struct FileSystemEntry {
    private readonly FullPath _path;
    private readonly FileSystemEntryData _data;

    public FileSystemEntry(FullPath path, FileSystemEntryData data) {
      _path = path;
      _data = data;
    }

    public FullPath Path => _path;
    public string Name => _path.Name;
    public long FileSize => _data.FileSize;
    public DateTime LastWriteTimeUtc => _data.LastWriteTimeUtc;
    public FileAttributes FileAttributes => _data.FileAttributes;
    public bool IsFile => _data.IsFile;
    public bool IsDirectory => _data.IsDirectory;
    /// <summary>
    /// Return <code>true</code> if the entry is either a junction point or
    /// symbolic link. A junction point applies only to directories, whereas
    /// a symbolic link applies to both files and directories.
    /// </summary>
    public bool IsReparsePoint => _data.IsReparsePoint;
    public bool IsReadOnly => _data.IsReadOnly;
    public bool IsSystem => _data.IsSystem;

    public override string ToString() {
      return string.Format("\"{0}\", {1}", _path.Name, _data);
    }
  }
}