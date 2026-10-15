using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides a Spectre-style base class for asynchronous commands without typed settings.
    /// </summary>
    /// <remarks>
    /// Commands derived from this class run through the same lightweight <see cref="CommandApp"/>
    /// execution pipeline as <see cref="IAsyncCommand"/> implementations.
    /// </remarks>
    public abstract class AsyncCommand : IAsyncCommand
    {
        Task<int> IAsyncCommand.ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return ExecuteAsync(context, cancellationToken);
        }

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="context">The command execution context.</param>
        /// <returns>A task that resolves to the process-style exit code.</returns>
        protected virtual Task<int> ExecuteAsync(CommandContext context)
        {
            throw Command.CreateMissingOverrideException(nameof(ExecuteAsync));
        }

        /// <summary>
        /// Executes the command asynchronously with cancellation support.
        /// </summary>
        /// <param name="context">The command execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that resolves to the process-style exit code.</returns>
        protected virtual Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
        {
            return ExecuteAsync(context);
        }
    }

    /// <summary>
    /// Provides a Spectre-style base class for asynchronous commands with strongly typed settings.
    /// </summary>
    /// <typeparam name="TSettings">The command settings type.</typeparam>
    /// <remarks>
    /// This adapter implements <see cref="IAsyncCommand{TSettings}"/> so existing Consoul command registration,
    /// settings binding, validation, and MVC-style console integration continue to apply.
    /// </remarks>
    public abstract class AsyncCommand<TSettings> : IAsyncCommand<TSettings> where TSettings : CommandSettings
    {
        Task<int> IAsyncCommand<TSettings>.ExecuteAsync(CommandContext context, TSettings settings, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return ExecuteAsync(context, settings, cancellationToken);
        }

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="context">The command execution context.</param>
        /// <param name="settings">The bound command settings.</param>
        /// <returns>A task that resolves to the process-style exit code.</returns>
        protected virtual Task<int> ExecuteAsync(CommandContext context, TSettings settings)
        {
            throw Command.CreateMissingOverrideException(nameof(ExecuteAsync));
        }

        /// <summary>
        /// Executes the command asynchronously with cancellation support.
        /// </summary>
        /// <param name="context">The command execution context.</param>
        /// <param name="settings">The bound command settings.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that resolves to the process-style exit code.</returns>
        protected virtual Task<int> ExecuteAsync(CommandContext context, TSettings settings, CancellationToken cancellationToken)
        {
            return ExecuteAsync(context, settings);
        }
    }
}
