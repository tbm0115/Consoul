using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ConsoulLibrary
{
    /// <summary>
    /// Lightweight routine settings section used by routine input transforms without external configuration packages.
    /// </summary>
    public sealed class RoutineSettingsSection
    {
        private readonly IReadOnlyDictionary<string, string> _values;
        private readonly string _path;

        /// <summary>
        /// An empty routine settings section.
        /// </summary>
        public static RoutineSettingsSection Empty { get; } = new RoutineSettingsSection(new Dictionary<string, string>());

        /// <summary>
        /// Creates a settings section from flattened key/value pairs.
        /// </summary>
        /// <param name="values">Values keyed by colon-delimited paths.</param>
        public RoutineSettingsSection(IReadOnlyDictionary<string, string> values)
            : this(values ?? new Dictionary<string, string>(), string.Empty)
        {
        }

        private RoutineSettingsSection(IReadOnlyDictionary<string, string> values, string path)
        {
            _values = values;
            _path = path ?? string.Empty;
        }

        /// <summary>
        /// Gets the final segment of this section's path.
        /// </summary>
        public string Key
        {
            get
            {
                if (string.IsNullOrEmpty(_path))
                {
                    return string.Empty;
                }

                int index = _path.LastIndexOf(':');
                return index < 0 ? _path : _path.Substring(index + 1);
            }
        }

        /// <summary>
        /// Gets this section's direct value, if one exists.
        /// </summary>
        public string Value
        {
            get
            {
                string value;
                return _values.TryGetValue(_path, out value) ? value : null;
            }
        }

        /// <summary>
        /// Gets a child value by key.
        /// </summary>
        /// <param name="key">Child key.</param>
        /// <returns>The child value, or null when it does not exist.</returns>
        public string this[string key] => GetSection(key).Value;

        /// <summary>
        /// Gets a child settings section.
        /// </summary>
        /// <param name="key">Child key.</param>
        /// <returns>The child settings section.</returns>
        public RoutineSettingsSection GetSection(string key)
            => new RoutineSettingsSection(_values, CombinePath(_path, key));

        /// <summary>
        /// Gets the immediate children of this section.
        /// </summary>
        /// <returns>Immediate child sections.</returns>
        public IEnumerable<RoutineSettingsSection> GetChildren()
        {
            string prefix = string.IsNullOrEmpty(_path) ? string.Empty : _path + ":";
            var childKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string key in _values.Keys)
            {
                if (!key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string remainder = key.Substring(prefix.Length);
                if (remainder.Length == 0)
                {
                    continue;
                }

                int separatorIndex = remainder.IndexOf(':');
                childKeys.Add(separatorIndex < 0 ? remainder : remainder.Substring(0, separatorIndex));
            }

            return childKeys.OrderBy(key => key, StringComparer.OrdinalIgnoreCase).Select(GetSection).ToArray();
        }

        /// <summary>
        /// Creates a settings section that contains transform values.
        /// </summary>
        /// <param name="transforms">Transform key/value pairs.</param>
        /// <returns>A routine settings section.</returns>
        public static RoutineSettingsSection FromTransforms(IReadOnlyDictionary<string, string> transforms)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (transforms != null)
            {
                foreach (var transform in transforms)
                {
                    values[CombinePath("Transforms", transform.Key)] = transform.Value;
                }
            }

            return new RoutineSettingsSection(values);
        }

        /// <summary>
        /// Loads the Consoul section from a JSON file using a small dependency-free parser for flat settings.
        /// </summary>
        /// <param name="path">JSON file path.</param>
        /// <returns>A routine settings section.</returns>
        public static RoutineSettingsSection FromJsonFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return Empty;
            }

            return FromJson(File.ReadAllText(path));
        }

        /// <summary>
        /// Loads the Consoul section from JSON using a small dependency-free parser for flat settings.
        /// </summary>
        /// <param name="json">JSON text.</param>
        /// <returns>A routine settings section.</returns>
        public static RoutineSettingsSection FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return Empty;
            }

            int index = 0;
            string root;
            if (!TryReadObject(json, ref index, out root))
            {
                return Empty;
            }

            var allValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            FlattenObject(root, string.Empty, allValues);

            var consoulValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in allValues)
            {
                const string prefix = "Consoul:";
                if (pair.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    consoulValues[pair.Key.Substring(prefix.Length)] = pair.Value;
                }
            }

            return new RoutineSettingsSection(consoulValues);
        }

        /// <summary>
        /// Creates a settings section from a Microsoft.Extensions.Configuration object without a compile-time dependency.
        /// </summary>
        /// <param name="configuration">Configuration root or section.</param>
        /// <returns>A routine settings section.</returns>
        public static RoutineSettingsSection FromConfigurationObject(object configuration)
        {
            if (configuration == null)
            {
                return Empty;
            }

            var settings = configuration as RoutineSettingsSection;
            if (settings != null)
            {
                return settings;
            }

            object section = IsConsoulSection(configuration)
                ? configuration
                : TryGetSection(configuration, "Consoul") ?? configuration;

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            CopyConfigurationChildren(section, string.Empty, values);
            return new RoutineSettingsSection(values);
        }

        private static bool IsConsoulSection(object configuration)
        {
            string key = TryGetStringProperty(configuration, "Key");
            return string.Equals(key, "Consoul", StringComparison.OrdinalIgnoreCase);
        }

        private static object TryGetSection(object configuration, string key)
        {
            var method = configuration.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(candidate =>
                    candidate.Name == "GetSection" &&
                    candidate.GetParameters().Length == 1 &&
                    candidate.GetParameters()[0].ParameterType == typeof(string));

            return method?.Invoke(configuration, new object[] { key });
        }

        private static void CopyConfigurationChildren(object section, string path, IDictionary<string, string> values)
        {
            if (section == null)
            {
                return;
            }

            string value = TryGetStringProperty(section, "Value");
            if (value != null && !string.IsNullOrEmpty(path))
            {
                values[path] = value;
            }

            IEnumerable children = TryGetChildren(section);
            if (children == null)
            {
                return;
            }

            foreach (object child in children)
            {
                string key = TryGetStringProperty(child, "Key");
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                CopyConfigurationChildren(child, CombinePath(path, key), values);
            }
        }

        private static IEnumerable TryGetChildren(object section)
        {
            var method = section.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(candidate => candidate.Name == "GetChildren" && candidate.GetParameters().Length == 0);

            return method?.Invoke(section, null) as IEnumerable;
        }

        private static string TryGetStringProperty(object target, string propertyName)
        {
            var property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            return property?.GetValue(target) as string;
        }

        private static void FlattenObject(string body, string prefix, IDictionary<string, string> values)
        {
            int index = 0;
            while (index < body.Length)
            {
                SkipSeparators(body, ref index);
                if (index >= body.Length)
                {
                    break;
                }

                string key;
                if (!TryReadString(body, ref index, out key))
                {
                    break;
                }

                SkipWhitespace(body, ref index);
                if (index >= body.Length || body[index] != ':')
                {
                    break;
                }

                index++;
                SkipWhitespace(body, ref index);
                string path = CombinePath(prefix, key);

                if (index < body.Length && body[index] == '{')
                {
                    string childBody;
                    if (!TryReadObject(body, ref index, out childBody))
                    {
                        break;
                    }

                    FlattenObject(childBody, path, values);
                    continue;
                }

                if (index < body.Length && body[index] == '"')
                {
                    string stringValue;
                    if (!TryReadString(body, ref index, out stringValue))
                    {
                        break;
                    }

                    values[path] = stringValue;
                    continue;
                }

                values[path] = ReadLiteral(body, ref index);
            }
        }

        private static string ReadLiteral(string text, ref int index)
        {
            var builder = new StringBuilder();
            var depth = 0;
            var inString = false;

            while (index < text.Length)
            {
                char ch = text[index];
                if (inString)
                {
                    builder.Append(ch);
                    if (ch == '"' && !IsEscaped(text, index))
                    {
                        inString = false;
                    }

                    index++;
                    continue;
                }

                if (ch == '"')
                {
                    inString = true;
                    builder.Append(ch);
                    index++;
                    continue;
                }

                if (ch == '[')
                {
                    depth++;
                }
                else if (ch == ']')
                {
                    depth--;
                }
                else if (depth == 0 && (ch == ',' || ch == '}'))
                {
                    break;
                }

                builder.Append(ch);
                index++;
            }

            string literal = builder.ToString().Trim();
            return string.Equals(literal, "null", StringComparison.OrdinalIgnoreCase) ? null : literal;
        }

        private static bool TryReadObject(string text, ref int index, out string body)
        {
            body = null;
            SkipWhitespace(text, ref index);
            if (index >= text.Length || text[index] != '{')
            {
                return false;
            }

            int start = index + 1;
            int depth = 0;
            var inString = false;

            for (; index < text.Length; index++)
            {
                char ch = text[index];
                if (inString)
                {
                    if (ch == '"' && !IsEscaped(text, index))
                    {
                        inString = false;
                    }

                    continue;
                }

                if (ch == '"')
                {
                    inString = true;
                    continue;
                }

                if (ch == '{')
                {
                    depth++;
                }
                else if (ch == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        body = text.Substring(start, index - start);
                        index++;
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool TryReadString(string text, ref int index, out string value)
        {
            value = null;
            SkipWhitespace(text, ref index);
            if (index >= text.Length || text[index] != '"')
            {
                return false;
            }

            index++;
            var builder = new StringBuilder();
            while (index < text.Length)
            {
                char ch = text[index++];
                if (ch == '"')
                {
                    value = builder.ToString();
                    return true;
                }

                if (ch == '\\' && index < text.Length)
                {
                    builder.Append(Unescape(text[index++]));
                    continue;
                }

                builder.Append(ch);
            }

            return false;
        }

        private static char Unescape(char ch)
        {
            switch (ch)
            {
                case '"':
                case '\\':
                case '/':
                    return ch;
                case 'b':
                    return '\b';
                case 'f':
                    return '\f';
                case 'n':
                    return '\n';
                case 'r':
                    return '\r';
                case 't':
                    return '\t';
                default:
                    return ch;
            }
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

        private static void SkipSeparators(string text, ref int index)
        {
            while (index < text.Length && (char.IsWhiteSpace(text[index]) || text[index] == ','))
            {
                index++;
            }
        }

        private static void SkipWhitespace(string text, ref int index)
        {
            while (index < text.Length && char.IsWhiteSpace(text[index]))
            {
                index++;
            }
        }

        private static string CombinePath(string left, string right)
        {
            if (string.IsNullOrEmpty(left))
            {
                return right ?? string.Empty;
            }

            if (string.IsNullOrEmpty(right))
            {
                return left;
            }

            return left + ":" + right;
        }
    }
}
