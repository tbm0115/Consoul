using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents an asynchronous command that can be run by <see cref="CommandApp"/>.
    /// </summary>
    public interface IAsyncCommand : ICommandLimiter<EmptyCommandSettings>
    {
        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="context">The command execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that resolves to the process-style exit code.</returns>
        Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default);
    }
}
