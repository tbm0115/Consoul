using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents one source pixel for a dependency-free terminal canvas image.
    /// </summary>
    public readonly struct CanvasImagePixel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasImagePixel"/> struct.
        /// </summary>
        /// <param name="red">The red channel.</param>
        /// <param name="green">The green channel.</param>
        /// <param name="blue">The blue channel.</param>
        public CanvasImagePixel(byte red, byte green, byte blue)
            : this(red, green, blue, 255)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasImagePixel"/> struct.
        /// </summary>
        /// <param name="red">The red channel.</param>
        /// <param name="green">The green channel.</param>
        /// <param name="blue">The blue channel.</param>
        /// <param name="alpha">The alpha channel.</param>
        public CanvasImagePixel(byte red, byte green, byte blue, byte alpha)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        /// <summary>
        /// Gets the red channel.
        /// </summary>
        public byte Red { get; }

        /// <summary>
        /// Gets the green channel.
        /// </summary>
        public byte Green { get; }

        /// <summary>
        /// Gets the blue channel.
        /// </summary>
        public byte Blue { get; }

        /// <summary>
        /// Gets the alpha channel.
        /// </summary>
        public byte Alpha { get; }

        /// <summary>
        /// Gets a fully transparent pixel.
        /// </summary>
        public static CanvasImagePixel Transparent => new CanvasImagePixel(0, 0, 0, 0);

        /// <summary>
        /// Creates a pixel from a console color.
        /// </summary>
        /// <param name="color">The console color to map.</param>
        /// <returns>A pixel using the approximate RGB value of the console color.</returns>
        public static CanvasImagePixel FromConsoleColor(ConsoleColor color)
        {
            return ConsoleColorPalette.ToPixel(color);
        }

        /// <summary>
        /// Maps the pixel to the nearest console color.
        /// </summary>
        /// <returns>The nearest console color.</returns>
        public ConsoleColor ToConsoleColor()
        {
            return ConsoleColorPalette.FindNearest(Red, Green, Blue);
        }
    }

    internal static class ConsoleColorPalette
    {
        private static readonly ConsoleRgb[] Colors =
        {
            new ConsoleRgb(ConsoleColor.Black, 0, 0, 0),
            new ConsoleRgb(ConsoleColor.DarkBlue, 0, 0, 128),
            new ConsoleRgb(ConsoleColor.DarkGreen, 0, 128, 0),
            new ConsoleRgb(ConsoleColor.DarkCyan, 0, 128, 128),
            new ConsoleRgb(ConsoleColor.DarkRed, 128, 0, 0),
            new ConsoleRgb(ConsoleColor.DarkMagenta, 128, 0, 128),
            new ConsoleRgb(ConsoleColor.DarkYellow, 128, 128, 0),
            new ConsoleRgb(ConsoleColor.Gray, 192, 192, 192),
            new ConsoleRgb(ConsoleColor.DarkGray, 128, 128, 128),
            new ConsoleRgb(ConsoleColor.Blue, 0, 0, 255),
            new ConsoleRgb(ConsoleColor.Green, 0, 255, 0),
            new ConsoleRgb(ConsoleColor.Cyan, 0, 255, 255),
            new ConsoleRgb(ConsoleColor.Red, 255, 0, 0),
            new ConsoleRgb(ConsoleColor.Magenta, 255, 0, 255),
            new ConsoleRgb(ConsoleColor.Yellow, 255, 255, 0),
            new ConsoleRgb(ConsoleColor.White, 255, 255, 255)
        };

        public static CanvasImagePixel ToPixel(ConsoleColor color)
        {
            foreach (ConsoleRgb candidate in Colors)
            {
                if (candidate.Color == color)
                {
                    return new CanvasImagePixel((byte)candidate.Red, (byte)candidate.Green, (byte)candidate.Blue);
                }
            }

            return new CanvasImagePixel(255, 255, 255);
        }

        public static ConsoleColor FindNearest(int red, int green, int blue)
        {
            ConsoleColor nearest = ConsoleColor.White;
            int nearestDistance = int.MaxValue;

            foreach (ConsoleRgb candidate in Colors)
            {
                int redDistance = red - candidate.Red;
                int greenDistance = green - candidate.Green;
                int blueDistance = blue - candidate.Blue;
                int distance =
                    (redDistance * redDistance) +
                    (greenDistance * greenDistance) +
                    (blueDistance * blueDistance);

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = candidate.Color;
                }
            }

            return nearest;
        }

        private readonly struct ConsoleRgb
        {
            public ConsoleRgb(ConsoleColor color, int red, int green, int blue)
            {
                Color = color;
                Red = red;
                Green = green;
                Blue = blue;
            }

            public ConsoleColor Color { get; }

            public int Red { get; }

            public int Green { get; }

            public int Blue { get; }
        }
    }
}
