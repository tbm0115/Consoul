using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Specifies migration-friendly exception formatting options for <see cref="AnsiConsole.WriteException(Exception, ExceptionFormats)"/>.
    /// </summary>
    [Flags]
    public enum ExceptionFormats
    {
        /// <summary>
        /// Uses Consoul's default exception rendering behavior.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Hides stack trace lines.
        /// </summary>
        NoStackTrace = 1,

        /// <summary>
        /// Shortens file paths in stack trace frames to their file names.
        /// </summary>
        ShortenPaths = 2,

        /// <summary>
        /// Shortens exception type names to their simple names.
        /// </summary>
        ShortenTypes = 4,

        /// <summary>
        /// Shortens method names in stack trace frames.
        /// </summary>
        ShortenMethods = 8,

        /// <summary>
        /// Accepts source-link intent from Spectre-style snippets. Consoul keeps this flag for compatibility even when the active terminal cannot render links.
        /// </summary>
        ShowLinks = 16,

        /// <summary>
        /// Shortens paths, exception type names, and method names.
        /// </summary>
        ShortenEverything = ShortenPaths | ShortenTypes | ShortenMethods
    }
}
