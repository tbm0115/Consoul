namespace ConsoulLibrary
{
    /// <summary>
    /// Renders command-line help for a <see cref="CommandApp"/>.
    /// </summary>
    public interface IHelpProvider
    {
        /// <summary>
        /// Writes top-level application help.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="context">The top-level help context.</param>
        void WriteApplicationHelp(IAnsiConsole console, CommandAppHelpContext context);

        /// <summary>
        /// Writes help for a command branch.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="context">The branch help context.</param>
        void WriteBranchHelp(IAnsiConsole console, CommandBranchHelpContext context);

        /// <summary>
        /// Writes help for a command.
        /// </summary>
        /// <param name="console">The console facade to write to.</param>
        /// <param name="context">The command help context.</param>
        void WriteCommandHelp(IAnsiConsole console, CommandHelpContext context);
    }
}
