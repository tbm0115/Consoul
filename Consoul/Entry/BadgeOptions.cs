using ConsoulLibrary.Color;
using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for compact single-line badges.
    /// </summary>
    public class BadgeOptions
    {
        /// <summary>
        /// Gets or sets the maximum rendered badge width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? MaxWidth { get; set; }

        /// <summary>
        /// Gets or sets the number of spaces written before and after the badge label.
        /// </summary>
        public int HorizontalPadding { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether badge labels should be uppercased before rendering.
        /// </summary>
        public bool Uppercase { get; set; } = true;

        /// <summary>
        /// Gets or sets the left edge text written before the badge label.
        /// </summary>
        public string LeftEdge { get; set; } = "[";

        /// <summary>
        /// Gets or sets the right edge text written after the badge label.
        /// </summary>
        public string RightEdge { get; set; } = "]";

        /// <summary>
        /// Gets or sets the text used to indicate that a badge label was truncated.
        /// </summary>
        public string Ellipsis { get; set; } = "...";

        /// <summary>
        /// Gets or sets the color scheme used for neutral badges.
        /// </summary>
        public ColorScheme? DefaultScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for informational badges.
        /// </summary>
        public ColorScheme? InfoScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for success badges.
        /// </summary>
        public ColorScheme? SuccessScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for warning badges.
        /// </summary>
        public ColorScheme? WarningScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for error badges.
        /// </summary>
        public ColorScheme? ErrorScheme { get; set; }

        /// <summary>
        /// Creates a copy of the current badge options.
        /// </summary>
        /// <returns>A new <see cref="BadgeOptions"/> instance with the same values.</returns>
        public BadgeOptions Clone()
        {
            return new BadgeOptions
            {
                MaxWidth = MaxWidth,
                HorizontalPadding = HorizontalPadding,
                Uppercase = Uppercase,
                LeftEdge = LeftEdge,
                RightEdge = RightEdge,
                Ellipsis = Ellipsis,
                DefaultScheme = DefaultScheme,
                InfoScheme = InfoScheme,
                SuccessScheme = SuccessScheme,
                WarningScheme = WarningScheme,
                ErrorScheme = ErrorScheme
            };
        }

        internal ColorScheme ResolveScheme(BadgeKind kind)
        {
            switch (kind)
            {
                case BadgeKind.Info:
                    return InfoScheme ?? RenderOptions.PromptScheme;
                case BadgeKind.Success:
                    return SuccessScheme ?? Scheme(ConsoleColor.Black, ConsoleColor.Green);
                case BadgeKind.Warning:
                    return WarningScheme ?? Scheme(ConsoleColor.Black, ConsoleColor.Yellow);
                case BadgeKind.Error:
                    return ErrorScheme ?? Scheme(ConsoleColor.White, ConsoleColor.DarkRed);
                default:
                    return DefaultScheme ?? RenderOptions.SubnoteScheme;
            }
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
