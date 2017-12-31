using System;
using SteamLibraryExplorer.SteamModel;

namespace SteamLibraryExplorer {
  public class MoveGameEventArgs : EventArgs {
    public MoveGameEventArgs(SteamGame game, string destinationLibraryPath) {
      Game = game;
      DestinationLibraryPath = destinationLibraryPath;
    }

    public SteamGame Game { get;  }
    public string DestinationLibraryPath { get; }
  }
}