using System.IO;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.SteamUtil {
  public class PathEventArgs {
    public PathEventArgs(FullPath path) {
      Path = path;
    }

    public FullPath Path { get; }
  }
}