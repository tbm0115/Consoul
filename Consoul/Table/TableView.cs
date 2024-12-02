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

        public int CellWidth => ((_renderOptions.MaximumTableWidth ?? Console.BufferWidth) / (CellCount > 0 ? CellCount : 1)) - 2;

        private List<TableCell> _contents = new List<TableCell>();
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
            var tableCell = new TableCell(content, CellWidth, _renderOptions);
            _contents.Add(tableCell);
            UpdateCellWidths();
        }

        private void UpdateCellWidths()
        {
            foreach (var cell in _contents)
            {
                cell.CellWidth = CellWidth;
            }
        }

        public void Render()
        {
            UpdateCellWidths();

            string line = _renderOptions.Lines.VerticalCharacter.ToString();
            using (var position = Consoul.SaveCursor())
            {
                Consoul.Write(line, writeLine: false);
                foreach (var cell in Contents)
                {
                    cell.Render();
                    Consoul.Write(line, writeLine: false);
                }
                Consoul.Write(string.Empty);
            }
        }

        public void Update(IEnumerable<string> contents)
        {
            string[] values = contents.ToArray();
            int contentCount = values.Length;
            for (int i = 0; i < contentCount; i++)
            {
                _contents[i].Contents = values[i];
                _contents[i].CellWidth = CellWidth;
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

            _message = new FixedMessage(maxWidth);
        }

        public void Render()
        {
            int textWidth = _message.MaxWidth.Value - 2;
            string contents = string.Empty;
            string text = _contents;
            if (text.Length > textWidth)
            {
                text = text.Substring(0, textWidth - 1);
            }
            contents += text;
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

            using (var pos = Consoul.SaveCursor())
            {
                char line = TableRenderOptions.Lines.HorizontalCharacter;
                string horizontalLine = new string(line, TableRenderOptions.MaximumTableWidth.Value);
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
