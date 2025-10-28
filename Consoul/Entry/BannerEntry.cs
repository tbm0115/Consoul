using ConsoulLibrary.Color;
using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// A stylized rendering of text surrounded by repeating characters.
    /// </summary>
    public class BannerEntry
    {
        /// <summary>
        /// Width of the banner (default is the <see cref="Console.BufferWidth"/>).
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The character to be repeated and surrounding the <see cref="Message" />.
        /// </summary>
        public char RepeatCharacter { get; set; } = '*';

        private string _message;
        private ColorScheme _scheme;

        public string Message { get { return _message; } set { _message = value; this.Render(); } }

        public ColorScheme ColorScheme { get { return _scheme; } set { _scheme = value; this.Render(); } }

        private FixedMessage[] _messages = new FixedMessage[5];

        public BannerEntry(string message, ColorScheme scheme) : this(message, scheme.Color, scheme.BackgroundColor) { }

        /// <summary>
        /// Constructs a new banner entry with an optional message and color.
        /// </summary>
        /// <param name="message">The initial message to be displayed in the banner.</param>
        /// <param name="color">The color of the text to be displayed.</param>
        public BannerEntry(string message, ConsoleColor? color = null, ConsoleColor? backgroundColor = null)
        {
            for (int i = 0; i < _messages.Length; i++)
            {
                _messages[i] = new FixedMessage(Console.BufferWidth);
            }
            _message = message;
            _scheme = new ColorScheme()
            {
                Color = RenderOptions.GetColorOrDefault(color),
                BackgroundColor = RenderOptions.GetBackgroundColorOrDefault(backgroundColor)
            };
            Width = Math.Max(Console.BufferWidth - 1, message.Length);
        }

        /// <summary>
        /// Renders the banner around the given message.
        /// </summary>
        public new void Render()
        {
            // Generate the banner strings
            string newline = "\r\n";
            string border = new string(RepeatCharacter, Width+2);
            int padding = (Width - _message.Length ) / 2;
            string paddedMessage = new string(' ', padding) + _message + new string(' ', Width - padding - _message.Length+2);

            // Render the banner
            _messages[0].Render(border + newline, color: _scheme.Color, backgroundColor: _scheme.BackgroundColor);
            _messages[1].Render(new string(' ', Width+2) + newline, color: _scheme.Color, backgroundColor: _scheme.BackgroundColor);
            _messages[2].Render(paddedMessage, color: _scheme.Color, backgroundColor: _scheme.BackgroundColor);
            _messages[3].Render(new string(' ', Width+2) + newline, color: _scheme.Color, backgroundColor: _scheme.BackgroundColor);
            _messages[4].Render(border + newline, color: _scheme.Color, backgroundColor: _scheme.BackgroundColor);
        }

        public static string Render(string message, ConsoleColor? color = null, ConsoleColor? backgroundColor = null)
        {
            var banner = new BannerEntry(message, color, backgroundColor);
            banner.Render();
            return banner.Message;
        }

        public static string Render(string message, ColorScheme scheme)
        {
            var banner = new BannerEntry(message, scheme);
            banner.Render();
            return banner.Message;
        }
    }
}
