using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders a responsive shortcut bar for available view actions and keyboard hints.
    /// </summary>
    public class ShortcutBarEntry : IRenderable
    {
        private readonly List<ShortcutItem> _items = new List<ShortcutItem>();
        private readonly ShortcutBarOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortcutBarEntry"/> class.
        /// </summary>
        /// <param name="options">Optional rendering options for the shortcut bar.</param>
        public ShortcutBarEntry(ShortcutBarOptions options = null)
        {
            _options = options != null ? options.Clone() : new ShortcutBarOptions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortcutBarEntry"/> class with items.
        /// </summary>
        /// <param name="items">The shortcut items to render.</param>
        /// <param name="options">Optional rendering options for the shortcut bar.</param>
        public ShortcutBarEntry(IEnumerable<ShortcutItem> items, ShortcutBarOptions options = null) : this(options)
        {
            if (items == null)
            {
                return;
            }

            foreach (ShortcutItem item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Gets the shortcut items rendered by the shortcut bar.
        /// </summary>
        public IList<ShortcutItem> Items => _items;

        /// <summary>
        /// Gets the options used to render the shortcut bar.
        /// </summary>
        public ShortcutBarOptions Options => _options;

        /// <summary>
        /// Adds a shortcut item.
        /// </summary>
        /// <param name="key">The key or command token.</param>
        /// <param name="description">The action description.</param>
        /// <param name="disabled">A value indicating whether the shortcut should render as disabled.</param>
        /// <param name="keyColor">Optional foreground color for the key token.</param>
        /// <param name="descriptionColor">Optional foreground color for the action description.</param>
        /// <returns>The added shortcut item.</returns>
        public ShortcutItem Add(
            string key,
            string description,
            bool disabled = false,
            ConsoleColor? keyColor = null,
            ConsoleColor? descriptionColor = null)
        {
            ShortcutItem item = ShortcutItem.Create(key, description, disabled, keyColor, descriptionColor);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds a shortcut item using reusable style objects.
        /// </summary>
        /// <param name="key">The key or command token.</param>
        /// <param name="description">The action description.</param>
        /// <param name="keyStyle">Optional style for the key token.</param>
        /// <param name="descriptionStyle">Optional style for the action description.</param>
        /// <param name="disabled">A value indicating whether the shortcut should render as disabled.</param>
        /// <returns>The added shortcut item.</returns>
        public ShortcutItem Add(
            string key,
            string description,
            Style keyStyle,
            Style descriptionStyle = null,
            bool disabled = false)
        {
            ShortcutItem item = ShortcutItem.Create(key, description, keyStyle, descriptionStyle, disabled);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds a shortcut item using Spectre-style style text.
        /// </summary>
        /// <param name="key">The key or command token.</param>
        /// <param name="description">The action description.</param>
        /// <param name="keyStyle">Optional style for the key token, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="descriptionStyle">Optional style for the action description, such as <c>grey</c>.</param>
        /// <param name="disabled">A value indicating whether the shortcut should render as disabled.</param>
        /// <returns>The added shortcut item.</returns>
        public ShortcutItem Add(
            string key,
            string description,
            string keyStyle,
            string descriptionStyle = null,
            bool disabled = false)
        {
            return AddStyled(key, description, disabled, keyStyle, descriptionStyle);
        }

        /// <summary>
        /// Adds a shortcut item using Spectre-style style text.
        /// </summary>
        /// <param name="key">The key or command token.</param>
        /// <param name="description">The action description.</param>
        /// <param name="disabled">A value indicating whether the shortcut should render as disabled.</param>
        /// <param name="keyStyle">Optional style for the key token, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="descriptionStyle">Optional style for the action description, such as <c>grey</c>.</param>
        /// <returns>The added shortcut item.</returns>
        public ShortcutItem AddStyled(
            string key,
            string description,
            bool disabled = false,
            string keyStyle = null,
            string descriptionStyle = null)
        {
            ShortcutItem item = ShortcutItem.CreateStyled(key, description, disabled, keyStyle, descriptionStyle);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds a shortcut item.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The added shortcut item.</returns>
        public ShortcutItem Add(ShortcutItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Renders the shortcut bar to the active console driver.
        /// </summary>
        public void Render()
        {
            foreach (IList<FixedMessageSegment> line in BuildLines(ResolveWidth()))
            {
                foreach (FixedMessageSegment segment in line)
                {
                    TerminalLinkWriter.WriteText(segment.Text, segment.Scheme ?? RenderOptions.DefaultScheme, segment.Link);
                }

                Consoul.Write(string.Empty, RenderOptions.DefaultScheme, true);
            }
        }

        /// <summary>
        /// Creates a plain text representation of the shortcut bar.
        /// </summary>
        /// <param name="width">Optional maximum width used instead of the active terminal width.</param>
        /// <returns>A bounded shortcut bar string.</returns>
        public string ToText(int? width = null)
        {
            return string.Join(Environment.NewLine, BuildLines(ResolveWidth(width)).Select(line => string.Concat(line.Select(segment => segment.Text))));
        }

        /// <summary>
        /// Renders a temporary shortcut bar to the active console driver.
        /// </summary>
        /// <param name="items">The shortcut items to render.</param>
        /// <param name="options">Optional rendering options for the shortcut bar.</param>
        public static void Render(IEnumerable<ShortcutItem> items, ShortcutBarOptions options = null)
        {
            new ShortcutBarEntry(items, options).Render();
        }

        /// <summary>
        /// Creates plain shortcut bar text without writing to the console.
        /// </summary>
        /// <param name="items">The shortcut items.</param>
        /// <param name="options">Optional rendering options for the shortcut bar.</param>
        /// <param name="width">Optional maximum width used instead of the active terminal width.</param>
        /// <returns>A bounded shortcut bar string.</returns>
        public static string Text(IEnumerable<ShortcutItem> items, ShortcutBarOptions options = null, int? width = null)
        {
            return new ShortcutBarEntry(items, options).ToText(width);
        }

        private IList<IList<FixedMessageSegment>> BuildLines(int width)
        {
            width = Math.Max(1, width);
            var lines = new List<IList<FixedMessageSegment>>();
            var current = new List<FixedMessageSegment>();
            int currentLength = 0;
            string separator = _options.ItemSeparator ?? string.Empty;

            foreach (ShortcutItem item in _items.Where(item => item != null && (_options.ShowDisabled || !item.Disabled)))
            {
                IList<FixedMessageSegment> itemSegments = BuildItemSegments(item, width);
                int itemLength = Measure(itemSegments);
                if (itemLength == 0)
                {
                    continue;
                }

                bool needsSeparator = current.Count > 0 && separator.Length > 0;
                int nextLength = currentLength + (needsSeparator ? separator.Length : 0) + itemLength;
                if (current.Count > 0 && nextLength > width)
                {
                    lines.Add(current);
                    current = new List<FixedMessageSegment>();
                    currentLength = 0;
                    needsSeparator = false;
                }

                if (needsSeparator)
                {
                    current.Add(new FixedMessageSegment(separator, _options.ResolveSeparatorScheme()));
                    currentLength += separator.Length;
                }

                foreach (FixedMessageSegment segment in itemSegments)
                {
                    current.Add(segment);
                    currentLength += segment.Text.Length;
                }
            }

            if (current.Count > 0)
            {
                lines.Add(current);
            }

            return lines;
        }

        private IList<FixedMessageSegment> BuildItemSegments(ShortcutItem item, int maxWidth)
        {
            string key = Normalize(item.Key);
            string description = Normalize(item.Description);
            string gap = _options.KeyDescriptionSeparator ?? string.Empty;
            ColorScheme keyScheme = item.Disabled ? _options.ResolveDisabledScheme() : item.KeyScheme ?? _options.ResolveKeyScheme();
            ColorScheme descriptionScheme = item.Disabled ? _options.ResolveDisabledScheme() : item.DescriptionScheme ?? _options.ResolveDescriptionScheme();
            bool forceScheme = item.Disabled;
            IList<FixedMessageSegment> keySegments = BuildKeySegments(key, keyScheme, forceScheme);
            IList<FixedMessageSegment> descriptionSegments = ParseMarkupSegments(description, descriptionScheme, forceScheme);

            int descriptionWidth = maxWidth - Measure(keySegments) - gap.Length;
            if (descriptionWidth >= 1)
            {
                var segments = new List<FixedMessageSegment>();
                segments.AddRange(keySegments);
                AddPlainSegment(segments, gap, RenderOptions.DefaultScheme);
                segments.AddRange(MarkupRenderText.TruncateSegments(descriptionSegments, descriptionWidth, _options.Ellipsis));
                return segments;
            }

            var combined = new List<FixedMessageSegment>();
            combined.AddRange(keySegments);
            AddPlainSegment(combined, gap, item.Disabled ? _options.ResolveDisabledScheme() : RenderOptions.DefaultScheme);
            combined.AddRange(descriptionSegments);
            return MarkupRenderText.TruncateSegments(combined, maxWidth, _options.Ellipsis);
        }

        private int ResolveWidth(int? width = null)
        {
            int terminalWidth = Math.Max(1, Consoul.ConsoleBufferWidth - 1);
            int requested = width ?? _options.Width ?? terminalWidth;
            return Math.Max(1, Math.Min(requested, terminalWidth));
        }

        private static int Measure(IEnumerable<FixedMessageSegment> segments)
        {
            return MarkupRenderText.GetLength(segments);
        }

        private static string Normalize(string value)
        {
            return (value ?? string.Empty)
                .Replace("\r\n", " ")
                .Replace('\r', ' ')
                .Replace('\n', ' ')
                .Trim();
        }

        private IList<FixedMessageSegment> BuildKeySegments(string key, ColorScheme keyScheme, bool forceScheme)
        {
            var segments = new List<FixedMessageSegment>();
            AddPlainSegment(segments, _options.KeyLeftEdge, keyScheme);
            AddSegments(segments, ParseMarkupSegments(key, keyScheme, forceScheme));
            AddPlainSegment(segments, _options.KeyRightEdge, keyScheme);
            return segments;
        }

        private static IList<FixedMessageSegment> ParseMarkupSegments(string value, ColorScheme scheme, bool forceScheme)
        {
            var result = new List<FixedMessageSegment>();
            foreach (FixedMessageSegment segment in MarkupRenderText.ParseSegments(value, scheme))
            {
                AddSegment(
                    result,
                    segment.Text,
                    forceScheme ? scheme : segment.Scheme ?? scheme,
                    segment.Link);
            }

            return result;
        }

        private static void AddPlainSegment(IList<FixedMessageSegment> segments, string text, ColorScheme scheme)
        {
            AddSegment(segments, text, scheme);
        }

        private static void AddSegments(IList<FixedMessageSegment> segments, IEnumerable<FixedMessageSegment> values)
        {
            foreach (FixedMessageSegment segment in values ?? Enumerable.Empty<FixedMessageSegment>())
            {
                AddSegment(segments, segment.Text, segment.Scheme ?? RenderOptions.DefaultScheme, segment.Link);
            }
        }

        private static void AddSegment(IList<FixedMessageSegment> segments, string text, ColorScheme scheme)
        {
            AddSegment(segments, text, scheme, null);
        }

        private static void AddSegment(IList<FixedMessageSegment> segments, string text, ColorScheme scheme, Link link)
        {
            text = text ?? string.Empty;
            if (text.Length == 0)
            {
                return;
            }

            if (segments.Count > 0 &&
                SameScheme(segments[segments.Count - 1].Scheme, scheme) &&
                TerminalLinkWriter.SameTarget(segments[segments.Count - 1].Link, link))
            {
                FixedMessageSegment previous = segments[segments.Count - 1];
                segments[segments.Count - 1] = new FixedMessageSegment(previous.Text + text, scheme, link);
                return;
            }

            segments.Add(new FixedMessageSegment(text, scheme, link));
        }

        private static bool SameScheme(ColorScheme? left, ColorScheme right)
        {
            return left.HasValue &&
                   left.Value.Color == right.Color &&
                   left.Value.BackgroundColor == right.BackgroundColor;
        }
    }
}
