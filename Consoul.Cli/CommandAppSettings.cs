using System.Globalization;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines runtime and help settings for <see cref="CommandApp"/>.
    /// </summary>
    public sealed class CommandAppSettings : ICommandAppSettings
    {
        /// <summary>
        /// Gets or sets the application name shown in help output.
        /// </summary>
        public string ApplicationName { get; set; } = "app";

        /// <summary>
        /// Gets or sets the application version shown in help output.
        /// </summary>
        public string ApplicationVersion { get; set; }

        /// <summary>
        /// Gets or sets the application description shown in help output.
        /// </summary>
        public string ApplicationDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether command lookup should be case-sensitive.
        /// </summary>
        public bool CaseSensitive { get; set; }

        /// <summary>
        /// Gets or sets the culture used when binding command setting values.
        /// </summary>
        public CultureInfo ApplicationCulture { get; set; } = CultureInfo.InvariantCulture;

        /// <summary>
        /// Gets or sets Spectre-style case-sensitivity behavior for command names, branches, aliases, and options.
        /// </summary>
        public CaseSensitivity CaseSensitivity
        {
            get => CaseSensitive ? ConsoulLibrary.CaseSensitivity.All : ConsoulLibrary.CaseSensitivity.None;
            set => CaseSensitive = value != ConsoulLibrary.CaseSensitivity.None;
        }

        /// <summary>
        /// Gets or sets a value indicating whether command exceptions should be rethrown instead of rendered as errors.
        /// </summary>
        public bool PropagateExceptions { get; set; }

        /// <summary>
        /// Gets or sets the exit code returned when command execution is canceled.
        /// </summary>
        public int CancellationExitCode { get; set; } = 130;

        /// <summary>
        /// Gets or sets a value indicating whether option default values should be hidden in help output.
        /// </summary>
        public bool HideOptionDefaultValues { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether help descriptions should trim trailing periods.
        /// </summary>
        public bool TrimTrailingPeriods { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether registered examples should be validated before commands run.
        /// </summary>
        public bool ValidateExamples { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether future argument parsers should reject unknown options.
        /// </summary>
        public bool StrictParsing { get; set; } = true;
    }
}
