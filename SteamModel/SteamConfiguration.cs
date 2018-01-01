using System.Collections.ObjectModel;
using System.IO;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.SteamModel {
  public class SteamConfiguration {
    public PropertyValue<FullPath> Location { get; } = new PropertyValue<FullPath>();
    public ObservableCollection<SteamLibrary> SteamLibraries { get; } = new ObservableCollection<SteamLibrary>();
  }
}