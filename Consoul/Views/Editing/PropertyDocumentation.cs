using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace ConsoulLibrary.Views.Editing
{
    /// <summary>
    /// Provides metadata about a property, including display information and documentation.
    /// </summary>
    public sealed class PropertyDocumentation
    {
        private readonly Lazy<string> _xmlSummary;

        internal PropertyDocumentation(PropertyInfo property)
        {
            Property = property ?? throw new ArgumentNullException(nameof(property));
            DisplayName = ResolveDisplayName(property);
            DisplayDescription = ResolveDescription(property);
            _xmlSummary = new Lazy<string>(() => XmlDocumentationProvider.GetSummary(property));
        }

        /// <summary>
        /// Gets the source property for the documentation entry.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Gets the resolved display name for the property, if any.
        /// </summary>
        public string? DisplayName { get; }

        /// <summary>
        /// Gets a human friendly description for the property, if any.
        /// </summary>
        public string? DisplayDescription { get; }

        /// <summary>
        /// Gets the XML documentation summary for the property, if available.
        /// </summary>
        public string XmlSummary => _xmlSummary.Value;

        private static string? ResolveDisplayName(PropertyInfo property)
        {
            var displayAttribute = property.GetCustomAttribute<DisplayAttribute>(inherit: true);
            if (displayAttribute?.GetName() is { } name)
            {
                return name;
            }

            var displayNameAttribute = property.GetCustomAttribute<DisplayNameAttribute>(inherit: true);
            if (!string.IsNullOrWhiteSpace(displayNameAttribute?.DisplayName))
            {
                return displayNameAttribute.DisplayName;
            }

            return null;
        }

        private static string? ResolveDescription(PropertyInfo property)
        {
            var displayAttribute = property.GetCustomAttribute<DisplayAttribute>(inherit: true);
            if (displayAttribute?.GetDescription() is { } description && !string.IsNullOrWhiteSpace(description))
            {
                return description;
            }

            var descriptionAttribute = property.GetCustomAttribute<DescriptionAttribute>(inherit: true);
            if (!string.IsNullOrWhiteSpace(descriptionAttribute?.Description))
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
        private static readonly ConcurrentDictionary<Assembly, XDocument?> CachedDocuments = new ConcurrentDictionary<Assembly, XDocument?>();

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

            var memberElement = document.Root?
                .Elements("members")
                .Elements("member")
                .FirstOrDefault(element => string.Equals(element.Attribute("name")?.Value, memberName, StringComparison.Ordinal));

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
                        var cref = element.Attribute("cref")?.Value;
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

        private static XDocument? GetXmlDocument(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            return CachedDocuments.GetOrAdd(assembly, LoadDocument);
        }

        private static XDocument? LoadDocument(Assembly assembly)
        {
            try
            {
                var xmlFileName = Path.ChangeExtension(assembly.Location, ".xml");
                if (!string.IsNullOrEmpty(xmlFileName) && File.Exists(xmlFileName))
                {
                    return XDocument.Load(xmlFileName);
                }

                var resourceName = assembly
                    .GetManifestResourceNames()
                    .FirstOrDefault(name => name.EndsWith(Path.GetFileName(xmlFileName), StringComparison.OrdinalIgnoreCase));

                if (resourceName != null)
                {
                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    return stream != null ? XDocument.Load(stream) : null;
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private static string? GetMemberElementName(MemberInfo member)
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

            return member.MemberType switch
            {
                MemberTypes.Property => $"P:{typeName}.{member.Name}",
                MemberTypes.Method => $"M:{typeName}.{member.Name}",
                MemberTypes.Field => $"F:{typeName}.{member.Name}",
                MemberTypes.Event => $"E:{typeName}.{member.Name}",
                MemberTypes.TypeInfo => $"T:{typeName}",
                _ => null,
            };
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
