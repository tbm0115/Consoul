using System;
using System.Linq.Expressions;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a selectable option in a view, along with an associated action to execute when the option is chosen.
    /// </summary>
    public class ViewOption : IViewOption
    {
        /// <summary>
        /// Gets or sets the <see cref="LineEntry"/> that represents the text displayed for this option.
        /// </summary>
        public LineEntry Entry { get; set; }

        /// <summary>
        /// Gets or sets the callback action to be invoked when this option is selected.
        /// </summary>
        public ViewOptionCallback Action { get; set; }

        /// <summary>
        /// Gets or sets the color of the option text.
        /// </summary>
        public ConsoleColor Color { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewOption"/> class with the specified message, action, and optional color.
        /// </summary>
        /// <param name="message">The message to display for this view option.</param>
        /// <param name="action">The callback action to invoke when this option is selected.</param>
        /// <param name="color">The color to display for the message text. If null, defaults to the <see cref="RenderOptions.DefaultColor"/>.</param>
        public ViewOption(string message, ViewOptionCallback action, ConsoleColor? color = null)
        {
            if (color == null)
                color = RenderOptions.DefaultScheme.Color;

            // Create a new LineEntry to represent the view option.
            Entry = new LineEntry(message, (ConsoleColor)color);

            Action = action;
            Color = Entry.Color;
        }

        /// <summary>
        /// Builds the message to be displayed for this view option, allowing for an optional template transformation.
        /// </summary>
        /// <param name="template">
        /// An optional expression used to customize the message. The expression should take a string as input and return a string.
        /// </param>
        /// <returns>The formatted message for this view option.</returns>
        public string Render(Expression<Func<string, string>> template = null)
        {
            if (template != null)
            {
                // If a template is provided, compile and apply it to the existing message.
                return template.Compile().Invoke(Entry.Message);
            }
            return Entry.Message;
        }
    }
}
