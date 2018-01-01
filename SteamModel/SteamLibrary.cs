using System.Collections.Generic;
using System.IO;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.SteamModel {
  public class SteamLibrary {
    public SteamLibrary(FullPath location, bool isMainLibrary, IEnumerable<SteamGame> games) {
      Location = location;
      IsMainLibrary = isMainLibrary;
      Games.AddRange(games);
    }
    public string Id => Location.FullName;
    public bool IsMainLibrary { get; }
    public string DisplayName => IsMainLibrary
      ? $"Main Steam Folder ({Location.FullName})"
      : $"Library Folder ({Location.FullName})";
    public FullPath Location { get; }
    public List<SteamGame> Games { get; } = new List<SteamGame>();
    public PropertyValue<long> TotalDiskSize { get; } = new PropertyValue<long>();
    public PropertyValue<long> FreeDiskSize { get; } = new PropertyValue<long>();
  }
}