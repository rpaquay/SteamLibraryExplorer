using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

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

    public bool DirectoryExists => Directory.Exists(FullName);
    public bool FileExists => File.Exists(FullName);

    public void CreateDirectory() {
      Directory.CreateDirectory(FullName);
    }

    public void DeleteDirectory() {
      Directory.Delete(FullName);
    }

    public void DeleteFile() {
      File.Delete(FullName);
    }
  }

  public static class FullPathExtensions {
    public static IEnumerable<FullPath> EnumerateFiles(this FullPath path) {
      return Directory.EnumerateFiles(path.FullName).Select(x => path.Combine(x));
    }

    public static IEnumerable<FullPath> EnumerateFiles(this FullPath path, string pattern) {
      return Directory.EnumerateFiles(path.FullName, pattern).Select(x => path.Combine(x));
    }

    public static IEnumerable<FullPath> EnumerateDirectories(this FullPath path) {
      return Directory.EnumerateDirectories(path.FullName).Select(x => path.Combine(x));
    }

    public static IEnumerable<FullPath> EnumerateFileSystemInfos(this FullPath path) {
      return Directory.EnumerateFileSystemEntries(path.FullName).Select(x => path.Combine(x));
    }

    public static FullPath Combine(this FullPath path, string name) {
      return new FullPath(Path.Combine(path.FullName, name));
    }
  }
}
