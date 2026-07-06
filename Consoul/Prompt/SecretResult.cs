namespace ConsoulLibrary
{
    /// <summary>
    /// Represents the outcome of a secret prompt interaction.
    /// </summary>
    public readonly struct SecretResult
    {
        private SecretResult(string value, bool isCanceled)
        {
            Value = value ?? string.Empty;
            IsCanceled = isCanceled;
        }

        /// <summary>
        /// Gets the secret value when <see cref="IsCanceled"/> is <c>false</c>.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets a value indicating whether the prompt interaction was canceled.
        /// </summary>
        public bool IsCanceled { get; }

        /// <summary>
        /// Gets a value indicating whether the prompt interaction produced a secret value.
        /// </summary>
        public bool HasValue => !IsCanceled;

        /// <summary>
        /// Creates a <see cref="SecretResult"/> representing a canceled interaction.
        /// </summary>
        public static SecretResult Canceled()
        {
            return new SecretResult(string.Empty, true);
        }

        /// <summary>
        /// Creates a <see cref="SecretResult"/> representing a successful secret input.
        /// </summary>
        /// <param name="value">The secret value.</param>
        public static SecretResult FromValue(string value)
        {
            return new SecretResult(value, false);
        }

        /// <summary>
        /// Converts a secret result to its captured value for Spectre-style assignment snippets.
        /// </summary>
        /// <param name="result">The secret result to convert.</param>
        public static implicit operator string(SecretResult result)
        {
            return result.Value;
        }
    }
}
