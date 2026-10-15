namespace ConsoulLibrary.Cli
{
    /// <summary>
    /// Provides a Spectre-style validation result alias for command settings and parameter validators.
    /// </summary>
    /// <remarks>
    /// Import <c>ConsoulLibrary.Cli</c> when porting snippets that call <c>ValidationResult.Success()</c>
    /// or <c>ValidationResult.Error(...)</c>. The type derives from <see cref="ConsoulLibrary.CommandValidationResult"/>,
    /// so existing Consoul validation plumbing accepts it anywhere a command validation result is expected.
    /// </remarks>
    public sealed class ValidationResult : ConsoulLibrary.CommandValidationResult
    {
        private ValidationResult(bool successful, string message)
            : base(successful, message)
        {
        }

        /// <summary>
        /// Creates a successful validation result.
        /// </summary>
        /// <returns>A successful validation result.</returns>
        public static new ValidationResult Success()
        {
            return new ValidationResult(true, string.Empty);
        }

        /// <summary>
        /// Creates a failed validation result.
        /// </summary>
        /// <param name="message">The validation failure message.</param>
        /// <returns>A failed validation result.</returns>
        public static new ValidationResult Error(string message)
        {
            return new ValidationResult(false, message);
        }
    }
}
