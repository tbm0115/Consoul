using System.Text.Json;

namespace ConsoulLibrary
{
    /// <summary>
    /// Optional System.Text.Json helpers for Consoul.
    /// </summary>
    public static class ConsoulTextJsonExtensions
    {
        /// <summary>
        /// Configures <see cref="EditObjectView"/> to render JSON editor values with System.Text.Json.
        /// </summary>
        /// <param name="options">Optional serializer options.</param>
        public static void UseSystemTextJsonForObjectEditor(JsonSerializerOptions options = null)
        {
            EditObjectView.JsonValueSerializer = (value, type) =>
                JsonSerializer.Serialize(value, type ?? value?.GetType() ?? typeof(object), options);
        }
    }
}
