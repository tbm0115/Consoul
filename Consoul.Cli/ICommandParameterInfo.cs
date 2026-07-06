using System;
using System.Reflection;

namespace ConsoulLibrary
{
    /// <summary>
    /// Describes a command argument or option.
    /// </summary>
    public interface ICommandParameterInfo
    {
        /// <summary>
        /// Gets the primary command-line name used in help and errors.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the full parameter template.
        /// </summary>
        string Template { get; }

        /// <summary>
        /// Gets the parameter description.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the default value declared for the parameter.
        /// </summary>
        object DefaultValue { get; }

        /// <summary>
        /// Gets a value indicating whether the parameter is required.
        /// </summary>
        bool IsRequired { get; }

        /// <summary>
        /// Gets a value indicating whether the parameter is an option.
        /// </summary>
        bool IsOption { get; }

        /// <summary>
        /// Gets the CLR property that receives the parameter value.
        /// </summary>
        PropertyInfo Property { get; }

        /// <summary>
        /// Gets the CLR value type that receives the parameter value.
        /// </summary>
        Type ValueType { get; }
    }
}
