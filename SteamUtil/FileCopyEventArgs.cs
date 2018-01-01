using System.IO;

namespace SteamLibraryExplorer.SteamUtil {
  public class FileCopyEventArgs {
    public FileCopyEventArgs(FileInfo sourceFile, FileInfo destinationFile) {
      SourceFile = sourceFile;
      DestinationFile = destinationFile;
    }

    public FileInfo SourceFile { get; }
    public FileInfo DestinationFile { get; }
  }
}