using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ConsoulLibrary
{
    /// <summary>
    /// Creates a dynamic view to edit the properties of the given object.
    /// </summary>
    [View("Edit Object View", GoBackMessage = "<-- Save Changes and Go Back")]
    public class EditObjectView : DynamicView<object>
    {

        public EditObjectView(object entity, System.Reflection.BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) : base()
        {
            Model = entity;

            var entityType = entity.GetType();
            Title = BannerEntry.Render($"Edit {entityType.Name} View");

            var properties = entityType.GetProperties(bindingAttr);
            foreach ( var property in properties )
            {
                var propertyType = property.PropertyType;
                if (IsSimpleType(propertyType, out Type simpleType))
                {
                    this._options.Add(new DynamicOption<object>(
                        () => $"Edit {property.Name}: " + property.GetValue(Model)?.ToString() ?? "<N/A>",
                        () =>
                        {
                            property.SetValue(Model, EditObject(property, simpleType));
                        },
                        () => ConsoleColor.Yellow
                    ));
                } else if (IsElementType(propertyType, out Type elementType))
                {
                    if (IsSimpleType(elementType, out Type simpleElementType))
                    {
                        // Simple "array" types should be rendered as editable Prompt
                        this._options.Add(new DynamicOption<object>(
                            () => $"Edit {property.Name}: {simpleElementType.Name}[{GetCollectionCount(Model, property)}]",
                            () => {
                                EditSimpleCollection(Model, property, simpleElementType);
                            },
                            () => ConsoleColor.DarkYellow));
                    } else
                    {
                        // Complex "array" types should be rendered as deeper EditObjectView
                        this._options.Add(new DynamicOption<object>(
                            () => $"Edit {property.Name}: {elementType.Name}[{GetCollectionCount(Model, property)}]",
                            () =>
                            {
                                EditComplexCollection(Model, property, elementType);
                            },
                            () => ConsoleColor.DarkYellow));
                    }
                } else if (IsDictionaryType(propertyType, out (Type, Type)? keyValuePairType))
                {
                    if (IsSimpleType(keyValuePairType?.Item1, out Type simpleKeyType))
                    {
                        // Simple dictionary value types should be rendered as editable Prompt
                        this._options.Add(new DynamicOption<object>(
                            () => $"Edit {property.Name}",
                            () =>
                            {
                                EditSimpleKeyDictionary(Model, property, simpleKeyType, keyValuePairType?.Item2);
                            },
                            () => ConsoleColor.DarkYellow));
                        //if (IsSimpleType(keyValuePairType?.Item2, out Type simpleValueType))
                        //{
                        //} else
                        //{
                        //    // Complex dictionary value types should be rendered as deeper EditObjectView
                        //}
                    } else
                    {
                        // Cannot render complex key
                        this._options.Add(new DynamicOption<object>(
                            () => $"View {property.Name}",
                            () => {
                                Consoul.Write($"Cannot render {property.Name} because it is a Dictionary with a complex key");
                                Consoul.Wait();
                            },
                            () => ConsoleColor.DarkRed));
                    }
                } else
                {
                    this._options.Add(new DynamicOption<object>(
                        () => $"Edit {property.Name}",
                        () =>
                        {
                            var propertyValue = property.GetValue(Model) ?? Activator.CreateInstance(propertyType);
                            var recursiveView = new EditObjectView(propertyValue);
                            recursiveView.Render();
                            property.SetValue(Model, recursiveView.Model);
                        },
                        () => ConsoleColor.DarkYellow
                    ));
                }
            }
        }

        private object EditObject(PropertyInfo property, Type propertyType = null)
        {
            Type expectedType = propertyType ?? property.PropertyType;
            return Consoul.Input($"Enter new {property.Name}\t({expectedType.Name})", expectedType);
        }

        private void EditSimpleKeyDictionary(object source, PropertyInfo property, Type simpleKeyType, Type elementType)
        {
            var prompt = new SelectionPrompt("Choose an item to edit or remove, or add a new item");

            var originalValue = property.GetValue(source) as IDictionary;
            if (originalValue == null)
                originalValue = Activator.CreateInstance(property.PropertyType) as IDictionary;
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

                choice = prompt.Render();
                if (choice == addChoice)
                {
                    var newKey = EditObject(property, simpleKeyType);
                    items.Add(newKey, Activator.CreateInstance(elementType));
                }
                else if (choice == finishChoice)
                {
                    break;
                }
                else
                {
                    // TODO: Create sub-menu for editing the key or editing the value
                    var choiceKey = items.ElementAt(choice).Key;
                    if (IsSimpleType(elementType, out Type simpleElementType))
                    {
                        items[choiceKey] = EditObject(property, simpleElementType);
                    }
                    else
                    {
                        var complexEditor = new EditObjectView(items[choiceKey]);
                        complexEditor.Render();
                        items[choiceKey] = complexEditor.Model;
                    }
                }
                choice = -1;
            }

            // Convert items back to the appropriate IDictionary type and set the property value
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
            var items = new List<object>((originalValue as object[]) ?? new object[] { });

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


                choice = prompt.Render();
                if (choice == addChoice)
                {
                    var newItem = EditObject(property, simpleType);
                    items.Add(newItem);
                } else if (choice == finishChoice)
                {
                    break;
                } else
                {
                    var updatedItem = EditObject(property, simpleType);
                    items[choice] = updatedItem;
                }
                choice = -1;
            }

            // Update property
            SetCollectionProperty(source, property, items);
        }

        private void EditComplexCollection(object source, PropertyInfo property, Type complexType)
        {
            var prompt = new SelectionPrompt("Choose an item to edit or remove, or add a new item");

            var originalValue = property.GetValue(Model);
            var items = new List<object>((originalValue as object[]) ?? new object[] { });

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

                choice = prompt.Render();
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
                    var editItemView = new EditObjectView(items[choice]);
                    editItemView.Render();
                }
                choice = -1;
            }

            // Update property
            SetCollectionProperty(source, property, items);

        }

        private static Type[] simpleTypes = new Type[]
        {
            typeof(bool),
            typeof(byte),
            typeof(sbyte),
            typeof(char),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(short),
            typeof(ushort),
            typeof(string),
            typeof(DateTime),
            typeof(Guid)
        };
        private static bool IsSimpleType(Type type, out Type simpleType)
        {
            simpleType = type;
            if (simpleTypes.Contains(type))
                return true;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                Type underlyingType = Nullable.GetUnderlyingType(type);
                simpleType = underlyingType;
                return simpleTypes.Contains(underlyingType);
            }
            return false;
        }
        private static bool IsElementType(Type type, out Type elementType)
        {
            if (type.IsArray)
            {
                elementType = type.GetElementType();
                return true;
            }

            if (type.IsGenericType)
            {
                Type genericTypeDef = type.GetGenericTypeDefinition();

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
            if (type.IsGenericType)
            {
                Type genericTypeDef = type.GetGenericTypeDefinition();

                if (genericTypeDef == typeof(Dictionary<,>))
                {
                    Type keyType = type.GetGenericArguments()[0];
                    Type valueType = type.GetGenericArguments()[1];
                    keyValuePairType = (keyType, valueType);
                    return true;
                }
            }

            keyValuePairType = null;
            return false;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="EditObjectView"/> for the provided <paramref name="entity"/>
        /// </summary>
        /// <param name="entity">Entity to create an <see cref="EditObjectView"/></param>
        /// <param name="bindingAttr">Scope the properties that should be editable based on accessibility</param>
        /// <returns>Dynamic view of the entity editor</returns>
        public static EditObjectView CreateView(object entity, BindingFlags bindingAttr)
        {
            var view = new EditObjectView(entity, bindingAttr);
            return view;
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

            // If it's an IEnumerable but not a generic one, use LINQ to count
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
                else if (genericTypeDef == typeof(Dictionary<,>))
                {
                    // Handle dictionary if necessary
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
    }
}
