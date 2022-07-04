using System.Linq;

namespace LeoConsole_apkg {
  public class ApkgIntegrity {
    // CheckPkgConflicts() {{{
    public static bool CheckPkgConflicts(string[] files, string savePath) {
      IList<string> installed;
      try {
        installed = InstalledFiles(savePath);
      } catch (Exception e) {
        ApkgOutput.MessageErr1("error loading installed files: " + e.Message);
        return false;
      }
      foreach (string file in files) {
        if (installed.Contains(file)) {
          return false;
        }
      }
      return true;
    } // }}}

    // Register() {{{
    public static void Register(string p, string pVersion, string[] f, string savePath) {
      ApkgOutput.MessageSuc0($"registering package {p} v{pVersion}");
      string baseDir = Path.Join(savePath, "var", "apkg", "installed", p);
      Directory.CreateDirectory(baseDir);
      File.WriteAllLines(Path.Join(baseDir, "files"), f);
      File.WriteAllLines(Path.Join(baseDir, "version"), {pVersion});
    } // }}}

    // Unregister() {{{
    public static void Unregister(string p, string savePath) {
      ApkgUtils.DeleteDirectory(
          Path.Join(savePath, "var", "apkg", "installed", p)
          );
    } // }}}

    // InstalledFiles() {{{
    public static IList<string> InstalledFiles(string savePath) {
      string databaseFolder = Path.Join(savePath, "var", "apkg", "installed");
      IList<string> res = Enumerable.Empty<string>().ToList();
      foreach (string p in Directory.GetDirectories(databaseFolder)) {
        foreach (string i in File.ReadLines(Path.Join(p, "files"))) {
          res.Add(i);
        }
      }
      return res;
    } // }}}
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
