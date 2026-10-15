using ConsoulLibrary.Color;
using System;
using RgbColor = ConsoulLibrary.Color.RgbColor;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for a responsive bar chart.
    /// </summary>
    public class BarChartOptions
    {
        /// <summary>
        /// Gets or sets the optional chart label rendered above the bars.
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
        /// Gets or sets a value indicating whether the chart should expand to the active terminal width when no explicit <see cref="Width"/> is set.
        /// </summary>
        public bool Expand { get; set; } = true;

        /// <summary>
        /// Gets or sets the requested label column width. The renderer may shrink it when the terminal is narrow.
        /// </summary>
        public int? LabelWidth { get; set; }

        /// <summary>
        /// Gets or sets the maximum value used to scale bars. When null, the largest item value is used.
        /// </summary>
        public double? MaximumValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether formatted values should render after bars.
        /// </summary>
        public bool ShowValues { get; set; } = true;

        /// <summary>
        /// Gets or sets the minimum bar width preserved before values are hidden or labels are shrunk.
        /// </summary>
        public int MinimumBarWidth { get; set; } = 4;

        /// <summary>
        /// Gets or sets the character used for filled bar units.
        /// </summary>
        public char BarCharacter { get; set; } = '#';

        /// <summary>
        /// Gets or sets the character used for empty bar units.
        /// </summary>
        public char EmptyCharacter { get; set; } = ' ';

        /// <summary>
        /// Gets or sets the formatter used for values rendered after bars.
        /// </summary>
        public Func<double, string> ValueFormatter { get; set; } = value => value.ToString("0.##");

        /// <summary>
        /// Gets or sets the color scheme used for labels.
        /// </summary>
        public ColorScheme? LabelScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for labels.
        /// </summary>
        public Decoration LabelDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for labels on ANSI-capable terminals.
        /// </summary>
        public RgbColor? LabelForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for labels on ANSI-capable terminals.
        /// </summary>
        public RgbColor? LabelBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the default color scheme used for bars.
        /// </summary>
        public ColorScheme? BarScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for bars.
        /// </summary>
        public Decoration BarDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for bars on ANSI-capable terminals.
        /// </summary>
        public RgbColor? BarForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for bars on ANSI-capable terminals.
        /// </summary>
        public RgbColor? BarBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for values.
        /// </summary>
        public ColorScheme? ValueScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for values.
        /// </summary>
        public Decoration ValueDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for values on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ValueForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for values on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ValueBackgroundRgb { get; set; }

        /// <summary>
        /// Creates a copy of the current bar chart options.
        /// </summary>
        /// <returns>A new <see cref="BarChartOptions"/> instance with the same values.</returns>
        public BarChartOptions Clone()
        {
            return new BarChartOptions
            {
                Label = Label,
                LabelAlignment = LabelAlignment,
                Width = Width,
                Expand = Expand,
                LabelWidth = LabelWidth,
                MaximumValue = MaximumValue,
                ShowValues = ShowValues,
                MinimumBarWidth = MinimumBarWidth,
                BarCharacter = BarCharacter,
                EmptyCharacter = EmptyCharacter,
                ValueFormatter = ValueFormatter,
                LabelScheme = LabelScheme,
                LabelDecoration = LabelDecoration,
                LabelForegroundRgb = LabelForegroundRgb,
                LabelBackgroundRgb = LabelBackgroundRgb,
                BarScheme = BarScheme,
                BarDecoration = BarDecoration,
                BarForegroundRgb = BarForegroundRgb,
                BarBackgroundRgb = BarBackgroundRgb,
                ValueScheme = ValueScheme,
                ValueDecoration = ValueDecoration,
                ValueForegroundRgb = ValueForegroundRgb,
                ValueBackgroundRgb = ValueBackgroundRgb
            };
        }

        internal int ResolveMinimumBarWidth()
        {
            return Math.Max(1, MinimumBarWidth);
        }

        internal ColorScheme ResolveLabelScheme()
        {
            return LabelScheme ?? RenderOptions.SubnoteScheme;
        }

        internal ColorScheme ResolveBarScheme()
        {
            return BarScheme ?? RenderOptions.OptionScheme;
        }

        internal ColorScheme ResolveValueScheme()
        {
            return ValueScheme ?? RenderOptions.DefaultScheme;
        }

        internal string FormatValue(double value)
        {
            return (ValueFormatter ?? (current => current.ToString("0.##"))).Invoke(value) ?? string.Empty;
        }
    }
}
