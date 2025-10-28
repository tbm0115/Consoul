using ConsoulLibrary.Color;
using System;
using System.Threading;
using System.Threading.Tasks;

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
        /// Maximum height that the message can occupy (for multi-line support).
        /// </summary>
        public int Height { get; set; } = 1;

        private DateTime _lastUpdate = DateTime.MinValue;
        private int RefreshRate { get; set; } = 1000 / 60;  // Minimum time between updates in milliseconds
        private string _pendingMessage = null;
        private ColorScheme? _pendingScheme = null;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly object _lock = new object();

        private bool _firstRender = true;
        private string _lastRenderedMessage = string.Empty;
        private ColorScheme? _lastRenderedScheme = null;

        /// <summary>
        /// Character used to fill whitespace around message
        /// </summary>
        public char Whitespace = ' ';

        /// <summary>
        /// Constructs a new fixed message.
        /// </summary>
        public FixedMessage()
        {
            Consoul.WindowResized += OnWindowResized; // Subscribe to window resize event
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
        /// Destructor to clean up resources.
        /// </summary>
        ~FixedMessage()
        {
            Consoul.WindowResized -= OnWindowResized; // Unsubscribe to prevent memory leaks
        }

        /// <summary>
        /// Initializes the coordinates for the fixed position.
        /// </summary>
        private void Initialize()
        {
            _x = Console.CursorLeft;
            _y = Console.CursorTop;
            _fw = Console.BufferWidth;
        }

        /// <summary>
        /// Resets the text area by clearing the content.
        /// </summary>
        public void Reset()
        {
            _firstRender = true;

            // Clear Message Space
            using (var cursorPosition = Consoul.SaveCursor())
            {
                for (int i = 0; i < Height; i++)
                {
                    var x = _x;
                    if (x >= Console.BufferWidth)
                        x = 0;
                    Console.SetCursorPosition(x, _y + i);
                    Consoul.Write(new string(Whitespace, MaxWidth ?? _fw), writeLine: false);
                }
            }
        }

        /// <summary>
        /// Renders the text for the fixed message.
        /// </summary>
        /// <param name="message">Text to be rendered.</param>
        /// <param name="color">Text color</param>
        public void Render(string message, ConsoleColor? color = null, ConsoleColor? backgroundColor = null)
        {
            if (_firstRender)
            {
                Initialize();
            }

            lock (_lock)
            {
                if ((DateTime.Now - _lastUpdate).TotalMilliseconds < RefreshRate)
                {
                    // Store the pending update if refresh rate limit is hit
                    _pendingMessage = message;
                    if (color != null || backgroundColor != null)
                    {
                        _pendingScheme = new ColorScheme()
                        {
                            Color = RenderOptions.GetColorOrDefault(color),
                            BackgroundColor = RenderOptions.GetBackgroundColorOrDefault(backgroundColor)
                        };
                    } else
                    {
                        _pendingScheme = null;
                    }

                    // Cancel any previous pending task
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource = new CancellationTokenSource();
                    var token = _cancellationTokenSource.Token;

                    // Schedule the update
                    Task.Delay(RefreshRate, token).ContinueWith(t =>
                    {
                        if (!t.IsCanceled)
                        {
                            RenderPendingMessage();
                        }
                    }, token);
                    return;
                }

                // Update immediately if within refresh rate limit
                _lastUpdate = DateTime.Now;

                RenderMessage(message, color, backgroundColor);
                _firstRender = false;
            }
        }

        /// <summary>
        /// Handles rendering of the pending message if one exists.
        /// </summary>
        private void RenderPendingMessage()
        {
            lock (_lock)
            {
                if (_pendingMessage != null)
                {
                    RenderMessage(_pendingMessage, _pendingScheme?.Color, _pendingScheme?.BackgroundColor);
                    _pendingMessage = null; // Clear the pending message once rendered
                    _pendingScheme = null;
                }
            }
        }

        /// <summary>
        /// Performs the actual rendering of the message, with consideration for multi-line content and avoiding line overflow.
        /// </summary>
        /// <param name="message">Text to be rendered.</param>
        /// <param name="color">Text color</param>
        private void RenderMessage(string message, ConsoleColor? color = null, ConsoleColor? backgroundColor = null)
        {
            if (message == null)
            {
                message = string.Empty;
            }
            var resolvedColor = RenderOptions.GetColorOrDefault(color);
            var resolvedBackground = RenderOptions.GetBackgroundColorOrDefault(backgroundColor);

            // Split message into lines based on MaxWidth and console buffer width to prevent overflow.
            var maxWidth = Math.Min(MaxWidth ?? _fw, _fw);
            var lines = SplitMessageIntoLines(message, maxWidth);

            // Limit lines to the available height
            if (lines.Count > Height)
            {
                lines = lines.GetRange(0, Height);
                if (lines[lines.Count - 1].Length > maxWidth)
                {
                    lines[lines.Count - 1] = lines[lines.Count - 1].Substring(0, Math.Max(0, maxWidth - 1)) + "…"; // Truncate the last line if needed
                }
            }

            // Clear Message Space
            Reset();

            // Write each line within the designated area
            using (var cursorPosition = Consoul.SaveCursor())
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    Console.SetCursorPosition(_x, _y + i);
                    Consoul.Write(lines[i], color: resolvedColor, backgroundColor: resolvedBackground, writeLine: false);
                }
            }

            _lastRenderedMessage = message;
            _lastRenderedScheme = new ColorScheme
            {
                Color = resolvedColor,
                BackgroundColor = resolvedBackground
            };
        }

        /// <summary>
        /// Splits the given message into lines that fit within the specified width.
        /// </summary>
        /// <param name="message">The message to be split.</param>
        /// <param name="width">The maximum width of each line.</param>
        /// <returns>A list of lines that fit within the specified width.</returns>
        private static System.Collections.Generic.List<string> SplitMessageIntoLines(string message, int width)
        {
            var lines = new System.Collections.Generic.List<string>();
            int currentIndex = 0;

            while (currentIndex < message.Length)
            {
                int length = Math.Min(width, message.Length - currentIndex);
                lines.Add(message.Substring(currentIndex, length));
                currentIndex += length;
            }

            return lines;
        }

        /// <summary>
        /// Handles console window resize events to adjust the rendered message.
        /// </summary>
        private void OnWindowResized(object sender, EventArgs e)
        {
            lock (_lock)
            {
                _fw = Console.BufferWidth; // Update the current buffer width
                if (_pendingMessage != null)
                {
                    RenderPendingMessage(); // If there's a pending message, render it
                }
                else
                {
                    // Re-render the last message if available to fit new console size
                    RenderMessage(_lastRenderedMessage ?? string.Empty, _lastRenderedScheme?.Color, _lastRenderedScheme?.BackgroundColor);
                }
            }
        }
    }
}
