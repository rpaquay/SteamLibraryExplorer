using System.IO;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.SteamUtil {
  public class FileDeleteEventArgs {
    public FileDeleteEventArgs(FullPath path) {
      Path = path;
    }

    public FullPath Path { get; }
  }
}