using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Win32;
using SteamLibraryExplorer.SteamModel;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer.SteamUtil {
  public class SteamDiscovery {
    private static readonly ILoggerFacade Logger = LoggerManagerFacade.GetLogger(typeof(SteamDiscovery));

    [NotNull]
    public Task<FullPath> LocateSteamFolderAsync() {
      return RunAsync(LocateSteamFolder);
    }

    [NotNull]
    public Task<SteamLibrary> LoadMainLibraryAsync([NotNull] FullPath steamLocation,
      CancellationToken cancellationToken) {
      return RunAsync(() => LoadLibrary(steamLocation, true, cancellationToken));
    }

    [NotNull]
    public Task<IEnumerable<SteamLibrary>> LoadAdditionalLibrariesAsync([NotNull] FullPath steamLocation,
      CancellationToken cancellationToken) {
      return RunAsync(() => LoadAdditionalLibraries(steamLocation, cancellationToken));
    }

    [NotNull]
    public Task DiscoverSizeOnDiskAsync([NotNull] IEnumerable<SteamLibrary> libraries,
      CancellationToken cancellationToken) {
      // Copy collection, since it could be cleared if a Refresh occurs.
      var copy = libraries.ToList();
      return RunAsync(() => DiscoverSizeOnDisk(copy, cancellationToken));
    }

    [NotNull]
    private IEnumerable<SteamLibrary> LoadAdditionalLibraries([NotNull] FullPath steamLocation,
      CancellationToken cancellationToken) {
      cancellationToken.ThrowIfCancellationRequested();

      var libraryPath = steamLocation.Combine("steamapps").Combine("libraryfolders.vdf");
      if (FileSystem.FileExists(libraryPath)) {
        var contents = FileSystem.ReadAllText(libraryPath);
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

    [NotNull]
    private static SteamLibrary LoadLibrary([NotNull] FullPath libraryRootPath, bool isMainLibrary,
      CancellationToken cancellationToken) {
      cancellationToken.ThrowIfCancellationRequested();

      Logger.Info("Loading game definitions from Steam library at \"{0}\"", libraryRootPath.FullName);

      var acfFiles = LoadAcfFiles(libraryRootPath).ToList();
      var gameDirs = LoadGameDirectories(libraryRootPath).ToList();
      var workshopFiles = LoadWorkshopFiles(libraryRootPath).ToList();

      var gameSet = new HashSet<FullPath>();
      gameSet.UnionWith(gameDirs);
      gameSet.UnionWith(acfFiles
        .Where(x => x.InstallDir != null)
        .Select(x => libraryRootPath.Combine("steamapps").Combine("common").Combine(Path.GetFileName(x.InstallDir))));

      var games = gameSet.Select(gameDir => {
        var acfFile = acfFiles
          .FirstOrDefault(acf => StringComparer.OrdinalIgnoreCase.Equals(acf.InstallDir, gameDir.Name));
        var workshopFile = workshopFiles
          .FirstOrDefault(wsFile => acfFile != null &&
                                    StringComparer.OrdinalIgnoreCase.Equals(wsFile.AppId, acfFile.AppId));
        Logger.Info("Found game: Directory=\"{0}\", ACF file=\"{1}\", Workshop file=\"{2}\"", 
          gameDir.FullName, acfFile?.Path.FullName ?? "<None>", workshopFile?.Path.FullName ?? "<None>");
        return new SteamGame(gameDir, acfFile, workshopFile);
      }).ToList();

      Logger.Info("Done loading game definitions from Steam library at \"{0}\"", libraryRootPath.FullName);
      return new SteamLibrary(libraryRootPath, isMainLibrary, games);
    }

    private void DiscoverSizeOnDisk([NotNull] IEnumerable<SteamLibrary> libraries,
      CancellationToken cancellationToken) {
      foreach (var library in libraries) {
        if (cancellationToken.IsCancellationRequested)
          break;

        ulong userFreeBytes;
        ulong totalBytes;
        ulong freeBytes;
        if (GetDiskFreeSpaceEx(library.Location.FullName, out userFreeBytes, out totalBytes, out freeBytes)) {
          library.FreeDiskSize.Value = (long) freeBytes;
          library.TotalDiskSize.Value = (long) totalBytes;
        }

        foreach (var game in library.Games) {
          DiscoverGameSizeOnDisk(game, cancellationToken);
        }
      }
    }

    private static void DiscoverGameSizeOnDisk([NotNull] SteamGame game, CancellationToken cancellationToken) {
      DiscoverGameSizeOnDiskRecursive(game, game.Location, cancellationToken);
      DiscoverGameSizeOnDiskRecursive(game, game.WorkshopLocation, cancellationToken);
    }

    private static void DiscoverGameSizeOnDiskRecursive([NotNull] SteamGame game, [CanBeNull] FullPath directoryPath,
      CancellationToken cancellationToken) {
      if (cancellationToken.IsCancellationRequested) {
        return;
      }
      if (directoryPath == null || !FileSystem.DirectoryExists(directoryPath)) {
        return;
      }
      var files = FileSystem.EnumerateFiles(directoryPath).ToList();
      var fileBytes = files.Aggregate(0L, (s, x) => s + FileSystem.GetFileSize(x));
      game.SizeOnDisk.Value += fileBytes;
      game.FileCount.Value += files.Count;
      foreach (var childDirectory in FileSystem.EnumerateDirectories(directoryPath)) {
        DiscoverGameSizeOnDiskRecursive(game, childDirectory, cancellationToken);
      }
    }

    [NotNull]
    private static IEnumerable<FullPath> LoadGameDirectories([NotNull] FullPath steamLocation) {
      return FileSystem.EnumerateDirectories(steamLocation.Combine("steamapps").Combine("common"));
    }

    [NotNull]
    private static IEnumerable<AcfFile> LoadAcfFiles([NotNull] FullPath steamLocation) {
      return FileSystem.EnumerateFiles(steamLocation.Combine("steamapps"), "*.acf")
        .Select(x => new AcfFile(x, FileSystem.ReadAllText(x)));
    }

    [NotNull]
    private static IEnumerable<AcfFile> LoadWorkshopFiles([NotNull] FullPath steamLocation) {
      return FileSystem.EnumerateFiles(steamLocation.Combine("steamapps").Combine("workshop"), "*.acf")
        .Select(x => new AcfFile(x, FileSystem.ReadAllText(x)));
    }

    [NotNull]
    private static Task<T> RunAsync<T>([NotNull] Func<T> func) {
      return Task.Run(() => func.Invoke());
    }

    [NotNull]
    private static Task RunAsync([NotNull] Action func) {
      return Task.Run(func);
    }

    [CanBeNull]
    private FullPath LocateSteamFolder() {
      var result = LocateSteamUsingProcess();
      if (result == null) {
        result = LocateSteamUsingRegistry();
      }
      return result;
    }

    [CanBeNull]
    private FullPath LocateSteamUsingProcess() {
      var steamProcesses = Process.GetProcessesByName("Steam");
      return steamProcesses.Select(GetSteamProcessFolder).FirstOrDefault(x => x != null);
    }

    [CanBeNull]
    private FullPath LocateSteamUsingRegistry() {
      try {
        var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
        if (key == null) {
          return null;
        }
        using (key) {
          var path = (string) key.GetValue("SteamPath");
          var installDir = new FullPath(path.Replace("/", "\\"));
          return FileSystem.DirectoryExists(installDir) ? installDir : null;
        }
      }
      catch (Exception) {
        return null;
      }
    }

    [CanBeNull]
    private FullPath GetSteamProcessFolder(Process process) {
      var processPath = new FullPath(process.MainModule.FileName);
      if (!FileSystem.FileExists(processPath)) {
        return null;
      }

      var dir = processPath.Parent;
      if (dir == null || !FileSystem.DirectoryExists(dir)) {
        return null;
      }

      if (!FileSystem.DirectoryExists(dir.Combine("steamapps"))) {
        return null;
      }

      return dir;
    }

    [CanBeNull]
    public static string GetProperty([NotNull] string contents, [NotNull] string propName) {
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

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetDiskFreeSpaceEx([NotNull] string lpDirectoryName,
      out ulong lpFreeBytesAvailable,
      out ulong lpTotalNumberOfBytes,
      out ulong lpTotalNumberOfFreeBytes);
  }
}