using ConsoulLibrary.Color;
using System;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents one item rendered by <see cref="BarChartEntry"/>.
    /// </summary>
    public class BarChartItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BarChartItem"/> class.
        /// </summary>
        /// <param name="label">The label rendered before the bar.</param>
        /// <param name="value">The numeric value represented by the bar.</param>
        /// <param name="scheme">Optional color scheme used for the bar.</param>
        public BarChartItem(string label, double value, ColorScheme? scheme = null)
        {
            Label = label ?? string.Empty;
            Value = value;
            Scheme = scheme;
        }

        /// <summary>
        /// Gets or sets the label rendered before the bar.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the numeric value represented by the bar.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets the optional color scheme used for the bar.
        /// </summary>
        public ColorScheme? Scheme { get; set; }

        /// <summary>
        /// Creates a bar chart item with an optional foreground color.
        /// </summary>
        /// <param name="label">The label rendered before the bar.</param>
        /// <param name="value">The numeric value represented by the bar.</param>
        /// <param name="color">Optional foreground color used for the bar.</param>
        /// <returns>A configured <see cref="BarChartItem"/>.</returns>
        public static BarChartItem Create(string label, double value, ConsoleColor? color = null)
        {
            ColorScheme? scheme = null;
            if (color.HasValue)
            {
                scheme = new ColorScheme
                {
                    Color = color.Value,
                    BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
                };
            }

            return new BarChartItem(label, value, scheme);
        }

        /// <summary>
        /// Creates a bar chart item with a Spectre-style color facade.
        /// </summary>
        /// <param name="label">The label rendered before the bar.</param>
        /// <param name="value">The numeric value represented by the bar.</param>
        /// <param name="color">The foreground color used for the bar.</param>
        /// <returns>A configured <see cref="BarChartItem"/>.</returns>
        public static BarChartItem Create(string label, double value, SpectreColor color)
        {
            return Create(label, value, (ConsoleColor?)color.ToConsoleColor());
        }

        /// <summary>
        /// Creates a bar chart item with an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="label">The label rendered before the bar.</param>
        /// <param name="value">The numeric value represented by the bar.</param>
        /// <param name="color">The foreground color used for the bar.</param>
        /// <returns>A configured <see cref="BarChartItem"/>.</returns>
        public static BarChartItem Create(string label, double value, RgbColor color)
        {
            return Create(label, value, (ConsoleColor?)color.ToConsoleColor());
        }

        /// <summary>
        /// Creates a bar chart item with Spectre-style style text.
        /// </summary>
        /// <param name="label">The label rendered before the bar.</param>
        /// <param name="value">The numeric value represented by the bar.</param>
        /// <param name="style">Optional style used for the bar, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <returns>A configured <see cref="BarChartItem"/>.</returns>
        public static BarChartItem Create(string label, double value, string style)
        {
            ColorScheme? scheme = string.IsNullOrWhiteSpace(style)
                ? (ColorScheme?)null
                : Style.Parse(style).ToColorScheme(RenderOptions.DefaultScheme);

            return new BarChartItem(label, value, scheme);
        }

        /// <summary>
        /// Creates a bar chart item with a reusable style.
        /// </summary>
        /// <param name="label">The label rendered before the bar.</param>
        /// <param name="value">The numeric value represented by the bar.</param>
        /// <param name="style">Optional style used for the bar.</param>
        /// <returns>A configured <see cref="BarChartItem"/>.</returns>
        public static BarChartItem Create(string label, double value, Style style)
        {
            return new BarChartItem(label, value, style?.ToColorScheme(RenderOptions.DefaultScheme));
        }
    }
}
