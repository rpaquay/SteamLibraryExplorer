using mtsuite.CoreFileSystem.Utils;

namespace mtsuite.CoreFileSystem.Win32 {
  /// <summary>
  /// Provide efficient/copy-free text representation of <see cref="IStringSource{T}"/> objects.
  /// </summary>
  public interface IStringSourceFormatter<in T> where T: IStringSource {
    int GetLength(T source);
    void CopyTo(T source, StringBuffer destination);
  }
}