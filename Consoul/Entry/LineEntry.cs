using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// A simple line entry implementation that maintains a fixed position within the console.
    /// </summary>
    public class LineEntry : FixedMessage
    {
        private string _message;
        private ConsoleColor _color;

        /// <summary>
        /// Gets or sets the line text and re-renders the fixed line when changed.
        /// </summary>
        public string Message { get { return _message; } set { _message = value; Render(_message, _color); } }

        /// <summary>
        /// Gets or sets the line foreground color and re-renders the fixed line when changed.
        /// </summary>
        public ConsoleColor Color { get { return _color; } set { _color = value; Render(_message, _color); } }

        /// <summary>
        /// Constructs a new line entry with an optional message and color.
        /// </summary>
        /// <param name="message">The initial message to be displayed on the line.</param>
        /// <param name="color">The color of the text to be displayed.</param>
        public LineEntry(string message, ConsoleColor? color = null) : base(Consoul.ConsoleBufferWidth)
        {
            _message = message;
            _color = color ?? RenderOptions.DefaultColor;
            Render(message, color);
        }
    }
}
