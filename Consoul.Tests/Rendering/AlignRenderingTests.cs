using System;
using ConsoulLibrary.Color;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for aligned text and captured render helpers.
    /// </summary>
    public class AlignRenderingTests
    {
        /// <summary>
        /// Aligned text stays inside the active terminal width.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(16)]
        [InlineData(32)]
        [InlineData(80)]
        public void Align_Text_DoesNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Align("This line is aligned inside the available width.", TextBlockAlignment.Center, width);

                VisualAssert.WroteText(scope.Driver, "This line");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Center and right alignment pad content within the configured width.
        /// </summary>
        [Fact]
        public void Align_Text_PadsWithinWidth()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                Consoul.Align("Center", TextBlockAlignment.Center, width: 16);
                Consoul.Align("Right", TextBlockAlignment.Right, width: 12);

                Assert.Contains(scope.Driver.Lines, line => line == "     Center     ");
                Assert.Contains(scope.Driver.Lines, line => line == "       Right");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Direct aligned text can contain raw markup without leaking tags or measuring by tag length.
        /// </summary>
        /// <param name="width">The configured alignment width.</param>
        [Theory]
        [InlineData(24)]
        [InlineData(32)]
        public void Align_RawMarkupText_AlignsByVisibleText(int width)
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Consoul.Align("[green]Ready[/] for [[prod]]", TextBlockAlignment.Center, width);

                VisualAssert.WroteText(scope.Driver, "Ready");
                VisualAssert.WroteText(scope.Driver, "[prod]");
                Assert.DoesNotContain("[green]", scope.Driver.Text);
                Assert.Contains(scope.Driver.Lines, line => line.Contains("Ready") && line.Length == width);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Captured render output is aligned as already-rendered plain text, even when it contains bracket-shaped text.
        /// </summary>
        [Fact]
        public void Align_CapturedOutput_KeepsLiteralBracketText()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Consoul.Align(
                    () => Consoul.Write("[green]literal[/]", ConsoleColor.Cyan, writeLine: true),
                    TextBlockAlignment.Center,
                    width: 28,
                    captureWidth: 20);

                VisualAssert.ContainsText(scope.Driver, "[green]literal[/]");
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text == "literal" && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Alignment honors custom colors and preserves explicit content lines.
        /// </summary>
        [Fact]
        public void Align_CustomOptions_RenderColorsAndLines()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                Consoul.Align("Alpha\nBeta", new AlignOptions
                {
                    Width = 20,
                    Alignment = TextBlockAlignment.Right,
                    Scheme = new ColorScheme
                    {
                        Color = ConsoleColor.Cyan,
                        BackgroundColor = ConsoleColor.Black
                    }
                });

                Assert.Contains(scope.Driver.Lines, line => line == "               Alpha");
                Assert.Contains(scope.Driver.Lines, line => line == "                Beta");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Alpha") && write.Color == ConsoleColor.Cyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Alignment can capture existing Consoul render helpers and center their plain text output.
        /// </summary>
        [Fact]
        public void Align_CapturesRenderableContent()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                Consoul.Align(
                    () => Consoul.Panel("Build complete", title: "Summary", width: 30),
                    TextBlockAlignment.Center,
                    width: 48,
                    color: ConsoleColor.Green,
                    captureWidth: 34);

                VisualAssert.WroteText(scope.Driver, "Summary");
                VisualAssert.WroteText(scope.Driver, "Build complete");
                Assert.Contains(scope.Driver.Lines, line => line.StartsWith("         ") && line.Contains("Summary"));
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build complete") && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed dynamic views can align captured sections as part of a dashboard.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderAlignedSections()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var view = Consoul.View("Dashboard", new DashboardModel { Status = "Green" })
                        .RenderModel(model =>
                        {
                            Consoul.Align(
                                () => Consoul.PropertyList(list =>
                                {
                                    list.Add("Status", model.Status, ConsoleColor.Green);
                                    list.Add("Owner", "Platform", ConsoleColor.Cyan);
                                }, new PropertyListOptions { Width = 30, LabelWidth = 8 }),
                                TextBlockAlignment.Center,
                                width: 44,
                                captureWidth: 32);
                        })
                        .Option("Refresh", model => model.Status = "Refreshed");

                    view.Render();

                    VisualAssert.WroteText(scope.Driver, "Dashboard");
                    VisualAssert.WroteText(scope.Driver, "Status");
                    VisualAssert.WroteText(scope.Driver, "Green");
                    Assert.Contains(scope.Driver.Lines, line => line.StartsWith("       ") && line.Contains("Status"));
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
