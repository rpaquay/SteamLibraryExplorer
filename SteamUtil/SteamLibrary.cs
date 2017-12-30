using System.Collections.Generic;
using System.IO;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.SteamUtil {
  public class SteamLibrary {
    public SteamLibrary(DirectoryInfo location, bool isMainLibrary, IEnumerable<SteamGame> games) {
      Location = location;
      IsMainLibrary = isMainLibrary;
      Games.AddRange(games);
    }
    public string Id => Location.FullName;
    public bool IsMainLibrary { get; }
    public string DisplayName => IsMainLibrary
      ? $"Main Steam Folder ({Location.FullName})"
      : $"Library Folder ({Location.FullName})";
    public DirectoryInfo Location { get; }
    public List<SteamGame> Games { get; } = new List<SteamGame>();
    public PropertyValue<long> TotalDiskSize { get; } = new PropertyValue<long>();
    public PropertyValue<long> FreeDiskSize { get; } = new PropertyValue<long>();
  }
}