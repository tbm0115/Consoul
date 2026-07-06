namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a model validation issue discovered while rendering a validation summary.
    /// </summary>
    public sealed class ValidationIssue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationIssue"/> class.
        /// </summary>
        /// <param name="propertyName">The source property name.</param>
        /// <param name="displayName">The display name rendered for the property.</param>
        /// <param name="message">The validation message.</param>
        public ValidationIssue(string propertyName, string displayName, string message)
        {
            PropertyName = propertyName ?? string.Empty;
            DisplayName = displayName ?? PropertyName;
            Message = message ?? string.Empty;
        }

        /// <summary>
        /// Gets the source property name.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Gets the display name rendered for the property.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets the validation message.
        /// </summary>
        public string Message { get; }
    }
}
