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

    private const string apkgVersion="2.0.0";
    private string ConfigFolder;
    // }}}

    public LeoConsoleApkgCommand(string savePath, string lcVersion) {
      ConfigFolder = Path.Join(savePath, "var", "apkg");
      Config = ApkgConfigHelper.ReadConfig(ConfigFolder);
      Repository = new ApkgRepository(savePath, lcVersion, Config.DebugMode);
    }

    // Command() {{{
    public void Command() {
      if (_Arguments.Length < 2) {
        LConsole.MessageErr0("you need to provide a subcommand\n");
        apkg_do_help();
        return;
      }

      switch (_Arguments[1]) {
        case "b": case "build": case "compile": apkg_do_build(); break;
        case "d": case "remove": case "uninstall": case "delete": apkg_do_remove(); break;
        case "g": case "get": case "install": apkg_do_get(); break;
        case "gl": case "get-local": apkg_do_get_local(); break;
        case "h": case "help": apkg_do_help(); break;
        case "i": case "info": apkg_do_info(); break;
        case "la": case "list-available": apkg_do_list_available(); break;
        case "li": case "list-installed": apkg_do_list_installed(); break;
        case "r": case "reload": apkg_do_reload(); break;
        case "s": case "search": apkg_do_search(); break;
        case "u": case "update": apkg_do_update(); break;
        default:
          LConsole.MessageErr0("apkg: unknown subcommand '" + _Arguments[1] + "'");
          break;
      }
    }
    // }}}

    // APKG COMMANDS {{{
    // get() {{{
    private void apkg_do_get() {
      if (_Arguments.Length < 3){
        LConsole.MessageErr0("you need to provide a package name");
        return;
      }
      string url;
      try {
        url = Repository.GetUrlFor(_Arguments[2]);
      } catch (Exception e) {
        LConsole.MessageErr1("cannot find your package");
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
        LConsole.MessageErr0("this command is only available in debug mode"); return;
      }
      if (_Arguments.Length < 3) {
        LConsole.MessageErr0("you need to pass a file or folder to install");
        return;
      }
      if (_Arguments[2].StartsWith("~")) {
        _Arguments[2] = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), _Arguments[2].Trim("~".ToCharArray()));
      }
      if (Directory.Exists(_Arguments[2])) {
        if (Processes.Run(
            ApkgUtils.GetBuilderPath(data.SavePath),
            _Arguments[2],
            data.CurrentWorkingPath
            ) != 0) {
          LConsole.MessageErr0("error building plugin");
          return;
        }
        foreach (string f in Directory.GetFiles(_Arguments[2])) {
          if (f.EndsWith(".lcp")) {
            LConsole.MessageSuc0("installing built archive: " + f);
            Repository.InstallLcpkg(f);
            return;
          }
        }
        return;
      }
      if (!_Arguments[2].EndsWith(".lcp")) {
        LConsole.MessageErr0("looks not like an lcp file");
        return;
      }
      Repository.InstallLcpkg(_Arguments[2]);
    } // }}}

    // build() {{{
    private void apkg_do_build() {
      if (!Config.DebugMode) {
        LConsole.MessageErr0("this command is only available in debug mode");
        return;
      }
      if (_Arguments.Length < 3) {
        LConsole.MessageErr0("you need to pass a folder to build");
        return;
      }
      if (!Directory.Exists(_Arguments[2])) {
        LConsole.MessageErr0("the folder you passed doesn't exists");
        return;
      }
      Processes.Run(ApkgUtils.GetBuilderPath(data.SavePath), _Arguments[2], data.CurrentWorkingPath);
    } // }}}

    // update() {{{
    private void apkg_do_update() {
      IList<string> allPlugins = Repository.AvailablePlugins();
      LConsole.MessageSuc0("updating plugins:");
      foreach (string p in allPlugins) {
        if (!Directory.Exists(Path.Join(data.SavePath, "var", "apkg", "installed", p))) {
          continue;
        }
        string installedVersion = File.ReadAllText(Path.Join(data.SavePath, "var", "apkg", "installed", p, "version")).Trim();
        string availableVersion = Repository.GetInfoFor(p).version;
        if (ApkgUtils.VersionGreater(availableVersion, installedVersion)) {
          LConsole.MessageSuc1($"update available for {p} (v{installedVersion} -> v{availableVersion})");
          string url = Repository.GetUrlFor(_Arguments[2]);
          string dlPath = Path.Join(data.DownloadPath, "apkg", $"{_Arguments[2]}.lcp");
          if (!ApkgUtils.DownloadFile(url, dlPath)) {
            return;
          }
          Repository.InstallLcpkg(dlPath);
          continue;
        }
        if (ApkgUtils.VersionGreater(installedVersion, availableVersion)) {
          LConsole.MessageWarn1($"{p} is newer than repository (installed {installedVersion}, repo {availableVersion})");
          continue;
        }
        LConsole.MessageSuc1($"{p} is up-to-date");
      }
    } // }}}

    // list_installed() {{{
    private void apkg_do_list_installed() {
      LConsole.MessageSuc0("your installed packages:");
      foreach (string filename in Directory.GetDirectories(
            Path.Join(ConfigFolder, "installed")
            )){
        string p = Path.GetFileName(filename);
        try {
          RepoPackage info = Repository.GetInfoFor(p);
          LConsole.MessageSuc1($"{p} - {info.description}");
        } catch (Exception e) {
          LConsole.MessageErr1($"{p} (not found in remote)");
        }
      }
    } // }}}

    // remove() {{{
    private void apkg_do_remove() {
      if (_Arguments.Length < 3){
        LConsole.MessageErr0("you need to provide a package name to remove");
        return;
      }
      Repository.RemovePackage(_Arguments[2]);
    } // }}}

    // list_avaiable() {{{
    private void apkg_do_list_available() {
      IList<string> list = Repository.AvailablePlugins();
      LConsole.MessageSuc0("available packages:");
      foreach (string p in list) {
        RepoPackage info = Repository.GetInfoFor(p);
        LConsole.MessageSuc1($"{p} - {info.description}");
      }
    } // }}}
    
    // search() {{{
    private void apkg_do_search() {
      string keyword = _Arguments[2];
      IList<string> list = Repository.AvailablePlugins();
      LConsole.MessageSuc0("results:");
      IList<string> matches = Enumerable.Empty<string>().ToList();
      foreach (string p in list) {
        if (p.ToLower().Contains(keyword.ToLower())) {
          matches.Add(p);
        }
      }
      if (matches.Count < 1) {
        LConsole.MessageErr1("none");
        return;
      }
      foreach (string p in matches) {
        LConsole.MessageSuc1(p);
      }
    } // }}}

    // info() {{{
    private void apkg_do_info() {
      LConsole.MessageSuc0("apkg information");
      LConsole.MessageSuc1(
          $"cache/download directory:      {Path.Join(data.DownloadPath, "apkg")}");
      LConsole.MessageSuc1(
          $"plugin installation directory: {Path.Join(data.SavePath, "plugins")}");
      LConsole.MessageSuc1(
          $"share installation directory:  {Path.Join(data.SavePath, "share")}");
      LConsole.MessageSuc1(
          $"config/database directory:     {ConfigFolder}");
      LConsole.MessageSuc1(
          $"docs directory:                {Path.Join(data.SavePath, "share", "docs", "apkg")}");
      LConsole.MessageSuc1($"apkg version: {apkgVersion}");
      if (Config.DebugMode) {
        LConsole.MessageWarn1("debug mode: ON");
      } else {
        LConsole.MessageSuc1("debug mode: off");
      }
    } // }}}

    // reload() {{{
    private void apkg_do_reload() {
      try {
        Repository.Reload(Config.Repositories);
      } catch (Exception e) {
        LConsole.MessageErr0("error reloading package database");
      }
    } // }}}

    // help() {{{
    private void apkg_do_help() {
      Console.WriteLine(@"
apkg is an advanced package tool for LeoConsole

For more detailed documentation see '$SAVEPATH/share/docs/apkg'

Available options:
    g(et)/install:             install package
    h(elp):                    print this help
    i(nfo):                    print where the plugins are downloaded and installed to
    l(ist-)a(vailable):        list available plugins
    l(ist-)i(nstalled):        list installed plugins
    d(elete)/remove/uninstall: remove package
    s(earch):                  search for a package
    r(eload):                  reload package database
    u(pdate):                  update packages");

      if (Config.DebugMode) {
        Console.WriteLine(@"
Available options in debug mode:
    b(uild)/compile:           build plugin in directory
    g(et-)l(ocal):             install local .lcp file");
      }
    } // }}}
    // }}}
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
