
namespace LeoConsole_apkg {
  public class ApkgInstaller {
    private ApkgOutput output = new ApkgOutput();
    private ApkgUtils utils = new ApkgUtils();

    // from local folder
    public void GetFile(string folder, string savePath) {
      output.MessageSuc0("installing from local folder...");
      if (!Directory.Exists(folder)) {
        output.MessageErr1("" + folder + " does not exist");
        return;
      }
      if (!utils.CompileFolder(folder)) {
        return;
      }
      if (!utils.InstallDLLs(folder, savePath)) {
        return;
      }
      output.MessageSuc0("" + folder + " was installed. restart LeoConsole to load it");
    }

    // compiled dll from http
    public void GetHTTP(string url, string dlPath, string savePath) {
      output.MessageSuc0("installing compiled dll file");
      string downloadPath = Path.Join(dlPath, "plugins", Path.GetFileName(url));
      if (!utils.CreatePluginsDownloadDir(dlPath)) {
        return;
      }
      if (!utils.DownloadFile(url, downloadPath)) {
        return;
      }
      output.MessageSuc0("installing downloaded dll...");
      try {
        File.Copy(downloadPath, Path.Join(savePath, "plugins", Path.GetFileName(downloadPath)), true);
      } catch (Exception e) {
        output.MessageErr1("cannot instal: " + e.Message);
        return;
      }
      output.MessageSuc0("" + url + " was installed. restart LeoConsole to enable it.");
    }

    // compiling from git repository
    public void GetGit(string url, string dlPath, string savePath) {
      output.MessageSuc0("installing from git repository");
      string name = Path.GetFileName(url).Split(".")[0];
      try {
        if (Directory.Exists(Path.Join(dlPath, "plugins", name))) {
          output.MessageSuc1("download directory already exists. override [y/n] ? ");
          string answer = Console.ReadLine();
          switch (answer) {
            case "y":
              if (!utils.DeleteDirectory(Path.Join(dlPath, "plugins", name))) {
                return;
              }
              break;
            case "Y":
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
      if (!utils.CompileFolder(Path.Join(dlPath, "plugins", name))) {
        return;
      }
      if (!utils.InstallDLLs(Path.Join(dlPath, "plugins", name), savePath)) {
        return;
      }
      output.MessageSuc0("" + url + " was installed. restart LeoConsole to load it");
      return;
    }
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
