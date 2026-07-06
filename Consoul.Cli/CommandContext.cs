using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides command metadata, arguments, and console access during command execution.
    /// </summary>
    public sealed class CommandContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandContext"/> class.
        /// </summary>
        /// <param name="commandName">The command name used to invoke the command.</param>
        /// <param name="arguments">The arguments passed to the command after the command name.</param>
        /// <param name="rawArguments">The original application argument list.</param>
        /// <param name="console">The console facade used by the command app.</param>
        public CommandContext(string commandName, IEnumerable<string> arguments, IEnumerable<string> rawArguments, IAnsiConsole console)
            : this(commandName, arguments, rawArguments, console, default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandContext"/> class.
        /// </summary>
        /// <param name="commandName">The command name used to invoke the command.</param>
        /// <param name="arguments">The arguments passed to the command after the command name.</param>
        /// <param name="rawArguments">The original application argument list.</param>
        /// <param name="console">The console facade used by the command app.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        public CommandContext(string commandName, IEnumerable<string> arguments, IEnumerable<string> rawArguments, IAnsiConsole console, CancellationToken cancellationToken)
        {
            CommandName = commandName ?? string.Empty;
            Arguments = (arguments ?? Enumerable.Empty<string>()).ToArray();
            Remaining = Arguments;
            RawArguments = (rawArguments ?? Enumerable.Empty<string>()).ToArray();
            Console = console ?? throw new ArgumentNullException(nameof(console));
            CancellationToken = cancellationToken;
            Data = new Dictionary<string, object>(StringComparer.Ordinal);
        }

        /// <summary>
        /// Gets the command name used to invoke the command.
        /// </summary>
        public string CommandName { get; }

        /// <summary>
        /// Gets the command name used to invoke the command.
        /// </summary>
        public string Name => CommandName;

        /// <summary>
        /// Gets the arguments passed to the command after the command name.
        /// </summary>
        public IReadOnlyList<string> Arguments { get; }

        /// <summary>
        /// Gets the remaining arguments passed to the command after the command name.
        /// </summary>
        public IReadOnlyList<string> Remaining { get; }

        /// <summary>
        /// Gets the original application argument list.
        /// </summary>
        public IReadOnlyList<string> RawArguments { get; }

        /// <summary>
        /// Gets the console facade used by the command app.
        /// </summary>
        public IAnsiConsole Console { get; }

        /// <summary>
        /// Gets the cancellation token for the current command execution.
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// Gets a per-command data bag shared by interceptors and the command handler.
        /// </summary>
        public IDictionary<string, object> Data { get; }
    }
}
