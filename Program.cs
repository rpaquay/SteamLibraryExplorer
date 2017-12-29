using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SteamLibraryExplorer {
  static class Program {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main() {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      var mainForm = new MainForm();
      var model = new Model();
      var view = new View(model, mainForm);
      var controller = new Controller(model, view);
      controller.Run();
    }
  }
}
