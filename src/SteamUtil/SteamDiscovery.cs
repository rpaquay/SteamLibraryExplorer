using JetBrains.Annotations;
using Microsoft.Win32;
using mtsuite.CoreFileSystem;
using SteamLibraryExplorer.SteamModel;
using SteamLibraryExplorer.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FileSystem = SteamLibraryExplorer.Utils.FileSystem;

namespace SteamLibraryExplorer.SteamUtil {
  public class SteamDiscovery : ISteamDiscovery {
    private static readonly ILoggerFacade Logger = LoggerManagerFacade.GetLogger(typeof(SteamDiscovery));
    private readonly Dictionary<FullPath, PathInfo> _pathInfos = new Dictionary<FullPath, PathInfo>();
    private readonly object _pathInfosLock = new object();

    private class PathInfo {
      public long FileCount { get; set; }
      public long SizeOnDisk { get; set; }

      public override string ToString() {
        return $"[FileCount={FileCount:n0}, SizeOnDisk={SizeOnDisk:n0}]";
      }
    }

    [NotNull]
    public Task<FullPath?> LocateSteamFolderAsync() {
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
    public Task DiscoverSizeOnDiskAsync([NotNull] IEnumerable<SteamLibrary> libraries, bool useCache, CancellationToken cancellationToken) {
      // Copy collection, since it could be cleared if a Refresh occurs.
      var copy = libraries.ToList();
      return RunAsync(() => DiscoverSizeOnDisk(copy, useCache, cancellationToken));
    }

    [NotNull]
    public Task<bool> RestartSteamAsync([NotNull] FullPath steamExePath) {
      return RunAsync(() => RestartSteam(steamExePath));
    }

    [NotNull]
    public Task<Process> FindSteamProcessAsync() {
      return RunAsync(FindSteamProcess);
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

    private void DiscoverSizeOnDisk([NotNull] IEnumerable<SteamLibrary> libraries, bool useCache, CancellationToken cancellationToken) {
      // Clear cache if not using it
      if (!useCache) {
        lock (_pathInfosLock) {
          _pathInfos.Clear();
        }
      }

      foreach (var library in libraries) {
        if (cancellationToken.IsCancellationRequested)
          break;

        ulong userFreeBytes;
        ulong totalBytes;
        ulong freeBytes;
        if (NativeMethods.GetDiskFreeSpaceEx(library.Location.FullName, out userFreeBytes, out totalBytes, out freeBytes)) {
          library.FreeDiskSize.Value = (long)freeBytes;
          library.TotalDiskSize.Value = (long)totalBytes;
        }

        foreach (var game in library.Games) {
          DiscoverGameSizeOnDisk(game, useCache, cancellationToken);
        }
      }
    }

    public static class ProperyValueThrottledUpdater {
      public static ProperyValueThrottledUpdater<T> Create<T>(PropertyValue<T> property) {
        return new ProperyValueThrottledUpdater<T>(property);
      }
    }

    public class ProperyValueThrottledUpdater<T> : IDisposable {
      private readonly PropertyValue<T> _property;
      private readonly Stopwatch _stopwatch = new Stopwatch();
      private T _value;

      public ProperyValueThrottledUpdater(PropertyValue<T> property) {
        _property = property;
        _stopwatch.Start();
        Value = property.Value;
      }

      public T Value {
        get { return _value; }
        set {
          _value = value;
          if (_stopwatch.ElapsedMilliseconds >= 10) {
            _stopwatch.Reset();
            _property.Value = _value;
          }
        }
      }

      public void Dispose() {
        _property.Value = _value;
      }
    }

    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    private void DiscoverGameSizeOnDisk([NotNull] SteamGame game, bool useCache, CancellationToken cancellationToken) {
      using (var gameFileCount = ProperyValueThrottledUpdater.Create(game.FileCount))
      using (var gameSizeOnDisk = ProperyValueThrottledUpdater.Create(game.SizeOnDisk))
      using (var gameWorkshopFileCount = ProperyValueThrottledUpdater.Create(game.WorkshopFileCount))
      using (var gameWorkshopSizeOnDisk = ProperyValueThrottledUpdater.Create(game.WorkshopSizeOnDisk)) {
        DiscoverGameSizeOnDiskRecursive(game.Location, useCache, (fileCount, sizeOnDisk) => {
          gameFileCount.Value += fileCount;
          gameSizeOnDisk.Value += sizeOnDisk;
        }, info => {
          info.FileCount = gameFileCount.Value;
          info.SizeOnDisk = gameSizeOnDisk.Value;
        }, cancellationToken);

        DiscoverGameSizeOnDiskRecursive(game.WorkshopLocation, useCache, (fileCount, sizeOnDisk) => {
          gameFileCount.Value += fileCount;
          gameSizeOnDisk.Value += sizeOnDisk;
          gameWorkshopFileCount.Value += fileCount;
          gameWorkshopSizeOnDisk.Value += sizeOnDisk;
        }, info => {
          info.FileCount = gameWorkshopFileCount.Value;
          info.SizeOnDisk = gameWorkshopSizeOnDisk.Value;
        }, cancellationToken);
      }
    }

    private void DiscoverGameSizeOnDiskRecursive([CanBeNull] FullPath? directoryPath, bool useCache,
      [NotNull] Action<long, long> updateValues, [NotNull] Action<PathInfo> saveValues, CancellationToken cancellationToken) {
      if (cancellationToken.IsCancellationRequested) {
        return;
      }
      if (directoryPath == null || !FileSystem.DirectoryExists(directoryPath.Value)) {
        return;
      }

      // Try cache lookup first
      if (useCache) {
        lock (_pathInfosLock) {
          PathInfo pathInfo;
          if (_pathInfos.TryGetValue(directoryPath.Value, out pathInfo)) {
            updateValues(pathInfo.FileCount, pathInfo.SizeOnDisk);
            return;
          }
        }
      }

      DiscoverGameSizeOnDiskRecursiveImpl(directoryPath.Value, updateValues, cancellationToken);

      // Store in cache (only if operation was completed!)
      if (useCache) {
        if (!cancellationToken.IsCancellationRequested) {
          lock (_pathInfosLock) {
            var pathInfo = new PathInfo();
            saveValues(pathInfo);
            _pathInfos[directoryPath.Value] = pathInfo;
          }
        }
      }
    }

    private static void DiscoverGameSizeOnDiskRecursiveImpl([NotNull] FullPath directoryPath,
      [NotNull] Action<long, long> updateValues, CancellationToken cancellationToken) {
      if (cancellationToken.IsCancellationRequested) {
        return;
      }

      foreach (var entry in FileSystem.EnumerateEntriesWithFileName(directoryPath)) {
        if (entry.IsFile) {
          updateValues(1, entry.FileSize);
        } else if (entry.IsDirectory) { 
          DiscoverGameSizeOnDiskRecursiveImpl(directoryPath.Combine(entry.FileName), updateValues, cancellationToken);
        }
      }
    }

    [NotNull]
    private static IEnumerable<FullPath> LoadGameDirectories([NotNull] FullPath steamLocation) {
      var gameFilesDirectory = steamLocation.Combine("steamapps").Combine("common");
      if (FileSystem.DirectoryExists(gameFilesDirectory)) {
        return FileSystem.EnumerateDirectories(gameFilesDirectory).Select(x => x.Path);
      }
      return Enumerable.Empty<FullPath>();
    }

    [NotNull]
    private static IEnumerable<AcfFile> LoadAcfFiles([NotNull] FullPath steamLocation) {
      return LoadAcfFilesWorkder(steamLocation.Combine("steamapps"));
    }

    [NotNull]
    private static IEnumerable<AcfFile> LoadWorkshopFiles([NotNull] FullPath steamLocation) {
      return LoadAcfFilesWorkder(steamLocation.Combine("steamapps").Combine("workshop"));
    }

    [NotNull]
    private static IEnumerable<AcfFile> LoadAcfFilesWorkder([NotNull] FullPath acfFilesDirectory) {
      if (FileSystem.DirectoryExists(acfFilesDirectory)) {
        return FileSystem.EnumerateFiles(acfFilesDirectory, "*.acf")
          .Select(x => new AcfFile(x.Path, FileSystem.ReadAllText(x.Path)));
      }
      return Enumerable.Empty<AcfFile>();
    }

    [NotNull]
    private static Task<T> RunAsync<T>([NotNull] Func<T> func) {
      return TaskUtils.Run(() => func.Invoke());
    }

    [NotNull]
    private static Task RunAsync([NotNull] Action func) {
      return TaskUtils.Run(func);
    }

    [CanBeNull]
    private FullPath? LocateSteamFolder() {
      var result = LocateSteamUsingProcess();
      if (result == null) {
        result = LocateSteamUsingRegistry();
      }
      return result;
    }

    [CanBeNull]
    private FullPath? LocateSteamUsingProcess() {
      var steamProcesses = Process.GetProcessesByName("Steam");
      return steamProcesses.Select(GetSteamProcessFolder).FirstOrDefault(x => x != null);
    }

    [CanBeNull]
    private FullPath? LocateSteamUsingRegistry() {
      try {
        var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
        if (key == null) {
          return null;
        }
        using (key) {
          var path = (string)key.GetValue("SteamPath");
          var installDir = new FullPath(path.Replace("/", "\\"));
          if (FileSystem.DirectoryExists(installDir))
            return installDir;
          return null;
        }
      } catch (Exception) {
        return null;
      }
    }

    [CanBeNull]
    private FullPath? GetSteamProcessFolder(Process process) {
      var processPath = new FullPath(process.MainModule.FileName);
      if (!FileSystem.FileExists(processPath)) {
        return null;
      }

      var dir = processPath.Parent;
      if (dir == null || !FileSystem.DirectoryExists(dir.Value)) {
        return null;
      }

      if (!FileSystem.DirectoryExists(dir.Value.Combine("steamapps"))) {
        return null;
      }

      return dir;
    }

    private bool RestartSteam([NotNull] FullPath steamExePath) {
      var process = FindSteamProcess();

      // Stop process if still running
      if (process != null) {
        if (process.MainWindowHandle != IntPtr.Zero) {
          Logger.Info("Closing Steam using the main window handle");
          SendEndSessionMessageToWindow(process.MainWindowHandle);
        } else {
          foreach (var handle in EnumerateProcessWindowHandles(process)) {
            Logger.Info("Closing Steam using one of the process window handles");
            SendEndSessionMessageToWindow(handle);
            if (process.WaitForExit((int)TimeSpan.FromSeconds(0.1).TotalMilliseconds)) {
              break;
            }
          }

          if (!process.WaitForExit((int)TimeSpan.FromSeconds(10).TotalMilliseconds)) {
            Logger.Warn("Steam not restarted because it took more than 10 seconds to close");
            return false;
          }
        }

        // Wait for the steam helper processes to stop (10 seconds max).
        //
        // Note: We use a busy loop to avod calling into C#/Win32 API on process
        //       handles to avoid potential "Access Denied" accessing process handles.
        var endWait = DateTime.UtcNow + TimeSpan.FromSeconds(10);
        var allProcessesExited = false;
        while (DateTime.UtcNow < endWait) {
          var helperProcesses = FindSteamHelperProcesses();
          if (helperProcesses.Count == 0) {
            allProcessesExited = true;
            break;
          }
          Thread.Sleep(100);
        }
        if (!allProcessesExited) {
          Logger.Warn("One of the steam helper process took more than 10 seconds to close. Restarting Steam may fail.");
        }
      }

      // Re-start steam now
      try {
        var si = new ProcessStartInfo(steamExePath.FullName);
        si.WindowStyle = ProcessWindowStyle.Minimized;
        var newProcess = Process.Start(si);
        if (newProcess == null) {
          Logger.Info("There was an error restarting Steam");
        } else {
          Logger.Info("Steam has been successfully restarted");
        }
      } catch (Exception e) {
        Logger.Error(e, "Steam not restarted because Start process failed");
        return false;
      }
      return true;
    }

    private List<Process> FindSteamHelperProcesses() {
      return Process.GetProcessesByName("steamwebhelper").Concat(Process.GetProcessesByName("steamservice")).ToList();
    }

    private Process FindSteamProcess() {
      return Process.GetProcessesByName("Steam")
        .Where(IsSteamProcess)
        .FirstOrDefault(x => x != null);
    }


    private static void SendEndSessionMessageToWindow(IntPtr handle) {
      // From https://msdn.microsoft.com/en-us/library/windows/desktop/aa376890(v=vs.85).aspx:
      //
      // ""Applications should respect the user's intentions and return TRUE. By default, the DefWindowProc
      // function returns TRUE for this message.
      // If shutting down would corrupt the system or media that is being burned, the application can
      // return FALSE.However, it is good practice to respect the user's actions.""
      var result = NativeMethods.SendMessage(handle, NativeMethods.WM_QUERYENDSESSION,
        new IntPtr(0), new IntPtr(NativeMethods.ENDSESSION_CLOSEAPP));
      if (result != 0) {
        Logger.Info("Steam has accepted the WM_QUERYENDSESSION message");
      }
      result = NativeMethods.SendMessage(handle, NativeMethods.WM_ENDSESSION, new IntPtr(1),
        new IntPtr(NativeMethods.ENDSESSION_CLOSEAPP));
      if (result == 0) {
        Logger.Info("Steam has accepted closing the main window handle");
      }
    }

    private bool IsSteamProcess(Process process) {
      var processPath = new FullPath(process.MainModule.FileName);
      if (!FileSystem.FileExists(processPath)) {
        return false;
      }

      var dir = processPath.Parent;
      if (dir == null || !FileSystem.DirectoryExists(dir.Value)) {
        return false;
      }

      if (!FileSystem.DirectoryExists(dir.Value.Combine("steamapps"))) {
        return false;
      }

      return true;
    }

    private static readonly ConcurrentDictionary<string, Regex> RegexCache = new ConcurrentDictionary<string, Regex>();

    [CanBeNull]
    public static string GetProperty([NotNull] string contents, [NotNull] string propName) {
      var regex = RegexCache.GetOrAdd(propName, name => {
        return new Regex("^.*\"" + name + "\".*\"(?<propValue>.+)\"$", RegexOptions.IgnoreCase);
      });
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

    static IList<IntPtr> EnumerateProcessWindowHandles(Process process) {
      var handles = new List<IntPtr>();

      foreach (ProcessThread thread in process.Threads)
        NativeMethods.EnumThreadWindows(thread.Id,
          (hWnd, lParam) => {
            handles.Add(hWnd);
            return true;
          }, IntPtr.Zero);

      return handles;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static class NativeMethods {
      public const int WM_QUERYENDSESSION = 0x0011;
      public const int ENDSESSION_CLOSEAPP = 0x1;
      public const int WM_ENDSESSION = 0x0016;

      [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool GetDiskFreeSpaceEx([NotNull] string lpDirectoryName,
        out ulong lpFreeBytesAvailable,
        out ulong lpTotalNumberOfBytes,
        out ulong lpTotalNumberOfFreeBytes);

      [DllImport("user32.dll")]
      public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

      public delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

      [DllImport("user32.dll")]
      public static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);
    }
  }
}