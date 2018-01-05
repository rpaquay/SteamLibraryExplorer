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

namespace mtsuite.CoreFileSystem.Win32 {
  /// <summary>
  /// File or directory entry information returned by <see
  /// cref="Win32.GetDirectoryEntries"/>, including the short <see cref="Name"/>
  /// and file attributes.
  /// </summary>
  public struct DirectoryEntry {
    private readonly string _name;
    private readonly WIN32_FIND_DATA _data;

    public DirectoryEntry(string name, WIN32_FIND_DATA data) {
      _name = name;
      _data = data;
    }

    public WIN32_FIND_DATA Data {
      get { return _data; }
    }

    public string Name {
      get { return _name; }
    }

    public FILE_ATTRIBUTE Attributes {
      get { return (FILE_ATTRIBUTE)_data.dwFileAttributes; }
    }

    public bool IsFile {
      get {
        return (Attributes & FILE_ATTRIBUTE.FILE_ATTRIBUTE_DIRECTORY) == 0;
      }
    }

    public bool IsDirectory {
      get {
        return (Attributes & FILE_ATTRIBUTE.FILE_ATTRIBUTE_DIRECTORY) != 0;
      }
    }

    /// <summary>
    /// Return <code>true</code> if the entry is either a junction point or
    /// symbolic link. A junction point applies only to directories, whereas
    /// a symbolic link applies to both files and directories.
    /// </summary>
    public bool IsReparsePoint {
      get {
        return (Attributes & FILE_ATTRIBUTE.FILE_ATTRIBUTE_REPARSE_POINT) != 0;
      }
    }

    public bool IsReadOnly {
      get {
        return (Attributes & FILE_ATTRIBUTE.FILE_ATTRIBUTE_READONLY) != 0;
      }
    }

    public override string ToString() {
      return string.Format("\"{0}\", file:{1}, dir:{2}, link:{3}, attrs:{4}", 
        _name, 
        IsFile,
        IsDirectory,
        IsReparsePoint,
        Enum.Format(typeof(FILE_ATTRIBUTE), Attributes, "f"));
    }
  }
}