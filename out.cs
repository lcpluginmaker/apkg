
namespace LeoConsole_apkg {
  public class ApkgOutput {
    // PRINTING NICE MESSAGES
    public void MessageSuc0(string msg) {
      writeColoredLine("§a==>§r " + msg);
    }
    public void MessageSuc1(string msg) {
      writeColoredLine("  §a->§r " + msg);
    }

    public void MessageErr0(string msg) {
      writeColoredLine("§c=>§r error: " + msg);
    }
    public void MessageErr1(string msg) {
      writeColoredLine("  §c->§r error: " + msg);
    }

    public void MessageWarn0(string msg) {
      writeColoredLine("§e=>§r warning: " + msg);
    }
    public void MessageWarn1(string msg) {
      writeColoredLine("  §e->§r warning: " + msg);
    }

    // TODO only temporary, until plugins have access to LeoConsole's native funtion for thar
    private void writeColoredLine(string value) {
        Char[] chars = value.ToCharArray();
        for (int i = 0; i < chars.Length; i++) {
            switch (chars[i]) {
                case '§':
                    switch (chars[i + 1]) {
                        case '0': Console.ForegroundColor = ConsoleColor.Black; i++; break;
                        case '1': Console.ForegroundColor = ConsoleColor.DarkBlue; i++; break;
                        case '2': Console.ForegroundColor = ConsoleColor.DarkGreen; i++; break;
                        case '3': Console.ForegroundColor = ConsoleColor.DarkCyan; i++; break;
                        case '4': Console.ForegroundColor = ConsoleColor.DarkRed; i++; break;
                        case '5': Console.ForegroundColor = ConsoleColor.DarkMagenta; i++; break;
                        case '6': Console.ForegroundColor = ConsoleColor.DarkYellow; i++; break;
                        case '7': Console.ForegroundColor = ConsoleColor.Gray; i++; break;
                        case '8': Console.ForegroundColor = ConsoleColor.DarkGray; i++; break;
                        case '9': Console.ForegroundColor = ConsoleColor.Blue; i++; break;
                        case 'a': Console.ForegroundColor = ConsoleColor.Green; i++; break;
                        case 'b': Console.ForegroundColor = ConsoleColor.Cyan; i++; break;
                        case 'c': Console.ForegroundColor = ConsoleColor.Red; i++; break;
                        case 'd': Console.ForegroundColor = ConsoleColor.Magenta; i++; break;
                        case 'e': Console.ForegroundColor = ConsoleColor.Yellow; i++; break;
                        case 'f': Console.ForegroundColor = ConsoleColor.White; i++; break;
                        case 'r': Console.ResetColor(); i++; break;
                        default: break;
                    } break;
                default: Console.Write(chars[i]); break;
            }
        }
        Console.Write("\n"); Console.ResetColor();
    }
  }
}

// vim: tabstop=2 softtabstop=2 shiftwidth=2 expandtab
