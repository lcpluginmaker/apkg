using System.IO;
using System.Diagnostics;
using ILeoConsole;
using ILeoConsole.Plugin;
using ILeoConsole.Core;

namespace LeoConsole_apkg
{
  public class apkg : ICommand
  {
    public string Name { get { return "apkg"; } }
    public string Description { get { return "advaned package management"; } }
    public Action CommandFunktion { get { return () => Command(); } }
    private string[] _InputProperties;
    public string[] InputProperties { get { return _InputProperties; } set { _InputProperties = value; } }
    

    public void Command() {
      try {
        if(_InputProperties.Length > 1) {
          switch (_InputProperties[1]) {
            case "help":
              printHelp();
              break;
            
            case "get":
              if (_InputProperties.Length < 3){
                Console.WriteLine("you need to provide a package name");
                break;
              }
              if (_InputProperties[2].StartsWith("https://")){
                Console.WriteLine("downloading packages is not supported yet");
                break;
              }
              if (_InputProperties[2].StartsWith("file://")){
                string folder = _InputProperties[2].Substring(7, _InputProperties[2].Length-7);
                Console.WriteLine("installing " + folder + "...");
                if (!Directory.Exists(folder)) {
                  Console.WriteLine(folder + "does not exist");
                  break;
                }
                Console.WriteLine("compiling " + folder + " with dotnet...");
                Process p = new Process();
                p.StartInfo.FileName = "dotnet";
                p.StartInfo.Arguments = "build";
                p.StartInfo.WorkingDirectory = folder;
                p.Start();
                p.WaitForExit();
                if (p.ExitCode != 0){
                  Console.WriteLine("failed compiling");
                  break;
                }
                Console.WriteLine("compiled successfully");
                Console.WriteLine("installing compiled plugin...");
                foreach (string filename in Directory.GetFiles(Path.Join(folder, "bin", "Debug", "net6.0"))){
                  if (filename.EndsWith(".dll")){
                    Console.WriteLine("copying " + filename + " to " + Path.Join("data", "plugins", Path.GetFileName(filename)) + "...");
                    File.Copy(filename, Path.Join("data", "plugins", Path.GetFileName(filename)), true);
                  }
                }
                Console.WriteLine(Path.GetFileName(folder) + " was installed. restart LeoConsole to enable it");
                break;
              }
              Console.WriteLine("downloading normal packages is not supported yet");
              break;
            
            default:
              Console.WriteLine("apkg: unnknown option");
              break;
          }
        } else {
          Console.WriteLine("you need to provide an argument");
          printHelp();
        }
      } catch (Exception ex) {
        Console.WriteLine("Exception: " + ex.Message + "\n");
      }
    }

    private void printHelp() {
      Console.WriteLine("apkg is an advanced package tool for LenConsole");
      Console.WriteLine("");
      Console.WriteLine("Besides the default functionality that the standard pkg command provides, it allows you to install plugins from unofficial repositories or even local folders, which is very handy for quick development and testing.");
      Console.WriteLine("");
      Console.WriteLine("Available options:");
      Console.WriteLine("    help: print this help");
      Console.WriteLine("    get:  install plugin");
    }
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab

