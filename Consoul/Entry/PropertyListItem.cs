using ConsoulLibrary.Color;
using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents one labeled value rendered by a <see cref="PropertyListEntry"/>.
    /// </summary>
    public class PropertyListItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyListItem"/> class.
        /// </summary>
        /// <param name="label">The label rendered before the value.</param>
        /// <param name="value">The value rendered for the label.</param>
        /// <param name="labelScheme">Optional color scheme used for the label.</param>
        /// <param name="valueScheme">Optional color scheme used for the value.</param>
        public PropertyListItem(string label, string value, ColorScheme? labelScheme = null, ColorScheme? valueScheme = null)
        {
            Label = label ?? string.Empty;
            Value = value ?? string.Empty;
            LabelScheme = labelScheme;
            ValueScheme = valueScheme;
        }

        /// <summary>
        /// Gets or sets the label rendered before the value.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the value rendered for the label.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the optional color scheme used for the label.
        /// </summary>
        public ColorScheme? LabelScheme { get; set; }

        /// <summary>
        /// Gets or sets the optional color scheme used for the value.
        /// </summary>
        public ColorScheme? ValueScheme { get; set; }

        /// <summary>
        /// Creates a property list item with optional foreground colors.
        /// </summary>
        /// <param name="label">The label rendered before the value.</param>
        /// <param name="value">The value rendered for the label.</param>
        /// <param name="valueColor">Optional foreground color used for the value.</param>
        /// <param name="labelColor">Optional foreground color used for the label.</param>
        /// <returns>A configured <see cref="PropertyListItem"/>.</returns>
        public static PropertyListItem Create(string label, object value, ConsoleColor? valueColor = null, ConsoleColor? labelColor = null)
        {
            ColorScheme? labelScheme = null;
            if (labelColor.HasValue)
            {
                labelScheme = new ColorScheme
                {
                    Color = labelColor.Value,
                    BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
                };
            }

            ColorScheme? valueScheme = null;
            if (valueColor.HasValue)
            {
                valueScheme = new ColorScheme
                {
                    Color = valueColor.Value,
                    BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
                };
            }

            return new PropertyListItem(label, value?.ToString() ?? string.Empty, labelScheme, valueScheme);
        }

        /// <summary>
        /// Creates a property list item with optional Spectre-style style text.
        /// </summary>
        /// <param name="label">The label rendered before the value.</param>
        /// <param name="value">The value rendered for the label.</param>
        /// <param name="valueStyle">Optional style used for the value, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="labelStyle">Optional style used for the label, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <returns>A configured <see cref="PropertyListItem"/>.</returns>
        public static PropertyListItem Create(string label, object value, string valueStyle, string labelStyle = null)
        {
            ColorScheme? labelScheme = string.IsNullOrWhiteSpace(labelStyle)
                ? (ColorScheme?)null
                : Style.Parse(labelStyle).ToColorScheme(RenderOptions.DefaultScheme);
            ColorScheme? valueScheme = string.IsNullOrWhiteSpace(valueStyle)
                ? (ColorScheme?)null
                : Style.Parse(valueStyle).ToColorScheme(RenderOptions.DefaultScheme);

            return new PropertyListItem(label, value?.ToString() ?? string.Empty, labelScheme, valueScheme);
        }

        /// <summary>
        /// Creates a property list item with optional reusable styles.
        /// </summary>
        /// <param name="label">The label rendered before the value.</param>
        /// <param name="value">The value rendered for the label.</param>
        /// <param name="valueStyle">Optional style used for the value.</param>
        /// <param name="labelStyle">Optional style used for the label.</param>
        /// <returns>A configured <see cref="PropertyListItem"/>.</returns>
        public static PropertyListItem Create(string label, object value, Style valueStyle, Style labelStyle = null)
        {
            ColorScheme? labelScheme = labelStyle == null
                ? (ColorScheme?)null
                : labelStyle.ToColorScheme(RenderOptions.DefaultScheme);
            ColorScheme? valueScheme = valueStyle == null
                ? (ColorScheme?)null
                : valueStyle.ToColorScheme(RenderOptions.DefaultScheme);

            return new PropertyListItem(label, value?.ToString() ?? string.Empty, labelScheme, valueScheme);
        }
    }
}
