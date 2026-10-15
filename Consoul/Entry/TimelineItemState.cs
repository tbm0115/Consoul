namespace ConsoulLibrary
{
    /// <summary>
    /// Defines the rendered state for an item in a timeline.
    /// </summary>
    public enum TimelineItemState
    {
        /// <summary>
        /// Renders a pending timeline item.
        /// </summary>
        Pending,

        /// <summary>
        /// Renders the currently active timeline item.
        /// </summary>
        Active,

        /// <summary>
        /// Renders a completed timeline item.
        /// </summary>
        Complete,

        /// <summary>
        /// Renders a timeline item with a warning state.
        /// </summary>
        Warning,

        /// <summary>
        /// Renders a timeline item with an error state.
        /// </summary>
        Error,

        /// <summary>
        /// Renders a skipped timeline item.
        /// </summary>
        Skipped
    }
}
