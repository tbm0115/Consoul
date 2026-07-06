using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsoulLibrary.Color;
using ConsoulLibrary.Views.Editing;

namespace ConsoulLibrary
{
    /// <summary>
    /// Creates a dynamic object editor view for the properties of the given object.
    /// </summary>
    [View("Edit Object View", GoBackMessage = "<-- Save Changes and Go Back")]
    public class EditObjectView : DynamicView<object>
    {
        private readonly List<EditablePropertyDescriptor> _descriptors;
        private readonly BindingFlags _bindingFlags;
        private readonly bool _documentEditorEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditObjectView"/> class.
        /// </summary>
        /// <param name="entity">The entity being edited.</param>
        /// <param name="bindingAttr">Binding flags describing the accessible properties.</param>
        /// <param name="enableJsonEditor">Compatibility parameter for existing callers. Set to <see langword="false"/> to disable the form document editor option.</param>
        public EditObjectView(object entity, BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, bool enableJsonEditor = true)
            : base()
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            Model = entity;
            _bindingFlags = bindingAttr;
            var entityType = entity.GetType();
            var documentEditorEnabled = enableJsonEditor;
            if (entityType.GetCustomAttribute<DisableFormDocumentEditorAttribute>(true) != null ||
                entityType.GetCustomAttribute<DisableJsonEditorAttribute>(true) != null)
            {
                documentEditorEnabled = false;
            }

            _documentEditorEnabled = documentEditorEnabled;
            Title = BannerEntry.Render($"Edit {entityType.Name} View");
            PromptFilterEnabled = true;

            _descriptors = entityType
                .GetProperties(bindingAttr)
                .Where(property =>
                    property.CanRead &&
                    property.GetIndexParameters().Length == 0 &&
                    PropertyUiMetadata.IsEditable(property) &&
                    !ShouldIgnore(property))
                .OrderBy(PropertyUiMetadata.GetOrder)
                .Select(property => CreateDescriptor(property))
                .ToList();

            if (_documentEditorEnabled && _descriptors.Count > 0)
            {
                _options.Add(new DynamicOption<object>(
                    () => DocumentEditorOptionLabel,
                    cancellationToken => OpenDocumentEditor(cancellationToken),
                    () => RenderOptions.OptionColor)
                    .Describe("Browse and edit properties in the form document."));
            }

            AddLegacyOptions(_descriptors);
        }

        /// <summary>
        /// Indicates whether the form document editor should highlight values using the default highlight scheme.
        /// </summary>
        public ConsoleColor HighlightColor { get; set; } = ConsoleColor.DarkCyan;

        /// <summary>
        /// Gets or sets the option label that opens the form document editor.
        /// </summary>
        public string DocumentEditorOptionLabel { get; set; } = "Open document editor";

        /// <summary>
        /// Gets a value indicating whether the form document editor option is enabled for this view.
        /// </summary>
        public bool IsDocumentEditorEnabled => _documentEditorEnabled;

        private static Func<object, Type, string> _formValueSerializer = SerializeJsonValueWithoutDependencies;

        /// <summary>
        /// Serializes values displayed by the form document editor's JSON-style value layout.
        /// </summary>
        public static Func<object, Type, string> FormValueSerializer
        {
            get => _formValueSerializer;
            set => _formValueSerializer = value ?? SerializeJsonValueWithoutDependencies;
        }

        /// <summary>
        /// Gets or sets the serializer used by the form document editor's JSON-style value layout.
        /// </summary>
        /// <remarks>
        /// This compatibility alias is retained for callers that adopted the original JSON editor terminology.
        /// Prefer <see cref="FormValueSerializer"/> for new MVC-style object editor views.
        /// </remarks>
        public static Func<object, Type, string> JsonValueSerializer
        {
            get => FormValueSerializer;
            set => FormValueSerializer = value;
        }

        private EditablePropertyDescriptor CreateDescriptor(PropertyInfo property)
        {
            var documentation = new PropertyDocumentation(property);
            IPropertyEditor editorOverride = null;
            IPropertyValueFormatter formatterOverride = null;
            IPropertyLayerProvider layerProviderOverride = null;

            var resolverAttribute = property.GetCustomAttribute<PropertyMetadataResolverAttribute>(true);
            if (resolverAttribute != null)
            {
                object resolverInstance;
                try
                {
                    resolverInstance = Activator.CreateInstance(resolverAttribute.ResolverType);
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException("Property metadata resolver attribute could not create resolver instance.", exception);
                }

                var resolver = resolverInstance as IPropertyMetadataResolver;
                if (resolver == null)
                {
                    throw new InvalidOperationException("Property metadata resolver attribute references an invalid resolver type.");
                }

                var resolvedMetadata = resolver.Resolve(property);
                if (resolvedMetadata != null)
                {
                    if (resolvedMetadata.Documentation != null)
                    {
                        documentation = resolvedMetadata.Documentation;
                    }

                    if (resolvedMetadata.Editor != null)
                    {
                        editorOverride = resolvedMetadata.Editor;
                    }

                    if (resolvedMetadata.Formatter != null)
                    {
                        formatterOverride = resolvedMetadata.Formatter;
                    }

                    if (resolvedMetadata.LayerProvider != null)
                    {
                        layerProviderOverride = resolvedMetadata.LayerProvider;
                    }
                }
            }

            return new EditablePropertyDescriptor(property, documentation, editorOverride, formatterOverride, layerProviderOverride);
        }

        private static bool ShouldIgnore(PropertyInfo property)
        {
            if (property == null)
            {
                return false;
            }

            return PropertyUiMetadata.IsIgnored(property);
        }

        private void AddLegacyOptions(IEnumerable<EditablePropertyDescriptor> descriptors)
        {
            var groups = CreateDescriptorGroups(descriptors);
            foreach (var group in groups)
            {
                if (!string.IsNullOrWhiteSpace(group.Name))
                {
                    _options.Add(DynamicOption<object>.Separator(group.Name, RenderOptions.SubnoteColor));
                }

                foreach (var descriptor in group.Descriptors)
                {
                    AddLegacyOption(descriptor);
                }
            }
        }

