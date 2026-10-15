namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a synchronous command with strongly-typed settings.
    /// </summary>
    /// <typeparam name="TSettings">The command settings type.</typeparam>
    public interface ICommand<TSettings> : ICommandLimiter<TSettings> where TSettings : CommandSettings
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="context">The command execution context.</param>
        /// <param name="settings">The bound command settings.</param>
        /// <returns>The process-style exit code.</returns>
        int Execute(CommandContext context, TSettings settings);
    }
}
