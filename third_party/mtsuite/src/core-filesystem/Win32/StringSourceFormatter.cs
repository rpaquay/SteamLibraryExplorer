using mtsuite.CoreFileSystem.Utils;

namespace mtsuite.CoreFileSystem.Win32 {
  /// <summary>
  /// Base implementation class utilty for <see cref="IStringSourceFormatter{T}"/>
  /// </summary>
  public abstract class StringSourceFormatter<T> : IStringSourceFormatter<T> where T : IStringSource {
    public abstract int GetLength(T source);
    public abstract void CopyTo(T source, StringBuffer destination);

    public string GetText(T source) {
      var sb = new StringBuffer();
      CopyTo(source, sb);
      return sb.ToString();
    }
  }
}