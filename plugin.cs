﻿using ILeoConsole.Core;
using ILeoConsole.Plugin;
using ILeoConsole;
using System.IO;
using System.Reflection;

namespace LeoConsole_apkg {
  // IData {{{
  public class ConsoleData : IData {
    public static User _User;
    public User User { get { return _User; } set { _User = value; } }
    public static string _SavePath;
    public string SavePath { get { return _SavePath; } set { _SavePath = value; } }
    public static string _DownloadPath;
    public string DownloadPath { get { return _DownloadPath; } set { _DownloadPath = value; } }
    public static string _Version;
    public string Version { get { return _Version; } set { _Version = value; } }
    public static string _CurrentWorkingPath;
    public string CurrentWorkingPath { get { return _CurrentWorkingPath; } set { _CurrentWorkingPath = value; } }
  }
  // }}}
  
  public class ApkgPlugin : IPlugin {
    // default stuff {{{
    public string Name { get { return "apkg"; } }
    public string Explanation { get { return "advanced package manager"; } }
    
    private IData _data;
    public IData data { get { return _data; } set { _data = value; } }
    
    private List<ICommand> _Commands;
    public List<ICommand> Commands { get { return _Commands; } set { _Commands = value; } }
    // }}}

    public void PluginInit(){
      _data = new ConsoleData();
      _Commands = new List<ICommand>();
    }

    public void RegisterCommands(){
      _Commands.Add(new LeoConsoleApkgCommand(data.SavePath, data.Version));
    }
    
    public void PluginMain() {
      LConsole.MessageSuc0("performing apkg self-check");
      // check if folders exist {{{
      string[] folders = {
        Path.Join(data.SavePath, "var"),
        Path.Join(data.SavePath, "var", "apkg"),
        Path.Join(data.SavePath, "var", "apkg", "installed"),
        Path.Join(data.SavePath, "var", "apkg", "repos-index"),
        Path.Join(data.DownloadPath, "apkg"),
      };
      foreach (string folder in folders) {
        if (!Directory.Exists(folder)) {
          try {
            Directory.CreateDirectory(folder);
          } catch (Exception e) {
            LConsole.MessageErr1("cannot create apkg dir: " + e.Message);
            LConsole.MessageErr0("something seems to be broken, you can not use apkg");
            return;
          }
        }
      }
      // }}}

      // install itself if not installed {{{
      if (!Directory.Exists(Path.Join(data.SavePath, "var", "apkg", "installed", "apkg"))) {
        ApkgConfig config = ApkgConfigHelper.ReadConfig(Path.Join(data.SavePath, "var", "apkg"));
        ApkgRepository repository = new ApkgRepository(data.SavePath, data.Version, false);
        // reload
        try {
          repository.Reload(config.Repositories);
        } catch (Exception e) {
          LConsole.MessageErr0("error reloading package database");
          LConsole.MessageErr0("something seems to be broken, you can not use apkg");
          return;
        }
        // re-install itself
        string url;
        try {
          url = repository.GetUrlFor("apkg");
        } catch (Exception e) {
          LConsole.MessageErr1("apkg not found in repository");
          LConsole.MessageErr0("something seems to be broken, you can not use apkg");
          return;
        }
        string dlPath = Path.Join(data.SavePath, "tmp", "apkg.lcp");
        if (!ApkgUtils.DownloadFile(url, dlPath)) {
          LConsole.MessageErr1("could not download apkg");
          LConsole.MessageErr0("something seems to be broken, you can not use apkg");
          return;
        }
        repository.InstallLcpkg(dlPath);
      }
      LConsole.MessageSuc1("self-check successfull");
    }
    // }}}

    public void PluginShutdown() { }
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
