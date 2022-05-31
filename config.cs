using System.Text.Json;

namespace LeoConsole_apkg {
  // we serialize the json config into this class
  public class ApkgConfig {
    public bool FirstRun { get; set; }
    public bool DebugMode { get; set; }
  }

  public class ApkgConfigHelper {
    public static ApkgConfig ReadConfig(string configFolder) {
      string configFile = Path.Join(configFolder, "config.json");
      // check if exists
      if (!File.Exists(configFile)) {
        ApkgUtils.FirstRun();
        ApkgConfig defConf = new ApkgConfig();
        defConf.FirstRun = false;
        string jsonString = JsonSerializer.Serialize(defConf);
        using (StreamWriter f = new StreamWriter(configFile)) {
          f.WriteLine(jsonString);
        }
      }
      // read
      string contents = File.ReadAllText(configFile);
      return JsonSerializer.Deserialize<ApkgConfig>(contents);
    }
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
