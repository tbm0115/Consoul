using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using ConsoulLibrary.Color;
using ConsoulLibrary.Views.Editing;

namespace ConsoulLibrary
{
    /// <summary>
    /// Creates a dynamic view to edit the properties of the given object.
    /// </summary>
    [View("Edit Object View", GoBackMessage = "<-- Save Changes and Go Back")]
    public class EditObjectView : DynamicView<object>
    {
        private readonly List<EditablePropertyDescriptor> _descriptors;
        private readonly BindingFlags _bindingFlags;
        private readonly bool _jsonEditorEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditObjectView"/> class.
        /// </summary>
        /// <param name="entity">The entity being edited.</param>
        /// <param name="bindingAttr">Binding flags describing the accessible properties.</param>
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
            var jsonEditorEnabled = enableJsonEditor;
            if (entityType.GetCustomAttribute<DisableJsonEditorAttribute>(true) != null)
            {
                jsonEditorEnabled = false;
            }

            _jsonEditorEnabled = jsonEditorEnabled;
            Title = BannerEntry.Render($"Edit {entityType.Name} View");

            _descriptors = entityType
                .GetProperties(bindingAttr)
                .Where(property => property.CanRead && property.CanWrite && !ShouldIgnore(property))
                .Select(property => CreateDescriptor(property))
                .ToList();

            if (_jsonEditorEnabled && _descriptors.Count > 0)
            {
                _options.Add(new DynamicOption<object>(
                    () => "Open JSON editor",
                    () => OpenJsonEditor(),
                    () => RenderOptions.OptionColor));
            }

            foreach (var descriptor in _descriptors)
            {
                AddLegacyOption(descriptor);
            }
        }

        /// <summary>
        /// Indicates whether the JSON editor should highlight values using the default highlight scheme.
        /// </summary>
        public ConsoleColor HighlightColor { get; set; } = ConsoleColor.DarkCyan;

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