        private static List<DescriptorGroup> CreateDescriptorGroups(IEnumerable<EditablePropertyDescriptor> descriptors)
        {
            var groups = new List<DescriptorGroup>();
            var groupsByName = new Dictionary<string, DescriptorGroup>(StringComparer.Ordinal);
            DescriptorGroup ungrouped = null;

            foreach (var descriptor in descriptors)
            {
                string groupName = PropertyUiMetadata.GetGroupName(descriptor.Property);
                if (string.IsNullOrWhiteSpace(groupName))
                {
                    if (ungrouped == null)
                    {
                        ungrouped = new DescriptorGroup(null);
                        groups.Add(ungrouped);
                    }

                    ungrouped.Descriptors.Add(descriptor);
                    continue;
                }

                if (!groupsByName.TryGetValue(groupName, out var group))
                {
                    group = new DescriptorGroup(groupName);
                    groupsByName.Add(groupName, group);
                    groups.Add(group);
                }

                group.Descriptors.Add(descriptor);
            }

            return groups;
        }

        private void AddLegacyOption(EditablePropertyDescriptor descriptor)
        {
            var property = descriptor.Property;
            var propertyType = property.PropertyType;
            string description = ResolveOptionDescription(descriptor);

            if (IsSimpleType(propertyType, out var simpleType))
            {
                _options.Add(new DynamicOption<object>(
                    () => $"Edit {descriptor.DisplayName}: " + (property.GetValue(Model)?.ToString() ?? "<N/A>"),
                    cancellationToken => EditDescriptorAsync(descriptor, cancellationToken),
                    () => ConsoleColor.Yellow)
                    .Describe(description));
                return;
            }

            if (IsElementType(propertyType, out var elementType))
            {
                if (IsSimpleType(elementType, out var simpleElementType))
                {
                    _options.Add(new DynamicOption<object>(
                        () => $"Edit {descriptor.DisplayName}: {simpleElementType.Name}[{GetCollectionCount(Model, property)}]",
                        cancellationToken => EditDescriptorAsync(descriptor, cancellationToken),
                        () => ConsoleColor.Green)
                        .Describe(description));
                }
                else
                {
                    _options.Add(new DynamicOption<object>(
                        () => $"Edit {descriptor.DisplayName}: {elementType.Name}[{GetCollectionCount(Model, property)}]",
                        cancellationToken => EditDescriptorAsync(descriptor, cancellationToken),
                        () => ConsoleColor.Green)
                        .Describe(description));
                }

                return;
            }

            Type keyType;
            Type valueType;
            if (IsDictionaryType(propertyType, out keyType, out valueType))
            {
                if (IsSimpleType(keyType, out var simpleKeyType))
                {
                    _options.Add(new DynamicOption<object>(
                        () => $"Edit {descriptor.DisplayName}",
                        cancellationToken => EditDescriptorAsync(descriptor, cancellationToken),
                        () => ConsoleColor.Green)
                        .Describe(description));
                }
                else
                {
                    _options.Add(new DynamicOption<object>(
                        () => $"View {descriptor.DisplayName}",
                        cancellationToken =>
                        {
                            Consoul.Write($"Cannot render {descriptor.DisplayName} because it is a Dictionary with a complex key");
                            Consoul.Wait(cancellationToken: cancellationToken);
                            return Task.CompletedTask;
                        },
                        () => ConsoleColor.DarkRed)
                        .Describe(description));
                }

                return;
            }

            _options.Add(new DynamicOption<object>(
                () => $"Edit {descriptor.DisplayName}",
                cancellationToken => EditDescriptorAsync(descriptor, cancellationToken),
                () => ConsoleColor.Green)
                .Describe(description));
        }

        private static string ResolveOptionDescription(EditablePropertyDescriptor descriptor)
        {
            if (descriptor == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(descriptor.Documentation.DisplayDescription))
            {
                return NormalizeOptionDescription(descriptor.Documentation.DisplayDescription);
            }

            if (!string.IsNullOrWhiteSpace(descriptor.Documentation.XmlSummary))
            {
                return NormalizeOptionDescription(descriptor.Documentation.XmlSummary);
            }

            return null;
        }

