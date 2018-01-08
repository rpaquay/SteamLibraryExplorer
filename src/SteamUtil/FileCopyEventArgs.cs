using System;
using JetBrains.Annotations;
using mtsuite.CoreFileSystem;

namespace SteamLibraryExplorer.SteamUtil {
  public class FileCopyEventArgs : EventArgs {
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