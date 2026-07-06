using System;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Maintains an in-place status message for a short-running operation.
    /// </summary>
    public class StatusScope : IDisposable
    {
        private readonly FixedMessage _message;
        private readonly StatusOptions _options;
        private readonly ConsoleColor _defaultActiveColor;
        private int _frameIndex;
        private ConsoleColor? _activeColorOverride;
        private ConsoleColor? _activeBackgroundColorOverride;
        private Spinner _spinner;
        private Style _spinnerStyle;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusScope"/> class.
        /// </summary>
        /// <param name="message">The initial status message.</param>
        /// <param name="options">Optional rendering options for the status message.</param>
        public StatusScope(string message, StatusOptions options = null)
        {
            _options = options != null ? options.Clone() : new StatusOptions();
            _defaultActiveColor = _options.ActiveColor;
            _spinner = _options.Spinner ?? (_options.HasSpinnerFrames ? new Spinner(_options.SpinnerFrames) : null);
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
        /// Gets the current status message without its prefix.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the status has been completed.
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// Gets a Spectre-style property context over the current status scope.
        /// </summary>
        public StatusContext Context => new StatusContext(this);

        /// <summary>
        /// Updates the active status message.
        /// </summary>
        /// <param name="message">The status message to render.</param>
        /// <param name="color">Optional foreground color for the message.</param>
        public void Update(string message, ConsoleColor? color = null)
        {
            ThrowIfDisposed();
            Message = message ?? string.Empty;
            _activeColorOverride = color;
            _activeBackgroundColorOverride = null;
            if (_options.AutoRefresh)
            {
                RenderActive();
            }
        }

        /// <summary>
        /// Updates the active status message using a Spectre-style color facade.
        /// </summary>
        /// <param name="message">The status message to render.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the message.</param>
        public void Update(string message, SpectreColor color)
        {
            Update(message, (ConsoleColor?)color.ToConsoleColor());
        }

        /// <summary>
        /// Updates the active status message using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="message">The status message to render.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the message.</param>
        public void Update(string message, RgbColor color)
        {
            Update(message, (ConsoleColor?)color.ToConsoleColor());
        }

        /// <summary>
        /// Updates the active status message using a reusable style.
        /// </summary>
        /// <param name="message">The status message to render.</param>
        /// <param name="style">Optional style applied to the message.</param>
        public void UpdateStyled(string message, Style style = null)
        {
            ThrowIfDisposed();
            Message = message ?? string.Empty;
            _activeColorOverride = style?.Foreground;
            _activeBackgroundColorOverride = style?.Background;
            if (_options.AutoRefresh)
            {
                RenderActive();
            }
        }

        /// <summary>
        /// Updates the active status message using a Spectre-style color facade.
        /// </summary>
        /// <param name="message">The status message to render.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the message.</param>
        public void UpdateStyled(string message, SpectreColor color)
        {
            UpdateStyled(message, new Style(color));
        }

        /// <summary>
        /// Updates the active status message using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="message">The status message to render.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the message.</param>
        public void UpdateStyled(string message, RgbColor color)
        {
            UpdateStyled(message, new Style(color));
        }

        /// <summary>
        /// Updates the active status message and returns the current scope for fluent status workflows.
        /// </summary>
        /// <param name="message">The status message to render.</param>
        /// <returns>The current status scope.</returns>
        public StatusScope Status(string message)
        {
            Update(message);
            return this;
        }

        /// <summary>
        /// Updates the active status message using a Spectre-style color facade and returns the current scope.
        /// </summary>
        /// <param name="message">The status message to render.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the message.</param>
        /// <returns>The current status scope.</returns>
        public StatusScope Status(string message, SpectreColor color)
        {
            Update(message, color);
            return this;
        }

        /// <summary>
        /// Updates the active status message using an RGB color mapped to Consoul's console palette and returns the current scope.
        /// </summary>
        /// <param name="message">The status message to render.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the message.</param>
        /// <returns>The current status scope.</returns>
        public StatusScope Status(string message, RgbColor color)
        {
            Update(message, color);
            return this;
        }

        /// <summary>
        /// Updates the active status message using a reusable style and returns the current scope for fluent status workflows.
        /// </summary>
        /// <param name="message">The status message to render.</param>
        /// <param name="style">Optional style applied to the message.</param>
        /// <returns>The current status scope.</returns>
        public StatusScope StatusStyled(string message, Style style = null)
        {
            UpdateStyled(message, style);
            return this;
        }

        /// <summary>
        /// Updates the active status message using a Spectre-style color facade and returns the current scope.
        /// </summary>
        /// <param name="message">The status message to render.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the message.</param>
        /// <returns>The current status scope.</returns>
        public StatusScope StatusStyled(string message, SpectreColor color)
        {
            UpdateStyled(message, color);
            return this;
        }

        /// <summary>
        /// Updates the active status message using an RGB color mapped to Consoul's console palette and returns the current scope.
        /// </summary>
        /// <param name="message">The status message to render.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the message.</param>
        /// <returns>The current status scope.</returns>
        public StatusScope StatusStyled(string message, RgbColor color)
        {
            UpdateStyled(message, color);
            return this;
        }

        /// <summary>
        /// Renders the current status message immediately.
        /// </summary>
        /// <returns>The current status scope.</returns>
        public StatusScope Refresh()
        {
            ThrowIfDisposed();
            RenderActive();
            return this;
        }

        /// <summary>
        /// Sets the spinner frames used for active status messages.
        /// </summary>
        /// <param name="spinner">The spinner frame set to use.</param>
        /// <returns>The current status scope.</returns>
        public StatusScope Spinner(Spinner spinner)
        {
            return SetSpinner(spinner, throwOnNull: false);
        }

        /// <summary>
        /// Sets the spinner frames used for active status messages.
        /// </summary>
        /// <param name="frames">The frames rendered by successive refreshes.</param>
        /// <returns>The current status scope.</returns>
        public StatusScope Frames(params string[] frames)
        {
            ThrowIfDisposed();
            _spinner = null;
            _options.SpinnerFrames = frames;
            if (_options.AutoRefresh)
            {
                RenderActive();
            }

            return this;
        }

        /// <summary>
        /// Sets the foreground color used for active status and spinner text.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current status scope.</returns>
        public StatusScope SpinnerStyle(Style style)
        {
            if (style == null)
            {
                throw new ArgumentNullException(nameof(style));
            }

            return SetSpinnerStyle(style);
        }

        /// <summary>
        /// Sets the foreground color used for active status and spinner text.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current status scope.</returns>
        public StatusScope SpinnerStyle(ConsoleColor color)
        {
            return SpinnerStyle(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for active status and spinner text from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current status scope.</returns>
        public StatusScope SpinnerStyle(SpectreColor color)
        {
            return SpinnerStyle(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for active status and spinner text from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current status scope.</returns>
        public StatusScope SpinnerStyle(RgbColor color)
        {
            return SpinnerStyle(new Style(color));
        }

        /// <summary>
        /// Sets the foreground color used for active status and spinner text.
        /// </summary>
        /// <param name="style">The style text to parse.</param>
        /// <returns>The current status scope.</returns>
        public StatusScope SpinnerStyle(string style)
        {
            return SpinnerStyle(Style.Parse(style));
        }

        /// <summary>
        /// Enables or disables immediate rendering after status changes.
        /// </summary>
        /// <param name="enabled"><c>true</c> to render changes immediately; otherwise, <c>false</c>.</param>
        /// <returns>The current status scope.</returns>
        public StatusScope AutoRefresh(bool enabled)
        {
            ThrowIfDisposed();
            _options.AutoRefresh = enabled;
            return this;
        }

        /// <summary>
        /// Sets the minimum time between rendered status updates.
        /// </summary>
        /// <param name="milliseconds">The refresh rate in milliseconds. Negative values are treated as zero.</param>
        /// <returns>The current status scope.</returns>
        public StatusScope RefreshRate(int milliseconds)
        {
            ThrowIfDisposed();
            int normalized = RefreshRateHelpers.Normalize(milliseconds);
            _options.RefreshRate = normalized;
            _message.RefreshRate = normalized;
            return this;
        }

        /// <summary>
        /// Sets the minimum time between rendered status updates.
        /// </summary>
        /// <param name="interval">The refresh interval. Negative values are treated as zero.</param>
        /// <returns>The current status scope.</returns>
        public StatusScope RefreshRate(TimeSpan interval)
        {
            return RefreshRate(RefreshRateHelpers.Normalize(interval));
        }

        /// <summary>
        /// Marks the status as successful.
        /// </summary>
        /// <param name="message">Optional final message. When omitted, the current message is reused.</param>
        public void Succeed(string message = null)
        {
            Complete(_options.SuccessPrefix, message, _options.SuccessColor);
        }

        /// <summary>
        /// Marks the status as failed.
        /// </summary>
        /// <param name="message">Optional final message. When omitted, the current message is reused.</param>
        public void Fail(string message = null)
        {
            Complete(_options.FailurePrefix, message, _options.FailureColor);
        }

        /// <summary>
        /// Releases fixed message resources used by the status scope.
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
            ColorScheme scheme = CreateScheme(_activeColorOverride ?? _options.ActiveColor, _activeBackgroundColorOverride);
            _message.RenderSegments(
                MarkupRenderText.ParseSegmentsWithPrefix(GetActivePrefix(), Message, scheme),
                scheme.Color,
                scheme.BackgroundColor);
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
                throw new ObjectDisposedException(nameof(StatusScope));
            }
        }

        private string GetActivePrefix()
        {
            if (!_options.HasSpinnerFrames)
            {
                return _options.ActivePrefix;
            }

            string frame = _options.GetSpinnerFrame(_frameIndex);
            _frameIndex++;
            return (frame ?? string.Empty) + (_options.SpinnerSeparator ?? string.Empty);
        }

        private static ColorScheme CreateScheme(ConsoleColor color, ConsoleColor? backgroundColor = null)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = backgroundColor ?? RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        internal Spinner CurrentSpinner => _spinner;

        internal Style CurrentSpinnerStyle => _spinnerStyle;

        internal StatusScope SetContextStatus(string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            Update(message);
            return this;
        }

        internal StatusScope SetSpinner(Spinner spinner, bool throwOnNull)
        {
            ThrowIfDisposed();
            if (spinner == null && throwOnNull)
            {
                throw new ArgumentNullException(nameof(spinner));
            }

            _spinner = spinner;
            _options.Spinner = spinner;
            _frameIndex = 0;
            if (_options.AutoRefresh)
            {
                RenderActive();
            }

            return this;
        }

        internal StatusScope SetSpinnerStyle(Style style)
        {
            ThrowIfDisposed();
            _spinnerStyle = style;
            _options.ActiveColor = style?.ToForegroundColor(_defaultActiveColor) ?? _defaultActiveColor;
            if (_options.AutoRefresh)
            {
                RenderActive();
            }

            return this;
        }
    }
}
