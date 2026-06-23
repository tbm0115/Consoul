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
        /// Initializes a new dynamic view option mapping.
        /// </summary>
        /// <param name="messageMethodName">Name of the method that returns the option label.</param>
        /// <param name="colorMethodName">Name of the method that returns the option color.</param>
        public DynamicViewOptionAttribute(string messageMethodName, string colorMethodName)
        {
            MessageMethod = messageMethodName;
            ColorMethod = colorMethodName;
        }
    }
}
