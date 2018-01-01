using System.Text;
using JetBrains.Annotations;

namespace SteamLibraryExplorer.Utils {
  public static class FullPathExtensions {
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
  }
}