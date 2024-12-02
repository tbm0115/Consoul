using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a delegate used to define the action to be executed when a view option is selected.
    /// </summary>
    public delegate void ViewOptionCallback();

    /// <summary>
    /// Represents a delegate used to dynamically set the display message for a view option.
    /// </summary>
    /// <returns>A string containing the message to display.</returns>
    public delegate string SetViewOptionMessage();

    /// <summary>
    /// Represents a delegate used to dynamically set the color for a view option.
    /// </summary>
    /// <returns>A <see cref="ConsoleColor"/> representing the color to display.</returns>
    public delegate ConsoleColor SetViewOptionColor();

    /// <summary>
    /// Defines the structure for a selectable view option.
    /// This interface provides a mechanism for assigning actions to view options.
    /// </summary>
    internal interface IViewOption
    {
        /// <summary>
        /// Gets or sets the callback action that will be executed when this view option is selected.
        /// </summary>
        ViewOptionCallback Action { get; set; }
    }
}
