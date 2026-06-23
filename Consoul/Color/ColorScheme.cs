using System;

namespace ConsoulLibrary.Color
{
    /// <summary>
    /// Represents a state of Console color settings.
    /// </summary>
    public struct ColorScheme
    {
        /// <see cref="Console.BackgroundColor"/>
        public ConsoleColor BackgroundColor;

        /// <see cref="Console.ForegroundColor"/>
        public ConsoleColor Color;
    }
}
