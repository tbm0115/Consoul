using System;
using System.Globalization;
using System.IO;
using ConsoulLibrary.Color;
using Xunit;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for bullet list view helpers.
    /// </summary>
    public class BulletListRenderingTests
    {
        /// <summary>
        /// Bullet lists wrap nested items without overflowing.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(12)]
        [InlineData(28)]
        [InlineData(80)]
        public void BulletList_NestedItems_DoNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.BulletList(list =>
                {
                    BulletListItem release = list.Add("Prepare release notes with a verylongtokenwithoutnaturalbreaks");
                    release.Add("Summarize API changes");
                    release.Add("Capture migration notes");
                    list.Add("Run smoke tests against narrow terminal widths");
                }, new BulletListOptions { Width = width });

                VisualAssert.WroteText(scope.Driver, "Prepare");
                VisualAssert.WroteText(scope.Driver, "Run");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Bullet lists can render numbered items with custom colors.
        /// </summary>
        [Fact]
        public void BulletList_NumberedOptions_ColorizeMarkersAndText()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var options = new BulletListOptions
                {
                    Width = 48,
                    Style = BulletListStyle.Numbered,
                    MarkerScheme = Scheme(ConsoleColor.Cyan),
                    TextScheme = Scheme(ConsoleColor.White)
                };

                Consoul.BulletList(options,
                    new BulletListItem("Build package"),
                    new BulletListItem("Publish preview"));

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("1.") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build") && write.Color == ConsoleColor.White);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Bullet-list fluent color helpers accept shared palette color values directly.
        /// </summary>
        [Fact]
        public void BulletList_ColorHelpers_AcceptSharedPaletteColors()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                new BulletList()
                    .Numbered()
                    .Width(40)
                    .MarkerColor(SpectreColor.Aqua)
                    .TextColor(RgbColor.Yellow)
                    .AddItems("Build package", "Publish preview")
                    .Render();

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("1.") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build package") && write.Color == ConsoleColor.Yellow);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Bullet-list style helpers preserve RGB and decoration metadata for true-color terminals.
        /// </summary>
        [Fact]
        public void BulletList_StyleHelpersEmitTrueColorSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 12, isTerminal: true),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });

            var list = new BulletList()
                .Width(60)
                .TextColor("bold #22c55e")
                .MarkerColor("#445566")
                .AddItems("Build");
            list.AddItemStyled("API", "#336699", "#778899");

            console.Write(list);

            string output = writer.ToString();
            Assert.Contains("\u001b[38;2;68;85;102m- \u001b[39m", output);
            Assert.Contains("\u001b[1;38;2;34;197;94mBuild\u001b[22;39m", output);
            Assert.Contains("\u001b[38;2;119;136;153m- \u001b[39m", output);
            Assert.Contains("\u001b[38;2;51;102;153mAPI\u001b[39m", output);
        }

        /// <summary>
        /// Bullet-list RGB selectors use 256-color SGR when the terminal supports eight-bit color.
        /// </summary>
        [Fact]
        public void BulletList_RgbSelectorsEmitEightBitSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 12, isTerminal: true),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.EightBit
            });

            var items = new[]
            {
                new
                {
                    Name = "API",
                    TextColor = RgbColor.FromHex("#336699"),
                    MarkerColor = RgbColor.FromHex("#445566")
                }
            };

            var list = new BulletList()
                .Width(60)
                .AddItems(items, item => item.Name, item => item.TextColor, item => item.MarkerColor);

            console.Write(list);

            string output = writer.ToString();
            Assert.Contains("\u001b[38;5;239m- \u001b[39m", output);
            Assert.Contains("\u001b[38;5;60mAPI\u001b[39m", output);
            Assert.DoesNotContain("\u001b[38;2", output);
        }

        /// <summary>
        /// Item-level color overrides win over list defaults.
        /// </summary>
        [Fact]
        public void BulletList_ItemOverrides_ColorizeItem()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                Consoul.BulletList(list =>
                {
                    list.Add("Healthy", ConsoleColor.Green, ConsoleColor.Green);
                    list.Add("Needs review", ConsoleColor.Yellow, ConsoleColor.Yellow);
                }, new BulletListOptions { Width = 40 });

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Healthy") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Needs") && write.Color == ConsoleColor.Yellow);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Bullet list item text treats raw markup as styled visible text.
        /// </summary>
        /// <param name="width">The configured list width.</param>
        [Theory]
        [InlineData(18)]
        [InlineData(32)]
        public void BulletList_RawMarkupItems_WrapByVisibleTextAndDoNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width + 4))
            {
                Consoul.BulletList(list =>
                {
                    BulletListItem release = list.Add("[green]ProductionClusterWithVeryLongNameWithoutNaturalBreaks[/]");
                    release.Add("[yellow]Review[/] [[literal]]");
                    list.Add("[white on navy]Approval required[/]");
                }, new BulletListOptions
                {
                    Width = width,
                    TextScheme = Scheme(ConsoleColor.White)
                });

                VisualAssert.ContainsText(scope.Driver, "Production");
                VisualAssert.ContainsText(scope.Driver, "Review");
                VisualAssert.ContainsText(scope.Driver, "[literal]");
                VisualAssert.ContainsText(scope.Driver, "Approval");
                VisualAssert.ContainsText(scope.Driver, "required");
                Assert.DoesNotContain("[green]", scope.Driver.Text);
                Assert.DoesNotContain("[yellow]", scope.Driver.Text);
                Assert.DoesNotContain("[white on navy]", scope.Driver.Text);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Production") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Review") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[literal]") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Approval") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= width, "Raw markup bullet list line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Bullet lists can be composed inside captured panels.
        /// </summary>
        [Fact]
        public void BulletList_ComposesInsidePanel()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Consoul.Panel(
                    () => Consoul.BulletList(list =>
                    {
                        list.Add("Review metrics");
                        list.Add("Approve release");
                    }, new BulletListOptions { Width = 32 }),
                    title: "Next",
                    width: 40);

                VisualAssert.WroteText(scope.Driver, "Next");
                VisualAssert.WroteText(scope.Driver, "Review");
                VisualAssert.WroteText(scope.Driver, "Approve");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed dynamic views can render bullet list sections from model state.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderBulletListSections()
        {
            using (var scope = new ConsoleDriverScope(44))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var model = new ReleaseNotes
                    {
                        Summary = "Preview package",
                        NeedsApproval = true
                    };
                    var view = Consoul.View("Notes", model)
                        .RenderModel(notes =>
                        {
                            Consoul.BulletList(list =>
                            {
                                list.Add(notes.Summary, ConsoleColor.Cyan);
                                list.Add(notes.NeedsApproval ? "Approval required" : "Ready to publish", notes.NeedsApproval ? ConsoleColor.Yellow : ConsoleColor.Green);
                            }, new BulletListOptions { Width = 40 });
                        })
                        .Option("Approve", notes => notes.NeedsApproval = false);

                    view.Render();

                    VisualAssert.WroteText(scope.Driver, "Preview");
                    VisualAssert.WroteText(scope.Driver, "Approval");
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

        private sealed class ReleaseNotes
        {
            public string Summary { get; set; } = string.Empty;

            public bool NeedsApproval { get; set; }
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
