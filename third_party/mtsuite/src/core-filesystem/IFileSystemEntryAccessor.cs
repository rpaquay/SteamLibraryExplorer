namespace mtsuite.CoreFileSystem {
  public interface IFileSystemEntryAccessor {
    string FileName { get; }
    FileSystemEntryData Data { get; }
  }
}