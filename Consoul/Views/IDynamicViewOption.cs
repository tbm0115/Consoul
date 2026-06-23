namespace ConsoulLibrary
{
    /// <summary>
    /// Defines the structure for a dynamic view option of type <typeparamref name="T"/>.
    /// This interface allows for dynamically setting properties such as the display message and color of a view option.
    /// </summary>
    /// <typeparam name="T">The type of the associated data for the dynamic view option.</typeparam>
    internal interface IDynamicViewOption<T>
    {
        /// <summary>
        /// Gets or sets the delegate used to define the display message for the view option.
        /// This delegate allows the message to be dynamically generated or updated.
        /// </summary>
        SetViewOptionMessage SetMessage { get; set; }

        /// <summary>
        /// Gets or sets the delegate used to define the display color for the view option.
        /// This delegate allows the color to be dynamically generated or updated.
        /// </summary>
        SetViewOptionColor SetColor { get; set; }
    }
}
