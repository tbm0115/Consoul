using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines text decoration flags accepted by Spectre-style style snippets.
    /// </summary>
    /// <remarks>
    /// Consoul's current terminal renderer preserves these flags for compatibility while foreground and background colors
    /// provide the visible styling in the built-in 16-color output path.
    /// </remarks>
    [Flags]
    public enum Decoration
    {
        /// <summary>
        /// No text decoration.
        /// </summary>
        None = 0,

        /// <summary>
        /// Bold text decoration.
        /// </summary>
        Bold = 1,

        /// <summary>
        /// Dim text decoration.
        /// </summary>
        Dim = 2,

        /// <summary>
        /// Italic text decoration.
        /// </summary>
        Italic = 4,

        /// <summary>
        /// Underline text decoration.
        /// </summary>
        Underline = 8,

        /// <summary>
        /// Strikethrough text decoration.
        /// </summary>
        Strikethrough = 16
    }
}
