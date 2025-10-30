using System;
using System.Text;
using ConsoulLibrary.Color;

namespace ConsoulLibrary.Views.Editing
{
    /// <summary>
    /// Provides default editors for property editing scenarios.
    /// </summary>
    public sealed class DefaultPropertyEditor : IPropertyEditor
    {
        /// <inheritdoc />
        public bool TryEdit(PropertyEditContext context, out object value)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var instruction = context.Documentation.DisplayDescription;
            if (string.IsNullOrWhiteSpace(instruction))
            {
                instruction = context.Documentation.XmlSummary;
            }

            if (!string.IsNullOrWhiteSpace(instruction))
            {
                Consoul.WriteCore(NormalizeWhitespace(instruction), RenderOptions.SubnoteColor);
            }

            var expectedType = Nullable.GetUnderlyingType(context.Property.PropertyType) ?? context.Property.PropertyType;
            var message = "Enter new " + GetPropertyDisplayName(context) + " (" + expectedType.Name + ")";
            value = Consoul.Input(message, expectedType);
            return true;
        }

        private static string GetPropertyDisplayName(PropertyEditContext context)
        {
            var displayName = context.Documentation.DisplayName;
            if (!string.IsNullOrWhiteSpace(displayName))
            {
                return displayName;
            }

            return context.Property.Name;
        }

        private static string NormalizeWhitespace(string value)
        {
            var builder = new StringBuilder();
            var space = false;
            foreach (var ch in value)
            {
                if (char.IsWhiteSpace(ch))
                {
                    if (!space)
                    {
                        builder.Append(' ');
                        space = true;
                    }
                }
                else
                {
                    builder.Append(ch);
                    space = false;
                }
            }

            return builder.ToString().Trim();
        }
    }

    /// <summary>
    /// Prompts the user for an existing file path.
    /// </summary>
    public sealed class FilePathPropertyEditor : IPropertyEditor
    {
        /// <inheritdoc />
        public bool TryEdit(PropertyEditContext context, out object value)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var prompt = context.Documentation.DisplayDescription;
            if (string.IsNullOrWhiteSpace(prompt))
            {
                prompt = "Please type or paste a filepath below:";
            }

            var summary = context.Documentation.XmlSummary;
            if (!string.IsNullOrWhiteSpace(summary))
            {
                Consoul.WriteCore(NormalizeWhitespace(summary), RenderOptions.SubnoteColor);
            }

            Consoul.WriteCore(NormalizeWhitespace(prompt), RenderOptions.SubnoteColor);
            var displayName = context.Documentation.DisplayName;
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = context.Property.Name;
            }

            var currentValue = context.Property.GetValue(context.Model);
            var currentText = currentValue != null ? currentValue.ToString() : string.Empty;
            value = Consoul.PromptForFilepath(currentText, "Select " + displayName, checkExists: false);
            return true;
        }

        private static string NormalizeWhitespace(string value)
        {
            var builder = new StringBuilder();
            var space = false;
            foreach (var ch in value)
            {
                if (char.IsWhiteSpace(ch))
                {
                    if (!space)
                    {
                        builder.Append(' ');
                        space = true;
                    }
                }
                else
                {
                    builder.Append(ch);
                    space = false;
                }
            }

            return builder.ToString().Trim();
        }
    }
}
