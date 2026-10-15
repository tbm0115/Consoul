using ConsoulLibrary.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for a responsive checklist.
    /// </summary>
    public class ChecklistOptions
    {
        /// <summary>
        /// Gets or sets the requested checklist width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the number of spaces written between the marker and item text.
        /// </summary>
        public int MarkerGap { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether item descriptions should be rendered.
        /// </summary>
        public bool ShowDescriptions { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether wrapped lines should align under the item label.
        /// </summary>
        public bool IndentWrappedLines { get; set; } = true;

        /// <summary>
        /// Gets or sets the marker rendered for pending items.
        /// </summary>
        public string PendingMarker { get; set; } = "[ ]";

        /// <summary>
        /// Gets or sets the marker rendered for completed items.
        /// </summary>
        public string CompleteMarker { get; set; } = "[x]";

        /// <summary>
        /// Gets or sets the marker rendered for warning items.
        /// </summary>
        public string WarningMarker { get; set; } = "[!]";

        /// <summary>
        /// Gets or sets the marker rendered for error items.
        /// </summary>
        public string ErrorMarker { get; set; } = "[X]";

        /// <summary>
        /// Gets or sets the marker rendered for skipped items.
        /// </summary>
        public string SkippedMarker { get; set; } = "[-]";

        /// <summary>
        /// Gets or sets the default color scheme used for item labels.
        /// </summary>
        public ColorScheme? LabelScheme { get; set; }

        /// <summary>
        /// Gets or sets the default ANSI decoration used for item labels.
        /// </summary>
        public Decoration LabelDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional default RGB foreground color used for item labels on ANSI-capable terminals.
        /// </summary>
        public RgbColor? LabelForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional default RGB background color used for item labels on ANSI-capable terminals.
        /// </summary>
        public RgbColor? LabelBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the default color scheme used for descriptions.
        /// </summary>
        public ColorScheme? DescriptionScheme { get; set; }

        /// <summary>
        /// Gets or sets the default ANSI decoration used for descriptions.
        /// </summary>
        public Decoration DescriptionDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional default RGB foreground color used for descriptions on ANSI-capable terminals.
        /// </summary>
        public RgbColor? DescriptionForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional default RGB background color used for descriptions on ANSI-capable terminals.
        /// </summary>
        public RgbColor? DescriptionBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for pending item markers.
        /// </summary>
        public ColorScheme? PendingScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for pending item markers.
        /// </summary>
        public Decoration PendingDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for pending item markers on ANSI-capable terminals.
        /// </summary>
        public RgbColor? PendingForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for pending item markers on ANSI-capable terminals.
        /// </summary>
        public RgbColor? PendingBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for completed item markers.
        /// </summary>
        public ColorScheme? CompleteScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for completed item markers.
        /// </summary>
        public Decoration CompleteDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for completed item markers on ANSI-capable terminals.
        /// </summary>
        public RgbColor? CompleteForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for completed item markers on ANSI-capable terminals.
        /// </summary>
        public RgbColor? CompleteBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for warning item markers.
        /// </summary>
        public ColorScheme? WarningScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for warning item markers.
        /// </summary>
        public Decoration WarningDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for warning item markers on ANSI-capable terminals.
        /// </summary>
        public RgbColor? WarningForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for warning item markers on ANSI-capable terminals.
        /// </summary>
        public RgbColor? WarningBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for error item markers.
        /// </summary>
        public ColorScheme? ErrorScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for error item markers.
        /// </summary>
        public Decoration ErrorDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for error item markers on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ErrorForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for error item markers on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ErrorBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for skipped item markers.
        /// </summary>
        public ColorScheme? SkippedScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for skipped item markers.
        /// </summary>
        public Decoration SkippedDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for skipped item markers on ANSI-capable terminals.
        /// </summary>
        public RgbColor? SkippedForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for skipped item markers on ANSI-capable terminals.
        /// </summary>
        public RgbColor? SkippedBackgroundRgb { get; set; }

        /// <summary>
        /// Creates a copy of the current checklist options.
        /// </summary>
        /// <returns>A new <see cref="ChecklistOptions"/> instance with the same values.</returns>
        public ChecklistOptions Clone()
        {
            return new ChecklistOptions
            {
                Width = Width,
                MarkerGap = MarkerGap,
                ShowDescriptions = ShowDescriptions,
                IndentWrappedLines = IndentWrappedLines,
                PendingMarker = PendingMarker,
                CompleteMarker = CompleteMarker,
                WarningMarker = WarningMarker,
                ErrorMarker = ErrorMarker,
                SkippedMarker = SkippedMarker,
                LabelScheme = LabelScheme,
                LabelDecoration = LabelDecoration,
                LabelForegroundRgb = LabelForegroundRgb,
                LabelBackgroundRgb = LabelBackgroundRgb,
                DescriptionScheme = DescriptionScheme,
                DescriptionDecoration = DescriptionDecoration,
                DescriptionForegroundRgb = DescriptionForegroundRgb,
                DescriptionBackgroundRgb = DescriptionBackgroundRgb,
                PendingScheme = PendingScheme,
                PendingDecoration = PendingDecoration,
                PendingForegroundRgb = PendingForegroundRgb,
                PendingBackgroundRgb = PendingBackgroundRgb,
                CompleteScheme = CompleteScheme,
                CompleteDecoration = CompleteDecoration,
                CompleteForegroundRgb = CompleteForegroundRgb,
                CompleteBackgroundRgb = CompleteBackgroundRgb,
                WarningScheme = WarningScheme,
                WarningDecoration = WarningDecoration,
                WarningForegroundRgb = WarningForegroundRgb,
                WarningBackgroundRgb = WarningBackgroundRgb,
                ErrorScheme = ErrorScheme,
                ErrorDecoration = ErrorDecoration,
                ErrorForegroundRgb = ErrorForegroundRgb,
                ErrorBackgroundRgb = ErrorBackgroundRgb,
                SkippedScheme = SkippedScheme,
                SkippedDecoration = SkippedDecoration,
                SkippedForegroundRgb = SkippedForegroundRgb,
                SkippedBackgroundRgb = SkippedBackgroundRgb
            };
        }

        internal Decoration ResolveMarkerDecoration(ChecklistItemState state)
        {
            switch (state)
            {
                case ChecklistItemState.Complete:
                    return CompleteDecoration;
                case ChecklistItemState.Warning:
                    return WarningDecoration;
                case ChecklistItemState.Error:
                    return ErrorDecoration;
                case ChecklistItemState.Skipped:
                    return SkippedDecoration;
                default:
                    return PendingDecoration;
            }
        }

        internal RgbColor? ResolveMarkerForegroundRgb(ChecklistItemState state)
        {
            switch (state)
            {
                case ChecklistItemState.Complete:
                    return CompleteForegroundRgb;
                case ChecklistItemState.Warning:
                    return WarningForegroundRgb;
                case ChecklistItemState.Error:
                    return ErrorForegroundRgb;
                case ChecklistItemState.Skipped:
                    return SkippedForegroundRgb;
                default:
                    return PendingForegroundRgb;
            }
        }

        internal RgbColor? ResolveMarkerBackgroundRgb(ChecklistItemState state)
        {
            switch (state)
            {
                case ChecklistItemState.Complete:
                    return CompleteBackgroundRgb;
                case ChecklistItemState.Warning:
                    return WarningBackgroundRgb;
                case ChecklistItemState.Error:
                    return ErrorBackgroundRgb;
                case ChecklistItemState.Skipped:
                    return SkippedBackgroundRgb;
                default:
                    return PendingBackgroundRgb;
            }
        }

        internal string ResolveMarker(ChecklistItemState state)
        {
            switch (state)
            {
                case ChecklistItemState.Complete:
                    return CompleteMarker ?? string.Empty;
                case ChecklistItemState.Warning:
                    return WarningMarker ?? string.Empty;
                case ChecklistItemState.Error:
                    return ErrorMarker ?? string.Empty;
                case ChecklistItemState.Skipped:
                    return SkippedMarker ?? string.Empty;
                default:
                    return PendingMarker ?? string.Empty;
            }
        }

        internal ColorScheme ResolveMarkerScheme(ChecklistItemState state)
        {
            switch (state)
            {
                case ChecklistItemState.Complete:
                    return CompleteScheme ?? Scheme(RenderOptions.OptionColor);
                case ChecklistItemState.Warning:
                    return WarningScheme ?? Scheme(System.ConsoleColor.Yellow);
                case ChecklistItemState.Error:
                    return ErrorScheme ?? RenderOptions.InvalidScheme;
                case ChecklistItemState.Skipped:
                    return SkippedScheme ?? RenderOptions.SubnoteScheme;
                default:
                    return PendingScheme ?? RenderOptions.SubnoteScheme;
            }
        }

        internal ColorScheme ResolveLabelScheme()
        {
            return LabelScheme ?? RenderOptions.DefaultScheme;
        }

        internal ColorScheme ResolveDescriptionScheme()
        {
            return DescriptionScheme ?? RenderOptions.SubnoteScheme;
        }

        private static ColorScheme Scheme(System.ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }
    }
}
