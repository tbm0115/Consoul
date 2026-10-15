using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents an error that occurs while binding command-line parameters to command settings.
    /// </summary>
    public class CommandParameterException : CommandParseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandParameterException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public CommandParameterException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandParameterException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public CommandParameterException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
