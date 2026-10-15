using ConsoulLibrary.Color;
using System;
using RgbColor = ConsoulLibrary.Color.RgbColor;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for a responsive timeline.
    /// </summary>
    public class TimelineOptions
    {
        /// <summary>
        /// Gets or sets the requested timeline width. The actual width is clamped to the current terminal width.
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
        /// Gets or sets a value indicating whether timestamps should be rendered when present.
        /// </summary>
        public bool ShowTimestamps { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether connector glyphs should be rendered between items.
        /// </summary>
        public bool ShowConnectors { get; set; } = true;

        /// <summary>
        /// Gets or sets the format string used for timestamps.
        /// </summary>
        public string TimestampFormat { get; set; } = "g";

        /// <summary>
        /// Gets or sets the connector rendered between timeline items.
        /// </summary>
        public string Connector { get; set; } = "|";

        /// <summary>
        /// Gets or sets the marker rendered for pending items.
        /// </summary>
        public string PendingMarker { get; set; } = "o";

        /// <summary>
        /// Gets or sets the marker rendered for active items.
        /// </summary>
        public string ActiveMarker { get; set; } = ">";

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
        /// Gets or sets the default color scheme used for item titles.
        /// </summary>
        public ColorScheme? TitleScheme { get; set; }

        /// <summary>
        /// Gets or sets the default ANSI decoration used for item titles.
        /// </summary>
        public Decoration TitleDecoration { get; set; }

        /// <summary>
        /// Gets or sets the default RGB foreground color used for item titles on ANSI-capable terminals.
        /// </summary>
        public RgbColor? TitleForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the default RGB background color used for item titles on ANSI-capable terminals.
        /// </summary>
        public RgbColor? TitleBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for descriptions.
        /// </summary>
        public ColorScheme? DescriptionScheme { get; set; }

        /// <summary>
        /// Gets or sets the default ANSI decoration used for descriptions.
        /// </summary>
        public Decoration DescriptionDecoration { get; set; }

        /// <summary>
        /// Gets or sets the default RGB foreground color used for descriptions on ANSI-capable terminals.
        /// </summary>
        public RgbColor? DescriptionForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the default RGB background color used for descriptions on ANSI-capable terminals.
        /// </summary>
        public RgbColor? DescriptionBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for pending markers.
        /// </summary>
        public ColorScheme? PendingScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for pending markers.
        /// </summary>
        public Decoration PendingDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for pending markers on ANSI-capable terminals.
        /// </summary>
        public RgbColor? PendingForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for pending markers on ANSI-capable terminals.
        /// </summary>
        public RgbColor? PendingBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for active markers.
        /// </summary>
        public ColorScheme? ActiveScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for active markers.
        /// </summary>
        public Decoration ActiveDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for active markers on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ActiveForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for active markers on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ActiveBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for completed markers.
        /// </summary>
        public ColorScheme? CompleteScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for completed markers.
        /// </summary>
        public Decoration CompleteDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for completed markers on ANSI-capable terminals.
        /// </summary>
        public RgbColor? CompleteForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for completed markers on ANSI-capable terminals.
        /// </summary>
        public RgbColor? CompleteBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for warning markers.
        /// </summary>
        public ColorScheme? WarningScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for warning markers.
        /// </summary>
        public Decoration WarningDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for warning markers on ANSI-capable terminals.
        /// </summary>
        public RgbColor? WarningForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for warning markers on ANSI-capable terminals.
        /// </summary>
        public RgbColor? WarningBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for error markers.
        /// </summary>
        public ColorScheme? ErrorScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for error markers.
        /// </summary>
        public Decoration ErrorDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for error markers on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ErrorForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for error markers on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ErrorBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for skipped markers.
        /// </summary>
        public ColorScheme? SkippedScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for skipped markers.
        /// </summary>
        public Decoration SkippedDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for skipped markers on ANSI-capable terminals.
        /// </summary>
        public RgbColor? SkippedForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for skipped markers on ANSI-capable terminals.
        /// </summary>
        public RgbColor? SkippedBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for connectors.
        /// </summary>
        public ColorScheme? ConnectorScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for connectors.
        /// </summary>
        public Decoration ConnectorDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for connectors on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ConnectorForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for connectors on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ConnectorBackgroundRgb { get; set; }

        /// <summary>
        /// Creates a copy of the current timeline options.
        /// </summary>
        /// <returns>A new <see cref="TimelineOptions"/> instance with the same values.</returns>
        public TimelineOptions Clone()
        {
            return new TimelineOptions
            {
                Width = Width,
                MarkerGap = MarkerGap,
                ShowDescriptions = ShowDescriptions,
                ShowTimestamps = ShowTimestamps,
                ShowConnectors = ShowConnectors,
                TimestampFormat = TimestampFormat,
                Connector = Connector,
                PendingMarker = PendingMarker,
                ActiveMarker = ActiveMarker,
                CompleteMarker = CompleteMarker,
                WarningMarker = WarningMarker,
                ErrorMarker = ErrorMarker,
                SkippedMarker = SkippedMarker,
                TitleScheme = TitleScheme,
                TitleDecoration = TitleDecoration,
                TitleForegroundRgb = TitleForegroundRgb,
                TitleBackgroundRgb = TitleBackgroundRgb,
                DescriptionScheme = DescriptionScheme,
                DescriptionDecoration = DescriptionDecoration,
                DescriptionForegroundRgb = DescriptionForegroundRgb,
                DescriptionBackgroundRgb = DescriptionBackgroundRgb,
                PendingScheme = PendingScheme,
                PendingDecoration = PendingDecoration,
                PendingForegroundRgb = PendingForegroundRgb,
                PendingBackgroundRgb = PendingBackgroundRgb,
                ActiveScheme = ActiveScheme,
                ActiveDecoration = ActiveDecoration,
                ActiveForegroundRgb = ActiveForegroundRgb,
                ActiveBackgroundRgb = ActiveBackgroundRgb,
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
                SkippedBackgroundRgb = SkippedBackgroundRgb,
                ConnectorScheme = ConnectorScheme,
                ConnectorDecoration = ConnectorDecoration,
                ConnectorForegroundRgb = ConnectorForegroundRgb,
                ConnectorBackgroundRgb = ConnectorBackgroundRgb
            };
        }

        internal string ResolveMarker(TimelineItemState state)
        {
            switch (state)
            {
                case TimelineItemState.Active:
                    return ActiveMarker ?? string.Empty;
                case TimelineItemState.Complete:
                    return CompleteMarker ?? string.Empty;
                case TimelineItemState.Warning:
                    return WarningMarker ?? string.Empty;
                case TimelineItemState.Error:
                    return ErrorMarker ?? string.Empty;
                case TimelineItemState.Skipped:
                    return SkippedMarker ?? string.Empty;
                default:
                    return PendingMarker ?? string.Empty;
            }
        }

        internal ColorScheme ResolveMarkerScheme(TimelineItemState state)
        {
            switch (state)
            {
                case TimelineItemState.Active:
                    return ActiveScheme ?? RenderOptions.PromptScheme;
                case TimelineItemState.Complete:
                    return CompleteScheme ?? Scheme(ConsoleColor.Green);
                case TimelineItemState.Warning:
                    return WarningScheme ?? Scheme(ConsoleColor.Yellow);
                case TimelineItemState.Error:
                    return ErrorScheme ?? RenderOptions.InvalidScheme;
                case TimelineItemState.Skipped:
                    return SkippedScheme ?? RenderOptions.SubnoteScheme;
                default:
                    return PendingScheme ?? RenderOptions.SubnoteScheme;
            }
        }

        internal ColorScheme ResolveTitleScheme()
        {
            return TitleScheme ?? RenderOptions.DefaultScheme;
        }

        internal ColorScheme ResolveDescriptionScheme()
        {
            return DescriptionScheme ?? RenderOptions.SubnoteScheme;
        }

        internal ColorScheme ResolveConnectorScheme()
        {
            return ConnectorScheme ?? RenderOptions.SubnoteScheme;
        }

        private static ColorScheme Scheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }
    }
}
