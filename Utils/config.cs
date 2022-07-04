using System.Text.Json;

namespace LeoConsole_apkg {
  // classes to serialize the config in {{{
  public class ConfigRepo {
    public string name { get; set; }
    public string url { get; set; }
  }

  public class ApkgConfig {
    public bool FirstRun { get; set; }
    public bool DebugMode { get; set; }
    public ConfigRepo[] Repositories { get; set; }
  }
  // }}}

  public class ApkgConfigHelper {
    private string DefaultRepoUrl = "https://raw.githubusercontent.com/alexcoder04/LeoConsole-repo-main/main/index.json";

    // DefaultConfig() {{{
    private static ApkgConfig DefaultConfig() {
      ApkgConfig defConf = new ApkgConfig();
      defConf.FirstRun = false;
      defConf.DebugMode = false;

      ConfigRepo defRepo = new ConfigRepo();
      defRepo.name = "main";
      defRepo.url = DefaultRepoUrl;

      defConf.Repositories = new ConfigRepo[]{defRepo};
      return defConf;
    } // }}}

    // InitConfig() {{{
    private static void InitConfig(string configFolder) {
      if (!Directory.Exists(configFolder)) {
        Directory.CreateDirectory(configFolder);
      }
      string jsonString = JsonSerializer.Serialize(DefaultConfig());
      using (StreamWriter f = new StreamWriter(Path.Join(configFolder, "config.json"))) {
        f.WriteLine(jsonString);
      }
    } // }}}

    // ReadConfig() {{{
    public static ApkgConfig ReadConfig(string configFolder) {
      string configFile = Path.Join(configFolder, "config.json");
      // check if exists
      if (!File.Exists(configFile)) {
        ApkgUtils.FirstRun();
        InitConfig(configFolder);
      }
      // read
      string contents = File.ReadAllText(configFile);
      return JsonSerializer.Deserialize<ApkgConfig>(contents);
    }
  } // }}}
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
