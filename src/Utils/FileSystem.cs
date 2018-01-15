using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using mtsuite.CoreFileSystem;
using mtsuite.CoreFileSystem.Win32;

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

    [NotNull]
    public static FileSystemEntry GetEntry([NotNull]FullPath path) {
      return Instance.GetEntryImpl(path);
    }

    public static void CopyFile([NotNull]FileSystemEntry sourceEntry, [NotNull]FullPath destinationPath, CopyFileOptions options, [NotNull]CopyFileCallback callback) {
      Instance.CopyFileImpl(sourceEntry, destinationPath, options, callback);
    }

    [NotNull]
    public static IEnumerable<FileSystemEntry> EnumerateFiles([NotNull]FullPath path, string pattern = null) {
      return Instance.EnumerateEntriesImpl(path, pattern).Where(x => x.IsFile);
    }

    [NotNull]
    public static IEnumerable<FileSystemEntry> EnumerateDirectories([NotNull]FullPath path, string pattern = null) {
      return Instance.EnumerateEntriesImpl(path, pattern).Where(x => x.IsDirectory);
    }

    [NotNull]
    public static IEnumerable<FileSystemEntry> EnumerateEntries([NotNull]FullPath path, string pattern = null) {
      return Instance.EnumerateEntriesImpl(path, pattern);
    }

    [NotNull]
    public static DirectoryEntriesEnumerator<FullPath> GetDirectoryEntriesEnumerator([NotNull]FullPath path, string pattern) {
      return Instance.GetDirectoryEntriesEnumeratorImpl(path, pattern);
    }

    protected abstract FileSystemEntry GetEntryImpl(FullPath path);
    protected abstract long GetFileSizeImpl(FullPath path);
    protected abstract string ReadAllTextImpl([NotNull]FullPath path);
    protected abstract bool DirectoryExistsImpl(FullPath path);
    protected abstract bool FileExistsImpl(FullPath path);
    protected abstract void CreateDirectoryImpl(FullPath path);
    protected abstract void DeleteDirectoryImpl(FullPath path);
    protected abstract void DeleteFileImpl(FullPath path);
    protected abstract void CopyFileImpl(FileSystemEntry sourceEntry, [NotNull]FullPath destinationPath, CopyFileOptions options, [NotNull]CopyFileCallback callback);
    protected abstract IEnumerable<FileSystemEntry> EnumerateEntriesImpl([NotNull]FullPath path, string pattern);
    protected abstract DirectoryEntriesEnumerator<FullPath> GetDirectoryEntriesEnumeratorImpl(FullPath path, string pattern);
  }
}