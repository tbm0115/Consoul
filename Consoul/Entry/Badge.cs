using System;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent badge construction over <see cref="BadgeEntry"/>.
    /// </summary>
    public class Badge : BadgeEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Badge"/> class.
        /// </summary>
        /// <param name="label">The badge label to render.</param>
        /// <param name="kind">The visual intent of the badge.</param>
        public Badge(string label, BadgeKind kind = BadgeKind.Default) : base(label, kind)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Badge"/> class with explicit options.
        /// </summary>
        /// <param name="label">The badge label to render.</param>
        /// <param name="kind">The visual intent of the badge.</param>
        /// <param name="options">Optional rendering options for the badge.</param>
        public Badge(string label, BadgeKind kind, BadgeOptions options) : base(label, kind, options)
        {
        }

        /// <summary>
        /// Sets the badge label.
        /// </summary>
        /// <param name="label">The badge label.</param>
        /// <returns>The current <see cref="Badge"/> instance.</returns>
        public Badge LabelText(string label)
        {
            Label = label ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the badge visual intent.
        /// </summary>
        /// <param name="kind">The badge visual intent.</param>
        /// <returns>The current <see cref="Badge"/> instance.</returns>
        public Badge KindOf(BadgeKind kind)
        {
            Kind = kind;
            return this;
        }

        /// <summary>
        /// Marks the badge as informational.
        /// </summary>
        /// <returns>The current <see cref="Badge"/> instance.</returns>
        public Badge Info()
        {
            Kind = BadgeKind.Info;
            return this;
        }

        /// <summary>
        /// Marks the badge as success.
        /// </summary>
        /// <returns>The current <see cref="Badge"/> instance.</returns>
        public Badge Success()
        {
            Kind = BadgeKind.Success;
            return this;
        }

        /// <summary>
        /// Marks the badge as warning.
        /// </summary>
        /// <returns>The current <see cref="Badge"/> instance.</returns>
        public Badge Warning()
        {
            Kind = BadgeKind.Warning;
            return this;
        }

        /// <summary>
        /// Marks the badge as error.
        /// </summary>
        /// <returns>The current <see cref="Badge"/> instance.</returns>
        public Badge Error()
        {
            Kind = BadgeKind.Error;
            return this;
        }

        /// <summary>
        /// Sets the maximum rendered badge width.
        /// </summary>
        /// <param name="width">The maximum rendered badge width.</param>
        /// <returns>The current <see cref="Badge"/> instance.</returns>
        public Badge MaxWidth(int? width)
        {
            Options.MaxWidth = width;
            return this;
        }

        /// <summary>
        /// Sets the number of spaces written before and after the badge label.
        /// </summary>
        /// <param name="padding">The horizontal padding amount.</param>
        /// <returns>The current <see cref="Badge"/> instance.</returns>
        public Badge Padding(int padding)
        {
            Options.HorizontalPadding = padding;
            return this;
        }

        /// <summary>
        /// Sets whether the label should be uppercased.
        /// </summary>
        /// <param name="uppercase">Whether the label should be uppercased.</param>
        /// <returns>The current <see cref="Badge"/> instance.</returns>
        public Badge Uppercase(bool uppercase = true)
        {
            Options.Uppercase = uppercase;
            return this;
        }

        /// <summary>
        /// Sets the left and right badge edge text.
        /// </summary>
        /// <param name="left">The left edge text.</param>
        /// <param name="right">The right edge text.</param>
        /// <returns>The current <see cref="Badge"/> instance.</returns>
        public Badge Edges(string left, string right)
        {
            Options.LeftEdge = left ?? string.Empty;
            Options.RightEdge = right ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the badge color scheme for its current kind.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        /// <returns>The current <see cref="Badge"/> instance.</returns>
        public Badge Color(ConsoleColor color, ConsoleColor? backgroundColor = null)
        {
            ColorScheme scheme = Scheme(color, backgroundColor);
            SetSchemeForKind(scheme);
            return this;
        }

        /// <summary>
        /// Sets the badge foreground color for its current kind.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="Badge"/> instance.</returns>
        public Badge Color(SpectreColor color)
        {
            SetSchemeForKind(Scheme(color));
            return this;
        }

        /// <summary>
        /// Sets the badge foreground color for its current kind.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="Badge"/> instance.</returns>
        public Badge Color(RgbColor color)
        {
            SetSchemeForKind(Scheme(color));
            return this;
        }

        /// <summary>
        /// Sets the badge color scheme for its current kind from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Badge"/> instance.</returns>
        public Badge Color(Style style)
        {
            SetSchemeForKind((style ?? Style.Plain).ToColorScheme(RenderOptions.DefaultScheme));
            return this;
        }

        /// <summary>
        /// Sets the badge color scheme for its current kind from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Badge"/> instance.</returns>
        public Badge Color(string style)
        {
            SetSchemeForKind(Style.ParseScheme(style, RenderOptions.DefaultScheme));
            return this;
        }

        private void SetSchemeForKind(ColorScheme scheme)
        {
            switch (Kind)
            {
                case BadgeKind.Info:
                    Options.InfoScheme = scheme;
                    break;
                case BadgeKind.Success:
                    Options.SuccessScheme = scheme;
                    break;
                case BadgeKind.Warning:
                    Options.WarningScheme = scheme;
                    break;
                case BadgeKind.Error:
                    Options.ErrorScheme = scheme;
                    break;
                default:
                    Options.DefaultScheme = scheme;
                    break;
            }
        }

        private static ColorScheme Scheme(ConsoleColor color, ConsoleColor? backgroundColor)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.GetBackgroundColorOrDefault(backgroundColor)
            };
        }

        private static ColorScheme Scheme(SpectreColor color)
        {
            return Scheme(color.ToConsoleColor(), null);
        }

        private static ColorScheme Scheme(RgbColor color)
        {
            return Scheme(color.ToConsoleColor(), null);
        }
    }
}
