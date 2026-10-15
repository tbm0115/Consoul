namespace ConsoulLibrary
{
    /// <summary>
    /// Defines optional navigation chrome rendered by MVC-style console views.
    /// </summary>
    public class ViewChromeOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the current navigation breadcrumb should render above the view.
        /// </summary>
        public bool ShowBreadcrumbs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether shortcut hints should render above the view.
        /// </summary>
        public bool ShowShortcuts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the generated go-back shortcut should appear in shortcut hints.
        /// </summary>
        public bool ShowGoBackShortcut { get; set; } = true;

        /// <summary>
        /// Gets or sets the shortcut token displayed for the generated go-back action.
        /// </summary>
        public string GoBackShortcut { get; set; } = "back";

        /// <summary>
        /// Gets or sets the description displayed for the generated go-back shortcut.
        /// </summary>
        public string GoBackDescription { get; set; } = "Go back";

        /// <summary>
        /// Gets or sets optional rendering options for breadcrumbs.
        /// </summary>
        public BreadcrumbOptions BreadcrumbOptions { get; set; }

        /// <summary>
        /// Gets or sets optional rendering options for shortcut hints.
        /// </summary>
        public ShortcutBarOptions ShortcutBarOptions { get; set; }

        /// <summary>
        /// Creates a copy of the current chrome options.
        /// </summary>
        /// <returns>A new <see cref="ViewChromeOptions"/> instance with the same values.</returns>
        public ViewChromeOptions Clone()
        {
            return new ViewChromeOptions
            {
                ShowBreadcrumbs = ShowBreadcrumbs,
                ShowShortcuts = ShowShortcuts,
                ShowGoBackShortcut = ShowGoBackShortcut,
                GoBackShortcut = GoBackShortcut,
                GoBackDescription = GoBackDescription,
                BreadcrumbOptions = BreadcrumbOptions != null ? BreadcrumbOptions.Clone() : null,
                ShortcutBarOptions = ShortcutBarOptions != null ? ShortcutBarOptions.Clone() : null
            };
        }
    }
}
