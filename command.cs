using ILeoConsole.Core;
using ILeoConsole.Plugin;
using ILeoConsole;

namespace LeoConsole_apkg {
  public class apkg : ICommand {
    public string Name { get { return "apkg"; } }
    public string Description { get { return "advanced package management"; } }
    public Action CommandFunktion { get { return () => Command(); } }
    private string[] _InputProperties;
    public string[] InputProperties { get { return _InputProperties; } set { _InputProperties = value; } }
    public IData data = new ConsoleData();
    private ApkgOutput output = new ApkgOutput();
    private ApkgUtils utils = new ApkgUtils();
    private ApkgInstaller installer = new ApkgInstaller();

    public void Command() {
      if (_InputProperties.Length < 2) {
        output.MessageErr0("you need to provide a subcommand\n");
        apkg_do_help();
        return;
      }
      if (!checkVar()) {
        output.MessageErr0("errors with var files");
        return;
      }
      switch (_InputProperties[1]) {
        case "get": apkg_do_get(); break;
        case "help": apkg_do_help(); break;
        case "info": apkg_do_info(); break;
        case "install": apkg_do_get(); break;
        case "list-available": apkg_do_list_available(); break;
        case "list-installed": apkg_do_list_installed(); break;
        case "remove": apkg_do_remove(); break;
        case "search": apkg_do_search(); break;
        case "update": apkg_do_update(); break;
        default: output.MessageErr0("apkg: unknown subcommand '" + _InputProperties[1] + "'"); break;
      }
    }

    // ------- APKG COMMANDS -------
    private void apkg_do_get() {
      if (_InputProperties.Length < 3){
        output.MessageErr0("you need to provide a package name or location");
        return;
      }
      // TODO for now installing from local package archives only
      installer.GetLCPKG(_InputProperties[2], data.SavePath);
    }

    private void apkg_do_update() {
      output.MessageErr0("update function is not implemented yet");
    }

    private void apkg_do_list_installed() {
      output.MessageWarn0("Listing installed packages is not supported yet");
      output.MessageSuc0("your installed plugin files:");
      try {
        foreach (string filename in Directory.GetFiles(Path.Join(data.SavePath, "plugins"))){
          output.MessageSuc1(filename);
        }
      } catch (Exception e) {
        output.MessageErr1(e.Message);
      }
    }

    private void apkg_do_remove() {
      if (_InputProperties.Length < 3){
        output.MessageErr0("you need to provide an argument");
        return;
      }
      if (!_InputProperties[2].EndsWith(".dll")) {
        output.MessageErr0("Uninstalling packages is not supported yet");
        output.MessageSuc1("Try passing the dll file to remove it");
        return;
      }
      string file = Path.Join(data.SavePath, "plugins", _InputProperties[2]);
      output.MessageSuc0("uninstalling " + file + "...");
      try {
        if (!File.Exists(file)) {
          output.MessageErr1("file does not exist");
          return;
        }
        File.Delete(file);
      } catch (Exception e) {
        output.MessageErr1("cannot remove: " + e.Message);
        return;
      }
      output.MessageSuc1("uninstalled successfully. restart LeoConsole for changes to take effect");
    }

    private void apkg_do_list_available() {
      if (!File.Exists(Path.Join(data.SavePath, "pkg", "PackageList.txt"))) {
        output.MessageErr0("package database could not be found. try 'pkg update'");
        return;
      }
      output.MessageSuc0("available packages:");
      try {
        foreach (string line in File.ReadLines(Path.Join(data.SavePath, "pkg", "PackageList.txt"))) {
          output.MessageSuc1(line.Split(" ")[1].Split(":")[1]);
        }
      } catch (Exception e) {
        output.MessageErr1(e.Message);
        return;
      }
    }
    
    private void apkg_do_search() {
      if (_InputProperties.Length < 3){
        output.MessageErr0("you need to provide a search term");
        return;
      }
      if (!File.Exists(Path.Join(data.SavePath, "pkg", "PackageList.txt"))) {
        output.MessageErr0("package database could not be found. try 'pkg update'");
        return;
      }
      string keyword = _InputProperties[2];
      output.MessageSuc0("results:");
      try {
        foreach (string line in File.ReadLines(Path.Join(data.SavePath, "pkg", "PackageList.txt"))) {
          string pkgName = line.Split(" ")[1].Split(":")[1];
          if (pkgName.ToLower().Contains(keyword.ToLower())) {
            output.MessageSuc1(pkgName);
          }
        }
      } catch (Exception e) {
        output.MessageErr1(e.Message);
        return;
      }
    }

    private void apkg_do_info() {
      output.MessageSuc0("apkg plugin information");
      output.MessageSuc1("cache/download directory:  " + Path.Join(data.DownloadPath, "plugins"));
      output.MessageSuc1("installation directory:    " + Path.Join(data.SavePath, "plugins"));
      output.MessageSuc1("config/database directory: " + Path.Join(data.SavePath, "var", "apkg"));
    }

    private void apkg_do_help() {
      Console.WriteLine(@"
apkg is an advanced package tool for LeoConsole

Besides the default functionality that the standard pkg command provides, it
allows you to install plugins from unofficial repositories or even local
folders, which is very handy for quick development and testing.

Available options:
    get/install:    install plugin from lcpkg file
    help:           print this help
    info:           print where the plugins are downloaded and installed to
    list-available: list plugins available in the default pkg repo
    list-installed: list installed .dll plugin files
    remove:         remove .dll file
    search:         search for a package in the default repos
    update:         update package database
");
    }

    // HELPER FUNCTIONS
    private bool checkVar() {
      string[] folders = {Path.Join(data.SavePath, "var"), Path.Join(data.SavePath, "var", "apkg")};
      foreach (string folder in folders) {
        if (!Directory.Exists(folder)) {
          try {
            Directory.CreateDirectory(folder);
          } catch (Exception e) {
            output.MessageErr1("cannot create var dir: " + e.Message);
            return false;
          }
        }
      }
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
      // show first run message if necessary
      if (!config.Contains("notFirstRun")) {
        firstRun();
      }
      // end
      return true;
    }

    private void printCopyright(){
      Console.WriteLine(@"
Source code is available on <https://github.com/alexcoder04/LeoConsole-apkg>

LeoConsole-apkg-plugin Copyright (c) 2022 alexcoder04
This program comes with ABSOLUTELY NO WARRANTY.
This is free software, and you are welcome to redistribute it
under certain conditions, see <https://www.gnu.org/licenses/gpl-3.0.txt> for more details.
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
