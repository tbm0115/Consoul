using System.Text.Json;

namespace ConsoulLibrary
{
    /// <summary>
    /// Optional System.Text.Json helpers for Consoul.
    /// </summary>
    public static class ConsoulTextJsonExtensions
    {
        /// <summary>
        /// Configures <see cref="EditObjectView"/> to render form document values with System.Text.Json.
        /// </summary>
        /// <param name="options">Optional serializer options.</param>
        public static void UseSystemTextJsonForFormValues(JsonSerializerOptions options = null)
        {
            EditObjectView.FormValueSerializer = (value, type) =>
                JsonSerializer.Serialize(value, type ?? value?.GetType() ?? typeof(object), options);
        }

        /// <summary>
        /// Configures <see cref="EditObjectView"/> to render form document values with System.Text.Json.
        /// </summary>
        /// <param name="options">Optional serializer options.</param>
        /// <remarks>
        /// This compatibility alias is retained for callers that adopted the original object-editor terminology.
        /// Prefer <see cref="UseSystemTextJsonForFormValues"/> for new MVC-style object editor views.
        /// </remarks>
        public static void UseSystemTextJsonForObjectEditor(JsonSerializerOptions options = null)
        {
            UseSystemTextJsonForFormValues(options);
        }

        /// <summary>
        /// Renders syntax-colored JSON text through Consoul's console renderer.
        /// </summary>
        /// <param name="json">The JSON text to render.</param>
        /// <param name="options">Optional rendering options for the JSON text.</param>
        public static void Json(string json, JsonTextOptions options = null)
        {
            JsonTextEntry.Render(json, options);
        }

        /// <summary>
        /// Serializes a value and renders it as syntax-colored JSON through Consoul's console renderer.
        /// </summary>
        /// <typeparam name="T">The value type to serialize.</typeparam>
        /// <param name="value">The value to serialize and render.</param>
        /// <param name="options">Optional rendering options for the JSON text.</param>
        /// <param name="serializerOptions">Optional serializer options used before rendering.</param>
        public static void JsonValue<T>(T value, JsonTextOptions options = null, JsonSerializerOptions serializerOptions = null)
        {
            JsonTextEntry.RenderValue(value, typeof(T), options, serializerOptions);
        }
    }
}
