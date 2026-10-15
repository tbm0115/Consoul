using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Supplies display metadata for a Consoul view.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ViewAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the title rendered for the view.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the label used for the generated navigation option that returns to the previous view.
        /// </summary>
        public string GoBackMessage { get; set; } = RenderOptions.DefaultGoBackMessage;

        /// <summary>
        /// Initializes a new view metadata attribute.
        /// </summary>
        /// <param name="title">Title rendered for the view.</param>
        public ViewAttribute(string title)
        {
            Title = title;
        }
    }
}
