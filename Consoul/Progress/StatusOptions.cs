using System;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for a status message scope.
    /// </summary>
    public class StatusOptions
    {
        private string[] _spinnerFrames;
        private Spinner _spinner;

        /// <summary>
        /// Gets or sets the maximum status message width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? MaxWidth { get; set; }

        /// <summary>
        /// Gets or sets the maximum status message height.
        /// </summary>
        public int Height { get; set; } = 1;

        /// <summary>
        /// Gets or sets the minimum time between rendered updates in milliseconds.
        /// </summary>
        public int RefreshRate { get; set; } = 0;

        /// <summary>
        /// Gets or sets a value indicating whether status updates render immediately.
        /// </summary>
        public bool AutoRefresh { get; set; } = true;

        /// <summary>
        /// Gets or sets the prefix rendered before active status messages.
        /// </summary>
        public string ActivePrefix { get; set; } = "... ";

        /// <summary>
        /// Gets or sets spinner frames that replace <see cref="ActivePrefix"/> for active status messages.
        /// </summary>
        public string[] SpinnerFrames
        {
            get => _spinnerFrames?.ToArray();
            set
            {
                _spinner = null;
                _spinnerFrames = value == null || value.Length == 0
                    ? null
                    : value.Select(frame => frame ?? string.Empty).ToArray();
            }
        }

        /// <summary>
        /// Gets or sets the named spinner used for active status messages.
        /// </summary>
        public Spinner Spinner
        {
            get => _spinner;
            set
            {
                _spinner = value;
                _spinnerFrames = value?.ToFrameArray();
            }
        }

        /// <summary>
        /// Gets or sets the text rendered between a spinner frame and the active status message.
        /// </summary>
        public string SpinnerSeparator { get; set; } = " ";

        /// <summary>
        /// Gets or sets the prefix rendered before successful status messages.
        /// </summary>
        public string SuccessPrefix { get; set; } = "[OK] ";

        /// <summary>
        /// Gets or sets the prefix rendered before failed status messages.
        /// </summary>
        public string FailurePrefix { get; set; } = "[FAIL] ";

        /// <summary>
        /// Gets or sets the color used for active status messages.
        /// </summary>
        public ConsoleColor ActiveColor { get; set; } = RenderOptions.SubnoteColor;

        /// <summary>
        /// Gets or sets the optional reusable style used for active status messages on ANSI-capable terminals.
        /// </summary>
        public Style ActiveStyle { get; set; }

        /// <summary>
        /// Gets or sets the color used for successful status messages.
        /// </summary>
        public ConsoleColor SuccessColor { get; set; } = ConsoleColor.Green;

        /// <summary>
        /// Gets or sets the color used for failed status messages.
        /// </summary>
        public ConsoleColor FailureColor { get; set; } = RenderOptions.InvalidColor;

        /// <summary>
        /// Creates a copy of the current status options.
        /// </summary>
        /// <returns>A new <see cref="StatusOptions"/> instance with the same values.</returns>
        public StatusOptions Clone()
        {
            var options = new StatusOptions
            {
                MaxWidth = MaxWidth,
                Height = Height,
                RefreshRate = RefreshRate,
                AutoRefresh = AutoRefresh,
                ActivePrefix = ActivePrefix,
                SpinnerSeparator = SpinnerSeparator,
                SuccessPrefix = SuccessPrefix,
                FailurePrefix = FailurePrefix,
                ActiveColor = ActiveColor,
                ActiveStyle = ActiveStyle,
                SuccessColor = SuccessColor,
                FailureColor = FailureColor
            };

            if (Spinner != null)
            {
                options.Spinner = Spinner;
            }
            else
            {
                options.SpinnerFrames = SpinnerFrames;
            }

            return options;
        }

        internal bool HasSpinnerFrames => _spinnerFrames != null && _spinnerFrames.Length > 0;

        internal string GetSpinnerFrame(int index)
        {
            if (!HasSpinnerFrames)
            {
                return string.Empty;
            }

            int normalizedIndex = Math.Abs(index % _spinnerFrames.Length);
            return _spinnerFrames[normalizedIndex];
        }
    }
}
