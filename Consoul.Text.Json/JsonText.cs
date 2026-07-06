using System;
using System.Text.Json;
using ConsoulLibrary.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent JSON text construction over <see cref="JsonTextEntry"/>.
    /// </summary>
    public sealed class JsonText : JsonTextEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonText"/> class.
        /// </summary>
        /// <param name="json">The JSON text to render.</param>
        public JsonText(string json) : base(json)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonText"/> class with explicit options.
        /// </summary>
        /// <param name="json">The JSON text to render.</param>
        /// <param name="options">Optional rendering options for the JSON text.</param>
        public JsonText(string json, JsonTextOptions options) : base(json, options)
        {
        }

        /// <summary>
        /// Serializes a value and creates a fluent JSON text builder without rendering it.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="valueType">The declared value type used by the serializer.</param>
        /// <param name="options">Optional rendering options for the JSON text.</param>
        /// <param name="serializerOptions">Optional serializer options used before rendering.</param>
        /// <returns>A reusable <see cref="JsonText"/> builder containing the serialized value.</returns>
        public static new JsonText FromValue(
            object value,
            Type valueType = null,
            JsonTextOptions options = null,
            JsonSerializerOptions serializerOptions = null)
        {
            JsonTextEntry entry = JsonTextEntry.FromValue(value, valueType, options, serializerOptions);
            return new JsonText(entry.Json, entry.Options);
        }

        /// <summary>
        /// Sets the requested render width.
        /// </summary>
        /// <param name="width">The requested width, or <see langword="null"/> to use the active terminal width.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public JsonText Width(int? width)
        {
            if (width.HasValue && width.Value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than zero.");
            }

            Options.Width = width;
            return this;
        }

        /// <summary>
        /// Sets whether valid JSON should be normalized with indentation before rendering.
        /// </summary>
        /// <param name="enabled">Whether pretty-printing should be enabled.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public JsonText PrettyPrint(bool enabled = true)
        {
            Options.PrettyPrint = enabled;
            return this;
        }

        /// <summary>
        /// Sets whether invalid JSON should throw instead of rendering a friendly error.
        /// </summary>
        /// <param name="enabled">Whether invalid JSON should throw.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public JsonText ThrowOnInvalidJson(bool enabled = true)
        {
            Options.ThrowOnInvalidJson = enabled;
            return this;
        }

        /// <summary>
        /// Sets the message rendered before invalid JSON content.
        /// </summary>
        /// <param name="message">The invalid JSON message.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public JsonText InvalidJsonMessage(string message)
        {
            Options.InvalidJsonMessage = message;
            return this;
        }

        /// <summary>
        /// Sets the member/property name color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public JsonText MemberColor(ConsoleColor color)
        {
            Options.PropertyNameScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the member/property name color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>cyan</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public JsonText MemberColor(string style)
        {
            Options.PropertyNameScheme = ParseScheme(style, Options.ResolvePropertyNameScheme());
            return this;
        }

        /// <summary>
        /// Sets the property name color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public JsonText PropertyNameColor(ConsoleColor color)
        {
            return MemberColor(color);
        }

        /// <summary>
        /// Sets the string value color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public JsonText StringColor(ConsoleColor color)
        {
            Options.StringScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the string value color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public JsonText StringColor(string style)
        {
            Options.StringScheme = ParseScheme(style, Options.ResolveStringScheme());
            return this;
        }

        /// <summary>
        /// Sets the number value color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public JsonText NumberColor(ConsoleColor color)
        {
            Options.NumberScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the number value color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>blue</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public JsonText NumberColor(string style)
        {
            Options.NumberScheme = ParseScheme(style, Options.ResolveNumberScheme());
            return this;
        }

        /// <summary>
        /// Sets the boolean value color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public JsonText BooleanColor(ConsoleColor color)
        {
            Options.BooleanScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the boolean value color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>yellow</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public JsonText BooleanColor(string style)
        {
            Options.BooleanScheme = ParseScheme(style, Options.ResolveBooleanScheme());
            return this;
        }

        /// <summary>
        /// Sets the null value color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public JsonText NullColor(ConsoleColor color)
        {
            Options.NullScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the null value color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>red</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public JsonText NullColor(string style)
        {
            Options.NullScheme = ParseScheme(style, Options.ResolveNullScheme());
            return this;
        }

        /// <summary>
        /// Sets the object brace color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public JsonText BracesColor(ConsoleColor color)
        {
            Options.BracesScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the object brace color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>magenta</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public JsonText BracesColor(string style)
        {
            Options.BracesScheme = ParseScheme(style, Options.ResolveBracesScheme());
            return this;
        }

        /// <summary>
        /// Sets the array bracket color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public JsonText BracketColor(ConsoleColor color)
        {
            Options.BracketScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the array bracket color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>purple</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public JsonText BracketColor(string style)
        {
            Options.BracketScheme = ParseScheme(style, Options.ResolveBracketScheme());
            return this;
        }

        /// <summary>
        /// Sets the colon color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public JsonText ColonColor(ConsoleColor color)
        {
            Options.ColonScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the colon color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>cyan</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public JsonText ColonColor(string style)
        {
            Options.ColonScheme = ParseScheme(style, Options.ResolveColonScheme());
            return this;
        }

        /// <summary>
        /// Sets the comma color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public JsonText CommaColor(ConsoleColor color)
        {
            Options.CommaScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the comma color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public JsonText CommaColor(string style)
        {
            Options.CommaScheme = ParseScheme(style, Options.ResolveCommaScheme());
            return this;
        }

        private static ColorScheme Scheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        private static ColorScheme ParseScheme(string style, ColorScheme fallback)
        {
            return Style.Parse(style).ToColorScheme(fallback);
        }

        private static ColorScheme ToScheme(Style style, ColorScheme fallback)
        {
            if (style == null)
            {
                throw new ArgumentNullException(nameof(style));
            }

            return style.ToColorScheme(fallback);
        }
    }
}
