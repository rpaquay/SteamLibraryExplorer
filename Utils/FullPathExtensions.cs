using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SteamLibraryExplorer.Utils {
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