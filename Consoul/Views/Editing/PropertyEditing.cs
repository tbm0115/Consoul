using System;
using System.Collections.Generic;
using System.Reflection;

namespace ConsoulLibrary.Views.Editing
{
    /// <summary>
    /// Represents contextual information for property editing operations.
    /// </summary>
    public sealed class PropertyEditContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyEditContext"/> class.
        /// </summary>
        /// <param name="model">The model containing the property to edit.</param>
        /// <param name="property">The property being edited.</param>
        /// <param name="documentation">Documentation describing the property.</param>
        /// <param name="currentValue">The current value of the property.</param>
        public PropertyEditContext(object model, PropertyInfo property, PropertyDocumentation documentation, object currentValue)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            Property = property ?? throw new ArgumentNullException(nameof(property));
            Documentation = documentation ?? throw new ArgumentNullException(nameof(documentation));
            CurrentValue = currentValue;
        }

        /// <summary>
        /// Gets the model that contains the property being edited.
        /// </summary>
        public object Model { get; }

        /// <summary>
        /// Gets the property being edited.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Gets the documentation entry for the property.
        /// </summary>
        public PropertyDocumentation Documentation { get; }

        /// <summary>
        /// Gets or sets the current value of the property.
        /// </summary>
        public object CurrentValue { get; set; }

        /// <summary>
        /// Applies the provided value to the property on the underlying model and updates the current value snapshot.
        /// </summary>
        /// <param name="value">The value that should be assigned to the property.</param>
        public void ApplyValue(object value)
        {
            Property.SetValue(Model, value);
            CurrentValue = value;
        }
    }

    /// <summary>
    /// Describes a formatter that can normalize values before they are assigned to a property.
    /// </summary>
    public interface IPropertyValueFormatter
    {
        /// <summary>
        /// Applies the formatter to the provided value.
        /// </summary>
        /// <param name="context">Context of the property edit.</param>
        /// <param name="value">The value produced by the editor.</param>
        /// <returns>The formatted value that should be assigned to the property.</returns>
        object Format(PropertyEditContext context, object value);
    }

    /// <summary>
    /// Defines an editor responsible for collecting a new value for a property.
    /// </summary>
    public interface IPropertyEditor
    {
        /// <summary>
        /// Attempts to collect a new value for the property.
        /// </summary>
        /// <param name="context">Context for the property edit.</param>
        /// <param name="value">Receives the updated value if the edit succeeded.</param>
        /// <returns><see langword="true"/> when the edit completes and a value should be assigned; otherwise <see langword="false"/>.</returns>
        bool TryEdit(PropertyEditContext context, out object value);
    }

    /// <summary>
    /// Identifies a custom editor used to capture a property's value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class PropertyEditorAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyEditorAttribute"/> class.
        /// </summary>
        /// <param name="editorType">Type implementing <see cref="IPropertyEditor"/> that will edit the property's value.</param>
        public PropertyEditorAttribute(Type editorType)
        {
            if (editorType == null)
            {
                throw new ArgumentNullException(nameof(editorType));
            }

            if (!typeof(IPropertyEditor).IsAssignableFrom(editorType))
            {
                throw new ArgumentException($"Editor type '{editorType.FullName}' must implement {nameof(IPropertyEditor)}.", nameof(editorType));
            }

            EditorType = editorType;
        }

        /// <summary>
        /// Gets the editor type associated with the property.
        /// </summary>
        public Type EditorType { get; }
    }

    /// <summary>
    /// Identifies a value formatter used to transform values after editing completes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class PropertyValueFormatterAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValueFormatterAttribute"/> class.
        /// </summary>
        /// <param name="formatterType">Type implementing <see cref="IPropertyValueFormatter"/>.</param>
        public PropertyValueFormatterAttribute(Type formatterType)
        {
            if (formatterType == null)
            {
                throw new ArgumentNullException(nameof(formatterType));
            }

            if (!typeof(IPropertyValueFormatter).IsAssignableFrom(formatterType))
            {
                throw new ArgumentException($"Formatter type '{formatterType.FullName}' must implement {nameof(IPropertyValueFormatter)}.", nameof(formatterType));
            }

            FormatterType = formatterType;
        }

        /// <summary>
        /// Gets the formatter type associated with the property.
        /// </summary>
        public Type FormatterType { get; }
    }

    /// <summary>
    /// Provides extensibility hooks for supplying documentation, editors and formatters when a property cannot be inspected directly.
    /// </summary>
    public interface IPropertyMetadataResolver
    {
        /// <summary>
        /// Resolves metadata used when editing the target property.
        /// </summary>
        /// <param name="property">The property that is being edited.</param>
        /// <returns>A metadata container describing custom documentation, editors, formatters and layers.</returns>
        ResolvedPropertyMetadata Resolve(PropertyInfo property);
    }

    /// <summary>
    /// Describes optional metadata supplied by <see cref="IPropertyMetadataResolver"/> implementations.
    /// </summary>
    public sealed class ResolvedPropertyMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResolvedPropertyMetadata"/> class.
        /// </summary>
        /// <param name="documentation">Documentation describing the property.</param>
        /// <param name="editor">Editor instance that should collect values for the property.</param>
        /// <param name="formatter">Formatter instance applied to the editor's result.</param>
        public ResolvedPropertyMetadata(PropertyDocumentation documentation, IPropertyEditor editor, IPropertyValueFormatter formatter)
            : this(documentation, editor, formatter, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolvedPropertyMetadata"/> class including layer overrides.
        /// </summary>
        /// <param name="documentation">Documentation describing the property.</param>
        /// <param name="editor">Editor instance that should collect values for the property.</param>
        /// <param name="formatter">Formatter instance applied to the editor's result.</param>
        /// <param name="layerProvider">Provider exposing additional editing layers for the property.</param>
        public ResolvedPropertyMetadata(PropertyDocumentation documentation, IPropertyEditor editor, IPropertyValueFormatter formatter, IPropertyLayerProvider layerProvider)
        {
            Documentation = documentation;
            Editor = editor;
            Formatter = formatter;
            LayerProvider = layerProvider;
        }

        /// <summary>
        /// Gets the documentation describing the property.
        /// </summary>
        public PropertyDocumentation Documentation { get; }

        /// <summary>
        /// Gets the editor that should capture the property's value.
        /// </summary>
        public IPropertyEditor Editor { get; }

        /// <summary>
        /// Gets the formatter that should post-process the value produced by the editor.
        /// </summary>
        public IPropertyValueFormatter Formatter { get; }

        /// <summary>
        /// Gets the provider that exposes additional editing layers for the property.
        /// </summary>
        public IPropertyLayerProvider LayerProvider { get; }
    }

    /// <summary>
    /// Describes a callback capable of editing a property through a custom layer.
    /// </summary>
    /// <param name="context">Context describing the property edit.</param>
    /// <returns><see langword="true"/> when the property's value was updated.</returns>
    public delegate bool PropertyLayerEditHandler(PropertyEditContext context);

    /// <summary>
    /// Represents an additional editing surface exposed for a property.
    /// </summary>
    public sealed class PropertyEditLayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyEditLayer"/> class.
        /// </summary>
        /// <param name="displayName">Display name describing the layer.</param>
        /// <param name="description">Optional description shown before the layer is invoked.</param>
        /// <param name="handler">Handler responsible for editing the property's value.</param>
        /// <param name="applyContextValue">Indicates whether the layer's handler expects the caller to apply <see cref="PropertyEditContext.CurrentValue"/> to the property upon completion.</param>
        public PropertyEditLayer(string displayName, string description, PropertyLayerEditHandler handler, bool applyContextValue)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            DisplayName = displayName ?? string.Empty;
            Description = description ?? string.Empty;
            Handler = handler;
            ApplyContextValue = applyContextValue;
        }

        /// <summary>
        /// Gets the display name describing the layer.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets a description presented prior to executing the layer.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the handler responsible for executing the layer.
        /// </summary>
        public PropertyLayerEditHandler Handler { get; }

        /// <summary>
        /// Gets a value indicating whether the edit context value should be applied to the property after the handler completes.
        /// </summary>
        public bool ApplyContextValue { get; }
    }

    /// <summary>
    /// Exposes additional editing layers that can be invoked for a property.
    /// </summary>
    public interface IPropertyLayerProvider
    {
        /// <summary>
        /// Retrieves the layers that can be executed for the supplied edit context.
        /// </summary>
        /// <param name="context">Context describing the property edit.</param>
        /// <returns>A sequence of layers that can edit the property.</returns>
        IEnumerable<PropertyEditLayer> GetLayers(PropertyEditContext context);
    }

    /// <summary>
    /// Specifies a resolver that supplies custom metadata for a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class PropertyMetadataResolverAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyMetadataResolverAttribute"/> class.
        /// </summary>
        /// <param name="resolverType">Type implementing <see cref="IPropertyMetadataResolver"/>.</param>
        public PropertyMetadataResolverAttribute(Type resolverType)
        {
            if (resolverType == null)
            {
                throw new ArgumentNullException(nameof(resolverType));
            }

            if (!typeof(IPropertyMetadataResolver).IsAssignableFrom(resolverType))
            {
                throw new ArgumentException("Resolver type '" + resolverType.FullName + "' must implement " + nameof(IPropertyMetadataResolver) + ".", nameof(resolverType));
            }

            ResolverType = resolverType;
        }

        /// <summary>
        /// Gets the resolver type associated with the property.
        /// </summary>
        public Type ResolverType { get; }
    }
}
