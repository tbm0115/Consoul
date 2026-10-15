namespace ConsoulLibrary
{
    /// <summary>
    /// Defines the visual intent for a callout rendered in a console view.
    /// </summary>
    public enum CalloutKind
    {
        /// <summary>
        /// Renders an informational callout.
        /// </summary>
        Info,

        /// <summary>
        /// Renders a successful outcome callout.
        /// </summary>
        Success,

        /// <summary>
        /// Renders a warning callout.
        /// </summary>
        Warning,

        /// <summary>
        /// Renders an error callout.
        /// </summary>
        Error
    }
}
