namespace ConsoulLibrary
{
    /// <summary>
    /// Defines the state rendered for a checklist item.
    /// </summary>
    public enum ChecklistItemState
    {
        /// <summary>
        /// Indicates that the item has not started or is waiting.
        /// </summary>
        Pending,

        /// <summary>
        /// Indicates that the item completed successfully.
        /// </summary>
        Complete,

        /// <summary>
        /// Indicates that the item needs attention but has not failed.
        /// </summary>
        Warning,

        /// <summary>
        /// Indicates that the item failed.
        /// </summary>
        Error,

        /// <summary>
        /// Indicates that the item was intentionally skipped.
        /// </summary>
        Skipped
    }
}
