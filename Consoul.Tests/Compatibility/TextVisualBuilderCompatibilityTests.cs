using System;
using System.Globalization;
using ConsoulLibrary.Tests.Rendering;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;
using Xunit;

namespace ConsoulLibrary.Tests.Compatibility
{
    /// <summary>
    /// Tests Spectre-style text and terminal-art builder compatibility.
    /// </summary>
    public class TextVisualBuilderCompatibilityTests
    {
        /// <summary>
        /// Text aliases render through the existing text block renderer.
        /// </summary>
        [Fact]
        public void Text_FluentBuilder_RendersWithAnsiConsoleWrite()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var text = new Text("Ready for deployment")
                    .Width(24)
                    .Centered()
                    .Padding(1)
                    .Color(ConsoleColor.Cyan);

                AnsiConsole.Write(text);

                VisualAssert.ContainsText(scope.Driver, "Ready for deployment");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready for deployment") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 24, "Text line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Text constructor overloads accept common color and style values used in quick visual snippets.
        /// </summary>
        [Fact]
        public void Text_Constructors_AcceptColorAndStyleValues()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                AnsiConsole.Write(new Text("Console constructor", ConsoleColor.Cyan));
                AnsiConsole.Write(new Text("Background constructor", ConsoleColor.White, ConsoleColor.DarkBlue));
                AnsiConsole.Write(new Text("Style constructor", "white on navy"));
                AnsiConsole.Write(new Text("Rgb constructor", RgbColor.Aqua));
                AnsiConsole.Write(new Text("Spectre constructor", SpectreColor.Green));

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Console constructor") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Background constructor") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Style constructor") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Rgb constructor") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Spectre constructor") && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Figlet aliases render through the existing figlet/headline renderer.
        /// </summary>
        [Fact]
        public void FigletText_FluentBuilder_RendersWithAnsiConsoleWrite()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var figlet = new FigletText("Go")
                    .Width(24)
                    .LeftJustified()
                    .Character('*')
                    .Color(ConsoleColor.Green);

                AnsiConsole.Write(figlet);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("*") && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Text and figlet color helpers accept Spectre-style style strings.
        /// </summary>
        [Fact]
        public void TextAndFiglet_FluentBuilders_AcceptStyleStrings()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var text = new Text("Ready")
                    .Width(16)
                    .Color("white on navy");
                var figlet = new FigletText("OK")
                    .Width(16)
                    .Character('#')
                    .Color("#22c55e");

                AnsiConsole.Write(text);
                AnsiConsole.Write(figlet);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#") && write.Color == ConsoleColor.DarkCyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Raw Spectre-style markup in text aliases renders as styled visible text.
        /// </summary>
        [Fact]
        public void Text_FluentBuilder_RendersMarkupText()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var text = new Text("Status: [green]Ready[/] [white on navy]prod[/]")
                    .Width(36)
                    .Color(ConsoleColor.Gray);

                AnsiConsole.Write(text);

                VisualAssert.ContainsText(scope.Driver, "Status: Ready prod");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Status: ") && write.Color == ConsoleColor.Gray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("prod") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.DoesNotContain("[green]", scope.Driver.Text);
                Assert.DoesNotContain("[white on navy]", scope.Driver.Text);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 36, "Text line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Paragraph aliases preserve fluent styled prose through the reusable renderable path.
        /// </summary>
        [Fact]
        public void Paragraph_FluentBuilder_AcceptsLineStyledAndMarkupHelpers()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var paragraph = new Paragraph("Status: ", ConsoleColor.Cyan)
                    .AppendLineStyled("Ready", "green")
                    .AppendMarkup("Next [yellow]review[/]")
                    .Width(32);

                AnsiConsole.Write(paragraph);

                VisualAssert.ContainsText(scope.Driver, "Status: Ready");
                VisualAssert.ContainsText(scope.Driver, "Next review");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Status: ") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("review") && write.Color == ConsoleColor.Yellow);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Text and figlet aliases accept Spectre-style justification vocabulary.
        /// </summary>
        [Fact]
        public void TextAndFiglet_FluentBuilders_AcceptJustifyAliases()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var text = new Text("Ready")
                    .Width(16)
                    .Justify(Justify.Right);
                var figlet = new FigletText("OK")
                    .Width(24)
                    .Character('#')
                    .Justify(Justify.Left);

                AnsiConsole.Write(text);
                AnsiConsole.Write(figlet);

                Assert.Contains(scope.Driver.Lines, line => line.EndsWith("Ready", StringComparison.Ordinal));
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Calendar aliases render marked days through the existing calendar renderer.
        /// </summary>
        [Fact]
        public void Calendar_FluentBuilder_RendersWithAnsiConsoleWrite()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var calendar = new Calendar(new DateTime(2026, 6, 1))
                    .Width(32)
                    .Culture(CultureInfo.InvariantCulture)
                    .FirstDayOfWeek(DayOfWeek.Monday)
                    .ShowAdjacentDays()
                    .HighlightToday(false)
                    .HeaderColor("yellow")
                    .WeekdayColor("darkgray")
                    .DayColor("aqua")
                    .MarkedDayColor("fuchsia")
                    .AddCalendarEvent(new DateTime(2026, 6, 15))
                    .AddEvent(new DateTime(2026, 6, 20), ConsoleColor.Green, "Release");

                AnsiConsole.Write(calendar);

                VisualAssert.ContainsText(scope.Driver, "June 2026");
                VisualAssert.ContainsText(scope.Driver, "15");
                VisualAssert.ContainsText(scope.Driver, "20");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("June 2026") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Mon") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Trim() == "15" && write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Trim() == "20" && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Calendar builders can project style text from model-backed event collections.
        /// </summary>
        [Fact]
        public void Calendar_FluentBuilder_AddCalendarEventsStyledProjectsModelCollections()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var schedule = new[]
                {
                    new { Name = "Release", Date = new DateTime(2026, 6, 15), StyleText = "white on navy" },
                    new { Name = "Freeze", Date = new DateTime(2026, 6, 20), StyleText = "green" }
                };

                var calendar = new Calendar(new DateTime(2026, 6, 1))
                    .Width(32)
                    .Culture(CultureInfo.InvariantCulture)
                    .FirstDayOfWeek(DayOfWeek.Monday)
                    .HighlightToday(false)
                    .AddCalendarEventsStyled(
                        schedule,
                        item => item.Date,
                        item => item.Name,
                        item => item.StyleText);

                AnsiConsole.Write(calendar);

                Assert.Equal(2, calendar.CalendarEvents.Count);
                Assert.Contains(calendar.CalendarEvents, item => item.Label == "Release");
                Assert.Contains(calendar.CalendarEvents, item => item.Label == "Freeze");
                VisualAssert.ContainsText(scope.Driver, "June 2026");
                VisualAssert.ContainsText(scope.Driver, "15");
                VisualAssert.ContainsText(scope.Driver, "20");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Trim() == "15" && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Trim() == "20" && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Calendar aliases accept Spectre-style mutable date properties, event overloads, and style helpers.
        /// </summary>
        [Fact]
        public void Calendar_FluentBuilder_AcceptsSpectreStyleDateEventAndStyleAliases()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var calendar = new Calendar(2026, 5, 10)
                    .Width(32)
                    .Culture(CultureInfo.InvariantCulture)
                    .FirstDayOfWeek(DayOfWeek.Monday)
                    .HighlightToday(false)
                    .HeaderStyle(new Style(ConsoleColor.Cyan))
                    .HighlightStyle(new Style(ConsoleColor.Yellow))
                    .AddCalendarEvent("Release", 2026, 6, 15, new Style(ConsoleColor.Green))
                    .AddCalendarEvent("Freeze", new DateTime(2026, 6, 20), new Style(ConsoleColor.Magenta));

                calendar.Year = 2026;
                calendar.Month = 6;
                calendar.Day = 21;
                calendar.CalendarEvents.Add(new CalendarEvent("Demo", 2026, 6, 25, new Style(ConsoleColor.Blue)));

                AnsiConsole.Write(calendar);

                Assert.Equal(2026, calendar.Year);
                Assert.Equal(6, calendar.Month);
                Assert.Equal(21, calendar.Day);
                Assert.Equal(3, calendar.CalendarEvents.Count);
                Assert.Contains(calendar.CalendarEvents, item => item is CalendarEvent calendarEvent && calendarEvent.Description == "Release");
                VisualAssert.ContainsText(scope.Driver, "June 2026");
                VisualAssert.ContainsText(scope.Driver, "15");
                VisualAssert.ContainsText(scope.Driver, "20");
                VisualAssert.ContainsText(scope.Driver, "25");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("June 2026") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Trim() == "15" && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Trim() == "20" && write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Trim() == "25" && write.Color == ConsoleColor.Blue);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Canvas aliases preserve fluent drawing chains while rendering through the existing canvas renderer.
        /// </summary>
        [Fact]
        public void Canvas_FluentBuilder_RendersWithAnsiConsoleWrite()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                var canvas = new Canvas(14, 4)
                    .MaxWidth(14)
                    .EmptyCharacter('.')
                    .DefaultColor("darkgray")
                    .Rectangle(0, 0, 13, 3, '#', "aqua")
                    .Text(2, 1, "API", "#22c55e")
                    .Line(6, 2, 11, 2, '-', "yellow")
                    .Set(12, 2, '>', "white on navy");

                AnsiConsole.Write(canvas);

                VisualAssert.ContainsText(scope.Driver, "API");
                VisualAssert.ContainsText(scope.Driver, ">");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("A") && write.Color == ConsoleColor.DarkCyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("-") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(">") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Canvas drawing helpers accept reusable styles across cells, text, lines, outlines, fills, and defaults.
        /// </summary>
        [Fact]
        public void Canvas_ColorHelpers_AcceptStyleObjects()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var canvas = new Canvas(16, 5)
                    .MaxWidth(16)
                    .EmptyCharacter('.')
                    .DefaultColor(new Style(ConsoleColor.DarkGray))
                    .Rectangle(0, 0, 15, 4, '#', new Style(ConsoleColor.Cyan))
                    .FillRectangle(1, 1, 3, 2, 'f', new Style(ConsoleColor.DarkMagenta))
                    .Line(5, 1, 10, 1, '-', new Style(ConsoleColor.Yellow))
                    .Text(2, 2, "OK", new Style(ConsoleColor.Green))
                    .Set(14, 3, '>', new Style(ConsoleColor.White, ConsoleColor.DarkBlue));

                AnsiConsole.Write(canvas);

                VisualAssert.ContainsText(scope.Driver, "OK");
                VisualAssert.ContainsText(scope.Driver, ">");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(".") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("f") && write.Color == ConsoleColor.DarkMagenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("-") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("O") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(">") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Text path color aliases accept reusable styles for every displayed path segment.
        /// </summary>
        [Fact]
        public void TextPath_ColorAliases_AcceptStyleObjects()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var textPath = new TextPath(@"C:\repo\src\Program.cs")
                    .Width(80)
                    .Separator("/")
                    .RootColor(new Style(ConsoleColor.DarkGray))
                    .DirectoryColor(new Style(ConsoleColor.Blue))
                    .StemColor(new Style(ConsoleColor.Cyan))
                    .ExtensionColor(new Style(ConsoleColor.Green))
                    .SeparatorColor(new Style(ConsoleColor.Yellow));
                var leafPath = new TextPath("/var/log/consoul.json")
                    .Width(64)
                    .LeafColor(new Style(ConsoleColor.Magenta));

                AnsiConsole.Write(textPath);
                AnsiConsole.Write(leafPath);

                VisualAssert.ContainsText(scope.Driver, "Program");
                VisualAssert.ContainsText(scope.Driver, "consoul");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("C:") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("repo") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Program") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(".cs") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("/") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("consoul") && write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(".json") && write.Color == ConsoleColor.Magenta);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }
    }
}
