using System;
using System.Linq;
using ConsoulLibrary.Color;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for vertically stacked row helpers.
    /// </summary>
    public class RowsResponsiveRenderingTests
    {
        /// <summary>
        /// Rows wrap long content and stay inside the active terminal width.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(18)]
        [InlineData(32)]
        [InlineData(80)]
        public void Rows_LongContent_DoesNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Rows(
                    new RowsOptions { Width = width, Spacing = 0 },
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
        /// Rows honor configured width, spacing, and row-level colors.
        /// </summary>
        [Fact]
        public void Rows_CustomOptions_RenderSpacingAndColors()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Consoul.Rows(rows =>
                {
                    rows.Add("Ready", ConsoleColor.Green);
                    rows.Add("Waiting for approval");
                }, new RowsOptions
                {
                    Width = 30,
                    Spacing = 2,
                    ContentScheme = new ColorScheme
                    {
                        Color = ConsoleColor.Cyan,
                        BackgroundColor = ConsoleColor.Black
                    }
                });

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Waiting") && write.Color == ConsoleColor.Cyan);
                Assert.True(scope.Driver.Lines.Count(line => line.Length == 0) >= 2);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 30, "Rows line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Rows treat raw markup content as styled visible text when measuring, wrapping, and rendering.
        /// </summary>
        /// <param name="width">The configured rows width.</param>
        [Theory]
        [InlineData(24)]
        [InlineData(40)]
        public void Rows_RawMarkupContent_DoNotLeakTagsOrOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Rows(rows =>
                {
                    rows.Add("[green]Ready[/] for [[prod]]");
                    rows.Add("[white on navy]Approval required for a very long deployment label[/]");
                }, new RowsOptions
                {
                    Width = width,
                    Spacing = 0,
                    ContentScheme = Scheme(ConsoleColor.DarkGray)
                });

                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.ContainsText(scope.Driver, "[prod]");
                VisualAssert.ContainsText(scope.Driver, "Approval");
                Assert.DoesNotContain("[green]", scope.Driver.Text);
                Assert.DoesNotContain("[white on navy]", scope.Driver.Text);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[prod]") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Approval") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= width, "Raw markup rows line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Collapsed rows measure raw markup content by visible text.
        /// </summary>
        [Fact]
        public void Rows_CollapsedWidth_MeasuresMarkupByVisibleText()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Consoul.Rows(rows =>
                {
                    rows.Add("[green]OK[/]");
                    rows.Add("[cyan]Go[/]");
                }, new RowsOptions { Expand = false, Spacing = 0 });

                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 2, "Collapsed raw markup row exceeded visible content width: " + line));
                VisualAssert.ContainsText(scope.Driver, "OK");
                VisualAssert.ContainsText(scope.Driver, "Go");
                Assert.DoesNotContain("[green]", scope.Driver.Text);
                Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Rows can capture existing Consoul render helpers as composable row content.
        /// </summary>
        [Fact]
        public void Rows_CapturesRenderableRowContent()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                Consoul.Rows(rows =>
                {
                    rows.Add(() => Consoul.Panel("Build complete", title: "Summary", width: 34), ConsoleColor.Cyan, captureWidth: 38);
                    rows.Add(() => Consoul.PropertyList(list =>
                    {
                        list.Add("Status", "Ready", ConsoleColor.Green);
                        list.Add("Owner", "Platform");
                    }, new PropertyListOptions { Width = 34, LabelWidth = 8 }), ConsoleColor.Green, captureWidth: 38);
                }, new RowsOptions { Width = 42, Spacing = 1 });

                VisualAssert.WroteText(scope.Driver, "Summary");
                VisualAssert.WroteText(scope.Driver, "Build complete");
                VisualAssert.WroteText(scope.Driver, "Status");
                VisualAssert.WroteText(scope.Driver, "Ready");
                VisualAssert.WroteText(scope.Driver, "Owner");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Rows can project model collections into mixed text and renderable row content.
        /// </summary>
        [Fact]
        public void Rows_AddRows_ProjectsModelCollections()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var statuses = new[]
                {
                    new DashboardStatus("API", "Ready", false),
                    new DashboardStatus("Worker", "Waiting for approval", true)
                };

                new Rows()
                    .Width(40)
                    .Spacing(0)
                    .AddRows(
                        statuses,
                        status => new Markup(status.Warning ? "[yellow]" + status.Name + ": " + status.State + "[/]" : "[green]" + status.Name + ": " + status.State + "[/]"),
                        status => status.Warning ? (ConsoleColor?)ConsoleColor.Yellow : ConsoleColor.Green)
                    .Render();

                VisualAssert.WroteText(scope.Driver, "API");
                VisualAssert.WroteText(scope.Driver, "Ready");
                VisualAssert.WroteText(scope.Driver, "Worker");
                VisualAssert.WroteText(scope.Driver, "Waiting");
                Assert.DoesNotContain("ConsoulLibrary.", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed dynamic views can stack render helpers as compact dashboards.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderRowsSections()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var model = new DashboardModel
                    {
                        Status = "Green",
                        Owner = "Platform"
                    };

                    var view = Consoul.View("Dashboard", model)
                        .RenderModel(dashboard =>
                        {
                            Consoul.Rows(rows =>
                            {
                                rows.Add(() => Consoul.Rule("Status", width: 32));
                                rows.Add(() => Consoul.PropertyList(list =>
                                {
                                    list.Add("State", dashboard.Status, ConsoleColor.Green);
                                    list.Add("Owner", dashboard.Owner, ConsoleColor.Cyan);
                                }, new PropertyListOptions { Width = 32, LabelWidth = 8 }));
                            }, new RowsOptions { Width = 36, Spacing = 0 });
                        })
                        .Option("Refresh", dashboard => dashboard.Status = "Refreshed");

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

        private static ColorScheme Scheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        private sealed class DashboardModel
        {
            public string Status { get; set; } = string.Empty;

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
