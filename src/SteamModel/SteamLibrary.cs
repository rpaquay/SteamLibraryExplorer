using JetBrains.Annotations;
using mtsuite.CoreFileSystem;
using SteamLibraryExplorer.Utils;
using System.Collections.Generic;

namespace SteamLibraryExplorer.SteamModel {
  public class SteamLibrary {
    public SteamLibrary([NotNull]FullPath location, bool isMainLibrary, [NotNull]IEnumerable<SteamGame> games) {
      Location = location;
      IsMainLibrary = isMainLibrary;
      Games.AddRange(games);
    }
    [NotNull]
    public string Id => Location.FullName;
    public bool IsMainLibrary { get; }
    [NotNull]
    public string DisplayName => IsMainLibrary
      ? $"Main Steam Folder ({Location.FullName})"
      : $"Library Folder ({Location.FullName})";
    [NotNull]
    public FullPath Location { get; }
    [NotNull]
    public List<SteamGame> Games { get; } = new List<SteamGame>();
    [NotNull]
    public PropertyValue<long> TotalDiskSize { get; } = new PropertyValue<long>();
    [NotNull]
    public PropertyValue<long> FreeDiskSize { get; } = new PropertyValue<long>();
  }
}