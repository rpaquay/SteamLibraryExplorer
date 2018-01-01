using System.IO;

namespace SteamLibraryExplorer.SteamUtil {
  public class FileDeleteEventArgs {
    public FileDeleteEventArgs(FileInfo file) {
      File = file;
    }

    public FileInfo File { get; }
  }
}