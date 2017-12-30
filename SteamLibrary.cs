using System.Collections.Generic;
using System.IO;

namespace SteamLibraryExplorer {
  public class SteamLibrary {
    public SteamLibrary(DirectoryInfo location, bool isMainLibrary, IEnumerable<SteamGame> games, long freeDiskSize, long totalDiskSize) {
      Location = location;
      IsMainLibrary = isMainLibrary;
      Games.AddRange(games);
      FreeDiskSize = freeDiskSize;
      TotalDiskSize = totalDiskSize;
    }
    public string Id => Location.FullName;
    public bool IsMainLibrary { get; }
    public string DisplayName => IsMainLibrary
      ? $"Main Steam Folder ({Location.FullName})"
      : $"Library Folder ({Location.FullName})";
    public DirectoryInfo Location { get; }
    public List<SteamGame> Games { get; } = new List<SteamGame>();
    public long TotalDiskSize { get; }
    public long FreeDiskSize { get; }
  }
}