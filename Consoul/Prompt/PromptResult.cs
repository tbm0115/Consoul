namespace ConsoulLibrary
{
    /// <summary>
    /// Represents the outcome of a selection prompt interaction.
    /// </summary>
    public readonly struct PromptResult
    {
        private PromptResult(int index, bool isCanceled)
        {
            Index = index;
            IsCanceled = isCanceled;
        }

        /// <summary>
        /// Gets the zero-based index selected by the user when <see cref="IsCanceled"/> is <c>false</c>.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets a value indicating whether the prompt interaction was canceled by the user.
        /// </summary>
        public bool IsCanceled { get; }

        /// <summary>
        /// Gets a value indicating whether the prompt interaction produced a valid selection.
        /// </summary>
        public bool HasSelection => !IsCanceled && Index >= 0;

        /// <summary>
        /// Creates a <see cref="PromptResult"/> representing a canceled interaction.
        /// </summary>
        public static PromptResult Canceled() => new PromptResult(-1, true);

        /// <summary>
        /// Creates a <see cref="PromptResult"/> representing a successful selection.
        /// </summary>
        /// <param name="index">The zero-based index that was selected.</param>
        public static PromptResult FromSelection(int index) => new PromptResult(index, false);
    }
}
