namespace ConsoulLibrary
{
    /// <summary>
    /// Represents the result of validating a prompt value.
    /// </summary>
    /// <remarks>
    /// Use this result-shaped validator when porting Spectre-style prompt snippets that return success or error
    /// objects instead of plain validation messages.
    /// </remarks>
    public sealed class PromptValidationResult
    {
        private PromptValidationResult(bool successful, string message)
        {
            Successful = successful;
            Message = message ?? string.Empty;
        }

        /// <summary>
        /// Gets a value indicating whether validation succeeded.
        /// </summary>
        public bool Successful { get; }

        /// <summary>
        /// Gets the validation message when validation failed.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Creates a successful validation result.
        /// </summary>
        /// <returns>A successful validation result.</returns>
        public static PromptValidationResult Success()
        {
            return new PromptValidationResult(true, string.Empty);
        }

        /// <summary>
        /// Creates a failed validation result.
        /// </summary>
        /// <param name="message">The validation failure message.</param>
        /// <returns>A failed validation result.</returns>
        public static PromptValidationResult Error(string message)
        {
            return new PromptValidationResult(false, message);
        }
    }
}
