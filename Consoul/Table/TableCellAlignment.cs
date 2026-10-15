namespace ConsoulLibrary
{
    /// <summary>
    /// Specifies how table cell text is aligned within its allocated column width.
    /// </summary>
    public enum TableCellAlignment
    {
        /// <summary>
        /// Uses the table's configured default alignment.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Aligns text to the left side of the cell.
        /// </summary>
        Left = 1,

        /// <summary>
        /// Centers text within the cell.
        /// </summary>
        Center = 2,

        /// <summary>
        /// Aligns text to the right side of the cell.
        /// </summary>
        Right = 3
    }
}
