using System;

namespace ConsoulLibrary.Views
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DynamicViewOptionAttribute : Attribute
    {
        /// <summary>
        /// Name of the local method used to dynamically build the message string.
        /// </summary>
        public string MessageMethod { get; set; }
        /// <summary>
        /// Name of the local method used to dynamically determine the 
        /// </summary>
        public string ColorMethod { get; set; }

        public DynamicViewOptionAttribute(string messageMethodName, string colorMethodName)
        {
            MessageMethod = messageMethodName;
            ColorMethod = colorMethodName;
        }
    }
}
