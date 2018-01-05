using System.Windows;
using SteamLibraryExplorer.Utils;

namespace SteamLibraryExplorer {
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application {
    private static readonly ILoggerFacade Logger = LoggerManagerFacade.GetLogger(typeof(App));

    public App() {
      Logger.Info("Application is starting");
      this.Exit += (sender, args) => Logger.Info("Application is exiting");
    }
  }
}
