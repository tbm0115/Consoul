namespace ConsoulLibrary
{
    /// <summary>
    /// Defines layout modes for migration-friendly figlet text rendering.
    /// </summary>
    public enum FigletLayoutMode
    {
        /// <summary>
        /// Preserves the normal spacing between rendered glyphs.
        /// </summary>
        FullSize,

        /// <summary>
        /// Removes inter-glyph spacing when glyphs can fit more tightly.
        /// </summary>
        Fitted,

        /// <summary>
        /// Uses the most compact spacing supported by Consoul's dependency-free glyph renderer.
        /// </summary>
        Smushed
    }
}
