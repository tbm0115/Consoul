namespace ConsoulLibrary
{
    /// <summary>
    /// Specifies how grouped prompt choices participate in selection results.
    /// </summary>
    public enum SelectionMode
    {
        /// <summary>
        /// Only leaf choices are returned as selections. In single-selection prompts, group headings cannot be selected.
        /// </summary>
        Leaf = 0,

        /// <summary>
        /// Group and leaf choices are each selectable as independent options.
        /// </summary>
        Independent = 1
    }
}
