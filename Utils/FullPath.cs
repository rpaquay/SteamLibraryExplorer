﻿using System.Diagnostics;
using System.IO;

namespace SteamLibraryExplorer.Utils {
  public class FullPath {
    public FullPath(string fullName) {
      Debug.Assert(!string.IsNullOrEmpty(fullName));
      FullName = fullName;
    }

    public string FullName { get; }
    public string Name => Path.GetFileName(FullName);
    public long Length => new FileInfo(FullName).Length;

    public FullPath Parent {
      get {
        var parentDir = Path.GetDirectoryName(FullName);
        return parentDir == null ? null : new FullPath(parentDir);
      }
    }

    public bool DirectoryExists => FileSystem.DirectoryExists(this);
    public bool FileExists => FileSystem.FileExists(this);

    public void CreateDirectory() {
      FileSystem.CreateDirectory(this);
    }

    public void DeleteDirectory() {
      FileSystem.DeleteDirectory(this);
    }

    public void DeleteFile() {
      FileSystem.DeleteFile(this);
    }
  }
}