
namespace LeoConsole_apkg {
  // Package Archive {{{
  public class PkgArchiveManifest {
    public float manifestVersion { get; set; }
    public string packageName { get; set; }
    public string packageVersion { get; set; }
    public string packageOS { get; set; }
    public string[] compatibleVersions { get; set; }
    public string[] depends { get; set; }
    public string[] files { get; set; }
    public ManifestProjectData project { get; set; }
  }

  public class ManifestProjectData {
    public string maintainer { get; set; }
    public string email { get; set; }
    public string homepage { get; set; }
    public string bugTracker { get; set; }
  }
  // }}}

  // Repo Index {{{
  public class RepoIndex {
    public float indexVersion { get; set; }
    public string name { get; set; }
    public string url { get; set; }
    public IndexProjectData project { get; set; }
    public RepoPackage[] packageList { get; set; }
  }

  public class RepoPackage {
    public string name { get; set; }
    public string description { get; set; }
    public string version { get; set; }
    public string os { get; set; }
    public string[] lc { get; set; }
    public string[] depends { get; set; }
    public string url { get; set; }
  }

  public class IndexProjectData {
    public string description { get; set; }
    public string maintainer { get; set; }
    public string email { get; set; }
    public string homepage { get; set; }
    public string bugTracker { get; set; }
  }
  // }}}
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
