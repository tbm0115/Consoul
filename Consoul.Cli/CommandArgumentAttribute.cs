using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Marks a command settings property as a positional command argument.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class CommandArgumentAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandArgumentAttribute"/> class.
        /// </summary>
        /// <param name="position">The zero-based argument position.</param>
        /// <param name="template">The argument template, such as <c>&lt;source&gt;</c> or <c>[destination]</c>.</param>
        public CommandArgumentAttribute(int position, string template)
        {
            if (position < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Argument position must be zero or greater.");
            }

            Position = position;
            Template = ValidateTemplate(template);
            Name = ParseName(Template);
            IsRequired = Template.StartsWith("<", StringComparison.Ordinal);
        }

        /// <summary>
        /// Gets the zero-based argument position.
        /// </summary>
        public int Position { get; }

        /// <summary>
        /// Gets the argument template.
        /// </summary>
        public string Template { get; }

        /// <summary>
        /// Gets the display name parsed from the template.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a value indicating whether the argument is required.
        /// </summary>
        public bool IsRequired { get; }

        private static string ParseName(string template)
        {
            string value = template.Substring(1, template.Length - 2);

            return value.Trim();
        }

        private static string ValidateTemplate(string template)
        {
            string value = (template ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new CommandTemplateException("Argument template is required.");
            }

            if (!IsRequiredTemplate(value) && !IsOptionalTemplate(value))
            {
                throw new CommandTemplateException("Argument template '" + value + "' must use <name> or [name] syntax.");
            }

            string name = value.Substring(1, value.Length - 2).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new CommandTemplateException("Argument template '" + value + "' must contain a name.");
            }

            return value;
        }

        private static bool IsRequiredTemplate(string template)
        {
            return template.Length >= 2 &&
                template[0] == '<' &&
                template[template.Length - 1] == '>';
        }

        private static bool IsOptionalTemplate(string template)
        {
            return template.Length >= 2 &&
                template[0] == '[' &&
                template[template.Length - 1] == ']';
        }
    }
}
