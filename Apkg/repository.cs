using ILeoConsole.Core;
using System.IO.Compression;
using System.Text.Json;

namespace LeoConsole_apkg {
  public class ApkgRepository {
    // variables and init {{{
    private IList<RepoPackage> Index;
    private string SavePath;
    private string ConfigDir;
    private string DownloadPath;
    private string ReposFolder;
    private string LeoConsoleVersion;

    public ApkgRepository(string sp, string v) {
      LeoConsoleVersion = v;
      SavePath = sp;
      DownloadPath = Path.Join(SavePath, "tmp", "apkg");
      ConfigDir = Path.Join(SavePath, "var", "apkg");
      ReposFolder = Path.Join(ConfigDir, "repos-index");
      Index = Enumerable.Empty<RepoPackage>().ToList();

      if (!Directory.Exists(ReposFolder)) {
        ApkgOutput.MessageWarn1("could not find repo cache, package list cannot be loaded");
        ApkgOutput.MessageWarn1("please run 'apkg reload'");
        return;
      }

      foreach (string r in Directory.GetFiles(ReposFolder)) {
        string text = File.ReadAllText(r);
        RepoIndex thisRepoIndex = JsonSerializer.Deserialize<RepoIndex>(text);
        foreach (RepoPackage p in thisRepoIndex.packageList) {
          Index.Add(p);
        }
      }
    }
    // }}}

    // GetUrlFor() {{{
    public string GetUrlFor(string package) {
      foreach (RepoPackage p in Index) {
        if (
            p.name == package
            && (p.os == "any" || p.os == ApkgUtils.GetRunningOS())
            && Array.Exists(p.lc, e => e == LeoConsoleVersion)
            ) {
          return p.url;
        }
      }
      throw new Exception("cannot find package");
    } // }}}

    // AvailablePlugins() {{{
    public IList<string> AvailablePlugins() {
      IList<string> pluginsList = Enumerable.Empty<string>().ToList();
      foreach (RepoPackage p in Index) {
        if (
            (p.os == "any" || p.os == ApkgUtils.GetRunningOS())
            && Array.Exists(p.lc, e => e == LeoConsoleVersion)
            ) {
          pluginsList.Add(p.name);
        }
      }
      return pluginsList;
    } // }}}

    // Reload() {{{
    public void Reload(ConfigRepo[] repos) {
      IList<RepoPackage> newIndex = Enumerable.Empty<RepoPackage>().ToList();
      foreach (ConfigRepo repo in repos) {
        ApkgOutput.MessageSuc1("loading " + repo.name);
        string tempFile = Path.Join(DownloadPath, repo.name + ".json");
        if (!ApkgUtils.DownloadFile(repo.url, tempFile)) {
          throw new Exception("error downloading " + repo.name);
        }
        string text = System.IO.File.ReadAllText(tempFile);
        RepoIndex thisRepoIndex = JsonSerializer.Deserialize<RepoIndex>(text);
        File.Copy(
            tempFile,
            Path.Join(ReposFolder, thisRepoIndex.name + ".json"),
            true
            );
        foreach (RepoPackage p in thisRepoIndex.packageList) {
          newIndex.Add(p);
        }
      }
      Index = newIndex;
    } // }}}

    // ExtractLcp() {{{
    private string ExtractLcp(string path) {
      string extractPath = Path.Join(DownloadPath, Path.GetFileName(path).Replace(".lcp", ""));
      // delete directory if already exists
      if (Directory.Exists(extractPath)) {
        if (!ApkgUtils.DeleteDirectory(extractPath)) {
          throw new Exception("cannot clean plugin extract directory");
        }
      }
      Directory.CreateDirectory(extractPath);
      // extract package archive
      ApkgOutput.MessageSuc0("extracting package");
      ZipFile.ExtractToDirectory(archiveFile, extractPath);
      return extractPath;
    } // }}}

    // ReadManifest() {{{
    private PkgArchiveManifest ReadManifest(string folder) {
      string text = File.ReadAllText(Path.Join(folder, "PKGINFO.json"));
      return JsonSerializer.Deserialize<PkgArchiveManifest>(text);
    } // }}}

