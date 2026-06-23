namespace ConsoulLibrary
{
    /// <summary>
    /// Represents the result of selecting an item from a typed set of prompt options.
    /// </summary>
    /// <typeparam name="T">The type of item presented by the prompt.</typeparam>
    public readonly struct SelectionResult<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectionResult{T}"/> struct.
        /// </summary>
        /// <param name="result">The prompt result that describes the selected index or cancellation state.</param>
        /// <param name="selectedItem">The item selected by the prompt, or the default value when no item was selected.</param>
        public SelectionResult(PromptResult result, T selectedItem)
        {
            Result = result;
            SelectedItem = selectedItem;
        }

        /// <summary>
        /// Gets the underlying prompt result.
        /// </summary>
        public PromptResult Result { get; }

        /// <summary>
        /// Gets the selected item, or the default value of <typeparamref name="T"/> when no item was selected.
        /// </summary>
        public T SelectedItem { get; }

        /// <summary>
        /// Gets the zero-based index selected by the user.
        /// </summary>
        public int Index => Result.Index;

        /// <summary>
        /// Gets a value indicating whether the prompt interaction was canceled by the user.
        /// </summary>
        public bool IsCanceled => Result.IsCanceled;

        /// <summary>
        /// Gets a value indicating whether the prompt interaction produced a valid selection.
        /// </summary>
        public bool HasSelection => Result.HasSelection;
    }
}
