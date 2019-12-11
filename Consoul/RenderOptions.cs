using System;
using System.Collections.Generic;

namespace ConsoulLibrary {
    public static class RenderOptions
    {
        /// <summary>
        /// Default color for general the Write method(s)
        /// </summary>
        public static ConsoleColor DefaultColor { get; set; } = ConsoleColor.White;

        /// <summary>
        /// Default color of main Prompt messages
        /// </summary>
        public static ConsoleColor PromptColor { get; set; } = ConsoleColor.Yellow;

        /// <summary>
        /// Default color for supplementary or help text
        /// </summary>
        public static ConsoleColor SubnoteColor { get; set; } = ConsoleColor.Gray;

        /// <summary>
        /// Default color for Invalid Operation messages
        /// </summary>
        public static ConsoleColor InvalidColor { get; set; } = ConsoleColor.Red;

        /// <summary>
        /// Default color for PromptOptions
        /// </summary>
        public static ConsoleColor OptionColor { get; set; } = ConsoleColor.DarkYellow;

        /// <summary>
        /// Messages colored with this will not be rendered
        /// </summary>
        public static List<ConsoleColor> BlacklistColors { get; set; } = new List<ConsoleColor>();

        /// <summary>
        /// Default Write mode for the library
        /// </summary>
        public static WriteModes WriteMode { get; set; } = WriteModes.WriteAll;

        /// <summary>
        /// Default message for the 'Go Back' selection
        /// </summary>
        public static string DefaultGoBackMessage { get; set; } = "<==\tGo Back";

        public static string ContinueMessage { get; set; } = "Press enter to continue...";

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
