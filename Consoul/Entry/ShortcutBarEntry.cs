using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;
using System.Linq;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

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
        /// Adds a shortcut item using Spectre-style color facades.
        /// </summary>
        /// <param name="key">The key or command token.</param>
        /// <param name="description">The action description.</param>
        /// <param name="keyColor">Foreground color used for the key token.</param>
        /// <param name="descriptionColor">Optional foreground color used for the action description.</param>
        /// <param name="disabled">A value indicating whether the shortcut should render as disabled.</param>
        /// <returns>The added shortcut item.</returns>
        public ShortcutItem Add(
            string key,
            string description,
            SpectreColor keyColor,
            SpectreColor? descriptionColor = null,
            bool disabled = false)
        {
            ShortcutItem item = ShortcutItem.Create(key, description, keyColor, descriptionColor, disabled);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds a shortcut item using RGB foreground colors.
        /// </summary>
        /// <param name="key">The key or command token.</param>
        /// <param name="description">The action description.</param>
        /// <param name="keyColor">Foreground color used for the key token.</param>
        /// <param name="descriptionColor">Optional foreground color used for the action description.</param>
        /// <param name="disabled">A value indicating whether the shortcut should render as disabled.</param>
        /// <returns>The added shortcut item.</returns>
        public ShortcutItem Add(
            string key,
            string description,
            RgbColor keyColor,
            RgbColor? descriptionColor = null,
            bool disabled = false)
        {
            ShortcutItem item = ShortcutItem.Create(key, description, keyColor, descriptionColor, disabled);
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
                    TerminalLinkWriter.WriteText(
                        segment.Text,
                        segment.Scheme ?? RenderOptions.DefaultScheme,
                        segment.Link,
                        segment.Decoration,
                        segment.ForegroundRgb,
                        segment.BackgroundRgb);
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
                    current.Add(CreateSegment(separator, ResolveSeparatorStyle()));
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
            StyledText keyStyle = item.Disabled ? ResolveDisabledStyle() : ResolveKeyStyle(item);
            StyledText descriptionStyle = item.Disabled ? ResolveDisabledStyle() : ResolveDescriptionStyle(item);
            bool forceScheme = item.Disabled;
            IList<FixedMessageSegment> keySegments = BuildKeySegments(key, keyStyle, forceScheme);
            IList<FixedMessageSegment> descriptionSegments = ParseMarkupSegments(description, descriptionStyle, forceScheme);

            int descriptionWidth = maxWidth - Measure(keySegments) - gap.Length;
            if (descriptionWidth >= 1)
            {
                var segments = new List<FixedMessageSegment>();
                segments.AddRange(keySegments);
                AddPlainSegment(segments, gap, new StyledText(RenderOptions.DefaultScheme, Decoration.None, null, null));
                segments.AddRange(MarkupRenderText.TruncateSegments(descriptionSegments, descriptionWidth, _options.Ellipsis));
                return segments;
            }

            var combined = new List<FixedMessageSegment>();
            combined.AddRange(keySegments);
            AddPlainSegment(combined, gap, item.Disabled ? ResolveDisabledStyle() : new StyledText(RenderOptions.DefaultScheme, Decoration.None, null, null));
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

        private IList<FixedMessageSegment> BuildKeySegments(string key, StyledText keyStyle, bool forceScheme)
        {
            var segments = new List<FixedMessageSegment>();
            AddPlainSegment(segments, _options.KeyLeftEdge, keyStyle);
            AddSegments(segments, ParseMarkupSegments(key, keyStyle, forceScheme));
            AddPlainSegment(segments, _options.KeyRightEdge, keyStyle);
            return segments;
        }

        private static IList<FixedMessageSegment> ParseMarkupSegments(string value, StyledText style, bool forceScheme)
        {
            var result = new List<FixedMessageSegment>();
            foreach (FixedMessageSegment segment in MarkupRenderText.ParseSegments(
                value,
                style.Scheme,
                style.Decoration,
                style.ForegroundRgb,
                style.BackgroundRgb))
            {
                AddSegment(
                    result,
                    segment.Text,
                    forceScheme ? style.Scheme : segment.Scheme ?? style.Scheme,
                    forceScheme && style.Decoration != Decoration.None ? style.Decoration : segment.Decoration,
                    segment.Link,
                    forceScheme ? style.ForegroundRgb : segment.ForegroundRgb,
                    forceScheme ? style.BackgroundRgb : segment.BackgroundRgb);
            }

            return result;
        }

        private static void AddPlainSegment(IList<FixedMessageSegment> segments, string text, StyledText style)
        {
            AddSegment(segments, text, style);
        }

        private static void AddSegments(IList<FixedMessageSegment> segments, IEnumerable<FixedMessageSegment> values)
        {
            foreach (FixedMessageSegment segment in values ?? Enumerable.Empty<FixedMessageSegment>())
            {
                AddSegment(
                    segments,
                    segment.Text,
                    segment.Scheme ?? RenderOptions.DefaultScheme,
                    segment.Decoration,
                    segment.Link,
                    segment.ForegroundRgb,
                    segment.BackgroundRgb);
            }
        }

        private static void AddSegment(IList<FixedMessageSegment> segments, string text, ColorScheme scheme)
        {
            AddSegment(segments, text, scheme, Decoration.None, null, null, null);
        }

        private static void AddSegment(IList<FixedMessageSegment> segments, string text, StyledText style)
        {
            AddSegment(segments, text, style.Scheme, style.Decoration, null, style.ForegroundRgb, style.BackgroundRgb);
        }

        private static void AddSegment(
            IList<FixedMessageSegment> segments,
            string text,
            ColorScheme scheme,
            Decoration decoration,
            Link link,
            RgbColor? foregroundRgb,
            RgbColor? backgroundRgb)
        {
            text = text ?? string.Empty;
            if (text.Length == 0)
            {
                return;
            }

            if (segments.Count > 0 &&
                SameScheme(segments[segments.Count - 1].Scheme, scheme) &&
                segments[segments.Count - 1].Decoration == decoration &&
                segments[segments.Count - 1].ForegroundRgb.Equals(foregroundRgb) &&
                segments[segments.Count - 1].BackgroundRgb.Equals(backgroundRgb) &&
                TerminalLinkWriter.SameTarget(segments[segments.Count - 1].Link, link))
            {
                FixedMessageSegment previous = segments[segments.Count - 1];
                segments[segments.Count - 1] = new FixedMessageSegment(
                    previous.Text + text,
                    scheme,
                    decoration,
                    link,
                    foregroundRgb,
                    backgroundRgb);
                return;
            }

            segments.Add(new FixedMessageSegment(text, scheme, decoration, link, foregroundRgb, backgroundRgb));
        }

        private static bool SameScheme(ColorScheme? left, ColorScheme right)
        {
            return left.HasValue &&
                   left.Value.Color == right.Color &&
                   left.Value.BackgroundColor == right.BackgroundColor;
        }

        private StyledText ResolveKeyStyle(ShortcutItem item)
        {
            ColorScheme scheme = item.KeyScheme ?? _options.ResolveKeyScheme();
            bool hasItemScheme = item.KeyScheme.HasValue;
            return new StyledText(
                scheme,
                item.KeyDecoration ?? (hasItemScheme ? Decoration.None : _options.KeyDecoration),
                item.KeyForegroundRgb ?? (hasItemScheme ? null : _options.KeyForegroundRgb),
                item.KeyBackgroundRgb ?? (hasItemScheme ? null : _options.KeyBackgroundRgb));
        }

        private StyledText ResolveDescriptionStyle(ShortcutItem item)
        {
            ColorScheme scheme = item.DescriptionScheme ?? _options.ResolveDescriptionScheme();
            bool hasItemScheme = item.DescriptionScheme.HasValue;
            return new StyledText(
                scheme,
                item.DescriptionDecoration ?? (hasItemScheme ? Decoration.None : _options.DescriptionDecoration),
                item.DescriptionForegroundRgb ?? (hasItemScheme ? null : _options.DescriptionForegroundRgb),
                item.DescriptionBackgroundRgb ?? (hasItemScheme ? null : _options.DescriptionBackgroundRgb));
        }

        private StyledText ResolveSeparatorStyle()
        {
            return new StyledText(
                _options.ResolveSeparatorScheme(),
                _options.SeparatorDecoration,
                _options.SeparatorForegroundRgb,
                _options.SeparatorBackgroundRgb);
        }

        private StyledText ResolveDisabledStyle()
        {
            return new StyledText(
                _options.ResolveDisabledScheme(),
                _options.DisabledDecoration,
                _options.DisabledForegroundRgb,
                _options.DisabledBackgroundRgb);
        }

        private static FixedMessageSegment CreateSegment(string text, StyledText style)
        {
            return new FixedMessageSegment(
                text,
                style.Scheme,
                style.Decoration,
                null,
                style.ForegroundRgb,
                style.BackgroundRgb);
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
