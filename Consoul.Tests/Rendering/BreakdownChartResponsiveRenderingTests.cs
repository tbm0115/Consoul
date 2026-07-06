using System;
using System.Collections.Generic;
using System.Linq;
using ConsoulLibrary.Color;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for breakdown chart view helpers.
    /// </summary>
    public class BreakdownChartResponsiveRenderingTests
    {
        /// <summary>
        /// Breakdown charts render stacked bars and legends within the active terminal width.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(18)]
        [InlineData(32)]
        [InlineData(80)]
        public void BreakdownChart_LongLabels_DoNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.BreakdownChart(chart =>
                {
                    chart.Add("Queued jobs with a very long label", 12, ConsoleColor.Yellow);
                    chart.Add("Running", 8, ConsoleColor.Cyan);
                    chart.Add("Failed", 2, ConsoleColor.Red);
                });

                VisualAssert.WroteText(scope.Driver, "Queued");
                VisualAssert.WroteText(scope.Driver, "Running");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Breakdown charts honor configured width, colors, and value formatters.
        /// </summary>
        [Fact]
        public void BreakdownChart_CustomOptions_RenderColorsAndFormattedValues()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var options = new BreakdownChartOptions
                {
                    Label = "Queue mix",
                    LabelAlignment = TextBlockAlignment.Center,
                    Width = 40,
                    SegmentCharacter = '=',
                    LabelScheme = new ColorScheme
                    {
                        Color = ConsoleColor.White,
                        BackgroundColor = ConsoleColor.Black
                    },
                    ValueScheme = new ColorScheme
                    {
                        Color = ConsoleColor.Cyan,
                        BackgroundColor = ConsoleColor.Black
                    },
                    ValueFormatter = value => value.ToString("0") + " items",
                    PercentageFormatter = value => value.ToString("0.0") + "%"
                };

                Consoul.BreakdownChart(chart =>
                {
                    chart.Add("Done", 75, ConsoleColor.Green);
                    chart.Add("Waiting", 25, ConsoleColor.DarkYellow);
                }, options);

                string labelLine = scope.Driver.Lines.First(line => line.Contains("Queue mix"));
                Assert.True(labelLine.IndexOf("Queue mix", StringComparison.Ordinal) > 10, "Expected centered chart label: " + labelLine);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Queue mix") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Done") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("75 items") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 40, "Breakdown chart line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Chart labels use the same bounded width as bars and legend rows at narrow and normal terminal widths.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(16)]
        [InlineData(24)]
        [InlineData(48)]
        public void BreakdownChart_Label_DoesNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                var chart = new BreakdownChart()
                    .Width(width + 12)
                    .Label("Build queue distribution and release health")
                    .LabelAlignment(Justify.Right)
                    .HideValues()
                    .ShowPercentage()
                    .AddItem("Queued jobs with a very long label", 12, ConsoleColor.Yellow)
                    .AddItem("Running", 8, ConsoleColor.Cyan)
                    .AddItem("Failed", 2, ConsoleColor.Red);

                AnsiConsole.Write(chart);

                VisualAssert.ContainsText(scope.Driver, "Build");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build"));
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= width, "Breakdown chart line exceeded terminal width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Breakdown chart titles and legend labels treat raw markup as styled visible text.
        /// </summary>
        /// <param name="width">The configured chart width.</param>
        [Theory]
        [InlineData(24)]
        [InlineData(40)]
        public void BreakdownChart_RawMarkupLabels_DoNotLeakTagsOrOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.BreakdownChart(chart =>
                {
                    chart.Add("[yellow]Queued[/]", 4, ConsoleColor.Yellow);
                    chart.Add("[green]Running jobs with very long label[/]", 2, ConsoleColor.Green);
                    chart.Add("[red]Failed [[prod]][/]", 1, ConsoleColor.Red);
                }, new BreakdownChartOptions
                {
                    Width = width,
                    Label = "[cyan]Queue mix[/]",
                    LabelScheme = new ColorScheme { Color = ConsoleColor.DarkGray }
                });

                VisualAssert.ContainsText(scope.Driver, "Queue mix");
                VisualAssert.ContainsText(scope.Driver, "Queued");
                VisualAssert.ContainsText(scope.Driver, "Running");
                VisualAssert.ContainsText(scope.Driver, "Failed");
                VisualAssert.ContainsText(scope.Driver, "[prod]");
                Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                Assert.DoesNotContain("[yellow]", scope.Driver.Text);
                Assert.DoesNotContain("[green]", scope.Driver.Text);
                Assert.DoesNotContain("[red]", scope.Driver.Text);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Queue mix") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Queued") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Running") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Failed") && write.Color == ConsoleColor.Red);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= width, "Raw markup breakdown chart line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Breakdown charts handle zero and negative values without overflowing or throwing.
        /// </summary>
        [Fact]
        public void BreakdownChart_ZeroAndNegativeValues_RenderLegendWithoutSegments()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                Consoul.BreakdownChart(chart =>
                {
                    chart.Add("Zero", 0);
                    chart.Add("Negative", -4);
                });

                VisualAssert.WroteText(scope.Driver, "Zero");
                VisualAssert.WroteText(scope.Driver, "Negative");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed dynamic views can render breakdown charts as composition sections.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderBreakdownChartSections()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var model = new QueueModel
                    {
                        Queued = 4,
                        Running = 2,
                        Failed = 1
                    };

                    var view = Consoul.View("Queue", model)
                        .RenderModel(queue =>
                        {
                            Consoul.BreakdownChart(chart =>
                            {
                                chart.Add("Queued", queue.Queued, ConsoleColor.Yellow);
                                chart.Add("Running", queue.Running, ConsoleColor.Cyan);
                                chart.Add("Failed", queue.Failed, ConsoleColor.Red);
                            }, new BreakdownChartOptions { Width = 36 });
                        })
                        .Option("Refresh", queue => queue.Running++);

                    view.Render();

                    VisualAssert.WroteText(scope.Driver, "Queued");
                    VisualAssert.WroteText(scope.Driver, "Running");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#") && write.Color == ConsoleColor.Yellow);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Enumerable.Empty<string>()));
                }
            }
        }

        private sealed class QueueModel
        {
            public int Queued { get; set; }

            public int Running { get; set; }

            public int Failed { get; set; }
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
