using JetBrains.Annotations;
using mtsuite.CoreFileSystem;
using SteamLibraryExplorer.Utils;
using System.Collections.ObjectModel;

namespace SteamLibraryExplorer.SteamModel {
  public class SteamConfiguration {
    [NotNull]
    public PropertyValue<FullPath?> Location { get; } = new PropertyValue<FullPath?>();
    [NotNull]
    public ObservableCollection<SteamLibrary> SteamLibraries { get; } = new ObservableCollection<SteamLibrary>();
  }
}