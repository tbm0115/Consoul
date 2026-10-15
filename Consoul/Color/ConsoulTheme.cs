using ConsoulLibrary.Color;
using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Describes the global color schemes used by Consoul rendering helpers.
    /// </summary>
    public sealed class ConsoulTheme
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoulTheme"/> class.
        /// </summary>
        public ConsoulTheme()
        {
            DefaultScheme = Scheme(ConsoleColor.White, ConsoleColor.Black);
            PromptScheme = Scheme(ConsoleColor.Yellow, ConsoleColor.Black);
            SubnoteScheme = Scheme(ConsoleColor.Gray, ConsoleColor.Black);
            InvalidScheme = Scheme(ConsoleColor.Red, ConsoleColor.Black);
            OptionScheme = Scheme(ConsoleColor.DarkYellow, ConsoleColor.Black);
            RoutineInputScheme = Scheme(ConsoleColor.Cyan, ConsoleColor.Black);
        }

        /// <summary>
        /// Gets a theme matching Consoul's default colors.
        /// </summary>
        public static ConsoulTheme Classic => new ConsoulTheme();

        /// <summary>
        /// Gets a theme with a brighter dashboard-oriented color set.
        /// </summary>
        public static ConsoulTheme Modern => new ConsoulTheme
        {
            DefaultScheme = Scheme(ConsoleColor.White, ConsoleColor.Black),
            PromptScheme = Scheme(ConsoleColor.Cyan, ConsoleColor.Black),
            SubnoteScheme = Scheme(ConsoleColor.DarkGray, ConsoleColor.Black),
            InvalidScheme = Scheme(ConsoleColor.Red, ConsoleColor.Black),
            OptionScheme = Scheme(ConsoleColor.Green, ConsoleColor.Black),
            RoutineInputScheme = Scheme(ConsoleColor.Magenta, ConsoleColor.Black)
        };

        /// <summary>
        /// Gets a high-contrast theme for demos and terminals with low color fidelity.
        /// </summary>
        public static ConsoulTheme HighContrast => new ConsoulTheme
        {
            DefaultScheme = Scheme(ConsoleColor.White, ConsoleColor.Black),
            PromptScheme = Scheme(ConsoleColor.Black, ConsoleColor.White),
            SubnoteScheme = Scheme(ConsoleColor.White, ConsoleColor.DarkGray),
            InvalidScheme = Scheme(ConsoleColor.White, ConsoleColor.DarkRed),
            OptionScheme = Scheme(ConsoleColor.Black, ConsoleColor.Yellow),
            RoutineInputScheme = Scheme(ConsoleColor.Black, ConsoleColor.Cyan)
        };

        /// <summary>
        /// Gets a low-emphasis theme for logs and CI output where color is secondary.
        /// </summary>
        public static ConsoulTheme Monochrome => new ConsoulTheme
        {
            DefaultScheme = Scheme(ConsoleColor.Gray, ConsoleColor.Black),
            PromptScheme = Scheme(ConsoleColor.White, ConsoleColor.Black),
            SubnoteScheme = Scheme(ConsoleColor.DarkGray, ConsoleColor.Black),
            InvalidScheme = Scheme(ConsoleColor.White, ConsoleColor.Black),
            OptionScheme = Scheme(ConsoleColor.Gray, ConsoleColor.Black),
            RoutineInputScheme = Scheme(ConsoleColor.Gray, ConsoleColor.Black)
        };

        /// <summary>
        /// Gets or sets the color scheme used for default text.
        /// </summary>
        public ColorScheme DefaultScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for prompt text and view titles.
        /// </summary>
        public ColorScheme PromptScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for supplementary or muted text.
        /// </summary>
        public ColorScheme SubnoteScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for validation and error text.
        /// </summary>
        public ColorScheme InvalidScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for selectable options and accents.
        /// </summary>
        public ColorScheme OptionScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for routine-provided input values.
        /// </summary>
        public ColorScheme RoutineInputScheme { get; set; }

        /// <summary>
        /// Captures the current render options as a theme.
        /// </summary>
        /// <returns>A theme populated from the active <see cref="RenderOptions"/> values.</returns>
        public static ConsoulTheme FromRenderOptions()
        {
            return new ConsoulTheme
            {
                DefaultScheme = RenderOptions.DefaultScheme,
                PromptScheme = RenderOptions.PromptScheme,
                SubnoteScheme = RenderOptions.SubnoteScheme,
                InvalidScheme = RenderOptions.InvalidScheme,
                OptionScheme = RenderOptions.OptionScheme,
                RoutineInputScheme = RenderOptions.RoutineInputScheme
            };
        }

        /// <summary>
        /// Creates a copy of the current theme.
        /// </summary>
        /// <returns>A new <see cref="ConsoulTheme"/> with the same color schemes.</returns>
        public ConsoulTheme Clone()
        {
            return new ConsoulTheme
            {
                DefaultScheme = DefaultScheme,
                PromptScheme = PromptScheme,
                SubnoteScheme = SubnoteScheme,
                InvalidScheme = InvalidScheme,
                OptionScheme = OptionScheme,
                RoutineInputScheme = RoutineInputScheme
            };
        }

        /// <summary>
        /// Applies this theme to <see cref="RenderOptions"/>.
        /// </summary>
        public void Apply()
        {
            RenderOptions.ApplyTheme(this);
        }

        /// <summary>
        /// Applies this theme and returns a scope that restores the previous theme when disposed.
        /// </summary>
        /// <returns>A disposable scope that restores the previous render theme.</returns>
        public IDisposable ApplyScoped()
        {
            return RenderOptions.UseTheme(this);
        }

        internal void ApplyToRenderOptions()
        {
            RenderOptions.DefaultScheme = DefaultScheme;
            RenderOptions.PromptScheme = PromptScheme;
            RenderOptions.SubnoteScheme = SubnoteScheme;
            RenderOptions.InvalidScheme = InvalidScheme;
            RenderOptions.OptionScheme = OptionScheme;
            RenderOptions.RoutineInputScheme = RoutineInputScheme;
        }

        private static ColorScheme Scheme(ConsoleColor color, ConsoleColor backgroundColor)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = backgroundColor
            };
        }
    }
}
