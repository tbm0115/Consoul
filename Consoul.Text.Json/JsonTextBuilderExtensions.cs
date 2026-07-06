using System;
using ConsoulLibrary.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides fluent style helpers for <see cref="JsonText"/> while leaving property setters available for object initializers.
    /// </summary>
    public static class JsonTextBuilderExtensions
    {
        /// <summary>
        /// Sets the indentation text used when valid JSON is pretty-printed.
        /// </summary>
        /// <param name="jsonText">The JSON text builder.</param>
        /// <param name="indentation">The indentation text to repeat for each nested level.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public static JsonText Indentation(this JsonText jsonText, string indentation = "   ")
        {
            return Apply(jsonText, text => text.Indentation = indentation);
        }

        /// <summary>
        /// Sets the member/property name style.
        /// </summary>
        /// <param name="jsonText">The JSON text builder.</param>
        /// <param name="style">The style used for member names.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public static JsonText MemberStyle(this JsonText jsonText, Style style)
        {
            return Apply(jsonText, text => text.Options.PropertyNameScheme = ToScheme(style, text.Options.ResolvePropertyNameScheme()));
        }

        /// <summary>
        /// Sets the string value style.
        /// </summary>
        /// <param name="jsonText">The JSON text builder.</param>
        /// <param name="style">The style used for string values.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public static JsonText StringStyle(this JsonText jsonText, Style style)
        {
            return Apply(jsonText, text => text.Options.StringScheme = ToScheme(style, text.Options.ResolveStringScheme()));
        }

        /// <summary>
        /// Sets the number value style.
        /// </summary>
        /// <param name="jsonText">The JSON text builder.</param>
        /// <param name="style">The style used for number values.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public static JsonText NumberStyle(this JsonText jsonText, Style style)
        {
            return Apply(jsonText, text => text.Options.NumberScheme = ToScheme(style, text.Options.ResolveNumberScheme()));
        }

        /// <summary>
        /// Sets the boolean value style.
        /// </summary>
        /// <param name="jsonText">The JSON text builder.</param>
        /// <param name="style">The style used for boolean values.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public static JsonText BooleanStyle(this JsonText jsonText, Style style)
        {
            return Apply(jsonText, text => text.Options.BooleanScheme = ToScheme(style, text.Options.ResolveBooleanScheme()));
        }

        /// <summary>
        /// Sets the null value style.
        /// </summary>
        /// <param name="jsonText">The JSON text builder.</param>
        /// <param name="style">The style used for null values.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public static JsonText NullStyle(this JsonText jsonText, Style style)
        {
            return Apply(jsonText, text => text.Options.NullScheme = ToScheme(style, text.Options.ResolveNullScheme()));
        }

        /// <summary>
        /// Sets the object brace style.
        /// </summary>
        /// <param name="jsonText">The JSON text builder.</param>
        /// <param name="style">The style used for object braces.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public static JsonText BracesStyle(this JsonText jsonText, Style style)
        {
            return Apply(jsonText, text => text.Options.BracesScheme = ToScheme(style, text.Options.ResolveBracesScheme()));
        }

        /// <summary>
        /// Sets the array bracket style.
        /// </summary>
        /// <param name="jsonText">The JSON text builder.</param>
        /// <param name="style">The style used for array brackets.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public static JsonText BracketStyle(this JsonText jsonText, Style style)
        {
            return Apply(jsonText, text => text.Options.BracketScheme = ToScheme(style, text.Options.ResolveBracketScheme()));
        }

        /// <summary>
        /// Sets the array bracket style.
        /// </summary>
        /// <param name="jsonText">The JSON text builder.</param>
        /// <param name="style">The style used for array brackets.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public static JsonText BracketsStyle(this JsonText jsonText, Style style)
        {
            return BracketStyle(jsonText, style);
        }

        /// <summary>
        /// Sets the colon style.
        /// </summary>
        /// <param name="jsonText">The JSON text builder.</param>
        /// <param name="style">The style used for colons.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public static JsonText ColonStyle(this JsonText jsonText, Style style)
        {
            return Apply(jsonText, text => text.Options.ColonScheme = ToScheme(style, text.Options.ResolveColonScheme()));
        }

        /// <summary>
        /// Sets the comma style.
        /// </summary>
        /// <param name="jsonText">The JSON text builder.</param>
        /// <param name="style">The style used for commas.</param>
        /// <returns>The current <see cref="JsonText"/> instance.</returns>
        public static JsonText CommaStyle(this JsonText jsonText, Style style)
        {
            return Apply(jsonText, text => text.Options.CommaScheme = ToScheme(style, text.Options.ResolveCommaScheme()));
        }

        private static JsonText Apply(JsonText jsonText, Action<JsonText> configure)
        {
            if (jsonText == null)
            {
                throw new ArgumentNullException(nameof(jsonText));
            }

            configure(jsonText);
            return jsonText;
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
