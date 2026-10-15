using System;
using System.Linq;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace ConsoulLibrary.Tests.Compatibility
{
    /// <summary>
    /// Tests Spectre-style dashboard widget builder compatibility.
    /// </summary>
    public class DashboardWidgetBuilderCompatibilityTests
    {
        /// <summary>
        /// Bar chart aliases render labels and alignment through the existing responsive chart renderer.
        /// </summary>
        [Fact]
        public void BarChart_FluentBuilder_AcceptsLabelAndAlignmentAliases()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var chart = new BarChart()
                    .Width(36)
                    .Label("Job Health")
                    .CenterLabel()
                    .MaximumValue(10)
                    .AddItem("Passing", 8, "green")
                    .AddItem("Failing", 2, "red");

                AnsiConsole.Write(chart);

                string labelLine = scope.Driver.Lines.First(line => line.Contains("Job Health"));
                Assert.Equal("Job Health", chart.Options.Label);
                Assert.Equal(TextBlockAlignment.Center, chart.Options.LabelAlignment);
                Assert.True(labelLine.IndexOf("Job Health", StringComparison.Ordinal) > 0, "Expected centered chart label: " + labelLine);
                VisualAssert.ContainsText(scope.Driver, "Passing");
                VisualAssert.ContainsText(scope.Driver, "Failing");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Job Health") && write.Color == RenderOptions.SubnoteScheme.Color);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#") && write.Color == ConsoleColor.Green);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 36, "Bar chart line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Breakdown chart aliases render labels, alignment, and singular percentage helpers through the responsive chart renderer.
        /// </summary>
        [Fact]
        public void BreakdownChart_FluentBuilder_AcceptsLabelAndPercentageAliases()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var chart = new BreakdownChart()
                    .Width(36)
                    .Label("Queue Mix")
                    .RightAlignLabel()
                    .HideValues()
                    .HidePercentage()
                    .ShowPercentage()
                    .AddItem("Queued", 4, "yellow")
                    .AddItem("Running", 2, "green");

                AnsiConsole.Write(chart);

                string labelLine = scope.Driver.Lines.First(line => line.Contains("Queue Mix"));
                Assert.Equal("Queue Mix", chart.Options.Label);
                Assert.Equal(TextBlockAlignment.Right, chart.Options.LabelAlignment);
                Assert.True(chart.Options.ShowPercentages);
                Assert.False(chart.Options.ShowValues);
                Assert.True(labelLine.IndexOf("Queue Mix", StringComparison.Ordinal) > 20, "Expected right-aligned chart label: " + labelLine);
                VisualAssert.ContainsText(scope.Driver, "Queued");
                VisualAssert.ContainsText(scope.Driver, "Running");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("%") && write.Color == RenderOptions.SubnoteScheme.Color);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#") && write.Color == ConsoleColor.Yellow);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 36, "Breakdown chart line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Chart Collapse helpers switch from terminal-width charts to measured content width for compact dashboard cards.
        /// </summary>
        [Fact]
        public void Charts_FluentBuilder_CollapseMeasuresContentWidth()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var bars = new BarChart()
                    .Width(40)
                    .Collapse()
                    .MinimumBarWidth(4)
                    .HideValue()
                    .AddItem("OK", 4, "green");

                var breakdown = new BreakdownChart()
                    .Width(40)
                    .Collapse()
                    .HideValue()
                    .HidePercentage()
                    .AddItem("Queued", 3, "yellow")
                    .AddItem("Run", 2, "green");

                AnsiConsole.Write(bars);
                AnsiConsole.Write(breakdown);

                Assert.Null(bars.Options.Width);
                Assert.Null(breakdown.Options.Width);
                Assert.False(bars.Options.Expand);
                Assert.False(breakdown.Options.Expand);
                Assert.False(bars.Options.Clone().Expand);
                Assert.False(breakdown.Options.Clone().Expand);
                VisualAssert.ContainsText(scope.Driver, "OK");
                VisualAssert.ContainsText(scope.Driver, "Queued");
                VisualAssert.ContainsText(scope.Driver, "Run");
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 9, "Collapsed chart line exceeded measured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Chart Expand helpers restore terminal-width rendering after a compact builder chain.
        /// </summary>
        [Fact]
        public void Charts_FluentBuilder_ExpandRestoresTerminalWidth()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                var bars = new BarChart()
                    .Collapse()
                    .Expand()
                    .HideValue()
                    .AddItem("OK", 4, "green");

                var breakdown = new BreakdownChart()
                    .Collapse()
                    .Expand()
                    .HideValue()
                    .HidePercentage()
                    .AddItem("Queued", 3, "yellow")
                    .AddItem("Run", 2, "green");

                AnsiConsole.Write(bars);
                AnsiConsole.Write(breakdown);

                Assert.True(bars.Options.Expand);
                Assert.True(breakdown.Options.Expand);
                Assert.Null(bars.Options.Width);
                Assert.Null(breakdown.Options.Width);
                Assert.Contains(scope.Driver.Lines, line => line.Length > 9 && line.Contains("OK"));
                Assert.Contains(scope.Driver.Lines, line => line.Length > 9 && line.Contains("#"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Chart builders can project model collections with per-item style strings.
        /// </summary>
        [Fact]
        public void Charts_FluentBuilder_AddItemsProjectsStyleStrings()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var jobs = new[]
                {
                    new DashboardChartSlice("Queued", 4, "white on navy"),
                    new DashboardChartSlice("Running", 2, "green")
                };

                var bars = new BarChart()
                    .Width(44)
                    .Label("Jobs")
                    .AddItems(
                        jobs,
                        job => "Bar " + job.Label,
                        job => job.Value,
                        job => job.StyleText);

                var breakdown = new BreakdownChart()
                    .Width(44)
                    .Label("Queue")
                    .ShowPercentage()
                    .AddItems(
                        jobs,
                        job => "Slice " + job.Label,
                        job => job.Value,
                        job => job.StyleText);

                AnsiConsole.Write(bars);
                AnsiConsole.Write(breakdown);

                VisualAssert.ContainsText(scope.Driver, "Bar Queued");
                VisualAssert.ContainsText(scope.Driver, "Bar Running");
                VisualAssert.ContainsText(scope.Driver, "Slice Queued");
                VisualAssert.ContainsText(scope.Driver, "Slice Running");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#") && write.Color == ConsoleColor.Green);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 44, "Projected chart line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Dashboard builders accept prepared item params for terse MVC-style POC summaries.
        /// </summary>
        [Fact]
        public void DashboardBuilders_FluentBuilder_AcceptsPreparedItemParams()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                var bars = new BarChart()
                    .Width(52)
                    .LabelWidth(12)
                    .MaximumValue(10)
                    .BarCharacters('=')
                    .AddItems(
                        BarChartItem.Create("Bar Build", 6, ConsoleColor.Green),
                        BarChartItem.Create("Bar Review", 3, "yellow"));

                var breakdown = new BreakdownChart()
                    .Width(52)
                    .SegmentCharacters('=')
                    .AddItems(
                        BreakdownChartItem.Create("Slice Passing", 7, ConsoleColor.Cyan),
                        BreakdownChartItem.Create("Slice Waiting", 3, "white on navy"));

                var metrics = new Metrics()
                    .Width(56)
                    .ColumnCount(1)
                    .MinimumColumnWidth(12)
                    .AddMetrics(
                        MetricItem.Create("Metric Build", 6, unit: "jobs", note: "ready", kind: MetricKind.Success, labelColor: ConsoleColor.Cyan, valueColor: ConsoleColor.Green, noteColor: ConsoleColor.DarkGray),
                        MetricItem.Create("Metric Review", 2, unit: "jobs", note: "waiting", kind: MetricKind.Warning, labelColor: ConsoleColor.Yellow, valueColor: ConsoleColor.Yellow, noteColor: ConsoleColor.Gray));

                var timeline = new Timeline()
                    .Width(56)
                    .HideTimestamps()
                    .Markers(complete: "ok", warning: "!!")
                    .AddSteps(
                        TimelineItem.Create("Step Build", TimelineItemState.Complete, "ready", titleColor: ConsoleColor.Green, markerColor: ConsoleColor.Green, descriptionColor: ConsoleColor.DarkGray),
                        TimelineItem.Create("Step Review", TimelineItemState.Warning, "waiting", titleColor: ConsoleColor.Yellow, markerColor: ConsoleColor.Yellow, descriptionColor: ConsoleColor.Gray));

                var shortcuts = new ShortcutBar()
                    .Width(56)
                    .AddShortcuts(
                        ShortcutItem.Create("Enter", "Select", keyColor: ConsoleColor.Cyan, descriptionColor: ConsoleColor.White),
                        ShortcutItem.Create("Esc", "Back", keyColor: ConsoleColor.Yellow, descriptionColor: ConsoleColor.Gray));

                AnsiConsole.Write(bars);
                AnsiConsole.Write(breakdown);
                AnsiConsole.Write(metrics);
                AnsiConsole.Write(timeline);
                AnsiConsole.Write(shortcuts);

                VisualAssert.ContainsText(scope.Driver, "Bar Build");
                VisualAssert.ContainsText(scope.Driver, "Bar Review");
                VisualAssert.ContainsText(scope.Driver, "Slice Passing");
                VisualAssert.ContainsText(scope.Driver, "Slice Waiting");
                VisualAssert.ContainsText(scope.Driver, "Metric Build");
                VisualAssert.ContainsText(scope.Driver, "Metric Review");
                VisualAssert.ContainsText(scope.Driver, "Step Build");
                VisualAssert.ContainsText(scope.Driver, "Step Review");
                VisualAssert.ContainsText(scope.Driver, "[Enter]");
                VisualAssert.ContainsText(scope.Driver, "Back");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Metric Build") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("6 jobs") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("ready") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Step Review") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("waiting") && write.Color == ConsoleColor.Gray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[Enter]") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Back") && write.Color == ConsoleColor.Gray);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 56, "Prepared dashboard item line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Metrics aliases render through the existing responsive metrics renderer.
        /// </summary>
        [Fact]
        public void Metrics_FluentBuilder_RendersWithAnsiConsoleWrite()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var metrics = new Metrics()
                    .Width(40)
                    .ColumnCount(2)
                    .MinimumColumnWidth(12)
                    .ColumnGap(2)
                    .LabelColor("darkgray")
                    .SuccessColor("green")
                    .WarningColor("yellow")
                    .NoteColor("aqua")
                    .AddSuccess("Passing", 8, "jobs", "healthy")
                    .AddWarning("Waiting", 2, note: "approval")
                    .AddMetricStyled("Manual", 3, "grey", "white on navy", "darkgray", "jobs", "direct");

                AnsiConsole.Write(metrics);

                VisualAssert.ContainsText(scope.Driver, "Passing");
                VisualAssert.ContainsText(scope.Driver, "Waiting");
                VisualAssert.ContainsText(scope.Driver, "Manual");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Passing") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("8 jobs") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("2") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Manual") && write.Color == ConsoleColor.Gray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("3 jobs") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("direct") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("healthy") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 40, "Metrics line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Metrics builders can project model collections without hand-written metric item loops.
        /// </summary>
        [Fact]
        public void Metrics_FluentBuilder_AddMetricsProjectsModelCollections()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var snapshots = new[]
                {
                    new DashboardMetric("Passing", 8, "jobs", "healthy", MetricKind.Success, ConsoleColor.Green, "grey", "white on navy", "darkgray"),
                    new DashboardMetric("Waiting", 2, "jobs", "approval", MetricKind.Warning, ConsoleColor.Yellow, "grey", "yellow", "aqua")
                };

                var metrics = new Metrics()
                    .Width(44)
                    .ColumnCount(2)
                    .MinimumColumnWidth(12)
                    .SuccessColor("green")
                    .WarningColor("yellow")
                    .NoteColor("aqua")
                    .AddMetrics(
                        snapshots,
                        snapshot => snapshot.Label,
                        snapshot => snapshot.Value,
                        snapshot => snapshot.Unit,
                        snapshot => snapshot.Note,
                        snapshot => snapshot.Kind)
                    .AddMetrics(
                        snapshots.Take(1),
                        snapshot => MetricItem.Create(
                            "Styled " + snapshot.Label,
                            snapshot.Value,
                            unit: snapshot.Unit,
                            note: "Projected item",
                            valueColor: ConsoleColor.Magenta,
                            noteColor: ConsoleColor.DarkGray))
                    .AddMetricsStyled(
                        snapshots.Take(1),
                        snapshot => "Styled text " + snapshot.Label,
                        snapshot => snapshot.Value,
                        snapshot => snapshot.Unit,
                        snapshot => "Style text " + snapshot.Note,
                        snapshot => snapshot.Kind,
                        snapshot => snapshot.LabelStyle,
                        snapshot => snapshot.ValueStyle,
                        snapshot => snapshot.NoteStyle);

                AnsiConsole.Write(metrics);

                VisualAssert.ContainsText(scope.Driver, "Passing");
                VisualAssert.ContainsText(scope.Driver, "Waiting");
                VisualAssert.ContainsText(scope.Driver, "Styled Passing");
                VisualAssert.ContainsText(scope.Driver, "Styled text Passing");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("8 jobs") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("2 jobs") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Projected item") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("8 jobs") && write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Styled text Passing") && write.Color == ConsoleColor.Gray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("8 jobs") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Style text healthy") && write.Color == ConsoleColor.DarkGray);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 44, "Projected metrics line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Metrics builders expose chainable Add aliases for quick MVC-style dashboard snippets.
        /// </summary>
        [Fact]
        public void Metrics_FluentBuilder_AddAliasesChainAndProjectModels()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var snapshots = new[]
                {
                    new DashboardMetric("Passing", 8, "jobs", "healthy", MetricKind.Success, ConsoleColor.Green, "grey", "white on navy", "darkgray"),
                    new DashboardMetric("Waiting", 2, "jobs", "approval", MetricKind.Warning, ConsoleColor.Yellow, "grey", "yellow", "aqua")
                };

                var metrics = new Metrics()
                    .Width(48)
                    .ColumnCount(1)
                    .MinimumColumnWidth(16);

                Assert.Same(metrics, metrics.Add("Passing", 8, "jobs", "healthy", MetricKind.Success, ConsoleColor.Cyan, ConsoleColor.Green, ConsoleColor.DarkGray));
                Assert.Same(metrics, metrics.Add(
                    MetricItem.Create(
                        "Prepared Review",
                        2,
                        unit: "jobs",
                        note: "waiting",
                        kind: MetricKind.Warning,
                        labelColor: ConsoleColor.Yellow,
                        valueColor: ConsoleColor.Yellow,
                        noteColor: ConsoleColor.Gray)));
                Assert.Same(metrics, metrics.Add(
                    snapshots,
                    snapshot => "Projected " + snapshot.Label,
                    snapshot => snapshot.Value,
                    snapshot => snapshot.Unit,
                    snapshot => snapshot.Note,
                    snapshot => snapshot.Kind));
                Assert.Same(metrics, metrics.Add(
                    snapshots.Take(1),
                    snapshot => MetricItem.Create(
                        "Factory " + snapshot.Label,
                        snapshot.Value,
                        unit: snapshot.Unit,
                        note: "factory",
                        valueColor: ConsoleColor.Magenta,
                        noteColor: ConsoleColor.DarkGray)));

                AnsiConsole.Write(metrics);

                VisualAssert.ContainsText(scope.Driver, "Passing");
                VisualAssert.ContainsText(scope.Driver, "Prepared Review");
                VisualAssert.ContainsText(scope.Driver, "Projected Waiting");
                VisualAssert.ContainsText(scope.Driver, "Factory Passing");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Passing") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("8 jobs") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("waiting") && write.Color == ConsoleColor.Gray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("factory") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("8 jobs") && write.Color == ConsoleColor.Magenta);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 48, "Add alias metrics line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Sparkline aliases render through the existing responsive sparkline renderer.
        /// </summary>
        [Fact]
        public void Sparkline_FluentBuilder_RendersWithAnsiConsoleWrite()
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
                    .LabelColor("yellow")
                    .LineColor("green")
                    .RangeColor("aqua");

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
        /// Sparkline builders can project model collections without hand-written value loops.
        /// </summary>
        [Fact]
        public void Sparkline_FluentBuilder_AddValuesProjectsModelCollections()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var samples = new[]
                {
                    new DashboardTrendPoint("Build", 12d),
                    new DashboardTrendPoint("Test", 18d),
                    new DashboardTrendPoint("Deploy", 10d)
                };

                var sparkline = new Sparkline()
                    .Width(36)
                    .Label("Duration")
                    .ShowRange()
                    .ValueFormatter(value => value.ToString("0") + "m")
                    .LineColor("green")
                    .RangeColor("aqua")
                    .AddValues(samples, sample => sample.Minutes);

                AnsiConsole.Write(sparkline);

                Assert.Equal(new[] { 12d, 18d, 10d }, sparkline.Values);
                VisualAssert.ContainsText(scope.Driver, "Duration");
                VisualAssert.ContainsText(scope.Driver, "10m..18m");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("10m..18m") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 36, "Projected sparkline line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Timeline aliases render through the existing responsive timeline renderer.
        /// </summary>
        [Fact]
        public void Timeline_FluentBuilder_RendersWithAnsiConsoleWrite()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var timeline = new Timeline()
                    .Width(40)
                    .HideTimestamps()
                    .MarkerGap(1)
                    .Markers(pending: "..", active: ">>", complete: "done")
                    .Connector(":")
                    .TitleColor("white")
                    .DescriptionColor("darkgray")
                    .CompleteColor("green")
                    .ActiveColor("aqua")
                    .ConnectorColor("olive")
                    .AddComplete("Build", "Artifacts ready")
                    .AddActive("Review", "Waiting for owner")
                    .AddStepStyled(
                        "Package",
                        TimelineItemState.Warning,
                        "Direct style text",
                        titleStyle: "white on navy",
                        markerStyle: "yellow",
                        descriptionStyle: "grey")
                    .AddPending("Publish");

                AnsiConsole.Write(timeline);

                VisualAssert.ContainsText(scope.Driver, "Build");
                VisualAssert.ContainsText(scope.Driver, "Review");
                VisualAssert.ContainsText(scope.Driver, "Package");
                VisualAssert.ContainsText(scope.Driver, "Publish");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("done") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Review") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Package") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Direct style text") && write.Color == ConsoleColor.Gray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Waiting for owner") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(":") && write.Color == ConsoleColor.DarkYellow);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 40, "Timeline line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Timeline builders can project model collections without hand-written timeline item loops.
        /// </summary>
        [Fact]
        public void Timeline_FluentBuilder_AddStepsProjectsModelCollections()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var steps = new[]
                {
                    new DashboardStep("Build", TimelineItemState.Complete, "Artifacts ready", new DateTimeOffset(2026, 6, 30, 9, 0, 0, TimeSpan.Zero), ConsoleColor.Green, "white on navy", "green", "grey"),
                    new DashboardStep("Review", TimelineItemState.Active, "Waiting for owner approval", new DateTimeOffset(2026, 6, 30, 9, 30, 0, TimeSpan.Zero), ConsoleColor.Cyan, "aqua", "aqua", "darkgray")
                };

                var timeline = new Timeline()
                    .Width(44)
                    .TimestampFormat("HH:mm")
                    .AddSteps(
                        steps,
                        step => step.Title,
                        step => step.State,
                        step => step.Description,
                        step => step.Timestamp)
                    .AddSteps(
                        steps.Take(1),
                        step => TimelineItem.Create(
                            step.Title + " colored",
                            step.State,
                            "Projected with item styling",
                            step.Timestamp.AddMinutes(5),
                            titleColor: step.Color,
                            markerColor: step.Color,
                            descriptionColor: ConsoleColor.DarkGray))
                    .AddStepsStyled(
                        steps.Take(1),
                        step => step.Title + " styled",
                        step => step.State,
                        step => "Styled " + step.Description,
                        step => step.Timestamp.AddMinutes(10),
                        step => step.TitleStyle,
                        step => step.MarkerStyle,
                        step => step.DescriptionStyle);

                AnsiConsole.Write(timeline);

                VisualAssert.ContainsText(scope.Driver, "Build");
                VisualAssert.ContainsText(scope.Driver, "Review");
                VisualAssert.ContainsText(scope.Driver, "09:00");
                VisualAssert.ContainsText(scope.Driver, "Build colored");
                VisualAssert.ContainsText(scope.Driver, "Build styled");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build colored") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Projected with item styling") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build styled") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Styled Artifacts ready") && write.Color == ConsoleColor.Gray);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 44, "Projected timeline line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Timeline builders expose chainable Add aliases for workflow summaries.
        /// </summary>
        [Fact]
        public void Timeline_FluentBuilder_AddAliasesChainAndProjectModels()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var steps = new[]
                {
                    new DashboardStep("Build", TimelineItemState.Complete, "Artifacts ready", new DateTimeOffset(2026, 6, 30, 9, 0, 0, TimeSpan.Zero), ConsoleColor.Green, "white on navy", "green", "grey"),
                    new DashboardStep("Review", TimelineItemState.Active, "Waiting for owner approval", new DateTimeOffset(2026, 6, 30, 9, 30, 0, TimeSpan.Zero), ConsoleColor.Cyan, "aqua", "aqua", "darkgray")
                };

                var timeline = new Timeline()
                    .Width(44)
                    .HideTimestamps()
                    .Markers(complete: "ok", active: "run", warning: "warn")
                    .Connector("|");

                Assert.Same(timeline, timeline.Add("Queued", TimelineItemState.Warning, "Waiting for capacity", titleColor: ConsoleColor.Yellow, markerColor: ConsoleColor.Yellow, descriptionColor: ConsoleColor.Gray));
                Assert.Same(timeline, timeline.Add(TimelineItem.Create(
                    "Prepared Build",
                    TimelineItemState.Complete,
                    "ready",
                    titleColor: ConsoleColor.Green,
                    markerColor: ConsoleColor.Green,
                    descriptionColor: ConsoleColor.DarkGray)));
                Assert.Same(timeline, timeline.Add(
                    steps,
                    step => "Projected " + step.Title,
                    step => step.State,
                    step => step.Description,
                    step => (DateTimeOffset?)null));
                Assert.Same(timeline, timeline.Add(
                    steps.Take(1),
                    step => TimelineItem.Create(
                        "Factory " + step.Title,
                        step.State,
                        "factory",
                        titleColor: step.Color,
                        markerColor: step.Color,
                        descriptionColor: ConsoleColor.DarkGray)));

                AnsiConsole.Write(timeline);

                VisualAssert.ContainsText(scope.Driver, "Queued");
                VisualAssert.ContainsText(scope.Driver, "Prepared Build");
                VisualAssert.ContainsText(scope.Driver, "Projected Review");
                VisualAssert.ContainsText(scope.Driver, "Factory Build");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Queued") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Prepared Build") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Waiting for owner approval"));
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("factory") && write.Color == ConsoleColor.DarkGray);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 44, "Add alias timeline line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Shortcut bar aliases render through the existing responsive shortcut bar renderer.
        /// </summary>
        [Fact]
        public void ShortcutBar_FluentBuilder_RendersWithAnsiConsoleWrite()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var shortcuts = new ShortcutBar()
                    .Width(36)
                    .KeyEdges("<", ">")
                    .KeyDescriptionSeparator(": ")
                    .ItemSeparator(" | ")
                    .Ellipsis("..")
                    .KeyColor("aqua")
                    .DescriptionColor("white")
                    .SeparatorColor("darkgray")
                    .DisabledColor("olive")
                    .AddShortcut("Enter", "Select")
                    .AddShortcut("Tab", "Next", "white on navy", "grey")
                    .AddShortcut("Esc", "Back")
                    .AddDisabled("F5", "Refresh preview");

                AnsiConsole.Write(shortcuts);

                VisualAssert.ContainsText(scope.Driver, "<Enter>");
                VisualAssert.ContainsText(scope.Driver, "<Tab>");
                VisualAssert.ContainsText(scope.Driver, "Select");
                VisualAssert.ContainsText(scope.Driver, "<F5>");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("<Enter>") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("<Tab>") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Next") && write.Color == ConsoleColor.Gray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Select") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("|") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("<F5>") && write.Color == ConsoleColor.DarkYellow);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 36, "Shortcut bar line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Shortcut bars can project model collections into action hints for MVC-style views.
        /// </summary>
        [Fact]
        public void ShortcutBar_FluentBuilder_AddShortcutsProjectsModelCollections()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var actions = new[]
                {
                    new DashboardShortcut("Enter", "Select", false, ConsoleColor.Cyan, "aqua", "white"),
                    new DashboardShortcut("Esc", "Back", false, ConsoleColor.Yellow, "white on navy", "grey"),
                    new DashboardShortcut("F5", "Refresh preview", true, ConsoleColor.Red, "red", "grey")
                };

                var shortcuts = new ShortcutBar()
                    .Width(44)
                    .KeyEdges("<", ">")
                    .ItemSeparator(" | ")
                    .AddShortcuts(
                        actions,
                        action => action.Key,
                        action => action.Description,
                        action => action.Disabled,
                        action => (ConsoleColor?)action.Color,
                        action => (ConsoleColor?)ConsoleColor.White)
                    .AddShortcuts(
                        actions.Take(2),
                        action => "S" + action.Key,
                        action => "Styled " + action.Description,
                        action => action.KeyStyle,
                        action => action.DescriptionStyle)
                    .AddShortcuts(
                        actions.Take(1),
                        action => "D" + action.Key,
                        action => "Disabled-aware " + action.Description,
                        action => action.Disabled,
                        action => action.KeyStyle,
                        action => action.DescriptionStyle)
                    .AddShortcuts(
                        actions.Take(1),
                        action => ShortcutItem.Create(
                            "?" + action.Key,
                            "Help for " + action.Description,
                            keyColor: ConsoleColor.Magenta,
                            descriptionColor: ConsoleColor.DarkGray));

                AnsiConsole.Write(shortcuts);

                VisualAssert.ContainsText(scope.Driver, "<Enter>");
                VisualAssert.ContainsText(scope.Driver, "Select");
                VisualAssert.ContainsText(scope.Driver, "<Esc>");
                VisualAssert.ContainsText(scope.Driver, "<F5>");
                VisualAssert.ContainsText(scope.Driver, "<SEnter>");
                VisualAssert.ContainsText(scope.Driver, "Styled Select");
                VisualAssert.ContainsText(scope.Driver, "<SEsc>");
                VisualAssert.ContainsText(scope.Driver, "<DEnter>");
                VisualAssert.ContainsText(scope.Driver, "<?Enter>");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("<Enter>") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("<Esc>") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("<SEnter>") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Styled Select") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("<SEsc>") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Styled Back") && write.Color == ConsoleColor.Gray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("<DEnter>") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Help for Select") && write.Color == ConsoleColor.DarkGray);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 44, "Projected shortcut bar line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Shortcut bars expose chainable Add aliases for compact view action hints.
        /// </summary>
        [Fact]
        public void ShortcutBar_FluentBuilder_AddAliasesChainAndProjectModels()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var actions = new[]
                {
                    new DashboardShortcut("Enter", "Select", false, ConsoleColor.Cyan, "aqua", "white"),
                    new DashboardShortcut("Esc", "Back", false, ConsoleColor.Yellow, "white on navy", "grey"),
                    new DashboardShortcut("F5", "Refresh preview", true, ConsoleColor.Red, "red", "grey")
                };

                var shortcuts = new ShortcutBar()
                    .Width(52)
                    .KeyEdges("[", "]")
                    .ItemSeparator(" | ");

                Assert.Same(shortcuts, shortcuts.Add("Enter", "Select", keyColor: ConsoleColor.Cyan, descriptionColor: ConsoleColor.White));
                Assert.Same(shortcuts, shortcuts.Add(ShortcutItem.Create("Esc", "Back", keyColor: ConsoleColor.Yellow, descriptionColor: ConsoleColor.Gray)));
                Assert.Same(shortcuts, shortcuts.Add(
                    actions,
                    action => "P" + action.Key,
                    action => "Projected " + action.Description,
                    action => action.Disabled,
                    action => (ConsoleColor?)action.Color,
                    action => (ConsoleColor?)ConsoleColor.White));
                Assert.Same(shortcuts, shortcuts.Add(
                    actions.Take(2),
                    action => "S" + action.Key,
                    action => "Styled " + action.Description,
                    action => action.KeyStyle,
                    action => action.DescriptionStyle));
                Assert.Same(shortcuts, shortcuts.Add(
                    actions.Take(1),
                    action => ShortcutItem.Create(
                        "?" + action.Key,
                        "Help for " + action.Description,
                        keyColor: ConsoleColor.Magenta,
                        descriptionColor: ConsoleColor.DarkGray)));

                AnsiConsole.Write(shortcuts);

                VisualAssert.ContainsText(scope.Driver, "[Enter]");
                VisualAssert.ContainsText(scope.Driver, "[Esc]");
                VisualAssert.ContainsText(scope.Driver, "[PF5]");
                VisualAssert.ContainsText(scope.Driver, "[SEsc]");
                VisualAssert.ContainsText(scope.Driver, "[?Enter]");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[Enter]") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[Esc]") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[PEnter]") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Projected Refresh preview") && write.Color == RenderOptions.SubnoteScheme.Color);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[SEsc]") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Help for Select") && write.Color == ConsoleColor.DarkGray);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 52, "Add alias shortcut bar line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        private sealed class DashboardStep
        {
            public DashboardStep(string title, TimelineItemState state, string description, DateTimeOffset timestamp, ConsoleColor color, string titleStyle = "", string markerStyle = "", string descriptionStyle = "")
            {
                Title = title;
                State = state;
                Description = description;
                Timestamp = timestamp;
                Color = color;
                TitleStyle = titleStyle;
                MarkerStyle = markerStyle;
                DescriptionStyle = descriptionStyle;
            }

            public string Title { get; }

            public TimelineItemState State { get; }

            public string Description { get; }

            public DateTimeOffset Timestamp { get; }

            public ConsoleColor Color { get; }

            public string TitleStyle { get; }

            public string MarkerStyle { get; }

            public string DescriptionStyle { get; }
        }

        private sealed class DashboardTrendPoint
        {
            public DashboardTrendPoint(string stage, double minutes)
            {
                Stage = stage;
                Minutes = minutes;
            }

            public string Stage { get; }

            public double Minutes { get; }
        }

        private sealed class DashboardMetric
        {
            public DashboardMetric(string label, int value, string unit, string note, MetricKind kind, ConsoleColor color, string labelStyle = "", string valueStyle = "", string noteStyle = "")
            {
                Label = label;
                Value = value;
                Unit = unit;
                Note = note;
                Kind = kind;
                Color = color;
                LabelStyle = labelStyle;
                ValueStyle = valueStyle;
                NoteStyle = noteStyle;
            }

            public string Label { get; }

            public int Value { get; }

            public string Unit { get; }

            public string Note { get; }

            public MetricKind Kind { get; }

            public ConsoleColor Color { get; }

            public string LabelStyle { get; }

            public string ValueStyle { get; }

            public string NoteStyle { get; }
        }

        private sealed class DashboardChartSlice
        {
            public DashboardChartSlice(string label, double value, string styleText)
            {
                Label = label;
                Value = value;
                StyleText = styleText;
            }

            public string Label { get; }

            public double Value { get; }

            public string StyleText { get; }
        }

        private sealed class DashboardShortcut
        {
            public DashboardShortcut(string key, string description, bool disabled, ConsoleColor color, string keyStyle = "", string descriptionStyle = "")
            {
                Key = key;
                Description = description;
                Disabled = disabled;
                Color = color;
                KeyStyle = keyStyle;
                DescriptionStyle = descriptionStyle;
            }

            public string Key { get; }

            public string Description { get; }

            public bool Disabled { get; }

            public ConsoleColor Color { get; }

            public string KeyStyle { get; }

            public string DescriptionStyle { get; }
        }
    }
}
