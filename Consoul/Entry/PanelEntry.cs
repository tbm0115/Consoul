using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders text inside a responsive bordered panel for console views.
    /// </summary>
    public class PanelEntry : IRenderable
    {
        private readonly PanelOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="PanelEntry"/> class.
        /// </summary>
        /// <param name="content">The panel content to render.</param>
        /// <param name="options">Optional rendering options for the panel.</param>
        public PanelEntry(string content, PanelOptions options = null)
        {
            Content = content ?? string.Empty;
            _options = options != null ? options.Clone() : new PanelOptions();
        }

        /// <summary>
        /// Gets or sets the panel content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets the options used to render the panel.
        /// </summary>
        public PanelOptions Options => _options;

        /// <summary>
        /// Renders the panel to the active console driver.
        /// </summary>
        public virtual void Render()
        {
            int width = ResolveWidth(_options, Content);
            if (_options.BorderStyle == PanelBorderStyle.None)
            {
                RenderBorderless(width);
                return;
            }

            if (width < 4)
            {
                RenderFallback(width);
                return;
            }

            Padding padding = _options.ResolveContentPadding();
            int leftPadding = Math.Max(0, padding.Left);
            int rightPadding = Math.Max(0, padding.Right);
            int innerWidth = Math.Max(1, width - 2 - leftPadding - rightPadding);
            ColorScheme borderScheme = _options.ResolveBorderScheme();
            ColorScheme contentScheme = _options.ResolveContentScheme();
            ColorScheme titleScheme = _options.ResolveTitleScheme();
            ColorScheme footerScheme = _options.ResolveFooterScheme();

            WriteBorder(
                BuildTopBorder(width),
                _options.Title,
                borderScheme,
                _options.BorderDecoration,
                _options.BorderForegroundRgb,
                _options.BorderBackgroundRgb,
                titleScheme,
                _options.TitleDecoration,
                _options.TitleForegroundRgb,
                _options.TitleBackgroundRgb);

            for (int index = 0; index < Math.Max(0, padding.Top); index++)
            {
                WriteContentLine(string.Empty, innerWidth, leftPadding, rightPadding, borderScheme, contentScheme);
            }

            foreach (List<FixedMessageSegment> line in MarkupRenderText.WrapSegments(
                Content,
                innerWidth,
                contentScheme,
                _options.ContentDecoration,
                _options.ContentForegroundRgb,
                _options.ContentBackgroundRgb))
            {
                WriteContentLine(line, innerWidth, leftPadding, rightPadding, borderScheme, contentScheme);
            }

            for (int index = 0; index < Math.Max(0, padding.Bottom); index++)
            {
                WriteContentLine(string.Empty, innerWidth, leftPadding, rightPadding, borderScheme, contentScheme);
            }

            WriteBorder(
                BuildBottomBorder(width),
                _options.Footer,
                borderScheme,
                _options.BorderDecoration,
                _options.BorderForegroundRgb,
                _options.BorderBackgroundRgb,
                footerScheme,
                _options.FooterDecoration,
                _options.FooterForegroundRgb,
                _options.FooterBackgroundRgb);
        }

        /// <summary>
        /// Renders a temporary panel to the active console driver.
        /// </summary>
        /// <param name="content">The panel content to render.</param>
        /// <param name="title">Optional title rendered in the top border.</param>
        /// <param name="width">Optional requested width. The actual width is clamped to the current terminal width.</param>
        /// <param name="color">Optional foreground color for content text.</param>
        /// <param name="borderColor">Optional foreground color for the panel border.</param>
        public static void Render(string content, string title = null, int? width = null, ConsoleColor? color = null, ConsoleColor? borderColor = null)
        {
            var options = new PanelOptions
            {
                Title = title,
                Width = width
            };

            if (color.HasValue)
            {
                options.ContentScheme = new ColorScheme
                {
                    Color = color.Value,
                    BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
                };
            }

            if (borderColor.HasValue)
            {
                options.BorderScheme = new ColorScheme
                {
                    Color = borderColor.Value,
                    BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
                };
            }

            new PanelEntry(content, options).Render();
        }

        /// <summary>
        /// Captures a render action and renders the captured text inside a responsive bordered panel.
        /// </summary>
        /// <param name="render">The render action used to produce panel content.</param>
        /// <param name="options">Optional rendering options for the panel.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        public static void Render(Action render, PanelOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            PanelOptions resolvedOptions = options != null ? options.Clone() : new PanelOptions();
            string content = RenderActionContent.Capture(render, captureWidth ?? ResolveInnerWidth(resolvedOptions), captureHeight);
            new PanelEntry(content, resolvedOptions).Render();
        }

        /// <summary>
        /// Captures a renderable component and renders the captured text inside a responsive bordered panel.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce panel content.</param>
        /// <param name="options">Optional rendering options for the panel.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        public static void Render(IConsoulRenderable renderable, PanelOptions options = null, int? captureWidth = null, int captureHeight = 200)
        {
            PanelOptions resolvedOptions = options != null ? options.Clone() : new PanelOptions();
            string content = RenderActionContent.Capture(renderable, captureWidth ?? ResolveInnerWidth(resolvedOptions), captureHeight);
            new PanelEntry(content, resolvedOptions).Render();
        }

        internal static int ResolveWidth(int? requestedWidth)
        {
            int terminalWidth = ResolveTerminalWidth();
            int requested = requestedWidth ?? terminalWidth;
            return Math.Max(1, Math.Min(requested, terminalWidth));
        }

        internal static int ResolveWidth(PanelOptions options, string content)
        {
            if (options == null)
            {
                return ResolveWidth((int?)null);
            }

            if (options.Width.HasValue || options.Expand)
            {
                return ResolveWidth(options.Width);
            }

            int terminalWidth = ResolveTerminalWidth();
            int requested = MeasureCollapsedWidth(options, content);
            return Math.Max(1, Math.Min(requested, terminalWidth));
        }

        private static int ResolveInnerWidth(PanelOptions options)
        {
            int width = options != null && !options.Width.HasValue && !options.Expand
                ? ResolveWidth((int?)null)
                : ResolveWidth(options, null);
            if (width < 4)
            {
                return width;
            }

            Padding padding = options != null ? options.ResolveContentPadding() : new Padding(0);
            int borderWidth = options != null && options.BorderStyle == PanelBorderStyle.None ? 0 : 2;
            return Math.Max(1, width - borderWidth - padding.Horizontal);
        }

        private static int ResolveTerminalWidth()
        {
            return Math.Max(1, Consoul.ConsoleBufferWidth - 1);
        }

        private static int MeasureCollapsedWidth(PanelOptions options, string content)
        {
            Padding padding = options.ResolveContentPadding();
            int borderWidth = options.BorderStyle == PanelBorderStyle.None ? 0 : 2;
            int contentWidth = MeasureContentWidth(content);
            int titleWidth = MeasureHeaderFooterWidth(options.Title);
            int footerWidth = MeasureHeaderFooterWidth(options.Footer);
            int innerWidth = Math.Max(contentWidth, Math.Max(titleWidth, footerWidth));
            return Math.Max(1, innerWidth + Math.Max(0, padding.Horizontal) + borderWidth);
        }

        private static int MeasureContentWidth(string content)
        {
            int maximum = 0;
            foreach (string line in NormalizeLines(content))
            {
                maximum = Math.Max(maximum, MarkupRenderText.GetPlainText(line).Length);
            }

            return maximum;
        }

        private static int MeasureHeaderFooterWidth(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }

            int maximum = 0;
            foreach (string line in NormalizeLines(text.Trim()))
            {
                maximum = Math.Max(maximum, MarkupRenderText.GetPlainText(line).Length);
            }

            return maximum + 2;
        }

        private static string[] NormalizeLines(string value)
        {
            return (value ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        }

        internal static IList<string> WrapLines(string content, int width)
        {
            var lines = new List<string>();
            width = Math.Max(1, width);
            content = content ?? string.Empty;

            string normalized = content.Replace("\r\n", "\n").Replace('\r', '\n');
            string[] sourceLines = normalized.Split('\n');
            if (sourceLines.Length == 0)
            {
                lines.Add(string.Empty);
                return lines;
            }

            foreach (string sourceLine in sourceLines)
            {
                if (sourceLine.Length == 0)
                {
                    lines.Add(string.Empty);
                    continue;
                }

                string remaining = sourceLine;
                while (remaining.Length > width)
                {
                    int breakIndex = FindWrapIndex(remaining, width);
                    string line = remaining.Substring(0, breakIndex).TrimEnd();
                    lines.Add(line);

                    remaining = remaining.Substring(breakIndex).TrimStart();
                    if (remaining.Length == 0)
                    {
                        break;
                    }
                }

                if (remaining.Length > 0)
                {
                    lines.Add(remaining);
                }
            }

            if (lines.Count == 0)
            {
                lines.Add(string.Empty);
            }

            return lines;
        }

        private static int FindWrapIndex(string text, int width)
        {
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

        private void RenderFallback(int width)
        {
            ColorScheme contentScheme = _options.ResolveContentScheme();
            foreach (List<FixedMessageSegment> line in MarkupRenderText.WrapSegments(
                Content,
                Math.Max(1, width),
                contentScheme,
                _options.ContentDecoration,
                _options.ContentForegroundRgb,
                _options.ContentBackgroundRgb))
            {
                MarkupRenderText.WriteSegments(MarkupRenderText.TruncateSegments(line, Math.Max(1, width)), contentScheme, true);
            }
        }

        private void RenderBorderless(int width)
        {
            width = Math.Max(1, width);
            Padding padding = _options.ResolveContentPadding();
            int leftPadding = Math.Min(Math.Max(0, padding.Left), Math.Max(0, width - 1));
            int rightPadding = Math.Min(Math.Max(0, padding.Right), Math.Max(0, width - leftPadding - 1));
            int innerWidth = Math.Max(1, width - leftPadding - rightPadding);
            ColorScheme contentScheme = _options.ResolveContentScheme();
            ColorScheme titleScheme = _options.ResolveTitleScheme();
            ColorScheme footerScheme = _options.ResolveFooterScheme();

            WriteBorderlessText(
                _options.Title,
                width,
                titleScheme,
                _options.TitleDecoration,
                _options.TitleForegroundRgb,
                _options.TitleBackgroundRgb,
                _options.TitleAlignment);

            for (int index = 0; index < Math.Max(0, padding.Top); index++)
            {
                WriteBorderlessContentLine(string.Empty, innerWidth, leftPadding, rightPadding, contentScheme);
            }

            foreach (List<FixedMessageSegment> line in MarkupRenderText.WrapSegments(
                Content,
                innerWidth,
                contentScheme,
                _options.ContentDecoration,
                _options.ContentForegroundRgb,
                _options.ContentBackgroundRgb))
            {
                WriteBorderlessContentLine(line, innerWidth, leftPadding, rightPadding, contentScheme);
            }

            for (int index = 0; index < Math.Max(0, padding.Bottom); index++)
            {
                WriteBorderlessContentLine(string.Empty, innerWidth, leftPadding, rightPadding, contentScheme);
            }

            WriteBorderlessText(
                _options.Footer,
                width,
                footerScheme,
                _options.FooterDecoration,
                _options.FooterForegroundRgb,
                _options.FooterBackgroundRgb,
                _options.FooterAlignment);
        }

        private static void WriteBorderlessText(
            string text,
            int width,
            ColorScheme scheme,
            Decoration decoration,
            RgbColor? foregroundRgb,
            RgbColor? backgroundRgb,
            TextBlockAlignment alignment)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            foreach (List<FixedMessageSegment> line in MarkupRenderText.WrapSegments(
                text.Trim(),
                width,
                scheme,
                decoration,
                foregroundRgb,
                backgroundRgb))
            {
                MarkupRenderText.WriteSegments(MarkupRenderText.AlignSegments(line, width, alignment, scheme), scheme, true);
            }
        }

        private static void WriteBorderlessContentLine(
            string content,
            int innerWidth,
            int leftPaddingWidth,
            int rightPaddingWidth,
            ColorScheme contentScheme)
        {
            WriteBorderlessContentLine(
                MarkupRenderText.ParseSegments(content, contentScheme),
                innerWidth,
                leftPaddingWidth,
                rightPaddingWidth,
                contentScheme);
        }

        private static void WriteBorderlessContentLine(
            IList<FixedMessageSegment> content,
            int innerWidth,
            int leftPaddingWidth,
            int rightPaddingWidth,
            ColorScheme contentScheme)
        {
            List<FixedMessageSegment> value = MarkupRenderText.TruncateSegments(content, innerWidth);
            MarkupRenderText.AddPadding(value, Math.Max(0, innerWidth - MarkupRenderText.GetLength(value)), contentScheme);
            MarkupRenderText.WriteSegments(
                MarkupRenderText.PadSegments(value, leftPaddingWidth, rightPaddingWidth, contentScheme),
                contentScheme,
                true);
        }

        private static string AlignText(string text, int width, TextBlockAlignment alignment)
        {
            width = Math.Max(1, width);
            text = text ?? string.Empty;
            if (text.Length > width)
            {
                return text.Substring(0, width);
            }

            int remaining = width - text.Length;
            switch (alignment)
            {
                case TextBlockAlignment.Center:
                    int left = remaining / 2;
                    return new string(' ', left) + text + new string(' ', remaining - left);
                case TextBlockAlignment.Right:
                    return new string(' ', remaining) + text;
                default:
                    return text + new string(' ', remaining);
            }
        }

        private string BuildTopBorder(int width)
        {
            PanelBorderCharacters border = _options.GetBorderCharacters();
            return BuildBorder(width, border.TopLeft, border.TopRight, border.Horizontal, _options.Title, _options.TitleAlignment);
        }

        private string BuildBottomBorder(int width)
        {
            PanelBorderCharacters border = _options.GetBorderCharacters();
            return BuildBorder(width, border.BottomLeft, border.BottomRight, border.Horizontal, _options.Footer, _options.FooterAlignment);
        }

        private void WriteContentLine(
            string content,
            int innerWidth,
            int leftPaddingWidth,
            int rightPaddingWidth,
            ColorScheme borderScheme,
            ColorScheme contentScheme)
        {
            string leftPadding = new string(' ', Math.Max(0, leftPaddingWidth));
            string rightPadding = new string(' ', Math.Max(0, rightPaddingWidth));
            string paddedContent = (content ?? string.Empty).PadRight(innerWidth);
            PanelBorderCharacters border = _options.GetBorderCharacters();

            WriteBorderSegment(border.Vertical.ToString(), borderScheme, false);
            WriteContentSegment(leftPadding, contentScheme, false);
            WriteContentSegment(paddedContent, contentScheme, false);
            WriteContentSegment(rightPadding, contentScheme, false);
            WriteBorderSegment(border.Vertical.ToString(), borderScheme, true);
        }

        private void WriteContentLine(
            IList<FixedMessageSegment> content,
            int innerWidth,
            int leftPaddingWidth,
            int rightPaddingWidth,
            ColorScheme borderScheme,
            ColorScheme contentScheme)
        {
            List<FixedMessageSegment> value = MarkupRenderText.TruncateSegments(content, innerWidth);
            MarkupRenderText.AddPadding(value, Math.Max(0, innerWidth - MarkupRenderText.GetLength(value)), contentScheme);
            PanelBorderCharacters border = _options.GetBorderCharacters();

            WriteBorderSegment(border.Vertical.ToString(), borderScheme, false);
            MarkupRenderText.WriteSegments(
                MarkupRenderText.PadSegments(value, leftPaddingWidth, rightPaddingWidth, contentScheme),
                contentScheme,
                false);
            WriteBorderSegment(border.Vertical.ToString(), borderScheme, true);
        }

        private static string BuildBorder(
            int width,
            char leftCorner,
            char rightCorner,
            char horizontal,
            string text,
            TextBlockAlignment alignment)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return leftCorner + new string(horizontal, width - 2) + rightCorner;
            }

            string visibleText = BuildBorderText(text, width);
            int remaining = Math.Max(0, width - 2 - visibleText.Length);
            int leftPadding;
            int rightPadding;

            switch (alignment)
            {
                case TextBlockAlignment.Center:
                    leftPadding = remaining / 2;
                    rightPadding = remaining - leftPadding;
                    break;
                case TextBlockAlignment.Right:
                    leftPadding = remaining;
                    rightPadding = 0;
                    break;
                default:
                    leftPadding = 0;
                    rightPadding = remaining;
                    break;
            }

            return leftCorner +
                new string(horizontal, leftPadding) +
                visibleText +
                new string(horizontal, rightPadding) +
                rightCorner;
        }

        private static string BuildBorderText(string text, int width)
        {
            string value = " " + MarkupRenderText.GetPlainText((text ?? string.Empty).Trim()).Trim() + " ";
            int maximumWidth = Math.Max(0, width - 2);
            if (value.Length <= maximumWidth)
            {
                return value;
            }

            return value.Substring(0, maximumWidth);
        }

        private void WriteBorder(
            string border,
            string text,
            ColorScheme borderScheme,
            Decoration borderDecoration,
            RgbColor? borderForegroundRgb,
            RgbColor? borderBackgroundRgb,
            ColorScheme textScheme,
            Decoration textDecoration,
            RgbColor? textForegroundRgb,
            RgbColor? textBackgroundRgb)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                WriteLine(border, borderScheme, borderDecoration, borderForegroundRgb, borderBackgroundRgb);
                return;
            }

            string visibleText = BuildBorderText(text, border.Length);
            int textStart = border.IndexOf(visibleText, StringComparison.Ordinal);
            if (textStart < 0)
            {
                WriteLine(border, borderScheme, borderDecoration, borderForegroundRgb, borderBackgroundRgb);
                return;
            }

            int trimmedOffset = visibleText.StartsWith(" ", StringComparison.Ordinal) ? 1 : 0;
            string trimmedText = visibleText.Trim();
            if (trimmedText.Length == 0)
            {
                WriteLine(border, borderScheme, borderDecoration, borderForegroundRgb, borderBackgroundRgb);
                return;
            }

            int trimmedStart = textStart + trimmedOffset;
            Write(border.Substring(0, trimmedStart), borderScheme, false, borderDecoration, borderForegroundRgb, borderBackgroundRgb);
            MarkupRenderText.WriteSegments(
                MarkupRenderText.TruncateSegments(
                    MarkupRenderText.TrimSegments(MarkupRenderText.ParseSegments(
                        text.Trim(),
                        textScheme,
                        textDecoration,
                        textForegroundRgb,
                        textBackgroundRgb)),
                    trimmedText.Length),
                textScheme,
                false);
            Write(
                border.Substring(trimmedStart + trimmedText.Length),
                borderScheme,
                true,
                borderDecoration,
                borderForegroundRgb,
                borderBackgroundRgb);
        }

        private void WriteBorderSegment(string value, ColorScheme scheme, bool writeLine)
        {
            Write(value, scheme, writeLine, _options.BorderDecoration, _options.BorderForegroundRgb, _options.BorderBackgroundRgb);
        }

        private void WriteContentSegment(string value, ColorScheme scheme, bool writeLine)
        {
            Write(value, scheme, writeLine, _options.ContentDecoration, _options.ContentForegroundRgb, _options.ContentBackgroundRgb);
        }

        private static void WriteLine(
            string value,
            ColorScheme scheme,
            Decoration decoration,
            RgbColor? foregroundRgb,
            RgbColor? backgroundRgb)
        {
            Write(value, scheme, true, decoration, foregroundRgb, backgroundRgb);
        }

        private static void Write(
            string value,
            ColorScheme scheme,
            bool writeLine,
            Decoration decoration = Decoration.None,
            RgbColor? foregroundRgb = null,
            RgbColor? backgroundRgb = null)
        {
            TerminalLinkWriter.WriteText(value, scheme, null, decoration, foregroundRgb, backgroundRgb);
            if (writeLine)
            {
                Consoul.Write(string.Empty);
            }
        }
    }
}
