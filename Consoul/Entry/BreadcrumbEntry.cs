using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;
using System.Linq;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders a responsive breadcrumb trail for MVC-style console navigation.
    /// </summary>
    public class BreadcrumbEntry : IRenderable
    {
        private readonly List<string> _items = new List<string>();
        private readonly List<BreadcrumbOptions.StyledText?> _itemStyles = new List<BreadcrumbOptions.StyledText?>();
        private readonly BreadcrumbOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="BreadcrumbEntry"/> class.
        /// </summary>
        /// <param name="options">Optional rendering options for the breadcrumb.</param>
        public BreadcrumbEntry(BreadcrumbOptions options = null)
        {
            _options = options != null ? options.Clone() : new BreadcrumbOptions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BreadcrumbEntry"/> class with items.
        /// </summary>
        /// <param name="items">The breadcrumb items to render.</param>
        /// <param name="options">Optional rendering options for the breadcrumb.</param>
        public BreadcrumbEntry(IEnumerable<string> items, BreadcrumbOptions options = null) : this(options)
        {
            if (items == null)
            {
                return;
            }

            foreach (string item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Gets the items rendered by the breadcrumb.
        /// </summary>
        public IList<string> Items => _items;

        /// <summary>
        /// Gets the options used to render the breadcrumb.
        /// </summary>
        public BreadcrumbOptions Options => _options;

        /// <summary>
        /// Adds an item to the breadcrumb trail.
        /// </summary>
        /// <param name="item">The item text.</param>
        /// <returns>The breadcrumb entry.</returns>
        public BreadcrumbEntry Add(string item)
        {
            return AddCore(item, null);
        }

        /// <summary>
        /// Adds an item to the breadcrumb trail using a foreground color.
        /// </summary>
        /// <param name="item">The item text.</param>
        /// <param name="color">The foreground color used for the item.</param>
        /// <returns>The breadcrumb entry.</returns>
        public BreadcrumbEntry Add(string item, ConsoleColor color)
        {
            return AddCore(item, new BreadcrumbOptions.StyledText(Scheme(color), Decoration.None, null, null));
        }

        /// <summary>
        /// Adds an item to the breadcrumb trail using a reusable style object.
        /// </summary>
        /// <param name="item">The item text.</param>
        /// <param name="style">Optional style used for the item.</param>
        /// <returns>The breadcrumb entry.</returns>
        public BreadcrumbEntry Add(string item, Style style)
        {
            return AddCore(
                item,
                style == null
                    ? (BreadcrumbOptions.StyledText?)null
                    : BreadcrumbOptions.StyledText.FromStyle(style, RenderOptions.DefaultScheme));
        }

        /// <summary>
        /// Adds an item to the breadcrumb trail using Spectre-style style text.
        /// </summary>
        /// <param name="item">The item text.</param>
        /// <param name="style">Optional style used for the item, such as <c>grey</c>, <c>aqua</c>, or <c>white on blue</c>.</param>
        /// <returns>The breadcrumb entry.</returns>
        public BreadcrumbEntry Add(string item, string style)
        {
            return Add(item, string.IsNullOrWhiteSpace(style) ? null : Style.Parse(style));
        }

        /// <summary>
        /// Adds items projected from a model collection to the breadcrumb trail using Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into breadcrumb items.</param>
        /// <param name="itemSelector">Selects the item text rendered for each breadcrumb segment.</param>
        /// <param name="styleSelector">Optionally selects style text for each breadcrumb segment.</param>
        /// <returns>The breadcrumb entry.</returns>
        public BreadcrumbEntry AddItems<T>(IEnumerable<T> items, Func<T, string> itemSelector, Func<T, string> styleSelector)
        {
            if (itemSelector == null)
            {
                throw new ArgumentNullException(nameof(itemSelector));
            }

            if (styleSelector == null)
            {
                throw new ArgumentNullException(nameof(styleSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                Add(itemSelector(item), styleSelector(item));
            }

            return this;
        }

        /// <summary>
        /// Adds items projected from a model collection to the breadcrumb trail using reusable style objects.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into breadcrumb items.</param>
        /// <param name="itemSelector">Selects the item text rendered for each breadcrumb segment.</param>
        /// <param name="styleSelector">Optionally selects a reusable style for each breadcrumb segment.</param>
        /// <returns>The breadcrumb entry.</returns>
        public BreadcrumbEntry AddItems<T>(IEnumerable<T> items, Func<T, string> itemSelector, Func<T, Style> styleSelector)
        {
            if (itemSelector == null)
            {
                throw new ArgumentNullException(nameof(itemSelector));
            }

            if (styleSelector == null)
            {
                throw new ArgumentNullException(nameof(styleSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                Add(itemSelector(item), styleSelector(item));
            }

            return this;
        }

        /// <summary>
        /// Adds items projected from a model collection to the breadcrumb trail using Spectre-style colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into breadcrumb items.</param>
        /// <param name="itemSelector">Selects the item text rendered for each breadcrumb segment.</param>
        /// <param name="colorSelector">Selects a Spectre-style color for each breadcrumb segment.</param>
        /// <returns>The breadcrumb entry.</returns>
        public BreadcrumbEntry AddItems<T>(IEnumerable<T> items, Func<T, string> itemSelector, Func<T, SpectreColor> colorSelector)
        {
            if (itemSelector == null)
            {
                throw new ArgumentNullException(nameof(itemSelector));
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
                Add(itemSelector(item), new Style(colorSelector(item)));
            }

            return this;
        }

        /// <summary>
        /// Adds items projected from a model collection to the breadcrumb trail using RGB colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into breadcrumb items.</param>
        /// <param name="itemSelector">Selects the item text rendered for each breadcrumb segment.</param>
        /// <param name="colorSelector">Selects an RGB color for each breadcrumb segment.</param>
        /// <returns>The breadcrumb entry.</returns>
        public BreadcrumbEntry AddItems<T>(IEnumerable<T> items, Func<T, string> itemSelector, Func<T, RgbColor> colorSelector)
        {
            if (itemSelector == null)
            {
                throw new ArgumentNullException(nameof(itemSelector));
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
                Add(itemSelector(item), new Style(colorSelector(item)));
            }

            return this;
        }

        private BreadcrumbEntry AddCore(string item, BreadcrumbOptions.StyledText? style)
        {
            string normalized = Normalize(item);
            if (normalized.Length > 0)
            {
                _items.Add(normalized);
                _itemStyles.Add(style);
            }

            return this;
        }

        /// <summary>
        /// Adds items projected from a model collection to the breadcrumb trail.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into breadcrumb items.</param>
        /// <param name="itemSelector">Selects the item text rendered for each breadcrumb segment.</param>
        /// <returns>The breadcrumb entry.</returns>
        public BreadcrumbEntry AddItems<T>(IEnumerable<T> items, Func<T, string> itemSelector)
        {
            if (itemSelector == null)
            {
                throw new ArgumentNullException(nameof(itemSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                Add(itemSelector(item));
            }

            return this;
        }

        /// <summary>
        /// Renders the breadcrumb to the active console driver.
        /// </summary>
        public void Render()
        {
            IList<FixedMessageSegment> segments = BuildSegments(ResolveWidth());
            foreach (FixedMessageSegment segment in segments)
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

        /// <summary>
        /// Creates a plain text representation of the breadcrumb.
        /// </summary>
        /// <param name="width">Optional maximum width used instead of the active terminal width.</param>
        /// <returns>A bounded single-line breadcrumb string.</returns>
        public string ToText(int? width = null)
        {
            return string.Concat(BuildSegments(ResolveWidth(width)).Select(segment => segment.Text));
        }

        /// <summary>
        /// Renders a temporary breadcrumb to the active console driver.
        /// </summary>
        /// <param name="items">The breadcrumb items to render.</param>
        /// <param name="options">Optional rendering options for the breadcrumb.</param>
        public static void Render(IEnumerable<string> items, BreadcrumbOptions options = null)
        {
            new BreadcrumbEntry(items, options).Render();
        }

        /// <summary>
        /// Creates a plain text breadcrumb without writing to the console.
        /// </summary>
        /// <param name="items">The breadcrumb items.</param>
        /// <param name="options">Optional rendering options for the breadcrumb.</param>
        /// <param name="width">Optional maximum width used instead of the active terminal width.</param>
        /// <returns>A bounded single-line breadcrumb string.</returns>
        public static string Text(IEnumerable<string> items, BreadcrumbOptions options = null, int? width = null)
        {
            return new BreadcrumbEntry(items, options).ToText(width);
        }

        private IList<FixedMessageSegment> BuildSegments(int width)
        {
            width = Math.Max(1, width);
            if (_items.Count == 0)
            {
                return new[] { new FixedMessageSegment(string.Empty, RenderOptions.DefaultScheme) };
            }

            IList<FixedMessageSegment> fullSegments = CreateSegments(_items);
            if (MarkupRenderText.GetLength(fullSegments) <= width)
            {
                return fullSegments;
            }

            if (_items.Count == 1)
            {
                return TrimMiddleSegments(
                    CreateItemSegments(0, 1),
                    width,
                    ResolveEllipsis(),
                    ResolveItemStyle(0, 1));
            }

            string separator = ResolveSeparator();
            string ellipsis = ResolveEllipsis();
            int firstLength = MarkupRenderText.GetPlainText(_items[0]).Length;
            int currentLength = MarkupRenderText.GetPlainText(_items[_items.Count - 1]).Length;
            int fixedWidth = separator.Length + separator.Length + ellipsis.Length;
            int available = width - fixedWidth;
            if (_items.Count == 2)
            {
                fixedWidth = separator.Length;
                available = width - fixedWidth;
            }

            if (available < 2)
            {
                return TrimMiddleSegments(
                    fullSegments,
                    width,
                    ellipsis,
                    ResolveItemStyle(_items.Count - 1, _items.Count));
            }

            int firstWidth = Math.Max(1, Math.Min(firstLength, available / 3));
            int currentWidth = Math.Max(1, available - firstWidth);
            if (currentWidth > currentLength)
            {
                int extra = currentWidth - currentLength;
                currentWidth = currentLength;
                firstWidth = Math.Min(firstLength, firstWidth + extra);
            }

            if (_items.Count == 2)
            {
                var segments = new List<FixedMessageSegment>();
                AddSegments(segments, TrimMiddleSegments(CreateItemSegments(0, _items.Count), firstWidth, ellipsis, ResolveItemStyle(0, _items.Count)));
                AddPlainSegment(segments, separator, _options.ResolveSeparatorStyle());
                AddSegments(segments, TrimMiddleSegments(CreateItemSegments(1, _items.Count), currentWidth, ellipsis, ResolveItemStyle(1, _items.Count)));
                return segments;
            }

            var result = new List<FixedMessageSegment>();
            AddSegments(result, TrimMiddleSegments(CreateItemSegments(0, _items.Count), firstWidth, ellipsis, ResolveItemStyle(0, _items.Count)));
            AddPlainSegment(result, separator, _options.ResolveSeparatorStyle());
            AddPlainSegment(result, ellipsis, _options.ResolveEllipsisStyle());
            AddPlainSegment(result, separator, _options.ResolveSeparatorStyle());
            AddSegments(result, TrimMiddleSegments(CreateItemSegments(_items.Count - 1, _items.Count), currentWidth, ellipsis, ResolveItemStyle(_items.Count - 1, _items.Count)));
            return result;
        }

        private IList<FixedMessageSegment> CreateSegments(IList<string> items)
        {
            var segments = new List<FixedMessageSegment>();
            string separator = ResolveSeparator();
            for (int index = 0; index < items.Count; index++)
            {
                if (index > 0)
                {
                    AddPlainSegment(segments, separator, _options.ResolveSeparatorStyle());
                }

                AddSegments(segments, CreateItemSegments(index, items.Count));
            }

            return segments;
        }

        private IList<FixedMessageSegment> CreateItemSegments(int index, int count)
        {
            BreadcrumbOptions.StyledText style = ResolveItemStyle(index, count);
            return MarkupRenderText.ParseSegments(
                _items[index],
                style.Scheme,
                style.Decoration,
                style.ForegroundRgb,
                style.BackgroundRgb);
        }

        private BreadcrumbOptions.StyledText ResolveItemStyle(int index, int count)
        {
            BreadcrumbOptions.StyledText? itemStyle = index >= 0 && index < _itemStyles.Count
                ? _itemStyles[index]
                : null;

            if (itemStyle.HasValue)
            {
                return itemStyle.Value;
            }

            if (_options.HighlightCurrent && index == count - 1)
            {
                return _options.ResolveCurrentStyle();
            }

            return _options.ResolveItemStyle();
        }

        private int ResolveWidth(int? width = null)
        {
            int terminalWidth = Math.Max(1, Consoul.ConsoleBufferWidth - 1);
            int requested = width ?? _options.Width ?? terminalWidth;
            return Math.Max(1, Math.Min(requested, terminalWidth));
        }

        private string Join(IList<string> items)
        {
            return string.Join(ResolveSeparator(), items);
        }

        private string ResolveSeparator()
        {
            return _options.Separator ?? string.Empty;
        }

        private string ResolveEllipsis()
        {
            return string.IsNullOrEmpty(_options.Ellipsis) ? "..." : _options.Ellipsis;
        }

        private static IList<FixedMessageSegment> TrimMiddleSegments(
            IList<FixedMessageSegment> segments,
            int width,
            string ellipsis,
            BreadcrumbOptions.StyledText ellipsisStyle)
        {
            width = Math.Max(0, width);
            if (width == 0 || segments == null || segments.Count == 0)
            {
                return new List<FixedMessageSegment>();
            }

            int length = MarkupRenderText.GetLength(segments);
            if (length <= width)
            {
                return new List<FixedMessageSegment>(segments);
            }

            ellipsis = ellipsis ?? string.Empty;
            if (ellipsis.Length == 0 || width <= ellipsis.Length)
            {
                return MarkupRenderText.TakeVisibleSegments(segments, width);
            }

            int remaining = width - ellipsis.Length;
            int left = remaining / 2;
            int right = remaining - left;
            var result = new List<FixedMessageSegment>();
            AddSegments(result, MarkupRenderText.TakeVisibleSegments(segments, left));
            AddPlainSegment(result, ellipsis, ellipsisStyle);
            AddSegments(result, MarkupRenderText.SkipVisibleSegments(segments, length - right));
            return result;
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

        private static void AddPlainSegment(IList<FixedMessageSegment> segments, string text, BreadcrumbOptions.StyledText style)
        {
            AddSegment(segments, text, style.Scheme, style.Decoration, null, style.ForegroundRgb, style.BackgroundRgb);
        }

        private static void AddSegment(IList<FixedMessageSegment> segments, string text, ColorScheme scheme)
        {
            AddSegment(segments, text, scheme, Decoration.None, null, null, null);
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

        private static ColorScheme Scheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        private static bool SameScheme(ColorScheme? left, ColorScheme right)
        {
            return left.HasValue &&
                   left.Value.Color == right.Color &&
                   left.Value.BackgroundColor == right.BackgroundColor;
        }

        private static string Normalize(string value)
        {
            return (value ?? string.Empty)
                .Replace("\r\n", " ")
                .Replace('\r', ' ')
                .Replace('\n', ' ')
                .Trim();
        }
    }
}
