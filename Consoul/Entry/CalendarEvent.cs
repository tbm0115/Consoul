using System;
using ConsoulLibrary.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a Spectre-style calendar event rendered as a highlighted calendar day.
    /// </summary>
    public class CalendarEvent : CalendarDay
    {
        private Style _customHighlightStyle;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarEvent"/> class.
        /// </summary>
        /// <param name="date">The event date.</param>
        /// <param name="customHighlightStyle">Optional style used for the event day.</param>
        public CalendarEvent(DateTime date, Style customHighlightStyle = null)
            : this(string.Empty, date.Year, date.Month, date.Day, customHighlightStyle)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarEvent"/> class.
        /// </summary>
        /// <param name="description">Optional event description.</param>
        /// <param name="date">The event date.</param>
        /// <param name="customHighlightStyle">Optional style used for the event day.</param>
        public CalendarEvent(string description, DateTime date, Style customHighlightStyle = null)
            : this(description, date.Year, date.Month, date.Day, customHighlightStyle)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarEvent"/> class.
        /// </summary>
        /// <param name="description">Optional event description.</param>
        /// <param name="year">The event year.</param>
        /// <param name="month">The event month.</param>
        /// <param name="day">The event day.</param>
        /// <param name="customHighlightStyle">Optional style used for the event day.</param>
        public CalendarEvent(string description, int year, int month, int day, Style customHighlightStyle = null)
            : base(
                new DateTime(year, month, day),
                description ?? string.Empty,
                ResolveScheme(customHighlightStyle),
                customHighlightStyle?.Decoration,
                customHighlightStyle?.ForegroundRgb,
                customHighlightStyle?.BackgroundRgb)
        {
            _customHighlightStyle = customHighlightStyle;
        }

        /// <summary>
        /// Gets or sets the event description.
        /// </summary>
        public string Description
        {
            get => Label;
            set => Label = value ?? string.Empty;
        }

        /// <summary>
        /// Gets or sets the event year.
        /// </summary>
        public int Year
        {
            get => Date.Year;
            set => SetDate(value, Month, Day);
        }

        /// <summary>
        /// Gets or sets the event month.
        /// </summary>
        public int Month
        {
            get => Date.Month;
            set => SetDate(Year, value, Math.Min(Day, DateTime.DaysInMonth(Year, value)));
        }

        /// <summary>
        /// Gets or sets the event day.
        /// </summary>
        public int Day
        {
            get => Date.Day;
            set => SetDate(Year, Month, value);
        }

        /// <summary>
        /// Gets or sets the custom highlight style used for the event day.
        /// </summary>
        public Style CustomHighlightStyle
        {
            get => _customHighlightStyle;
            set
            {
                _customHighlightStyle = value;
                Scheme = ResolveScheme(value);
                Decoration = value?.Decoration;
                ForegroundRgb = value?.ForegroundRgb;
                BackgroundRgb = value?.BackgroundRgb;
            }
        }

        private void SetDate(int year, int month, int day)
        {
            Date = new DateTime(year, month, day);
        }

        private static ColorScheme? ResolveScheme(Style style)
        {
            return style?.ToColorScheme(RenderOptions.DefaultScheme);
        }
    }
}
