using ConsoulLibrary.Color;
using System;
using RgbColor = ConsoulLibrary.Color.RgbColor;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for compact metric summaries.
    /// </summary>
    public class MetricsOptions
    {
        /// <summary>
        /// Gets or sets the requested render width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the requested number of metric columns.
        /// </summary>
        public int? Columns { get; set; }

        /// <summary>
        /// Gets or sets the minimum preferred width for a metric column.
        /// </summary>
        public int MinimumColumnWidth { get; set; } = 14;

        /// <summary>
        /// Gets or sets the number of spaces written between metric columns.
        /// </summary>
        public int ColumnGap { get; set; } = 2;

        /// <summary>
        /// Gets or sets a value indicating whether metric notes should be rendered.
        /// </summary>
        public bool ShowNotes { get; set; } = true;

        /// <summary>
        /// Gets or sets the color scheme used for metric labels.
        /// </summary>
        public ColorScheme? LabelScheme { get; set; }

        /// <summary>
        /// Gets or sets the default ANSI decoration used for metric labels.
        /// </summary>
        public Decoration LabelDecoration { get; set; }

        /// <summary>
        /// Gets or sets the default RGB foreground color used for metric labels on ANSI-capable terminals.
        /// </summary>
        public RgbColor? LabelForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the default RGB background color used for metric labels on ANSI-capable terminals.
        /// </summary>
        public RgbColor? LabelBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the default color scheme used for metric values.
        /// </summary>
        public ColorScheme? ValueScheme { get; set; }

        /// <summary>
        /// Gets or sets the default ANSI decoration used for metric values.
        /// </summary>
        public Decoration ValueDecoration { get; set; }

        /// <summary>
        /// Gets or sets the default RGB foreground color used for default metric values on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ValueForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the default RGB background color used for default metric values on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ValueBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for metric notes.
        /// </summary>
        public ColorScheme? NoteScheme { get; set; }

        /// <summary>
        /// Gets or sets the default ANSI decoration used for metric notes.
        /// </summary>
        public Decoration NoteDecoration { get; set; }

        /// <summary>
        /// Gets or sets the default RGB foreground color used for metric notes on ANSI-capable terminals.
        /// </summary>
        public RgbColor? NoteForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the default RGB background color used for metric notes on ANSI-capable terminals.
        /// </summary>
        public RgbColor? NoteBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for informational metric values.
        /// </summary>
        public ColorScheme? InfoScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for informational metric values.
        /// </summary>
        public Decoration InfoDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for informational metric values on ANSI-capable terminals.
        /// </summary>
        public RgbColor? InfoForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for informational metric values on ANSI-capable terminals.
        /// </summary>
        public RgbColor? InfoBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for successful or healthy metric values.
        /// </summary>
        public ColorScheme? SuccessScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for successful or healthy metric values.
        /// </summary>
        public Decoration SuccessDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for successful or healthy metric values on ANSI-capable terminals.
        /// </summary>
        public RgbColor? SuccessForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for successful or healthy metric values on ANSI-capable terminals.
        /// </summary>
        public RgbColor? SuccessBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for warning metric values.
        /// </summary>
        public ColorScheme? WarningScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for warning metric values.
        /// </summary>
        public Decoration WarningDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for warning metric values on ANSI-capable terminals.
        /// </summary>
        public RgbColor? WarningForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for warning metric values on ANSI-capable terminals.
        /// </summary>
        public RgbColor? WarningBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for error or unhealthy metric values.
        /// </summary>
        public ColorScheme? ErrorScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for error or unhealthy metric values.
        /// </summary>
        public Decoration ErrorDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for error or unhealthy metric values on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ErrorForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for error or unhealthy metric values on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ErrorBackgroundRgb { get; set; }

        /// <summary>
        /// Creates a copy of the current metrics options.
        /// </summary>
        /// <returns>A new <see cref="MetricsOptions"/> instance with the same values.</returns>
        public MetricsOptions Clone()
        {
            return new MetricsOptions
            {
                Width = Width,
                Columns = Columns,
                MinimumColumnWidth = MinimumColumnWidth,
                ColumnGap = ColumnGap,
                ShowNotes = ShowNotes,
                LabelScheme = LabelScheme,
                LabelDecoration = LabelDecoration,
                LabelForegroundRgb = LabelForegroundRgb,
                LabelBackgroundRgb = LabelBackgroundRgb,
                ValueScheme = ValueScheme,
                ValueDecoration = ValueDecoration,
                ValueForegroundRgb = ValueForegroundRgb,
                ValueBackgroundRgb = ValueBackgroundRgb,
                NoteScheme = NoteScheme,
                NoteDecoration = NoteDecoration,
                NoteForegroundRgb = NoteForegroundRgb,
                NoteBackgroundRgb = NoteBackgroundRgb,
                InfoScheme = InfoScheme,
                InfoDecoration = InfoDecoration,
                InfoForegroundRgb = InfoForegroundRgb,
                InfoBackgroundRgb = InfoBackgroundRgb,
                SuccessScheme = SuccessScheme,
                SuccessDecoration = SuccessDecoration,
                SuccessForegroundRgb = SuccessForegroundRgb,
                SuccessBackgroundRgb = SuccessBackgroundRgb,
                WarningScheme = WarningScheme,
                WarningDecoration = WarningDecoration,
                WarningForegroundRgb = WarningForegroundRgb,
                WarningBackgroundRgb = WarningBackgroundRgb,
                ErrorScheme = ErrorScheme,
                ErrorDecoration = ErrorDecoration,
                ErrorForegroundRgb = ErrorForegroundRgb,
                ErrorBackgroundRgb = ErrorBackgroundRgb
            };
        }

        internal ColorScheme ResolveLabelScheme()
        {
            return LabelScheme ?? RenderOptions.SubnoteScheme;
        }

        internal ColorScheme ResolveValueScheme(MetricKind kind)
        {
            switch (kind)
            {
                case MetricKind.Info:
                    return InfoScheme ?? RenderOptions.PromptScheme;
                case MetricKind.Success:
                    return SuccessScheme ?? Scheme(ConsoleColor.Green);
                case MetricKind.Warning:
                    return WarningScheme ?? Scheme(ConsoleColor.Yellow);
                case MetricKind.Error:
                    return ErrorScheme ?? RenderOptions.InvalidScheme;
                default:
                    return ValueScheme ?? RenderOptions.DefaultScheme;
            }
        }

        internal ColorScheme ResolveNoteScheme()
        {
            return NoteScheme ?? RenderOptions.SubnoteScheme;
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
