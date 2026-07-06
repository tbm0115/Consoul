using System;
using ConsoulLibrary.Color;
using ConsoulLibrary.Tests.Rendering;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;
using Xunit;

namespace ConsoulLibrary.Tests.Compatibility
{
    /// <summary>
    /// Tests dashboard and chart builder compatibility with reusable style objects.
    /// </summary>
    public class DashboardStyleObjectBuilderCompatibilityTests
    {
        /// <summary>
        /// Metrics color helpers accept reusable styles and keep responsive rendering bounded.
        /// </summary>
        [Fact]
        public void Metrics_ColorHelpers_AcceptStyleObjects()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var metrics = new Metrics()
                    .Width(56)
                    .ColumnCount(2)
                    .MinimumColumnWidth(16)
                    .ColumnGap(2)
                    .LabelColor(Foreground(RgbColor.DarkGray))
                    .ValueColor(new Style(RgbColor.White, RgbColor.Navy))
                    .InfoColor(Foreground(RgbColor.Aqua))
                    .SuccessColor(Foreground(RgbColor.Green))
                    .WarningColor(Foreground(RgbColor.Yellow))
                    .ErrorColor(Foreground(RgbColor.Red))
                    .NoteColor(Foreground(RgbColor.DarkGray))
                    .AddMetric("Total", 10, "jobs")
                    .AddInfo("Queued", 3, "items", "fresh")
                    .AddSuccess("Passing", 8, "jobs", "healthy")
                    .AddWarning("Waiting", 2, note: "approval")
                    .AddError("Failing", 1, note: "blocked");

                AnsiConsole.Write(metrics);

                VisualAssert.ContainsText(scope.Driver, "Total");
                VisualAssert.ContainsText(scope.Driver, "Failing");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Total") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("10 jobs") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("3 items") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("8 jobs") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("2") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("1") && write.Color == ConsoleColor.Red);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("healthy") && write.Color == ConsoleColor.DarkGray);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 56, "Metrics line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Sparkline color helpers accept reusable styles and render through the responsive entry pipeline.
        /// </summary>
        [Fact]
        public void Sparkline_ColorHelpers_AcceptStyleObjects()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var sparkline = new Sparkline(new[] { 1d, 2d, 4d })
                    .Width(36)
                    .Label("Latency")
                    .LabelWidth(8)
                    .Range(0, 4)
                    .ShowRange()
                    .Characters("abc")
                    .ValueFormatter(value => value.ToString("0"))
                    .LabelColor(Foreground(RgbColor.Yellow))
                    .LineColor(Foreground(RgbColor.Green))
                    .RangeColor(Foreground(RgbColor.Aqua));

                AnsiConsole.Write(sparkline);

                VisualAssert.ContainsText(scope.Driver, "Latency");
                VisualAssert.ContainsText(scope.Driver, "0..4");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Latency") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("c") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("0..4") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 36, "Sparkline line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Sparkline color helpers accept Spectre-style color values without wrapper styles.
        /// </summary>
        [Fact]
        public void Sparkline_ColorHelpers_AcceptColorAndRgbColorValues()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var colorSparkline = new Sparkline(new[] { 1d, 2d, 4d })
                    .Width(40)
                    .Label("Color")
                    .LabelWidth(8)
                    .Range(0, 4)
                    .ShowRange()
                    .Characters("abc")
                    .ValueFormatter(value => value.ToString("0"))
                    .LabelColor(SpectreColor.Yellow)
                    .LineColor(RgbColor.Green)
                    .RangeColor(SpectreColor.Aqua);

                var rgbSparkline = new Sparkline(new[] { 0d, 1d, 2d })
                    .Width(40)
                    .Label("Rgb")
                    .LabelWidth(8)
                    .Range(0, 2)
                    .ShowRange()
                    .Characters("xyz")
                    .ValueFormatter(value => value.ToString("0"))
                    .LabelColor(RgbColor.White)
                    .LineColor(SpectreColor.Red)
                    .RangeColor(RgbColor.DarkGray);

                AnsiConsole.Write(colorSparkline);
                AnsiConsole.Write(rgbSparkline);

