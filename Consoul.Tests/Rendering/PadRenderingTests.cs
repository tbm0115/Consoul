using System;
using ConsoulLibrary.Color;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for padded text and captured render helpers.
    /// </summary>
    public class PadRenderingTests
    {
        /// <summary>
        /// Padded text wraps long content and stays inside the active terminal width.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(16)]
        [InlineData(32)]
        [InlineData(80)]
        public void Pad_Text_DoesNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Pad(
                    "This padded content contains a verylongtokenwithoutnaturalbreaks and explanatory text.",
                    left: 2,
                    right: 2,
                    width: width);

                VisualAssert.WroteText(scope.Driver, "This");
                VisualAssert.WroteText(scope.Driver, "verylong");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Padding honors configured width, top/bottom padding, and colors.
        /// </summary>
        [Fact]
        public void Pad_CustomOptions_RenderPaddingAndColors()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Consoul.Pad("Alpha Beta", new PadOptions
                {
                    Width = 18,
                    Left = 2,
                    Right = 3,
                    Top = 1,
                    Bottom = 1,
                    Scheme = new ColorScheme
                    {
                        Color = ConsoleColor.Cyan,
                        BackgroundColor = ConsoleColor.Black
                    }
                });

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Alpha Beta") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Lines, line => line.StartsWith("  Alpha Beta") && line.Length == 18);
                Assert.Contains(scope.Driver.Lines, line => line.Length == 18 && line.Trim().Length == 0);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 18, "Padded line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Direct padded text can contain raw markup without leaking tags or overflowing.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(22)]
        [InlineData(38)]
        public void Pad_RawMarkupText_DoesNotLeakTagsOrOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Pad(
                    "[green]Ready[/] for [[prod]] [white on navy]Approval required for deployment[/]",
                    new PadOptions
                    {
                        Width = width,
                        Left = 2,
                        Right = 1
                    });

                VisualAssert.WroteText(scope.Driver, "Ready");
                VisualAssert.WroteText(scope.Driver, "[prod]");
                VisualAssert.WroteText(scope.Driver, "Approval");
                Assert.DoesNotContain("[green]", scope.Driver.Text);
                Assert.DoesNotContain("[white on navy]", scope.Driver.Text);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Approval") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Collapsed Padder instances measure raw markup by visible text, not source tag length.
        /// </summary>
        [Fact]
        public void Padder_CollapsedWidth_MeasuresMarkupByVisibleText()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                AnsiConsole.Write(new Padder("[green]OK[/]").Padding(1, 0));

                Assert.DoesNotContain("[green]", scope.Driver.Text);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("OK") && write.Color == ConsoleColor.Green);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 4, "Collapsed Padder measured raw markup instead of visible text: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Captured render output is treated as already-rendered plain text, even when it contains bracket-shaped text.
        /// </summary>
        [Fact]
        public void Pad_CapturedOutput_KeepsLiteralBracketText()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Consoul.Pad(
                    () => Consoul.Write("[green]literal[/]", ConsoleColor.Cyan, writeLine: true),
                    left: 1,
                    width: 24,
                    captureWidth: 24);

                VisualAssert.ContainsText(scope.Driver, "[green]literal[/]");
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text == "literal" && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Padding can capture existing Consoul render helpers and inset their plain text output.
        /// </summary>
        [Fact]
        public void Pad_CapturesRenderableContent()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                Consoul.Pad(
                    () => Consoul.Panel("Build complete", title: "Summary", width: 34),
                    left: 3,
                    right: 1,
                    width: 42,
                    color: ConsoleColor.Green,
                    captureWidth: 38);

                VisualAssert.WroteText(scope.Driver, "Summary");
                VisualAssert.WroteText(scope.Driver, "Build complete");
                Assert.Contains(scope.Driver.Lines, line => line.StartsWith("   ") && line.Contains("Summary"));
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build complete") && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed dynamic views can pad render helper sections as part of a dashboard.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderPaddedSections()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var view = Consoul.View("Dashboard", new DashboardModel { Status = "Green" })
                        .RenderModel(model =>
                        {
                            Consoul.Pad(
                                () => Consoul.PropertyList(list =>
                                {
                                    list.Add("Status", model.Status, ConsoleColor.Green);
                                    list.Add("Owner", "Platform", ConsoleColor.Cyan);
                                }, new PropertyListOptions { Width = 32, LabelWidth = 8 }),
                                left: 2,
                                width: 38,
                                captureWidth: 34);
                        })
                        .Option("Refresh", model => model.Status = "Refreshed");

                    view.Render();

                    VisualAssert.WroteText(scope.Driver, "Dashboard");
                    VisualAssert.WroteText(scope.Driver, "Status");
                    VisualAssert.WroteText(scope.Driver, "Green");
                    Assert.Contains(scope.Driver.Lines, line => line.StartsWith("  ") && line.Contains("Status"));
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
