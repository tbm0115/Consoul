using System;
using System.Globalization;

namespace ConsoulLibrary.Color
{
    /// <summary>
    /// Represents an RGB color that can be mapped to the nearest console color for Consoul's built-in renderer.
    /// </summary>
    public readonly struct RgbColor : IEquatable<RgbColor>
    {
        private static readonly RgbColor[] ConsolePalette =
        {
            new RgbColor("black", 0, 0, 0, ConsoleColor.Black),
            new RgbColor("navy", 0, 0, 128, ConsoleColor.DarkBlue),
            new RgbColor("darkgreen", 0, 128, 0, ConsoleColor.DarkGreen),
            new RgbColor("teal", 0, 128, 128, ConsoleColor.DarkCyan),
            new RgbColor("maroon", 128, 0, 0, ConsoleColor.DarkRed),
            new RgbColor("purple", 128, 0, 128, ConsoleColor.DarkMagenta),
            new RgbColor("olive", 128, 128, 0, ConsoleColor.DarkYellow),
            new RgbColor("silver", 192, 192, 192, ConsoleColor.Gray),
            new RgbColor("darkgrey", 128, 128, 128, ConsoleColor.DarkGray),
            new RgbColor("blue", 0, 0, 255, ConsoleColor.Blue),
            new RgbColor("lime", 0, 255, 0, ConsoleColor.Green),
            new RgbColor("aqua", 0, 255, 255, ConsoleColor.Cyan),
            new RgbColor("red", 255, 0, 0, ConsoleColor.Red),
            new RgbColor("fuchsia", 255, 0, 255, ConsoleColor.Magenta),
            new RgbColor("yellow", 255, 255, 0, ConsoleColor.Yellow),
            new RgbColor("white", 255, 255, 255, ConsoleColor.White)
        };

        private RgbColor(string name, byte red, byte green, byte blue, ConsoleColor consoleColor)
        {
            Name = name ?? string.Empty;
            R = red;
            G = green;
            B = blue;
            ConsoleColor = consoleColor;
        }

        /// <summary>
        /// Gets the color's optional friendly name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the red channel.
        /// </summary>
        public byte R { get; }

        /// <summary>
        /// Gets the green channel.
        /// </summary>
        public byte G { get; }

        /// <summary>
        /// Gets the blue channel.
        /// </summary>
        public byte B { get; }

        /// <summary>
        /// Gets the nearest <see cref="ConsoleColor"/> used by Consoul's built-in renderer.
        /// </summary>
        public ConsoleColor ConsoleColor { get; }

        /// <summary>
        /// Gets black.
        /// </summary>
        public static RgbColor Black => FromConsoleColor(ConsoleColor.Black);

        /// <summary>
        /// Gets blue.
        /// </summary>
        public static RgbColor Blue => FromConsoleColor(ConsoleColor.Blue);

        /// <summary>
        /// Gets cyan.
        /// </summary>
        public static RgbColor Aqua => FromConsoleColor(ConsoleColor.Cyan);

        /// <summary>
        /// Gets cyan.
        /// </summary>
        public static RgbColor Cyan => FromConsoleColor(ConsoleColor.Cyan);

        /// <summary>
        /// Gets green.
        /// </summary>
        public static RgbColor Green => FromConsoleColor(ConsoleColor.Green);

        /// <summary>
        /// Gets gray.
        /// </summary>
        public static RgbColor Grey => FromConsoleColor(ConsoleColor.Gray);

        /// <summary>
        /// Gets gray.
        /// </summary>
        public static RgbColor Gray => FromConsoleColor(ConsoleColor.Gray);

        /// <summary>
        /// Gets yellow.
        /// </summary>
        public static RgbColor Yellow => FromConsoleColor(ConsoleColor.Yellow);

        /// <summary>
        /// Gets red.
        /// </summary>
        public static RgbColor Red => FromConsoleColor(ConsoleColor.Red);

        /// <summary>
        /// Gets white.
        /// </summary>
        public static RgbColor White => FromConsoleColor(ConsoleColor.White);

        /// <summary>
        /// Gets magenta.
        /// </summary>
        public static RgbColor Fuchsia => FromConsoleColor(ConsoleColor.Magenta);

        /// <summary>
        /// Gets magenta.
        /// </summary>
        public static RgbColor Magenta => FromConsoleColor(ConsoleColor.Magenta);

        /// <summary>
        /// Gets dark blue.
        /// </summary>
        public static RgbColor Navy => FromConsoleColor(ConsoleColor.DarkBlue);

        /// <summary>
        /// Gets dark cyan.
        /// </summary>
        public static RgbColor Teal => FromConsoleColor(ConsoleColor.DarkCyan);

        /// <summary>
        /// Gets dark red.
        /// </summary>
        public static RgbColor Maroon => FromConsoleColor(ConsoleColor.DarkRed);

        /// <summary>
        /// Gets dark yellow.
        /// </summary>
        public static RgbColor Olive => FromConsoleColor(ConsoleColor.DarkYellow);

        /// <summary>
        /// Gets dark magenta.
        /// </summary>
        public static RgbColor Purple => FromConsoleColor(ConsoleColor.DarkMagenta);

        /// <summary>
        /// Gets lime green.
        /// </summary>
        public static RgbColor Lime => FromConsoleColor(ConsoleColor.Green);

        /// <summary>
        /// Gets silver.
        /// </summary>
        public static RgbColor Silver => FromConsoleColor(ConsoleColor.Gray);

        /// <summary>
        /// Gets dark gray.
        /// </summary>
        public static RgbColor DarkGrey => FromConsoleColor(ConsoleColor.DarkGray);

        /// <summary>
        /// Gets dark gray.
        /// </summary>
        public static RgbColor DarkGray => FromConsoleColor(ConsoleColor.DarkGray);

        /// <summary>
        /// Gets dark blue.
        /// </summary>
        public static RgbColor DarkBlue => FromConsoleColor(ConsoleColor.DarkBlue);

        /// <summary>
        /// Gets dark cyan.
        /// </summary>
        public static RgbColor DarkCyan => FromConsoleColor(ConsoleColor.DarkCyan);

        /// <summary>
        /// Gets dark green.
        /// </summary>
        public static RgbColor DarkGreen => FromConsoleColor(ConsoleColor.DarkGreen);

        /// <summary>
        /// Gets dark magenta.
        /// </summary>
        public static RgbColor DarkMagenta => FromConsoleColor(ConsoleColor.DarkMagenta);

        /// <summary>
        /// Gets dark red.
        /// </summary>
        public static RgbColor DarkRed => FromConsoleColor(ConsoleColor.DarkRed);

        /// <summary>
        /// Gets dark yellow.
        /// </summary>
        public static RgbColor DarkYellow => FromConsoleColor(ConsoleColor.DarkYellow);

        /// <summary>
        /// Creates a color from RGB channel values.
        /// </summary>
        /// <param name="red">Red channel from 0 to 255.</param>
        /// <param name="green">Green channel from 0 to 255.</param>
        /// <param name="blue">Blue channel from 0 to 255.</param>
        /// <returns>The RGB color mapped to its nearest console color.</returns>
        public static RgbColor Rgb(int red, int green, int blue)
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
        public static RgbColor FromRgb(int red, int green, int blue)
        {
            byte r = Clamp(red);
            byte g = Clamp(green);
            byte b = Clamp(blue);
            return new RgbColor(string.Empty, r, g, b, FindNearestConsoleColor(r, g, b));
        }

        /// <summary>
        /// Creates a color from a hex token such as <c>#22c55e</c>, <c>22c55e</c>, or <c>#0f8</c>.
        /// </summary>
        /// <param name="hex">The hex color token.</param>
        /// <returns>The RGB color mapped to its nearest console color.</returns>
        public static RgbColor FromHex(string hex)
        {
            if (!TryFromHex(hex, out RgbColor color))
            {
                throw new ArgumentException("Hex color could not be parsed.", nameof(hex));
            }

            return color;
        }

        /// <summary>
        /// Tries to create a color from a hex token such as <c>#22c55e</c>, <c>22c55e</c>, or <c>#0f8</c>.
        /// </summary>
        /// <param name="hex">The hex color token.</param>
        /// <param name="color">The parsed color when parsing succeeds.</param>
        /// <returns><c>true</c> when the color was parsed; otherwise, <c>false</c>.</returns>
        public static bool TryFromHex(string hex, out RgbColor color)
        {
            string value = (hex ?? string.Empty).Trim();
            if (value.StartsWith("#", StringComparison.Ordinal))
            {
                value = value.Substring(1);
            }

            if (value.Length == 3)
            {
                value = new string(new[]
                {
                    value[0], value[0],
                    value[1], value[1],
                    value[2], value[2]
                });
            }

            if (value.Length != 6 ||
                !int.TryParse(value.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int red) ||
                !int.TryParse(value.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int green) ||
                !int.TryParse(value.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int blue))
            {
                color = default(RgbColor);
                return false;
            }

            color = FromRgb(red, green, blue);
            return true;
        }

        /// <summary>
        /// Creates a color from a named color token or hex token.
        /// </summary>
        /// <param name="name">The color token.</param>
        /// <returns>The color mapped to its nearest console color.</returns>
        public static RgbColor FromName(string name)
        {
            if (TryFromHex(name, out RgbColor hexColor))
            {
                return hexColor;
            }

            ConsoleColor consoleColor = Style.ParseColor(name);
            return FromConsoleColor(consoleColor);
        }

        /// <summary>
        /// Tries to create a color from a named color token or hex token.
        /// </summary>
        /// <param name="name">The color token.</param>
        /// <param name="color">The parsed color when parsing succeeds.</param>
        /// <returns><c>true</c> when the color token was parsed; otherwise, <c>false</c>.</returns>
        public static bool TryFromName(string name, out RgbColor color)
        {
            if (TryFromHex(name, out color))
            {
                return true;
            }

            ConsoleColor consoleColor;
            if (!Style.TryParseColor(name, out consoleColor))
            {
                color = default(RgbColor);
                return false;
            }

            color = FromConsoleColor(consoleColor);
            return true;
        }

        /// <summary>
        /// Creates a color from a console color.
        /// </summary>
        /// <param name="consoleColor">The console color.</param>
        /// <returns>The matching RGB color.</returns>
        public static RgbColor FromConsoleColor(ConsoleColor consoleColor)
        {
            foreach (RgbColor color in ConsolePalette)
            {
                if (color.ConsoleColor == consoleColor)
                {
                    return color;
                }
            }

            return ConsolePalette[0];
        }

        /// <summary>
        /// Converts the color to its nearest console color.
        /// </summary>
        /// <returns>The nearest console color.</returns>
        public ConsoleColor ToConsoleColor()
        {
            return ConsoleColor;
        }

        /// <summary>
        /// Converts a <see cref="RgbColor"/> to its nearest <see cref="ConsoleColor"/>.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        public static implicit operator ConsoleColor(RgbColor color)
        {
            return color.ToConsoleColor();
        }

        /// <summary>
        /// Converts a <see cref="ConsoleColor"/> to a <see cref="RgbColor"/>.
        /// </summary>
        /// <param name="color">The console color to convert.</param>
        public static implicit operator RgbColor(ConsoleColor color)
        {
            return FromConsoleColor(color);
        }

        /// <summary>
        /// Determines whether two colors are equal.
        /// </summary>
        /// <param name="left">The left color.</param>
        /// <param name="right">The right color.</param>
        /// <returns><c>true</c> when the colors are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(RgbColor left, RgbColor right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two colors are different.
        /// </summary>
        /// <param name="left">The left color.</param>
        /// <param name="right">The right color.</param>
        /// <returns><c>true</c> when the colors are different; otherwise, <c>false</c>.</returns>
        public static bool operator !=(RgbColor left, RgbColor right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Determines whether this color equals another color.
        /// </summary>
        /// <param name="other">The other color.</param>
        /// <returns><c>true</c> when the colors are equal; otherwise, <c>false</c>.</returns>
        public bool Equals(RgbColor other)
        {
            return R == other.R &&
                G == other.G &&
                B == other.B &&
                ConsoleColor == other.ConsoleColor;
        }

        /// <summary>
        /// Determines whether this color equals another object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><c>true</c> when the object is an equal color; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is RgbColor other && Equals(other);
        }

        /// <summary>
        /// Gets a hash code for this color.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = R;
                hash = (hash * 397) ^ G;
                hash = (hash * 397) ^ B;
                hash = (hash * 397) ^ (int)ConsoleColor;
                return hash;
            }
        }

        /// <summary>
        /// Returns the friendly name when available; otherwise, returns a hex color token.
        /// </summary>
        /// <returns>The color text.</returns>
        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Name)
                ? "#" + R.ToString("x2", CultureInfo.InvariantCulture) + G.ToString("x2", CultureInfo.InvariantCulture) + B.ToString("x2", CultureInfo.InvariantCulture)
                : Name;
        }

        private static byte Clamp(int value)
        {
            if (value < 0)
            {
                return 0;
            }

            if (value > 255)
            {
                return 255;
            }

            return (byte)value;
        }

        private static ConsoleColor FindNearestConsoleColor(byte red, byte green, byte blue)
        {
            ConsoleColor closest = ConsoleColor.White;
            double closestDistance = double.MaxValue;
            foreach (RgbColor candidate in ConsolePalette)
            {
                double distance = Distance(red, green, blue, candidate.R, candidate.G, candidate.B);
                if (distance < closestDistance)
                {
                    closest = candidate.ConsoleColor;
                    closestDistance = distance;
                }
            }

            return closest;
        }

        private static double Distance(int red, int green, int blue, int candidateRed, int candidateGreen, int candidateBlue)
        {
            int deltaRed = red - candidateRed;
            int deltaGreen = green - candidateGreen;
            int deltaBlue = blue - candidateBlue;
            return (deltaRed * deltaRed) + (deltaGreen * deltaGreen) + (deltaBlue * deltaBlue);
        }
    }
}