        private static string NormalizeOptionDescription(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var builder = new StringBuilder();
            bool space = false;
            foreach (char ch in value)
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

        private void EditSimpleProperty(EditablePropertyDescriptor descriptor, Type propertyType, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var editor = descriptor.EditorOverride ?? ResolveEditor(descriptor.Property);
            var context = descriptor.CreateContext(Model, cancellationToken);
            if (editor.TryEdit(context, out var value))
            {
                if (descriptor.FormatterOverride != null)
                {
                    value = descriptor.FormatterOverride.Format(context, value);
                }
                else
                {
                    value = ApplyFormatter(descriptor.Property, context, value);
                }
                value = ConvertIfNeeded(value, propertyType);
                context.CurrentValue = value;
                descriptor.Property.SetValue(Model, value);
            }
        }

        private void EditComplexProperty(EditablePropertyDescriptor descriptor, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var propertyValue = descriptor.Property.GetValue(Model) ?? Activator.CreateInstance(descriptor.Property.PropertyType);
            var recursiveView = new EditObjectView(propertyValue, _bindingFlags, _documentEditorEnabled);
            recursiveView.RenderAsync(cancellationToken).GetAwaiter().GetResult();
            descriptor.Property.SetValue(Model, recursiveView.Model);
        }

        private object EditObject(PropertyInfo property, Type propertyType = null, CancellationToken cancellationToken = default)
        {
            var expectedType = propertyType ?? property.PropertyType;
            return Consoul.Input($"Enter new {property.Name}\t({expectedType.Name})", expectedType, cancellationToken: cancellationToken);
        }

        private Task EditDescriptorAsync(EditablePropertyDescriptor descriptor, CancellationToken cancellationToken)
        {
            EditDescriptor(descriptor, cancellationToken);
            return Task.CompletedTask;
        }

        private void EditDescriptor(EditablePropertyDescriptor descriptor, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var context = descriptor.CreateContext(Model, cancellationToken);
            var layers = descriptor.GetLayers(context);
            if (layers.Count > 0)
            {
                var prompt = new SelectionPrompt("Choose how to edit " + descriptor.DisplayName);
                var defaultChoice = prompt.Add("Use default editor", ConsoleColor.Yellow);
                var layerChoices = new Dictionary<int, PropertyEditLayer>();

                foreach (var layer in layers)
                {
                    var index = prompt.Add(layer.DisplayName, ConsoleColor.Green);
                    layerChoices[index] = layer;
                }

                var result = prompt.Render(cancellationToken);
                if (result.IsCanceled || !result.HasSelection)
                {
                    return;
                }

                if (result.Index == defaultChoice)
                {
                    EditDescriptorUsingDefaultEditor(descriptor, cancellationToken);
                    return;
                }

                if (!layerChoices.ContainsKey(result.Index))
                {
                    return;
                }

                var selectedLayer = layerChoices[result.Index];
                if (!string.IsNullOrWhiteSpace(selectedLayer.Description))
                {
                    Consoul.WriteCore(selectedLayer.Description, RenderOptions.SubnoteColor);
                    Consoul.WriteCore(string.Empty, RenderOptions.DefaultColor);
                }

                bool updated = false;
                try
                {
                    updated = selectedLayer.Handler(context);
                }
                catch (Exception exception)
                {
                    Consoul.WriteCore("Layer failed: " + exception.Message, RenderOptions.InvalidColor);
                    Consoul.Wait(cancellationToken: cancellationToken);
                    return;
                }

                if (updated)
                {
                    if (selectedLayer.ApplyContextValue)
                    {
                        descriptor.Property.SetValue(Model, context.CurrentValue);
                    }

                    return;
                }

                EditDescriptorUsingDefaultEditor(descriptor, cancellationToken);
                return;
            }

            EditDescriptorUsingDefaultEditor(descriptor, cancellationToken);
        }

        private void EditDescriptorUsingDefaultEditor(EditablePropertyDescriptor descriptor, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var propertyType = descriptor.Property.PropertyType;

            if (IsSimpleType(propertyType, out var simpleType))
            {
                EditSimpleProperty(descriptor, simpleType, cancellationToken);
                return;
            }

            if (IsElementType(propertyType, out var elementType))
            {
                if (IsSimpleType(elementType, out var simpleElementType))
                {
                    EditSimpleCollection(Model, descriptor.Property, simpleElementType, cancellationToken);
                }
                else
                {
                    EditComplexCollection(Model, descriptor.Property, elementType, cancellationToken);
                }

                return;
            }

            Type keyType;
            Type valueType;
            if (IsDictionaryType(propertyType, out keyType, out valueType))
            {
                if (IsSimpleType(keyType, out var simpleKeyType))
                {
                    EditSimpleKeyDictionary(Model, descriptor, simpleKeyType, valueType, cancellationToken);
                }
                else
                {
                    Consoul.Write($"Cannot render {descriptor.DisplayName} because it is a Dictionary with a complex key");
                    Consoul.Wait(cancellationToken: cancellationToken);
                }

                return;
            }

            EditComplexProperty(descriptor, cancellationToken);
        }

        private Task OpenDocumentEditor(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }

            var editor = new ObjectFormDocumentEditor(this);
            editor.Run(cancellationToken);
            return Task.CompletedTask;
        }

        private static string SerializeJsonValueWithoutDependencies(object value, Type valueType)
        {
            if (value == null)
            {
                return "null";
            }

            Type type = Nullable.GetUnderlyingType(valueType ?? value.GetType()) ?? valueType ?? value.GetType();

            if (type == typeof(string) || type == typeof(char) || type == typeof(Guid) || type == typeof(DateTime) || type.IsEnum)
            {
                return QuoteJsonString(Convert.ToString(value, CultureInfo.InvariantCulture));
            }

            if (type == typeof(bool))
            {
                return (bool)value ? "true" : "false";
            }

            if (IsNumericType(type))
            {
                return Convert.ToString(value, CultureInfo.InvariantCulture);
            }

            var enumerable = value as IEnumerable;
            if (enumerable != null && !(value is string))
            {
                var values = enumerable.Cast<object>()
                    .Select(item => SerializeJsonValueWithoutDependencies(item, item?.GetType() ?? typeof(object)));
                return "[" + string.Join(", ", values) + "]";
            }

            return QuoteJsonString(value.ToString());
        }

