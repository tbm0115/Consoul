using System;
using System.Globalization;
using ConsoulLibrary.Tests.Rendering;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;
using Xunit;

namespace ConsoulLibrary.Tests.Compatibility
{
    /// <summary>
    /// Tests common visual builder compatibility with reusable style objects.
    /// </summary>
    public class CommonVisualStyleObjectBuilderCompatibilityTests
    {
        /// <summary>
        /// Status and list color helpers accept reusable styles across the common POC widgets.
        /// </summary>
        [Fact]
        public void StatusAndListBuilders_ColorHelpers_AcceptStyleObjects()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var badge = new Badge("ready")
                    .MaxWidth(12)
                    .Color(new Style(ConsoleColor.White, ConsoleColor.DarkGreen));
                var callout = new Callout("Status change pending.")
                    .Title("Deploy")
                    .Width(36)
                    .BorderColor(new Style(ConsoleColor.DarkGray))
                    .TitleColor(new Style(ConsoleColor.Yellow))
                    .ContentColor(new Style(ConsoleColor.Cyan));
                var breadcrumb = new Breadcrumb()
                    .Width(40)
                    .Separator(" / ")
                    .ItemColor(new Style(ConsoleColor.Blue))
                    .CurrentColor(new Style(ConsoleColor.Green))
                    .SeparatorColor(new Style(ConsoleColor.DarkGray))
                    .AddItem("Home")
                    .AddItem("Services")
                    .AddItem("Deployments");
                var list = new BulletList()
                    .Width(36)
                    .Markers("-", "*")
                    .MarkerColor(new Style(ConsoleColor.Yellow))
                    .TextColor(new Style(ConsoleColor.White));
                var checklist = new Checklist()
                    .Width(42)
                    .LabelColor(new Style(ConsoleColor.White))
                    .DescriptionColor(new Style(ConsoleColor.DarkGray))
                    .AddSuccess("Build", "Artifacts are ready")
                    .AddWarning("Approval", "Waiting for owner");
                var shortcuts = new ShortcutBar()
                    .Width(44)
                    .KeyEdges("<", ">")
                    .KeyDescriptionSeparator(": ")
                    .ItemSeparator(" | ")
                    .KeyColor(new Style(ConsoleColor.Cyan))
                    .DescriptionColor(new Style(ConsoleColor.White))
                    .SeparatorColor(new Style(ConsoleColor.DarkGray))
                    .DisabledColor(new Style(ConsoleColor.DarkYellow))
                    .AddShortcut("Enter", "Select")
                    .AddDisabled("F5", "Refresh preview");

                list.AddItem("Checks");
                list.AddItem("Release");
                AnsiConsole.Write(badge);
                AnsiConsole.Write(callout);
                AnsiConsole.Write(breadcrumb);
                AnsiConsole.Write(list);
                AnsiConsole.Write(checklist);
                AnsiConsole.Write(shortcuts);

