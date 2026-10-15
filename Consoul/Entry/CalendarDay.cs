using ConsoulLibrary.Color;
using System;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

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
        /// <param name="decoration">Optional ANSI decoration used for the date cell.</param>
        /// <param name="foregroundRgb">Optional RGB foreground color used on ANSI-capable terminals.</param>
        /// <param name="backgroundRgb">Optional RGB background color used on ANSI-capable terminals.</param>
        public CalendarDay(
            DateTime date,
            string label = null,
            ColorScheme? scheme = null,
            Decoration? decoration = null,
            RgbColor? foregroundRgb = null,
            RgbColor? backgroundRgb = null)
        {
            Date = date.Date;
            Label = label;
            Scheme = scheme;
            Decoration = decoration;
            ForegroundRgb = foregroundRgb;
            BackgroundRgb = backgroundRgb;
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
        /// Gets or sets optional ANSI decoration metadata used for the date cell.
        /// </summary>
        public Decoration? Decoration { get; set; }

        /// <summary>
        /// Gets or sets the original RGB foreground color used on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the original RGB background color used on ANSI-capable terminals.
        /// </summary>
        public RgbColor? BackgroundRgb { get; set; }

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
        /// Creates a highlighted calendar day with a Spectre-style foreground color.
        /// </summary>
        /// <param name="date">The calendar date to highlight.</param>
        /// <param name="color">Foreground color used for the date cell.</param>
        /// <param name="label">Optional label describing the highlighted date.</param>
        /// <returns>A configured <see cref="CalendarDay"/>.</returns>
        public static CalendarDay Create(DateTime date, SpectreColor color, string label = null)
        {
            return Create(date, color.ToRgbColor(), label);
        }

        /// <summary>
        /// Creates a highlighted calendar day with an RGB foreground color.
        /// </summary>
        /// <param name="date">The calendar date to highlight.</param>
        /// <param name="color">Foreground color used for the date cell.</param>
        /// <param name="label">Optional label describing the highlighted date.</param>
        /// <returns>A configured <see cref="CalendarDay"/>.</returns>
        public static CalendarDay Create(DateTime date, RgbColor color, string label = null)
        {
            return new CalendarDay(date, label, CreateScheme(color), ConsoulLibrary.Decoration.None, color);
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
            return CreateFromStyle(date, style, label);
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
            return string.IsNullOrWhiteSpace(style)
                ? new CalendarDay(date, label)
                : CreateFromStyle(date, Style.Parse(style), label);
        }

        private static CalendarDay CreateFromStyle(DateTime date, Style style, string label)
        {
            if (style == null)
            {
                return new CalendarDay(date, label);
            }

            return new CalendarDay(
                date,
                label,
                style.ToColorScheme(RenderOptions.DefaultScheme),
                style.Decoration,
                style.ForegroundRgb,
                style.BackgroundRgb);
        }

        private static ColorScheme CreateScheme(RgbColor color)
        {
            return new ColorScheme
            {
                Color = color.ToConsoleColor(),
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }
    }
}
