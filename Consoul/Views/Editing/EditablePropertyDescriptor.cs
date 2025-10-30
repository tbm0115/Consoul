using System;
using System.Reflection;

namespace ConsoulLibrary.Views.Editing
{
    /// <summary>
    /// Represents a property that can be edited within the JSON editor experience.
    /// </summary>
    public sealed class EditablePropertyDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditablePropertyDescriptor"/> class.
        /// </summary>
        /// <param name="property">The property represented by the descriptor.</param>
        /// <param name="documentation">Documentation describing the property.</param>
        public EditablePropertyDescriptor(PropertyInfo property, PropertyDocumentation documentation)
            : this(property, documentation, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditablePropertyDescriptor"/> class with explicit overrides.
        /// </summary>
        /// <param name="property">The property represented by the descriptor.</param>
        /// <param name="documentation">Documentation describing the property.</param>
        /// <param name="editorOverride">Editor instance overriding the default editor resolution.</param>
        /// <param name="formatterOverride">Formatter instance overriding the default formatter resolution.</param>
        public EditablePropertyDescriptor(PropertyInfo property, PropertyDocumentation documentation, IPropertyEditor editorOverride, IPropertyValueFormatter formatterOverride)
        {
            Property = property ?? throw new ArgumentNullException(nameof(property));
            Documentation = documentation ?? throw new ArgumentNullException(nameof(documentation));
            EditorOverride = editorOverride;
            FormatterOverride = formatterOverride;
        }

        /// <summary>
        /// Gets the source property.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Gets the documentation describing the property.
        /// </summary>
        public PropertyDocumentation Documentation { get; }

        /// <summary>
        /// Gets an editor that overrides the default resolution, if any.
        /// </summary>
        public IPropertyEditor EditorOverride { get; }

        /// <summary>
        /// Gets a formatter that overrides the default resolution, if any.
        /// </summary>
        public IPropertyValueFormatter FormatterOverride { get; }

        /// <summary>
        /// Gets the display label for the property.
        /// </summary>
        public string DisplayName => Documentation.DisplayName ?? Property.Name;

        /// <summary>
        /// Creates a context describing the property for editing.
        /// </summary>
        /// <param name="model">Model containing the property.</param>
        /// <returns>The constructed edit context.</returns>
        public PropertyEditContext CreateContext(object model)
        {
            var value = Property.GetValue(model);
            return new PropertyEditContext(model, Property, Documentation, value);
        }
    }
}
