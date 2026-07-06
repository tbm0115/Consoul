using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a command app error raised by Consoul's command-line runner.
    /// </summary>
    public class CommandAppException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAppException"/> class.
        /// </summary>
        public CommandAppException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAppException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public CommandAppException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAppException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public CommandAppException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
