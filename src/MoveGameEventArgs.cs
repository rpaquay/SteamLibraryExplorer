using System;
using JetBrains.Annotations;
using SteamLibraryExplorer.SteamModel;

namespace SteamLibraryExplorer {
  public class MoveGameEventArgs : EventArgs {
    public MoveGameEventArgs([NotNull]SteamGame game, [NotNull]string destinationLibraryPath) {
      Game = game;
      DestinationLibraryPath = destinationLibraryPath;
    }

    [NotNull]
    public SteamGame Game { get;  }
    [NotNull]
    public string DestinationLibraryPath { get; }
  }
}