using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents an error in a command argument or option template.
    /// </summary>
    public class CommandTemplateException : CommandConfigurationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandTemplateException"/> class.
        /// </summary>
        public CommandTemplateException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandTemplateException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public CommandTemplateException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandTemplateException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public CommandTemplateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
