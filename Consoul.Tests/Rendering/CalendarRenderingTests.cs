using System;
using System.Globalization;
using ConsoulLibrary.Color;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for calendar view helpers.
    /// </summary>
    public class CalendarRenderingTests
    {
        /// <summary>
        /// Calendars render month grids at common terminal widths without overflowing.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(20)]
        [InlineData(32)]
        [InlineData(80)]
        public void Calendar_MonthGrid_DoesNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Calendar(new DateTime(2026, 6, 1), new CalendarOptions
                {
                    Width = width,
                    Culture = CultureInfo.InvariantCulture
                });

                VisualAssert.WroteText(scope.Driver, "June 2026");
                VisualAssert.WroteText(scope.Driver, "15");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Calendars support Monday-first layouts and adjacent-month days.
        /// </summary>
        [Fact]
        public void Calendar_MondayFirstWithAdjacentDays_RendersExpectedHeader()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                Consoul.Calendar(new DateTime(2026, 1, 1), new CalendarOptions
                {
                    Width = 32,
                    FirstDayOfWeek = DayOfWeek.Monday,
                    ShowAdjacentDays = true,
                    Culture = CultureInfo.InvariantCulture
                });

                Assert.Contains(scope.Driver.Lines, line => line.Contains("Mon") && line.Contains("Tue") && line.Contains("Sun"));
                VisualAssert.WroteText(scope.Driver, "29");
                VisualAssert.WroteText(scope.Driver, "31");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Calendars honor header, weekday, day, and marked-day colors.
        /// </summary>
        [Fact]
        public void Calendar_CustomOptions_RenderColorsAndMarkedDays()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var options = new CalendarOptions
                {
                    Width = 32,
                    Culture = CultureInfo.InvariantCulture,
                    HeaderScheme = Scheme(ConsoleColor.Yellow),
                    WeekdayScheme = Scheme(ConsoleColor.DarkGray),
                    DayScheme = Scheme(ConsoleColor.Cyan),
                    MarkedDayScheme = Scheme(ConsoleColor.Magenta)
                };

                Consoul.Calendar(new DateTime(2026, 6, 1), calendar =>
                {
                    calendar.Add(new DateTime(2026, 6, 15));
                    calendar.Add(new DateTime(2026, 6, 20), ConsoleColor.Green);
                }, options);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("June 2026") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Sun") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Trim() == "1" && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Trim() == "15" && write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Trim() == "20" && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Calendar builders can project schedule model collections into highlighted dates.
        /// </summary>
        [Fact]
        public void Calendar_FluentBuilder_AddCalendarEventsProjectsModelCollections()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var events = new[]
                {
                    new ScheduleEvent("Release", new DateTime(2026, 6, 18), ConsoleColor.Green),
                    new ScheduleEvent("Freeze", new DateTime(2026, 6, 25), ConsoleColor.Yellow)
                };

                var calendar = new Calendar(new DateTime(2026, 6, 1))
                    .Width(32)
                    .Culture(CultureInfo.InvariantCulture)
                    .AddCalendarEvents(
                        events,
                        entry => entry.Date,
                        entry => entry.Label,
                        entry => (ConsoleColor?)entry.Color)
                    .AddCalendarEvents(
                        new[] { events[0] },
                        entry => CalendarDay.Create(entry.Date.AddDays(1), ConsoleColor.Magenta, "After " + entry.Label));

                AnsiConsole.Write(calendar);

                Assert.Contains(calendar.CalendarEvents, day => day.Date == new DateTime(2026, 6, 18) && day.Label == "Release");
                Assert.Contains(calendar.CalendarEvents, day => day.Date == new DateTime(2026, 6, 25) && day.Label == "Freeze");
                Assert.Contains(calendar.CalendarEvents, day => day.Date == new DateTime(2026, 6, 19) && day.Label == "After Release");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Trim() == "18" && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Trim() == "25" && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Trim() == "19" && write.Color == ConsoleColor.Magenta);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed dynamic views can render calendars as schedule sections.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderCalendarSections()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var model = new ScheduleModel
                    {
                        Month = new DateTime(2026, 6, 1),
                        ReleaseDay = new DateTime(2026, 6, 18)
                    };

                    var view = Consoul.View("Release", model)
                        .RenderModel(schedule =>
                        {
                            Consoul.Calendar(schedule.Month, calendar =>
                            {
                                calendar.Add(schedule.ReleaseDay, ConsoleColor.Green, "Release");
                            }, new CalendarOptions
                            {
                                Width = 32,
                                Culture = CultureInfo.InvariantCulture
                            });
                        })
                        .Option("Refresh", schedule => schedule.ReleaseDay = schedule.ReleaseDay.AddDays(1));

                    view.Render();

                    VisualAssert.WroteText(scope.Driver, "June 2026");
                    VisualAssert.WroteText(scope.Driver, "18");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        private static ColorScheme Scheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = ConsoleColor.Black
            };
        }

        private sealed class ScheduleModel
        {
            public DateTime Month { get; set; }

            public DateTime ReleaseDay { get; set; }
        }

        private sealed class ScheduleEvent
        {
            public ScheduleEvent(string label, DateTime date, ConsoleColor color)
            {
                Label = label;
                Date = date;
                Color = color;
            }

            public string Label { get; }

            public DateTime Date { get; }

            public ConsoleColor Color { get; }
        }

        private sealed class TestRoutine : Routine
        {
            public TestRoutine(string[] inputs)
            {
                foreach (string input in inputs)
                {
                    Enqueue(new RoutineInput { Value = input });
                }
            }
        }
    }
}
