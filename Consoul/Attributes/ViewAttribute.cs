using System;

namespace ConsoulLibrary.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ViewAttribute : Attribute
    {
        public string Title { get; set; }

        public string GoBackMessage { get; set; } = RenderOptions.DefaultGoBackMessage;

        public ViewAttribute(string title)
        {
            Title = title;
        }
    }
}
