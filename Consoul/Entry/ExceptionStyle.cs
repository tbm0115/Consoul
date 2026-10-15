namespace ConsoulLibrary
{
    /// <summary>
    /// Defines Spectre-style color settings for structured exception output.
    /// </summary>
    public class ExceptionStyle
    {
        /// <summary>
        /// Gets or sets the style used for exception type lines.
        /// </summary>
        public Style Exception { get; set; }

        /// <summary>
        /// Gets or sets the style used for exception message lines.
        /// </summary>
        public Style Message { get; set; }

        /// <summary>
        /// Gets or sets the style used for method portions of stack trace lines.
        /// </summary>
        public Style Method { get; set; }

        /// <summary>
        /// Gets or sets the style used for path portions of stack trace lines.
        /// </summary>
        public Style Path { get; set; }

        /// <summary>
        /// Gets or sets the style used for line number portions of stack trace lines.
        /// </summary>
        public Style LineNumber { get; set; }
    }
}
