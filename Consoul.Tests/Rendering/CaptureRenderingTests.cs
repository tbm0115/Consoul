using System;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Tests plain-text capture of Consoul render output.
    /// </summary>
    public class CaptureRenderingTests
    {
        /// <summary>
        /// Capturing render helpers records plain text without markup tags.
        /// </summary>
        [Fact]
        public void Capture_CapturesMarkupAndPanelAsPlainText()
        {
            string output = Consoul.Capture(() =>
            {
                Consoul.MarkupLine("Status: [green]Ready[/]");
                Consoul.Panel("Build complete", title: "Summary", width: 32);
            }, width: 40);

            Assert.Contains("Status: Ready", output);
            Assert.Contains("Summary", output);
            Assert.Contains("Build complete", output);
            Assert.DoesNotContain("[green]", output);
            AssertLinesStayWithinWidth(output, 40);
        }

        /// <summary>
        /// Capturing live output returns the final virtual screen after clear and refresh operations.
        /// </summary>
        [Fact]
        public void Capture_UsesFinalScreenAfterClear()
        {
            string output = Consoul.Capture(() =>
            {
                Consoul.Write("Before");
                Consoul.Live(live =>
                {
                    int count = 1;
                    live.Refresh(() => Consoul.Write("Count: " + count));
                    count = 2;
                    live.Refresh(() => Consoul.Write("Count: " + count));
                }, new LiveDisplayOptions { RenderTitleRule = false });
            }, width: 30);

            Assert.Contains("Count: 2", output);
            Assert.DoesNotContain("Before", output);
            Assert.DoesNotContain("Count: 1", output);
            AssertLinesStayWithinWidth(output, 30);
        }

        /// <summary>
        /// Capturing output scopes the virtual console and restores the previous driver afterward.
        /// </summary>
        [Fact]
        public void Capture_RestoresPreviousConsoleDriver()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                string output = Consoul.Capture(() => Consoul.Write("Captured"), width: 20);
                Consoul.Write("Visible");

                Assert.Contains("Captured", output);
                VisualAssert.WroteText(scope.Driver, "Visible");
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("Captured"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Renderable components can be captured without leaking writes to the outer console driver.
        /// </summary>
        [Fact]
        public void Capture_Renderable_CapturesReusableComponent()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                IConsoulRenderable widget = new PanelEntry(
                    "Reusable content",
                    new PanelOptions
                    {
                        Title = "Widget",
                        Width = 28
                    });

                string output = Consoul.Capture(widget, width: 32);

                Assert.Contains("Widget", output);
                Assert.Contains("Reusable content", output);
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("Reusable content"));

                Consoul.Render(widget);

                VisualAssert.WroteText(scope.Driver, "Reusable content");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Render actions can be promoted to reusable renderables for capture and composition.
        /// </summary>
        [Fact]
        public void RenderableAction_CapturesAndComposesAdHocVisual()
        {
            IConsoulRenderable summary = Consoul.Renderable(() =>
            {
                Consoul.Rule("Summary", width: 32);
                Consoul.PropertyList(list =>
                {
                    list.Add("Service", "API", ConsoleColor.Cyan);
                    list.Add("State", "Ready", ConsoleColor.Green);
                }, new PropertyListOptions { Width = 32, LabelWidth = 10 });
            });

            string output = Consoul.Capture(summary, width: 36);

            Assert.Contains("Summary", output);
            Assert.Contains("Service", output);
            Assert.Contains("Ready", output);
            AssertLinesStayWithinWidth(output, 36);

            using (var scope = new ConsoleDriverScope(48))
            {
                Consoul.Panel(summary, title: "Ad hoc", width: 42, captureWidth: 36);

                VisualAssert.WroteText(scope.Driver, "Ad hoc");
                VisualAssert.WroteText(scope.Driver, "Summary");
                VisualAssert.WroteText(scope.Driver, "Ready");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style renderable aliases can be captured, written, and adapted without implementing Consoul-specific names.
        /// </summary>
        [Fact]
        public void SpectreRenderableAlias_CapturesWritesAndAdaptsToConsoulRenderable()
        {
            IRenderable renderable = new SpectreOnlyRenderable("Spectre alias ready");

            Assert.IsAssignableFrom<IConsoulRenderable>(renderable);

            string output = Consoul.Capture(renderable, width: 36);
            string ansiOutput = AnsiConsole.Capture(renderable, width: 36);
            IConsoulRenderable adapted = Consoul.Renderable(renderable);

            Assert.Contains("Spectre alias ready", output);
            Assert.Contains("Spectre alias ready", ansiOutput);
            Assert.Contains("Spectre alias ready", Consoul.Capture(adapted, width: 36));
            AssertLinesStayWithinWidth(output, 36);

            using (var scope = new ConsoleDriverScope(48))
            {
                AnsiConsole.Write((object)renderable);
                AnsiConsole.WriteLine((object)renderable);
                AnsiConsole.Render(renderable);
                AnsiConsole.Console.Render(renderable);
                Consoul.Panel(adapted, title: "Alias", width: 36, captureWidth: 32);

                VisualAssert.WroteText(scope.Driver, "Spectre alias ready");
                VisualAssert.WroteText(scope.Driver, "Alias");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Renderable action factories reject null actions at the public API boundary.
        /// </summary>
        [Fact]
        public void RenderableAction_NullRender_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Consoul.Renderable((Action)null!));
            Assert.Throws<ArgumentNullException>(() => Consoul.Renderable((IRenderable)null!));
            Assert.Throws<ArgumentNullException>(() => Consoul.Capture((IRenderable)null!));
            Assert.Throws<ArgumentNullException>(() => Consoul.Render((IRenderable)null!));
            Assert.Throws<ArgumentNullException>(() => AnsiConsole.Write((IRenderable)null!));
            Assert.Throws<ArgumentNullException>(() => new RenderActionEntry(null));
        }

        /// <summary>
        /// Renderable components compose directly into rows, grids, layouts, and panels without adapter lambdas.
        /// </summary>
        [Fact]
        public void Renderables_ComposeIntoRowsGridLayoutAndPanel()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                var summary = new PanelEntry(
                    "Build ready",
                    new PanelOptions
                    {
                        Title = "Summary",
                        Width = 24
                    });

                var checklist = new ChecklistEntry(new ChecklistOptions { Width = 24 });
                checklist.Add("Tests", ChecklistItemState.Complete);
                checklist.Add("Docs", ChecklistItemState.Pending);

                var grid = new GridEntry(new GridOptions
                {
                    Width = 56,
                    Columns = 2,
                    MinimumColumnWidth = 20
                });
                grid.Add(summary, "Overview", ConsoleColor.Cyan, captureWidth: 24);
                grid.Add(checklist, "Tasks", ConsoleColor.Green, captureWidth: 24);

                var rows = new RowsEntry(new RowsOptions { Width = 60, Spacing = 1 });
                rows.Add(new RuleEntry("POC dashboard", new RuleOptions { Width = 60 }));
                rows.Add(grid, captureWidth: 56);

                Consoul.Panel(rows, title: "Release", width: 68, captureWidth: 60);

                VisualAssert.WroteText(scope.Driver, "Release");
                VisualAssert.WroteText(scope.Driver, "POC dashboard");
                VisualAssert.WroteText(scope.Driver, "Build ready");
                VisualAssert.WroteText(scope.Driver, "Tests");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Wrapper helpers accept reusable renderable components without requiring adapter lambdas.
        /// </summary>
        [Fact]
        public void RenderableWrappers_CaptureComponentsWithoutAdapterLambdas()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                var message = new TextBlockEntry(
                    "Reusable widget content",
                    new TextBlockOptions { Width = 28 });

                Consoul.Callout(message, CalloutKind.Success, new CalloutOptions { Width = 40 }, captureWidth: 28);
                Consoul.Pad(message, left: 2, width: 36, captureWidth: 28);
                Consoul.Align(message, TextBlockAlignment.Center, width: 48, captureWidth: 28);

                Assert.True(scope.Driver.Writes.Count > 0);
                VisualAssert.WroteText(scope.Driver, "Reusable widget content");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Banner entries can be captured and composed through the reusable renderable contract.
        /// </summary>
        [Fact]
        public void RenderableBanner_CapturesAndComposesAsHeaderWidget()
        {
            using (var banner = new BannerEntry("Release Dashboard"))
            {
                string output = Consoul.Capture(banner, width: 32);

                Assert.Contains("Release Dashboard", output);
                AssertLinesStayWithinWidth(output, 32);
            }

            using (var scope = new ConsoleDriverScope(48))
            using (var banner = new BannerEntry("Release Dashboard"))
            {
                Consoul.Panel(banner, title: "Header", width: 42, captureWidth: 36);

                VisualAssert.WroteText(scope.Driver, "Header");
                VisualAssert.WroteText(scope.Driver, "Release Dashboard");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Long banner titles are clipped to the available terminal width instead of overflowing.
        /// </summary>
        [Fact]
        public void RenderableBanner_LongTitle_StaysWithinTerminalWidth()
        {
            using (var scope = new ConsoleDriverScope(24))
            using (var banner = new BannerEntry("Long release dashboard heading that cannot fit")
            {
                Width = 80
            })
            {
                Consoul.Render(banner);

                VisualAssert.NoOverflow(scope.Driver);
                Assert.All(scope.Driver.Writes, write =>
                    Assert.True(write.Text.Length <= 24, "Banner write exceeded terminal width: " + write.Text));
            }
        }

        /// <summary>
        /// Manual tables render inline when passed through the reusable renderable contract.
        /// </summary>
        [Fact]
        public void RenderableTable_RendersInlineWithoutClearingExistingContent()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                var table = new TableView(new TableRenderOptions { ShowFooterHelp = false });
                table.AddHeaders("Service", "Status");
                table.AddRow(new[] { "API", "Ready" });

                Consoul.Write("Before table");
                Consoul.Render(table);

                VisualAssert.WroteText(scope.Driver, "Before table");
                VisualAssert.WroteText(scope.Driver, "Service");
                VisualAssert.WroteText(scope.Driver, "API");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed tables compose into higher-level renderables without losing their table formatting.
        /// </summary>
        [Fact]
        public void RenderableDynamicTable_ComposesInsideRowsAndPanels()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var services = new[]
                {
                    new RenderableService { Name = "API", Status = "Ready" },
                    new RenderableService { Name = "Worker", Status = "Pending" }
                };

                DynamicTableView<RenderableService> table = Consoul.Table(
                    services,
                    new TableRenderOptions { ShowFooterHelp = false },
                    service => service.Name,
                    service => service.Status);

                var rows = new RowsEntry(new RowsOptions { Width = 64, Spacing = 1 });
                rows.Add(new RuleEntry("Services", new RuleOptions { Width = 64 }));
                rows.Add(table, captureWidth: 64);

                Consoul.Panel(rows, title: "Dashboard", width: 72, captureWidth: 64);

                VisualAssert.WroteText(scope.Driver, "Dashboard");
                VisualAssert.WroteText(scope.Driver, "Services");
                VisualAssert.WroteText(scope.Driver, "Worker");
                VisualAssert.WroteText(scope.Driver, "Pending");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Capturing output restores the previous console driver when the render action throws.
        /// </summary>
        [Fact]
        public void Capture_RestoresPreviousConsoleDriverWhenRenderThrows()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                Assert.Throws<InvalidOperationException>(() => Consoul.Capture(() =>
                {
                    Consoul.Write("Captured");
                    throw new InvalidOperationException("Boom");
                }));

                Consoul.Write("After");
                VisualAssert.WroteText(scope.Driver, "After");
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("Captured"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        private static void AssertLinesStayWithinWidth(string output, int width)
        {
            Assert.All(output.Split(new[] { Environment.NewLine }, StringSplitOptions.None), line =>
                Assert.True(line.Length <= width, "Captured line exceeded width " + width + ": " + line));
        }

        private sealed class RenderableService
        {
            public string Name { get; set; } = string.Empty;

            public string Status { get; set; } = string.Empty;
        }

        private sealed class SpectreOnlyRenderable : IRenderable
        {
            private readonly string _text;

            public SpectreOnlyRenderable(string text)
            {
                _text = text;
            }

            public void Render()
            {
                Consoul.MarkupLine("[cyan]" + _text + "[/]");
            }
        }
    }
}
