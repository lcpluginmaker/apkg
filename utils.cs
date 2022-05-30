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
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
