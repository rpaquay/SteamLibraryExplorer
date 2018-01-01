using JetBrains.Annotations;

namespace SteamLibraryExplorer.SteamModel {
  public class Model {
    [NotNull]
    public SteamConfiguration SteamConfiguration { get; } = new SteamConfiguration();
  }
}
