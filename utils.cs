using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace LeoConsole_apkg {
  public class ApkgUtils {
    // delete directory
    public static bool DeleteDirectory(string folder) {
      try {
        Directory.Delete(folder, true);
      } catch (Exception e) {
        ApkgOutput.MessageErr1("cannot delete " + folder + ": " + e.Message);
        return false;
      }
      ApkgOutput.MessageSuc1("" + folder + " deleted");
      return true;
    }

    // get running os
    public static string GetRunningOS() {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
        return "win64";
      }
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
        return "lnx64";
      }
      return "other";
    }

    // get path of the apkg-builder binary
    public static string GetBuilderPath(string savePath) {
      if (GetRunningOS() == "lnx64") {
        string path = Path.Join(savePath, "share", "scripts", "apkg-build-lnx64");
        if (File.Exists(path)) {
          return path;
        }
        throw new Exception("builder binary not found");
      }
      if (GetRunningOS() == "win64") {
        string path = Path.Join(savePath, "share", "scripts", "apkg-build-win64.exe");
        if (File.Exists(path)) {
          return path;
        }
        throw new Exception("builder binary not found");
      }
      throw new Exception("unknown OS");
    }

    // run a process with parameters and wait for it to finish
    public static bool RunProcess(string name, string args, string pwd) {
      try {
        Process p = new Process();
        p.StartInfo.FileName = name;
        p.StartInfo.Arguments = args;
        p.StartInfo.WorkingDirectory = pwd;
        p.Start();
        p.WaitForExit();
        if (p.ExitCode != 0) {
          ApkgOutput.MessageErr1("" + name + " returned an error");
          return false;
        }
      } catch (Exception e) {
        ApkgOutput.MessageErr1("cannot run " + name + ": " + e.Message);
        return false;
      }
      return true;
    }

    // download a file to given location
    public static bool DownloadFile(string url, string location) {
      ApkgOutput.MessageSuc1("downloading " + url + " to " + location + "...");
      try {
        WebClient webClient = new WebClient();
        webClient.DownloadFile(url, location);
      } catch (Exception e) {
        ApkgOutput.MessageErr1("cannot download: " + e.Message);
        return false;
      }
      return true;
    }

    // compare two semantic versions
    public static bool VersionGreater(string v1, string v2) {
      string[] v1a = v1.Split(".");
      string[] v2a = v2.Split(".");
      for (int i = 0; i < 2; i++) {
        if (int.Parse(v1a[i]) > int.Parse(v2a[i])) {
          return true;
        }
        if (int.Parse(v1a[i]) < int.Parse(v2a[i])) {
          return false;
        }
      }
      return false;
    }

    public static void PrintCopyright(){
      Console.WriteLine(@"
Source code is available on <https://github.com/alexcoder04/LeoConsole-apkg>

LeoConsole-apkg-plugin Copyright (c) 2022 alexcoder04
This program comes with ABSOLUTELY NO WARRANTY.
This is free software, and you are welcome to redistribute it
under certain conditions, see <https://www.gnu.org/licenses/gpl-3.0.txt>
for more details.
");
    }

    public static void FirstRun() {
      PrintCopyright();
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
