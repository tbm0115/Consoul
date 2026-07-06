using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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
            ColorScheme markerScheme = item.MarkerScheme ?? _options.ResolveMarkerScheme(item.State);
            ColorScheme titleScheme = item.TitleScheme ?? _options.ResolveTitleScheme();
            ColorScheme descriptionScheme = item.DescriptionScheme ?? _options.ResolveDescriptionScheme();
            IList<List<FixedMessageSegment>> titleLines = MarkupRenderText.WrapSegments(
                BuildTitle(item),
                layout.ContentWidth,
                titleScheme);
            WriteLines(marker, markerScheme, titleLines, titleScheme, layout);

            if (_options.ShowDescriptions && !string.IsNullOrWhiteSpace(item.Description))
            {
                string connector = hasNext ? PadOrTrim(_options.Connector, layout.MarkerWidth) : PadOrTrim(string.Empty, layout.MarkerWidth);
                ColorScheme connectorScheme = _options.ShowConnectors ? _options.ResolveConnectorScheme() : RenderOptions.DefaultScheme;
                IList<List<FixedMessageSegment>> descriptionLines = MarkupRenderText.WrapSegments(
                    item.Description,
                    layout.ContentWidth,
                    descriptionScheme);
                WriteLines(_options.ShowConnectors ? connector : PadOrTrim(string.Empty, layout.MarkerWidth), connectorScheme, descriptionLines, descriptionScheme, layout);
            }

            if (_options.ShowConnectors && hasNext)
            {
                WriteLines(
                    PadOrTrim(_options.Connector, layout.MarkerWidth),
                    _options.ResolveConnectorScheme(),
                    new[] { new List<FixedMessageSegment>() },
                    RenderOptions.DefaultScheme,
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
            ColorScheme markerScheme,
            IList<List<FixedMessageSegment>> contentLines,
            ColorScheme contentScheme,
            Layout layout)
        {
            foreach (List<FixedMessageSegment> contentLine in contentLines)
            {
                string markerText = PadOrTrim(marker, layout.MarkerWidth);
                string gap = new string(' ', layout.Gap);
                int contentWidth = Math.Max(1, layout.Width - markerText.Length - gap.Length);

                Consoul.Write(markerText, markerScheme, false);
                Consoul.Write(gap, RenderOptions.DefaultScheme, false);
                List<FixedMessageSegment> content = MarkupRenderText.TruncateSegments(contentLine, contentWidth);
                MarkupRenderText.AddPadding(content, Math.Max(0, contentWidth - MarkupRenderText.GetLength(content)), contentScheme);
                MarkupRenderText.WriteSegments(content, contentScheme, true);
            }
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
    }
}
