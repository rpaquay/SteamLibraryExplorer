using System.Diagnostics;
using System.IO;
using SteamLibraryExplorer.Property;

namespace SteamLibraryExplorer {
  public class SteamGame {
    public SteamGame(DirectoryInfo location, AcfFile acfFile) {
      Debug.Assert(location != null || acfFile != null);
      Location = location;
      AcfFile = acfFile;
    }

    public DirectoryInfo Location { get; }
    public AcfFile AcfFile { get; }
    public string DisplayName {
      get {
        if (AcfFile != null) {
          var name = AcfFile?.GameDisplayName;
          if (name != null) {
            return name;
          }
          name = AcfFile.InstallDir;
          if (name != null) {
            return name;
          }
        }
        if (Location != null) {
          return Location.Name;
        }
        Debug.Assert(false);
        return "n/a";
      }
    }
    public PropertyValue<bool> CalculatingSizeOnDisk { get; } = new PropertyValue<bool>(false);
    public PropertyValue<long> SizeOnDisk { get; } = new PropertyValue<long>();
    public PropertyValue<long> FileCount { get; } = new PropertyValue<long>();

  }
}