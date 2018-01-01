using System.Windows;
using NLog;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer {
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public App() {
      NLogUtils.ConfigureApplication();
      _logger.Info("Application is starting");
      this.Exit += (sender, args) => _logger.Info("Application is exiting");
    }
  }
}
