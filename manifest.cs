
namespace LeoConsole_apkg {
  public class ManifestBuildInstruction {
    public string command { get; set; }
    public string[] args { get; set; }
    public string folder { get; set; }
    public string[] dlls { get; set; }
  }

  public class ManifestProjectData {
    public string maintainer { get; set; }
    public string email { get; set; }
    public string homepage { get; set; }
    public string bugTracker { get; set; }
  }

  // class to serialize manifest files into
  public class Manifest {
      public float manifestVersion { get; set; }
      public string packageName { get; set; }
      public ManifestBuildInstruction build { get; set; }
      public ManifestProjectData project { get; set; }
  }

  public class PkgArchiveManifest {
    public string packageName { get; set; }
    public string packageVersion { get; set; }
    public string[] files { get; set; }
    public ManifestProjectData project { get; set; }
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
