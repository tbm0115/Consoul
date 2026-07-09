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
        /// Gets or sets the ANSI decoration used for non-current breadcrumb items.
        /// </summary>
        public Decoration ItemDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for non-current breadcrumb items on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ItemForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for non-current breadcrumb items on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ItemBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for the final breadcrumb item.
        /// </summary>
        public ColorScheme? CurrentScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for the final breadcrumb item.
        /// </summary>
        public Decoration CurrentDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for the final breadcrumb item on ANSI-capable terminals.
        /// </summary>
        public RgbColor? CurrentForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for the final breadcrumb item on ANSI-capable terminals.
        /// </summary>
        public RgbColor? CurrentBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for separators.
        /// </summary>
        public ColorScheme? SeparatorScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for separators.
        /// </summary>
        public Decoration SeparatorDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for separators on ANSI-capable terminals.
        /// </summary>
        public RgbColor? SeparatorForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for separators on ANSI-capable terminals.
        /// </summary>
        public RgbColor? SeparatorBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for the ellipsis marker.
        /// </summary>
        public ColorScheme? EllipsisScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for the ellipsis marker.
        /// </summary>
        public Decoration EllipsisDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for the ellipsis marker on ANSI-capable terminals.
        /// </summary>
        public RgbColor? EllipsisForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for the ellipsis marker on ANSI-capable terminals.
        /// </summary>
        public RgbColor? EllipsisBackgroundRgb { get; set; }

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
                ItemDecoration = ItemDecoration,
                ItemForegroundRgb = ItemForegroundRgb,
                ItemBackgroundRgb = ItemBackgroundRgb,
                CurrentScheme = CurrentScheme,
                CurrentDecoration = CurrentDecoration,
                CurrentForegroundRgb = CurrentForegroundRgb,
                CurrentBackgroundRgb = CurrentBackgroundRgb,
                SeparatorScheme = SeparatorScheme,
                SeparatorDecoration = SeparatorDecoration,
                SeparatorForegroundRgb = SeparatorForegroundRgb,
                SeparatorBackgroundRgb = SeparatorBackgroundRgb,
                EllipsisScheme = EllipsisScheme,
                EllipsisDecoration = EllipsisDecoration,
                EllipsisForegroundRgb = EllipsisForegroundRgb,
                EllipsisBackgroundRgb = EllipsisBackgroundRgb
            };
        }

        internal ColorScheme ResolveItemScheme()
        {
            return ItemScheme ?? RenderOptions.SubnoteScheme;
        }

        internal StyledText ResolveItemStyle()
        {
            return new StyledText(ResolveItemScheme(), ItemDecoration, ItemForegroundRgb, ItemBackgroundRgb);
        }

        internal ColorScheme ResolveCurrentScheme()
        {
            return CurrentScheme ?? RenderOptions.PromptScheme;
        }

        internal StyledText ResolveCurrentStyle()
        {
            return new StyledText(ResolveCurrentScheme(), CurrentDecoration, CurrentForegroundRgb, CurrentBackgroundRgb);
        }

        internal ColorScheme ResolveSeparatorScheme()
        {
            return SeparatorScheme ?? RenderOptions.SubnoteScheme;
        }

        internal StyledText ResolveSeparatorStyle()
        {
            return new StyledText(ResolveSeparatorScheme(), SeparatorDecoration, SeparatorForegroundRgb, SeparatorBackgroundRgb);
        }

        internal ColorScheme ResolveEllipsisScheme()
        {
            return EllipsisScheme ?? ResolveSeparatorScheme();
        }

        internal StyledText ResolveEllipsisStyle()
        {
            if (EllipsisScheme.HasValue ||
                EllipsisDecoration != Decoration.None ||
                EllipsisForegroundRgb.HasValue ||
                EllipsisBackgroundRgb.HasValue)
            {
                return new StyledText(ResolveEllipsisScheme(), EllipsisDecoration, EllipsisForegroundRgb, EllipsisBackgroundRgb);
            }

            return ResolveSeparatorStyle();
        }

        internal readonly struct StyledText
        {
            public StyledText(
                ColorScheme scheme,
                Decoration decoration,
                RgbColor? foregroundRgb,
                RgbColor? backgroundRgb)
            {
                Scheme = scheme;
                Decoration = decoration;
                ForegroundRgb = foregroundRgb;
                BackgroundRgb = backgroundRgb;
            }

            public ColorScheme Scheme { get; }

            public Decoration Decoration { get; }

            public RgbColor? ForegroundRgb { get; }

            public RgbColor? BackgroundRgb { get; }

            public static StyledText FromStyle(Style style, ColorScheme fallback)
            {
                Style resolved = style ?? Style.Plain;
                return new StyledText(
                    resolved.ToColorScheme(fallback),
                    resolved.Decoration,
                    resolved.ForegroundRgb,
                    resolved.BackgroundRgb);
            }
        }
    }
}
