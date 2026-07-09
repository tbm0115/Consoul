using System;
using ConsoulLibrary.Color;

namespace ConsoulLibrary
{
    internal static class PromptRenderHelpers
    {
        internal static ConsoleColor ResolveStyleColor(Style style, ConsoleColor fallback)
        {
            return style == null ? fallback : style.ToForegroundColor(fallback);
        }

        internal static StyledText ResolveStyle(Style style, ConsoleColor fallback)
        {
            if (style == null)
            {
                return FromColor(fallback);
            }

            return new StyledText(
                style.ToColorScheme(CreateScheme(fallback)),
                style.Decoration,
                style.ForegroundRgb,
                style.BackgroundRgb);
        }

        internal static StyledText FromColor(ConsoleColor color)
        {
            return new StyledText(CreateScheme(color), Decoration.None, null, null);
        }

        internal static StyledText FromComponents(
            ConsoleColor color,
            Decoration decoration,
            RgbColor? foregroundRgb,
            RgbColor? backgroundRgb)
        {
            return new StyledText(CreateScheme(color), decoration, foregroundRgb, backgroundRgb);
        }

        internal static void WriteMarkupLine(string text, ConsoleColor baseColor)
        {
            WriteMarkup(text, baseColor, writeLine: true);
        }

        internal static void WriteMarkupLine(string text, StyledText baseStyle)
        {
            WriteMarkup(text, baseStyle, writeLine: true);
        }

        internal static void WriteMarkup(string text, ConsoleColor baseColor, bool writeLine)
        {
            WriteMarkup(text, FromColor(baseColor), writeLine);
        }

        internal static void WriteMarkup(string text, StyledText baseStyle, bool writeLine)
        {
            MarkupRenderText.WriteSegments(
                MarkupRenderText.ParseSegments(
                    text,
                    baseStyle.Scheme,
                    baseStyle.Decoration,
                    baseStyle.ForegroundRgb,
                    baseStyle.BackgroundRgb),
                baseStyle.Scheme,
                writeLine);
        }

        internal static void WriteMarkupOrPlainLine(string text, ConsoleColor baseColor)
        {
            WriteMarkupOrPlainLine(text, FromColor(baseColor));
        }

        internal static void WriteMarkupOrPlainLine(string text, StyledText baseStyle)
        {
            WriteMarkupOrPlainLine(text, baseStyle, ContainsMarkup(text));
        }

        internal static void WriteMarkupOrPlainLine(string text, ConsoleColor baseColor, bool parseMarkup)
        {
            WriteMarkupOrPlainLine(text, FromColor(baseColor), parseMarkup);
        }

        internal static void WriteMarkupOrPlainLine(string text, StyledText baseStyle, bool parseMarkup)
        {
            if (!parseMarkup)
            {
                WriteText(text, baseStyle, writeLine: true);
                return;
            }

            WriteMarkupLine(text, baseStyle);
        }

        internal static void WriteFormattedMarkupOrPlainLine(string format, ConsoleColor baseColor, params object[] args)
        {
            WriteFormattedMarkupOrPlainLine(format, FromColor(baseColor), args);
        }

        internal static void WriteFormattedMarkupOrPlainLine(string format, StyledText baseStyle, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                return;
            }

            string message;
            bool parseMarkup;
            TryFormatMarkupOrPlain(format, out message, out parseMarkup, args);
            WriteMarkupOrPlainLine(message, baseStyle, parseMarkup);
        }

        internal static bool TryFormatMarkupOrPlain(string format, out string message, out bool parseMarkup, params object[] args)
        {
            message = format ?? string.Empty;
            parseMarkup = ContainsMarkup(format);
            try
            {
                message = parseMarkup
                    ? MarkupEntry.FormatMarkup(format, args)
                    : string.Format(format, args);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        internal static void WriteSearchHighlightedText(string text, string filterText, Style highlightStyle, ConsoleColor baseColor)
        {
            WriteSearchHighlightedText(text, filterText, highlightStyle, FromColor(baseColor));
        }

        internal static void WriteSearchHighlightedText(string text, string filterText, Style highlightStyle, StyledText baseStyle)
        {
            string visibleText = MarkupRenderText.GetPlainText(text);
            if (highlightStyle == null || string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(filterText))
            {
                WriteMarkupLine(text, baseStyle);
                return;
            }

            int matchIndex = visibleText.IndexOf(filterText, StringComparison.OrdinalIgnoreCase);
            if (matchIndex < 0)
            {
                WriteMarkupLine(text, baseStyle);
                return;
            }

            StyledText highlight = ResolveStyle(highlightStyle, baseStyle.Scheme.Color);
            int currentIndex = 0;
            while (matchIndex >= 0)
            {
                if (matchIndex > currentIndex)
                {
                    WriteText(visibleText.Substring(currentIndex, matchIndex - currentIndex), baseStyle, writeLine: false);
                }

                WriteText(visibleText.Substring(matchIndex, filterText.Length), highlight, writeLine: false);
                currentIndex = matchIndex + filterText.Length;
                matchIndex = visibleText.IndexOf(filterText, currentIndex, StringComparison.OrdinalIgnoreCase);
            }

            if (currentIndex < visibleText.Length)
            {
                WriteText(visibleText.Substring(currentIndex), baseStyle, writeLine: false);
            }

            Consoul.ConsoleDriver.WriteLine(string.Empty);
        }

        internal static bool ContainsVisibleText(string text, string filterText)
        {
            return MarkupRenderText.GetPlainText(text).IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        internal static bool ContainsMarkup(string text)
        {
            return !string.Equals(MarkupRenderText.GetPlainText(text), text ?? string.Empty, StringComparison.Ordinal);
        }

        private static ColorScheme CreateScheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        internal static void WriteText(string text, StyledText style, bool writeLine)
        {
            TerminalLinkWriter.WriteText(
                text,
                style.Scheme,
                null,
                style.Decoration,
                style.ForegroundRgb,
                style.BackgroundRgb);

            if (writeLine)
            {
                Consoul.ConsoleDriver.WriteLine(string.Empty);
            }
        }

        internal readonly struct StyledText
        {
            public StyledText(ColorScheme scheme, Decoration decoration, RgbColor? foregroundRgb, RgbColor? backgroundRgb)
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
