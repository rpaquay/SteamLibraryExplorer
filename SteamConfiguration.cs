﻿using System.Collections.ObjectModel;
using System.IO;
using SteamLibraryExplorer.Property;

namespace SteamLibraryExplorer {
  public class SteamConfiguration {
    public PropertyValue<DirectoryInfo> Location { get; } = new PropertyValue<DirectoryInfo>();
    public ObservableCollection<SteamLibrary> SteamLibraries { get; } = new ObservableCollection<SteamLibrary>();
  }
}