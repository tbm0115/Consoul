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
        /// Optional helper text rendered under the option.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Optional shortcut text that can select the option.
        /// </summary>
        public string Shortcut { get; set; }

        /// <summary>
        /// Optional comma- or semicolon-separated search terms that can match prompt filtering.
        /// </summary>
        public string SearchTerms { get; set; }

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
