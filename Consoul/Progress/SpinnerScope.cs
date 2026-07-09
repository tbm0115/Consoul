using System;
using System.Collections.Generic;
using System.Linq;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Maintains an in-place spinner message for a short-running operation.
    /// </summary>
    public class SpinnerScope : IDisposable
    {
        private readonly FixedMessage _message;
        private readonly SpinnerOptions _options;
        private int _frameIndex;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpinnerScope"/> class.
        /// </summary>
        /// <param name="message">The initial spinner message.</param>
        /// <param name="options">Optional rendering options for the spinner.</param>
        public SpinnerScope(string message, SpinnerOptions options = null)
        {
            _options = options != null ? options.Clone() : new SpinnerOptions();
            _message = new FixedMessage
            {
                MaxWidth = _options.MaxWidth,
                Height = Math.Max(1, _options.Height),
                RefreshRate = Math.Max(0, _options.RefreshRate)
            };

            Message = message ?? string.Empty;
            RenderActive();
            Consoul.LineBreak();
        }

        /// <summary>
        /// Gets the current spinner message without its frame or completion prefix.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets the current frame index.
        /// </summary>
        public int FrameIndex => _frameIndex;

        /// <summary>
        /// Gets a value indicating whether the spinner has been completed.
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// Advances the spinner to the next frame.
        /// </summary>
        /// <param name="message">Optional message to render with the next frame.</param>
        public void Tick(string message = null)
        {
            ThrowIfDisposed();
            if (message != null)
            {
                Message = message;
            }

            _frameIndex++;
            if (_options.AutoRefresh)
            {
                RenderActive();
            }
        }

        /// <summary>
        /// Updates the spinner message without advancing the frame.
        /// </summary>
        /// <param name="message">The message to render.</param>
        public void Update(string message)
        {
            ThrowIfDisposed();
            Message = message ?? string.Empty;
            if (_options.AutoRefresh)
            {
                RenderActive();
            }
        }

        /// <summary>
        /// Renders the current spinner message immediately.
        /// </summary>
        /// <returns>The current spinner scope.</returns>
        public SpinnerScope Refresh()
        {
            ThrowIfDisposed();
            RenderActive();
            return this;
        }

        /// <summary>
        /// Sets the spinner frames used for active messages.
        /// </summary>
        /// <param name="spinner">The spinner frame set to use.</param>
        /// <returns>The current spinner scope.</returns>
        public SpinnerScope Spinner(Spinner spinner)
        {
            ThrowIfDisposed();
            _options.Frames = spinner?.ToFrameArray();
            if (_options.AutoRefresh)
            {
                RenderActive();
            }

            return this;
        }

        /// <summary>
        /// Sets the spinner frames used for active messages.
        /// </summary>
        /// <param name="frames">The frames rendered by successive ticks.</param>
        /// <returns>The current spinner scope.</returns>
        public SpinnerScope Frames(params string[] frames)
        {
            ThrowIfDisposed();
            _options.Frames = frames;
            if (_options.AutoRefresh)
            {
                RenderActive();
            }

            return this;
        }

        /// <summary>
        /// Sets the spinner frames used for active messages.
        /// </summary>
        /// <param name="frames">The frames rendered by successive ticks.</param>
        /// <returns>The current spinner scope.</returns>
        public SpinnerScope Frames(IEnumerable<string> frames)
        {
            ThrowIfDisposed();
            _options.Frames = frames == null ? null : frames.ToArray();
            if (_options.AutoRefresh)
            {
                RenderActive();
            }

            return this;
        }

        /// <summary>
        /// Sets the foreground color used for active spinner text.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current spinner scope.</returns>
        public SpinnerScope SpinnerStyle(Style style)
        {
            ThrowIfDisposed();
            if (style == null)
            {
                throw new ArgumentNullException(nameof(style));
            }

            _options.ActiveColor = style.ToForegroundColor(_options.ActiveColor);
            _options.ActiveStyle = style;
            if (_options.AutoRefresh)
            {
                RenderActive();
            }

            return this;
        }

        /// <summary>
        /// Sets the foreground color used for active spinner text.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current spinner scope.</returns>
        public SpinnerScope SpinnerStyle(ConsoleColor color)
        {
            return SpinnerStyle(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for active spinner text from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current spinner scope.</returns>
        public SpinnerScope SpinnerStyle(SpectreColor color)
        {
            return SpinnerStyle(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for active spinner text from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current spinner scope.</returns>
        public SpinnerScope SpinnerStyle(RgbColor color)
        {
            return SpinnerStyle(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for active spinner text.
        /// </summary>
        /// <param name="style">The style text to parse.</param>
        /// <returns>The current spinner scope.</returns>
        public SpinnerScope SpinnerStyle(string style)
        {
            return SpinnerStyle(Style.Parse(style));
        }

        /// <summary>
        /// Enables or disables immediate rendering after spinner changes.
        /// </summary>
        /// <param name="enabled"><c>true</c> to render changes immediately; otherwise, <c>false</c>.</param>
        /// <returns>The current spinner scope.</returns>
        public SpinnerScope AutoRefresh(bool enabled)
        {
            ThrowIfDisposed();
            _options.AutoRefresh = enabled;
            return this;
        }

        /// <summary>
        /// Sets the minimum time between rendered spinner updates.
        /// </summary>
        /// <param name="milliseconds">The refresh rate in milliseconds. Negative values are treated as zero.</param>
        /// <returns>The current spinner scope.</returns>
        public SpinnerScope RefreshRate(int milliseconds)
        {
            ThrowIfDisposed();
            int normalized = RefreshRateHelpers.Normalize(milliseconds);
            _options.RefreshRate = normalized;
            _message.RefreshRate = normalized;
            return this;
        }

        /// <summary>
        /// Sets the minimum time between rendered spinner updates.
        /// </summary>
        /// <param name="interval">The refresh interval. Negative values are treated as zero.</param>
        /// <returns>The current spinner scope.</returns>
        public SpinnerScope RefreshRate(TimeSpan interval)
        {
            return RefreshRate(RefreshRateHelpers.Normalize(interval));
        }

        /// <summary>
        /// Marks the spinner as successful.
        /// </summary>
        /// <param name="message">Optional final message. When omitted, the current message is reused.</param>
        public void Succeed(string message = null)
        {
            Complete(_options.SuccessPrefix, message, _options.SuccessColor);
        }

        /// <summary>
        /// Marks the spinner as failed.
        /// </summary>
        /// <param name="message">Optional final message. When omitted, the current message is reused.</param>
        public void Fail(string message = null)
        {
            Complete(_options.FailurePrefix, message, _options.FailureColor);
        }

        /// <summary>
        /// Releases fixed message resources used by the spinner scope.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _message.Dispose();
        }

        private void RenderActive()
        {
            StyledText style = ResolveActiveStyle();
            _message.RenderSegments(
                MarkupRenderText.ParseSegmentsWithPrefix(
                    GetActivePrefix(),
                    Message,
                    style.Scheme,
                    style.Decoration,
                    style.ForegroundRgb,
                    style.BackgroundRgb),
                style.Scheme.Color,
                style.Scheme.BackgroundColor);
        }

        private void Complete(string prefix, string message, ConsoleColor color)
        {
            ThrowIfDisposed();
            Message = message ?? Message ?? string.Empty;
            IsCompleted = true;
            ColorScheme scheme = CreateScheme(color);
            _message.RenderSegments(
                MarkupRenderText.ParseSegmentsWithPrefix(prefix, Message, scheme),
                scheme.Color,
                scheme.BackgroundColor);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SpinnerScope));
            }
        }

        private string GetActivePrefix()
        {
            string frame = _options.GetFrame(_frameIndex);
            if (string.IsNullOrEmpty(frame))
            {
                return string.Empty;
            }

            return frame + (_options.MessageSeparator ?? string.Empty);
        }

        private static ColorScheme CreateScheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        private StyledText ResolveActiveStyle()
        {
            return _options.ActiveStyle == null
                ? CreateStyle(_options.ActiveColor)
                : ResolveStyle(_options.ActiveStyle, _options.ActiveColor);
        }

        private static StyledText ResolveStyle(Style style, ConsoleColor fallbackColor)
        {
            return style == null
                ? CreateStyle(fallbackColor)
                : new StyledText(
                    style.ToColorScheme(CreateScheme(fallbackColor)),
                    style.Decoration,
                    style.ForegroundRgb,
                    style.BackgroundRgb);
        }

        private static StyledText CreateStyle(ConsoleColor color)
        {
            return new StyledText(CreateScheme(color), Decoration.None, null, null);
        }

        private readonly struct StyledText
        {
            public StyledText(ColorScheme scheme, Decoration decoration, RgbColor? foregroundRgb, RgbColor? backgroundRgb)
            {
                Scheme = scheme;
                Decoration = decoration;
                ForegroundRgb = foregroundRgb;
                BackgroundRgb = backgroundRgb;
            }

            public ColorScheme Scheme { get; }

            public Decoration Decoration { get; }

            public RgbColor? ForegroundRgb { get; }

            public RgbColor? BackgroundRgb { get; }
        }
    }
}
