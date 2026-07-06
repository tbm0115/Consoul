using ConsoulLibrary.Color;
using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for a responsive breakdown chart.
    /// </summary>
    public class BreakdownChartOptions
    {
        /// <summary>
        /// Gets or sets the optional chart label rendered above the bar and legend.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the alignment used for the chart label.
        /// </summary>
        public TextBlockAlignment LabelAlignment { get; set; } = TextBlockAlignment.Left;

        /// <summary>
        /// Gets or sets the requested chart width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the stacked bar should render.
        /// </summary>
        public bool ShowBar { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether legend rows should render.
        /// </summary>
        public bool ShowLegend { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether values should render in legend rows.
        /// </summary>
        public bool ShowValues { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether percentages should render in legend rows.
        /// </summary>
        public bool ShowPercentages { get; set; } = true;

        /// <summary>
        /// Gets or sets the character used for populated chart segments.
        /// </summary>
        public char SegmentCharacter { get; set; } = '#';

        /// <summary>
        /// Gets or sets the character used when no positive segment value exists.
        /// </summary>
        public char EmptyCharacter { get; set; } = ' ';

        /// <summary>
        /// Gets or sets the formatter used for values rendered in legend rows.
        /// </summary>
        public Func<double, string> ValueFormatter { get; set; } = value => value.ToString("0.##");

        /// <summary>
        /// Gets or sets the formatter used for percentages rendered in legend rows.
        /// </summary>
        public Func<double, string> PercentageFormatter { get; set; } = value => value.ToString("0") + "%";

        /// <summary>
        /// Gets or sets the default color scheme used for chart segments.
        /// </summary>
        public ColorScheme? SegmentScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for legend labels.
        /// </summary>
        public ColorScheme? LabelScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for legend values.
        /// </summary>
        public ColorScheme? ValueScheme { get; set; }

        /// <summary>
        /// Creates a copy of the current breakdown chart options.
        /// </summary>
        /// <returns>A new <see cref="BreakdownChartOptions"/> instance with the same values.</returns>
        public BreakdownChartOptions Clone()
        {
            return new BreakdownChartOptions
            {
                Label = Label,
                LabelAlignment = LabelAlignment,
                Width = Width,
                ShowBar = ShowBar,
                ShowLegend = ShowLegend,
                ShowValues = ShowValues,
                ShowPercentages = ShowPercentages,
                SegmentCharacter = SegmentCharacter,
                EmptyCharacter = EmptyCharacter,
                ValueFormatter = ValueFormatter,
                PercentageFormatter = PercentageFormatter,
                SegmentScheme = SegmentScheme,
                LabelScheme = LabelScheme,
                ValueScheme = ValueScheme
            };
        }

        internal ColorScheme ResolveSegmentScheme()
        {
            return SegmentScheme ?? RenderOptions.OptionScheme;
        }

        internal ColorScheme ResolveLabelScheme()
        {
            return LabelScheme ?? RenderOptions.DefaultScheme;
        }

        internal ColorScheme ResolveValueScheme()
        {
            return ValueScheme ?? RenderOptions.SubnoteScheme;
        }

        internal string FormatValue(double value)
        {
            return (ValueFormatter ?? (current => current.ToString("0.##"))).Invoke(value) ?? string.Empty;
        }

        internal string FormatPercentage(double value)
        {
            return (PercentageFormatter ?? (current => current.ToString("0") + "%")).Invoke(value) ?? string.Empty;
        }
    }
}
