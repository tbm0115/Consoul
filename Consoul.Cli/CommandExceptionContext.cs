using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides exception metadata to a configured <see cref="ICommandExceptionHandler"/>.
    /// </summary>
    public sealed class CommandExceptionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandExceptionContext"/> class.
        /// </summary>
        /// <param name="exception">The exception raised while running the command app.</param>
        /// <param name="arguments">The original application argument list.</param>
        /// <param name="console">The console facade used by the command app.</param>
        /// <param name="settings">The command app settings active for this run.</param>
        internal CommandExceptionContext(
            Exception exception,
            IEnumerable<string> arguments,
            IAnsiConsole console,
            CommandAppSettings settings)
        {
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
            Arguments = (arguments ?? Enumerable.Empty<string>()).ToArray();
            Console = console ?? throw new ArgumentNullException(nameof(console));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// Gets the exception raised while running the command app.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Gets the original application argument list.
        /// </summary>
        public IReadOnlyList<string> Arguments { get; }

        /// <summary>
        /// Gets the console facade used by the command app.
        /// </summary>
        public IAnsiConsole Console { get; }

        /// <summary>
        /// Gets the command app settings active for this run.
        /// </summary>
        public CommandAppSettings Settings { get; }
    }
}
