using System;
using System.IO;
using System.Linq;
using System.Text;

namespace SteamLibraryExplorer.Utils {
  public static class DirectoryInfoExtensions {
    public static FileInfo GetFile(this DirectoryInfo directoryInfo, string fileName) {
      return directoryInfo
        .EnumerateFiles()
        .FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(fileName, x.Name));
    }

    public static DirectoryInfo GetDirectory(this DirectoryInfo directoryInfo, string fileName) {
      return directoryInfo
        .EnumerateDirectories()
        .FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(fileName, x.Name));
    }

    public static string GetRelativePathTo(this DirectoryInfo entry, DirectoryInfo baseDir) {
      StringBuilder sb = new StringBuilder();
      sb.Insert(0, entry.Name);
      for (var parent = entry.Parent; parent != null; parent = parent.Parent) {
        if (parent.FullName == baseDir.FullName) {
          sb.Insert(0, ".\\");
          return sb.ToString();
        }
        sb.Insert(0, "\\");
        sb.Insert(0, parent.Name);
      }
      return entry.FullName;
    }

    public static string GetRelativePathTo(this FileInfo entry, DirectoryInfo baseDir) {
      StringBuilder sb = new StringBuilder();
      sb.Insert(0, entry.Name);
      for (var parent = entry.Directory; parent != null; parent = parent.Parent) {
        if (parent.FullName == baseDir.FullName) {
          sb.Insert(0, ".\\");
          return sb.ToString();
        }
        sb.Insert(0, "\\");
        sb.Insert(0, parent.Name);
      }
      return entry.FullName;
    }
  }
}