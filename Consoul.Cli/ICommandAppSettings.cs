using System.Globalization;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines runtime and help settings for a command-line application.
    /// </summary>
    public interface ICommandAppSettings
    {
        /// <summary>
        /// Gets or sets the application name shown in help output.
        /// </summary>
        string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets the application version shown in help output.
        /// </summary>
        string ApplicationVersion { get; set; }

        /// <summary>
        /// Gets or sets the application description shown in help output.
        /// </summary>
        string ApplicationDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether command lookup should be case-sensitive.
        /// </summary>
        bool CaseSensitive { get; set; }

        /// <summary>
        /// Gets or sets the culture used when binding command setting values.
        /// </summary>
        CultureInfo ApplicationCulture { get; set; }

        /// <summary>
        /// Gets or sets Spectre-style case-sensitivity behavior for command names, branches, aliases, and options.
        /// </summary>
        CaseSensitivity CaseSensitivity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether command exceptions should be rethrown instead of rendered as errors.
        /// </summary>
        bool PropagateExceptions { get; set; }

        /// <summary>
        /// Gets or sets the exit code returned when command execution is canceled.
        /// </summary>
        int CancellationExitCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether option default values should be hidden in help output.
        /// </summary>
        bool HideOptionDefaultValues { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether help descriptions should trim trailing periods.
        /// </summary>
        bool TrimTrailingPeriods { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether registered examples should be validated before commands run.
        /// </summary>
        bool ValidateExamples { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether unknown options should be rejected.
        /// </summary>
        bool StrictParsing { get; set; }
    }
}
