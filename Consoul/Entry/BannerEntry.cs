using ConsoulLibrary.Color;
using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// A stylized rendering of text surrounded by repeating characters.
    /// </summary>
    public class BannerEntry : IDisposable, IRenderable
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
        private bool _hasRendered;
        private int _left;
        private int _top;

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
            Width = Math.Max(1, Consoul.ConsoleBufferWidth - 2);
        }

        /// <summary>
        /// Renders the banner around the given message.
        /// </summary>
        public void Render()
        {
            if (!_hasRendered)
            {
                _left = Math.Max(0, Math.Min(Consoul.ConsoleDriver.CursorLeft, Consoul.ConsoleBufferWidth - 1));
                _top = Consoul.ConsoleDriver.CursorTop;
            }

            int renderWidth = GetRenderWidth();
            string border = new string(RepeatCharacter, renderWidth);
            string blank = new string(' ', renderWidth);
            string message = FitMessage(_message, renderWidth);
            string paddedMessage = Center(message, renderWidth);
            string[] lines = { border, blank, paddedMessage, blank, border };

            for (int i = 0; i < lines.Length; i++)
            {
                _messages[i].MaxWidth = renderWidth;
                _messages[i].Height = 1;

                if (!_hasRendered)
                {
                    int lineTop = Math.Max(0, Math.Min(_top + i, Consoul.ConsoleDriver.BufferHeight - 1));
                    Consoul.ConsoleDriver.SetCursorPosition(_left, lineTop);
                }

                _messages[i].Render(lines[i], color: _scheme.Color, backgroundColor: _scheme.BackgroundColor);
            }

            _hasRendered = true;

            int nextTop = Math.Max(0, Math.Min(_top + lines.Length, Consoul.ConsoleDriver.BufferHeight - 1));
            Consoul.ConsoleDriver.SetCursorPosition(_left, nextTop);
        }

        private int GetRenderWidth()
        {
            int availableWidth = Math.Max(1, Consoul.ConsoleBufferWidth - Math.Max(0, _left));
            int requestedWidth = Width > 0 ? Width + 2 : availableWidth;
            return Math.Max(1, Math.Min(requestedWidth, availableWidth));
        }

        private static string FitMessage(string message, int width)
        {
            message = message ?? string.Empty;
            if (message.Length <= width)
            {
                return message;
            }

            if (width <= 1)
            {
                return message.Substring(0, width);
            }

            return message.Substring(0, width - 1) + "…";
        }

        private static string Center(string text, int width)
        {
            text = text ?? string.Empty;
            if (text.Length >= width)
            {
                return text;
            }

            int leftPadding = (width - text.Length) / 2;
            int rightPadding = width - text.Length - leftPadding;
            return new string(' ', leftPadding) + text + new string(' ', rightPadding);
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
