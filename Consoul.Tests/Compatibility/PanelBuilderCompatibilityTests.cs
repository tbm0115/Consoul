using System;
using ConsoulLibrary.Tests.Rendering;
using ConsoulLibrary.Color;
using SpectreColor = ConsoulLibrary.Color.Color;
using Xunit;

namespace ConsoulLibrary.Tests.Compatibility
{
    /// <summary>
    /// Tests Spectre-style panel builder compatibility.
    /// </summary>
    public class PanelBuilderCompatibilityTests
    {
        /// <summary>
        /// Fluent panels render through the existing panel renderer.
        /// </summary>
        [Fact]
        public void Panel_FluentBuilder_RendersWithAnsiConsoleWrite()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var panel = new Panel("Build complete and ready for deployment.")
                    .Header("Status")
                    .Footer("Esc to close")
                    .Border(PanelBorderStyle.Rounded)
                    .Width(36)
                    .Padding(1)
                    .HeaderAlignment(TextBlockAlignment.Center)
                    .FooterAlignment(TextBlockAlignment.Right)
                    .BorderColor(ConsoleColor.Green)
                    .HeaderColor(ConsoleColor.Cyan)
                    .FooterColor(ConsoleColor.Yellow)
                    .ContentColor(ConsoleColor.White);

                AnsiConsole.Write(panel);

                VisualAssert.ContainsText(scope.Driver, "Status");
                VisualAssert.ContainsText(scope.Driver, "Build complete");
                VisualAssert.ContainsText(scope.Driver, "Esc to close");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Status") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Esc to close") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build complete") && write.Color == ConsoleColor.White);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Fluent panels remain reusable renderable components for capture and composition.
        /// </summary>
        [Fact]
        public void Panel_FluentBuilder_CapturesResponsivePanel()
        {
            var panel = new Panel("A very long status message that should wrap inside the panel frame.")
                .Title("Snapshot")
                .AsciiBorder()
                .Width(30)
                .Padding(1);

            string captured = AnsiConsole.Capture(panel, width: 32);

            Assert.Contains("Snapshot", captured);
            Assert.Contains("status", captured);
            Assert.Contains("message", captured);
            Assert.All(SplitLines(captured), line => Assert.True(line.Length <= 32, "Captured panel line exceeded width: " + line));
        }

        /// <summary>
        /// Fluent panel color helpers accept Spectre-style style strings.
        /// </summary>
        [Fact]
        public void Panel_FluentBuilder_AcceptsStyleStrings()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var panel = new Panel("Build ready")
                    .Header("Status")
                    .Footer("prod")
                    .AsciiBorder()
                    .Width(32)
                    .BorderColor("grey")
                    .HeaderColor("aqua")
                    .FooterColor("white on navy")
                    .ContentColor("#22c55e");

