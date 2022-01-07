using System.IO;
using ILeoConsole;
using ILeoConsole.Plugin;
using ILeoConsole.Core;

namespace LeoConsole_apkg
{

  public class ConsoleData : IData
  {
    public static User _User;
    public User User { get { return _User; } set { _User = value; } }
    public static string _SavePath;
    public string SavePath { get { return _SavePath; } set { _SavePath = value; } }
    public static string _DownloadPath;
    public string DownloadPath { get { return _DownloadPath; } set { _DownloadPath = value; } }
    public string Version { get { return "?"; } } // this will be removed soon
  }
  
  public class ApkgPlugin : IPlugin
  {
    public string Name { get { return "apkg"; } }
    public string Explanation { get { return "advanced package tool (apt lol)"; } }
    
    private IData _data;
    public IData data { get { return _data; } set { _data = value; } }
    
    private List<ICommand> _Commands;
    public List<ICommand> Commands { get { return _Commands; } set { _Commands = value; } }
    
    public void PluginMain() {
      _data = new ConsoleData();
      
      _Commands = new List<ICommand>();
      _Commands.Add(new apkg());
    }
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
