using ConsoulLibrary.Color;
using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for a responsive sparkline chart.
    /// </summary>
    public class SparklineOptions
    {
        /// <summary>
        /// Gets or sets the requested chart width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets an optional label rendered before the sparkline.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the requested label width. The renderer may shrink it when the terminal is narrow.
        /// </summary>
        public int? LabelWidth { get; set; }

        /// <summary>
        /// Gets or sets the minimum value used to scale the sparkline. When null, the minimum data value is used.
        /// </summary>
        public double? MinimumValue { get; set; }

        /// <summary>
        /// Gets or sets the maximum value used to scale the sparkline. When null, the maximum data value is used.
        /// </summary>
        public double? MaximumValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the rendered range should appear after the sparkline.
        /// </summary>
        public bool ShowRange { get; set; }

        /// <summary>
        /// Gets or sets the characters used from low to high values.
        /// </summary>
        public string Characters { get; set; } = " .:-=+*#@";

        /// <summary>
        /// Gets or sets the formatter used for range values.
        /// </summary>
        public Func<double, string> ValueFormatter { get; set; } = value => value.ToString("0.##");

        /// <summary>
        /// Gets or sets the color scheme used for the label.
        /// </summary>
        public ColorScheme? LabelScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for the sparkline.
        /// </summary>
        public ColorScheme? LineScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for range values.
        /// </summary>
        public ColorScheme? RangeScheme { get; set; }

        /// <summary>
        /// Creates a copy of the current sparkline options.
        /// </summary>
        /// <returns>A new <see cref="SparklineOptions"/> instance with the same values.</returns>
        public SparklineOptions Clone()
        {
            return new SparklineOptions
            {
                Width = Width,
                Label = Label,
                LabelWidth = LabelWidth,
                MinimumValue = MinimumValue,
                MaximumValue = MaximumValue,
                ShowRange = ShowRange,
                Characters = Characters,
                ValueFormatter = ValueFormatter,
                LabelScheme = LabelScheme,
                LineScheme = LineScheme,
                RangeScheme = RangeScheme
            };
        }

        internal string ResolveCharacters()
        {
            return string.IsNullOrEmpty(Characters) ? " .:-=+*#@" : Characters;
        }

        internal ColorScheme ResolveLabelScheme()
        {
            return LabelScheme ?? RenderOptions.SubnoteScheme;
        }

        internal ColorScheme ResolveLineScheme()
        {
            return LineScheme ?? RenderOptions.OptionScheme;
        }

        internal ColorScheme ResolveRangeScheme()
        {
            return RangeScheme ?? RenderOptions.DefaultScheme;
        }

        internal string FormatValue(double value)
        {
            return (ValueFormatter ?? (current => current.ToString("0.##"))).Invoke(value) ?? string.Empty;
        }
    }
}
