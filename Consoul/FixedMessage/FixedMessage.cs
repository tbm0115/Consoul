using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents text that is rendered in place within the console and can be edited after initial rendering.
    /// </summary>
    public class FixedMessage
    {
        protected int _x, _y, _fw;

        /// <summary>
        /// Maximum width that the message can be rendered within.
        /// </summary>
        public int? MaxWidth { get; set; }

        /// <summary>
        /// Constructs a new fixed message.
        /// </summary>
        public FixedMessage()
        {
            Initialize();
        }

        /// <summary>
        /// Constructs a new fixed message.
        /// </summary>
        /// <param name="maxWidth"><see cref="MaxWidth"/></param>
        public FixedMessage(int maxWidth) : this()
        {
            MaxWidth = maxWidth;
        }

        /// <summary>
        /// Initializes the coordinates for the fixed position.
        /// </summary>
        public void Initialize()
        {
            _x = Console.CursorLeft;
            _y = Console.CursorTop;
            _fw = Console.BufferWidth;
        }

        /// <summary>
        /// Updates the text for the fixed message.
        /// </summary>
        /// <param name="message">Text to be rendered.</param>
        /// <param name="color">Text color</param>
        public void Update(string message, ConsoleColor? color = null)
        {
            int prevX, prevY;
            prevX = Console.CursorLeft;
            prevY = Console.CursorTop;
            if (message?.Length > (MaxWidth ?? _fw))
                message = message.Substring(0, (MaxWidth ?? _fw) - 3) + "...";
            // Clear Message Space
            Console.SetCursorPosition(_x, _y);
            Consoul.Write(new string(' ', MaxWidth ?? _fw), writeLine: false);
            // Write Message
            Console.SetCursorPosition(_x, _y);
            Consoul.Write(message, color, false);
            Console.SetCursorPosition(prevX, prevY);
        }
    }
}
