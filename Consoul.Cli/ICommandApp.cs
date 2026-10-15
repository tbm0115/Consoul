using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a command-line application runner.
    /// </summary>
    public interface ICommandApp
    {
        /// <summary>
        /// Configures the command app.
        /// </summary>
        /// <param name="configure">The configuration callback.</param>
        void Configure(Action<CommandAppConfigurator> configure);

        /// <summary>
        /// Runs the command app.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>The process-style exit code.</returns>
        int Run(string[] args);

        /// <summary>
        /// Runs the command app.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>The process-style exit code.</returns>
        int Run(IEnumerable<string> args);

        /// <summary>
        /// Runs the command app asynchronously.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that resolves to the process-style exit code.</returns>
        Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default);

        /// <summary>
        /// Runs the command app asynchronously.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that resolves to the process-style exit code.</returns>
        Task<int> RunAsync(IEnumerable<string> args, CancellationToken cancellationToken = default);

    }
}
