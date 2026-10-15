using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using ConsoulLibrary.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Delegate for handling when a <see cref="TableView"/> query yields no results.
    /// </summary>
    /// <param name="sender">The table that raised the event.</param>
    /// <param name="e">Details about the query that yielded no rows.</param>
    public delegate void TableQueryYieldsNoResults(object sender, TableQueryYieldsNoResultsEventArgs e);

    /// <summary>
    /// Represents a rendered table row composed of multiple table cells.
    /// </summary>
    public class TableRow : IDisposable
    {
        private readonly TableRenderOptions _renderOptions;
        private IReadOnlyList<TableCellAlignment> _columnAlignments = Array.Empty<TableCellAlignment>();
        private IReadOnlyList<Padding> _columnPaddings = Array.Empty<Padding>();
        private IReadOnlyList<Style> _columnStyles = Array.Empty<Style>();

        /// <summary>
        /// Gets the number of cells in the row.
        /// </summary>
        public int CellCount => _contents.Count;

        private readonly List<TableCell> _contents = new List<TableCell>();

        /// <summary>
        /// Gets the cells contained by this row.
        /// </summary>
        public IEnumerable<TableCell> Contents => _contents;

        /// <summary>
        /// Gets the cell at the specified index.
        /// </summary>
        /// <param name="index">Zero-based cell index.</param>
        /// <returns>The table cell at the specified index.</returns>
        public TableCell this[int index] => _contents[index];

        /// <summary>
        /// Initializes a row with the provided cell contents.
        /// </summary>
        /// <param name="contents">Cell text values for the row.</param>
        /// <param name="options">Render options applied to cells in this row.</param>
        public TableRow(IEnumerable<string> contents, TableRenderOptions options = default)
        {
            _renderOptions = options;

            foreach (var content in contents)
            {
                Add(content);
            }
        }

        /// <summary>
        /// Adds content as a new table cell.
        /// </summary>
        /// <param name="content"></param>
        public void Add(string content)
        {
            var tableCell = new TableCell(content, 0, _renderOptions);
            _contents.Add(tableCell);
        }

        private int GetTableWidth()
        {
            int configuredWidth = _renderOptions?.GetMaximumTableWidth() ?? Consoul.ConsoleBufferWidth;

            if (_contents.Count == 0)
            {
                return configuredWidth;
            }

            int separatorCount = _contents.Count + 1;
            int width = separatorCount + _contents.Sum(cell => Math.Max(0, cell.CellWidth));
            return Math.Min(width, configuredWidth);
        }

        /// <summary>
        /// Gets the width the row will occupy when rendered.
        /// </summary>
        /// <returns>The row render width in console cells.</returns>
        public int GetRenderWidth() => GetTableWidth();

        /// <summary>
        /// Renders the row with optional color schemes for content and separator lines.
        /// </summary>
        /// <param name="contentScheme">Optional color scheme for cell content.</param>
        /// <param name="lineScheme">Optional color scheme for row separators.</param>
        /// <param name="isHeader">Indicates whether the row is a header row.</param>
        public void Render(ColorScheme? contentScheme = null, ColorScheme? lineScheme = null, bool isHeader = false)
        {
            TableRenderStyle contentStyle = TableRenderStyle.From(contentScheme ?? RenderOptions.DefaultScheme);
            TableRenderStyle? lineStyle = lineScheme.HasValue
                ? TableRenderStyle.From(lineScheme.Value)
                : (TableRenderStyle?)null;
            Render(contentStyle, lineStyle, isHeader);
        }

        internal void Render(TableRenderStyle contentStyle, TableRenderStyle? lineStyle, bool isHeader = false)
        {
            string line = _renderOptions.Lines.GetBorderCharacters().Vertical.ToString();
            bool showVertical = isHeader
                ? _renderOptions?.Lines?.HeaderVertical ?? true
                : _renderOptions?.Lines?.ContentVertical ?? true;

            int topPadding = ResolveVerticalPadding(top: true);
            int bottomPadding = ResolveVerticalPadding(top: false);
            for (int i = 0; i < topPadding; i++)
            {
                RenderPaddingRow(showVertical, line, contentStyle, lineStyle);
            }

            int rowTop = Consoul.ConsoleDriver.CursorTop;
            int rowLeft = Consoul.ConsoleDriver.CursorLeft;

            if (showVertical)
            {
                TableRenderStyleWriter.Write(line, lineStyle, writeLine: false);
            }

            int currentLeft = Consoul.ConsoleDriver.CursorLeft;
            for (int i = 0; i < _contents.Count; i++)
            {
                var cell = _contents[i];
                int startLeft = Math.Max(0, Math.Min(currentLeft, Consoul.ConsoleBufferWidth - 1));
                int availableWidth = Math.Max(0, Consoul.ConsoleBufferWidth - startLeft);
                int renderWidth = Math.Min(Math.Max(0, cell.CellWidth), availableWidth);

                Consoul.ConsoleDriver.SetCursorPosition(startLeft, rowTop);
                cell.Render(renderWidth, ResolveCellStyle(i, contentStyle), ResolveCellAlignment(i, isHeader), ResolveCellPadding(i));

                currentLeft = startLeft + renderWidth;
                if (currentLeft >= Consoul.ConsoleBufferWidth)
                {
                    currentLeft = Consoul.ConsoleBufferWidth - 1;
                }

                if (showVertical)
                {
                    Consoul.ConsoleDriver.SetCursorPosition(Math.Max(0, Math.Min(currentLeft, Consoul.ConsoleBufferWidth - 1)), rowTop);
                    TableRenderStyleWriter.Write(line, lineStyle, writeLine: false);
                    currentLeft = Consoul.ConsoleDriver.CursorLeft;
                }
                else
                {
                    Consoul.ConsoleDriver.SetCursorPosition(Math.Max(0, Math.Min(currentLeft, Consoul.ConsoleBufferWidth - 1)), rowTop);
                }
            }

            if (_contents.Count == 0 && showVertical)
            {
                int closingLeft = Math.Min(rowLeft + GetTableWidth() - 1, Consoul.ConsoleBufferWidth - 1);
                Consoul.ConsoleDriver.SetCursorPosition(Math.Max(0, closingLeft), rowTop);
                TableRenderStyleWriter.Write(line, lineStyle, writeLine: false);
            }

            Consoul.Write(string.Empty);

            for (int i = 0; i < bottomPadding; i++)
            {
                RenderPaddingRow(showVertical, line, contentStyle, lineStyle);
            }
        }

        /// <summary>
        /// Updates existing cells with new content values.
        /// </summary>
        /// <param name="contents">New cell text values.</param>
        public void Update(IEnumerable<string> contents)
        {
            string[] values = contents.ToArray();
            for (int i = 0; i < values.Length; i++)
            {
                if (i < _contents.Count)
                {
                    _contents[i].Contents = values[i];
                }
                else
                {
                    Add(values[i]);
                }
            }

            for (int i = values.Length; i < _contents.Count; i++)
            {
                _contents[i].Contents = string.Empty;
            }
        }

        /// <summary>
        /// Updates one cell in the row.
        /// </summary>
        /// <param name="columnIndex">Zero-based cell index.</param>
        /// <param name="content">New cell text.</param>
        public void UpdateCell(int columnIndex, string content)
        {
            if (columnIndex < 0 || columnIndex >= _contents.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(columnIndex));
            }

            _contents[columnIndex].Contents = content ?? string.Empty;
        }

        internal void EnsureCellCount(int count)
        {
            while (_contents.Count < count)
            {
                Add(string.Empty);
            }
        }

        /// <summary>
        /// Applies calculated column widths to this row's cells.
        /// </summary>
        /// <param name="columnWidths">Column widths indexed by cell position.</param>
        public void ApplyColumnWidths(IReadOnlyList<int> columnWidths)
        {
            if (columnWidths == null)
            {
                return;
            }

            for (int i = 0; i < _contents.Count && i < columnWidths.Count; i++)
            {
                _contents[i].CellWidth = columnWidths[i];
            }
        }

        /// <summary>
        /// Applies per-column cell alignments to this row.
        /// </summary>
        /// <param name="columnAlignments">Column alignments indexed by cell position.</param>
        public void ApplyColumnAlignments(IReadOnlyList<TableCellAlignment> columnAlignments)
        {
            _columnAlignments = columnAlignments ?? Array.Empty<TableCellAlignment>();
        }

        /// <summary>
        /// Applies per-column cell padding to this row.
        /// </summary>
        /// <param name="columnPaddings">Column padding indexed by cell position.</param>
        public void ApplyColumnPaddings(IReadOnlyList<Padding> columnPaddings)
        {
            _columnPaddings = columnPaddings ?? Array.Empty<Padding>();
        }

        /// <summary>
        /// Applies per-column cell styles to this row.
        /// </summary>
        /// <param name="columnStyles">Column styles indexed by cell position.</param>
        public void ApplyColumnStyles(IReadOnlyList<Style> columnStyles)
        {
            _columnStyles = columnStyles ?? Array.Empty<Style>();
        }

        /// <summary>
        /// Gets the length of the cell content at the specified index.
        /// </summary>
        /// <param name="index">Zero-based cell index.</param>
        /// <returns>The content length, or 0 when the index is outside the row.</returns>
        public int GetCellContentLength(int index)
        {
            if (index < 0 || index >= _contents.Count)
            {
                return 0;
            }

            return _contents[index].DisplayContentLength;
        }

        /// <summary>
        /// Gets the desired rendered width for the cell content and configured padding.
        /// </summary>
        /// <param name="index">Zero-based cell index.</param>
        /// <returns>The desired cell width, or 0 when the index is outside the row.</returns>
        public int GetCellDesiredWidth(int index)
        {
            if (index < 0 || index >= _contents.Count)
            {
                return 0;
            }

            Padding padding = ResolveCellPadding(index);
            return GetCellContentLength(index) + Math.Max(0, padding.Left) + Math.Max(0, padding.Right);
        }

        private TableCellAlignment ResolveCellAlignment(int index, bool isHeader)
        {
            if (_columnAlignments != null &&
                index >= 0 &&
                index < _columnAlignments.Count &&
                _columnAlignments[index] != TableCellAlignment.Default)
            {
                return _columnAlignments[index];
            }

            TableCellAlignment alignment = isHeader
                ? _renderOptions?.HeaderAlignment ?? TableCellAlignment.Left
                : _renderOptions?.ContentAlignment ?? TableCellAlignment.Left;

            return alignment == TableCellAlignment.Default ? TableCellAlignment.Left : alignment;
        }

        private Padding ResolveCellPadding(int index)
        {
            if (_columnPaddings != null && index >= 0 && index < _columnPaddings.Count && _columnPaddings[index] != null)
            {
                return _columnPaddings[index];
            }

            return new Padding(1, 0, 0, 0);
        }

        private TableRenderStyle ResolveCellStyle(int index, TableRenderStyle fallback)
        {
            if (_columnStyles != null && index >= 0 && index < _columnStyles.Count && _columnStyles[index] != null)
            {
                return fallback.Apply(_columnStyles[index]);
            }

            return fallback;
        }

        private int ResolveVerticalPadding(bool top)
        {
            int padding = 0;
            for (int index = 0; index < _contents.Count; index++)
            {
                Padding cellPadding = ResolveCellPadding(index);
                padding = Math.Max(padding, top ? cellPadding.Top : cellPadding.Bottom);
            }

            return padding;
        }

        private void RenderPaddingRow(bool showVertical, string verticalLine, TableRenderStyle contentStyle, TableRenderStyle? lineStyle)
        {
            int rowTop = Consoul.ConsoleDriver.CursorTop;
            int rowLeft = Consoul.ConsoleDriver.CursorLeft;

            if (showVertical)
            {
                TableRenderStyleWriter.Write(verticalLine, lineStyle, writeLine: false);
            }

            int currentLeft = Consoul.ConsoleDriver.CursorLeft;
            for (int index = 0; index < _contents.Count; index++)
            {
                TableCell cell = _contents[index];
                int startLeft = Math.Max(0, Math.Min(currentLeft, Consoul.ConsoleBufferWidth - 1));
                int availableWidth = Math.Max(0, Consoul.ConsoleBufferWidth - startLeft);
                int renderWidth = Math.Min(Math.Max(0, cell.CellWidth), availableWidth);

                Consoul.ConsoleDriver.SetCursorPosition(startLeft, rowTop);
                if (renderWidth > 0)
                {
                    TableRenderStyleWriter.Write(new string(' ', renderWidth), ResolveCellStyle(index, contentStyle), writeLine: false);
                }

                currentLeft = startLeft + renderWidth;
                if (currentLeft >= Consoul.ConsoleBufferWidth)
                {
                    currentLeft = Consoul.ConsoleBufferWidth - 1;
                }

                Consoul.ConsoleDriver.SetCursorPosition(Math.Max(0, Math.Min(currentLeft, Consoul.ConsoleBufferWidth - 1)), rowTop);
                if (showVertical)
                {
                    TableRenderStyleWriter.Write(verticalLine, lineStyle, writeLine: false);
                    currentLeft = Consoul.ConsoleDriver.CursorLeft;
                }
            }

            if (_contents.Count == 0 && showVertical)
            {
                int closingLeft = Math.Min(rowLeft + GetTableWidth() - 1, Consoul.ConsoleBufferWidth - 1);
                Consoul.ConsoleDriver.SetCursorPosition(Math.Max(0, closingLeft), rowTop);
                TableRenderStyleWriter.Write(verticalLine, lineStyle, writeLine: false);
            }

            Consoul.Write(string.Empty);
        }

        /// <summary>
        /// Releases fixed message resources used by the row cells.
        /// </summary>
        public void Dispose()
        {
            foreach (var cell in _contents)
            {
                cell.Dispose();
            }
        }
    }

    /// <summary>
    /// Represents a single rendered table cell.
    /// </summary>
    public class TableCell : IDisposable
    {
        private readonly TableRenderOptions _renderOptions;

        private int _cellWidth;
        /// <summary>
        /// Gets or sets the width reserved for this cell.
        /// </summary>
        public int CellWidth
        {
            get => _cellWidth;
            set
            {
                int width = Math.Max(0, value);
                _cellWidth = width;
                _message.MaxWidth = width;
            }
        }

        private string _contents;
        /// <summary>
        /// Gets or sets the text rendered inside this cell.
        /// </summary>
        public string Contents
        {
            get => _contents;
            set
            {
                _contents = value;
            }
        }

        internal int DisplayContentLength => GetDisplayText(_contents).Length;

        /// <summary>
        /// Gets or sets the alignment used when no render-pass alignment is supplied.
        /// </summary>
        public TableCellAlignment Alignment { get; set; } = TableCellAlignment.Default;

        private FixedMessage _message;
        private bool _hasRendered;

        /// <summary>
        /// Initializes a table cell with content and a maximum width.
        /// </summary>
        /// <param name="content">Text rendered inside the cell.</param>
        /// <param name="maxWidth">Maximum width reserved for the cell.</param>
        /// <param name="options">Render options applied to this cell.</param>
        public TableCell(string content, int maxWidth, TableRenderOptions options = default)
        {
            _renderOptions = options;
            _contents = content;

            _cellWidth = Math.Max(0, maxWidth);
            _message = new FixedMessage(_cellWidth);
        }

        /// <summary>
        /// Renders the cell at its fixed position.
        /// </summary>
        /// <param name="overrideWidth">Optional width to use for this render pass.</param>
        /// <param name="contentScheme">Optional color scheme for the cell content.</param>
        /// <param name="alignment">Optional alignment for this render pass.</param>
        /// <param name="padding">Optional padding applied inside the cell.</param>
        public void Render(int? overrideWidth = null, ColorScheme? contentScheme = null, TableCellAlignment alignment = TableCellAlignment.Default, Padding padding = null)
        {
            Render(overrideWidth, TableRenderStyle.From(contentScheme ?? RenderOptions.DefaultScheme), alignment, padding);
        }

        internal void Render(int? overrideWidth, TableRenderStyle contentStyle, TableCellAlignment alignment = TableCellAlignment.Default, Padding padding = null)
        {
            int width = overrideWidth.HasValue ? Math.Max(0, overrideWidth.Value) : _cellWidth;
            _cellWidth = width;
            _message.MaxWidth = width;

            if (width <= 0)
            {
                if (_hasRendered)
                {
                    _message.Reset();
                }

                _message.Render(string.Empty);
                _hasRendered = true;
                return;
            }

            Padding effectivePadding = padding ?? new Padding(1, 0, 0, 0);
            int leftPadding = Math.Min(width, Math.Max(0, effectivePadding.Left));
            int rightPadding = Math.Min(Math.Max(0, width - leftPadding), Math.Max(0, effectivePadding.Right));
            int innerWidth = Math.Max(0, width - leftPadding - rightPadding);
            TableCellAlignment effectiveAlignment = alignment != TableCellAlignment.Default
                ? alignment
                : Alignment;

            List<FixedMessageSegment> contentSegments = innerWidth == 0
                ? new List<FixedMessageSegment>()
                : TruncateSegments(ParseMarkupSegments(_contents, contentStyle), innerWidth);
            int contentLength = GetSegmentLength(contentSegments);
            int alignmentPadding = Math.Max(0, innerWidth - contentLength);
            int leftAlignmentPadding = ResolveLeftAlignmentPadding(alignmentPadding, effectiveAlignment);
            int rightAlignmentPadding = alignmentPadding - leftAlignmentPadding;

            var contents = new List<FixedMessageSegment>();
            AddPaddingSegment(contents, leftPadding + leftAlignmentPadding, contentStyle);
            contents.AddRange(contentSegments);
            AddPaddingSegment(contents, rightAlignmentPadding + rightPadding, contentStyle);
            PadSegmentsOrTrim(contents, width, contentStyle);

            if (_hasRendered)
            {
                _message.Reset();
            }

            _message.RenderSegments(contents, contentStyle.Scheme.Color, contentStyle.Scheme.BackgroundColor);
            _hasRendered = true;
        }

        private static string AlignText(string text, int width, TableCellAlignment alignment)
        {
            text = text ?? string.Empty;
            width = Math.Max(0, width);
            if (width == 0)
            {
                return string.Empty;
            }

            if (text.Length >= width)
            {
                return text.Length == width ? text : text.Substring(0, width);
            }

            int padding = width - text.Length;
            switch (alignment)
            {
                case TableCellAlignment.Center:
                    int left = padding / 2;
                    int right = padding - left;
                    return new string(' ', left) + text + new string(' ', right);
                case TableCellAlignment.Right:
                    return new string(' ', padding) + text;
                default:
                    return text.PadRight(width);
            }
        }

        private static List<FixedMessageSegment> ParseMarkupSegments(string text, TableRenderStyle defaultStyle)
        {
            var segments = new List<FixedMessageSegment>();
            var styledScheme = new StyledColorScheme(
                defaultStyle.Scheme,
                defaultStyle.ForegroundRgb,
                defaultStyle.BackgroundRgb);
            foreach (MarkupSegment segment in MarkupEntry.Parse(text ?? string.Empty, styledScheme, defaultStyle.Decoration))
            {
                if (segment.Text.Length > 0)
                {
                    segments.Add(new FixedMessageSegment(
                        segment.Text,
                        segment.Scheme,
                        segment.Decoration,
                        segment.Link,
                        segment.ForegroundRgb,
                        segment.BackgroundRgb));
                }
            }

            return segments;
        }

        private static List<FixedMessageSegment> TruncateSegments(IList<FixedMessageSegment> segments, int width)
        {
            width = Math.Max(0, width);
            if (width == 0 || segments == null || segments.Count == 0)
            {
                return new List<FixedMessageSegment>();
            }

            int totalLength = GetSegmentLength(segments);
            if (totalLength <= width)
            {
                return new List<FixedMessageSegment>(segments);
            }

            int visibleLength = width <= 1 ? width : width - 1;
            var truncated = TakeVisibleSegments(segments, visibleLength);
            if (width > 1)
            {
                ColorScheme? scheme = truncated.Count > 0
                    ? truncated[truncated.Count - 1].Scheme
                    : (segments[0].Scheme ?? RenderOptions.DefaultScheme);
                Link link = truncated.Count > 0 ? truncated[truncated.Count - 1].Link : segments[0].Link;
                Decoration decoration = truncated.Count > 0 ? truncated[truncated.Count - 1].Decoration : segments[0].Decoration;
                RgbColor? foregroundRgb = truncated.Count > 0 ? truncated[truncated.Count - 1].ForegroundRgb : segments[0].ForegroundRgb;
                RgbColor? backgroundRgb = truncated.Count > 0 ? truncated[truncated.Count - 1].BackgroundRgb : segments[0].BackgroundRgb;
                truncated.Add(new FixedMessageSegment("…", scheme, decoration, link, foregroundRgb, backgroundRgb));
            }

            return truncated;
        }

        private static List<FixedMessageSegment> TakeVisibleSegments(IList<FixedMessageSegment> segments, int width)
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

                result.Add(new FixedMessageSegment(
                    text.Substring(0, remaining),
                    segment.Scheme,
                    segment.Decoration,
                    segment.Link,
                    segment.ForegroundRgb,
                    segment.BackgroundRgb));
                remaining = 0;
            }

            return result;
        }

        private static int GetSegmentLength(IEnumerable<FixedMessageSegment> segments)
        {
            return (segments ?? Enumerable.Empty<FixedMessageSegment>()).Sum(segment => (segment.Text ?? string.Empty).Length);
        }

        private static int ResolveLeftAlignmentPadding(int totalPadding, TableCellAlignment alignment)
        {
            totalPadding = Math.Max(0, totalPadding);
            switch (alignment)
            {
                case TableCellAlignment.Center:
                    return totalPadding / 2;
                case TableCellAlignment.Right:
                    return totalPadding;
                default:
                    return 0;
            }
        }

        private static void AddPaddingSegment(IList<FixedMessageSegment> segments, int count, TableRenderStyle style)
        {
            if (count <= 0)
            {
                return;
            }

            segments.Add(new FixedMessageSegment(
                new string(' ', count),
                style.Scheme,
                style.Decoration,
                null,
                style.ForegroundRgb,
                style.BackgroundRgb));
        }

        private static void PadSegmentsOrTrim(IList<FixedMessageSegment> segments, int width, TableRenderStyle style)
        {
            width = Math.Max(0, width);
            int length = GetSegmentLength(segments);
            if (length < width)
            {
                AddPaddingSegment(segments, width - length, style);
                return;
            }

            if (length <= width)
            {
                return;
            }

            List<FixedMessageSegment> truncated = TakeVisibleSegments(segments, width);
            segments.Clear();
            foreach (FixedMessageSegment segment in truncated)
            {
                segments.Add(segment);
            }
        }

        private static string GetDisplayText(string text)
        {
            return MarkupEntry.RemoveMarkupText(text ?? string.Empty);
        }

        /// <summary>
        /// Releases the fixed message resource used by the cell.
        /// </summary>
        public void Dispose()
        {
            _message?.Dispose();
        }
    }

    /// <summary>
    /// Renders and prompts over tabular data in the console.
    /// </summary>
    public class TableView : IDisposable, IRenderable
    {
        /// <summary>
        /// Gets or sets the options used to render table layout, borders, and colors.
        /// </summary>
        public TableRenderOptions TableRenderOptions { get; set; }

        private TableNavigator _tableNavigator;
        private TableInputHandler _tableInputHandler;

        // FixedMessage instances for each row and header
        private TableRow _header { get; set; }
        private TableRow _footer { get; set; }
        private List<TableRow> _rows = new List<TableRow>();
        private List<int> _columnWidths = new List<int>();
        private readonly List<TableCellAlignment> _columnAlignments = new List<TableCellAlignment>();
        private readonly List<int?> _columnPreferredWidths = new List<int?>();
        private readonly List<Padding> _columnPaddings = new List<Padding>();
        private readonly List<Style> _headerColumnStyles = new List<Style>();
        private readonly List<Style> _footerColumnStyles = new List<Style>();

        /// <summary>
        /// Occurs when a table query yields no matching rows.
        /// </summary>
        public event TableQueryYieldsNoResults QueryYieldsNoResults;

        /// <summary>
        /// Gets the number of content rows in the table.
        /// </summary>
        public int RowCount => _rows.Count;

        /// <summary>
        /// Gets the number of configured header cells in the table.
        /// </summary>
        public int HeaderCount => _header?.CellCount ?? 0;

        /// <summary>
        /// Gets the content rows in the table.
        /// </summary>
        public IReadOnlyList<TableRow> ContentRows => _rows;

        /// <summary>
        /// Raises the <see cref="QueryYieldsNoResults"/> event.
        /// </summary>
        /// <param name="message">Message associated with the empty query result.</param>
        /// <param name="query">Query text that yielded no rows.</param>
        protected virtual void OnQueryYieldsNoResults(string message, string query)
        {
            QueryYieldsNoResults?.Invoke(this, new TableQueryYieldsNoResultsEventArgs(message, query));
        }

        /// <summary>
        /// Initializes a table view with optional render options.
        /// </summary>
        /// <param name="options">Render options used by this table view.</param>
        public TableView(TableRenderOptions options = default)
        {
            TableRenderOptions = options ?? new TableRenderOptions();
            _tableInputHandler = new TableInputHandler();
            _header = new TableRow(new string[] { }, TableRenderOptions);
            _footer = new TableRow(new string[] { }, TableRenderOptions);
            Consoul.WindowResized += OnWindowResized; // Subscribe to window resize event
        }

        private void OnWindowResized(object sender, EventArgs e)
        {
            Render();
        }

        private void InitializeNavigator()
        {
            _tableNavigator = new TableNavigator(_rows.Count);
        }

        /// <summary>
        /// Renders the table and optional heading message.
        /// </summary>
        /// <param name="message">Optional message rendered before the table.</param>
        /// <param name="color">Optional foreground color for the message.</param>
        /// <param name="clearConsole">Indicates whether to clear the console before rendering.</param>
        public void Render(string message = default, ConsoleColor? color = null, bool clearConsole = true)
        {
            if (clearConsole)
            {
                Consoul.ConsoleDriver.Clear();
            }
            else
            {
                Consoul.ConsoleDriver.SetCursorPosition(0, 0);
            }

            RenderCore(message, color);
        }

        /// <summary>
        /// Renders the table at the current cursor position without clearing the console or moving to the origin.
        /// </summary>
        /// <param name="message">Optional message rendered before the table.</param>
        /// <param name="color">Optional foreground color for the message.</param>
        public void RenderInline(string message = default, ConsoleColor? color = null)
        {
            RenderCore(message, color);
        }

        /// <summary>
        /// Renders the table at the current cursor position when used as a reusable visual component.
        /// </summary>
        void IConsoulRenderable.Render()
        {
            RenderInline();
        }

        /// <summary>
        /// Prompts the user to select one or more rows from the table.
        /// </summary>
        /// <param name="message">Optional message to display before prompting.</param>
        /// <param name="color">Optional color for the message text.</param>
        /// <param name="allowEmpty">Specifies whether the user is allowed to accept an empty selection.</param>
        /// <param name="clearConsole">If <c>true</c>, clears the console before rendering.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A result describing selected row indexes, cancellation, or accepted empty selection.</returns>
        public TableSelectionResult Select(string message = "", ConsoleColor? color = null, bool allowEmpty = false, bool clearConsole = true, CancellationToken cancellationToken = default)
        {
            if (_tableNavigator == null)
                InitializeNavigator();

            string inputBuffer = "";

            do
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return TableSelectionResult.Canceled();
                }

                Render(message, color, inputBuffer == "" && clearConsole);

                if (!string.IsNullOrEmpty(inputBuffer))
                {
                    Consoul.Write($"\nCurrent input: {inputBuffer}", ConsoleColor.Cyan);
                }

                var keyInfo = Consoul.ConsoleDriver.ReadKey(intercept: true);
                if (cancellationToken.IsCancellationRequested)
                {
                    return TableSelectionResult.Canceled();
                }

                if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    _tableNavigator.MoveUp();
                    continue;
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    _tableNavigator.MoveDown();
                    continue;
                }
                else if (keyInfo.Key == ConsoleKey.Spacebar)
                {
                    _tableNavigator.ToggleSelection();
                    continue;
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    if (!string.IsNullOrEmpty(inputBuffer))
                    {
                        if (int.TryParse(inputBuffer, out int rowNumber) && rowNumber >= 1 && rowNumber <= _rows.Count)
                        {
                            _tableNavigator.SetCurrentRow(rowNumber - 1);
                            return TableSelectionResult.FromSelection(_tableNavigator.CurrentRow);
                        }
                        inputBuffer = "";
                        continue;
                    }
                    else
                    {
                        if (_tableNavigator.SelectedRows.Count > 0)
                        {
                            return TableSelectionResult.FromSelections(_tableNavigator.SelectedRows);
                        }

                        if (_tableNavigator.CurrentRow >= 0)
                        {
                            return TableSelectionResult.FromSelection(_tableNavigator.CurrentRow);
                        }

                        if (_tableNavigator.HoveredRow >= 0)
                        {
                            return TableSelectionResult.FromSelection(_tableNavigator.HoveredRow);
                        }

                        if (allowEmpty)
                        {
                            return TableSelectionResult.Empty();
                        }

                        continue;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    return TableSelectionResult.Canceled();
                }
                else if (char.IsDigit(keyInfo.KeyChar))
                {
                    inputBuffer += keyInfo.KeyChar;
                    continue;
                }
                else if (keyInfo.Key == ConsoleKey.Backspace && inputBuffer.Length > 0)
                {
                    inputBuffer = inputBuffer.Substring(0, inputBuffer.Length - 1);
                    continue;
                }

            } while (true);
        }

        /// <summary>
        /// Prompts the user to select an option from the table and returns the selected row index.
        /// </summary>
        /// <param name="message">Optional message to display before prompting.</param>
        /// <param name="color">Optional color for the message text.</param>
        /// <param name="allowEmpty">Specifies whether the user is allowed to skip the selection.</param>
        /// <param name="clearConsole">If <c>true</c>, clears the console before rendering.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The selected row index, or null if the user chooses to exit without a selection.</returns>
        public int? Prompt(string message = "", ConsoleColor? color = null, bool allowEmpty = false, bool clearConsole = true, CancellationToken cancellationToken = default)
        {
            TableSelectionResult result = Select(message, color, allowEmpty, clearConsole, cancellationToken);
            return result.HasSelection ? result.Index : (int?)null;
        }


        /// <summary>
        /// Adds a header cell to the table.
        /// </summary>
        /// <param name="label">Header text to add.</param>
        /// <param name="alignment">Optional column alignment for this header and its cells.</param>
        public void AddHeader(string label, TableCellAlignment alignment = TableCellAlignment.Default)
        {
            _header.Add(label);
            if (alignment != TableCellAlignment.Default)
            {
                SetColumnAlignment(_header.CellCount - 1, alignment);
            }
            else
            {
                ApplyColumnAlignments();
            }
            ApplyColumnPaddings();
            ApplyColumnStyles();
            //TableRenderOptions.Normalize(Contents);
        }

        /// <summary>
        /// Adds multiple header cells to the table.
        /// </summary>
        /// <param name="labels">Header texts to add.</param>
        public void AddHeaders(params string[] labels)
        {
            foreach (var label in labels)
            {
                AddHeader(label);
            }
        }

        /// <summary>
        /// Sets header text for a single table column.
        /// </summary>
        /// <param name="columnIndex">Zero-based column index.</param>
        /// <param name="label">Header text for the column.</param>
        public void SetHeader(int columnIndex, string label)
        {
            if (columnIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(columnIndex));
            }

            _header.EnsureCellCount(columnIndex + 1);
            _header.UpdateCell(columnIndex, label ?? string.Empty);
            _header.ApplyColumnAlignments(_columnAlignments);
            _header.ApplyColumnPaddings(_columnPaddings);
            _header.ApplyColumnStyles(_headerColumnStyles);
            _columnWidths.Clear();
        }

        /// <summary>
        /// Adds a new row to the table and optionally re-renders the table.
        /// </summary>
        /// <param name="row">The new row to add.</param>
        /// <param name="renderAfterAdding">If <c>true</c>, renders the table after adding the row.</param>
        public void AddRow(IEnumerable<string> row, bool renderAfterAdding = false)
        {
            string[] cellValues = row.ToArray();
            //if (cellValues.Length != _header?.Contents?.Count())
            //{
            //    Consoul.Write("Row does not match the number of headers.", ConsoleColor.Red);
            //    return;
            //}

            var tableRow = new TableRow(cellValues, TableRenderOptions);
            tableRow.ApplyColumnAlignments(_columnAlignments);
            tableRow.ApplyColumnPaddings(_columnPaddings);
            _rows.Add(tableRow);
            if (_tableNavigator != null)
            {
                // Update the navigator to include the new row
                ResetNavigator();
            }

            if (renderAfterAdding)
            {
                Render();
            }
        }

        /// <summary>
        /// Inserts a new row into the table and optionally re-renders the table.
        /// </summary>
        /// <param name="rowIndex">Zero-based index where the row should be inserted. Use <see cref="RowCount"/> to append.</param>
        /// <param name="row">The new row to insert.</param>
        /// <param name="renderAfterInserting">If <c>true</c>, renders the table after inserting the row.</param>
        public void InsertRow(int rowIndex, IEnumerable<string> row, bool renderAfterInserting = false)
        {
            if (rowIndex < 0 || rowIndex > _rows.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }

            string[] cellValues = (row ?? Enumerable.Empty<string>()).ToArray();
            var tableRow = new TableRow(cellValues, TableRenderOptions);
            tableRow.ApplyColumnAlignments(_columnAlignments);
            tableRow.ApplyColumnPaddings(_columnPaddings);
            _rows.Insert(rowIndex, tableRow);
            ResetNavigator();

            if (renderAfterInserting)
            {
                Render();
            }
        }

        /// <summary>
        /// Adds multiple rows to the table.
        /// </summary>
        /// <param name="rows">Rows to add.</param>
        /// <param name="renderAfterAdding">If <c>true</c>, renders the table after each row is added.</param>
        public void AddRows(IEnumerable<IEnumerable<string>> rows, bool renderAfterAdding = false)
        {
            foreach (var row in rows)
            {
                AddRow(row, renderAfterAdding);
            }
        }

        /// <summary>
        /// Gets a content row by index.
        /// </summary>
        /// <param name="rowIndex">Zero-based row index.</param>
        /// <returns>The content row at the specified index.</returns>
        public TableRow GetRow(int rowIndex)
        {
            ThrowIfRowIndexOutOfRange(rowIndex);
            return _rows[rowIndex];
        }

        /// <summary>
        /// Removes one content row from the table.
        /// </summary>
        /// <param name="rowIndex">Zero-based row index.</param>
        /// <param name="renderAfterRemoving">If <c>true</c>, renders the table after removing the row.</param>
        public void RemoveRow(int rowIndex, bool renderAfterRemoving = false)
        {
            ThrowIfRowIndexOutOfRange(rowIndex);
            _rows[rowIndex].Dispose();
            _rows.RemoveAt(rowIndex);
            ResetNavigator();

            if (renderAfterRemoving)
            {
                Render();
            }
        }

        /// <summary>
        /// Removes every content row from the table.
        /// </summary>
        /// <param name="renderAfterClearing">If <c>true</c>, renders the table after clearing rows.</param>
        public void ClearRows(bool renderAfterClearing = false)
        {
            foreach (var row in _rows)
            {
                row.Dispose();
            }

            _rows.Clear();
            ResetNavigator();
            if (renderAfterClearing)
            {
                Render();
            }
        }

        /// <summary>
        /// Updates a content row.
        /// </summary>
        /// <param name="rowIndex">Zero-based row index.</param>
        /// <param name="contents">Replacement cell values.</param>
        /// <param name="renderAfterUpdating">If <c>true</c>, renders the table after updating the row.</param>
        public void UpdateRow(int rowIndex, IEnumerable<string> contents, bool renderAfterUpdating = false)
        {
            ThrowIfRowIndexOutOfRange(rowIndex);
            _rows[rowIndex].Update(contents ?? Enumerable.Empty<string>());
            _rows[rowIndex].ApplyColumnAlignments(_columnAlignments);
            _rows[rowIndex].ApplyColumnPaddings(_columnPaddings);
            if (renderAfterUpdating)
            {
                Render();
            }
        }

        /// <summary>
        /// Updates one content cell.
        /// </summary>
        /// <param name="rowIndex">Zero-based row index.</param>
        /// <param name="columnIndex">Zero-based column index.</param>
        /// <param name="content">Replacement cell text.</param>
        /// <param name="renderAfterUpdating">If <c>true</c>, renders the table after updating the cell.</param>
        public void UpdateCell(int rowIndex, int columnIndex, string content, bool renderAfterUpdating = false)
        {
            ThrowIfRowIndexOutOfRange(rowIndex);
            _rows[rowIndex].UpdateCell(columnIndex, content);
            if (renderAfterUpdating)
            {
                Render();
            }
        }

        /// <summary>
        /// Sets the alignment for a single table column.
        /// </summary>
        /// <param name="columnIndex">Zero-based column index.</param>
        /// <param name="alignment">Alignment to apply to the column.</param>
        public void SetColumnAlignment(int columnIndex, TableCellAlignment alignment)
        {
            if (columnIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(columnIndex));
            }

            while (_columnAlignments.Count <= columnIndex)
            {
                _columnAlignments.Add(TableCellAlignment.Default);
            }

            _columnAlignments[columnIndex] = alignment;
            ApplyColumnAlignments();
        }

        /// <summary>
        /// Sets alignments for multiple table columns.
        /// </summary>
        /// <param name="alignments">Column alignments indexed by cell position.</param>
        public void SetColumnAlignments(params TableCellAlignment[] alignments)
        {
            _columnAlignments.Clear();
            if (alignments != null && alignments.Length > 0)
            {
                _columnAlignments.AddRange(alignments);
            }

            ApplyColumnAlignments();
        }

        /// <summary>
        /// Sets the preferred width for a single table column.
        /// </summary>
        /// <param name="columnIndex">Zero-based column index.</param>
        /// <param name="width">Preferred column width, or <see langword="null"/> to use content-based allocation.</param>
        public void SetColumnWidth(int columnIndex, int? width)
        {
            if (columnIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(columnIndex));
            }

            if (width.HasValue && width.Value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Column width must be greater than zero.");
            }

            while (_columnPreferredWidths.Count <= columnIndex)
            {
                _columnPreferredWidths.Add(null);
            }

            _columnPreferredWidths[columnIndex] = width;
            _columnWidths.Clear();
        }

        /// <summary>
        /// Sets footer text for a single table column.
        /// </summary>
        /// <param name="columnIndex">Zero-based column index.</param>
        /// <param name="footer">Footer text for the column.</param>
        public void SetColumnFooter(int columnIndex, string footer)
        {
            if (columnIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(columnIndex));
            }

            _footer.EnsureCellCount(columnIndex + 1);
            _footer.UpdateCell(columnIndex, footer ?? string.Empty);
            _footer.ApplyColumnAlignments(_columnAlignments);
            _footer.ApplyColumnPaddings(_columnPaddings);
            _footer.ApplyColumnStyles(_footerColumnStyles);
            _columnWidths.Clear();
        }

        /// <summary>
        /// Sets a style override for one header cell.
        /// </summary>
        /// <param name="columnIndex">Zero-based column index.</param>
        /// <param name="style">Style applied to the header cell, or <see langword="null"/> to use the table default.</param>
        public void SetHeaderStyle(int columnIndex, Style style)
        {
            if (columnIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(columnIndex));
            }

            while (_headerColumnStyles.Count <= columnIndex)
            {
                _headerColumnStyles.Add(null);
            }

            _headerColumnStyles[columnIndex] = style;
            _header?.ApplyColumnStyles(_headerColumnStyles);
        }

        /// <summary>
        /// Sets a style override for one configured column footer cell.
        /// </summary>
        /// <param name="columnIndex">Zero-based column index.</param>
        /// <param name="style">Style applied to the footer cell, or <see langword="null"/> to use the table default.</param>
        public void SetColumnFooterStyle(int columnIndex, Style style)
        {
            if (columnIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(columnIndex));
            }

            while (_footerColumnStyles.Count <= columnIndex)
            {
                _footerColumnStyles.Add(null);
            }

            _footerColumnStyles[columnIndex] = style;
            _footer?.ApplyColumnStyles(_footerColumnStyles);
        }

        /// <summary>
        /// Sets padding for a single table column.
        /// </summary>
        /// <param name="columnIndex">Zero-based column index.</param>
        /// <param name="padding">Padding to apply inside the column, or <see langword="null"/> to use the default cell padding.</param>
        public void SetColumnPadding(int columnIndex, Padding padding)
        {
            if (columnIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(columnIndex));
            }

            while (_columnPaddings.Count <= columnIndex)
            {
                _columnPaddings.Add(null);
            }

            _columnPaddings[columnIndex] = padding?.Clone();
            ApplyColumnPaddings();
            _columnWidths.Clear();
        }

        private void ResetNavigator()
        {
            _tableNavigator = _tableNavigator == null ? null : new TableNavigator(_rows.Count);
        }

        private void ThrowIfRowIndexOutOfRange(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= _rows.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }
        }

        private int GetTableWidth()
        {
            if (_columnWidths.Count == 0 && ((_header?.CellCount ?? 0) > 0 || _rows.Count > 0))
            {
                RecalculateColumnWidths();
            }

            int configuredWidth = TableRenderOptions.GetMaximumTableWidth();
            if (_columnWidths.Count == 0)
            {
                return configuredWidth;
            }

            int separators = _columnWidths.Count + 1;
            int width = separators + _columnWidths.Sum();
            if (width == 0)
            {
                return configuredWidth;
            }

            return Math.Min(width, configuredWidth);
        }

        private void RecalculateColumnWidths()
        {
            int headerColumns = _header?.CellCount ?? 0;
            int rowColumns = _rows.Count == 0 ? 0 : _rows.Max(r => r.CellCount);
            int footerColumns = TableRenderOptions.ShowFooters ? _footer?.CellCount ?? 0 : 0;
            int dataColumnCount = Math.Max(Math.Max(headerColumns, rowColumns), footerColumns);
            bool showRowNumbers = ShouldRenderRowNumbers(dataColumnCount);
            int columnCount = dataColumnCount + (showRowNumbers ? 1 : 0);

            _columnWidths.Clear();

            if (columnCount == 0)
            {
                return;
            }

            int configuredWidth = TableRenderOptions.GetMaximumTableWidth();
            int separators = columnCount + 1;
            int availableWidth = Math.Max(0, configuredWidth - separators);

            var desired = new int[columnCount];

            int dataOffset = showRowNumbers ? 1 : 0;
            if (showRowNumbers)
            {
                desired[0] = Math.Max(GetRowNumberHeader().Length, _rows.Count.ToString(CultureInfo.CurrentCulture).Length) + 1;
            }

            if (_header != null)
            {
                for (int i = 0; i < dataColumnCount; i++)
                {
                    desired[i + dataOffset] = Math.Max(desired[i + dataOffset], _header.GetCellDesiredWidth(i));
                }
            }

            foreach (var row in _rows)
            {
                for (int i = 0; i < dataColumnCount; i++)
                {
                    desired[i + dataOffset] = Math.Max(desired[i + dataOffset], row.GetCellDesiredWidth(i));
                }
            }

            if (_footer != null)
            {
                for (int i = 0; i < dataColumnCount; i++)
                {
                    desired[i + dataOffset] = Math.Max(desired[i + dataOffset], _footer.GetCellDesiredWidth(i));
                }
            }

            for (int i = 0; i < desired.Length; i++)
            {
                desired[i] = Math.Max(desired[i], 1);
            }

            int?[] preferredWidths = BuildPreferredWidths(dataColumnCount, dataOffset, columnCount);
            _columnWidths = AllocateColumnWidths(desired, availableWidth, preferredWidths, TableRenderOptions.Expand);

            if (!showRowNumbers)
            {
                _header?.ApplyColumnWidths(_columnWidths);
                _footer?.ApplyColumnWidths(_columnWidths);
                foreach (var row in _rows)
                {
                    row.ApplyColumnWidths(_columnWidths);
                }
            }

            ApplyColumnAlignments();
            ApplyColumnPaddings();
            ApplyColumnStyles();
        }

        private void ApplyColumnAlignments()
        {
            _header?.ApplyColumnAlignments(_columnAlignments);
            _footer?.ApplyColumnAlignments(_columnAlignments);
            foreach (var row in _rows)
            {
                row.ApplyColumnAlignments(_columnAlignments);
            }
        }

        private void ApplyColumnPaddings()
        {
            _header?.ApplyColumnPaddings(_columnPaddings);
            _footer?.ApplyColumnPaddings(_columnPaddings);
            foreach (var row in _rows)
            {
                row.ApplyColumnPaddings(_columnPaddings);
            }
        }

        private void ApplyColumnStyles()
        {
            _header?.ApplyColumnStyles(_headerColumnStyles);
            _footer?.ApplyColumnStyles(_footerColumnStyles);
        }

        private bool ShouldRenderRowNumbers(int dataColumnCount)
        {
            return TableRenderOptions.ShowRowNumbers && dataColumnCount > 0;
        }

        private string GetRowNumberHeader()
        {
            return string.IsNullOrEmpty(TableRenderOptions.RowNumberHeader)
                ? "#"
                : TableRenderOptions.RowNumberHeader;
        }

        private IReadOnlyList<TableCellAlignment> GetRenderColumnAlignments()
        {
            if (!ShouldRenderRowNumbers(Math.Max(_header?.CellCount ?? 0, _rows.Count == 0 ? 0 : _rows.Max(r => r.CellCount))))
            {
                return _columnAlignments;
            }

            var alignments = new List<TableCellAlignment> { TableRenderOptions.RowNumberAlignment };
            alignments.AddRange(_columnAlignments);
            return alignments;
        }

        private IReadOnlyList<Padding> GetRenderColumnPaddings()
        {
            if (!ShouldRenderRowNumbers(Math.Max(_header?.CellCount ?? 0, _rows.Count == 0 ? 0 : _rows.Max(r => r.CellCount))))
            {
                return _columnPaddings;
            }

            var paddings = new List<Padding> { null };
            paddings.AddRange(_columnPaddings);
            return paddings;
        }

        private IReadOnlyList<Style> GetRenderColumnStyles(IReadOnlyList<Style> columnStyles)
        {
            int dataColumnCount = Math.Max(
                Math.Max(_header?.CellCount ?? 0, _rows.Count == 0 ? 0 : _rows.Max(r => r.CellCount)),
                TableRenderOptions.ShowFooters ? _footer?.CellCount ?? 0 : 0);
            if (!ShouldRenderRowNumbers(dataColumnCount))
            {
                return columnStyles;
            }

            var styles = new List<Style> { null };
            styles.AddRange(columnStyles ?? Array.Empty<Style>());
            return styles;
        }

        private TableRow CreateNumberedHeaderRow()
        {
            TableRow row = CreateNumberedRow(GetRowNumberHeader(), _header);
            row.ApplyColumnStyles(GetRenderColumnStyles(_headerColumnStyles));
            return row;
        }

        private TableRow CreateNumberedContentRow(int rowIndex)
        {
            string rowNumber = (rowIndex + 1).ToString(CultureInfo.CurrentCulture);
            return CreateNumberedRow(rowNumber, _rows[rowIndex]);
        }

        private TableRow CreateNumberedFooterRow()
        {
            TableRow row = CreateNumberedRow(string.Empty, _footer);
            row.ApplyColumnStyles(GetRenderColumnStyles(_footerColumnStyles));
            return row;
        }

        private TableRow CreateNumberedRow(string firstCell, TableRow sourceRow)
        {
            IEnumerable<string> cells = new[] { firstCell ?? string.Empty }
                .Concat(sourceRow?.Contents.Select(cell => cell.Contents ?? string.Empty) ?? Enumerable.Empty<string>());
            var row = new TableRow(cells, TableRenderOptions);
            row.ApplyColumnWidths(_columnWidths);
            row.ApplyColumnAlignments(GetRenderColumnAlignments());
            row.ApplyColumnPaddings(GetRenderColumnPaddings());
            return row;
        }

        private static List<int> AllocateColumnWidths(IReadOnlyList<int> desired, int availableWidth, bool expand)
        {
            int columnCount = desired.Count;
            var result = Enumerable.Repeat(0, columnCount).ToList();

            if (columnCount == 0 || availableWidth <= 0)
            {
                return result;
            }

            int desiredTotal = desired.Sum();

            if (desiredTotal <= availableWidth)
            {
                for (int i = 0; i < columnCount; i++)
                {
                    result[i] = desired[i];
                }

                if (!expand)
                {
                    return result;
                }

                int remaining = availableWidth - desiredTotal;
                int index = 0;
                while (remaining > 0)
                {
                    result[index % columnCount]++;
                    index++;
                    remaining--;
                }

                return result;
            }

            double scale = (double)availableWidth / desiredTotal;
            int assigned = 0;
            var zeroWidthCandidates = new List<int>();

            for (int i = 0; i < columnCount; i++)
            {
                int width = (int)Math.Floor(desired[i] * scale);
                if (desired[i] > 0 && width == 0)
                {
                    zeroWidthCandidates.Add(i);
                }

                result[i] = width;
                assigned += width;
            }

            int remainingAllocation = availableWidth - assigned;

            foreach (int index in zeroWidthCandidates)
            {
                if (remainingAllocation == 0)
                {
                    break;
                }

                result[index]++;
                remainingAllocation--;
            }

            if (remainingAllocation > 0)
            {
                var order = Enumerable.Range(0, columnCount)
                    .OrderByDescending(i => desired[i] - result[i])
                    .ThenBy(i => i)
                    .ToList();

                int orderIndex = 0;
                while (remainingAllocation > 0 && order.Count > 0)
                {
                    int target = order[orderIndex];
                    result[target]++;
                    remainingAllocation--;
                    orderIndex = (orderIndex + 1) % order.Count;
                }
            }

            return result;
        }

        private static List<int> AllocateColumnWidths(IReadOnlyList<int> desired, int availableWidth, IReadOnlyList<int?> preferredWidths, bool expand)
        {
            if (preferredWidths == null || !preferredWidths.Any(width => width.HasValue))
            {
                return AllocateColumnWidths(desired, availableWidth, expand);
            }

            int columnCount = desired.Count;
            var result = Enumerable.Repeat(0, columnCount).ToList();
            if (columnCount == 0 || availableWidth <= 0)
            {
                return result;
            }

            var fixedIndexes = Enumerable.Range(0, columnCount)
                .Where(index => index < preferredWidths.Count && preferredWidths[index].HasValue)
                .ToList();

            var fixedDesired = Enumerable.Repeat(0, columnCount).ToArray();
            foreach (int index in fixedIndexes)
            {
                fixedDesired[index] = Math.Max(1, preferredWidths[index].Value);
            }

            int fixedTotal = fixedDesired.Sum();
            if (fixedTotal >= availableWidth)
            {
                return AllocateColumnWidths(fixedDesired, availableWidth, expand);
            }

            foreach (int index in fixedIndexes)
            {
                result[index] = fixedDesired[index];
            }

            int remainingWidth = availableWidth - fixedTotal;
            var flexibleIndexes = Enumerable.Range(0, columnCount)
                .Where(index => !fixedIndexes.Contains(index))
                .ToList();

            if (flexibleIndexes.Count == 0)
            {
                return result;
            }

            List<int> flexibleDesired = flexibleIndexes.Select(index => desired[index]).ToList();
            List<int> flexibleWidths = AllocateColumnWidths(flexibleDesired, remainingWidth, expand);

            for (int index = 0; index < flexibleIndexes.Count; index++)
            {
                result[flexibleIndexes[index]] = flexibleWidths[index];
            }

            return result;
        }

        private int?[] BuildPreferredWidths(int dataColumnCount, int dataOffset, int columnCount)
        {
            var widths = new int?[columnCount];
            for (int index = 0; index < dataColumnCount && index < _columnPreferredWidths.Count; index++)
            {
                widths[index + dataOffset] = _columnPreferredWidths[index];
            }

            return widths;
        }

        private TableRenderStyle GetRowContentStyle(int rowIndex)
        {
            if (_tableNavigator != null)
            {
                if (_tableNavigator.SelectedRows.Contains(rowIndex))
                {
                    return TableRenderStyle.From(TableRenderOptions.SelectionScheme);
                }

                if (_tableNavigator.HoveredRow == rowIndex)
                {
                    return TableRenderStyle.From(TableRenderOptions.HighlightedScheme);
                }
            }

            return rowIndex % 2 == 0
                ? TableRenderOptions.ResolveContentStyle1()
                : TableRenderOptions.ResolveContentStyle2();
        }

        private void RenderCore(string message, ConsoleColor? color)
        {
            RecalculateColumnWidths();
            int tableWidth = GetTableWidth();
            string topLine = BuildHorizontalBorder(TableBorderLinePosition.Top, tableWidth);
            string middleLine = BuildHorizontalBorder(TableBorderLinePosition.Middle, tableWidth);
            string bottomLine = BuildHorizontalBorder(TableBorderLinePosition.Bottom, tableWidth);
            TableRenderStyle lineStyle = TableRenderOptions.Lines.ResolveStyle();
            string title = !string.IsNullOrWhiteSpace(message) ? message : TableRenderOptions.Title;
            ColorScheme titleScheme = !string.IsNullOrWhiteSpace(message) && color.HasValue
                ? new ColorScheme { Color = color.Value, BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor }
                : TableRenderOptions.TitleScheme;
            Decoration titleDecoration = !string.IsNullOrWhiteSpace(message)
                ? Decoration.None
                : TableRenderOptions.TitleDecoration;

            bool hasConfiguredHeader = _header != null && _header.CellCount > 0;
            bool hasConfiguredFooter = TableRenderOptions.ShowFooters && HasConfiguredFooter();
            int dataColumnCount = Math.Max(
                Math.Max(_header?.CellCount ?? 0, _rows.Count == 0 ? 0 : _rows.Max(r => r.CellCount)),
                hasConfiguredFooter ? _footer?.CellCount ?? 0 : 0);
            bool showRowNumbers = ShouldRenderRowNumbers(dataColumnCount);
            bool hasHeader = TableRenderOptions.ShowHeaders && hasConfiguredHeader;
            bool hasRows = _rows.Count > 0;
            bool hasEmptyMessage = !hasRows && !string.IsNullOrWhiteSpace(TableRenderOptions.EmptyMessage);
            bool hasTableContent = hasHeader || hasRows || hasEmptyMessage || hasConfiguredFooter;
            bool usePresetBorder = TableRenderOptions.Lines.UsesPresetBorder();
            bool hasHorizontalLines = TableRenderOptions.Lines.HasHorizontalLines();

            RenderTableText(
                title,
                tableWidth,
                titleScheme,
                titleDecoration,
                !string.IsNullOrWhiteSpace(message) ? null : TableRenderOptions.TitleForegroundRgb,
                !string.IsNullOrWhiteSpace(message) ? null : TableRenderOptions.TitleBackgroundRgb,
                TableRenderOptions.TitleAlignment);

            if (hasTableContent && hasHorizontalLines)
            {
                TableRenderStyleWriter.Write(topLine, lineStyle);
            }

            if (hasHeader)
            {
                if (showRowNumbers)
                {
                    using (var header = CreateNumberedHeaderRow())
                    {
                        header.Render(TableRenderOptions.ResolveHeaderStyle(), lineStyle, isHeader: true);
                    }
                }
                else
                {
                    _header.Render(TableRenderOptions.ResolveHeaderStyle(), lineStyle, isHeader: true);
                }

                if (TableRenderOptions.Lines.HeaderHorizontal)
                {
                    TableRenderStyleWriter.Write(middleLine, lineStyle);
                }
            }

            for (int i = 0; i < _rows.Count; i++)
            {
                if (!usePresetBorder && TableRenderOptions.Lines.ContentHorizontal)
                {
                    TableRenderStyleWriter.Write(middleLine, lineStyle);
                }

                var contentStyle = GetRowContentStyle(i);
                if (showRowNumbers)
                {
                    using (var row = CreateNumberedContentRow(i))
                    {
                        row.Render(contentStyle, lineStyle);
                    }
                }
                else
                {
                    _rows[i].Render(contentStyle, lineStyle);
                }

                if (usePresetBorder && TableRenderOptions.Lines.ContentHorizontal && i < _rows.Count - 1)
                {
                    TableRenderStyleWriter.Write(middleLine, lineStyle);
                }
            }

            if (hasEmptyMessage)
            {
                RenderEmptyMessage(tableWidth, lineStyle);
            }

            if (hasConfiguredFooter)
            {
                if (TableRenderOptions.Lines.ContentHorizontal && (hasRows || hasHeader || hasEmptyMessage))
                {
                    TableRenderStyleWriter.Write(middleLine, lineStyle);
                }

                if (showRowNumbers)
                {
                    using (var footer = CreateNumberedFooterRow())
                    {
                        footer.Render(TableRenderOptions.ResolveFooterStyle(), lineStyle);
                    }
                }
                else
                {
                    _footer.Render(TableRenderOptions.ResolveFooterStyle(), lineStyle);
                }
            }

            if ((hasRows || hasConfiguredFooter) && TableRenderOptions.Lines.ContentHorizontal)
            {
                TableRenderStyleWriter.Write(usePresetBorder ? bottomLine : middleLine, lineStyle);
            }
            else if (usePresetBorder && (hasHeader || hasEmptyMessage || hasConfiguredFooter) && !hasRows)
            {
                TableRenderStyleWriter.Write(bottomLine, lineStyle);
            }
            else if (!usePresetBorder && hasEmptyMessage && TableRenderOptions.Lines.ContentHorizontal)
            {
                TableRenderStyleWriter.Write(middleLine, lineStyle);
            }

            RenderTableText(
                TableRenderOptions.Caption,
                tableWidth,
                TableRenderOptions.CaptionScheme,
                TableRenderOptions.CaptionDecoration,
                TableRenderOptions.CaptionForegroundRgb,
                TableRenderOptions.CaptionBackgroundRgb,
                TableRenderOptions.CaptionAlignment);
            RenderSummary(tableWidth);

            if (TableRenderOptions.ShowFooterHelp)
            {
                Consoul.Write("Use Arrow Keys to navigate, Enter to confirm, Space to select/deselect, Escape to exit, or enter row number to jump.", ConsoleColor.Gray);
            }
        }

        private bool HasConfiguredFooter()
        {
            return _footer != null &&
                _footer.CellCount > 0 &&
                _footer.Contents.Any(cell => !string.IsNullOrWhiteSpace(cell.Contents));
        }

        private enum TableBorderLinePosition
        {
            Top,
            Middle,
            Bottom
        }

        private string BuildHorizontalBorder(TableBorderLinePosition position, int width)
        {
            if (!TableRenderOptions.Lines.UsesPresetBorder())
            {
                return new string(TableRenderOptions.Lines.HorizontalCharacter, Math.Max(1, width));
            }

            TableRenderOptions.TableBorderCharacters border = TableRenderOptions.Lines.GetBorderCharacters();
            char left;
            char join;
            char right;

            switch (position)
            {
                case TableBorderLinePosition.Top:
                    left = border.TopLeft;
                    join = border.TopJoin;
                    right = border.TopRight;
                    break;
                case TableBorderLinePosition.Bottom:
                    left = border.BottomLeft;
                    join = border.BottomJoin;
                    right = border.BottomRight;
                    break;
                default:
                    left = border.MiddleLeft;
                    join = border.MiddleJoin;
                    right = border.MiddleRight;
                    break;
            }

            var builder = new System.Text.StringBuilder();
            builder.Append(left);

            if (_columnWidths.Count == 0)
            {
                builder.Append(new string(border.Horizontal, Math.Max(0, width - 2)));
            }
            else
            {
                for (int index = 0; index < _columnWidths.Count; index++)
                {
                    builder.Append(new string(border.Horizontal, Math.Max(0, _columnWidths[index])));
                    builder.Append(index == _columnWidths.Count - 1 ? right : join);
                }
            }

            return FitBorderLine(builder.ToString(), width);
        }

        private void RenderEmptyMessage(int tableWidth, TableRenderStyle lineStyle)
        {
            ColorScheme emptyScheme = TableRenderOptions.EmptyMessageScheme;
            TableRenderOptions.TableBorderCharacters border = TableRenderOptions.Lines.GetBorderCharacters();
            string vertical = border.Vertical.ToString();
            bool showVertical = TableRenderOptions.Lines.ContentVertical;
            int innerWidth = Math.Max(1, tableWidth - (showVertical ? 2 : 0));

            foreach (List<FixedMessageSegment> wrappedLine in MarkupRenderText.WrapSegments(
                TableRenderOptions.EmptyMessage,
                innerWidth,
                emptyScheme,
                TableRenderOptions.EmptyMessageDecoration,
                TableRenderOptions.EmptyMessageForegroundRgb,
                TableRenderOptions.EmptyMessageBackgroundRgb))
            {
                List<FixedMessageSegment> line = MarkupRenderText.TruncateSegments(wrappedLine, innerWidth);
                line = MarkupRenderText.PadSegments(line, 0, Math.Max(0, innerWidth - MarkupRenderText.GetLength(line)), emptyScheme);
                if (showVertical)
                {
                    TableRenderStyleWriter.Write(vertical, lineStyle, writeLine: false);
                }

                MarkupRenderText.WriteSegments(line, emptyScheme, false);

                if (showVertical)
                {
                    TableRenderStyleWriter.Write(vertical, lineStyle, writeLine: false);
                }

                Consoul.Write(string.Empty);
            }
        }

        private static void RenderTableText(
            string text,
            int tableWidth,
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

            int width = Math.Max(1, tableWidth);
            foreach (List<FixedMessageSegment> line in MarkupRenderText.WrapSegments(
                text,
                width,
                scheme,
                decoration,
                foregroundRgb,
                backgroundRgb))
            {
                MarkupRenderText.WriteSegments(
                    MarkupRenderText.AlignSegments(line, width, alignment, scheme),
                    scheme,
                    true);
            }
        }

        private void RenderSummary(int tableWidth)
        {
            if (string.IsNullOrWhiteSpace(TableRenderOptions.SummaryText))
            {
                return;
            }

            ColorScheme summaryScheme = TableRenderOptions.SummaryScheme;
            int width = Math.Max(1, tableWidth);
            foreach (List<FixedMessageSegment> line in MarkupRenderText.WrapSegments(
                TableRenderOptions.SummaryText,
                width,
                summaryScheme,
                TableRenderOptions.SummaryDecoration,
                TableRenderOptions.SummaryForegroundRgb,
                TableRenderOptions.SummaryBackgroundRgb))
            {
                MarkupRenderText.WriteSegments(
                    MarkupRenderText.AlignSegments(line, width, TextBlockAlignment.Left, summaryScheme),
                    summaryScheme,
                    true);
            }
        }

        private static string FitBorderLine(string line, int width)
        {
            width = Math.Max(1, width);
            line = line ?? string.Empty;
            if (line.Length == width)
            {
                return line;
            }

            if (line.Length > width)
            {
                return line.Substring(0, width);
            }

            return line.PadRight(width);
        }

        private static string AlignText(string text, int width, TextBlockAlignment alignment)
        {
            text = text ?? string.Empty;
            width = Math.Max(1, width);
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

        private static string PadOrTrim(string text, int width)
        {
            text = text ?? string.Empty;
            width = Math.Max(1, width);
            if (text.Length > width)
            {
                return text.Substring(0, width);
            }

            return text.PadRight(width);
        }

        /// <summary>
        /// Creates a table with headers and rows.
        /// </summary>
        /// <param name="items">Rows to add to the table.</param>
        /// <param name="columnNames">Header labels for the table.</param>
        /// <returns>A table view containing the provided headers and rows.</returns>
        public static TableView Create(IEnumerable<IEnumerable<string>> items, params string[] columnNames)
        {
            var table = new TableView();
            table.AddHeaders(columnNames);
            table.AddRows(items);
            return table;
        }

        /// <summary>
        /// Releases resize listeners and fixed message resources used by the table.
        /// </summary>
        public void Dispose()
        {
            Consoul.WindowResized -= OnWindowResized;
            _header?.Dispose();
            foreach (var row in _rows)
            {
                row.Dispose();
            }
        }
    }
}
