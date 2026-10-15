namespace ConsoulLibrary
{
    /// <summary>
    /// Represents the outcome of a typed text prompt interaction.
    /// </summary>
    /// <typeparam name="T">The parsed value type.</typeparam>
    public readonly struct TextPromptResult<T>
    {
        private TextPromptResult(T value, string rawInput, bool isCanceled)
        {
            Value = value;
            RawInput = rawInput ?? string.Empty;
            IsCanceled = isCanceled;
        }

        /// <summary>
        /// Gets the parsed value when <see cref="IsCanceled"/> is <c>false</c>.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Gets the raw input text used to create <see cref="Value"/>.
        /// </summary>
        public string RawInput { get; }

        /// <summary>
        /// Gets whether the user canceled, the cancellation token was signaled, or input ended before a value was accepted.
        /// </summary>
        public bool IsCanceled { get; }

        /// <summary>
        /// Gets a value indicating whether the prompt interaction produced a parsed value.
        /// </summary>
        public bool HasValue => !IsCanceled;

        /// <summary>
        /// Creates a <see cref="TextPromptResult{T}"/> representing a canceled interaction.
        /// </summary>
        public static TextPromptResult<T> Canceled()
        {
            return new TextPromptResult<T>(default(T), string.Empty, true);
        }

        /// <summary>
        /// Creates a <see cref="TextPromptResult{T}"/> representing a successful input.
        /// </summary>
        /// <param name="value">The parsed value.</param>
        /// <param name="rawInput">The raw input text.</param>
        public static TextPromptResult<T> FromValue(T value, string rawInput)
        {
            return new TextPromptResult<T>(value, rawInput, false);
        }

        /// <summary>
        /// Converts a text prompt result to its parsed value for Spectre-style assignment snippets.
        /// </summary>
        /// <param name="result">The text prompt result to convert.</param>
        /// <remarks>Check <see cref="HasValue"/> first; conversion discards cancellation and returns the default value of <typeparamref name="T"/> for a canceled prompt.</remarks>
        public static implicit operator T(TextPromptResult<T> result)
        {
            return result.Value;
        }
    }
}
