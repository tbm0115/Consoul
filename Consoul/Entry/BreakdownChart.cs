using System;
using System.Collections.Generic;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent breakdown chart construction over <see cref="BreakdownChartEntry"/>.
    /// </summary>
    public class BreakdownChart : BreakdownChartEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BreakdownChart"/> class.
        /// </summary>
        public BreakdownChart() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BreakdownChart"/> class with explicit options.
        /// </summary>
        /// <param name="options">Optional rendering options for the chart.</param>
        public BreakdownChart(BreakdownChartOptions options) : base(options)
        {
        }

        /// <summary>
        /// Adds an item to the chart.
        /// </summary>
        /// <param name="label">The item label.</param>
        /// <param name="value">The item value.</param>
        /// <param name="color">Optional foreground color used for the item segment.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart AddItem(string label, double value, ConsoleColor? color = null)
        {
            Add(label, value, color);
            return this;
        }

        /// <summary>
        /// Adds an item to the chart with a Spectre-style color facade.
        /// </summary>
        /// <param name="label">The item label.</param>
        /// <param name="value">The item value.</param>
        /// <param name="color">The foreground color used for the item segment.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart AddItem(string label, double value, SpectreColor color)
        {
            Add(BreakdownChartItem.Create(label, value, color));
            return this;
        }

        /// <summary>
        /// Adds an item to the chart with an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="label">The item label.</param>
        /// <param name="value">The item value.</param>
        /// <param name="color">The foreground color used for the item segment.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart AddItem(string label, double value, RgbColor color)
        {
            Add(BreakdownChartItem.Create(label, value, color));
            return this;
        }

        /// <summary>
        /// Adds an item to the chart with a segment color parsed from Spectre-style style text.
        /// </summary>
        /// <param name="label">The item label.</param>
        /// <param name="value">The item value.</param>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart AddItem(string label, double value, string style)
        {
            Add(BreakdownChartItem.Create(
                label,
                value,
                string.IsNullOrWhiteSpace(style) ? null : Style.Parse(style),
                Options.ResolveSegmentScheme()));
            return this;
        }

        /// <summary>
        /// Adds an item to the chart with a segment color resolved from a reusable style.
        /// </summary>
        /// <param name="label">The item label.</param>
        /// <param name="value">The item value.</param>
        /// <param name="style">Optional style used for the item segment.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart AddItem(string label, double value, Style style)
        {
            Add(BreakdownChartItem.Create(label, value, style, Options.ResolveSegmentScheme()));
            return this;
        }

        /// <summary>
        /// Adds an existing item to the chart.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart AddItem(BreakdownChartItem item)
        {
            Add(item);
            return this;
        }

        /// <summary>
        /// Adds existing items to the chart.
        /// </summary>
        /// <param name="items">The items to add.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart AddItems(IEnumerable<BreakdownChartItem> items)
        {
            if (items == null)
            {
                return this;
            }

            foreach (BreakdownChartItem item in items)
            {
                AddItem(item);
            }

            return this;
        }

        /// <summary>
        /// Adds items projected from a model collection.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into chart rows.</param>
        /// <param name="labelSelector">Selects the item label.</param>
        /// <param name="valueSelector">Selects the item value.</param>
        /// <param name="colorSelector">Optionally selects the foreground color used for each item segment.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart AddItems<T>(
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
        /// <param name="labelSelector">Selects the item label.</param>
        /// <param name="valueSelector">Selects the item value.</param>
        /// <param name="styleSelector">Selects the style used for each item segment.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart AddItems<T>(
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
        /// <param name="labelSelector">Selects the item label.</param>
        /// <param name="valueSelector">Selects the item value.</param>
        /// <param name="colorSelector">Selects the foreground color used for each item segment.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart AddItems<T>(
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
        /// <param name="labelSelector">Selects the item label.</param>
        /// <param name="valueSelector">Selects the item value.</param>
        /// <param name="colorSelector">Selects the foreground color used for each item segment.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart AddItems<T>(
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
        /// Adds items projected from a model collection with per-item segment colors parsed from Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into chart rows.</param>
        /// <param name="labelSelector">Selects the item label.</param>
        /// <param name="valueSelector">Selects the item value.</param>
        /// <param name="styleSelector">Selects style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart AddItems<T>(
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
        /// Sets the optional chart label rendered above the bar and legend.
        /// </summary>
        /// <param name="label">The chart label text.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart Label(string label)
        {
            Options.Label = label;
            return this;
        }

        /// <summary>
        /// Sets the requested chart width.
        /// </summary>
        /// <param name="width">The requested width. The actual width is clamped to the active terminal.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart Width(int? width)
        {
            Options.Width = width;
            return this;
        }

        /// <summary>
        /// Expands the chart to the active terminal width.
        /// </summary>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart Expand()
        {
            Options.Width = null;
            return this;
        }

        /// <summary>
        /// Left-aligns the chart label.
        /// </summary>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart LeftAlignLabel()
        {
            Options.LabelAlignment = TextBlockAlignment.Left;
            return this;
        }

        /// <summary>
        /// Centers the chart label.
        /// </summary>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart CenterLabel()
        {
            Options.LabelAlignment = TextBlockAlignment.Center;
            return this;
        }

        /// <summary>
        /// Right-aligns the chart label.
        /// </summary>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart RightAlignLabel()
        {
            Options.LabelAlignment = TextBlockAlignment.Right;
            return this;
        }

        /// <summary>
        /// Sets the chart label alignment.
        /// </summary>
        /// <param name="alignment">The label alignment.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart LabelAlignment(TextBlockAlignment alignment)
        {
            Options.LabelAlignment = alignment;
            return this;
        }

        /// <summary>
        /// Sets the chart label alignment using a Spectre-style justification alias.
        /// </summary>
        /// <param name="justify">The label justification.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart LabelAlignment(Justify justify)
        {
            Options.LabelAlignment = JustifyMap.ToTextBlockAlignment(justify);
            return this;
        }

        /// <summary>
        /// Sets whether the stacked bar should render.
        /// </summary>
        /// <param name="show">Whether the stacked bar should render.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart ShowBar(bool show = true)
        {
            Options.ShowBar = show;
            return this;
        }

        /// <summary>
        /// Hides the stacked bar.
        /// </summary>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart HideBar()
        {
            Options.ShowBar = false;
            return this;
        }

        /// <summary>
        /// Sets whether legend rows should render.
        /// </summary>
        /// <param name="show">Whether legend rows should render.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart ShowLegend(bool show = true)
        {
            Options.ShowLegend = show;
            return this;
        }

        /// <summary>
        /// Hides legend rows.
        /// </summary>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart HideLegend()
        {
            Options.ShowLegend = false;
            return this;
        }

        /// <summary>
        /// Sets whether values should render in legend rows.
        /// </summary>
        /// <param name="show">Whether values should render.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart ShowValues(bool show = true)
        {
            Options.ShowValues = show;
            return this;
        }

        /// <summary>
        /// Hides values in legend rows.
        /// </summary>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart HideValues()
        {
            Options.ShowValues = false;
            return this;
        }

        /// <summary>
        /// Sets whether percentages should render in legend rows.
        /// </summary>
        /// <param name="show">Whether percentages should render.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart ShowPercentages(bool show = true)
        {
            Options.ShowPercentages = show;
            return this;
        }

        /// <summary>
        /// Sets whether percentages should render in legend rows using the common singular alias.
        /// </summary>
        /// <param name="show">Whether percentages should render.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart ShowPercentage(bool show = true)
        {
            Options.ShowPercentages = show;
            return this;
        }

        /// <summary>
        /// Hides percentages in legend rows.
        /// </summary>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart HidePercentages()
        {
            Options.ShowPercentages = false;
            return this;
        }

        /// <summary>
        /// Hides percentages in legend rows using the common singular alias.
        /// </summary>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart HidePercentage()
        {
            Options.ShowPercentages = false;
            return this;
        }

        /// <summary>
        /// Sets the filled and empty characters used by the stacked bar.
        /// </summary>
        /// <param name="segment">The populated segment character.</param>
        /// <param name="empty">The empty segment character.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart SegmentCharacters(char segment, char empty = ' ')
        {
            Options.SegmentCharacter = segment;
            Options.EmptyCharacter = empty;
            return this;
        }

        /// <summary>
        /// Sets the formatter used for values rendered in legend rows.
        /// </summary>
        /// <param name="formatter">The value formatter.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart ValueFormatter(Func<double, string> formatter)
        {
            Options.ValueFormatter = formatter;
            return this;
        }

        /// <summary>
        /// Sets the formatter used for percentages rendered in legend rows.
        /// </summary>
        /// <param name="formatter">The percentage formatter.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart PercentageFormatter(Func<double, string> formatter)
        {
            Options.PercentageFormatter = formatter;
            return this;
        }

        /// <summary>
        /// Sets the default segment foreground color.
        /// </summary>
        /// <param name="color">The default segment foreground color.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart SegmentColor(ConsoleColor color)
        {
            Options.SegmentScheme = Scheme(color);
            Options.SegmentDecoration = Decoration.None;
            Options.SegmentForegroundRgb = null;
            Options.SegmentBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the default segment foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The default segment foreground color.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart SegmentColor(SpectreColor color)
        {
            Options.SegmentScheme = Scheme(color);
            Options.SegmentDecoration = Decoration.None;
            Options.SegmentForegroundRgb = color.ToRgbColor();
            Options.SegmentBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the default segment foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The default segment foreground color.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart SegmentColor(RgbColor color)
        {
            Options.SegmentScheme = Scheme(color);
            Options.SegmentDecoration = Decoration.None;
            Options.SegmentForegroundRgb = color;
            Options.SegmentBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the default segment color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart SegmentColor(Style style)
        {
            ApplySegmentStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the default segment color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart SegmentColor(string style)
        {
            ApplySegmentStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the legend label foreground color.
        /// </summary>
        /// <param name="color">The legend label foreground color.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart LabelColor(ConsoleColor color)
        {
            Options.LabelScheme = Scheme(color);
            Options.LabelDecoration = Decoration.None;
            Options.LabelForegroundRgb = null;
            Options.LabelBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the legend label foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The legend label foreground color.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart LabelColor(SpectreColor color)
        {
            Options.LabelScheme = Scheme(color);
            Options.LabelDecoration = Decoration.None;
            Options.LabelForegroundRgb = color.ToRgbColor();
            Options.LabelBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the legend label foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The legend label foreground color.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart LabelColor(RgbColor color)
        {
            Options.LabelScheme = Scheme(color);
            Options.LabelDecoration = Decoration.None;
            Options.LabelForegroundRgb = color;
            Options.LabelBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the legend label color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart LabelColor(Style style)
        {
            ApplyLabelStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the legend label color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart LabelColor(string style)
        {
            ApplyLabelStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the legend value foreground color.
        /// </summary>
        /// <param name="color">The legend value foreground color.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart ValueColor(ConsoleColor color)
        {
            Options.ValueScheme = Scheme(color);
            Options.ValueDecoration = Decoration.None;
            Options.ValueForegroundRgb = null;
            Options.ValueBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the legend value foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The legend value foreground color.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart ValueColor(SpectreColor color)
        {
            Options.ValueScheme = Scheme(color);
            Options.ValueDecoration = Decoration.None;
            Options.ValueForegroundRgb = color.ToRgbColor();
            Options.ValueBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the legend value foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The legend value foreground color.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart ValueColor(RgbColor color)
        {
            Options.ValueScheme = Scheme(color);
            Options.ValueDecoration = Decoration.None;
            Options.ValueForegroundRgb = color;
            Options.ValueBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the legend value color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart ValueColor(Style style)
        {
            ApplyValueStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the legend value color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>cyan</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="BreakdownChart"/> instance.</returns>
        public BreakdownChart ValueColor(string style)
        {
            ApplyValueStyle(Style.Parse(style));
            return this;
        }

        private void ApplySegmentStyle(Style style)
        {
            Options.SegmentScheme = Scheme(style, Options.ResolveSegmentScheme());
            Options.SegmentDecoration = style?.Decoration ?? Decoration.None;
            Options.SegmentForegroundRgb = style?.ForegroundRgb;
            Options.SegmentBackgroundRgb = style?.BackgroundRgb;
        }

        private void ApplyLabelStyle(Style style)
        {
            Options.LabelScheme = Scheme(style, Options.ResolveLabelScheme());
            Options.LabelDecoration = style?.Decoration ?? Decoration.None;
            Options.LabelForegroundRgb = style?.ForegroundRgb;
            Options.LabelBackgroundRgb = style?.BackgroundRgb;
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
