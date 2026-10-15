using System;
using System.Collections.Generic;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent sparkline construction over <see cref="SparklineEntry"/>.
    /// </summary>
    public class Sparkline : SparklineEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sparkline"/> class.
        /// </summary>
        public Sparkline()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sparkline"/> class with explicit options.
        /// </summary>
        /// <param name="options">Optional rendering options for the sparkline.</param>
        public Sparkline(SparklineOptions options) : base(options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sparkline"/> class with values.
        /// </summary>
        /// <param name="values">The values to render.</param>
        /// <param name="options">Optional rendering options for the sparkline.</param>
        public Sparkline(IEnumerable<double> values, SparklineOptions options = null) : base(values, options)
        {
        }

        /// <summary>
        /// Adds a sparkline value.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline AddValue(double value)
        {
            Add(value);
            return this;
        }

        /// <summary>
        /// Adds sparkline values.
        /// </summary>
        /// <param name="values">The values to add.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline AddValues(IEnumerable<double> values)
        {
            AddRange(values);
            return this;
        }

        /// <summary>
        /// Adds sparkline values.
        /// </summary>
        /// <param name="values">The values to add.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline AddValues(params double[] values)
        {
            return AddValues((IEnumerable<double>)values);
        }

        /// <summary>
        /// Adds sparkline values projected from a model collection.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into sparkline values.</param>
        /// <param name="valueSelector">Selects the numeric value rendered by the sparkline.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline AddValues<T>(IEnumerable<T> items, Func<T, double> valueSelector)
        {
            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddValue(valueSelector(item));
            }

            return this;
        }

        /// <summary>
        /// Sets the requested sparkline width.
        /// </summary>
        /// <param name="width">The requested width. The actual width is clamped to the active terminal.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline Width(int? width)
        {
            Options.Width = width;
            return this;
        }

        /// <summary>
        /// Expands the sparkline to the active terminal width.
        /// </summary>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline Expand()
        {
            Options.Width = null;
            return this;
        }

        /// <summary>
        /// Sets the optional label rendered before the sparkline.
        /// </summary>
        /// <param name="label">The label text.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline Label(string label)
        {
            Options.Label = label;
            return this;
        }

        /// <summary>
        /// Sets the requested label width.
        /// </summary>
        /// <param name="width">The requested label width.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline LabelWidth(int? width)
        {
            Options.LabelWidth = width;
            return this;
        }

        /// <summary>
        /// Sets the minimum value used to scale the sparkline.
        /// </summary>
        /// <param name="value">The minimum value. When null, the minimum data value is used.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline MinimumValue(double? value)
        {
            Options.MinimumValue = value;
            return this;
        }

        /// <summary>
        /// Sets the minimum value used to scale the sparkline using a shorter Spectre-style alias.
        /// </summary>
        /// <param name="value">The minimum value. When null, the minimum data value is used.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline MinValue(double? value)
        {
            return MinimumValue(value);
        }

        /// <summary>
        /// Sets the minimum value used to scale the sparkline using a chain-oriented Spectre-style alias.
        /// </summary>
        /// <param name="value">The minimum value. When null, the minimum data value is used.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline WithMinValue(double? value)
        {
            return MinimumValue(value);
        }

        /// <summary>
        /// Sets the maximum value used to scale the sparkline.
        /// </summary>
        /// <param name="value">The maximum value. When null, the maximum data value is used.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline MaximumValue(double? value)
        {
            Options.MaximumValue = value;
            return this;
        }

        /// <summary>
        /// Sets the maximum value used to scale the sparkline using a shorter Spectre-style alias.
        /// </summary>
        /// <param name="value">The maximum value. When null, the maximum data value is used.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline MaxValue(double? value)
        {
            return MaximumValue(value);
        }

        /// <summary>
        /// Sets the maximum value used to scale the sparkline using a chain-oriented Spectre-style alias.
        /// </summary>
        /// <param name="value">The maximum value. When null, the maximum data value is used.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline WithMaxValue(double? value)
        {
            return MaximumValue(value);
        }

        /// <summary>
        /// Sets both explicit range values used to scale the sparkline.
        /// </summary>
        /// <param name="minimum">The minimum value. When null, the minimum data value is used.</param>
        /// <param name="maximum">The maximum value. When null, the maximum data value is used.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline Range(double? minimum, double? maximum)
        {
            Options.MinimumValue = minimum;
            Options.MaximumValue = maximum;
            return this;
        }

        /// <summary>
        /// Shows or hides the formatted range after the sparkline.
        /// </summary>
        /// <param name="show">Whether the range should render.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline ShowRange(bool show = true)
        {
            Options.ShowRange = show;
            return this;
        }

        /// <summary>
        /// Hides the formatted range after the sparkline.
        /// </summary>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline HideRange()
        {
            Options.ShowRange = false;
            return this;
        }

        /// <summary>
        /// Sets the characters used from low to high values.
        /// </summary>
        /// <param name="characters">The sparkline character ramp.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline Characters(string characters)
        {
            Options.Characters = characters;
            return this;
        }

        /// <summary>
        /// Sets the formatter used for range values.
        /// </summary>
        /// <param name="formatter">The value formatter.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline ValueFormatter(Func<double, string> formatter)
        {
            Options.ValueFormatter = formatter;
            return this;
        }

        /// <summary>
        /// Sets the formatter used for range values using a Spectre-style alias.
        /// </summary>
        /// <param name="formatter">The value formatter.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline UseValueFormatter(Func<double, string> formatter)
        {
            return ValueFormatter(formatter);
        }

        /// <summary>
        /// Sets the label foreground color.
        /// </summary>
        /// <param name="color">The label foreground color.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline LabelColor(ConsoleColor color)
        {
            Options.LabelScheme = Scheme(color);
            Options.LabelDecoration = Decoration.None;
            Options.LabelForegroundRgb = null;
            Options.LabelBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the label foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The label foreground color.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline LabelColor(SpectreColor color)
        {
            Options.LabelScheme = Scheme(color);
            Options.LabelDecoration = Decoration.None;
            Options.LabelForegroundRgb = color.ToRgbColor();
            Options.LabelBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the label foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The label foreground color.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline LabelColor(RgbColor color)
        {
            Options.LabelScheme = Scheme(color);
            Options.LabelDecoration = Decoration.None;
            Options.LabelForegroundRgb = color;
            Options.LabelBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the label color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline LabelColor(Style style)
        {
            ApplyLabelStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the label color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline LabelColor(string style)
        {
            ApplyLabelStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the sparkline foreground color.
        /// </summary>
        /// <param name="color">The sparkline foreground color.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline LineColor(ConsoleColor color)
        {
            Options.LineScheme = Scheme(color);
            Options.LineDecoration = Decoration.None;
            Options.LineForegroundRgb = null;
            Options.LineBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the sparkline foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The sparkline foreground color.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline LineColor(SpectreColor color)
        {
            Options.LineScheme = Scheme(color);
            Options.LineDecoration = Decoration.None;
            Options.LineForegroundRgb = color.ToRgbColor();
            Options.LineBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the sparkline foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The sparkline foreground color.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline LineColor(RgbColor color)
        {
            Options.LineScheme = Scheme(color);
            Options.LineDecoration = Decoration.None;
            Options.LineForegroundRgb = color;
            Options.LineBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the sparkline color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline LineColor(Style style)
        {
            ApplyLineStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the sparkline color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline LineColor(string style)
        {
            ApplyLineStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the range foreground color.
        /// </summary>
        /// <param name="color">The range foreground color.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline RangeColor(ConsoleColor color)
        {
            Options.RangeScheme = Scheme(color);
            Options.RangeDecoration = Decoration.None;
            Options.RangeForegroundRgb = null;
            Options.RangeBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the range foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The range foreground color.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline RangeColor(SpectreColor color)
        {
            Options.RangeScheme = Scheme(color);
            Options.RangeDecoration = Decoration.None;
            Options.RangeForegroundRgb = color.ToRgbColor();
            Options.RangeBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the range foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The range foreground color.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline RangeColor(RgbColor color)
        {
            Options.RangeScheme = Scheme(color);
            Options.RangeDecoration = Decoration.None;
            Options.RangeForegroundRgb = color;
            Options.RangeBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the range color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline RangeColor(Style style)
        {
            ApplyRangeStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the range color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>cyan</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Sparkline"/> instance.</returns>
        public Sparkline RangeColor(string style)
        {
            ApplyRangeStyle(Style.Parse(style));
            return this;
        }

        private void ApplyLabelStyle(Style style)
        {
            Options.LabelScheme = Scheme(style, Options.ResolveLabelScheme());
            Options.LabelDecoration = style?.Decoration ?? Decoration.None;
            Options.LabelForegroundRgb = style?.ForegroundRgb;
            Options.LabelBackgroundRgb = style?.BackgroundRgb;
        }

        private void ApplyLineStyle(Style style)
        {
            Options.LineScheme = Scheme(style, Options.ResolveLineScheme());
            Options.LineDecoration = style?.Decoration ?? Decoration.None;
            Options.LineForegroundRgb = style?.ForegroundRgb;
            Options.LineBackgroundRgb = style?.BackgroundRgb;
        }

        private void ApplyRangeStyle(Style style)
        {
            Options.RangeScheme = Scheme(style, Options.ResolveRangeScheme());
            Options.RangeDecoration = style?.Decoration ?? Decoration.None;
            Options.RangeForegroundRgb = style?.ForegroundRgb;
            Options.RangeBackgroundRgb = style?.BackgroundRgb;
        }

        private static ColorScheme Scheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        private static ColorScheme Scheme(SpectreColor color)
        {
            return Scheme(color.ToConsoleColor());
        }

        private static ColorScheme Scheme(RgbColor color)
        {
            return Scheme(color.ToConsoleColor());
        }

        private static ColorScheme Scheme(Style style, ColorScheme fallback)
        {
            return (style ?? Style.Plain).ToColorScheme(fallback);
        }
    }
}
