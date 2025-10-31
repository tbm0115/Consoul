using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace ConsoulLibrary.Views.Editing
{
    /// <summary>
    /// Identifies the properties that describe a remote constructor so options can be resolved dynamically.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class RemoteConstructorOptionsAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteConstructorOptionsAttribute"/> class.
        /// </summary>
        /// <param name="assemblyPathPropertyName">Property on the containing model that supplies the assembly path.</param>
        /// <param name="typeNamePropertyName">Property on the containing model that supplies the fully qualified type name.</param>
        public RemoteConstructorOptionsAttribute(string assemblyPathPropertyName, string typeNamePropertyName)
        {
            if (string.IsNullOrWhiteSpace(assemblyPathPropertyName))
            {
                throw new ArgumentException("Assembly path property name must be supplied.", nameof(assemblyPathPropertyName));
            }

            if (string.IsNullOrWhiteSpace(typeNamePropertyName))
            {
                throw new ArgumentException("Type name property name must be supplied.", nameof(typeNamePropertyName));
            }

            AssemblyPathPropertyName = assemblyPathPropertyName;
            TypeNamePropertyName = typeNamePropertyName;
        }

        /// <summary>
        /// Gets the name of the property that provides the assembly path.
        /// </summary>
        public string AssemblyPathPropertyName { get; }

        /// <summary>
        /// Gets the name of the property that provides the fully qualified type name.
        /// </summary>
        public string TypeNamePropertyName { get; }

        /// <summary>
        /// Gets or sets a display name that should override the property's resolved display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets a description presented when editing the property.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets additional instructions appended to the property's XML documentation summary.
        /// </summary>
        public string Instructions { get; set; }
    }

    /// <summary>
    /// Provides metadata that enables <see cref="EditObjectView"/> to inspect remote constructor definitions.
    /// </summary>
    public sealed class RemoteConstructorMetadataResolver : IPropertyMetadataResolver
    {
        /// <inheritdoc />
        public ResolvedPropertyMetadata Resolve(PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            var options = property.GetCustomAttribute<RemoteConstructorOptionsAttribute>(true);
            if (options == null)
            {
                throw new InvalidOperationException("Remote constructor metadata requires the RemoteConstructorOptionsAttribute.");
            }

            var documentation = ComposeDocumentation(property, options);
            var layerProvider = new RemoteConstructorLayerProvider(property, options);

            return new ResolvedPropertyMetadata(documentation, new DefaultPropertyEditor(), null, layerProvider);
        }

        private static PropertyDocumentation ComposeDocumentation(PropertyInfo property, RemoteConstructorOptionsAttribute options)
        {
            var baseDocumentation = new PropertyDocumentation(property);
            var hasCustomDisplay = !string.IsNullOrWhiteSpace(options.DisplayName) || !string.IsNullOrWhiteSpace(options.Description) || !string.IsNullOrWhiteSpace(options.Instructions);
            if (!hasCustomDisplay)
            {
                return baseDocumentation;
            }

            var displayName = !string.IsNullOrWhiteSpace(options.DisplayName) ? options.DisplayName : baseDocumentation.DisplayName;
            var description = !string.IsNullOrWhiteSpace(options.Description) ? options.Description : baseDocumentation.DisplayDescription;
            var baseSummary = baseDocumentation.XmlSummary;

            Func<string> xmlSummaryProvider = delegate
            {
                if (!string.IsNullOrWhiteSpace(options.Instructions))
                {
                    if (string.IsNullOrWhiteSpace(baseSummary))
                    {
                        return options.Instructions;
                    }

                    return baseSummary + Environment.NewLine + options.Instructions;
                }

                return baseSummary;
            };

            return new PropertyDocumentation(property, displayName, description, xmlSummaryProvider);
        }
    }

    /// <summary>
    /// Represents a nested dictionary describing a remote constructor and its parameters.
    /// </summary>
    internal sealed class RemoteConstructorParameterDictionary
    {
        /// <summary>
        /// Gets or sets the assembly path that declares the constructor.
        /// </summary>
        [PropertyEditorIgnore]
        public string AssemblyPath { get; set; }

        /// <summary>
        /// Gets or sets the fully qualified type name containing the constructor.
        /// </summary>
        [PropertyEditorIgnore]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the display name for the constructor or type.
        /// </summary>
        [PropertyEditorIgnore]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the documentation describing the constructor.
        /// </summary>
        [PropertyEditorIgnore]
        public string Documentation { get; set; }

        /// <summary>
        /// Gets the constructor parameters that should be edited.
        /// </summary>
        [PropertyMetadataResolver(typeof(RemoteConstructorMetadataResolver))]
        [RemoteConstructorOptions(nameof(AssemblyPath), nameof(Type))]
        public Dictionary<string, object> CtorParameters { get; } = new Dictionary<string, object>();
    }

    internal sealed class RemoteConstructorLayerProvider : IPropertyLayerProvider
    {
        private readonly PropertyInfo _property;
        private readonly RemoteConstructorOptionsAttribute _options;

        public RemoteConstructorLayerProvider(PropertyInfo property, RemoteConstructorOptionsAttribute options)
        {
            _property = property;
            _options = options;
        }

        public IEnumerable<PropertyEditLayer> GetLayers(PropertyEditContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Model is RemoteConstructorParameterDictionary)
            {
                return Array.Empty<PropertyEditLayer>();
            }

            return new[]
            {
                new PropertyEditLayer(
                    "Edit constructor parameters",
                    "Loads the remote constructor definition and opens an editor for each parameter.",
                    RunConstructorEditor,
                    false)
            };
        }

        private bool RunConstructorEditor(PropertyEditContext context)
        {
            string assemblyPath;
            string targetType;
            string displayName;
            string documentation;
            if (!TryResolveTarget(context, out assemblyPath, out targetType, out displayName, out documentation))
            {
                Consoul.Write("Unable to resolve the remote type for '" + _property.Name + "'.", ConsoleColor.Red);
                return false;
            }

            IDictionary existingValues = context.CurrentValue as IDictionary;
            RemoteConstructorParameterDictionary parameters;
            try
            {
                parameters = BuildConstructorDictionary(assemblyPath, targetType, existingValues);
            }
            catch (Exception exception)
            {
                Consoul.Write("Failed to inspect constructor: " + exception.Message, ConsoleColor.Red);
                Consoul.Wait();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                Consoul.Write("Editing " + displayName + " parameters for " + targetType + ".", ConsoleColor.Cyan);
            }

            if (!string.IsNullOrWhiteSpace(documentation))
            {
                Consoul.Write(documentation, ConsoleColor.DarkGreen);
            }

            var view = new EditObjectView(parameters, false);
            view.Render();

            var flattened = FlattenParameters(parameters.CtorParameters);
            ApplyResolvedParameters(context, flattened);
            return false;
        }

        private bool TryResolveTarget(PropertyEditContext context, out string assemblyPath, out string targetType, out string displayName, out string documentation)
        {
            assemblyPath = null;
            targetType = null;
            displayName = null;
            documentation = null;

            var dictionaryModel = context.Model as RemoteConstructorParameterDictionary;
            if (dictionaryModel != null)
            {
                assemblyPath = dictionaryModel.AssemblyPath;
                targetType = dictionaryModel.Type;
                displayName = dictionaryModel.DisplayName;
                documentation = dictionaryModel.Documentation;
                return !string.IsNullOrWhiteSpace(assemblyPath) && !string.IsNullOrWhiteSpace(targetType);
            }

            var model = context.Model;
            if (model == null)
            {
                return false;
            }

            var modelType = model.GetType();
            var assemblyProperty = modelType.GetProperty(_options.AssemblyPathPropertyName);
            var typeProperty = modelType.GetProperty(_options.TypeNamePropertyName);

            if (assemblyProperty != null)
            {
                var value = assemblyProperty.GetValue(model);
                assemblyPath = value != null ? value.ToString() : null;
            }

            if (typeProperty != null)
            {
                var value = typeProperty.GetValue(model);
                targetType = value != null ? value.ToString() : null;
                displayName = context.Documentation.DisplayName;
                if (string.IsNullOrWhiteSpace(displayName))
                {
                    displayName = typeProperty.Name;
                }
            }

            return !string.IsNullOrWhiteSpace(assemblyPath) && !string.IsNullOrWhiteSpace(targetType);
        }

        private static void ApplyResolvedParameters(PropertyEditContext context, Dictionary<string, object> values)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (values == null)
            {
                return;
            }

            if (context.Property.CanWrite)
            {
                context.ApplyValue(values);
                return;
            }

            var destination = context.Property.GetValue(context.Model) as IDictionary;
            if (destination != null)
            {
                destination.Clear();
                foreach (var kvp in values)
                {
                    destination[kvp.Key] = kvp.Value;
                }

                context.CurrentValue = destination;
                return;
            }

            context.CurrentValue = values;
        }

        private static RemoteConstructorParameterDictionary BuildConstructorDictionary(string assemblyPath, string targetType, IDictionary existingValues)
        {
            try
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                var type = assembly.GetType(targetType, true);
                return BuildConstructorDictionaryFromType(assemblyPath, targetType, existingValues, type);
            }
            catch (Exception loadException)
            {
                Consoul.Write("Falling back to XML documentation: " + loadException.Message, ConsoleColor.DarkYellow);
                var fallback = BuildConstructorDictionaryFromDocumentation(assemblyPath, targetType, existingValues);
                if (fallback != null)
                {
                    return fallback;
                }

                throw;
            }
        }

        private static RemoteConstructorParameterDictionary BuildConstructorDictionaryFromType(string assemblyPath, string targetType, IDictionary existingValues, Type type)
        {
            var constructors = type.GetConstructors();
            var constructor = constructors.OrderByDescending(info => info.GetParameters().Length).FirstOrDefault();
            if (constructor == null)
            {
                throw new InvalidOperationException("Target type does not expose a public constructor.");
            }

            var dictionary = new RemoteConstructorParameterDictionary
            {
                AssemblyPath = assemblyPath,
                Type = targetType,
                DisplayName = type.Name,
                Documentation = LookupTypeSummary(type)
            };

            foreach (var parameter in constructor.GetParameters())
            {
                object existingValue = null;
                if (existingValues != null && existingValues.Contains(parameter.Name))
                {
                    existingValue = existingValues[parameter.Name];
                }

                if (existingValue is RemoteConstructorParameterDictionary nestedDictionary)
                {
                    dictionary.CtorParameters.Add(parameter.Name, nestedDictionary);
                    continue;
                }

                if (existingValue is IDictionary nestedValues && !IsSimpleParameter(parameter.ParameterType))
                {
                    var nested = new RemoteConstructorParameterDictionary
                    {
                        AssemblyPath = assemblyPath,
                        Type = parameter.ParameterType.FullName,
                        DisplayName = parameter.Name,
                        Documentation = LookupParameterSummary(parameter)
                    };

                    foreach (DictionaryEntry entry in nestedValues)
                    {
                        var keyText = entry.Key != null ? entry.Key.ToString() : string.Empty;
                        if (!nested.CtorParameters.ContainsKey(keyText))
                        {
                            nested.CtorParameters.Add(keyText, entry.Value);
                        }
                    }

                    dictionary.CtorParameters.Add(parameter.Name, nested);
                    continue;
                }

                if (existingValue != null)
                {
                    dictionary.CtorParameters.Add(parameter.Name, existingValue);
                    continue;
                }

                object defaultValue = null;
                if (IsSimpleParameter(parameter.ParameterType))
                {
                    if (parameter.HasDefaultValue)
                    {
                        defaultValue = parameter.DefaultValue;
                    }
                    else
                    {
                        defaultValue = string.Empty;
                    }
                }
                else
                {
                    defaultValue = new RemoteConstructorParameterDictionary
                    {
                        AssemblyPath = assemblyPath,
                        Type = parameter.ParameterType.FullName,
                        DisplayName = parameter.Name,
                        Documentation = LookupParameterSummary(parameter)
                    };
                }

                dictionary.CtorParameters.Add(parameter.Name, defaultValue);
            }

            return dictionary;
        }

        private static RemoteConstructorParameterDictionary BuildConstructorDictionaryFromDocumentation(string assemblyPath, string targetType, IDictionary existingValues)
        {
            var documentationPath = Path.ChangeExtension(assemblyPath, ".xml");
            if (string.IsNullOrEmpty(documentationPath) || !File.Exists(documentationPath))
            {
                return null;
            }

            var document = new XmlDocument();
            document.Load(documentationPath);

            var constructorNodes = document.SelectNodes("/doc/members/member[starts-with(@name, 'M:" + targetType + ".#ctor')]");
            if (constructorNodes == null || constructorNodes.Count == 0)
            {
                return null;
            }

            var dictionary = new RemoteConstructorParameterDictionary
            {
                AssemblyPath = assemblyPath,
                Type = targetType,
                DisplayName = GetDisplayName(targetType),
                Documentation = LookupTypeSummary(document, targetType)
            };

            var constructor = constructorNodes[0] as XmlElement;
            if (constructor == null)
            {
                return dictionary;
            }

            var signature = constructor.GetAttribute("name");
            var parameterTypeNames = ParseParameterTypes(signature);
            var parameters = constructor.SelectNodes("param");
            if (parameters != null)
            {
                for (var index = 0; index < parameters.Count; index++)
                {
                    var element = parameters[index] as XmlElement;
                    if (element == null)
                    {
                        continue;
                    }

                    var parameterName = element.GetAttribute("name");
                    object value = null;
                    if (existingValues != null && existingValues.Contains(parameterName))
                    {
                        value = existingValues[parameterName];
                    }
                    else
                    {
                        var parameterTypeName = parameterTypeNames.Count > index ? parameterTypeNames[index] : string.Empty;
                        value = CreateFallbackValue(parameterTypeName, assemblyPath, document);
                    }

                    if (!dictionary.CtorParameters.ContainsKey(parameterName))
                    {
                        dictionary.CtorParameters.Add(parameterName, value);
                    }
                }
            }

            return dictionary;
        }

        private static object CreateFallbackValue(string typeName, string assemblyPath, XmlDocument document)
        {
            if (IsSimpleParameter(typeName))
            {
                return string.Empty;
            }

            var nested = new RemoteConstructorParameterDictionary
            {
                DisplayName = GetDisplayName(typeName),
                Type = typeName,
                AssemblyPath = assemblyPath,
                Documentation = LookupTypeSummary(document, typeName)
            };

            return nested;
        }

        private static bool IsSimpleParameter(Type type)
        {
            if (type == null)
            {
                return true;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return IsSimpleParameter(type.GetGenericArguments()[0]);
            }

            if (type.IsPrimitive || type.IsEnum)
            {
                return true;
            }

            if (type == typeof(string) || type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(decimal))
            {
                return true;
            }

            if (type == typeof(Guid) || type == typeof(TimeSpan))
            {
                return true;
            }

            if (type == typeof(Uri))
            {
                return true;
            }

            return false;
        }

        private static bool IsSimpleParameter(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return true;
            }

            var normalized = typeName.Trim();
            if (normalized.StartsWith("System.Nullable", StringComparison.Ordinal))
            {
                var start = normalized.IndexOf('{');
                var end = normalized.LastIndexOf('}');
                if (start >= 0 && end > start)
                {
                    var inner = normalized.Substring(start + 1, end - start - 1);
                    return IsSimpleParameter(inner);
                }

                return true;
            }
            if (normalized.IndexOf("System.", StringComparison.Ordinal) == 0)
            {
                normalized = normalized.Substring("System.".Length);
            }

            switch (normalized)
            {
                case "Boolean":
                case "Byte":
                case "Char":
                case "Decimal":
                case "Double":
                case "Guid":
                case "Int16":
                case "Int32":
                case "Int64":
                case "SByte":
                case "Single":
                case "String":
                case "TimeSpan":
                case "UInt16":
                case "UInt32":
                case "UInt64":
                case "DateTime":
                case "DateTimeOffset":
                case "Uri":
                    return true;
            }

            return false;
        }

        private static List<string> ParseParameterTypes(string signature)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(signature))
            {
                return result;
            }

            var start = signature.IndexOf('(');
            var end = signature.LastIndexOf(')');
            if (start < 0 || end < 0 || end <= start)
            {
                return result;
            }

            var parameters = signature.Substring(start + 1, end - start - 1);
            if (string.IsNullOrWhiteSpace(parameters))
            {
                return result;
            }

            foreach (var part in parameters.Split(','))
            {
                result.Add(part.Trim());
            }

            return result;
        }

        private static Dictionary<string, object> FlattenParameters(Dictionary<string, object> source)
        {
            var result = new Dictionary<string, object>();
            foreach (var kvp in source)
            {
                if (kvp.Value is RemoteConstructorParameterDictionary nested)
                {
                    result[kvp.Key] = FlattenParameters(nested.CtorParameters);
                }
                else if (kvp.Value is Dictionary<string, object> nestedDictionary)
                {
                    result[kvp.Key] = FlattenParameters(nestedDictionary);
                }
                else
                {
                    result[kvp.Key] = kvp.Value;
                }
            }

            return result;
        }

        private static string LookupTypeSummary(Type type)
        {
            try
            {
                var assembly = type.Assembly;
                var fileName = Path.GetFileNameWithoutExtension(assembly.Location) + ".xml";
                var directory = Path.GetDirectoryName(assembly.Location);
                if (string.IsNullOrEmpty(directory))
                {
                    return string.Empty;
                }

                var xmlPath = Path.Combine(directory, fileName);
                if (!File.Exists(xmlPath))
                {
                    return string.Empty;
                }

                var document = new XmlDocument();
                document.Load(xmlPath);
                return LookupTypeSummary(document, type.FullName);
            }
            catch
            {
            }

            return string.Empty;
        }

        private static string LookupTypeSummary(XmlDocument document, string typeName)
        {
            try
            {
                var memberName = "T:" + typeName;
                var node = document.SelectSingleNode("/doc/members/member[@name='" + memberName + "']/summary");
                if (node != null)
                {
                    return node.InnerText.Trim();
                }
            }
            catch
            {
            }

            return string.Empty;
        }

        private static string LookupParameterSummary(ParameterInfo parameter)
        {
            try
            {
                var assembly = parameter.Member.Module.Assembly;
                var directory = Path.GetDirectoryName(assembly.Location);
                if (string.IsNullOrEmpty(directory))
                {
                    return string.Empty;
                }

                var xmlPath = Path.Combine(directory, Path.GetFileNameWithoutExtension(assembly.Location) + ".xml");
                if (!File.Exists(xmlPath))
                {
                    return string.Empty;
                }

                var document = new XmlDocument();
                document.Load(xmlPath);
                var typeName = parameter.Member.DeclaringType.FullName;
                var ctorName = parameter.Member.Name == ".ctor" ? "ctor" : parameter.Member.Name;
                var memberSignature = "M:" + typeName + "." + ctorName;

                var memberNodes = document.SelectNodes("/doc/members/member[starts-with(@name, '" + memberSignature + "')]");
                if (memberNodes == null)
                {
                    return string.Empty;
                }

                foreach (XmlNode member in memberNodes)
                {
                    var parameterNodes = member.SelectNodes("param");
                    if (parameterNodes == null)
                    {
                        continue;
                    }

                    foreach (XmlNode param in parameterNodes)
                    {
                        var element = param as XmlElement;
                        if (element != null && string.Equals(element.GetAttribute("name"), parameter.Name, StringComparison.Ordinal))
                        {
                            return element.InnerText.Trim();
                        }
                    }
                }
            }
            catch
            {
            }

            return string.Empty;
        }

        private static string GetDisplayName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return string.Empty;
            }

            var index = typeName.LastIndexOf('.');
            if (index >= 0 && index < typeName.Length - 1)
            {
                return typeName.Substring(index + 1);
            }

            return typeName;
        }
    }
}
