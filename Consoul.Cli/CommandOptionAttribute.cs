using System;
using System.Collections.Generic;

namespace ConsoulLibrary
{
    /// <summary>
    /// Marks a command settings property as a named command-line option.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class CommandOptionAttribute : Attribute
    {
        private readonly List<string> _shortNames = new List<string>();
        private readonly List<string> _longNames = new List<string>();
        private readonly List<string> _aliases = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandOptionAttribute"/> class.
        /// </summary>
        /// <param name="template">The option template, such as <c>-f|--force</c> or <c>-o|--output &lt;PATH&gt;</c>.</param>
        public CommandOptionAttribute(string template) : this(template, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandOptionAttribute"/> class.
        /// </summary>
        /// <param name="template">The option template, such as <c>-f|--force</c> or <c>-o|--output &lt;PATH&gt;</c>.</param>
        /// <param name="isRequired">Whether the option must be provided.</param>
        public CommandOptionAttribute(string template, bool isRequired)
        {
            Template = ValidateTemplate(template);
            IsRequired = isRequired;
            ParseTemplate(Template);
        }

        /// <summary>
        /// Gets the original option template.
        /// </summary>
        public string Template { get; }

        /// <summary>
        /// Gets the parsed long option names.
        /// </summary>
        public IReadOnlyList<string> LongNames => _longNames.AsReadOnly();

        /// <summary>
        /// Gets the parsed short option names.
        /// </summary>
        public IReadOnlyList<string> ShortNames => _shortNames.AsReadOnly();

        /// <summary>
        /// Gets the parsed option aliases.
        /// </summary>
        public IReadOnlyList<string> Aliases => _aliases.AsReadOnly();

        /// <summary>
        /// Gets or sets the value name shown in help output.
        /// </summary>
        public string ValueName { get; set; }

        /// <summary>
        /// Gets or sets the environment variable used as a fallback value when the option is not provided.
        /// </summary>
        public string EnvironmentVariable { get; set; }

        /// <summary>
        /// Gets whether the option must be provided.
        /// </summary>
        public bool IsRequired { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the option should be hidden from help output.
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an option value may be omitted.
        /// </summary>
        public bool ValueIsOptional { get; set; }

        private void ParseTemplate(string template)
        {
            foreach (string rawPart in template.Split('|'))
            {
                string part = rawPart.Trim();
                if (part.Length == 0)
                {
                    throw new CommandTemplateException("Option template '" + template + "' contains an empty alias segment.");
                }

                int valueStart = part.IndexOf(' ');
                if (valueStart >= 0)
                {
                    string valueTemplate = part.Substring(valueStart + 1).Trim();
                    part = part.Substring(0, valueStart).Trim();
                    ValidateValueTemplate(valueTemplate);
                    if (IsOptionalValueTemplate(valueTemplate))
                    {
                        ValueIsOptional = true;
                    }

                    if (string.IsNullOrWhiteSpace(ValueName))
                    {
                        ValueName = ParseValueName(valueTemplate);
                    }
                }

                AddAlias(part, template);
            }

            if (_aliases.Count == 0)
            {
                throw new CommandTemplateException("Option template '" + template + "' must contain at least one option alias.");
            }
        }

        private void AddAlias(string alias, string template)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                throw new CommandTemplateException("Option template '" + template + "' contains an empty option alias.");
            }

            if (!alias.StartsWith("-", StringComparison.Ordinal))
            {
                throw new CommandTemplateException("Option alias '" + alias + "' must start with '-' or '--'.");
            }

            if (string.Equals(alias, "-", StringComparison.Ordinal) ||
                string.Equals(alias, "--", StringComparison.Ordinal))
            {
                throw new CommandTemplateException("Option alias '" + alias + "' is invalid.");
            }

            if (alias.IndexOf(' ') >= 0)
            {
                throw new CommandTemplateException("Option alias '" + alias + "' must not contain whitespace.");
            }

            if (alias.StartsWith("--", StringComparison.Ordinal))
            {
                _longNames.Add(alias);
                _aliases.Add(alias);
            }
            else
            {
                _shortNames.Add(alias);
                _aliases.Add(alias);
            }
        }

        private static string ValidateTemplate(string template)
        {
            string value = (template ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new CommandTemplateException("Option template is required.");
            }

            return value;
        }

        private static void ValidateValueTemplate(string valueTemplate)
        {
            string value = (valueTemplate ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new CommandTemplateException("Option value template is required when an option alias is followed by whitespace.");
            }

            if (!IsRequiredValueTemplate(value) && !IsOptionalValueTemplate(value))
            {
                throw new CommandTemplateException("Option value template '" + value + "' must use <name> or [name] syntax.");
            }

            string name = value.Substring(1, value.Length - 2).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new CommandTemplateException("Option value template '" + value + "' must contain a value name.");
            }
        }

        private static bool IsRequiredValueTemplate(string valueTemplate)
        {
            string value = (valueTemplate ?? string.Empty).Trim();
            return value.Length >= 2 && value[0] == '<' && value[value.Length - 1] == '>';
        }

        private static bool IsOptionalValueTemplate(string valueTemplate)
        {
            string value = (valueTemplate ?? string.Empty).Trim();
            return value.Length >= 2 && value[0] == '[' && value[value.Length - 1] == ']';
        }

        private static string ParseValueName(string valueTemplate)
        {
            string value = (valueTemplate ?? string.Empty).Trim();
            ValidateValueTemplate(value);
            return value.Substring(1, value.Length - 2).Trim();
        }
    }
}
