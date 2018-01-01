  using System.Collections.Generic;
  using System.IO;
  using System.Linq;

namespace SteamLibraryExplorer.Utils {
  public class FileSystemImpl : FileSystem {
    protected override string ReadAllTextImpl(FullPath path) {
      return File.ReadAllText(path.FullName);
    }

    protected override long GetFileSizeImpl(FullPath fullPath) {
      return new FileInfo(fullPath.FullName).Length;
    }

    protected override bool DirectoryExistsImpl(FullPath fullPath) {
      return Directory.Exists(fullPath.FullName);
    }

    protected override bool FileExistsImpl(FullPath fullPath) {
      return File.Exists(fullPath.FullName);
    }

    protected override void CreateDirectoryImpl(FullPath path) {
      Directory.CreateDirectory(path.FullName);
    }

    protected override void DeleteDirectoryImpl(FullPath path) {
      Directory.Delete(path.FullName);
    }

    protected override void DeleteFileImpl(FullPath path) {
      File.Delete(path.FullName);
    }

    protected override IEnumerable<FullPath> EnumerateFilesImpl(FullPath path) {
      return Directory.EnumerateFiles(path.FullName).Select(x => path.Combine(x));
    }

    protected override IEnumerable<FullPath> EnumerateFilesImpl(FullPath path, string pattern) {
      return Directory.EnumerateFiles(path.FullName, pattern).Select(x => path.Combine(x));
    }

    protected override IEnumerable<FullPath> EnumerateDirectoriesImpl(FullPath path) {
      return Directory.EnumerateDirectories(path.FullName).Select(x => path.Combine(x));
    }

    protected override IEnumerable<FullPath> EnumerateEntriesImpl(FullPath path) {
      return Directory.EnumerateFileSystemEntries(path.FullName).Select(x => path.Combine(x));
    }
  }
}