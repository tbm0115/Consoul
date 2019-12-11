using System;

namespace ConsoulLibrary.Views
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ViewOptionAttribute : Attribute
    {
        public string Message { get; set; }
        public ConsoleColor Color { get; set; } = RenderOptions.DefaultColor;

        public ViewOptionAttribute(string message)
        {
            Message = message;
        }
    }
}
