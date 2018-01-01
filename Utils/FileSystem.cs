namespace SteamLibraryExplorer.Utils {
  public abstract class FileSystem {
    public static readonly FileSystem Instance = new FileSystemImpl();

    public static void CreateDirectory(FullPath path) {
      Instance.CreateDirectoryImpl(path);
    }

    public static void DeleteDirectory(FullPath path) {
      Instance.DeleteDirectoryImpl(path);
    }

    public static void DeleteFile(FullPath path) {
      Instance.DeleteFileImpl(path);
    }

    public static bool DirectoryExists(FullPath fullPath) {
      return Instance.DirectoryExistsImpl(fullPath);
    }

    public static bool FileExists(FullPath fullPath) {
      return Instance.FileExistsImpl(fullPath);
    }

    protected abstract bool DirectoryExistsImpl(FullPath fullPath);
    protected abstract bool FileExistsImpl(FullPath fullPath);
    protected abstract void CreateDirectoryImpl(FullPath path);
    protected abstract void DeleteDirectoryImpl(FullPath path);
    protected abstract void DeleteFileImpl(FullPath path);

  }
}