using System.Text.Json;

namespace LeoConsole_apkg {
  public class FileUtils {
    // DeleteParentDirs() {{{
    public static void DeleteParentDirs(string file, string savePath) {
      string parent = Directory.GetParent(file).FullName;
      while (parent != savePath) {
        if (Directory.EnumerateFileSystemEntries(parent).Any()) {
          break;
        }
        Directory.Delete(parent, true);
        parent = Directory.GetParent(parent).FullName;
      }
    } // }}}

    // DeleteFiles() {{{
    public static void DeleteFiles(string[] files, string savePath) {
      foreach (string f in files) {
        string path = Path.Join(savePath, f);
        ApkgOutput.MessageSuc1("deleting " + path);
        File.Delete(path);
        FileUtils.DeleteParentDirs(path, savePath);
      }
    } // }}}

    // ReadManifest() {{{
    public static PkgArchiveManifest ReadManifest(string folder) {
      string text = File.ReadAllText(Path.Join(folder, "PKGINFO.json"));
      return JsonSerializer.Deserialize<PkgArchiveManifest>(text);
    } // }}}
  }
}
