using System;
using ConsoulLibrary.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a lightweight foreground/background console style parsed from Spectre-style style text.
    /// </summary>
    public sealed class Style
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Style"/> class.
        /// </summary>
        /// <param name="foreground">Optional foreground color.</param>
        /// <param name="background">Optional background color.</param>
        public Style(ConsoleColor? foreground = null, ConsoleColor? background = null)
            : this(Decoration.None, foreground, background, null, null)
        {
        }

        private Style(
            Decoration decoration,
            ConsoleColor? foreground,
            ConsoleColor? background,
            RgbColor? foregroundRgb,
            RgbColor? backgroundRgb)
        {
            Foreground = foreground;
            Background = background;
            Decoration = decoration;
            ForegroundRgb = foregroundRgb;
            BackgroundRgb = backgroundRgb;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Style"/> class.
        /// </summary>
        /// <param name="foreground">Foreground color mapped to the nearest console color.</param>
        public Style(RgbColor foreground)
            : this(Decoration.None, foreground.ToConsoleColor(), null, foreground, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Style"/> class.
        /// </summary>
        /// <param name="foreground">Foreground color mapped to the nearest console color.</param>
        public Style(ConsoulLibrary.Color.Color foreground)
            : this(foreground.ToRgbColor())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Style"/> class.
        /// </summary>
        /// <param name="foreground">Foreground color mapped to the nearest console color.</param>
        /// <param name="background">Background color mapped to the nearest console color.</param>
        public Style(RgbColor foreground, RgbColor background)
            : this(Decoration.None, foreground.ToConsoleColor(), background.ToConsoleColor(), foreground, background)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Style"/> class.
        /// </summary>
        /// <param name="foreground">Foreground color mapped to the nearest console color.</param>
        /// <param name="background">Background color mapped to the nearest console color.</param>
        public Style(ConsoulLibrary.Color.Color foreground, ConsoulLibrary.Color.Color background)
            : this(foreground.ToRgbColor(), background.ToRgbColor())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Style"/> class with text decoration metadata.
        /// </summary>
        /// <param name="decoration">Text decoration flags accepted for Spectre-style compatibility.</param>
        /// <param name="foreground">Optional foreground color.</param>
        /// <param name="background">Optional background color.</param>
        public Style(Decoration decoration, ConsoleColor? foreground = null, ConsoleColor? background = null)
            : this(decoration, foreground, background, null, null)
        {
        }

        private Style(Decoration decoration, RgbColor? foregroundRgb, RgbColor? backgroundRgb)
            : this(
                decoration,
                foregroundRgb.HasValue ? (ConsoleColor?)foregroundRgb.Value.ToConsoleColor() : null,
                backgroundRgb.HasValue ? (ConsoleColor?)backgroundRgb.Value.ToConsoleColor() : null,
                foregroundRgb,
                backgroundRgb)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Style"/> class with text decoration metadata.
        /// </summary>
        /// <param name="foreground">Foreground color mapped to the nearest console color.</param>
        /// <param name="decoration">Text decoration flags accepted for Spectre-style compatibility.</param>
        public Style(RgbColor foreground, Decoration decoration)
            : this(decoration, (RgbColor?)foreground, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Style"/> class with text decoration metadata.
        /// </summary>
        /// <param name="foreground">Foreground color mapped to the nearest console color.</param>
        /// <param name="decoration">Text decoration flags accepted for Spectre-style compatibility.</param>
        public Style(ConsoulLibrary.Color.Color foreground, Decoration decoration)
            : this(foreground.ToRgbColor(), decoration)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Style"/> class with text decoration metadata.
        /// </summary>
        /// <param name="foreground">Foreground color mapped to the nearest console color.</param>
        /// <param name="background">Background color mapped to the nearest console color.</param>
        /// <param name="decoration">Text decoration flags accepted for Spectre-style compatibility.</param>
        public Style(RgbColor foreground, RgbColor background, Decoration decoration)
            : this(decoration, (RgbColor?)foreground, background)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Style"/> class with text decoration metadata.
        /// </summary>
        /// <param name="foreground">Foreground color mapped to the nearest console color.</param>
        /// <param name="background">Background color mapped to the nearest console color.</param>
        /// <param name="decoration">Text decoration flags accepted for Spectre-style compatibility.</param>
        public Style(ConsoulLibrary.Color.Color foreground, ConsoulLibrary.Color.Color background, Decoration decoration)
            : this(foreground.ToRgbColor(), background.ToRgbColor(), decoration)
        {
        }

        /// <summary>
        /// Gets the optional foreground color.
        /// </summary>
        public ConsoleColor? Foreground { get; }

        /// <summary>
        /// Gets the optional background color.
        /// </summary>
        public ConsoleColor? Background { get; }

        /// <summary>
        /// Gets text decoration flags accepted for Spectre-style compatibility.
        /// </summary>
        public Decoration Decoration { get; }

        /// <summary>
        /// Gets the original RGB foreground color when the style was created from RGB or hex input.
        /// </summary>
        public RgbColor? ForegroundRgb { get; }

        /// <summary>
        /// Gets the original RGB background color when the style was created from RGB or hex input.
        /// </summary>
        public RgbColor? BackgroundRgb { get; }

        /// <summary>
        /// Gets a style with no explicit foreground, background, or decoration.
        /// </summary>
        public static Style Plain => new Style();

        /// <summary>
        /// Gets a style with bold decoration metadata.
        /// </summary>
        public static Style Bold => new Style(Decoration.Bold);

        /// <summary>
        /// Gets a style with dim decoration metadata.
        /// </summary>
        public static Style Dim => new Style(Decoration.Dim);

        /// <summary>
        /// Gets a style with italic decoration metadata.
        /// </summary>
        public static Style Italic => new Style(Decoration.Italic);

        /// <summary>
        /// Gets a style with underline decoration metadata.
        /// </summary>
        public static Style Underline => new Style(Decoration.Underline);

        /// <summary>
        /// Gets a style with strikethrough decoration metadata.
        /// </summary>
        public static Style Strikethrough => new Style(Decoration.Strikethrough);

        /// <summary>
        /// Creates a <see cref="ColorScheme"/> by applying this style over a fallback scheme.
        /// </summary>
        /// <param name="fallback">Fallback scheme used when either style component is absent.</param>
        /// <returns>A resolved color scheme.</returns>
        public ColorScheme ToColorScheme(ColorScheme? fallback)
        {
            ColorScheme resolvedFallback = fallback ?? RenderOptions.DefaultScheme;
            return new ColorScheme
            {
                Color = Foreground ?? resolvedFallback.Color,
                BackgroundColor = Background ?? resolvedFallback.BackgroundColor
            };
        }

        /// <summary>
        /// Resolves the foreground color from this style, falling back when the style did not specify one.
        /// </summary>
        /// <param name="fallback">Fallback foreground color.</param>
        /// <returns>The resolved foreground color.</returns>
        public ConsoleColor ToForegroundColor(ConsoleColor fallback)
        {
            return Foreground ?? fallback;
        }

        /// <summary>
        /// Creates a copy of this style with a different foreground color.
        /// </summary>
        /// <param name="foreground">The foreground color, or <see langword="null"/> to leave the foreground unspecified.</param>
        /// <returns>A new style with the requested foreground color.</returns>
        public Style WithForeground(ConsoleColor? foreground)
        {
            return new Style(Decoration, foreground, Background, null, BackgroundRgb);
        }

        /// <summary>
        /// Creates a copy of this style with a different RGB foreground color mapped to the nearest console color.
        /// </summary>
        /// <param name="foreground">The RGB foreground color.</param>
        /// <returns>A new style with the requested foreground color.</returns>
        public Style WithForeground(RgbColor foreground)
        {
            return new Style(Decoration, foreground.ToConsoleColor(), Background, foreground, BackgroundRgb);
        }

        /// <summary>
        /// Creates a copy of this style with a different RGB foreground color mapped to the nearest console color.
        /// </summary>
        /// <param name="foreground">The RGB foreground color.</param>
        /// <returns>A new style with the requested foreground color.</returns>
        public Style WithForeground(ConsoulLibrary.Color.Color foreground)
        {
            return WithForeground(foreground.ToRgbColor());
        }

        /// <summary>
        /// Creates a copy of this style with a different background color.
        /// </summary>
        /// <param name="background">The background color, or <see langword="null"/> to leave the background unspecified.</param>
        /// <returns>A new style with the requested background color.</returns>
        public Style WithBackground(ConsoleColor? background)
        {
            return new Style(Decoration, Foreground, background, ForegroundRgb, null);
        }

        /// <summary>
        /// Creates a copy of this style with a different RGB background color mapped to the nearest console color.
        /// </summary>
        /// <param name="background">The RGB background color.</param>
        /// <returns>A new style with the requested background color.</returns>
        public Style WithBackground(RgbColor background)
        {
            return new Style(Decoration, Foreground, background.ToConsoleColor(), ForegroundRgb, background);
        }

        /// <summary>
        /// Creates a copy of this style with a different RGB background color mapped to the nearest console color.
        /// </summary>
        /// <param name="background">The RGB background color.</param>
        /// <returns>A new style with the requested background color.</returns>
        public Style WithBackground(ConsoulLibrary.Color.Color background)
        {
            return WithBackground(background.ToRgbColor());
        }

        /// <summary>
        /// Creates a copy of this style with replacement decoration metadata.
        /// </summary>
        /// <param name="decoration">The replacement decoration flags.</param>
        /// <returns>A new style with the requested decoration metadata.</returns>
        public Style WithDecoration(Decoration decoration)
        {
            return new Style(decoration, Foreground, Background, ForegroundRgb, BackgroundRgb);
        }

        /// <summary>
        /// Creates a copy of this style with additional decoration metadata.
        /// </summary>
        /// <param name="decoration">The decoration flags to add.</param>
        /// <returns>A new style with the requested decoration metadata added.</returns>
        public Style AddDecoration(Decoration decoration)
        {
            return WithDecoration(Decoration | decoration);
        }

        /// <summary>
        /// Creates a copy of this style with decoration metadata removed.
        /// </summary>
        /// <param name="decoration">The decoration flags to remove.</param>
        /// <returns>A new style without the requested decoration metadata.</returns>
        public Style RemoveDecoration(Decoration decoration)
        {
            return WithDecoration(Decoration & ~decoration);
        }

        /// <summary>
        /// Creates a copy of this style with decoration metadata removed.
        /// </summary>
        /// <param name="decoration">The decoration flags to remove.</param>
        /// <returns>A new style without the requested decoration metadata.</returns>
        public Style WithoutDecoration(Decoration decoration)
        {
            return RemoveDecoration(decoration);
        }

        /// <summary>
        /// Parses a style string such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.
        /// </summary>
        /// <param name="style">The style text to parse.</param>
        /// <returns>The parsed style.</returns>
        public static Style Parse(string style)
        {
            Style parsed;
            if (!TryParse(style, out parsed))
            {
                throw new ArgumentException("Style could not be parsed.", nameof(style));
            }

            return parsed;
        }

        /// <summary>
        /// Tries to parse a style string such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.
        /// </summary>
        /// <param name="style">The style text to parse.</param>
        /// <param name="parsed">The parsed style when parsing succeeds.</param>
        /// <returns><c>true</c> when the style text is recognized; otherwise, <c>false</c>.</returns>
        public static bool TryParse(string style, out Style parsed)
        {
            ColorScheme scheme;
            if (!MarkupEntry.TryParseStyle(style, RenderOptions.DefaultScheme, out scheme))
            {
                parsed = null;
                return false;
            }

            RgbColor? foregroundRgb;
            RgbColor? backgroundRgb;
            TryParseRgbMetadata(style, out foregroundRgb, out backgroundRgb);

            parsed = new Style(
                ParseDecoration(style),
                scheme.Color,
                scheme.BackgroundColor,
                foregroundRgb,
                backgroundRgb);
            return true;
        }

        /// <summary>
        /// Parses a single color token such as <c>green</c>, <c>grey</c>, <c>aqua</c>, or <c>#22c55e</c>.
        /// </summary>
        /// <param name="color">The color text to parse.</param>
        /// <returns>The nearest console color.</returns>
        public static ConsoleColor ParseColor(string color)
        {
            ConsoleColor parsed;
            if (!TryParseColor(color, out parsed))
            {
                throw new ArgumentException("Color could not be parsed.", nameof(color));
            }

            return parsed;
        }

        /// <summary>
        /// Parses a style string and returns its foreground color, or the provided fallback when the style only specified a background.
        /// </summary>
        /// <param name="style">The style text to parse.</param>
        /// <param name="fallback">Fallback foreground color.</param>
        /// <returns>The resolved foreground color.</returns>
        public static ConsoleColor ParseForeground(string style, ConsoleColor fallback)
        {
            return Parse(style).ToForegroundColor(fallback);
        }

        /// <summary>
        /// Tries to parse a single color token such as <c>green</c>, <c>grey</c>, <c>aqua</c>, or <c>#22c55e</c>.
        /// </summary>
        /// <param name="color">The color text to parse.</param>
        /// <param name="parsed">The parsed console color when parsing succeeds.</param>
        /// <returns><c>true</c> when the color text is recognized; otherwise, <c>false</c>.</returns>
        public static bool TryParseColor(string color, out ConsoleColor parsed)
        {
            return MarkupEntry.TryParseColor(color, out parsed);
        }

        internal static ColorScheme ParseScheme(string style, ColorScheme fallback)
        {
            ColorScheme scheme;
            if (!MarkupEntry.TryParseStyle(style, fallback, out scheme))
            {
                throw new ArgumentException("Style could not be parsed.", nameof(style));
            }

            return scheme;
        }

        private static Decoration ParseDecoration(string style)
        {
            return ParseDecorationTokens(style);
        }

        internal static Decoration ParseDecorationTokens(string style)
        {
            if (string.IsNullOrWhiteSpace(style))
            {
                return Decoration.None;
            }

            Decoration decoration = Decoration.None;
            string[] tokens = style.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string token in tokens)
            {
                decoration |= ParseDecorationToken(token);
            }

            return decoration;
        }

        internal static Decoration ParseDecorationToken(string token)
        {
            switch (NormalizeDecorationToken(token))
            {
                case "bold":
                case "b":
                    return Decoration.Bold;
                case "dim":
                case "faint":
                    return Decoration.Dim;
                case "italic":
                case "i":
                    return Decoration.Italic;
                case "underline":
                case "u":
                    return Decoration.Underline;
                case "strikethrough":
                case "strike":
                    return Decoration.Strikethrough;
                case "slowblink":
                case "rapidblink":
                case "blink":
                    return Decoration.Blink;
                case "reverse":
                case "invert":
                    return Decoration.Reverse;
                case "conceal":
                case "hidden":
                    return Decoration.Conceal;
                default:
                    return Decoration.None;
            }
        }

        private static string NormalizeDecorationToken(string token)
        {
            return (token ?? string.Empty)
                .Trim()
                .Replace("-", string.Empty)
                .Replace("_", string.Empty)
                .ToLowerInvariant();
        }

        private static void TryParseRgbMetadata(string style, out RgbColor? foregroundRgb, out RgbColor? backgroundRgb)
        {
            foregroundRgb = null;
            backgroundRgb = null;

            style = RemoveDecorationTokens(style);
            if (string.IsNullOrWhiteSpace(style))
            {
                return;
            }

            string foregroundToken = style;
            string backgroundToken = null;
            int onIndex = style.IndexOf(" on ", StringComparison.OrdinalIgnoreCase);
            if (onIndex >= 0)
            {
                foregroundToken = style.Substring(0, onIndex);
                backgroundToken = style.Substring(onIndex + 4);
            }
            else
            {
                int colonIndex = style.IndexOf(':');
                if (colonIndex >= 0)
                {
                    foregroundToken = style.Substring(0, colonIndex);
                    backgroundToken = style.Substring(colonIndex + 1);
                }
            }

            if (RgbColor.TryFromHex(foregroundToken, out RgbColor foreground))
            {
                foregroundRgb = foreground;
            }

            if (RgbColor.TryFromHex(backgroundToken, out RgbColor background))
            {
                backgroundRgb = background;
            }
        }

        private static string RemoveDecorationTokens(string style)
        {
            if (string.IsNullOrWhiteSpace(style))
            {
                return string.Empty;
            }

            string[] tokens = style.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var builder = new System.Text.StringBuilder(style.Length);
            foreach (string token in tokens)
            {
                if (ParseDecorationToken(token) != Decoration.None)
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(token);
            }

            return builder.ToString();
        }
    }
}
