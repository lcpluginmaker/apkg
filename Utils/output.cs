using ILeoConsole.Core;

// TODO: this functionality will be integrated into ILeoConsole soon
namespace LeoConsole_apkg {
  public class ApkgOutput {
    // PRINTING NICE MESSAGES
    public static void MessageSuc0(string msg) {
      LConsole.WriteLine("§a==>§r " + msg);
    }
    public static void MessageSuc1(string msg) {
      LConsole.WriteLine("  §a->§r " + msg);
    }

    public static void MessageErr0(string msg) {
      LConsole.WriteLine("§c==>§r error: " + msg);
    }
    public static void MessageErr1(string msg) {
      LConsole.WriteLine("  §c->§r error: " + msg);
    }

    public static void MessageWarn0(string msg) {
      LConsole.WriteLine("§e==>§r warning: " + msg);
    }
    public static void MessageWarn1(string msg) {
      LConsole.WriteLine("  §e->§r warning: " + msg);
    }
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
