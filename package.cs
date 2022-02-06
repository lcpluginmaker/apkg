using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.Json;

namespace LeoConsole_apkg {
  public class ApkgPackage {
    private ApkgUtils utils = new ApkgUtils();
    private ApkgOutput output = new ApkgOutput();
    public string Folder;
    public string SavePath;
    public Manifest PkgManifest;

    public ApkgPackage(string folder, string savePath) {
      Folder = folder;
      SavePath = savePath;
      if (File.Exists(Path.Join(Folder, "manifest.apkg.json"))) {
        loadManifestFromFile();
      } else {
        loadDefaultManifest();
      }
    }

    private void loadManifestFromFile() {
      output.MessageSuc1("found manifest file, parsing...");
      string text = System.IO.File.ReadAllText(Path.Join(Folder, "manifest.apkg.json"));
      PkgManifest = JsonSerializer.Deserialize<Manifest>(text);
    }

    private void loadDefaultManifest() {
      output.MessageWarn1("no manifest file found, using default");
      PkgManifest = new Manifest();
      PkgManifest.manifestVersion = 1.0F;
      PkgManifest.packageName = Path.GetFileName(Folder);
      PkgManifest.build = new ManifestBuildInstruction();
      PkgManifest.build.command = "dotnet";
      PkgManifest.build.args = "build --nologo";
      PkgManifest.build.folder = ".";
      PkgManifest.project = new ManifestProjectData();
    }

    public bool Compile() {
      output.MessageSuc0("compiling " + Folder);
      output.MessageSuc1("compiling package '" + PkgManifest.packageName + "' by '" + PkgManifest.project.maintainer + "'...");
      output.MessageSuc1("compiling with '" + PkgManifest.build.command + " " + PkgManifest.build.args + "' in 'project://" + PkgManifest.build.folder + "'...");
      if (!utils.RunProcess(PkgManifest.build.command, PkgManifest.build.args, Path.Join(Folder, PkgManifest.build.folder))) {
        return false;
      }
      output.MessageSuc1("compiled " + Folder + " successfully");
      return true;
    }

    public bool InstallDLLs() {
      output.MessageSuc0("installing dlls from " + Folder + "...");
      try {
        foreach (string filename in Directory.GetFiles(Path.Join(Folder, "bin", "Debug", "net6.0"))){
          if (filename.EndsWith(".dll")){
            output.MessageSuc1("copying " + filename + " to " + Path.Join(SavePath, "plugins", Path.GetFileName(filename)) + "...");
            File.Copy(filename, Path.Join(SavePath, "plugins", Path.GetFileName(filename)), true);
          }
        }
      } catch (Exception e) {
        output.MessageErr1("cannot install: " + e.Message);
        return false;
      }
      return true;
    }
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
