using System;
using System.IO;
using System.Linq;
using System.Text;

namespace SteamLibraryExplorer.Utils {
  public static class DirectoryInfoExtensions {
    public static FullPath GetFile(this FullPath directoryInfo, string fileName) {
      return directoryInfo
        .EnumerateFiles()
        .FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(fileName, x.Name));
    }

    public static FullPath GetDirectory(this FullPath directoryInfo, string fileName) {
      return directoryInfo
        .EnumerateDirectories()
        .FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(fileName, x.Name));
    }

    public static string GetRelativePathTo(this FullPath entry, FullPath baseDir) {
      StringBuilder sb = new StringBuilder();
      sb.Insert(0, entry.Name);
      for (var parent = entry.Parent; parent != null; parent = parent.Parent) {
        if (StringComparer.OrdinalIgnoreCase.Equals(parent.FullName, baseDir.FullName)) {
          sb.Insert(0, ".\\");
          return sb.ToString();
        }
        sb.Insert(0, "\\");
        sb.Insert(0, parent.Name);
      }
      return entry.FullName;
    }

    public static FullPath CombineDirectory(this FullPath directory, string name) {
      return new FullPath(Path.Combine(directory.FullName, name));
    }

    public static FullPath CombineFile(this FullPath directory, string name) {
      return new FullPath(Path.Combine(directory.FullName, name));
    }
  }
}