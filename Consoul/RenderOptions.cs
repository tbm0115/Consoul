using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;

namespace ConsoulLibrary {

    /// <summary>
    /// Provides global rendering defaults for Consoul output, prompts, routines, and view behavior.
    /// </summary>
    public static class RenderOptions
    {
        /// <summary>
        /// Default color scheme for general the Write method(s)
        /// </summary>
        public static ColorScheme DefaultScheme { get; set; } = new ColorScheme()
        {
            Color = ConsoleColor.White,
            BackgroundColor = ConsoleColor.Black
        };
        /// <summary>
        /// Gets the foreground color from <see cref="DefaultScheme"/>.
        /// </summary>
        public static ConsoleColor DefaultColor => DefaultScheme.Color;

        /// <summary>
        /// Default color scheme of main Prompt messages
        /// </summary>
        public static ColorScheme PromptScheme { get; set; } = new ColorScheme()
        {
            Color = ConsoleColor.Yellow,
            BackgroundColor = ConsoleColor.Black
        };
        /// <summary>
        /// Gets the foreground color from <see cref="PromptScheme"/>.
        /// </summary>
        public static ConsoleColor PromptColor => PromptScheme.Color;

        /// <summary>
        /// Default color scheme for supplementary or help text
        /// </summary>
        public static ColorScheme SubnoteScheme { get; set; } = new ColorScheme()
        {
            Color = ConsoleColor.Gray,
            BackgroundColor = ConsoleColor.Black
        };
        /// <summary>
        /// Gets the foreground color from <see cref="SubnoteScheme"/>.
        /// </summary>
        public static ConsoleColor SubnoteColor => SubnoteScheme.Color;

        /// <summary>
        /// Default color for Invalid Operation messages
        /// </summary>
        public static ColorScheme InvalidScheme { get; set; } = new ColorScheme()
        {
            Color = ConsoleColor.Red,
            BackgroundColor = ConsoleColor.Black
        };
        /// <summary>
        /// Gets the foreground color from <see cref="InvalidScheme"/>.
        /// </summary>
        public static ConsoleColor InvalidColor => InvalidScheme.Color;

        /// <summary>
        /// Default color for PromptOptions
        /// </summary>
        public static ColorScheme OptionScheme { get; set; } = new ColorScheme()
        {
            Color = ConsoleColor.DarkYellow,
            BackgroundColor = ConsoleColor.Black
        };
        /// <summary>
        /// Gets the foreground color from <see cref="OptionScheme"/>.
        /// </summary>
        public static ConsoleColor OptionColor => OptionScheme.Color;

        /// <summary>
        /// Default color for rendering Routine input values
        /// </summary>
        public static ColorScheme RoutineInputScheme { get; set; } = new ColorScheme()
        {
            Color = ConsoleColor.Cyan,
            BackgroundColor = ConsoleColor.Black
        };
        /// <summary>
        /// Gets the foreground color from <see cref="RoutineInputScheme"/>.
        /// </summary>
        public static ConsoleColor RoutineInputColor => RoutineInputScheme.Color;

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
        public static string DefaultGoBackMessage { get; set; } = "←\tGo Back";

        /// <summary>
        /// Gets or sets the message shown when Consoul waits for the user to continue.
        /// </summary>
        public static string ContinueMessage { get; set; } = "Press enter to continue…";

        /// <summary>
        /// Upon error within a view, Consoul will Wait() after capturing the error and displaying the message.
        /// </summary>
        public static bool WaitOnError { get; set; } = false;

        /// <summary>
        /// Defines how Consoul applies write suppression rules.
        /// </summary>
        public enum WriteModes
        {
            /// <summary>
            /// Writes all messages
            /// </summary>
            WriteAll,
            /// <summary>
            /// Suppresses all messages
            /// </summary>
            SuppressAll,
            /// <summary>
            /// Suppresses messages of certain <see cref="ConsoleColor"/>
            /// </summary>
            SuppressBlacklist
        }

        /// <summary>
        /// Returns the provided non-null <paramref name="color"/> or <see cref="DefaultScheme"/> color
        /// </summary>
        /// <param name="color">Attempted color to use in the console.</param>
        /// <returns>Non-null color</returns>
        public static ConsoleColor GetColorOrDefault(ConsoleColor? color = null)
        {
            if (color == null)
                color = DefaultScheme.Color;
            return (ConsoleColor)color;
        }

        /// <summary>
        /// Returns the provided non-null <paramref name="color"/> or <see cref="DefaultScheme"/> color
        /// </summary>
        /// <param name="color">Attempted color to use in the console.</param>
        /// <returns>Non-null color</returns>
        public static ConsoleColor GetBackgroundColorOrDefault(ConsoleColor? color = null)
        {
            if (color == null)
                color = DefaultScheme.BackgroundColor;
            return (ConsoleColor)color;
        }
    }
}
