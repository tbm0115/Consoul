using System.Collections.Generic;
using System.IO;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines Spectre-style settings used when creating an <see cref="IAnsiConsole"/> facade.
    /// </summary>
    public sealed class AnsiConsoleSettings
    {
        /// <summary>
        /// Gets or sets the ANSI escape sequence support preference.
        /// </summary>
        public AnsiSupport Ansi { get; set; } = AnsiSupport.Detect;

        /// <summary>
        /// Gets or sets the color-system support preference.
        /// </summary>
        public ColorSystemSupport ColorSystem { get; set; } = ColorSystemSupport.Detect;

        /// <summary>
        /// Gets or sets the output target for created console instances.
        /// </summary>
        public IAnsiConsoleOutput Out { get; set; }

        /// <summary>
        /// Gets or sets the input source for created console instances.
        /// </summary>
        /// <remarks>When unset, the created console has no input. Set <see cref="System.Console.In"/> for standard input. Built-in prompts treat end-of-input as cancellation; a blank line still participates in validation or default-value selection.</remarks>
        public TextReader In { get; set; }

        /// <summary>
        /// Gets or sets the interactive terminal support preference.
        /// </summary>
        public InteractionSupport Interactive { get; set; } = InteractionSupport.Detect;

        /// <summary>
        /// Gets or sets an optional exclusivity-mode object preserved for source-compatible settings initialization.
        /// </summary>
        public object ExclusivityMode { get; set; }

        /// <summary>
        /// Gets or sets profile enrichment options preserved for source-compatible settings initialization.
        /// </summary>
        public ProfileEnrichment Enrichment { get; set; } = new ProfileEnrichment();

        /// <summary>
        /// Gets or sets optional environment variables preserved for source-compatible settings initialization.
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; set; }
    }

    /// <summary>
    /// Indicates whether ANSI escape sequences should be used.
    /// </summary>
    public enum AnsiSupport
    {
        /// <summary>
        /// Detect ANSI support from the active terminal.
        /// </summary>
        Detect,

        /// <summary>
        /// Treat ANSI escape sequences as supported.
        /// </summary>
        Yes,

        /// <summary>
        /// Treat ANSI escape sequences as unsupported.
        /// </summary>
        No
    }

    /// <summary>
    /// Indicates the requested color-system support level.
    /// </summary>
    public enum ColorSystemSupport
    {
        /// <summary>
        /// Detect color support from the active terminal.
        /// </summary>
        Detect,

        /// <summary>
        /// Disable color output.
        /// </summary>
        NoColors,

        /// <summary>
        /// Use legacy terminal colors.
        /// </summary>
        Legacy,

        /// <summary>
        /// Use the standard 16-color console palette.
        /// </summary>
        Standard,

        /// <summary>
        /// Use 8-bit color where available.
        /// </summary>
        EightBit,

        /// <summary>
        /// Use true color where available.
        /// </summary>
        TrueColor
    }

    /// <summary>
    /// Indicates whether interactive terminal behavior should be enabled.
    /// </summary>
    public enum InteractionSupport
    {
        /// <summary>
        /// Detect interaction support from the active terminal.
        /// </summary>
        Detect,

        /// <summary>
        /// Treat the terminal as interactive.
        /// </summary>
        Yes,

        /// <summary>
        /// Treat the terminal as non-interactive.
        /// </summary>
        No
    }

    /// <summary>
    /// Provides source-compatible profile enrichment settings for created console facades.
    /// </summary>
    public sealed class ProfileEnrichment
    {
        /// <summary>
        /// Gets or sets whether terminal environment data should be considered.
        /// </summary>
        public bool UseEnvironment { get; set; } = true;
    }
}
