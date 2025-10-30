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

        /// <summary>
        /// Initializes a new instance of the <see cref="EditObjectView"/> class.
        /// </summary>
        /// <param name="entity">The entity being edited.</param>
        /// <param name="bindingAttr">Binding flags describing the accessible properties.</param>
        public EditObjectView(object entity, BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            : base()
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            Model = entity;
            _bindingFlags = bindingAttr;

            var entityType = entity.GetType();
            Title = BannerEntry.Render($"Edit {entityType.Name} View");

            _descriptors = entityType
                .GetProperties(bindingAttr)
                .Where(property => property.CanRead && property.CanWrite)
                .Select(property => new EditablePropertyDescriptor(property, new PropertyDocumentation(property)))
                .ToList();

            if (_descriptors.Count > 0)
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

        private void AddLegacyOption(EditablePropertyDescriptor descriptor)
        {
            var property = descriptor.Property;
            var propertyType = property.PropertyType;

            if (IsSimpleType(propertyType, out var simpleType))
            {
                _options.Add(new DynamicOption<object>(
                    () => $"Edit {descriptor.DisplayName}: " + (property.GetValue(Model)?.ToString() ?? "<N/A>"),
                    () => EditSimpleProperty(descriptor, simpleType),
                    () => ConsoleColor.Yellow));
                return;
            }

            if (IsElementType(propertyType, out var elementType))
            {
                if (IsSimpleType(elementType, out var simpleElementType))
                {
                    _options.Add(new DynamicOption<object>(
                        () => $"Edit {descriptor.DisplayName}: {simpleElementType.Name}[{GetCollectionCount(Model, property)}]",
                        () => EditSimpleCollection(Model, property, simpleElementType),
                        () => ConsoleColor.DarkYellow));
                }
                else
                {
                    _options.Add(new DynamicOption<object>(
                        () => $"Edit {descriptor.DisplayName}: {elementType.Name}[{GetCollectionCount(Model, property)}]",
                        () => EditComplexCollection(Model, property, elementType),
                        () => ConsoleColor.DarkYellow));
                }

                return;
            }

            if (IsDictionaryType(propertyType, out var keyValuePairType))
            {
                if (IsSimpleType(keyValuePairType?.Item1, out var simpleKeyType))
                {
                    _options.Add(new DynamicOption<object>(
                        () => $"Edit {descriptor.DisplayName}",
                        () => EditSimpleKeyDictionary(Model, property, simpleKeyType, keyValuePairType?.Item2),
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
                () => EditComplexProperty(descriptor),
                () => ConsoleColor.DarkYellow));
        }

        private void EditSimpleProperty(EditablePropertyDescriptor descriptor, Type propertyType)
        {
            var editor = ResolveEditor(descriptor.Property);
            var context = descriptor.CreateContext(Model);
            if (editor.TryEdit(context, out var value))
            {
                value = ApplyFormatter(descriptor.Property, context, value);
                value = ConvertIfNeeded(value, propertyType);
                context.CurrentValue = value;
                descriptor.Property.SetValue(Model, value);
            }
        }

        private void EditComplexProperty(EditablePropertyDescriptor descriptor)
        {
            var propertyValue = descriptor.Property.GetValue(Model) ?? Activator.CreateInstance(descriptor.Property.PropertyType);
            var recursiveView = new EditObjectView(propertyValue, _bindingFlags);
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

            if (IsDictionaryType(propertyType, out var keyValueType))
            {
                if (IsSimpleType(keyValueType?.Item1, out var simpleKeyType))
                {
                    EditSimpleKeyDictionary(Model, descriptor.Property, simpleKeyType, keyValueType?.Item2);
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

        private static object? ConvertIfNeeded(object? value, Type targetType)
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
                return (IPropertyEditor)Activator.CreateInstance(attribute.EditorType)!;
            }

            if (property.PropertyType == typeof(string) && property.Name.EndsWith("Path", StringComparison.OrdinalIgnoreCase))
            {
                return new FilePathPropertyEditor();
            }

            return new DefaultPropertyEditor();
        }

        private static object? ApplyFormatter(PropertyInfo property, PropertyEditContext context, object? value)
        {
            var attribute = property.GetCustomAttribute<PropertyValueFormatterAttribute>(inherit: true);
            if (attribute == null)
            {
                return value;
            }

            var formatter = (IPropertyValueFormatter)Activator.CreateInstance(attribute.FormatterType)!;
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

        private static bool IsDictionaryType(Type type, out (Type, Type)? keyValuePairType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var arguments = type.GetGenericArguments();
                keyValuePairType = (arguments[0], arguments[1]);
                return true;
            }

            keyValuePairType = null;
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

        private void EditSimpleKeyDictionary(object source, PropertyInfo property, Type simpleKeyType, Type elementType)
        {
            var prompt = new SelectionPrompt("Choose an item to edit or remove, or add a new item");

            var originalValue = property.GetValue(source) as IDictionary;
            if (originalValue == null)
            {
                originalValue = Activator.CreateInstance(property.PropertyType) as IDictionary;
            }

            var items = new Dictionary<object, object>();
            foreach (var key in originalValue.Keys)
            {
                items.Add(key, originalValue[key]);
            }

            int choice = -1;
            while (choice < 0)
            {
                prompt.Clear();
                foreach (var item in items)
                {
                    prompt.Add(item.ToString());
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
                    var newKey = EditObject(property, simpleKeyType);
                    items.Add(newKey, elementType != null ? Activator.CreateInstance(elementType) : null);
                }
                else if (choice == finishChoice)
                {
                    break;
                }
                else
                {
                    var choiceKey = items.ElementAt(choice).Key;
                    if (elementType != null && IsSimpleType(elementType, out var simpleElementType))
                    {
                        items[choiceKey] = EditObject(property, simpleElementType);
                    }
                    else if (elementType != null)
                    {
                        var itemValue = items[choiceKey] ?? Activator.CreateInstance(elementType);
                        var complexEditor = new EditObjectView(itemValue);
                        complexEditor.Render();
                        items[choiceKey] = complexEditor.Model;
                    }
                }

                choice = -1;
            }

            var updatedDictionary = Activator.CreateInstance(property.PropertyType) as IDictionary;
            foreach (var item in items)
            {
                updatedDictionary.Add(item.Key, item.Value);
            }

            property.SetValue(source, updatedDictionary);
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
                    var editItemView = new EditObjectView(items[choice], _bindingFlags);
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
                        if (builder.Length == 0 || builder[^1] != '\n')
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

                for (var i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];
                    if (line.DescriptorIndex == _selectedIndex)
                    {
                        using (new ColorScope(RenderOptions.DefaultColor, _view.HighlightColor))
                        {
                            Console.WriteLine(line.Text);
                        }
                    }
                    else
                    {
                        Console.WriteLine(line.Text);
                    }
                }
            }

            private List<DocumentLine> BuildDocument()
            {
                var lines = new List<DocumentLine>
                {
                    new DocumentLine("{", -1)
                };

                for (var index = 0; index < _view._descriptors.Count; index++)
                {
                    var descriptor = _view._descriptors[index];
                    var summaryLines = BuildCommentLines(descriptor);
                    foreach (var line in summaryLines)
                    {
                        lines.Add(new DocumentLine(line, -1));
                    }

                    var value = descriptor.Property.GetValue(_view.Model);
                    var jsonValue = FormatJsonValue(value);
                    var suffix = index == _view._descriptors.Count - 1 ? string.Empty : ",";
                    lines.Add(new DocumentLine($"  \"{descriptor.DisplayName}\": {jsonValue}{suffix}", index));
                }

                lines.Add(new DocumentLine("}", -1));
                return lines;
            }

            private static IEnumerable<string> BuildCommentLines(EditablePropertyDescriptor descriptor)
            {
                var comments = new List<string>();
                var summary = descriptor.Documentation.XmlSummary;
                var description = descriptor.Documentation.DisplayDescription;

                var text = !string.IsNullOrWhiteSpace(summary) ? summary : description;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    foreach (var line in NormalizeForComment(text).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        comments.Add($"  // {line.Trim()}");
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

            private static string FormatJsonValue(object value)
            {
                if (value == null)
                {
                    return "null";
                }

                try
                {
                    return JsonSerializer.Serialize(value, value.GetType());
                }
                catch
                {
                    return JsonSerializer.Serialize(value.ToString());
                }
            }

            private readonly struct DocumentLine
            {
                public DocumentLine(string text, int descriptorIndex)
                {
                    Text = text;
                    DescriptorIndex = descriptorIndex;
                }

                public string Text { get; }

                public int DescriptorIndex { get; }
            }
        }
    }
}
