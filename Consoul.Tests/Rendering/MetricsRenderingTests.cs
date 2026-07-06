using System;
using ConsoulLibrary.Color;
using Xunit;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for dashboard metric helpers.
    /// </summary>
    public class MetricsRenderingTests
    {
        /// <summary>
        /// Metrics wrap and reflow at multiple terminal widths without overflowing.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(14)]
        [InlineData(32)]
        [InlineData(80)]
        public void Metrics_ManyItems_DoNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Metrics(metrics =>
                {
                    metrics.Add("Builds", 42, note: "Today", kind: MetricKind.Success);
                    metrics.Add("Failures", 2, note: "Needs triage", kind: MetricKind.Warning);
                    metrics.Add("Latency", 128, "ms", "P95", MetricKind.Info);
                    metrics.Add("Queue depth", "verylongmetricvaluewithoutbreaks", note: "Workers pending", kind: MetricKind.Error);
                }, new MetricsOptions { Width = width, MinimumColumnWidth = 12 });

                VisualAssert.WroteText(scope.Driver, "Builds");
                VisualAssert.WroteText(scope.Driver, "Latency");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Metrics honor custom colors and explicit column counts.
        /// </summary>
        [Fact]
        public void Metrics_CustomOptions_ColorizeSegments()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var options = new MetricsOptions
                {
                    Width = 40,
                    Columns = 2,
                    ColumnGap = 4,
                    LabelScheme = Scheme(ConsoleColor.DarkGray),
                    SuccessScheme = Scheme(ConsoleColor.Green),
                    NoteScheme = Scheme(ConsoleColor.Cyan)
                };

                Consoul.Metrics(options,
                    new MetricItem("Ready", 12, note: "Healthy", kind: MetricKind.Success),
                    new MetricItem("Waiting", 3, note: "Approval"));

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("12") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Healthy") && write.Color == ConsoleColor.Cyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Metrics accept shared palette and RGB colors through the fluent color helpers.
        /// </summary>
        [Fact]
        public void Metrics_ColorHelpers_AcceptSharedPaletteColors()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var metrics = new Metrics()
                    .Width(64)
                    .ColumnCount(1)
                    .LabelColor(SpectreColor.White)
                    .ValueColor(RgbColor.Magenta)
                    .NoteColor(SpectreColor.Blue)
                    .InfoColor(RgbColor.Cyan)
                    .SuccessColor(SpectreColor.Green)
                    .WarningColor(RgbColor.Yellow)
                    .ErrorColor(SpectreColor.Red)
                    .AddMetric("Default Label", "BASE-VALUE", note: "base note")
                    .AddInfo("Info Label", "INFO-VALUE", note: "info note")
                    .AddSuccess("Success Label", "SUCCESS-VALUE", note: "success note")
                    .AddWarning("Warning Label", "WARNING-VALUE", note: "warning note")
                    .AddError("Error Label", "ERROR-VALUE", note: "error note");

                AnsiConsole.Write(metrics);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Default Label") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("BASE-VALUE") && write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("base note") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("INFO-VALUE") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("SUCCESS-VALUE") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("WARNING-VALUE") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("ERROR-VALUE") && write.Color == ConsoleColor.Red);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Metrics treat raw markup in labels, values, units, and notes as styled visible text.
        /// </summary>
        /// <param name="width">The configured metrics width.</param>
        [Theory]
        [InlineData(24)]
        [InlineData(40)]
        public void Metrics_RawMarkupTextSlots_DoNotLeakTagsOrOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Metrics(metrics =>
                {
                    metrics.Add(
                        "[cyan]Passing[/]",
                        "[green]Ready[/]",
                        "[white on navy]jobs[/]",
                        "[yellow]Needs [[prod]][/]",
                        MetricKind.Success);
                    metrics.Add(
                        "[red]Failures with a very long label[/]",
                        "[red]2[/]",
                        note: "[white on darkred]Investigate[/]",
                        kind: MetricKind.Error);
                }, new MetricsOptions
                {
                    Width = width,
                    Columns = 1,
                    LabelScheme = Scheme(ConsoleColor.DarkGray),
                    NoteScheme = Scheme(ConsoleColor.Gray)
                });

                VisualAssert.ContainsText(scope.Driver, "Passing");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.ContainsText(scope.Driver, "jobs");
                VisualAssert.ContainsText(scope.Driver, "Needs [prod]");
                VisualAssert.ContainsText(scope.Driver, "Failures");
                VisualAssert.ContainsText(scope.Driver, "Investigate");
                Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                Assert.DoesNotContain("[green]", scope.Driver.Text);
                Assert.DoesNotContain("[white on navy]", scope.Driver.Text);
                Assert.DoesNotContain("[yellow]", scope.Driver.Text);
                Assert.DoesNotContain("[red]", scope.Driver.Text);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Passing") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("jobs") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Needs [prod]") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Failures") && write.Color == ConsoleColor.Red);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Investigate") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkRed);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= width, "Raw markup metrics line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Metrics can hide notes for compact dashboard rows.
        /// </summary>
        [Fact]
        public void Metrics_HideNotes_OmitsNotes()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                Consoul.Metrics(metrics =>
                {
                    metrics.Add("Builds", 12, note: "Hidden note");
                    metrics.Add("Tests", 99, "%", note: "Also hidden");
                }, new MetricsOptions { ShowNotes = false, Width = 32 });

                VisualAssert.WroteText(scope.Driver, "Builds");
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("Hidden"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Metrics can be composed inside captured panels.
        /// </summary>
        [Fact]
        public void Metrics_ComposesInsidePanel()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Consoul.Panel(
                    () => Consoul.Metrics(metrics =>
                    {
                        metrics.Add("API", "Ready", kind: MetricKind.Success);
                        metrics.Add("Worker", "Warn", kind: MetricKind.Warning);
                    }, new MetricsOptions { Width = 32, Columns = 2 }),
                    title: "Health",
                    width: 40);

                VisualAssert.WroteText(scope.Driver, "Health");
                VisualAssert.WroteText(scope.Driver, "API");
                VisualAssert.WroteText(scope.Driver, "Worker");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed dynamic views can render metrics from model state.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderMetricSections()
        {
            using (var scope = new ConsoleDriverScope(44))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var model = new DeployMetrics { Builds = 42, Failures = 1, LatencyMs = 128 };
                    var view = Consoul.View("Metrics", model)
                        .RenderModel(values =>
                        {
                            Consoul.Metrics(metrics =>
                            {
                                metrics.Add("Builds", values.Builds, kind: MetricKind.Success);
                                metrics.Add("Failures", values.Failures, kind: values.Failures == 0 ? MetricKind.Success : MetricKind.Warning);
                                metrics.Add("Latency", values.LatencyMs, "ms", "P95", MetricKind.Info);
                            }, new MetricsOptions { Width = 40 });
                        })
                        .Option("Clear failures", values => values.Failures = 0);

                    view.Render();

                    VisualAssert.WroteText(scope.Driver, "Builds");
                    VisualAssert.WroteText(scope.Driver, "Latency");
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

        private sealed class DeployMetrics
        {
            public int Builds { get; set; }

            public int Failures { get; set; }

            public int LatencyMs { get; set; }
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