        private static bool IsNumericType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        private static string QuoteJsonString(string value)
        {
            if (value == null)
            {
                return "null";
            }

            var builder = new StringBuilder(value.Length + 2);
            builder.Append('"');
            foreach (char ch in value)
            {
                switch (ch)
                {
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '\b':
                        builder.Append("\\b");
                        break;
                    case '\f':
                        builder.Append("\\f");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        if (char.IsControl(ch))
                        {
                            builder.Append("\\u");
                            builder.Append(((int)ch).ToString("x4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            builder.Append(ch);
                        }

                        break;
                }
            }

            builder.Append('"');
            return builder.ToString();
        }

        private static object ConvertIfNeeded(object value, Type targetType)
        {
            if (value == null)
            {
                return null;
            }

            var expectedType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (expectedType.IsInstanceOfType(value))
            {
                return value;
            }

            if (value is string text)
            {
                if (Nullable.GetUnderlyingType(targetType) != null && string.IsNullOrEmpty(text))
                {
                    return null;
                }

                if (expectedType.IsEnum)
                {
                    return Enum.Parse(expectedType, text, ignoreCase: true);
                }

                if (expectedType == typeof(Guid))
                {
                    return Guid.Parse(text);
                }

                if (expectedType == typeof(TimeSpan))
                {
                    return TimeSpan.Parse(text, CultureInfo.CurrentCulture);
                }

                if (expectedType == typeof(DateTimeOffset))
                {
                    return DateTimeOffset.Parse(text, CultureInfo.CurrentCulture);
                }

                if (expectedType == typeof(Uri))
                {
                    return new Uri(text, UriKind.RelativeOrAbsolute);
                }

                if (expectedType == typeof(Version))
                {
                    return Version.Parse(text);
                }
            }

            return Convert.ChangeType(value, expectedType, CultureInfo.CurrentCulture);
        }

        private static IPropertyEditor ResolveEditor(PropertyInfo property)
        {
            var attribute = property.GetCustomAttribute<PropertyEditorAttribute>(inherit: true);
            if (attribute != null)
            {
                var editor = Activator.CreateInstance(attribute.EditorType) as IPropertyEditor;
                if (editor == null)
                {
                    throw new InvalidOperationException("Property editor attribute references an invalid editor type.");
                }

                return editor;
            }

            if (property.PropertyType == typeof(string) && property.Name.EndsWith("Path", StringComparison.OrdinalIgnoreCase))
            {
                return new FilePathPropertyEditor();
            }

            return new DefaultPropertyEditor();
        }

        private static object ApplyFormatter(PropertyInfo property, PropertyEditContext context, object value)
        {
            var attribute = property.GetCustomAttribute<PropertyValueFormatterAttribute>(inherit: true);
            if (attribute == null)
            {
                return value;
            }

            var formatter = Activator.CreateInstance(attribute.FormatterType) as IPropertyValueFormatter;
            if (formatter == null)
            {
                throw new InvalidOperationException("Property formatter attribute references an invalid formatter type.");
            }

            return formatter.Format(context, value);
        }

        private static bool IsSimpleType(Type type, out Type simpleType)
        {
            simpleType = type;
            if (type.IsEnum)
            {
                return true;
            }

            var primitives = new[]
            {
                typeof(bool), typeof(byte), typeof(sbyte), typeof(char), typeof(decimal), typeof(double), typeof(float),
                typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(short), typeof(ushort), typeof(string), typeof(DateTime), typeof(Guid)
            };

            if (primitives.Contains(type))
            {
                return true;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                simpleType = Nullable.GetUnderlyingType(type);
                return simpleType != null && (simpleType.IsEnum || primitives.Contains(simpleType));
            }

            return false;
        }

        private static bool IsElementType(Type type, out Type elementType)
        {
            if (type.IsArray)
            {
                elementType = type.GetElementType();
                return elementType != null;
            }

            if (IsDictionaryLikeType(type))
            {
                elementType = null;
                return false;
            }

            if (type.IsGenericType)
            {
                var genericTypeDef = type.GetGenericTypeDefinition();
                if (IsSupportedCollectionTypeDefinition(genericTypeDef))
                {
                    elementType = type.GetGenericArguments()[0];
                    return true;
                }
            }

            foreach (Type interfaceType in type.GetInterfaces())
            {
                if (!interfaceType.IsGenericType)
                {
                    continue;
                }

                var genericTypeDef = interfaceType.GetGenericTypeDefinition();
                if (IsSupportedCollectionTypeDefinition(genericTypeDef))
                {
                    elementType = interfaceType.GetGenericArguments()[0];
                    return true;
                }
            }

            elementType = null;
            return false;
        }

        private static bool IsSupportedCollectionTypeDefinition(Type genericTypeDefinition)
        {
            return genericTypeDefinition == typeof(List<>) ||
                genericTypeDefinition == typeof(IList<>) ||
                genericTypeDefinition == typeof(ICollection<>) ||
                genericTypeDefinition == typeof(IEnumerable<>) ||
                genericTypeDefinition == typeof(IReadOnlyList<>) ||
                genericTypeDefinition == typeof(IReadOnlyCollection<>) ||
                genericTypeDefinition == typeof(ISet<>);
        }

        private static bool IsDictionaryLikeType(Type type)
        {
            if (type == null)
            {
                return false;
            }

            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                return true;
            }

            Type keyType;
            Type valueType;
            return TryGetDictionaryTypes(type, out keyType, out valueType);
        }

        private static bool IsDictionaryType(Type type, out Type keyType, out Type valueType)
        {
            if (TryGetDictionaryTypes(type, out keyType, out valueType))
            {
                return true;
            }

            keyType = null;
            valueType = null;
            return false;
        }

        private static bool TryGetDictionaryTypes(Type type, out Type keyType, out Type valueType)
        {
            if (type != null && type.IsGenericType && IsSupportedDictionaryTypeDefinition(type.GetGenericTypeDefinition()))
            {
                var arguments = type.GetGenericArguments();
                keyType = arguments[0];
                valueType = arguments[1];
                return true;
            }

            if (type != null)
            {
                foreach (Type interfaceType in type.GetInterfaces())
                {
                    if (!interfaceType.IsGenericType || !IsSupportedDictionaryTypeDefinition(interfaceType.GetGenericTypeDefinition()))
                    {
                        continue;
                    }

                    var arguments = interfaceType.GetGenericArguments();
                    keyType = arguments[0];
                    valueType = arguments[1];
                    return true;
                }
            }

            keyType = null;
            valueType = null;
            return false;
        }

        private static bool IsSupportedDictionaryTypeDefinition(Type genericTypeDefinition)
        {
            return genericTypeDefinition == typeof(Dictionary<,>) ||
                genericTypeDefinition == typeof(IDictionary<,>) ||
                genericTypeDefinition == typeof(IReadOnlyDictionary<,>);
        }

        private static int GetCollectionCount(object target, PropertyInfo propertyInfo)
        {
            var value = propertyInfo.GetValue(target);
            if (value == null)
            {
                return 0;
            }

            var propertyType = propertyInfo.PropertyType;

            if (propertyType.IsArray)
            {
                return ((Array)value).Length;
            }

            if (typeof(ICollection).IsAssignableFrom(propertyType))
            {
                return ((ICollection)value).Count;
            }

            if (propertyType.IsGenericType)
            {
                var genericTypeDef = propertyType.GetGenericTypeDefinition();
                if (genericTypeDef == typeof(IEnumerable<>))
                {
                    return ((IEnumerable)value).Cast<object>().Count();
                }
            }

            if (typeof(IEnumerable).IsAssignableFrom(propertyType))
            {
                return ((IEnumerable)value).Cast<object>().Count();
            }

            throw new InvalidOperationException("Unsupported collection type.");
        }

        private static void SetCollectionProperty(object target, PropertyInfo propertyInfo, List<object> values)
        {
            var propertyType = propertyInfo.PropertyType;
            var elementType = GetElementType(propertyType);

            if (elementType == null)
            {
                return;
            }

            var convertedValues = values.Select(value => ConvertIfNeeded(value, elementType)).ToArray();

            if (propertyType.IsArray)
            {
                var array = Array.CreateInstance(elementType, convertedValues.Length);
                Array.Copy(convertedValues, array, convertedValues.Length);
                propertyInfo.SetValue(target, array);
                return;
            }

            var collection = CreateCollectionInstance(propertyType, elementType);
            if (collection == null)
            {
                return;
            }

            foreach (var value in convertedValues)
            {
                AddCollectionValue(collection, value);
            }

            propertyInfo.SetValue(target, collection);
        }

        private static object CreateCollectionInstance(Type propertyType, Type elementType)
        {
            var listType = typeof(List<>).MakeGenericType(elementType);
            if (propertyType.IsAssignableFrom(listType))
            {
                return Activator.CreateInstance(listType);
            }

            var setType = typeof(HashSet<>).MakeGenericType(elementType);
            if (propertyType.IsAssignableFrom(setType))
            {
                return Activator.CreateInstance(setType);
            }

            if (!propertyType.IsInterface && !propertyType.IsAbstract)
            {
                try
                {
                    var collection = Activator.CreateInstance(propertyType);
                    if (collection != null && HasCollectionAddMethod(collection.GetType(), elementType))
                    {
                        return collection;
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        private static bool HasCollectionAddMethod(Type collectionType, Type elementType)
        {
            return collectionType.GetMethod("Add", new[] { elementType }) != null;
        }

        private static void AddCollectionValue(object collection, object value)
        {
            if (collection is IList list)
            {
                list.Add(value);
                return;
            }

            var valueType = value != null ? value.GetType() : typeof(object);
            var addMethod = collection.GetType().GetMethod("Add", new[] { valueType });
            if (addMethod == null)
            {
                Type collectionElementType = GetElementType(collection.GetType());
                if (collectionElementType != null)
                {
                    addMethod = collection.GetType().GetMethod("Add", new[] { collectionElementType });
                }
            }

            if (addMethod != null)
            {
                addMethod.Invoke(collection, new[] { value });
            }
        }

        private static Type GetElementType(Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }

            if (type.IsGenericType)
            {
                return type.GetGenericArguments()[0];
            }

            return null;
        }

        private void EditSimpleKeyDictionary(object source, EditablePropertyDescriptor descriptor, Type simpleKeyType, Type elementType, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var property = descriptor.Property;
            var originalValue = property.GetValue(source) as IDictionary;
            if (originalValue == null)
            {
                try
                {
                    originalValue = Activator.CreateInstance(property.PropertyType) as IDictionary;
                }
                catch
                {
                    originalValue = null;
                }

                if (originalValue == null)
                {
                    originalValue = CreateDictionaryInstance(property.PropertyType, simpleKeyType, elementType);
                }

                if (originalValue == null)
                {
                    Consoul.Write("Unable to create dictionary instance for " + descriptor.DisplayName + ".", RenderOptions.InvalidColor);
                    Consoul.Wait(cancellationToken: cancellationToken);
                    return;
                }
            }

            var displayName = descriptor.DisplayName;
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = property.Name;
            }

            var updatedDictionary = EditDictionaryEntries(displayName, originalValue, property.PropertyType, simpleKeyType, elementType, cancellationToken);
            if (property.CanWrite)
            {
                property.SetValue(source, updatedDictionary);
                return;
            }

            if (originalValue != null)
            {
                originalValue.Clear();
                foreach (DictionaryEntry entry in updatedDictionary)
                {
                    originalValue[entry.Key] = entry.Value;
                }

                return;
            }

            Consoul.Write("Unable to update read-only dictionary for " + descriptor.DisplayName + ".", RenderOptions.InvalidColor);
            Consoul.Wait(cancellationToken: cancellationToken);
        }

        private IDictionary EditDictionaryEntries(string ownerDisplayName, IDictionary originalValue, Type dictionaryType, Type simpleKeyType, Type elementType, CancellationToken cancellationToken)
        {
            var prompt = new SelectionPrompt("Choose an item to edit or remove, or add a new item");
            var entries = new List<KeyValuePair<object, object>>();

            foreach (DictionaryEntry entry in originalValue)
            {
                entries.Add(new KeyValuePair<object, object>(entry.Key, entry.Value));
            }

            while (true)
            {
                prompt.Clear();
                for (int index = 0; index < entries.Count; index++)
                {
                    var pair = entries[index];
                    prompt.Add(FormatDictionaryEntry(pair));
                }

                int addChoice = prompt.Add("Add", ConsoleColor.Green);
                int finishChoice = prompt.Add("Finish", ConsoleColor.Gray);

                var result = prompt.Render(cancellationToken);
                if (result.IsCanceled)
                {
                    break;
                }

                if (!result.HasSelection)
                {
                    continue;
                }

                if (result.Index == addChoice)
                {
                    var newKey = PromptForDictionaryKey(ownerDisplayName, simpleKeyType, cancellationToken);
                    if (newKey == null)
                    {
                        continue;
                    }

                    if (entries.Any(entry => KeysEqual(entry.Key, newKey)))
                    {
                        Consoul.Write("Key '" + newKey + "' already exists.", RenderOptions.InvalidColor);
                        Consoul.Wait(cancellationToken: cancellationToken);
                        continue;
                    }

                    var defaultValue = CreateDefaultValue(elementType);
                    entries.Add(new KeyValuePair<object, object>(newKey, defaultValue));
                    continue;
                }

                if (result.Index == finishChoice)
                {
                    break;
                }

                if (result.Index < 0 || result.Index >= entries.Count)
                {
                    continue;
                }

                var selection = entries[result.Index];
                var updatedValue = EditDictionaryEntryValue(ownerDisplayName, selection.Key, selection.Value, elementType, cancellationToken);
                entries[result.Index] = new KeyValuePair<object, object>(selection.Key, updatedValue);
            }

            var updatedDictionary = CreateDictionaryInstance(dictionaryType, simpleKeyType, elementType);
            if (updatedDictionary == null)
            {
                updatedDictionary = new Dictionary<object, object>();
            }

            foreach (var entry in entries)
            {
                updatedDictionary.Add(entry.Key, entry.Value);
            }

            return updatedDictionary;
        }

        private static string FormatDictionaryEntry(KeyValuePair<object, object> entry)
        {
            var keyText = entry.Key != null ? entry.Key.ToString() : "<null>";
            string valueText;
            if (entry.Value == null)
            {
                valueText = "<null>";
            }
            else if (entry.Value is IDictionary)
            {
                valueText = "{...}";
            }
            else
            {
                valueText = entry.Value.ToString();
            }
            return keyText + " = " + valueText;
        }

        private static object PromptForDictionaryKey(string ownerDisplayName, Type simpleKeyType, CancellationToken cancellationToken)
        {
            if (simpleKeyType == null)
            {
                return null;
            }

            var label = ownerDisplayName + " key";
            return Consoul.Input("Enter new " + label + "\t(" + simpleKeyType.Name + ")", simpleKeyType, cancellationToken: cancellationToken);
        }

        private object EditDictionaryEntryValue(string ownerDisplayName, object key, object currentValue, Type elementType, CancellationToken cancellationToken)
        {
            if (elementType != null && IsSimpleType(elementType, out var simpleElementType))
            {
                var label = ownerDisplayName + "[" + key + "]";
                var value = Consoul.Input("Enter new " + label + "\t(" + simpleElementType.Name + ")", simpleElementType, cancellationToken: cancellationToken);
                if (elementType != null)
                {
                    return ConvertIfNeeded(value, elementType);
                }

                return value;
            }

            var valueInstance = currentValue;
            if (valueInstance == null)
            {
                valueInstance = CreateDefaultValue(elementType);
            }

            if (valueInstance != null)
            {
                var runtimeType = valueInstance.GetType();
                if (IsSimpleType(runtimeType, out var runtimeSimpleType))
                {
                    var labelBuilder = new StringBuilder();
                    labelBuilder.Append(ownerDisplayName);
                    labelBuilder.Append('[');
                    if (key != null)
                    {
                        labelBuilder.Append(key);
                    }
                    labelBuilder.Append(']');

                    var promptLabel = labelBuilder.ToString();
                    var runtimeValue = Consoul.Input("Enter new " + promptLabel + "\t(" + runtimeSimpleType.Name + ")", runtimeSimpleType, cancellationToken: cancellationToken);
                    return ConvertIfNeeded(runtimeValue, runtimeType);
                }
            }

            if (valueInstance is IDictionary nestedDictionary)
            {
                Type nestedKeyType;
                Type nestedValueType;
                if (!IsDictionaryType(valueInstance.GetType(), out nestedKeyType, out nestedValueType))
                {
                    if (elementType != null && IsDictionaryType(elementType, out nestedKeyType, out nestedValueType))
                    {
                        // use declared value type if runtime information is unavailable
                    }
                    else
                    {
                        nestedKeyType = typeof(object);
                        nestedValueType = typeof(object);
                    }
                }

                var nestedName = ownerDisplayName + "[" + (key != null ? key.ToString() : string.Empty) + "]";
                var updated = EditDictionaryEntries(nestedName, nestedDictionary, valueInstance.GetType(), nestedKeyType, nestedValueType, cancellationToken);
                return updated;
            }

            if (elementType != null && IsDictionaryType(elementType, out var declaredKeyType, out var declaredValueType))
            {
                var nestedValue = CreateDictionaryInstance(elementType, declaredKeyType, declaredValueType);
                if (nestedValue != null)
                {
                    var nestedName = ownerDisplayName + "[" + (key != null ? key.ToString() : string.Empty) + "]";
                    var updatedNested = EditDictionaryEntries(nestedName, nestedValue, elementType, declaredKeyType, declaredValueType, cancellationToken);
                    return updatedNested;
                }
            }

            if (valueInstance == null && elementType == null)
            {
                return null;
            }

            if (valueInstance == null)
            {
                Consoul.Write("Unable to instantiate value for key '" + key + "'.", RenderOptions.InvalidColor);
                Consoul.Wait(cancellationToken: cancellationToken);
                return currentValue;
            }

            var editor = new EditObjectView(valueInstance, _bindingFlags, _documentEditorEnabled);
            editor.RenderAsync(cancellationToken).GetAwaiter().GetResult();
            return editor.Model;
        }

        private static bool KeysEqual(object left, object right)
        {
            if (left == null && right == null)
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            return left.Equals(right);
        }

        private static IDictionary CreateDictionaryInstance(Type dictionaryType, Type keyType, Type valueType)
        {
            if (dictionaryType != null)
            {
                try
                {
                    var created = Activator.CreateInstance(dictionaryType) as IDictionary;
                    if (created != null)
                    {
                        return created;
                    }
                }
                catch
                {
                }
            }

            var resolvedKeyType = keyType ?? typeof(object);
            var resolvedValueType = valueType ?? typeof(object);

            try
            {
                var fallbackType = typeof(Dictionary<,>).MakeGenericType(resolvedKeyType, resolvedValueType);
                return Activator.CreateInstance(fallbackType) as IDictionary;
            }
            catch
            {
                return new Dictionary<object, object>();
            }
        }

        private static object CreateDefaultValue(Type type)
        {
            if (type == null)
            {
                return null;
            }

            if (type == typeof(string))
            {
                return string.Empty;
            }

            try
            {
                return Activator.CreateInstance(type);
            }
            catch
            {
                return null;
            }
        }

        private void EditSimpleCollection(object source, PropertyInfo property, Type simpleType, CancellationToken cancellationToken)
        {
            var prompt = new SelectionPrompt("Choose an item to edit or remove, or add a new item");

            var originalValue = property.GetValue(Model);
            var items = new List<object>((originalValue as IEnumerable ?? Array.Empty<object>()).Cast<object>());

            int choice = -1;
            while (choice < 0)
            {
                prompt.Clear();
                foreach (var item in items)
                {
                    prompt.Add(item?.ToString());
                }

                int addChoice = prompt.Add("Add", ConsoleColor.Green);
                int finishChoice = prompt.Add("Finish", ConsoleColor.Gray);

                var result = prompt.Render(cancellationToken);
                if (result.IsCanceled)
                {
                    break;
                }

                if (!result.HasSelection)
                {
                    choice = -1;
                    continue;
                }

                choice = result.Index;
                if (choice == addChoice)
                {
                    var newValue = EditObject(property, simpleType, cancellationToken);
                    items.Add(ConvertIfNeeded(newValue, simpleType));
                }
                else if (choice == finishChoice)
                {
                    break;
                }
                else
                {
                    var updatedValue = EditObject(property, simpleType, cancellationToken);
                    items[choice] = ConvertIfNeeded(updatedValue, simpleType);
                }

                choice = -1;
            }

            SetCollectionProperty(source, property, items);
        }

        private void EditComplexCollection(object source, PropertyInfo property, Type complexType, CancellationToken cancellationToken)
        {
            var prompt = new SelectionPrompt("Choose an item to edit or remove, or add a new item");

            var originalValue = property.GetValue(Model);
            var items = new List<object>((originalValue as IEnumerable ?? Array.Empty<object>()).Cast<object>().ToList());

            int choice = -1;
            while (choice < 0)
            {
                prompt.Clear();
                foreach (var item in items)
                {
                    prompt.Add(item?.ToString());
                }

                int addChoice = prompt.Add("Add", ConsoleColor.Green);
                int finishChoice = prompt.Add("Finish", ConsoleColor.Gray);

                var result = prompt.Render(cancellationToken);
                if (result.IsCanceled)
                {
                    break;
                }

                if (!result.HasSelection)
                {
                    choice = -1;
                    continue;
                }

                choice = result.Index;
                if (choice == addChoice)
                {
                    var newItem = Activator.CreateInstance(complexType);
                    items.Add(newItem);
                }
                else if (choice == finishChoice)
                {
                    break;
                }
                else
                {
                    var editItemView = new EditObjectView(items[choice], _bindingFlags, _documentEditorEnabled);
                    editItemView.RenderAsync(cancellationToken).GetAwaiter().GetResult();
                }

                choice = -1;
            }

            SetCollectionProperty(source, property, items);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="EditObjectView"/> for the provided <paramref name="entity"/>.
        /// </summary>
        /// <param name="entity">Entity to create an <see cref="EditObjectView"/>.</param>
        /// <param name="bindingAttr">Scope the properties that should be editable based on accessibility.</param>
        /// <returns>Dynamic view of the entity editor.</returns>
        public static EditObjectView CreateView(object entity, BindingFlags bindingAttr)
        {
            return new EditObjectView(entity, bindingAttr);
        }

        private sealed class DescriptorGroup
        {
            public DescriptorGroup(string name)
            {
                Name = name;
                Descriptors = new List<EditablePropertyDescriptor>();
            }

            public string Name { get; }

            public List<EditablePropertyDescriptor> Descriptors { get; }
        }

        private sealed class ObjectFormDocumentEditor
        {
            private readonly EditObjectView _view;
            private int _selectedIndex;

            public ObjectFormDocumentEditor(EditObjectView view)
            {
                _view = view;
            }

            public void Run(CancellationToken cancellationToken = default)
            {
                var previousCursorVisible = Consoul.ConsoleDriver.CursorVisible;
                Consoul.ConsoleDriver.CursorVisible = false;

                try
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    ConsoleKeyInfo keyInfo;
                    do
                    {
                        RenderDocument();
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        keyInfo = Consoul.ConsoleDriver.ReadKey(true);

                        if (keyInfo.Key == ConsoleKey.DownArrow || keyInfo.Key == ConsoleKey.RightArrow || keyInfo.Key == ConsoleKey.Tab)
                        {
                            MoveNext();
                        }
                        else if (keyInfo.Key == ConsoleKey.UpArrow || keyInfo.Key == ConsoleKey.LeftArrow)
                        {
                            MovePrevious();
                        }
                        else if (keyInfo.Key == ConsoleKey.Enter)
                        {
                            EditSelection();
                        }
                    }
                    while (!cancellationToken.IsCancellationRequested && keyInfo.Key != ConsoleKey.Escape && keyInfo.Key != ConsoleKey.Backspace);
                }
                finally
                {
                    Consoul.ConsoleDriver.CursorVisible = previousCursorVisible;
                    Consoul.ConsoleDriver.Clear();
                }
            }

            private void EditSelection()
            {
                if (_view._descriptors.Count == 0)
                {
                    return;
                }

                var descriptor = _view._descriptors[_selectedIndex];
                var property = descriptor.Property;

                Consoul.WriteCore(string.Empty, RenderOptions.DefaultColor);
                Consoul.WriteCore($"Editing {descriptor.DisplayName} from {_view.Model.GetType().Name}.", RenderOptions.DefaultColor);
                Consoul.WriteCore(string.Empty, RenderOptions.DefaultColor);

                var summary = descriptor.Documentation.XmlSummary;
                if (!string.IsNullOrWhiteSpace(summary))
                {
                    Consoul.WriteCore($"Description: {NormalizeWhitespace(summary)}", RenderOptions.SubnoteColor);
                }
                else if (!string.IsNullOrWhiteSpace(descriptor.Documentation.DisplayDescription))
                {
                    Consoul.WriteCore($"Description: {descriptor.Documentation.DisplayDescription}", RenderOptions.SubnoteColor);
                }

                _view.EditDescriptor(descriptor);

            }

            private static string NormalizeWhitespace(string value)
            {
                var builder = new StringBuilder();
                var space = false;
                foreach (var ch in value)
                {
                    if (ch == '\r')
                    {
                        continue;
                    }

                    if (ch == '\n')
                    {
                        if (builder.Length == 0 || builder[builder.Length - 1] != '\n')
                        {
                            builder.Append('\n');
                        }

                        space = false;
                        continue;
                    }

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

            private void MoveNext()
            {
                if (_view._descriptors.Count == 0)
                {
                    return;
                }

                _selectedIndex = (_selectedIndex + 1) % _view._descriptors.Count;
            }

            private void MovePrevious()
            {
                if (_view._descriptors.Count == 0)
                {
                    return;
                }

                _selectedIndex--;
                if (_selectedIndex < 0)
                {
                    _selectedIndex = _view._descriptors.Count - 1;
                }
            }

            private void RenderDocument()
            {
                Consoul.ConsoleDriver.Clear();
                Consoul.ConsoleDriver.WriteLine(_view.Title);
                Consoul.ConsoleDriver.WriteLine("Press Esc to return to the object editor.");
                Consoul.ConsoleDriver.WriteLine(string.Empty);
                var lines = BuildDocument();

                foreach (var line in lines)
                {
                    var highlight = line.DescriptorIndex == _selectedIndex;
                    line.Write(highlight, _view.HighlightColor);
                }
            }

            private List<DocumentLine> BuildDocument()
            {
                var lines = new List<DocumentLine>
                {
                    new DocumentLine(new[] { CreateDefaultSegment("{") }, -1)
                };

                for (var index = 0; index < _view._descriptors.Count; index++)
                {
                    var descriptor = _view._descriptors[index];
                    foreach (var commentLine in BuildCommentLines(descriptor))
                    {
                        lines.Add(commentLine);
                    }

                    var value = descriptor.Property.GetValue(_view.Model);
                    var suffix = index == _view._descriptors.Count - 1 ? string.Empty : ",";
                    lines.Add(BuildPropertyLine(descriptor, value, suffix, index));
                }

                lines.Add(new DocumentLine(new[] { CreateDefaultSegment("}") }, -1));
                return lines;
            }

            private static IEnumerable<DocumentLine> BuildCommentLines(EditablePropertyDescriptor descriptor)
            {
                var comments = new List<DocumentLine>();
                var summary = descriptor.Documentation.XmlSummary;
                var description = descriptor.Documentation.DisplayDescription;

                var text = !string.IsNullOrWhiteSpace(summary) ? summary : description;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var normalized = NormalizeForComment(text);
                    var pieces = normalized.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var piece in pieces)
                    {
                        var segmentText = "  // " + piece.Trim();
                        comments.Add(new DocumentLine(new[] { CreateCommentSegment(segmentText) }, -1));
                    }
                }

                return comments;
            }

            private static string NormalizeForComment(string text)
            {
                var builder = new StringBuilder(text.Length);
                var inTag = false;
                foreach (var ch in text)
                {
                    if (ch == '<')
                    {
                        inTag = true;
                        continue;
                    }

                    if (ch == '>')
                    {
                        inTag = false;
                        continue;
                    }

                    if (!inTag)
                    {
                        builder.Append(ch);
                    }
                }

                return NormalizeWhitespace(builder.ToString());
            }

            private DocumentLine BuildPropertyLine(EditablePropertyDescriptor descriptor, object value, string suffix, int descriptorIndex)
            {
                var segments = new List<DocumentSegment>
                {
                    CreateDefaultSegment("  "),
                    CreateStringSegment("\"" + descriptor.DisplayName + "\""),
                    CreateDefaultSegment(": ")
                };

                segments.AddRange(FormatJsonSegments(value, descriptor.Property.PropertyType));

                if (!string.IsNullOrEmpty(suffix))
                {
                    segments.Add(CreateDefaultSegment(suffix));
                }

                return new DocumentLine(segments, descriptorIndex);
            }

            private static IEnumerable<DocumentSegment> FormatJsonSegments(object value, Type valueType)
            {
                string json;
                try
                {
                    var serializer = EditObjectView.FormValueSerializer ?? SerializeJsonValueWithoutDependencies;
                    var declaredType = valueType ?? value?.GetType() ?? typeof(object);
                    json = serializer(value, declaredType) ?? SerializeJsonValueWithoutDependencies(value, declaredType);
                }
                catch
                {
                    json = SerializeJsonValueWithoutDependencies(value, valueType ?? value?.GetType() ?? typeof(object));
                }

                return TokenizeJson(json);
            }

            private static IEnumerable<DocumentSegment> TokenizeJson(string json)
            {
                var segments = new List<DocumentSegment>();
                var builder = new StringBuilder();
                var inString = false;

                for (var index = 0; index < json.Length; index++)
                {
                    var ch = json[index];

                    if (inString)
                    {
                        builder.Append(ch);
                        if (ch == '"' && !IsEscaped(json, index))
                        {
                            segments.Add(CreateStringSegment(builder.ToString()));
                            builder.Clear();
                            inString = false;
                        }

                        continue;
                    }

                    if (ch == '"')
                    {
                        if (builder.Length > 0)
                        {
                            segments.Add(CreateDefaultSegment(builder.ToString()));
                            builder.Clear();
                        }

                        builder.Append(ch);
                        inString = true;
                        continue;
                    }

                    if (char.IsDigit(ch) || (ch == '-' && index + 1 < json.Length && char.IsDigit(json[index + 1])))
                    {
                        if (builder.Length > 0)
                        {
                            segments.Add(CreateDefaultSegment(builder.ToString()));
                            builder.Clear();
                        }

                        var numberBuilder = new StringBuilder();
                        numberBuilder.Append(ch);
                        index++;
                        while (index < json.Length && (char.IsDigit(json[index]) || json[index] == '.' || json[index] == 'e' || json[index] == 'E' || json[index] == '+' || json[index] == '-'))
                        {
                            numberBuilder.Append(json[index]);
                            index++;
                        }

                        index--;
                        segments.Add(CreateNumberSegment(numberBuilder.ToString()));
                        continue;
                    }

                    builder.Append(ch);
                }

                if (builder.Length > 0)
                {
                    segments.Add(CreateDefaultSegment(builder.ToString()));
                }

                return segments;
            }

            private static bool IsEscaped(string text, int index)
            {
                var backslashes = 0;
                var current = index - 1;
                while (current >= 0 && text[current] == '\\')
                {
                    backslashes++;
                    current--;
                }

                return backslashes % 2 == 1;
            }

            private static DocumentSegment CreateDefaultSegment(string text)
            {
                return new DocumentSegment(text, ConsoleColor.White, ConsoleColor.Black);
            }

            private static DocumentSegment CreateStringSegment(string text)
            {
                return new DocumentSegment(text, ConsoleColor.Yellow, ConsoleColor.Black);
            }

            private static DocumentSegment CreateNumberSegment(string text)
            {
                return new DocumentSegment(text, ConsoleColor.Cyan, ConsoleColor.Black);
            }

            private static DocumentSegment CreateCommentSegment(string text)
            {
                return new DocumentSegment(text, ConsoleColor.DarkGreen, ConsoleColor.Black);
            }

            private sealed class DocumentLine
            {
                public DocumentLine(IEnumerable<DocumentSegment> segments, int descriptorIndex)
                {
                    if (segments == null)
                    {
                        throw new ArgumentNullException(nameof(segments));
                    }

                    Segments = new List<DocumentSegment>(segments);
                    DescriptorIndex = descriptorIndex;
                }

                public IList<DocumentSegment> Segments { get; }

                public int DescriptorIndex { get; }

                public void Write(bool highlight, ConsoleColor highlightColor)
                {
                    foreach (var segment in Segments)
                    {
                        var background = highlight ? (ConsoleColor?)highlightColor : segment.Background;
                        using (var scope = new ColorScope(segment.Foreground, background))
                        {
                            Consoul.ConsoleDriver.Write(segment.Text);
                        }
                    }

                    Consoul.ConsoleDriver.WriteLine(string.Empty);
                }
            }

            private sealed class DocumentSegment
            {
                public DocumentSegment(string text, ConsoleColor foreground, ConsoleColor background)
                {
                    Text = text ?? string.Empty;
                    Foreground = foreground;
                    Background = background;
                }

                public string Text { get; }

                public ConsoleColor Foreground { get; }

                public ConsoleColor Background { get; }
            }
        }
    }
}
