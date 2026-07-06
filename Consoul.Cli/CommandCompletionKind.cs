namespace ConsoulLibrary
{
    /// <summary>
    /// Identifies the kind of command-line completion suggestion returned by <see cref="CommandApp.GetCompletions(string[])"/>.
    /// </summary>
    public enum CommandCompletionKind
    {
        /// <summary>
        /// A built-in command app token such as help or version.
        /// </summary>
        BuiltIn,

        /// <summary>
        /// A registered command or command alias.
        /// </summary>
        Command,

        /// <summary>
        /// A registered command branch or branch alias.
        /// </summary>
        Branch,

        /// <summary>
        /// A command or branch option alias.
        /// </summary>
        Option,

        /// <summary>
        /// A suggested option value.
        /// </summary>
        Value
    }
}
