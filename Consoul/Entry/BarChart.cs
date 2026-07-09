using System;
using System.Collections.Generic;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent bar chart construction over <see cref="BarChartEntry"/>.
    /// </summary>
    public class BarChart : BarChartEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BarChart"/> class.
        /// </summary>
        public BarChart() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BarChart"/> class with explicit options.
        /// </summary>
        /// <param name="options">Optional rendering options for the chart.</param>
        public BarChart(BarChartOptions options) : base(options)
        {
        }

        /// <summary>
        /// Adds an item to the chart.
        /// </summary>
        /// <param name="label">The label rendered before the bar.</param>
        /// <param name="value">The numeric value represented by the bar.</param>
        /// <param name="color">Optional foreground color used for the bar.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart AddItem(string label, double value, ConsoleColor? color = null)
        {
            Add(label, value, color);
            return this;
        }

        /// <summary>
        /// Adds an item to the chart with a Spectre-style color facade.
        /// </summary>
        /// <param name="label">The label rendered before the bar.</param>
        /// <param name="value">The numeric value represented by the bar.</param>
        /// <param name="color">The foreground color used for the bar.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart AddItem(string label, double value, SpectreColor color)
        {
            Add(BarChartItem.Create(label, value, color));
            return this;
        }

        /// <summary>
        /// Adds an item to the chart with an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="label">The label rendered before the bar.</param>
        /// <param name="value">The numeric value represented by the bar.</param>
        /// <param name="color">The foreground color used for the bar.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart AddItem(string label, double value, RgbColor color)
        {
            Add(BarChartItem.Create(label, value, color));
            return this;
        }

        /// <summary>
        /// Adds an item to the chart with a bar color parsed from Spectre-style style text.
        /// </summary>
        /// <param name="label">The label rendered before the bar.</param>
        /// <param name="value">The numeric value represented by the bar.</param>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart AddItem(string label, double value, string style)
        {
            Add(BarChartItem.Create(
                label,
                value,
                string.IsNullOrWhiteSpace(style) ? null : Style.Parse(style),
                Options.ResolveBarScheme()));
            return this;
        }

        /// <summary>
        /// Adds an item to the chart with a bar color resolved from a reusable style.
        /// </summary>
        /// <param name="label">The label rendered before the bar.</param>
        /// <param name="value">The numeric value represented by the bar.</param>
        /// <param name="style">Optional style used for the bar.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart AddItem(string label, double value, Style style)
        {
            Add(BarChartItem.Create(label, value, style, Options.ResolveBarScheme()));
            return this;
        }

        /// <summary>
        /// Adds an existing item to the chart.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart AddItem(BarChartItem item)
        {
            Add(item);
            return this;
        }

        /// <summary>
        /// Adds existing items to the chart.
        /// </summary>
        /// <param name="items">The items to add.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart AddItems(IEnumerable<BarChartItem> items)
        {
            if (items == null)
            {
                return this;
            }

            foreach (BarChartItem item in items)
            {
                AddItem(item);
            }

            return this;
        }

        /// <summary>
        /// Adds existing items to the chart.
        /// </summary>
        /// <param name="firstItem">The first item to add.</param>
        /// <param name="additionalItems">Additional items to add.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart AddItems(BarChartItem firstItem, params BarChartItem[] additionalItems)
        {
            AddItem(firstItem);

            if (additionalItems != null)
            {
                foreach (BarChartItem item in additionalItems)
                {
                    AddItem(item);
                }
            }

            return this;
        }

        /// <summary>
        /// Adds items projected from a model collection.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into chart rows.</param>
        /// <param name="labelSelector">Selects the label rendered before each bar.</param>
        /// <param name="valueSelector">Selects the numeric value represented by each bar.</param>
        /// <param name="colorSelector">Optionally selects the foreground color used for each bar.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart AddItems<T>(
            IEnumerable<T> items,
            Func<T, string> labelSelector,
            Func<T, double> valueSelector,
            Func<T, ConsoleColor?> colorSelector = null)
        {
            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

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
                AddItem(
                    labelSelector(item),
                    valueSelector(item),
                    colorSelector != null ? colorSelector(item) : null);
            }

            return this;
        }

        /// <summary>
        /// Adds items projected from a model collection with reusable per-item styles.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into chart rows.</param>
        /// <param name="labelSelector">Selects the label rendered before each bar.</param>
        /// <param name="valueSelector">Selects the numeric value represented by each bar.</param>
        /// <param name="styleSelector">Selects the style used for each bar.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart AddItems<T>(
            IEnumerable<T> items,
            Func<T, string> labelSelector,
            Func<T, double> valueSelector,
            Func<T, Style> styleSelector)
        {
            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }

            if (styleSelector == null)
            {
                throw new ArgumentNullException(nameof(styleSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddItem(labelSelector(item), valueSelector(item), styleSelector(item));
            }

            return this;
        }

        /// <summary>
        /// Adds items projected from a model collection with Spectre-style per-item colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into chart rows.</param>
        /// <param name="labelSelector">Selects the label rendered before each bar.</param>
        /// <param name="valueSelector">Selects the numeric value represented by each bar.</param>
        /// <param name="colorSelector">Selects the foreground color used for each bar.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart AddItems<T>(
            IEnumerable<T> items,
            Func<T, string> labelSelector,
            Func<T, double> valueSelector,
            Func<T, SpectreColor> colorSelector)
        {
            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }

            if (colorSelector == null)
            {
                throw new ArgumentNullException(nameof(colorSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddItem(labelSelector(item), valueSelector(item), colorSelector(item));
            }

            return this;
        }

        /// <summary>
        /// Adds items projected from a model collection with RGB per-item colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into chart rows.</param>
        /// <param name="labelSelector">Selects the label rendered before each bar.</param>
        /// <param name="valueSelector">Selects the numeric value represented by each bar.</param>
        /// <param name="colorSelector">Selects the foreground color used for each bar.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart AddItems<T>(
            IEnumerable<T> items,
            Func<T, string> labelSelector,
            Func<T, double> valueSelector,
            Func<T, RgbColor> colorSelector)
        {
            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }

            if (colorSelector == null)
            {
                throw new ArgumentNullException(nameof(colorSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddItem(labelSelector(item), valueSelector(item), colorSelector(item));
            }

            return this;
        }

        /// <summary>
        /// Adds items projected from a model collection with per-item colors parsed from Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into chart rows.</param>
        /// <param name="labelSelector">Selects the label rendered before each bar.</param>
        /// <param name="valueSelector">Selects the numeric value represented by each bar.</param>
        /// <param name="styleSelector">Selects style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart AddItems<T>(
            IEnumerable<T> items,
            Func<T, string> labelSelector,
            Func<T, double> valueSelector,
            Func<T, string> styleSelector)
        {
            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }

            if (styleSelector == null)
            {
                throw new ArgumentNullException(nameof(styleSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddItem(labelSelector(item), valueSelector(item), styleSelector(item));
            }

            return this;
        }

        /// <summary>
        /// Sets the optional chart label rendered above the bars.
        /// </summary>
        /// <param name="label">The chart label text.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart Label(string label)
        {
            Options.Label = label;
            return this;
        }

        /// <summary>
        /// Sets the requested chart width.
        /// </summary>
        /// <param name="width">The requested width. The actual width is clamped to the active terminal.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart Width(int? width)
        {
            Options.Width = width;
            return this;
        }

        /// <summary>
        /// Expands the chart to the active terminal width.
        /// </summary>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart Expand()
        {
            Options.Width = null;
            return this;
        }

        /// <summary>
        /// Sets the label column width.
        /// </summary>
        /// <param name="width">The requested label width.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart LabelWidth(int? width)
        {
            Options.LabelWidth = width;
            return this;
        }

        /// <summary>
        /// Left-aligns the chart label.
        /// </summary>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart LeftAlignLabel()
        {
            Options.LabelAlignment = TextBlockAlignment.Left;
            return this;
        }

        /// <summary>
        /// Centers the chart label.
        /// </summary>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart CenterLabel()
        {
            Options.LabelAlignment = TextBlockAlignment.Center;
            return this;
        }

        /// <summary>
        /// Right-aligns the chart label.
        /// </summary>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart RightAlignLabel()
        {
            Options.LabelAlignment = TextBlockAlignment.Right;
            return this;
        }

        /// <summary>
        /// Sets the chart label alignment.
        /// </summary>
        /// <param name="alignment">The label alignment.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart LabelAlignment(TextBlockAlignment alignment)
        {
            Options.LabelAlignment = alignment;
            return this;
        }

        /// <summary>
        /// Sets the chart label alignment using a Spectre-style justification alias.
        /// </summary>
        /// <param name="justify">The label justification.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart LabelAlignment(Justify justify)
        {
            Options.LabelAlignment = JustifyMap.ToTextBlockAlignment(justify);
            return this;
        }

        /// <summary>
        /// Sets the maximum value used to scale bars.
        /// </summary>
        /// <param name="value">The maximum value. When null, the largest item value is used.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart MaximumValue(double? value)
        {
            Options.MaximumValue = value;
            return this;
        }

        /// <summary>
        /// Sets the maximum value used to scale bars using a shorter Spectre-style alias.
        /// </summary>
        /// <param name="value">The maximum value. When null, the largest item value is used.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart MaxValue(double? value)
        {
            return MaximumValue(value);
        }

        /// <summary>
        /// Sets the maximum value used to scale bars using a chain-oriented Spectre-style alias.
        /// </summary>
        /// <param name="value">The maximum value. When null, the largest item value is used.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart WithMaxValue(double? value)
        {
            return MaximumValue(value);
        }

        /// <summary>
        /// Sets whether formatted values should render after bars.
        /// </summary>
        /// <param name="show">Whether formatted values should render.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart ShowValues(bool show = true)
        {
            Options.ShowValues = show;
            return this;
        }

        /// <summary>
        /// Sets whether formatted values should render after bars using the common singular alias.
        /// </summary>
        /// <param name="show">Whether formatted values should render.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart ShowValue(bool show = true)
        {
            return ShowValues(show);
        }

        /// <summary>
        /// Hides formatted values after bars.
        /// </summary>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart HideValues()
        {
            Options.ShowValues = false;
            return this;
        }

        /// <summary>
        /// Hides formatted values after bars using the common singular alias.
        /// </summary>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart HideValue()
        {
            return HideValues();
        }

        /// <summary>
        /// Sets the minimum bar width preserved before values are hidden or labels are shrunk.
        /// </summary>
        /// <param name="width">The minimum bar width.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart MinimumBarWidth(int width)
        {
            Options.MinimumBarWidth = width;
            return this;
        }

        /// <summary>
        /// Sets the filled and empty characters used by bars.
        /// </summary>
        /// <param name="filled">The filled bar character.</param>
        /// <param name="empty">The empty bar character.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart BarCharacters(char filled, char empty = ' ')
        {
            Options.BarCharacter = filled;
            Options.EmptyCharacter = empty;
            return this;
        }

        /// <summary>
        /// Sets the formatter used for values rendered after bars.
        /// </summary>
        /// <param name="formatter">The value formatter.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart ValueFormatter(Func<double, string> formatter)
        {
            Options.ValueFormatter = formatter;
            return this;
        }

        /// <summary>
        /// Sets the formatter used for values rendered after bars using a Spectre-style alias.
        /// </summary>
        /// <param name="formatter">The value formatter.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart UseValueFormatter(Func<double, string> formatter)
        {
            return ValueFormatter(formatter);
        }

        /// <summary>
        /// Sets the label foreground color.
        /// </summary>
        /// <param name="color">The label foreground color.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart LabelColor(ConsoleColor color)
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
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart LabelColor(SpectreColor color)
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
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart LabelColor(RgbColor color)
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
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart LabelColor(Style style)
        {
            ApplyLabelStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the label color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart LabelColor(string style)
        {
            ApplyLabelStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the default bar foreground color.
        /// </summary>
        /// <param name="color">The default bar foreground color.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart BarColor(ConsoleColor color)
        {
            Options.BarScheme = Scheme(color);
            Options.BarDecoration = Decoration.None;
            Options.BarForegroundRgb = null;
            Options.BarBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the default bar foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The default bar foreground color.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart BarColor(SpectreColor color)
        {
            Options.BarScheme = Scheme(color);
            Options.BarDecoration = Decoration.None;
            Options.BarForegroundRgb = color.ToRgbColor();
            Options.BarBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the default bar foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The default bar foreground color.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart BarColor(RgbColor color)
        {
            Options.BarScheme = Scheme(color);
            Options.BarDecoration = Decoration.None;
            Options.BarForegroundRgb = color;
            Options.BarBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the default bar color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart BarColor(Style style)
        {
            ApplyBarStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the default bar color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart BarColor(string style)
        {
            ApplyBarStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the value foreground color.
        /// </summary>
        /// <param name="color">The value foreground color.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart ValueColor(ConsoleColor color)
        {
            Options.ValueScheme = Scheme(color);
            Options.ValueDecoration = Decoration.None;
            Options.ValueForegroundRgb = null;
            Options.ValueBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the value foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The value foreground color.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart ValueColor(SpectreColor color)
        {
            Options.ValueScheme = Scheme(color);
            Options.ValueDecoration = Decoration.None;
            Options.ValueForegroundRgb = color.ToRgbColor();
            Options.ValueBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the value foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The value foreground color.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart ValueColor(RgbColor color)
        {
            Options.ValueScheme = Scheme(color);
            Options.ValueDecoration = Decoration.None;
            Options.ValueForegroundRgb = color;
            Options.ValueBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the value color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart ValueColor(Style style)
        {
            ApplyValueStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the value color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>cyan</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="BarChart"/> instance.</returns>
        public BarChart ValueColor(string style)
        {
            ApplyValueStyle(Style.Parse(style));
            return this;
        }

        private void ApplyLabelStyle(Style style)
        {
            Options.LabelScheme = Scheme(style, Options.ResolveLabelScheme());
            Options.LabelDecoration = style?.Decoration ?? Decoration.None;
            Options.LabelForegroundRgb = style?.ForegroundRgb;
            Options.LabelBackgroundRgb = style?.BackgroundRgb;
        }

        private void ApplyBarStyle(Style style)
        {
            Options.BarScheme = Scheme(style, Options.ResolveBarScheme());
            Options.BarDecoration = style?.Decoration ?? Decoration.None;
            Options.BarForegroundRgb = style?.ForegroundRgb;
            Options.BarBackgroundRgb = style?.BackgroundRgb;
        }

        private void ApplyValueStyle(Style style)
        {
            Options.ValueScheme = Scheme(style, Options.ResolveValueScheme());
            Options.ValueDecoration = style?.Decoration ?? Decoration.None;
            Options.ValueForegroundRgb = style?.ForegroundRgb;
            Options.ValueBackgroundRgb = style?.BackgroundRgb;
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
