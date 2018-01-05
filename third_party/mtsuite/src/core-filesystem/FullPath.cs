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
using mtsuite.CoreFileSystem.Utils;
using mtsuite.CoreFileSystem.Win32;

namespace mtsuite.CoreFileSystem {
  public class FullPath : IStringSource {
    private readonly FullPath _parent;
    private readonly string _name;

    public FullPath(string path) {
      if (!PathHelpers.IsPathAbsolute(path))
        throw new ArgumentException("Path should be absolute", path);
      _name = path;
    }

    public FullPath(FullPath parent, string name) {
      if (parent == null)
        ThrowArgumentNullException("parent");
      if (string.IsNullOrEmpty(name))
        ThrowArgumentNullException("name");
      _parent = parent;
      _name = name;
    }

    private static void ThrowArgumentNullException(string paramName) {
      throw new ArgumentNullException(paramName);
    }

    public string Path {
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
      return Path;
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
          result ++;
      }
      return result;
    }
  }
}