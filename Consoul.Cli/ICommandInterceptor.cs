namespace ConsoulLibrary
{
    /// <summary>
    /// Runs cross-cutting logic before and after command execution.
    /// </summary>
    public interface ICommandInterceptor
    {
        /// <summary>
        /// Runs before the command handler executes.
        /// </summary>
        /// <param name="context">The command execution context.</param>
        /// <param name="settings">The bound command settings, or <c>null</c> when the command does not use typed settings.</param>
        void Intercept(CommandContext context, CommandSettings settings);

        /// <summary>
        /// Runs after the command handler executes and can adjust the exit code.
        /// </summary>
        /// <param name="context">The command execution context.</param>
        /// <param name="settings">The bound command settings, or <c>null</c> when the command does not use typed settings.</param>
        /// <param name="result">The process-style exit code.</param>
        void InterceptResult(CommandContext context, CommandSettings settings, ref int result);
    }
}
