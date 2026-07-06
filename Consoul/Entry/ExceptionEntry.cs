using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders structured exception details inside a responsive diagnostic panel.
    /// </summary>
    public class ExceptionEntry : IRenderable
    {
        private readonly ExceptionRenderOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionEntry"/> class.
        /// </summary>
        /// <param name="exception">The exception to render.</param>
        /// <param name="message">Optional context message rendered before the exception details.</param>
        /// <param name="options">Optional rendering options for the exception panel.</param>
        public ExceptionEntry(Exception exception, string message = null, ExceptionRenderOptions options = null)
        {
            Error = exception ?? throw new ArgumentNullException(nameof(exception));
            Message = message;
            _options = options != null ? options.Clone() : new ExceptionRenderOptions();
        }

        /// <summary>
        /// Gets the exception rendered by this entry.
        /// </summary>
        public Exception Error { get; }

        /// <summary>
        /// Gets or sets optional context text rendered before exception details.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets the options used to render the exception.
        /// </summary>
        public ExceptionRenderOptions Options => _options;

        /// <summary>
        /// Renders the exception to the active console driver.
        /// </summary>
        public void Render()
        {
            int width = PanelEntry.ResolveWidth(_options.Width);
            if (width < 4)
            {
                RenderFallback(width);
                return;
            }

            int innerWidth = Math.Max(1, width - 4);
            ColorScheme borderScheme = _options.ResolveBorderScheme();
            ColorScheme titleScheme = _options.ResolveTitleScheme();

            WriteTopBorder(width, borderScheme, titleScheme);
            foreach (ExceptionLine line in BuildLines())
            {
                WriteWrappedLine(line, innerWidth, borderScheme);
            }

            Consoul.Write(BuildBottomBorder(width), borderScheme);
        }

        /// <summary>
        /// Renders a temporary exception panel to the active console driver.
        /// </summary>
        /// <param name="exception">The exception to render.</param>
        /// <param name="message">Optional context message rendered before the exception details.</param>
        /// <param name="options">Optional rendering options for the exception panel.</param>
        public static void Render(Exception exception, string message = null, ExceptionRenderOptions options = null)
        {
            new ExceptionEntry(exception, message, options).Render();
        }

        private IEnumerable<ExceptionLine> BuildLines()
        {
            if (!string.IsNullOrWhiteSpace(Message))
            {
                ColorScheme contextScheme = _options.ResolveContextScheme();
                yield return ExceptionLine.Markup(Message.Trim(), contextScheme);
                yield return new ExceptionLine(string.Empty, _options.ResolveContextScheme());
            }

            Exception current = Error;
            int depth = 0;
            while (current != null)
            {
                if (depth > 0)
                {
                    yield return new ExceptionLine(string.Empty, _options.ResolveContextScheme());
                }

                string label = depth == 0 ? "Type" : "Inner " + depth;
                yield return new ExceptionLine(label + ": " + FormatTypeName(current.GetType()), _options.ResolveTypeScheme());
                ColorScheme messageScheme = _options.ResolveMessageScheme();
                yield return ExceptionLine.MarkupWithPrefix("Message: ", current.Message ?? string.Empty, messageScheme);

                if (_options.IncludeStackTrace && !_options.Format.HasFlag(ExceptionFormats.NoStackTrace))
                {
                    foreach (ExceptionLine stackLine in BuildStackLines(current))
                    {
                        yield return stackLine;
                    }
                }

                if (!_options.IncludeInnerExceptions)
                {
                    break;
                }

                current = current.InnerException;
                depth++;
            }
        }

        private IEnumerable<ExceptionLine> BuildStackLines(Exception exception)
        {
            string stackTrace = exception.StackTrace;
            if (string.IsNullOrWhiteSpace(stackTrace))
            {
                yield break;
            }

            yield return new ExceptionLine("Stack trace:", _options.ResolveStackTraceScheme());
            IEnumerable<string> frames = stackTrace
                .Replace("\r\n", "\n")
                .Replace('\r', '\n')
                .Split('\n')
                .Select(line => line.Trim())
                .Where(line => line.Length > 0);

            if (_options.MaxStackFrames.HasValue)
            {
                frames = frames.Take(Math.Max(0, _options.MaxStackFrames.Value));
            }

            foreach (string frame in frames)
            {
                yield return new ExceptionLine("  " + FormatStackFrame(frame), _options.ResolveStackTraceScheme());
            }
        }

        private string FormatTypeName(Type type)
        {
            if (type == null)
            {
                return string.Empty;
            }

            return _options.Format.HasFlag(ExceptionFormats.ShortenTypes) ? type.Name : type.FullName;
        }

        private string FormatStackFrame(string frame)
        {
            string formatted = frame ?? string.Empty;
            if (_options.Format.HasFlag(ExceptionFormats.ShortenMethods))
            {
                formatted = ShortenMethodName(formatted);
            }

            if (_options.Format.HasFlag(ExceptionFormats.ShortenPaths))
            {
                formatted = ShortenPath(formatted);
            }

            return formatted;
        }

        private static string ShortenMethodName(string frame)
        {
            const string marker = "at ";
            int markerIndex = frame.IndexOf(marker, StringComparison.Ordinal);
            if (markerIndex < 0)
            {
                return frame;
            }

            int methodStart = markerIndex + marker.Length;
            int parenIndex = frame.IndexOf('(', methodStart);
            if (parenIndex <= methodStart)
            {
                return frame;
            }

            string method = frame.Substring(methodStart, parenIndex - methodStart);
            string shortened = LastSegments(method, 2);
            return frame.Substring(0, methodStart) + shortened + frame.Substring(parenIndex);
        }

        private static string ShortenPath(string frame)
        {
            const string marker = " in ";
            int markerIndex = frame.LastIndexOf(marker, StringComparison.Ordinal);
            if (markerIndex < 0)
            {
                return frame;
            }

            int pathStart = markerIndex + marker.Length;
            int lineIndex = frame.IndexOf(":line ", pathStart, StringComparison.OrdinalIgnoreCase);
            int pathEnd = lineIndex >= 0 ? lineIndex : frame.Length;
            string path = frame.Substring(pathStart, pathEnd - pathStart);
            string fileName = Path.GetFileName(path);
            if (string.IsNullOrEmpty(fileName))
            {
                return frame;
            }

            return frame.Substring(0, pathStart) + fileName + frame.Substring(pathEnd);
        }

        private static string LastSegments(string value, int segmentCount)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value ?? string.Empty;
            }

            string[] segments = value.Split('.');
            if (segments.Length <= segmentCount)
            {
                return value;
            }

            return string.Join(".", segments.Skip(segments.Length - segmentCount));
        }

        private void RenderFallback(int width)
        {
            int fallbackWidth = Math.Max(1, width);
            foreach (ExceptionLine line in BuildLines())
            {
                foreach (IList<FixedMessageSegment> wrappedLine in WrapSegments(line.Segments, fallbackWidth))
                {
                    MarkupRenderText.WriteSegments(PadOrTrimSegments(wrappedLine, fallbackWidth, line.Scheme), line.Scheme, true);
                }
            }
        }

        private void WriteWrappedLine(ExceptionLine line, int innerWidth, ColorScheme borderScheme)
        {
            foreach (IList<FixedMessageSegment> wrappedLine in WrapSegments(line.Segments, innerWidth))
            {
                Consoul.Write("|", borderScheme, false);
                Consoul.Write(" ", line.Scheme, false);
                MarkupRenderText.WriteSegments(PadOrTrimSegments(wrappedLine, innerWidth, line.Scheme), line.Scheme, false);
                Consoul.Write(" ", line.Scheme, false);
                Consoul.Write("|", borderScheme, true);
            }
        }

        private void WriteTopBorder(int width, ColorScheme borderScheme, ColorScheme titleScheme)
        {
            string title = _options.Title;
            if (string.IsNullOrWhiteSpace(title))
            {
                Consoul.Write("+" + new string('-', width - 2) + "+", borderScheme);
                return;
            }

            int contentWidth = Math.Max(0, width - 2);
            if (contentWidth == 0)
            {
                Consoul.Write("++", borderScheme);
                return;
            }

            IList<FixedMessageSegment> titleSegments = MarkupRenderText.TruncateSegments(
                MarkupRenderText.ParseSegments(title.Trim(), titleScheme),
                Math.Max(0, contentWidth - 2));
            int titleWidth = MarkupRenderText.GetLength(titleSegments);

            Consoul.Write("+", borderScheme, false);
            if (contentWidth == 1)
            {
                Consoul.Write("-", borderScheme, false);
                Consoul.Write("+", borderScheme, true);
                return;
            }

            Consoul.Write(" ", borderScheme, false);
            MarkupRenderText.WriteSegments(titleSegments, titleScheme, false);
            Consoul.Write(" ", borderScheme, false);
            int remaining = Math.Max(0, contentWidth - titleWidth - 2);
            Consoul.Write(new string('-', remaining), borderScheme, false);
            Consoul.Write("+", borderScheme, true);
        }

        private static string BuildBottomBorder(int width)
        {
            return "+" + new string('-', width - 2) + "+";
        }

        private static IEnumerable<IList<FixedMessageSegment>> WrapSegments(IEnumerable<FixedMessageSegment> segments, int width)
        {
            width = Math.Max(1, width);
            var lines = new List<IList<FixedMessageSegment>>();
            var source = segments?.ToList() ?? new List<FixedMessageSegment>();
            if (source.Count == 0 || MarkupRenderText.GetLength(source) == 0)
            {
                lines.Add(new List<FixedMessageSegment>());
                return lines;
            }

            var remaining = new List<FixedMessageSegment>(source);
            while (MarkupRenderText.GetLength(remaining) > 0)
            {
                List<FixedMessageSegment> line = MarkupRenderText.TakeWrappedSegments(remaining, width, out remaining);
                lines.Add(line);
            }

            return lines;
        }

        private static List<FixedMessageSegment> PadOrTrimSegments(
            IList<FixedMessageSegment> segments,
            int width,
            ColorScheme scheme)
        {
            width = Math.Max(0, width);
            List<FixedMessageSegment> result = MarkupRenderText.TruncateSegments(segments, width);
            MarkupRenderText.AddPadding(result, Math.Max(0, width - MarkupRenderText.GetLength(result)), scheme);
            return result;
        }

        private readonly struct ExceptionLine
        {
            public ExceptionLine(string text, ColorScheme scheme)
            {
                Scheme = scheme;
                Segments = new[] { new FixedMessageSegment(text ?? string.Empty, scheme) };
            }

            private ExceptionLine(IReadOnlyList<FixedMessageSegment> segments, ColorScheme scheme)
            {
                Scheme = scheme;
                Segments = segments ?? new[] { new FixedMessageSegment(string.Empty, scheme) };
            }

            public ColorScheme Scheme { get; }

            public IReadOnlyList<FixedMessageSegment> Segments { get; }

            public static ExceptionLine Markup(string text, ColorScheme scheme)
            {
                return new ExceptionLine(MarkupRenderText.ParseSegments(text, scheme), scheme);
            }

            public static ExceptionLine MarkupWithPrefix(string prefix, string text, ColorScheme scheme)
            {
                return new ExceptionLine(MarkupRenderText.ParseSegmentsWithPrefix(prefix, text, scheme), scheme);
            }
        }
    }
}
