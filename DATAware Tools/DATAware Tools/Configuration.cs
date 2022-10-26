using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATAware_Tools
{
    public static class Configuration
    {
        public static class NamedPipe
        {
            public static string MessageReceived { get; set; } = "received";
            public static string MessageSent { get; set; } = "sent";
        }

        public static class FileNameConvention
        {
            private static string BaseName { get; } = "/MasterOut_{0}.csv";
            public static string MergeToolOutput { get; set; } = string.Format(BaseName, "Merged");
            public static string ConvertToolOutput { get; set; } = string.Format(BaseName, "Converted");
        }
    }

    public static class DataStructure
    {
        public enum extensions
        {
            csv,
            xls,
            xlsx,
            fixedWidth
        }
    }

    public static class ConsoleHelper
    {
        public static void clearLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        public static void WriteInline(string line)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            line += new string(' ', Console.WindowWidth - line.Length - 1);
            Console.Write(line);
        }
    }
}
