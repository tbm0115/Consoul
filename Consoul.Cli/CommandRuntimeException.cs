using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents an error that occurs while creating or executing a command.
    /// </summary>
    public class CommandRuntimeException : CommandAppException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandRuntimeException"/> class.
        /// </summary>
        public CommandRuntimeException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandRuntimeException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public CommandRuntimeException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandRuntimeException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public CommandRuntimeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
