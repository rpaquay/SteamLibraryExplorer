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
using mtsuite.CoreFileSystem;

namespace tests.FileSystemHelpers {
  public abstract class FileEntrySetup {
    private readonly FileSystemSetup _fileSystemSetup;
    private readonly DirectorySetup _parent;
    private readonly FullPath _path;
    private static readonly char[] InvalidNameCharacters = {
      System.IO.Path.DirectorySeparatorChar,
      System.IO.Path.VolumeSeparatorChar,
      System.IO.Path.AltDirectorySeparatorChar
    };

    protected FileEntrySetup(FileSystemSetup fileSystemSetup, FullPath path) {
      if (fileSystemSetup == null)
        throw new ArgumentNullException("fileSystemSetup");
      if (path == null)
        throw new ArgumentNullException("path");
      _fileSystemSetup = fileSystemSetup;
      _parent = null;
      _path = path;
    }

    protected FileEntrySetup(DirectorySetup parent, string name) {
      if (parent == null)
        throw new ArgumentNullException("parent");
      if (string.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");
      if (name.IndexOfAny(InvalidNameCharacters) >= 0 || name == "." || name == "..")
        throw new ArgumentException("Name must be a single path component", "name");

      _fileSystemSetup = parent.FileSystemSetup;
      _parent = parent;
      _path = parent.Path.Combine(name);
    }

    public FileSystemSetup FileSystemSetup { get { return _fileSystemSetup; } }

    public DirectorySetup Parent {
      get {
        if (_parent == null) {
          throw new ApplicationException("Root directory does not have a parent");
        }
        return _parent;
      }
    }

    public FullPath Path { get { return _path; } }

    public void Delete() {
      FileSystemSetup.FileSystem.DeleteEntry(GetFileSystemEntry());
    }

    public FileAttributes FileAttributes {
      get { return GetFileSystemEntry().FileAttributes; }
    }

    public void SetReadOnlyAttribute() {
      _fileSystemSetup.SetReadOnlyAttribute(this);
    }

    public void SetSystemAttribute() {
      _fileSystemSetup.SetSystemAttribute(this);
    }

    private FileSystemEntry GetFileSystemEntry() {
      return FileSystemSetup.FileSystem.GetEntry(Path);
    }

    public bool Exists() {
      FileSystemEntry entry;
      if (!FileSystemSetup.FileSystem.TryGetEntry(Path, out entry))
        return false;
      var mappedEntry = MapEntry(_parent, entry);
      return mappedEntry.GetType().Equals(this.GetType());
    }

    protected FileEntrySetup MapEntry(DirectorySetup parent, FileSystemEntry entry) {
      if (parent == null) {
        if (entry.IsDirectory && !entry.IsReparsePoint) {
          return new DirectorySetup(FileSystemSetup, entry.Path);
        }
      } else {
        if (entry.IsFile && !entry.IsReparsePoint) {
          return new FileSetup(parent, entry.Name);
        }
        if (entry.IsFile && entry.IsReparsePoint) {
          return new FileLinkSetup(parent, entry.Name);
        }
        if (entry.IsDirectory && !entry.IsReparsePoint) {
          return new DirectorySetup(parent, entry.Name);
        }
        if (entry.IsDirectory && entry.IsReparsePoint) {
          var info = _fileSystemSetup.FileSystem.GetReparsePointInfo(entry.Path);
          if (info.IsJunctionPoint) {
            return new JunctionPointSetup(parent, entry.Name);
          } else {
            return new DirectoryLinkSetup(parent, entry.Name);
          }
        }
      }

      throw new ArgumentException("Unknown entry type");
    }
  }
}