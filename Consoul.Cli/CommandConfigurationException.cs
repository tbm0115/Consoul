using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents an error in command app configuration.
    /// </summary>
    public class CommandConfigurationException : CommandAppException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandConfigurationException"/> class.
        /// </summary>
        public CommandConfigurationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandConfigurationException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public CommandConfigurationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandConfigurationException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public CommandConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
