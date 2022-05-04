using ILeoConsole.Core;

namespace LeoConsole_apkg {
  public class ApkgOutput {
    private LConsole console = new LConsole();

    // PRINTING NICE MESSAGES
    public void MessageSuc0(string msg) {
      LConsole.WriteLine("§a==>§r " + msg);
    }
    public void MessageSuc1(string msg) {
      LConsole.WriteLine("  §a->§r " + msg);
    }

    public void MessageErr0(string msg) {
      LConsole.WriteLine("§c==>§r error: " + msg);
    }
    public void MessageErr1(string msg) {
      LConsole.WriteLine("  §c->§r error: " + msg);
    }

    public void MessageWarn0(string msg) {
      LConsole.WriteLine("§e==>§r warning: " + msg);
    }
    public void MessageWarn1(string msg) {
      LConsole.WriteLine("  §e->§r warning: " + msg);
    }
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
