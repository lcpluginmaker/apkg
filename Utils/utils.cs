using ILeoConsole.Core;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace LeoConsole_apkg {
  public class ApkgUtils {
    // GetRunningOS() {{{
    public static string GetRunningOS() {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
        return "win64";
      }
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
        return "lnx64";
      }
      return "other";
    } // }}}

    // GetBuilderPath() {{{
    public static string GetBuilderPath(string savePath) {
      string basePath = Path.Join(savePath, "share", "apkg", "bin");
      if (GetRunningOS() == "lnx64") {
        string path = Path.Join(basePath, "apkg-build-lnx64");
        if (File.Exists(path)) {
          return path;
        }
        throw new Exception("builder binary not found");
      }
      if (GetRunningOS() == "win64") {
        string path = Path.Join(basePath, "apkg-build-win64.exe");
        if (File.Exists(path)) {
          return path;
        }
        throw new Exception("builder binary not found");
      }
      throw new Exception("unknown OS");
    } // }}}

    // DownloadFile() {{{
    public static bool DownloadFile(string url, string location) {
      LConsole.MessageSuc1($"downloading {url} to {location}...");
      try {
        WebClient webClient = new WebClient();
        webClient.DownloadFile(url, location);
      } catch (Exception e) {
        LConsole.MessageErr1("cannot download: " + e.Message);
        return false;
      }
      return true;
    } // }}}

    // VersionGreater() {{{
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
    } // }}}

    // PrintCopyright() {{{
    public static void PrintCopyright(){
      Console.WriteLine(@"
Source code is available on <https://github.com/lcpluginmaker/apkg>

LeoConsole-apkg-plugin Copyright (c) 2022 alexcoder04
This program comes with ABSOLUTELY NO WARRANTY.
This is free software, and you are welcome to redistribute it
under certain conditions, see <https://www.gnu.org/licenses/gpl-3.0.txt>
for more details.
");
    } // }}}

    // FirstRun() {{{
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
    } // }}}
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
