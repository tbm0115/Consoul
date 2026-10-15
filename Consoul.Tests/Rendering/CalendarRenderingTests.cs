using System;
using System.Globalization;
using System.IO;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
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
        /// Calendar style helpers preserve RGB and decoration metadata for true-color terminals.
        /// </summary>
        [Fact]
        public void Calendar_StyleHelpersEmitTrueColorSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 12, isTerminal: true),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });

            var calendar = new Calendar(new DateTime(2026, 6, 1))
                .Width(40)
                .Culture(CultureInfo.InvariantCulture)
                .FirstDayOfWeek(DayOfWeek.Monday)
                .HighlightToday(false)
                .HeaderStyle("bold #22c55e")
                .WeekdayColor("italic #778899")
                .DayColor("#336699")
                .MarkedDayColor("underline #112233")
                .AddCalendarEvent(new DateTime(2026, 6, 15))
                .AddCalendarEvent("Release", new DateTime(2026, 6, 20), new Style(RgbColor.FromHex("#abcdef"), Decoration.Italic));

            console.Write(calendar);

            string output = writer.ToString();
            Assert.Contains("June 2026", output);
            Assert.Contains("\u001b[1;38;2;34;197;94m", output);
            Assert.Contains("\u001b[3;38;2;119;136;153mMon", output);
            Assert.Contains("\u001b[38;2;51;102;153m  1", output);
            Assert.Contains("\u001b[4;38;2;17;34;51m 15", output);
            Assert.Contains("\u001b[3;38;2;171;205;239m 20", output);
        }

        /// <summary>
        /// Calendar RGB selector helpers use 256-color SGR when the terminal supports eight-bit color.
        /// </summary>
        [Fact]
        public void Calendar_RgbSelectorsEmitEightBitSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 12, isTerminal: true),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.EightBit
            });

            var events = new[]
            {
                new StyledScheduleEvent("Release", new DateTime(2026, 6, 15), RgbColor.FromHex("#445566")),
                new StyledScheduleEvent("Freeze", new DateTime(2026, 6, 20), RgbColor.FromHex("#336699"))
            };

            var calendar = new Calendar(new DateTime(2026, 6, 1))
                .Width(40)
                .Culture(CultureInfo.InvariantCulture)
                .FirstDayOfWeek(DayOfWeek.Monday)
                .HighlightToday(false)
                .HeaderColor(RgbColor.FromHex("#112233"))
                .AddCalendarEvents(events, item => item.Date, item => item.Label, item => item.Color);

            console.Write(calendar);

            string output = writer.ToString();
            Assert.Contains("\u001b[38;5;234m", output);
            Assert.Contains("\u001b[38;5;239m 15", output);
            Assert.Contains("\u001b[38;5;60m 20", output);
            Assert.DoesNotContain("\u001b[38;2", output);
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
        /// Calendar builders accept prepared highlighted days for quick MVC schedule summaries.
        /// </summary>
        [Fact]
        public void Calendar_FluentBuilder_AddCalendarEventsAcceptsPreparedParams()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                CalendarDay release = CalendarDay.Create(new DateTime(2026, 6, 18), ConsoleColor.Green, "Release");
                CalendarDay freeze = CalendarDay.Create(new DateTime(2026, 6, 25), ConsoleColor.Yellow, "Freeze");

                var calendar = new Calendar(new DateTime(2026, 6, 1))
                    .Width(32)
                    .Culture(CultureInfo.InvariantCulture)
                    .AddCalendarEvents(release, freeze);

                AnsiConsole.Write(calendar);

                Assert.Contains(calendar.CalendarEvents, day => day.Date == new DateTime(2026, 6, 18) && day.Label == "Release");
                Assert.Contains(calendar.CalendarEvents, day => day.Date == new DateTime(2026, 6, 25) && day.Label == "Freeze");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Trim() == "18" && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Trim() == "25" && write.Color == ConsoleColor.Yellow);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Calendar display aliases map to the existing option model and render localized labels.
        /// </summary>
        [Fact]
        public void Calendar_FluentDisplayAliases_RenderLocalizedCalendar()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var calendar = new Calendar(2026, 1)
                    .Width(36)
                    .UseCulture("fr-FR")
                    .FirstDayOfWeek(DayOfWeek.Monday)
                    .ShowTitle()
                    .ShowWeekdays()
                    .ShowAdjacentDays()
                    .HideToday();

                AnsiConsole.Write(calendar);

                Assert.Equal("fr-FR", calendar.Options.Culture.Name);
                Assert.Equal(DayOfWeek.Monday, calendar.Options.FirstDayOfWeek);
                Assert.True(calendar.Options.ShowHeader);
                Assert.True(calendar.Options.ShowWeekdayHeader);
                Assert.True(calendar.Options.ShowAdjacentDays);
                Assert.False(calendar.Options.HighlightToday);
                VisualAssert.ContainsText(scope.Driver, "janvier 2026");
                Assert.Contains(scope.Driver.Lines, line => line.Contains("lun", StringComparison.OrdinalIgnoreCase) && line.Contains("dim", StringComparison.OrdinalIgnoreCase));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Calendar hide aliases keep compact POC schedule widgets terse without boolean arguments.
        /// </summary>
        [Fact]
        public void Calendar_FluentHideAliases_SuppressOptionalChrome()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var calendar = new Calendar(2026, 1)
                    .Width(32)
                    .UseCulture(CultureInfo.InvariantCulture)
                    .ShowTitle()
                    .HideTitle()
                    .ShowWeekdays()
                    .HideWeekdays()
                    .ShowAdjacentDays()
                    .HideAdjacentDays()
                    .ShowToday()
                    .HideToday();

                AnsiConsole.Write(calendar);

                Assert.False(calendar.Options.ShowHeader);
                Assert.False(calendar.Options.ShowWeekdayHeader);
                Assert.False(calendar.Options.ShowAdjacentDays);
                Assert.False(calendar.Options.HighlightToday);
                Assert.DoesNotContain("January 2026", scope.Driver.Text);
                Assert.DoesNotContain("Sun", scope.Driver.Text);
                Assert.DoesNotContain("Mon", scope.Driver.Text);
                VisualAssert.ContainsText(scope.Driver, "1");
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

        private sealed class StyledScheduleEvent
        {
            public StyledScheduleEvent(string label, DateTime date, RgbColor color)
            {
                Label = label;
                Date = date;
                Color = color;
            }

            public string Label { get; }

            public DateTime Date { get; }

            public RgbColor Color { get; }
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
