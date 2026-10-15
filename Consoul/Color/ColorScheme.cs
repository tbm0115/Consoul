using System;

namespace ConsoulLibrary.Color
{
    /// <summary>
    /// Represents a state of Console color settings.
    /// </summary>
    public struct ColorScheme
    {
        /// <summary>
        /// The background color used when writing text.
        /// </summary>
        public ConsoleColor BackgroundColor;

        /// <summary>
        /// The foreground color used when writing text.
        /// </summary>
        public ConsoleColor Color;
    }
}
