using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides mutable access to a manual <see cref="Table"/> row set for live dashboard snippets.
    /// </summary>
    public sealed class TableRowCollection : IEnumerable<TableRow>
    {
        private readonly Table _table;

        internal TableRowCollection(Table table)
        {
            _table = table ?? throw new ArgumentNullException(nameof(table));
        }

        /// <summary>
        /// Gets the number of rows in the table.
        /// </summary>
        public int Count => _table.RowCount;

        /// <summary>
        /// Gets a row by index.
        /// </summary>
        /// <param name="index">Zero-based row index.</param>
        /// <returns>The table row at the specified index.</returns>
        public TableRow this[int index] => _table.GetRow(index);

        /// <summary>
        /// Removes every row from the table.
        /// </summary>
        public void Clear()
        {
            _table.ClearRows();
        }

        /// <summary>
        /// Adds a row to the table.
        /// </summary>
        /// <param name="cells">The row cell values.</param>
        /// <returns>The current row collection.</returns>
        public TableRowCollection Add(params string[] cells)
        {
            _table.AddRow(cells ?? new string[0]);
            return this;
        }

        /// <summary>
        /// Adds a row to the table.
        /// </summary>
        /// <param name="cells">The row cell values.</param>
        /// <returns>The current row collection.</returns>
        public TableRowCollection Add(params object[] cells)
        {
            _table.AddRow(cells ?? new object[0]);
            return this;
        }

        /// <summary>
        /// Inserts a row into the table.
        /// </summary>
        /// <param name="rowIndex">Zero-based index where the row should be inserted. Use <see cref="Count"/> to append.</param>
        /// <param name="cells">The row cell values.</param>
        /// <returns>The current row collection.</returns>
        public TableRowCollection Insert(int rowIndex, params string[] cells)
        {
            _table.InsertRow(rowIndex, cells ?? new string[0]);
            return this;
        }

        /// <summary>
        /// Inserts a row into the table.
        /// </summary>
        /// <param name="rowIndex">Zero-based index where the row should be inserted. Use <see cref="Count"/> to append.</param>
        /// <param name="cells">The row cell values.</param>
        /// <returns>The current row collection.</returns>
        public TableRowCollection Insert(int rowIndex, params object[] cells)
        {
            _table.InsertRow(rowIndex, FormatCells(cells));
            return this;
        }

        /// <summary>
        /// Removes one row from the table.
        /// </summary>
        /// <param name="rowIndex">Zero-based row index.</param>
        /// <returns>The current row collection.</returns>
        public TableRowCollection RemoveAt(int rowIndex)
        {
            _table.RemoveRow(rowIndex);
            return this;
        }

        /// <summary>
        /// Updates an entire row.
        /// </summary>
        /// <param name="rowIndex">Zero-based row index.</param>
        /// <param name="cells">Replacement cell values.</param>
        /// <returns>The current row collection.</returns>
        public TableRowCollection Update(int rowIndex, params string[] cells)
        {
            _table.UpdateRow(rowIndex, cells ?? new string[0]);
            return this;
        }

        /// <summary>
        /// Updates an entire row.
        /// </summary>
        /// <param name="rowIndex">Zero-based row index.</param>
        /// <param name="cells">Replacement cell values.</param>
        /// <returns>The current row collection.</returns>
        public TableRowCollection Update(int rowIndex, IEnumerable<string> cells)
        {
            _table.UpdateRow(rowIndex, cells ?? Enumerable.Empty<string>());
            return this;
        }

        /// <summary>
        /// Updates a single cell.
        /// </summary>
        /// <param name="rowIndex">Zero-based row index.</param>
        /// <param name="columnIndex">Zero-based column index.</param>
        /// <param name="content">Replacement cell text.</param>
        /// <returns>The current row collection.</returns>
        public TableRowCollection Update(int rowIndex, int columnIndex, string content)
        {
            _table.UpdateCell(rowIndex, columnIndex, content ?? string.Empty);
            return this;
        }

        /// <summary>
        /// Updates a single cell using the current culture to format the value.
        /// </summary>
        /// <param name="rowIndex">Zero-based row index.</param>
        /// <param name="columnIndex">Zero-based column index.</param>
        /// <param name="content">Replacement cell value.</param>
        /// <returns>The current row collection.</returns>
        public TableRowCollection Update(int rowIndex, int columnIndex, object content)
        {
            return Update(rowIndex, columnIndex, Table.FormatCellValue(content));
        }

        /// <summary>
        /// Updates a single cell by capturing a renderable component as text.
        /// </summary>
        /// <param name="rowIndex">Zero-based row index.</param>
        /// <param name="columnIndex">Zero-based column index.</param>
        /// <param name="renderable">Renderable component used as the new cell content.</param>
        /// <returns>The current row collection.</returns>
        public TableRowCollection Update(int rowIndex, int columnIndex, IConsoulRenderable renderable)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable));
            }

            return Update(rowIndex, columnIndex, Table.FormatCellValue(renderable));
        }

        /// <summary>
        /// Returns an enumerator over the table rows.
        /// </summary>
        /// <returns>An enumerator over table rows.</returns>
        public IEnumerator<TableRow> GetEnumerator()
        {
            return _table.ContentRows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static IEnumerable<string> FormatCells(object[] cells)
        {
            if (cells == null)
            {
                return Enumerable.Empty<string>();
            }

            return cells.Select(Table.FormatCellValue);
        }
    }
}
