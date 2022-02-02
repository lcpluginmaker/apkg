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

    // compile a repository
    public bool CompileFolder(string folder) {
      output.MessageSuc0("compiling " + folder + " with dotnet...");
      if (File.Exists(Path.Join(folder, "manifest.apkg.json"))) {
        string text = System.IO.File.ReadAllText(Path.Join(folder, "manifest.apkg.json"));
        Manifest manifestData = JsonSerializer.Deserialize<Manifest>(text);
        output.MessageSuc1("compiling package " + manifestData.packageName + " by " + manifestData.project.maintainer);
        output.MessageSuc1("compiling with '" + manifestData.build.command + " " + manifestData.build.args + "' in 'project://" + manifestData.build.folder + "'...");
        if (!RunProcess(manifestData.build.command, manifestData.build.args, Path.Join(folder, manifestData.build.folder))) {
          return false;
        }
      } else {
        output.MessageWarn1("manifest.apkg.json not found, trying 'dotnet build'...");
        if (!RunProcess("dotnet", "build", folder)) {
          return false;
        }
      }
      output.MessageSuc1("compiled " + folder + " successfully");
      return true;
    }

    // download a file to given location
    public bool DownloadFile(string url, string location) {
      output.MessageSuc0("downloading " + url + " to " + location + "...");
      try {
        WebClient webClient = new WebClient();
        webClient.DownloadFile(url, location);
      } catch (Exception e) {
        output.MessageErr1("cannot download: " + e.Message);
        return false;
      }
      return true;
    }

    // install all dlls from bin subfolder of a repository
    public bool InstallDLLs(string from_folder, string savePath) {
      output.MessageSuc0("installing dlls from " + from_folder + "...");
      try {
        foreach (string filename in Directory.GetFiles(Path.Join(from_folder, "bin", "Debug", "net6.0"))){
          if (filename.EndsWith(".dll")){
            output.MessageSuc1("copying " + filename + " to " + Path.Join(savePath, "plugins", Path.GetFileName(filename)) + "...");
            File.Copy(filename, Path.Join(savePath, "plugins", Path.GetFileName(filename)), true);
          }
        }
      } catch (Exception e) {
        output.MessageErr1("cannot install: " + e.Message);
        return false;
      }
      return true;
    }

    // clone a repository to given location
    public bool GitClone(string url, string parent, string folder, string dlPath) {
      output.MessageSuc0("cloning " + url + " to " + parent + "/" + folder + "...");
      if (!CreatePluginsDownloadDir(dlPath)) {
        return false;
      }
      if (!RunProcess("git", "clone " + url + " " + folder, parent)) {
        return false;
      }
      output.MessageSuc1("cloned repo successfully");
      return true;
    }
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
