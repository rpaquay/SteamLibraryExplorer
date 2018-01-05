using JetBrains.Annotations;
using mtsuite.CoreFileSystem;
using System.Text;

namespace SteamLibraryExplorer.Utils {
  public static class FullPathExtensions {
    [NotNull]
    public static FullPath Combine(this FullPath path, [NotNull]string name1, [NotNull]string name2) {
      return path.Combine(name1).Combine(name2);
    }

    [NotNull]
    public static FullPath Combine(this FullPath path, [NotNull]string name1, [NotNull]string name2, [NotNull]string name3) {
      return path.Combine(name1).Combine(name2).Combine(name3);
    }

    [NotNull]
    public static FullPath Combine(this FullPath path, [NotNull]string name1, [NotNull]string name2, [NotNull]string name3, [NotNull]string name4) {
      return path.Combine(name1).Combine(name2).Combine(name3).Combine(name4);
    }


    [NotNull]
    public static string GetRelativePathTo([NotNull]this FullPath entry, [NotNull]FullPath baseDir) {
      var sb = new StringBuilder();
      sb.Insert(0, entry.Name);
      for (var parent = entry.Parent; parent != null; parent = parent.Parent) {
        if (parent.Equals(baseDir)) {
          sb.Insert(0, ".\\");
          return sb.ToString();
        }
        sb.Insert(0, "\\");
        sb.Insert(0, parent.Name);
      }
      return entry.FullName;
    }
  }}