                VisualAssert.ContainsText(scope.Driver, "READY");
                VisualAssert.ContainsText(scope.Driver, "Deploy");
                VisualAssert.ContainsText(scope.Driver, "Deployments");
                VisualAssert.ContainsText(scope.Driver, "Checks");
                VisualAssert.ContainsText(scope.Driver, "Waiting for owner");
                VisualAssert.ContainsText(scope.Driver, "<Enter>");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("READY") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkGreen);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Status change pending.") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Home") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deployments") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("-") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Checks") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Waiting for owner") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("<Enter>") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Refresh preview") && write.Color == ConsoleColor.DarkYellow);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Status, navigation, and summary builders can project model-backed Spectre-style colors without wrapper styles.
        /// </summary>
        [Fact]
        public void StatusNavigationAndSummaryBuilders_ColorSelectors_AcceptColorAndRgbColorValues()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var items = new[]
                {
                    new CommonPaletteItem("API", "Ready", "a", new DateTime(2026, 6, 20), SpectreColor.Blue, RgbColor.Aqua),
                    new CommonPaletteItem("Worker", "Waiting", "w", new DateTime(2026, 6, 21), SpectreColor.Red, RgbColor.Yellow)
                };
                var colorItems = new[] { items[0] };
                var rgbItems = new[] { items[1] };

                var list = new BulletList()
                    .Width(54)
                    .Markers("-", "*");
                list.AddItems(colorItems, item => "List color " + item.Name, item => item.Color, item => item.Color);
                BulletListItem parent = list.AddItem("Nested", ConsoleColor.Gray, ConsoleColor.Gray);
                parent.AddItems(rgbItems, item => "List rgb " + item.Name, item => item.RgbColor, item => item.RgbColor);

                var calendar = new Calendar(new DateTime(2026, 6, 1))
                    .Width(32)
                    .Culture(CultureInfo.InvariantCulture)
                    .FirstDayOfWeek(DayOfWeek.Monday)
                    .HighlightToday(false)
                    .AddCalendarEvents(colorItems, item => item.Date, item => item.Name, item => item.Color)
                    .AddCalendarEvents(rgbItems, item => item.Date, item => item.RgbColor);

                var properties = new PropertyListEntry(new PropertyListOptions
                {
                    Width = 54,
                    LabelWidth = 14
                });
                properties.AddItems(colorItems, item => "Prop " + item.Name, item => item.State, item => item.Color, item => item.Color);
                properties.AddItems(rgbItems, item => "Prop " + item.Name, item => item.State, item => item.RgbColor, item => item.RgbColor);

                var shortcuts = new ShortcutBar()
                    .Width(54)
                    .KeyEdges("[", "]")
                    .KeyDescriptionSeparator(": ")
                    .ItemSeparator(" | ")
                    .AddShortcuts(colorItems, item => item.Shortcut, item => "Shortcut color " + item.Name, item => item.Color, item => item.Color)
                    .AddShortcuts(rgbItems, item => item.Shortcut, item => "Shortcut rgb " + item.Name, item => item.RgbColor, item => item.RgbColor);

                var tree = new Tree("Root")
                    .Width(54)
                    .Guide(TreeGuide.Ascii)
                    .AddNodes(colorItems, item => "Tree color " + item.Name, item => item.Color);
                tree.AddNode("Nested", ConsoleColor.Gray)
                    .AddNodes(rgbItems, item => "Tree rgb " + item.Name, item => item.RgbColor);

                AnsiConsole.Write(list);
                AnsiConsole.Write(calendar);
                AnsiConsole.Write(properties);
                AnsiConsole.Write(shortcuts);
                AnsiConsole.Write(tree);

                VisualAssert.ContainsText(scope.Driver, "List color API");
                VisualAssert.ContainsText(scope.Driver, "List rgb Worker");
                VisualAssert.ContainsText(scope.Driver, "Prop API");
                VisualAssert.ContainsText(scope.Driver, "Shortcut rgb Worker");
                VisualAssert.ContainsText(scope.Driver, "Tree color API");
                VisualAssert.ContainsText(scope.Driver, "Tree rgb Worker");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("List color API") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("List rgb Worker") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Trim() == "20" && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Trim() == "21" && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Prop API") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Prop Worker") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Waiting") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[a]") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Shortcut color API") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[w]") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Shortcut rgb Worker") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Tree color API") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Tree rgb Worker") && write.Color == ConsoleColor.Yellow);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 54, "Color-projected status/navigation line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Common visual item factories and per-item builders accept reusable style objects.
        /// </summary>
        [Fact]
        public void CommonVisualItemFactories_AcceptStyleObjects()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var list = new BulletList()
                    .Width(44)
                    .Markers("-", "*");
                BulletListItem root = list.AddItem("Root", new Style(ConsoleColor.Cyan), new Style(ConsoleColor.Yellow));
                root.AddItem("Child", new Style(ConsoleColor.Green), new Style(ConsoleColor.DarkGray));
                list.AddItem(BulletListItem.Create("Factory item", new Style(ConsoleColor.White), new Style(ConsoleColor.Magenta)));

                var checklist = new Checklist()
                    .Width(48);
                checklist.AddItem(
                    "Approval",
                    new Style(ConsoleColor.White),
                    new Style(ConsoleColor.Yellow),
                    new Style(ConsoleColor.DarkGray),
                    ChecklistItemState.Warning,
                    "Waiting");
                checklist.AddItem(ChecklistItem.Create(
                    "Factory check",
                    new Style(ConsoleColor.Cyan),
                    new Style(ConsoleColor.Green),
                    new Style(ConsoleColor.DarkBlue),
                    state: ChecklistItemState.Complete,
                    description: "From factory"));

                var shortcuts = new ShortcutBar()
                    .Width(48)
                    .KeyEdges("<", ">")
                    .KeyDescriptionSeparator(": ");
                shortcuts.AddShortcut("d", "Deploy", new Style(ConsoleColor.Cyan), new Style(ConsoleColor.White));
                shortcuts.Add(ShortcutItem.Create("x", "Factory", new Style(ConsoleColor.Magenta), new Style(ConsoleColor.Yellow)));

                var calendar = new Calendar(new DateTime(2026, 6, 1))
                    .Width(32)
                    .Culture(CultureInfo.InvariantCulture)
                    .FirstDayOfWeek(DayOfWeek.Monday)
                    .HighlightToday(false)
                    .AddEvent(new DateTime(2026, 6, 20), new Style(ConsoleColor.Blue), "Demo");
                calendar.Add(CalendarDay.Create(new DateTime(2026, 6, 21), new Style(ConsoleColor.Magenta), "Factory"));

                AnsiConsole.Write(list);
                AnsiConsole.Write(checklist);
                AnsiConsole.Write(shortcuts);
                AnsiConsole.Write(calendar);

                VisualAssert.ContainsText(scope.Driver, "Root");
                VisualAssert.ContainsText(scope.Driver, "Child");
                VisualAssert.ContainsText(scope.Driver, "Approval");
                VisualAssert.ContainsText(scope.Driver, "<d>");
                VisualAssert.ContainsText(scope.Driver, "20");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("-") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Root") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("*") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Child") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Factory item") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Approval") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Waiting") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Factory check") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("From factory") && write.Color == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("<d>") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("<x>") && write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Factory") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Trim() == "20" && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Trim() == "21" && write.Color == ConsoleColor.Magenta);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 48, "Common visual item line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Composition builder color helpers accept reusable styles without breaking bounded rendering.
        /// </summary>
        [Fact]
        public void CompositionBuilders_ColorHelpers_AcceptStyleObjects()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var rows = new Rows()
                    .Width(36)
                    .Spacing(0)
                    .ContentColor(new Style(ConsoleColor.Green))
                    .AddRow("Summary")
                    .AddRow("Ready");
                var columns = new Columns()
                    .Width(48)
                    .ColumnCount(2)
                    .MinimumColumnWidth(12)
                    .Gutter(2)
                    .HeaderColor(new Style(ConsoleColor.Yellow))
                    .ContentColor(new Style(ConsoleColor.White))
                    .AddColumn("API ready", "Service")
                    .AddColumn("Worker waiting", "Worker");
                var grid = new Grid()
                    .Width(48)
                    .AddColumns(2)
                    .MinimumColumnWidth(10)
                    .Gutter(2)
                    .HeaderColor(new Style(ConsoleColor.Yellow))
                    .ContentColor(new Style(ConsoleColor.Cyan))
                    .AddCell("Job", "Name")
                    .AddCell("State", "Status")
                    .AddRow("Build", "Ready");
                var layout = new Layout("Dashboard")
                    .Width(52)
                    .ColumnCount(2)
                    .MinimumPaneWidth(14)
                    .Gutter(2)
                    .TitleColor(new Style(ConsoleColor.Yellow))
                    .ContentColor(new Style(ConsoleColor.White))
                    .AddPane("Running", "Service")
                    .AddPane("Waiting", "Worker");
                var align = new Align("Centered", HorizontalAlignment.Center)
                    .Width(20)
                    .Color(new Style(ConsoleColor.Magenta));

                AnsiConsole.Write(rows);
                AnsiConsole.Write(columns);
                AnsiConsole.Write(grid);
                AnsiConsole.Write(layout);
                AnsiConsole.Write(align);

                VisualAssert.ContainsText(scope.Driver, "Summary");
                VisualAssert.ContainsText(scope.Driver, "Service");
                VisualAssert.ContainsText(scope.Driver, "Build");
                VisualAssert.ContainsText(scope.Driver, "Running");
                VisualAssert.ContainsText(scope.Driver, "Centered");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Summary") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Service") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("API ready") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Running") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Centered") && write.Color == ConsoleColor.Magenta);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 52, "Composed visual line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Composition builders can project model-backed Spectre-style colors without wrapper styles.
        /// </summary>
        [Fact]
        public void CompositionBuilders_ColorSelectors_AcceptColorAndRgbColorValues()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var sections = new[]
                {
                    new CompositionColorSection("API", "Service", SpectreColor.Blue, RgbColor.Aqua),
                    new CompositionColorSection("Worker", "Background", SpectreColor.Red, RgbColor.Yellow)
                };

                var rows = new Rows()
                    .Width(60)
                    .Spacing(0)
                    .AddRows(sections, section => "Row color " + section.Name, section => section.Color)
                    .AddRows(sections, section => "Row rgb " + section.Name, section => section.RgbColor);

                var columns = new Columns()
                    .Width(60)
                    .ColumnCount(2)
                    .MinimumColumnWidth(10)
                    .Gutter(1)
                    .AddColumns(sections, section => "Column color " + section.Name, section => section.Title, section => section.Color)
                    .AddColumns(sections, section => "Column rgb " + section.Name, section => section.Title + " RGB", section => section.RgbColor);

                var grid = new Grid()
                    .Width(60)
                    .AddColumns(2)
                    .MinimumColumnWidth(10)
                    .Gutter(1)
                    .AddCells(sections, section => "Grid color " + section.Name, section => section.Title, section => section.Color)
                    .AddCells(sections, section => "Grid rgb " + section.Name, section => section.Title + " RGB", section => section.RgbColor);

                var layout = new Layout("Dashboard")
                    .Width(60)
                    .ColumnCount(2)
                    .MinimumPaneWidth(10)
                    .Gutter(1)
                    .AddPanes(sections, section => "Pane color " + section.Name, section => section.Title, section => section.Color)
                    .AddPanes(sections, section => "Pane rgb " + section.Name, section => section.Title + " RGB", section => section.RgbColor);

                var layoutEntry = new LayoutEntry(new LayoutOptions
                {
                    Width = 60,
                    Columns = 2,
                    MinimumPaneWidth = 10,
                    Gutter = 1
                });
                layoutEntry.AddPanes(sections, section => "Entry color " + section.Name, section => section.Title, section => section.Color);
                layoutEntry.AddPanes(sections, section => "Entry rgb " + section.Name, section => section.Title + " RGB", section => section.RgbColor);

                AnsiConsole.Write(rows);
                AnsiConsole.Write(columns);
                AnsiConsole.Write(grid);
                AnsiConsole.Write(layout);
                layoutEntry.Render();

                VisualAssert.ContainsText(scope.Driver, "Row color API");
                VisualAssert.ContainsText(scope.Driver, "Column rgb Worker");
                VisualAssert.ContainsText(scope.Driver, "Grid color API");
                VisualAssert.ContainsText(scope.Driver, "Pane rgb Worker");
                VisualAssert.ContainsText(scope.Driver, "Entry color Worker");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Row color API") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Row rgb API") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Column color Worker") && write.Color == ConsoleColor.Red);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Column rgb Worker") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Grid color API") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Grid rgb API") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Pane color Worker") && write.Color == ConsoleColor.Red);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Pane rgb Worker") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Entry color API") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Entry rgb API") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 60, "Color-projected composition line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Calendar and terminal-art color helpers accept reusable styles for quick branded POC views.
        /// </summary>
        [Fact]
        public void CalendarAndFigletBuilders_ColorHelpers_AcceptStyleObjects()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var calendar = new Calendar(new DateTime(2026, 6, 1))
                    .Width(32)
                    .Culture(CultureInfo.InvariantCulture)
                    .FirstDayOfWeek(DayOfWeek.Monday)
                    .ShowAdjacentDays()
                    .HighlightToday(false)
                    .HeaderColor(new Style(ConsoleColor.Yellow))
                    .WeekdayColor(new Style(ConsoleColor.DarkGray))
                    .DayColor(new Style(ConsoleColor.White))
                    .AdjacentDayColor(new Style(ConsoleColor.DarkBlue))
                    .TodayColor(new Style(ConsoleColor.Green))
                    .MarkedDayColor(new Style(ConsoleColor.Magenta))
                    .AddCalendarEvent(new DateTime(2026, 6, 15));
                var today = new Calendar(DateTime.Today)
                    .Width(32)
                    .Culture(CultureInfo.InvariantCulture)
                    .HighlightToday()
                    .TodayColor(new Style(ConsoleColor.Green));
                var figlet = new FigletText("Go")
                    .Width(24)
                    .LeftJustified()
                    .Character('#')
                    .Color(new Style(ConsoleColor.Cyan));

                AnsiConsole.Write(calendar);
                AnsiConsole.Write(today);
                AnsiConsole.Write(figlet);

                VisualAssert.ContainsText(scope.Driver, "June 2026");
                VisualAssert.ContainsText(scope.Driver, "15");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("June 2026") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Mon") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Trim() == "15" && write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Color == ConsoleColor.DarkBlue && !string.IsNullOrWhiteSpace(write.Text));
                Assert.Contains(scope.Driver.Writes, write => write.Text.Trim() == DateTime.Today.Day.ToString(CultureInfo.InvariantCulture) && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#") && write.Color == ConsoleColor.Cyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        private sealed class CompositionColorSection
        {
            public CompositionColorSection(string name, string title, SpectreColor color, RgbColor rgbColor)
            {
                Name = name;
                Title = title;
                Color = color;
                RgbColor = rgbColor;
            }

            public string Name { get; }

            public string Title { get; }

            public SpectreColor Color { get; }

            public RgbColor RgbColor { get; }
        }

        private sealed class CommonPaletteItem
        {
            public CommonPaletteItem(string name, string state, string shortcut, DateTime date, SpectreColor color, RgbColor rgbColor)
            {
                Name = name;
                State = state;
                Shortcut = shortcut;
                Date = date;
                Color = color;
                RgbColor = rgbColor;
            }

            public string Name { get; }

            public string State { get; }

            public string Shortcut { get; }

            public DateTime Date { get; }

            public SpectreColor Color { get; }

            public RgbColor RgbColor { get; }
        }
    }
}
