using System;
using JetBrains.Annotations;

namespace SteamLibraryExplorer.SteamUtil {
  public class MoveGameResult {
    public MoveGameResult(MoveGameResultKind kind, Exception error) {
      Kind = kind;
      Error = error;
    }

    [NotNull]
    public static MoveGameResult CreateError([NotNull]Exception e) {
      if (e is OperationCanceledException) {
        return new MoveGameResult(MoveGameResultKind.Cancelled, null);
      }
      return new MoveGameResult(MoveGameResultKind.Error, e);
    }

    [NotNull]
    public static MoveGameResult CreateOk() {
      return new MoveGameResult(MoveGameResultKind.Ok, null);
    }

    public MoveGameResultKind Kind { get; }
    [CanBeNull]
    public Exception Error { get; }
  }
}