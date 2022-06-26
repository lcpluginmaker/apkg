using ILeoConsole.Core;
using System.IO.Compression;
using System.Text.Json;

namespace LeoConsole_apkg {
  public class ApkgRepository {
    private IList<RepoPackage> index;
    private string savePath;
    private string configDir;
    private string reposFolder;
    private string lcVersion;

    public ApkgRepository(string sp, string v) {
      lcVersion = v;
      savePath = sp;
      configDir = Path.Join(savePath, "var", "apkg");
      reposFolder = Path.Join(configDir, "repos-index");
      index = Enumerable.Empty<RepoPackage>().ToList();
      if (Directory.Exists(reposFolder)) {
        foreach (string r in Directory.GetFiles(reposFolder)) {
          string text = File.ReadAllText(r);
          RepoIndex thisRepoIndex = JsonSerializer.Deserialize<RepoIndex>(text);
          foreach (RepoPackage p in thisRepoIndex.packageList) {
            index.Add(p);
          }
        }
        return;
      }
      ApkgOutput.MessageWarn1("could not find repo cache, package list could not be loaded");
      ApkgOutput.MessageWarn1("please run 'apkg reload'");
    }

    public string GetUrlFor(string package) {
      foreach (RepoPackage p in index) {
        if (
            p.name == package
            && (p.os == "any" || p.os == ApkgUtils.GetRunningOS())
            && Array.Exists(p.lc, e => e == lcVersion)
            ) {
          return p.url;
        }
      }
      throw new Exception("cannot find package");
    }

    public IList<string> AvailablePlugins() {
      IList<string> pluginsList = Enumerable.Empty<string>().ToList();
      foreach (RepoPackage p in index) {
        if (
            (p.os == "any" || p.os == ApkgUtils.GetRunningOS())
            && Array.Exists(p.lc, e => e == lcVersion)
            ) {
          pluginsList.Add(p.name);
        }
      }
      return pluginsList;
    }

    public void Reload(ConfigRepo[] repos) {
      IList<RepoPackage> newIndex = Enumerable.Empty<RepoPackage>().ToList();
      foreach (ConfigRepo repo in repos) {
        ApkgOutput.MessageSuc1("loading " + repo.name);
        if (!ApkgUtils.DownloadFile(repo.url, Path.Join(savePath, "tmp", "repo.json"))) {
          throw new Exception("error downloading " + repo.name);
        }
        string text = System.IO.File.ReadAllText(Path.Join(savePath, "tmp", "repo.json"));
        RepoIndex thisRepoIndex = JsonSerializer.Deserialize<RepoIndex>(text);
        File.Copy(
            Path.Join(savePath, "tmp", "repo.json"),
            Path.Join(reposFolder, thisRepoIndex.name + ".json"),
            true
            );
        foreach (RepoPackage p in thisRepoIndex.packageList) {
          newIndex.Add(p);
        }
      }
      index = newIndex;
    }

