using System;
using System.Globalization;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Maintains a fixed positioned progress bar with an optional message and block representing progress.
    /// </summary>
    public class ProgressBar : IDisposable
    {
        private FixedMessage _messageArea;
        private FixedMessage _barArea;
        private double _progress = 0.0;
        private int _barWidth = Math.Max(1, Consoul.ConsoleBufferWidth - 10);
        private int _refreshRate = 1000 / 60;

        /// <summary>
        /// Gets or sets the preferred maximum number of characters used for the progress bar itself.
        /// </summary>
        /// <remarks>The actual rendered width is clamped to the current terminal width on each update.</remarks>
        public int BarWidth
        {
            get => _barWidth;
            set => _barWidth = Math.Max(1, value);
        }

        /// <summary>
        /// The character that is used to render each 'tick' in the progress bar.
        /// </summary>
        public char BlockCharacter { get; set; } = (char)0x2588;

        /// <summary>
        /// Gets or sets the character used for the unfilled portion of the bar when <see cref="ShowTrack"/> is enabled.
        /// </summary>
        public char EmptyCharacter { get; set; } = ' ';

        /// <summary>
        /// Gets or sets a value indicating whether the unfilled portion of the bar should be rendered.
        /// </summary>
        public bool ShowTrack { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a percentage should be rendered after the bar when space allows.
        /// </summary>
        public bool ShowPercentage { get; set; }

        /// <summary>
        /// Gets or sets the minimum time between rendered updates in milliseconds.
        /// </summary>
        public int RefreshRate
        {
            get => _refreshRate;
            set
            {
                _refreshRate = Math.Max(0, value);
                if (_messageArea != null)
                {
                    _messageArea.RefreshRate = _refreshRate;
                }

                if (_barArea != null)
                {
                    _barArea.RefreshRate = _refreshRate;
                }
            }
        }

        /// <summary>
        /// Current progress value between 0.0 and 1.0.
        /// </summary>
        public double Progress => _progress;

        /// <summary>
        /// Constructs a new progress bar with an optional initial message.
        /// </summary>
        /// <param name="initialMessage">Optional initial message to display above the progress bar</param>
        public ProgressBar(string initialMessage = "")
        {
            Initialize(initialMessage);
        }

        /// <summary>
        /// Initializes the fixed position of the progress bar.
        /// </summary>
        private void Initialize(string initialMessage)
        {
            _messageArea = new FixedMessage
            {
                RefreshRate = RefreshRate
            };
            RenderMessage(initialMessage, CreateStyle(RenderOptions.DefaultColor));
            Consoul.LineBreak();
            _barArea = new FixedMessage
            {
                RefreshRate = RefreshRate
            };
            _barArea.Render(string.Empty);
            Consoul.LineBreak();
        }

        /// <summary>
        /// Resets the progress of the rendered bar.
        /// </summary>
        /// <param name="reAffix">If <c>true</c>, resets the fixed position</param>
        public void Reset(bool reAffix = false)
        {
            _progress = 0.0;
            if (reAffix)
            {
                _messageArea?.Dispose();
                _barArea?.Dispose();
                Initialize(string.Empty);
            }
            else
            {
                Update(0.0, "", ConsoleColor.Gray, ConsoleColor.Gray);
            }
        }

        /// <summary>
        /// Updates the progress bar with a new progress value and optional message.
        /// </summary>
        /// <param name="progress">Progress value as a value between 0.0 and 1.0</param>
        /// <param name="message">Optional message to display above the progress bar</param>
        /// <param name="messageColor">Optional color for the message text</param>
        /// <param name="barColor">Optional color for the progress bar</param>
        public void Update(double progress, string message = null, ConsoleColor? messageColor = null, ConsoleColor? barColor = null)
        {
            UpdateCore(
                progress,
                message,
                CreateStyle(messageColor ?? RenderOptions.DefaultColor),
                CreateStyle(barColor ?? RenderOptions.SubnoteColor));
        }

        /// <summary>
        /// Updates the progress bar with a Spectre-style color facade for the message text.
        /// </summary>
        /// <param name="progress">Progress value as a value between 0.0 and 1.0.</param>
        /// <param name="message">Optional message to display above the progress bar.</param>
        /// <param name="messageColor">Color facade used for the message text.</param>
        public void Update(double progress, string message, SpectreColor messageColor)
        {
            UpdateCore(
                progress,
                message,
                ResolveStyle(new Style(messageColor), RenderOptions.DefaultColor),
                CreateStyle(RenderOptions.SubnoteColor));
        }

        /// <summary>
        /// Updates the progress bar with RGB color mapped to the nearest console color for the message text.
        /// </summary>
        /// <param name="progress">Progress value as a value between 0.0 and 1.0.</param>
        /// <param name="message">Optional message to display above the progress bar.</param>
        /// <param name="messageColor">RGB color used for the message text.</param>
        public void Update(double progress, string message, RgbColor messageColor)
        {
            UpdateCore(
                progress,
                message,
                ResolveStyle(new Style(messageColor), RenderOptions.DefaultColor),
                CreateStyle(RenderOptions.SubnoteColor));
        }

        /// <summary>
        /// Updates the progress bar with Spectre-style color facades for the message and bar text.
        /// </summary>
        /// <param name="progress">Progress value as a value between 0.0 and 1.0.</param>
        /// <param name="message">Optional message to display above the progress bar.</param>
        /// <param name="messageColor">Color facade used for the message text.</param>
        /// <param name="barColor">Color facade used for the progress bar.</param>
        public void Update(double progress, string message, SpectreColor messageColor, SpectreColor barColor)
        {
            UpdateCore(
                progress,
                message,
                ResolveStyle(new Style(messageColor), RenderOptions.DefaultColor),
                ResolveStyle(new Style(barColor), RenderOptions.SubnoteColor));
        }

        /// <summary>
        /// Updates the progress bar with RGB colors mapped to the nearest console colors for the message and bar text.
        /// </summary>
        /// <param name="progress">Progress value as a value between 0.0 and 1.0.</param>
        /// <param name="message">Optional message to display above the progress bar.</param>
        /// <param name="messageColor">RGB color used for the message text.</param>
        /// <param name="barColor">RGB color used for the progress bar.</param>
        public void Update(double progress, string message, RgbColor messageColor, RgbColor barColor)
        {
            UpdateCore(
                progress,
                message,
                ResolveStyle(new Style(messageColor), RenderOptions.DefaultColor),
                ResolveStyle(new Style(barColor), RenderOptions.SubnoteColor));
        }

        /// <summary>
        /// Updates the progress bar with a reusable style for the message and bar.
        /// </summary>
        /// <param name="progress">Progress value as a value between 0.0 and 1.0.</param>
        /// <param name="message">Optional message to display above the progress bar.</param>
        /// <param name="messageStyle">Optional style applied to the message text.</param>
        /// <param name="barStyle">Optional style applied to the progress bar text.</param>
        public void UpdateStyled(double progress, string message = null, Style messageStyle = null, Style barStyle = null)
        {
            UpdateCore(
                progress,
                message,
                ResolveStyle(messageStyle, RenderOptions.DefaultColor),
                ResolveStyle(barStyle, RenderOptions.SubnoteColor));
        }

        private void UpdateCore(double progress, string message, StyledText messageStyle, StyledText barStyle)
        {
            _progress = NormalizeProgress(progress);

            RenderMessage(message, messageStyle);
            RenderBar(FormatBar(_progress), barStyle);
        }

        private void RenderMessage(string message, StyledText style)
        {
            _messageArea.RenderSegments(
                MarkupRenderText.ParseSegments(
                    message ?? string.Empty,
                    style.Scheme,
                    style.Decoration,
                    style.ForegroundRgb,
                    style.BackgroundRgb),
                style.Scheme.Color,
                style.Scheme.BackgroundColor);
        }

        private void RenderBar(string bar, StyledText style)
        {
            _barArea.RenderSegments(
                new[]
                {
                    new FixedMessageSegment(
                        bar,
                        style.Scheme,
                        style.Decoration,
                        null,
                        style.ForegroundRgb,
                        style.BackgroundRgb)
                },
                style.Scheme.Color,
                style.Scheme.BackgroundColor);
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

        private static ColorScheme CreateScheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        private string FormatBar(double progress)
        {
            int terminalWidth = Math.Max(1, Consoul.ConsoleBufferWidth - 1);
            string percentage = ShowPercentage ? FormatPercentage(progress) : string.Empty;
            string suffix = string.IsNullOrEmpty(percentage) ? string.Empty : " " + percentage;
            bool renderPercentage = suffix.Length > 0 && suffix.Length < terminalWidth;
            int availableBarWidth = terminalWidth - (renderPercentage ? suffix.Length : 0);
            int barWidth = Math.Max(1, Math.Min(BarWidth, availableBarWidth));
            int filledWidth = progress <= 0.0 ? 0 : (int)(progress * barWidth);
            filledWidth = Math.Max(0, Math.Min(barWidth, filledWidth));

            string text = new string(BlockCharacter, filledWidth);
            if (ShowTrack)
            {
                text += new string(EmptyCharacter, barWidth - filledWidth);
            }

            if (renderPercentage)
            {
                text += suffix;
            }

            return text.Length <= terminalWidth ? text : text.Substring(0, terminalWidth);
        }

        private static double NormalizeProgress(double progress)
        {
            if (double.IsNaN(progress) || double.IsNegativeInfinity(progress) || progress < 0.0)
            {
                return 0.0;
            }

            if (double.IsPositiveInfinity(progress) || progress > 1.0)
            {
                return 1.0;
            }

            return progress;
        }

        private static string FormatPercentage(double progress)
        {
            int percentage = (int)Math.Round(progress * 100, MidpointRounding.AwayFromZero);
            return (percentage.ToString(CultureInfo.InvariantCulture) + "%").PadLeft(4);
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

        /// <summary>
        /// Releases fixed message resources used by the progress bar.
        /// </summary>
        public void Dispose()
        {
            _messageArea?.Dispose();
            _barArea?.Dispose();
        }
    }
}
