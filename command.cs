using ILeoConsole.Core;
using ILeoConsole.Plugin;
using ILeoConsole;

namespace LeoConsole_apkg {
  public class LeoConsoleApkgCommand : ICommand {
    // default stuff {{{
    public string Name { get { return "apkg"; } }
    public string Description { get { return "package manager"; } }
    public Action CommandFunction { get { return () => Command(); } }
    public Action HelpFunction { get { return () => apkg_do_help(); } }
    private string[] _Arguments;
    public string[] Arguments
      { get { return _Arguments; } set { _Arguments = value; } }
    public IData data = new ConsoleData();
    // }}}

    // variables {{{
    private ApkgRepository Repository;
    private ApkgConfig Config;

    private const string apkgVersion="1.3.0";
    private string ConfigFolder;
    // }}}

    public LeoConsoleApkgCommand(string savePath, string lcVersion) {
      Repository = new ApkgRepository(savePath, lcVersion);
      ConfigFolder = Path.Join(savePath, "var", "apkg");
      Config = ApkgConfigHelper.ReadConfig(ConfigFolder);
    }

    // Command() {{{
    public void Command() {
      if (_Arguments.Length < 2) {
        ApkgOutput.MessageErr0("you need to provide a subcommand\n");
        apkg_do_help();
        return;
      }

      switch (_Arguments[1]) {
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
        case "build": apkg_do_build(); break;
        case "get-local": apkg_do_get_local(); break;
        default:
          ApkgOutput.MessageErr0("apkg: unknown subcommand '" + _Arguments[1] + "'");
          break;
      }
    }
    // }}}

    // APKG COMMANDS {{{
    // get() {{{
    private void apkg_do_get() {
      if (_Arguments.Length < 3){
        ApkgOutput.MessageErr0("you need to provide a package name");
        return;
      }
      try {
        Repository.Reload(Config.Repositories);
      } catch (Exception e) {
        ApkgOutput.MessageErr0("error reloading package database");
        return;
      }
      string url;
      try {
        url = Repository.GetUrlFor(_Arguments[2]);
      } catch (Exception e) {
        ApkgOutput.MessageErr1("cannot find your package");
        return;
      }
      string dlPath = Path.Join(data.DownloadPath, "apkg", $"{_Arguments[2]}.lcp");
      if (!ApkgUtils.DownloadFile(url, dlPath)) {
        return;
      }
      Repository.InstallLcpkg(dlPath);
    } // }}}

    // get_local() {{{
    private void apkg_do_get_local() {
      if (!Config.DebugMode) {
        ApkgOutput.MessageErr0("this command is only available in debug mode"); return;
      }
      if (_Arguments.Length < 3) {
        ApkgOutput.MessageErr0("you need to pass a file or folder to install");
        return;
      }
      if (Directory.Exists(_Arguments[2])) {
        if (!ApkgUtils.RunProcess(
            ApkgUtils.GetBuilderPath(data.SavePath),
            _Arguments[2],
            data.CurrentWorkingPath
            )) {
          ApkgOutput.MessageErr0("error building plugin");
          return;
        }
        foreach (string f in Directory.GetFiles(_Arguments[2])) {
          if (f.EndsWith(".lcp")) {
            ApkgOutput.MessageSuc0("installing built archive: " + f);
            Repository.InstallLcpkg(f);
            return;
          }
        }
        return;
      }
      if (!_Arguments[2].EndsWith(".lcp")) {
        ApkgOutput.MessageErr0("looks not like an lcp file");
        return;
      }
      Repository.InstallLcpkg(_Arguments[2]);
    } // }}}

    // build() {{{
    private void apkg_do_build() {
      if (!Config.DebugMode) {
        ApkgOutput.MessageErr0("this command is only available in debug mode");
        return;
      }
      if (_Arguments.Length < 3) {
        ApkgOutput.MessageErr0("you need to pass a folder to build");
        return;
      }
      if (!Directory.Exists(_Arguments[2])) {
        ApkgOutput.MessageErr0("the folder you passed doesn't exists");
        return;
      }
      ApkgUtils.RunProcess(
          ApkgUtils.GetBuilderPath(data.SavePath),
          _Arguments[2],
          data.CurrentWorkingPath
          );
    } // }}}

    // update() {{{
    private void apkg_do_update() {
      ApkgOutput.MessageErr0("update function is not implemented yet");
    } // }}}

    // list_installed() {{{
    private void apkg_do_list_installed() {
      ApkgOutput.MessageSuc0("your installed packages:");
      try {
        foreach (string filename in Directory.GetDirectories(
              Path.Join(ConfigFolder, "installed")
              )){
          ApkgOutput.MessageSuc1(Path.GetFileName(filename));
        }
      } catch (Exception e) {
        ApkgOutput.MessageErr1(e.Message);
      }
    } // }}}

    // remove() {{{
    private void apkg_do_remove() {
      if (_Arguments.Length < 3){
        ApkgOutput.MessageErr0("you need to provide a package name to remove");
        return;
      }
      Repository.RemovePackage(_Arguments[2]);
    } // }}}

    // list_avaiable() {{{
    private void apkg_do_list_available() {
      IList<string> list = Repository.AvailablePlugins();
      ApkgOutput.MessageSuc0("available packages:");
      foreach (string p in list) {
        ApkgOutput.MessageSuc1(p);
      }
    } // }}}
    
    // search() {{{
    private void apkg_do_search() {
      string keyword = _Arguments[2];
      IList<string> list = Repository.AvailablePlugins();
      ApkgOutput.MessageSuc0("results:");
      if (list.Length < 1) {
        ApkgOutput.MessageErr1("none");
        return;
      }
      foreach (string p in list) {
        if (p.ToLower().Contains(keyword.ToLower())) {
          ApkgOutput.MessageSuc1(p);
        }
      }
    } // }}}

    // info() {{{
    private void apkg_do_info() {
      ApkgOutput.MessageSuc0("apkg information");
      ApkgOutput.MessageSuc1(
          $"cache/download directory:      {Path.Join(data.DownloadPath, "apkg")}");
      ApkgOutput.MessageSuc1(
          $"plugin installation directory: {Path.Join(data.SavePath, "plugins"))}");
      ApkgOutput.MessageSuc1(
          $"share installation directory:  {Path.Join(data.SavePath, "share"))}");
      ApkgOutput.MessageSuc1(
          $"config/database directory:     {ConfigFolder}");
      ApkgOutput.MessageSuc1(
          $"docs directory:                {Path.Join(data.SavePath, "share", "docs", "apkg")}");
      ApkgOutput.MessageSuc1($"apkg version: {apkgVersion}");
      if (Config.DebugMode) {
        ApkgOutput.MessageWarn1("debug mode: ON");
      } else {
        ApkgOutput.MessageSuc1("debug mode: off");
      }
    } // }}}

    // reload() {{{
    private void apkg_do_reload() {
      try {
        Repository.Reload(Config.Repositories);
      } catch (Exception e) {
        ApkgOutput.MessageErr0("error reloading package database");
      }
    } // }}}

    // help() {{{
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
    update:         update packages");
      if (Config.DebugMode) {
        Console.WriteLine(@"
Available options in debug mode:
    build:         build plugin in directory
    get-local:     install local .lcpkg file");
      }
    } // }}}
    // }}}
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
