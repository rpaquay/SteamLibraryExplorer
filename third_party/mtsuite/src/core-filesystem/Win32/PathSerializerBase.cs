using mtsuite.CoreFileSystem.Utils;

namespace mtsuite.CoreFileSystem.Win32 {
  /// <summary>
  /// Base implementation class utilty for <see cref="IPathSerializer{T}"/>
  /// </summary>
  public abstract class PathSerializerBase<TPath> : IPathSerializer<TPath> {
    public abstract int GetLength(TPath source);
    public abstract void CopyTo(TPath source, StringBuffer destination);

    public string GetText(TPath source) {
      var sb = new StringBuffer(256);
      CopyTo(source, sb);
      return sb.ToString();
    }
  }
}