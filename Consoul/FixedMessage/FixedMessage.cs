using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents text that is rendered in place within the console and can be edited after initial rendering.
    /// </summary>
    public class FixedMessage : IDisposable
    {
        /// <summary>
        /// Captures the fixed cursor left, cursor top, and console width used by the rendered message.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the minimum time between rendered updates in milliseconds.
        /// </summary>
        public int RefreshRate { get; set; } = 1000 / 60;
        private string _pendingMessage = null;
        private ColorScheme? _pendingScheme = null;
        private IReadOnlyList<FixedMessageSegment> _pendingSegments = null;
        private ConsoleColor? _pendingFallbackColor = null;
        private ConsoleColor? _pendingFallbackBackgroundColor = null;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly object _lock = new object();

        private bool _firstRender = true;
        private string _lastRenderedMessage = string.Empty;
        private ColorScheme? _lastRenderedScheme = null;
        private IReadOnlyList<FixedMessageSegment> _lastRenderedSegments = null;
        private ConsoleColor? _lastSegmentFallbackColor = null;
        private ConsoleColor? _lastSegmentFallbackBackgroundColor = null;
        private bool _disposed;
        private IConsoleDriver _renderDriver;

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
            Dispose(false);
        }

        /// <summary>
        /// Releases the resize listener and pending update resources used by the fixed message.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases resources used by the fixed message.
        /// </summary>
        /// <param name="disposing">Indicates whether managed resources should be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _pendingMessage = null;
            _pendingScheme = null;
            _pendingSegments = null;
            Consoul.WindowResized -= OnWindowResized;
            _cancellationTokenSource.Cancel();
            if (disposing)
            {
                _cancellationTokenSource.Dispose();
            }
        }

        /// <summary>
        /// Initializes the coordinates for the fixed position.
        /// </summary>
        private void Initialize()
        {
            _renderDriver = Consoul.ConsoleDriver;
            _x = _renderDriver.CursorLeft;
            _y = _renderDriver.CursorTop;
            _fw = Consoul.ConsoleBufferWidth;
        }

        internal bool HasFixedPosition => _renderDriver != null;

        internal int FixedLeft => _x;

        internal int FixedTop => _y;

        internal void SetFixedPosition(int left, int top)
        {
            if (_disposed)
            {
                return;
            }

            _renderDriver = Consoul.ConsoleDriver;
            int width = Consoul.ConsoleBufferWidth;
            int height = Consoul.ConsoleBufferHeight;
            _x = Math.Max(0, Math.Min(left, Math.Max(0, width - 1)));
            _y = Math.Max(0, Math.Min(top, Math.Max(0, height - 1)));
            _fw = width;
            _firstRender = false;
            _lastUpdate = DateTime.MinValue;
            ClearPendingUpdate();
        }

        /// <summary>
        /// Resets the text area by clearing the content.
        /// </summary>
        public void Reset()
        {
            if (_disposed)
            {
                return;
            }

            if (_renderDriver == null || !ReferenceEquals(_renderDriver, Consoul.ConsoleDriver))
            {
                Initialize();
            }

            _fw = Consoul.ConsoleBufferWidth;
            _firstRender = true;

            // Clear Message Space
            using (var cursorPosition = Consoul.SaveCursor())
            {
                for (int i = 0; i < Height; i++)
                {
                    int x = GetSafeLeft();
                    int y = GetSafeTop(_y + i);
                    Consoul.ConsoleDriver.SetCursorPosition(x, y);
                    var clearWidth = Math.Min(MaxWidth ?? _fw, Math.Max(0, _fw - x));
                    Consoul.Write(new string(Whitespace, Math.Max(0, clearWidth)), writeLine: false);
                }
            }
        }

        /// <summary>
        /// Renders the text for the fixed message.
        /// </summary>
        /// <param name="message">Text to be rendered.</param>
        /// <param name="color">Text color</param>
        /// <param name="backgroundColor">Background color for the rendered text.</param>
        public void Render(string message, ConsoleColor? color = null, ConsoleColor? backgroundColor = null)
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    return;
                }

                if (_firstRender || !ReferenceEquals(_renderDriver, Consoul.ConsoleDriver))
                {
                    Initialize();
                    _lastUpdate = DateTime.MinValue;
                    ClearPendingUpdate();
                }

                if ((DateTime.Now - _lastUpdate).TotalMilliseconds < RefreshRate)
                {
                    // Store the pending update if refresh rate limit is hit
                    _pendingMessage = message;
                    _pendingSegments = null;
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
                        if (!t.IsCanceled && !_disposed)
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

        internal void RenderSegments(IEnumerable<FixedMessageSegment> segments, ConsoleColor? fallbackColor = null, ConsoleColor? fallbackBackgroundColor = null)
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    return;
                }

                if (_firstRender || !ReferenceEquals(_renderDriver, Consoul.ConsoleDriver))
                {
                    Initialize();
                    _lastUpdate = DateTime.MinValue;
                    ClearPendingUpdate();
                }

                IReadOnlyList<FixedMessageSegment> normalized = NormalizeSegments(segments);
                if ((DateTime.Now - _lastUpdate).TotalMilliseconds < RefreshRate)
                {
                    _pendingMessage = null;
                    _pendingScheme = null;
                    _pendingSegments = normalized;
                    _pendingFallbackColor = fallbackColor;
                    _pendingFallbackBackgroundColor = fallbackBackgroundColor;

                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource = new CancellationTokenSource();
                    var token = _cancellationTokenSource.Token;

                    Task.Delay(RefreshRate, token).ContinueWith(t =>
                    {
                        if (!t.IsCanceled && !_disposed)
                        {
                            RenderPendingMessage();
                        }
                    }, token);
                    return;
                }

                _lastUpdate = DateTime.Now;

                RenderSegmentMessage(normalized, fallbackColor, fallbackBackgroundColor);
                _firstRender = false;
            }
        }

        /// <summary>
        /// Renders the text for the fixed message using a reusable style.
        /// </summary>
        /// <param name="message">Text to be rendered.</param>
        /// <param name="style">Optional style applied to the rendered text.</param>
        public void RenderStyled(string message, Style style = null)
        {
            Render(message, style?.Foreground, style?.Background);
        }

        /// <summary>
        /// Handles rendering of the pending message if one exists.
        /// </summary>
        private void RenderPendingMessage()
        {
            lock (_lock)
            {
                if (_disposed || !ReferenceEquals(_renderDriver, Consoul.ConsoleDriver))
                {
                    ClearPendingUpdate();
                    return;
                }

                if (_pendingSegments != null)
                {
                    _lastUpdate = DateTime.Now;
                    RenderSegmentMessage(_pendingSegments, _pendingFallbackColor, _pendingFallbackBackgroundColor);
                    _firstRender = false;
                    ClearPendingUpdate();
                }
                else if (_pendingMessage != null)
                {
                    _lastUpdate = DateTime.Now;
                    RenderMessage(_pendingMessage, _pendingScheme?.Color, _pendingScheme?.BackgroundColor);
                    _firstRender = false;
                    ClearPendingUpdate();
                }
            }
        }

        private void ClearPendingUpdate()
        {
            _pendingMessage = null;
            _pendingScheme = null;
            _pendingSegments = null;
            _pendingFallbackColor = null;
            _pendingFallbackBackgroundColor = null;
        }

        /// <summary>
        /// Performs the actual rendering of the message, with consideration for multi-line content and avoiding line overflow.
        /// </summary>
        /// <param name="message">Text to be rendered.</param>
        /// <param name="color">Text color</param>
        /// <param name="backgroundColor">Background color for the rendered text.</param>
        private void RenderMessage(string message, ConsoleColor? color = null, ConsoleColor? backgroundColor = null)
        {
            _fw = Consoul.ConsoleBufferWidth;
            if (message == null)
            {
                message = string.Empty;
            }
            var resolvedColor = RenderOptions.GetColorOrDefault(color);
            var resolvedBackground = RenderOptions.GetBackgroundColorOrDefault(backgroundColor);

            int x = GetSafeLeft();
            int availableWidth = Math.Max(0, _fw - x);

            // Split message into lines based on MaxWidth and available buffer width to prevent overflow.
            var maxWidth = Math.Min(MaxWidth ?? availableWidth, availableWidth);
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
                    Consoul.ConsoleDriver.SetCursorPosition(x, GetSafeTop(_y + i));
                    Consoul.Write(lines[i], color: resolvedColor, backgroundColor: resolvedBackground, writeLine: false);
                }
            }

            _lastRenderedMessage = message;
            _lastRenderedScheme = new ColorScheme
            {
                Color = resolvedColor,
                BackgroundColor = resolvedBackground
            };
            _lastRenderedSegments = null;
            _lastSegmentFallbackColor = null;
            _lastSegmentFallbackBackgroundColor = null;
        }

        private void RenderSegmentMessage(IReadOnlyList<FixedMessageSegment> segments, ConsoleColor? fallbackColor = null, ConsoleColor? fallbackBackgroundColor = null)
        {
            _fw = Consoul.ConsoleBufferWidth;

            int x = GetSafeLeft();
            int availableWidth = Math.Max(0, _fw - x);
            int maxWidth = Math.Min(MaxWidth ?? availableWidth, availableWidth);
            IList<IList<FixedMessageSegment>> lines = SplitSegmentsIntoLines(segments, maxWidth);

            if (lines.Count > Height)
            {
                lines = lines.Take(Height).ToList();
            }

            Reset();

            ConsoleColor resolvedFallback = RenderOptions.GetColorOrDefault(fallbackColor);
            ConsoleColor resolvedFallbackBackground = RenderOptions.GetBackgroundColorOrDefault(fallbackBackgroundColor);
            using (var cursorPosition = Consoul.SaveCursor())
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    Consoul.ConsoleDriver.SetCursorPosition(x, GetSafeTop(_y + i));
                    foreach (FixedMessageSegment segment in lines[i])
                    {
                        if (segment.Text.Length == 0)
                        {
                            continue;
                        }

                        ColorScheme scheme = segment.Scheme ?? new ColorScheme
                        {
                            Color = resolvedFallback,
                            BackgroundColor = resolvedFallbackBackground
                        };
                        TerminalLinkWriter.WriteText(segment.Text, scheme, segment.Link);
                    }
                }
            }

            _lastRenderedMessage = string.Concat(segments.Select(segment => segment.Text));
            _lastRenderedScheme = new ColorScheme
            {
                Color = resolvedFallback,
                BackgroundColor = resolvedFallbackBackground
            };
            _lastRenderedSegments = segments;
            _lastSegmentFallbackColor = fallbackColor;
            _lastSegmentFallbackBackgroundColor = fallbackBackgroundColor;
        }

        private int GetSafeLeft()
        {
            int bufferWidth = Consoul.ConsoleBufferWidth;
            if (bufferWidth <= 0)
            {
                return 0;
            }

            return _x >= bufferWidth ? 0 : Math.Max(0, _x);
        }

        private static int GetSafeTop(int top)
        {
            int bufferHeight = Consoul.ConsoleBufferHeight;
            if (bufferHeight <= 0)
            {
                return 0;
            }

            return Math.Max(0, Math.Min(top, bufferHeight - 1));
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
            if (width <= 0)
            {
                lines.Add(string.Empty);
                return lines;
            }

            int currentIndex = 0;

            while (currentIndex < message.Length)
            {
                int length = Math.Min(width, message.Length - currentIndex);
                lines.Add(message.Substring(currentIndex, length));
                currentIndex += length;
            }

            return lines;
        }

        private static IReadOnlyList<FixedMessageSegment> NormalizeSegments(IEnumerable<FixedMessageSegment> segments)
        {
            if (segments == null)
            {
                return new[] { new FixedMessageSegment(string.Empty) };
            }

            return segments
                .Where(segment => segment.Text != null)
                .Select(segment => new FixedMessageSegment(segment.Text, segment.Scheme, segment.Link))
                .ToList();
        }

        private static IList<IList<FixedMessageSegment>> SplitSegmentsIntoLines(IReadOnlyList<FixedMessageSegment> segments, int width)
        {
            var lines = new List<IList<FixedMessageSegment>>();
            var currentLine = new List<FixedMessageSegment>();
            if (width <= 0)
            {
                lines.Add(currentLine);
                return lines;
            }

            int currentWidth = 0;
            foreach (FixedMessageSegment segment in segments)
            {
                string text = segment.Text ?? string.Empty;
                int index = 0;
                while (index < text.Length)
                {
                    if (currentWidth >= width)
                    {
                        lines.Add(currentLine);
                        currentLine = new List<FixedMessageSegment>();
                        currentWidth = 0;
                    }

                    int take = Math.Min(width - currentWidth, text.Length - index);
                    currentLine.Add(new FixedMessageSegment(text.Substring(index, take), segment.Scheme, segment.Link));
                    currentWidth += take;
                    index += take;
                }
            }

            lines.Add(currentLine);
            return lines;
        }

        /// <summary>
        /// Handles console window resize events to adjust the rendered message.
        /// </summary>
        private void OnWindowResized(object sender, EventArgs e)
        {
            lock (_lock)
            {
                if (_disposed || _renderDriver == null || !ReferenceEquals(_renderDriver, Consoul.ConsoleDriver))
                {
                    ClearPendingUpdate();
                    return;
                }

                _fw = Consoul.ConsoleBufferWidth; // Update the current buffer width
                if (_pendingMessage != null)
                {
                    RenderPendingMessage(); // If there's a pending message, render it
                }
                else if (_pendingSegments != null)
                {
                    RenderPendingMessage();
                }
                else if (_lastRenderedSegments != null)
                {
                    RenderSegmentMessage(_lastRenderedSegments, _lastSegmentFallbackColor, _lastSegmentFallbackBackgroundColor);
                    _firstRender = false;
                }
                else
                {
                    // Re-render the last message if available to fit new console size
                    RenderMessage(_lastRenderedMessage ?? string.Empty, _lastRenderedScheme?.Color, _lastRenderedScheme?.BackgroundColor);
                    _firstRender = false;
                }
            }
        }
    }

    internal readonly struct FixedMessageSegment
    {
        public FixedMessageSegment(string text, ColorScheme? scheme = null)
            : this(text, scheme, null)
        {
        }

        public FixedMessageSegment(string text, ColorScheme? scheme, Link link)
        {
            Text = text ?? string.Empty;
            Scheme = scheme;
            Link = link;
        }

        public string Text { get; }

        public ColorScheme? Scheme { get; }

        public Link Link { get; }
    }
}
