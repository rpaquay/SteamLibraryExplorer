using System.Diagnostics;
using JetBrains.Annotations;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.SteamModel {
  public class SteamGame {
    public SteamGame([NotNull]FullPath location, [CanBeNull]AcfFile acfFile, [CanBeNull]AcfFile workshopFile) {
      Debug.Assert(acfFile == null || workshopFile == null || acfFile.AppId == workshopFile.AppId);
      Location = location;
      AcfFile = acfFile;
      WorkshopFile = workshopFile;
    }

    [NotNull]
    public FullPath Location { get; }
    [CanBeNull]
    public AcfFile AcfFile { get; }
    [CanBeNull]
    public AcfFile WorkshopFile { get; }

    [NotNull]
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
        return Location.Name;
      }
    }

    [NotNull]
    public PropertyValue<bool> CalculatingSizeOnDisk { get; } = new PropertyValue<bool>(false);
    [NotNull]
    public PropertyValue<long> SizeOnDisk { get; } = new PropertyValue<long>();
    [NotNull]
    public PropertyValue<long> FileCount { get; } = new PropertyValue<long>();

    [NotNull]
    public PropertyValue<long> WorkshopSizeOnDisk { get; } = new PropertyValue<long>();
    [NotNull]
    public PropertyValue<long> WorkshopFileCount { get; } = new PropertyValue<long>();

    /// <summary>
    /// The directory where the workshop files are located.
    /// The value is <code>null</code> if there is no workshop file.
    /// The directory may not exist (if there are no workshop files).
    /// </summary>
    [CanBeNull]
    public FullPath WorkshopLocation {
      get {
        if (WorkshopFile == null || WorkshopFile.AppId == null || WorkshopFile.Path.Parent == null) {
          return null;
        }

        return WorkshopFile.Path.Parent.Combine("content").Combine(WorkshopFile.AppId);
      }
    }
  }
}