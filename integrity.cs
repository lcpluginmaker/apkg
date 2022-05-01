using System.Linq;

namespace LeoConsole_apkg {
  public class ApkgIntegrity {
    private ApkgOutput output = new ApkgOutput();

    public bool CheckPkgConflicts(string[] files, string savePath) {
      IList<string> installed;
      try {
        installed = InstalledFiles(savePath);
      } catch (Exception e) {
        output.MessageErr1("error loading installed files: " + e.Message);
        return false;
      }
      foreach (string file in files) {
        if (installed.Contains(file)) {
          return false;
        }
      }
      return true;
    }

    public void InstallFiles(string[] files, string savePath, string package) {
      File.WriteAllLines(
          Path.Join(savePath, "var", "apkg", "files-installed", package),
          files
          );
    }

    public IList<string> InstalledFiles(string savePath) {
      string databaseFolder = Path.Join(savePath, "var", "apkg", "files-installed");
      if (!Directory.Exists(databaseFolder)) {
        output.MessageErr1("database folder does not exist");
        Console.WriteLine("Create empty database [y/n]? ");
        string answer = Console.ReadLine();
        switch (answer.ToLower()) {
          case "y":
            Directory.CreateDirectory(databaseFolder);
            return Enumerable.Empty<string>().ToList();
            break;
          default:
            throw new Exception("database file does not exist");
            break;
        }
      }
      IList<string> res = Enumerable.Empty<string>().ToList();
      foreach (string f in Directory.GetFiles(databaseFolder)) {
        foreach (string installedFile in File.ReadLines(f)) {
          res.Add(installedFile);
        }
      }
      return res;
    }
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
