using System;
using System.Collections.Generic;
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
            string line = _renderOptions.Lines.VerticalCharacter.ToString();
            bool showVertical = isHeader
                ? _renderOptions?.Lines?.HeaderVertical ?? true
                : _renderOptions?.Lines?.ContentVertical ?? true;
            int rowTop = Consoul.ConsoleDriver.CursorTop;
            int rowLeft = Consoul.ConsoleDriver.CursorLeft;

            if (showVertical)
            {
                Consoul.Write(line, lineScheme?.Color, lineScheme?.BackgroundColor, writeLine: false);
            }

            int currentLeft = Consoul.ConsoleDriver.CursorLeft;
            for (int i = 0; i < _contents.Count; i++)
            {
                var cell = _contents[i];
                int startLeft = Math.Max(0, Math.Min(currentLeft, Consoul.ConsoleBufferWidth - 1));
                int availableWidth = Math.Max(0, Consoul.ConsoleBufferWidth - startLeft);
                int renderWidth = Math.Min(Math.Max(0, cell.CellWidth), availableWidth);

                Consoul.ConsoleDriver.SetCursorPosition(startLeft, rowTop);
                cell.Render(renderWidth, contentScheme);

                currentLeft = startLeft + renderWidth;
                if (currentLeft >= Consoul.ConsoleBufferWidth)
                {
                    currentLeft = Consoul.ConsoleBufferWidth - 1;
                }

                if (showVertical)
                {
                    Consoul.ConsoleDriver.SetCursorPosition(Math.Max(0, Math.Min(currentLeft, Consoul.ConsoleBufferWidth - 1)), rowTop);
                    Consoul.Write(line, lineScheme?.Color, lineScheme?.BackgroundColor, writeLine: false);
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
                Consoul.Write(line, lineScheme?.Color, lineScheme?.BackgroundColor, writeLine: false);
            }

            Consoul.Write(string.Empty);
        }

        /// <summary>
        /// Updates existing cells with new content values.
        /// </summary>
        /// <param name="contents">New cell text values.</param>
        public void Update(IEnumerable<string> contents)
        {
            string[] values = contents.ToArray();
            int contentCount = values.Length;
            for (int i = 0; i < contentCount && i < _contents.Count; i++)
            {
                _contents[i].Contents = values[i];
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

            return (_contents[index].Contents ?? string.Empty).Length;
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
        public void Render(int? overrideWidth = null, ColorScheme? contentScheme = null)
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

            int innerWidth = Math.Max(0, width - 1);
            string text = _contents ?? string.Empty;

            if (innerWidth > 0 && text.Length > innerWidth)
            {
                int truncateLength = Math.Max(0, innerWidth - 1);
                text = truncateLength > 0 ? text.Substring(0, truncateLength) + "…" : text.Substring(0, innerWidth);
            }
            else if (innerWidth == 0 && text.Length > width)
            {
                text = text.Substring(0, width);
            }

            string contents = innerWidth > 0
                ? " " + text.PadRight(innerWidth)
                : text;

            if (_hasRendered)
            {
                _message.Reset();
            }

            _message.Render(contents, contentScheme?.Color, contentScheme?.BackgroundColor);
            _hasRendered = true;
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
    public class TableView : IDisposable
    {
        /// <summary>
        /// Gets or sets the options used to render table layout, borders, and colors.
        /// </summary>
        public TableRenderOptions TableRenderOptions { get; set; }

        private TableNavigator _tableNavigator;
        private TableInputHandler _tableInputHandler;

        // FixedMessage instances for each row and header
        private TableRow _header { get; set; }
        private List<TableRow> _rows = new List<TableRow>();
        private List<int> _columnWidths = new List<int>();

        /// <summary>
        /// Occurs when a table query yields no matching rows.
        /// </summary>
        public event TableQueryYieldsNoResults QueryYieldsNoResults;

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
                Consoul.ConsoleDriver.Clear();
            else
                Consoul.ConsoleDriver.SetCursorPosition(0, 0);

            if (!string.IsNullOrEmpty(message))
            {
                Consoul.Write(message, color, writeLine: true);
            }

            RecalculateColumnWidths();
            char line = TableRenderOptions.Lines.HorizontalCharacter;
            int tableWidth = GetTableWidth();
            string horizontalLine = new string(line, tableWidth);
            ColorScheme? lineScheme = TableRenderOptions.Lines.Color;

            bool hasHeader = _header != null && _header.CellCount > 0;
            bool hasRows = _rows.Count > 0;

            if (hasHeader || hasRows)
            {
                Consoul.Write(horizontalLine, lineScheme?.Color, lineScheme?.BackgroundColor);
            }

            if (hasHeader)
            {
                _header.Render(TableRenderOptions.HeaderScheme, lineScheme.GetValueOrDefault(), isHeader: true);
                if (TableRenderOptions.Lines.HeaderHorizontal)
                {
                    Consoul.Write(horizontalLine, lineScheme?.Color, lineScheme?.BackgroundColor);
                }
            }

            for (int i = 0; i < _rows.Count; i++)
            {
                if (TableRenderOptions.Lines.ContentHorizontal)
                {
                    Consoul.Write(horizontalLine, lineScheme?.Color, lineScheme?.BackgroundColor);
                }

                var contentScheme = GetRowContentScheme(i);
                _rows[i].Render(contentScheme, lineScheme);
            }

            if (hasRows && TableRenderOptions.Lines.ContentHorizontal)
            {
                Consoul.Write(horizontalLine, lineScheme?.Color, lineScheme?.BackgroundColor);
            }

                //_headerMessage = new FixedMessage(Console.BufferWidth);
                //_headerMessage.Render(string.Join(TableRenderOptions.Lines.VerticalCharacter.ToString(), _headers), ConsoleColor.White);

                //// Render Rows using FixedMessage instances
                //for (int i = 0; i < _contents.Count; i++)
                //{
                //    // Create FixedMessage instances if they do not exist
                //    if (_rowMessages.Count <= i)
                //        _rowMessages.Add(new FixedMessage(Console.BufferWidth));

                //    bool isSelected = _tableNavigator != null && _tableNavigator.SelectedRows.Contains(i);
                //    bool isHighlighted = _tableNavigator != null && _tableNavigator.CurrentRow == i;

                //    // Render row with appropriate styling
                //    var rowText = string.Join(" | ", _contents[i]);
                //    var rowColor = isSelected
                //        ? TableRenderOptions.SelectionScheme
                //        : isHighlighted
                //            ? TableRenderOptions.HighlightedScheme
                //            : (i % 2 == 0
                //                ? TableRenderOptions.ContentScheme1
                //                : TableRenderOptions.ContentScheme2);

                //    _rowMessages[i].Render(rowText, rowColor.Color, rowColor.BackgroundColor);
                //}

            // Optionally render footer prompts
            Consoul.Write("Use Arrow Keys to navigate, Enter to confirm, Space to select/deselect, Escape to exit, or enter row number to jump.", ConsoleColor.Gray);
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
            if (_tableNavigator == null)
                InitializeNavigator();

            string inputBuffer = "";

            do
            {
                Render(message, color, inputBuffer == "" && clearConsole);

                if (!string.IsNullOrEmpty(inputBuffer))
                {
                    Consoul.Write($"\nCurrent input: {inputBuffer}", ConsoleColor.Cyan);
                }

                var keyInfo = Consoul.ConsoleDriver.ReadKey(intercept: true);

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
                            return _tableNavigator.CurrentRow;
                        }
                        inputBuffer = "";
                        continue;
                    }
                    else
                    {
                        if (_tableNavigator.CurrentRow >= 0)
                        {
                            return _tableNavigator.CurrentRow;
                        }

                        if (_tableNavigator.HoveredRow >= 0)
                        {
                            return _tableNavigator.HoveredRow;
                        }

                        if (allowEmpty)
                        {
                            return null;
                        }

                        continue;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    return null;
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
        /// Adds a header cell to the table.
        /// </summary>
        /// <param name="label">Header text to add.</param>
        public void AddHeader(string label)
        {
            _header.Add(label);
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

            _rows.Add(new TableRow(cellValues, TableRenderOptions));
            if (_tableNavigator != null)
            {
                // Update the navigator to include the new row
                _tableNavigator = new TableNavigator(_rows.Count);
            }

            if (renderAfterAdding)
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
            int columnCount = Math.Max(headerColumns, rowColumns);

            _columnWidths.Clear();

            if (columnCount == 0)
            {
                return;
            }

            int configuredWidth = TableRenderOptions.GetMaximumTableWidth();
            int separators = columnCount + 1;
            int availableWidth = Math.Max(0, configuredWidth - separators);

            var desired = new int[columnCount];

            if (_header != null)
            {
                for (int i = 0; i < columnCount; i++)
                {
                    desired[i] = Math.Max(desired[i], _header.GetCellContentLength(i) + 1);
                }
            }

            foreach (var row in _rows)
            {
                for (int i = 0; i < columnCount; i++)
                {
                    desired[i] = Math.Max(desired[i], row.GetCellContentLength(i) + 1);
                }
            }

            for (int i = 0; i < desired.Length; i++)
            {
                desired[i] = Math.Max(desired[i], 1);
            }

            _columnWidths = AllocateColumnWidths(desired, availableWidth);

            _header?.ApplyColumnWidths(_columnWidths);
            foreach (var row in _rows)
            {
                row.ApplyColumnWidths(_columnWidths);
            }
        }

        private static List<int> AllocateColumnWidths(IReadOnlyList<int> desired, int availableWidth)
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

        private ColorScheme GetRowContentScheme(int rowIndex)
        {
            if (_tableNavigator != null)
            {
                if (_tableNavigator.SelectedRows.Contains(rowIndex))
                {
                    return TableRenderOptions.SelectionScheme;
                }

                if (_tableNavigator.HoveredRow == rowIndex)
                {
                    return TableRenderOptions.HighlightedScheme;
                }
            }

            return rowIndex % 2 == 0
                ? TableRenderOptions.ContentScheme1
                : TableRenderOptions.ContentScheme2;
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