    public void InstallLcpkg(string archiveFile) {
      ApkgOutput.MessageSuc0("installing package");
      ApkgOutput.MessageSuc1("preparing to extract package");
      string extractPath = Path.Join(savePath, "tmp", "apkg", Path.GetFileName(archiveFile).Replace(".lcp", ""));
      // delete directory if already exists
      if (Directory.Exists(extractPath)) {
        if (!ApkgUtils.DeleteDirectory(extractPath)) {
          ApkgOutput.MessageErr1("cannot clean plugin extract directory");
          return;
        }
      }
      // extract package archive
      try {
        Directory.CreateDirectory(extractPath);
      } catch (Exception e) {
        ApkgOutput.MessageErr1("cannot create plugin extract dir: " + e.Message);
        return;
      }
      ApkgOutput.MessageSuc0("extracting package");
      try {
        ZipFile.ExtractToDirectory(archiveFile, extractPath);
      } catch (Exception e) {
        ApkgOutput.MessageErr1("cannot extract plugin: " + e.Message);
        return;
      }
      // integrity
      ApkgOutput.MessageSuc0("checking package integrity");
      string text = File.ReadAllText(Path.Join(extractPath, "PKGINFO.json"));
      PkgArchiveManifest manifest = JsonSerializer.Deserialize<PkgArchiveManifest>(text);
      if (!Array.Exists(manifest.compatibleVersions, e => e == lcVersion)) {
        ApkgOutput.MessageErr1("your LeoConsole version is incompatible with this plugin");
        return;
      }
      foreach (string pack in manifest.depends) {
        string url;
        try {
          url = GetUrlFor(pack);
        } catch (Exception e) {
          ApkgOutput.MessageErr1("cannot find your package");
          return;
        }
        string dlPath = Path.Join(savePath, "tmp", "apkg", $"{pack}.lcp");
        if (!ApkgUtils.DownloadFile(url, dlPath)) {
          return;
        }
        InstallLcpkg(dlPath);
      }
      if (!ApkgIntegrity.CheckPkgConflicts(manifest.files, savePath)) {
        if (Directory.Exists(Path.Join(configDir, "installed", manifest.packageName))) {
          string installedVersion = File.ReadAllText(Path.Join(configDir, "installed", manifest.packageName, "version")).Trim();
          if (installedVersion == manifest.packageVersion) {
            if (!LConsole.YesNoDialog("reinstall same package version?", true)) {
              ApkgOutput.MessageErr1("installation aborted");
              return;
            }
          } else if (ApkgUtils.VersionGreater(installedVersion, manifest.packageVersion)) {
            if (!LConsole.YesNoDialog($"downgrade package ({installedVersion}->{manifest.packageVersion})?", false)) {
              ApkgOutput.MessageErr1("installation aborted");
              return;
            }
          }
          RemovePackage(manifest.packageName);
        } else {
          ApkgOutput.MessageErr1("this package conflicts with some installed package");
          return;
        }
      }
      ApkgOutput.MessageSuc0($"installing files for {manifest.project.maintainer}/{manifest.packageName}");
      foreach (string file in manifest.files) {
        ApkgOutput.MessageSuc1("copying " + file);
        Directory.CreateDirectory(Directory.GetParent(Path.Join(savePath, file)).FullName);
        // copy file
        File.Copy(
            Path.Join(extractPath, file),
            Path.Join(savePath, file),
            true
            );
        if (
            (file.StartsWith("share/scripts") || file.StartsWith("share/go-plugin")) 
            && ApkgUtils.GetRunningOS() == "lnx64"
            ) {
          ApkgOutput.MessageSuc1("marking " + file + " as executable");
          if (!ApkgUtils.RunProcess("chmod", "+x " + Path.Join(savePath, file), savePath)) {
            ApkgOutput.MessageWarn1("cannot mark " + file + " as executable");
          }
        }
      }
      ApkgIntegrity.Register(
          manifest.packageName, manifest.packageVersion, manifest.files, savePath
          );
      ApkgOutput.MessageSuc0("successfully installed " + manifest.packageName);
    }

    public void RemovePackage(string p) {
      ApkgOutput.MessageSuc0("removing " + p);
      if (!Directory.Exists(
            Path.Join(configDir, "installed", p)
            )) {
        ApkgOutput.MessageErr0("this package is not installed");
        return;
      }
      try {
        foreach (string f in File.ReadLines(
              Path.Join(configDir, "installed", p, "files")
              )) {
          string path = Path.Join(savePath, f);
          ApkgOutput.MessageSuc1("deleting " + path);
          File.Delete(path);
          string parent = Directory.GetParent(path).FullName;
          while (parent != savePath) {
            if (Directory.EnumerateFileSystemEntries(parent).Any()) {
              break;
            }
            ApkgUtils.DeleteDirectory(parent);
            parent = Directory.GetParent(parent).FullName;
          }
        }
      } catch (Exception e) {
        ApkgOutput.MessageErr0("removing package failed: " + e.Message);
        return;
      }
      ApkgIntegrity.Unregister(p, savePath);
    }
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
