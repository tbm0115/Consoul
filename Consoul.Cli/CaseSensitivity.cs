namespace ConsoulLibrary
{
    /// <summary>
    /// Defines Spectre-style command and option lookup case-sensitivity settings.
    /// </summary>
    public enum CaseSensitivity
    {
        /// <summary>
        /// Command names, aliases, branches, and options are matched case-insensitively.
        /// </summary>
        None = 0,

        /// <summary>
        /// Command names, aliases, branches, and options are matched case-sensitively.
        /// </summary>
        All = 1
    }
}
