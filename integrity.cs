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

    public void InstallFiles(string[] files, string savePath) {
      IList<string> installed = InstalledFiles(savePath);
      foreach (string newFile in files) {
        installed.Add(newFile);
      }
      File.WriteAllLines(
          Path.Join(savePath, "var", "apkg", "files-installed"),
          installed
          );
    }

    public IList<string> InstalledFiles(string savePath) {
      string databaseFile = Path.Join(savePath, "var", "apkg", "files-installed");
      if (!File.Exists(databaseFile)) {
        output.MessageErr1("database file does not exist");
        Console.WriteLine("Create empty database [y/n]? ");
        string answer = Console.ReadLine();
        switch (answer.ToLower()) {
          case "y":
            File.Create(databaseFile).Dispose();
            return Enumerable.Empty<string>().ToList();
            break;
          default:
            throw new Exception("database file does not exist");
            break;
        }
      }
      return File.ReadLines(databaseFile).ToList();
    }
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
