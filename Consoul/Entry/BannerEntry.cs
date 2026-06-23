using ConsoulLibrary.Color;
using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// A stylized rendering of text surrounded by repeating characters.
    /// </summary>
    public class BannerEntry : IDisposable
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

        /// <summary>
        /// Gets or sets the message rendered in the center of the banner.
        /// </summary>
        public string Message { get { return _message; } set { _message = value; this.Render(); } }

        /// <summary>
        /// Gets or sets the color scheme used for the banner text and fill area.
        /// </summary>
        public ColorScheme ColorScheme { get { return _scheme; } set { _scheme = value; this.Render(); } }

        private FixedMessage[] _messages = new FixedMessage[5];

        /// <summary>
        /// Constructs a new banner entry with the specified message and color scheme.
        /// </summary>
        /// <param name="message">The initial message to be displayed in the banner.</param>
        /// <param name="scheme">The color scheme used for the banner.</param>
        public BannerEntry(string message, ColorScheme scheme) : this(message, scheme.Color, scheme.BackgroundColor) { }

        /// <summary>
        /// Constructs a new banner entry with an optional message and color.
        /// </summary>
        /// <param name="message">The initial message to be displayed in the banner.</param>
        /// <param name="color">The color of the text to be displayed.</param>
        /// <param name="backgroundColor">The background color of the banner.</param>
        public BannerEntry(string message, ConsoleColor? color = null, ConsoleColor? backgroundColor = null)
        {
            for (int i = 0; i < _messages.Length; i++)
            {
                _messages[i] = new FixedMessage(Consoul.ConsoleBufferWidth);
            }
            _message = message;
            _scheme = new ColorScheme()
            {
                Color = RenderOptions.GetColorOrDefault(color),
                BackgroundColor = RenderOptions.GetBackgroundColorOrDefault(backgroundColor)
            };
            Width = Math.Max(Consoul.ConsoleBufferWidth - 1, message.Length);
        }

        /// <summary>
        /// Renders the banner around the given message.
        /// </summary>
        public void Render()
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

        /// <summary>
        /// Releases fixed message resources used by the banner.
        /// </summary>
        public void Dispose()
        {
            foreach (var message in _messages)
            {
                message?.Dispose();
            }
        }

        /// <summary>
        /// Renders a temporary banner and returns the rendered message text.
        /// </summary>
        /// <param name="message">The message to display in the banner.</param>
        /// <param name="color">The foreground color used for the banner.</param>
        /// <param name="backgroundColor">The background color used for the banner.</param>
        /// <returns>The message rendered by the banner.</returns>
        public static string Render(string message, ConsoleColor? color = null, ConsoleColor? backgroundColor = null)
        {
            using (var banner = new BannerEntry(message, color, backgroundColor))
            {
                banner.Render();
                return banner.Message;
            }
        }

        /// <summary>
        /// Renders a temporary banner using a color scheme and returns the rendered message text.
        /// </summary>
        /// <param name="message">The message to display in the banner.</param>
        /// <param name="scheme">The color scheme used for the banner.</param>
        /// <returns>The message rendered by the banner.</returns>
        public static string Render(string message, ColorScheme scheme)
        {
            using (var banner = new BannerEntry(message, scheme))
            {
                banner.Render();
                return banner.Message;
            }
        }
    }
}
