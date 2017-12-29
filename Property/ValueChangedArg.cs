using System;

namespace SteamLibraryExplorer
{
  public class ValueChangedArg<T> : EventArgs {
    public T OldValue { get; set; }
    public T NewValue { get; set; }
  }
}