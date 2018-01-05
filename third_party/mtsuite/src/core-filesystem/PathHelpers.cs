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
using System.IO;
using System.Linq;
using System.Text;

namespace mtsuite.CoreFileSystem {
  public static class PathHelpers {
    public static readonly string DirectorySeparatorString = Path.DirectorySeparatorChar.ToString();
    public  static readonly string AltDirectorySeparatorString = Path.AltDirectorySeparatorChar.ToString();
    private static readonly string LongDiskPathPrefix = @"\\?\";
    private static readonly string LongUncPathPrefix = @"\\?\UNC\";
    private static readonly string UncPathPrefix  = @"\\";

    /// <summary>
    /// Return <code>true</code> if <paramref name="path"/> is an absolute path.
    /// </summary>
    public static bool IsPathAbsolute(string path) {
      return GetPathRootPrefixInfo(path).RootPrefixKind != RootPrefixKind.None;
    }

    /// <summary>
    /// Return <code>true</code> if <paramref name="path"/> is not an absolute path.
    /// </summary>
    public static bool IsPathRelative(string path) {
      return !IsPathAbsolute(path);
    }

    public static bool HasDirectorySeparators(string path) {
      return path.IndexOf(Path.DirectorySeparatorChar) < 0;
    }

    public static bool HasAltDirectorySeparators(string path) {
      return path.IndexOf(Path.AltDirectorySeparatorChar) < 0;
    }

    /// <summary>
    /// Return the <paramref name="path"/> with its last last component removed,
    /// or the empty string if the path is a "root" path (.e.g "c:\). Throws an
    /// exception if <paramref name="path"/> is not an absolute path.
    /// </summary>
    public static string GetParent(string path) {
      if (!IsPathAbsolute(path))
        throw new ArgumentException("Path should be absolute", path);

      // Ignore '\' at the end of path
      var startIndex = path.Length - 1;
      var count = path.Length;
      if (path.Last() == Path.DirectorySeparatorChar) {
        startIndex--;
        count--;
      }
      var lastIndex = path.LastIndexOf(Path.DirectorySeparatorChar, startIndex, count);
      if (lastIndex < 0)
        return "";

      // Keep the terminating '\' to avoid returned invalid root path (e.g. 'c:')
      var result = path.Substring(0, lastIndex + 1);
      if (result == LongDiskPathPrefix || result == UncPathPrefix || result == LongUncPathPrefix)
        return "";
      return result;
    }

    /// <summary>
    /// Return the last component of the <paramref name="path"/>. Throws an
    /// exception if <paramref name="path"/> is not an absolute path.
    /// </summary>
    public static string GetName(string path) {
      if (!IsPathAbsolute(path))
        throw new ArgumentException("Path should be absolute", path);

      var startIndex = path.Length - 1;
      var count = path.Length;
      if (path.Last() == Path.DirectorySeparatorChar) {
        startIndex--;
        count--;
      }
      var lastIndex = path.LastIndexOf(Path.DirectorySeparatorChar, startIndex, count);
      if (lastIndex < 0)
        return "";

      // Check the remaining prefix is not a root path prefix (e.g. "\\").
      var prefix = path.Substring(0, lastIndex + 1);
      if (prefix == LongDiskPathPrefix || prefix == UncPathPrefix || prefix == LongUncPathPrefix)
        return "";

      return path.Substring(lastIndex + 1, count - lastIndex - 1);
    }

