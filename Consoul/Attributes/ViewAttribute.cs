using System;

namespace Consoul.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ViewAttribute : Attribute
    {
        public string Title { get; set; }

        public ViewAttribute(string title)
        {
            Title = title;
        }
    }
}
