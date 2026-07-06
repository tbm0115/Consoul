using System;
using ConsoulLibrary.Color;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for layout panes.
    /// </summary>
    public class LayoutResponsiveRenderingTests
    {
        /// <summary>
        /// Layout panes wrap long content and stay inside the active terminal width.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(24)]
        [InlineData(40)]
        [InlineData(100)]
        public void Layout_DashboardPanes_DoNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Layout(layout =>
                {
                    layout.Add("Green\nAll systems ready", "Status", ConsoleColor.Green);
                    layout.Add("Build queue contains a verylongtokenwithoutnaturalbreaks", "Pipeline", ConsoleColor.Cyan);
                    layout.Add("Platform team owns the deployment window", "Owner", ConsoleColor.Yellow);
                }, new LayoutOptions { Columns = 3, MinimumPaneWidth = 12 });

                VisualAssert.WroteText(scope.Driver, "Status");
                VisualAssert.WroteText(scope.Driver, "Pipeline");
                VisualAssert.WroteText(scope.Driver, "Owner");
                VisualAssert.WroteText(scope.Driver, "Green");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Layout pane titles and content can contain raw markup without leaking tags or overflowing.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(24)]
        [InlineData(44)]
        public void Layout_RawMarkupTitlesAndContent_DoNotLeakTagsOrOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Layout(layout =>
                {
                    layout.Add("[green]Ready[/] for [[prod]]", "[cyan]Status[/]");
                    layout.Add("[white on navy]Approval required for deployment[/]", "[yellow]Gate[/]");
                }, new LayoutOptions { Columns = 2, MinimumPaneWidth = 10 });

                VisualAssert.WroteText(scope.Driver, "Status");
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
        /// Layout panes use fewer columns when the terminal cannot fit the preferred count.
        /// </summary>
        [Fact]
        public void Layout_NarrowWidth_StacksPanes()
        {
            using (var scope = new ConsoleDriverScope(22))
            {
                Consoul.Layout(layout =>
                {
                    layout.Add("First content", "Alpha");
                    layout.Add("Second content", "Beta");
                    layout.Add("Third content", "Gamma");
                }, new LayoutOptions { Columns = 3, MinimumPaneWidth = 14 });

                Assert.DoesNotContain(scope.Driver.Lines, line => line.Contains("Alpha") && line.Contains("Beta"));
                Assert.DoesNotContain(scope.Driver.Lines, line => line.Contains("Beta") && line.Contains("Gamma"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Layout panes can capture existing Consoul render helpers as composable pane content.
        /// </summary>
        [Fact]
        public void Layout_CapturesRenderablePaneContent()
        {
            using (var scope = new ConsoleDriverScope(76))
            {
                Consoul.Layout(layout =>
                {
                    layout.Add(() => Consoul.Panel("Release train ready", title: "State", width: 30), "Summary", ConsoleColor.Cyan, captureWidth: 34);
                    layout.Add(() => Consoul.BreakdownChart(chart =>
                    {
                        chart.Add("Ready", 8, ConsoleColor.Green);
                        chart.Add("Blocked", 2, ConsoleColor.Red);
                    }, new BreakdownChartOptions { Width = 30 }), "Health", ConsoleColor.Green, captureWidth: 34);
                }, new LayoutOptions { Columns = 2, MinimumPaneWidth = 20 });

                VisualAssert.WroteText(scope.Driver, "Summary");
                VisualAssert.WroteText(scope.Driver, "State");
                VisualAssert.WroteText(scope.Driver, "Release train ready");
                VisualAssert.WroteText(scope.Driver, "Health");
                VisualAssert.WroteText(scope.Driver, "Ready");
                VisualAssert.WroteText(scope.Driver, "Blocked");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Layout panes can be projected from model collections without adapter loops.
        /// </summary>
        [Fact]
        public void Layout_AddPanes_ProjectsModelCollections()
        {
            var panes = new LayoutProjectionPane[]
            {
                new LayoutProjectionPane("Status", "Green and ready", ConsoleColor.Green),
                new LayoutProjectionPane("Jobs", new Text("Jobs: 4").Width(18), ConsoleColor.Cyan),
                new LayoutProjectionPane("Owner", 2, ConsoleColor.Yellow)
            };

            using (var scope = new ConsoleDriverScope(64))
            {
                Consoul.Layout(layout =>
                {
                    layout.AddPanes(
                        panes,
                        pane => pane.Content,
                        pane => pane.Title,
                        pane => pane.Color,
                        captureWidth: 24);
                }, new LayoutOptions { Columns = 3, MinimumPaneWidth = 14 });

                VisualAssert.WroteText(scope.Driver, "Status");
                VisualAssert.WroteText(scope.Driver, "Green and ready");
                VisualAssert.WroteText(scope.Driver, "Jobs");
                VisualAssert.WroteText(scope.Driver, "Jobs: 4");
                VisualAssert.WroteText(scope.Driver, "Owner");
                VisualAssert.WroteText(scope.Driver, "2");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Green and ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Jobs: 4") && write.Color == ConsoleColor.Cyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Fluent layouts keep AddPanes chainable and preserve nested layout pane names.
        /// </summary>
        [Fact]
        public void Layout_AddPanes_FluentBuilderPreservesNestedLayoutNames()
        {
            var panes = new LayoutProjectionPane[]
            {
                new LayoutProjectionPane("Summary", new Panel("Release train ready").Header("State").Width(26), ConsoleColor.Green),
                new LayoutProjectionPane(null!, new Layout("Nested").MinimumSize(14).Update("Nested model content"), ConsoleColor.Cyan)
            };

            using (var scope = new ConsoleDriverScope(54))
            {
                var layout = new Layout("Dashboard")
                    .Width(50)
                    .ColumnCount(2)
                    .MinimumPaneWidth(14)
                    .AddPanes(
                        panes,
                        pane => pane.Content,
                        pane => pane.Title,
                        pane => pane.Color,
                        captureWidth: 28);

                AnsiConsole.Write(layout);

                VisualAssert.WroteText(scope.Driver, "Summary");
                VisualAssert.WroteText(scope.Driver, "State");
                VisualAssert.WroteText(scope.Driver, "Release train ready");
                VisualAssert.WroteText(scope.Driver, "Nested");
                VisualAssert.WroteText(scope.Driver, "Nested model content");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Row-oriented layouts honor configured width and color schemes.
        /// </summary>
        [Fact]
        public void Layout_Rows_RenderConfiguredColors()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Consoul.Layout(layout =>
                {
                    layout.Add("Ready", "Status", ConsoleColor.Green);
                    layout.Add("Waiting for approval", "Deployment");
                }, new LayoutOptions
                {
                    Direction = LayoutDirection.Rows,
                    Width = 30,
                    TitleScheme = new ColorScheme
                    {
                        Color = ConsoleColor.Yellow,
                        BackgroundColor = ConsoleColor.Black
                    },
                    ContentScheme = new ColorScheme
                    {
                        Color = ConsoleColor.Cyan,
                        BackgroundColor = ConsoleColor.Black
                    }
                });

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Status") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Waiting") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 30, "Layout line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Nested layout panes recalculate captured content when the terminal width changes.
        /// </summary>
        [Fact]
        public void Layout_NestedLivePanes_RecalculateAfterResize()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var layout = new Layout("Root")
                    .MinimumPaneWidth(12)
                    .SplitColumns(
                        new Layout("Left"),
                        new Layout("Right"));

                layout["Left"].Update("Alpha verylongtokenwithoutnaturalbreaks");
                layout["Right"].Update(new Text("Beta waits for a narrow terminal").Width(null));

                AnsiConsole.Write(layout);
                VisualAssert.ContainsText(scope.Driver, "Alpha");
                VisualAssert.ContainsText(scope.Driver, "Beta");
                VisualAssert.NoOverflow(scope.Driver);

                scope.Driver.Resize(24);
                scope.Driver.Clear();

                AnsiConsole.Write(layout);

                VisualAssert.ContainsText(scope.Driver, "Alpha");
                VisualAssert.ContainsText(scope.Driver, "Beta");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed dynamic views can render layout panes as compact model summaries.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderLayoutPanes()
        {
            using (var scope = new ConsoleDriverScope(48))
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
                            Consoul.Layout(layout =>
                            {
                                layout.Add(dashboard.Status, "Status", ConsoleColor.Green);
                                layout.Add(dashboard.Jobs.ToString(), "Jobs", ConsoleColor.Cyan);
                                layout.Add(dashboard.Owner, "Owner", ConsoleColor.Yellow);
                            }, new LayoutOptions { Columns = 3, MinimumPaneWidth = 12 });
                        })
                        .Option("Refresh", dashboard => dashboard.Jobs++);

                    view.Render();

                    VisualAssert.WroteText(scope.Driver, "Dashboard");
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

        private sealed class LayoutProjectionPane
        {
            public LayoutProjectionPane(string title, object content, ConsoleColor? color)
            {
                Title = title;
                Content = content;
                Color = color;
            }

            public string Title { get; }

            public object Content { get; }

            public ConsoleColor? Color { get; }
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