    // InstallFiles() {{{
    private void InstallFiles(string[] files, string packageDir) {
      foreach (string file in files) {
        ApkgOutput.MessageSuc1("copying " + file);
        Directory.CreateDirectory(Directory.GetParent(Path.Join(SavePath, file)).FullName);
        // copy file
        File.Copy(Path.Join(packageDir, file), Path.Join(SavePath, file), true);
        if (
            (file.StartsWith("share/scripts") || file.StartsWith("share/go-plugin")) 
            && ApkgUtils.GetRunningOS() == "lnx64"
            ) {
          ApkgOutput.MessageSuc1($"marking {file} as executable");
          if (!ApkgUtils.RunProcess("chmod", "+x " + Path.Join(SavePath, file), SavePath)) {
            ApkgOutput.MessageWarn1($"cannot mark {file} as executable");
          }
        }
      }
    } // }}}

    // InstallLcpkg() {{{
    public void InstallLcpkg(string archiveFile) {
      ApkgOutput.MessageSuc0("installing package");
      ApkgOutput.MessageSuc1("preparing to extract package");

      string tempFolder;
      try {
        tempFolder = ExtractLcp(archiveFile);
      } catch (Exception e) {
        ApkgOutput.MessageErr0($"cannot extract lcp archive: {e.Message}");
      }

      ApkgOutput.MessageSuc0("checking package compatibility");
      PkgArchiveManifest manifest = ReadManifest(tempFolder);
      
      if (!Array.Exists(manifest.compatibleVersions, e => e == LeoConsoleVersion)) {
        ApkgOutput.MessageErr1("your LeoConsole version is incompatible with this plugin");
        return;
      }

      // install dependencies {{{
      foreach (string pack in manifest.depends) {
        string url;
        try {
          url = GetUrlFor(pack);
        } catch (Exception e) {
          ApkgOutput.MessageErr1("cannot find a dependency");
          return;
        }
        string dlPath = Path.Join(DownloadPath, $"{pack}.lcp");
        if (!ApkgUtils.DownloadFile(url, dlPath)) {
          return;
        }
        InstallLcpkg(dlPath);
      } // }}}

      // TODO: check with which package(s) exactly incompatible {{{
      if (!ApkgIntegrity.CheckPkgConflicts(manifest.files, SavePath)) {
        if (Directory.Exists(Path.Join(ConfigDir, "installed", manifest.packageName))) {
          string installedVersion = File.ReadAllText(Path.Join(ConfigDir, "installed", manifest.packageName, "version")).Trim();
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
      } // }}}

      ApkgOutput.MessageSuc0($"installing files for {manifest.project.maintainer}/{manifest.packageName}");
      InstallFiles(manifest.files, tempFolder);

      ApkgIntegrity.Register(
          manifest.packageName, manifest.packageVersion, manifest.files, SavePath
          );
      ApkgOutput.MessageSuc0("successfully installed " + manifest.packageName);
    } // }}}

    // RemovePackage() {{{
    public void RemovePackage(string p) {
      ApkgOutput.MessageSuc0("removing " + p);
      if (!Directory.Exists(
            Path.Join(ConfigDir, "installed", p)
            )) {
        ApkgOutput.MessageErr0("this package is not installed");
        return;
      }
      try {
        foreach (string f in File.ReadLines(
              Path.Join(ConfigDir, "installed", p, "files")
              )) {
          string path = Path.Join(SavePath, f);
          ApkgOutput.MessageSuc1("deleting " + path);
          File.Delete(path);
          // delete parent dirs if empty {{{
          string parent = Directory.GetParent(path).FullName;
          while (parent != SavePath) {
            if (Directory.EnumerateFileSystemEntries(parent).Any()) {
              break;
            }
            ApkgUtils.DeleteDirectory(parent);
            parent = Directory.GetParent(parent).FullName;
          } // }}}
        }
      } catch (Exception e) {
        ApkgOutput.MessageErr0("removing package failed: " + e.Message);
        return;
      }
      ApkgIntegrity.Unregister(p, SavePath);
    } // }}}
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
