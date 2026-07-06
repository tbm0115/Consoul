using System;
using System.Collections.Generic;
using System.Linq;
using ConsoulLibrary.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders a responsive help surface for MVC-style view options.
    /// </summary>
    public class ViewHelpEntry : IRenderable
    {
        private readonly List<ViewHelpItem> _items = new List<ViewHelpItem>();
        private readonly ViewHelpOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewHelpEntry"/> class.
        /// </summary>
        /// <param name="title">Optional title rendered above the help items.</param>
        /// <param name="options">Optional rendering options for the help surface.</param>
        public ViewHelpEntry(string title = null, ViewHelpOptions options = null)
        {
            Title = title;
            _options = options != null ? options.Clone() : new ViewHelpOptions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewHelpEntry"/> class with items.
        /// </summary>
        /// <param name="items">Items to render in the help surface.</param>
        /// <param name="title">Optional title rendered above the help items.</param>
        /// <param name="options">Optional rendering options for the help surface.</param>
        public ViewHelpEntry(IEnumerable<ViewHelpItem> items, string title = null, ViewHelpOptions options = null) : this(title, options)
        {
            if (items == null)
            {
                return;
            }

            foreach (ViewHelpItem item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Gets or sets the optional title rendered above the help items.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets the items rendered by the help surface.
        /// </summary>
        public IList<ViewHelpItem> Items => _items;

        /// <summary>
        /// Gets the options used to render the help surface.
        /// </summary>
        public ViewHelpOptions Options => _options;

        /// <summary>
        /// Adds an action item to the help surface.
        /// </summary>
        /// <param name="label">Action label.</param>
        /// <param name="description">Optional helper text describing the action.</param>
        /// <param name="shortcut">Optional shortcut that can select the action.</param>
        /// <param name="disabledReason">Optional reason the action is currently unavailable.</param>
        /// <param name="color">Optional foreground color used for the item label.</param>
        /// <returns>The added help item.</returns>
        public ViewHelpItem Add(string label, string description = null, string shortcut = null, string disabledReason = null, ConsoleColor? color = null)
        {
            ColorScheme? scheme = null;
            if (color.HasValue)
            {
                scheme = new ColorScheme
                {
                    Color = color.Value,
                    BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
                };
            }

            var item = new ViewHelpItem(label, description, shortcut, disabledReason, false, scheme);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds an action item to the help surface using a reusable style object.
        /// </summary>
        /// <param name="label">Action label.</param>
        /// <param name="style">Optional style used for the item label.</param>
        /// <param name="description">Optional helper text describing the action.</param>
        /// <param name="shortcut">Optional shortcut that can select the action.</param>
        /// <param name="disabledReason">Optional reason the action is currently unavailable.</param>
        /// <returns>The added help item.</returns>
        public ViewHelpItem Add(
            string label,
            Style style,
            string description = null,
            string shortcut = null,
            string disabledReason = null)
        {
            ViewHelpItem item = ViewHelpItem.Styled(label, style, description, shortcut, disabledReason);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds an action item to the help surface using Spectre-style style text.
        /// </summary>
        /// <param name="label">Action label.</param>
        /// <param name="style">Optional style used for the item label, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="description">Optional helper text describing the action.</param>
        /// <param name="shortcut">Optional shortcut that can select the action.</param>
        /// <param name="disabledReason">Optional reason the action is currently unavailable.</param>
        /// <returns>The added help item.</returns>
        public ViewHelpItem AddStyled(
            string label,
            string style,
            string description = null,
            string shortcut = null,
            string disabledReason = null)
        {
            ViewHelpItem item = ViewHelpItem.Styled(label, style, description, shortcut, disabledReason);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds a display-only separator to the help surface.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="color">Optional foreground color used for the separator.</param>
        /// <returns>The added help item.</returns>
        public ViewHelpItem AddSeparator(string label = null, ConsoleColor? color = null)
        {
            ViewHelpItem item = ViewHelpItem.Separator(label, color);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds a display-only separator to the help surface using a reusable style object.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="style">Optional style used for the separator.</param>
        /// <returns>The added help item.</returns>
        public ViewHelpItem AddSeparator(string label, Style style)
        {
            ViewHelpItem item = ViewHelpItem.Separator(label, style);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds a display-only separator to the help surface using Spectre-style style text.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="style">Optional style used for the separator, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <returns>The added help item.</returns>
        public ViewHelpItem AddSeparatorStyled(string label, string style)
        {
            ViewHelpItem item = ViewHelpItem.Separator(label, style);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds an item to the help surface.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The added help item.</returns>
        public ViewHelpItem Add(ViewHelpItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Renders the help surface to the active console driver.
        /// </summary>
        public void Render()
        {
            int width = PanelEntry.ResolveWidth(_options.Width);
            if (width <= 0)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(Title))
            {
                new RuleEntry(Title, new RuleOptions
                {
                    Width = width,
                    Alignment = RuleTitleAlignment.Left,
                    RuleScheme = _options.ResolveTitleScheme(),
                    TitleScheme = _options.ResolveTitleScheme()
                }).Render();
            }

            if (_items.Count == 0)
            {
                WritePlainPadded("No actions available.", width, _options.ResolveDescriptionScheme(), true);
                return;
            }

            Layout layout = ResolveLayout(width);
            foreach (ViewHelpItem item in _items)
            {
                if (item.IsSeparator)
                {
                    RenderSeparator(item, width);
                    continue;
                }

                if (layout.Stacked)
                {
                    RenderStacked(item, width);
                }
                else
                {
                    RenderColumns(item, layout);
                }
            }
        }

        /// <summary>
        /// Renders a temporary help surface to the active console driver.
        /// </summary>
        /// <param name="items">Items to render in the help surface.</param>
        /// <param name="title">Optional title rendered above the help items.</param>
        /// <param name="options">Optional rendering options for the help surface.</param>
        public static void Render(IEnumerable<ViewHelpItem> items, string title = null, ViewHelpOptions options = null)
        {
            new ViewHelpEntry(items, title, options).Render();
        }

        private Layout ResolveLayout(int width)
        {
            var actionItems = _items.Where(item => !item.IsSeparator).ToArray();
            int longestLabel = actionItems
                .Select(item => MarkupRenderText.GetPlainText(item.Label ?? string.Empty).Length)
                .DefaultIfEmpty(0)
                .Max();
            int labelWidth = _options.LabelWidth ?? Math.Min(longestLabel, Math.Max(8, width / 3));
            labelWidth = Math.Max(0, Math.Min(labelWidth, Math.Max(0, width - 1)));

            int shortcutWidth = 0;
            if (_options.ShowShortcuts)
            {
                int longestShortcut = actionItems
                    .Select(item => FormatShortcut(item.Shortcut).Length)
                    .DefaultIfEmpty(0)
                    .Max();
                shortcutWidth = _options.ShortcutWidth ?? Math.Min(longestShortcut, Math.Max(0, width / 5));
                shortcutWidth = Math.Max(0, Math.Min(shortcutWidth, Math.Max(0, width - labelWidth - 1)));
            }

            int descriptionWidth = width - labelWidth - shortcutWidth;
            bool stacked = width < 36 || labelWidth < 4 || descriptionWidth < 8;
            return new Layout(width, stacked, labelWidth, shortcutWidth, Math.Max(1, descriptionWidth));
        }

        private void RenderColumns(ViewHelpItem item, Layout layout)
        {
            string description = BuildDescription(item);
            ColorScheme labelScheme = item.Scheme ?? _options.ResolveLabelScheme();
            ColorScheme shortcutScheme = _options.ResolveShortcutScheme();
            ColorScheme descriptionScheme = ResolveDescriptionScheme(item);
            IList<List<FixedMessageSegment>> descriptionLines = MarkupRenderText.WrapSegments(description, layout.DescriptionWidth, descriptionScheme);
            if (descriptionLines.Count == 0)
            {
                descriptionLines.Add(new List<FixedMessageSegment>());
            }

            List<FixedMessageSegment> label = PadOrTrimSegments(item.Label, layout.LabelWidth, labelScheme);
            string shortcut = layout.ShortcutWidth > 0 ? PadOrTrim(FormatShortcut(item.Shortcut), layout.ShortcutWidth) : string.Empty;
            for (int index = 0; index < descriptionLines.Count; index++)
            {
                if (index == 0)
                {
                    MarkupRenderText.WriteSegments(label, labelScheme, false);
                    if (layout.ShortcutWidth > 0)
                    {
                        Consoul.Write(shortcut, shortcutScheme, false);
                    }
                }
                else
                {
                    Consoul.Write(new string(' ', layout.LabelWidth + layout.ShortcutWidth), RenderOptions.DefaultScheme, false);
                }

                WritePadded(descriptionLines[index], layout.DescriptionWidth, descriptionScheme, true);
            }
        }

        private void RenderStacked(ViewHelpItem item, int width)
        {
            ColorScheme labelScheme = item.Scheme ?? _options.ResolveLabelScheme();
            foreach (List<FixedMessageSegment> labelLine in MarkupRenderText.WrapSegments(item.Label, width, labelScheme))
            {
                WritePadded(labelLine, width, labelScheme, true);
            }

            if (_options.ShowShortcuts && !string.IsNullOrWhiteSpace(item.Shortcut))
            {
                WriteIndented(FormatShortcut(item.Shortcut).TrimEnd(), width, _options.ResolveShortcutScheme());
            }

            string description = BuildDescription(item);
            if (!string.IsNullOrWhiteSpace(description))
            {
                ColorScheme descriptionScheme = ResolveDescriptionScheme(item);
                foreach (List<FixedMessageSegment> line in MarkupRenderText.WrapSegments(description, Math.Max(1, width - 2), descriptionScheme))
                {
                    WriteIndented(line, width, descriptionScheme);
                }
            }
        }

        private void RenderSeparator(ViewHelpItem item, int width)
        {
            string label = item.Label ?? string.Empty;
            string text = string.IsNullOrWhiteSpace(label)
                ? new string('-', width)
                : "-- " + label.Trim() + " ";
            int visibleLength = MarkupRenderText.GetPlainText(text).Length;
            if (visibleLength < width)
            {
                text += new string('-', width - visibleLength);
            }

            WritePadded(
                MarkupRenderText.ParseSegments(text, item.Scheme ?? _options.ResolveSeparatorScheme()),
                width,
                item.Scheme ?? _options.ResolveSeparatorScheme(),
                true);
        }

        private string BuildDescription(ViewHelpItem item)
        {
            string description = _options.ShowDescriptions ? item.Description : null;
            string disabledReason = _options.ShowDisabledReasons ? item.DisabledReason : null;
            string disabled = string.IsNullOrWhiteSpace(disabledReason)
                ? null
                : (_options.DisabledPrefix ?? string.Empty) + disabledReason.Trim();

            if (string.IsNullOrWhiteSpace(description))
            {
                return disabled ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(disabled))
            {
                return description.Trim();
            }

            return description.Trim() + " " + disabled;
        }

        private ColorScheme ResolveDescriptionScheme(ViewHelpItem item)
        {
            if (string.IsNullOrWhiteSpace(item.Description) && !string.IsNullOrWhiteSpace(item.DisabledReason))
            {
                return _options.ResolveDisabledScheme();
            }

            return _options.ResolveDescriptionScheme();
        }

        private string FormatShortcut(string shortcut)
        {
            if (string.IsNullOrWhiteSpace(shortcut))
            {
                return string.Empty;
            }

            return (_options.ShortcutPrefix ?? string.Empty) + shortcut.Trim() + (_options.ShortcutSuffix ?? string.Empty) + " ";
        }

        private static void WriteIndented(string text, int width, ColorScheme scheme)
        {
            int innerWidth = Math.Max(1, width - 2);
            string value = "  " + PadOrTrim(text, innerWidth);
            WritePlainPadded(value, width, scheme, true);
        }

        private static void WriteIndented(IList<FixedMessageSegment> segments, int width, ColorScheme scheme)
        {
            int innerWidth = Math.Max(1, width - 2);
            List<FixedMessageSegment> inner = PadOrTrimSegments(segments, innerWidth, scheme);
            List<FixedMessageSegment> value = MarkupRenderText.PadSegments(inner, 2, 0, scheme);
            WritePadded(value, width, scheme, true);
        }

        private static void WritePlainPadded(string text, int width, ColorScheme scheme, bool writeLine)
        {
            Consoul.Write(PadOrTrim(text, width), scheme, writeLine);
        }

        private static void WritePadded(IList<FixedMessageSegment> segments, int width, ColorScheme scheme, bool writeLine)
        {
            MarkupRenderText.WriteSegments(PadOrTrimSegments(segments, width, scheme), scheme, writeLine);
        }

        private static List<FixedMessageSegment> PadOrTrimSegments(string text, int width, ColorScheme scheme)
        {
            return PadOrTrimSegments(MarkupRenderText.ParseSegments(text, scheme), width, scheme);
        }

        private static List<FixedMessageSegment> PadOrTrimSegments(IList<FixedMessageSegment> segments, int width, ColorScheme scheme)
        {
            List<FixedMessageSegment> result = MarkupRenderText.TruncateSegments(segments, Math.Max(0, width));
            MarkupRenderText.AddPadding(result, Math.Max(0, width - MarkupRenderText.GetLength(result)), scheme);
            return result;
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
            public Layout(int width, bool stacked, int labelWidth, int shortcutWidth, int descriptionWidth)
            {
                Width = width;
                Stacked = stacked;
                LabelWidth = labelWidth;
                ShortcutWidth = shortcutWidth;
                DescriptionWidth = descriptionWidth;
            }

            public int Width { get; }

            public bool Stacked { get; }

            public int LabelWidth { get; }

            public int ShortcutWidth { get; }

            public int DescriptionWidth { get; }
        }
    }
}
