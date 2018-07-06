using System;

namespace ckb
{
    class Utils
    {

        static ProgramSettings settings;

        static public void InitUtils(ProgramSettings s)
        {
            settings = s;
        }

        static public void PrintFatal(string message, int code = -1)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("FATAL: " + message);
            Console.ResetColor();
            Environment.Exit(code);
        }

        static public void PrintFatalSoft(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        static public void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        static public void PrintWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        static public void PrintVerbose(string message)
        {
            if (!settings.verbose)
                return;
            Console.WriteLine(message);
        }
    }
}