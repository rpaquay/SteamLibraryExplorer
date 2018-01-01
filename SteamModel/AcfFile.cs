using SteamLibraryExplorer.SteamUtil;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.SteamModel {
  public class AcfFile {
    public AcfFile(FullPath path, string contents) {
      Path = path;
      Contents = contents;
    }

    public FullPath Path { get; }
    public string Contents { get; }

    public string AppId => GetProperty("appid");
    public string InstallDir => GetProperty("installdir");
    public string GameDisplayName => GetProperty("name");

    private string GetProperty(string propName) {
      return SteamDiscovery.GetProperty(Contents, propName);
    }
  }
}