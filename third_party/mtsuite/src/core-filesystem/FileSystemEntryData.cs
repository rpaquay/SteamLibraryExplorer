using System;
using System.IO;
using mtsuite.CoreFileSystem.Win32;

namespace mtsuite.CoreFileSystem {
  public struct FileSystemEntryData {
    private readonly FileAttributes _attributes;
    private readonly long _fileSize;
    private readonly long _lastWriteTimeUtc;

    public FileSystemEntryData(WIN32_FIND_DATA data)
      : this(data.dwFileAttributes, data.nFileSizeHigh, data.nFileSizeLow,
        data.ftLastWriteTime_dwHighDateTime, data.ftLastWriteTime_dwHighDateTime) {
    }

    public FileSystemEntryData(WIN32_FILE_ATTRIBUTE_DATA data)
      : this(data.fileAttributes, data.fileSizeHigh, data.fileSizeLow,
        data.ftLastWriteTimeHigh, data.ftLastWriteTimeLow) {
    }

    public FileSystemEntryData(uint attributes, uint fileSizeHigh, uint fileSizeLow,
      uint ftLastWriteTimeHigh, uint ftLastWriteTimeLow) {
      _attributes = (FileAttributes)attributes;
      _fileSize = HighLowToLong(fileSizeHigh, fileSizeLow);
      _lastWriteTimeUtc = HighLowToLong(ftLastWriteTimeHigh, ftLastWriteTimeLow);
    }

    public long FileSize => _fileSize;

    public DateTime LastWriteTimeUtc => DateTime.FromFileTimeUtc(_lastWriteTimeUtc);

    public FileAttributes FileAttributes => _attributes;

    public bool IsFile => (_attributes & FileAttributes.Directory) == 0;
    public bool IsDirectory => (_attributes & FileAttributes.Directory) != 0;
    /// <summary>
    /// Return <code>true</code> if the entry is either a junction point or
    /// symbolic link. A junction point applies only to directories, whereas
    /// a symbolic link applies to both files and directories.
    /// </summary>
    public bool IsReparsePoint => (_attributes & FileAttributes.ReparsePoint) != 0;
    public bool IsReadOnly => (_attributes & FileAttributes.ReadOnly) != 0;
    public bool IsSystem => (_attributes & FileAttributes.System) != 0;

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
}