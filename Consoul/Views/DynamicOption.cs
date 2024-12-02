namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a dynamic option for a console view, which can hold a customizable entry of type <typeparamref name="T"/>.
    /// This class is designed for dynamically created options, allowing flexible configuration of display and action behavior.
    /// </summary>
    /// <typeparam name="T">The type of data represented by the dynamic entry.</typeparam>
    public class DynamicOption<T> : IViewOption
    {
        /// <summary>
        /// Gets or sets the dynamic entry associated with this option.
        /// The entry includes properties such as message and color that can be updated dynamically.
        /// </summary>
        public DynamicEntry<T> Entry { get; set; }

        /// <summary>
        /// Gets or sets the callback action that will be executed when this option is selected.
        /// </summary>
        public ViewOptionCallback Action { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicOption{T}"/> class with specified message and action.
        /// Optionally, the color expression for the entry can also be set.
        /// </summary>
        /// <param name="messageExpression">The delegate expression to define the display message for this option.</param>
        /// <param name="action">The callback action to execute when this option is selected.</param>
        /// <param name="colorExpression">Optional parameter to define the color of the entry dynamically.</param>
        public DynamicOption(SetViewOptionMessage messageExpression, ViewOptionCallback action, SetViewOptionColor colorExpression = null)
        {
            // Initialize a new dynamic entry with the given message and optional color expression
            Entry = new DynamicEntry<T>(messageExpression, colorExpression);

            // Assign the provided action to execute when the option is selected
            Action = action;
        }
    }
}
