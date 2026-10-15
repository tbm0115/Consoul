using System;
using System.Threading;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides a Spectre-style base class for synchronous commands without typed settings.
    /// </summary>
    /// <remarks>
    /// Commands derived from this class still run through Consoul's lightweight <see cref="CommandApp"/>
    /// execution pipeline, including interceptors, exception handling, and <see cref="CommandContext"/>
    /// console access.
    /// </remarks>
    public abstract class Command : ICommand
    {
        int ICommand.Execute(CommandContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return Execute(context, context.CancellationToken);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="context">The command execution context.</param>
        /// <returns>The process-style exit code.</returns>
        protected virtual int Execute(CommandContext context)
        {
            throw CreateMissingOverrideException(nameof(Execute));
        }

        /// <summary>
        /// Executes the command with cancellation support.
        /// </summary>
        /// <param name="context">The command execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The process-style exit code.</returns>
        protected virtual int Execute(CommandContext context, CancellationToken cancellationToken)
        {
            return Execute(context);
        }

        internal static CommandRuntimeException CreateMissingOverrideException(string methodName)
        {
            return new CommandRuntimeException(
                "Command base classes require overriding " + methodName + " in the derived command type.");
        }
    }

    /// <summary>
    /// Provides a Spectre-style base class for synchronous commands with strongly typed settings.
    /// </summary>
    /// <typeparam name="TSettings">The command settings type.</typeparam>
    /// <remarks>
    /// This adapter implements <see cref="ICommand{TSettings}"/> so existing Consoul command registration,
    /// settings binding, validation, and MVC-style console integration continue to apply.
    /// </remarks>
    public abstract class Command<TSettings> : ICommand<TSettings> where TSettings : CommandSettings
    {
        int ICommand<TSettings>.Execute(CommandContext context, TSettings settings)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return Execute(context, settings, context.CancellationToken);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="context">The command execution context.</param>
        /// <param name="settings">The bound command settings.</param>
        /// <returns>The process-style exit code.</returns>
        protected virtual int Execute(CommandContext context, TSettings settings)
        {
            throw Command.CreateMissingOverrideException(nameof(Execute));
        }

        /// <summary>
        /// Executes the command with cancellation support.
        /// </summary>
        /// <param name="context">The command execution context.</param>
        /// <param name="settings">The bound command settings.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The process-style exit code.</returns>
        protected virtual int Execute(CommandContext context, TSettings settings, CancellationToken cancellationToken)
        {
            return Execute(context, settings);
        }
    }
}
