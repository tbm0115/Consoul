using System;

namespace Consoul.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ViewOptionAttribute : Attribute
    {
        public string Message { get; set; }
        public ConsoleColor Color { get; set; } = ConsoleColor.White;

        public ViewOptionAttribute(string message)
        {
            Message = message;
        }
    }
}
