using System;

namespace Consoul.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DynamicViewOptionAttribute : Attribute
    {
        public string BuildMethod { get; set; }
        public ConsoleColor Color { get; set; } = ConsoleColor.White;

        public DynamicViewOptionAttribute(string buildMethodName)
        {
            BuildMethod = buildMethodName;
        }
    }
}
