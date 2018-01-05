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
using System.Linq;
using mtsuite.CoreFileSystem;

namespace tests.FileSystemHelpers {
  public class DirectorySetup : FileEntrySetup {
    /// <summary>
    /// Create the root directory
    /// </summary>
    public DirectorySetup(FileSystemSetup fileSystemSetup, FullPath path)
      : base(fileSystemSetup, path) {
    }

    /// <summary>
    /// Create a sub directory of <paramref name="parent"/>
    /// </summary>
    public DirectorySetup(DirectorySetup parent, string name)
      : base(parent, name) {
    }

    public FileSetup CreateFile(string name, int size) {
      var file = new FileSetup(this, name);
      using (var stream = FileSystemSetup.FileSystem.CreateFile(file.Path)) {
        var buffer = new byte[size];
        stream.Write(buffer, 0, buffer.Length);
      }
      return file;
    }

    public FileLinkSetup CreateFileLink(string name, string target) {
      var fileLink = new FileLinkSetup(this, name);
      FileSystemSetup.FileSystem.CreateFileSymbolicLink(fileLink.Path, target);
      return fileLink;
    }

    public DirectoryLinkSetup CreateDirectoryLink(string name, string target) {
      var directoryLink = new DirectoryLinkSetup(this, name);
      FileSystemSetup.FileSystem.CreateDirectorySymbolicLink(directoryLink.Path, target);
      return directoryLink;
    }

    public JunctionPointSetup CreateJunctionPoint(string name, string target) {
      var junctionPoint = new JunctionPointSetup(this, name);
      FileSystemSetup.FileSystem.CreateJunctionPoint(junctionPoint.Path, target);
      return junctionPoint;
    }

    public DirectorySetup CreateDirectory(string name) {
      var directory = new DirectorySetup(this, name);
      FileSystemSetup.FileSystem.CreateDirectory(directory.Path);
      return directory;
    }

    public List<FileEntrySetup> GetEntries() {
      return FileSystemSetup.FileSystem.GetDirectoryEntries(Path).Item
        .Select(x => MapEntry(this, x))
        .ToList();
    }

    public FileEntrySetup GetEntry(string name) {
      var entry = FileSystemSetup.FileSystem.GetEntry(Path.Combine(name));

      return MapEntry(this, entry);
    }

    public DirectorySetup GetDirectory(string name) {
      return GetEntry<DirectorySetup>(name);
    }

    public FileSetup GetFile(string name) {
      return GetEntry<FileSetup>(name);
    }

    public FileLinkSetup GetFileLink(string name) {
      return GetEntry<FileLinkSetup>(name);
    }

    public DirectoryLinkSetup GetDirectoryLink(string name) {
      return GetEntry<DirectoryLinkSetup>(name);
    }

    public JunctionPointSetup GetJunctionPoint(string name) {
      return GetEntry<JunctionPointSetup>(name);
    }

    public T GetEntry<T>(string name) where T : FileEntrySetup {
      var entry = GetEntry(name);
      var result = entry as T;
      if (result == null)
        throw new ArgumentException("Invalid entry type");
      return result;
    }
  }
}