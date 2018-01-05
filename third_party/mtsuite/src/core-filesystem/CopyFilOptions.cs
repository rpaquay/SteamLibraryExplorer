using System;

namespace mtsuite.CoreFileSystem {
  [Flags]
  public enum CopyFileOptions {
    Default = 0,
    /// <summary>
    /// Unbuffered copy, recommened for large files
    /// </summary>
    Unbuffered = 0x0001,
  }
}