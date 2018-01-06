using System;
using mtsuite.CoreFileSystem.Utils;
using mtsuite.CoreFileSystem.Win32;

namespace mtsuite.CoreFileSystem {
  public class FullPathStringSourceFormatter : StringSourceFormatter<FullPath> {
    public override int GetLengthImpl(FullPath source) {
      return source.Length;
    }

    public override void CopyToImpl(FullPath source, StringBuffer destination) {
      source.CopyTo(destination);
    }
  }

  public class EnsureLongPathStringSourceFormatter : StringSourceFormatter<FullPath> {
    public override int GetLengthImpl(FullPath source) {
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

    public override void CopyToImpl(FullPath source, StringBuffer destination) {
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
