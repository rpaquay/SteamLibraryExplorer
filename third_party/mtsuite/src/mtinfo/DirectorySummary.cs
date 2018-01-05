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
using mtsuite.CoreFileSystem;
using mtsuite.shared.Collections;

namespace mtinfo {
  public class DirectorySummaryRoot {
    public DirectorySummaryRoot() {
      LongestPaths = new PriorityQueue<LongestPathInfo>();
    }
    public DirectorySummary Summary { get; set; }
    public PriorityQueue<LongestPathInfo> LongestPaths { get; set; }
  }

  public class LongestPathInfo : IComparable<LongestPathInfo> {
    public FullPath Path { get; set; }
    public int Length { get; set; }

    public int CompareTo(LongestPathInfo other) {
      if (other == null) {
        return 1;
      }
      return Path.Length.CompareTo(other.Path.Length);
    }
  }

  public class DirectorySummary {
    private readonly int _depth;
    private readonly FileSystemEntry _directoryEntry;
    private readonly DirectoryStats _stats;
    private readonly List<DirectorySummary> _children;

    public DirectorySummary(FileSystemEntry directoryEntry, int depth) {
      _directoryEntry = directoryEntry;
      _depth = depth;
      _stats = new DirectoryStats();
      _children = new List<DirectorySummary>();
    }

    public FileSystemEntry DirectoryEntry {
      get { return _directoryEntry; }
    }

    public int Depth {
      get { return _depth; }
    }

    public List<DirectorySummary> Children {
      get { return _children; }
    }

    public DirectoryStats Stats {
      get { return _stats; }
    }
  }
}