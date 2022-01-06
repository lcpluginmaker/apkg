using System.IO;
using System.Diagnostics;
using System.Net;
using ILeoConsole;
using ILeoConsole.Plugin;
using ILeoConsole.Core;

namespace LeoConsole_apkg
{
  public class apkg : ICommand
  {
    // ------- DEFAULT PLUGIN STUFF -------
    public string Name { get { return "apkg"; } }
    public string Description { get { return "advaned package management"; } }
    public Action CommandFunktion { get { return () => Command(); } }
    private string[] _InputProperties;
    public string[] InputProperties { get { return _InputProperties; } set { _InputProperties = value; } }

    public void Command() {
      if (_InputProperties.Length < 2) {
        Console.WriteLine("you need to provide an argument\n");
        apkg_do_help();
        return;
      }
      switch (_InputProperties[1]) {
        case "help":
          apkg_do_help();
          break;
        
        case "get":
          apkg_do_get();
          break;
        
        default:
          Console.WriteLine("apkg: unknown option " + _InputProperties[1]);
          break;
      }
    }

    // ------- APKG COMMANDS -------
    private void apkg_do_get() {
      if (_InputProperties.Length < 3){
        Console.WriteLine("you need to provide a package name or location");
        return;
      }
      if (_InputProperties[2].StartsWith("https://")){
        string url = _InputProperties[2];
        if (url.EndsWith(".git")) {
          Console.WriteLine("installing git repositories is not supported yet");
          return;
        }
        if (!url.EndsWith(".dll")) {
          Console.WriteLine("only downoading .dll files is supported");
          return;
        }
        string downloadPath = Path.Join("data", "tmp", "plugins", Path.GetFileName(url));
        if (!downloadFile(url, downloadPath)) {
          Console.WriteLine("error downloading");
          return;
        }
        Console.WriteLine("installing downloaded package...");
        try {
          File.Copy(downloadPath, Path.Join("data", "plugins", Path.GetFileName(downloadPath)), true);
        } catch (Exception e) {
          Console.WriteLine("error copying: " + e.Message);
          return;
        }
        Console.WriteLine(url + " was installed. restart LeoConsole to enable it.");
        return;
      }
      if (_InputProperties[2].StartsWith("file://")){
        string folder = _InputProperties[2].Substring(7, _InputProperties[2].Length-7);
        Console.WriteLine("installing " + folder + "...");
        if (!Directory.Exists(folder)) {
          Console.WriteLine(folder + "does not exist");
          return;
        }
        if (!compileFolder(folder)) {
          return;
        }
        Console.WriteLine("installing compiled plugin...");
        foreach (string filename in Directory.GetFiles(Path.Join(folder, "bin", "Debug", "net6.0"))){
          if (filename.EndsWith(".dll")){
            Console.WriteLine("copying " + filename + " to " + Path.Join("data", "plugins", Path.GetFileName(filename)) + "...");
            try {
              File.Copy(filename, Path.Join("data", "plugins", Path.GetFileName(filename)), true);
            } catch (Exception e) {
              Console.WriteLine("Error copying " + filename + ": " + e.Message);
              return;
            }
          }
        }
        Console.WriteLine(Path.GetFileName(folder) + " was installed. restart LeoConsole to load it");
        return;
      }
      Console.WriteLine("downloading normal plugins is not supported yet");
      Console.WriteLine("please use pkg get " + InputProperties[2]);
    }

    private void apkg_do_help() {
      Console.WriteLine("apkg is an advanced package tool for LeoConsole");
      Console.WriteLine("");
      Console.WriteLine("Besides the default functionality that the standard pkg command provides,");
      Console.WriteLine("it allows you to install plugins from unofficial repositories or even local folders,");
      Console.WriteLine("which is very handy for quick development and testing.");
      Console.WriteLine("");
      Console.WriteLine("Available options:");
      Console.WriteLine("    help: print this help");
      Console.WriteLine("    get:  install plugin from default repo <name>, git repo <https://*.git>, folder <file://*> or url <https://*.dll>");
      Console.WriteLine("");
      Console.WriteLine("Source code is available on https://github.com/alexcoder04/LeoConsole-apkg");
      Console.WriteLine("");
    }

    // ------- HELPER FUNCTIONS -------
    private bool downloadFile(string url, string location) {
      Console.WriteLine("downloading " + url + " to " + location + "...");
      try {
        WebClient webClient = new WebClient();
        webClient.DownloadFile(url, location);
      } catch (Exception e) {
        Console.WriteLine("error downloading: " + e.Message);
        return false;
      }
      return true;
    }

    private bool compileFolder(string folder) {
      Console.WriteLine("compiling " + folder + " with dotnet...");
      try {
        Process p = new Process();
        p.StartInfo.FileName = "dotnet";
        p.StartInfo.Arguments = "build";
        p.StartInfo.WorkingDirectory = folder;
        p.Start();
        p.WaitForExit();
        if (p.ExitCode != 0){
          Console.WriteLine("failed compiling: dotnet failed");
          return false;
        }
      } catch (Exception e) {
        Console.WriteLine("Error compiling: " + e.Message);
        return false;
      }
      Console.WriteLine("compiled successfully");
      return true;
    }
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab

