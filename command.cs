using ILeoConsole.Core;
using ILeoConsole.Plugin;
using ILeoConsole;

namespace LeoConsole_apkg {
  public class LeoConsoleApkgCommand : ICommand {
    public string Name { get { return "apkg"; } }
    public string Description { get { return "advanced package management"; } }
    public Action CommandFunktion { get { return () => Command(); } }
    private string[] _InputProperties;
    public string[] InputProperties
      { get { return _InputProperties; } set { _InputProperties = value; } }
    public IData data = new ConsoleData();

    private ApkgOutput output = new ApkgOutput();
    private ApkgUtils utils = new ApkgUtils();
    private ApkgRepository repository = new ApkgRepository();
    private ApkgIntegrity integrity = new ApkgIntegrity();

    private bool debugMode = false;

    public void Command() {
      readConfig();

      if (_InputProperties.Length < 2) {
        output.MessageErr0("you need to provide a subcommand\n");
        apkg_do_help();
        return;
      }

      switch (_InputProperties[1]) {
        case "get": apkg_do_get(); break;
        case "help": apkg_do_help(); break;
        case "info": apkg_do_info(); break;
        case "install": apkg_do_get(); break;
        case "list-available": apkg_do_list_available(); break;
        case "list-installed": apkg_do_list_installed(); break;
        case "reload": apkg_do_reload(); break;
        case "remove": apkg_do_remove(); break;
        case "search": apkg_do_search(); break;
        case "update": apkg_do_update(); break;
        case "get-local":
          if (!debugMode) {
            output.MessageErr0("this command is only available in debug mode");
            break;
          }
          repository.InstallLcpkg(_InputProperties[2], data.SavePath);
          break;
        default:
          output.MessageErr0("apkg: unknown subcommand '" + _InputProperties[1] + "'");
          break;
      }
    }

    // ------- APKG COMMANDS -------
    private void apkg_do_get() {
      if (_InputProperties.Length < 3){
        output.MessageErr0("you need to provide a package name");
        return;
      }
      try {
        repository.Reload(data.SavePath);
      } catch (Exception e) {
        output.MessageErr0("error reloading package database");
        return;
      }
      string url;
      try {
        url = repository.GetUrlFor(_InputProperties[2], data.SavePath);
      } catch (Exception e) {
        output.MessageErr1("cannot find your package");
        return;
      }
      string dlPath = Path.Join(data.SavePath, "tmp", "package.lcpkg");
      if (!utils.DownloadFile(url, dlPath)) {
        return;
      }
      repository.InstallLcpkg(dlPath, data.SavePath);
    }

    private void apkg_do_update() {
      output.MessageErr0("update function is not implemented yet");
    }

    private void apkg_do_list_installed() {
      output.MessageSuc0("your installed packages:");
      try {
        foreach (string filename in Directory.GetDirectories(
              Path.Join(data.SavePath, "var", "apkg", "installed")
              )){
          output.MessageSuc1(Path.GetFileName(filename));
        }
      } catch (Exception e) {
        output.MessageErr1(e.Message);
      }
    }

    private void apkg_do_remove() {
      if (_InputProperties.Length < 3){
        output.MessageErr0("you need to provide a package name");
        return;
      }
      repository.RemovePackage(_InputProperties[2], data.SavePath);
    }

    private void apkg_do_list_available() {
      IList<string> list = repository.AvailablePlugins(data.SavePath);
      output.MessageSuc0("available packages:");
      foreach (string p in list) {
        output.MessageSuc1(p);
      }
    }
    
    private void apkg_do_search() {
      string keyword = _InputProperties[2];
      IList<string> list = repository.AvailablePlugins(data.SavePath);
      output.MessageSuc0("results:");
      foreach (string p in list) {
        if (p.ToLower().Contains(keyword.ToLower())) {
          output.MessageSuc1(p);
        }
      }
    }

    private void apkg_do_info() {
      output.MessageSuc0("apkg information");
      output.MessageSuc1(
          "cache/download directory:  "
          + Path.Join(data.DownloadPath, "apkg"));
      output.MessageSuc1(
          "installation directory:    "
          + Path.Join(data.SavePath, "plugins"));
      output.MessageSuc1(
          "config/database directory: "
          + Path.Join(data.SavePath, "var", "apkg"));
      output.MessageSuc1(
          "docs directory:            "
          + Path.Join(data.SavePath, "share", "docs", "apkg"));
      if (debugMode) {
        output.MessageWarn1("debug mode: ON");
      } else {
        output.MessageSuc1("debug mode: off");
      }
    }

    private void apkg_do_reload() {
      try {
        repository.Reload(data.SavePath);
      } catch (Exception e) {
        output.MessageErr0("error realoding package database");
        return;
      }
    }

    private void apkg_do_help() {
      Console.WriteLine(@"
apkg is an advanced package tool for LeoConsole

For more detailed documentation see '$SAVEPATH/share/docs/apkg'

Available options:
    get/install:    install package
    help:           print this help
    info:           print where the plugins are downloaded and installed to
    list-available: list available plugins
    list-installed: list installed plugins
    remove:         remove package
    search:         search for a package
    reload:         reload package database
    update:         update packages
");
      if (debugMode) {
        Console.WriteLine(@"
Available options in debug mode:
    get-local:     install local .lcpkg file
");
      }
    }

    // HELPER FUNCTIONS
    private void readConfig() {
      // create config
      string configFile = Path.Join(data.SavePath, "var", "apkg", "config");
      if (!File.Exists(configFile)) {
        firstRun();
        string[] lines = {"notFirstRun"};
        using (StreamWriter outputFile = new StreamWriter(configFile)) {
          foreach (string line in lines) {
            outputFile.WriteLine(line);
          }
        }
      }
      // read config
      IEnumerable<string> config = File.ReadLines(configFile);
      if (!config.Contains("notFirstRun")) {
        firstRun();
      }
      if (config.Contains("debugModeOn")) {
        debugMode = true;
      }
    }

    private void printCopyright(){
      Console.WriteLine(@"
Source code is available on <https://github.com/alexcoder04/LeoConsole-apkg>

LeoConsole-apkg-plugin Copyright (c) 2022 alexcoder04
This program comes with ABSOLUTELY NO WARRANTY.
This is free software, and you are welcome to redistribute it
under certain conditions, see <https://www.gnu.org/licenses/gpl-3.0.txt>
for more details.
");
    }

    private void firstRun() {
      printCopyright();
      Console.WriteLine(@"
You are running apkg for the first time. Please READ CAREFULLY following information:

 - apkg installs plugin files into $SAVEPATH/plugins and $SAVEPATH/share. These
   files are then managed by apkg. Manually changing or deleting them may cause
   irrecoverable errors.
 - apkg keeps track of installed plugins and other information in $SAVEPATH/var/apkg
   Modifing these files manually or deleting them will brick your install.

Enjoy apkg!
(press any key to continue...)
");
      Console.ReadKey();
    }
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
