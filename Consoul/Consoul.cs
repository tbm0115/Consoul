using System;

namespace Consoul
{
    public static class Consoul
    {
        public static void Wait()
        {
            Consoul.Write("Press enter to continue...", ConsoleColor.Gray);
            Console.ReadLine();
        }
        public static string Input(string message, ConsoleColor color = ConsoleColor.White, bool allowEmpty = false)
        {
            string output = string.Empty;
            bool valid = false;
            do
            {
                //Console.Clear();
                Write(message, color);
                output = Console.ReadLine();
                if (allowEmpty)
                {
                    valid = true;
                }
                else if (!string.IsNullOrEmpty(output))
                {
                    valid = true;
                }
            } while (!valid);
            return output;
        }
        public static void Write(string message, ConsoleColor color = ConsoleColor.White, bool writeLine = true)
        {
            Console.ForegroundColor = color;
            if (writeLine)
            {
                Console.WriteLine(message);
            }
            else
            {
                Console.Write(message);
            }
        }
        public static bool Ask(string message, bool clear = false, bool allowEmpty = false)
        {
            string input = "";
            do
            {
                if (clear)
                {
                    Console.Clear();
                }
                Write(message, ConsoleColor.Yellow);
                Write("(Y=Yes, N" + (allowEmpty ? " or Press Enter" : "") + "=No)", ConsoleColor.Gray);
                input = Console.ReadLine();
                if (input.ToLower() != "y" && input.ToLower() != "n" && !string.IsNullOrEmpty(input))
                {
                    Write("Invalid input!", ConsoleColor.Red);
                }
            } while ((allowEmpty ? false : string.IsNullOrEmpty(input)) && input.ToLower() != "y" && input.ToLower() != "n");
            return input.ToLower() == "y";
        }
        public static int Prompt(string message, bool clear = false, params string[] options)
        {
            return (new Prompt(message, clear, options)).Run();

        }
        public static int prompt(string message, PromptOption[] options, bool clear = false)
        {
            return (new Prompt(message, clear, options)).Run();
        }

    }

}
