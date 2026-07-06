using System;
using System.Collections.Generic;
using System.Linq;
using ConsoulLibrary.Color;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for bar chart view helpers.
    /// </summary>
    public class BarChartResponsiveRenderingTests
    {
        /// <summary>
        /// Bar charts render labels, bars, and values within the active terminal width.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(18)]
        [InlineData(32)]
        [InlineData(80)]
        public void BarChart_LongLabelsAndValues_DoNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.BarChart(chart =>
                {
                    chart.Add("Build jobs with a very long label", 12, ConsoleColor.Green);
                    chart.Add("Tests", 8, ConsoleColor.Cyan);
                    chart.Add("Deployments", 3, ConsoleColor.Yellow);
                });

                VisualAssert.WroteText(scope.Driver, "Build");
                VisualAssert.WroteText(scope.Driver, "Tests");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Bar charts honor configured width, schemes, and value formatting.
        /// </summary>
        [Fact]
        public void BarChart_CustomOptions_RenderColorsAndFormattedValues()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var options = new BarChartOptions
                {
                    Label = "Completion",
                    LabelAlignment = TextBlockAlignment.Right,
                    Width = 40,
                    LabelWidth = 8,
                    MaximumValue = 100,
                    BarCharacter = '=',
                    LabelScheme = new ColorScheme
                    {
                        Color = ConsoleColor.Yellow,
                        BackgroundColor = ConsoleColor.Black
                    },
                    ValueScheme = new ColorScheme
                    {
                        Color = ConsoleColor.Cyan,
                        BackgroundColor = ConsoleColor.Black
                    },
                    ValueFormatter = value => value.ToString("0") + "%"
                };

                Consoul.BarChart(chart =>
                {
                    chart.Add("Done", 75, ConsoleColor.Green);
                    chart.Add("Waiting", 25, ConsoleColor.DarkYellow);
                }, options);

                string labelLine = scope.Driver.Lines.First(line => line.Contains("Completion"));
                Assert.True(labelLine.IndexOf("Completion", StringComparison.Ordinal) > 20, "Expected right-aligned chart label: " + labelLine);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Completion") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Done") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("75%") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 40, "Bar chart line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Chart labels use the same bounded width as chart rows at narrow and normal terminal widths.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(16)]
        [InlineData(24)]
        [InlineData(48)]
        public void BarChart_Label_DoesNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                var chart = new BarChart()
                    .Width(width + 12)
                    .Label("Build queue throughput and release health")
                    .LabelAlignment(Justify.Right)
                    .MaximumValue(100)
                    .AddItem("Very long service name", 80, ConsoleColor.Green)
                    .AddItem(string.Empty, 20, ConsoleColor.Yellow);

                AnsiConsole.Write(chart);

                VisualAssert.ContainsText(scope.Driver, "Build");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build"));
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= width, "Bar chart line exceeded terminal width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Bar chart titles and row labels treat raw markup as styled visible text.
        /// </summary>
        /// <param name="width">The configured chart width.</param>
        [Theory]
        [InlineData(24)]
        [InlineData(40)]
        public void BarChart_RawMarkupLabels_DoNotLeakTagsOrOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.BarChart(chart =>
                {
                    chart.Add("[green]Passing[/]", 8, ConsoleColor.Green);
                    chart.Add("[red]Failing jobs with very long label[/]", 2, ConsoleColor.Red);
                }, new BarChartOptions
                {
                    Width = width,
                    Label = "[cyan]Queue [[prod]][/]",
                    LabelWidth = Math.Min(12, Math.Max(6, width / 3)),
                    MaximumValue = 10,
                    LabelScheme = new ColorScheme { Color = ConsoleColor.DarkGray }
                });

                VisualAssert.ContainsText(scope.Driver, "Queue [prod]");
                VisualAssert.ContainsText(scope.Driver, "Passing");
                VisualAssert.ContainsText(scope.Driver, "Failing");
                Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                Assert.DoesNotContain("[green]", scope.Driver.Text);
                Assert.DoesNotContain("[red]", scope.Driver.Text);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Queue") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Passing") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Failing") && write.Color == ConsoleColor.Red);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= width, "Raw markup bar chart line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Bar charts handle zero and negative values without overflowing or throwing.
        /// </summary>
        [Fact]
        public void BarChart_ZeroAndNegativeValues_RenderEmptyBars()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                Consoul.BarChart(chart =>
                {
                    chart.Add("Zero", 0);
                    chart.Add("Negative", -4);
                }, new BarChartOptions { MaximumValue = 10 });

                VisualAssert.WroteText(scope.Driver, "Zero");
                VisualAssert.WroteText(scope.Driver, "Negative");
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("#"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed dynamic views can render bar charts as metric sections.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderBarChartSections()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var model = new DashboardModel
                    {
                        Passing = 8,
                        Failing = 2
                    };

                    var view = Consoul.View("Dashboard", model)
                        .RenderModel(dashboard =>
                        {
                            Consoul.BarChart(chart =>
                            {
                                chart.Add("Passing", dashboard.Passing, ConsoleColor.Green);
                                chart.Add("Failing", dashboard.Failing, ConsoleColor.Red);
                            }, new BarChartOptions { MaximumValue = 10, Width = 36 });
                        })
                        .Option("Refresh", dashboard => dashboard.Passing++);

                    view.Render();

                    VisualAssert.WroteText(scope.Driver, "Passing");
                    VisualAssert.WroteText(scope.Driver, "Failing");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#") && write.Color == ConsoleColor.Green);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Enumerable.Empty<string>()));
                }
            }
        }

        private sealed class DashboardModel
        {
            public int Passing { get; set; }

            public int Failing { get; set; }
        }

        private sealed class TestRoutine : Routine
        {
            public TestRoutine(IEnumerable<string> inputs)
            {
                foreach (string input in inputs)
                {
                    Enqueue(new RoutineInput { Value = input });
                }
            }
        }
    }
}
