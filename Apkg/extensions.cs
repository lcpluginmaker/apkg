using System;
using System.Text.Json;
using ILeoConsole.Core;

namespace LeoConsole_apkg {
  // metadata serialization classes {{{
  public class ApkgExtensionMetadata {
    public string name { get; set; }
    public float metadataVersion { get; set; }
    public string description { get; set; }
    public string author { get; set; }
    public bool disabled { get; set; }
  }

  public class ApkgExtensionInfo {
    public string Name;
    public string Description;
    public string Author;

    public ApkgExtensionInfo(string n, string d, string a) {
      Name = n;
      Description = d;
      Author = a;
    }
  }

  public class ApkgExtensionRuntimeData {
    public string[] args { get; set; }
    public string savePath { get; set; }
  }
  // }}}

  public class ApkgExtensionManager {
    // variables and init {{{
    private IList<ApkgExtensionMetadata> Extensions = new List<ApkgExtensionMetadata>();

    private string SavePath;
    private string ExtensionsFolder;

    public ApkgExtensionManager(string savePath) {
      SavePath = savePath;
      ExtensionsFolder = Path.Join(SavePath, "share", "apkg", "extensions");
      if (!Directory.Exists(ExtensionsFolder)) {
        Directory.CreateDirectory(ExtensionsFolder);
      }
      LConsole.MessageSuc0("registering apkg extensions");
      string[] files = Directory.GetFiles(ExtensionsFolder);
      foreach (string f in files) {
        if (!f.EndsWith(".json")) {
          continue;
        }

        string baseName = Path.GetFileNameWithoutExtension(f);
        if (!files.Contains(Path.Join(ExtensionsFolder, baseName))) {
          continue;
        }

        try {
          string metadataString = File.ReadAllText(f);
          ApkgExtensionMetadata metadata = JsonSerializer.Deserialize<ApkgExtensionMetadata>(metadataString);
          metadata.name = baseName;
          Extensions.Add(metadata);
          LConsole.MessageSuc1($"registered apkg extension: {metadata.name}");
        } catch {
          LConsole.MessageErr1($"failed to load '{baseName}' apkg extension");
          continue;
        }
      }
    }
    // }}}

    // EncodeData() {{{
    private string EncodeData(string[] args) {
      ApkgExtensionRuntimeData data = new ApkgExtensionRuntimeData();
      data.args = args;
      data.savePath = SavePath;
      return System.Convert.ToBase64String(
          System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data))
          );
    }
    // }}}

    // GetList(){{{
    public IList<ApkgExtensionInfo> GetList() {
      IList<ApkgExtensionInfo> res = new List<ApkgExtensionInfo>();
      foreach (ApkgExtensionMetadata m in Extensions) {
        if (!m.disabled) {
          Console.WriteLine(m.name, m.description, m.author);
          res.Add(new ApkgExtensionInfo(m.name, m.description, m.author));
        }
      }
      return res;
    }
    // }}}

    // Exists() {{{
    public bool Exists(string name) {
      foreach (ApkgExtensionMetadata m in Extensions) {
        if (m.name == name && !m.disabled) {
          return true;
        }
      }
      return false;
    }
    // }}}

    // Run() {{{
    public void Run(string[] args) {
      foreach (ApkgExtensionMetadata m in Extensions) {
        if (m.name != args[1]) {
          continue;
        }

        if (m.disabled) {
          continue;
        }

        if (Processes.Run(
              Path.Join(ExtensionsFolder, m.name),
              EncodeData(new ArraySegment<string>(args, 2, args.Length-2).ToArray()),
              SavePath
              ) != 0) {
          LConsole.MessageErr0($"{m.name} extension failed to run");
        }
        return;
      }
    }
    // }}}
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
