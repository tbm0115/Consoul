using ConsoulLibrary.Color;
using System;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

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
        /// Gets or sets an optional color scheme for the value.
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
        /// Gets or sets an optional color scheme for the note.
        /// </summary>
        public ColorScheme? NoteScheme { get; set; }

        /// <summary>
        /// Gets or sets optional ANSI decoration used for the note.
        /// </summary>
        public Decoration? NoteDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for the note on ANSI-capable terminals.
        /// </summary>
        public RgbColor? NoteForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for the note on ANSI-capable terminals.
        /// </summary>
        public RgbColor? NoteBackgroundRgb { get; set; }

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
        /// Creates a metric item using Spectre-style color facades.
        /// </summary>
        /// <param name="label">The metric label.</param>
        /// <param name="value">The primary metric value.</param>
        /// <param name="labelColor">Foreground color used for the label.</param>
        /// <param name="valueColor">Optional foreground color used for the value.</param>
        /// <param name="noteColor">Optional foreground color used for the note.</param>
        /// <param name="unit">Optional unit text rendered next to the value.</param>
        /// <param name="note">Optional supporting note rendered below the value.</param>
        /// <param name="kind">The visual intent of the metric value.</param>
        /// <returns>A configured <see cref="MetricItem"/> instance.</returns>
        public static MetricItem Create(
            string label,
            object value,
            SpectreColor labelColor,
            SpectreColor? valueColor = null,
            SpectreColor? noteColor = null,
            string unit = null,
            string note = null,
            MetricKind kind = MetricKind.Default)
        {
            return Create(
                label,
                value,
                labelColor.ToRgbColor(),
                valueColor.HasValue ? valueColor.Value.ToRgbColor() : (RgbColor?)null,
                noteColor.HasValue ? noteColor.Value.ToRgbColor() : (RgbColor?)null,
                unit,
                note,
                kind);
        }

        /// <summary>
        /// Creates a metric item using RGB foreground colors.
        /// </summary>
        /// <param name="label">The metric label.</param>
        /// <param name="value">The primary metric value.</param>
        /// <param name="labelColor">Foreground color used for the label.</param>
        /// <param name="valueColor">Optional foreground color used for the value.</param>
        /// <param name="noteColor">Optional foreground color used for the note.</param>
        /// <param name="unit">Optional unit text rendered next to the value.</param>
        /// <param name="note">Optional supporting note rendered below the value.</param>
        /// <param name="kind">The visual intent of the metric value.</param>
        /// <returns>A configured <see cref="MetricItem"/> instance.</returns>
        public static MetricItem Create(
            string label,
            object value,
            RgbColor labelColor,
            RgbColor? valueColor = null,
            RgbColor? noteColor = null,
            string unit = null,
            string note = null,
            MetricKind kind = MetricKind.Default)
        {
            return new MetricItem(label, value, unit, note, kind)
            {
                LabelScheme = Scheme(labelColor.ToConsoleColor()),
                ValueScheme = valueColor.HasValue ? Scheme(valueColor.Value.ToConsoleColor()) : (ColorScheme?)null,
                NoteScheme = noteColor.HasValue ? Scheme(noteColor.Value.ToConsoleColor()) : (ColorScheme?)null,
                LabelForegroundRgb = labelColor,
                ValueForegroundRgb = valueColor,
                NoteForegroundRgb = noteColor
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
            var item = new MetricItem(label, value, unit, note, kind)
            {
                LabelScheme = Scheme(labelStyle),
                ValueScheme = Scheme(valueStyle),
                NoteScheme = Scheme(noteStyle)
            };
            ApplyLabelStyle(item, labelStyle);
            ApplyValueStyle(item, valueStyle);
            ApplyNoteStyle(item, noteStyle);
            return item;
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
            return Create(
                label,
                value,
                string.IsNullOrWhiteSpace(labelStyle) ? null : Style.Parse(labelStyle),
                string.IsNullOrWhiteSpace(valueStyle) ? null : Style.Parse(valueStyle),
                string.IsNullOrWhiteSpace(noteStyle) ? null : Style.Parse(noteStyle),
                unit,
                note,
                kind);
        }

        private static void ApplyLabelStyle(MetricItem item, Style style)
        {
            if (item == null || style == null)
            {
                return;
            }

            item.LabelDecoration = style.Decoration;
            item.LabelForegroundRgb = style.ForegroundRgb;
            item.LabelBackgroundRgb = style.BackgroundRgb;
        }

        private static void ApplyValueStyle(MetricItem item, Style style)
        {
            if (item == null || style == null)
            {
                return;
            }

            item.ValueDecoration = style.Decoration;
            item.ValueForegroundRgb = style.ForegroundRgb;
            item.ValueBackgroundRgb = style.BackgroundRgb;
        }

        private static void ApplyNoteStyle(MetricItem item, Style style)
        {
            if (item == null || style == null)
            {
                return;
            }

            item.NoteDecoration = style.Decoration;
            item.NoteForegroundRgb = style.ForegroundRgb;
            item.NoteBackgroundRgb = style.BackgroundRgb;
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
