using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Validates a bound command argument or option before command execution.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public abstract class ParameterValidationAttribute : Attribute
    {
        /// <summary>
        /// Validates a bound command parameter.
        /// </summary>
        /// <param name="context">The parameter context.</param>
        /// <returns>The validation result.</returns>
        public abstract CommandValidationResult Validate(CommandParameterContext context);
    }
}
