using System;
using System.Text.Json;

namespace ConsoulLibrary
{
    /// <summary>
    /// Optional JSON rendering helpers for injected <see cref="IAnsiConsole"/> instances.
    /// </summary>
    public static class ConsoulTextJsonConsoleExtensions
    {
        /// <summary>
        /// Renders syntax-colored JSON text through the supplied console facade.
        /// </summary>
        /// <param name="console">The console facade used to render the JSON.</param>
        /// <param name="json">The JSON text to render.</param>
        /// <param name="options">Optional rendering options for the JSON text.</param>
        public static void Json(this IAnsiConsole console, string json, JsonTextOptions options = null)
        {
            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            console.Write(new JsonTextEntry(json, options));
        }

        /// <summary>
        /// Serializes a value and renders it as syntax-colored JSON through the supplied console facade.
        /// </summary>
        /// <typeparam name="T">The value type to serialize.</typeparam>
        /// <param name="console">The console facade used to render the JSON.</param>
        /// <param name="value">The value to serialize and render.</param>
        /// <param name="options">Optional rendering options for the JSON text.</param>
        /// <param name="serializerOptions">Optional serializer options used before rendering.</param>
        public static void JsonValue<T>(
            this IAnsiConsole console,
            T value,
            JsonTextOptions options = null,
            JsonSerializerOptions serializerOptions = null)
        {
            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            console.Write(JsonTextEntry.FromValue(value, typeof(T), options, serializerOptions));
        }
    }
}
