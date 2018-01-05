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

using System;
using System.Collections.Generic;
using mtsuite.CoreFileSystem.Utils;
using mtsuite.CoreFileSystem.Win32;

namespace mtsuite.CoreFileSystem {
  /// <summary>
  /// Represents a fully qualified path.
  /// </summary>
  public class FullPath : IEquatable<FullPath>, IComparable<FullPath>, IStringSource {
    private readonly FullPath _parent;
    private readonly string _name;

    /// <summary>
    /// Construct a <see cref="FullPath"/> instance from a valid fully qualifed path
    /// represented as the <see cref="string"/> <paramref name="path"/>.
    /// Throws an exception if the <paramref name="path"/> is not valie.
    /// </summary>
    public FullPath(string path) {
      if (!PathHelpers.IsPathAbsolute(path))
        ThrowArgumentException("Path should be absolute", "path");
      if (PathHelpers.HasAltDirectorySeparators(path))
        ThrowArgumentException("Path should only contain valid directory separators", "path");
      _name = path;
    }

    /// <summary>
    /// Construct a <see cref="FullPath"/> instance from a valid parent <see cref="FullPath"/>
    /// and a relative name.
    /// Throws an exception if the <paramref name="name"/> is not valie.
    /// </summary>
    public FullPath(FullPath parent, string name) {
      if (parent == null)
        ThrowArgumentNullException("parent");
      if (string.IsNullOrEmpty(name))
        ThrowArgumentNullException("name");
      if (PathHelpers.HasAltDirectorySeparators(name) || PathHelpers.HasDirectorySeparators(name))
        ThrowArgumentException("Name should not contain directory separators", "name");
      _parent = parent;
      _name = name;
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
        if (_parent == null) {
          return PathHelpers.GetName(_name);
        }
        return _name;
      }
    }

    public FullPath Parent {
      get {
        if (_parent != null)
          return _parent;

        var parentPath = PathHelpers.GetParent(_name);
        return string.IsNullOrEmpty(parentPath) ? null : new FullPath(parentPath);
      }
    }

    public FullPath Combine(string name) {
      return new FullPath(this, name);
    }

    public bool HasTrailingSeparator {
      get { return _name[_name.Length - 1] == System.IO.Path.DirectorySeparatorChar; }
    }

    private void BuildPath(StringBuffer sb) {
      if (_parent != null) {
        _parent.BuildPath(sb);
        if (!_parent.HasTrailingSeparator)
          sb.Append(System.IO.Path.DirectorySeparatorChar);
      }
      sb.Append(_name);
    }

    public override string ToString() {
      return FullName;
    }

    public int Length {
      get { return GetLength(this); }
    }

    public string Text {
      get { return ToString(); }
    }

    public void CopyTo(StringBuffer sb) {
      BuildPath(sb);
    }

    private static int GetLength(FullPath path) {
      var result = path._name.Length;
      while (path._parent != null) {
        path = path._parent;
        result += path._name.Length;
        if (!path.HasTrailingSeparator)
          result++;
      }
      return result;
    }

    public bool Equals(FullPath other) {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return Equals(_parent, other._parent) &&
             string.Equals(_name, other._name, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((FullPath)obj);
    }

    public override int GetHashCode() {
      unchecked {
        return ((_parent != null ? _parent.GetHashCode() : 0) * 397) ^
               (_name != null ? _name.GetHashCode() : 0);
      }
    }

    public int CompareTo(FullPath other) {
      if (ReferenceEquals(this, other)) return 0;
      if (ReferenceEquals(null, other)) return 1;
      var parentComparison = Comparer<FullPath>.Default.Compare(_parent, other._parent);
      if (parentComparison != 0) return parentComparison;
      return string.Compare(_name, other._name, StringComparison.OrdinalIgnoreCase);
    }
  }
}