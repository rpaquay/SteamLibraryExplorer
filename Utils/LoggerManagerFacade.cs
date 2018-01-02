using System;

namespace SteamLibraryExplorer.Utils {
  public class LoggerManagerFacade {
    private static bool _nlogAvailable;
    public static void Configure() {
      try {
        NLogUtils.NLogFacade.ConfigureApplication();
      }
      catch (Exception) {
        _nlogAvailable = false;
      }
      
    }

    public static ILoggerFacade GetLogger(Type type) {
      if (!_nlogAvailable) {
        return new NullLoggerFacade();
      }
      try {
        return NLogUtils.NLogFacade.GetLogger(type.FullName);
      }
      catch (Exception) {
        return new NullLoggerFacade();
      }
    }

    public class NullLoggerFacade : ILoggerFacade {
      public void Info(string format, params object[] args) {
      }

      public void Warn(string format, params object[] args) {
      }

      public void Error(string format, params object[] args) {
      }
    }
  }

  public interface ILoggerFacade {
    void Info(string format, params object[] args);
    void Warn(string format, params object[] args);
    void Error(string format, params object[] args);
  }
}
