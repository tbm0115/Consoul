namespace ConsoulLibrary
{
    /// <summary>
    /// Defines text overflow behavior for migration-friendly text renderables.
    /// </summary>
    public enum Overflow
    {
        /// <summary>
        /// Wraps overflowing text onto additional lines.
        /// </summary>
        Wrap,

        /// <summary>
        /// Crops overflowing text at the configured width.
        /// </summary>
        Crop,

        /// <summary>
        /// Uses an ellipsis marker when overflowing text is cropped.
        /// </summary>
        Ellipsis
    }
}
