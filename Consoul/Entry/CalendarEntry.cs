using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders a responsive month calendar for console views.
    /// </summary>
    public class CalendarEntry : IRenderable
    {
        private readonly List<CalendarDay> _markedDays = new List<CalendarDay>();
        private readonly CalendarOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarEntry"/> class.
        /// </summary>
        /// <param name="month">Any date within the month to render.</param>
        /// <param name="options">Optional rendering options for the calendar.</param>
        public CalendarEntry(DateTime month, CalendarOptions options = null)
        {
            Month = new DateTime(month.Year, month.Month, 1);
            _options = options != null ? options.Clone() : new CalendarOptions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarEntry"/> class.
        /// </summary>
        /// <param name="year">The year of the month to render.</param>
        /// <param name="month">The month number to render.</param>
        /// <param name="options">Optional rendering options for the calendar.</param>
        public CalendarEntry(int year, int month, CalendarOptions options = null)
            : this(new DateTime(year, month, 1), options)
        {
        }

        /// <summary>
        /// Gets the first day of the rendered month.
        /// </summary>
        public DateTime Month { get; protected set; }

        /// <summary>
        /// Gets the marked days rendered by the calendar.
        /// </summary>
        public IList<CalendarDay> MarkedDays => _markedDays;

        /// <summary>
        /// Gets the options used to render the calendar.
        /// </summary>
        public CalendarOptions Options => _options;

        /// <summary>
        /// Adds a highlighted day to the calendar.
        /// </summary>
        /// <param name="date">The date to highlight.</param>
        /// <param name="color">Optional foreground color used for the date cell.</param>
        /// <param name="label">Optional label describing the highlighted date.</param>
        /// <returns>The added highlighted day.</returns>
        public CalendarDay Add(DateTime date, ConsoleColor? color = null, string label = null)
        {
            var day = CalendarDay.Create(date, color, label);
            _markedDays.Add(day);
            return day;
        }

        /// <summary>
        /// Adds a highlighted day to the calendar using Spectre-style style text.
        /// </summary>
        /// <param name="date">The date to highlight.</param>
        /// <param name="style">Optional style used for the date cell, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="label">Optional label describing the highlighted date.</param>
        /// <returns>The added highlighted day.</returns>
        public CalendarDay Add(DateTime date, string style, string label = null)
        {
            var day = CalendarDay.Create(date, style, label);
            _markedDays.Add(day);
            return day;
        }

        /// <summary>
        /// Adds a highlighted day to the calendar using a reusable style object.
        /// </summary>
        /// <param name="date">The date to highlight.</param>
        /// <param name="style">Optional style used for the date cell.</param>
        /// <param name="label">Optional label describing the highlighted date.</param>
        /// <returns>The added highlighted day.</returns>
        public CalendarDay Add(DateTime date, Style style, string label = null)
        {
            var day = CalendarDay.Create(date, style, label);
            _markedDays.Add(day);
            return day;
        }

        /// <summary>
        /// Adds a highlighted day to the calendar.
        /// </summary>
        /// <param name="day">The highlighted day to add.</param>
        /// <returns>The added highlighted day.</returns>
        public CalendarDay Add(CalendarDay day)
        {
            if (day == null)
            {
                throw new ArgumentNullException(nameof(day));
            }

            _markedDays.Add(day);
            return day;
        }

        /// <summary>
        /// Renders the calendar to the active console driver.
        /// </summary>
        public void Render()
        {
            int width = PanelEntry.ResolveWidth(_options.Width);
            Layout layout = ResolveLayout(width);

            if (_options.ShowHeader)
            {
                string title = Month.ToString("MMMM yyyy", _options.Culture ?? CultureInfo.CurrentCulture);
                Consoul.Write(Align(title, layout.GridWidth), _options.ResolveHeaderScheme());
            }

            if (_options.ShowWeekdayHeader)
            {
                RenderWeekdayHeader(layout);
            }

            foreach (DateTime[] week in BuildWeeks())
            {
                RenderWeek(week, layout);
            }
        }

        /// <summary>
        /// Renders a temporary calendar to the active console driver.
        /// </summary>
        /// <param name="month">Any date within the month to render.</param>
        /// <param name="options">Optional rendering options for the calendar.</param>
        public static void Render(DateTime month, CalendarOptions options = null)
        {
            new CalendarEntry(month, options).Render();
        }

        /// <summary>
        /// Renders a temporary calendar to the active console driver.
        /// </summary>
        /// <param name="year">The year of the month to render.</param>
        /// <param name="month">The month number to render.</param>
        /// <param name="options">Optional rendering options for the calendar.</param>
        public static void Render(int year, int month, CalendarOptions options = null)
        {
            new CalendarEntry(year, month, options).Render();
        }

        private Layout ResolveLayout(int width)
        {
            int gutter = width >= 20 ? 1 : 0;
            int preferredCellWidth = Math.Max(1, _options.CellWidth);
            int maximumCellWidth = Math.Max(1, (width - (gutter * 6)) / 7);
            int cellWidth = Math.Max(1, Math.Min(preferredCellWidth, maximumCellWidth));
            int gridWidth = (cellWidth * 7) + (gutter * 6);
            return new Layout(cellWidth, gutter, Math.Max(1, Math.Min(width, gridWidth)));
        }

        private void RenderWeekdayHeader(Layout layout)
        {
            CultureInfo culture = _options.Culture ?? CultureInfo.CurrentCulture;
            for (int offset = 0; offset < 7; offset++)
            {
                DateTime date = new DateTime(2024, 1, 7).AddDays(NormalizeDayOffset((int)_options.FirstDayOfWeek + offset));
                string name = culture.DateTimeFormat.GetAbbreviatedDayName(date.DayOfWeek);
                WriteCell(name, layout, _options.ResolveWeekdayScheme(), offset == 6);
            }
        }

        private IEnumerable<DateTime[]> BuildWeeks()
        {
            DateTime first = Month;
            int offset = NormalizeDayOffset((int)first.DayOfWeek - (int)_options.FirstDayOfWeek);
            DateTime current = first.AddDays(-offset);
            DateTime last = first.AddMonths(1).AddDays(-1);

            do
            {
                var week = new DateTime[7];
                for (int day = 0; day < 7; day++)
                {
                    week[day] = current;
                    current = current.AddDays(1);
                }

                yield return week;
            }
            while (current <= last || current.DayOfWeek != _options.FirstDayOfWeek);
        }

        private void RenderWeek(DateTime[] week, Layout layout)
        {
            for (int index = 0; index < week.Length; index++)
            {
                DateTime day = week[index];
                bool inMonth = day.Month == Month.Month && day.Year == Month.Year;
                string text = inMonth || _options.ShowAdjacentDays ? day.Day.ToString(CultureInfo.InvariantCulture) : string.Empty;
                WriteCell(text, layout, ResolveDayScheme(day, inMonth), index == week.Length - 1);
            }
        }

        private ColorScheme ResolveDayScheme(DateTime day, bool inMonth)
        {
            CalendarDay markedDay = _markedDays.LastOrDefault(candidate => candidate.Date.Date == day.Date);
            if (markedDay != null)
            {
                return markedDay.Scheme ?? _options.ResolveMarkedDayScheme();
            }

            if (_options.HighlightToday && day.Date == DateTime.Today)
            {
                return _options.ResolveTodayScheme();
            }

            return inMonth ? _options.ResolveDayScheme() : _options.ResolveAdjacentDayScheme();
        }

        private void WriteCell(string text, Layout layout, ColorScheme scheme, bool writeLine)
        {
            Consoul.Write(PadOrTrim(text, layout.CellWidth), scheme, writeLine && layout.GutterWidth == 0);
            if (layout.GutterWidth > 0)
            {
                if (!writeLine)
                {
                    Consoul.Write(new string(' ', layout.GutterWidth), RenderOptions.DefaultScheme, false);
                }
                else
                {
                    Consoul.Write(string.Empty, RenderOptions.DefaultScheme, true);
                }
            }
        }

        private string Align(string text, int width)
        {
            text = text ?? string.Empty;
            if (text.Length > width)
            {
                return text.Substring(0, width);
            }

            int padding = width - text.Length;
            int left = padding / 2;
            return new string(' ', left) + text + new string(' ', padding - left);
        }

        private static string PadOrTrim(string text, int width)
        {
            text = text ?? string.Empty;
            width = Math.Max(1, width);
            if (text.Length > width)
            {
                return text.Substring(0, width);
            }

            return text.PadLeft(width);
        }

        private static int NormalizeDayOffset(int value)
        {
            return ((value % 7) + 7) % 7;
        }

        private readonly struct Layout
        {
            public Layout(int cellWidth, int gutterWidth, int gridWidth)
            {
                CellWidth = cellWidth;
                GutterWidth = gutterWidth;
                GridWidth = gridWidth;
            }

            public int CellWidth { get; }

            public int GutterWidth { get; }

            public int GridWidth { get; }
        }
    }
}
