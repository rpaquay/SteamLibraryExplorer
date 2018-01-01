using JetBrains.Annotations;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.SteamUtil {
  public class PathEventArgs {
    public PathEventArgs([NotNull]FullPath path) {
      Path = path;
    }

    [NotNull]
    public FullPath Path { get; }
  }
}