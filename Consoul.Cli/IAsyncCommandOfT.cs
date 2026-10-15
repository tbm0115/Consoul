using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents an asynchronous command with strongly-typed settings.
    /// </summary>
    /// <typeparam name="TSettings">The command settings type.</typeparam>
    public interface IAsyncCommand<TSettings> : ICommandLimiter<TSettings> where TSettings : CommandSettings
    {
        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="context">The command execution context.</param>
        /// <param name="settings">The bound command settings.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that resolves to the process-style exit code.</returns>
        Task<int> ExecuteAsync(CommandContext context, TSettings settings, CancellationToken cancellationToken = default);
    }
}
