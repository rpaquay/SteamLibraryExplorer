using System.Diagnostics;
using System.IO;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.SteamModel {
  public class SteamGame {
    public SteamGame(DirectoryInfo location, AcfFile acfFile, AcfFile workshopFile) {
      Debug.Assert(location != null || acfFile != null);
      Debug.Assert(acfFile == null || workshopFile == null || acfFile.AppId == workshopFile.AppId);
      Location = location;
      AcfFile = acfFile;
      WorkshopFile = workshopFile;
    }

    public DirectoryInfo Location { get; }
    public AcfFile AcfFile { get; }
    public AcfFile WorkshopFile { get; }

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

    /// <summary>
    /// The directory where the workshop files are located.
    /// The value is <code>null</code> if there is no workshop file.
    /// The directory may not exist (if there are no workshop files).
    /// </summary>
    public DirectoryInfo WorkshopLocation {
      get {
        if (Location == null || WorkshopFile == null || WorkshopFile.AppId == null) {
          return null;
        }

        return WorkshopFile.FileInfo.Directory.CombineDirectory("content").CombineDirectory(WorkshopFile.AppId);
      }
    }
  }
}