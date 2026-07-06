using ConsoulLibrary.Color;
using System;

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
        /// Gets or sets an optional color scheme for the title.
        /// </summary>
        public ColorScheme? TitleScheme { get; set; }

        /// <summary>
        /// Gets or sets an optional color scheme for the description.
        /// </summary>
        public ColorScheme? DescriptionScheme { get; set; }

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
            return new TimelineItem(title, state, description, timestamp)
            {
                TitleScheme = Scheme(titleStyle),
                MarkerScheme = Scheme(markerStyle),
                DescriptionScheme = Scheme(descriptionStyle)
            };
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
            return new TimelineItem(title, state, description, timestamp)
            {
                TitleScheme = string.IsNullOrWhiteSpace(titleStyle) ? (ColorScheme?)null : Style.Parse(titleStyle).ToColorScheme(RenderOptions.DefaultScheme),
                MarkerScheme = string.IsNullOrWhiteSpace(markerStyle) ? (ColorScheme?)null : Style.Parse(markerStyle).ToColorScheme(RenderOptions.DefaultScheme),
                DescriptionScheme = string.IsNullOrWhiteSpace(descriptionStyle) ? (ColorScheme?)null : Style.Parse(descriptionStyle).ToColorScheme(RenderOptions.DefaultScheme)
            };
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
