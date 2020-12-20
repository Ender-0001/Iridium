using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iridium
{
    class Logger
    {
        public static void Info(string stuff)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"INFO: {stuff}");
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void Error(string stuff)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR: {stuff}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Warning(string stuff)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"WARNING: {stuff}");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
