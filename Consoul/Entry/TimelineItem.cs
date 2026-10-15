using ConsoulLibrary.Color;
using System;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents one step or event rendered by a timeline.
    /// </summary>
    public class TimelineItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimelineItem"/> class.
        /// </summary>
        public TimelineItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimelineItem"/> class.
        /// </summary>
        /// <param name="title">The primary item text.</param>
        /// <param name="state">The rendered item state.</param>
        /// <param name="description">Optional descriptive text rendered below the title.</param>
        /// <param name="timestamp">Optional timestamp rendered before the title.</param>
        public TimelineItem(string title, TimelineItemState state = TimelineItemState.Pending, string description = null, DateTimeOffset? timestamp = null)
        {
            Title = title ?? string.Empty;
            State = state;
            Description = description;
            Timestamp = timestamp;
        }

        /// <summary>
        /// Gets or sets the primary item text.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets optional descriptive text rendered below the title.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets an optional timestamp rendered before the title.
        /// </summary>
        public DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the rendered item state.
        /// </summary>
        public TimelineItemState State { get; set; }

        /// <summary>
        /// Gets or sets an optional color scheme for the marker.
        /// </summary>
        public ColorScheme? MarkerScheme { get; set; }

        /// <summary>
        /// Gets or sets optional ANSI decoration used for the marker.
        /// </summary>
        public Decoration? MarkerDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for the marker on ANSI-capable terminals.
        /// </summary>
        public RgbColor? MarkerForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for the marker on ANSI-capable terminals.
        /// </summary>
        public RgbColor? MarkerBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets an optional color scheme for the title.
        /// </summary>
        public ColorScheme? TitleScheme { get; set; }

        /// <summary>
        /// Gets or sets optional ANSI decoration used for the title.
        /// </summary>
        public Decoration? TitleDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for the title on ANSI-capable terminals.
        /// </summary>
        public RgbColor? TitleForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for the title on ANSI-capable terminals.
        /// </summary>
        public RgbColor? TitleBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets an optional color scheme for the description.
        /// </summary>
        public ColorScheme? DescriptionScheme { get; set; }

        /// <summary>
        /// Gets or sets optional ANSI decoration used for the description.
        /// </summary>
        public Decoration? DescriptionDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for the description on ANSI-capable terminals.
        /// </summary>
        public RgbColor? DescriptionForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for the description on ANSI-capable terminals.
        /// </summary>
        public RgbColor? DescriptionBackgroundRgb { get; set; }

        /// <summary>
        /// Creates a timeline item.
        /// </summary>
        /// <param name="title">The primary item text.</param>
        /// <param name="state">The rendered item state.</param>
        /// <param name="description">Optional descriptive text rendered below the title.</param>
        /// <param name="timestamp">Optional timestamp rendered before the title.</param>
        /// <param name="titleColor">Optional foreground color used for the title.</param>
        /// <param name="markerColor">Optional foreground color used for the marker.</param>
        /// <param name="descriptionColor">Optional foreground color used for the description.</param>
        /// <returns>A configured <see cref="TimelineItem"/> instance.</returns>
        public static TimelineItem Create(
            string title,
            TimelineItemState state = TimelineItemState.Pending,
            string description = null,
            DateTimeOffset? timestamp = null,
            ConsoleColor? titleColor = null,
            ConsoleColor? markerColor = null,
            ConsoleColor? descriptionColor = null)
        {
            return new TimelineItem(title, state, description, timestamp)
            {
                TitleScheme = titleColor.HasValue ? Scheme(titleColor.Value) : (ColorScheme?)null,
                MarkerScheme = markerColor.HasValue ? Scheme(markerColor.Value) : (ColorScheme?)null,
                DescriptionScheme = descriptionColor.HasValue ? Scheme(descriptionColor.Value) : (ColorScheme?)null
            };
        }

        /// <summary>
        /// Creates a timeline item using Spectre-style color facades.
        /// </summary>
        /// <param name="title">The primary item text.</param>
        /// <param name="titleColor">Foreground color used for the title.</param>
        /// <param name="markerColor">Optional foreground color used for the marker.</param>
        /// <param name="descriptionColor">Optional foreground color used for the description.</param>
        /// <param name="state">The rendered item state.</param>
        /// <param name="description">Optional descriptive text rendered below the title.</param>
        /// <param name="timestamp">Optional timestamp rendered before the title.</param>
        /// <returns>A configured <see cref="TimelineItem"/> instance.</returns>
        public static TimelineItem Create(
            string title,
            SpectreColor titleColor,
            SpectreColor? markerColor = null,
            SpectreColor? descriptionColor = null,
            TimelineItemState state = TimelineItemState.Pending,
            string description = null,
            DateTimeOffset? timestamp = null)
        {
            return Create(
                title,
                titleColor.ToRgbColor(),
                markerColor.HasValue ? markerColor.Value.ToRgbColor() : (RgbColor?)null,
                descriptionColor.HasValue ? descriptionColor.Value.ToRgbColor() : (RgbColor?)null,
                state,
                description,
                timestamp);
        }

        /// <summary>
        /// Creates a timeline item using RGB foreground colors.
        /// </summary>
        /// <param name="title">The primary item text.</param>
        /// <param name="titleColor">Foreground color used for the title.</param>
        /// <param name="markerColor">Optional foreground color used for the marker.</param>
        /// <param name="descriptionColor">Optional foreground color used for the description.</param>
        /// <param name="state">The rendered item state.</param>
        /// <param name="description">Optional descriptive text rendered below the title.</param>
        /// <param name="timestamp">Optional timestamp rendered before the title.</param>
        /// <returns>A configured <see cref="TimelineItem"/> instance.</returns>
        public static TimelineItem Create(
            string title,
            RgbColor titleColor,
            RgbColor? markerColor = null,
            RgbColor? descriptionColor = null,
            TimelineItemState state = TimelineItemState.Pending,
            string description = null,
            DateTimeOffset? timestamp = null)
        {
            return new TimelineItem(title, state, description, timestamp)
            {
                TitleScheme = Scheme(titleColor.ToConsoleColor()),
                MarkerScheme = markerColor.HasValue ? Scheme(markerColor.Value.ToConsoleColor()) : (ColorScheme?)null,
                DescriptionScheme = descriptionColor.HasValue ? Scheme(descriptionColor.Value.ToConsoleColor()) : (ColorScheme?)null,
                TitleForegroundRgb = titleColor,
                MarkerForegroundRgb = markerColor,
                DescriptionForegroundRgb = descriptionColor
            };
        }

        /// <summary>
        /// Creates a timeline item using reusable style objects.
        /// </summary>
        /// <param name="title">The primary item text.</param>
        /// <param name="titleStyle">Optional style used for the title.</param>
        /// <param name="markerStyle">Optional style used for the marker.</param>
        /// <param name="descriptionStyle">Optional style used for the description.</param>
        /// <param name="state">The rendered item state.</param>
        /// <param name="description">Optional descriptive text rendered below the title.</param>
        /// <param name="timestamp">Optional timestamp rendered before the title.</param>
        /// <returns>A configured <see cref="TimelineItem"/> instance.</returns>
        public static TimelineItem Create(
            string title,
            Style titleStyle,
            Style markerStyle = null,
            Style descriptionStyle = null,
            TimelineItemState state = TimelineItemState.Pending,
            string description = null,
            DateTimeOffset? timestamp = null)
        {
            var item = new TimelineItem(title, state, description, timestamp)
            {
                TitleScheme = Scheme(titleStyle),
                MarkerScheme = Scheme(markerStyle),
                DescriptionScheme = Scheme(descriptionStyle)
            };
            ApplyTitleStyle(item, titleStyle);
            ApplyMarkerStyle(item, markerStyle);
            ApplyDescriptionStyle(item, descriptionStyle);
            return item;
        }

        /// <summary>
        /// Creates a timeline item using Spectre-style style text.
        /// </summary>
        /// <param name="title">The primary item text.</param>
        /// <param name="state">The rendered item state.</param>
        /// <param name="description">Optional descriptive text rendered below the title.</param>
        /// <param name="timestamp">Optional timestamp rendered before the title.</param>
        /// <param name="titleStyle">Optional style used for the title, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="markerStyle">Optional style used for the marker, such as <c>grey</c> or <c>#22c55e</c>.</param>
        /// <param name="descriptionStyle">Optional style used for the description, such as <c>darkgray</c>.</param>
        /// <returns>A configured <see cref="TimelineItem"/> instance.</returns>
        public static TimelineItem CreateStyled(
            string title,
            TimelineItemState state = TimelineItemState.Pending,
            string description = null,
            DateTimeOffset? timestamp = null,
            string titleStyle = null,
            string markerStyle = null,
            string descriptionStyle = null)
        {
            return Create(
                title,
                string.IsNullOrWhiteSpace(titleStyle) ? null : Style.Parse(titleStyle),
                string.IsNullOrWhiteSpace(markerStyle) ? null : Style.Parse(markerStyle),
                string.IsNullOrWhiteSpace(descriptionStyle) ? null : Style.Parse(descriptionStyle),
                state,
                description,
                timestamp);
        }

        private static void ApplyTitleStyle(TimelineItem item, Style style)
        {
            if (item == null || style == null)
            {
                return;
            }

            item.TitleDecoration = style.Decoration;
            item.TitleForegroundRgb = style.ForegroundRgb;
            item.TitleBackgroundRgb = style.BackgroundRgb;
        }

        private static void ApplyMarkerStyle(TimelineItem item, Style style)
        {
            if (item == null || style == null)
            {
                return;
            }

            item.MarkerDecoration = style.Decoration;
            item.MarkerForegroundRgb = style.ForegroundRgb;
            item.MarkerBackgroundRgb = style.BackgroundRgb;
        }

        private static void ApplyDescriptionStyle(TimelineItem item, Style style)
        {
            if (item == null || style == null)
            {
                return;
            }

            item.DescriptionDecoration = style.Decoration;
            item.DescriptionForegroundRgb = style.ForegroundRgb;
            item.DescriptionBackgroundRgb = style.BackgroundRgb;
        }

        private static ColorScheme Scheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        private static ColorScheme? Scheme(Style style)
        {
            return style?.ToColorScheme(RenderOptions.DefaultScheme);
        }
    }
}
