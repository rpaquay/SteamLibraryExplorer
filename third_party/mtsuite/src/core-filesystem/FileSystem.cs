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
using System.IO;
using System.Linq;
using mtsuite.CoreFileSystem.ObjectPool;
using mtsuite.CoreFileSystem.Win32;

namespace mtsuite.CoreFileSystem {
  public class FileSystem : IFileSystem {
    private readonly IPool<List<FileSystemEntry>> _entryListPool = new ListPool<FileSystemEntry>();
    private readonly Win32<FullPath> _win32;

    public FileSystem() : this(new PathSerializers.AsIsSerializer()) {
    }

    public FileSystem(IPathSerializer<FullPath> pathSerializer) {
      _win32 = new Win32<FullPath>(pathSerializer);
    }

    public FileSystemEntry GetEntry(FullPath path) {
      var data = _win32.GetFileAttributesEx(path);
      return new FileSystemEntry(path, new FileSystemEntryData(data));
    }

    public bool TryGetEntry(FullPath path, out FileSystemEntry entry) {
      WIN32_FILE_ATTRIBUTE_DATA data;
      if (_win32.TryGetFileAttributesEx(path, out data) != Win32Errors.ERROR_SUCCESS) {
        entry = default(FileSystemEntry);
        return false;
      }

      entry = new FileSystemEntry(path, new FileSystemEntryData(data));
      return true;
    }

    public DirectoryEntriesEnumerator<FullPath> GetDirectoryEntriesEnumerator(FullPath path, string pattern = null) {
      return _win32.GetDirectoryEntriesEnumerator(path, pattern);
    }

    public FromPool<List<FileSystemEntry>> GetDirectoryEntries(FullPath path, string pattern = null) {
      using (var entries = _win32.GetDirectoryEntries(path, pattern)) {
        var result = _entryListPool.AllocateFrom();
        foreach (var x in entries.Item) {
          result.Item.Add(new FileSystemEntry(path.Combine(x.FileName), new FileSystemEntryData(x.Data)));
        }
        return result;
      }
    }

    public IEnumerable<FileSystemEntry> EnumerateDirectoryEntries(FullPath path, string pattern = null) {
      return _win32.EnumerateDirectoryEntries(path, pattern)
        .Select(entry => new FileSystemEntry(path.Combine(entry.FileName), new FileSystemEntryData(entry.Data)));
    }

    public void DeleteEntry(FileSystemEntry entry) {
      RemoveAccessDeniedAttributes(entry);
      var path = entry.Path;
      if (entry.IsDirectory) {
        _win32.DeleteDirectory(path);
      } else {
        _win32.DeleteFile(path);
      }
    }

    private void RemoveAccessDeniedAttributes(FileSystemEntry entry) {
      if (entry.IsReadOnly || entry.IsSystem) {
        var attrs = entry.FileAttributes & ~(FileAttributes.ReadOnly | FileAttributes.System);
        _win32.SetFileAttributes(entry.Path, (FILE_ATTRIBUTE)attrs);
      }
    }

    public void CopyFile(FileSystemEntry sourceEntry, FileSystemEntry destinationEntry, CopyFileOptions options, CopyFileCallback callback) {
      CopyFileWorker(sourceEntry, destinationEntry.Path, destinationEntry, options, callback);
    }

    public void CopyFile(FileSystemEntry sourceEntry, FullPath destinationPath, CopyFileOptions options, CopyFileCallback callback) {
      FileSystemEntry destinationEntry;
      if (TryGetEntry(destinationPath, out destinationEntry)) {
        CopyFileWorker(sourceEntry, destinationPath, destinationEntry, options, callback);
      } else {
        CopyFileWorker(sourceEntry, destinationPath, null, options, callback);
      }
    }

