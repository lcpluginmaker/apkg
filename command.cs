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

        case "list-installed":
          apkg_do_list_installed();
          break;
        
        default:
          Console.WriteLine("apkg: unknown option " + _InputProperties[1]);
          break;
      }
    }

    // ------- APKG COMMANDS -------
    private void apkg_do_get() {
      if (_InputProperties.Length < 3){
        Console.WriteLine(" => error: you need to provide a package name or location");
        return;
      }
      if (_InputProperties[2].StartsWith("https://")){
        string url = _InputProperties[2];
        if (url.EndsWith(".git")) {
          get_git(url);
          return;
        }
        if (url.EndsWith(".dll")) {
          get_http(url);
          return;
        }
        Console.WriteLine(" => error: only downoading .dll files or git repositories is supported");
        return;
      }
      if (_InputProperties[2].StartsWith("file://")){
        string folder = _InputProperties[2].Substring(7, _InputProperties[2].Length-7);
        get_file(folder);
        return;
      }
      Console.WriteLine(" => downloading normal plugins is not supported yet");
      Console.WriteLine("   -> please use pkg get " + InputProperties[2]);
    }

    private void apkg_do_list_installed() {
      Console.WriteLine("=> your installed plugin files:");
      try {
        foreach (string filename in Directory.GetFiles(Path.Join("data", "plugins"))){
          Console.WriteLine("   -> " + filename);
        }
      } catch (Exception e) {
        Console.WriteLine("   -> error: " + e.Message);
      }
    }

    private void apkg_do_help() {
      Console.WriteLine("apkg is an advanced package tool for LeoConsole");
      Console.WriteLine("");
      Console.WriteLine("Besides the default functionality that the standard pkg command provides,");
      Console.WriteLine("it allows you to install plugins from unofficial repositories or even local folders,");
      Console.WriteLine("which is very handy for quick development and testing.");
      Console.WriteLine("");
      Console.WriteLine("Available options:");
      Console.WriteLine("    help:           print this help");
      Console.WriteLine("    get:            install plugin from default repo <name>, git repo <https://*.git>, folder <file://*> or url <https://*.dll>");
      Console.WriteLine("    list-installed: list installed .dll plugin files");
      Console.WriteLine("");
      Console.WriteLine("Source code is available on <https://github.com/alexcoder04/LeoConsole-apkg>");
      Console.WriteLine("");
      Console.WriteLine("LeoConsole-apkg-plugin Copyright (C) 2022 alexcoder04");
      Console.WriteLine("This program comes with ABSOLUTELY NO WARRANTY.");
      Console.WriteLine("This is free software, and you are welcome to redistribute it");
      Console.WriteLine("under certain conditions, see <https://www.gnu.org/licenses/gpl-3.0.txt> for more details.");
      Console.WriteLine("");
    }

    // ------- HELPER FUNCTIONS -------
    // DIFFERENT TYPES OF INSTALLATION
    // from local folder
    private void get_file(string folder) {
      Console.WriteLine(" => installing from local folder...");
      if (!Directory.Exists(folder)) {
        Console.WriteLine("   -> error: " + folder + "does not exist");
        return;
      }
      if (!compileFolder(folder)) {
        return;
      }
      if (!install_dlls(folder)) {
        return;
      }
      Console.WriteLine(" => " + folder + " was installed. restart LeoConsole to load it");
    }

    // compiled dll from http
    private void get_http(string url) {
      Console.WriteLine(" => installing compiled dll file");
      string downloadPath = Path.Join("data", "tmp", "plugins", Path.GetFileName(url));
      if (!downloadFile(url, downloadPath)) {
        return;
      }
      Console.WriteLine(" => installing downloaded dll...");
      try {
        File.Copy(downloadPath, Path.Join("data", "plugins", Path.GetFileName(downloadPath)), true);
      } catch (Exception e) {
        Console.WriteLine("   -> error installing: " + e.Message);
        return;
      }
      Console.WriteLine(" => " + url + " was installed. restart LeoConsole to enable it.");
    }

    // compiling from git repository
    private void get_git(string url) {
      Console.WriteLine(" => installing from git repository");
      string name = Path.GetFileName(url).Split(".")[0];
      if (!gitClone(url, Path.Join("data", "tmp", "plugins"), name)) {
        return;
      }
      if (!compileFolder(Path.Join("data", "tmp", "plugins", name))) {
        return;
      }
      if (!install_dlls(Path.Join("data", "tmp", "plugins", name))) {
        return;
      }
      Console.WriteLine(" => " + url + " was installed. restart LeoConsole to load it");
      return;
    }

    // MORE LOW-LEVEL HELPER FUNCTIONS
    // download a file to given location
    private bool downloadFile(string url, string location) {
      Console.WriteLine(" => downloading " + url + " to " + location + "...");
      try {
        WebClient webClient = new WebClient();
        webClient.DownloadFile(url, location);
      } catch (Exception e) {
        Console.WriteLine("   -> error downloading: " + e.Message);
        return false;
      }
      return true;
    }

    // install all dlls from bin subfolder of a repository
    private bool install_dlls(string from_folder) {
      Console.WriteLine(" => installing dlls from " + from_folder + "...");
      try {
        foreach (string filename in Directory.GetFiles(Path.Join(from_folder, "bin", "Debug", "net6.0"))){
          if (filename.EndsWith(".dll")){
            Console.WriteLine("   -> copying " + filename + " to " + Path.Join("data", "plugins", Path.GetFileName(filename)) + "...");
            File.Copy(filename, Path.Join("data", "plugins", Path.GetFileName(filename)), true);
          }
        }
      } catch (Exception e) {
        Console.WriteLine("   -> error installing: " + e.Message);
        return false;
      }
      return true;
    }

    // clone a repository to given location
    private bool gitClone(string url, string parent, string folder) {
      Console.WriteLine(" => cloning " + url + " to " + parent + "/" + folder + "...");
      if (!runProcess("git", "clone " + url + " " + folder, parent)) {
        return false;
      }
      Console.WriteLine("   -> cloned repo successfully");
      return true;
    }

    // compile a repository with dotnet
    private bool compileFolder(string folder) {
      Console.WriteLine(" => compiling " + folder + " with dotnet...");
      if (!runProcess("dotnet", "build", folder)) {
        return false;
      }
      Console.WriteLine("   -> compiled " + folder + " successfully");
      return true;
    }

    // run a process with parameters and wait for it to finish
    private bool runProcess(string name, string args, string pwd) {
      try {
        Process p = new Process();
        p.StartInfo.FileName = name;
        p.StartInfo.Arguments = args;
        p.StartInfo.WorkingDirectory = pwd;
        p.Start();
        p.WaitForExit();
        if (p.ExitCode != 0) {
          Console.WriteLine("   -> " + name + " returned an error");
          return false;
        }
      } catch (Exception e) {
        Console.WriteLine("   -> error running " + name + ": " + e.Message);
        return false;
      }
      return true;
    }
  }
}

// keep this, this is the most important thing
// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab

