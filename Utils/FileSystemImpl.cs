  using System.IO;

namespace SteamLibraryExplorer.Utils {
  public class FileSystemImpl : FileSystem {
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
  }
}