    /// <summary>
    /// Return <paramref name="path"/> where the (optional) long path prefix is removed.
    /// Note: An exception is thrown if <paramref name="path"/> is not an absolute path.
    /// </summary>
    public static string StripLongPathPrefix(string path) {
      if (string.IsNullOrEmpty(path))
        throw new ArgumentNullException("path");

      path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

      var info = GetPathRootPrefixInfo(path);
      switch (info.RootPrefixKind) {
        case RootPrefixKind.None:
          throw new ArgumentException(string.Format("Path \"{0}\" should be absolute", path), "path");
        case RootPrefixKind.LongDiskPath:
          return path.Substring(LongDiskPathPrefix.Length);
        case RootPrefixKind.LongUncPath:
          return @"\\" + path.Substring(LongUncPathPrefix.Length);
        case RootPrefixKind.UncPath:
        case RootPrefixKind.DiskPath:
          return path;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    /// <summary>
    /// Return <paramref name="path"/> with the appropriate long path prefix.
    /// Note: An exception is thrown if <paramref name="path"/> is not an absolute path.
    /// </summary>
    public static string MakeLongPath(string path) {
      if (string.IsNullOrEmpty(path))
        throw new ArgumentNullException("path");

      path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

      var info = GetPathRootPrefixInfo(path);
      switch (info.RootPrefixKind) {
        case RootPrefixKind.LongDiskPath:
          return path;
        case RootPrefixKind.LongUncPath:
          return path;
        case RootPrefixKind.DiskPath:
          return LongDiskPathPrefix + path;
        case RootPrefixKind.UncPath:
          return LongUncPathPrefix + path.Substring(2);
        case RootPrefixKind.None:
          throw new ArgumentException("Path should be absolute", path);
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    /// <summary>
    /// Returns a valid absolute path given a valid current directory and a path
    /// from unvalidated user input. Throws an exception if <paramref name="userPath"/> cannot
    /// be validated into a valid absolute path.
    /// </summary>
    public static string NormalizeUserInputPath(string currentDirectory, string userPath) {
      if (!IsPathAbsolute(currentDirectory))
        throw new ArgumentException("Current directory path should be absolute", currentDirectory);

      var path = userPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
      if (!IsPathAbsolute(path)) {
        // Special case: path is of the form "x:": Add a terminating "\" to make it valid
        if (path.Length == 2 && char.IsLetter(path[0]) && path[1] == Path.VolumeSeparatorChar) {
          path = path + Path.DirectorySeparatorChar;
        }
        else {
          currentDirectory = currentDirectory.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
          path = CombinePaths(currentDirectory, path);
        }
      }

      path = NormalizePath(path);
      return path;
    }

    /// <summary>
    /// Return <paramref name="path"/> ensuring <paramref name="path"/> has a trailing directory separator.
    /// </summary>
    public static string AppendTrailingSeparator(string path) {
      if (string.IsNullOrEmpty(path))
        throw new ArgumentNullException("path");

      path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
      if (path[path.Length - 1] == Path.DirectorySeparatorChar)
        return path;

      return path + Path.DirectorySeparatorChar;
    }

    /// <summary>
    /// Return <paramref name="path"/> ensuring if the (optional) trailing
    /// directory separator is removed.
    /// </summary>
    public static string RemoveTrailingSeparator(string path) {
      if (string.IsNullOrEmpty(path))
        throw new ArgumentNullException("path");

      path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
      if (path[path.Length - 1] == Path.DirectorySeparatorChar)
        return path;

      return path + Path.DirectorySeparatorChar;
    }

    /// <summary>
    /// Combine absolute source path <paramref name="path1"/> with the relative
    /// path <paramref name="path2"/>. This is nothing more than a helper method
    /// to add a directory separator characters between two paths if needed.
    /// </summary>
    public static string CombinePaths(string path1, string path2) {
      if (string.IsNullOrEmpty(path2))
        throw new ArgumentNullException("path2");

      path1 = path1 ?? "";
      path1 = path1.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
      path2 = path2.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

      if (path1 == "") {
        if (!IsPathAbsolute(path2))
          throw new ArgumentException("Path should be absolute", path2);
        return path2;
      }

      if (!IsPathAbsolute(path1))
        throw new ArgumentException("Path should be absolute", path1);
      if (IsPathAbsolute(path2))
        throw new ArgumentException("Path should be relative", path2);

      var hasTrailingSeparator = path1[path1.Length - 1] == Path.DirectorySeparatorChar;
      var hasLeadingSeparator = path2[0] == Path.DirectorySeparatorChar;
      if (hasTrailingSeparator) {
        if (hasLeadingSeparator)
          return path1 + path2.Substring(1);
        else
          return path1 + path2;
      } else {
        if (hasLeadingSeparator)
          return path1 + path2;
        else {
          return path1 + DirectorySeparatorString + path2;
        }
      }
    }

    /// <summary>
    /// Normalize <paramref name="path"/> into an abolute path, by removing any occurrence
    /// of <code>.</code> or <code>..</code> inside the path.
    /// </summary>
    public static string NormalizePath(string path) {
      if (string.IsNullOrEmpty(path))
        throw new ArgumentNullException("path");

      path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

      var sb = new StringBuilder();
      var lexer = new PathLexer(path);

      var prefixInfo = GetPathRootPrefixInfo(path);
      if (prefixInfo.Length > 0) {
        lexer.Skip(prefixInfo.Length);
      }

      var components = new Stack<string>();
      foreach (var component in lexer.Split(Path.DirectorySeparatorChar)) {
        if (component == ".") {
          // skip it
        } else if (component == "..") {
          if (components.Count == 0)
            throw new ArgumentException("Invalid path: too many '..'");
          components.Pop();
        } else {
          components.Push(component);
        }
      }

      foreach (var component in components.Reverse()) {
        if (sb.Length > 0)
          sb.Append(Path.DirectorySeparatorChar);
        sb.Append(component);
      }
      sb.Insert(0, path.Substring(0, prefixInfo.Length));
      return sb.ToString();
    }

    /// <summary>
    /// Return <code>true</code> if <paramref name="path"/> is an absolute path.
    /// </summary>
    private static PathRootPrefixInfo GetPathRootPrefixInfo(string path) {
      if (string.IsNullOrEmpty(path))
        return default(PathRootPrefixInfo);

      // Long path format (UNC)
      if (path.StartsWith(LongUncPathPrefix, StringComparison.OrdinalIgnoreCase))
        return new PathRootPrefixInfo(LongUncPathPrefix.Length, RootPrefixKind.LongUncPath);

      // Long path format
      if (path.StartsWith(LongDiskPathPrefix, StringComparison.OrdinalIgnoreCase)) {
        int diskPrefix = GetDiskRootPrefix(path, LongDiskPathPrefix.Length);
        if (diskPrefix == 0)
          return default(PathRootPrefixInfo);
        return new PathRootPrefixInfo(LongDiskPathPrefix.Length + diskPrefix, RootPrefixKind.LongDiskPath);
      }

      // Device format: 'X:\' prefix
      int localDiskPrefix = GetDiskRootPrefix(path, 0);
      if (localDiskPrefix > 0)
        return new PathRootPrefixInfo(localDiskPrefix, RootPrefixKind.DiskPath);

      // UNC Format: '\\' prefix
      if (path.Length >= 2 && path[0] == Path.DirectorySeparatorChar && path[1] == Path.DirectorySeparatorChar)
        return new PathRootPrefixInfo(2, RootPrefixKind.UncPath);

      return default(PathRootPrefixInfo);
    }

    private static int GetDiskRootPrefix(string path, int index) {
      if (path.Length >= index + 3 && char.IsLetter(path[index + 0]) && path[index + 1] == Path.VolumeSeparatorChar && path[index + 2] == Path.DirectorySeparatorChar) {
        return 3;
      }
      return 0;
    }

    private struct PathRootPrefixInfo {
      private readonly int _length;
      private readonly RootPrefixKind _rootPrefixKind;

      public PathRootPrefixInfo(int length, RootPrefixKind rootPrefixKind) {
        _length = length;
        _rootPrefixKind = rootPrefixKind;
      }

      public int Length {
        get { return _length; }
      }

      public RootPrefixKind RootPrefixKind {
        get { return _rootPrefixKind; }
      }
    }

    private enum RootPrefixKind {
      None,
      LongDiskPath,
      LongUncPath,
      DiskPath,
      UncPath,
    }

    private class PathLexer {
      private readonly string _path;
      private int _index;

      public PathLexer(string path) {
        _path = path;
      }

      public void Skip(int length) {
        _index += length;
      }

      public IEnumerable<string> Split(char sep) {
        return _path.Substring(_index).Split(new[] { sep }, StringSplitOptions.RemoveEmptyEntries);
      }
    }
  }
}