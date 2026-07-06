using ConsoulLibrary.Color;
using System;

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
        /// Gets or sets the color scheme used for descriptions.
        /// </summary>
        public ColorScheme? DescriptionScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for pending markers.
        /// </summary>
        public ColorScheme? PendingScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for active markers.
        /// </summary>
        public ColorScheme? ActiveScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for completed markers.
        /// </summary>
        public ColorScheme? CompleteScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for warning markers.
        /// </summary>
        public ColorScheme? WarningScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for error markers.
        /// </summary>
        public ColorScheme? ErrorScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for skipped markers.
        /// </summary>
        public ColorScheme? SkippedScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for connectors.
        /// </summary>
        public ColorScheme? ConnectorScheme { get; set; }

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
                DescriptionScheme = DescriptionScheme,
                PendingScheme = PendingScheme,
                ActiveScheme = ActiveScheme,
                CompleteScheme = CompleteScheme,
                WarningScheme = WarningScheme,
                ErrorScheme = ErrorScheme,
                SkippedScheme = SkippedScheme,
                ConnectorScheme = ConnectorScheme
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
