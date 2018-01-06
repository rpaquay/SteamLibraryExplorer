using System;
using mtsuite.CoreFileSystem.Utils;
using mtsuite.CoreFileSystem.Win32;

namespace mtsuite.CoreFileSystem {
  public static class StringSourceFormatters {
    /// <summary>
    /// A <see cref="IStringSourceFormatter{FullPath}"/> that formats <see cref="FullPath"/>
    /// as their contents without any change.
    /// </summary>
    public class AsIsFormatter : StringSourceFormatter<FullPath> {
      public override int GetLength(FullPath source) {
        return source.Length;
      }

      public override void CopyTo(FullPath source, StringBuffer destination) {
        source.CopyTo(destination);
      }
    }

    /// <summary>
    /// A <see cref="IStringSourceFormatter{FullPath}"/> that unconditionally formats
    /// <see cref="FullPath"/> with the "long path" syntax.
    /// </summary>
    public class AlwaysLongPathFormatter : StringSourceFormatter<FullPath> {
      public override int GetLength(FullPath source) {
        switch (source.PathKind) {
          case PathHelpers.RootPrefixKind.LongDiskPath:
          case PathHelpers.RootPrefixKind.LongUncPath:
            return source.Length;
          case PathHelpers.RootPrefixKind.DiskPath:
            return PathHelpers.LongDiskPathPrefix.Length + source.Length;
          case PathHelpers.RootPrefixKind.UncPath:
            // UNC long path are special. We must transform "\\server\..." into "\\?\UNC\server\..."
            // where LongUncPathPrefix is ""\\?\UNC\".
            // So, we remove the leading 2 characters from the UNC path.
            return PathHelpers.LongUncPathPrefix.Length + source.Length - 2;
          default:
            throw new ArgumentOutOfRangeException();
        }
      }

      public override void CopyTo(FullPath source, StringBuffer destination) {
        switch (source.PathKind) {
          case PathHelpers.RootPrefixKind.LongDiskPath:
          case PathHelpers.RootPrefixKind.LongUncPath:
            source.CopyTo(destination);
            break;
          case PathHelpers.RootPrefixKind.DiskPath:
            destination.Append(PathHelpers.LongDiskPathPrefix);
            source.CopyTo(destination);
            break;
          case PathHelpers.RootPrefixKind.UncPath:
            // UNC long path are special. We must transform "\\server\..." into "\\?\UNC\server\..."
            // where LongUncPathPrefix is "\\?\UNC\".
            // So, we remove the leading 2 characters from the UNC path.
            destination.Append(@"\\?\UN");
            source.CopyTo(destination);
            destination.Data[6] = 'C';
            destination.Data[7] = '\\';
            break;
          default:
            throw new ArgumentOutOfRangeException();
        }
      }
    }
  }
}
