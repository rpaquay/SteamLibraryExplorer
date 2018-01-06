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
using mtsuite.CoreFileSystem.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace mtsuite.CoreFileSystem {
  /// <summary>
  /// Represents a fully qualified path.
  /// </summary>
  public struct FullPath : IEquatable<FullPath>, IComparable<FullPath>, IStringSource {
    private class FullPathValue {
      /// <summary>
      /// The parent path, or <code>null</code> <see cref="_name"/> is a root path
      /// </summary>
      private readonly object _parentValue;

      /// <summary>
      /// The "name" part (i.e file name or directory name) of the path, which may be the root path name (e.g. "C:").
      /// </summary>
      private readonly string _name;

      public FullPathValue(object parentValue, string name) {
        Debug.Assert(parentValue != null);
        Debug.Assert(name != null);
        _parentValue = parentValue;
        _name = name;
      }

      /// <summary>
      /// The parent path, or <code>null</code> <see cref="_name"/> is a root path
      /// </summary>
      public object ParentValue {
        get { return _parentValue; }
      }

      public string Name {
        get { return _name; }
      }

      public static int GetHashCode(object value) {
        var name = value as string;
        if (name != null) {
          return StringComparer.OrdinalIgnoreCase.GetHashCode(name);
        }

        unchecked {
          var pathValue = (FullPathValue) value;
          return (GetHashCode(pathValue._parentValue) * 397) ^ GetHashCode(pathValue._name);
        }
      }

      public static bool HasTrailingSeparator(object value) {
        var name = value as string;
        if (name != null) {
          return name[name.Length - 1] == Path.DirectorySeparatorChar;
        }

        var pathValue = (FullPathValue) value;
        return HasTrailingSeparator(pathValue._name);
      }

      public static PathHelpers.RootPrefixKind GetRootPrefixKind(object value) {
        var name = value as string;
        if (name != null) {
          return PathHelpers.GetPathRootPrefixInfo(name).RootPrefixKind;
        }

        var pathValue = (FullPathValue) value;
        return GetRootPrefixKind(pathValue._parentValue);
      }

      public static void BuildPath(object value, StringBuffer sb) {
        var name = value as string;
        if (name != null) {
          sb.Append(name);
          return;
        }

        var pathValue = (FullPathValue) value;
        BuildPath(pathValue._parentValue, sb);
        if (!HasTrailingSeparator(pathValue._parentValue)) {
          sb.Append(PathHelpers.DirectorySeparatorString);
        }

        sb.Append(pathValue._name);
      }

      public static int GetLength(object value) {
        var name = value as string;
        if (name != null) {
          return name.Length;
        }

        var pathValue = (FullPathValue) value;
        var result = GetLength(pathValue._parentValue);
        if (!HasTrailingSeparator(pathValue._parentValue)) {
          result++;
        }

        result += pathValue._name.Length;
        return result;

      }

      public static bool EqualsOperator(object value, object other) {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(value, other)) return true;

        var name1 = value as string;
        var name2 = other as string;
        if (name1 != null || name2 != null) {
          return StringComparer.OrdinalIgnoreCase.Equals(name1, name2);
        }

        var pathValue1 = (FullPathValue) value;
        var pathValue2 = (FullPathValue) other;
        return EqualsOperator(pathValue1._parentValue, pathValue2._parentValue) &&
               EqualsOperator(pathValue1._name, pathValue2._name);
      }

      public static int Compare(object value, object other) {
        if (ReferenceEquals(value, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;

        var name1 = value as string;
        var name2 = other as string;
        if (name1 != null || name2 != null) {
          return StringComparer.OrdinalIgnoreCase.Compare(name1, name2);
        }

        var pathValue1 = (FullPathValue) value;
        var pathValue2 = (FullPathValue) other;
        var parentComparison = Compare(pathValue1._parentValue, pathValue2._parentValue);
        if (parentComparison != 0) return parentComparison;
        return Compare(pathValue1._name, pathValue2._name);
      }
    }

    /// <summary>
    /// String or <see cref="FullPathValue"/>
    /// </summary>
    private readonly object _value;

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
      var parent = CreatePathValue(PathHelpers.GetParent(path));
      _value = parent == null ? (object) path : new FullPathValue(parent, PathHelpers.GetName(path));
    }

    /// <summary>
    /// Construct a <see cref="FullPath"/> instance from a valid parent <see cref="FullPath"/>
    /// and a relative name.
    /// Throws an exception if the <paramref name="name"/> is not valid.
    /// </summary>
    public FullPath(FullPath parent, string name) {
      if (parent._value == null)
        ThrowArgumentNullException("parent");
      if (string.IsNullOrEmpty(name))
        ThrowArgumentNullException("name");
      if (PathHelpers.HasAltDirectorySeparators(name) || PathHelpers.HasDirectorySeparators(name))
        ThrowArgumentException("Name should not contain directory separators", "name");
      _value = new FullPathValue(parent._value, name);
    }

    private FullPath(object value) {
      Debug.Assert(value != null);
      _value = value;
    }

    private static object CreatePathValue(string path) {
      if (path == null) {
        return null;
      }

      var parentPath = PathHelpers.GetParent(path);
      if (parentPath == null) {
        return path;
      }

      var name = PathHelpers.GetName(path);
      return new FullPathValue(CreatePathValue(parentPath), name);
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
        if (_value is FullPathValue) {
          return ((FullPathValue) _value).Name;
        }

        return (string) _value;
      }
    }

    public FullPath? Parent {
      get {
        if (_value is string) {
          return null;
        }

        return new FullPath(((FullPathValue) _value).ParentValue);
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
        }
        else {
          yield return name.Substring(index, nextSep - index);
          index = nextSep + 1;
        }
      }
    }

    public bool HasTrailingSeparator {
      get { return FullPathValue.HasTrailingSeparator(_value); }
    }

    public PathHelpers.RootPrefixKind PathKind {
      get { return FullPathValue.GetRootPrefixKind(_value); }
    }

    public enum LongPathPrefixKind {
      None,
      LongDiskPath,
      LongUncPath,
    }

    private void BuildPath(StringBuffer sb) {
      FullPathValue.BuildPath(_value, sb);
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
      return FullPathValue.GetLength(path._value);
    }

    public bool Equals(FullPath other) {
      return FullPathValue.EqualsOperator(_value, other._value);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      if (obj.GetType() != GetType()) return false;
      return Equals((FullPath) obj);
    }

    public override int GetHashCode() {
      return FullPathValue.GetHashCode(_value);
    }

    public int CompareTo(FullPath other) {
      return FullPathValue.Compare(_value, other._value);
    }
  }
}