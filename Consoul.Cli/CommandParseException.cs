using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents an error that occurs while parsing command arguments or binding settings.
    /// </summary>
    public class CommandParseException : CommandAppException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandParseException"/> class.
        /// </summary>
        public CommandParseException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandParseException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public CommandParseException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandParseException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public CommandParseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
