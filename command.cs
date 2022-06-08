using ILeoConsole.Core;
using ILeoConsole.Plugin;
using ILeoConsole;

namespace LeoConsole_apkg {
  public class LeoConsoleApkgCommand : ICommand {
    public string Name { get { return "apkg"; } }
    public string Description { get { return "advanced package manager"; } }
    public Action CommandFunktion { get { return () => Command(); } }
    private string[] _InputProperties;
    public string[] InputProperties
      { get { return _InputProperties; } set { _InputProperties = value; } }
    public IData data = new ConsoleData();

    private ApkgRepository repository;
    private ApkgConfig config;

    private const string apkgVersion="1.0.0";
    private string configFolder;

    public LeoConsoleApkgCommand(string savePath) {
      repository = new ApkgRepository(savePath);
      configFolder = Path.Join(savePath, "var", "apkg");
      config = ApkgConfigHelper.ReadConfig(configFolder);
    }

    public void Command() {
      if (_InputProperties.Length < 2) {
        ApkgOutput.MessageErr0("you need to provide a subcommand\n");
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
        case "build":
          if (!config.DebugMode) {
            ApkgOutput.MessageErr0("this command is only available in debug mode"); return;
          }
          if (_InputProperties.Length < 3) {
            ApkgOutput.MessageErr0("pass a folder to build");
            return;
          }
          if (Directory.Exists(_InputProperties[2])) {
            ApkgOutput.MessageErr0("the folder you passed doesn't exists");
            return;
          }
          ApkgUtils.RunProcess(
              ApkgUtils.GetBuilderPath(data.SavePath),
              _InputProperties[2],
              data.CurrentWorkingPath
              );
          break;
        case "get-local":
          if (!config.DebugMode) {
            ApkgOutput.MessageErr0("this command is only available in debug mode"); return;
          }
          if (_InputProperties.Length < 3) {
            ApkgOutput.MessageErr0("pass a file or folder to install");
            return;
          }
          if (Directory.Exists(_InputProperties[2])) {
            ApkgUtils.RunProcess(
                ApkgUtils.GetBuilderPath(data.SavePath),
                _InputProperties[2],
                data.CurrentWorkingPath
                );
            foreach (string f in Directory.GetFiles(_InputProperties[2])) {
              if (f.EndsWith(".lcpkg")) {
                ApkgOutput.MessageSuc0("installing built archive: " + f);
                repository.InstallLcpkg(f);
                return;
              }
            }
            return;
          }
          if (!_InputProperties[2].EndsWith(".lcpkg")) {
            ApkgOutput.MessageErr0("looks not like an lcpkg file");
            return;
          }
          repository.InstallLcpkg(_InputProperties[2]);
          break;
        default:
          ApkgOutput.MessageErr0("apkg: unknown subcommand '" + _InputProperties[1] + "'");
          break;
      }
    }

    // ------- APKG COMMANDS -------
    private void apkg_do_get() {
      if (_InputProperties.Length < 3){
        ApkgOutput.MessageErr0("you need to provide a package name");
        return;
      }
      try {
        repository.Reload();
      } catch (Exception e) {
        ApkgOutput.MessageErr0("error reloading package database");
        return;
      }
      string url;
      try {
        url = repository.GetUrlFor(_InputProperties[2]);
      } catch (Exception e) {
        ApkgOutput.MessageErr1("cannot find your package");
        return;
      }
      string dlPath = Path.Join(data.DownloadPath, "apkg", "package.lcpkg");
      if (!ApkgUtils.DownloadFile(url, dlPath)) {
        return;
      }
      repository.InstallLcpkg(dlPath);
    }

    private void apkg_do_update() {
      ApkgOutput.MessageErr0("update function is not implemented yet");
    }

    private void apkg_do_list_installed() {
      ApkgOutput.MessageSuc0("your installed packages:");
      try {
        foreach (string filename in Directory.GetDirectories(
              Path.Join(configFolder, "installed")
              )){
          ApkgOutput.MessageSuc1(Path.GetFileName(filename));
        }
      } catch (Exception e) {
        ApkgOutput.MessageErr1(e.Message);
      }
    }

    private void apkg_do_remove() {
      if (_InputProperties.Length < 3){
        ApkgOutput.MessageErr0("you need to provide a package name");
        return;
      }
      repository.RemovePackage(_InputProperties[2]);
    }

    private void apkg_do_list_available() {
      IList<string> list = repository.AvailablePlugins();
      ApkgOutput.MessageSuc0("available packages:");
      foreach (string p in list) {
        ApkgOutput.MessageSuc1(p);
      }
    }
    
    private void apkg_do_search() {
      string keyword = _InputProperties[2];
      IList<string> list = repository.AvailablePlugins();
      ApkgOutput.MessageSuc0("results:");
      foreach (string p in list) {
        if (p.ToLower().Contains(keyword.ToLower())) {
          ApkgOutput.MessageSuc1(p);
        }
      }
    }

    private void apkg_do_info() {
      ApkgOutput.MessageSuc0("apkg information");
      ApkgOutput.MessageSuc1(
          "cache/download directory:  "
          + Path.Join(data.DownloadPath, "apkg"));
      ApkgOutput.MessageSuc1(
          "installation directory:    "
          + Path.Join(data.SavePath, "plugins"));
      ApkgOutput.MessageSuc1(
          "config/database directory: "
          + Path.Join(configFolder));
      ApkgOutput.MessageSuc1(
          "docs directory:            "
          + Path.Join(data.SavePath, "share", "docs", "apkg"));
      ApkgOutput.MessageSuc1("apkg version: " + apkgVersion);
      if (config.DebugMode) {
        ApkgOutput.MessageWarn1("debug mode: ON");
      } else {
        ApkgOutput.MessageSuc1("debug mode: off");
      }
    }

    private void apkg_do_reload() {
      try {
        repository.Reload();
      } catch (Exception e) {
        ApkgOutput.MessageErr0("error realoding package database");
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
      if (config.DebugMode) {
        Console.WriteLine(@"
Available options in debug mode:
    build:         build plugin in directory
    get-local:     install local .lcpkg file
");
      }
    }
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
