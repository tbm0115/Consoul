namespace ConsoulLibrary
{
    /// <summary>
    /// Defines the visual intent for a badge rendered in a console view.
    /// </summary>
    public enum BadgeKind
    {
        /// <summary>
        /// Renders a neutral badge.
        /// </summary>
        Default,

        /// <summary>
        /// Renders an informational badge.
        /// </summary>
        Info,

        /// <summary>
        /// Renders a successful outcome badge.
        /// </summary>
        Success,

        /// <summary>
        /// Renders a warning badge.
        /// </summary>
        Warning,

        /// <summary>
        /// Renders an error badge.
        /// </summary>
        Error
    }
}