    private void CopyFileWorker(FileSystemEntry sourceEntry, FullPath destinationPath, FileSystemEntry? destinationEntry, CopyFileOptions options, CopyFileCallback callback) {
      // If the source is a reparse point, delete the destination and
      // copy the reparse point.
      if (sourceEntry.IsReparsePoint) {
        if (destinationEntry.HasValue) {
          try {
            DeleteEntry(destinationEntry.Value);
          } catch {
            // Nothing to do here, as CopyDirectoryReparsePoint will report an exception below.
          }
        }
        if (sourceEntry.IsDirectory) {
          _win32.CopyDirectoryReparsePoint(sourceEntry.Path, destinationPath);
        } else {
          _win32.CopyFileReparsePoint(sourceEntry.Path, destinationPath);
        }
      } else {
        // If destination exists and is read-only, remove the read-only attribute
        if (destinationEntry.HasValue) {
          try {
            RemoveAccessDeniedAttributes(destinationEntry.Value);
          } catch {
            // Nothing to do here, as CopyFile will report an exception below.
          }
        }
        _win32.CopyFile(sourceEntry.Path, destinationPath, options, callback);
      }
    }

    public FileStream OpenFile(FullPath path, FileAccess access) {
      var fileAccess = NativeMethods.EFileAccess.GenericRead;
      if ((access & FileAccess.Read) != 0)
        fileAccess = NativeMethods.EFileAccess.FILE_GENERIC_READ;
      if ((access & FileAccess.Write) != 0)
        fileAccess |= NativeMethods.EFileAccess.FILE_GENERIC_WRITE;

      var handle = _win32.OpenFile(path, fileAccess,
        NativeMethods.EFileShare.Read,
        NativeMethods.ECreationDisposition.OpenExisting, NativeMethods.EFileAttributes.Normal);
      return new FileStream(handle, access);
    }

    public FileStream CreateFile(FullPath path) {
      var handle = _win32.OpenFile(path,
        NativeMethods.EFileAccess.FILE_GENERIC_READ | NativeMethods.EFileAccess.FILE_GENERIC_WRITE,
        NativeMethods.EFileShare.None,
        NativeMethods.ECreationDisposition.CreateAlways,
        NativeMethods.EFileAttributes.Normal);
      return new FileStream(handle, FileAccess.ReadWrite);
    }

    public void CreateDirectory(FullPath path) {
      CreateDirectoryWorker(path);
    }

    public void CreateFileSymbolicLink(FullPath path, string target) {
      _win32.CreateFileSymbolicLink(path, target);
    }

    public void CreateDirectorySymbolicLink(FullPath path, string target) {
      _win32.CreateDirectorySymbolicLink(path, target);
    }

    public void CreateJunctionPoint(FullPath path, string target) {
      // Convert target into an absolute path
      var targetPath = PathHelpers.IsPathAbsolute(target) ? target : path.Parent?.Combine(target).FullName;
      targetPath = PathHelpers.NormalizePath(targetPath);

      _win32.CreateJunctionPoint(path, targetPath);
    }

    public ReparsePointInfo GetReparsePointInfo(FullPath path) {
      var info = _win32.GetReparsePointInfo(path);
      return new ReparsePointInfo {
        IsJunctionPoint = info.IsJunctionPoint,
        IsSymbolicLink = info.IsSymbolicLink,
        Target = info.Target,
        IsTargetRelative = info.IsTargetRelative,
        CreationTimeUtc = info.CreationTimeUtc,
        LastAccessTimeUtc = info.LastAccessTimeUtc,
        LastWriteTimeUtc = info.LastWriteTimeUtc,
      };
    }

    public void CreateDirectoryWorker(FullPath? path) {
      if (path == null)
        return;

      try {
        _win32.CreateDirectory(path.Value);
      } catch {
        if (!TryCreateParent(path.Value))
          throw;
        _win32.CreateDirectory(path.Value);
      }
    }

    private bool TryCreateParent(FullPath path) {
      try {
        CreateDirectoryWorker(path.Parent);
        return true;
      } catch {
        return false;
      }
    }
  }
}