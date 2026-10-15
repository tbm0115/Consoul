using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders a responsive checklist for workflow state, validation results, and proof-of-concept dashboards.
    /// </summary>
    public class ChecklistEntry : IRenderable
    {
        private readonly List<ChecklistItem> _items = new List<ChecklistItem>();
        private readonly ChecklistOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChecklistEntry"/> class.
        /// </summary>
        /// <param name="options">Optional rendering options for the checklist.</param>
        public ChecklistEntry(ChecklistOptions options = null)
        {
            _options = options != null ? options.Clone() : new ChecklistOptions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChecklistEntry"/> class with items.
        /// </summary>
        /// <param name="items">The checklist items to render.</param>
        /// <param name="options">Optional rendering options for the checklist.</param>
        public ChecklistEntry(IEnumerable<ChecklistItem> items, ChecklistOptions options = null) : this(options)
        {
            if (items == null)
            {
                return;
            }

            foreach (ChecklistItem item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Gets the items rendered by the checklist.
        /// </summary>
        public IList<ChecklistItem> Items => _items;

        /// <summary>
        /// Gets the options used to render the checklist.
        /// </summary>
        public ChecklistOptions Options => _options;

        /// <summary>
        /// Adds an item to the checklist.
        /// </summary>
        /// <param name="label">The item label.</param>
        /// <param name="state">The state rendered for the item.</param>
        /// <param name="description">Optional descriptive text rendered under the item label.</param>
        /// <param name="labelColor">Optional foreground color used for the item label.</param>
        /// <param name="markerColor">Optional foreground color used for the item marker.</param>
        /// <param name="descriptionColor">Optional foreground color used for the item description.</param>
        /// <returns>The added checklist item.</returns>
        public ChecklistItem Add(
            string label,
            ChecklistItemState state = ChecklistItemState.Pending,
            string description = null,
            ConsoleColor? labelColor = null,
            ConsoleColor? markerColor = null,
            ConsoleColor? descriptionColor = null)
        {
            ChecklistItem item = ChecklistItem.Create(label, state, description, labelColor, markerColor, descriptionColor);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds an item to the checklist using reusable style objects.
        /// </summary>
        /// <param name="label">The item label.</param>
        /// <param name="labelStyle">Optional style used for the item label.</param>
        /// <param name="markerStyle">Optional style used for the item marker.</param>
        /// <param name="descriptionStyle">Optional style used for the item description.</param>
        /// <param name="state">The state rendered for the item.</param>
        /// <param name="description">Optional descriptive text rendered under the item label.</param>
        /// <returns>The added checklist item.</returns>
        public ChecklistItem Add(
            string label,
            Style labelStyle,
            Style markerStyle = null,
            Style descriptionStyle = null,
            ChecklistItemState state = ChecklistItemState.Pending,
            string description = null)
        {
            ChecklistItem item = ChecklistItem.Create(label, labelStyle, markerStyle, descriptionStyle, state, description);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds an item to the checklist using Spectre-style style text.
        /// </summary>
        /// <param name="label">The item label.</param>
        /// <param name="labelStyle">Optional style used for the item label, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="markerStyle">Optional style used for the item marker, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <param name="descriptionStyle">Optional style used for the item description, such as <c>darkgray</c>.</param>
        /// <param name="state">The state rendered for the item.</param>
        /// <param name="description">Optional descriptive text rendered under the item label.</param>
        /// <returns>The added checklist item.</returns>
        public ChecklistItem Add(
            string label,
            string labelStyle,
            string markerStyle = null,
            string descriptionStyle = null,
            ChecklistItemState state = ChecklistItemState.Pending,
            string description = null)
        {
            return AddStyled(label, state, description, labelStyle, markerStyle, descriptionStyle);
        }

        /// <summary>
        /// Adds an item to the checklist using Spectre-style style text.
        /// </summary>
        /// <param name="label">The item label.</param>
        /// <param name="state">The state rendered for the item.</param>
        /// <param name="description">Optional descriptive text rendered under the item label.</param>
        /// <param name="labelStyle">Optional style used for the item label, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="markerStyle">Optional style used for the item marker, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <param name="descriptionStyle">Optional style used for the item description, such as <c>darkgray</c>.</param>
        /// <returns>The added checklist item.</returns>
        public ChecklistItem AddStyled(
            string label,
            ChecklistItemState state = ChecklistItemState.Pending,
            string description = null,
            string labelStyle = null,
            string markerStyle = null,
            string descriptionStyle = null)
        {
            ChecklistItem item = ChecklistItem.CreateStyled(label, state, description, labelStyle, markerStyle, descriptionStyle);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds an item to the checklist.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The added checklist item.</returns>
        public ChecklistItem Add(ChecklistItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Renders the checklist to the active console driver.
        /// </summary>
        public void Render()
        {
            if (_items.Count == 0)
            {
                return;
            }

            int width = PanelEntry.ResolveWidth(_options.Width);
            Layout layout = ResolveLayout(width);
            foreach (ChecklistItem item in _items)
            {
                RenderItem(item, layout);
            }
        }

        /// <summary>
        /// Renders a temporary checklist to the active console driver.
        /// </summary>
        /// <param name="items">The checklist items to render.</param>
        /// <param name="options">Optional rendering options for the checklist.</param>
        public static void Render(IEnumerable<ChecklistItem> items, ChecklistOptions options = null)
        {
            new ChecklistEntry(items, options).Render();
        }

        private Layout ResolveLayout(int width)
        {
            int markerWidth = _items
                .Select(item => (_options.ResolveMarker(item.State) ?? string.Empty).Length)
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

        private void RenderItem(ChecklistItem item, Layout layout)
        {
            string marker = PadOrTrim(_options.ResolveMarker(item.State), layout.MarkerWidth);
            ColorScheme markerScheme = item.MarkerScheme ?? _options.ResolveMarkerScheme(item.State);
            ColorScheme labelScheme = item.LabelScheme ?? _options.ResolveLabelScheme();
            ColorScheme descriptionScheme = item.DescriptionScheme ?? _options.ResolveDescriptionScheme();
            Decoration labelDecoration = item.LabelDecoration ?? (item.LabelScheme.HasValue ? Decoration.None : _options.LabelDecoration);
            RgbColor? labelForegroundRgb = item.LabelForegroundRgb ?? (item.LabelScheme.HasValue ? null : _options.LabelForegroundRgb);
            RgbColor? labelBackgroundRgb = item.LabelBackgroundRgb ?? (item.LabelScheme.HasValue ? null : _options.LabelBackgroundRgb);
            Decoration markerDecoration = item.MarkerDecoration ?? (item.MarkerScheme.HasValue ? Decoration.None : _options.ResolveMarkerDecoration(item.State));
            RgbColor? markerForegroundRgb = item.MarkerForegroundRgb ?? (item.MarkerScheme.HasValue ? null : _options.ResolveMarkerForegroundRgb(item.State));
            RgbColor? markerBackgroundRgb = item.MarkerBackgroundRgb ?? (item.MarkerScheme.HasValue ? null : _options.ResolveMarkerBackgroundRgb(item.State));
            Decoration descriptionDecoration = item.DescriptionDecoration ?? (item.DescriptionScheme.HasValue ? Decoration.None : _options.DescriptionDecoration);
            RgbColor? descriptionForegroundRgb = item.DescriptionForegroundRgb ?? (item.DescriptionScheme.HasValue ? null : _options.DescriptionForegroundRgb);
            RgbColor? descriptionBackgroundRgb = item.DescriptionBackgroundRgb ?? (item.DescriptionScheme.HasValue ? null : _options.DescriptionBackgroundRgb);
            IList<List<FixedMessageSegment>> labelLines = MarkupRenderText.WrapSegments(
                item.Label,
                layout.ContentWidth,
                labelScheme,
                labelDecoration,
                labelForegroundRgb,
                labelBackgroundRgb);
            WriteLines(
                marker,
                markerScheme,
                labelLines,
                labelScheme,
                layout,
                markerDecoration,
                markerForegroundRgb,
                markerBackgroundRgb);

            if (_options.ShowDescriptions && !string.IsNullOrWhiteSpace(item.Description))
            {
                IList<List<FixedMessageSegment>> descriptionLines = MarkupRenderText.WrapSegments(
                    item.Description,
                    layout.ContentWidth,
                    descriptionScheme,
                    descriptionDecoration,
                    descriptionForegroundRgb,
                    descriptionBackgroundRgb);
                WriteLines(
                    PadOrTrim(string.Empty, layout.MarkerWidth),
                    RenderOptions.DefaultScheme,
                    descriptionLines,
                    descriptionScheme,
                    layout,
                    Decoration.None,
                    null,
                    null);
            }
        }

        private void WriteLines(
            string marker,
            ColorScheme markerScheme,
            IList<List<FixedMessageSegment>> contentLines,
            ColorScheme contentScheme,
            Layout layout,
            Decoration markerDecoration,
            RgbColor? markerForegroundRgb,
            RgbColor? markerBackgroundRgb)
        {
            for (int index = 0; index < contentLines.Count; index++)
            {
                bool firstLine = index == 0;
                string markerText = firstLine ? marker : PadOrTrim(string.Empty, layout.MarkerWidth);
                string gap = new string(' ', layout.Gap);

                if (!_options.IndentWrappedLines && !firstLine)
                {
                    markerText = string.Empty;
                    gap = string.Empty;
                }

                TerminalLinkWriter.WriteText(
                    markerText,
                    firstLine ? markerScheme : RenderOptions.DefaultScheme,
                    null,
                    firstLine ? markerDecoration : Decoration.None,
                    firstLine ? markerForegroundRgb : null,
                    firstLine ? markerBackgroundRgb : null);
                Consoul.Write(gap, RenderOptions.DefaultScheme, false);
                int contentWidth = Math.Max(1, layout.Width - markerText.Length - gap.Length);
                List<FixedMessageSegment> content = MarkupRenderText.TruncateSegments(contentLines[index], contentWidth);
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
