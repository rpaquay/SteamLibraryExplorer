using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SteamLibraryExplorer.SteamModel;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.SteamUtil {
  public class SteamDiscovery {
    public Task<FullPath> LocateSteamFolderAsync() {
      return RunAsync(LocateSteamUsingProcess);
    }

    public Task<SteamLibrary> LoadMainLibraryAsync(FullPath steamLocation, CancellationToken cancellationToken) {
      return RunAsync(() => LoadLibrary(steamLocation, true, cancellationToken));
    }

    public Task<IEnumerable<SteamLibrary>> LoadAdditionalLibrariesAsync(FullPath steamLocation, CancellationToken cancellationToken) {
      return RunAsync(() => LoadAdditionalLibraries(steamLocation, cancellationToken));
    }

    public Task DiscoverSizeOnDiskAsync(IEnumerable<SteamLibrary> libraries, CancellationToken cancellationToken) {
      // Copy collection, since it could be cleared if a Refresh occurs.
      var copy = libraries.ToList();
      return RunAsync(() => DiscoverSizeOnDisk(copy, cancellationToken));
    }

    private IEnumerable<SteamLibrary> LoadAdditionalLibraries(FullPath steamLocation, CancellationToken cancellationToken) {
      var libraryFile = steamLocation.GetDirectory("steamapps").GetFile("libraryfolders.vdf");
      if (libraryFile != null) {
        var contents = File.ReadAllText(libraryFile.FullName);
        for (var i = 1; i <= 100; i++) {
          var libId = i.ToString();
          var libPath = GetProperty(contents, libId);
          if (libPath == null) {
            break;
          }
          var libDirectory = new FullPath(libPath);
          yield return LoadLibrary(libDirectory, false, cancellationToken);
        }
      }
    }

    private static SteamLibrary LoadLibrary(FullPath steamLocation, bool isMainLibrary, CancellationToken cancellationToken) {
      var acfFiles = LoadAcfFiles(steamLocation).ToList();
      var gameDirs = LoadGameDirectories(steamLocation).ToList();
      var workshopFiles = LoadWorkshopFiles(steamLocation).ToList();

      var gameSet = new HashSet<FullPath>(new DirectoryInfoComparer());
      gameSet.UnionWith(gameDirs);
      gameSet.UnionWith(acfFiles
        .Where(x => x.InstallDir != null)
        .Select(x => steamLocation.CombineDirectory("steamapps").CombineDirectory("common").CombineDirectory(Path.GetFileName(x.InstallDir))));

      var games = gameSet.Select(gameDir => {
        var acfFile = acfFiles
          .FirstOrDefault(acf => StringComparer.OrdinalIgnoreCase.Equals(acf.InstallDir, gameDir.Name));
        var workshopFile = workshopFiles
          .FirstOrDefault(wsFile => acfFile != null && StringComparer.OrdinalIgnoreCase.Equals(wsFile.AppId, acfFile.AppId));
        return new SteamGame(gameDir.DirectoryExists ? gameDir : null, acfFile, workshopFile);
      });

      return new SteamLibrary(steamLocation, isMainLibrary, games);
    }

    private void DiscoverSizeOnDisk(IEnumerable<SteamLibrary> libraries, CancellationToken cancellationToken) {
      foreach (var library in libraries) {
        if (cancellationToken.IsCancellationRequested)
          break;

        ulong userFreeBytes;
        ulong totalBytes;
        ulong freeBytes;
        if (GetDiskFreeSpaceEx(library.Location.FullName, out userFreeBytes, out totalBytes, out freeBytes)) {
          library.FreeDiskSize.Value = (long)freeBytes;
          library.TotalDiskSize.Value = (long)totalBytes;
        }

        foreach (var game in library.Games) {
          DiscoverGameSizeOnDisk(game, cancellationToken);
        }
      }
    }

    private static void DiscoverGameSizeOnDisk(SteamGame game, CancellationToken cancellationToken) {
      DiscoverGameSizeOnDiskRecursive(game, game.Location, cancellationToken);
      DiscoverGameSizeOnDiskRecursive(game, game.WorkshopLocation, cancellationToken);
    }

    private static void DiscoverGameSizeOnDiskRecursive(SteamGame game, FullPath directoryPath, CancellationToken cancellationToken) {
      if (cancellationToken.IsCancellationRequested) {
        return;
      }
      if (directoryPath == null || !directoryPath.DirectoryExists) {
        return;
      }
      var files = directoryPath.EnumerateFiles().ToList();
      var fileBytes = files.Aggregate(0L, (s, x) => s + x.Length);
      game.SizeOnDisk.Value += fileBytes;
      game.FileCount.Value += files.Count;
      foreach (var childDirectory in directoryPath.EnumerateDirectories()) {
        DiscoverGameSizeOnDiskRecursive(game, childDirectory, cancellationToken);
      }
    }

    private static IEnumerable<FullPath> LoadGameDirectories(FullPath steamLocation) {
      return steamLocation.GetDirectory("steamapps").GetDirectory("common").EnumerateDirectories();
    }

    private static IEnumerable<AcfFile> LoadAcfFiles(FullPath steamLocation) {
      return steamLocation.GetDirectory("steamapps")
        .EnumerateFiles("*.acf")
        .Select(x => new AcfFile(x, File.ReadAllText(x.FullName)));
    }

    private static IEnumerable<AcfFile> LoadWorkshopFiles(FullPath steamLocation) {
      return steamLocation.GetDirectory("steamapps").GetDirectory("workshop")
        .EnumerateFiles("*.acf")
        .Select(x => new AcfFile(x, File.ReadAllText(x.FullName)));
    }

    private static Task<T> RunAsync<T>(Func<T> func) {
      return Task.Run(() => func.Invoke());
    }

    private static Task RunAsync(Action func) {
      return Task.Run(func);
    }

    private FullPath LocateSteamUsingProcess() {
      var steamProcesses = Process.GetProcessesByName("Steam");
      return steamProcesses.Select(GetSteamProcessFolder).FirstOrDefault(x => x != null);
    }

    private FullPath GetSteamProcessFolder(Process process) {
      var processPath = new FullPath(process.MainModule.FileName);
      if (!processPath.FileExists) {
        return null;
      }

      var dir = processPath.Parent;
      if (dir == null || !dir.DirectoryExists) {
        return null;
      }

      if (dir.GetDirectory("steamapps") == null) {
        return null;
      }

      return dir;
    }

    public static string GetProperty(string contents, string propName) {
      var regex = new Regex("^.*\"" + propName + "\".*\"(?<propValue>.+)\"$", RegexOptions.IgnoreCase);
      using (var reader = new StringReader(contents)) {
        for (var line = reader.ReadLine(); line != null; line = reader.ReadLine()) {
          var match = regex.Match(line);
          if (match.Success) {
            return match.Groups["propValue"].Value.Replace("\\\\", "\\");
          }
        }
      }
      return null;
    }

    public class DirectoryInfoComparer : IEqualityComparer<FullPath> {
      public bool Equals(FullPath x, FullPath y) {
        return StringComparer.OrdinalIgnoreCase.Equals(x.FullName, y.FullName);
      }

      public int GetHashCode(FullPath obj) {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.FullName);
      }
    }


    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetDiskFreeSpaceEx(string lpDirectoryName,
      out ulong lpFreeBytesAvailable,
      out ulong lpTotalNumberOfBytes,
      out ulong lpTotalNumberOfFreeBytes);
  }
}
