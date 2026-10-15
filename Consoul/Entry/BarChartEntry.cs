using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders a responsive horizontal bar chart for console view content.
    /// </summary>
    public class BarChartEntry : IRenderable
    {
        private readonly List<BarChartItem> _items = new List<BarChartItem>();
        private readonly BarChartOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="BarChartEntry"/> class.
        /// </summary>
        /// <param name="options">Optional rendering options for the chart.</param>
        public BarChartEntry(BarChartOptions options = null)
        {
            _options = options != null ? options.Clone() : new BarChartOptions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BarChartEntry"/> class with chart items.
        /// </summary>
        /// <param name="items">The chart items to render.</param>
        /// <param name="options">Optional rendering options for the chart.</param>
        public BarChartEntry(IEnumerable<BarChartItem> items, BarChartOptions options = null) : this(options)
        {
            if (items == null)
            {
                return;
            }

            foreach (BarChartItem item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Gets the items rendered by the chart.
        /// </summary>
        public IList<BarChartItem> Items => _items;

        /// <summary>
        /// Gets the options used to render the chart.
        /// </summary>
        public BarChartOptions Options => _options;

        /// <summary>
        /// Adds an item to the chart.
        /// </summary>
        /// <param name="label">The label rendered before the bar.</param>
        /// <param name="value">The numeric value represented by the bar.</param>
        /// <param name="color">Optional foreground color used for the bar.</param>
        /// <returns>The added chart item.</returns>
        public BarChartItem Add(string label, double value, ConsoleColor? color = null)
        {
            var item = BarChartItem.Create(label, value, color);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds an item to the chart using Spectre-style style text.
        /// </summary>
        /// <param name="label">The label rendered before the bar.</param>
        /// <param name="value">The numeric value represented by the bar.</param>
        /// <param name="style">Optional style used for the bar, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <returns>The added chart item.</returns>
        public BarChartItem Add(string label, double value, string style)
        {
            var item = BarChartItem.Create(label, value, style);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds an item to the chart using a reusable style.
        /// </summary>
        /// <param name="label">The label rendered before the bar.</param>
        /// <param name="value">The numeric value represented by the bar.</param>
        /// <param name="style">Optional style used for the bar.</param>
        /// <returns>The added chart item.</returns>
        public BarChartItem Add(string label, double value, Style style)
        {
            var item = BarChartItem.Create(label, value, style);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds an item to the chart.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The added chart item.</returns>
        public BarChartItem Add(BarChartItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Renders the chart to the active console driver.
        /// </summary>
        public void Render()
        {
            if (_items.Count == 0)
            {
                return;
            }

            int width = ResolveWidth();
            if (width <= 0)
            {
                return;
            }

            double maximum = ResolveMaximumValue();
            Layout layout = ResolveLayout(width);
            RenderLabel(width);
            foreach (BarChartItem item in _items)
            {
                RenderItem(item, maximum, layout);
            }
        }

        /// <summary>
        /// Renders a temporary bar chart to the active console driver.
        /// </summary>
        /// <param name="items">The chart items to render.</param>
        /// <param name="options">Optional rendering options for the chart.</param>
        public static void Render(IEnumerable<BarChartItem> items, BarChartOptions options = null)
        {
            new BarChartEntry(items, options).Render();
        }

        private int ResolveWidth()
        {
            if (_options.Width.HasValue || _options.Expand)
            {
                return PanelEntry.ResolveWidth(_options.Width);
            }

            int terminalWidth = PanelEntry.ResolveWidth((int?)null);
            int requestedWidth = MeasureCollapsedWidth();
            return Math.Max(1, Math.Min(requestedWidth, terminalWidth));
        }

        private int MeasureCollapsedWidth()
        {
            int titleWidth = string.IsNullOrEmpty(_options.Label)
                ? 0
                : MarkupRenderText.GetPlainText(NormalizeInline(_options.Label)).Length;
            int longestLabel = _items
                .Select(item => MarkupRenderText.GetPlainText(NormalizeInline(item.Label)).Length)
                .DefaultIfEmpty(0)
                .Max();
            int labelWidth = _options.LabelWidth ?? longestLabel;
            int valueWidth = _options.ShowValues
                ? _items.Select(item => _options.FormatValue(item.Value).Length).DefaultIfEmpty(0).Max()
                : 0;
            int barWidth = _options.ResolveMinimumBarWidth();
            int rowWidth = labelWidth + (labelWidth > 0 ? 1 : 0) + barWidth + (valueWidth > 0 ? 1 + valueWidth : 0);

            return Math.Max(1, Math.Max(titleWidth, rowWidth));
        }

        private double ResolveMaximumValue()
        {
            double maximum = _options.MaximumValue ?? _items.Select(item => Math.Max(0, item.Value)).DefaultIfEmpty(0).Max();
            return Math.Max(0, maximum);
        }

        private Layout ResolveLayout(int width)
        {
            int longestLabel = _items.Select(item => MarkupRenderText.GetPlainText(NormalizeInline(item.Label)).Length).DefaultIfEmpty(0).Max();
            int labelWidth = _options.LabelWidth ?? Math.Min(longestLabel, Math.Max(4, width / 3));
            labelWidth = Math.Max(0, Math.Min(labelWidth, Math.Max(0, width - 1)));

            string[] valueTexts = _items.Select(item => _options.FormatValue(item.Value)).ToArray();
            int valueWidth = _options.ShowValues
                ? valueTexts.Select(value => value.Length).DefaultIfEmpty(0).Max()
                : 0;

            int minimumBarWidth = _options.ResolveMinimumBarWidth();
            bool showValues = _options.ShowValues && valueWidth > 0;
            while (showValues && labelWidth + 1 + minimumBarWidth + 1 + valueWidth > width)
            {
                valueWidth--;
                if (valueWidth <= 0)
                {
                    showValues = false;
                    valueWidth = 0;
                }
            }

            while (labelWidth > 0 && labelWidth + 1 + minimumBarWidth + (showValues ? 1 + valueWidth : 0) > width)
            {
                labelWidth--;
            }

            int barWidth = width - labelWidth - (labelWidth > 0 ? 1 : 0) - (showValues ? 1 + valueWidth : 0);
            if (barWidth < 1)
            {
                barWidth = 1;
                labelWidth = Math.Max(0, width - 1);
                showValues = false;
                valueWidth = 0;
            }

            return new Layout(labelWidth, barWidth, showValues, valueWidth);
        }

        private void RenderLabel(int width)
        {
            if (string.IsNullOrEmpty(_options.Label) || width <= 0)
            {
                return;
            }

            StyledText labelStyle = ResolveLabelStyle();
            MarkupRenderText.WriteSegments(
                MarkupRenderText.AlignSegments(
                    MarkupRenderText.ParseSegments(
                        NormalizeInline(_options.Label),
                        labelStyle.Scheme,
                        labelStyle.Decoration,
                        labelStyle.ForegroundRgb,
                        labelStyle.BackgroundRgb),
                    width,
                    _options.LabelAlignment,
                    labelStyle.Scheme),
                labelStyle.Scheme,
                true);
        }

        private void RenderItem(BarChartItem item, double maximum, Layout layout)
        {
            if (layout.LabelWidth > 0)
            {
                StyledText labelStyle = ResolveLabelStyle();
                MarkupRenderText.WriteSegments(PadOrTrimSegments(item.Label, layout.LabelWidth, labelStyle), labelStyle.Scheme, false);
                Consoul.Write(" ", RenderOptions.DefaultScheme, false);
            }

            int filled = 0;
            double value = Math.Max(0, item.Value);
            if (maximum > 0 && value > 0)
            {
                filled = (int)Math.Round((value / maximum) * layout.BarWidth, MidpointRounding.AwayFromZero);
                filled = Math.Max(1, Math.Min(layout.BarWidth, filled));
            }

            string bar = new string(_options.BarCharacter, filled) + new string(_options.EmptyCharacter, layout.BarWidth - filled);
            StyledText barStyle = ResolveBarStyle(item);
            TerminalLinkWriter.WriteText(
                bar,
                barStyle.Scheme,
                null,
                barStyle.Decoration,
                barStyle.ForegroundRgb,
                barStyle.BackgroundRgb);
            if (!layout.ShowValues)
            {
                Consoul.Write(string.Empty);
            }

            if (layout.ShowValues)
            {
                StyledText valueStyle = ResolveValueStyle();
                Consoul.Write(" ", RenderOptions.DefaultScheme, false);
                TerminalLinkWriter.WriteText(
                    PadOrTrim(_options.FormatValue(item.Value), layout.ValueWidth),
                    valueStyle.Scheme,
                    null,
                    valueStyle.Decoration,
                    valueStyle.ForegroundRgb,
                    valueStyle.BackgroundRgb);
                Consoul.Write(string.Empty);
            }
        }

        private StyledText ResolveLabelStyle()
        {
            return new StyledText(
                _options.ResolveLabelScheme(),
                _options.LabelDecoration,
                _options.LabelForegroundRgb,
                _options.LabelBackgroundRgb);
        }

        private StyledText ResolveBarStyle(BarChartItem item)
        {
            ColorScheme scheme = item.Scheme ?? _options.ResolveBarScheme();
            bool hasItemScheme = item.Scheme.HasValue;
            return new StyledText(
                scheme,
                item.Decoration ?? (hasItemScheme ? Decoration.None : _options.BarDecoration),
                item.ForegroundRgb ?? (hasItemScheme ? null : _options.BarForegroundRgb),
                item.BackgroundRgb ?? (hasItemScheme ? null : _options.BarBackgroundRgb));
        }

        private StyledText ResolveValueStyle()
        {
            return new StyledText(
                _options.ResolveValueScheme(),
                _options.ValueDecoration,
                _options.ValueForegroundRgb,
                _options.ValueBackgroundRgb);
        }

        private static List<FixedMessageSegment> PadOrTrimSegments(string text, int width, StyledText style)
        {
            width = Math.Max(0, width);
            List<FixedMessageSegment> result = MarkupRenderText.TruncateSegments(
                MarkupRenderText.ParseSegments(
                    NormalizeInline(text),
                    style.Scheme,
                    style.Decoration,
                    style.ForegroundRgb,
                    style.BackgroundRgb),
                width);
            AddPadding(result, Math.Max(0, width - MarkupRenderText.GetLength(result)), style);
            return result;
        }

        private static void AddPadding(IList<FixedMessageSegment> segments, int count, StyledText style)
        {
            if (count <= 0)
            {
                return;
            }

            segments.Add(new FixedMessageSegment(
                new string(' ', count),
                style.Scheme,
                style.Decoration,
                null,
                style.ForegroundRgb,
                style.BackgroundRgb));
        }

        private static string PadOrTrim(string text, int width)
        {
            text = text ?? string.Empty;
            width = Math.Max(0, width);
            if (text.Length > width)
            {
                return text.Substring(0, width);
            }

            return text.PadRight(width);
        }

        private static string NormalizeInline(string text)
        {
            return (text ?? string.Empty)
                .Replace('\r', ' ')
                .Replace('\n', ' ');
        }

        private static string AlignText(string text, int width, TextBlockAlignment alignment)
        {
            text = (text ?? string.Empty).Replace('\r', ' ').Replace('\n', ' ');
            width = Math.Max(0, width);
            if (text.Length > width)
            {
                text = text.Substring(0, width);
            }

            int padding = Math.Max(0, width - text.Length);
            switch (alignment)
            {
                case TextBlockAlignment.Center:
                    int left = padding / 2;
                    return new string(' ', left) + text + new string(' ', padding - left);
                case TextBlockAlignment.Right:
                    return new string(' ', padding) + text;
                default:
                    return text + new string(' ', padding);
            }
        }

        private readonly struct Layout
        {
            public Layout(int labelWidth, int barWidth, bool showValues, int valueWidth)
            {
                LabelWidth = labelWidth;
                BarWidth = barWidth;
                ShowValues = showValues;
                ValueWidth = valueWidth;
            }

            public int LabelWidth { get; }

            public int BarWidth { get; }

            public bool ShowValues { get; }

            public int ValueWidth { get; }
        }

        private readonly struct StyledText
        {
            public StyledText(ColorScheme scheme, Decoration decoration, RgbColor? foregroundRgb, RgbColor? backgroundRgb)
            {
                Scheme = scheme;
                Decoration = decoration;
                ForegroundRgb = foregroundRgb;
                BackgroundRgb = backgroundRgb;
            }

            public ColorScheme Scheme { get; }

            public Decoration Decoration { get; }

            public RgbColor? ForegroundRgb { get; }

            public RgbColor? BackgroundRgb { get; }
        }
    }
}
