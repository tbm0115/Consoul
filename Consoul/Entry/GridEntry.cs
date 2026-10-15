using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders responsive columns for console view content.
    /// </summary>
    public class GridEntry : IRenderable
    {
        private readonly List<GridCell> _cells = new List<GridCell>();
        private readonly List<GridColumn> _columns = new List<GridColumn>();
        private readonly GridOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="GridEntry"/> class.
        /// </summary>
        /// <param name="options">Optional rendering options for the grid.</param>
        public GridEntry(GridOptions options = null)
        {
            _options = options != null ? options.Clone() : new GridOptions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridEntry"/> class with text cells.
        /// </summary>
        /// <param name="cells">The cell contents to render.</param>
        /// <param name="options">Optional rendering options for the grid.</param>
        public GridEntry(IEnumerable<string> cells, GridOptions options = null) : this(options)
        {
            if (cells == null)
            {
                return;
            }

            foreach (string cell in cells)
            {
                Add(cell);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridEntry"/> class with mixed plain values and renderable cells.
        /// </summary>
        /// <param name="cells">The cell values to render.</param>
        /// <param name="options">Optional rendering options for the grid.</param>
        public GridEntry(IEnumerable<object> cells, GridOptions options = null) : this(options)
        {
            if (cells == null)
            {
                return;
            }

            foreach (object cell in cells)
            {
                Add(cell);
            }
        }

        /// <summary>
        /// Gets the cells rendered by the grid.
        /// </summary>
        public IList<GridCell> Cells => _cells;

        /// <summary>
        /// Gets configured grid columns used by Spectre-style manual grid builders.
        /// </summary>
        public IList<GridColumn> Columns => _columns;

        /// <summary>
        /// Gets the options used to render the grid.
        /// </summary>
        public GridOptions Options => _options;

        /// <summary>
        /// Adds a configured grid column.
        /// </summary>
        /// <param name="column">The column configuration.</param>
        /// <returns>The added <see cref="GridColumn"/>.</returns>
        protected GridColumn AddColumnDefinition(GridColumn column)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            _columns.Add(column);
            _options.Columns = _columns.Count;
            return column;
        }

        /// <summary>
        /// Adds a text cell to the grid.
        /// </summary>
        /// <param name="content">The cell content.</param>
        /// <param name="header">Optional header rendered above the content.</param>
        /// <param name="color">Optional foreground color used for the content.</param>
        /// <returns>The added grid cell.</returns>
        public GridCell Add(string content, string header = null, ConsoleColor? color = null)
        {
            var cell = GridCell.Create(content, header, color);
            _cells.Add(cell);
            return cell;
        }

        /// <summary>
        /// Adds a cell to the grid, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The cell content.</param>
        /// <param name="header">Optional header rendered above the content.</param>
        /// <param name="color">Optional foreground color used for the content.</param>
        /// <returns>The added grid cell.</returns>
        public GridCell Add(object content, string header = null, ConsoleColor? color = null)
        {
            var cell = content as GridCell;
            if (cell != null)
            {
                return Add(cell);
            }

            var renderable = content as IConsoulRenderable;
            if (renderable != null)
            {
                return Add(renderable, header, color);
            }

            return Add(RenderActionContent.FormatValue(content), header, color);
        }

        /// <summary>
        /// Adds a text cell to the grid using Spectre-style style text.
        /// </summary>
        /// <param name="content">The cell content.</param>
        /// <param name="header">Optional header rendered above the content.</param>
        /// <param name="style">Optional style used for the content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <returns>The added grid cell.</returns>
        public GridCell Add(string content, string header, string style)
        {
            var cell = GridCell.Create(content, header, style);
            _cells.Add(cell);
            return cell;
        }

        /// <summary>
        /// Adds a cell using Spectre-style style text, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The cell content.</param>
        /// <param name="header">Optional header rendered above the content.</param>
        /// <param name="style">Optional style used for the content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <returns>The added grid cell.</returns>
        public GridCell Add(object content, string header, string style)
        {
            var cell = content as GridCell;
            if (cell != null)
            {
                return Add(cell);
            }

            var renderable = content as IConsoulRenderable;
            if (renderable != null)
            {
                return Add(renderable, header, style);
            }

            return Add(RenderActionContent.FormatValue(content), header, style);
        }

        /// <summary>
        /// Adds a text cell to the grid using a reusable style.
        /// </summary>
        /// <param name="content">The cell content.</param>
        /// <param name="header">Optional header rendered above the content.</param>
        /// <param name="style">Optional style used for the content.</param>
        /// <returns>The added grid cell.</returns>
        public GridCell Add(string content, string header, Style style)
        {
            var cell = GridCell.Create(content, header, style);
            _cells.Add(cell);
            return cell;
        }

        /// <summary>
        /// Adds a cell to the grid using a reusable style, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The cell content.</param>
        /// <param name="header">Optional header rendered above the content.</param>
        /// <param name="style">Optional style used for the content.</param>
        /// <returns>The added grid cell.</returns>
        public GridCell Add(object content, string header, Style style)
        {
            var cell = content as GridCell;
            if (cell != null)
            {
                return Add(cell);
            }

            var renderable = content as IConsoulRenderable;
            if (renderable != null)
            {
                return Add(renderable, header, style);
            }

            return Add(RenderActionContent.FormatValue(content), header, style);
        }

        /// <summary>
        /// Adds a cell whose content is captured from a Consoul render action.
        /// </summary>
        /// <param name="render">The render action used to produce cell content.</param>
        /// <param name="header">Optional header rendered above the content.</param>
        /// <param name="color">Optional foreground color used for the captured content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        /// <returns>The added grid cell.</returns>
        public GridCell Add(Action render, string header = null, ConsoleColor? color = null, int? captureWidth = null, int captureHeight = 200)
        {
            return Add(RenderActionContent.Capture(render, captureWidth ?? _options.Width, captureHeight), header, color);
        }

        /// <summary>
        /// Adds a cell whose content is captured from a Consoul render action using Spectre-style style text.
        /// </summary>
        /// <param name="render">The render action used to produce cell content.</param>
        /// <param name="header">Optional header rendered above the content.</param>
        /// <param name="style">Optional style used for the captured content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        /// <returns>The added grid cell.</returns>
        public GridCell Add(Action render, string header, string style, int? captureWidth = null, int captureHeight = 200)
        {
            return Add(RenderActionContent.Capture(render, captureWidth ?? _options.Width, captureHeight), header, style);
        }

        /// <summary>
        /// Adds a cell whose content is captured from a Consoul render action using a reusable style.
        /// </summary>
        /// <param name="render">The render action used to produce cell content.</param>
        /// <param name="header">Optional header rendered above the content.</param>
        /// <param name="style">Optional style used for the captured content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        /// <returns>The added grid cell.</returns>
        public GridCell Add(Action render, string header, Style style, int? captureWidth = null, int captureHeight = 200)
        {
            return Add(RenderActionContent.Capture(render, captureWidth ?? _options.Width, captureHeight), header, style);
        }

        /// <summary>
        /// Adds a cell whose content is captured from a Consoul renderable component.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce cell content.</param>
        /// <param name="header">Optional header rendered above the content.</param>
        /// <param name="color">Optional foreground color used for the captured content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The added grid cell.</returns>
        public GridCell Add(IConsoulRenderable renderable, string header = null, ConsoleColor? color = null, int? captureWidth = null, int captureHeight = 200)
        {
            return Add(RenderActionContent.Capture(renderable, captureWidth ?? _options.Width, captureHeight), header, color);
        }

        /// <summary>
        /// Adds a cell whose content is captured from a Consoul renderable component using Spectre-style style text.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce cell content.</param>
        /// <param name="header">Optional header rendered above the content.</param>
        /// <param name="style">Optional style used for the captured content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The added grid cell.</returns>
        public GridCell Add(IConsoulRenderable renderable, string header, string style, int? captureWidth = null, int captureHeight = 200)
        {
            return Add(RenderActionContent.Capture(renderable, captureWidth ?? _options.Width, captureHeight), header, style);
        }

        /// <summary>
        /// Adds a cell whose content is captured from a Consoul renderable component using a reusable style.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce cell content.</param>
        /// <param name="header">Optional header rendered above the content.</param>
        /// <param name="style">Optional style used for the captured content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The added grid cell.</returns>
        public GridCell Add(IConsoulRenderable renderable, string header, Style style, int? captureWidth = null, int captureHeight = 200)
        {
            return Add(RenderActionContent.Capture(renderable, captureWidth ?? _options.Width, captureHeight), header, style);
        }

        /// <summary>
        /// Adds a cell to the grid.
        /// </summary>
        /// <param name="cell">The cell to add.</param>
        /// <returns>The added grid cell.</returns>
        public GridCell Add(GridCell cell)
        {
            if (cell == null)
            {
                throw new ArgumentNullException(nameof(cell));
            }

            _cells.Add(cell);
            return cell;
        }

        /// <summary>
        /// Renders the grid to the active console driver.
        /// </summary>
        public void Render()
        {
            if (_cells.Count == 0)
            {
                return;
            }

            int width = ResolveGridWidth();
            int columns = ResolveColumnCount(width);
            int gutter = columns > 1 ? _options.ResolveGutter() : 0;
            IReadOnlyList<int> columnWidths = ResolveColumnWidths(width, columns, gutter);

            for (int start = 0; start < _cells.Count; start += columns)
            {
                List<List<GridRenderedLine>> renderedCells = new List<List<GridRenderedLine>>();
                int rowCellCount = Math.Min(columns, _cells.Count - start);
                for (int offset = 0; offset < rowCellCount; offset++)
                {
                    int definitionIndex = GetColumnDefinitionIndex(start + offset);
                    renderedCells.Add(BuildCellLines(
                        _cells[start + offset],
                        columnWidths[offset],
                        GetColumnDefinition(definitionIndex)));
                }

                int rowHeight = renderedCells.Count == 0 ? 0 : renderedCells.Max(lines => lines.Count);
                for (int lineIndex = 0; lineIndex < rowHeight; lineIndex++)
                {
                    for (int cellIndex = 0; cellIndex < rowCellCount; cellIndex++)
                    {
                        GridRenderedLine line = lineIndex < renderedCells[cellIndex].Count
                            ? renderedCells[cellIndex][lineIndex]
                            : new GridRenderedLine(
                                Array.Empty<FixedMessageSegment>(),
                                _options.ResolveContentScheme(),
                                _options.ContentDecoration,
                                _options.ContentForegroundRgb,
                                _options.ContentBackgroundRgb);

                        List<FixedMessageSegment> padded = PadOrTrimSegments(
                            line.Segments,
                            columnWidths[cellIndex],
                            line.Scheme,
                            line.Decoration,
                            line.ForegroundRgb,
                            line.BackgroundRgb);
                        bool isLastCell = cellIndex == rowCellCount - 1;
                        MarkupRenderText.WriteSegments(padded, line.Scheme, isLastCell);

                        if (!isLastCell && gutter > 0)
                        {
                            Consoul.Write(new string(' ', gutter), RenderOptions.DefaultScheme, false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Renders a temporary grid to the active console driver.
        /// </summary>
        /// <param name="cells">The cell contents to render.</param>
        /// <param name="options">Optional rendering options for the grid.</param>
        public static void Render(IEnumerable<string> cells, GridOptions options = null)
        {
            new GridEntry(cells, options).Render();
        }

        private int ResolveGridWidth()
        {
            if (_options.Width.HasValue || _options.Expand)
            {
                return PanelEntry.ResolveWidth(_options.Width);
            }

            int terminalWidth = PanelEntry.ResolveWidth((int?)null);
            int preferredColumns = ResolvePreferredColumnCount();
            int measured = MeasureCollapsedWidth(preferredColumns);
            return Math.Max(1, Math.Min(measured, terminalWidth));
        }

        private int ResolvePreferredColumnCount()
        {
            int configuredColumns = _columns.Count > 0
                ? _columns.Count
                : _options.Columns.GetValueOrDefault();
            if (configuredColumns > 0)
            {
                return Math.Min(configuredColumns, Math.Max(1, _cells.Count));
            }

            return Math.Max(1, _cells.Count);
        }

        private int MeasureCollapsedWidth(int preferredColumns)
        {
            int columns = Math.Max(1, Math.Min(preferredColumns, Math.Max(1, _cells.Count)));
            int gutter = columns > 1 ? _options.ResolveGutter() : 0;
            int minimumColumnWidth = _options.ResolveMinimumColumnWidth();
            int total = gutter * Math.Max(0, columns - 1);

            for (int columnIndex = 0; columnIndex < columns; columnIndex++)
            {
                GridColumn column = GetColumnDefinition(columnIndex);
                int measured = column != null && column.Width.HasValue
                    ? Math.Max(1, column.Width.Value)
                    : MeasureColumnContentWidth(columnIndex, columns, column);

                total += Math.Max(minimumColumnWidth, measured);
            }

            return total;
        }

        private int MeasureColumnContentWidth(int columnIndex, int columns, GridColumn column)
        {
            Padding padding = column != null ? column.ResolvePadding() : new Padding(0);
            int paddingWidth = Math.Max(0, padding.Horizontal);
            int maximum = 1;

            for (int cellIndex = columnIndex; cellIndex < _cells.Count; cellIndex += columns)
            {
                GridCell cell = _cells[cellIndex];
                maximum = Math.Max(maximum, MeasureTextWidth(cell.Header));
                maximum = Math.Max(maximum, MeasureTextWidth(cell.Content));
            }

            return maximum + paddingWidth;
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

        private int ResolveColumnCount(int width)
        {
            int gutter = _options.ResolveGutter();
            int minimumColumnWidth = _options.ResolveMinimumColumnWidth();
            int configuredColumns = _columns.Count > 0
                ? _columns.Count
                : _options.Columns.GetValueOrDefault();
            int preferredColumns = configuredColumns > 0
                ? configuredColumns
                : _options.Columns.HasValue
                ? Math.Max(1, _options.Columns.Value)
                : _cells.Count;
            int maximumByWidth = Math.Max(1, (width + gutter) / (minimumColumnWidth + gutter));
            return Math.Max(1, Math.Min(Math.Min(preferredColumns, _cells.Count), maximumByWidth));
        }

        private IReadOnlyList<int> ResolveColumnWidths(int width, int columns, int gutter)
        {
            if (!_options.Width.HasValue && !_options.Expand)
            {
                return ResolveCollapsedColumnWidths(width, columns, gutter);
            }

            int availableWidth = Math.Max(1, width - (gutter * Math.Max(0, columns - 1)));
            var widths = new int[columns];
            int flexibleColumns = 0;
            int explicitTotal = 0;

            for (int index = 0; index < columns; index++)
            {
                GridColumn column = GetColumnDefinition(index);
                if (column != null && column.Width.HasValue)
                {
                    widths[index] = Math.Max(1, column.Width.Value);
                    explicitTotal += widths[index];
                }
                else
                {
                    flexibleColumns++;
                }
            }

            int overage = explicitTotal + flexibleColumns - availableWidth;
            while (overage > 0)
            {
                bool reduced = false;
                for (int index = 0; index < columns && overage > 0; index++)
                {
                    if (widths[index] <= 1)
                    {
                        continue;
                    }

                    widths[index]--;
                    overage--;
                    explicitTotal--;
                    reduced = true;
                }

                if (!reduced)
                {
                    break;
                }
            }

            int remainingWidth = Math.Max(flexibleColumns, availableWidth - explicitTotal);
            int remainingFlexibleColumns = flexibleColumns;
            for (int index = 0; index < columns; index++)
            {
                if (widths[index] > 0)
                {
                    continue;
                }

                int widthForColumn = remainingFlexibleColumns <= 1
                    ? remainingWidth
                    : remainingWidth / remainingFlexibleColumns;
                widths[index] = Math.Max(1, widthForColumn);
                remainingWidth -= widths[index];
                remainingFlexibleColumns--;
            }

            return widths;
        }

        private IReadOnlyList<int> ResolveCollapsedColumnWidths(int width, int columns, int gutter)
        {
            int availableWidth = Math.Max(1, width - (gutter * Math.Max(0, columns - 1)));
            int minimumColumnWidth = _options.ResolveMinimumColumnWidth();
            var widths = new int[columns];
            int total = 0;

            for (int index = 0; index < columns; index++)
            {
                GridColumn column = GetColumnDefinition(index);
                int measured = column != null && column.Width.HasValue
                    ? Math.Max(1, column.Width.Value)
                    : MeasureColumnContentWidth(index, columns, column);

                widths[index] = Math.Max(minimumColumnWidth, measured);
                total += widths[index];
            }

            int overage = total - availableWidth;
            while (overage > 0)
            {
                bool reduced = false;
                for (int index = 0; index < columns && overage > 0; index++)
                {
                    if (widths[index] <= 1)
                    {
                        continue;
                    }

                    widths[index]--;
                    overage--;
                    reduced = true;
                }

                if (!reduced)
                {
                    break;
                }
            }

            return widths;
        }

        private List<GridRenderedLine> BuildCellLines(GridCell cell, int columnWidth, GridColumn column)
        {
            var lines = new List<GridRenderedLine>();
            Padding padding = column != null ? column.ResolvePadding() : new Padding(0);
            int leftPadding = Math.Max(0, padding.Left);
            int rightPadding = Math.Max(0, padding.Right);
            if (leftPadding + rightPadding >= columnWidth)
            {
                leftPadding = Math.Min(leftPadding, Math.Max(0, columnWidth - 1));
                rightPadding = Math.Max(0, columnWidth - leftPadding - 1);
            }

            int innerWidth = Math.Max(1, columnWidth - leftPadding - rightPadding);
            TextBlockAlignment alignment = column != null && column.Alignment.HasValue
                ? JustifyMap.ToTextBlockAlignment(column.Alignment.Value)
                : TextBlockAlignment.Left;
            bool noWrap = column != null && column.NoWrap;

            if (!string.IsNullOrEmpty(cell.Header))
            {
                ColorScheme headerScheme = _options.ResolveHeaderScheme();
                foreach (List<FixedMessageSegment> line in BuildTextLines(
                    cell.Header,
                    innerWidth,
                    noWrap,
                    headerScheme,
                    _options.HeaderDecoration,
                    _options.HeaderForegroundRgb,
                    _options.HeaderBackgroundRgb))
                {
                    lines.Add(new GridRenderedLine(FormatCellLine(
                        line,
                        columnWidth,
                        innerWidth,
                        leftPadding,
                        rightPadding,
                        alignment,
                        headerScheme,
                        _options.HeaderDecoration,
                        _options.HeaderForegroundRgb,
                        _options.HeaderBackgroundRgb),
                        headerScheme,
                        _options.HeaderDecoration,
                        _options.HeaderForegroundRgb,
                        _options.HeaderBackgroundRgb));
                }
            }

            ColorScheme contentScheme = cell.Scheme ?? _options.ResolveContentScheme();
            bool hasCellStyle = cell.Scheme.HasValue ||
                cell.Decoration != Decoration.None ||
                cell.ForegroundRgb.HasValue ||
                cell.BackgroundRgb.HasValue;
            Decoration contentDecoration = hasCellStyle ? cell.Decoration : _options.ContentDecoration;
            RgbColor? contentForegroundRgb = hasCellStyle ? cell.ForegroundRgb : _options.ContentForegroundRgb;
            RgbColor? contentBackgroundRgb = hasCellStyle ? cell.BackgroundRgb : _options.ContentBackgroundRgb;
            foreach (List<FixedMessageSegment> line in BuildTextLines(
                cell.Content,
                innerWidth,
                noWrap,
                contentScheme,
                contentDecoration,
                contentForegroundRgb,
                contentBackgroundRgb))
            {
                lines.Add(new GridRenderedLine(
                    FormatCellLine(
                        line,
                        columnWidth,
                        innerWidth,
                        leftPadding,
                        rightPadding,
                        alignment,
                        contentScheme,
                        contentDecoration,
                        contentForegroundRgb,
                        contentBackgroundRgb),
                    contentScheme,
                    contentDecoration,
                    contentForegroundRgb,
                    contentBackgroundRgb));
            }

            if (lines.Count == 0)
            {
                lines.Add(new GridRenderedLine(
                    FormatCellLine(
                        new List<FixedMessageSegment>(),
                        columnWidth,
                        innerWidth,
                        leftPadding,
                        rightPadding,
                        alignment,
                        contentScheme,
                        contentDecoration,
                        contentForegroundRgb,
                        contentBackgroundRgb),
                    contentScheme,
                    contentDecoration,
                    contentForegroundRgb,
                    contentBackgroundRgb));
            }

            return lines;
        }

        private GridColumn GetColumnDefinition(int index)
        {
            return index >= 0 && index < _columns.Count ? _columns[index] : null;
        }

        private int GetColumnDefinitionIndex(int cellIndex)
        {
            int configuredColumnCount = _columns.Count > 0
                ? _columns.Count
                : Math.Max(1, _options.Columns.GetValueOrDefault());
            return configuredColumnCount <= 0 ? 0 : cellIndex % configuredColumnCount;
        }

        private static IEnumerable<List<FixedMessageSegment>> BuildTextLines(
            string text,
            int width,
            bool noWrap,
            ColorScheme scheme,
            Decoration decoration,
            RgbColor? foregroundRgb,
            RgbColor? backgroundRgb)
        {
            if (!noWrap)
            {
                return MarkupRenderText.WrapSegments(text, width, scheme, decoration, foregroundRgb, backgroundRgb);
            }

            return MarkupRenderText.GetLogicalLines(text, scheme, decoration, foregroundRgb, backgroundRgb);
        }

        private static List<FixedMessageSegment> FormatCellLine(
            IList<FixedMessageSegment> text,
            int columnWidth,
            int innerWidth,
            int leftPadding,
            int rightPadding,
            TextBlockAlignment alignment,
            ColorScheme scheme,
            Decoration decoration,
            RgbColor? foregroundRgb,
            RgbColor? backgroundRgb)
        {
            List<FixedMessageSegment> aligned = AlignSegments(
                text,
                innerWidth,
                alignment,
                scheme,
                decoration,
                foregroundRgb,
                backgroundRgb);
            List<FixedMessageSegment> value = PadSegments(
                aligned,
                leftPadding,
                rightPadding,
                scheme,
                decoration,
                foregroundRgb,
                backgroundRgb);
            return PadOrTrimSegments(value, columnWidth, scheme, decoration, foregroundRgb, backgroundRgb);
        }

        private static List<FixedMessageSegment> AlignSegments(
            IList<FixedMessageSegment> segments,
            int width,
            TextBlockAlignment alignment,
            ColorScheme scheme,
            Decoration decoration,
            RgbColor? foregroundRgb,
            RgbColor? backgroundRgb)
        {
            width = Math.Max(0, width);
            var content = MarkupRenderText.TruncateSegments(segments, width);
            int remaining = Math.Max(0, width - MarkupRenderText.GetLength(content));

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
            AddPadding(aligned, leftPadding, scheme, decoration, foregroundRgb, backgroundRgb);
            aligned.AddRange(content);
            AddPadding(aligned, rightPadding, scheme, decoration, foregroundRgb, backgroundRgb);
            return aligned;
        }

        private static List<FixedMessageSegment> PadSegments(
            IList<FixedMessageSegment> segments,
            int leftPadding,
            int rightPadding,
            ColorScheme scheme,
            Decoration decoration,
            RgbColor? foregroundRgb,
            RgbColor? backgroundRgb)
        {
            var result = new List<FixedMessageSegment>();
            AddPadding(result, leftPadding, scheme, decoration, foregroundRgb, backgroundRgb);
            if (segments != null)
            {
                result.AddRange(segments);
            }

            AddPadding(result, rightPadding, scheme, decoration, foregroundRgb, backgroundRgb);
            return result;
        }

        private static List<FixedMessageSegment> PadOrTrimSegments(
            IList<FixedMessageSegment> segments,
            int width,
            ColorScheme scheme,
            Decoration decoration,
            RgbColor? foregroundRgb,
            RgbColor? backgroundRgb)
        {
            List<FixedMessageSegment> result = MarkupRenderText.TruncateSegments(segments, Math.Max(0, width));
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

        private readonly struct GridRenderedLine
        {
            public GridRenderedLine(
                IEnumerable<FixedMessageSegment> segments,
                ColorScheme scheme,
                Decoration decoration = Decoration.None,
                RgbColor? foregroundRgb = null,
                RgbColor? backgroundRgb = null)
            {
                Segments = segments == null
                    ? new List<FixedMessageSegment>()
                    : new List<FixedMessageSegment>(segments);
                Scheme = scheme;
                Decoration = decoration;
                ForegroundRgb = foregroundRgb;
                BackgroundRgb = backgroundRgb;
            }

            public IList<FixedMessageSegment> Segments { get; }

            public ColorScheme Scheme { get; }

            public Decoration Decoration { get; }

            public RgbColor? ForegroundRgb { get; }

            public RgbColor? BackgroundRgb { get; }
        }
    }
}
