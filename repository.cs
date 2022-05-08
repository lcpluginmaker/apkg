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
      if (!archiveFile.EndsWith(".lcpkg")) {
        output.MessageErr1("this does not look like an apkg package archive");
        return;
      }
      output.MessageSuc1("preparing to extract package");
      string extractPath = Path.Join(savePath, "tmp", "plugin-extract");
      if (Directory.Exists(extractPath)) {
        if (!utils.DeleteDirectory(extractPath)) {
          output.MessageErr1("cannot clean plugin extract directory");
          return;
        }
      }
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
      output.MessageSuc0("checking package integrity");
      string text = File.ReadAllText(Path.Join(extractPath, "PKGINFO.json"));
      PkgArchiveManifest manifest = JsonSerializer.Deserialize<PkgArchiveManifest>(text);
      if (!integrity.CheckPkgConflicts(manifest.files, savePath)) {
        output.MessageWarn1("conflicts with some installed package");
        // TODO
        //output.MessageErr1("this package conflicts with some installed package");
        //return;
      }
      output.MessageSuc0(
          $"installing files for {manifest.packageName} from {manifest.project.maintainer}"
          );
      foreach (string file in manifest.files) {
        output.MessageSuc1("copying " + file);
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
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
