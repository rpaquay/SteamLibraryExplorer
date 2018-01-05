using System;
using System.Threading;

namespace SteamLibraryExplorer.Utils {
  public class LoggerManagerFacade {
    private static readonly Lazy<bool> IsNLogAvailable =
      new Lazy<bool>(ConfigureNLog, LazyThreadSafetyMode.ExecutionAndPublication);

    private static bool ConfigureNLog() {
      try {
        NLogUtils.NLogFacade.ConfigureApplication();
        return true;
      }
      catch (Exception) {
        return false;
      }
    }

    public static ILoggerFacade GetLogger(Type type) {
      if (!IsNLogAvailable.Value) {
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

      public void Error(Exception e, string format, params object[] args) {
      }
    }
  }
}