using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Describes the terminal surface Consoul is currently rendering into.
    /// </summary>
    public sealed class TerminalProfile
    {
        private readonly bool? _supportsColor;
        private readonly TerminalCapabilities _capabilities;

        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalProfile"/> class.
        /// </summary>
        /// <param name="width">The current terminal width in character cells.</param>
        /// <param name="height">The current terminal height in character cells.</param>
        /// <param name="isInputRedirected">Indicates whether input is redirected away from an interactive terminal.</param>
        /// <param name="isOutputRedirected">Indicates whether output is redirected away from an interactive terminal.</param>
        /// <param name="isErrorRedirected">Indicates whether error output is redirected away from an interactive terminal.</param>
        /// <param name="isVirtual">Indicates whether the profile is backed by a virtual Consoul driver such as capture or tests.</param>
        public TerminalProfile(
            int width,
            int height,
            bool isInputRedirected = false,
            bool isOutputRedirected = false,
            bool isErrorRedirected = false,
            bool isVirtual = false)
            : this(width, height, isInputRedirected, isOutputRedirected, isErrorRedirected, isVirtual, null, null)
        {
        }

        internal TerminalProfile(
            int width,
            int height,
            bool isInputRedirected,
            bool isOutputRedirected,
            bool isErrorRedirected,
            bool isVirtual,
            bool? supportsColor,
            TerminalCapabilities capabilities)
        {
            Width = Math.Max(1, width);
            Height = Math.Max(1, height);
            IsInputRedirected = isInputRedirected;
            IsOutputRedirected = isOutputRedirected;
            IsErrorRedirected = isErrorRedirected;
            IsVirtual = isVirtual;
            _supportsColor = supportsColor;
            _capabilities = capabilities;
        }

        /// <summary>
        /// Gets the current terminal width in character cells.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the current terminal height in character cells.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets a value indicating whether input is redirected away from an interactive terminal.
        /// </summary>
        public bool IsInputRedirected { get; }

        /// <summary>
        /// Gets a value indicating whether output is redirected away from an interactive terminal.
        /// </summary>
        public bool IsOutputRedirected { get; }

        /// <summary>
        /// Gets a value indicating whether error output is redirected away from an interactive terminal.
        /// </summary>
        public bool IsErrorRedirected { get; }

        /// <summary>
        /// Gets a value indicating whether Consoul is rendering through a virtual console driver.
        /// </summary>
        public bool IsVirtual { get; }

        /// <summary>
        /// Gets a value indicating whether both input and output are connected to an interactive terminal.
        /// </summary>
        public bool IsInteractive => !IsInputRedirected && !IsOutputRedirected;

        /// <summary>
        /// Gets a value indicating whether Consoul can safely retain color metadata for the current output path.
        /// </summary>
        public bool SupportsColor => _supportsColor ?? (IsVirtual || !IsOutputRedirected);

        /// <summary>
        /// Gets Spectre-style terminal capability metadata for the current profile.
        /// </summary>
        public TerminalCapabilities Capabilities => _capabilities ?? TerminalCapabilities.FromProfile(this);

        /// <summary>
        /// Determines whether the terminal is narrower than the supplied breakpoint.
        /// </summary>
        /// <param name="breakpoint">Width breakpoint to compare against.</param>
        /// <returns><c>true</c> when <see cref="Width"/> is less than <paramref name="breakpoint"/>.</returns>
        public bool IsNarrow(int breakpoint = 80)
        {
            return Width < Math.Max(1, breakpoint);
        }

        /// <summary>
        /// Determines whether the terminal is at least as wide as the supplied breakpoint.
        /// </summary>
        /// <param name="breakpoint">Width breakpoint to compare against.</param>
        /// <returns><c>true</c> when <see cref="Width"/> is greater than or equal to <paramref name="breakpoint"/>.</returns>
        public bool IsWide(int breakpoint = 120)
        {
            return Width >= Math.Max(1, breakpoint);
        }

        /// <summary>
        /// Resolves a requested render width to a value that fits inside the active terminal.
        /// </summary>
        /// <param name="requestedWidth">Optional requested width. When <see langword="null"/> or less than one, the terminal width is used.</param>
        /// <param name="minimumWidth">Preferred minimum width. If the terminal is narrower than this value, the terminal width wins.</param>
        /// <returns>A positive width no larger than <see cref="Width"/>.</returns>
        public int ResolveWidth(int? requestedWidth = null, int minimumWidth = 1)
        {
            int terminalWidth = Math.Max(1, Width);
            int minimum = Math.Max(1, minimumWidth);
            int requested = requestedWidth.HasValue && requestedWidth.Value > 0
                ? requestedWidth.Value
                : terminalWidth;

            if (minimum > terminalWidth)
            {
                minimum = terminalWidth;
            }

            return Math.Max(minimum, Math.Min(requested, terminalWidth));
        }

        /// <summary>
        /// Returns a compact representation of the terminal profile.
        /// </summary>
        /// <returns>A compact terminal profile string.</returns>
        public override string ToString()
        {
            string mode = IsVirtual ? "virtual" : IsInteractive ? "interactive" : "redirected";
            return Width + "x" + Height + " " + mode;
        }

        internal static TerminalProfile FromDriver(IConsoleDriver driver)
        {
            if (driver == null)
            {
                throw new ArgumentNullException(nameof(driver));
            }

            var profiled = driver as IConsoleDriverProfileProvider;
            if (profiled != null)
            {
                return profiled.Profile;
            }

            return new TerminalProfile(
                driver.BufferWidth,
                driver.BufferHeight,
                driver.IsInputRedirected,
                driver.IsOutputRedirected,
                driver.IsErrorRedirected,
                driver.IsVirtual);
        }
    }

    internal interface IConsoleDriverProfileProvider
    {
        TerminalProfile Profile { get; }
    }
}
