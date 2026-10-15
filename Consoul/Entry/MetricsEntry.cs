using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;
using System.Linq;
using RgbColor = ConsoulLibrary.Color.RgbColor;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders compact metric summaries for dashboards and model-backed views.
    /// </summary>
    public class MetricsEntry : IRenderable
    {
        private readonly List<MetricItem> _items = new List<MetricItem>();
        private readonly MetricsOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricsEntry"/> class.
        /// </summary>
        /// <param name="options">Optional rendering options for the metrics.</param>
        public MetricsEntry(MetricsOptions options = null)
        {
            _options = options != null ? options.Clone() : new MetricsOptions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricsEntry"/> class with items.
        /// </summary>
        /// <param name="items">The metric items to render.</param>
        /// <param name="options">Optional rendering options for the metrics.</param>
        public MetricsEntry(IEnumerable<MetricItem> items, MetricsOptions options = null) : this(options)
        {
            if (items == null)
            {
                return;
            }

            foreach (MetricItem item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Gets the metric items rendered by the metrics helper.
        /// </summary>
        public IList<MetricItem> Items => _items;

        /// <summary>
        /// Gets the options used to render the metrics helper.
        /// </summary>
        public MetricsOptions Options => _options;

        /// <summary>
        /// Adds a metric item.
        /// </summary>
        /// <param name="label">The metric label.</param>
        /// <param name="value">The primary metric value.</param>
        /// <param name="unit">Optional unit text rendered next to the value.</param>
        /// <param name="note">Optional supporting note rendered below the value.</param>
        /// <param name="kind">The visual intent of the metric value.</param>
        /// <param name="labelColor">Optional foreground color used for the label.</param>
        /// <param name="valueColor">Optional foreground color used for the value.</param>
        /// <param name="noteColor">Optional foreground color used for the note.</param>
        /// <returns>The added metric item.</returns>
        public MetricItem Add(
            string label,
            object value,
            string unit = null,
            string note = null,
            MetricKind kind = MetricKind.Default,
            ConsoleColor? labelColor = null,
            ConsoleColor? valueColor = null,
            ConsoleColor? noteColor = null)
        {
            MetricItem item = MetricItem.Create(label, value, unit, note, kind, labelColor, valueColor, noteColor);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds a metric item using reusable style objects.
        /// </summary>
        /// <param name="label">The metric label.</param>
        /// <param name="value">The primary metric value.</param>
        /// <param name="labelStyle">Optional style used for the label.</param>
        /// <param name="valueStyle">Optional style used for the value.</param>
        /// <param name="noteStyle">Optional style used for the note.</param>
        /// <param name="unit">Optional unit text rendered next to the value.</param>
        /// <param name="note">Optional supporting note rendered below the value.</param>
        /// <param name="kind">The visual intent of the metric value.</param>
        /// <returns>The added metric item.</returns>
        public MetricItem Add(
            string label,
            object value,
            Style labelStyle,
            Style valueStyle = null,
            Style noteStyle = null,
            string unit = null,
            string note = null,
            MetricKind kind = MetricKind.Default)
        {
            MetricItem item = MetricItem.Create(label, value, labelStyle, valueStyle, noteStyle, unit, note, kind);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds a metric item using Spectre-style style text.
        /// </summary>
        /// <param name="label">The metric label.</param>
        /// <param name="value">The primary metric value.</param>
        /// <param name="labelStyle">Optional style used for the label, such as <c>grey</c>.</param>
        /// <param name="valueStyle">Optional style used for the value, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="noteStyle">Optional style used for the note, such as <c>darkgray</c>.</param>
        /// <param name="unit">Optional unit text rendered next to the value.</param>
        /// <param name="note">Optional supporting note rendered below the value.</param>
        /// <param name="kind">The visual intent of the metric value.</param>
        /// <returns>The added metric item.</returns>
        public MetricItem AddStyled(
            string label,
            object value,
            string labelStyle = null,
            string valueStyle = null,
            string noteStyle = null,
            string unit = null,
            string note = null,
            MetricKind kind = MetricKind.Default)
        {
            MetricItem item = MetricItem.CreateStyled(label, value, labelStyle, valueStyle, noteStyle, unit, note, kind);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds a metric item.
        /// </summary>
        /// <param name="item">The metric item to add.</param>
        /// <returns>The added metric item.</returns>
        public MetricItem Add(MetricItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Renders the metrics to the active console driver.
        /// </summary>
        public void Render()
        {
            if (_items.Count == 0)
            {
                return;
            }

            int width = PanelEntry.ResolveWidth(_options.Width);
            int columns = ResolveColumns(width);
            int gap = ResolveGap(width, columns);
            int columnWidth = Math.Max(1, (width - (gap * Math.Max(0, columns - 1))) / columns);

            for (int index = 0; index < _items.Count; index += columns)
            {
                IList<IList<MetricLine>> cells = _items
                    .Skip(index)
                    .Take(columns)
                    .Select(item => BuildCell(item, columnWidth))
                    .ToList();
                RenderRow(cells, columnWidth, gap);
            }
        }

        /// <summary>
        /// Renders temporary metrics to the active console driver.
        /// </summary>
        /// <param name="items">The metric items to render.</param>
        /// <param name="options">Optional rendering options for the metrics.</param>
        public static void Render(IEnumerable<MetricItem> items, MetricsOptions options = null)
        {
            new MetricsEntry(items, options).Render();
        }

        private int ResolveColumns(int width)
        {
            int itemCount = Math.Max(1, _items.Count);
            int minimumColumnWidth = Math.Max(1, _options.MinimumColumnWidth);
            int automatic = Math.Max(1, width / minimumColumnWidth);
            int requested = _options.Columns ?? automatic;
            return Math.Max(1, Math.Min(itemCount, requested));
        }

        private int ResolveGap(int width, int columns)
        {
            int gap = Math.Max(0, _options.ColumnGap);
            while (columns > 1 && gap > 0 && (columns * 1) + (gap * (columns - 1)) > width)
            {
                gap--;
            }

            return gap;
        }

        private IList<MetricLine> BuildCell(MetricItem item, int width)
        {
            var lines = new List<MetricLine>();
            AddWrapped(lines, item.Label, width, ResolveLabelStyle(item));

            string value = (item.Value ?? string.Empty).ToString().Trim();
            string unit = (item.Unit ?? string.Empty).Trim();
            string valueText = unit.Length == 0 ? value : value + " " + unit;
            AddWrapped(lines, valueText, width, ResolveValueStyle(item));

            if (_options.ShowNotes && !string.IsNullOrWhiteSpace(item.Note))
            {
                AddWrapped(lines, item.Note, width, ResolveNoteStyle(item));
            }

            return lines;
        }

        private StyledText ResolveLabelStyle(MetricItem item)
        {
            ColorScheme scheme = item.LabelScheme ?? _options.ResolveLabelScheme();
            bool hasItemScheme = item.LabelScheme.HasValue;
            return new StyledText(
                scheme,
                item.LabelDecoration ?? (hasItemScheme ? Decoration.None : _options.LabelDecoration),
                item.LabelForegroundRgb ?? (hasItemScheme ? null : _options.LabelForegroundRgb),
                item.LabelBackgroundRgb ?? (hasItemScheme ? null : _options.LabelBackgroundRgb));
        }

        private StyledText ResolveValueStyle(MetricItem item)
        {
            ColorScheme scheme = item.ValueScheme ?? _options.ResolveValueScheme(item.Kind);
            bool hasItemScheme = item.ValueScheme.HasValue;
            StyledText optionStyle = ResolveOptionValueStyle(item.Kind);
            return new StyledText(
                scheme,
                item.ValueDecoration ?? (hasItemScheme ? Decoration.None : optionStyle.Decoration),
                item.ValueForegroundRgb ?? (hasItemScheme ? null : optionStyle.ForegroundRgb),
                item.ValueBackgroundRgb ?? (hasItemScheme ? null : optionStyle.BackgroundRgb));
        }

        private StyledText ResolveNoteStyle(MetricItem item)
        {
            ColorScheme scheme = item.NoteScheme ?? _options.ResolveNoteScheme();
            bool hasItemScheme = item.NoteScheme.HasValue;
            return new StyledText(
                scheme,
                item.NoteDecoration ?? (hasItemScheme ? Decoration.None : _options.NoteDecoration),
                item.NoteForegroundRgb ?? (hasItemScheme ? null : _options.NoteForegroundRgb),
                item.NoteBackgroundRgb ?? (hasItemScheme ? null : _options.NoteBackgroundRgb));
        }

        private StyledText ResolveOptionValueStyle(MetricKind kind)
        {
            switch (kind)
            {
                case MetricKind.Info:
                    return new StyledText(
                        _options.ResolveValueScheme(kind),
                        _options.InfoDecoration,
                        _options.InfoForegroundRgb,
                        _options.InfoBackgroundRgb);
                case MetricKind.Success:
                    return new StyledText(
                        _options.ResolveValueScheme(kind),
                        _options.SuccessDecoration,
                        _options.SuccessForegroundRgb,
                        _options.SuccessBackgroundRgb);
                case MetricKind.Warning:
                    return new StyledText(
                        _options.ResolveValueScheme(kind),
                        _options.WarningDecoration,
                        _options.WarningForegroundRgb,
                        _options.WarningBackgroundRgb);
                case MetricKind.Error:
                    return new StyledText(
                        _options.ResolveValueScheme(kind),
                        _options.ErrorDecoration,
                        _options.ErrorForegroundRgb,
                        _options.ErrorBackgroundRgb);
                default:
                    return new StyledText(
                        _options.ResolveValueScheme(kind),
                        _options.ValueDecoration,
                        _options.ValueForegroundRgb,
                        _options.ValueBackgroundRgb);
            }
        }

        private static void AddWrapped(IList<MetricLine> lines, string text, int width, StyledText style)
        {
            foreach (List<FixedMessageSegment> line in MarkupRenderText.WrapSegments(
                NormalizeInline(text),
                Math.Max(1, width),
                style.Scheme,
                style.Decoration,
                style.ForegroundRgb,
                style.BackgroundRgb))
            {
                lines.Add(new MetricLine(line, style));
            }
        }

        private static void RenderRow(IList<IList<MetricLine>> cells, int columnWidth, int gap)
        {
            int height = cells.Select(cell => cell.Count).DefaultIfEmpty(0).Max();
            string gapText = new string(' ', Math.Max(0, gap));
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < cells.Count; column++)
                {
                    MetricLine line = row < cells[column].Count
                        ? cells[column][row]
                        : MetricLine.Empty;

                    MarkupRenderText.WriteSegments(PadOrTrimSegments(line.Segments, columnWidth, line.Style), line.Style.Scheme, false);
                    if (column < cells.Count - 1 && gapText.Length > 0)
                    {
                        Consoul.Write(gapText, RenderOptions.DefaultScheme, false);
                    }
                }

                Consoul.Write(string.Empty, RenderOptions.DefaultScheme, true);
            }
        }

        private static List<FixedMessageSegment> PadOrTrimSegments(IList<FixedMessageSegment> segments, int width, StyledText style)
        {
            width = Math.Max(0, width);
            List<FixedMessageSegment> result = MarkupRenderText.TruncateSegments(segments, width);
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

        private static string NormalizeInline(string text)
        {
            return (text ?? string.Empty)
                .Replace('\r', ' ')
                .Replace('\n', ' ');
        }

        private readonly struct MetricLine
        {
            public static readonly MetricLine Empty = new MetricLine(new List<FixedMessageSegment>(), new StyledText(RenderOptions.DefaultScheme, Decoration.None, null, null));

            public MetricLine(IList<FixedMessageSegment> segments, StyledText style)
            {
                Segments = segments ?? new List<FixedMessageSegment>();
                Style = style;
            }

            public IList<FixedMessageSegment> Segments { get; }

            public StyledText Style { get; }
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
