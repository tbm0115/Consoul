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

            return AddCalendarEvents(
                items,
                dateSelector,
                labelSelector,
                ToConsoleColorSelector(colorSelector));
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

            return AddCalendarEvents(
                items,
                dateSelector,
                labelSelector,
                ToConsoleColorSelector(colorSelector));
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

        private static Func<T, ConsoleColor?> ToConsoleColorSelector<T>(Func<T, SpectreColor> selector)
        {
            return selector == null
                ? (Func<T, ConsoleColor?>)null
                : item => selector(item).ToConsoleColor();
        }

        private static Func<T, ConsoleColor?> ToConsoleColorSelector<T>(Func<T, RgbColor> selector)
        {
            return selector == null
                ? (Func<T, ConsoleColor?>)null
                : item => selector(item).ToConsoleColor();
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
            Options.HeaderScheme = Scheme(color);
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
            Options.HeaderScheme = Style.ParseScheme(style, Options.ResolveHeaderScheme());
            return this;
        }

        /// <summary>
        /// Sets the month title style.
        /// </summary>
        /// <param name="style">The style used for the month title.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar HeaderStyle(Style style)
        {
            Options.HeaderScheme = (style ?? new Style()).ToColorScheme(Options.ResolveHeaderScheme());
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
            Options.WeekdayScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the weekday label color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar WeekdayColor(Style style)
        {
            Options.WeekdayScheme = ToScheme(style, Options.ResolveWeekdayScheme());
            return this;
        }

        /// <summary>
        /// Sets the weekday label color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar WeekdayColor(string style)
        {
            Options.WeekdayScheme = Style.ParseScheme(style, Options.ResolveWeekdayScheme());
            return this;
        }

        /// <summary>
        /// Sets the ordinary day foreground color.
        /// </summary>
        /// <param name="color">The ordinary day foreground color.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar DayColor(ConsoleColor color)
        {
            Options.DayScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the ordinary day color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar DayColor(Style style)
        {
            Options.DayScheme = ToScheme(style, Options.ResolveDayScheme());
            return this;
        }

        /// <summary>
        /// Sets the ordinary day color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>white</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar DayColor(string style)
        {
            Options.DayScheme = Style.ParseScheme(style, Options.ResolveDayScheme());
            return this;
        }

        /// <summary>
        /// Sets the adjacent-month day foreground color.
        /// </summary>
        /// <param name="color">The adjacent-month day foreground color.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AdjacentDayColor(ConsoleColor color)
        {
            Options.AdjacentDayScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the adjacent-month day color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AdjacentDayColor(Style style)
        {
            Options.AdjacentDayScheme = ToScheme(style, Options.ResolveAdjacentDayScheme());
            return this;
        }

        /// <summary>
        /// Sets the adjacent-month day color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar AdjacentDayColor(string style)
        {
            Options.AdjacentDayScheme = Style.ParseScheme(style, Options.ResolveAdjacentDayScheme());
            return this;
        }

        /// <summary>
        /// Sets today's date foreground color.
        /// </summary>
        /// <param name="color">Today's date foreground color.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar TodayColor(ConsoleColor color)
        {
            Options.TodayScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets today's date color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar TodayColor(Style style)
        {
            Options.TodayScheme = ToScheme(style, Options.ResolveTodayScheme());
            return this;
        }

        /// <summary>
        /// Sets today's date color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>cyan</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar TodayColor(string style)
        {
            Options.TodayScheme = Style.ParseScheme(style, Options.ResolveTodayScheme());
            return this;
        }

        /// <summary>
        /// Sets the default marked-day foreground color.
        /// </summary>
        /// <param name="color">The default marked-day foreground color.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar MarkedDayColor(ConsoleColor color)
        {
            Options.MarkedDayScheme = Scheme(color);
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
            Options.MarkedDayScheme = Style.ParseScheme(style, Options.ResolveMarkedDayScheme());
            return this;
        }

        /// <summary>
        /// Sets the default highlighted event-day style.
        /// </summary>
        /// <param name="style">The default style used for highlighted event days.</param>
        /// <returns>The current <see cref="Calendar"/> instance.</returns>
        public Calendar HighlightStyle(Style style)
        {
            Options.MarkedDayScheme = (style ?? new Style()).ToColorScheme(Options.ResolveMarkedDayScheme());
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

        private static ColorScheme Scheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        private static ColorScheme ToScheme(Style style, ColorScheme fallback)
        {
            return (style ?? Style.Plain).ToColorScheme(fallback);
        }
    }
}
