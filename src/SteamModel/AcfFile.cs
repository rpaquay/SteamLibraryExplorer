using System.Collections.Concurrent;
using JetBrains.Annotations;
using mtsuite.CoreFileSystem;
using SteamLibraryExplorer.SteamUtil;

namespace SteamLibraryExplorer.SteamModel {
  public class AcfFile {
    private readonly ConcurrentDictionary<string, string> _propertyCache = new ConcurrentDictionary<string, string>();
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
      return _propertyCache.GetOrAdd(propName, name => {
        return SteamDiscovery.GetProperty(Contents, name);
      });
    }
  }
}