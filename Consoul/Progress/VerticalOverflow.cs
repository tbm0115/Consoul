namespace ConsoulLibrary
{
    /// <summary>
    /// Defines how live display content behaves when it is taller than the terminal.
    /// </summary>
    public enum VerticalOverflow
    {
        /// <summary>
        /// Writes all content and lets the terminal scroll naturally.
        /// </summary>
        Visible,

        /// <summary>
        /// Crops content to the available terminal height.
        /// </summary>
        Crop,

        /// <summary>
        /// Crops content and shows an ellipsis marker where content was removed.
        /// </summary>
        Ellipsis
    }
}
