using System;
using System.Collections.Generic;
using System.Linq;

namespace Consoul
{
    public static class Consoul
    {
        //public static Options Options { get; set; } = new Options();

        public static void Wait()
        {
            Consoul._write("Press enter to continue...", RenderOptions.SubnoteColor);
            Console.ReadLine();
        }
        public static string Input(string message, ConsoleColor? color = null, bool allowEmpty = false)
        {
            string output = string.Empty;
            bool valid = false;
            do
            {
                //Console.Clear();
                Consoul._write(message, RenderOptions.GetColor(color));
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
        internal static void _write(string message, ConsoleColor color, bool writeLine = true)
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
        public static void Write(string message, ConsoleColor? color = null, bool writeLine = true)
        {
            switch (RenderOptions.WriteMode)
            {
                case RenderOptions.WriteModes.SuppressAll:
                    // Do nothing
                    break;
                case RenderOptions.WriteModes.SuppressBlacklist:
                    if (!RenderOptions.BlacklistColors.Any(c => c == color))
                    {
                        _write(message, RenderOptions.GetColor(color), writeLine);
                    }
                    break;
                default: // Include WriteAll
                    _write(message, RenderOptions.GetColor(color), writeLine);
                    break;
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
                Consoul._write(message, RenderOptions.PromptColor);
                Consoul._write("(Y=Yes, N" + (allowEmpty ? " or Press Enter" : "") + "=No)", RenderOptions.SubnoteColor);
                input = Console.ReadLine();
                if (input.ToLower() != "y" && input.ToLower() != "n" && !string.IsNullOrEmpty(input))
                {
                    Consoul._write("Invalid input!", RenderOptions.InvalidColor);
                }
            } while ((allowEmpty ? false : string.IsNullOrEmpty(input)) && input.ToLower() != "y" && input.ToLower() != "n");
            return input.ToLower() == "y";
        }
        public static int Prompt(string message, bool clear = false, params string[] options)
        {
            return (new Prompt(message, clear, options)).Run();

        }
        public static int Prompt(string message, PromptOption[] options, bool clear = false)
        {
            return (new Prompt(message, clear, options)).Run();
        }

    }
    public static class RenderOptions
    {
        public static ConsoleColor DefaultColor { get; set; } = ConsoleColor.White;
        public static ConsoleColor PromptColor { get; set; } = ConsoleColor.Yellow;
        public static ConsoleColor SubnoteColor { get; set; } = ConsoleColor.Gray;
        public static ConsoleColor InvalidColor { get; set; } = ConsoleColor.Red;
        public static ConsoleColor OptionColor { get; set; } = ConsoleColor.DarkYellow;
        public static List<ConsoleColor> BlacklistColors { get; set; } = new List<ConsoleColor>();
        public static WriteModes WriteMode { get; set; } = WriteModes.WriteAll;

        public enum WriteModes
        {
            WriteAll,
            SuppressAll,
            SuppressBlacklist
        }

        public static ConsoleColor GetColor(ConsoleColor? color = null)
        {
            if (color == null)
                color = DefaultColor;
            return (ConsoleColor)color;
        }
    }
}
