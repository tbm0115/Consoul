namespace ConsoulLibrary
{
    /// <summary>
    /// Describes terminal capabilities inferred from a <see cref="TerminalProfile"/>.
    /// </summary>
    public sealed class TerminalCapabilities
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalCapabilities"/> class.
        /// </summary>
        /// <param name="ansi">Indicates whether ANSI escape sequences are supported.</param>
        /// <param name="unicode">Indicates whether Unicode glyphs are supported.</param>
        /// <param name="links">Indicates whether hyperlink markup can be preserved.</param>
        /// <param name="interactive">Indicates whether input and output are interactive.</param>
        /// <param name="legacy">Indicates whether the terminal should be treated as a legacy terminal.</param>
        /// <param name="colorSystem">The color-system level available to renderers.</param>
        public TerminalCapabilities(
            bool ansi,
            bool unicode,
            bool links,
            bool interactive,
            bool legacy,
            ColorSystem colorSystem)
        {
            Ansi = ansi;
            Unicode = unicode;
            Links = links;
            Interactive = interactive;
            Legacy = legacy;
            ColorSystem = colorSystem;
        }

        /// <summary>
        /// Gets a value indicating whether ANSI escape sequences are supported.
        /// </summary>
        public bool Ansi { get; }

        /// <summary>
        /// Gets a value indicating whether Unicode glyphs are supported.
        /// </summary>
        public bool Unicode { get; }

        /// <summary>
        /// Gets a value indicating whether hyperlink markup can be preserved.
        /// </summary>
        public bool Links { get; }

        /// <summary>
        /// Gets a value indicating whether the terminal is interactive.
        /// </summary>
        public bool Interactive { get; }

        /// <summary>
        /// Gets a value indicating whether the terminal should be treated as legacy.
        /// </summary>
        public bool Legacy { get; }

        /// <summary>
        /// Gets the color-system level available to renderers.
        /// </summary>
        public ColorSystem ColorSystem { get; }

        internal static TerminalCapabilities FromProfile(TerminalProfile profile)
        {
            bool ansi = profile.IsVirtual || !profile.IsOutputRedirected;
            bool color = profile.SupportsColor;
            bool unicode = profile.IsVirtual || !profile.IsOutputRedirected;
            bool legacy = !ansi;

            return new TerminalCapabilities(
                ansi,
                unicode,
                links: ansi,
                interactive: profile.IsInteractive,
                legacy: legacy,
                colorSystem: color ? ColorSystem.Standard : ColorSystem.NoColors);
        }
    }

    /// <summary>
    /// Describes the terminal color-system level exposed through Spectre-style profile capabilities.
    /// </summary>
    public enum ColorSystem
    {
        /// <summary>
        /// Indicates that color output is unavailable.
        /// </summary>
        NoColors,

        /// <summary>
        /// Indicates legacy terminal colors.
        /// </summary>
        Legacy,

        /// <summary>
        /// Indicates the standard 16-color console palette.
        /// </summary>
        Standard,

        /// <summary>
        /// Indicates 8-bit color support.
        /// </summary>
        EightBit,

        /// <summary>
        /// Indicates true-color support.
        /// </summary>
        TrueColor
    }
}
