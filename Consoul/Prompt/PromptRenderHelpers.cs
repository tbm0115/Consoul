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

        internal static void WriteMarkupLine(string text, ConsoleColor baseColor)
        {
            WriteMarkup(text, baseColor, writeLine: true);
        }

        internal static void WriteMarkup(string text, ConsoleColor baseColor, bool writeLine)
        {
            ColorScheme scheme = CreateScheme(baseColor);
            MarkupRenderText.WriteSegments(MarkupRenderText.ParseSegments(text, scheme), scheme, writeLine);
        }

        internal static void WriteMarkupOrPlainLine(string text, ConsoleColor baseColor)
        {
            if (!ContainsMarkup(text))
            {
                Consoul.WriteCore(text, baseColor);
                return;
            }

            WriteMarkupLine(text, baseColor);
        }

        internal static void WriteMarkupOrPlainLine(string text, ConsoleColor baseColor, bool parseMarkup)
        {
            if (!parseMarkup)
            {
                Consoul.WriteCore(text, baseColor);
                return;
            }

            WriteMarkupLine(text, baseColor);
        }

        internal static void WriteFormattedMarkupOrPlainLine(string format, ConsoleColor baseColor, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                return;
            }

            string message;
            bool parseMarkup;
            TryFormatMarkupOrPlain(format, out message, out parseMarkup, args);
            WriteMarkupOrPlainLine(message, baseColor, parseMarkup);
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
            string visibleText = MarkupRenderText.GetPlainText(text);
            if (highlightStyle == null || string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(filterText))
            {
                WriteMarkupLine(text, baseColor);
                return;
            }

            int matchIndex = visibleText.IndexOf(filterText, StringComparison.OrdinalIgnoreCase);
            if (matchIndex < 0)
            {
                WriteMarkupLine(text, baseColor);
                return;
            }

            ConsoleColor highlightColor = ResolveStyleColor(highlightStyle, baseColor);
            int currentIndex = 0;
            while (matchIndex >= 0)
            {
                if (matchIndex > currentIndex)
                {
                    Consoul.WriteCore(visibleText.Substring(currentIndex, matchIndex - currentIndex), baseColor, writeLine: false);
                }

                Consoul.WriteCore(visibleText.Substring(matchIndex, filterText.Length), highlightColor, writeLine: false);
                currentIndex = matchIndex + filterText.Length;
                matchIndex = visibleText.IndexOf(filterText, currentIndex, StringComparison.OrdinalIgnoreCase);
            }

            if (currentIndex < visibleText.Length)
            {
                Consoul.WriteCore(visibleText.Substring(currentIndex), baseColor, writeLine: false);
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
    }
}
