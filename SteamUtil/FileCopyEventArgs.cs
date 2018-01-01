using System.IO;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.SteamUtil {
  public class FileCopyEventArgs {
    public FileCopyEventArgs(FullPath sourcePath, FullPath destinationPath) {
      SourcePath = sourcePath;
      DestinationPath = destinationPath;
    }

    public FullPath SourcePath { get; }
    public FullPath DestinationPath { get; }
  }
}