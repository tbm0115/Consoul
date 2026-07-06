namespace ConsoulLibrary
{
    /// <summary>
    /// Defines named border presets for <see cref="PanelEntry"/>.
    /// </summary>
    public enum PanelBorderStyle
    {
        /// <summary>
        /// Uses the individual border characters configured on <see cref="PanelOptions"/>.
        /// </summary>
        Custom = 0,

        /// <summary>
        /// Uses square Unicode box-drawing corners.
        /// </summary>
        Square = 1,

        /// <summary>
        /// Uses rounded Unicode box-drawing corners.
        /// </summary>
        Rounded = 2,

        /// <summary>
        /// Uses ASCII plus, dash, and pipe characters for maximum terminal compatibility.
        /// </summary>
        Ascii = 3,

        /// <summary>
        /// Uses heavy Unicode box-drawing corners and borders.
        /// </summary>
        Heavy = 4,

        /// <summary>
        /// Uses double-line Unicode box-drawing corners and borders.
        /// </summary>
        Double = 5,

        /// <summary>
        /// Renders panel content, padding, title, and footer without a visible border.
        /// </summary>
        None = 6
    }
}
