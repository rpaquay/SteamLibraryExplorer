namespace SteamLibraryExplorer.SteamUtil {
  public enum MovePhase {
    DiscoveringSourceFiles,
    CopyingFiles,
    DeletingSourceDirectory,
    DeletingDestinationAfterCancellation,
  }
}