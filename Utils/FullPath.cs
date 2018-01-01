using System;
using System.IO;
using JetBrains.Annotations;

namespace SteamLibraryExplorer.Utils {
  public class FullPath : IComparable<FullPath>, IEquatable<FullPath> {
    /// <summary>
    /// Initialize a <see cref="FullPath"/> instance with a non-empty absolute path.
    /// </summary>
    public FullPath([NotNull]string fullName) {
      if (string.IsNullOrEmpty(fullName)) {
        throw new ArgumentException(@"Full path is empty", nameof(fullName));
      }
      FullName = new FileInfo(fullName).FullName;
    }

    [NotNull]
    public string FullName { get; }

    [NotNull]
    public string Name => Path.GetFileName(FullName);

    /// <summary>
    /// Return the parent path or <code>null</code> if the path is a root path.
    /// </summary>
    [CanBeNull]
    public FullPath Parent {
      get {
        var parentDir = Path.GetDirectoryName(FullName);
        return parentDir == null ? null : new FullPath(parentDir);
      }
    }

    [NotNull]
    public FullPath Combine([NotNull]string name) {
      return new FullPath(Path.Combine(FullName, name));
    }

    public int CompareTo([CanBeNull]FullPath other) {
      if (ReferenceEquals(other, null)) {
        return 1;
      }
      return StringComparer.OrdinalIgnoreCase.Compare(FullName, other.FullName);
    }

    public bool Equals(FullPath other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      return StringComparer.OrdinalIgnoreCase.Equals(FullName, other.FullName);
    }

    public override int GetHashCode() {
      return StringComparer.OrdinalIgnoreCase.GetHashCode(FullName);
    }

    public override bool Equals(object obj) {
      if (obj is FullPath) {
        return Equals((FullPath) obj);
      }
      return false;
    }

    public override string ToString() {
      return FullName;
    }
  }
}
