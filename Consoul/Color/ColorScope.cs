using System;

namespace ConsoulLibrary.Color
{
    /// <summary>
    /// Disposable container for the current console color scheme. Resets the console color to original scheme when disposed.
    /// </summary>
    public class ColorScope : IDisposable
    {
        /// <summary>
        /// Reference to the original console color scheme at the time of construction.
        /// </summary>
        public ColorScheme OriginalColor { get; private set; }

        /// <summary>
        /// Reference to the current console color scheme.
        /// </summary>
        public ColorScheme CurrentColor
            => new ColorScheme()
            {
                BackgroundColor = Consoul.ConsoleDriver.BackgroundColor,
                Color = Consoul.ConsoleDriver.ForegroundColor
            };

        /// <summary>
        /// Constructs a new color state memory.
        /// </summary>
        public ColorScope()
        {
            OriginalColor = new ColorScheme()
            {
                BackgroundColor = Consoul.ConsoleDriver.BackgroundColor,
                Color = Consoul.ConsoleDriver.ForegroundColor
            };
        }

        /// <summary>
        /// Constructs a new color state memory and updates the color scheme.
        /// </summary>
        /// <param name="color"><inheritdoc cref="Console.ForegroundColor"/></param>
        /// <param name="backgroundColor"><inheritdoc cref="Console.BackgroundColor"/></param>
        public ColorScope(ConsoleColor color, ConsoleColor? backgroundColor = null) : this()
        {
            SetColorScheme(color, backgroundColor);
        }

        /// <summary>
        /// Sets the current console color scheme.
        /// </summary>
        /// <param name="color"><inheritdoc cref="Console.ForegroundColor"/></param>
        /// <param name="backgroundColor"><inheritdoc cref="Console.BackgroundColor"/></param>
        public void SetColorScheme(ConsoleColor color, ConsoleColor? backgroundColor = null)
        {
            Consoul.ConsoleDriver.ForegroundColor = color;
            if (backgroundColor != null)
            {
                Consoul.ConsoleDriver.BackgroundColor = backgroundColor.Value;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Consoul.ConsoleDriver.BackgroundColor = OriginalColor.BackgroundColor;
            Consoul.ConsoleDriver.ForegroundColor = OriginalColor.Color;
        }
    }
}
