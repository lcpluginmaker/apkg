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
    private bool DebugMode;

    public ApkgRepository(string sp, string v, bool dm) {
      LeoConsoleVersion = v;
      SavePath = sp;
      DebugMode = dm;
      DownloadPath = Path.Join(SavePath, "tmp", "apkg");
      ConfigDir = Path.Join(SavePath, "var", "apkg");
      ReposFolder = Path.Join(ConfigDir, "repos-index");
      Index = Enumerable.Empty<RepoPackage>().ToList();

      if (!Directory.Exists(ReposFolder)) {
        LConsole.MessageWarn1("could not find repo cache, package list cannot be loaded");
        LConsole.MessageWarn1("please run 'apkg reload'");
        return;
      }

      foreach (string r in Directory.GetFiles(ReposFolder)) {
        string text = File.ReadAllText(r);
        RepoIndex thisRepoIndex = JsonSerializer.Deserialize<RepoIndex>(text);
        foreach (RepoPackage p in thisRepoIndex.packageList) {
          Index.Add(p);
        }
      }
    } // }}}

    // GetInfoFor() {{{
    public RepoPackage GetInfoFor(string package) {
      foreach (RepoPackage rp in Index) {
        if (rp.name == package) {
          return rp;
        }
      }
      throw new Exception("package not found");
    }
    //}}}

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
        LConsole.MessageSuc0("loading " + repo.name);
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
        Directory.Delete(extractPath, true);
      }
      Directory.CreateDirectory(extractPath);
      // extract package archive
      LConsole.MessageSuc0("extracting package");
      ZipFile.ExtractToDirectory(path, extractPath);
      return extractPath;
    } // }}}

    // InstallFiles() {{{
    private void InstallFiles(string[] files, string packageDir) {
      foreach (string file in files) {
        LConsole.MessageSuc1("copying " + file);
        Directory.CreateDirectory(Directory.GetParent(Path.Join(SavePath, file)).FullName);
        // copy file
        File.Copy(Path.Join(packageDir, file), Path.Join(SavePath, file), true);
        if (
          (
            file.StartsWith("share/scripts")
            || file.StartsWith("share/go-plugin")
            || file.StartsWith("share/apkg/bin")
            || (file.StartsWith("share/apkg/extensions") && !file.EndsWith(".json"))
          )
            && ApkgUtils.GetRunningOS() == "lnx64"
        ) {
          LConsole.MessageSuc1($"marking {file} as executable");
          if (!ApkgUtils.RunProcess("chmod", "+x " + Path.Join(SavePath, file), SavePath)) {
            LConsole.MessageWarn1($"cannot mark {file} as executable");
          }
        }
      }
    } // }}}

    // InstallLcpkg() {{{
    public void InstallLcpkg(string archiveFile) {
      LConsole.MessageSuc0("installing package");
      LConsole.MessageSuc1("preparing to extract package");

      string tempFolder = "";
      try {
        tempFolder = ExtractLcp(archiveFile);
      } catch (Exception e) {
        LConsole.MessageErr0($"cannot extract lcp archive: {e.Message}");
        return;
      }

      LConsole.MessageSuc0("checking package compatibility");
      PkgArchiveManifest manifest = FileUtils.ReadManifest(tempFolder);
      
      if (!Array.Exists(manifest.compatibleVersions, e => e == LeoConsoleVersion)) {
        LConsole.MessageErr1("your LeoConsole version is incompatible with this plugin");
        return;
      }

      // install dependencies {{{
      foreach (string pack in manifest.depends) {
        if (Directory.Exists(Path.Join(ConfigDir, "installed", pack))) {
          LConsole.MessageSuc1($"dependency {pack} already installed");
          continue;
        }
        LConsole.MessageSuc0($"installing dependency {pack}");
        string url;
        try {
          url = GetUrlFor(pack);
        } catch (Exception e) {
          LConsole.MessageErr1("cannot find a dependency");
          return;
        }
        string dlPath = Path.Join(DownloadPath, $"{pack}.lcp");
        if (!ApkgUtils.DownloadFile(url, dlPath)) {
          return;
        }
        InstallLcpkg(dlPath);
      } // }}}

      // check conflicts {{{
      string conflictsWith = ApkgIntegrity.CheckPkgConflicts(manifest.files, SavePath);
      if (conflictsWith != "") {
        if (conflictsWith != manifest.packageName) {
          LConsole.MessageErr0($"{manifest.packageName} conflicts with {conflictsWith}, aborting install");
          return;
        }
        // conflicting with itself ask about reinstalling {{{
        string installedVersion = File.ReadAllText(Path.Join(ConfigDir, "installed", manifest.packageName, "version")).Trim();
        if (installedVersion == manifest.packageVersion) {
          if (DebugMode) {
            LConsole.MessageWarn1("reinstalling same package version");
          } else {
            if (!LConsole.YesNoDialog("reinstall same package version?", true)) {
              LConsole.MessageErr1("installation aborted");
              return;
            }
          }
        }
        if (ApkgUtils.VersionGreater(installedVersion, manifest.packageVersion)) {
          if (DebugMode) {
            LConsole.MessageWarn1("downgrading package");
          } else {
            if (!LConsole.YesNoDialog($"downgrade package ({installedVersion}->{manifest.packageVersion})?", false)) {
              LConsole.MessageErr1("installation aborted");
              return;
            }
          }
        } // }}}
        RemovePackage(manifest.packageName);
      } // }}}

      LConsole.MessageSuc0($"installing files for {manifest.project.maintainer}/{manifest.packageName}");
      InstallFiles(manifest.files, tempFolder);

      ApkgIntegrity.Register(
          manifest.packageName, manifest.packageVersion, manifest.files, SavePath
          );
      LConsole.MessageSuc0("successfully installed " + manifest.packageName);
    } // }}}

    // RemovePackage() {{{
    public void RemovePackage(string p) {
      LConsole.MessageSuc0("removing " + p);
      if (!Directory.Exists(Path.Join(ConfigDir, "installed", p))) {
        LConsole.MessageErr0($"{p} is not installed");
        return;
      }
      try {
        FileUtils.DeleteFiles(File.ReadLines(Path.Join(ConfigDir, "installed", p, "files")).ToArray(), SavePath);
      } catch (Exception e) {
        LConsole.MessageErr0("removing package failed: " + e.Message);
        return;
      }
      ApkgIntegrity.Unregister(p, SavePath);
    } // }}}
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
