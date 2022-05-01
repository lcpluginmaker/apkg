using System.IO.Compression;
using System.Text.Json;

namespace LeoConsole_apkg {
  public class ApkgInstaller {
    private ApkgOutput output = new ApkgOutput();
    private ApkgUtils utils = new ApkgUtils();
    private ApkgIntegrity integrity = new ApkgIntegrity();

    // from package archive
    public void GetLCPKG(string archiveFile, string savePath) {
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
        output.MessageErr1("this package conflicts with some installed package");
        return;
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
      integrity.InstallFiles(manifest.files, savePath, manifest.packageName);
      output.MessageSuc0("successfully installed " + manifest.packageName);
    }

    // from local folder
    public void GetFile(string folder, string savePath) {
      output.MessageSuc0("installing from local folder...");
      if (!Directory.Exists(folder)) {
        output.MessageErr1("" + folder + " does not exist");
        return;
      }
      ApkgPackage package = new ApkgPackage(folder, savePath);
      if (!package.Compile()) {
        return;
      }
      if (!package.InstallDLLs()) {
        return;
      }
      output.MessageSuc0("" + folder + " was installed. restart LeoConsole to load it");
    }

    // compiling from git repository
    public void GetGit(string url, string dlPath, string savePath) {
      output.MessageSuc0("installing from git repository");
      string name = Path.GetFileName(url).Split(".")[0];
      try {
        if (Directory.Exists(Path.Join(dlPath, "plugins", name))) {
          output.MessageSuc1("download directory already exists. override [y/n] ? ");
          string answer = Console.ReadLine();
          switch (answer.ToLower()) {
            case "y":
              if (!utils.DeleteDirectory(Path.Join(dlPath, "plugins", name))) {
                return;
              }
              break;
            default:
              output.MessageSuc1("operation aborted");
              return;
              break;
          }
        }
      } catch (Exception e) {
        output.MessageErr1("cannot check download location: " + e.Message);
        return;
      }
      if (!utils.GitClone(url, Path.Join(dlPath, "plugins"), name, dlPath)) {
        return;
      }
      ApkgPackage package = new ApkgPackage(Path.Join(dlPath, "plugins", name), savePath);
      if (!package.Compile()) {
        return;
      }
      if (!package.InstallDLLs()) {
        return;
      }
      output.MessageSuc0(url + " was installed. restart LeoConsole to load it");
      return;
    }
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
