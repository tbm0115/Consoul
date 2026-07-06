using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary
{
    internal static class MarkupRenderText
    {
        public static List<FixedMessageSegment> ParseSegments(string text, ColorScheme defaultScheme)
        {
            var segments = new List<FixedMessageSegment>();
            foreach (MarkupSegment segment in MarkupEntry.Parse(text ?? string.Empty, defaultScheme))
            {
                if (segment.Text.Length > 0)
                {
                    segments.Add(new FixedMessageSegment(segment.Text, segment.Scheme, segment.Link));
                }
            }

            return segments;
        }

        public static List<FixedMessageSegment> ParseSegmentsWithPrefix(string prefix, string text, ColorScheme defaultScheme)
        {
            List<FixedMessageSegment> messageSegments = ParseSegments(text, defaultScheme);
            var segments = new List<FixedMessageSegment>();
            if (!string.IsNullOrEmpty(prefix))
            {
                if (messageSegments.Count > 0 && HasScheme(messageSegments[0], defaultScheme) && messageSegments[0].Link == null)
                {
                    segments.Add(new FixedMessageSegment(prefix + messageSegments[0].Text, defaultScheme));
                    segments.AddRange(messageSegments.Skip(1));
                    return segments;
                }

                segments.Add(new FixedMessageSegment(prefix, defaultScheme));
            }

            segments.AddRange(messageSegments);
            return segments;
        }

        public static int GetLength(IEnumerable<FixedMessageSegment> segments)
        {
            return (segments ?? Enumerable.Empty<FixedMessageSegment>()).Sum(segment => (segment.Text ?? string.Empty).Length);
        }

        public static string GetPlainText(string text)
        {
            return MarkupEntry.RemoveMarkupText(text ?? string.Empty);
        }

        public static string GetPlainText(IEnumerable<FixedMessageSegment> segments)
        {
            return string.Concat((segments ?? Enumerable.Empty<FixedMessageSegment>()).Select(segment => segment.Text ?? string.Empty));
        }

        public static List<List<FixedMessageSegment>> GetLogicalLines(string text, ColorScheme defaultScheme)
        {
            var lines = new List<List<FixedMessageSegment>> { new List<FixedMessageSegment>() };
            foreach (FixedMessageSegment segment in ParseSegments(text, defaultScheme))
            {
                string normalized = (segment.Text ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');
                string[] parts = normalized.Split('\n');
                for (int index = 0; index < parts.Length; index++)
                {
                    if (parts[index].Length > 0)
                    {
                        lines[lines.Count - 1].Add(new FixedMessageSegment(parts[index], segment.Scheme, segment.Link));
                    }

                    if (index < parts.Length - 1)
                    {
                        lines.Add(new List<FixedMessageSegment>());
                    }
                }
            }

            return lines;
        }

        public static List<List<FixedMessageSegment>> WrapSegments(string text, int width, ColorScheme defaultScheme)
        {
            width = Math.Max(1, width);
            var lines = new List<List<FixedMessageSegment>>();
            foreach (List<FixedMessageSegment> logicalLine in GetLogicalLines(text, defaultScheme))
            {
                if (logicalLine.Count == 0 || GetLength(logicalLine) == 0)
                {
                    lines.Add(new List<FixedMessageSegment>());
                    continue;
                }

                List<FixedMessageSegment> remaining = new List<FixedMessageSegment>(logicalLine);
                while (GetLength(remaining) > width)
                {
                    int breakIndex = FindWrapIndex(GetPlainText(remaining), width);
                    lines.Add(TrimEnd(TakeVisibleSegments(remaining, breakIndex)));
                    remaining = TrimStart(SkipVisibleSegments(remaining, breakIndex));
                    if (remaining.Count == 0)
                    {
                        break;
                    }
                }

                if (remaining.Count > 0)
                {
                    lines.Add(remaining);
                }
            }

            if (lines.Count == 0)
            {
                lines.Add(new List<FixedMessageSegment>());
            }

            return lines;
        }

        public static List<FixedMessageSegment> TakeVisibleSegments(IList<FixedMessageSegment> segments, int width)
        {
            var result = new List<FixedMessageSegment>();
            int remaining = Math.Max(0, width);
            if (remaining == 0 || segments == null)
            {
                return result;
            }

            foreach (FixedMessageSegment segment in segments)
            {
                if (remaining == 0)
                {
                    break;
                }

                string text = segment.Text ?? string.Empty;
                if (text.Length <= remaining)
                {
                    result.Add(segment);
                    remaining -= text.Length;
                    continue;
                }

                result.Add(new FixedMessageSegment(text.Substring(0, remaining), segment.Scheme, segment.Link));
                remaining = 0;
            }

            return result;
        }

        public static List<FixedMessageSegment> SkipVisibleSegments(IList<FixedMessageSegment> segments, int count)
        {
            var result = new List<FixedMessageSegment>();
            int remaining = Math.Max(0, count);
            if (segments == null)
            {
                return result;
            }

            foreach (FixedMessageSegment segment in segments)
            {
                string text = segment.Text ?? string.Empty;
                if (remaining >= text.Length)
                {
                    remaining -= text.Length;
                    continue;
                }

                if (remaining > 0)
                {
                    text = text.Substring(remaining);
                    remaining = 0;
                }

                result.Add(new FixedMessageSegment(text, segment.Scheme, segment.Link));
            }

            return result;
        }

        public static List<FixedMessageSegment> TakeWrappedSegments(
            IList<FixedMessageSegment> segments,
            int width,
            out List<FixedMessageSegment> remaining)
        {
            width = Math.Max(1, width);
            if (segments == null || segments.Count == 0)
            {
                remaining = new List<FixedMessageSegment>();
                return new List<FixedMessageSegment>();
            }

            int totalLength = GetLength(segments);
            if (totalLength <= width)
            {
                remaining = new List<FixedMessageSegment>();
                return new List<FixedMessageSegment>(segments);
            }

            int breakIndex = FindWrapIndex(GetPlainText(segments), width);
            remaining = TrimStart(SkipVisibleSegments(segments, breakIndex));
            return TrimEnd(TakeVisibleSegments(segments, breakIndex));
        }

        public static List<FixedMessageSegment> TruncateSegments(IList<FixedMessageSegment> segments, int width, string ellipsis = null)
        {
            width = Math.Max(0, width);
            if (width == 0 || segments == null || segments.Count == 0)
            {
                return new List<FixedMessageSegment>();
            }

            int totalLength = GetLength(segments);
            if (totalLength <= width)
            {
                return new List<FixedMessageSegment>(segments);
            }

            if (string.IsNullOrEmpty(ellipsis))
            {
                return TakeVisibleSegments(segments, width);
            }

            if (width <= ellipsis.Length)
            {
                ColorScheme? ellipsisScheme = segments[0].Scheme ?? RenderOptions.DefaultScheme;
                return new List<FixedMessageSegment>
                {
                    new FixedMessageSegment(new string('.', width), ellipsisScheme, segments[0].Link)
                };
            }

            var truncated = TakeVisibleSegments(segments, width - ellipsis.Length);
            ColorScheme? finalScheme = truncated.Count > 0
                ? truncated[truncated.Count - 1].Scheme
                : (segments[0].Scheme ?? RenderOptions.DefaultScheme);
            Link finalLink = truncated.Count > 0 ? truncated[truncated.Count - 1].Link : segments[0].Link;
            truncated.Add(new FixedMessageSegment(ellipsis, finalScheme, finalLink));
            return truncated;
        }

        public static List<FixedMessageSegment> AlignSegments(IList<FixedMessageSegment> segments, int width, TextBlockAlignment alignment, ColorScheme paddingScheme)
        {
            width = Math.Max(0, width);
            var content = TruncateSegments(segments, width);
            int remaining = Math.Max(0, width - GetLength(content));

            int leftPadding;
            switch (alignment)
            {
                case TextBlockAlignment.Center:
                    leftPadding = remaining / 2;
                    break;
                case TextBlockAlignment.Right:
                    leftPadding = remaining;
                    break;
                default:
                    leftPadding = 0;
                    break;
            }

            int rightPadding = remaining - leftPadding;
            var aligned = new List<FixedMessageSegment>();
            AddPadding(aligned, leftPadding, paddingScheme);
            aligned.AddRange(content);
            AddPadding(aligned, rightPadding, paddingScheme);
            return aligned;
        }

        public static List<FixedMessageSegment> PadSegments(
            IList<FixedMessageSegment> segments,
            int leftPadding,
            int rightPadding,
            ColorScheme paddingScheme)
        {
            var result = new List<FixedMessageSegment>();
            AddPadding(result, leftPadding, paddingScheme);
            if (segments != null)
            {
                result.AddRange(segments);
            }

            AddPadding(result, rightPadding, paddingScheme);
            return result;
        }

        public static List<FixedMessageSegment> TrimSegments(IList<FixedMessageSegment> segments)
        {
            return TrimEnd(TrimStart(segments));
        }

        public static void WriteSegments(IEnumerable<FixedMessageSegment> segments, ColorScheme fallbackScheme, bool writeLine)
        {
            foreach (FixedMessageSegment segment in segments ?? Enumerable.Empty<FixedMessageSegment>())
            {
                string text = segment.Text ?? string.Empty;
                if (text.Length == 0)
                {
                    continue;
                }

                ColorScheme scheme = segment.Scheme ?? fallbackScheme;
                TerminalLinkWriter.WriteText(text, scheme, segment.Link);
            }

            if (writeLine)
            {
                Consoul.Write(string.Empty);
            }
        }

        public static void AddPadding(IList<FixedMessageSegment> segments, int count, ColorScheme scheme)
        {
            if (count <= 0)
            {
                return;
            }

            segments.Add(new FixedMessageSegment(new string(' ', count), scheme));
        }

        private static int FindWrapIndex(string text, int width)
        {
            text = text ?? string.Empty;
            int end = Math.Min(width, text.Length);
            for (int index = end - 1; index > 0; index--)
            {
                if (char.IsWhiteSpace(text[index]))
                {
                    return index + 1;
                }
            }

            return end;
        }

        private static bool HasScheme(FixedMessageSegment segment, ColorScheme scheme)
        {
            return segment.Scheme.HasValue &&
                segment.Scheme.Value.Color == scheme.Color &&
                segment.Scheme.Value.BackgroundColor == scheme.BackgroundColor;
        }

        private static List<FixedMessageSegment> TrimStart(IList<FixedMessageSegment> segments)
        {
            var result = new List<FixedMessageSegment>();
            bool trimming = true;
            foreach (FixedMessageSegment segment in segments ?? Array.Empty<FixedMessageSegment>())
            {
                string text = segment.Text ?? string.Empty;
                if (trimming)
                {
                    text = text.TrimStart();
                    trimming = text.Length == 0;
                }

                if (text.Length > 0)
                {
                    result.Add(new FixedMessageSegment(text, segment.Scheme, segment.Link));
                }
            }

            return result;
        }

        private static List<FixedMessageSegment> TrimEnd(IList<FixedMessageSegment> segments)
        {
            if (segments == null || segments.Count == 0)
            {
                return new List<FixedMessageSegment>();
            }

            var result = new List<FixedMessageSegment>(segments);
            for (int index = result.Count - 1; index >= 0; index--)
            {
                FixedMessageSegment segment = result[index];
                string text = (segment.Text ?? string.Empty).TrimEnd();
                if (text.Length > 0)
                {
                    result[index] = new FixedMessageSegment(text, segment.Scheme, segment.Link);
                    break;
                }

                result.RemoveAt(index);
            }

            return result;
        }
    }
}
