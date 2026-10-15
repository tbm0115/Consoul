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
        /// Gets or sets the text decoration used for neutral badges on ANSI-capable terminals.
        /// </summary>
        public Decoration DefaultDecoration { get; set; }

        /// <summary>
        /// Gets or sets the original RGB foreground used for neutral badges on true-color terminals.
        /// </summary>
        public RgbColor? DefaultForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the original RGB background used for neutral badges on true-color terminals.
        /// </summary>
        public RgbColor? DefaultBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for informational badges.
        /// </summary>
        public ColorScheme? InfoScheme { get; set; }

        /// <summary>
        /// Gets or sets the text decoration used for informational badges on ANSI-capable terminals.
        /// </summary>
        public Decoration InfoDecoration { get; set; }

        /// <summary>
        /// Gets or sets the original RGB foreground used for informational badges on true-color terminals.
        /// </summary>
        public RgbColor? InfoForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the original RGB background used for informational badges on true-color terminals.
        /// </summary>
        public RgbColor? InfoBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for success badges.
        /// </summary>
        public ColorScheme? SuccessScheme { get; set; }

        /// <summary>
        /// Gets or sets the text decoration used for success badges on ANSI-capable terminals.
        /// </summary>
        public Decoration SuccessDecoration { get; set; }

        /// <summary>
        /// Gets or sets the original RGB foreground used for success badges on true-color terminals.
        /// </summary>
        public RgbColor? SuccessForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the original RGB background used for success badges on true-color terminals.
        /// </summary>
        public RgbColor? SuccessBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for warning badges.
        /// </summary>
        public ColorScheme? WarningScheme { get; set; }

        /// <summary>
        /// Gets or sets the text decoration used for warning badges on ANSI-capable terminals.
        /// </summary>
        public Decoration WarningDecoration { get; set; }

        /// <summary>
        /// Gets or sets the original RGB foreground used for warning badges on true-color terminals.
        /// </summary>
        public RgbColor? WarningForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the original RGB background used for warning badges on true-color terminals.
        /// </summary>
        public RgbColor? WarningBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for error badges.
        /// </summary>
        public ColorScheme? ErrorScheme { get; set; }

        /// <summary>
        /// Gets or sets the text decoration used for error badges on ANSI-capable terminals.
        /// </summary>
        public Decoration ErrorDecoration { get; set; }

        /// <summary>
        /// Gets or sets the original RGB foreground used for error badges on true-color terminals.
        /// </summary>
        public RgbColor? ErrorForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the original RGB background used for error badges on true-color terminals.
        /// </summary>
        public RgbColor? ErrorBackgroundRgb { get; set; }

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
                DefaultDecoration = DefaultDecoration,
                DefaultForegroundRgb = DefaultForegroundRgb,
                DefaultBackgroundRgb = DefaultBackgroundRgb,
                InfoScheme = InfoScheme,
                InfoDecoration = InfoDecoration,
                InfoForegroundRgb = InfoForegroundRgb,
                InfoBackgroundRgb = InfoBackgroundRgb,
                SuccessScheme = SuccessScheme,
                SuccessDecoration = SuccessDecoration,
                SuccessForegroundRgb = SuccessForegroundRgb,
                SuccessBackgroundRgb = SuccessBackgroundRgb,
                WarningScheme = WarningScheme,
                WarningDecoration = WarningDecoration,
                WarningForegroundRgb = WarningForegroundRgb,
                WarningBackgroundRgb = WarningBackgroundRgb,
                ErrorScheme = ErrorScheme,
                ErrorDecoration = ErrorDecoration,
                ErrorForegroundRgb = ErrorForegroundRgb,
                ErrorBackgroundRgb = ErrorBackgroundRgb
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

        internal StyledBadge ResolveStyle(BadgeKind kind)
        {
            switch (kind)
            {
                case BadgeKind.Info:
                    return new StyledBadge(ResolveScheme(kind), InfoDecoration, InfoForegroundRgb, InfoBackgroundRgb);
                case BadgeKind.Success:
                    return new StyledBadge(ResolveScheme(kind), SuccessDecoration, SuccessForegroundRgb, SuccessBackgroundRgb);
                case BadgeKind.Warning:
                    return new StyledBadge(ResolveScheme(kind), WarningDecoration, WarningForegroundRgb, WarningBackgroundRgb);
                case BadgeKind.Error:
                    return new StyledBadge(ResolveScheme(kind), ErrorDecoration, ErrorForegroundRgb, ErrorBackgroundRgb);
                default:
                    return new StyledBadge(ResolveScheme(kind), DefaultDecoration, DefaultForegroundRgb, DefaultBackgroundRgb);
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

        internal readonly struct StyledBadge
        {
            public StyledBadge(ColorScheme scheme, Decoration decoration, RgbColor? foregroundRgb, RgbColor? backgroundRgb)
            {
                Scheme = scheme;
                Decoration = decoration;
                ForegroundRgb = foregroundRgb;
                BackgroundRgb = backgroundRgb;
            }

            public ColorScheme Scheme { get; }

            public Decoration Decoration { get; }

            public RgbColor? ForegroundRgb { get; }

            public RgbColor? BackgroundRgb { get; }
        }
    }
}
