using System;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for a spinner scope.
    /// </summary>
    public class SpinnerOptions
    {
        private static readonly string[] DefaultFrames = new[] { "|", "/", "-", "\\" };
        private string[] _frames = DefaultFrames;

        /// <summary>
        /// Gets or sets the maximum spinner message width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? MaxWidth { get; set; }

        /// <summary>
        /// Gets or sets the maximum spinner message height.
        /// </summary>
        public int Height { get; set; } = 1;

        /// <summary>
        /// Gets or sets the minimum time between rendered updates in milliseconds.
        /// </summary>
        public int RefreshRate { get; set; } = 0;

        /// <summary>
        /// Gets or sets a value indicating whether spinner updates render immediately.
        /// </summary>
        public bool AutoRefresh { get; set; } = true;

        /// <summary>
        /// Gets or sets the spinner frames rendered by successive ticks.
        /// </summary>
        public string[] Frames
        {
            get => _frames.ToArray();
            set
            {
                _frames = value == null || value.Length == 0
                    ? DefaultFrames
                    : value.Select(frame => frame ?? string.Empty).ToArray();
            }
        }

        /// <summary>
        /// Gets or sets the text rendered between the spinner frame and message.
        /// </summary>
        public string MessageSeparator { get; set; } = " ";

        /// <summary>
        /// Gets or sets the prefix rendered before successful final messages.
        /// </summary>
        public string SuccessPrefix { get; set; } = "[OK] ";

        /// <summary>
        /// Gets or sets the prefix rendered before failed final messages.
        /// </summary>
        public string FailurePrefix { get; set; } = "[FAIL] ";

        /// <summary>
        /// Gets or sets the color used for active spinner messages.
        /// </summary>
        public ConsoleColor ActiveColor { get; set; } = RenderOptions.SubnoteColor;

        /// <summary>
        /// Gets or sets the color used for successful final messages.
        /// </summary>
        public ConsoleColor SuccessColor { get; set; } = ConsoleColor.Green;

        /// <summary>
        /// Gets or sets the color used for failed final messages.
        /// </summary>
        public ConsoleColor FailureColor { get; set; } = RenderOptions.InvalidColor;

        /// <summary>
        /// Creates a copy of the current spinner options.
        /// </summary>
        /// <returns>A new <see cref="SpinnerOptions"/> instance with the same values.</returns>
        public SpinnerOptions Clone()
        {
            return new SpinnerOptions
            {
                MaxWidth = MaxWidth,
                Height = Height,
                RefreshRate = RefreshRate,
                AutoRefresh = AutoRefresh,
                Frames = Frames,
                MessageSeparator = MessageSeparator,
                SuccessPrefix = SuccessPrefix,
                FailurePrefix = FailurePrefix,
                ActiveColor = ActiveColor,
                SuccessColor = SuccessColor,
                FailureColor = FailureColor
            };
        }

        internal string GetFrame(int index)
        {
            if (_frames.Length == 0)
            {
                return string.Empty;
            }

            int normalizedIndex = Math.Abs(index % _frames.Length);
            return _frames[normalizedIndex];
        }
    }
}
