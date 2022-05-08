using ILeoConsole.Core;
using ILeoConsole.Plugin;
using ILeoConsole;
using System.IO;
using System.Reflection;

namespace LeoConsole_apkg {
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
  
  public class ApkgPlugin : IPlugin {
    public string Name { get { return "apkg"; } }
    public string Explanation { get { return "advanced package tool (apt lol)"; } }
    
    private IData _data;
    public IData data { get { return _data; } set { _data = value; } }
    
    private List<ICommand> _Commands;
    public List<ICommand> Commands { get { return _Commands; } set { _Commands = value; } }

    public void PluginInit(){
      _data = new ConsoleData();
      
      _Commands = new List<ICommand>();
      _Commands.Add(new LeoConsoleApkgCommand());
    }
    
    public void PluginMain() {
      ApkgOutput output = new ApkgOutput();
      output.MessageSuc0("performing apkg self-check");
      string[] folders = {
        Path.Join(data.SavePath, "var"),
        Path.Join(data.SavePath, "var", "apkg"),
        Path.Join(data.SavePath, "var", "apkg", "installed"),
      };
      foreach (string folder in folders) {
        if (!Directory.Exists(folder)) {
          try {
            Directory.CreateDirectory(folder);
          } catch (Exception e) {
            output.MessageErr1("cannot create var dir: " + e.Message);
            return;
          }
        }
      }
      string reposListFile = Path.Join(data.SavePath, "var", "apkg", "repos");
      if (!File.Exists(reposListFile)) {
        ApkgRepository repository = new ApkgRepository();
        ApkgUtils utils = new ApkgUtils();
        // enable test repository by default
        string[] lines = {"https://raw.githubusercontent.com/alexcoder04/LeoConsole-apkg-repo-test/main/index.json"};
        using (StreamWriter f = new StreamWriter(reposListFile)) {
          foreach (string line in lines) {
            f.WriteLine(line);
          }
        }
        try {
          repository.Reload(data.SavePath);
        } catch (Exception e) {
          output.MessageErr0("error reloading package database");
          output.MessageErr0("something seems to be broken, you can not use apkg");
          return;
        }
        string url;
        try {
          url = repository.GetUrlFor("apkg", data.SavePath);
        } catch (Exception e) {
          output.MessageErr1("apkg not found in repository");
          output.MessageErr0("something seems to be broken, you can not use apkg");
          return;
        }
        string dlPath = Path.Join(data.SavePath, "tmp", "package.lcpkg");
        if (!utils.DownloadFile(url, dlPath)) {
          return;
        }
        repository.InstallLcpkg(dlPath, data.SavePath);
      }
      output.MessageSuc1("self-check successfull");
    }

    public void PluginShutdown(){
    }
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
