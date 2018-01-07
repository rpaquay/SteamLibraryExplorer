using System;
using mtsuite.CoreFileSystem.Utils;
using mtsuite.CoreFileSystem.Win32;

namespace mtsuite.CoreFileSystem {
  public static class PathSerializers {
    /// <summary>
    /// A <see cref="IPathSerializer{TSource}"/> that formats <see cref="FullPath"/>
    /// as their contents without any change.
    /// </summary>
    public class AsIsSerializer : PathSerializerBase<FullPath> {
      public override int GetLength(FullPath source) {
        return source.Length;
      }

      public override void CopyTo(FullPath source, StringBuffer destination) {
        source.CopyTo(destination);
      }
    }

    /// <summary>
    /// A <see cref="IPathSerializer{TSource}"/> that formats <see cref="FullPath"/>
    /// with the "long path" syntax when the path is longer than LONG_PATH_LIMIT (260).
    /// This ensure compatibility with FAT file systems, as the long path syntax is
    /// only used when needed.
    /// </summary>
    public class LongPathAsNeededSerializer : PathSerializerBase<FullPath> {
      private const int LongPathLimit = 248;

      public override int GetLength(FullPath source) {
        var sourceLength = source.Length;
        switch (source.PathKind) {

          case PathHelpers.RootPrefixKind.LongDiskPath: {
              var pathLen = sourceLength - PathHelpers.LongDiskPathPrefix.Length;
              return pathLen >= LongPathLimit ? sourceLength : pathLen;
            }

          case PathHelpers.RootPrefixKind.LongUncPath: {
              var pathLen = sourceLength - PathHelpers.LongUncPathPrefix.Length + 2;
              return pathLen >= LongPathLimit ? sourceLength : pathLen;
            }

          case PathHelpers.RootPrefixKind.DiskPath: {
              var pathLen = sourceLength;
              return pathLen >= LongPathLimit ? PathHelpers.LongDiskPathPrefix.Length + pathLen : pathLen;
            }

          case PathHelpers.RootPrefixKind.UncPath: {
              // UNC long path are special. We must transform "\\server\..." into "\\?\UNC\server\..."
              // where LongUncPathPrefix is ""\\?\UNC\".
              // So, we remove the leading 2 characters from the UNC path.
              var pathLen = sourceLength;
              return pathLen >= LongPathLimit ? PathHelpers.LongUncPathPrefix.Length + pathLen - 2 : pathLen;
            }

          default:
            throw new ArgumentOutOfRangeException();
        }
      }

      public override void CopyTo(FullPath source, StringBuffer destination) {
        switch (source.PathKind) {

          case PathHelpers.RootPrefixKind.LongDiskPath: {
              var position = destination.Length;
              source.CopyTo(destination);
              var length = destination.Length - position;
              if (length < LongPathLimit) {
                destination.DeleteAt(position, PathHelpers.LongDiskPathPrefix.Length);
              }
            }
            break;

          case PathHelpers.RootPrefixKind.LongUncPath: {
              var position = destination.Length;
              source.CopyTo(destination);
              var length = destination.Length - position;
              if (length < LongPathLimit) {
                // UNC long path are special. We must transform "\\?\UNC\server\..." into "\\server\...", 
                // where LongUncPathPrefix is "\\?\UNC\".
                // So, we remove the leading 2 characters from the UNC path.
                destination.DeleteAt(position, PathHelpers.LongUncPathPrefix.Length - 2);
                destination.Data[position] = '\\';
              }
            }
            break;

          case PathHelpers.RootPrefixKind.DiskPath: {
              var position = destination.Length;
              source.CopyTo(destination);
              var length = destination.Length - position;
              if (length >= LongPathLimit) {
                destination.InsertAt(position, PathHelpers.LongDiskPathPrefix);
              }
            }
            break;

          case PathHelpers.RootPrefixKind.UncPath: {
              var position = destination.Length;
              source.CopyTo(destination);
              var length = destination.Length - position;
              if (length >= LongPathLimit) {
                destination.InsertAt(position, PathHelpers.LongUncPathPrefix.Substring(0, PathHelpers.LongUncPathPrefix.Length - 2));
                destination.Data[position + 6] = 'C';
              }
            }
            break;

          default:
            throw new ArgumentOutOfRangeException();
        }
      }
    }
    /// <summary>
    /// A <see cref="IPathSerializer{TSource}"/> that unconditionally formats
    /// <see cref="FullPath"/> with the "long path" syntax.
    /// </summary>
    public class AlwaysLongPathSerializer : PathSerializerBase<FullPath> {
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

    /// <summary>
    /// A <see cref="IPathSerializer{TSource}"/> that unconditionally formats
    /// <see cref="FullPath"/> with the "long path" syntax.
    /// </summary>
    public class NeverLongPathSerializer : PathSerializerBase<FullPath> {
      public override int GetLength(FullPath source) {
        switch (source.PathKind) {
          case PathHelpers.RootPrefixKind.LongDiskPath:
            return source.Length - PathHelpers.LongDiskPathPrefix.Length;

          case PathHelpers.RootPrefixKind.LongUncPath:
            // UNC long path are special. We must transform "\\server\..." into "\\?\UNC\server\..."
            // where LongUncPathPrefix is "\\?\UNC\".
            // So, we remove the leading 2 characters from the UNC path.
            return source.Length - PathHelpers.LongUncPathPrefix.Length + 2;

          case PathHelpers.RootPrefixKind.DiskPath:
          case PathHelpers.RootPrefixKind.UncPath:
            return source.Length;

          default:
            throw new ArgumentOutOfRangeException();
        }
      }

      public override void CopyTo(FullPath source, StringBuffer destination) {
        switch (source.PathKind) {
          case PathHelpers.RootPrefixKind.LongDiskPath: {
              var position = destination.Length;
              source.CopyTo(destination);
              destination.DeleteAt(position, PathHelpers.LongDiskPathPrefix.Length);
            }
            break;

          case PathHelpers.RootPrefixKind.LongUncPath: {
              var position = destination.Length;
              source.CopyTo(destination);
              // UNC long path are special. We must transform "\\server\..." into "\\?\UNC\server\..."
              // where LongUncPathPrefix is "\\?\UNC\".
              // So, we remove the leading 2 characters from the UNC path.
              destination.DeleteAt(position, PathHelpers.LongUncPathPrefix.Length - 2);
              destination.Data[position] = '\\';
            }
            break;

          case PathHelpers.RootPrefixKind.DiskPath:
          case PathHelpers.RootPrefixKind.UncPath:
            source.CopyTo(destination);
            break;
          default:
            throw new ArgumentOutOfRangeException();
        }
      }
    }

  }
}
