using ConsoulLibrary.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for responsive breadcrumb navigation cues.
    /// </summary>
    public class BreadcrumbOptions
    {
        /// <summary>
        /// Gets or sets the requested render width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the separator written between breadcrumb items.
        /// </summary>
        public string Separator { get; set; } = " > ";

        /// <summary>
        /// Gets or sets the text used when middle breadcrumb items are elided.
        /// </summary>
        public string Ellipsis { get; set; } = "...";

        /// <summary>
        /// Gets or sets a value indicating whether the final breadcrumb item should use the current item color scheme.
        /// </summary>
        public bool HighlightCurrent { get; set; } = true;

        /// <summary>
        /// Gets or sets the color scheme used for non-current breadcrumb items.
        /// </summary>
        public ColorScheme? ItemScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for the final breadcrumb item.
        /// </summary>
        public ColorScheme? CurrentScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for separators.
        /// </summary>
        public ColorScheme? SeparatorScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for the ellipsis marker.
        /// </summary>
        public ColorScheme? EllipsisScheme { get; set; }

        /// <summary>
        /// Creates a copy of the current breadcrumb options.
        /// </summary>
        /// <returns>A new <see cref="BreadcrumbOptions"/> instance with the same values.</returns>
        public BreadcrumbOptions Clone()
        {
            return new BreadcrumbOptions
            {
                Width = Width,
                Separator = Separator,
                Ellipsis = Ellipsis,
                HighlightCurrent = HighlightCurrent,
                ItemScheme = ItemScheme,
                CurrentScheme = CurrentScheme,
                SeparatorScheme = SeparatorScheme,
                EllipsisScheme = EllipsisScheme
            };
        }

        internal ColorScheme ResolveItemScheme()
        {
            return ItemScheme ?? RenderOptions.SubnoteScheme;
        }

        internal ColorScheme ResolveCurrentScheme()
        {
            return CurrentScheme ?? RenderOptions.PromptScheme;
        }

        internal ColorScheme ResolveSeparatorScheme()
        {
            return SeparatorScheme ?? RenderOptions.SubnoteScheme;
        }

        internal ColorScheme ResolveEllipsisScheme()
        {
            return EllipsisScheme ?? ResolveSeparatorScheme();
        }
    }
}
