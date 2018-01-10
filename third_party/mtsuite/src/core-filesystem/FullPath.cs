// Copyright 2015 Renaud Paquay All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using mtsuite.CoreFileSystem.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace mtsuite.CoreFileSystem {
  /// <summary>
  /// Represents a fully qualified path.
  /// </summary>
  public struct FullPath : IEquatable<FullPath>, IComparable<FullPath> {
    /// <summary>
    /// If there is a parent path, <see cref="_parent"/> the boxed instance of the parent <see cref="FullPath"/>.
    /// If there is no parent path, <see cref="_parent"/> is <code>null</code>, and <see cref="_name"/> is a root path.
    /// </summary>
    private readonly object _parent;

    /// <summary>
    /// The "name" part (i.e file name or directory name) of the path, which may be the root path name (e.g. "C:\").
    /// </summary>
    private readonly string _name;

    /// <summary>
    /// Construct a <see cref="FullPath"/> instance from a valid fully qualifed path
    /// represented as the <see cref="string"/> <paramref name="path"/>.
    /// Throws an exception if the <paramref name="path"/> is not valid.
    /// </summary>
    public FullPath(string path) {
      if (!PathHelpers.IsPathAbsolute(path))
        ThrowArgumentException("Path should be absolute", "path");
      if (PathHelpers.HasAltDirectorySeparators(path))
        ThrowArgumentException("Path should only contain valid directory separators", "path");
      _parent = CreatePath(PathHelpers.GetParent(path));
      _name = _parent == null ? path : PathHelpers.GetName(path);
    }

    /// <summary>
    /// Construct a <see cref="FullPath"/> instance from a valid parent <see cref="FullPath"/>
    /// and a relative name.
    /// Throws an exception if the <paramref name="name"/> is not valid.
    /// </summary>
    public FullPath(FullPath parent, string name) {
      if (parent._name == null)
        ThrowArgumentNullException("parent");
      if (string.IsNullOrEmpty(name))
        ThrowArgumentNullException("name");
      if (PathHelpers.HasAltDirectorySeparators(name) || PathHelpers.HasDirectorySeparators(name))
        ThrowArgumentException("Name should not contain directory separators", "name");
      _parent = parent;
      _name = name;
    }

    private static FullPath? CreatePath(string path) {
      if (path == null) {
        return null;
      }
      var parentPath = PathHelpers.GetParent(path);
      if (parentPath == null) {
        return new FullPath(path);
      }

      var name = PathHelpers.GetName(path);
      return new FullPath(parentPath).Combine(name);
    }

    private static void ThrowArgumentNullException(string paramName) {
      throw new ArgumentNullException(paramName);
    }

    private static void ThrowArgumentException(string message, string paramName) {
      throw new ArgumentException(message, paramName);
    }

    public string FullName {
      get {
        var sb = new StringBuffer(256);
        BuildPath(sb);
        return sb.ToString();
      }
    }

    public string Name {
      get {
        return _name;
      }
    }

    public FullPath? Parent {
      get {
        if (_parent == null)
          return null;
        return (FullPath)_parent;
      }
    }

    public FullPath Combine(string name) {
      if (string.IsNullOrEmpty(name)) {
        ThrowArgumentNullException("name");
      }

      if (!PathHelpers.HasDirectorySeparators(name)) {
        return new FullPath(this, name);
      }
      var current = this;
      foreach (var segment in SplitRelativePath(name)) {
        current = new FullPath(current, segment);
      }
      return current;
    }

    private static IEnumerable<string> SplitRelativePath(string name) {
      var index = 0;
      while (index < name.Length) {
        int nextSep = name.IndexOf(Path.DirectorySeparatorChar, index);
        if (nextSep < 0) {
          yield return name.Substring(index);
          index = name.Length;
        } else {
          yield return name.Substring(index, nextSep - index);
          index = nextSep + 1;
        }
      }
    }

    public bool HasTrailingSeparator {
      get { return _name[_name.Length - 1] == Path.DirectorySeparatorChar; }
    }

    public PathHelpers.RootPrefixKind PathKind {
      get {
        if (_parent != null) {
          return ((FullPath)_parent).PathKind;
        }

        return PathHelpers.GetPathRootPrefixInfo(_name).RootPrefixKind;
      }
    }

    public enum LongPathPrefixKind {
      None,
      LongDiskPath,
      LongUncPath,
    }

    private void BuildPath(StringBuffer sb) {
      if (_parent != null) {
        ((FullPath)_parent).BuildPath(sb);
        if (!((FullPath)_parent).HasTrailingSeparator)
          sb.Append(PathHelpers.DirectorySeparatorString);
      }
      sb.Append(_name);
    }

    public override string ToString() {
      return FullName;
    }

    public int Length {
      get { return GetLength(this); }
    }

    public void CopyTo(StringBuffer sb) {
      BuildPath(sb);
    }

    private static int GetLength(FullPath path) {
      var result = path._name.Length;
      while (path._parent != null) {
        path = (FullPath)path._parent;
        result += path._name.Length;
        if (!path.HasTrailingSeparator)
          result++;
      }
      return result;
    }

    public bool Equals(FullPath other) {
      return Equals(_parent, other._parent) &&
             string.Equals(_name, other._name, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object obj) {
      if (obj is FullPath) {
        return Equals((FullPath) obj);
      }

      return false;
    }

    public override int GetHashCode() {
      unchecked {
        return ((_parent != null ? _parent.GetHashCode() : 0) * 397) ^
               StringComparer.OrdinalIgnoreCase.GetHashCode(_name);
      }
    }

    public int CompareTo(FullPath other) {
      if (_parent == null) {
        if (other._parent == null) {
          return string.Compare(_name, other._name, StringComparison.OrdinalIgnoreCase);
        } else {
          return -1;
        }
      } else {
        if (other._parent == null) {
          return 1;
        } else {
          var parentComparison = Comparer<FullPath>.Default.Compare((FullPath)_parent, (FullPath)other._parent);
          if (parentComparison != 0) return parentComparison;
          return string.Compare(_name, other._name, StringComparison.OrdinalIgnoreCase);
        }
      }
    }
  }
}