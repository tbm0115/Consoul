using System;
using System.Reflection;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides contextual information for command argument and option validation.
    /// </summary>
    public sealed class CommandParameterContext
    {
        internal CommandParameterContext(CommandSettings settings, PropertyInfo property, string parameterName, object value)
            : this(settings, new CommandParameterInfo(
                parameterName,
                parameterName,
                string.Empty,
                null,
                false,
                false,
                property),
                value)
        {
        }

        internal CommandParameterContext(CommandSettings settings, ICommandParameterInfo parameter, object value)
        {
            Settings = settings;
            Parameter = parameter;
            Property = parameter == null ? null : parameter.Property;
            ParameterName = parameter == null ? string.Empty : parameter.Name;
            PropertyName = Property == null ? string.Empty : Property.Name;
            ValueType = parameter == null ? typeof(object) : parameter.ValueType;
            Value = value;
        }

        /// <summary>
        /// Gets the command settings instance that owns the parameter.
        /// </summary>
        public CommandSettings Settings { get; }

        /// <summary>
        /// Gets the command parameter metadata.
        /// </summary>
        public ICommandParameterInfo Parameter { get; }

        /// <summary>
        /// Gets the property that receives the parameter value.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Gets the command-line argument or option name used in help and errors.
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        /// Gets the CLR property name that receives the parameter value.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Gets the CLR type that receives the parameter value.
        /// </summary>
        public Type ValueType { get; }

        /// <summary>
        /// Gets the bound parameter value.
        /// </summary>
        public object Value { get; }
    }

    internal sealed class CommandParameterInfo : ICommandParameterInfo
    {
        public CommandParameterInfo(
            string name,
            string template,
            string description,
            object defaultValue,
            bool isRequired,
            bool isOption,
            PropertyInfo property)
        {
            Name = name ?? string.Empty;
            Template = template ?? string.Empty;
            Description = description ?? string.Empty;
            DefaultValue = defaultValue;
            IsRequired = isRequired;
            IsOption = isOption;
            Property = property;
            ValueType = property == null ? typeof(object) : property.PropertyType;
        }

        public string Name { get; }

        public string Template { get; }

        public string Description { get; }

        public object DefaultValue { get; }

        public bool IsRequired { get; }

        public bool IsOption { get; }

        public PropertyInfo Property { get; }

        public Type ValueType { get; }
    }
}
