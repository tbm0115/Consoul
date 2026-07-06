using System;
using ConsoulLibrary.Color;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for grid view helpers.
    /// </summary>
    public class GridResponsiveRenderingTests
    {
        /// <summary>
        /// Grids wrap long content and stay inside the active terminal width.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(20)]
        [InlineData(32)]
        [InlineData(80)]
        public void Grid_LongContent_DoesNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Grid(
                    new GridOptions { Columns = 3, MinimumColumnWidth = 10 },
                    "Build queue contains a verylongtokenwithoutnaturalbreaks",
                    "Tests are passing",
                    "Deployment waiting for approval");

                VisualAssert.WroteText(scope.Driver, "Build");
                VisualAssert.WroteText(scope.Driver, "Tests");
                VisualAssert.WroteText(scope.Driver, "Deployment");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Grids honor configured width, headers, and content colors.
        /// </summary>
        [Fact]
        public void Grid_CustomOptions_RenderHeadersAndColors()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var options = new GridOptions
                {
                    Width = 36,
                    Columns = 2,
                    HeaderScheme = new ColorScheme
                    {
                        Color = ConsoleColor.Yellow,
                        BackgroundColor = ConsoleColor.Black
                    },
                    ContentScheme = new ColorScheme
                    {
                        Color = ConsoleColor.Cyan,
                        BackgroundColor = ConsoleColor.Black
                    }
                };

                Consoul.Grid(grid =>
                {
                    grid.Add("Green", "Status", ConsoleColor.Green);
                    grid.Add("42", "Jobs");
                }, options);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Status") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Green") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Jobs") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("42") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 36, "Grid line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Grid headers and cells can contain raw markup without leaking tags or measuring by tag length.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(24)]
        [InlineData(42)]
        public void Grid_RawMarkupHeadersAndCells_DoNotLeakTagsOrOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Grid(grid =>
                {
                    grid.Add("[green]Ready[/] for [[prod]]", "[cyan]Status[/]");
                    grid.Add("[white on navy]Approval required for deployment[/]", "[yellow]Gate[/]");
                }, new GridOptions { Columns = 2, MinimumColumnWidth = 10 });

                VisualAssert.WroteText(scope.Driver, "Ready");
                VisualAssert.WroteText(scope.Driver, "[prod]");
                VisualAssert.WroteText(scope.Driver, "Approval");
                Assert.DoesNotContain("[green]", scope.Driver.Text);
                Assert.DoesNotContain("[white on navy]", scope.Driver.Text);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Status") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Approval") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Collapsed grids measure raw-markup cells by visible text, not source tag length.
        /// </summary>
        [Fact]
        public void Grid_CollapsedWidth_MeasuresMarkupByVisibleText()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Consoul.Grid(grid =>
                {
                    grid.Add("[green]OK[/]");
                    grid.Add("[cyan]Go[/]");
                }, new GridOptions
                {
                    Columns = 2,
                    Expand = false,
                    Gutter = 0,
                    MinimumColumnWidth = 1
                });

                Assert.DoesNotContain("[green]", scope.Driver.Text);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 4, "Collapsed grid measured raw markup instead of visible text: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Grids stack into fewer columns when the terminal cannot fit the preferred column count.
        /// </summary>
        [Fact]
        public void Grid_NarrowWidth_StacksCells()
        {
            using (var scope = new ConsoleDriverScope(18))
            {
                Consoul.Grid(
                    new GridOptions { Columns = 3, MinimumColumnWidth = 12 },
                    "Alpha",
                    "Beta",
                    "Gamma");

                Assert.DoesNotContain(scope.Driver.Lines, line => line.Contains("Alpha") && line.Contains("Beta"));
                Assert.DoesNotContain(scope.Driver.Lines, line => line.Contains("Beta") && line.Contains("Gamma"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Grids can capture existing Consoul render helpers as composable cell content.
        /// </summary>
        [Fact]
        public void Grid_CapturesRenderableCellContent()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                Consoul.Grid(grid =>
                {
                    grid.Add(() => Consoul.BarChart(chart =>
                    {
                        chart.Add("Queued", 3, ConsoleColor.Yellow);
                        chart.Add("Done", 7, ConsoleColor.Green);
                    }, new BarChartOptions { Width = 28, MaximumValue = 10 }), "Work", ConsoleColor.Cyan, captureWidth: 32);

                    grid.Add(() => Consoul.TextBlock(
                        "Captured notes stay inside the grid cell.",
                        new TextBlockOptions { Width = 28 }), "Notes", ConsoleColor.Green, captureWidth: 32);
                }, new GridOptions { Columns = 2, MinimumColumnWidth = 18 });

                VisualAssert.WroteText(scope.Driver, "Work");
                VisualAssert.WroteText(scope.Driver, "Queued");
                VisualAssert.WroteText(scope.Driver, "Done");
                VisualAssert.WroteText(scope.Driver, "Notes");
                VisualAssert.WroteText(scope.Driver, "Captured");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Grids can project model collections into headed dashboard cells.
        /// </summary>
        [Fact]
        public void Grid_AddCells_ProjectsModelCollections()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var statuses = new[]
                {
                    new DashboardStatus("API", "Ready", false),
                    new DashboardStatus("Worker", "Waiting", true),
                    new DashboardStatus("Search", "Queued", true)
                };

                new Grid()
                    .Width(48)
                    .ColumnCount(3)
                    .AddCells(
                        statuses,
                        status => new Markup(status.Warning ? "[yellow]" + status.State + "[/]" : "[green]" + status.State + "[/]"),
                        status => status.Name,
                        status => status.Warning ? (ConsoleColor?)ConsoleColor.Yellow : ConsoleColor.Green)
                    .Render();

                VisualAssert.WroteText(scope.Driver, "API");
                VisualAssert.WroteText(scope.Driver, "Ready");
                VisualAssert.WroteText(scope.Driver, "Worker");
                VisualAssert.WroteText(scope.Driver, "Waiting");
                VisualAssert.WroteText(scope.Driver, "Search");
                VisualAssert.WroteText(scope.Driver, "Queued");
                Assert.DoesNotContain("ConsoulLibrary.", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed dynamic views can render grids as model-summary sections.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderGridSections()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var model = new DashboardModel
                    {
                        Status = "Green",
                        Jobs = 4,
                        Owner = "Platform"
                    };

                    var view = Consoul.View("Dashboard", model)
                        .RenderModel(dashboard =>
                        {
                            Consoul.Grid(grid =>
                            {
                                grid.Add(dashboard.Status, "Status", ConsoleColor.Green);
                                grid.Add(dashboard.Jobs.ToString(), "Jobs", ConsoleColor.Cyan);
                                grid.Add(dashboard.Owner, "Owner", ConsoleColor.Yellow);
                            }, new GridOptions { Columns = 3, MinimumColumnWidth = 10 });
                        })
                        .Option("Refresh", dashboard => dashboard.Jobs++);

                    view.Render();

                    VisualAssert.WroteText(scope.Driver, "Status");
                    VisualAssert.WroteText(scope.Driver, "Green");
                    VisualAssert.WroteText(scope.Driver, "Owner");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        private sealed class DashboardModel
        {
            public string Status { get; set; } = string.Empty;

            public int Jobs { get; set; }

            public string Owner { get; set; } = string.Empty;
        }

        private sealed class DashboardStatus
        {
            public DashboardStatus(string name, string state, bool warning)
            {
                Name = name;
                State = state;
                Warning = warning;
            }

            public string Name { get; private set; }

            public string State { get; private set; }

            public bool Warning { get; private set; }
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