            return property.GetCustomAttribute<PropertyEditorIgnoreAttribute>(true) != null;
        }

        private void AddLegacyOption(EditablePropertyDescriptor descriptor)
        {
            var property = descriptor.Property;
            var propertyType = property.PropertyType;

            if (IsSimpleType(propertyType, out var simpleType))
            {
                _options.Add(new DynamicOption<object>(
                    () => $"Edit {descriptor.DisplayName}: " + (property.GetValue(Model)?.ToString() ?? "<N/A>"),
                    () => EditDescriptor(descriptor),
                    () => ConsoleColor.Yellow));
                return;
            }

            if (IsElementType(propertyType, out var elementType))
            {
                if (IsSimpleType(elementType, out var simpleElementType))
                {
                    _options.Add(new DynamicOption<object>(
                        () => $"Edit {descriptor.DisplayName}: {simpleElementType.Name}[{GetCollectionCount(Model, property)}]",
                        () => EditDescriptor(descriptor),
                        () => ConsoleColor.DarkYellow));
                }
                else
                {
                    _options.Add(new DynamicOption<object>(
                        () => $"Edit {descriptor.DisplayName}: {elementType.Name}[{GetCollectionCount(Model, property)}]",
                        () => EditDescriptor(descriptor),
                        () => ConsoleColor.DarkYellow));
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
                        () => EditDescriptor(descriptor),
                        () => ConsoleColor.DarkYellow));
                }
                else
                {
                    _options.Add(new DynamicOption<object>(
                        () => $"View {descriptor.DisplayName}",
                        () =>
                        {
                            Consoul.Write($"Cannot render {descriptor.DisplayName} because it is a Dictionary with a complex key");
                            Consoul.Wait();
                        },
                        () => ConsoleColor.DarkRed));
                }

                return;
            }

            _options.Add(new DynamicOption<object>(
                () => $"Edit {descriptor.DisplayName}",
                () => EditDescriptor(descriptor),
                () => ConsoleColor.DarkYellow));
        }

        private void EditSimpleProperty(EditablePropertyDescriptor descriptor, Type propertyType)
        {
            var editor = descriptor.EditorOverride ?? ResolveEditor(descriptor.Property);
            var context = descriptor.CreateContext(Model);
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

        private void EditComplexProperty(EditablePropertyDescriptor descriptor)
        {
            var propertyValue = descriptor.Property.GetValue(Model) ?? Activator.CreateInstance(descriptor.Property.PropertyType);
            var recursiveView = new EditObjectView(propertyValue, _bindingFlags, _jsonEditorEnabled);
            recursiveView.Render();
            descriptor.Property.SetValue(Model, recursiveView.Model);
        }

        private object EditObject(PropertyInfo property, Type propertyType = null)
        {
            var expectedType = propertyType ?? property.PropertyType;
            return Consoul.Input($"Enter new {property.Name}\t({expectedType.Name})", expectedType);
        }

        private void EditDescriptor(EditablePropertyDescriptor descriptor)
        {
            var context = descriptor.CreateContext(Model);
            var layers = descriptor.GetLayers(context);
            if (layers.Count > 0)
            {
                var prompt = new SelectionPrompt("Choose how to edit " + descriptor.DisplayName);
                var defaultChoice = prompt.Add("Use default editor", ConsoleColor.Yellow);
                var layerChoices = new Dictionary<int, PropertyEditLayer>();

                foreach (var layer in layers)
                {
                    var index = prompt.Add(layer.DisplayName, ConsoleColor.Cyan);
                    layerChoices[index] = layer;
                }

                var result = prompt.Render();
                if (result.IsCanceled || !result.HasSelection)
                {
                    return;
                }

                if (result.Index == defaultChoice)
                {
                    EditDescriptorUsingDefaultEditor(descriptor);
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
                    Consoul.Wait();
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

                EditDescriptorUsingDefaultEditor(descriptor);
                return;
            }

            EditDescriptorUsingDefaultEditor(descriptor);
        }

        private void EditDescriptorUsingDefaultEditor(EditablePropertyDescriptor descriptor)
        {
            var propertyType = descriptor.Property.PropertyType;

            if (IsSimpleType(propertyType, out var simpleType))
            {
                EditSimpleProperty(descriptor, simpleType);
                return;
            }

            if (IsElementType(propertyType, out var elementType))
            {
                if (IsSimpleType(elementType, out var simpleElementType))
                {
                    EditSimpleCollection(Model, descriptor.Property, simpleElementType);
                }
                else
                {
                    EditComplexCollection(Model, descriptor.Property, elementType);
                }

                return;
            }

            Type keyType;
            Type valueType;
            if (IsDictionaryType(propertyType, out keyType, out valueType))
            {
                if (IsSimpleType(keyType, out var simpleKeyType))
                {
                    EditSimpleKeyDictionary(Model, descriptor, simpleKeyType, valueType);
                }
                else
                {
                    Consoul.Write($"Cannot render {descriptor.DisplayName} because it is a Dictionary with a complex key");
                    Consoul.Wait();
                }

                return;
            }

            EditComplexProperty(descriptor);
        }

        private void OpenJsonEditor()
        {
            var editor = new JsonObjectEditor(this);
            editor.Run();
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

            return Convert.ChangeType(value, expectedType);
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
                return primitives.Contains(simpleType);
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

            if (type.IsGenericType)
            {
                var genericTypeDef = type.GetGenericTypeDefinition();
                if (genericTypeDef == typeof(List<>) || genericTypeDef == typeof(IEnumerable<>) || genericTypeDef == typeof(ICollection<>))
                {
                    elementType = type.GetGenericArguments()[0];
                    return true;
                }
            }

            elementType = null;
            return false;
        }

        private static bool IsDictionaryType(Type type, out Type keyType, out Type valueType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var arguments = type.GetGenericArguments();
                keyType = arguments[0];
                valueType = arguments[1];
                return true;
            }

            keyType = null;
            valueType = null;
            return false;
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

            var convertedValues = values.Select(value => Convert.ChangeType(value, elementType)).ToArray();

            if (propertyType.IsArray)
            {
                var array = Array.CreateInstance(elementType, convertedValues.Length);
                Array.Copy(convertedValues, array, convertedValues.Length);
                propertyInfo.SetValue(target, array);
            }
            else if (propertyType.IsGenericType)
            {
                var genericTypeDef = propertyType.GetGenericTypeDefinition();

                if (genericTypeDef == typeof(List<>))
                {
                    var listType = typeof(List<>).MakeGenericType(elementType);
                    var list = (IList)Activator.CreateInstance(listType);
                    foreach (var value in convertedValues)
                    {
                        list.Add(value);
                    }

                    propertyInfo.SetValue(target, list);
                }
                else if (genericTypeDef == typeof(IEnumerable<>) || genericTypeDef == typeof(ICollection<>))
                {
                    var listType = typeof(List<>).MakeGenericType(elementType);
                    var list = (IList)Activator.CreateInstance(listType);
                    foreach (var value in convertedValues)
                    {
                        list.Add(value);
                    }

                    propertyInfo.SetValue(target, list);
                }
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

        private void EditSimpleKeyDictionary(object source, EditablePropertyDescriptor descriptor, Type simpleKeyType, Type elementType)
        {
            var property = descriptor.Property;
            var originalValue = property.GetValue(source) as IDictionary;
            if (originalValue == null)
            {
                originalValue = Activator.CreateInstance(property.PropertyType) as IDictionary;
                if (originalValue == null)
                {
                    Consoul.Write("Unable to create dictionary instance for " + descriptor.DisplayName + ".", RenderOptions.InvalidColor);
                    Consoul.Wait();
                    return;
                }
            }

            var displayName = descriptor.DisplayName;
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = property.Name;
            }

            var updatedDictionary = EditDictionaryEntries(displayName, originalValue, property.PropertyType, simpleKeyType, elementType);
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
            Consoul.Wait();
        }

        private IDictionary EditDictionaryEntries(string ownerDisplayName, IDictionary originalValue, Type dictionaryType, Type simpleKeyType, Type elementType)
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

                int addChoice = prompt.Add("Add", ConsoleColor.DarkYellow);
                int finishChoice = prompt.Add("Finish", ConsoleColor.Gray);

                var result = prompt.Render();
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
                    var newKey = PromptForDictionaryKey(ownerDisplayName, simpleKeyType);
                    if (newKey == null)
                    {
                        continue;
                    }

                    if (entries.Any(entry => KeysEqual(entry.Key, newKey)))
                    {
                        Consoul.Write("Key '" + newKey + "' already exists.", RenderOptions.InvalidColor);
                        Consoul.Wait();
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
                var updatedValue = EditDictionaryEntryValue(ownerDisplayName, selection.Key, selection.Value, elementType);
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

        private static object PromptForDictionaryKey(string ownerDisplayName, Type simpleKeyType)
        {
            if (simpleKeyType == null)
            {
                return null;
            }

            var label = ownerDisplayName + " key";
            return Consoul.Input("Enter new " + label + "\t(" + simpleKeyType.Name + ")", simpleKeyType);
        }

        private object EditDictionaryEntryValue(string ownerDisplayName, object key, object currentValue, Type elementType)
        {
            if (elementType != null && IsSimpleType(elementType, out var simpleElementType))
            {
                var label = ownerDisplayName + "[" + key + "]";
                var value = Consoul.Input("Enter new " + label + "\t(" + simpleElementType.Name + ")", simpleElementType);
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
                    var runtimeValue = Consoul.Input("Enter new " + promptLabel + "\t(" + runtimeSimpleType.Name + ")", runtimeSimpleType);
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
                var updated = EditDictionaryEntries(nestedName, nestedDictionary, valueInstance.GetType(), nestedKeyType, nestedValueType);
                return updated;
            }

            if (elementType != null && IsDictionaryType(elementType, out var declaredKeyType, out var declaredValueType))
            {
                var nestedValue = CreateDictionaryInstance(elementType, declaredKeyType, declaredValueType);
                if (nestedValue != null)
                {
                    var nestedName = ownerDisplayName + "[" + (key != null ? key.ToString() : string.Empty) + "]";
                    var updatedNested = EditDictionaryEntries(nestedName, nestedValue, elementType, declaredKeyType, declaredValueType);
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
                Consoul.Wait();
                return currentValue;
            }

            var editor = new EditObjectView(valueInstance, _bindingFlags, _jsonEditorEnabled);
            editor.Render();
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

        private void EditSimpleCollection(object source, PropertyInfo property, Type simpleType)
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

                int addChoice = prompt.Add("Add", ConsoleColor.DarkYellow);
                int finishChoice = prompt.Add("Finish", ConsoleColor.Gray);

                var result = prompt.Render();
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
                    var newValue = EditObject(property, simpleType);
                    items.Add(ConvertIfNeeded(newValue, simpleType));
                }
                else if (choice == finishChoice)
                {
                    break;
                }
                else
                {
                    var updatedValue = EditObject(property, simpleType);
                    items[choice] = ConvertIfNeeded(updatedValue, simpleType);
                }

                choice = -1;
            }

            SetCollectionProperty(source, property, items);
        }

        private void EditComplexCollection(object source, PropertyInfo property, Type complexType)
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

                int addChoice = prompt.Add("Add", ConsoleColor.DarkYellow);
                int finishChoice = prompt.Add("Finish", ConsoleColor.Gray);

                var result = prompt.Render();
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
                    var editItemView = new EditObjectView(items[choice], _bindingFlags, _jsonEditorEnabled);
                    editItemView.Render();
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

        private sealed class JsonObjectEditor
        {
            private readonly EditObjectView _view;
            private int _selectedIndex;

            public JsonObjectEditor(EditObjectView view)
            {
                _view = view;
            }

            public void Run()
            {
                var previousCursorVisible = Console.CursorVisible;
                Console.CursorVisible = false;

                try
                {
                    ConsoleKeyInfo keyInfo;
                    do
                    {
                        RenderDocument();
                        keyInfo = Console.ReadKey(true);

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
                    while (keyInfo.Key != ConsoleKey.Escape && keyInfo.Key != ConsoleKey.Backspace);
                }
                finally
                {
                    Console.CursorVisible = previousCursorVisible;
                    Console.Clear();
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

                Consoul.WriteCore(string.Empty, RenderOptions.DefaultColor);
                Consoul.WriteCore("Press any key to return to the editor…", RenderOptions.SubnoteColor);
                Console.ReadKey(true);
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
                Console.Clear();
                Console.WriteLine(_view.Title);
                Console.WriteLine();
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

                segments.AddRange(FormatJsonSegments(value));

                if (!string.IsNullOrEmpty(suffix))
                {
                    segments.Add(CreateDefaultSegment(suffix));
                }

                return new DocumentLine(segments, descriptorIndex);
            }

            private static IEnumerable<DocumentSegment> FormatJsonSegments(object value)
            {
                if (value == null)
                {
                    return new[] { CreateDefaultSegment("null") };
                }

                string json;
                try
                {
                    json = JsonSerializer.Serialize(value, value.GetType());
                }
                catch
                {
                    json = JsonSerializer.Serialize(value != null ? value.ToString() : string.Empty);
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
                            Console.Write(segment.Text);
                        }
                    }

                    Console.WriteLine();
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
