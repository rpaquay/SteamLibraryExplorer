using JetBrains.Annotations;
using SteamLibraryExplorer.SteamUtil;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.SteamModel {
  public class AcfFile {
    public AcfFile([NotNull]FullPath path, [NotNull]string contents) {
      Path = path;
      Contents = contents;
    }

    [NotNull]
    public FullPath Path { get; }
    [NotNull]
    public string Contents { get; }

    [CanBeNull]
    public string AppId => GetProperty("appid");
    [CanBeNull]
    public string InstallDir => GetProperty("installdir");
    [CanBeNull]
    public string GameDisplayName => GetProperty("name");

    [CanBeNull]
    private string GetProperty([NotNull]string propName) {
      return SteamDiscovery.GetProperty(Contents, propName);
    }
  }
}