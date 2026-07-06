using ConsoulLibrary.Color;
using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents one compact metric rendered by a metrics view helper.
    /// </summary>
    public class MetricItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetricItem"/> class.
        /// </summary>
        public MetricItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricItem"/> class.
        /// </summary>
        /// <param name="label">The metric label.</param>
        /// <param name="value">The primary metric value.</param>
        /// <param name="unit">Optional unit text rendered next to the value.</param>
        /// <param name="note">Optional supporting note rendered below the value.</param>
        /// <param name="kind">The visual intent of the metric value.</param>
        public MetricItem(string label, object value, string unit = null, string note = null, MetricKind kind = MetricKind.Default)
        {
            Label = label ?? string.Empty;
            Value = value?.ToString() ?? string.Empty;
            Unit = unit;
            Note = note;
            Kind = kind;
        }

        /// <summary>
        /// Gets or sets the metric label.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the primary metric value.
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets optional unit text rendered next to the value.
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Gets or sets optional supporting note rendered below the value.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the visual intent of the metric value.
        /// </summary>
        public MetricKind Kind { get; set; }

        /// <summary>
        /// Gets or sets an optional color scheme for the label.
        /// </summary>
        public ColorScheme? LabelScheme { get; set; }

        /// <summary>
        /// Gets or sets an optional color scheme for the value.
        /// </summary>
        public ColorScheme? ValueScheme { get; set; }

        /// <summary>
        /// Gets or sets an optional color scheme for the note.
        /// </summary>
        public ColorScheme? NoteScheme { get; set; }

        /// <summary>
        /// Creates a metric item.
        /// </summary>
        /// <param name="label">The metric label.</param>
        /// <param name="value">The primary metric value.</param>
        /// <param name="unit">Optional unit text rendered next to the value.</param>
        /// <param name="note">Optional supporting note rendered below the value.</param>
        /// <param name="kind">The visual intent of the metric value.</param>
        /// <param name="labelColor">Optional foreground color used for the label.</param>
        /// <param name="valueColor">Optional foreground color used for the value.</param>
        /// <param name="noteColor">Optional foreground color used for the note.</param>
        /// <returns>A configured <see cref="MetricItem"/> instance.</returns>
        public static MetricItem Create(
            string label,
            object value,
            string unit = null,
            string note = null,
            MetricKind kind = MetricKind.Default,
            ConsoleColor? labelColor = null,
            ConsoleColor? valueColor = null,
            ConsoleColor? noteColor = null)
        {
            return new MetricItem(label, value, unit, note, kind)
            {
                LabelScheme = labelColor.HasValue ? Scheme(labelColor.Value) : (ColorScheme?)null,
                ValueScheme = valueColor.HasValue ? Scheme(valueColor.Value) : (ColorScheme?)null,
                NoteScheme = noteColor.HasValue ? Scheme(noteColor.Value) : (ColorScheme?)null
            };
        }

        /// <summary>
        /// Creates a metric item using reusable style objects.
        /// </summary>
        /// <param name="label">The metric label.</param>
        /// <param name="value">The primary metric value.</param>
        /// <param name="labelStyle">Optional style used for the label.</param>
        /// <param name="valueStyle">Optional style used for the value.</param>
        /// <param name="noteStyle">Optional style used for the note.</param>
        /// <param name="unit">Optional unit text rendered next to the value.</param>
        /// <param name="note">Optional supporting note rendered below the value.</param>
        /// <param name="kind">The visual intent of the metric value.</param>
        /// <returns>A configured <see cref="MetricItem"/> instance.</returns>
        public static MetricItem Create(
            string label,
            object value,
            Style labelStyle,
            Style valueStyle = null,
            Style noteStyle = null,
            string unit = null,
            string note = null,
            MetricKind kind = MetricKind.Default)
        {
            return new MetricItem(label, value, unit, note, kind)
            {
                LabelScheme = Scheme(labelStyle),
                ValueScheme = Scheme(valueStyle),
                NoteScheme = Scheme(noteStyle)
            };
        }

        /// <summary>
        /// Creates a metric item using Spectre-style style text.
        /// </summary>
        /// <param name="label">The metric label.</param>
        /// <param name="value">The primary metric value.</param>
        /// <param name="labelStyle">Optional style used for the label, such as <c>grey</c>.</param>
        /// <param name="valueStyle">Optional style used for the value, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="noteStyle">Optional style used for the note, such as <c>darkgray</c>.</param>
        /// <param name="unit">Optional unit text rendered next to the value.</param>
        /// <param name="note">Optional supporting note rendered below the value.</param>
        /// <param name="kind">The visual intent of the metric value.</param>
        /// <returns>A configured <see cref="MetricItem"/> instance.</returns>
        public static MetricItem CreateStyled(
            string label,
            object value,
            string labelStyle = null,
            string valueStyle = null,
            string noteStyle = null,
            string unit = null,
            string note = null,
            MetricKind kind = MetricKind.Default)
        {
            return new MetricItem(label, value, unit, note, kind)
            {
                LabelScheme = string.IsNullOrWhiteSpace(labelStyle) ? (ColorScheme?)null : Style.Parse(labelStyle).ToColorScheme(RenderOptions.DefaultScheme),
                ValueScheme = string.IsNullOrWhiteSpace(valueStyle) ? (ColorScheme?)null : Style.Parse(valueStyle).ToColorScheme(RenderOptions.DefaultScheme),
                NoteScheme = string.IsNullOrWhiteSpace(noteStyle) ? (ColorScheme?)null : Style.Parse(noteStyle).ToColorScheme(RenderOptions.DefaultScheme)
            };
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
