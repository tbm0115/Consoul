using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using ConsoulLibrary.Views.Editing;

namespace ConsoulLibrary.Test.Views
{
    /// <summary>
    /// Demonstrates the <see cref="EditObjectView"/> using nested dictionaries and custom metadata layers.
    /// </summary>
    public class EntityEditorView : StaticView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityEditorView"/> class.
        /// </summary>
        public EntityEditorView()
        {
            Title = new BannerEntry("Edit Object View").Message;
        }

        /// <summary>
        /// Opens the editor for a simple person model.
        /// </summary>
        [ViewOption("Edit Person")]
        internal void EditPerson()
        {
            var person = new Person();
            var view = new EditObjectView(person);
            view.Render();

            var name = (person.LastName ?? string.Empty) + ", " + (person.FirstName ?? string.Empty);
            var birthDate = person.DateOfBirth.HasValue ? person.DateOfBirth.Value.ToString("MM/dd/yyyy") : "<N/A>";
            Consoul.Write(name + " created on " + birthDate + "!", ConsoleColor.Green);

            if (person.Spouse != null)
            {
                var spouseName = person.Spouse.Spouse != null ? person.Spouse.Spouse.FirstName : string.Empty;
                Consoul.Write("\tMarried to " + spouseName + " on " + person.Spouse.Anniversary.ToString("MM/dd/yyyy"));
            }

            if (person.Children != null)
            {
                Consoul.Write("\tHas Children");
                foreach (var child in person.Children)
                {
                    var childBirth = child.Value.DateOfBirth.HasValue ? child.Value.DateOfBirth.Value.ToString("MM/dd/yyyy") : "<N/A>";
                    Consoul.Write("\t\t" + child.Value.FirstName + ", created on " + childBirth);
                }
            }

            Consoul.Wait();
        }

