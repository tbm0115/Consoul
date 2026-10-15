using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines text decoration flags accepted by Spectre-style style snippets.
    /// </summary>
    /// <remarks>
    /// ANSI-capable terminals render these flags with SGR control sequences. Other terminals keep the visible text and
    /// color output readable without emitting control characters.
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
        Strikethrough = 16,

        /// <summary>
        /// Blinking text decoration.
        /// </summary>
        Blink = 32,

        /// <summary>
        /// Reverse-video text decoration.
        /// </summary>
        Reverse = 64,

        /// <summary>
        /// Concealed text decoration.
        /// </summary>
        Conceal = 128
    }
}
