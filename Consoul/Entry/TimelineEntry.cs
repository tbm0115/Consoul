using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RgbColor = ConsoulLibrary.Color.RgbColor;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders a responsive timeline for workflow steps, event history, and model state changes.
    /// </summary>
    public class TimelineEntry : IRenderable
    {
        private readonly List<TimelineItem> _items = new List<TimelineItem>();
        private readonly TimelineOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimelineEntry"/> class.
        /// </summary>
        /// <param name="options">Optional rendering options for the timeline.</param>
        public TimelineEntry(TimelineOptions options = null)
        {
            _options = options != null ? options.Clone() : new TimelineOptions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimelineEntry"/> class with items.
        /// </summary>
        /// <param name="items">The timeline items to render.</param>
        /// <param name="options">Optional rendering options for the timeline.</param>
        public TimelineEntry(IEnumerable<TimelineItem> items, TimelineOptions options = null) : this(options)
        {
            if (items == null)
            {
                return;
            }

            foreach (TimelineItem item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Gets the items rendered by the timeline.
        /// </summary>
        public IList<TimelineItem> Items => _items;

        /// <summary>
        /// Gets the options used to render the timeline.
        /// </summary>
        public TimelineOptions Options => _options;

        /// <summary>
        /// Adds an item to the timeline.
        /// </summary>
        /// <param name="title">The primary item text.</param>
        /// <param name="state">The rendered item state.</param>
        /// <param name="description">Optional descriptive text rendered below the title.</param>
        /// <param name="timestamp">Optional timestamp rendered before the title.</param>
        /// <param name="titleColor">Optional foreground color used for the title.</param>
        /// <param name="markerColor">Optional foreground color used for the marker.</param>
        /// <param name="descriptionColor">Optional foreground color used for the description.</param>
        /// <returns>The added timeline item.</returns>
        public TimelineItem Add(
            string title,
            TimelineItemState state = TimelineItemState.Pending,
            string description = null,
            DateTimeOffset? timestamp = null,
            ConsoleColor? titleColor = null,
            ConsoleColor? markerColor = null,
            ConsoleColor? descriptionColor = null)
        {
            TimelineItem item = TimelineItem.Create(title, state, description, timestamp, titleColor, markerColor, descriptionColor);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds an item to the timeline using reusable style objects.
        /// </summary>
        /// <param name="title">The primary item text.</param>
        /// <param name="titleStyle">Optional style used for the title.</param>
        /// <param name="markerStyle">Optional style used for the marker.</param>
        /// <param name="descriptionStyle">Optional style used for the description.</param>
        /// <param name="state">The rendered item state.</param>
        /// <param name="description">Optional descriptive text rendered below the title.</param>
        /// <param name="timestamp">Optional timestamp rendered before the title.</param>
        /// <returns>The added timeline item.</returns>
        public TimelineItem Add(
            string title,
            Style titleStyle,
            Style markerStyle = null,
            Style descriptionStyle = null,
            TimelineItemState state = TimelineItemState.Pending,
            string description = null,
            DateTimeOffset? timestamp = null)
        {
            TimelineItem item = TimelineItem.Create(title, titleStyle, markerStyle, descriptionStyle, state, description, timestamp);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds an item to the timeline using Spectre-style style text.
        /// </summary>
        /// <param name="title">The primary item text.</param>
        /// <param name="state">The rendered item state.</param>
        /// <param name="description">Optional descriptive text rendered below the title.</param>
        /// <param name="timestamp">Optional timestamp rendered before the title.</param>
        /// <param name="titleStyle">Optional style used for the title, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="markerStyle">Optional style used for the marker, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <param name="descriptionStyle">Optional style used for the description, such as <c>darkgray</c>.</param>
        /// <returns>The added timeline item.</returns>
        public TimelineItem AddStyled(
            string title,
            TimelineItemState state = TimelineItemState.Pending,
            string description = null,
            DateTimeOffset? timestamp = null,
            string titleStyle = null,
            string markerStyle = null,
            string descriptionStyle = null)
        {
            TimelineItem item = TimelineItem.CreateStyled(title, state, description, timestamp, titleStyle, markerStyle, descriptionStyle);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds an item to the timeline.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The added timeline item.</returns>
        public TimelineItem Add(TimelineItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Renders the timeline to the active console driver.
        /// </summary>
        public void Render()
        {
            if (_items.Count == 0)
            {
                return;
            }

            int width = PanelEntry.ResolveWidth(_options.Width);
            Layout layout = ResolveLayout(width);
            for (int index = 0; index < _items.Count; index++)
            {
                RenderItem(_items[index], layout, index < _items.Count - 1);
            }
        }

        /// <summary>
        /// Renders a temporary timeline to the active console driver.
        /// </summary>
        /// <param name="items">The timeline items to render.</param>
        /// <param name="options">Optional rendering options for the timeline.</param>
        public static void Render(IEnumerable<TimelineItem> items, TimelineOptions options = null)
        {
            new TimelineEntry(items, options).Render();
        }

        private Layout ResolveLayout(int width)
        {
            int markerWidth = _items
                .Select(item => (_options.ResolveMarker(item.State) ?? string.Empty).Length)
                .Concat(new[] { (_options.Connector ?? string.Empty).Length })
                .DefaultIfEmpty(0)
                .Max();
            markerWidth = Math.Min(markerWidth, width);

            int gap = markerWidth > 0 ? Math.Max(0, _options.MarkerGap) : 0;
            if (markerWidth + gap >= width)
            {
                gap = Math.Max(0, width - markerWidth);
            }

            int contentWidth = Math.Max(1, width - markerWidth - gap);
            return new Layout(width, markerWidth, gap, contentWidth);
        }

        private void RenderItem(TimelineItem item, Layout layout, bool hasNext)
        {
            string marker = PadOrTrim(_options.ResolveMarker(item.State), layout.MarkerWidth);
            StyledText markerStyle = ResolveMarkerStyle(item);
            StyledText titleStyle = ResolveTitleStyle(item);
            StyledText descriptionStyle = ResolveDescriptionStyle(item);
            IList<List<FixedMessageSegment>> titleLines = MarkupRenderText.WrapSegments(
                BuildTitle(item),
                layout.ContentWidth,
                titleStyle.Scheme,
                titleStyle.Decoration,
                titleStyle.ForegroundRgb,
                titleStyle.BackgroundRgb);
            WriteLines(marker, markerStyle, titleLines, titleStyle, layout);

            if (_options.ShowDescriptions && !string.IsNullOrWhiteSpace(item.Description))
            {
                string connector = hasNext ? PadOrTrim(_options.Connector, layout.MarkerWidth) : PadOrTrim(string.Empty, layout.MarkerWidth);
                StyledText connectorStyle = _options.ShowConnectors
                    ? ResolveConnectorStyle()
                    : new StyledText(RenderOptions.DefaultScheme, Decoration.None, null, null);
                IList<List<FixedMessageSegment>> descriptionLines = MarkupRenderText.WrapSegments(
                    item.Description,
                    layout.ContentWidth,
                    descriptionStyle.Scheme,
                    descriptionStyle.Decoration,
                    descriptionStyle.ForegroundRgb,
                    descriptionStyle.BackgroundRgb);
                WriteLines(_options.ShowConnectors ? connector : PadOrTrim(string.Empty, layout.MarkerWidth), connectorStyle, descriptionLines, descriptionStyle, layout);
            }

            if (_options.ShowConnectors && hasNext)
            {
                WriteLines(
                    PadOrTrim(_options.Connector, layout.MarkerWidth),
                    ResolveConnectorStyle(),
                    new[] { new List<FixedMessageSegment>() },
                    new StyledText(RenderOptions.DefaultScheme, Decoration.None, null, null),
                    layout);
            }
        }

        private string BuildTitle(TimelineItem item)
        {
            string title = item?.Title ?? string.Empty;
            if (!_options.ShowTimestamps || item?.Timestamp == null)
            {
                return title;
            }

            string timestamp = item.Timestamp.Value.ToString(_options.TimestampFormat, CultureInfo.CurrentCulture);
            return "[" + timestamp + "] " + title;
        }

        private void WriteLines(
            string marker,
            StyledText markerStyle,
            IList<List<FixedMessageSegment>> contentLines,
            StyledText contentStyle,
            Layout layout)
        {
            foreach (List<FixedMessageSegment> contentLine in contentLines)
            {
                string markerText = PadOrTrim(marker, layout.MarkerWidth);
                string gap = new string(' ', layout.Gap);
                int contentWidth = Math.Max(1, layout.Width - markerText.Length - gap.Length);

                TerminalLinkWriter.WriteText(
                    markerText,
                    markerStyle.Scheme,
                    null,
                    markerStyle.Decoration,
                    markerStyle.ForegroundRgb,
                    markerStyle.BackgroundRgb);
                Consoul.Write(gap, RenderOptions.DefaultScheme, false);
                List<FixedMessageSegment> content = MarkupRenderText.TruncateSegments(contentLine, contentWidth);
                AddPadding(content, Math.Max(0, contentWidth - MarkupRenderText.GetLength(content)), contentStyle);
                MarkupRenderText.WriteSegments(content, contentStyle.Scheme, true);
            }
        }

        private StyledText ResolveMarkerStyle(TimelineItem item)
        {
            ColorScheme scheme = item.MarkerScheme ?? _options.ResolveMarkerScheme(item.State);
            bool hasItemScheme = item.MarkerScheme.HasValue;
            StyledText optionStyle = ResolveOptionMarkerStyle(item.State);
            return new StyledText(
                scheme,
                item.MarkerDecoration ?? (hasItemScheme ? Decoration.None : optionStyle.Decoration),
                item.MarkerForegroundRgb ?? (hasItemScheme ? null : optionStyle.ForegroundRgb),
                item.MarkerBackgroundRgb ?? (hasItemScheme ? null : optionStyle.BackgroundRgb));
        }

        private StyledText ResolveTitleStyle(TimelineItem item)
        {
            ColorScheme scheme = item.TitleScheme ?? _options.ResolveTitleScheme();
            bool hasItemScheme = item.TitleScheme.HasValue;
            return new StyledText(
                scheme,
                item.TitleDecoration ?? (hasItemScheme ? Decoration.None : _options.TitleDecoration),
                item.TitleForegroundRgb ?? (hasItemScheme ? null : _options.TitleForegroundRgb),
                item.TitleBackgroundRgb ?? (hasItemScheme ? null : _options.TitleBackgroundRgb));
        }

        private StyledText ResolveDescriptionStyle(TimelineItem item)
        {
            ColorScheme scheme = item.DescriptionScheme ?? _options.ResolveDescriptionScheme();
            bool hasItemScheme = item.DescriptionScheme.HasValue;
            return new StyledText(
                scheme,
                item.DescriptionDecoration ?? (hasItemScheme ? Decoration.None : _options.DescriptionDecoration),
                item.DescriptionForegroundRgb ?? (hasItemScheme ? null : _options.DescriptionForegroundRgb),
                item.DescriptionBackgroundRgb ?? (hasItemScheme ? null : _options.DescriptionBackgroundRgb));
        }

        private StyledText ResolveConnectorStyle()
        {
            return new StyledText(
                _options.ResolveConnectorScheme(),
                _options.ConnectorDecoration,
                _options.ConnectorForegroundRgb,
                _options.ConnectorBackgroundRgb);
        }

        private StyledText ResolveOptionMarkerStyle(TimelineItemState state)
        {
            switch (state)
            {
                case TimelineItemState.Active:
                    return new StyledText(
                        _options.ResolveMarkerScheme(state),
                        _options.ActiveDecoration,
                        _options.ActiveForegroundRgb,
                        _options.ActiveBackgroundRgb);
                case TimelineItemState.Complete:
                    return new StyledText(
                        _options.ResolveMarkerScheme(state),
                        _options.CompleteDecoration,
                        _options.CompleteForegroundRgb,
                        _options.CompleteBackgroundRgb);
                case TimelineItemState.Warning:
                    return new StyledText(
                        _options.ResolveMarkerScheme(state),
                        _options.WarningDecoration,
                        _options.WarningForegroundRgb,
                        _options.WarningBackgroundRgb);
                case TimelineItemState.Error:
                    return new StyledText(
                        _options.ResolveMarkerScheme(state),
                        _options.ErrorDecoration,
                        _options.ErrorForegroundRgb,
                        _options.ErrorBackgroundRgb);
                case TimelineItemState.Skipped:
                    return new StyledText(
                        _options.ResolveMarkerScheme(state),
                        _options.SkippedDecoration,
                        _options.SkippedForegroundRgb,
                        _options.SkippedBackgroundRgb);
                default:
                    return new StyledText(
                        _options.ResolveMarkerScheme(state),
                        _options.PendingDecoration,
                        _options.PendingForegroundRgb,
                        _options.PendingBackgroundRgb);
            }
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

        private readonly struct Layout
        {
            public Layout(int width, int markerWidth, int gap, int contentWidth)
            {
                Width = width;
                MarkerWidth = markerWidth;
                Gap = gap;
                ContentWidth = contentWidth;
            }

            public int Width { get; }

            public int MarkerWidth { get; }

            public int Gap { get; }

            public int ContentWidth { get; }
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
