using System.Collections.ObjectModel;
using JetBrains.Annotations;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.SteamModel {
  public class SteamConfiguration {
    [NotNull]
    public PropertyValue<FullPath> Location { get; } = new PropertyValue<FullPath>();
    [NotNull]
    public ObservableCollection<SteamLibrary> SteamLibraries { get; } = new ObservableCollection<SteamLibrary>();
  }
}