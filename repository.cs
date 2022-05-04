using ILeoConsole.Core;
using System.Text.Json;

namespace LeoConsole_apkg {
  public class ApkgRepository {
    private ApkgUtils utils = new ApkgUtils();
    private ApkgOutput output = new ApkgOutput();
    private IList<RepoPackage> index;

    public string GetUrlFor(string package, string savePath) {
      foreach (RepoPackage p in index) {
        if (p.name == package) {
          return p.url;
        }
      }
      throw new Exception("cannot find package");
    }

    public void Reload(string savePath) {
      string reposListFile = Path.Join(savePath, "var", "apkg", "repos");
      output.MessageSuc0("reloading package index");
      IList<RepoPackage> newIndex = Enumerable.Empty<RepoPackage>().ToList();
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
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
