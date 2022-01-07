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
    public IData data = new ConsoleData();

    public void Command() {
      if (_InputProperties.Length < 2) {
        messageErr0("you need to provide an argument\n");
        apkg_do_help();
        return;
      }
      switch (_InputProperties[1]) {
        case "get": apkg_do_get(); break;
        case "help": apkg_do_help(); break;
        case "info": apkg_do_info(); break;
        case "list-available": apkg_do_list_available(); break;
        case "list-installed": apkg_do_list_installed(); break;
        case "search": apkg_do_search(); break;
        case "update": apkg_do_update(); break;
        default: messageErr0("apkg: unknown option " + _InputProperties[1]); break;
      }
    }

    // ------- APKG COMMANDS -------
    private void apkg_do_get() {
      if (_InputProperties.Length < 3){
        messageErr0("you need to provide a package name or location");
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
        messageErr0("only downoading .dll files or git repositories is supported");
        return;
      }
      if (_InputProperties[2].StartsWith("file://")){
        string folder = _InputProperties[2].Substring(7, _InputProperties[2].Length-7);
        get_file(folder);
        return;
      }
      messageErr0("currently plugins have no permission to call default LeoConsole functions");
      messageSuc1("run 'pkg get " + InputProperties[2] + "' instead");
    }

    private void apkg_do_update() {
      messageErr0("currently plugins have no permission to call default LeoConsole functions");
      messageSuc1("run 'pkg update' instead");
    }

    private void apkg_do_list_installed() {
      messageSuc0("your installed plugin files:");
      try {
        foreach (string filename in Directory.GetFiles(Path.Join(data.SavePath, "plugins"))){
          messageSuc1(filename);
        }
      } catch (Exception e) {
        messageErr1(e.Message);
      }
    }

    private void apkg_do_list_available() {
      if (!File.Exists(Path.Join(data.SavePath, "pkg", "PackageList.txt"))) {
        messageErr0("package database could not be found. try pkg update");
        return;
      }
      messageSuc0("available packages:");
      try {
        foreach (string line in File.ReadLines(Path.Join(data.SavePath, "pkg", "PackageList.txt"))) {
          messageSuc1(line.Split(" ")[1].Split(":")[1]);
        }
      } catch (Exception e) {
        messageErr1(e.Message);
        return;
      }
    }
    
    private void apkg_do_search() {
      if (_InputProperties.Length < 3){
        messageErr0("you need to provide a package name or location");
        return;
      }
      if (!File.Exists(Path.Join(data.SavePath, "pkg", "PackageList.txt"))) {
        messageErr0("package database could not be found. try pkg update");
        return;
      }
      string keyword = _InputProperties[2];
      messageSuc0("results:");
      try {
        foreach (string line in File.ReadLines(Path.Join(data.SavePath, "pkg", "PackageList.txt"))) {
          string pkgName = line.Split(" ")[1].Split(":")[1];
          if (pkgName.ToLower().Contains(keyword.ToLower())) {
            messageSuc1(pkgName);
          }
        }
      } catch (Exception e) {
        messageErr1(e.Message);
        return;
      }
    }

    private void apkg_do_info() {
      messageSuc0("apkg plugin information");
      messageSuc1("cache/download directory: " + Path.Join(data.DownloadPath, "plugins"));
      messageSuc1("installation directory: " + Path.Join(data.SavePath, "plugins"));
    }

    private void apkg_do_help() {
      Console.WriteLine("apkg is an advanced package tool for LeoConsole");
      Console.WriteLine("");
      Console.WriteLine("Besides the default functionality that the standard pkg command provides,");
      Console.WriteLine("it allows you to install plugins from unofficial repositories or even local folders,");
      Console.WriteLine("which is very handy for quick development and testing.");
      Console.WriteLine("");
      Console.WriteLine("Available options:");
      Console.WriteLine("    get:            install plugin from default repo <name>, git repo <https://*.git>, folder <file://*> or url <https://*.dll>");
      Console.WriteLine("    help:           print this help");
      Console.WriteLine("    info:           print where the plugins are downloaded and installed to");
      Console.WriteLine("    list-available: list plugins available in the default pkg repo");
      Console.WriteLine("    list-installed: list installed .dll plugin files");
      Console.WriteLine("    search:         search for a package in the default repos");
      Console.WriteLine("    update:         update package database");
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
      messageSuc0("installing from local folder...");
      if (!Directory.Exists(folder)) {
        messageErr1("" + folder + "does not exist");
        return;
      }
      if (!compileFolder(folder)) {
        return;
      }
      if (!install_dlls(folder)) {
        return;
      }
      messageSuc0("" + folder + " was installed. restart LeoConsole to load it");
    }

    // compiled dll from http
    private void get_http(string url) {
      messageSuc0("installing compiled dll file");
      string downloadPath = Path.Join(data.DownloadPath, "plugins", Path.GetFileName(url));
      if (!createPluginsDownloadDir()) {
        return;
      }
      if (!downloadFile(url, downloadPath)) {
        return;
      }
      messageSuc0("installing downloaded dll...");
      try {
        File.Copy(downloadPath, Path.Join(data.SavePath, "plugins", Path.GetFileName(downloadPath)), true);
      } catch (Exception e) {
        messageErr1("cannot instal: " + e.Message);
        return;
      }
      messageSuc0("" + url + " was installed. restart LeoConsole to enable it.");
    }

    // compiling from git repository
    private void get_git(string url) {
      messageSuc0("installing from git repository");
      string name = Path.GetFileName(url).Split(".")[0];
      try {
        if (Directory.Exists(Path.Join(data.DownloadPath, "plugins", name))) {
          messageSuc1("download directory already exists. override [y/n] ? ");
          string answer = Console.ReadLine();
          switch (answer) {
            case "y":
              if (!deleteDirectory(Path.Join(data.DownloadPath, "plugins", name))) {
                return;
              }
              break;
            case "Y":
              if (!deleteDirectory(Path.Join(data.DownloadPath, "plugins", name))) {
                return;
              }
              break;
            default:
              messageSuc1("operation aborted");
              return;
              break;
          }
        }
      } catch (Exception e) {
        messageErr1("cannot check download location: " + e.Message);
        return;
      }
      if (!gitClone(url, Path.Join(data.DownloadPath, "plugins"), name)) {
        return;
      }
      if (!compileFolder(Path.Join(data.DownloadPath, "plugins", name))) {
        return;
      }
      if (!install_dlls(Path.Join(data.DownloadPath, "plugins", name))) {
        return;
      }
      messageSuc0("" + url + " was installed. restart LeoConsole to load it");
      return;
    }

    // MORE LOW-LEVEL HELPER FUNCTIONS
    // download a file to given location
    private bool downloadFile(string url, string location) {
      messageSuc0("downloading " + url + " to " + location + "...");
      try {
        WebClient webClient = new WebClient();
        webClient.DownloadFile(url, location);
      } catch (Exception e) {
        messageErr1("cannot download: " + e.Message);
        return false;
      }
      return true;
    }

    // install all dlls from bin subfolder of a repository
    private bool install_dlls(string from_folder) {
      messageSuc0("installing dlls from " + from_folder + "...");
      try {
        foreach (string filename in Directory.GetFiles(Path.Join(from_folder, "bin", "Debug", "net6.0"))){
          if (filename.EndsWith(".dll")){
            messageSuc1("copying " + filename + " to " + Path.Join(data.SavePath, "plugins", Path.GetFileName(filename)) + "...");
            File.Copy(filename, Path.Join(data.SavePath, "plugins", Path.GetFileName(filename)), true);
          }
        }
      } catch (Exception e) {
        messageErr1("cannot install: " + e.Message);
        return false;
      }
      return true;
    }

    // clone a repository to given location
    private bool gitClone(string url, string parent, string folder) {
      messageSuc0("cloning " + url + " to " + parent + "/" + folder + "...");
      if (!createPluginsDownloadDir()) {
        return false;
      }
      if (!runProcess("git", "clone " + url + " " + folder, parent)) {
        return false;
      }
      messageSuc1("cloned repo successfully");
      return true;
    }

    // compile a repository with dotnet
    private bool compileFolder(string folder) {
      messageSuc0("compiling " + folder + " with dotnet...");
      if (!runProcess("dotnet", "build", folder)) {
        return false;
      }
      messageSuc1("compiled " + folder + " successfully");
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
          messageErr1("" + name + " returned an error");
          return false;
        }
      } catch (Exception e) {
        messageErr1("cannot run " + name + ": " + e.Message);
        return false;
      }
      return true;
    }

    private bool createPluginsDownloadDir() {
      if (Directory.Exists(Path.Join(data.DownloadPath, "plugins"))) {
        return true;
      }
      try {
        Directory.CreateDirectory(Path.Join(data.DownloadPath, "plugins"));
      } catch (Exception e) {
        messageErr1("cannot create plugins download dir: " + e.Message);
        return false;
      }
      messageSuc1("created plugins download directory");
      return true;
    }

    private bool deleteDirectory(string folder) {
      try {
        Directory.Delete(folder, true);
      } catch (Exception e) {
        messageErr1("cannot delete " + folder + ": " + e.Message);
        return false;
      }
      messageSuc1("" + folder + " deleted");
      return true;
    }

    // PRINTING NICE MESSAGES
    private void messageSuc0(string msg) {
      writeColoredLine(" §a=>§r " + msg);
    }

    private void messageSuc1(string msg) {
      writeColoredLine("   §a->§r " + msg);
    }

    private void messageErr0(string msg) {
      writeColoredLine(" §c=>§r error: " + msg);
    }

    private void messageErr1(string msg) {
      writeColoredLine("   §c->§r error: " + msg);
    }

    // TODO only temporary, until plugins have access to LeoConsole's native funtion for thar
    private void writeColoredLine(string value) {
        Char[] chars = value.ToCharArray();
        for (int i = 0; i < chars.Length; i++) {
            switch (chars[i]) {
                case '§':
                    switch (chars[i + 1]) {
                        case '0': Console.ForegroundColor = ConsoleColor.Black; i++; break;
                        case '1': Console.ForegroundColor = ConsoleColor.DarkBlue; i++; break;
                        case '2': Console.ForegroundColor = ConsoleColor.DarkGreen; i++; break;
                        case '3': Console.ForegroundColor = ConsoleColor.DarkCyan; i++; break;
                        case '4': Console.ForegroundColor = ConsoleColor.DarkRed; i++; break;
                        case '5': Console.ForegroundColor = ConsoleColor.DarkMagenta; i++; break;
                        case '6': Console.ForegroundColor = ConsoleColor.DarkYellow; i++; break;
                        case '7': Console.ForegroundColor = ConsoleColor.Gray; i++; break;
                        case '8': Console.ForegroundColor = ConsoleColor.DarkGray; i++; break;
                        case '9': Console.ForegroundColor = ConsoleColor.Blue; i++; break;
                        case 'a': Console.ForegroundColor = ConsoleColor.Green; i++; break;
                        case 'b': Console.ForegroundColor = ConsoleColor.Cyan; i++; break;
                        case 'c': Console.ForegroundColor = ConsoleColor.Red; i++; break;
                        case 'd': Console.ForegroundColor = ConsoleColor.Magenta; i++; break;
                        case 'e': Console.ForegroundColor = ConsoleColor.Yellow; i++; break;
                        case 'f': Console.ForegroundColor = ConsoleColor.White; i++; break;
                        case 'r': Console.ResetColor(); i++; break;
                        default: break;
                    } break;
                default: Console.Write(chars[i]); break;
            }
        }
        Console.Write("\n"); Console.ResetColor();
    }
  }
}

// keep this, this is the most important thing
// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab

