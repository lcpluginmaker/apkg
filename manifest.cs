
namespace LeoConsole_apkg {
  public class ManifestProjectData {
    public string maintainer { get; set; }
    public string email { get; set; }
    public string homepage { get; set; }
    public string bugTracker { get; set; }
  }

  public class RepoPackage {
    public string name { get; set; }
    public string description { get; set; }
    public string version { get; set; }
    public string os { get; set; }
    public string url { get; set; }
  }

  public class IndexProjectData {
    public string description { get; set; }
    public string maintainer { get; set; }
    public string email { get; set; }
    public string homepage { get; set; }
    public string bugTracker { get; set; }
  }

  // class to serialize lcpkg manifests into
  public class PkgArchiveManifest {
    public string packageName { get; set; }
    public string packageVersion { get; set; }
    public string[] files { get; set; }
    public ManifestProjectData project { get; set; }
  }

  // class to serialize repo indexes into
  public class RepoIndex {
    public float indexVersion { get; set; }
    public string name { get; set; }
    public string url { get; set; }
    public IndexProjectData project { get; set; }
    public RepoPackage[] packageList { get; set; }
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
