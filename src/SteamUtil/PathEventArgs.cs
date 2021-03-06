﻿using System;
using JetBrains.Annotations;
using mtsuite.CoreFileSystem;

namespace SteamLibraryExplorer.SteamUtil {
  public class PathEventArgs : EventArgs {
    public PathEventArgs([NotNull]FullPath path) {
      Path = path;
    }

    [NotNull]
    public FullPath Path { get; }
  }
}