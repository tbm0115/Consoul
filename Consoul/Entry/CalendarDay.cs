using ConsoulLibrary.Color;
using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a highlighted day rendered by a <see cref="CalendarEntry"/>.
    /// </summary>
    public class CalendarDay
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarDay"/> class.
        /// </summary>
        /// <param name="date">The calendar date to highlight.</param>
        /// <param name="label">Optional label describing the highlighted date.</param>
        /// <param name="scheme">Optional color scheme used for the date cell.</param>
        public CalendarDay(DateTime date, string label = null, ColorScheme? scheme = null)
        {
            Date = date.Date;
            Label = label;
            Scheme = scheme;
        }

        /// <summary>
        /// Gets or sets the highlighted date.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets an optional label describing the highlighted date.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets an optional color scheme used for the date cell.
        /// </summary>
        public ColorScheme? Scheme { get; set; }

        /// <summary>
        /// Creates a highlighted calendar day with an optional foreground color.
        /// </summary>
        /// <param name="date">The calendar date to highlight.</param>
        /// <param name="color">Optional foreground color used for the date cell.</param>
        /// <param name="label">Optional label describing the highlighted date.</param>
        /// <returns>A configured <see cref="CalendarDay"/>.</returns>
        public static CalendarDay Create(DateTime date, ConsoleColor? color = null, string label = null)
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

            return new CalendarDay(date, label, scheme);
        }

        /// <summary>
        /// Creates a highlighted calendar day using a reusable style object.
        /// </summary>
        /// <param name="date">The calendar date to highlight.</param>
        /// <param name="style">Optional style used for the date cell.</param>
        /// <param name="label">Optional label describing the highlighted date.</param>
        /// <returns>A configured <see cref="CalendarDay"/>.</returns>
        public static CalendarDay Create(DateTime date, Style style, string label = null)
        {
            return new CalendarDay(date, label, style?.ToColorScheme(RenderOptions.DefaultScheme));
        }

        /// <summary>
        /// Creates a highlighted calendar day using Spectre-style style text.
        /// </summary>
        /// <param name="date">The calendar date to highlight.</param>
        /// <param name="style">Optional style used for the date cell, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="label">Optional label describing the highlighted date.</param>
        /// <returns>A configured <see cref="CalendarDay"/>.</returns>
        public static CalendarDay Create(DateTime date, string style, string label = null)
        {
            ColorScheme? scheme = string.IsNullOrWhiteSpace(style)
                ? (ColorScheme?)null
                : Style.Parse(style).ToColorScheme(RenderOptions.DefaultScheme);

            return new CalendarDay(date, label, scheme);
        }
    }
}
