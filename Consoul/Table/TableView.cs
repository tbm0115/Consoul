using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Delegate for handling when a <see cref="TableView"/> query yields no results.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void TableQueryYieldsNoResults(object sender, TableQueryYieldsNoResultsEventArgs e);

    public class TableRow
    {
        private readonly TableRenderOptions _renderOptions;

        public int CellCount => _contents.Count;

        private readonly List<TableCell> _contents = new List<TableCell>();
        public IEnumerable<TableCell> Contents => _contents;


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
            int initialWidth = Math.Min(_renderOptions?.MaximumTableWidth ?? Console.BufferWidth, Console.BufferWidth);
            var tableCell = new TableCell(content, initialWidth, _renderOptions);
            _contents.Add(tableCell);
            UpdateCellWidths();
        }

        private void UpdateCellWidths()
        {
            if (_contents.Count == 0)
            {
                return;
            }

            int tableWidth = Math.Min(_renderOptions?.MaximumTableWidth ?? Console.BufferWidth, Console.BufferWidth);
            int separatorCount = _contents.Count + 1;
            int availableWidth = Math.Max(0, tableWidth - separatorCount);
            int baseWidth = availableWidth / _contents.Count;
            int remainder = availableWidth % _contents.Count;

            for (int i = 0; i < _contents.Count; i++)
            {
                int width = Math.Max(0, baseWidth + (i < remainder ? 1 : 0));
                _contents[i].CellWidth = width;
            }
        }

        private int GetTableWidth()
        {
            int configuredWidth = Math.Min(_renderOptions?.MaximumTableWidth ?? Console.BufferWidth, Console.BufferWidth);

            if (_contents.Count == 0)
            {
                return configuredWidth;
            }

            int separatorCount = _contents.Count + 1;
            int width = separatorCount + _contents.Sum(cell => cell.CellWidth);
            return Math.Min(width, configuredWidth);
        }

        public int GetRenderWidth() => GetTableWidth();

        public void Render()
        {
            UpdateCellWidths();

            string line = _renderOptions.Lines.VerticalCharacter.ToString();
            int rowTop = Console.CursorTop;
            int rowLeft = Console.CursorLeft;

            Consoul.Write(line, writeLine: false);

            int currentLeft = Console.CursorLeft;
            for (int i = 0; i < _contents.Count; i++)
            {
                var cell = _contents[i];
                int startLeft = Math.Max(0, Math.Min(currentLeft, Console.BufferWidth - 1));
                int availableWidth = Math.Max(0, Console.BufferWidth - startLeft);
                int renderWidth = Math.Min(cell.CellWidth, availableWidth);

                Console.SetCursorPosition(startLeft, rowTop);
                cell.Render(renderWidth);

                currentLeft = startLeft + renderWidth;
                if (currentLeft >= Console.BufferWidth)
                {
                    currentLeft = Console.BufferWidth - 1;
                }

                Console.SetCursorPosition(Math.Max(0, Math.Min(currentLeft, Console.BufferWidth - 1)), rowTop);
                Consoul.Write(line, writeLine: false);
                currentLeft = Console.CursorLeft;
            }

            if (_contents.Count == 0)
            {
                int closingLeft = Math.Min(rowLeft + GetTableWidth() - 1, Console.BufferWidth - 1);
                Console.SetCursorPosition(Math.Max(0, closingLeft), rowTop);
                Consoul.Write(line, writeLine: false);
            }

            Consoul.Write(string.Empty);
        }

        public void Update(IEnumerable<string> contents)
        {
            string[] values = contents.ToArray();
            int contentCount = values.Length;
            UpdateCellWidths();
            for (int i = 0; i < contentCount && i < _contents.Count; i++)
            {
                _contents[i].Contents = values[i];
            }
        }
    }

    public class TableCell
    {
        private readonly TableRenderOptions _renderOptions;

        private int _cellWidth;
        public int CellWidth
        {
            get => _cellWidth;
            set
            {
                _cellWidth = value;
                _message.MaxWidth = Math.Max(0, value);
                Render();
            }
        }

        private string _contents;
        public string Contents
        {
            get => _contents;
            set
            {
                _contents = value;
                _message.MaxWidth = _cellWidth;
                Render();
            }
        }

        private FixedMessage _message;

        public TableCell(string content, int maxWidth, TableRenderOptions options = default)
        {
            _renderOptions = options;
            _contents = content;

            _cellWidth = Math.Max(0, maxWidth);
            _message = new FixedMessage(maxWidth);
        }

        public void Render(int? overrideWidth = null)
        {
            if (overrideWidth.HasValue)
            {
                int width = Math.Max(0, overrideWidth.Value);
                _message.MaxWidth = width;
                _cellWidth = width;
            }

            int maxWidth = _message.MaxWidth ?? _cellWidth;
            if (maxWidth <= 0)
            {
                _message.Render(string.Empty);
                return;
            }

            int innerWidth = Math.Max(0, maxWidth - 1);
            string text = _contents ?? string.Empty;

            if (innerWidth > 0 && text.Length > innerWidth)
            {
                int truncateLength = Math.Max(0, innerWidth - 1);
                text = truncateLength > 0 ? text.Substring(0, truncateLength) + "…" : text.Substring(0, innerWidth);
            }
            else if (innerWidth == 0 && text.Length > maxWidth)
            {
                text = text.Substring(0, maxWidth);
            }

            string contents = innerWidth > 0
                ? " " + text.PadRight(innerWidth)
                : text;

            _message.Render(contents);
        }
    }

    public class TableView
    {
        public TableRenderOptions TableRenderOptions { get; set; }

        private TableNavigator _tableNavigator;
        private TableInputHandler _tableInputHandler;

        // FixedMessage instances for each row and header
        private TableRow _header { get; set; }
        private List<TableRow> _rows = new List<TableRow>();

        public event TableQueryYieldsNoResults QueryYieldsNoResults;

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

        public void Render(string message = default, ConsoleColor? color = null, bool clearConsole = true)
        {
            //TableRenderOptions.Normalize(Contents);

            if (clearConsole)
                Console.Clear();

            char line = TableRenderOptions.Lines.HorizontalCharacter;
            int tableWidth = GetTableWidth();
            string horizontalLine = new string(line, tableWidth);
            // Render Headers with FixedMessage
            if (_header != null)
            {
                Consoul.Write(horizontalLine);
                _header.Render();
            }

            for (int i = 0; i < _rows.Count; i++)
            {
                Consoul.Write(horizontalLine);
                _rows[i].Render();
            }
            Consoul.Write(horizontalLine);

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

                var keyInfo = Console.ReadKey(intercept: true);

                if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    _tableNavigator.MoveUp();
                    UpdateRenderedRow(_tableNavigator.PreviousRow);
                    UpdateRenderedRow(_tableNavigator.CurrentRow);
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    _tableNavigator.MoveDown();
                    UpdateRenderedRow(_tableNavigator.PreviousRow);
                    UpdateRenderedRow(_tableNavigator.CurrentRow);
                }
                else if (keyInfo.Key == ConsoleKey.Spacebar)
                {
                    _tableNavigator.ToggleSelection();
                    UpdateRenderedRow(_tableNavigator.CurrentRow);
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    if (!string.IsNullOrEmpty(inputBuffer))
                    {
                        if (int.TryParse(inputBuffer, out int rowNumber) && rowNumber >= 1 && rowNumber <= _rows.Count)
                        {
                            _tableNavigator.SetCurrentRow(rowNumber - 1);
                            UpdateRenderedRow(_tableNavigator.CurrentRow);
                            return _tableNavigator.CurrentRow;
                        }
                        inputBuffer = "";
                    }
                    else
                    {
                        return _tableNavigator.CurrentRow == 0 && allowEmpty ? (int?)null : _tableNavigator.CurrentRow;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    return null;
                }
                else if (char.IsDigit(keyInfo.KeyChar))
                {
                    inputBuffer += keyInfo.KeyChar;
                }
                else if (keyInfo.Key == ConsoleKey.Backspace && inputBuffer.Length > 0)
                {
                    inputBuffer = inputBuffer.Substring(0, inputBuffer.Length - 1);
                }

            } while (true);
        }


        public void AddHeader(string label)
        {
            _header.Add(label);
            //TableRenderOptions.Normalize(Contents);
        }

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

        public void AddRows(IEnumerable<IEnumerable<string>> rows, bool renderAfterAdding = false)
        {
            foreach (var row in rows)
            {
                AddRow(row, renderAfterAdding);
            }
        }

        private int GetTableWidth()
        {
            int width = _header?.GetRenderWidth() ?? 0;

            foreach (var row in _rows)
            {
                width = Math.Max(width, row.GetRenderWidth());
            }

            int configuredWidth = Math.Min(TableRenderOptions.MaximumTableWidth ?? Console.BufferWidth, Console.BufferWidth);
            if (width == 0)
            {
                return configuredWidth;
            }

            return Math.Min(width, configuredWidth);
        }

        private void UpdateRenderedRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= _rows.Count) return;

            bool isSelected = _tableNavigator.SelectedRows.Contains(rowIndex);
            bool isHighlighted = _tableNavigator.CurrentRow == rowIndex;
            var rowText = string.Join(" | ", _rows[rowIndex]);
            var rowColor = isSelected
                ? TableRenderOptions.SelectionScheme
                : isHighlighted
                    ? TableRenderOptions.HighlightedScheme
                    : (rowIndex % 2 == 0
                        ? TableRenderOptions.ContentScheme1
                        : TableRenderOptions.ContentScheme2);
            var backgroundColor = isHighlighted ? ConsoleColor.DarkGray : ConsoleColor.Black;

            _rows[rowIndex].Render();
            //_rowMessages[rowIndex].Render(rowText, rowColor.Color, rowColor.BackgroundColor);
        }

        public static TableView Create(IEnumerable<IEnumerable<string>> items, params string[] columnNames)
        {
            var table = new TableView();
            table.AddHeaders(columnNames);
            table.AddRows(items);
            return table;
        }
    }
}
