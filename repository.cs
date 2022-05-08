using ILeoConsole.Core;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace LeoConsole_apkg {
  public class ApkgRepository {
    private ApkgUtils utils = new ApkgUtils();
    private ApkgOutput output = new ApkgOutput();
    private ApkgIntegrity integrity = new ApkgIntegrity();
    private IList<RepoPackage> index;

    public string GetRunningOS() {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
        return "win64";
      }
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
        return "lnx64";
      }
      return "other";
    }

    public string GetUrlFor(string package, string savePath) {
      foreach (RepoPackage p in index) {
        if (p.name == package && (p.os == "any" || p.os == GetRunningOS())) {
          return p.url;
        }
      }
      throw new Exception("cannot find package");
    }

    public IList<string> AvailablePlugins(string savePath) {
      IList<string> pluginsList = Enumerable.Empty<string>().ToList();
      try {
        Reload(savePath);
      } catch (Exception e) {
        return pluginsList;
      }
      foreach (RepoPackage p in index) {
        pluginsList.Add(p.name);
      }
      return pluginsList;
    }

    public void Reload(string savePath) {
      string reposListFile = Path.Join(savePath, "var", "apkg", "repos");
      output.MessageSuc0("reloading package index");
      IList<RepoPackage> newIndex = Enumerable.Empty<RepoPackage>().ToList();
      if (!File.Exists(reposListFile)) {
        output.MessageErr0("repos list ($SAVEPATH/var/apkg/repos) does not exist");
        throw new Exception("repos file does not exist");
      }
      foreach (string repo in File.ReadLines(reposListFile)) {
        output.MessageSuc1("loading " + repo);
        if (!utils.DownloadFile(repo, Path.Join(savePath, "tmp", "repo.json"))) {
          throw new Exception("error downloading " + repo);
        }
        string text = System.IO.File.ReadAllText(Path.Join(savePath, "tmp", "repo.json"));
        RepoIndex thisRepoIndex = JsonSerializer.Deserialize<RepoIndex>(text);
        foreach (RepoPackage p in thisRepoIndex.packageList) {
          newIndex.Add(p);
        }
      }
      index = newIndex;
    }

    public void InstallLcpkg(string archiveFile, string savePath) {
      output.MessageSuc1("preparing to extract package");
      string extractPath = Path.Join(savePath, "tmp", "plugin-extract");
      // delete directory if already exists
      if (Directory.Exists(extractPath)) {
        if (!utils.DeleteDirectory(extractPath)) {
          output.MessageErr1("cannot clean plugin extract directory");
          return;
        }
      }
      // extract package archive
      try {
        Directory.CreateDirectory(extractPath);
      } catch (Exception e) {
        output.MessageErr1("cannot create plugin extract dir: " + e.Message);
        return;
      }
      output.MessageSuc0("extracting package");
      try {
        ZipFile.ExtractToDirectory(archiveFile, extractPath);
      } catch (Exception e) {
        output.MessageErr1("cannot extract plugin: " + e.Message);
        return;
      }
      // integrity
      output.MessageSuc0("checking package integrity");
      string text = File.ReadAllText(Path.Join(extractPath, "PKGINFO.json"));
      PkgArchiveManifest manifest = JsonSerializer.Deserialize<PkgArchiveManifest>(text);
      if (!integrity.CheckPkgConflicts(manifest.files, savePath)) {
        if (Directory.Exists(Path.Join(savePath, "var", "apkg", "installed", manifest.packageName))) {
          string installedVersion = File.ReadAllText(Path.Join(savePath, "var", "apkg", "installed", manifest.packageName, "version")).Trim();
          if (installedVersion == manifest.packageVersion) {
            Console.WriteLine("reinstall same package version [y/n]?");
            string answer = Console.ReadLine();
            if (answer.ToLower() != "y") {
              output.MessageErr1("installation aborted");
              return;
            }
          } else if (utils.VersionGreater(installedVersion, manifest.packageVersion)) {
            Console.WriteLine("downgrade package (" + installedVersion + "->" + manifest.packageVersion + ") [y/n]?");
            string answer = Console.ReadLine();
            if (answer.ToLower() != "y") {
              output.MessageErr1("installation aborted");
              return;
            }
          }
          RemovePackage(manifest.packageName, savePath);
        } else {
          output.MessageErr1("this package conflicts with some installed package");
          return;
        }
      }
      output.MessageSuc0(
          $"installing files for {manifest.packageName} from {manifest.project.maintainer}"
          );
      foreach (string file in manifest.files) {
        output.MessageSuc1("copying " + file);
        // create parent folders
        string[] parts = file.Split("/");
        for (int i = 0; i < parts.Length - 1; i++) {
          string d = "";
          for (int j = 0; j <= i; j++) {
            d = Path.Join(d, parts[j]);
          }
          if (!Directory.Exists(Path.Join(savePath, d))) {
            Directory.CreateDirectory(Path.Join(savePath, d));
          }
        }
        // copy file
        File.Copy(
            Path.Join(extractPath, file),
            Path.Join(savePath, file),
            true
            );
      }
      integrity.Register(
          manifest.packageName, manifest.packageVersion, manifest.files, savePath
          );
      output.MessageSuc0("successfully installed " + manifest.packageName);
    }

    public void RemovePackage(string p, string savePath) {
      output.MessageSuc0("removing " + p);
      if (!Directory.Exists(
            Path.Join(savePath, "var", "apkg", "installed", p)
            )) {
        output.MessageErr0("this package is not installed");
        return;
      }
      try {
        foreach (string f in File.ReadLines(
              Path.Join(savePath, "var", "apkg", "installed", p, "files")
              )) {
          string path = Path.Join(savePath, f);
          output.MessageSuc1("deleting " + path);
          File.Delete(path);
        }
      } catch (Exception e) {
        output.MessageErr0("removing package failed");
        return;
      }
      integrity.Unregister(p, savePath);
    }
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
