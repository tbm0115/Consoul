using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Indicates that a method can be rendered as an option within a view.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ViewOptionAttribute : Attribute
    {
        /// <summary>
        /// Rendered text for the option.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Rendered text color for the option.
        /// </summary>
        public ConsoleColor Color { get; set; } = RenderOptions.DefaultColor;

        /// <summary>
        /// Constructs a new instance of the attribute.
        /// </summary>
        /// <param name="message">Message for the option</param>
        public ViewOptionAttribute(string message)
        {
            Message = message;
        }
    }
}
