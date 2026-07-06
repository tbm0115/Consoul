using System;
using ConsoulLibrary.Color;
using ConsoulLibrary.Tests.Rendering;
using Xunit;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for panel and rule view helpers.
    /// </summary>
    public class PanelRuleRenderingTests
    {
        /// <summary>
        /// Panels wrap long content and stay inside the active terminal width.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(20)]
        [InlineData(32)]
        [InlineData(80)]
        public void Panel_LongContent_DoesNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Panel(
                    "This panel contains a verylongtokenwithoutnaturalbreaks and a second sentence for wrapping.",
                    title: "Summary");

                VisualAssert.WroteText(scope.Driver, "Summary");
                VisualAssert.WroteText(scope.Driver, "verylongtoken");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Panels honor custom widths, colors, and multiline content without overflowing.
        /// </summary>
        [Fact]
        public void Panel_CustomOptions_RendersWithinConfiguredWidth()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var options = new PanelOptions
                {
                    Title = "Details",
                    Width = 24,
                    Padding = 2,
                    BorderScheme = new ColorScheme
                    {
                        Color = ConsoleColor.Green,
                        BackgroundColor = ConsoleColor.Black
                    },
                    ContentScheme = new ColorScheme
                    {
                        Color = ConsoleColor.Cyan,
                        BackgroundColor = ConsoleColor.Black
                    }
                };

                Consoul.Panel("Alpha\nBeta Gamma Delta", options);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Details") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Alpha") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 24, "Panel line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Panels can use named border presets with aligned titles and footers.
        /// </summary>
        [Fact]
        public void Panel_BorderPresetAndFooter_RendersStyledFrame()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var options = new PanelOptions
                {
                    Title = "Release",
                    Footer = "Esc to close",
                    Width = 36,
                    BorderStyle = PanelBorderStyle.Rounded,
                    TitleAlignment = TextBlockAlignment.Center,
                    FooterAlignment = TextBlockAlignment.Right,
                    FooterScheme = new ColorScheme
                    {
                        Color = ConsoleColor.Yellow,
                        BackgroundColor = ConsoleColor.Black
                    }
                };

                Consoul.Panel("Build complete", options);

                VisualAssert.ContainsText(scope.Driver, "╭");
                VisualAssert.ContainsText(scope.Driver, "╮");
                VisualAssert.ContainsText(scope.Driver, "╰");
                VisualAssert.ContainsText(scope.Driver, "╯");
                VisualAssert.ContainsText(scope.Driver, "Release");
                VisualAssert.ContainsText(scope.Driver, "Esc to close");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Esc to close") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Lines, line => line.StartsWith("╭") && line.EndsWith("╮") && line.IndexOf("Release", StringComparison.Ordinal) > 10);
                Assert.Contains(scope.Driver.Lines, line => line.StartsWith("╰") && line.EndsWith("╯") && line.IndexOf("Esc to close", StringComparison.Ordinal) > 18);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Panel and rule builders accept shared palette color values directly.
        /// </summary>
        [Fact]
        public void PanelAndRule_ColorHelpers_AcceptSharedPaletteColors()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var panel = new Panel("Panel content")
                    .Width(36)
                    .RoundedBorder()
                    .Header("Panel header", SpectreColor.Aqua, Justify.Center)
                    .Footer("Panel footer", RgbColor.Yellow, Justify.Right)
                    .BorderColor(SpectreColor.Green)
                    .ContentColor(RgbColor.White)
                    .HeaderColor(RgbColor.Aqua)
                    .FooterColor(SpectreColor.Yellow);

                AnsiConsole.Write(panel);

                var rule = new Rule("Rule color", SpectreColor.Red, Justify.Left)
                    .Width(36)
                    .Character('=')
                    .RuleColor(RgbColor.Green)
                    .TitleColor(SpectreColor.Aqua);

                AnsiConsole.Write(rule);

                var rgbRule = new Rule("Rule rgb", RgbColor.Yellow, Justify.Right)
                    .Width(36)
                    .Character('-')
                    .RuleColor(SpectreColor.Red);

                AnsiConsole.Write(rgbRule);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Panel header") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Panel footer") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Panel content") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("╭") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Rule color") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Rule rgb") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("-") && write.Color == ConsoleColor.Red);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Long panel footer text is truncated inside the frame rather than overflowing narrow terminals.
        /// </summary>
        [Theory]
        [InlineData(20)]
        [InlineData(28)]
        public void Panel_LongFooter_DoesNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Panel(
                    "Ready",
                    new PanelOptions
                    {
                        Title = "Deployment",
                        Footer = "This footer text is intentionally too long for the frame",
                        Width = width,
                        BorderStyle = PanelBorderStyle.Heavy,
                        FooterAlignment = TextBlockAlignment.Center
                    });

                VisualAssert.ContainsText(scope.Driver, "┏");
                VisualAssert.ContainsText(scope.Driver, "┗");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Panels can capture existing render helpers as content without overflowing.
        /// </summary>
        [Fact]
        public void Panel_RenderAction_CapturesStructuredContent()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Consoul.Panel(
                    () => Consoul.PropertyList(list =>
                    {
                        list.Add("Environment", "Production", ConsoleColor.Cyan);
                        list.Add("Status", "Ready", ConsoleColor.Green);
                    }, new PropertyListOptions { LabelWidth = 12 }),
                    title: "Snapshot",
                    width: 42,
                    borderColor: ConsoleColor.Green);

                VisualAssert.WroteText(scope.Driver, "Snapshot");
                VisualAssert.WroteText(scope.Driver, "Environment");
                VisualAssert.WroteText(scope.Driver, "Production");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Snapshot") && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Captured panel content defaults to the panel inner width and reflows at narrow terminal widths.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(24)]
        [InlineData(32)]
        public void Panel_RenderAction_DoesNotOverflowAtNarrowWidths(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Panel(
                    () => Consoul.Table(
                        new[]
                        {
                            new BuildRecord { Name = "Compile", Status = "Succeeded" },
                            new BuildRecord { Name = "Package", Status = "Waiting for approval" }
                        }),
                    new PanelOptions
                    {
                        Title = "Build",
                        Width = width,
                        Padding = 1
                });

                VisualAssert.WroteText(scope.Driver, "Build");
                Assert.True(scope.Driver.Writes.Count > 0);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Rules render titled separators at multiple widths without overflowing.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(16)]
        [InlineData(40)]
        public void Rule_TitledSeparator_DoesNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Rule("Actions", alignment: RuleTitleAlignment.Left);
                Consoul.Rule("Status", alignment: RuleTitleAlignment.Center);
                Consoul.Rule("Done", alignment: RuleTitleAlignment.Right);

                VisualAssert.WroteText(scope.Driver, "Actions");
                VisualAssert.WroteText(scope.Driver, "Status");
                VisualAssert.WroteText(scope.Driver, "Done");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed views can use panel and rule helpers as view-rendering sections.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderPanelAndRuleSections()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var view = Consoul.View("Dashboard", new DashboardModel { Name = "Build", Status = "Green" })
                        .RenderModel(model =>
                        {
                            Consoul.Rule("Status");
                            Consoul.Panel(model.Name + ": " + model.Status, title: "Current");
                        })
                        .Option("Refresh", model => model.Status = "Refreshed");

                    view.Render();

                    VisualAssert.WroteText(scope.Driver, "Status");
                    VisualAssert.WroteText(scope.Driver, "Current");
                    VisualAssert.WroteText(scope.Driver, "Build: Green");
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
            public string Name { get; set; } = string.Empty;

            public string Status { get; set; } = string.Empty;
        }

        private sealed class BuildRecord
        {
            public string Name { get; set; } = string.Empty;

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
