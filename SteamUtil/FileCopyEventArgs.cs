using System.IO;
using JetBrains.Annotations;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.SteamUtil {
  public class FileCopyEventArgs {
    public FileCopyEventArgs([NotNull]FullPath sourcePath, [NotNull]FullPath destinationPath) {
      SourcePath = sourcePath;
      DestinationPath = destinationPath;
    }

    [NotNull]
    public FullPath SourcePath { get; }
    [NotNull]
    public FullPath DestinationPath { get; }
  }
}