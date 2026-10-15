using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders vertically stacked text or captured render-helper rows.
    /// </summary>
    public class RowsEntry : IRenderable
    {
        private readonly List<RowsItem> _rows = new List<RowsItem>();
        private readonly RowsOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="RowsEntry"/> class.
        /// </summary>
        /// <param name="options">Optional rendering options for the rows.</param>
        public RowsEntry(RowsOptions options = null)
        {
            _options = options != null ? options.Clone() : new RowsOptions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RowsEntry"/> class with text rows.
        /// </summary>
        /// <param name="rows">The row contents to render.</param>
        /// <param name="options">Optional rendering options for the rows.</param>
        public RowsEntry(IEnumerable<string> rows, RowsOptions options = null) : this(options)
        {
            if (rows == null)
            {
                return;
            }

            foreach (string row in rows)
            {
                Add(row);
            }
        }

        /// <summary>
        /// Gets the rows rendered by this entry.
        /// </summary>
        public IList<RowsItem> Rows => _rows;

        /// <summary>
        /// Gets the options used to render the rows.
        /// </summary>
        public RowsOptions Options => _options;

        /// <summary>
        /// Adds a text row.
        /// </summary>
        /// <param name="content">The row content.</param>
        /// <param name="color">Optional foreground color used for the row content.</param>
        /// <returns>The added row item.</returns>
        public RowsItem Add(string content, ConsoleColor? color = null)
        {
            var row = RowsItem.Create(content, color);
            _rows.Add(row);
            return row;
        }

        /// <summary>
        /// Adds a row, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The row content.</param>
        /// <param name="color">Optional foreground color used for the row content.</param>
        /// <returns>The added row item.</returns>
        public RowsItem Add(object content, ConsoleColor? color = null)
        {
            var row = content as RowsItem;
            if (row != null)
            {
                return Add(row);
            }

            var renderable = content as IConsoulRenderable;
            if (renderable != null)
            {
                return Add(renderable, color);
            }

            return Add(RenderActionContent.FormatValue(content), color);
        }

        /// <summary>
        /// Adds a text row using Spectre-style style text.
        /// </summary>
        /// <param name="content">The row content.</param>
        /// <param name="style">Optional style used for the row content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <returns>The added row item.</returns>
        public RowsItem Add(string content, string style)
        {
            var row = RowsItem.Create(content, style);
            _rows.Add(row);
            return row;
        }

        /// <summary>
        /// Adds a row using Spectre-style style text, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The row content.</param>
        /// <param name="style">Optional style used for the row content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <returns>The added row item.</returns>
        public RowsItem Add(object content, string style)
        {
            var row = content as RowsItem;
            if (row != null)
            {
                return Add(row);
            }

            var renderable = content as IConsoulRenderable;
            if (renderable != null)
            {
                return Add(renderable, style);
            }

            return Add(RenderActionContent.FormatValue(content), style);
        }

        /// <summary>
        /// Adds a text row using a reusable style.
        /// </summary>
        /// <param name="content">The row content.</param>
        /// <param name="style">Optional style used for the row content.</param>
        /// <returns>The added row item.</returns>
        public RowsItem Add(string content, Style style)
        {
            var row = RowsItem.Create(content, style);
            _rows.Add(row);
            return row;
        }

        /// <summary>
        /// Adds a row using a reusable style, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The row content.</param>
        /// <param name="style">Optional style used for the row content.</param>
        /// <returns>The added row item.</returns>
        public RowsItem Add(object content, Style style)
        {
            var row = content as RowsItem;
            if (row != null)
            {
                return Add(row);
            }

            var renderable = content as IConsoulRenderable;
            if (renderable != null)
            {
                return Add(renderable, style);
            }

            return Add(RenderActionContent.FormatValue(content), style);
        }

        /// <summary>
        /// Adds a row whose content is captured from a Consoul render action.
        /// </summary>
        /// <param name="render">The render action used to produce row content.</param>
        /// <param name="color">Optional foreground color used for the captured content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        /// <returns>The added row item.</returns>
        public RowsItem Add(Action render, ConsoleColor? color = null, int? captureWidth = null, int captureHeight = 200)
        {
            return Add(CaptureRow(render, captureWidth, captureHeight), color);
        }

        /// <summary>
        /// Adds a row whose content is captured from a Consoul render action using Spectre-style style text.
        /// </summary>
        /// <param name="render">The render action used to produce row content.</param>
        /// <param name="style">Optional style used for the captured content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        /// <returns>The added row item.</returns>
        public RowsItem Add(Action render, string style, int? captureWidth = null, int captureHeight = 200)
        {
            return Add(CaptureRow(render, captureWidth, captureHeight), style);
        }

        /// <summary>
        /// Adds a row whose content is captured from a Consoul render action using a reusable style.
        /// </summary>
        /// <param name="render">The render action used to produce row content.</param>
        /// <param name="style">Optional style used for the captured content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        /// <returns>The added row item.</returns>
        public RowsItem Add(Action render, Style style, int? captureWidth = null, int captureHeight = 200)
        {
            return Add(CaptureRow(render, captureWidth, captureHeight), style);
        }

        /// <summary>
        /// Adds a row whose content is captured from a Consoul renderable component.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce row content.</param>
        /// <param name="color">Optional foreground color used for the captured content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The added row item.</returns>
        public RowsItem Add(IConsoulRenderable renderable, ConsoleColor? color = null, int? captureWidth = null, int captureHeight = 200)
        {
            return Add(CaptureRow(renderable, captureWidth, captureHeight), color);
        }

        /// <summary>
        /// Adds a row whose content is captured from a Consoul renderable component using Spectre-style style text.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce row content.</param>
        /// <param name="style">Optional style used for the captured content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The added row item.</returns>
        public RowsItem Add(IConsoulRenderable renderable, string style, int? captureWidth = null, int captureHeight = 200)
        {
            return Add(CaptureRow(renderable, captureWidth, captureHeight), style);
        }

        /// <summary>
        /// Adds a row whose content is captured from a Consoul renderable component using a reusable style.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce row content.</param>
        /// <param name="style">Optional style used for the captured content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The added row item.</returns>
        public RowsItem Add(IConsoulRenderable renderable, Style style, int? captureWidth = null, int captureHeight = 200)
        {
            return Add(CaptureRow(renderable, captureWidth, captureHeight), style);
        }

        /// <summary>
        /// Adds a configured row item.
        /// </summary>
        /// <param name="row">The configured row item to add.</param>
        /// <returns>The added row item.</returns>
        public RowsItem Add(RowsItem row)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            _rows.Add(row);
            return row;
        }

        /// <summary>
        /// Renders the rows to the active console driver.
        /// </summary>
        public void Render()
        {
            if (_rows.Count == 0)
            {
                return;
            }

            int width = ResolveRowsWidth();
            int spacing = _options.ResolveSpacing();
            for (int rowIndex = 0; rowIndex < _rows.Count; rowIndex++)
            {
                RowsItem row = _rows[rowIndex];
                ColorScheme scheme = row.Scheme ?? _options.ResolveContentScheme();
                bool hasRowStyle = row.Scheme.HasValue ||
                    row.Decoration != Decoration.None ||
                    row.ForegroundRgb.HasValue ||
                    row.BackgroundRgb.HasValue;
                Decoration decoration = hasRowStyle ? row.Decoration : _options.ContentDecoration;
                RgbColor? foregroundRgb = hasRowStyle ? row.ForegroundRgb : _options.ContentForegroundRgb;
                RgbColor? backgroundRgb = hasRowStyle ? row.BackgroundRgb : _options.ContentBackgroundRgb;
                foreach (List<FixedMessageSegment> line in MarkupRenderText.WrapSegments(
                    row.Content,
                    width,
                    scheme,
                    decoration,
                    foregroundRgb,
                    backgroundRgb))
                {
                    MarkupRenderText.WriteSegments(
                        PadOrTrimSegments(line, width, scheme, decoration, foregroundRgb, backgroundRgb),
                        scheme,
                        true);
                }

                if (rowIndex < _rows.Count - 1)
                {
                    WriteBlankLines(spacing);
                }
            }
        }

        /// <summary>
        /// Renders temporary rows to the active console driver.
        /// </summary>
        /// <param name="rows">The row contents to render.</param>
        /// <param name="options">Optional rendering options for the rows.</param>
        public static void Render(IEnumerable<string> rows, RowsOptions options = null)
        {
            new RowsEntry(rows, options).Render();
        }

        private int ResolveRowsWidth()
        {
            if (_options.Width.HasValue || _options.Expand)
            {
                return PanelEntry.ResolveWidth(_options.Width);
            }

            int terminalWidth = PanelEntry.ResolveWidth((int?)null);
            int measured = 1;
            foreach (RowsItem row in _rows)
            {
                measured = Math.Max(measured, MeasureTextWidth(row.Content));
            }

            return Math.Max(1, Math.Min(measured, terminalWidth));
        }

        private static int MeasureTextWidth(string text)
        {
            int maximum = 0;
            foreach (List<FixedMessageSegment> line in MarkupRenderText.GetLogicalLines(text, RenderOptions.DefaultScheme))
            {
                maximum = Math.Max(maximum, MarkupRenderText.GetLength(line));
            }

            return maximum;
        }

        private string CaptureRow(Action render, int? captureWidth, int captureHeight)
        {
            return NormalizeCapturedRow(RenderActionContent.Capture(render, captureWidth ?? _options.Width, captureHeight));
        }

        private string CaptureRow(IConsoulRenderable renderable, int? captureWidth, int captureHeight)
        {
            return NormalizeCapturedRow(RenderActionContent.Capture(renderable, captureWidth ?? _options.Width, captureHeight));
        }

        private static string NormalizeCapturedRow(string content)
        {
            string normalized = (content ?? string.Empty).TrimEnd('\r', '\n').Replace("\r\n", "\n").Replace('\r', '\n');
            string[] lines = normalized.Split('\n');
            for (int index = 0; index < lines.Length; index++)
            {
                lines[index] = lines[index].TrimEnd(' ');
            }

            return string.Join("\n", lines);
        }

        private static void WriteBlankLines(int count)
        {
            for (int index = 0; index < count; index++)
            {
                Consoul.Write(string.Empty);
            }
        }

        private static List<FixedMessageSegment> PadOrTrimSegments(
            IList<FixedMessageSegment> segments,
            int width,
            ColorScheme scheme,
            Decoration decoration,
            RgbColor? foregroundRgb,
            RgbColor? backgroundRgb)
        {
            width = Math.Max(1, width);
            List<FixedMessageSegment> result = MarkupRenderText.TruncateSegments(segments, width);
            AddPadding(
                result,
                Math.Max(0, width - MarkupRenderText.GetLength(result)),
                scheme,
                decoration,
                foregroundRgb,
                backgroundRgb);
            return result;
        }

        private static void AddPadding(
            IList<FixedMessageSegment> segments,
            int count,
            ColorScheme scheme,
            Decoration decoration,
            RgbColor? foregroundRgb,
            RgbColor? backgroundRgb)
        {
            if (count <= 0)
            {
                return;
            }

            segments.Add(new FixedMessageSegment(
                new string(' ', count),
                scheme,
                decoration,
                null,
                foregroundRgb,
                backgroundRgb));
        }
    }
}
