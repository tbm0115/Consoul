using System;
using System.Collections.Generic;

namespace ConsoulLibrary
{
    /// <summary>
    /// Handles navigation through the rows of a table.
    /// </summary>
    public class TableNavigator
    {
        private int _hoveredRow = -1;
        private int _currentRow = -1;
        private readonly int _totalRows;
        private readonly HashSet<int> _selectedRows;

        /// <summary>
        /// Initializes a new table navigator for a fixed number of rows.
        /// </summary>
        /// <param name="totalRows">The total number of navigable rows.</param>
        public TableNavigator(int totalRows)
        {
            _totalRows = totalRows;
            _selectedRows = new HashSet<int>();
        }

        /// <summary>
        /// Gets the zero-based row currently under the navigation cursor.
        /// </summary>
        public int HoveredRow => _hoveredRow;

        /// <summary>
        /// Gets the zero-based row that has been confirmed or selected.
        /// </summary>
        public int CurrentRow => _currentRow;

        /// <summary>
        /// Gets the row that was hovered before the latest navigation change.
        /// </summary>
        public int PreviousRow { get; private set; } = -1;

        /// <summary>
        /// Gets the selected row indexes.
        /// </summary>
        public IReadOnlyCollection<int> SelectedRows => _selectedRows;

        /// <summary>
        /// Moves the hovered row upward within the table bounds.
        /// </summary>
        public void MoveUp()
        {
            PreviousRow = _hoveredRow;

            if (_totalRows <= 0)
            {
                _hoveredRow = -1;
                return;
            }

            if (_hoveredRow <= 0)
            {
                _hoveredRow = 0;
                return;
            }

            _hoveredRow--;
        }

        /// <summary>
        /// Moves the hovered row downward within the table bounds.
        /// </summary>
        public void MoveDown()
        {
            PreviousRow = _hoveredRow;

            if (_totalRows <= 0)
            {
                _hoveredRow = -1;
                return;
            }

            if (_hoveredRow < 0)
            {
                _hoveredRow = 0;
                return;
            }

            _hoveredRow = Math.Min(_totalRows - 1, _hoveredRow + 1);
        }

        /// <summary>
        /// Toggles selection for the currently hovered row.
        /// </summary>
        public void ToggleSelection()
        {
            if (_hoveredRow < 0)
            {
                return;
            }

            if (_selectedRows.Contains(_hoveredRow))
            {
                _selectedRows.Remove(_hoveredRow);

                if (_currentRow == _hoveredRow)
                {
                    _currentRow = _selectedRows.Count > 0 ? GetFirstSelectedRow() : -1;
                }
            }
            else
            {
                _selectedRows.Add(_hoveredRow);
                _currentRow = _hoveredRow;
            }
        }

        /// <summary>
        /// Sets the current and hovered row explicitly.
        /// </summary>
        /// <param name="rowNumber">Zero-based row number to select, or -1 to clear selection.</param>
        public void SetCurrentRow(int rowNumber)
        {
            if (rowNumber < -1 || rowNumber > _totalRows - 1)
            {
                throw new ArgumentOutOfRangeException(nameof(rowNumber));
            }

            PreviousRow = _hoveredRow;
            _hoveredRow = rowNumber;
            _currentRow = rowNumber;
            _selectedRows.Clear();

            if (rowNumber >= 0)
            {
                _selectedRows.Add(rowNumber);
            }
        }

        private int GetFirstSelectedRow()
        {
            int smallest = int.MaxValue;

            foreach (var row in _selectedRows)
            {
                if (row < smallest)
                {
                    smallest = row;
                }
            }

            return smallest == int.MaxValue ? -1 : smallest;
        }
    }
}
