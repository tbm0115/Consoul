using System;
using System.Collections.Generic;

namespace ConsoulLibrary
{
    /// <summary>
    /// Handles navigation through the rows of a table.
    /// </summary>
    public class TableNavigator
    {
        private int _currentRow = -1;
        private readonly int _totalRows;
        private readonly HashSet<int> _selectedRows;

        public TableNavigator(int totalRows)
        {
            _totalRows = totalRows;
            _selectedRows = new HashSet<int>();
        }

        public int CurrentRow => _currentRow;
        public int PreviousRow { get; private set; } = -1;
        public IReadOnlyCollection<int> SelectedRows => _selectedRows;

        public void MoveUp()
        {
            PreviousRow = _currentRow;

            if (_totalRows <= 0)
            {
                _currentRow = -1;
                return;
            }

            if (_currentRow <= 0)
            {
                _currentRow = 0;
                return;
            }

            _currentRow--;
        }

        public void MoveDown()
        {
            PreviousRow = _currentRow;

            if (_totalRows <= 0)
            {
                _currentRow = -1;
                return;
            }

            if (_currentRow < 0)
            {
                _currentRow = 0;
                return;
            }

            _currentRow = Math.Min(_totalRows - 1, _currentRow + 1);
        }

        public void ToggleSelection()
        {
            if (_currentRow < 0)
            {
                return;
            }

            if (_selectedRows.Contains(_currentRow))
                _selectedRows.Remove(_currentRow);
            else
                _selectedRows.Add(_currentRow);
        }

        public void SetCurrentRow(int rowNumber)
        {
            if (rowNumber < -1 || rowNumber > _totalRows - 1)
            {
                throw new ArgumentOutOfRangeException(nameof(rowNumber));
            }

            PreviousRow = _currentRow;
            _currentRow = rowNumber;
        }
    }
}
