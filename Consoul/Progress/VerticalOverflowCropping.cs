namespace ConsoulLibrary
{
    /// <summary>
    /// Defines which side of overflowing live display content is cropped.
    /// </summary>
    public enum VerticalOverflowCropping
    {
        /// <summary>
        /// Crops older content from the top and keeps the bottom of the display visible.
        /// </summary>
        Top,

        /// <summary>
        /// Crops newer content from the bottom and keeps the top of the display visible.
        /// </summary>
        Bottom
    }
}
