using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SteamLibraryExplorer.SteamModel;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.SteamUtil {
  public interface ISteamDiscovery {
    Task<FullPath> LocateSteamFolderAsync();
    Task<SteamLibrary> LoadMainLibraryAsync([NotNull] FullPath steamLocation, CancellationToken cancellationToken);
    Task<IEnumerable<SteamLibrary>> LoadAdditionalLibrariesAsync([NotNull] FullPath steamLocation, CancellationToken cancellationToken);
    Task DiscoverSizeOnDiskAsync([NotNull] IEnumerable<SteamLibrary> libraries, bool useCache, CancellationToken cancellationToken);
    Task<bool> RestartSteamAsync();
    Task<Process> FindSteamProcessAsync();
  }
}