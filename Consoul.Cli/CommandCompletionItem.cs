namespace ConsoulLibrary
{
    /// <summary>
    /// Represents one command-line completion suggestion produced by a <see cref="CommandApp"/>.
    /// </summary>
    public sealed class CommandCompletionItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandCompletionItem"/> class.
        /// </summary>
        /// <param name="value">The token to complete.</param>
        /// <param name="kind">The suggestion kind.</param>
        /// <param name="description">Optional help text for display-capable shells.</param>
        public CommandCompletionItem(string value, CommandCompletionKind kind, string description = "")
        {
            Value = value ?? string.Empty;
            Kind = kind;
            Description = description ?? string.Empty;
        }

        /// <summary>
        /// Gets the token value that should be inserted by a shell completion adapter.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets the suggestion kind.
        /// </summary>
        public CommandCompletionKind Kind { get; }

        /// <summary>
        /// Gets optional help text for display-capable shells.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Returns the completion value.
        /// </summary>
        /// <returns>The completion value.</returns>
        public override string ToString()
        {
            return Value;
        }
    }
}
