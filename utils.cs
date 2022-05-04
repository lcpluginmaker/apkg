using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.Json;

namespace LeoConsole_apkg {
  public class ApkgUtils {
    private ApkgOutput output = new ApkgOutput();

    // delete directory
    public bool DeleteDirectory(string folder) {
      try {
        Directory.Delete(folder, true);
      } catch (Exception e) {
        output.MessageErr1("cannot delete " + folder + ": " + e.Message);
        return false;
      }
      output.MessageSuc1("" + folder + " deleted");
      return true;
    }

    // run a process with parameters and wait for it to finish
    public bool RunProcess(string name, string args, string pwd) {
      try {
        Process p = new Process();
        p.StartInfo.FileName = name;
        p.StartInfo.Arguments = args;
        p.StartInfo.WorkingDirectory = pwd;
        p.Start();
        p.WaitForExit();
        if (p.ExitCode != 0) {
          output.MessageErr1("" + name + " returned an error");
          return false;
        }
      } catch (Exception e) {
        output.MessageErr1("cannot run " + name + ": " + e.Message);
        return false;
      }
      return true;
    }

    // create directory to store plugins temporarily
    public bool CreatePluginsDownloadDir(string dlPath) {
      if (Directory.Exists(Path.Join(dlPath, "plugins"))) {
        return true;
      }
      try {
        Directory.CreateDirectory(Path.Join(dlPath, "plugins"));
      } catch (Exception e) {
        output.MessageErr1("cannot create plugins download dir: " + e.Message);
        return false;
      }
      output.MessageSuc1("created plugins download directory");
      return true;
    }

    // download a file to given location
    public bool DownloadFile(string url, string location) {
      output.MessageSuc1("downloading " + url + " to " + location + "...");
      try {
        WebClient webClient = new WebClient();
        webClient.DownloadFile(url, location);
      } catch (Exception e) {
        output.MessageErr1("cannot download: " + e.Message);
        return false;
      }
      return true;
    }
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
