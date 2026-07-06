using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents the outcome of an interactive table row selection.
    /// </summary>
    public readonly struct TableSelectionResult
    {
        private TableSelectionResult(IReadOnlyList<int> indexes, bool isCanceled)
        {
            Indexes = indexes ?? new int[0];
            IsCanceled = isCanceled;
        }

        /// <summary>
        /// Gets the zero-based row indexes selected by the user when <see cref="IsCanceled"/> is <c>false</c>.
        /// </summary>
        public IReadOnlyList<int> Indexes { get; }

        /// <summary>
        /// Gets the first selected row index, or <c>-1</c> when no row was selected.
        /// </summary>
        public int Index => HasSelection ? Indexes[0] : -1;

        /// <summary>
        /// Gets a value indicating whether the table interaction was canceled by the user or a cancellation token.
        /// </summary>
        public bool IsCanceled { get; }

        /// <summary>
        /// Gets a value indicating whether the table interaction produced at least one selected row.
        /// </summary>
        public bool HasSelection => !IsCanceled && Indexes.Count > 0;

        /// <summary>
        /// Gets a value indicating whether the user accepted an empty selection.
        /// </summary>
        public bool IsEmpty => !IsCanceled && Indexes.Count == 0;

        /// <summary>
        /// Creates a <see cref="TableSelectionResult"/> representing a canceled interaction.
        /// </summary>
        /// <returns>A canceled selection result.</returns>
        public static TableSelectionResult Canceled()
        {
            return new TableSelectionResult(new int[0], true);
        }

        /// <summary>
        /// Creates a <see cref="TableSelectionResult"/> representing an accepted empty selection.
        /// </summary>
        /// <returns>An empty selection result.</returns>
        public static TableSelectionResult Empty()
        {
            return new TableSelectionResult(new int[0], false);
        }

        /// <summary>
        /// Creates a <see cref="TableSelectionResult"/> representing a single selected row.
        /// </summary>
        /// <param name="index">The zero-based selected row index.</param>
        /// <returns>A successful selection result.</returns>
        public static TableSelectionResult FromSelection(int index)
        {
            return FromSelections(new[] { index });
        }

        /// <summary>
        /// Creates a <see cref="TableSelectionResult"/> representing one or more selected rows.
        /// </summary>
        /// <param name="indexes">The zero-based selected row indexes.</param>
        /// <returns>A successful selection result.</returns>
        public static TableSelectionResult FromSelections(IEnumerable<int> indexes)
        {
            int[] values = indexes == null
                ? new int[0]
                : indexes.Where(index => index >= 0).Distinct().OrderBy(index => index).ToArray();
            return new TableSelectionResult(values, false);
        }
    }

    /// <summary>
    /// Represents the outcome of an interactive model-backed table row selection.
    /// </summary>
    /// <typeparam name="T">The type of model item represented by table rows.</typeparam>
    public readonly struct TableSelectionResult<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableSelectionResult{T}"/> struct.
        /// </summary>
        /// <param name="result">The underlying row-index selection result.</param>
        /// <param name="selectedItems">The model items selected by the table.</param>
        public TableSelectionResult(TableSelectionResult result, IReadOnlyList<T> selectedItems)
        {
            Result = result;
            SelectedItems = selectedItems ?? new T[0];
        }

        /// <summary>
        /// Gets the underlying row-index selection result.
        /// </summary>
        public TableSelectionResult Result { get; }

        /// <summary>
        /// Gets the model items selected by the table.
        /// </summary>
        public IReadOnlyList<T> SelectedItems { get; }

        /// <summary>
        /// Gets the first selected model item, or the default value of <typeparamref name="T"/> when no item was selected.
        /// </summary>
        public T SelectedItem => SelectedItems.Count > 0 ? SelectedItems[0] : default(T);

        /// <summary>
        /// Gets the zero-based visible row indexes selected by the user.
        /// </summary>
        public IReadOnlyList<int> Indexes => Result.Indexes;

        /// <summary>
        /// Gets the first selected visible row index, or <c>-1</c> when no row was selected.
        /// </summary>
        public int Index => Result.Index;

        /// <summary>
        /// Gets a value indicating whether the table interaction was canceled by the user or a cancellation token.
        /// </summary>
        public bool IsCanceled => Result.IsCanceled;

        /// <summary>
        /// Gets a value indicating whether the table interaction produced at least one selected row.
        /// </summary>
        public bool HasSelection => Result.HasSelection;

        /// <summary>
        /// Gets a value indicating whether the user accepted an empty selection.
        /// </summary>
        public bool IsEmpty => Result.IsEmpty;
    }
}
