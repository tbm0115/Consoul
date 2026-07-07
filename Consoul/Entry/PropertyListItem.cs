using ConsoulLibrary.Color;
using System;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

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
        /// Gets or sets optional ANSI decoration used for the label.
        /// </summary>
        public Decoration? LabelDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for the label on ANSI-capable terminals.
        /// </summary>
        public RgbColor? LabelForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for the label on ANSI-capable terminals.
        /// </summary>
        public RgbColor? LabelBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional color scheme used for the value.
        /// </summary>
        public ColorScheme? ValueScheme { get; set; }

        /// <summary>
        /// Gets or sets optional ANSI decoration used for the value.
        /// </summary>
        public Decoration? ValueDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for the value on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ValueForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for the value on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ValueBackgroundRgb { get; set; }

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
        /// Creates a property list item with optional Spectre-style foreground colors.
        /// </summary>
        /// <param name="label">The label rendered before the value.</param>
        /// <param name="value">The value rendered for the label.</param>
        /// <param name="valueColor">Foreground color used for the value.</param>
        /// <param name="labelColor">Optional foreground color used for the label.</param>
        /// <returns>A configured <see cref="PropertyListItem"/>.</returns>
        public static PropertyListItem Create(string label, object value, SpectreColor valueColor, SpectreColor? labelColor = null)
        {
            var item = Create(label, value, valueColor.ToRgbColor(), labelColor.HasValue ? labelColor.Value.ToRgbColor() : (RgbColor?)null);
            return item;
        }

        /// <summary>
        /// Creates a property list item with optional RGB foreground colors.
        /// </summary>
        /// <param name="label">The label rendered before the value.</param>
        /// <param name="value">The value rendered for the label.</param>
        /// <param name="valueColor">RGB foreground color used for the value.</param>
        /// <param name="labelColor">Optional RGB foreground color used for the label.</param>
        /// <returns>A configured <see cref="PropertyListItem"/>.</returns>
        public static PropertyListItem Create(string label, object value, RgbColor valueColor, RgbColor? labelColor = null)
        {
            return new PropertyListItem(
                label,
                value?.ToString() ?? string.Empty,
                labelColor.HasValue ? Scheme(labelColor.Value.ToConsoleColor()) : (ColorScheme?)null,
                Scheme(valueColor.ToConsoleColor()))
            {
                LabelForegroundRgb = labelColor,
                ValueForegroundRgb = valueColor
            };
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
            return Create(
                label,
                value,
                string.IsNullOrWhiteSpace(valueStyle) ? null : Style.Parse(valueStyle),
                string.IsNullOrWhiteSpace(labelStyle) ? null : Style.Parse(labelStyle));
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
            var item = new PropertyListItem(
                label,
                value?.ToString() ?? string.Empty,
                Scheme(labelStyle),
                Scheme(valueStyle));
            ApplyLabelStyle(item, labelStyle);
            ApplyValueStyle(item, valueStyle);
            return item;
        }

        private static void ApplyLabelStyle(PropertyListItem item, Style style)
        {
            if (item == null || style == null)
            {
                return;
            }

            item.LabelDecoration = style.Decoration;
            item.LabelForegroundRgb = style.ForegroundRgb;
            item.LabelBackgroundRgb = style.BackgroundRgb;
        }

        private static void ApplyValueStyle(PropertyListItem item, Style style)
        {
            if (item == null || style == null)
            {
                return;
            }

            item.ValueDecoration = style.Decoration;
            item.ValueForegroundRgb = style.ForegroundRgb;
            item.ValueBackgroundRgb = style.BackgroundRgb;
        }

        private static ColorScheme Scheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        private static ColorScheme? Scheme(Style style)
        {
            return style?.ToColorScheme(RenderOptions.DefaultScheme);
        }
    }
}
