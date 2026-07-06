using System;
using System.Threading;
using System.Threading.Tasks;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Tests live display dashboard rendering behavior.
    /// </summary>
    public class LiveDisplayRenderingTests
    {
        /// <summary>
        /// Live displays clear and re-render the latest dashboard state on each refresh.
        /// </summary>
        [Fact]
        public void LiveDisplay_RefreshClearsAndRendersLatestDashboard()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                int jobs = 1;

                using (LiveDisplayContext live = Consoul.Live(new LiveDisplayOptions
                {
                    Title = "Queue",
                    RenderTitleRule = false
                }))
                {
                    live.Refresh(() => RenderQueue(jobs));
                    jobs = 2;
                    live.Refresh(() => RenderQueue(jobs));

                    Assert.Equal(2, live.RefreshCount);
                }

                Assert.Equal(2, scope.Driver.ClearCount);
                VisualAssert.WroteText(scope.Driver, "Queue");
                VisualAssert.WroteText(scope.Driver, "Jobs");
                VisualAssert.WroteText(scope.Driver, "2");
                VisualAssert.ContainsText(scope.Driver, "2");
                Assert.DoesNotContain(scope.Driver.Lines, line => line.Contains("1"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Live displays can render a responsive title rule before dashboard content.
        /// </summary>
        [Fact]
        public void LiveDisplay_TitleRuleStaysWithinBounds()
        {
            using (var scope = new ConsoleDriverScope(24))
            {
                using (LiveDisplayContext live = Consoul.Live(new LiveDisplayOptions { Title = "Queue" }))
                {
                    live.Refresh(() => Consoul.Panel("Ready", title: "Status"));
                }

                VisualAssert.WroteText(scope.Driver, "Queue");
                VisualAssert.WroteText(scope.Driver, "Status");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Live action helpers dispose the active context after the operation exits.
        /// </summary>
        [Fact]
        public void Live_ActionHelper_DisposesContext()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                LiveDisplayContext captured = null!;

                Consoul.Live(live =>
                {
                    captured = live;
                    live.Refresh(() => Consoul.Write("Running"));
                    live.Succeed("Done");
                });

                Assert.NotNull(captured);
                Assert.True(captured.IsCompleted);
                Assert.Throws<ObjectDisposedException>(() => captured.Refresh(() => Consoul.Write("After")));
                VisualAssert.WroteText(scope.Driver, "Done");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Async live helpers pass the supplied cancellation token to the operation.
        /// </summary>
        [Fact]
        public async Task LiveAsync_ActionHelper_PassesCancellationToken()
        {
            using (var scope = new ConsoleDriverScope(40))
            using (var cancellation = new CancellationTokenSource())
            {
                CancellationToken observed = CancellationToken.None;

                await Consoul.LiveAsync((live, token) =>
                {
                    observed = token;
                    live.Refresh(() => Consoul.Write("Async dashboard"));
                    return Task.CompletedTask;
                }, cancellationToken: cancellation.Token);

                Assert.Equal(cancellation.Token, observed);
                VisualAssert.WroteText(scope.Driver, "Async dashboard");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Target-based live displays support Spectre-style refresh, target replacement, auto-clear, and result-returning starts.
        /// </summary>
        [Fact]
        public async Task LiveDisplay_TargetStartUpdateTargetAndResults_Work()
        {
            using (var scope = new ConsoleDriverScope(48, 12))
            {
                var target = new Text("Starting");

                int refreshes = AnsiConsole.Live(target, new LiveDisplayOptions { ClearBeforeRender = false })
                    .AutoClear(true)
                    .Start(live =>
                    {
                        Assert.Same(target, live.Target);
                        target.ContentText("Processing");
                        live.Refresh();
                        live.UpdateTarget(new Text("Complete"));
                        live.UpdateTarget(() => Consoul.Write("Action target"));
                        return live.RefreshCount;
                    });

                string asyncResult = await AnsiConsole.Live(new Text("Async starting"), new LiveDisplayOptions { ClearBeforeRender = false })
                    .StartAsync(async live =>
                    {
                        await Task.Delay(1);
                        live.UpdateTarget("Async complete");
                        return "ready";
                    });

                Assert.Equal(4, refreshes);
                Assert.Equal("ready", asyncResult);
                Assert.True(scope.Driver.ClearCount >= 1);
                VisualAssert.WroteText(scope.Driver, "Starting");
                VisualAssert.WroteText(scope.Driver, "Processing");
                VisualAssert.WroteText(scope.Driver, "Complete");
                VisualAssert.WroteText(scope.Driver, "Action target");
                VisualAssert.WroteText(scope.Driver, "Async starting");
                VisualAssert.WroteText(scope.Driver, "Async complete");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Mutable table row collections can drive target-based live displays.
        /// </summary>
        [Fact]
        public void LiveDisplay_TargetTableRowsMutateDuringRefresh()
        {
            using (var scope = new ConsoleDriverScope(48, 16))
            {
                var table = new Table()
                    .Width(42)
                    .HideFooter()
                    .AddColumns("Service", "State")
                    .AddRow("API", "Starting");

                int refreshes = AnsiConsole.Live(table)
                    .Start(live =>
                    {
                        table.Rows.Update(0, 1, new Markup("[green]Ready[/]"));
                        live.Refresh();

                        table.Rows.Clear();
                        table.Rows.Add("Worker", "Online");
                        live.Refresh();
                        return live.RefreshCount;
                    });

                Assert.Equal(3, refreshes);
                VisualAssert.WroteText(scope.Driver, "Starting");
                VisualAssert.WroteText(scope.Driver, "Ready");
                VisualAssert.WroteText(scope.Driver, "Worker");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Mutable nested layouts can drive target-based live dashboards.
        /// </summary>
        [Fact]
        public void LiveDisplay_TargetLayoutPanesMutateDuringRefresh()
        {
            using (var scope = new ConsoleDriverScope(54, 20))
            {
                var services = new Table()
                    .Width(28)
                    .HideFooter()
                    .AddColumns("Service", "State")
                    .AddRow("API", "Starting");

                var layout = new Layout("Dashboard")
                    .Width(48)
                    .MinimumPaneWidth(16)
                    .SplitRows(
                        new Layout("Header"),
                        new Layout("Body").SplitColumns(
                            new Layout("Status"),
                            new Layout("Metrics")));

                layout["Header"].Update("Starting");
                layout["Status"].Update(services);
                layout["Metrics"].Update("Jobs: 1");

                int refreshes = AnsiConsole.Live(layout)
                    .Start(live =>
                    {
                        layout["Header"].Update("Running");
                        services.Rows.Update(0, 1, new Markup("[green]Ready[/]"));
                        layout["Metrics"].Update("Jobs: 2");
                        live.Refresh();

                        layout["Status"].Update(new Text("Complete").Width(18));
                        live.Refresh();
                        return live.RefreshCount;
                    });

                Assert.Equal(3, refreshes);
                VisualAssert.WroteText(scope.Driver, "Starting");
                VisualAssert.WroteText(scope.Driver, "Running");
                VisualAssert.WroteText(scope.Driver, "Ready");
                VisualAssert.WroteText(scope.Driver, "Complete");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Target-based live display overflow options crop captured renderable output within the virtual terminal.
        /// </summary>
        [Fact]
        public void LiveDisplay_TargetOverflowCropping_StaysWithinBounds()
        {
            using (var scope = new ConsoleDriverScope(24, 4))
            {
                IConsoulRenderable target = Consoul.Renderable(() =>
                {
                    for (int index = 1; index <= 8; index++)
                    {
                        Consoul.Write("Line " + index);
                    }
                });

                AnsiConsole.Live(target, new LiveDisplayOptions { ClearBeforeRender = false })
                    .Overflow(VerticalOverflow.Crop)
                    .Cropping(VerticalOverflowCropping.Top)
                    .Start(live => { });

                VisualAssert.ContainsText(scope.Driver, "Line 8");
                Assert.DoesNotContain("Line 1", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Target-based live display ellipsis mode marks cropped output.
        /// </summary>
        [Fact]
        public void LiveDisplay_TargetOverflowEllipsis_ShowsMarker()
        {
            using (var scope = new ConsoleDriverScope(24, 3))
            {
                IConsoulRenderable target = Consoul.Renderable(() =>
                {
                    for (int index = 1; index <= 6; index++)
                    {
                        Consoul.Write("Item " + index);
                    }
                });

                AnsiConsole.Live(target, new LiveDisplayOptions { ClearBeforeRender = false })
                    .Overflow(VerticalOverflow.Ellipsis)
                    .Cropping(VerticalOverflowCropping.Bottom)
                    .Start(live => { });

                VisualAssert.ContainsText(scope.Driver, "...");
                Assert.DoesNotContain("Item 6", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        private static void RenderQueue(int jobs)
        {
            Consoul.PropertyList(list =>
            {
                list.Add("Jobs", jobs, ConsoleColor.Cyan);
                list.Add("State", jobs > 1 ? "Busy" : "Idle", ConsoleColor.Green);
            }, new PropertyListOptions { Width = 32, LabelWidth = 8 });
        }
    }
}
