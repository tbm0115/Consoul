using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;
using System.Linq;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders titled text panes as responsive dashboard-style rows or columns.
    /// </summary>
    public class LayoutEntry : IRenderable
    {
        private readonly List<LayoutPane> _panes = new List<LayoutPane>();
        private readonly LayoutOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutEntry"/> class.
        /// </summary>
        /// <param name="options">Optional layout rendering options.</param>
        public LayoutEntry(LayoutOptions options = null)
        {
            _options = options != null ? options.Clone() : new LayoutOptions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutEntry"/> class with the provided panes.
        /// </summary>
        /// <param name="panes">Panes to render.</param>
        /// <param name="options">Optional layout rendering options.</param>
        public LayoutEntry(IEnumerable<LayoutPane> panes, LayoutOptions options = null) : this(options)
        {
            if (panes == null)
            {
                return;
            }

            foreach (LayoutPane pane in panes)
            {
                Add(pane);
            }
        }

        /// <summary>
        /// Gets the panes rendered by the layout.
        /// </summary>
        public IList<LayoutPane> Panes => _panes;

        /// <summary>
        /// Gets the options used to render the layout.
        /// </summary>
        public LayoutOptions Options => _options;

        /// <summary>
        /// Adds a pane to the layout.
        /// </summary>
        /// <param name="content">The pane content.</param>
        /// <param name="title">Optional title rendered above the pane content.</param>
        /// <param name="color">Optional foreground color used for pane content.</param>
        /// <returns>The added pane.</returns>
        public LayoutPane Add(string content, string title = null, ConsoleColor? color = null)
        {
            LayoutPane pane = LayoutPane.Create(content, title, color);
            _panes.Add(pane);
            return pane;
        }

        /// <summary>
        /// Adds a pane to the layout using Spectre-style style text.
        /// </summary>
        /// <param name="content">The pane content.</param>
        /// <param name="title">Optional title rendered above the pane content.</param>
        /// <param name="style">Optional style used for pane content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <returns>The added pane.</returns>
        public LayoutPane Add(string content, string title, string style)
        {
            LayoutPane pane = LayoutPane.Create(content, title, style);
            _panes.Add(pane);
            return pane;
        }

        /// <summary>
        /// Adds a pane to the layout using a reusable style.
        /// </summary>
        /// <param name="content">The pane content.</param>
        /// <param name="title">Optional title rendered above the pane content.</param>
        /// <param name="style">Optional style used for pane content.</param>
        /// <returns>The added pane.</returns>
        public LayoutPane Add(string content, string title, Style style)
        {
            LayoutPane pane = LayoutPane.Create(content, title, style);
            _panes.Add(pane);
            return pane;
        }

        /// <summary>
        /// Adds a pane whose content is captured from a Consoul render action.
        /// </summary>
        /// <param name="render">The render action used to produce pane content.</param>
        /// <param name="title">Optional title rendered above the pane content.</param>
        /// <param name="color">Optional foreground color used for the captured pane content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        /// <returns>The added pane.</returns>
        public LayoutPane Add(Action render, string title = null, ConsoleColor? color = null, int? captureWidth = null, int captureHeight = 200)
        {
            return Add(RenderActionContent.Capture(render, captureWidth ?? _options.Width, captureHeight), title, color);
        }

        /// <summary>
        /// Adds a pane whose content is captured from a Consoul render action using Spectre-style style text.
        /// </summary>
        /// <param name="render">The render action used to produce pane content.</param>
        /// <param name="title">Optional title rendered above the pane content.</param>
        /// <param name="style">Optional style used for the captured pane content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        /// <returns>The added pane.</returns>
        public LayoutPane Add(Action render, string title, string style, int? captureWidth = null, int captureHeight = 200)
        {
            return Add(RenderActionContent.Capture(render, captureWidth ?? _options.Width, captureHeight), title, style);
        }

        /// <summary>
        /// Adds a pane whose content is captured from a Consoul render action using a reusable style.
        /// </summary>
        /// <param name="render">The render action used to produce pane content.</param>
        /// <param name="title">Optional title rendered above the pane content.</param>
        /// <param name="style">Optional style used for the captured pane content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        /// <returns>The added pane.</returns>
        public LayoutPane Add(Action render, string title, Style style, int? captureWidth = null, int captureHeight = 200)
        {
            return Add(RenderActionContent.Capture(render, captureWidth ?? _options.Width, captureHeight), title, style);
        }

        /// <summary>
        /// Adds a pane whose content is captured from a Consoul renderable component.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce pane content.</param>
        /// <param name="title">Optional title rendered above the pane content.</param>
        /// <param name="color">Optional foreground color used for the captured pane content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The added pane.</returns>
        public LayoutPane Add(IConsoulRenderable renderable, string title = null, ConsoleColor? color = null, int? captureWidth = null, int captureHeight = 200)
        {
            return Add(RenderActionContent.Capture(renderable, captureWidth ?? _options.Width, captureHeight), title, color);
        }

        /// <summary>
        /// Adds a pane whose content is captured from a Consoul renderable component using Spectre-style style text.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce pane content.</param>
        /// <param name="title">Optional title rendered above the pane content.</param>
        /// <param name="style">Optional style used for the captured pane content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The added pane.</returns>
        public LayoutPane Add(IConsoulRenderable renderable, string title, string style, int? captureWidth = null, int captureHeight = 200)
        {
            return Add(RenderActionContent.Capture(renderable, captureWidth ?? _options.Width, captureHeight), title, style);
        }

        /// <summary>
        /// Adds a pane whose content is captured from a Consoul renderable component using a reusable style.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce pane content.</param>
        /// <param name="title">Optional title rendered above the pane content.</param>
        /// <param name="style">Optional style used for the captured pane content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The added pane.</returns>
        public LayoutPane Add(IConsoulRenderable renderable, string title, Style style, int? captureWidth = null, int captureHeight = 200)
        {
            return Add(RenderActionContent.Capture(renderable, captureWidth ?? _options.Width, captureHeight), title, style);
        }

        /// <summary>
        /// Adds a configured pane to the layout.
        /// </summary>
        /// <param name="pane">The pane to add.</param>
        /// <returns>The added pane.</returns>
        public LayoutPane Add(LayoutPane pane)
        {
            if (pane == null)
            {
                throw new ArgumentNullException(nameof(pane));
            }

            _panes.Add(pane);
            return pane;
        }

        /// <summary>
        /// Adds panes projected from a model collection.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into panes.</param>
        /// <param name="contentSelector">Selects the pane content rendered for each model item.</param>
        /// <param name="titleSelector">Optionally selects the title rendered above each projected pane.</param>
        /// <param name="colorSelector">Optionally selects the foreground color used for each projected pane.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing render actions or renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing render actions or renderable content.</param>
        /// <returns>The current <see cref="LayoutEntry"/> instance.</returns>
        public LayoutEntry AddPanes<T>(
            IEnumerable<T> items,
            Func<T, object> contentSelector,
            Func<T, string> titleSelector = null,
            Func<T, ConsoleColor?> colorSelector = null,
            int? captureWidth = null,
            int captureHeight = 200)
        {
            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddProjectedPane(
                    contentSelector(item),
                    titleSelector != null ? titleSelector(item) : null,
                    colorSelector != null ? colorSelector(item) : null,
                    captureWidth,
                    captureHeight);
            }

            return this;
        }

        /// <summary>
        /// Adds panes projected from a model collection with Spectre-style pane colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into panes.</param>
        /// <param name="contentSelector">Selects the pane content rendered for each model item.</param>
        /// <param name="titleSelector">Optionally selects the title rendered above each projected pane.</param>
        /// <param name="colorSelector">Selects the foreground color used for each projected pane.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing render actions or renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing render actions or renderable content.</param>
        /// <returns>The current <see cref="LayoutEntry"/> instance.</returns>
        public LayoutEntry AddPanes<T>(
            IEnumerable<T> items,
            Func<T, object> contentSelector,
            Func<T, string> titleSelector,
            Func<T, SpectreColor> colorSelector,
            int? captureWidth = null,
            int captureHeight = 200)
        {
            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
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
                AddProjectedPane(
                    contentSelector(item),
                    titleSelector != null ? titleSelector(item) : null,
                    colorSelector(item).ToConsoleColor(),
                    captureWidth,
                    captureHeight);
            }

            return this;
        }

        /// <summary>
        /// Adds panes projected from a model collection with RGB pane colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into panes.</param>
        /// <param name="contentSelector">Selects the pane content rendered for each model item.</param>
        /// <param name="titleSelector">Optionally selects the title rendered above each projected pane.</param>
        /// <param name="colorSelector">Selects the foreground color used for each projected pane.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing render actions or renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing render actions or renderable content.</param>
        /// <returns>The current <see cref="LayoutEntry"/> instance.</returns>
        public LayoutEntry AddPanes<T>(
            IEnumerable<T> items,
            Func<T, object> contentSelector,
            Func<T, string> titleSelector,
            Func<T, RgbColor> colorSelector,
            int? captureWidth = null,
            int captureHeight = 200)
        {
            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
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
                AddProjectedPane(
                    contentSelector(item),
                    titleSelector != null ? titleSelector(item) : null,
                    colorSelector(item).ToConsoleColor(),
                    captureWidth,
                    captureHeight);
            }

            return this;
        }

        /// <summary>
        /// Adds panes projected from a model collection using Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into panes.</param>
        /// <param name="contentSelector">Selects the pane content rendered for each model item.</param>
        /// <param name="titleSelector">Optionally selects the title rendered above each projected pane.</param>
        /// <param name="styleSelector">Optionally selects style text such as <c>green</c>, <c>#22c55e</c>, or <c>white on blue</c> for each projected pane.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing render actions or renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing render actions or renderable content.</param>
        /// <returns>The current <see cref="LayoutEntry"/> instance.</returns>
        public LayoutEntry AddPanes<T>(
            IEnumerable<T> items,
            Func<T, object> contentSelector,
            Func<T, string> titleSelector,
            Func<T, string> styleSelector,
            int? captureWidth = null,
            int captureHeight = 200)
        {
            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddProjectedPane(
                    contentSelector(item),
                    titleSelector != null ? titleSelector(item) : null,
                    styleSelector != null ? styleSelector(item) : null,
                    captureWidth,
                    captureHeight);
            }

            return this;
        }

        /// <summary>
        /// Renders the layout to the active console driver.
        /// </summary>
        public void Render()
        {
            List<LayoutPane> visiblePanes = _panes.Where(pane => pane != null && pane.IsVisible).ToList();
            if (visiblePanes.Count == 0)
            {
                return;
            }

            int width = PanelEntry.ResolveWidth(_options.Width);
            if (_options.Direction == LayoutDirection.Rows)
            {
                RenderRows(visiblePanes, width);
                return;
            }

            RenderColumns(visiblePanes, width);
        }

        /// <summary>
        /// Renders temporary layout panes to the active console driver.
        /// </summary>
        /// <param name="panes">Panes to render.</param>
        /// <param name="options">Optional layout rendering options.</param>
        public static void Render(IEnumerable<LayoutPane> panes, LayoutOptions options = null)
        {
            new LayoutEntry(panes, options).Render();
        }

        private void RenderRows(IList<LayoutPane> panes, int width)
        {
            int rowSpacing = _options.ResolveRowSpacing();
            for (int paneIndex = 0; paneIndex < panes.Count; paneIndex++)
            {
                foreach (LayoutRenderedLine line in BuildPaneLines(panes[paneIndex], width))
                {
                    MarkupRenderText.WriteSegments(PadOrTrimSegments(line.Segments, width, line.Scheme), line.Scheme, true);
                }

                if (paneIndex < panes.Count - 1)
                {
                    WriteBlankLines(rowSpacing);
                }
            }
        }

        private void RenderColumns(IList<LayoutPane> panes, int width)
        {
            int columns = ResolveColumnCount(panes, width);
            int gutter = columns > 1 ? _options.ResolveGutter() : 0;

            for (int start = 0; start < panes.Count; start += columns)
            {
                int rowPaneCount = Math.Min(columns, panes.Count - start);
                LayoutPane[] rowPanes = panes.Skip(start).Take(rowPaneCount).ToArray();
                int[] paneWidths = ResolvePaneWidths(rowPanes, width, gutter);
                List<List<LayoutRenderedLine>> renderedPanes = new List<List<LayoutRenderedLine>>();
                for (int offset = 0; offset < rowPaneCount; offset++)
                {
                    renderedPanes.Add(BuildPaneLines(rowPanes[offset], paneWidths[offset]));
                }

                int rowHeight = renderedPanes.Count == 0 ? 0 : renderedPanes.Max(lines => lines.Count);
                for (int lineIndex = 0; lineIndex < rowHeight; lineIndex++)
                {
                    for (int paneIndex = 0; paneIndex < rowPaneCount; paneIndex++)
                    {
                        LayoutRenderedLine line = lineIndex < renderedPanes[paneIndex].Count
                            ? renderedPanes[paneIndex][lineIndex]
                            : new LayoutRenderedLine(Array.Empty<FixedMessageSegment>(), _options.ResolveContentScheme());

                        bool isLastPane = paneIndex == rowPaneCount - 1;
                        MarkupRenderText.WriteSegments(PadOrTrimSegments(line.Segments, paneWidths[paneIndex], line.Scheme), line.Scheme, isLastPane);

                        if (!isLastPane && gutter > 0)
                        {
                            Consoul.Write(new string(' ', gutter), RenderOptions.DefaultScheme, false);
                        }
                    }
                }

                if (start + columns < panes.Count)
                {
                    WriteBlankLines(_options.ResolveRowSpacing());
                }
            }
        }

        private int ResolveColumnCount(IList<LayoutPane> panes, int width)
        {
            int gutter = _options.ResolveGutter();
            int minimumPaneWidth = Math.Max(_options.ResolveMinimumPaneWidth(), panes.Max(ResolveMinimumSize));
            int preferredColumns = _options.Columns.HasValue
                ? Math.Max(1, _options.Columns.Value)
                : panes.Count;
            int maximumByWidth = Math.Max(1, (width + gutter) / (minimumPaneWidth + gutter));
            return Math.Max(1, Math.Min(Math.Min(preferredColumns, panes.Count), maximumByWidth));
        }

        private int[] ResolvePaneWidths(IList<LayoutPane> panes, int width, int gutter)
        {
            int count = panes.Count;
            int available = Math.Max(count, width - (gutter * Math.Max(0, count - 1)));
            int[] widths = new int[count];
            int fixedTotal = 0;
            int flexibleCount = 0;

            for (int index = 0; index < count; index++)
            {
                if (panes[index].SizeValue.HasValue)
                {
                    widths[index] = Math.Max(1, panes[index].SizeValue.Value);
                    fixedTotal += widths[index];
                }
                else
                {
                    flexibleCount++;
                }
            }

            if (fixedTotal >= available || flexibleCount == 0)
            {
                return ClampWidths(widths, panes, available);
            }

            int remaining = available - fixedTotal;
            int ratioTotal = panes
                .Where(pane => !pane.SizeValue.HasValue)
                .Sum(pane => Math.Max(1, pane.RatioValue));

            int assignedFlexible = 0;
            for (int index = 0; index < count; index++)
            {
                if (panes[index].SizeValue.HasValue)
                {
                    continue;
                }

                int ratio = Math.Max(1, panes[index].RatioValue);
                widths[index] = Math.Max(1, (remaining * ratio) / Math.Max(1, ratioTotal));
                assignedFlexible += widths[index];
            }

            int remainder = remaining - assignedFlexible;
            for (int index = 0; remainder > 0 && index < count; index++)
            {
                if (!panes[index].SizeValue.HasValue)
                {
                    widths[index]++;
                    remainder--;
                }
            }

            return ClampWidths(widths, panes, available);
        }

        private int[] ClampWidths(int[] widths, IList<LayoutPane> panes, int available)
        {
            int count = widths.Length;
            int remaining = available;
            for (int index = 0; index < count; index++)
            {
                int panesLeft = count - index - 1;
                int requested = widths[index] > 0 ? widths[index] : ResolveMinimumSize(panes[index]);
                int maximum = Math.Max(1, remaining - panesLeft);
                widths[index] = Math.Max(1, Math.Min(requested, maximum));
                remaining -= widths[index];
            }

            return widths;
        }

        private int ResolveMinimumSize(LayoutPane pane)
        {
            return Math.Max(1, pane.MinimumSizeValue ?? _options.ResolveMinimumPaneWidth());
        }

        private List<LayoutRenderedLine> BuildPaneLines(LayoutPane pane, int width)
        {
            var lines = new List<LayoutRenderedLine>();
            if (!string.IsNullOrWhiteSpace(pane.Title))
            {
                ColorScheme titleScheme = _options.ResolveTitleScheme();
                foreach (List<FixedMessageSegment> line in MarkupRenderText.WrapSegments(pane.Title.Trim(), width, titleScheme))
                {
                    lines.Add(new LayoutRenderedLine(line, titleScheme));
                }
            }

            ColorScheme contentScheme = pane.Scheme ?? _options.ResolveContentScheme();
            foreach (List<FixedMessageSegment> line in MarkupRenderText.WrapSegments(pane.ResolveContent(width), width, contentScheme))
            {
                lines.Add(new LayoutRenderedLine(line, contentScheme));
            }

            if (lines.Count == 0)
            {
                lines.Add(new LayoutRenderedLine(Array.Empty<FixedMessageSegment>(), contentScheme));
            }

            return lines;
        }

        private static void WriteBlankLines(int count)
        {
            for (int index = 0; index < count; index++)
            {
                Consoul.Write(string.Empty);
            }
        }

        private static List<FixedMessageSegment> PadOrTrimSegments(IList<FixedMessageSegment> segments, int width, ColorScheme scheme)
        {
            List<FixedMessageSegment> result = MarkupRenderText.TruncateSegments(segments, Math.Max(0, width));
            MarkupRenderText.AddPadding(result, Math.Max(0, width - MarkupRenderText.GetLength(result)), scheme);
            return result;
        }

        private LayoutPane AddProjectedPane(object content, string title, ConsoleColor? color, int? captureWidth, int captureHeight)
        {
            var pane = content as LayoutPane;
            if (pane != null)
            {
                return Add(pane);
            }

            var layout = content as Layout;
            if (layout != null)
            {
                return Add(CreateProjectedLayoutPane(layout, title, color, captureWidth, captureHeight));
            }

            var renderable = content as IConsoulRenderable;
            if (renderable != null)
            {
                return Add(renderable, title, color, captureWidth, captureHeight);
            }

            var render = content as Action;
            if (render != null)
            {
                return Add(render, title, color, captureWidth, captureHeight);
            }

            return Add(RenderActionContent.FormatValue(content), title, color);
        }

        private LayoutPane AddProjectedPane(object content, string title, string style, int? captureWidth, int captureHeight)
        {
            var pane = content as LayoutPane;
            if (pane != null)
            {
                return Add(pane);
            }

            var layout = content as Layout;
            if (layout != null)
            {
                return Add(CreateProjectedLayoutPane(layout, title, style, captureWidth, captureHeight));
            }

            var renderable = content as IConsoulRenderable;
            if (renderable != null)
            {
                return Add(renderable, title, style, captureWidth, captureHeight);
            }

            var render = content as Action;
            if (render != null)
            {
                return Add(render, title, style, captureWidth, captureHeight);
            }

            return Add(RenderActionContent.FormatValue(content), title, style);
        }

        private static LayoutPane CreateProjectedLayoutPane(Layout layout, string title, ConsoleColor? color, int? captureWidth, int captureHeight)
        {
            return LayoutPane.CreateLive(layout, title ?? layout.Name, color, captureWidth, captureHeight)
                .Size(layout.PaneSize)
                .Ratio(layout.PaneRatio)
                .MinimumSize(layout.PaneMinimumSize)
                .Visible(layout.PaneVisible);
        }

        private static LayoutPane CreateProjectedLayoutPane(Layout layout, string title, string style, int? captureWidth, int captureHeight)
        {
            ColorScheme? scheme = string.IsNullOrWhiteSpace(style)
                ? (ColorScheme?)null
                : Style.Parse(style).ToColorScheme(RenderOptions.DefaultScheme);

            return LayoutPane.CreateLive(layout, title ?? layout.Name, scheme, captureWidth, captureHeight)
                .Size(layout.PaneSize)
                .Ratio(layout.PaneRatio)
                .MinimumSize(layout.PaneMinimumSize)
                .Visible(layout.PaneVisible);
        }

        private readonly struct LayoutRenderedLine
        {
            public LayoutRenderedLine(IEnumerable<FixedMessageSegment> segments, ColorScheme scheme)
            {
                Segments = segments == null
                    ? new List<FixedMessageSegment>()
                    : new List<FixedMessageSegment>(segments);
                Scheme = scheme;
            }

            public IList<FixedMessageSegment> Segments { get; }

            public ColorScheme Scheme { get; }
        }
    }
}
