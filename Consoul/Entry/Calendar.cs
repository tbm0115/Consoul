using System;
using System.Collections.Generic;
using System.Globalization;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent calendar construction over <see cref="CalendarEntry"/>.
    /// </summary>
    public class Calendar : CalendarEntry
    {
        private int _day;

        /// <summary>
        /// Initializes a new instance of the <see cref="Calendar"/> class for the current month.
        /// </summary>
        public Calendar() : this(DateTime.Today)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Calendar"/> class.
        /// </summary>
        /// <param name="month">Any date within the month to render.</param>
        public Calendar(DateTime month) : this(month.Year, month.Month, month.Day)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Calendar"/> class.
        /// </summary>
        /// <param name="year">The year of the month to render.</param>
        /// <param name="month">The month number to render.</param>
        public Calendar(int year, int month) : this(year, month, 1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Calendar"/> class.
        /// </summary>
        /// <param name="year">The year of the month to render.</param>
        /// <param name="month">The month number to render.</param>
        /// <param name="day">The selected day within the rendered month.</param>
        public Calendar(int year, int month, int day) : base(year, month)
        {
            SetCalendarDate(year, month, day);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Calendar"/> class with explicit options.
        /// </summary>
        /// <param name="month">Any date within the month to render.</param>
        /// <param name="options">Optional rendering options for the calendar.</param>
        public Calendar(DateTime month, CalendarOptions options) : base(month, options)
        {
            SetCalendarDate(month.Year, month.Month, month.Day);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Calendar"/> class with explicit options.
        /// </summary>
        /// <param name="year">The year of the month to render.</param>
        /// <param name="month">The month number to render.</param>
        /// <param name="day">The selected day within the rendered month.</param>
        /// <param name="options">Optional rendering options for the calendar.</param>
        public Calendar(int year, int month, int day, CalendarOptions options) : base(year, month, options)
        {
            SetCalendarDate(year, month, day);
        }

        /// <summary>
        /// Gets or sets the rendered calendar year.
        /// </summary>
        public int Year
        {
            get => base.Month.Year;
            set => SetCalendarDate(value, Month, Day);
        }

        /// <summary>
        /// Gets or sets the rendered calendar month number.
        /// </summary>
        public new int Month
        {
            get => base.Month.Month;
            set => SetCalendarDate(Year, value, Math.Min(Day, DateTime.DaysInMonth(Year, value)));
        }

        /// <summary>
        /// Gets or sets the selected calendar day.
        /// </summary>
        public int Day
        {
            get => _day;
            set => SetCalendarDate(Year, Month, value);
        }

        /// <summary>
        /// Gets the calendar events rendered as highlighted days.
        /// </summary>
        public IList<CalendarDay> CalendarEvents => MarkedDays;

        /// <summary>
        /// Adds a highlighted calendar date.
        /// </summary>
        /// <param name="date">The date to highlight.</param>
        /// <param name="color">Optional foreground color used for the date cell.</param>
        /// <param name="label">Optional label describing the highlighted date.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AddCalendarEvent(DateTime date, ConsoleColor? color = null, string label = null)
        {
            Add(date, color, label);
            return this;
        }

        /// <summary>
        /// Adds a highlighted calendar date with a Spectre-style foreground color.
        /// </summary>
        /// <param name="date">The date to highlight.</param>
        /// <param name="color">Foreground color used for the date cell.</param>
        /// <param name="label">Optional label describing the highlighted date.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AddCalendarEvent(DateTime date, SpectreColor color, string label = null)
        {
            Add(CalendarDay.Create(date, color, label));
            return this;
        }

        /// <summary>
        /// Adds a highlighted calendar date with an RGB foreground color.
        /// </summary>
        /// <param name="date">The date to highlight.</param>
        /// <param name="color">Foreground color used for the date cell.</param>
        /// <param name="label">Optional label describing the highlighted date.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AddCalendarEvent(DateTime date, RgbColor color, string label = null)
        {
            Add(CalendarDay.Create(date, color, label));
            return this;
        }

        /// <summary>
        /// Adds a highlighted calendar date using Spectre-style style text.
        /// </summary>
        /// <param name="date">The date to highlight.</param>
        /// <param name="style">Optional style used for the date cell, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="label">Optional label describing the highlighted date.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AddCalendarEvent(DateTime date, string style, string label = null)
        {
            Add(date, style, label);
            return this;
        }

        /// <summary>
        /// Adds a highlighted calendar event using a Spectre-style <see cref="Style"/>.
        /// </summary>
        /// <param name="date">The event date.</param>
        /// <param name="customEventHighlightStyle">Optional style used for the event day.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AddCalendarEvent(DateTime date, Style customEventHighlightStyle)
        {
            return AddCalendarEvent(string.Empty, date, customEventHighlightStyle);
        }

        /// <summary>
        /// Adds a highlighted calendar event with a description.
        /// </summary>
        /// <param name="description">Optional event description.</param>
        /// <param name="date">The event date.</param>
        /// <param name="customEventHighlightStyle">Optional style used for the event day.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AddCalendarEvent(string description, DateTime date, Style customEventHighlightStyle = null)
        {
            Add(new CalendarEvent(description, date, customEventHighlightStyle));
            return this;
        }

        /// <summary>
        /// Adds a highlighted calendar event.
        /// </summary>
        /// <param name="year">The event year.</param>
        /// <param name="month">The event month.</param>
        /// <param name="day">The event day.</param>
        /// <param name="customEventHighlightStyle">Optional style used for the event day.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AddCalendarEvent(int year, int month, int day, Style customEventHighlightStyle = null)
        {
            return AddCalendarEvent(string.Empty, year, month, day, customEventHighlightStyle);
        }

        /// <summary>
        /// Adds a highlighted calendar event with a description.
        /// </summary>
        /// <param name="description">Optional event description.</param>
        /// <param name="year">The event year.</param>
        /// <param name="month">The event month.</param>
        /// <param name="day">The event day.</param>
        /// <param name="customEventHighlightStyle">Optional style used for the event day.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AddCalendarEvent(string description, int year, int month, int day, Style customEventHighlightStyle = null)
        {
            Add(new CalendarEvent(description, year, month, day, customEventHighlightStyle));
            return this;
        }

        /// <summary>
        /// Adds a highlighted calendar date.
        /// </summary>
        /// <param name="date">The date to highlight.</param>
        /// <param name="color">Optional foreground color used for the date cell.</param>
        /// <param name="label">Optional label describing the highlighted date.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AddEvent(DateTime date, ConsoleColor? color = null, string label = null)
        {
            return AddCalendarEvent(date, color, label);
        }

        /// <summary>
        /// Adds a highlighted calendar date with a Spectre-style foreground color.
        /// </summary>
        /// <param name="date">The date to highlight.</param>
        /// <param name="color">Foreground color used for the date cell.</param>
        /// <param name="label">Optional label describing the highlighted date.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AddEvent(DateTime date, SpectreColor color, string label = null)
        {
            return AddCalendarEvent(date, color, label);
        }

        /// <summary>
        /// Adds a highlighted calendar date with an RGB foreground color.
        /// </summary>
        /// <param name="date">The date to highlight.</param>
        /// <param name="color">Foreground color used for the date cell.</param>
        /// <param name="label">Optional label describing the highlighted date.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AddEvent(DateTime date, RgbColor color, string label = null)
        {
            return AddCalendarEvent(date, color, label);
        }

        /// <summary>
        /// Adds a highlighted calendar date using Spectre-style style text.
        /// </summary>
        /// <param name="date">The date to highlight.</param>
        /// <param name="style">Optional style used for the date cell, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="label">Optional label describing the highlighted date.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AddEvent(DateTime date, string style, string label = null)
        {
            return AddCalendarEvent(date, style, label);
        }

        /// <summary>
        /// Adds a highlighted calendar date using a reusable style object.
        /// </summary>
        /// <param name="date">The date to highlight.</param>
        /// <param name="style">Optional style used for the date cell.</param>
        /// <param name="label">Optional label describing the highlighted date.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AddEvent(DateTime date, Style style, string label = null)
        {
            Add(date, style, label);
            return this;
        }

        /// <summary>
        /// Adds a highlighted calendar day.
        /// </summary>
        /// <param name="day">The highlighted day to add.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AddCalendarEvent(CalendarDay day)
        {
            Add(day);
            return this;
        }

        /// <summary>
        /// Adds highlighted calendar dates projected from a model collection.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into highlighted dates.</param>
        /// <param name="dateSelector">Selects the date to highlight.</param>
        /// <param name="labelSelector">Optionally selects the label describing the highlighted date.</param>
        /// <param name="colorSelector">Optionally selects the foreground color used for the date cell.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AddCalendarEvents<T>(
            IEnumerable<T> items,
            Func<T, DateTime> dateSelector,
            Func<T, string> labelSelector = null,
            Func<T, ConsoleColor?> colorSelector = null)
        {
            if (dateSelector == null)
            {
                throw new ArgumentNullException(nameof(dateSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddCalendarEvent(
                    dateSelector(item),
                    colorSelector != null ? colorSelector(item) : null,
                    labelSelector != null ? labelSelector(item) : null);
            }

            return this;
        }

        /// <summary>
        /// Adds highlighted calendar dates projected from a model collection with Spectre-style date colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into highlighted dates.</param>
        /// <param name="dateSelector">Selects the date to highlight.</param>
        /// <param name="colorSelector">Selects the foreground color used for the date cell.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AddCalendarEvents<T>(
            IEnumerable<T> items,
            Func<T, DateTime> dateSelector,
            Func<T, SpectreColor> colorSelector)
        {
            return AddCalendarEvents(items, dateSelector, null, colorSelector);
        }

        /// <summary>
        /// Adds highlighted calendar dates projected from a model collection with Spectre-style date colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into highlighted dates.</param>
        /// <param name="dateSelector">Selects the date to highlight.</param>
        /// <param name="labelSelector">Optionally selects the label describing the highlighted date.</param>
        /// <param name="colorSelector">Selects the foreground color used for the date cell.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AddCalendarEvents<T>(
            IEnumerable<T> items,
            Func<T, DateTime> dateSelector,
            Func<T, string> labelSelector,
            Func<T, SpectreColor> colorSelector)
        {
            if (colorSelector == null)
            {
                throw new ArgumentNullException(nameof(colorSelector));
            }

            if (dateSelector == null)
            {
                throw new ArgumentNullException(nameof(dateSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddCalendarEvent(
                    dateSelector(item),
                    colorSelector(item),
                    labelSelector != null ? labelSelector(item) : null);
            }

            return this;
        }

        /// <summary>
        /// Adds highlighted calendar dates projected from a model collection with RGB date colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into highlighted dates.</param>
        /// <param name="dateSelector">Selects the date to highlight.</param>
        /// <param name="colorSelector">Selects the foreground color used for the date cell.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AddCalendarEvents<T>(
            IEnumerable<T> items,
            Func<T, DateTime> dateSelector,
            Func<T, RgbColor> colorSelector)
        {
            return AddCalendarEvents(items, dateSelector, null, colorSelector);
        }

        /// <summary>
        /// Adds highlighted calendar dates projected from a model collection with RGB date colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into highlighted dates.</param>
        /// <param name="dateSelector">Selects the date to highlight.</param>
        /// <param name="labelSelector">Optionally selects the label describing the highlighted date.</param>
        /// <param name="colorSelector">Selects the foreground color used for the date cell.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AddCalendarEvents<T>(
            IEnumerable<T> items,
            Func<T, DateTime> dateSelector,
            Func<T, string> labelSelector,
            Func<T, RgbColor> colorSelector)
        {
            if (colorSelector == null)
            {
                throw new ArgumentNullException(nameof(colorSelector));
            }

            if (dateSelector == null)
            {
                throw new ArgumentNullException(nameof(dateSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddCalendarEvent(
                    dateSelector(item),
                    colorSelector(item),
                    labelSelector != null ? labelSelector(item) : null);
            }

            return this;
        }

        /// <summary>
        /// Adds highlighted calendar dates projected from a model collection using Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into highlighted dates.</param>
        /// <param name="dateSelector">Selects the date to highlight.</param>
        /// <param name="labelSelector">Optionally selects the label describing the highlighted date.</param>
        /// <param name="styleSelector">Optionally selects style text used for the date cell, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AddCalendarEventsStyled<T>(
            IEnumerable<T> items,
            Func<T, DateTime> dateSelector,
            Func<T, string> labelSelector = null,
            Func<T, string> styleSelector = null)
        {
            if (dateSelector == null)
            {
                throw new ArgumentNullException(nameof(dateSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddCalendarEvent(
                    dateSelector(item),
                    styleSelector != null ? styleSelector(item) : null,
                    labelSelector != null ? labelSelector(item) : null);
            }

            return this;
        }

        /// <summary>
        /// Adds highlighted calendar dates projected from a model collection.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into highlighted dates.</param>
        /// <param name="daySelector">Selects the highlighted day to add for each model item.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AddCalendarEvents<T>(IEnumerable<T> items, Func<T, CalendarDay> daySelector)
        {
            if (daySelector == null)
            {
                throw new ArgumentNullException(nameof(daySelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                AddCalendarEvent(daySelector(item));
            }

            return this;
        }

        /// <summary>
        /// Sets the requested calendar width.
        /// </summary>
        /// <param name="width">The requested width. The actual width is clamped to the active terminal.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar Width(int? width)
        {
            Options.Width = width;
            return this;
        }

        /// <summary>
        /// Sets the first day of each rendered week.
        /// </summary>
        /// <param name="day">The first day of the week.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar FirstDayOfWeek(DayOfWeek day)
        {
            Options.FirstDayOfWeek = day;
            return this;
        }

        /// <summary>
        /// Shows or hides the month title.
        /// </summary>
        /// <param name="show">Whether the month title should render.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar ShowHeader(bool show = true)
        {
            Options.ShowHeader = show;
            return this;
        }

        /// <summary>
        /// Hides the month title.
        /// </summary>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar HideHeader()
        {
            Options.ShowHeader = false;
            return this;
        }

        /// <summary>
        /// Shows or hides weekday labels.
        /// </summary>
        /// <param name="show">Whether weekday labels should render.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar ShowWeekdayHeader(bool show = true)
        {
            Options.ShowWeekdayHeader = show;
            return this;
        }

        /// <summary>
        /// Shows or hides adjacent-month days.
        /// </summary>
        /// <param name="show">Whether adjacent-month days should render.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar ShowAdjacentDays(bool show = true)
        {
            Options.ShowAdjacentDays = show;
            return this;
        }

        /// <summary>
        /// Shows or hides today's date highlight.
        /// </summary>
        /// <param name="show">Whether today's date should be highlighted.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar HighlightToday(bool show = true)
        {
            Options.HighlightToday = show;
            return this;
        }

        /// <summary>
        /// Sets the preferred width of each day cell.
        /// </summary>
        /// <param name="width">The preferred day cell width.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar CellWidth(int width)
        {
            Options.CellWidth = width;
            return this;
        }

        /// <summary>
        /// Sets the culture used for month and weekday labels.
        /// </summary>
        /// <param name="culture">The culture used for labels.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar Culture(CultureInfo culture)
        {
            Options.Culture = culture ?? CultureInfo.CurrentCulture;
            return this;
        }

        /// <summary>
        /// Sets the month title foreground color.
        /// </summary>
        /// <param name="color">The month title foreground color.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar HeaderColor(ConsoleColor color)
        {
            ApplyColor(CalendarStyleRole.Header, color);
            return this;
        }

        /// <summary>
        /// Sets the month title foreground color.
        /// </summary>
        /// <param name="color">The month title foreground color.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar HeaderColor(SpectreColor color)
        {
            ApplyColor(CalendarStyleRole.Header, color);
            return this;
        }

        /// <summary>
        /// Sets the month title foreground color.
        /// </summary>
        /// <param name="color">The month title foreground color.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar HeaderColor(RgbColor color)
        {
            ApplyColor(CalendarStyleRole.Header, color);
            return this;
        }

        /// <summary>
        /// Sets the month title color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar HeaderColor(Style style)
        {
            return HeaderStyle(style);
        }

        /// <summary>
        /// Sets the month title color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>yellow</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar HeaderColor(string style)
        {
            return HeaderStyle(Style.Parse(style));
        }

        /// <summary>
        /// Sets the month title style.
        /// </summary>
        /// <param name="style">The style used for the month title.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar HeaderStyle(Style style)
        {
            ApplyStyle(CalendarStyleRole.Header, style);
            return this;
        }

        /// <summary>
        /// Sets the month title style from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>yellow</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar HeaderStyle(string style)
        {
            return HeaderStyle(Style.Parse(style));
        }

        /// <summary>
        /// Sets the weekday label foreground color.
        /// </summary>
        /// <param name="color">The weekday label foreground color.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar WeekdayColor(ConsoleColor color)
        {
            ApplyColor(CalendarStyleRole.Weekday, color);
            return this;
        }

        /// <summary>
        /// Sets the weekday label foreground color.
        /// </summary>
        /// <param name="color">The weekday label foreground color.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar WeekdayColor(SpectreColor color)
        {
            ApplyColor(CalendarStyleRole.Weekday, color);
            return this;
        }

        /// <summary>
        /// Sets the weekday label foreground color.
        /// </summary>
        /// <param name="color">The weekday label foreground color.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar WeekdayColor(RgbColor color)
        {
            ApplyColor(CalendarStyleRole.Weekday, color);
            return this;
        }

        /// <summary>
        /// Sets the weekday label color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar WeekdayColor(Style style)
        {
            ApplyStyle(CalendarStyleRole.Weekday, style);
            return this;
        }

        /// <summary>
        /// Sets the weekday label color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar WeekdayColor(string style)
        {
            return WeekdayColor(Style.Parse(style));
        }

        /// <summary>
        /// Sets the ordinary day foreground color.
        /// </summary>
        /// <param name="color">The ordinary day foreground color.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar DayColor(ConsoleColor color)
        {
            ApplyColor(CalendarStyleRole.Day, color);
            return this;
        }

        /// <summary>
        /// Sets the ordinary day foreground color.
        /// </summary>
        /// <param name="color">The ordinary day foreground color.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar DayColor(SpectreColor color)
        {
            ApplyColor(CalendarStyleRole.Day, color);
            return this;
        }

        /// <summary>
        /// Sets the ordinary day foreground color.
        /// </summary>
        /// <param name="color">The ordinary day foreground color.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar DayColor(RgbColor color)
        {
            ApplyColor(CalendarStyleRole.Day, color);
            return this;
        }

        /// <summary>
        /// Sets the ordinary day color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar DayColor(Style style)
        {
            ApplyStyle(CalendarStyleRole.Day, style);
            return this;
        }

        /// <summary>
        /// Sets the ordinary day color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>white</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar DayColor(string style)
        {
            return DayColor(Style.Parse(style));
        }

        /// <summary>
        /// Sets the adjacent-month day foreground color.
        /// </summary>
        /// <param name="color">The adjacent-month day foreground color.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AdjacentDayColor(ConsoleColor color)
        {
            ApplyColor(CalendarStyleRole.AdjacentDay, color);
            return this;
        }

        /// <summary>
        /// Sets the adjacent-month day foreground color.
        /// </summary>
        /// <param name="color">The adjacent-month day foreground color.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AdjacentDayColor(SpectreColor color)
        {
            ApplyColor(CalendarStyleRole.AdjacentDay, color);
            return this;
        }

        /// <summary>
        /// Sets the adjacent-month day foreground color.
        /// </summary>
        /// <param name="color">The adjacent-month day foreground color.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AdjacentDayColor(RgbColor color)
        {
            ApplyColor(CalendarStyleRole.AdjacentDay, color);
            return this;
        }

        /// <summary>
        /// Sets the adjacent-month day color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AdjacentDayColor(Style style)
        {
            ApplyStyle(CalendarStyleRole.AdjacentDay, style);
            return this;
        }

        /// <summary>
        /// Sets the adjacent-month day color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AdjacentDayColor(string style)
        {
            return AdjacentDayColor(Style.Parse(style));
        }

        /// <summary>
        /// Sets today's date foreground color.
        /// </summary>
        /// <param name="color">Today's date foreground color.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar TodayColor(ConsoleColor color)
        {
            ApplyColor(CalendarStyleRole.Today, color);
            return this;
        }

        /// <summary>
        /// Sets today's date foreground color.
        /// </summary>
        /// <param name="color">Today's date foreground color.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar TodayColor(SpectreColor color)
        {
            ApplyColor(CalendarStyleRole.Today, color);
            return this;
        }

        /// <summary>
        /// Sets today's date foreground color.
        /// </summary>
        /// <param name="color">Today's date foreground color.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar TodayColor(RgbColor color)
        {
            ApplyColor(CalendarStyleRole.Today, color);
            return this;
        }

        /// <summary>
        /// Sets today's date color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar TodayColor(Style style)
        {
            ApplyStyle(CalendarStyleRole.Today, style);
            return this;
        }

        /// <summary>
        /// Sets today's date color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>cyan</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar TodayColor(string style)
        {
            return TodayColor(Style.Parse(style));
        }

        /// <summary>
        /// Sets the default marked-day foreground color.
        /// </summary>
        /// <param name="color">The default marked-day foreground color.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar MarkedDayColor(ConsoleColor color)
        {
            ApplyColor(CalendarStyleRole.MarkedDay, color);
            return this;
        }

        /// <summary>
        /// Sets the default marked-day foreground color.
        /// </summary>
        /// <param name="color">The default marked-day foreground color.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar MarkedDayColor(SpectreColor color)
        {
            ApplyColor(CalendarStyleRole.MarkedDay, color);
            return this;
        }

        /// <summary>
        /// Sets the default marked-day foreground color.
        /// </summary>
        /// <param name="color">The default marked-day foreground color.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar MarkedDayColor(RgbColor color)
        {
            ApplyColor(CalendarStyleRole.MarkedDay, color);
            return this;
        }

        /// <summary>
        /// Sets the default marked-day color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar MarkedDayColor(Style style)
        {
            return HighlightStyle(style);
        }

        /// <summary>
        /// Sets the default marked-day color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar MarkedDayColor(string style)
        {
            return HighlightStyle(Style.Parse(style));
        }

        /// <summary>
        /// Sets the default highlighted event-day style.
        /// </summary>
        /// <param name="style">The default style used for highlighted event days.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar HighlightStyle(Style style)
        {
            ApplyStyle(CalendarStyleRole.MarkedDay, style);
            return this;
        }

        /// <summary>
        /// Sets the default highlighted event-day style from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar HighlightStyle(string style)
        {
            return HighlightStyle(Style.Parse(style));
        }

        private void SetCalendarDate(int year, int month, int day)
        {
            _ = new DateTime(year, month, day);
            base.Month = new DateTime(year, month, 1);
            _day = day;
        }

        private void ApplyColor(CalendarStyleRole role, ConsoleColor color)
        {
            SetStyle(role, Scheme(color), Decoration.None, null, null);
        }

        private void ApplyColor(CalendarStyleRole role, SpectreColor color)
        {
            ApplyColor(role, color.ToRgbColor());
        }

        private void ApplyColor(CalendarStyleRole role, RgbColor color)
        {
            SetStyle(role, Scheme(color), Decoration.None, color, null);
        }

        private void ApplyStyle(CalendarStyleRole role, Style style)
        {
            Style resolvedStyle = style ?? Style.Plain;
            SetStyle(
                role,
                resolvedStyle.ToColorScheme(ResolveScheme(role)),
                resolvedStyle.Decoration,
                resolvedStyle.ForegroundRgb,
                resolvedStyle.BackgroundRgb);
        }

        private ColorScheme ResolveScheme(CalendarStyleRole role)
        {
            switch (role)
            {
                case CalendarStyleRole.Header:
                    return Options.ResolveHeaderScheme();
                case CalendarStyleRole.Weekday:
                    return Options.ResolveWeekdayScheme();
                case CalendarStyleRole.Day:
                    return Options.ResolveDayScheme();
                case CalendarStyleRole.AdjacentDay:
                    return Options.ResolveAdjacentDayScheme();
                case CalendarStyleRole.Today:
                    return Options.ResolveTodayScheme();
                case CalendarStyleRole.MarkedDay:
                    return Options.ResolveMarkedDayScheme();
                default:
                    throw new ArgumentOutOfRangeException(nameof(role), role, null);
            }
        }

        private void SetStyle(
            CalendarStyleRole role,
            ColorScheme scheme,
            Decoration decoration,
            RgbColor? foregroundRgb,
            RgbColor? backgroundRgb)
        {
            switch (role)
            {
                case CalendarStyleRole.Header:
                    Options.HeaderScheme = scheme;
                    Options.HeaderDecoration = decoration;
                    Options.HeaderForegroundRgb = foregroundRgb;
                    Options.HeaderBackgroundRgb = backgroundRgb;
                    break;
                case CalendarStyleRole.Weekday:
                    Options.WeekdayScheme = scheme;
                    Options.WeekdayDecoration = decoration;
                    Options.WeekdayForegroundRgb = foregroundRgb;
                    Options.WeekdayBackgroundRgb = backgroundRgb;
                    break;
                case CalendarStyleRole.Day:
                    Options.DayScheme = scheme;
                    Options.DayDecoration = decoration;
                    Options.DayForegroundRgb = foregroundRgb;
                    Options.DayBackgroundRgb = backgroundRgb;
                    break;
                case CalendarStyleRole.AdjacentDay:
                    Options.AdjacentDayScheme = scheme;
                    Options.AdjacentDayDecoration = decoration;
                    Options.AdjacentDayForegroundRgb = foregroundRgb;
                    Options.AdjacentDayBackgroundRgb = backgroundRgb;
                    break;
                case CalendarStyleRole.Today:
                    Options.TodayScheme = scheme;
                    Options.TodayDecoration = decoration;
                    Options.TodayForegroundRgb = foregroundRgb;
                    Options.TodayBackgroundRgb = backgroundRgb;
                    break;
                case CalendarStyleRole.MarkedDay:
                    Options.MarkedDayScheme = scheme;
                    Options.MarkedDayDecoration = decoration;
                    Options.MarkedDayForegroundRgb = foregroundRgb;
                    Options.MarkedDayBackgroundRgb = backgroundRgb;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(role), role, null);
            }
        }

        private static ColorScheme Scheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        private static ColorScheme Scheme(SpectreColor color)
        {
            return Scheme(color.ToRgbColor());
        }

        private static ColorScheme Scheme(RgbColor color)
        {
            return new ColorScheme
            {
                Color = color.ToConsoleColor(),
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        private enum CalendarStyleRole
        {
            Header,
            Weekday,
            Day,
            AdjacentDay,
            Today,
            MarkedDay
        }
    }
}
