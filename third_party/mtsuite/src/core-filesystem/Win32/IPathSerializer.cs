using mtsuite.CoreFileSystem.Utils;

namespace mtsuite.CoreFileSystem.Win32 {
  /// <summary>
  /// Provides efficient and memory allocation free serialization of <see cref="TPath"/> objects into UTF-16 buffers.
  /// </summary>
  public interface IPathSerializer<in TPath> {
    int GetLength(TPath source);
    void CopyTo(TPath source, StringBuffer destination);
  }
}