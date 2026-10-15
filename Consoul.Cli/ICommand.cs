namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a synchronous command that can be run by <see cref="CommandApp"/>.
    /// </summary>
    public interface ICommand : ICommandLimiter<EmptyCommandSettings>
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="context">The command execution context.</param>
        /// <returns>The process-style exit code.</returns>
        int Execute(CommandContext context);
    }
}
