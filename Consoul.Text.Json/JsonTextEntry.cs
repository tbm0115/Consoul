using System;
using System.Text;
using System.Text.Json;
using ConsoulLibrary.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders syntax-colored JSON text through Consoul's console driver.
    /// </summary>
    public class JsonTextEntry : IConsoulRenderable
    {
        private readonly JsonTextOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTextEntry"/> class.
        /// </summary>
        /// <param name="json">The JSON text to render.</param>
        /// <param name="options">Optional rendering options for the JSON text.</param>
        public JsonTextEntry(string json, JsonTextOptions options = null)
        {
            Json = json ?? string.Empty;
            _options = options != null ? options.Clone() : new JsonTextOptions();
        }

        /// <summary>
        /// Gets or sets the JSON text rendered by this entry.
        /// </summary>
        public string Json { get; set; }

        /// <summary>
        /// Gets the options used to render the JSON text.
        /// </summary>
        public JsonTextOptions Options => _options;

        /// <summary>
        /// Gets or sets the indentation text used when valid JSON is pretty-printed.
        /// </summary>
        public string Indentation
        {
            get => _options.Indentation;
            set => _options.Indentation = value;
        }

        /// <summary>
        /// Gets or sets the style used for member/property names.
        /// </summary>
        public Style MemberStyle
        {
            get => _options.MemberStyle;
            set => _options.MemberStyle = value;
        }

        /// <summary>
        /// Gets or sets the style used for string values.
        /// </summary>
        public Style StringStyle
        {
            get => _options.StringStyle;
            set => _options.StringStyle = value;
        }

        /// <summary>
        /// Gets or sets the style used for numeric values.
        /// </summary>
        public Style NumberStyle
        {
            get => _options.NumberStyle;
            set => _options.NumberStyle = value;
        }

        /// <summary>
        /// Gets or sets the style used for boolean values.
        /// </summary>
        public Style BooleanStyle
        {
            get => _options.BooleanStyle;
            set => _options.BooleanStyle = value;
        }

        /// <summary>
        /// Gets or sets the style used for null values.
        /// </summary>
        public Style NullStyle
        {
            get => _options.NullStyle;
            set => _options.NullStyle = value;
        }

        /// <summary>
        /// Gets or sets the style used for JSON object braces.
        /// </summary>
        public Style BracesStyle
        {
            get => _options.BracesStyle;
            set => _options.BracesStyle = value;
        }

        /// <summary>
        /// Gets or sets the style used for JSON array brackets.
        /// </summary>
        public Style BracketsStyle
        {
            get => _options.BracketsStyle;
            set => _options.BracketsStyle = value;
        }

        /// <summary>
        /// Gets or sets the style used for JSON colons.
        /// </summary>
        public Style ColonStyle
        {
            get => _options.ColonStyle;
            set => _options.ColonStyle = value;
        }

        /// <summary>
        /// Gets or sets the style used for JSON commas.
        /// </summary>
        public Style CommaStyle
        {
            get => _options.CommaStyle;
            set => _options.CommaStyle = value;
        }

        /// <summary>
        /// Renders the JSON text to the active console driver.
        /// </summary>
        public void Render()
        {
            string json = Json;
            try
            {
                json = NormalizeJson(json, _options.PrettyPrint, _options.Indentation);
            }
            catch (JsonException exception)
            {
                if (_options.ThrowOnInvalidJson)
                {
                    throw;
                }

                RenderInvalidJson(exception);
                return;
            }

            RenderTokens(json);
        }

        /// <summary>
        /// Renders temporary JSON text to the active console driver.
        /// </summary>
        /// <param name="json">The JSON text to render.</param>
        /// <param name="options">Optional rendering options for the JSON text.</param>
        public static void Render(string json, JsonTextOptions options = null)
        {
            new JsonTextEntry(json, options).Render();
        }

        /// <summary>
        /// Serializes a value and creates a reusable JSON text entry without rendering it.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="valueType">The declared value type used by the serializer.</param>
        /// <param name="options">Optional rendering options for the JSON text.</param>
        /// <param name="serializerOptions">Optional serializer options used before rendering.</param>
        /// <returns>A reusable <see cref="JsonTextEntry"/> containing the serialized value.</returns>
        public static JsonTextEntry FromValue(object value, Type valueType = null, JsonTextOptions options = null, JsonSerializerOptions serializerOptions = null)
        {
            JsonTextOptions renderOptions = options != null ? options.Clone() : new JsonTextOptions();
            JsonSerializerOptions serializer = CreateSerializerOptions(serializerOptions, renderOptions.PrettyPrint);
            string json = JsonSerializer.Serialize(value, valueType ?? value?.GetType() ?? typeof(object), serializer);
            return new JsonTextEntry(json, renderOptions);
        }

        /// <summary>
        /// Serializes a value and renders it as syntax-colored JSON.
        /// </summary>
        /// <param name="value">The value to serialize and render.</param>
        /// <param name="valueType">The declared value type used by the serializer.</param>
        /// <param name="options">Optional rendering options for the JSON text.</param>
        /// <param name="serializerOptions">Optional serializer options used before rendering.</param>
        public static void RenderValue(object value, Type valueType = null, JsonTextOptions options = null, JsonSerializerOptions serializerOptions = null)
        {
            FromValue(value, valueType, options, serializerOptions).Render();
        }

        private static JsonSerializerOptions CreateSerializerOptions(JsonSerializerOptions serializerOptions, bool writeIndented)
        {
            var options = serializerOptions != null
                ? new JsonSerializerOptions(serializerOptions)
                : new JsonSerializerOptions();

            options.WriteIndented = writeIndented;
            return options;
        }

        private static string NormalizeJson(string json, bool prettyPrint, string indentation)
        {
            if (!prettyPrint)
            {
                using (JsonDocument.Parse(json ?? string.Empty))
                {
                    return json ?? string.Empty;
                }
            }

            using (JsonDocument document = JsonDocument.Parse(json ?? string.Empty))
            {
                var builder = new StringBuilder();
                WriteElement(builder, document.RootElement, 0, indentation ?? string.Empty);
                return builder.ToString();
            }
        }

        private static void WriteElement(StringBuilder builder, JsonElement element, int depth, string indentation)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    WriteObject(builder, element, depth, indentation);
                    break;
                case JsonValueKind.Array:
                    WriteArray(builder, element, depth, indentation);
                    break;
                default:
                    builder.Append(element.GetRawText());
                    break;
            }
        }

        private static void WriteObject(StringBuilder builder, JsonElement element, int depth, string indentation)
        {
            builder.Append('{');

            int count = 0;
            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (count == 0)
                {
                    builder.AppendLine();
                }
                else
                {
                    builder.AppendLine(",");
                }

                AppendIndentation(builder, depth + 1, indentation);
                builder.Append(JsonSerializer.Serialize(property.Name));
                builder.Append(": ");
                WriteElement(builder, property.Value, depth + 1, indentation);
                count++;
            }

            if (count > 0)
            {
                builder.AppendLine();
                AppendIndentation(builder, depth, indentation);
            }

            builder.Append('}');
        }

        private static void WriteArray(StringBuilder builder, JsonElement element, int depth, string indentation)
        {
            builder.Append('[');

            int count = 0;
            foreach (JsonElement item in element.EnumerateArray())
            {
                if (count == 0)
                {
                    builder.AppendLine();
                }
                else
                {
                    builder.AppendLine(",");
                }

                AppendIndentation(builder, depth + 1, indentation);
                WriteElement(builder, item, depth + 1, indentation);
                count++;
            }

            if (count > 0)
            {
                builder.AppendLine();
                AppendIndentation(builder, depth, indentation);
            }

            builder.Append(']');
        }

        private static void AppendIndentation(StringBuilder builder, int depth, string indentation)
        {
            for (int index = 0; index < depth; index++)
            {
                builder.Append(indentation);
            }
        }

        private void RenderInvalidJson(JsonException exception)
        {
            int width = ResolveWidth(_options.Width);
            ColorScheme invalidScheme = _options.ResolveInvalidScheme();
            WriteWrapped(
                string.Format("{0}: {1}", _options.InvalidJsonMessage ?? "Invalid JSON", exception.Message),
                invalidScheme,
                width,
                0,
                true);

            foreach (string line in GetLines(Json))
            {
                WriteWrapped(line, invalidScheme, width, 0, true);
            }
        }

        private void RenderTokens(string json)
        {
            int width = ResolveWidth(_options.Width);
            foreach (string line in GetLines(json))
            {
                int index = 0;
                int column = 0;
                while (index < line.Length)
                {
                    char current = line[index];
                    if (char.IsWhiteSpace(current))
                    {
                        int end = ReadWhile(line, index, ch => char.IsWhiteSpace(ch));
                        column = WriteWrapped(line.Substring(index, end - index), _options.ResolvePlainScheme(), width, column, false);
                        index = end;
                        continue;
                    }

                    if (current == '"')
                    {
                        int end = ReadString(line, index);
                        string token = line.Substring(index, end - index);
                        ColorScheme scheme = IsPropertyName(line, end)
                            ? _options.ResolvePropertyNameScheme()
                            : _options.ResolveStringScheme();
                        column = WriteWrapped(token, scheme, width, column, false);
                        index = end;
                        continue;
                    }

                    if (current == '-' || char.IsDigit(current))
                    {
                        int end = ReadWhile(line, index, IsNumberCharacter);
                        column = WriteWrapped(line.Substring(index, end - index), _options.ResolveNumberScheme(), width, column, false);
                        index = end;
                        continue;
                    }

                    if (MatchesLiteral(line, index, "true"))
                    {
                        column = WriteWrapped("true", _options.ResolveBooleanScheme(), width, column, false);
                        index += 4;
                        continue;
                    }

                    if (MatchesLiteral(line, index, "false"))
                    {
                        column = WriteWrapped("false", _options.ResolveBooleanScheme(), width, column, false);
                        index += 5;
                        continue;
                    }

                    if (MatchesLiteral(line, index, "null"))
                    {
                        column = WriteWrapped("null", _options.ResolveNullScheme(), width, column, false);
                        index += 4;
                        continue;
                    }

                    column = WriteWrapped(current.ToString(), ResolvePunctuationScheme(current), width, column, false);
                    index++;
                }

                Consoul.Write(string.Empty, _options.ResolvePlainScheme(), true);
            }
        }

        private static int ResolveWidth(int? requestedWidth)
        {
            int terminalWidth = Math.Max(1, Consoul.TerminalWidth - 1);
            int requested = requestedWidth ?? terminalWidth;
            return Math.Max(1, Math.Min(requested, terminalWidth));
        }

        private static string[] GetLines(string text)
        {
            text = (text ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');
            string[] lines = text.Split('\n');
            return lines.Length == 0 ? new[] { string.Empty } : lines;
        }

        private static int WriteWrapped(string text, ColorScheme scheme, int width, int column, bool endLine)
        {
            text = text ?? string.Empty;
            width = Math.Max(1, width);
            int offset = 0;

            while (offset < text.Length)
            {
                int available = width - column;
                if (available <= 0)
                {
                    Consoul.Write(string.Empty, scheme, true);
                    column = 0;
                    available = width;
                }

                int count = Math.Min(available, text.Length - offset);
                Consoul.Write(text.Substring(offset, count), scheme, false);
                offset += count;
                column += count;

                if (offset < text.Length && column >= width)
                {
                    Consoul.Write(string.Empty, scheme, true);
                    column = 0;
                }
            }

            if (endLine)
            {
                Consoul.Write(string.Empty, scheme, true);
                return 0;
            }

            return column;
        }

        private static int ReadWhile(string text, int start, Func<char, bool> predicate)
        {
            int index = start;
            while (index < text.Length && predicate(text[index]))
            {
                index++;
            }

            return index;
        }

        private static int ReadString(string text, int start)
        {
            bool escaped = false;
            int index = start + 1;
            while (index < text.Length)
            {
                char current = text[index];
                if (escaped)
                {
                    escaped = false;
                }
                else if (current == '\\')
                {
                    escaped = true;
                }
                else if (current == '"')
                {
                    return index + 1;
                }

                index++;
            }

            return text.Length;
        }

        private static bool IsPropertyName(string line, int tokenEnd)
        {
            int index = tokenEnd;
            while (index < line.Length && char.IsWhiteSpace(line[index]))
            {
                index++;
            }

            return index < line.Length && line[index] == ':';
        }

        private static bool IsNumberCharacter(char character)
        {
            return char.IsDigit(character)
                || character == '-'
                || character == '+'
                || character == '.'
                || character == 'e'
                || character == 'E';
        }

        private static bool MatchesLiteral(string text, int index, string literal)
        {
            if (index + literal.Length > text.Length)
            {
                return false;
            }

            return string.Compare(text, index, literal, 0, literal.Length, StringComparison.Ordinal) == 0;
        }

        private ColorScheme ResolvePunctuationScheme(char punctuation)
        {
            switch (punctuation)
            {
                case '{':
                case '}':
                    return _options.ResolveBracesScheme();
                case '[':
                case ']':
                    return _options.ResolveBracketScheme();
                case ':':
                    return _options.ResolveColonScheme();
                case ',':
                    return _options.ResolveCommaScheme();
                default:
                    return _options.ResolvePunctuationScheme();
            }
        }
    }
}