        /// <summary>
        /// Opens the editor for a mixed object that resolves remote constructor metadata.
        /// </summary>
        [ViewOption("Edit Mixed Object")]
        public void EditMixedObject()
        {
            var adapterType = typeof(RemoteConstructorLayerProvider.SampleRemoteAdapter);
            var mixedObject = new MixedObject
            {
                AdapterSourceAssembly = adapterType.Assembly.Location,
                AdapterSourceType = adapterType.FullName,
                Options = new Dictionary<string, object>()
            };

            var view = new EditObjectView(mixedObject);
            view.Render();

            if (mixedObject.Options != null && mixedObject.Options.Count > 0)
            {
                foreach (var kvp in mixedObject.Options)
                {
                    Consoul.Write("\t{Key:White}: {Value:Green}", args: new object[] { kvp.Key, kvp.Value ?? string.Empty });
                }
            }
            else
            {
                Consoul.Write("No constructor options found!", ConsoleColor.Red);
            }

            Consoul.Wait();
        }
    }

    /// <summary>
    /// Sample model demonstrating person editing.
    /// </summary>
    public class Person
    {
        /// <summary>
        /// Legal first name of the person.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Legal last name of the person.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Legal date of birth of the person.
        /// </summary>
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// Legal married spouse of the person.
        /// </summary>
        public Marriage Spouse { get; set; }

        /// <summary>
        /// Legal dependent children of the person.
        /// </summary>
        public Dictionary<string, Person> Children { get; set; }
    }

    /// <summary>
    /// Represents a marriage relationship for the sample.
    /// </summary>
    public class Marriage
    {
        /// <summary>
        /// Legal marriage date.
        /// </summary>
        public DateTime Anniversary { get; set; }

        /// <summary>
        /// Reference to the spouse.
        /// </summary>
        public Person Spouse { get; set; }
    }

    /// <summary>
    /// Sample model demonstrating remote constructor metadata.
    /// </summary>
    public class MixedObject
    {
        /// <summary>
        /// Location of the remote type.
        /// </summary>
        public string AdapterSourceAssembly { get; set; }

        /// <summary>
        /// Fully qualified remote type name.
        /// </summary>
        public string AdapterSourceType { get; set; }

        /// <summary>
        /// Constructor options for the referenced type.
        /// </summary>
        [PropertyMetadataResolver(typeof(RemoteConstructorMetadataResolver))]
        public Dictionary<string, object> Options { get; set; }

        /// <summary>
        /// Resolves metadata for constructor option dictionaries.
        /// </summary>
        public sealed class RemoteConstructorMetadataResolver : IPropertyMetadataResolver
        {
            /// <inheritdoc />
            public ResolvedPropertyMetadata Resolve(PropertyInfo property)
            {
                var documentation = new PropertyDocumentation(
                    property,
                    "Options",
                    "Key/value pairs configuring the remote adapter.",
                    new Func<string>(() => "Each key should match a constructor parameter on the adapter type."));

                return new ResolvedPropertyMetadata(
                    documentation,
                    new DefaultPropertyEditor(),
                    null,
                    new RemoteConstructorLayerProvider());
            }
        }

        /// <summary>
        /// Provides a layered editing experience for constructor dictionaries.
        /// </summary>
        public sealed class RemoteConstructorLayerProvider : IPropertyLayerProvider
        {
            /// <inheritdoc />
            public IEnumerable<PropertyEditLayer> GetLayers(PropertyEditContext context)
            {
                return new[]
                {
                    new PropertyEditLayer(
                        "Edit adapter constructor",
                        "Loads the remote constructor and opens a nested editor for each parameter.",
                        RunConstructorEditor,
                        false)
                };
            }

            private static bool RunConstructorEditor(PropertyEditContext context)
            {
                string assemblyPath = string.Empty;
                string targetType = string.Empty;

                var mixed = context.Model as MixedObject;
                if (mixed != null)
                {
                    assemblyPath = mixed.AdapterSourceAssembly;
                    targetType = mixed.AdapterSourceType;
                }

                var dictionaryModel = context.Model as ConstructorParameterDictionary;
                if (dictionaryModel != null)
                {
                    assemblyPath = dictionaryModel.AssemblyPath;
                    targetType = dictionaryModel.Type;

                    if (!string.IsNullOrEmpty(dictionaryModel.DisplayName))
                    {
                        Consoul.Write("Editing " + dictionaryModel.DisplayName + " parameters for " + dictionaryModel.Type + ".", ConsoleColor.Cyan);
                    }

                    if (!string.IsNullOrEmpty(dictionaryModel.Documentation))
                    {
                        Consoul.Write(dictionaryModel.Documentation, ConsoleColor.DarkGreen);
                    }
                }

                if (string.IsNullOrWhiteSpace(assemblyPath) || string.IsNullOrWhiteSpace(targetType))
                {
                    Consoul.Write("Unable to resolve the remote type without an Assembly path or target type.", ConsoleColor.Red);
                    return false;
                }

                var existingValues = context.CurrentValue as IDictionary;
                ConstructorParameterDictionary parameters;
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

                var view = new EditObjectView(parameters);
                view.Render();

                var flattened = FlattenParameters(parameters.CtorParameters);
                ApplyResolvedParameters(context, flattened);
                return false;
            }

            /// <summary>
            /// Applies the resolved constructor parameters to the edit context, respecting writable and read-only properties.
            /// </summary>
            /// <param name="context">Edit context describing the property being updated.</param>
            /// <param name="values">Flattened constructor parameter values.</param>
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

            /// <summary>
            /// Builds a constructor dictionary for the supplied type using reflection when possible.
            /// </summary>
            /// <param name="assemblyPath">Path to the assembly that defines the target type.</param>
            /// <param name="targetType">Fully qualified target type name.</param>
            /// <param name="existingValues">Existing parameter values supplied by the caller.</param>
            /// <returns>A populated constructor dictionary describing the remote parameters.</returns>
            private static ConstructorParameterDictionary BuildConstructorDictionary(string assemblyPath, string targetType, IDictionary existingValues)
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

            /// <summary>
            /// Uses reflection to inspect the remote constructor and hydrate parameter metadata.
            /// </summary>
            /// <param name="assemblyPath">Path to the remote assembly.</param>
            /// <param name="targetType">Fully qualified target type name.</param>
            /// <param name="existingValues">Existing parameter values captured from prior edits.</param>
            /// <param name="type">Resolved remote type instance.</param>
            /// <returns>The constructor dictionary describing the remote parameters.</returns>
            private static ConstructorParameterDictionary BuildConstructorDictionaryFromType(string assemblyPath, string targetType, IDictionary existingValues, Type type)
            {
                var constructors = type.GetConstructors();

                var constructor = constructors.OrderByDescending(info => info.GetParameters().Length).FirstOrDefault();
                if (constructor == null)
                {
                    throw new InvalidOperationException("Target type does not expose a public constructor.");
                }

                var dictionary = new ConstructorParameterDictionary
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

                    if (existingValue is ConstructorParameterDictionary nestedDictionary)
                    {
                        dictionary.CtorParameters.Add(parameter.Name, nestedDictionary);
                        continue;
                    }

                    if (existingValue is IDictionary nestedValues && !IsSimpleParameter(parameter.ParameterType))
                    {
                        var nested = new ConstructorParameterDictionary
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
                        defaultValue = parameter.HasDefaultValue ? parameter.DefaultValue : string.Empty;
                    }
                    else
                    {
                        defaultValue = new ConstructorParameterDictionary
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

            /// <summary>
            /// Uses XML documentation as a fallback to compose constructor parameters when the assembly cannot be loaded.
            /// </summary>
            /// <param name="assemblyPath">Path to the assembly associated with the documentation.</param>
            /// <param name="targetType">Target type identifier used in documentation.</param>
            /// <param name="existingValues">Existing parameter values captured from prior edits.</param>
            /// <returns>A constructor dictionary derived from the XML documentation or null if unavailable.</returns>
            private static ConstructorParameterDictionary BuildConstructorDictionaryFromDocumentation(string assemblyPath, string targetType, IDictionary existingValues)
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

                var dictionary = new ConstructorParameterDictionary
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

            /// <summary>
            /// Creates a placeholder value using documentation when the remote type cannot be inspected.
            /// </summary>
            /// <param name="typeName">Remote parameter type name.</param>
            /// <param name="assemblyPath">Assembly path associated with the remote type.</param>
            /// <param name="document">XML documentation loaded for the assembly.</param>
            /// <returns>A placeholder value compatible with the editor.</returns>
            private static object CreateFallbackValue(string typeName, string assemblyPath, XmlDocument document)
            {
                if (IsSimpleParameter(typeName))
                {
                    return string.Empty;
                }

                var nested = new ConstructorParameterDictionary
                {
                    DisplayName = GetDisplayName(typeName),
                    Type = typeName,
                    AssemblyPath = assemblyPath,
                    Documentation = LookupTypeSummary(document, typeName)
                };

                return nested;
            }

            /// <summary>
            /// Determines whether a parameter type name represents a simple value.
            /// </summary>
            /// <param name="typeName">Type name extracted from documentation.</param>
            /// <returns><c>true</c> when the type should be treated as a scalar; otherwise, <c>false</c>.</returns>
            private static bool IsSimpleParameter(string typeName)
            {
                if (string.IsNullOrEmpty(typeName))
                {
                    return false;
                }

                var normalized = typeName.Trim();
                if (normalized.StartsWith("System.Nullable{", StringComparison.Ordinal))
                {
                    normalized = normalized.Substring("System.Nullable{".Length);
                    if (normalized.EndsWith("}", StringComparison.Ordinal))
                    {
                        normalized = normalized.Substring(0, normalized.Length - 1);
                    }
                }

                var simpleTypes = new[]
                {
                    "System.Boolean",
                    "System.Byte",
                    "System.SByte",
                    "System.Char",
                    "System.Decimal",
                    "System.Double",
                    "System.Single",
                    "System.Int32",
                    "System.UInt32",
                    "System.Int64",
                    "System.UInt64",
                    "System.Int16",
                    "System.UInt16",
                    "System.String",
                    "System.DateTime",
                    "System.Guid"
                };

                return simpleTypes.Any(candidate => string.Equals(candidate, normalized, StringComparison.Ordinal));
            }

            /// <summary>
            /// Produces a human-readable display name for a documented type.
            /// </summary>
            /// <param name="typeName">Fully qualified type name.</param>
            /// <returns>The simplified display name.</returns>
            private static string GetDisplayName(string typeName)
            {
                if (string.IsNullOrEmpty(typeName))
                {
                    return string.Empty;
                }

                var lastDot = typeName.LastIndexOf('.');
                if (lastDot >= 0 && lastDot < typeName.Length - 1)
                {
                    return typeName.Substring(lastDot + 1);
                }

                return typeName;
            }

            /// <summary>
            /// Parses parameter type identifiers from a constructor signature string in XML documentation.
            /// </summary>
            /// <param name="signature">Constructor signature extracted from the documentation.</param>
            /// <returns>A list of parameter type identifiers.</returns>
            private static IList<string> ParseParameterTypes(string signature)
            {
                var parameterTypes = new List<string>();
                if (string.IsNullOrEmpty(signature))
                {
                    return parameterTypes;
                }

                var start = signature.IndexOf('(');
                var end = signature.LastIndexOf(')');
                if (start < 0 || end <= start)
                {
                    return parameterTypes;
                }

                var list = signature.Substring(start + 1, end - start - 1);
                if (string.IsNullOrEmpty(list))
                {
                    return parameterTypes;
                }

                var builder = new StringBuilder();
                var depth = 0;
                for (var index = 0; index < list.Length; index++)
                {
                    var character = list[index];
                    if (character == '{' || character == '[')
                    {
                        depth++;
                    }
                    else if (character == '}' || character == ']')
                    {
                        if (depth > 0)
                        {
                            depth--;
                        }
                    }

                    if (character == ',' && depth == 0)
                    {
                        parameterTypes.Add(builder.ToString().Trim());
                        builder.Clear();
                        continue;
                    }

                    builder.Append(character);
                }

                if (builder.Length > 0)
                {
                    parameterTypes.Add(builder.ToString().Trim());
                }

                return parameterTypes;
            }

            private static bool IsSimpleParameter(Type type)
            {
                var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
                if (underlyingType.IsEnum)
                {
                    return true;
                }

                var simpleTypes = new[]
                {
                    typeof(bool), typeof(byte), typeof(sbyte), typeof(char), typeof(decimal), typeof(double), typeof(float),
                    typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(short), typeof(ushort), typeof(string), typeof(DateTime), typeof(Guid)
                };

                return simpleTypes.Any(candidate => candidate == underlyingType);
            }

            private static string LookupParameterSummary(ParameterInfo parameter)
            {
                try
                {
                    var assembly = parameter.Member.Module.Assembly;
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

            private static Dictionary<string, object> FlattenParameters(Dictionary<string, object> source)
            {
                var result = new Dictionary<string, object>();
                foreach (var kvp in source)
                {
                    if (kvp.Value is ConstructorParameterDictionary nested)
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

            /// <summary>
            /// Reads a type summary from the loaded XML documentation.
            /// </summary>
            /// <param name="document">XML documentation instance.</param>
            /// <param name="typeName">Fully qualified type identifier.</param>
            /// <returns>The extracted summary or an empty string.</returns>
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

            /// <summary>
            /// Demonstrates a remote adapter with nested configuration.
            /// </summary>
            public sealed class SampleRemoteAdapter
            {
                /// <summary>
                /// Constructs the sample adapter.
                /// </summary>
                /// <param name="configPath">Filepath to a JSON configuration file for the adapter.</param>
                /// <param name="options">Nested options for the adapter.</param>
                public SampleRemoteAdapter(string configPath, SampleAdapterOptions options)
                {
                }
            }

            /// <summary>
            /// Demonstrates nested adapter options.
            /// </summary>
            public sealed class SampleAdapterOptions
            {
                /// <summary>
                /// Optional polling interval.
                /// </summary>
                public int IntervalMilliseconds { get; set; }

                /// <summary>
                /// Optional fallback configuration path.
                /// </summary>
                public string FallbackConfig { get; set; }
            }
        }
    }

    /// <summary>
    /// Wrapper model that exposes constructor parameters for nested editing.
    /// </summary>
    public class ConstructorParameterDictionary
    {
        /// <summary>
        /// Gets or sets the assembly path associated with the remote type.
        /// </summary>
        public string AssemblyPath { get; set; }

        /// <summary>
        /// Gets or sets the type name associated with the remote type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the display name used when editing nested parameters.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets additional documentation describing the nested parameter context.
        /// </summary>
        public string Documentation { get; set; }

        /// <summary>
        /// Gets the constructor parameters for the associated type.
        /// </summary>
        [PropertyMetadataResolver(typeof(MixedObject.RemoteConstructorMetadataResolver))]
        public Dictionary<string, object> CtorParameters { get; set; } = new Dictionary<string, object>();
    }
}
