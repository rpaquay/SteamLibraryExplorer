using System;

namespace SteamLibraryExplorer.Utils {
  public interface ILoggerFacade {
    void Info(string format, params object[] args);
    void Warn(string format, params object[] args);
    void Error(string format, params object[] args);
    void Error(Exception e, string format, params object[] args);
  }
}