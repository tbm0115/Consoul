namespace ConsoulLibrary
{
    /// <summary>
    /// Base type for strongly-typed command settings.
    /// </summary>
    public class CommandSettings
    {
        /// <summary>
        /// Validates the bound command settings before command execution.
        /// </summary>
        /// <returns>The validation result.</returns>
        public virtual CommandValidationResult Validate()
        {
            return CommandValidationResult.Success();
        }
    }
}
