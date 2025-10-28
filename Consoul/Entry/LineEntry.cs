using System;

namespace ConsoulLibrary.Entry
{
    /// <summary>
    /// A simple line entry.
    /// </summary>
    public class LineEntry : ILineEntry
    {
        /// <summary>
        /// Static text to be rendered.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Static value for the color of the text.
        /// </summary>
        public ConsoleColor Color { get; set; } = RenderOptions.DefaultColor;

        /// <summary>
        /// Constructs a new line entry.
        /// </summary>
        /// <param name="message"><see cref="Message"/></param>
        /// <param name="color"><see cref="Color"/></param>
        public LineEntry(string message, ConsoleColor? color = null)
        {
            Message = message;
            Color = color ?? RenderOptions.DefaultColor;
        }
    }
}
