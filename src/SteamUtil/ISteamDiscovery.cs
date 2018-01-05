using JetBrains.Annotations;
using mtsuite.CoreFileSystem;
using SteamLibraryExplorer.SteamModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SteamLibraryExplorer.SteamUtil {
  public interface ISteamDiscovery {
    [NotNull]
    [ItemCanBeNull]
    Task<FullPath> LocateSteamFolderAsync();

    [NotNull]
    [ItemNotNull]
    Task<SteamLibrary> LoadMainLibraryAsync([NotNull] FullPath steamLocation, CancellationToken cancellationToken);

    [NotNull]
    [ItemNotNull]
    Task<IEnumerable<SteamLibrary>> LoadAdditionalLibrariesAsync([NotNull] FullPath steamLocation, CancellationToken cancellationToken);

    [NotNull]
    Task DiscoverSizeOnDiskAsync([NotNull] IEnumerable<SteamLibrary> libraries, bool useCache, CancellationToken cancellationToken);

    [NotNull]
    Task<bool> RestartSteamAsync([NotNull] FullPath steamExePath);

    [NotNull]
    [ItemCanBeNull]
    Task<Process> FindSteamProcessAsync();
  }
}