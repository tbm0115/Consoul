namespace ConsoulLibrary
{
    /// <summary>
    /// Renders command app exceptions and chooses the resulting process-style exit code.
    /// </summary>
    public interface ICommandExceptionHandler
    {
        /// <summary>
        /// Handles an exception raised while running a command app.
        /// </summary>
        /// <param name="context">The exception context.</param>
        /// <returns>The process-style exit code.</returns>
        int Handle(CommandExceptionContext context);
    }
}
