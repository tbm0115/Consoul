using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;

namespace ConsoulLibrary.Views.Editing
{
    /// <summary>
    /// Provides metadata about a property, including display information and documentation.
    /// </summary>
    public sealed class PropertyDocumentation
    {
        private readonly Lazy<string> _xmlSummary;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyDocumentation"/> class using the property's metadata.
        /// </summary>
        /// <param name="property">The property supplying attribute-based documentation.</param>
        public PropertyDocumentation(PropertyInfo property)
            : this(property, ResolveDisplayName(property), ResolveDescription(property), new Func<string>(() => XmlDocumentationProvider.GetSummary(property)))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyDocumentation"/> class using explicit values.
        /// </summary>
        /// <param name="property">The property associated with the documentation.</param>
        /// <param name="displayName">Optional display name describing the property.</param>
        /// <param name="displayDescription">Optional description instructing how to populate the property.</param>
        /// <param name="xmlSummaryProvider">Provider that supplies an XML summary when requested.</param>
        public PropertyDocumentation(PropertyInfo property, string displayName, string displayDescription, Func<string> xmlSummaryProvider)
        {
            Property = property ?? throw new ArgumentNullException(nameof(property));
            DisplayName = displayName;
            DisplayDescription = displayDescription;
            _xmlSummary = new Lazy<string>(() => xmlSummaryProvider != null ? xmlSummaryProvider() ?? string.Empty : string.Empty);
        }

        /// <summary>
        /// Gets the source property for the documentation entry.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Gets the resolved display name for the property, if any.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets a human friendly description for the property, if any.
        /// </summary>
        public string DisplayDescription { get; }

        /// <summary>
        /// Gets the XML documentation summary for the property, if available.
        /// </summary>
        public string XmlSummary => _xmlSummary.Value;

        private static string ResolveDisplayName(PropertyInfo property)
        {
            var displayAttribute = property.GetCustomAttribute<DisplayAttribute>(true);
            if (displayAttribute != null)
            {
                var name = displayAttribute.GetName();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    return name;
                }
            }

            var displayNameAttribute = property.GetCustomAttribute<DisplayNameAttribute>(true);
            if (displayNameAttribute != null && !string.IsNullOrWhiteSpace(displayNameAttribute.DisplayName))
            {
                return displayNameAttribute.DisplayName;
            }

            return null;
        }

        private static string ResolveDescription(PropertyInfo property)
        {
            var displayAttribute = property.GetCustomAttribute<DisplayAttribute>(true);
            if (displayAttribute != null)
            {
                var description = displayAttribute.GetDescription();
                if (!string.IsNullOrWhiteSpace(description))
                {
                    return description;
                }
            }

            var descriptionAttribute = property.GetCustomAttribute<DescriptionAttribute>(true);
            if (descriptionAttribute != null && !string.IsNullOrWhiteSpace(descriptionAttribute.Description))
            {
                return descriptionAttribute.Description;
            }

            return null;
        }
    }

    /// <summary>
    /// Provides access to XML documentation summaries embedded in, or located next to, an assembly.
    /// </summary>
    internal static class XmlDocumentationProvider
    {
        private static readonly ConcurrentDictionary<Assembly, Lazy<XDocument>> CachedDocuments = new ConcurrentDictionary<Assembly, Lazy<XDocument>>();

        public static string GetSummary(MemberInfo member)
        {
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            var document = GetXmlDocument(member.Module.Assembly);
            if (document == null)
            {
                return string.Empty;
            }

            var memberName = GetMemberElementName(member);
            if (memberName == null)
            {
                return string.Empty;
            }

            var root = document.Root;
            if (root == null)
            {
                return string.Empty;
            }

            XElement memberElement = null;
            foreach (var element in root.Elements("members").Elements("member"))
            {
                var attribute = element.Attribute("name");
                var value = attribute != null ? attribute.Value : null;
                if (string.Equals(value, memberName, StringComparison.Ordinal))
                {
                    memberElement = element;
                    break;
                }
            }

            if (memberElement == null)
            {
                return string.Empty;
            }

            var summaryElement = memberElement.Element("summary");
            if (summaryElement == null)
            {
                return string.Empty;
            }

            var builder = new System.Text.StringBuilder();
            foreach (var node in summaryElement.Nodes())
            {
                if (node is XText text)
                {
                    builder.Append(text.Value);
                }
                else if (node is XElement element)
                {
                    if (string.Equals(element.Name.LocalName, "see", StringComparison.OrdinalIgnoreCase))
                    {
                        var crefAttribute = element.Attribute("cref");
                        var cref = crefAttribute != null ? crefAttribute.Value : null;
                        if (!string.IsNullOrWhiteSpace(cref))
                        {
                            builder.Append(FormatCref(cref));
                        }
                    }
                    else if (string.Equals(element.Name.LocalName, "para", StringComparison.OrdinalIgnoreCase))
                    {
                        builder.AppendLine();
                        builder.Append(element.Value);
                    }
                    else
                    {
                        builder.Append(element.Value);
                    }
                }
            }

            return builder.ToString().Trim();
        }

        private static XDocument GetXmlDocument(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            var lazy = CachedDocuments.GetOrAdd(assembly, CreateLazyDocument);
            return lazy.Value;
        }

        private static Lazy<XDocument> CreateLazyDocument(Assembly assembly)
        {
            return new Lazy<XDocument>(() => LoadDocument(assembly), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private static XDocument LoadDocument(Assembly assembly)
        {
            try
            {
                var xmlFileName = Path.ChangeExtension(assembly.Location, ".xml");
                if (!string.IsNullOrEmpty(xmlFileName) && File.Exists(xmlFileName))
                {
                    return XDocument.Load(xmlFileName);
                }

                var fileName = Path.GetFileName(xmlFileName);
                if (!string.IsNullOrEmpty(fileName))
                {
                    var resourceName = assembly
                        .GetManifestResourceNames()
                        .FirstOrDefault(name => name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

                    if (resourceName != null)
                    {
                        var stream = assembly.GetManifestResourceStream(resourceName);
                        if (stream != null)
                        {
                            try
                            {
                                return XDocument.Load(stream);
                            }
                            finally
                            {
                                stream.Dispose();
                            }
                        }
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private static string GetMemberElementName(MemberInfo member)
        {
            var type = member.DeclaringType;
            if (type == null)
            {
                return null;
            }

            var typeName = type.FullName?.Replace('+', '.');
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            switch (member.MemberType)
            {
                case MemberTypes.Property:
                    return "P:" + typeName + "." + member.Name;
                case MemberTypes.Method:
                    return "M:" + typeName + "." + member.Name;
                case MemberTypes.Field:
                    return "F:" + typeName + "." + member.Name;
                case MemberTypes.Event:
                    return "E:" + typeName + "." + member.Name;
                case MemberTypes.TypeInfo:
                    return "T:" + typeName;
                default:
                    return null;
            }
        }

        private static string FormatCref(string cref)
        {
            var colonIndex = cref.IndexOf(':');
            if (colonIndex >= 0 && colonIndex + 1 < cref.Length)
            {
                cref = cref.Substring(colonIndex + 1);
            }

            return cref;
        }
    }
}
