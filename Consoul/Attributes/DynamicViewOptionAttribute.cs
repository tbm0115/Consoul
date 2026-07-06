using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Marks a dynamic view method as an option whose label and color are resolved by companion methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DynamicViewOptionAttribute : Attribute
    {
        /// <summary>
        /// Name of the local method used to dynamically build the message string.
        /// </summary>
        public string MessageMethod { get; set; }
        /// <summary>
        /// Name of the local method used to dynamically determine the option color.
        /// </summary>
        public string ColorMethod { get; set; }

        /// <summary>
        /// Name of the local method used to dynamically build optional helper text for the option.
        /// </summary>
        public string DescriptionMethod { get; set; }

        /// <summary>
        /// Name of the local method used to dynamically build optional shortcut text for the option.
        /// </summary>
        public string ShortcutMethod { get; set; }

        /// <summary>
        /// Name of the local method used to dynamically build hidden search terms for the option.
        /// </summary>
        public string SearchTermsMethod { get; set; }

        /// <summary>
        /// Initializes a new dynamic view option mapping.
        /// </summary>
        /// <param name="messageMethodName">Name of the method that returns the option label.</param>
        /// <param name="colorMethodName">Name of the method that returns the option color.</param>
        public DynamicViewOptionAttribute(string messageMethodName, string colorMethodName)
        {
            MessageMethod = messageMethodName;
            ColorMethod = colorMethodName;
        }

        /// <summary>
        /// Initializes a new dynamic view option mapping with optional helper text.
        /// </summary>
        /// <param name="messageMethodName">Name of the method that returns the option label.</param>
        /// <param name="colorMethodName">Name of the method that returns the option color.</param>
        /// <param name="descriptionMethodName">Name of the method that returns option helper text.</param>
        public DynamicViewOptionAttribute(string messageMethodName, string colorMethodName, string descriptionMethodName)
            : this(messageMethodName, colorMethodName)
        {
            DescriptionMethod = descriptionMethodName;
        }

        /// <summary>
        /// Initializes a new dynamic view option mapping with optional helper and shortcut text.
        /// </summary>
        /// <param name="messageMethodName">Name of the method that returns the option label.</param>
        /// <param name="colorMethodName">Name of the method that returns the option color.</param>
        /// <param name="descriptionMethodName">Name of the method that returns option helper text.</param>
        /// <param name="shortcutMethodName">Name of the method that returns option shortcut text.</param>
        public DynamicViewOptionAttribute(string messageMethodName, string colorMethodName, string descriptionMethodName, string shortcutMethodName)
            : this(messageMethodName, colorMethodName, descriptionMethodName)
        {
            ShortcutMethod = shortcutMethodName;
        }

        /// <summary>
        /// Initializes a new dynamic view option mapping with optional helper text, shortcut text, and hidden search terms.
        /// </summary>
        /// <param name="messageMethodName">Name of the method that returns the option label.</param>
        /// <param name="colorMethodName">Name of the method that returns the option color.</param>
        /// <param name="descriptionMethodName">Name of the method that returns option helper text.</param>
        /// <param name="shortcutMethodName">Name of the method that returns option shortcut text.</param>
        /// <param name="searchTermsMethodName">Name of the method that returns option search terms.</param>
        public DynamicViewOptionAttribute(
            string messageMethodName,
            string colorMethodName,
            string descriptionMethodName,
            string shortcutMethodName,
            string searchTermsMethodName)
            : this(messageMethodName, colorMethodName, descriptionMethodName, shortcutMethodName)
        {
            SearchTermsMethod = searchTermsMethodName;
        }
    }
}
