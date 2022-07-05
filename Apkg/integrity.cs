using System.Linq;

namespace LeoConsole_apkg {
  public class ApkgIntegrity {
    // CheckPkgConflicts() {{{
    public static string CheckPkgConflicts(string[] files, string savePath) {
      string databaseFolder = Path.Join(savePath, "var", "apkg", "installed");
      foreach (string p in Directory.GetDirectories(databaseFolder)) {
        foreach (string i in File.ReadLines(Path.Join(p, "files"))) {
          if (Array.Exists(files, f => f == i)) {
            return Path.GetFileName(p);
          }
        }
      }
      return "";
    } // }}}

    // Register() {{{
    public static void Register(string p, string pVersion, string[] f, string savePath) {
      ApkgOutput.MessageSuc0($"registering package {p} v{pVersion}");
      string baseDir = Path.Join(savePath, "var", "apkg", "installed", p);
      Directory.CreateDirectory(baseDir);
      File.WriteAllLines(Path.Join(baseDir, "files"), f);
      File.WriteAllLines(Path.Join(baseDir, "version"), new string[1]{pVersion});
    } // }}}

    // Unregister() {{{
    public static void Unregister(string p, string savePath) {
      Directory.Delete(Path.Join(savePath, "var", "apkg", "installed", p), true);
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
