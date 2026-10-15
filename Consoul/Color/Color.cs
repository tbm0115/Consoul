using System;

namespace ConsoulLibrary.Color
{
    /// <summary>
    /// Provides a Spectre-style RGB color facade for pasted console snippets while rendering through Consoul's built-in console palette.
    /// </summary>
    public readonly struct Color : IEquatable<Color>
    {
        private readonly RgbColor _rgbColor;

        private Color(RgbColor rgbColor)
        {
            _rgbColor = rgbColor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct from RGB channel values.
        /// </summary>
        /// <param name="red">Red channel from 0 to 255.</param>
        /// <param name="green">Green channel from 0 to 255.</param>
        /// <param name="blue">Blue channel from 0 to 255.</param>
        public Color(int red, int green, int blue)
            : this(RgbColor.FromRgb(red, green, blue))
        {
        }

        /// <summary>
        /// Gets the color's optional friendly name.
        /// </summary>
        public string Name => _rgbColor.Name;

        /// <summary>
        /// Gets the red channel.
        /// </summary>
        public byte R => _rgbColor.R;

        /// <summary>
        /// Gets the green channel.
        /// </summary>
        public byte G => _rgbColor.G;

        /// <summary>
        /// Gets the blue channel.
        /// </summary>
        public byte B => _rgbColor.B;

        /// <summary>
        /// Gets the nearest <see cref="ConsoleColor"/> used by Consoul's built-in renderer.
        /// </summary>
        public ConsoleColor ConsoleColor => _rgbColor.ConsoleColor;

        /// <summary>
        /// Gets black.
        /// </summary>
        public static Color Black => FromRgbColor(RgbColor.Black);

        /// <summary>
        /// Gets blue.
        /// </summary>
        public static Color Blue => FromRgbColor(RgbColor.Blue);

        /// <summary>
        /// Gets cyan.
        /// </summary>
        public static Color Aqua => FromRgbColor(RgbColor.Aqua);

        /// <summary>
        /// Gets cyan.
        /// </summary>
        public static Color Cyan => FromRgbColor(RgbColor.Cyan);

        /// <summary>
        /// Gets green.
        /// </summary>
        public static Color Green => FromRgbColor(RgbColor.Green);

        /// <summary>
        /// Gets gray.
        /// </summary>
        public static Color Grey => FromRgbColor(RgbColor.Grey);

        /// <summary>
        /// Gets gray.
        /// </summary>
        public static Color Gray => FromRgbColor(RgbColor.Gray);

        /// <summary>
        /// Gets yellow.
        /// </summary>
        public static Color Yellow => FromRgbColor(RgbColor.Yellow);

        /// <summary>
        /// Gets red.
        /// </summary>
        public static Color Red => FromRgbColor(RgbColor.Red);

        /// <summary>
        /// Gets white.
        /// </summary>
        public static Color White => FromRgbColor(RgbColor.White);

        /// <summary>
        /// Gets magenta.
        /// </summary>
        public static Color Fuchsia => FromRgbColor(RgbColor.Fuchsia);

        /// <summary>
        /// Gets magenta.
        /// </summary>
        public static Color Magenta => FromRgbColor(RgbColor.Magenta);

        /// <summary>
        /// Gets dark blue.
        /// </summary>
        public static Color Navy => FromRgbColor(RgbColor.Navy);

        /// <summary>
        /// Gets dark cyan.
        /// </summary>
        public static Color Teal => FromRgbColor(RgbColor.Teal);

        /// <summary>
        /// Gets dark red.
        /// </summary>
        public static Color Maroon => FromRgbColor(RgbColor.Maroon);

        /// <summary>
        /// Gets dark yellow.
        /// </summary>
        public static Color Olive => FromRgbColor(RgbColor.Olive);

        /// <summary>
        /// Gets dark magenta.
        /// </summary>
        public static Color Purple => FromRgbColor(RgbColor.Purple);

        /// <summary>
        /// Gets lime green.
        /// </summary>
        public static Color Lime => FromRgbColor(RgbColor.Lime);

        /// <summary>
        /// Gets silver.
        /// </summary>
        public static Color Silver => FromRgbColor(RgbColor.Silver);

        /// <summary>
        /// Gets dark gray.
        /// </summary>
        public static Color DarkGrey => FromRgbColor(RgbColor.DarkGrey);

        /// <summary>
        /// Gets dark gray.
        /// </summary>
        public static Color DarkGray => FromRgbColor(RgbColor.DarkGray);

        /// <summary>
        /// Gets dark blue.
        /// </summary>
        public static Color DarkBlue => FromRgbColor(RgbColor.DarkBlue);

        /// <summary>
        /// Gets dark cyan.
        /// </summary>
        public static Color DarkCyan => FromRgbColor(RgbColor.DarkCyan);

        /// <summary>
        /// Gets dark green.
        /// </summary>
        public static Color DarkGreen => FromRgbColor(RgbColor.DarkGreen);

        /// <summary>
        /// Gets dark magenta.
        /// </summary>
        public static Color DarkMagenta => FromRgbColor(RgbColor.DarkMagenta);

        /// <summary>
        /// Gets dark red.
        /// </summary>
        public static Color DarkRed => FromRgbColor(RgbColor.DarkRed);

        /// <summary>
        /// Gets dark yellow.
        /// </summary>
        public static Color DarkYellow => FromRgbColor(RgbColor.DarkYellow);

        /// <summary>
        /// Creates a color from RGB channel values.
        /// </summary>
        /// <param name="red">Red channel from 0 to 255.</param>
        /// <param name="green">Green channel from 0 to 255.</param>
        /// <param name="blue">Blue channel from 0 to 255.</param>
        /// <returns>The RGB color mapped to its nearest console color.</returns>
        public static Color Rgb(int red, int green, int blue)
        {
            return FromRgb(red, green, blue);
        }

        /// <summary>
        /// Creates a color from RGB channel values.
        /// </summary>
        /// <param name="red">Red channel from 0 to 255.</param>
        /// <param name="green">Green channel from 0 to 255.</param>
        /// <param name="blue">Blue channel from 0 to 255.</param>
        /// <returns>The RGB color mapped to its nearest console color.</returns>
        public static Color FromRgb(int red, int green, int blue)
        {
            return FromRgbColor(RgbColor.FromRgb(red, green, blue));
        }

        /// <summary>
        /// Creates a color from a hex token such as <c>#22c55e</c>, <c>22c55e</c>, or <c>#0f8</c>.
        /// </summary>
        /// <param name="hex">The hex color token.</param>
        /// <returns>The RGB color mapped to its nearest console color.</returns>
        public static Color FromHex(string hex)
        {
            return FromRgbColor(RgbColor.FromHex(hex));
        }

        /// <summary>
        /// Tries to create a color from a hex token such as <c>#22c55e</c>, <c>22c55e</c>, or <c>#0f8</c>.
        /// </summary>
        /// <param name="hex">The hex color token.</param>
        /// <param name="color">The parsed color when parsing succeeds.</param>
        /// <returns><c>true</c> when the color was parsed; otherwise, <c>false</c>.</returns>
        public static bool TryFromHex(string hex, out Color color)
        {
            RgbColor rgbColor;
            if (!RgbColor.TryFromHex(hex, out rgbColor))
            {
                color = default(Color);
                return false;
            }

            color = FromRgbColor(rgbColor);
            return true;
        }

        /// <summary>
        /// Creates a color from a named color token or hex token.
        /// </summary>
        /// <param name="name">The color token.</param>
        /// <returns>The color mapped to its nearest console color.</returns>
        public static Color FromName(string name)
        {
            return FromRgbColor(RgbColor.FromName(name));
        }

        /// <summary>
        /// Tries to create a color from a named color token or hex token.
        /// </summary>
        /// <param name="name">The color token.</param>
        /// <param name="color">The parsed color when parsing succeeds.</param>
        /// <returns><c>true</c> when the color token was parsed; otherwise, <c>false</c>.</returns>
        public static bool TryFromName(string name, out Color color)
        {
            RgbColor rgbColor;
            if (!RgbColor.TryFromName(name, out rgbColor))
            {
                color = default(Color);
                return false;
            }

            color = FromRgbColor(rgbColor);
            return true;
        }

        /// <summary>
        /// Creates a color from a console color.
        /// </summary>
        /// <param name="consoleColor">The console color.</param>
        /// <returns>The matching RGB color.</returns>
        public static Color FromConsoleColor(ConsoleColor consoleColor)
        {
            return FromRgbColor(RgbColor.FromConsoleColor(consoleColor));
        }

        /// <summary>
        /// Creates a color from Consoul's existing RGB color type.
        /// </summary>
        /// <param name="rgbColor">The RGB color.</param>
        /// <returns>The color facade.</returns>
        public static Color FromRgbColor(RgbColor rgbColor)
        {
            return new Color(rgbColor);
        }

        /// <summary>
        /// Converts this color to Consoul's existing RGB color type.
        /// </summary>
        /// <returns>The RGB color.</returns>
        public RgbColor ToRgbColor()
        {
            return _rgbColor;
        }

        /// <summary>
        /// Converts the color to its nearest console color.
        /// </summary>
        /// <returns>The nearest console color.</returns>
        public ConsoleColor ToConsoleColor()
        {
            return _rgbColor.ToConsoleColor();
        }

        /// <summary>
        /// Converts a <see cref="Color"/> to its nearest <see cref="ConsoleColor"/>.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        public static implicit operator ConsoleColor(Color color)
        {
            return color.ToConsoleColor();
        }

        /// <summary>
        /// Converts a <see cref="ConsoleColor"/> to a <see cref="Color"/>.
        /// </summary>
        /// <param name="color">The console color to convert.</param>
        public static implicit operator Color(ConsoleColor color)
        {
            return FromConsoleColor(color);
        }

        /// <summary>
        /// Converts a <see cref="RgbColor"/> to a <see cref="Color"/>.
        /// </summary>
        /// <param name="color">The RGB color to convert.</param>
        public static implicit operator Color(RgbColor color)
        {
            return FromRgbColor(color);
        }

        /// <summary>
        /// Converts a <see cref="Color"/> to a <see cref="RgbColor"/>.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        public static explicit operator RgbColor(Color color)
        {
            return color.ToRgbColor();
        }

        /// <summary>
        /// Determines whether two colors are equal.
        /// </summary>
        /// <param name="left">The left color.</param>
        /// <param name="right">The right color.</param>
        /// <returns><c>true</c> when the colors are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Color left, Color right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two colors are different.
        /// </summary>
        /// <param name="left">The left color.</param>
        /// <param name="right">The right color.</param>
        /// <returns><c>true</c> when the colors are different; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Color left, Color right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Determines whether this color equals another color.
        /// </summary>
        /// <param name="other">The other color.</param>
        /// <returns><c>true</c> when the colors are equal; otherwise, <c>false</c>.</returns>
        public bool Equals(Color other)
        {
            return _rgbColor.Equals(other._rgbColor);
        }

        /// <summary>
        /// Determines whether this color equals another object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><c>true</c> when the object is an equal color; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is Color other && Equals(other);
        }

        /// <summary>
        /// Gets a hash code for this color.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode()
        {
            return _rgbColor.GetHashCode();
        }

        /// <summary>
        /// Returns the friendly name when available; otherwise, returns a hex color token.
        /// </summary>
        /// <returns>The color text.</returns>
        public override string ToString()
        {
            return _rgbColor.ToString();
        }
    }
}
