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
        {
            Foreground = foreground;
            Background = background;
            Decoration = Decoration.None;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Style"/> class.
        /// </summary>
        /// <param name="foreground">Foreground color mapped to the nearest console color.</param>
        public Style(RgbColor foreground) : this((ConsoleColor?)foreground.ToConsoleColor())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Style"/> class.
        /// </summary>
        /// <param name="foreground">Foreground color mapped to the nearest console color.</param>
        public Style(ConsoulLibrary.Color.Color foreground) : this((ConsoleColor?)foreground.ToConsoleColor())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Style"/> class.
        /// </summary>
        /// <param name="foreground">Foreground color mapped to the nearest console color.</param>
        /// <param name="background">Background color mapped to the nearest console color.</param>
        public Style(RgbColor foreground, RgbColor background)
            : this((ConsoleColor?)foreground.ToConsoleColor(), background.ToConsoleColor())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Style"/> class.
        /// </summary>
        /// <param name="foreground">Foreground color mapped to the nearest console color.</param>
        /// <param name="background">Background color mapped to the nearest console color.</param>
        public Style(ConsoulLibrary.Color.Color foreground, ConsoulLibrary.Color.Color background)
            : this((ConsoleColor?)foreground.ToConsoleColor(), background.ToConsoleColor())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Style"/> class with text decoration metadata.
        /// </summary>
        /// <param name="decoration">Text decoration flags accepted for Spectre-style compatibility.</param>
        /// <param name="foreground">Optional foreground color.</param>
        /// <param name="background">Optional background color.</param>
        public Style(Decoration decoration, ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            Foreground = foreground;
            Background = background;
            Decoration = decoration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Style"/> class with text decoration metadata.
        /// </summary>
        /// <param name="foreground">Foreground color mapped to the nearest console color.</param>
        /// <param name="decoration">Text decoration flags accepted for Spectre-style compatibility.</param>
        public Style(RgbColor foreground, Decoration decoration)
            : this(decoration, foreground.ToConsoleColor())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Style"/> class with text decoration metadata.
        /// </summary>
        /// <param name="foreground">Foreground color mapped to the nearest console color.</param>
        /// <param name="decoration">Text decoration flags accepted for Spectre-style compatibility.</param>
        public Style(ConsoulLibrary.Color.Color foreground, Decoration decoration)
            : this(decoration, foreground.ToConsoleColor())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Style"/> class with text decoration metadata.
        /// </summary>
        /// <param name="foreground">Foreground color mapped to the nearest console color.</param>
        /// <param name="background">Background color mapped to the nearest console color.</param>
        /// <param name="decoration">Text decoration flags accepted for Spectre-style compatibility.</param>
        public Style(RgbColor foreground, RgbColor background, Decoration decoration)
            : this(decoration, foreground.ToConsoleColor(), background.ToConsoleColor())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Style"/> class with text decoration metadata.
        /// </summary>
        /// <param name="foreground">Foreground color mapped to the nearest console color.</param>
        /// <param name="background">Background color mapped to the nearest console color.</param>
        /// <param name="decoration">Text decoration flags accepted for Spectre-style compatibility.</param>
        public Style(ConsoulLibrary.Color.Color foreground, ConsoulLibrary.Color.Color background, Decoration decoration)
            : this(decoration, foreground.ToConsoleColor(), background.ToConsoleColor())
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
            return new Style(Decoration, foreground, Background);
        }

        /// <summary>
        /// Creates a copy of this style with a different RGB foreground color mapped to the nearest console color.
        /// </summary>
        /// <param name="foreground">The RGB foreground color.</param>
        /// <returns>A new style with the requested foreground color.</returns>
        public Style WithForeground(RgbColor foreground)
        {
            return WithForeground((ConsoleColor?)foreground.ToConsoleColor());
        }

        /// <summary>
        /// Creates a copy of this style with a different RGB foreground color mapped to the nearest console color.
        /// </summary>
        /// <param name="foreground">The RGB foreground color.</param>
        /// <returns>A new style with the requested foreground color.</returns>
        public Style WithForeground(ConsoulLibrary.Color.Color foreground)
        {
            return WithForeground((ConsoleColor?)foreground.ToConsoleColor());
        }

        /// <summary>
        /// Creates a copy of this style with a different background color.
        /// </summary>
        /// <param name="background">The background color, or <see langword="null"/> to leave the background unspecified.</param>
        /// <returns>A new style with the requested background color.</returns>
        public Style WithBackground(ConsoleColor? background)
        {
            return new Style(Decoration, Foreground, background);
        }

        /// <summary>
        /// Creates a copy of this style with a different RGB background color mapped to the nearest console color.
        /// </summary>
        /// <param name="background">The RGB background color.</param>
        /// <returns>A new style with the requested background color.</returns>
        public Style WithBackground(RgbColor background)
        {
            return WithBackground((ConsoleColor?)background.ToConsoleColor());
        }

        /// <summary>
        /// Creates a copy of this style with a different RGB background color mapped to the nearest console color.
        /// </summary>
        /// <param name="background">The RGB background color.</param>
        /// <returns>A new style with the requested background color.</returns>
        public Style WithBackground(ConsoulLibrary.Color.Color background)
        {
            return WithBackground((ConsoleColor?)background.ToConsoleColor());
        }

        /// <summary>
        /// Creates a copy of this style with replacement decoration metadata.
        /// </summary>
        /// <param name="decoration">The replacement decoration flags.</param>
        /// <returns>A new style with the requested decoration metadata.</returns>
        public Style WithDecoration(Decoration decoration)
        {
            return new Style(decoration, Foreground, Background);
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

            parsed = new Style(ParseDecoration(style), scheme.Color, scheme.BackgroundColor);
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

        private static Decoration ParseDecorationToken(string token)
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
    }
}
