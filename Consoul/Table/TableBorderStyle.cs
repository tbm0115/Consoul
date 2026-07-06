namespace ConsoulLibrary
{
    /// <summary>
    /// Defines preset border styles used by <see cref="TableView"/> and <see cref="DynamicTableView{T}"/>.
    /// </summary>
    public enum TableBorderStyle
    {
        /// <summary>
        /// Uses the legacy Consoul table separator behavior with repeated horizontal line characters.
        /// </summary>
        Simple,

        /// <summary>
        /// Uses square Unicode box-drawing corners and separators.
        /// </summary>
        Square,

        /// <summary>
        /// Uses rounded Unicode box-drawing corners with square joins.
        /// </summary>
        Rounded,

        /// <summary>
        /// Uses ASCII plus, dash, and pipe characters for maximum terminal compatibility.
        /// </summary>
        Ascii,

        /// <summary>
        /// Uses heavy Unicode box-drawing corners and separators.
        /// </summary>
        Heavy,

        /// <summary>
        /// Uses double-line Unicode box-drawing corners and separators.
        /// </summary>
        Double
    }
}
