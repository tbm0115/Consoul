namespace ConsoulLibrary
{
    /// <summary>
    /// Represents the result of validating command settings.
    /// </summary>
    public class CommandValidationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandValidationResult"/> class.
        /// </summary>
        /// <param name="successful">Whether validation succeeded.</param>
        /// <param name="message">The validation message when validation failed.</param>
        protected CommandValidationResult(bool successful, string message)
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
        public static CommandValidationResult Success()
        {
            return new CommandValidationResult(true, string.Empty);
        }

        /// <summary>
        /// Creates a failed validation result.
        /// </summary>
        /// <param name="message">The validation failure message.</param>
        /// <returns>A failed validation result.</returns>
        public static CommandValidationResult Error(string message)
        {
            return new CommandValidationResult(false, message);
        }
    }
}
