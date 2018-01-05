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
using System.ComponentModel;
using System.IO;
using mtsuite.CoreFileSystem;
using mtsuite.CoreFileSystem.Win32;

namespace tests.FileSystemHelpers {
  public class FileSystemSetup : IDisposable {
    private readonly IFileSystem _fileSystem;
    private readonly Lazy<DirectorySetup> _root;

    public FileSystemSetup() {
      _fileSystem = new FileSystem();
      _root = new Lazy<DirectorySetup>(CreateRootDirectory);
    }

    public bool UseLongPaths { get; set; }

    public DirectorySetup Root { get { return _root.Value; } }

    public IFileSystem FileSystem { get { return _fileSystem; } }

    public void Dispose() {
      if (_root.IsValueCreated) {
        var path = _root.Value.Path;
        if (_root.Value.Exists()) {
          Console.WriteLine();
          Console.WriteLine("CLEANUP: Deleting root folder of test file system: \"{0}\"", path.FullName);
          DeleteDirectoryEntriesRecurse(path);
          _fileSystem.DeleteEntry(_fileSystem.GetEntry(path));
        }
      }
    }

    public void SetReadOnlyAttribute(FileEntrySetup entry) {
      SetAttributes(entry, entry.FileAttributes | FileAttributes.ReadOnly);
    }

    public void SetSystemAttribute(FileEntrySetup entry) {
      SetAttributes(entry, entry.FileAttributes | FileAttributes.System);
    }

    public void SetAttributes(FileEntrySetup entry, FileAttributes attributes) {
      new Win32().SetFileAttributes(entry.Path, (FILE_ATTRIBUTE)attributes);
    }

    private void DeleteDirectoryEntriesRecurse(FullPath directory) {
      using (var entries = _fileSystem.GetDirectoryEntries(directory)) {
        foreach (var entry in entries.Item) {
          if (entry.IsDirectory && !entry.IsReparsePoint) {
            DeleteDirectoryEntriesRecurse(entry.Path);
          }
          _fileSystem.DeleteEntry(entry);
        }
      }
    }

    public bool SupportsSymbolicLinkCreation() {
      FileEntrySetup f2;

      try {
        f2 = Root.CreateFileLink("b", "a");
      } catch (Win32Exception e) {
        if (e.NativeErrorCode == (int)Win32Errors.ERROR_PRIVILEGE_NOT_HELD)
          return false;
        throw;
      }
      f2.Delete();
      return true;
    }

    private DirectorySetup CreateRootDirectory() {
      var result = new DirectorySetup(this, CreateTemporaryFolder());
      Console.WriteLine("SETUP: Created temporary root folder for test file system: \"{0}\"", result.Path.FullName);
      Console.WriteLine();
      return result;
    }

    private FullPath CreateTemporaryFolder() {
      var temporaryPath = Path.GetTempPath();
      if (UseLongPaths) {
        temporaryPath = PathHelpers.MakeLongPath(temporaryPath);
      }
      var tempPath = new FullPath(temporaryPath);
      // Note: This is not 100% safe due to possible race conditions.
      for (var i = 0; i < 100; i++) {
        var folderPath = tempPath.Combine(Path.GetRandomFileName());
        FileSystemEntry entry;
        if (_fileSystem.TryGetEntry(folderPath, out entry))
          continue;

        try {
          _fileSystem.CreateDirectory(folderPath);
          return folderPath;
        } catch {
          // Try again!
        }
      }
      throw new ApplicationException("Error creating temporary folder: too many tries");
    }
  }
}
