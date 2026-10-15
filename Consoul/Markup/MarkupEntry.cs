using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders lightweight Spectre-style markup through Consoul's console driver.
    /// </summary>
    public class MarkupEntry : IRenderable
    {
        private static readonly ConsoleRgb[] ConsoleColors =
        {
            new ConsoleRgb(ConsoleColor.Black, 0, 0, 0),
            new ConsoleRgb(ConsoleColor.DarkBlue, 0, 0, 128),
            new ConsoleRgb(ConsoleColor.DarkGreen, 0, 128, 0),
            new ConsoleRgb(ConsoleColor.DarkCyan, 0, 128, 128),
            new ConsoleRgb(ConsoleColor.DarkRed, 128, 0, 0),
            new ConsoleRgb(ConsoleColor.DarkMagenta, 128, 0, 128),
            new ConsoleRgb(ConsoleColor.DarkYellow, 128, 128, 0),
            new ConsoleRgb(ConsoleColor.Gray, 192, 192, 192),
            new ConsoleRgb(ConsoleColor.DarkGray, 128, 128, 128),
            new ConsoleRgb(ConsoleColor.Blue, 0, 0, 255),
            new ConsoleRgb(ConsoleColor.Green, 0, 255, 0),
            new ConsoleRgb(ConsoleColor.Cyan, 0, 255, 255),
            new ConsoleRgb(ConsoleColor.Red, 255, 0, 0),
            new ConsoleRgb(ConsoleColor.Magenta, 255, 0, 255),
            new ConsoleRgb(ConsoleColor.Yellow, 255, 255, 0),
            new ConsoleRgb(ConsoleColor.White, 255, 255, 255)
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkupEntry"/> class.
        /// </summary>
        /// <param name="markup">The markup text to render.</param>
        /// <param name="writeLine">Indicates whether to append a newline after rendering.</param>
        public MarkupEntry(string markup, bool writeLine = true)
        {
            Markup = markup ?? string.Empty;
            WriteLine = writeLine;
        }

        /// <summary>
        /// Gets or sets the markup text rendered by this entry.
        /// </summary>
        public string Markup { get; set; }

        /// <summary>
        /// Gets or sets the default style applied to untagged markup text.
        /// </summary>
        public Style DefaultStyle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to append a newline after rendering.
        /// </summary>
        public bool WriteLine { get; set; }

        /// <summary>
        /// Renders the markup to the active console driver.
        /// </summary>
        public void Render()
        {
            Render(Markup, WriteLine, DefaultStyle);
        }

        /// <summary>
        /// Renders temporary markup text to the active console driver.
        /// </summary>
        /// <param name="markup">The markup text to render.</param>
        /// <param name="writeLine">Indicates whether to append a newline after rendering.</param>
        public static void Render(string markup, bool writeLine)
        {
            Render(markup, writeLine, null);
        }

        /// <summary>
        /// Renders temporary markup text to the active console driver.
        /// </summary>
        /// <param name="markup">The markup text to render.</param>
        /// <param name="writeLine">Indicates whether to append a newline after rendering.</param>
        /// <param name="defaultStyle">Optional default style applied to untagged text.</param>
        public static void Render(string markup, bool writeLine, Style defaultStyle)
        {
            StyledColorScheme defaultScheme = defaultStyle != null
                ? StyledColorScheme.FromStyle(defaultStyle, RenderOptions.DefaultScheme)
                : StyledColorScheme.FromScheme(RenderOptions.DefaultScheme);
            Decoration defaultDecoration = defaultStyle != null ? defaultStyle.Decoration : Decoration.None;

            foreach (MarkupSegment segment in Parse(markup, defaultScheme, defaultDecoration))
            {
                if (segment.Text.Length > 0)
                {
                    TerminalLinkWriter.WriteText(
                        segment.Text,
                        segment.Scheme,
                        segment.Link,
                        segment.Decoration,
                        segment.ForegroundRgb,
                        segment.BackgroundRgb);
                }
            }

            if (writeLine)
            {
                Consoul.Write(string.Empty);
            }
        }

        internal static string EscapeMarkupText(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length);
            foreach (char character in value)
            {
                switch (character)
                {
                    case '[':
                        builder.Append("[[");
                        break;
                    case ']':
                        builder.Append("]]");
                        break;
                    default:
                        builder.Append(character);
                        break;
                }
            }

            return builder.ToString();
        }

        internal static string FormatInterpolated(FormattableString markup)
        {
            return FormatInterpolated(CultureInfo.CurrentCulture, markup);
        }

        internal static string FormatMarkup(string markup, params object[] args)
        {
            return FormatMarkup(CultureInfo.CurrentCulture, markup, args);
        }

        internal static string FormatMarkup(IFormatProvider provider, string markup, params object[] args)
        {
            if (markup == null)
            {
                return string.Empty;
            }

            return string.Format(
                new EscapingFormatProvider(provider ?? CultureInfo.CurrentCulture),
                markup,
                args ?? Array.Empty<object>());
        }

        internal static string FormatInterpolated(IFormatProvider provider, FormattableString markup)
        {
            if (markup == null)
            {
                return string.Empty;
            }

            return string.Format(
                new EscapingFormatProvider(provider ?? CultureInfo.CurrentCulture),
                markup.Format,
                markup.GetArguments());
        }

        internal static string RemoveMarkupText(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length);
            foreach (MarkupSegment segment in Parse(value, RenderOptions.DefaultScheme))
            {
                builder.Append(segment.Text);
            }

            return builder.ToString();
        }

        internal static IEnumerable<MarkupSegment> Parse(string markup, ColorScheme defaultScheme)
        {
            return Parse(markup, defaultScheme, Decoration.None);
        }

        internal static IEnumerable<MarkupSegment> Parse(string markup, ColorScheme defaultScheme, Decoration defaultDecoration)
        {
            return Parse(markup, StyledColorScheme.FromScheme(defaultScheme), defaultDecoration);
        }

        internal static IEnumerable<MarkupSegment> Parse(string markup, StyledColorScheme defaultScheme, Decoration defaultDecoration)
        {
            markup = Emoji.Replace(markup);

            var stack = new List<MarkupState> { new MarkupState(defaultScheme, defaultDecoration, null) };
            var text = new StringBuilder();

            int index = 0;
            while (index < markup.Length)
            {
                char current = markup[index];
                if (current == '[')
                {
                    if (index + 1 < markup.Length && markup[index + 1] == '[')
                    {
                        text.Append('[');
                        index += 2;
                        continue;
                    }

                    int closeIndex = markup.IndexOf(']', index + 1);
                    if (closeIndex < 0)
                    {
                        text.Append(current);
                        index++;
                        continue;
                    }

                    string tag = markup.Substring(index + 1, closeIndex - index - 1);
                    MarkupState currentState = stack[stack.Count - 1];
                    if (IsCloseTag(tag, currentState.Scheme, currentState.Decoration))
                    {
                        if (stack.Count > 1)
                        {
                            foreach (MarkupSegment segment in Flush(text, currentState))
                            {
                                yield return segment;
                            }

                            stack.RemoveAt(stack.Count - 1);
                        }
                        else
                        {
                            text.Append('[');
                            text.Append(tag);
                            text.Append(']');
                        }

                        index = closeIndex + 1;
                        continue;
                    }

                    StyledColorScheme nextScheme;
                    Decoration nextDecoration;
                    Link nextLink;
                    bool hasLink;
                    if (TryParseTag(
                        tag,
                        currentState.Scheme,
                        currentState.Decoration,
                        out nextScheme,
                        out nextDecoration,
                        out nextLink,
                        out hasLink))
                    {
                        foreach (MarkupSegment segment in Flush(text, currentState))
                        {
                            yield return segment;
                        }

                        stack.Add(new MarkupState(nextScheme, nextDecoration, hasLink ? nextLink : currentState.Link));
                        index = closeIndex + 1;
                        continue;
                    }

                    text.Append(markup.Substring(index, closeIndex - index + 1));
                    index = closeIndex + 1;
                    continue;
                }

                if (current == ']' && index + 1 < markup.Length && markup[index + 1] == ']')
                {
                    text.Append(']');
                    index += 2;
                    continue;
                }

                text.Append(current);
                index++;
            }

            foreach (MarkupSegment segment in Flush(text, stack[stack.Count - 1]))
            {
                yield return segment;
            }
        }

        private static IEnumerable<MarkupSegment> Flush(StringBuilder text, MarkupState state)
        {
            if (text.Length == 0)
            {
                yield break;
            }

            yield return new MarkupSegment(text.ToString(), state.Scheme, state.Decoration, state.Link);
            text.Length = 0;
        }

        internal static bool TryParseStyle(string tag, ColorScheme currentScheme, out ColorScheme scheme)
        {
            return TryParseTag(tag, currentScheme, out scheme);
        }

        private static bool TryParseTag(string tag, ColorScheme currentScheme, out ColorScheme scheme)
        {
            StyledColorScheme richScheme;
            Decoration ignoredDecoration;
            Link ignoredLink;
            bool ignoredHasLink;
            bool parsed = TryParseTag(
                tag,
                StyledColorScheme.FromScheme(currentScheme),
                Decoration.None,
                out richScheme,
                out ignoredDecoration,
                out ignoredLink,
                out ignoredHasLink);
            scheme = richScheme.Scheme;
            return parsed;
        }

        private static bool TryParseTag(
            string tag,
            StyledColorScheme currentScheme,
            Decoration currentDecoration,
            out StyledColorScheme scheme,
            out Decoration decoration,
            out Link link,
            out bool hasLink)
        {
            tag = (tag ?? string.Empty).Trim();
            link = null;
            hasLink = false;
            decoration = currentDecoration;
            ColorScheme resolvedScheme;
            if (TryResolveScheme(tag, out resolvedScheme))
            {
                scheme = StyledColorScheme.FromScheme(resolvedScheme);
                return true;
            }

            string contentTag;
            if (TryParseContentTag(
                tag,
                currentScheme,
                currentDecoration,
                out contentTag,
                out scheme,
                out decoration,
                out link,
                out hasLink))
            {
                tag = contentTag;
                if (string.IsNullOrWhiteSpace(tag))
                {
                    return true;
                }

                if (TryResolveScheme(tag, out resolvedScheme))
                {
                    scheme = StyledColorScheme.FromScheme(resolvedScheme);
                    return true;
                }
            }

            string foregroundToken = tag;
            string backgroundToken = null;
            int onIndex = CultureInvariantIndexOf(tag, " on ");
            if (onIndex >= 0)
            {
                foregroundToken = tag.Substring(0, onIndex);
                backgroundToken = tag.Substring(onIndex + 4);
            }
            else
            {
                int colonIndex = tag.IndexOf(':');
                if (colonIndex >= 0)
                {
                    foregroundToken = tag.Substring(0, colonIndex);
                    backgroundToken = tag.Substring(colonIndex + 1);
                }
            }

            ParsedColor foreground;
            if (!TryParseColorToken(foregroundToken, out foreground))
            {
                scheme = currentScheme;
                decoration = currentDecoration;
                return false;
            }

            ParsedColor background = new ParsedColor(currentScheme.Scheme.BackgroundColor, currentScheme.BackgroundRgb);
            if (!string.IsNullOrWhiteSpace(backgroundToken) && !TryParseColorToken(backgroundToken, out background))
            {
                scheme = currentScheme;
                decoration = currentDecoration;
                return false;
            }

            scheme = new StyledColorScheme(
                new ColorScheme
                {
                    Color = foreground.Color,
                    BackgroundColor = background.Color
                },
                foreground.Rgb,
                background.Rgb);
            return true;
        }

        private static bool IsCloseTag(string tag, StyledColorScheme currentScheme, Decoration currentDecoration)
        {
            tag = (tag ?? string.Empty).Trim();
            if (tag == "/")
            {
                return true;
            }

            if (!tag.StartsWith("/", StringComparison.Ordinal))
            {
                return false;
            }

            string closingTag = tag.Substring(1).Trim();
            if (closingTag.Length == 0)
            {
                return true;
            }

            StyledColorScheme ignored;
            Decoration ignoredDecoration;
            Link ignoredLink;
            bool ignoredHasLink;
            return TryParseTag(
                closingTag,
                currentScheme,
                currentDecoration,
                out ignored,
                out ignoredDecoration,
                out ignoredLink,
                out ignoredHasLink);
        }

        private static bool TryParseContentTag(string tag, ColorScheme currentScheme, out string contentTag, out ColorScheme scheme)
        {
            StyledColorScheme richScheme;
            Decoration ignoredDecoration;
            Link ignoredLink;
            bool ignoredHasLink;
            bool parsed = TryParseContentTag(
                tag,
                StyledColorScheme.FromScheme(currentScheme),
                Decoration.None,
                out contentTag,
                out richScheme,
                out ignoredDecoration,
                out ignoredLink,
                out ignoredHasLink);
            scheme = richScheme.Scheme;
            return parsed;
        }

        private static bool TryParseContentTag(
            string tag,
            StyledColorScheme currentScheme,
            Decoration currentDecoration,
            out string contentTag,
            out StyledColorScheme scheme,
            out Decoration decoration,
            out Link link,
            out bool hasLink)
        {
            contentTag = tag ?? string.Empty;
            scheme = currentScheme;
            decoration = currentDecoration;
            link = null;
            hasLink = false;

            if (string.IsNullOrWhiteSpace(contentTag))
            {
                return false;
            }

            if (IsLinkTag(contentTag))
            {
                string url;
                if (TryParseLinkUrl(contentTag, out url) && !string.IsNullOrWhiteSpace(url))
                {
                    link = new Link(url);
                }

                hasLink = true;
                contentTag = string.Empty;
                return true;
            }

            string[] tokens = contentTag.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
            {
                return false;
            }

            bool removedStyle = false;
            bool removedLink = false;
            Decoration parsedDecoration = Decoration.None;
            var builder = new StringBuilder(contentTag.Length);
            foreach (string token in tokens)
            {
                if (IsStyleToken(token))
                {
                    removedStyle = true;
                    parsedDecoration |= Style.ParseDecorationToken(token);
                    continue;
                }

                if (IsLinkTag(token))
                {
                    string url;
                    if (TryParseLinkUrl(token, out url) && !string.IsNullOrWhiteSpace(url))
                    {
                        link = new Link(url);
                    }

                    removedLink = true;
                    hasLink = true;
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(token);
            }

            if (!removedStyle && !removedLink)
            {
                return false;
            }

            contentTag = builder.ToString();
            decoration = currentDecoration | parsedDecoration;
            return true;
        }

        private static bool IsLinkTag(string tag)
        {
            string normalized = NormalizeToken(tag);
            return normalized == "link" ||
                normalized.StartsWith("link=", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("link:", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryParseLinkUrl(string tag, out string url)
        {
            url = null;
            tag = (tag ?? string.Empty).Trim();
            if (string.Equals(NormalizeToken(tag), "link", StringComparison.Ordinal))
            {
                return true;
            }

            int separator = tag.IndexOf('=');
            if (separator < 0)
            {
                separator = tag.IndexOf(':');
            }

            if (separator < 0 || separator >= tag.Length - 1)
            {
                return true;
            }

            url = tag.Substring(separator + 1).Trim();
            if (url.Length >= 2 &&
                ((url[0] == '"' && url[url.Length - 1] == '"') ||
                 (url[0] == '\'' && url[url.Length - 1] == '\'')))
            {
                url = url.Substring(1, url.Length - 2);
            }

            return true;
        }

        private static bool IsStyleToken(string token)
        {
            switch (NormalizeToken(token))
            {
                case "bold":
                case "b":
                case "dim":
                case "faint":
                case "italic":
                case "i":
                case "underline":
                case "u":
                case "strikethrough":
                case "strike":
                case "slowblink":
                case "rapidblink":
                case "blink":
                case "reverse":
                case "invert":
                case "conceal":
                case "hidden":
                    return true;
                default:
                    return false;
            }
        }

        private static bool TryResolveScheme(string tag, out ColorScheme scheme)
        {
            switch (NormalizeToken(tag))
            {
                case "default":
                    scheme = RenderOptions.DefaultScheme;
                    return true;
                case "prompt":
                    scheme = RenderOptions.PromptScheme;
                    return true;
                case "subnote":
                case "muted":
                    scheme = RenderOptions.SubnoteScheme;
                    return true;
                case "invalid":
                case "error":
                    scheme = RenderOptions.InvalidScheme;
                    return true;
                case "option":
                    scheme = RenderOptions.OptionScheme;
                    return true;
                case "routine":
                case "routineinput":
                    scheme = RenderOptions.RoutineInputScheme;
                    return true;
                default:
                    scheme = RenderOptions.DefaultScheme;
                    return false;
            }
        }

        internal static bool TryParseColor(string token, out ConsoleColor color)
        {
            ParsedColor parsed;
            if (TryParseColorToken(token, out parsed))
            {
                color = parsed.Color;
                return true;
            }

            color = RenderOptions.DefaultColor;
            return false;
        }

        private static bool TryParseColorToken(string token, out ParsedColor color)
        {
            string normalized = NormalizeToken(token);
            RgbColor rgbColor;
            if (RgbColor.TryFromHex(token, out rgbColor))
            {
                color = new ParsedColor(rgbColor.ToConsoleColor(), rgbColor);
                return true;
            }

            if (normalized == "grey")
            {
                normalized = "gray";
            }
            else if (normalized == "darkgrey")
            {
                normalized = "darkgray";
            }
            else if (TryParseColorAlias(normalized, out ConsoleColor aliasColor))
            {
                color = new ParsedColor(aliasColor, null);
                return true;
            }

            foreach (ConsoleColor candidate in Enum.GetValues(typeof(ConsoleColor)))
            {
                if (NormalizeToken(candidate.ToString()) == normalized)
                {
                    color = new ParsedColor(candidate, null);
                    return true;
                }
            }

            color = default(ParsedColor);
            return false;
        }

        private static bool TryParseHexColor(string token, out ConsoleColor color)
        {
            color = RenderOptions.DefaultColor;
            token = (token ?? string.Empty).Trim();
            if (!token.StartsWith("#", StringComparison.Ordinal))
            {
                return false;
            }

            string hex = token.Substring(1);
            if (hex.Length == 3)
            {
                hex = new string(new[] { hex[0], hex[0], hex[1], hex[1], hex[2], hex[2] });
            }

            if (hex.Length != 6)
            {
                return false;
            }

            int red;
            int green;
            int blue;
            if (!TryParseHexByte(hex, 0, out red) ||
                !TryParseHexByte(hex, 2, out green) ||
                !TryParseHexByte(hex, 4, out blue))
            {
                return false;
            }

            color = FindNearestConsoleColor(red, green, blue);
            return true;
        }

        private static bool TryParseHexByte(string hex, int startIndex, out int value)
        {
            return int.TryParse(
                hex.Substring(startIndex, 2),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out value);
        }

        private static ConsoleColor FindNearestConsoleColor(int red, int green, int blue)
        {
            ConsoleColor closest = ConsoleColor.White;
            int closestDistance = int.MaxValue;
            foreach (ConsoleRgb candidate in ConsoleColors)
            {
                int redDistance = red - candidate.Red;
                int greenDistance = green - candidate.Green;
                int blueDistance = blue - candidate.Blue;
                int distance =
                    (redDistance * redDistance) +
                    (greenDistance * greenDistance) +
                    (blueDistance * blueDistance);

                if (distance < closestDistance)
                {
                    closest = candidate.Color;
                    closestDistance = distance;
                }
            }

            return closest;
        }

        private static bool TryParseColorAlias(string normalized, out ConsoleColor color)
        {
            switch (normalized)
            {
                case "aqua":
                    color = ConsoleColor.Cyan;
                    return true;
                case "fuchsia":
                    color = ConsoleColor.Magenta;
                    return true;
                case "lime":
                    color = ConsoleColor.Green;
                    return true;
                case "maroon":
                    color = ConsoleColor.DarkRed;
                    return true;
                case "navy":
                    color = ConsoleColor.DarkBlue;
                    return true;
                case "olive":
                    color = ConsoleColor.DarkYellow;
                    return true;
                case "purple":
                    color = ConsoleColor.DarkMagenta;
                    return true;
                case "silver":
                    color = ConsoleColor.Gray;
                    return true;
                case "teal":
                    color = ConsoleColor.DarkCyan;
                    return true;
                default:
                    color = RenderOptions.DefaultColor;
                    return false;
            }
        }

        private static string NormalizeToken(string token)
        {
            token = token ?? string.Empty;
            var builder = new StringBuilder(token.Length);
            for (int index = 0; index < token.Length; index++)
            {
                char character = token[index];
                if (!char.IsWhiteSpace(character) && character != '-' && character != '_')
                {
                    builder.Append(char.ToLowerInvariant(character));
                }
            }

            return builder.ToString();
        }

        private static int CultureInvariantIndexOf(string value, string search)
        {
            return value.IndexOf(search, StringComparison.OrdinalIgnoreCase);
        }

        private readonly struct ConsoleRgb
        {
            public ConsoleRgb(ConsoleColor color, int red, int green, int blue)
            {
                Color = color;
                Red = red;
                Green = green;
                Blue = blue;
            }

            public ConsoleColor Color { get; }

            public int Red { get; }

            public int Green { get; }

            public int Blue { get; }
        }

        private readonly struct MarkupState
        {
            public MarkupState(StyledColorScheme scheme, Decoration decoration, Link link)
            {
                Scheme = scheme;
                Decoration = decoration;
                Link = link;
            }

            public StyledColorScheme Scheme { get; }

            public Decoration Decoration { get; }

            public Link Link { get; }
        }

        private readonly struct ParsedColor
        {
            public ParsedColor(ConsoleColor color, RgbColor? rgb)
            {
                Color = color;
                Rgb = rgb;
            }

            public ConsoleColor Color { get; }

            public RgbColor? Rgb { get; }
        }

        private sealed class EscapingFormatProvider : IFormatProvider, ICustomFormatter
        {
            private readonly IFormatProvider _fallbackProvider;

            public EscapingFormatProvider(IFormatProvider fallbackProvider)
            {
                _fallbackProvider = fallbackProvider ?? CultureInfo.CurrentCulture;
            }

            public object GetFormat(Type formatType)
            {
                if (formatType == typeof(ICustomFormatter))
                {
                    return this;
                }

                return _fallbackProvider.GetFormat(formatType);
            }

            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                if (arg == null)
                {
                    return string.Empty;
                }

                string value;
                IFormattable formattable = arg as IFormattable;
                if (formattable != null)
                {
                    value = formattable.ToString(format, _fallbackProvider);
                }
                else
                {
                    value = arg.ToString();
                }

                return EscapeMarkupText(value);
            }
        }
    }

    internal sealed class MarkupSegment
    {
        public MarkupSegment(string text, ColorScheme scheme)
            : this(text, scheme, Decoration.None, null)
        {
        }

        public MarkupSegment(string text, ColorScheme scheme, Link link)
            : this(text, scheme, Decoration.None, link)
        {
        }

        public MarkupSegment(string text, ColorScheme scheme, Decoration decoration, Link link)
            : this(text, StyledColorScheme.FromScheme(scheme), decoration, link)
        {
        }

        public MarkupSegment(string text, StyledColorScheme scheme, Decoration decoration, Link link)
        {
            Text = text ?? string.Empty;
            Scheme = scheme.Scheme;
            ForegroundRgb = scheme.ForegroundRgb;
            BackgroundRgb = scheme.BackgroundRgb;
            Decoration = decoration;
            Link = link;
        }

        public string Text { get; }

        public ColorScheme Scheme { get; }

        public RgbColor? ForegroundRgb { get; }

        public RgbColor? BackgroundRgb { get; }

        public Decoration Decoration { get; }

        public Link Link { get; }
    }
}