                VisualAssert.ContainsText(scope.Driver, "Color");
                VisualAssert.ContainsText(scope.Driver, "Rgb");
                VisualAssert.ContainsText(scope.Driver, "0..4");
                VisualAssert.ContainsText(scope.Driver, "0..2");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Color") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("c") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("0..4") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Rgb") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("z") && write.Color == ConsoleColor.Red);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("0..2") && write.Color == ConsoleColor.DarkGray);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 40, "Sparkline color line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Bar and breakdown chart color helpers accept reusable styles without bypassing responsive rendering.
        /// </summary>
        [Fact]
        public void Charts_ColorHelpers_AcceptStyleObjects()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var bar = new BarChart()
                    .Width(40)
                    .LabelWidth(8)
                    .MaximumValue(10)
                    .BarCharacters('=')
                    .ValueFormatter(value => value.ToString("0") + " jobs")
                    .LabelColor(Foreground(RgbColor.Yellow))
                    .BarColor(Foreground(RgbColor.Green))
                    .ValueColor(Foreground(RgbColor.Aqua))
                    .AddItem("Passing", 8)
                    .AddItem("Failing", 2);

                var breakdown = new BreakdownChart()
                    .Width(40)
                    .SegmentCharacters('=')
                    .ValueFormatter(value => value.ToString("0") + " jobs")
                    .PercentageFormatter(value => value.ToString("0") + "%")
                    .SegmentColor(Foreground(RgbColor.Green))
                    .LabelColor(Foreground(RgbColor.White))
                    .ValueColor(Foreground(RgbColor.Aqua))
                    .AddItem("Queued", 4)
                    .AddItem("Running", 2);

                AnsiConsole.Write(bar);
                AnsiConsole.Write(breakdown);

                VisualAssert.ContainsText(scope.Driver, "Passing");
                VisualAssert.ContainsText(scope.Driver, "Queued");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Passing") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("8 jobs") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Queued") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("4 jobs") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 40, "Chart line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Bar and breakdown chart color helpers accept Spectre-style color values without wrapper styles.
        /// </summary>
        [Fact]
        public void Charts_ColorHelpers_AcceptColorAndRgbColorValues()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                var slices = new[]
                {
                    new DashboardColorSlice("Blue", 5, SpectreColor.Blue, RgbColor.Aqua),
                    new DashboardColorSlice("Named", 3, SpectreColor.Red, RgbColor.Yellow)
                };

                var bar = new BarChart()
                    .Width(56)
                    .LabelWidth(14)
                    .MaximumValue(10)
                    .BarCharacters('=')
                    .LabelColor(SpectreColor.Yellow)
                    .BarColor(RgbColor.Green)
                    .ValueColor(SpectreColor.Aqua)
                    .AddItem("Direct color", 8, SpectreColor.Red)
                    .AddItem("Direct rgb", 6, RgbColor.Yellow)
                    .AddItems(slices, slice => "Color " + slice.Label, slice => slice.Value, slice => slice.Color)
                    .AddItems(slices, slice => "Rgb " + slice.Label, slice => slice.Value, slice => slice.RgbColor);
                bar.Add(BarChartItem.Create("Factory color", 4, SpectreColor.Magenta));
                bar.Add(BarChartItem.Create("Factory rgb", 3, RgbColor.DarkYellow));

                var breakdown = new BreakdownChart()
                    .Width(56)
                    .SegmentCharacters('=')
                    .SegmentColor(SpectreColor.Green)
                    .LabelColor(RgbColor.White)
                    .ValueColor(SpectreColor.Aqua)
                    .AddItem("Break color", 4, SpectreColor.Red)
                    .AddItem("Break rgb", 2, RgbColor.Yellow)
                    .AddItems(slices, slice => "Slice " + slice.Label, slice => slice.Value, slice => slice.Color)
                    .AddItems(slices, slice => "Slice rgb " + slice.Label, slice => slice.Value, slice => slice.RgbColor);
                breakdown.Add(BreakdownChartItem.Create("Break factory color", 4, SpectreColor.Magenta));
                breakdown.Add(BreakdownChartItem.Create("Break factory rgb", 3, RgbColor.DarkYellow));

                AnsiConsole.Write(bar);
                AnsiConsole.Write(breakdown);

                VisualAssert.ContainsText(scope.Driver, "Direct color");
                VisualAssert.ContainsText(scope.Driver, "Rgb Blue");
                VisualAssert.ContainsText(scope.Driver, "Break color");
                VisualAssert.ContainsText(scope.Driver, "Slice rgb Named");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.Red);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.DarkYellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Direct color") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Break color") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("8") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 56, "Color-projected chart line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Metrics and timeline builders accept Spectre-style color selectors from model-backed dashboard state.
        /// </summary>
        [Fact]
        public void MetricsAndTimeline_ColorSelectors_AcceptColorAndRgbColorValues()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var items = new[]
                {
                    new DashboardPaletteItem("API", 42, "jobs", "healthy", MetricKind.Success, TimelineItemState.Complete, SpectreColor.Blue, RgbColor.Aqua),
                    new DashboardPaletteItem("Worker", 7, "jobs", "waiting", MetricKind.Warning, TimelineItemState.Warning, SpectreColor.Red, RgbColor.Yellow)
                };

                var metrics = new Metrics()
                    .Width(64)
                    .ColumnCount(1)
                    .MinimumColumnWidth(16)
                    .AddMetrics(
                        items,
                        item => "Metric color " + item.Label,
                        item => item.Value,
                        item => item.Color,
                        item => item.Color,
                        item => item.Color,
                        unitSelector: item => item.Unit,
                        noteSelector: item => item.Note,
                        kindSelector: item => item.Kind)
                    .AddMetrics(
                        items,
                        item => "Metric rgb " + item.Label,
                        item => item.Value,
                        item => item.RgbColor,
                        item => item.RgbColor,
                        item => item.RgbColor,
                        unitSelector: item => item.Unit,
                        noteSelector: item => item.Note,
                        kindSelector: item => item.Kind);

                var timeline = new Timeline()
                    .Width(64)
                    .HideTimestamps()
                    .MarkerGap(1)
                    .Markers(complete: "ok", warning: "!!")
                    .AddSteps(
                        items,
                        item => "Step color " + item.Label,
                        item => item.Color,
                        item => item.Color,
                        item => item.Color,
                        stateSelector: item => item.State,
                        descriptionSelector: item => item.Note)
                    .AddSteps(
                        items,
                        item => "Step rgb " + item.Label,
                        item => item.RgbColor,
                        item => item.RgbColor,
                        item => item.RgbColor,
                        stateSelector: item => item.State,
                        descriptionSelector: item => item.Note);

                AnsiConsole.Write(metrics);
                AnsiConsole.Write(timeline);

                VisualAssert.ContainsText(scope.Driver, "Metric color API");
                VisualAssert.ContainsText(scope.Driver, "Metric rgb Worker");
                VisualAssert.ContainsText(scope.Driver, "Step color API");
                VisualAssert.ContainsText(scope.Driver, "Step rgb Worker");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Metric color API") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("42 jobs") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("healthy") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Metric rgb API") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Metric rgb Worker") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Step color Worker") && write.Color == ConsoleColor.Red);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Step rgb API") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Step rgb Worker") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("!!") && write.Color == ConsoleColor.Red);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("!!") && write.Color == ConsoleColor.Yellow);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 64, "Color-projected dashboard line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Dashboard item factories and per-item builders accept reusable style objects.
        /// </summary>
        [Fact]
        public void DashboardItemFactories_AcceptStyleObjects()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var bar = new BarChart()
                    .Width(42)
                    .LabelWidth(8)
                    .MaximumValue(10)
                    .BarCharacters('=')
                    .ValueFormatter(value => value.ToString("0"));
                bar.AddItem("Styled", 5, new Style(ConsoleColor.Magenta));
                bar.Add(BarChartItem.Create("Factory", 3, new Style(ConsoleColor.DarkYellow)));

                var breakdown = new BreakdownChart()
                    .Width(42)
                    .SegmentCharacters('=')
                    .ValueFormatter(value => value.ToString("0"))
                    .PercentageFormatter(value => value.ToString("0") + "%");
                breakdown.AddItem("Segment", 6, new Style(ConsoleColor.Blue));
                breakdown.Add(BreakdownChartItem.Create("Slice", 4, new Style(ConsoleColor.DarkGreen)));

                var metrics = new Metrics()
                    .Width(52)
                    .ColumnCount(1)
                    .MinimumColumnWidth(12)
                    .AddMetric(
                        "Latency",
                        42,
                        new Style(ConsoleColor.DarkGray),
                        new Style(ConsoleColor.Green),
                        new Style(ConsoleColor.Cyan),
                        unit: "ms",
                        note: "p95",
                        kind: MetricKind.Success);
                metrics.AddMetric(MetricItem.Create(
                    "Errors",
                    1,
                    new Style(ConsoleColor.Yellow),
                    new Style(ConsoleColor.Red),
                    new Style(ConsoleColor.DarkYellow),
                    note: "blocking",
                    kind: MetricKind.Error));

                var timeline = new Timeline()
                    .Width(52)
                    .HideTimestamps()
                    .MarkerGap(1)
                    .Markers(complete: "ok", warning: "!!");
                timeline.AddStep(
                    "Deploy",
                    new Style(ConsoleColor.White),
                    new Style(ConsoleColor.Green),
                    new Style(ConsoleColor.DarkGray),
                    TimelineItemState.Complete,
                    "Done");
                timeline.AddStep(TimelineItem.Create(
                    "Factory step",
                    new Style(ConsoleColor.Yellow),
                    new Style(ConsoleColor.Magenta),
                    new Style(ConsoleColor.Cyan),
                    TimelineItemState.Warning,
                    "From factory"));

                AnsiConsole.Write(bar);
                AnsiConsole.Write(breakdown);
                AnsiConsole.Write(metrics);
                AnsiConsole.Write(timeline);

                VisualAssert.ContainsText(scope.Driver, "Styled");
                VisualAssert.ContainsText(scope.Driver, "Segment");
                VisualAssert.ContainsText(scope.Driver, "Latency");
                VisualAssert.ContainsText(scope.Driver, "Factory step");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.DarkYellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.DarkGreen);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Latency") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("42 ms") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("p95") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Errors") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("1") && write.Color == ConsoleColor.Red);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("blocking") && write.Color == ConsoleColor.DarkYellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("ok") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Done") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Factory step") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("From factory") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 52, "Dashboard item line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Timeline color helpers accept reusable styles for every state marker and supporting text.
        /// </summary>
        [Fact]
        public void Timeline_ColorHelpers_AcceptStyleObjects()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var timeline = new Timeline()
                    .Width(52)
                    .HideTimestamps()
                    .MarkerGap(1)
                    .Markers(pending: "..", active: ">>", complete: "ok", warning: "!!", error: "xx", skipped: "--")
                    .Connector(":")
                    .TitleColor(Foreground(RgbColor.White))
                    .DescriptionColor(Foreground(RgbColor.DarkGray))
                    .PendingColor(Foreground(RgbColor.Grey))
                    .ActiveColor(Foreground(RgbColor.Aqua))
                    .CompleteColor(Foreground(RgbColor.Green))
                    .WarningColor(Foreground(RgbColor.Yellow))
                    .ErrorColor(Foreground(RgbColor.Red))
                    .SkippedColor(Foreground(RgbColor.DarkGray))
                    .ConnectorColor(Foreground(RgbColor.Olive))
                    .AddComplete("Build", "Artifacts ready")
                    .AddActive("Review", "Waiting for owner")
                    .AddPending("Publish")
                    .AddWarning("Approve", "Manual gate")
                    .AddError("Rollback", "Failed smoke")
                    .AddSkipped("Notify");

                AnsiConsole.Write(timeline);

                VisualAssert.ContainsText(scope.Driver, "Build");
                VisualAssert.ContainsText(scope.Driver, "Rollback");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("ok") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(">>") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("..") && write.Color == ConsoleColor.Gray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("!!") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("xx") && write.Color == ConsoleColor.Red);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("--") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Review") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Waiting for owner") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(":") && write.Color == ConsoleColor.DarkYellow);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 52, "Timeline line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        private static Style Foreground(RgbColor color)
        {
            return Style.Plain.WithForeground(color);
        }

        private sealed class DashboardColorSlice
        {
            public DashboardColorSlice(string label, double value, SpectreColor color, RgbColor rgbColor)
            {
                Label = label;
                Value = value;
                Color = color;
                RgbColor = rgbColor;
            }

            public string Label { get; }

            public double Value { get; }

            public SpectreColor Color { get; }

            public RgbColor RgbColor { get; }
        }

        private sealed class DashboardPaletteItem
        {
            public DashboardPaletteItem(
                string label,
                int value,
                string unit,
                string note,
                MetricKind kind,
                TimelineItemState state,
                SpectreColor color,
                RgbColor rgbColor)
            {
                Label = label;
                Value = value;
                Unit = unit;
                Note = note;
                Kind = kind;
                State = state;
                Color = color;
                RgbColor = rgbColor;
            }

            public string Label { get; }

            public int Value { get; }

            public string Unit { get; }

            public string Note { get; }

            public MetricKind Kind { get; }

            public TimelineItemState State { get; }

            public SpectreColor Color { get; }

            public RgbColor RgbColor { get; }
        }
    }
}
