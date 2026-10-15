namespace ConsoulLibrary
{
    /// <summary>
    /// Represents an option flag that may be absent, present without a value, or present with a value.
    /// </summary>
    /// <typeparam name="T">The flag value type.</typeparam>
    public readonly struct FlagValue<T> : IFlagValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlagValue{T}"/> struct.
        /// </summary>
        /// <param name="isSet">A value indicating whether the flag was present on the command line.</param>
        /// <param name="value">The parsed flag value.</param>
        public FlagValue(bool isSet, T value)
        {
            IsSet = isSet;
            Value = value;
        }

        /// <summary>
        /// Gets a value indicating whether the flag was present on the command line.
        /// </summary>
        public bool IsSet { get; }

        /// <summary>
        /// Gets the parsed flag value.
        /// </summary>
        public T Value { get; }

        object IFlagValue.Value => Value;

        /// <summary>
        /// Creates a flag value that was not provided on the command line.
        /// </summary>
        /// <returns>An unset flag value.</returns>
        public static FlagValue<T> None()
        {
            return new FlagValue<T>(false, default(T));
        }

        /// <summary>
        /// Creates a flag value that was provided on the command line.
        /// </summary>
        /// <param name="value">The parsed flag value.</param>
        /// <returns>A set flag value.</returns>
        public static FlagValue<T> Set(T value)
        {
            return new FlagValue<T>(true, value);
        }

        /// <summary>
        /// Returns a display string for the flag value.
        /// </summary>
        /// <returns>The display string.</returns>
        public override string ToString()
        {
            return IsSet ? (Value == null ? string.Empty : Value.ToString()) : string.Empty;
        }
    }
}
