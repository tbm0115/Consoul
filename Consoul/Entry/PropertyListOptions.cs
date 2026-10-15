using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for a responsive property list.
    /// </summary>
    public class PropertyListOptions
    {
        /// <summary>
        /// Gets or sets the requested property list width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the preferred label column width. When omitted, the renderer derives a compact label width.
        /// </summary>
        public int? LabelWidth { get; set; }

        /// <summary>
        /// Gets or sets the separator rendered between labels and values.
        /// </summary>
        public string Separator { get; set; } = ": ";

        /// <summary>
        /// Gets or sets a value indicating whether wrapped values align beneath the first value column.
        /// </summary>
        public bool IndentWrappedValues { get; set; } = true;

        /// <summary>
        /// Gets or sets the default color scheme used for labels.
        /// </summary>
        public ColorScheme? LabelScheme { get; set; }

        /// <summary>
        /// Gets or sets the default ANSI decoration used for labels.
        /// </summary>
        public Decoration LabelDecoration { get; set; }

        /// <summary>
        /// Gets or sets the default RGB foreground color used for labels on ANSI-capable terminals.
        /// </summary>
        public RgbColor? LabelForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the default RGB background color used for labels on ANSI-capable terminals.
        /// </summary>
        public RgbColor? LabelBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for separators.
        /// </summary>
        public ColorScheme? SeparatorScheme { get; set; }

        /// <summary>
        /// Gets or sets the default color scheme used for values.
        /// </summary>
        public ColorScheme? ValueScheme { get; set; }

        /// <summary>
        /// Gets or sets the default ANSI decoration used for values.
        /// </summary>
        public Decoration ValueDecoration { get; set; }

        /// <summary>
        /// Gets or sets the default RGB foreground color used for values on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ValueForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the default RGB background color used for values on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ValueBackgroundRgb { get; set; }

        /// <summary>
        /// Creates a copy of the current property list options.
        /// </summary>
        /// <returns>A new <see cref="PropertyListOptions"/> instance with the same values.</returns>
        public PropertyListOptions Clone()
        {
            return new PropertyListOptions
            {
                Width = Width,
                LabelWidth = LabelWidth,
                Separator = Separator,
                IndentWrappedValues = IndentWrappedValues,
                LabelScheme = LabelScheme,
                LabelDecoration = LabelDecoration,
                LabelForegroundRgb = LabelForegroundRgb,
                LabelBackgroundRgb = LabelBackgroundRgb,
                SeparatorScheme = SeparatorScheme,
                ValueScheme = ValueScheme,
                ValueDecoration = ValueDecoration,
                ValueForegroundRgb = ValueForegroundRgb,
                ValueBackgroundRgb = ValueBackgroundRgb
            };
        }

        internal ColorScheme ResolveLabelScheme()
        {
            return LabelScheme ?? RenderOptions.SubnoteScheme;
        }

        internal ColorScheme ResolveSeparatorScheme()
        {
            return SeparatorScheme ?? ResolveLabelScheme();
        }

        internal ColorScheme ResolveValueScheme()
        {
            return ValueScheme ?? RenderOptions.DefaultScheme;
        }
    }
}
