using ConsoulLibrary.Color;
using System;
using System.Globalization;
using RgbColor = ConsoulLibrary.Color.RgbColor;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for a responsive calendar.
    /// </summary>
    public class CalendarOptions
    {
        /// <summary>
        /// Gets or sets the requested calendar width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the first day of the rendered week.
        /// </summary>
        public DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Sunday;

        /// <summary>
        /// Gets or sets a value indicating whether the month title should be rendered.
        /// </summary>
        public bool ShowHeader { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether weekday names should be rendered.
        /// </summary>
        public bool ShowWeekdayHeader { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether days from adjacent months should be rendered in the grid.
        /// </summary>
        public bool ShowAdjacentDays { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether today's date should be highlighted.
        /// </summary>
        public bool HighlightToday { get; set; } = true;

        /// <summary>
        /// Gets or sets the preferred width of each day cell.
        /// </summary>
        public int CellWidth { get; set; } = 3;

        /// <summary>
        /// Gets or sets the calendar culture used for month and weekday names.
        /// </summary>
        public CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;

        /// <summary>
        /// Gets or sets the color scheme used for the month title.
        /// </summary>
        public ColorScheme? HeaderScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for the month title.
        /// </summary>
        public Decoration HeaderDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for the month title on ANSI-capable terminals.
        /// </summary>
        public RgbColor? HeaderForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for the month title on ANSI-capable terminals.
        /// </summary>
        public RgbColor? HeaderBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for weekday labels.
        /// </summary>
        public ColorScheme? WeekdayScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for weekday labels.
        /// </summary>
        public Decoration WeekdayDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for weekday labels on ANSI-capable terminals.
        /// </summary>
        public RgbColor? WeekdayForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for weekday labels on ANSI-capable terminals.
        /// </summary>
        public RgbColor? WeekdayBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for ordinary days in the active month.
        /// </summary>
        public ColorScheme? DayScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for ordinary days in the active month.
        /// </summary>
        public Decoration DayDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for ordinary days on ANSI-capable terminals.
        /// </summary>
        public RgbColor? DayForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for ordinary days on ANSI-capable terminals.
        /// </summary>
        public RgbColor? DayBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for adjacent-month days.
        /// </summary>
        public ColorScheme? AdjacentDayScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for adjacent-month days.
        /// </summary>
        public Decoration AdjacentDayDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for adjacent-month days on ANSI-capable terminals.
        /// </summary>
        public RgbColor? AdjacentDayForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for adjacent-month days on ANSI-capable terminals.
        /// </summary>
        public RgbColor? AdjacentDayBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for today's date.
        /// </summary>
        public ColorScheme? TodayScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for today's date.
        /// </summary>
        public Decoration TodayDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for today's date on ANSI-capable terminals.
        /// </summary>
        public RgbColor? TodayForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for today's date on ANSI-capable terminals.
        /// </summary>
        public RgbColor? TodayBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the default color scheme used for marked days.
        /// </summary>
        public ColorScheme? MarkedDayScheme { get; set; }

        /// <summary>
        /// Gets or sets the default ANSI decoration used for marked days.
        /// </summary>
        public Decoration MarkedDayDecoration { get; set; }

        /// <summary>
        /// Gets or sets the default RGB foreground color used for marked days on ANSI-capable terminals.
        /// </summary>
        public RgbColor? MarkedDayForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the default RGB background color used for marked days on ANSI-capable terminals.
        /// </summary>
        public RgbColor? MarkedDayBackgroundRgb { get; set; }

        /// <summary>
        /// Creates a copy of the current calendar options.
        /// </summary>
        /// <returns>A new <see cref="CalendarOptions"/> instance with the same values.</returns>
        public CalendarOptions Clone()
        {
            return new CalendarOptions
            {
                Width = Width,
                FirstDayOfWeek = FirstDayOfWeek,
                ShowHeader = ShowHeader,
                ShowWeekdayHeader = ShowWeekdayHeader,
                ShowAdjacentDays = ShowAdjacentDays,
                HighlightToday = HighlightToday,
                CellWidth = CellWidth,
                Culture = Culture,
                HeaderScheme = HeaderScheme,
                HeaderDecoration = HeaderDecoration,
                HeaderForegroundRgb = HeaderForegroundRgb,
                HeaderBackgroundRgb = HeaderBackgroundRgb,
                WeekdayScheme = WeekdayScheme,
                WeekdayDecoration = WeekdayDecoration,
                WeekdayForegroundRgb = WeekdayForegroundRgb,
                WeekdayBackgroundRgb = WeekdayBackgroundRgb,
                DayScheme = DayScheme,
                DayDecoration = DayDecoration,
                DayForegroundRgb = DayForegroundRgb,
                DayBackgroundRgb = DayBackgroundRgb,
                AdjacentDayScheme = AdjacentDayScheme,
                AdjacentDayDecoration = AdjacentDayDecoration,
                AdjacentDayForegroundRgb = AdjacentDayForegroundRgb,
                AdjacentDayBackgroundRgb = AdjacentDayBackgroundRgb,
                TodayScheme = TodayScheme,
                TodayDecoration = TodayDecoration,
                TodayForegroundRgb = TodayForegroundRgb,
                TodayBackgroundRgb = TodayBackgroundRgb,
                MarkedDayScheme = MarkedDayScheme,
                MarkedDayDecoration = MarkedDayDecoration,
                MarkedDayForegroundRgb = MarkedDayForegroundRgb,
                MarkedDayBackgroundRgb = MarkedDayBackgroundRgb
            };
        }

        internal ColorScheme ResolveHeaderScheme()
        {
            return HeaderScheme ?? RenderOptions.PromptScheme;
        }

        internal ColorScheme ResolveWeekdayScheme()
        {
            return WeekdayScheme ?? RenderOptions.SubnoteScheme;
        }

        internal ColorScheme ResolveDayScheme()
        {
            return DayScheme ?? RenderOptions.DefaultScheme;
        }

        internal ColorScheme ResolveAdjacentDayScheme()
        {
            return AdjacentDayScheme ?? RenderOptions.SubnoteScheme;
        }

        internal ColorScheme ResolveTodayScheme()
        {
            return TodayScheme ?? RenderOptions.OptionScheme;
        }

        internal ColorScheme ResolveMarkedDayScheme()
        {
            return MarkedDayScheme ?? RenderOptions.InvalidScheme;
        }
    }
}
