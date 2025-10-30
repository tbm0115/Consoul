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
        public bool TryEdit(PropertyEditContext context, out object? value)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var instruction = !string.IsNullOrWhiteSpace(context.Documentation.DisplayDescription)
                ? context.Documentation.DisplayDescription
                : context.Documentation.XmlSummary;

            if (!string.IsNullOrWhiteSpace(instruction))
            {
                Consoul.WriteCore(NormalizeWhitespace(instruction), RenderOptions.SubnoteColor);
            }

            var expectedType = Nullable.GetUnderlyingType(context.Property.PropertyType) ?? context.Property.PropertyType;
            var message = $"Enter new {GetPropertyDisplayName(context)} ({expectedType.Name})";
            value = Consoul.Input(message, expectedType);
            return true;
        }

        private static string GetPropertyDisplayName(PropertyEditContext context)
        {
            if (!string.IsNullOrWhiteSpace(context.Documentation.DisplayName))
            {
                return context.Documentation.DisplayName!;
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
        public bool TryEdit(PropertyEditContext context, out object? value)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var prompt = string.IsNullOrWhiteSpace(context.Documentation.DisplayDescription)
                ? $"Please type or paste a filepath below:"
                : context.Documentation.DisplayDescription!;

            var summary = context.Documentation.XmlSummary;
            if (!string.IsNullOrWhiteSpace(summary))
            {
                Consoul.WriteCore(NormalizeWhitespace(summary), RenderOptions.SubnoteColor);
            }

            Consoul.WriteCore(NormalizeWhitespace(prompt), RenderOptions.SubnoteColor);
            var displayName = !string.IsNullOrWhiteSpace(context.Documentation.DisplayName)
                ? context.Documentation.DisplayName
                : context.Property.Name;
            value = Consoul.PromptForFilepath(context.Property.GetValue(context.Model)?.ToString() ?? string.Empty, $"Select {displayName}", checkExists: false);
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
