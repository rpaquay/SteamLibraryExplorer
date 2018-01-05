using mtsuite.CoreFileSystem;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using mtsuite.CoreFileSystem.ObjectPool;

namespace SteamLibraryExplorer.Utils {
  public class FileSystemImpl : FileSystem {
    private readonly IFileSystem _coreFileSystem = new mtsuite.CoreFileSystem.FileSystem();

    protected override string ReadAllTextImpl(FullPath path) {
      using (var fileSteam = _coreFileSystem.OpenFile(path, FileAccess.Read)) {
        using (var streamReader = new StreamReader(fileSteam, Encoding.UTF8, true, 4096, false))
          return streamReader.ReadToEnd();
      }
    }

    protected override long GetFileSizeImpl(FullPath path) {
      return _coreFileSystem.GetEntry(path).FileSize;
    }

    protected override bool DirectoryExistsImpl(FullPath path) {
      FileSystemEntry entry;
      if (!_coreFileSystem.TryGetEntry(path, out entry)) {
        return false;
      }
      return entry.IsDirectory;
    }

    protected override bool FileExistsImpl(FullPath path) {
      FileSystemEntry entry;
      if (!_coreFileSystem.TryGetEntry(path, out entry)) {
        return false;
      }
      return entry.IsFile;
    }

    protected override void CreateDirectoryImpl(FullPath path) {
      _coreFileSystem.CreateDirectory(path);
    }

    protected override void DeleteDirectoryImpl(FullPath path) {
      _coreFileSystem.DeleteEntry(_coreFileSystem.GetEntry(path));
    }

    protected override void DeleteFileImpl(FullPath path) {
      _coreFileSystem.DeleteEntry(_coreFileSystem.GetEntry(path));
    }

    protected override IEnumerable<FileSystemEntry> EnumerateFilesImpl(FullPath path) {
      using (var entries = _coreFileSystem.GetDirectoryEntries(path)) {
        return entries.Item.Where(x => x.IsFile).ToList();
      }
    }

    protected override IEnumerable<FileSystemEntry> EnumerateDirectoriesImpl(FullPath path) {
      using (var entries = _coreFileSystem.GetDirectoryEntries(path)) {
        return entries.Item.Where(x => x.IsDirectory).ToList();
      }
    }

    protected override IEnumerable<FileSystemEntry> EnumerateEntriesImpl(FullPath path) {
      using (var entries = _coreFileSystem.GetDirectoryEntries(path)) {
        return entries.Item.ToList();
      }
    }
  }
}