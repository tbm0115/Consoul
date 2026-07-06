using ConsoulLibrary.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for responsive shortcut bars.
    /// </summary>
    public class ShortcutBarOptions
    {
        /// <summary>
        /// Gets or sets the requested render width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the text written before each key token.
        /// </summary>
        public string KeyLeftEdge { get; set; } = "[";

        /// <summary>
        /// Gets or sets the text written after each key token.
        /// </summary>
        public string KeyRightEdge { get; set; } = "]";

        /// <summary>
        /// Gets or sets the text written between the key token and description.
        /// </summary>
        public string KeyDescriptionSeparator { get; set; } = " ";

        /// <summary>
        /// Gets or sets the separator written between shortcut items on the same line.
        /// </summary>
        public string ItemSeparator { get; set; } = "  ";

        /// <summary>
        /// Gets or sets the text used when a shortcut item is truncated.
        /// </summary>
        public string Ellipsis { get; set; } = "...";

        /// <summary>
        /// Gets or sets a value indicating whether disabled shortcut items should be rendered.
        /// </summary>
        public bool ShowDisabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the default color scheme used for key tokens.
        /// </summary>
        public ColorScheme? KeyScheme { get; set; }

        /// <summary>
        /// Gets or sets the default color scheme used for descriptions.
        /// </summary>
        public ColorScheme? DescriptionScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for item separators.
        /// </summary>
        public ColorScheme? SeparatorScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for disabled shortcut items.
        /// </summary>
        public ColorScheme? DisabledScheme { get; set; }

        /// <summary>
        /// Creates a copy of the current shortcut bar options.
        /// </summary>
        /// <returns>A new <see cref="ShortcutBarOptions"/> instance with the same values.</returns>
        public ShortcutBarOptions Clone()
        {
            return new ShortcutBarOptions
            {
                Width = Width,
                KeyLeftEdge = KeyLeftEdge,
                KeyRightEdge = KeyRightEdge,
                KeyDescriptionSeparator = KeyDescriptionSeparator,
                ItemSeparator = ItemSeparator,
                Ellipsis = Ellipsis,
                ShowDisabled = ShowDisabled,
                KeyScheme = KeyScheme,
                DescriptionScheme = DescriptionScheme,
                SeparatorScheme = SeparatorScheme,
                DisabledScheme = DisabledScheme
            };
        }

        internal ColorScheme ResolveKeyScheme()
        {
            return KeyScheme ?? RenderOptions.PromptScheme;
        }

        internal ColorScheme ResolveDescriptionScheme()
        {
            return DescriptionScheme ?? RenderOptions.SubnoteScheme;
        }

        internal ColorScheme ResolveSeparatorScheme()
        {
            return SeparatorScheme ?? RenderOptions.SubnoteScheme;
        }

        internal ColorScheme ResolveDisabledScheme()
        {
            return DisabledScheme ?? RenderOptions.SubnoteScheme;
        }
    }
}
