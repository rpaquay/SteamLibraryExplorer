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
using mtsuite.CoreFileSystem.Win32;

namespace mtsuite.CoreFileSystem {
  public struct FileSystemEntry {
    private readonly FullPath _path;
    private readonly FileSystemEntryData _data;

    public FileSystemEntry(FullPath path, uint attributes, uint fileSizeHigh, uint fileSizeLow,
      uint ftLastWriteTimeHigh, uint ftLastWriteTimeLow) {
      _path = path;
      _data = new FileSystemEntryData(attributes, fileSizeHigh, fileSizeLow, ftLastWriteTimeHigh, ftLastWriteTimeLow);
    }

    public FileSystemEntry(FullPath path, WIN32_FIND_DATA data) {
      _path = path;
      _data = new FileSystemEntryData(data);
    }

    public FileSystemEntry(FullPath path, WIN32_FILE_ATTRIBUTE_DATA data) {
      _path = path;
      _data = new FileSystemEntryData(data);
    }

    public FullPath Path { get { return _path; } }

    public string Name { get { return _path.Name; } }

    public long FileSize { get { return _data.FileSize; } }

    public DateTime LastWriteTimeUtc { get { return _data.LastWriteTimeUtc; } }

    public FileAttributes FileAttributes { get { return _data.FileAttributes; } }

    public bool IsFile { get { return _data.IsFile; } }

    public bool IsDirectory { get { return _data.IsDirectory; } }

    /// <summary>
    /// Return <code>true</code> if the entry is either a junction point or
    /// symbolic link. A junction point applies only to directories, whereas
    /// a symbolic link applies to both files and directories.
    /// </summary>
    public bool IsReparsePoint { get { return _data.IsReparsePoint; } }

    public bool IsReadOnly { get { return _data.IsReadOnly; } }

    public bool IsSystem { get { return _data.IsSystem; } }

    public override string ToString() {
      return string.Format("\"{0}\", {1}", _path.Name, _data);
    }
  }

  public struct FileSystemEntryData {
    private readonly FileAttributes _attributes;
    private readonly long _fileSize;
    private readonly long _lastWriteTimeUtc;

    public FileSystemEntryData(uint attributes, uint fileSizeHigh, uint fileSizeLow,
      uint ftLastWriteTimeHigh, uint ftLastWriteTimeLow) {
      _attributes = (FileAttributes)attributes;
      _fileSize = HighLowToLong(fileSizeHigh, fileSizeLow);
      _lastWriteTimeUtc = HighLowToLong(ftLastWriteTimeHigh, ftLastWriteTimeLow);
    }

    public FileSystemEntryData(WIN32_FIND_DATA data)
      : this(data.dwFileAttributes, data.nFileSizeHigh, data.nFileSizeLow,
        data.ftLastWriteTime_dwHighDateTime, data.ftLastWriteTime_dwHighDateTime) {
    }

    public FileSystemEntryData(WIN32_FILE_ATTRIBUTE_DATA data)
      : this(data.fileAttributes, data.fileSizeHigh, data.fileSizeLow,
        data.ftLastWriteTimeHigh, data.ftLastWriteTimeLow) {
    }

    public long FileSize { get { return _fileSize; } }

    public DateTime LastWriteTimeUtc { get { return DateTime.FromFileTimeUtc(_lastWriteTimeUtc); } }

    public FileAttributes FileAttributes { get { return _attributes; } }

    public bool IsFile { get { return (_attributes & FileAttributes.Directory) == 0; } }

    public bool IsDirectory { get { return (_attributes & FileAttributes.Directory) != 0; } }

    /// <summary>
    /// Return <code>true</code> if the entry is either a junction point or
    /// symbolic link. A junction point applies only to directories, whereas
    /// a symbolic link applies to both files and directories.
    /// </summary>
    public bool IsReparsePoint { get { return (_attributes & FileAttributes.ReparsePoint) != 0; } }

    public bool IsReadOnly { get { return (_attributes & FileAttributes.ReadOnly) != 0; } }

    public bool IsSystem { get { return (_attributes & FileAttributes.System) != 0; } }

    public override string ToString() {
      return string.Format("file:{0}, dir:{1}, link:{2}, attrs:{3}, date: {4}",
        IsFile,
        IsDirectory,
        IsReparsePoint,
        Enum.Format(typeof(FileAttributes), _attributes, "f"),
        LastWriteTimeUtc);
    }

    private static long HighLowToLong(uint high, uint low) {
      return low + ((long)high << 32);
    }
  }

  public struct FileSystemEntryWithFileName {
    private readonly WIN32_FIND_DATA _win32Data;
    private readonly FileSystemEntryData _data;

    public FileSystemEntryWithFileName(WIN32_FIND_DATA data) {
      _win32Data = data;
      _data = new FileSystemEntryData(data);
    }

    public string FileName { get { return _win32Data.GetFileName(); } }

    public long FileSize { get { return _data.FileSize; } }

    public DateTime LastWriteTimeUtc { get { return _data.LastWriteTimeUtc; } }

    public FileAttributes FileAttributes { get { return _data.FileAttributes; } }

    public bool IsFile { get { return _data.IsFile; } }

    public bool IsDirectory { get { return _data.IsDirectory; } }

    /// <summary>
    /// Return <code>true</code> if the entry is either a junction point or
    /// symbolic link. A junction point applies only to directories, whereas
    /// a symbolic link applies to both files and directories.
    /// </summary>
    public bool IsReparsePoint { get { return _data.IsReparsePoint; } }

    public bool IsReadOnly { get { return _data.IsReadOnly; } }

    public bool IsSystem { get { return _data.IsSystem; } }

    public override string ToString() {
      return string.Format("\"{0}\", {1}", FileName, _data);
    }
  }

}