using System;
using System.Collections.Generic;
using System.Linq;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

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
                scheme = Scheme(color.Value);
            }

            var item = new ViewHelpItem(label, description, shortcut, disabledReason, false, scheme);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds an action item to the help surface using a Spectre-style color facade.
        /// </summary>
        /// <param name="label">Action label.</param>
        /// <param name="description">Optional helper text describing the action.</param>
        /// <param name="shortcut">Optional shortcut that can select the action.</param>
        /// <param name="disabledReason">Optional reason the action is currently unavailable.</param>
        /// <param name="color">Foreground color used for the item label.</param>
        /// <returns>The added help item.</returns>
        public ViewHelpItem Add(string label, string description, string shortcut, string disabledReason, SpectreColor color)
        {
            return Add(label, description, shortcut, disabledReason, color.ToRgbColor());
        }

        /// <summary>
        /// Adds an action item to the help surface using an RGB foreground color.
        /// </summary>
        /// <param name="label">Action label.</param>
        /// <param name="description">Optional helper text describing the action.</param>
        /// <param name="shortcut">Optional shortcut that can select the action.</param>
        /// <param name="disabledReason">Optional reason the action is currently unavailable.</param>
        /// <param name="color">Foreground color used for the item label.</param>
        /// <returns>The added help item.</returns>
        public ViewHelpItem Add(string label, string description, string shortcut, string disabledReason, RgbColor color)
        {
            var item = new ViewHelpItem(
                label,
                description,
                shortcut,
                disabledReason,
                false,
                Scheme(color.ToConsoleColor()),
                Decoration.None,
                color,
                null);
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
        /// Adds a display-only separator to the help surface using a Spectre-style color facade.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="color">Foreground color used for the separator.</param>
        /// <returns>The added help item.</returns>
        public ViewHelpItem AddSeparator(string label, SpectreColor color)
        {
            ViewHelpItem item = ViewHelpItem.Separator(label, color);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds a display-only separator to the help surface using an RGB foreground color.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="color">Foreground color used for the separator.</param>
        /// <returns>The added help item.</returns>
        public ViewHelpItem AddSeparator(string label, RgbColor color)
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
        /// Adds existing help items to the help surface.
        /// </summary>
        /// <param name="items">The items to add.</param>
        /// <returns>The current <see cref="ViewHelpEntry"/> instance.</returns>
        public ViewHelpEntry AddItems(IEnumerable<ViewHelpItem> items)
        {
            if (items == null)
            {
                return this;
            }

            foreach (ViewHelpItem item in items)
            {
                Add(item);
            }

            return this;
        }

        /// <summary>
        /// Adds existing help items to the help surface.
        /// </summary>
        /// <param name="firstItem">The first item to add.</param>
        /// <param name="additionalItems">Additional items to add.</param>
        /// <returns>The current <see cref="ViewHelpEntry"/> instance.</returns>
        public ViewHelpEntry AddItems(ViewHelpItem firstItem, params ViewHelpItem[] additionalItems)
        {
            Add(firstItem);

            if (additionalItems != null)
            {
                foreach (ViewHelpItem item in additionalItems)
                {
                    Add(item);
                }
            }

            return this;
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
                ViewHelpOptions.StyledText titleStyle = _options.ResolveTitleStyle();
                new RuleEntry(Title, new RuleOptions
                {
                    Width = width,
                    Alignment = RuleTitleAlignment.Left,
                    RuleScheme = titleStyle.Scheme,
                    RuleDecoration = titleStyle.Decoration,
                    RuleForegroundRgb = titleStyle.ForegroundRgb,
                    RuleBackgroundRgb = titleStyle.BackgroundRgb,
                    TitleScheme = titleStyle.Scheme,
                    TitleDecoration = titleStyle.Decoration,
                    TitleForegroundRgb = titleStyle.ForegroundRgb,
                    TitleBackgroundRgb = titleStyle.BackgroundRgb
                }).Render();
            }

            if (_items.Count == 0)
            {
                WritePlainPadded("No actions available.", width, _options.ResolveDescriptionStyle(), true);
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
            ViewHelpOptions.StyledText labelStyle = ResolveLabelStyle(item);
            ViewHelpOptions.StyledText shortcutStyle = _options.ResolveShortcutStyle();
            ViewHelpOptions.StyledText descriptionStyle = ResolveDescriptionStyle(item);
            IList<List<FixedMessageSegment>> descriptionLines = MarkupRenderText.WrapSegments(
                description,
                layout.DescriptionWidth,
                descriptionStyle.Scheme,
                descriptionStyle.Decoration,
                descriptionStyle.ForegroundRgb,
                descriptionStyle.BackgroundRgb);
            if (descriptionLines.Count == 0)
            {
                descriptionLines.Add(new List<FixedMessageSegment>());
            }

            List<FixedMessageSegment> label = PadOrTrimSegments(item.Label, layout.LabelWidth, labelStyle);
            string shortcut = layout.ShortcutWidth > 0 ? PadOrTrim(FormatShortcut(item.Shortcut), layout.ShortcutWidth) : string.Empty;
            for (int index = 0; index < descriptionLines.Count; index++)
            {
                if (index == 0)
                {
                    MarkupRenderText.WriteSegments(label, labelStyle.Scheme, false);
                    if (layout.ShortcutWidth > 0)
                    {
                        WriteStyled(shortcut, shortcutStyle, false);
                    }
                }
                else
                {
                    Consoul.Write(new string(' ', layout.LabelWidth + layout.ShortcutWidth), RenderOptions.DefaultScheme, false);
                }

                WritePadded(descriptionLines[index], layout.DescriptionWidth, descriptionStyle, true);
            }
        }

        private void RenderStacked(ViewHelpItem item, int width)
        {
            ViewHelpOptions.StyledText labelStyle = ResolveLabelStyle(item);
            foreach (List<FixedMessageSegment> labelLine in MarkupRenderText.WrapSegments(
                item.Label,
                width,
                labelStyle.Scheme,
                labelStyle.Decoration,
                labelStyle.ForegroundRgb,
                labelStyle.BackgroundRgb))
            {
                WritePadded(labelLine, width, labelStyle, true);
            }

            if (_options.ShowShortcuts && !string.IsNullOrWhiteSpace(item.Shortcut))
            {
                WriteIndented(FormatShortcut(item.Shortcut).TrimEnd(), width, _options.ResolveShortcutStyle());
            }

            string description = BuildDescription(item);
            if (!string.IsNullOrWhiteSpace(description))
            {
                ViewHelpOptions.StyledText descriptionStyle = ResolveDescriptionStyle(item);
                foreach (List<FixedMessageSegment> line in MarkupRenderText.WrapSegments(
                    description,
                    Math.Max(1, width - 2),
                    descriptionStyle.Scheme,
                    descriptionStyle.Decoration,
                    descriptionStyle.ForegroundRgb,
                    descriptionStyle.BackgroundRgb))
                {
                    WriteIndented(line, width, descriptionStyle);
                }
            }
        }

        private void RenderSeparator(ViewHelpItem item, int width)
        {
            ViewHelpOptions.StyledText separatorStyle = ResolveSeparatorStyle(item);
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
                MarkupRenderText.ParseSegments(
                    text,
                    separatorStyle.Scheme,
                    separatorStyle.Decoration,
                    separatorStyle.ForegroundRgb,
                    separatorStyle.BackgroundRgb),
                width,
                separatorStyle,
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

        private ViewHelpOptions.StyledText ResolveLabelStyle(ViewHelpItem item)
        {
            ViewHelpOptions.StyledText fallback = _options.ResolveLabelStyle();
            bool hasItemScheme = item.Scheme.HasValue;
            return new ViewHelpOptions.StyledText(
                item.Scheme ?? fallback.Scheme,
                item.Decoration ?? (hasItemScheme ? Decoration.None : fallback.Decoration),
                item.ForegroundRgb ?? (hasItemScheme ? null : fallback.ForegroundRgb),
                item.BackgroundRgb ?? (hasItemScheme ? null : fallback.BackgroundRgb));
        }

        private ViewHelpOptions.StyledText ResolveSeparatorStyle(ViewHelpItem item)
        {
            ViewHelpOptions.StyledText fallback = _options.ResolveSeparatorStyle();
            bool hasItemScheme = item.Scheme.HasValue;
            return new ViewHelpOptions.StyledText(
                item.Scheme ?? fallback.Scheme,
                item.Decoration ?? (hasItemScheme ? Decoration.None : fallback.Decoration),
                item.ForegroundRgb ?? (hasItemScheme ? null : fallback.ForegroundRgb),
                item.BackgroundRgb ?? (hasItemScheme ? null : fallback.BackgroundRgb));
        }

        private ViewHelpOptions.StyledText ResolveDescriptionStyle(ViewHelpItem item)
        {
            if (string.IsNullOrWhiteSpace(item.Description) && !string.IsNullOrWhiteSpace(item.DisabledReason))
            {
                return _options.ResolveDisabledStyle();
            }

            return _options.ResolveDescriptionStyle();
        }

        private string FormatShortcut(string shortcut)
        {
            if (string.IsNullOrWhiteSpace(shortcut))
            {
                return string.Empty;
            }

            return (_options.ShortcutPrefix ?? string.Empty) + shortcut.Trim() + (_options.ShortcutSuffix ?? string.Empty) + " ";
        }

        private static void WriteIndented(string text, int width, ViewHelpOptions.StyledText style)
        {
            int innerWidth = Math.Max(1, width - 2);
            string value = "  " + PadOrTrim(text, innerWidth);
            WritePlainPadded(value, width, style, true);
        }

        private static void WriteIndented(IList<FixedMessageSegment> segments, int width, ViewHelpOptions.StyledText style)
        {
            int innerWidth = Math.Max(1, width - 2);
            List<FixedMessageSegment> inner = PadOrTrimSegments(segments, innerWidth, style);
            List<FixedMessageSegment> value = MarkupRenderText.PadSegments(inner, 2, 0, style.Scheme);
            WritePadded(value, width, style, true);
        }

        private static void WritePlainPadded(string text, int width, ViewHelpOptions.StyledText style, bool writeLine)
        {
            WriteStyled(PadOrTrim(text, width), style, writeLine);
        }

        private static void WritePadded(IList<FixedMessageSegment> segments, int width, ViewHelpOptions.StyledText style, bool writeLine)
        {
            MarkupRenderText.WriteSegments(PadOrTrimSegments(segments, width, style), style.Scheme, writeLine);
        }

        private static List<FixedMessageSegment> PadOrTrimSegments(string text, int width, ViewHelpOptions.StyledText style)
        {
            return PadOrTrimSegments(
                MarkupRenderText.ParseSegments(
                    text,
                    style.Scheme,
                    style.Decoration,
                    style.ForegroundRgb,
                    style.BackgroundRgb),
                width,
                style);
        }

        private static List<FixedMessageSegment> PadOrTrimSegments(IList<FixedMessageSegment> segments, int width, ViewHelpOptions.StyledText style)
        {
            List<FixedMessageSegment> result = MarkupRenderText.TruncateSegments(segments, Math.Max(0, width));
            AddPadding(result, Math.Max(0, width - MarkupRenderText.GetLength(result)), style);
            return result;
        }

        private static void AddPadding(IList<FixedMessageSegment> segments, int count, ViewHelpOptions.StyledText style)
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

        private static void WriteStyled(string text, ViewHelpOptions.StyledText style, bool writeLine)
        {
            TerminalLinkWriter.WriteText(
                text,
                style.Scheme,
                null,
                style.Decoration,
                style.ForegroundRgb,
                style.BackgroundRgb);

            if (writeLine)
            {
                Consoul.Write(string.Empty);
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

        private static ColorScheme Scheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
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
