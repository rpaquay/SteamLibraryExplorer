using System.IO;
using SteamLibraryExplorer.SteamUtil;

namespace SteamLibraryExplorer.SteamModel {
  public class AcfFile {
    public AcfFile(FileInfo fileInfo, string contents) {
      FileInfo = fileInfo;
      Contents = contents;
    }

    public FileInfo FileInfo { get; }
    public string Contents { get; }

    public string InstallDir => GetProperty("installdir");
    public string GameDisplayName => GetProperty("name");

    private string GetProperty(string propName) {
      return SteamDiscovery.GetProperty(Contents, propName);
    }
  }
}