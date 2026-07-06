namespace ConsoulLibrary
{
    /// <summary>
    /// Represents an option flag that may be absent, present without a value, or present with a value.
    /// </summary>
    public interface IFlagValue
    {
        /// <summary>
        /// Gets a value indicating whether the flag was present on the command line.
        /// </summary>
        bool IsSet { get; }

        /// <summary>
        /// Gets the parsed flag value.
        /// </summary>
        object Value { get; }
    }
}
