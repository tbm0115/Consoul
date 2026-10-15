using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents the outcome of a multi-selection prompt interaction.
    /// </summary>
    public readonly struct MultiSelectionResult : IReadOnlyList<int>
    {
        private MultiSelectionResult(IReadOnlyList<int> indexes, bool isCanceled)
        {
            Indexes = indexes ?? new int[0];
            IsCanceled = isCanceled;
        }

        /// <summary>
        /// Gets the zero-based indexes selected by the user when <see cref="IsCanceled"/> is <c>false</c>.
        /// </summary>
        public IReadOnlyList<int> Indexes { get; }

        /// <summary>
        /// Gets whether the user canceled, the cancellation token was signaled, or input ended before selections were accepted.
        /// </summary>
        public bool IsCanceled { get; }

        /// <summary>
        /// Gets a value indicating whether the prompt interaction produced at least one valid selection.
        /// </summary>
        public bool HasSelection => !IsCanceled && Indexes.Count > 0;

        /// <summary>
        /// Gets the number of selected indexes.
        /// </summary>
        public int Count => Indexes.Count;

        /// <summary>
        /// Gets the selected index at the specified position.
        /// </summary>
        /// <param name="index">The position in the selected index list.</param>
        public int this[int index] => Indexes[index];

        /// <summary>
        /// Creates a <see cref="MultiSelectionResult"/> representing a canceled interaction.
        /// </summary>
        public static MultiSelectionResult Canceled()
        {
            return new MultiSelectionResult(new int[0], true);
        }

        /// <summary>
        /// Creates a <see cref="MultiSelectionResult"/> representing successful selections.
        /// </summary>
        /// <param name="indexes">The zero-based indexes that were selected.</param>
        public static MultiSelectionResult FromSelections(IEnumerable<int> indexes)
        {
            int[] values = indexes == null ? new int[0] : indexes.Distinct().ToArray();
            return new MultiSelectionResult(values, false);
        }

        /// <summary>
        /// Converts a multi-selection result to a list of selected indexes for Spectre-style assignment snippets.
        /// </summary>
        /// <param name="result">The multi-selection result to convert.</param>
        /// <remarks>Check <see cref="IsCanceled"/> first; conversion returns the selected indexes without preserving cancellation.</remarks>
        public static implicit operator List<int>(MultiSelectionResult result)
        {
            return new List<int>(result.Indexes);
        }

        /// <summary>
        /// Returns an enumerator that iterates through selected indexes.
        /// </summary>
        /// <returns>An enumerator for selected indexes.</returns>
        public IEnumerator<int> GetEnumerator()
        {
            return Indexes.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through selected indexes.
        /// </summary>
        /// <returns>An enumerator for selected indexes.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Represents the result of selecting multiple items from a typed set of prompt options.
    /// </summary>
    /// <typeparam name="T">The type of item presented by the prompt.</typeparam>
    public readonly struct MultiSelectionResult<T> : IReadOnlyList<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiSelectionResult{T}"/> struct.
        /// </summary>
        /// <param name="result">The prompt result that describes selected indexes or cancellation state.</param>
        /// <param name="selectedItems">The items selected by the prompt.</param>
        public MultiSelectionResult(MultiSelectionResult result, IReadOnlyList<T> selectedItems)
        {
            Result = result;
            SelectedItems = selectedItems ?? new T[0];
        }

        /// <summary>
        /// Gets the underlying prompt result.
        /// </summary>
        public MultiSelectionResult Result { get; }

        /// <summary>
        /// Gets the selected items.
        /// </summary>
        public IReadOnlyList<T> SelectedItems { get; }

        /// <summary>
        /// Gets the zero-based indexes selected by the user.
        /// </summary>
        public IReadOnlyList<int> Indexes => Result.Indexes;

        /// <summary>
        /// Gets whether the user canceled, the cancellation token was signaled, or input ended before selections were accepted.
        /// </summary>
        public bool IsCanceled => Result.IsCanceled;

        /// <summary>
        /// Gets a value indicating whether the prompt interaction produced at least one valid selection.
        /// </summary>
        public bool HasSelection => Result.HasSelection;

        /// <summary>
        /// Gets the number of selected items.
        /// </summary>
        public int Count => SelectedItems.Count;

        /// <summary>
        /// Gets the selected item at the specified position.
        /// </summary>
        /// <param name="index">The position in the selected item list.</param>
        public T this[int index] => SelectedItems[index];

        /// <summary>
        /// Converts a multi-selection result to a list of selected items for Spectre-style assignment snippets.
        /// </summary>
        /// <param name="result">The multi-selection result to convert.</param>
        /// <remarks>Check <see cref="IsCanceled"/> first; conversion returns <see cref="SelectedItems"/> without preserving cancellation.</remarks>
        public static implicit operator List<T>(MultiSelectionResult<T> result)
        {
            return new List<T>(result.SelectedItems);
        }

        /// <summary>
        /// Returns an enumerator that iterates through selected items.
        /// </summary>
        /// <returns>An enumerator for selected items.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return SelectedItems.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through selected items.
        /// </summary>
        /// <returns>An enumerator for selected items.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
