using System;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace SteamLibraryExplorer.Utils.NLogUtils {
  public class NLogFacade {
    public static void ConfigureApplication() {
      // Step 1. Create configuration object 
      var config = new LoggingConfiguration();

      // Step 2. Create targets and add them to the configuration 
      var consoleTarget = new ColoredConsoleTarget();
      config.AddTarget("console", consoleTarget);

      var fileTarget = new FileTarget();
      config.AddTarget("file", fileTarget);

      var errorFileTarget = new FileTarget();
      config.AddTarget("error-file", errorFileTarget);

      // Step 3. Set target properties 
      const string layout = @"[${date:format=HH\:mm\:ss} ${logger}] ${message}";

      consoleTarget.Layout = layout;

      fileTarget.FileName = "${tempdir}/${processname}.log";
      fileTarget.Layout = layout; ;
      fileTarget.KeepFileOpen = true; // For performance
      fileTarget.DeleteOldFileOnStartup = true;

      errorFileTarget.FileName = "${tempdir}/${processname}-${shortdate}.errors.log";
      errorFileTarget.Layout = layout; ;
      errorFileTarget.KeepFileOpen = true; // For performance

      // Step 4. Define rules
      var rule1 = new LoggingRule("*", LogLevel.Warn, consoleTarget);
      config.LoggingRules.Add(rule1);

      var rule2 = new LoggingRule("*", LogLevel.Info, fileTarget);
      config.LoggingRules.Add(rule2);

      var rule3 = new LoggingRule("*", LogLevel.Warn, errorFileTarget);
      config.LoggingRules.Add(rule3);

      // Step 5. Activate the configuration
      LogManager.Configuration = config;
    }

    public static ILoggerFacade GetLogger(string typeFullName) {
      return new LoggerFacade(LogManager.GetLogger(typeFullName));
    }

    public class LoggerFacade : ILoggerFacade {
      private readonly Logger _logger;

      public LoggerFacade(Logger logger) {
        _logger = logger;
      }

      public void Info(string format, params object[] args) {
        _logger.Info(format, args);
      }

      public void Warn(string format, params object[] args) {
        _logger.Warn(format, args);
      }

      public void Error(string format, params object[] args) {
        _logger.Error(format, args);
      }

      public void Error(Exception e, string format, params object[] args) {
        _logger.Error(e, format, args);
      }
    }
  }
}
