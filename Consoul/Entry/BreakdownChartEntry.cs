using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders a responsive breakdown chart for composition-style console view content.
    /// </summary>
    public class BreakdownChartEntry : IRenderable
    {
        private readonly List<BreakdownChartItem> _items = new List<BreakdownChartItem>();
        private readonly BreakdownChartOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="BreakdownChartEntry"/> class.
        /// </summary>
        /// <param name="options">Optional rendering options for the chart.</param>
        public BreakdownChartEntry(BreakdownChartOptions options = null)
        {
            _options = options != null ? options.Clone() : new BreakdownChartOptions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BreakdownChartEntry"/> class with chart items.
        /// </summary>
        /// <param name="items">The chart items to render.</param>
        /// <param name="options">Optional rendering options for the chart.</param>
        public BreakdownChartEntry(IEnumerable<BreakdownChartItem> items, BreakdownChartOptions options = null) : this(options)
        {
            if (items == null)
            {
                return;
            }

            foreach (BreakdownChartItem item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Gets the items rendered by the chart.
        /// </summary>
        public IList<BreakdownChartItem> Items => _items;

        /// <summary>
        /// Gets the options used to render the chart.
        /// </summary>
        public BreakdownChartOptions Options => _options;

        /// <summary>
        /// Adds an item to the chart.
        /// </summary>
        /// <param name="label">The item label.</param>
        /// <param name="value">The item value.</param>
        /// <param name="color">Optional foreground color used for the item segment.</param>
        /// <returns>The added chart item.</returns>
        public BreakdownChartItem Add(string label, double value, ConsoleColor? color = null)
        {
            var item = BreakdownChartItem.Create(label, value, color);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds an item to the chart using Spectre-style style text.
        /// </summary>
        /// <param name="label">The item label.</param>
        /// <param name="value">The item value.</param>
        /// <param name="style">Optional style used for the item segment, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <returns>The added chart item.</returns>
        public BreakdownChartItem Add(string label, double value, string style)
        {
            var item = BreakdownChartItem.Create(label, value, style);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds an item to the chart using a reusable style.
        /// </summary>
        /// <param name="label">The item label.</param>
        /// <param name="value">The item value.</param>
        /// <param name="style">Optional style used for the item segment.</param>
        /// <returns>The added chart item.</returns>
        public BreakdownChartItem Add(string label, double value, Style style)
        {
            var item = BreakdownChartItem.Create(label, value, style);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds an item to the chart.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The added chart item.</returns>
        public BreakdownChartItem Add(BreakdownChartItem item)
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

            int width = PanelEntry.ResolveWidth(_options.Width);
            if (width <= 0)
            {
                return;
            }

            double total = _items.Select(item => Math.Max(0, item.Value)).Sum();
            RenderLabel(width);
            if (_options.ShowBar)
            {
                RenderBar(width, total);
            }

            if (_options.ShowLegend)
            {
                RenderLegend(width, total);
            }
        }

        /// <summary>
        /// Renders a temporary breakdown chart to the active console driver.
        /// </summary>
        /// <param name="items">The chart items to render.</param>
        /// <param name="options">Optional rendering options for the chart.</param>
        public static void Render(IEnumerable<BreakdownChartItem> items, BreakdownChartOptions options = null)
        {
            new BreakdownChartEntry(items, options).Render();
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

        private void RenderBar(int width, double total)
        {
            int[] lengths = ResolveSegmentLengths(width, total);
            int written = 0;
            for (int index = 0; index < _items.Count; index++)
            {
                int length = lengths[index];
                if (length <= 0)
                {
                    continue;
                }

                written += length;
                StyledText segmentStyle = ResolveSegmentStyle(_items[index]);
                TerminalLinkWriter.WriteText(
                    new string(_options.SegmentCharacter, length),
                    segmentStyle.Scheme,
                    null,
                    segmentStyle.Decoration,
                    segmentStyle.ForegroundRgb,
                    segmentStyle.BackgroundRgb);
            }

            if (written < width)
            {
                StyledText valueStyle = ResolveValueStyle();
                TerminalLinkWriter.WriteText(
                    new string(_options.EmptyCharacter, width - written),
                    valueStyle.Scheme,
                    null,
                    valueStyle.Decoration,
                    valueStyle.ForegroundRgb,
                    valueStyle.BackgroundRgb);
            }

            Consoul.Write(string.Empty);
        }

        private int[] ResolveSegmentLengths(int width, double total)
        {
            var lengths = new int[_items.Count];
            if (total <= 0)
            {
                return lengths;
            }

            var remainders = new List<SegmentRemainder>();
            int used = 0;
            for (int index = 0; index < _items.Count; index++)
            {
                double value = Math.Max(0, _items[index].Value);
                if (value <= 0)
                {
                    continue;
                }

                double exact = (value / total) * width;
                int length = (int)Math.Floor(exact);
                lengths[index] = length;
                used += length;
                remainders.Add(new SegmentRemainder(index, exact - length));
            }

            int remaining = width - used;
            foreach (SegmentRemainder remainder in remainders.OrderByDescending(item => item.Remainder))
            {
                if (remaining <= 0)
                {
                    break;
                }

                lengths[remainder.Index]++;
                remaining--;
            }

            return lengths;
        }

        private void RenderLegend(int width, double total)
        {
            foreach (BreakdownChartItem item in _items)
            {
                string valueText = BuildValueText(item, total);
                string prefix = _options.SegmentCharacter.ToString() + _options.SegmentCharacter + " ";
                int valueWidth = valueText.Length;
                bool showValue = valueWidth > 0 && prefix.Length + 1 + valueWidth < width;
                int labelWidth = width - prefix.Length - (showValue ? valueWidth + 1 : 0);
                if (labelWidth < 0)
                {
                    labelWidth = 0;
                    showValue = false;
                }

                StyledText segmentStyle = ResolveSegmentStyle(item);
                TerminalLinkWriter.WriteText(
                    PadOrTrim(prefix, Math.Min(prefix.Length, width)),
                    segmentStyle.Scheme,
                    null,
                    segmentStyle.Decoration,
                    segmentStyle.ForegroundRgb,
                    segmentStyle.BackgroundRgb);

                if (labelWidth > 0)
                {
                    StyledText labelStyle = ResolveLabelStyle();
                    MarkupRenderText.WriteSegments(PadOrTrimSegments(item.Label, labelWidth, labelStyle), labelStyle.Scheme, !showValue);
                }

                if (showValue)
                {
                    StyledText valueStyle = ResolveValueStyle();
                    Consoul.Write(" ", RenderOptions.DefaultScheme, false);
                    TerminalLinkWriter.WriteText(
                        PadOrTrim(valueText, valueWidth),
                        valueStyle.Scheme,
                        null,
                        valueStyle.Decoration,
                        valueStyle.ForegroundRgb,
                        valueStyle.BackgroundRgb);
                    Consoul.Write(string.Empty);
                }
                else if (labelWidth == 0)
                {
                    Consoul.Write(string.Empty, RenderOptions.DefaultScheme, true);
                }
            }
        }

        private string BuildValueText(BreakdownChartItem item, double total)
        {
            var parts = new List<string>();
            if (_options.ShowValues)
            {
                parts.Add(_options.FormatValue(item.Value));
            }

            if (_options.ShowPercentages)
            {
                double percentage = total > 0 ? Math.Max(0, item.Value) / total * 100.0 : 0;
                parts.Add(_options.FormatPercentage(percentage));
            }

            return string.Join(" ", parts);
        }

        private StyledText ResolveSegmentStyle(BreakdownChartItem item)
        {
            ColorScheme scheme = item.Scheme ?? _options.ResolveSegmentScheme();
            bool hasItemScheme = item.Scheme.HasValue;
            return new StyledText(
                scheme,
                item.Decoration ?? (hasItemScheme ? Decoration.None : _options.SegmentDecoration),
                item.ForegroundRgb ?? (hasItemScheme ? null : _options.SegmentForegroundRgb),
                item.BackgroundRgb ?? (hasItemScheme ? null : _options.SegmentBackgroundRgb));
        }

        private StyledText ResolveLabelStyle()
        {
            return new StyledText(
                _options.ResolveLabelScheme(),
                _options.LabelDecoration,
                _options.LabelForegroundRgb,
                _options.LabelBackgroundRgb);
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

        private readonly struct SegmentRemainder
        {
            public SegmentRemainder(int index, double remainder)
            {
                Index = index;
                Remainder = remainder;
            }

            public int Index { get; }

            public double Remainder { get; }
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
