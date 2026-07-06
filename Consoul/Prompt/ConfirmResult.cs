namespace ConsoulLibrary
{
    /// <summary>
    /// Represents the outcome of a confirmation prompt interaction.
    /// </summary>
    public readonly struct ConfirmResult
    {
        private ConfirmResult(bool value, string rawInput, bool isCanceled)
        {
            Value = value;
            RawInput = rawInput ?? string.Empty;
            IsCanceled = isCanceled;
        }

        /// <summary>
        /// Gets the confirmed value when <see cref="IsCanceled"/> is <c>false</c>.
        /// </summary>
        public bool Value { get; }

        /// <summary>
        /// Gets the raw input text used to create <see cref="Value"/>.
        /// </summary>
        public string RawInput { get; }

        /// <summary>
        /// Gets a value indicating whether the prompt interaction was canceled by the user.
        /// </summary>
        public bool IsCanceled { get; }

        /// <summary>
        /// Gets a value indicating whether the prompt interaction produced a confirmation value.
        /// </summary>
        public bool HasValue => !IsCanceled;

        /// <summary>
        /// Creates a <see cref="ConfirmResult"/> representing a canceled interaction.
        /// </summary>
        public static ConfirmResult Canceled()
        {
            return new ConfirmResult(false, string.Empty, true);
        }

        /// <summary>
        /// Creates a <see cref="ConfirmResult"/> representing a successful confirmation.
        /// </summary>
        /// <param name="value">The confirmed value.</param>
        /// <param name="rawInput">The raw input text.</param>
        public static ConfirmResult FromValue(bool value, string rawInput)
        {
            return new ConfirmResult(value, rawInput, false);
        }

        /// <summary>
        /// Converts a confirmation result to its boolean value for Spectre-style conditional snippets.
        /// </summary>
        /// <param name="result">The confirmation result to convert.</param>
        public static implicit operator bool(ConfirmResult result)
        {
            return result.Value;
        }
    }
}