                AnsiConsole.Write(panel);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Status") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build ready") && write.Color == ConsoleColor.DarkCyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("prod") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("+") && write.Color == ConsoleColor.Gray);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Raw Spectre-style markup in panel content, headers, and footers renders as styled visible text.
        /// </summary>
        [Fact]
        public void Panel_FluentBuilder_RendersMarkupContentHeaderAndFooter()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var panel = new Panel("[green]Ready[/] for [cyan]prod[/]")
                    .Header("[yellow]Status[/]")
                    .Footer("[white on navy]Esc[/]")
                    .AsciiBorder()
                    .Width(34)
                    .ContentColor(ConsoleColor.Gray)
                    .HeaderColor(ConsoleColor.White)
                    .FooterColor(ConsoleColor.DarkGray);

                AnsiConsole.Write(panel);

                VisualAssert.ContainsText(scope.Driver, "Status");
                VisualAssert.ContainsText(scope.Driver, "Ready for prod");
                VisualAssert.ContainsText(scope.Driver, "Esc");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Status") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("prod") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Esc") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.DoesNotContain("[green]", scope.Driver.Text);
                Assert.DoesNotContain("[yellow]", scope.Driver.Text);
                Assert.DoesNotContain("[white on navy]", scope.Driver.Text);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 34, "Panel line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Fluent panels accept Spectre-style justification vocabulary for header and footer alignment.
        /// </summary>
        [Fact]
        public void Panel_FluentBuilder_AcceptsJustifyAliases()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var panel = new Panel("Build ready")
                    .Header("Status")
                    .Footer("prod")
                    .Width(34)
                    .AsciiBorder()
                    .HeaderAlignment(Justify.Center)
                    .FooterAlignment(Justify.Right);
                var inlineNativeAlignment = new Panel("Ready")
                    .Title("Native", TextBlockAlignment.Right)
                    .Footer("left", TextBlockAlignment.Left);
                var titleAlignmentAlias = new Panel("Ready")
                    .Title("Alias")
                    .TitleAlignment(Justify.Center);
                var nativeTitleAlignmentAlias = new Panel("Ready")
                    .TitleAlignment(TextBlockAlignment.Right);

                AnsiConsole.Write(panel);

                Assert.Equal(TextBlockAlignment.Right, inlineNativeAlignment.Options.TitleAlignment);
                Assert.Equal(TextBlockAlignment.Left, inlineNativeAlignment.Options.FooterAlignment);
                Assert.Equal(TextBlockAlignment.Center, titleAlignmentAlias.Options.TitleAlignment);
                Assert.Equal(TextBlockAlignment.Right, nativeTitleAlignmentAlias.Options.TitleAlignment);
                VisualAssert.ContainsText(scope.Driver, "Status");
                VisualAssert.ContainsText(scope.Driver, "prod");
                Assert.Contains(scope.Driver.Lines, line => line.Contains("Status") && line.IndexOf("Status", StringComparison.Ordinal) > 10);
                Assert.Contains(scope.Driver.Lines, line => line.Contains("prod") && line.IndexOf("prod", StringComparison.Ordinal) > 20);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Fluent panels accept Spectre-style panel headers with justification.
        /// </summary>
        [Fact]
        public void Panel_FluentBuilder_AcceptsPanelHeaderObject()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var panel = new Panel("Build ready")
                    .Header(new PanelHeader("Deploy", Justify.Right))
                    .Footer("done", Justify.Center)
                    .Width(34)
                    .AsciiBorder();

                AnsiConsole.Write(panel);

                Assert.Equal("Deploy", panel.Options.Title);
                Assert.Equal(TextBlockAlignment.Right, panel.Options.TitleAlignment);
                Assert.Equal(TextBlockAlignment.Center, panel.Options.FooterAlignment);
                Assert.Contains(scope.Driver.Lines, line => line.Contains("Deploy") && line.IndexOf("Deploy", StringComparison.Ordinal) > 20);
                Assert.Contains(scope.Driver.Lines, line => line.Contains("done") && line.IndexOf("done", StringComparison.Ordinal) > 10);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Panel headers, titles, and footers can be supplied as reusable renderables without wrapper metadata.
        /// </summary>
        [Fact]
        public void Panel_FluentBuilder_AcceptsRenderableHeaderTitleAndFooter()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var renderedFooter = Consoul.Renderable(() => AnsiConsole.Markup("[grey]Esc[/]"));
                var panel = new Panel("[green]Ready[/] for deployment")
                    .Header(new Markup("[cyan]Deploy[/]"), Justify.Center)
                    .Footer(renderedFooter, Justify.Right)
                    .Width(38)
                    .AsciiBorder()
                    .ContentColor(ConsoleColor.White);
                var titled = new Panel("Queued")
                    .Title(new Text("Snapshot", ConsoleColor.Yellow), TextBlockAlignment.Right)
                    .Footer(new Markup("[white on navy]done[/]"));

                AnsiConsole.Write(panel);
                AnsiConsole.Write(titled);

                Assert.Equal("[cyan]Deploy[/]", panel.Options.Title);
                Assert.Equal(TextBlockAlignment.Center, panel.Options.TitleAlignment);
                Assert.Equal("Esc", panel.Options.Footer);
                Assert.Equal(TextBlockAlignment.Right, panel.Options.FooterAlignment);
                Assert.Equal("Snapshot", titled.Options.Title);
                Assert.Equal(TextBlockAlignment.Right, titled.Options.TitleAlignment);
                Assert.Equal("[white on navy]done[/]", titled.Options.Footer);
                VisualAssert.ContainsText(scope.Driver, "Deploy");
                VisualAssert.ContainsText(scope.Driver, "Ready for deployment");
                VisualAssert.ContainsText(scope.Driver, "Esc");
                VisualAssert.ContainsText(scope.Driver, "Snapshot");
                VisualAssert.ContainsText(scope.Driver, "done");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("done") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                Assert.DoesNotContain("[white on navy]", scope.Driver.Text);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 56, "Renderable-header panel line exceeded terminal width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Panel header/footer adapters support reusable metadata and inline style overloads.
        /// </summary>
        [Fact]
        public void Panel_FluentBuilder_AcceptsStyledHeaderAndFooterMetadata()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var panel = new Panel("Build ready")
                    .Header("Deploy", "aqua", Justify.Center)
                    .Footer("done", "white on navy", Justify.Right)
                    .Width(34)
                    .AsciiBorder();

                var inlineFooter = new Panel("Ready")
                    .Footer("hint", new Style(ConsoleColor.DarkGray), Justify.Center);
                var inlineTitle = new Panel("Ready")
                    .Title("Inline", "bold yellow", Justify.Left);
                var colorTitle = new Panel("Ready")
                    .Title("Color", SpectreColor.Aqua, Justify.Center);
                var rgbTitle = new Panel("Ready")
                    .Title("Rgb", RgbColor.Yellow, Justify.Right);

                AnsiConsole.Write(panel);

                Assert.Equal("Deploy", panel.Options.Title);
                Assert.Equal(TextBlockAlignment.Center, panel.Options.TitleAlignment);
                Assert.Equal("done", panel.Options.Footer);
                Assert.Equal(TextBlockAlignment.Right, panel.Options.FooterAlignment);
                Assert.Equal(ConsoleColor.Cyan, panel.Options.TitleScheme.GetValueOrDefault().Color);
                Assert.Equal(ConsoleColor.White, panel.Options.FooterScheme.GetValueOrDefault().Color);
                Assert.Equal(ConsoleColor.DarkBlue, panel.Options.FooterScheme.GetValueOrDefault().BackgroundColor);
                Assert.Equal(TextBlockAlignment.Center, inlineFooter.Options.FooterAlignment);
                Assert.True(inlineFooter.Options.FooterScheme.HasValue);
                Assert.Equal(ConsoleColor.DarkGray, inlineFooter.Options.FooterScheme.GetValueOrDefault().Color);
                Assert.Equal(TextBlockAlignment.Left, inlineTitle.Options.TitleAlignment);
                Assert.Equal(Decoration.Bold, inlineTitle.Options.TitleDecoration);
                Assert.Equal(ConsoleColor.Yellow, inlineTitle.Options.TitleScheme.GetValueOrDefault().Color);
                Assert.Equal(TextBlockAlignment.Center, colorTitle.Options.TitleAlignment);
                Assert.Equal(ConsoleColor.Cyan, colorTitle.Options.TitleScheme.GetValueOrDefault().Color);
                Assert.Equal(RgbColor.Aqua, colorTitle.Options.TitleForegroundRgb);
                Assert.Equal(TextBlockAlignment.Right, rgbTitle.Options.TitleAlignment);
                Assert.Equal(ConsoleColor.Yellow, rgbTitle.Options.TitleScheme.GetValueOrDefault().Color);
                Assert.Equal(RgbColor.Yellow, rgbTitle.Options.TitleForegroundRgb);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("done") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Fluent panels apply explicit four-side padding without exceeding the configured width.
        /// </summary>
        [Fact]
        public void Panel_FluentBuilder_AppliesFourSidePadding()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var panel = new Panel("Ready")
                    .Header("Status", Justify.Center)
                    .Footer("Esc", Justify.Right)
                    .Width(24)
                    .AsciiBorder()
                    .Padding(2, 1, 3, 1);

                AnsiConsole.Write(panel);

                string blankContentLine = "|" + new string(' ', 22) + "|";
                Assert.Equal(new Padding(2, 1, 3, 1).Horizontal, panel.Options.ContentPadding.Horizontal);
                Assert.Equal(2, CountLines(scope.Driver.Lines, blankContentLine));
                Assert.Contains(scope.Driver.Lines, line => line.Contains("|  Ready") && line.EndsWith("   |", StringComparison.Ordinal));
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 24, "Padded panel line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Fluent panels accept compact horizontal and vertical padding arguments.
        /// </summary>
        [Fact]
        public void Panel_FluentBuilder_AppliesHorizontalAndVerticalPadding()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var panel = new Panel("Ready")
                    .Width(20)
                    .AsciiBorder()
                    .Padding(2, 1);

                AnsiConsole.Write(panel);

                Assert.NotNull(panel.Options.ContentPadding);
                Assert.Equal(2, panel.Options.ContentPadding.Left);
                Assert.Equal(2, panel.Options.ContentPadding.Right);
                Assert.Equal(1, panel.Options.ContentPadding.Top);
                Assert.Equal(1, panel.Options.ContentPadding.Bottom);
                Assert.Contains(scope.Driver.Lines, line => line.Contains("|  Ready") && line.EndsWith("  |", StringComparison.Ordinal));
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 20, "Two-argument padded panel line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Fluent panel padding mutators build from the current legacy padding value.
        /// </summary>
        [Fact]
        public void Panel_FluentBuilder_PaddingMutatorsBuildFromCurrentPadding()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var panel = new Panel("Ready")
                    .Width(22)
                    .AsciiBorder()
                    .Padding(1)
                    .PadLeft(3)
                    .PadTop(1)
                    .PadRight(2)
                    .PadBottom(1);

                AnsiConsole.Write(panel);

                Assert.NotNull(panel.Options.ContentPadding);
                Assert.Equal(3, panel.Options.ContentPadding.Left);
                Assert.Equal(1, panel.Options.ContentPadding.Top);
                Assert.Equal(2, panel.Options.ContentPadding.Right);
                Assert.Equal(1, panel.Options.ContentPadding.Bottom);
                Assert.Contains(scope.Driver.Lines, line => line.Contains("|   Ready") && line.EndsWith("  |", StringComparison.Ordinal));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Fluent panels can capture renderable content at the final panel width.
        /// </summary>
        [Fact]
        public void Panel_FluentBuilder_CapturesRenderableContentAtRenderWidth()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var table = new Table()
                    .Width(80)
                    .AsciiBorder()
                    .HideFooter()
                    .AddColumns("Job", "State")
                    .AddRow("Build", "Ready")
                    .AddRow("Deploy", "Waiting");

                var panel = new Panel(table)
                    .Header("Pipeline")
                    .Footer("Esc to close")
                    .Width(40)
                    .Padding(1)
                    .RoundedBorder()
                    .BorderColor(ConsoleColor.Green)
                    .HeaderColor(ConsoleColor.Cyan)
                    .FooterColor(ConsoleColor.Yellow)
                    .ContentColor(ConsoleColor.White);

                AnsiConsole.Write(panel);

                VisualAssert.ContainsText(scope.Driver, "Pipeline");
                VisualAssert.ContainsText(scope.Driver, "Build");
                VisualAssert.ContainsText(scope.Driver, "Deploy");
                VisualAssert.ContainsText(scope.Driver, "Esc to close");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Pipeline") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build") && write.Color == ConsoleColor.White);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 40, "Renderable panel line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Fluent panels can capture render actions and custom border characters.
        /// </summary>
        [Fact]
        public void Panel_FluentBuilder_CapturesActionContentWithCustomBorder()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var panel = new Panel(() =>
                    {
                        AnsiConsole.MarkupLine("[green]Ready[/]");
                        AnsiConsole.Badge("prod", BadgeKind.Info);
                    })
                    .Header("Status")
                    .Width(32)
                    .Padding(0)
                    .BorderCharacters('[', ']', '[', ']', '=', '!')
                    .BorderColor(ConsoleColor.DarkGray);

                AnsiConsole.Write(panel);

                VisualAssert.ContainsText(scope.Driver, "Status");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.ContainsText(scope.Driver, "PROD");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[") && write.Color == ConsoleColor.DarkGray);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 32, "Action panel line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Panel box border objects and color aliases support pasted builder snippets.
        /// </summary>
        [Fact]
        public void Panel_FluentBuilder_AcceptsBoxBorderObjectsAndColorAliases()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var panel = new Panel("Build ready")
                    .Header("Status", Justify.Center)
                    .Footer("prod", Justify.Right)
                    .Width(34)
                    .Border(BoxBorder.Double)
                    .BorderColor(Colors.Yellow)
                    .HeaderColor(Colors.Aqua)
                    .FooterColor(Colors.Grey)
                    .ContentColor(Colors.White);

                AnsiConsole.Write(panel);

                VisualAssert.ContainsText(scope.Driver, "Status");
                VisualAssert.ContainsText(scope.Driver, "Build ready");
                VisualAssert.ContainsText(scope.Driver, "prod");
                VisualAssert.ContainsText(scope.Driver, "╔");
                VisualAssert.ContainsText(scope.Driver, "╝");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("╔") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Status") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("prod") && write.Color == ConsoleColor.Gray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build ready") && write.Color == ConsoleColor.White);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style box border aliases map to Consoul's closest available panel renderers.
        /// </summary>
        [Fact]
        public void Panel_FluentBuilder_AcceptsCommonBoxBorderAliases()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Assert.Same(BoxBorder.Ascii, BoxBorder.Ascii2);
                Assert.Same(BoxBorder.Ascii, BoxBorder.AsciiDoubleHead);
                Assert.Same(BoxBorder.Ascii, BoxBorder.Markdown);
                Assert.Same(BoxBorder.Ascii, BoxBorder.Safe);
                Assert.Same(BoxBorder.Heavy, BoxBorder.HeavyEdge);
                Assert.Same(BoxBorder.Heavy, BoxBorder.HeavyHead);
                Assert.Same(BoxBorder.Double, BoxBorder.DoubleEdge);
                Assert.Same(BoxBorder.Square, BoxBorder.Simple);
                Assert.Equal(PanelBorderStyle.Heavy, (PanelBorderStyle)BoxBorder.SimpleHeavy);
                Assert.Equal(PanelBorderStyle.Heavy, (PanelBorderStyle)BoxBorder.MinimalHeavyHead);
                Assert.Equal(PanelBorderStyle.Double, (PanelBorderStyle)BoxBorder.MinimalDoubleHead);
                Assert.Equal(PanelBorderStyle.None, (PanelBorderStyle)BoxBorder.Minimal);

                AssertBorder(new Panel("Simple").SimpleHeavyBorder(), PanelBorderStyle.Heavy);
                AssertBorder(new Panel("Minimal").MinimalBorder(), PanelBorderStyle.None);
                AssertBorder(new Panel("Minimal heavy").MinimalHeavyHeadBorder(), PanelBorderStyle.Heavy);
                AssertBorder(new Panel("Minimal double").MinimalDoubleHeadBorder(), PanelBorderStyle.Double);
                AssertBorder(new Panel("Horizontal").HorizontalBorder(), PanelBorderStyle.Square);
                AssertBorder(new Panel("Ascii2").Ascii2Border(), PanelBorderStyle.Ascii);
                AssertBorder(new Panel("Ascii double").AsciiDoubleHeadBorder(), PanelBorderStyle.Ascii);
                AssertBorder(new Panel("Markdown").MarkdownBorder(), PanelBorderStyle.Ascii);
                AssertBorder(new Panel("Heavy edge").HeavyEdgeBorder(), PanelBorderStyle.Heavy);
                AssertBorder(new Panel("Heavy head").HeavyHeadBorder(), PanelBorderStyle.Heavy);
                AssertBorder(new Panel("Double edge").DoubleEdgeBorder(), PanelBorderStyle.Double);
                AssertBorder(new Panel("Safe").SafeBorder(), PanelBorderStyle.Ascii);

                var safePanel = new Panel("Safe border")
                    .Header("Safe")
                    .Width(24)
                    .SafeBorder();
                var heavyPanel = new Panel("Heavy border")
                    .Width(24)
                    .MinimalHeavyHeadBorder();
                var borderlessPanel = new Panel("No frame")
                    .Width(24)
                    .MinimalBorder();
                var doublePanel = new Panel("Double")
                    .Width(24)
                    .MinimalDoubleHeadBorder();

                AnsiConsole.Write(safePanel);
                AnsiConsole.Write(heavyPanel);
                AnsiConsole.Write(borderlessPanel);
                AnsiConsole.Write(doublePanel);

                Assert.Equal(PanelBorderStyle.Ascii, safePanel.Options.BorderStyle);
                Assert.Equal(PanelBorderStyle.Heavy, heavyPanel.Options.BorderStyle);
                Assert.Equal(PanelBorderStyle.None, borderlessPanel.Options.BorderStyle);
                Assert.Equal(PanelBorderStyle.Double, doublePanel.Options.BorderStyle);
                VisualAssert.ContainsText(scope.Driver, "Safe border");
                VisualAssert.ContainsText(scope.Driver, "Heavy border");
                VisualAssert.ContainsText(scope.Driver, "No frame");
                VisualAssert.ContainsText(scope.Driver, "Double");
                Assert.Contains(scope.Driver.Lines, line => line.StartsWith("+", StringComparison.Ordinal) && line.EndsWith("+", StringComparison.Ordinal));
                Assert.Contains(scope.Driver.Lines, line => line.Contains("┏", StringComparison.Ordinal));
                Assert.Contains(scope.Driver.Lines, line => line.Contains("╔", StringComparison.Ordinal));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Fluent panels can suppress the visible frame while preserving title, footer, padding, and colors.
        /// </summary>
        [Fact]
        public void Panel_FluentBuilder_NoBorderRendersHeaderFooterAndContent()
        {
            using (var scope = new ConsoleDriverScope(36))
            {
                var panel = new Panel("Build ready")
                    .Header("Status", Justify.Center)
                    .Footer("done", Justify.Right)
                    .Width(30)
                    .NoBorder()
                    .Padding(2, 1, 3, 1)
                    .HeaderColor(ConsoleColor.Cyan)
                    .FooterColor(ConsoleColor.Gray)
                    .ContentColor(ConsoleColor.White);

                AnsiConsole.Write(panel);

                Assert.Equal(PanelBorderStyle.None, panel.Options.BorderStyle);
                VisualAssert.ContainsText(scope.Driver, "Status");
                VisualAssert.ContainsText(scope.Driver, "Build ready");
                VisualAssert.ContainsText(scope.Driver, "done");
                AssertNoPanelBorderCharacters(scope.Driver.Lines);
                Assert.Contains(scope.Driver.Lines, line => line.StartsWith("  Build ready", StringComparison.Ordinal) && line.Length == 30);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Status") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build ready") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("done") && write.Color == ConsoleColor.Gray);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 30, "Borderless panel line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style box border objects include a borderless preset for pasted panel snippets.
        /// </summary>
        [Fact]
        public void Panel_FluentBuilder_AcceptsBoxBorderNone()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                var panel = new Panel(() =>
                    {
                        AnsiConsole.MarkupLine("[green]Ready[/]");
                        AnsiConsole.MarkupLine("[yellow]Waiting[/]");
                    })
                    .Header("Pipeline")
                    .Width(24)
                    .Padding(1)
                    .Border(BoxBorder.None)
                    .HeaderColor(Colors.Aqua)
                    .ContentColor(Colors.White);

                AnsiConsole.Write(panel);

                Assert.Equal(PanelBorderStyle.None, panel.Options.BorderStyle);
                VisualAssert.ContainsText(scope.Driver, "Pipeline");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.ContainsText(scope.Driver, "Waiting");
                AssertNoPanelBorderCharacters(scope.Driver.Lines);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Pipeline") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.White);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 24, "BoxBorder.None panel line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Fluent panels can collapse to their content instead of expanding to the terminal width.
        /// </summary>
        [Fact]
        public void Panel_FluentBuilder_CollapseAutoSizesToContent()
        {
            using (var scope = new ConsoleDriverScope(60))
            {
                var panel = new Panel("OK")
                    .AsciiBorder()
                    .Padding(1)
                    .Collapse();

                AnsiConsole.Write(panel);

                Assert.False(panel.Options.Expand);
                Assert.Null(panel.Options.Width);
                VisualAssert.ContainsText(scope.Driver, "OK");
                Assert.Contains("+----+", scope.Driver.Lines);
                Assert.Contains("| OK |", scope.Driver.Lines);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 6, "Collapsed panel line exceeded measured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Collapsed action-backed panels capture deferred content before measuring the final frame width.
        /// </summary>
        [Fact]
        public void Panel_FluentBuilder_CollapseCapturesActionContentBeforeMeasuring()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var panel = new Panel(() =>
                    {
                        AnsiConsole.MarkupLine("[green]Ready[/]");
                        AnsiConsole.MarkupLine("[yellow]Waiting[/]");
                    })
                    .AsciiBorder()
                    .Padding(1)
                    .Collapse()
                    .ContentColor(Colors.White);

                AnsiConsole.Write(panel);

                Assert.False(panel.Options.Expand);
                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.ContainsText(scope.Driver, "Waiting");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.White);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 11, "Collapsed action panel line exceeded measured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Panel options can request collapsed layout for existing PanelEntry usage.
        /// </summary>
        [Fact]
        public void PanelOptions_ExpandFalse_CollapsesPanelEntry()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var panel = new PanelEntry(
                    "Alpha",
                    new PanelOptions
                    {
                        BorderStyle = PanelBorderStyle.Ascii,
                        Expand = false,
                        Padding = 0
                    });

                panel.Render();

                Assert.False(panel.Options.Expand);
                VisualAssert.ContainsText(scope.Driver, "Alpha");
                Assert.Contains("+-----+", scope.Driver.Lines);
                Assert.Contains("|Alpha|", scope.Driver.Lines);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 7, "PanelOptions collapsed line exceeded measured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        private static string[] SplitLines(string value)
        {
            return (value ?? string.Empty).Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        }

        private static int CountLines(System.Collections.Generic.IEnumerable<string> lines, string value)
        {
            int count = 0;
            foreach (string line in lines)
            {
                if (string.Equals(line, value, StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }

        private static void AssertBorder(Panel panel, PanelBorderStyle style)
        {
            Assert.Equal(style, panel.Options.BorderStyle);
        }

        private static void AssertNoPanelBorderCharacters(System.Collections.Generic.IEnumerable<string> lines)
        {
            string text = string.Join(Environment.NewLine, lines);
            foreach (char border in new[] { '+', '|', '╔', '╗', '╚', '╝', '╭', '╮', '╰', '╯', '┌', '┐', '└', '┘', '┏', '┓', '┗', '┛', '─', '│', '═', '║', '━', '┃' })
            {
                Assert.DoesNotContain(border.ToString(), text);
            }
        }
    }
}
