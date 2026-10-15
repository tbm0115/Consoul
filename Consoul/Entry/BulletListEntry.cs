using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders responsive bullet or numbered lists for grouped notes, requirements, and dashboard details.
    /// </summary>
    public class BulletListEntry : IRenderable
    {
        private readonly List<BulletListItem> _items = new List<BulletListItem>();
        private readonly BulletListOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="BulletListEntry"/> class.
        /// </summary>
        /// <param name="options">Optional rendering options for the bullet list.</param>
        public BulletListEntry(BulletListOptions options = null)
        {
            _options = options != null ? options.Clone() : new BulletListOptions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BulletListEntry"/> class with items.
        /// </summary>
        /// <param name="items">The list items to render.</param>
        /// <param name="options">Optional rendering options for the bullet list.</param>
        public BulletListEntry(IEnumerable<BulletListItem> items, BulletListOptions options = null) : this(options)
        {
            if (items == null)
            {
                return;
            }

            foreach (BulletListItem item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Gets the items rendered by the bullet list.
        /// </summary>
        public IList<BulletListItem> Items => _items;

        /// <summary>
        /// Gets the options used to render the bullet list.
        /// </summary>
        public BulletListOptions Options => _options;

        /// <summary>
        /// Adds an item to the list.
        /// </summary>
        /// <param name="text">The item text.</param>
        /// <param name="textColor">Optional foreground color used for item text.</param>
        /// <param name="markerColor">Optional foreground color used for the item marker.</param>
        /// <returns>The added list item.</returns>
        public BulletListItem Add(string text, ConsoleColor? textColor = null, ConsoleColor? markerColor = null)
        {
            BulletListItem item = BulletListItem.Create(text, textColor, markerColor);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds an item to the list using reusable style objects.
        /// </summary>
        /// <param name="text">The item text.</param>
        /// <param name="textStyle">Optional style used for item text.</param>
        /// <param name="markerStyle">Optional style used for the item marker.</param>
        /// <returns>The added list item.</returns>
        public BulletListItem Add(string text, Style textStyle, Style markerStyle = null)
        {
            BulletListItem item = BulletListItem.Create(text, textStyle, markerStyle);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds an item to the list using Spectre-style style text.
        /// </summary>
        /// <param name="text">The item text.</param>
        /// <param name="textStyle">Optional style used for item text, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="markerStyle">Optional style used for the item marker, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <returns>The added list item.</returns>
        public BulletListItem Add(string text, string textStyle, string markerStyle = null)
        {
            return AddStyled(text, textStyle, markerStyle);
        }

        /// <summary>
        /// Adds an item to the list using Spectre-style style text.
        /// </summary>
        /// <param name="text">The item text.</param>
        /// <param name="textStyle">Optional style used for item text, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="markerStyle">Optional style used for the item marker, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <returns>The added list item.</returns>
        public BulletListItem AddStyled(string text, string textStyle = null, string markerStyle = null)
        {
            BulletListItem item = BulletListItem.Create(text, textStyle, markerStyle);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds an item to the list.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The added list item.</returns>
        public BulletListItem Add(BulletListItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Renders the bullet list to the active console driver.
        /// </summary>
        public void Render()
        {
            if (_items.Count == 0)
            {
                return;
            }

            int width = PanelEntry.ResolveWidth(_options.Width);
            RenderItems(_items, 0, width);
        }

        /// <summary>
        /// Renders a temporary bullet list to the active console driver.
        /// </summary>
        /// <param name="items">The list items to render.</param>
        /// <param name="options">Optional rendering options for the bullet list.</param>
        public static void Render(IEnumerable<BulletListItem> items, BulletListOptions options = null)
        {
            new BulletListEntry(items, options).Render();
        }

        private void RenderItems(IList<BulletListItem> items, int level, int width)
        {
            if (items == null)
            {
                return;
            }

            for (int index = 0; index < items.Count; index++)
            {
                BulletListItem item = items[index];
                if (item == null)
                {
                    continue;
                }

                RenderItem(item, level, index, width);
                RenderItems(item.Children, level + 1, width);
            }
        }

        private void RenderItem(BulletListItem item, int level, int index, int width)
        {
            string marker = _options.ResolveMarker(level, index);
            int indent = Math.Max(0, _options.IndentSize) * Math.Max(0, level);
            string firstPrefix = new string(' ', indent) + marker + new string(' ', Math.Max(0, _options.MarkerGap));
            string continuationPrefix = new string(' ', firstPrefix.Length);
            ColorScheme markerScheme = item.MarkerScheme ?? _options.ResolveMarkerScheme();
            ColorScheme textScheme = item.TextScheme ?? _options.ResolveTextScheme();
            Decoration textDecoration = item.TextDecoration ?? (item.TextScheme.HasValue ? Decoration.None : _options.TextDecoration);
            RgbColor? textForegroundRgb = item.TextForegroundRgb ?? (item.TextScheme.HasValue ? null : _options.TextForegroundRgb);
            RgbColor? textBackgroundRgb = item.TextBackgroundRgb ?? (item.TextScheme.HasValue ? null : _options.TextBackgroundRgb);
            Decoration markerDecoration = item.MarkerDecoration ?? (item.MarkerScheme.HasValue ? Decoration.None : _options.MarkerDecoration);
            RgbColor? markerForegroundRgb = item.MarkerForegroundRgb ?? (item.MarkerScheme.HasValue ? null : _options.MarkerForegroundRgb);
            RgbColor? markerBackgroundRgb = item.MarkerBackgroundRgb ?? (item.MarkerScheme.HasValue ? null : _options.MarkerBackgroundRgb);
            IList<List<FixedMessageSegment>> lines = MarkupRenderText.WrapSegments(
                item.Text,
                Math.Max(1, width - Math.Min(firstPrefix.Length, width - 1)),
                textScheme,
                textDecoration,
                textForegroundRgb,
                textBackgroundRgb);

            for (int lineIndex = 0; lineIndex < lines.Count; lineIndex++)
            {
                bool firstLine = lineIndex == 0;
                string prefix = firstLine ? firstPrefix : continuationPrefix;
                WriteLine(
                    prefix,
                    lines[lineIndex],
                    firstLine ? markerScheme : RenderOptions.DefaultScheme,
                    textScheme,
                    width,
                    firstLine ? markerDecoration : Decoration.None,
                    firstLine ? markerForegroundRgb : null,
                    firstLine ? markerBackgroundRgb : null);
            }
        }

        private static void WriteLine(
            string prefix,
            IList<FixedMessageSegment> text,
            ColorScheme prefixScheme,
            ColorScheme textScheme,
            int width,
            Decoration prefixDecoration,
            RgbColor? prefixForegroundRgb,
            RgbColor? prefixBackgroundRgb)
        {
            string visiblePrefix = ClipPrefix(prefix, width);
            int contentWidth = Math.Max(0, width - visiblePrefix.Length);
            List<FixedMessageSegment> visibleText = MarkupRenderText.TruncateSegments(text, contentWidth);

            if (visiblePrefix.Length > 0)
            {
                TerminalLinkWriter.WriteText(
                    visiblePrefix,
                    prefixScheme,
                    null,
                    prefixDecoration,
                    prefixForegroundRgb,
                    prefixBackgroundRgb);
            }

            MarkupRenderText.WriteSegments(visibleText, textScheme, true);
        }

        private static string ClipPrefix(string prefix, int width)
        {
            prefix = prefix ?? string.Empty;
            int maximumPrefixLength = Math.Max(0, width - 1);
            return prefix.Length > maximumPrefixLength
                ? prefix.Substring(0, maximumPrefixLength)
                : prefix;
        }
    }
}
