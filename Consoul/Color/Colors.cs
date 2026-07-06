using System;

namespace ConsoulLibrary.Color
{
    /// <summary>
    /// Provides named console color aliases for Spectre-style builder snippets.
    /// </summary>
    /// <remarks>
    /// Use <c>using ConsoulLibrary.Color;</c> to use aliases such as <c>Colors.Aqua</c> with Consoul APIs
    /// that accept <see cref="ConsoleColor"/> values.
    /// </remarks>
    public static class Colors
    {
        /// <summary>
        /// Gets the black console color.
        /// </summary>
        public static ConsoleColor Black => ConsoleColor.Black;

        /// <summary>
        /// Gets the blue console color.
        /// </summary>
        public static ConsoleColor Blue => ConsoleColor.Blue;

        /// <summary>
        /// Gets the cyan console color.
        /// </summary>
        public static ConsoleColor Aqua => ConsoleColor.Cyan;

        /// <summary>
        /// Gets the cyan console color.
        /// </summary>
        public static ConsoleColor Cyan => ConsoleColor.Cyan;

        /// <summary>
        /// Gets the green console color.
        /// </summary>
        public static ConsoleColor Green => ConsoleColor.Green;

        /// <summary>
        /// Gets the gray console color.
        /// </summary>
        public static ConsoleColor Grey => ConsoleColor.Gray;

        /// <summary>
        /// Gets the gray console color.
        /// </summary>
        public static ConsoleColor Gray => ConsoleColor.Gray;

        /// <summary>
        /// Gets the yellow console color.
        /// </summary>
        public static ConsoleColor Yellow => ConsoleColor.Yellow;

        /// <summary>
        /// Gets the red console color.
        /// </summary>
        public static ConsoleColor Red => ConsoleColor.Red;

        /// <summary>
        /// Gets the white console color.
        /// </summary>
        public static ConsoleColor White => ConsoleColor.White;

        /// <summary>
        /// Gets the magenta console color.
        /// </summary>
        public static ConsoleColor Fuchsia => ConsoleColor.Magenta;

        /// <summary>
        /// Gets the magenta console color.
        /// </summary>
        public static ConsoleColor Magenta => ConsoleColor.Magenta;

        /// <summary>
        /// Gets the dark blue console color.
        /// </summary>
        public static ConsoleColor Navy => ConsoleColor.DarkBlue;

        /// <summary>
        /// Gets the dark cyan console color.
        /// </summary>
        public static ConsoleColor Teal => ConsoleColor.DarkCyan;

        /// <summary>
        /// Gets the dark red console color.
        /// </summary>
        public static ConsoleColor Maroon => ConsoleColor.DarkRed;

        /// <summary>
        /// Gets the dark yellow console color.
        /// </summary>
        public static ConsoleColor Olive => ConsoleColor.DarkYellow;

        /// <summary>
        /// Gets the dark magenta console color.
        /// </summary>
        public static ConsoleColor Purple => ConsoleColor.DarkMagenta;

        /// <summary>
        /// Gets the green console color.
        /// </summary>
        public static ConsoleColor Lime => ConsoleColor.Green;

        /// <summary>
        /// Gets the gray console color.
        /// </summary>
        public static ConsoleColor Silver => ConsoleColor.Gray;

        /// <summary>
        /// Gets the dark gray console color.
        /// </summary>
        public static ConsoleColor DarkGrey => ConsoleColor.DarkGray;

        /// <summary>
        /// Gets the dark gray console color.
        /// </summary>
        public static ConsoleColor DarkGray => ConsoleColor.DarkGray;

        /// <summary>
        /// Gets the dark blue console color.
        /// </summary>
        public static ConsoleColor DarkBlue => ConsoleColor.DarkBlue;

        /// <summary>
        /// Gets the dark cyan console color.
        /// </summary>
        public static ConsoleColor DarkCyan => ConsoleColor.DarkCyan;

        /// <summary>
        /// Gets the dark green console color.
        /// </summary>
        public static ConsoleColor DarkGreen => ConsoleColor.DarkGreen;

        /// <summary>
        /// Gets the dark magenta console color.
        /// </summary>
        public static ConsoleColor DarkMagenta => ConsoleColor.DarkMagenta;

        /// <summary>
        /// Gets the dark red console color.
        /// </summary>
        public static ConsoleColor DarkRed => ConsoleColor.DarkRed;

        /// <summary>
        /// Gets the dark yellow console color.
        /// </summary>
        public static ConsoleColor DarkYellow => ConsoleColor.DarkYellow;

        /// <summary>
        /// Parses a Spectre-style color token into the nearest console color.
        /// </summary>
        /// <param name="name">Color token such as <c>green</c>, <c>grey</c>, <c>aqua</c>, or <c>#22c55e</c>.</param>
        /// <returns>The nearest console color.</returns>
        public static ConsoleColor FromName(string name)
        {
            return Style.ParseColor(name);
        }

        /// <summary>
        /// Tries to parse a Spectre-style color token into the nearest console color.
        /// </summary>
        /// <param name="name">Color token such as <c>green</c>, <c>grey</c>, <c>aqua</c>, or <c>#22c55e</c>.</param>
        /// <param name="color">The parsed color when parsing succeeds.</param>
        /// <returns><c>true</c> when the color token is recognized; otherwise, <c>false</c>.</returns>
        public static bool TryFromName(string name, out ConsoleColor color)
        {
            return Style.TryParseColor(name, out color);
        }
    }
}
