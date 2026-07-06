using ConsoulLibrary.Color;
using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders wrapped, aligned, and padded text for console views.
    /// </summary>
    public class TextBlockEntry : IRenderable
    {
        private readonly TextBlockOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextBlockEntry"/> class.
        /// </summary>
        /// <param name="content">The text content to render.</param>
        /// <param name="options">Optional rendering options for the text block.</param>
        public TextBlockEntry(string content, TextBlockOptions options = null)
        {
            Content = content ?? string.Empty;
            _options = options != null ? options.Clone() : new TextBlockOptions();
        }

        /// <summary>
        /// Gets or sets the text content rendered by the block.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets the options used to render the text block.
        /// </summary>
        public TextBlockOptions Options => _options;

        /// <summary>
        /// Gets the character count of the text content.
        /// </summary>
        public int Length => Content?.Length ?? 0;

        /// <summary>
        /// Gets the number of logical lines in the text content.
        /// </summary>
        public int Lines => SplitLogicalLines(Content).Length;

        /// <summary>
        /// Gets or sets the text alignment.
        /// </summary>
        public Justify? Justification
        {
            get => ToJustify(_options.Alignment);
            set => _options.Alignment = value.HasValue ? JustifyMap.ToTextBlockAlignment(value.Value) : TextBlockAlignment.Left;
        }

        /// <summary>
        /// Gets or sets how content wider than the available line width should be handled.
        /// </summary>
        public Overflow? Overflow
        {
            get => _options.Overflow;
            set => _options.Overflow = value ?? ConsoulLibrary.Overflow.Wrap;
        }

        /// <summary>
        /// Renders the text block to the active console driver.
        /// </summary>
        public void Render()
        {
            int width = PanelEntry.ResolveWidth(_options.Width);
            int leftPadding = Math.Max(0, _options.LeftPadding);
            int rightPadding = Math.Max(0, _options.RightPadding);
            int topPadding = Math.Max(0, _options.TopPadding);
            int bottomPadding = Math.Max(0, _options.BottomPadding);

            if (leftPadding + rightPadding >= width)
            {
                leftPadding = Math.Min(leftPadding, Math.Max(0, width - 1));
                rightPadding = Math.Max(0, width - leftPadding - 1);
            }

            int innerWidth = Math.Max(1, width - leftPadding - rightPadding);
            ColorScheme scheme = _options.ResolveScheme();

            WriteBlankLines(topPadding, scheme);

            foreach (System.Collections.Generic.List<FixedMessageSegment> line in GetRenderSegmentLines(Content, innerWidth, _options.Overflow, scheme))
            {
                System.Collections.Generic.List<FixedMessageSegment> aligned = MarkupRenderText.AlignSegments(line, innerWidth, _options.Alignment, scheme);
                MarkupRenderText.WriteSegments(
                    MarkupRenderText.PadSegments(aligned, leftPadding, rightPadding, scheme),
                    scheme,
                    true);
            }

            WriteBlankLines(bottomPadding, scheme);
        }

        /// <summary>
        /// Renders a temporary text block to the active console driver.
        /// </summary>
        /// <param name="content">The text content to render.</param>
        /// <param name="options">Optional rendering options for the text block.</param>
        public static void Render(string content, TextBlockOptions options = null)
        {
            new TextBlockEntry(content, options).Render();
        }

        private static string[] SplitLogicalLines(string content)
        {
            content = (content ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');
            return content.Split('\n');
        }

        private static System.Collections.Generic.IEnumerable<string> GetRenderLines(string content, int width, Overflow overflow)
        {
            if (overflow == ConsoulLibrary.Overflow.Wrap)
            {
                return PanelEntry.WrapLines(content, width);
            }

            string[] lines = SplitLogicalLines(content);
            var rendered = new string[lines.Length == 0 ? 1 : lines.Length];
            if (lines.Length == 0)
            {
                rendered[0] = string.Empty;
                return rendered;
            }

            for (int index = 0; index < lines.Length; index++)
            {
                rendered[index] = ApplyOverflow(lines[index], width, overflow);
            }

            return rendered;
        }

        private static System.Collections.Generic.IEnumerable<System.Collections.Generic.List<FixedMessageSegment>> GetRenderSegmentLines(
            string content,
            int width,
            Overflow overflow,
            ColorScheme scheme)
        {
            if (overflow == ConsoulLibrary.Overflow.Wrap)
            {
                return MarkupRenderText.WrapSegments(content, width, scheme);
            }

            System.Collections.Generic.List<System.Collections.Generic.List<FixedMessageSegment>> lines = MarkupRenderText.GetLogicalLines(content, scheme);
            var rendered = new System.Collections.Generic.List<System.Collections.Generic.List<FixedMessageSegment>>();
            string ellipsis = overflow == ConsoulLibrary.Overflow.Ellipsis ? "..." : null;
            foreach (System.Collections.Generic.List<FixedMessageSegment> line in lines)
            {
                rendered.Add(MarkupRenderText.TruncateSegments(line, width, ellipsis));
            }

            return rendered;
        }

        private static string ApplyOverflow(string text, int width, Overflow overflow)
        {
            text = text ?? string.Empty;
            width = System.Math.Max(1, width);
            if (text.Length <= width)
            {
                return text;
            }

            if (overflow == ConsoulLibrary.Overflow.Ellipsis)
            {
                if (width <= 3)
                {
                    return new string('.', width);
                }

                return text.Substring(0, width - 3) + "...";
            }

            return text.Substring(0, width);
        }

        private static Justify ToJustify(TextBlockAlignment alignment)
        {
            switch (alignment)
            {
                case TextBlockAlignment.Center:
                    return Justify.Center;
                case TextBlockAlignment.Right:
                    return Justify.Right;
                default:
                    return Justify.Left;
            }
        }

        private static void WriteBlankLines(int count, ColorScheme scheme)
        {
            for (int index = 0; index < count; index++)
            {
                Consoul.Write(string.Empty, scheme, true);
            }
        }

        private static string Align(string text, int width, TextBlockAlignment alignment)
        {
            text = text ?? string.Empty;
            if (text.Length > width)
            {
                text = text.Substring(0, width);
            }

            int remaining = Math.Max(0, width - text.Length);
            switch (alignment)
            {
                case TextBlockAlignment.Center:
                    int left = remaining / 2;
                    int right = remaining - left;
                    return new string(' ', left) + text + new string(' ', right);
                case TextBlockAlignment.Right:
                    return new string(' ', remaining) + text;
                default:
                    return text.PadRight(width);
            }
        }
    }
}
