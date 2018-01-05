using System.Collections.Generic;
using JetBrains.Annotations;
using mtsuite.CoreFileSystem;

namespace SteamLibraryExplorer.Utils {
  public abstract class FileSystem {
    [NotNull]
    public static readonly FileSystem Instance = new FileSystemImpl();

    public static void CreateDirectory([NotNull]FullPath path) {
      Instance.CreateDirectoryImpl(path);
    }

    public static void DeleteDirectory([NotNull]FullPath path) {
      Instance.DeleteDirectoryImpl(path);
    }

    public static void DeleteFile([NotNull]FullPath path) {
      Instance.DeleteFileImpl(path);
    }

    public static bool DirectoryExists([NotNull]FullPath fullPath) {
      return Instance.DirectoryExistsImpl(fullPath);
    }

    public static bool FileExists([NotNull]FullPath fullPath) {
      return Instance.FileExistsImpl(fullPath);
    }

    public static long GetFileSize([NotNull]FullPath fullPath) {
      return Instance.GetFileSizeImpl(fullPath);
    }

    public static string ReadAllText([NotNull] FullPath path) {
      return Instance.ReadAllTextImpl(path);
    }

    protected abstract string ReadAllTextImpl(FullPath path);

    [NotNull]
    public static IEnumerable<FullPath> EnumerateFiles([NotNull]FullPath path) {
      return Instance.EnumerateFilesImpl(path);
    }

    [NotNull]
    public static IEnumerable<FullPath> EnumerateDirectories([NotNull]FullPath path) {
      return Instance.EnumerateDirectoriesImpl(path);
    }

    [NotNull]
    public static IEnumerable<FullPath> EnumerateEntries([NotNull]FullPath path) {
      return Instance.EnumerateEntriesImpl(path);
    }

    protected abstract long GetFileSizeImpl(FullPath path);
    protected abstract bool DirectoryExistsImpl(FullPath path);
    protected abstract bool FileExistsImpl(FullPath path);
    protected abstract void CreateDirectoryImpl(FullPath path);
    protected abstract void DeleteDirectoryImpl(FullPath path);
    protected abstract void DeleteFileImpl(FullPath path);
    protected abstract IEnumerable<FullPath> EnumerateFilesImpl(FullPath path);
    protected abstract IEnumerable<FullPath> EnumerateDirectoriesImpl(FullPath path);
    protected abstract IEnumerable<FullPath> EnumerateEntriesImpl(FullPath path);
  }
}