using System;

namespace SteamLibraryExplorer.SteamUtil {
  public class MoveGameResult {
    public MoveGameResult(SteamGameMover.MoveGameResultKind kind, Exception error) {
      Kind = kind;
      Error = error;
    }

    public static MoveGameResult CreateError(Exception e) {
      if (e is OperationCanceledException) {
        return new MoveGameResult(SteamGameMover.MoveGameResultKind.Cancelled, null);
      }
      return new MoveGameResult(SteamGameMover.MoveGameResultKind.Error, e);
    }

    public static MoveGameResult CreateOk() {
      return new MoveGameResult(SteamGameMover.MoveGameResultKind.Ok, null);
    }

    public SteamGameMover.MoveGameResultKind Kind { get; }
    public Exception Error { get; }
  }
}