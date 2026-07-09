using System;
using System.Globalization;
using System.IO;
using ConsoulLibrary.Color;
using Xunit;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for breadcrumb view helpers.
    /// </summary>
    public class BreadcrumbRenderingTests
    {
        /// <summary>
        /// Breadcrumbs elide middle items and stay within the active terminal width.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(12)]
        [InlineData(24)]
        [InlineData(80)]
        public void Breadcrumb_LongTrail_DoesNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Breadcrumb(
                    new BreadcrumbOptions { Width = width },
                    "Home",
                    "Administration",
                    "Preview Environments",
                    "Settings Editor");

                Assert.NotEmpty(scope.Driver.Writes);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Breadcrumbs honor custom item, current item, separator, and ellipsis colors.
        /// </summary>
        [Fact]
        public void Breadcrumb_CustomOptions_ColorizesSegments()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var options = new BreadcrumbOptions
                {
                    Width = 24,
                    Separator = " / ",
                    ItemScheme = Scheme(ConsoleColor.Blue),
                    CurrentScheme = Scheme(ConsoleColor.Green),
                    SeparatorScheme = Scheme(ConsoleColor.DarkGray),
                    EllipsisScheme = Scheme(ConsoleColor.Yellow)
                };

                Consoul.Breadcrumb(options, "Home", "Projects", "Releases", "Editor");

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Home") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("/") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("...") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Editor") && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Breadcrumb fluent color helpers accept shared palette color values directly.
        /// </summary>
        [Fact]
        public void Breadcrumb_ColorHelpers_AcceptSharedPaletteColors()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                new Breadcrumb()
                    .Add("Home")
                    .Add("Operations")
                    .Add("Deployments")
                    .Separator(" / ")
                    .Width(48)
                    .ItemColor(SpectreColor.Blue)
                    .CurrentColor(RgbColor.Aqua)
                    .SeparatorColor(SpectreColor.DarkGray)
                    .Render();

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Home") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deployments") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("/") && write.Color == ConsoleColor.DarkGray);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Breadcrumb style helpers and model selectors preserve RGB and decoration metadata for true-color terminals.
        /// </summary>
        [Fact]
        public void Breadcrumb_StyleHelpersAndProjectionSelectorsEmitTrueColorSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 90, height: 8, isTerminal: true),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });
            var segments = new[]
            {
                new StyledBreadcrumbSegment("Services", RgbColor.FromHex("#112233"), SpectreColor.FromHex("#445566"))
            };

            var breadcrumb = new Breadcrumb()
                .Width(88)
                .Separator(" / ")
                .ItemColor("italic #778899")
                .CurrentColor("bold #22c55e")
                .SeparatorColor("underline #336699")
                .AddItem("Home")
                .AddItems(segments, segment => "RGB " + segment.Label, segment => segment.RgbColor)
                .AddItems(segments, segment => "Color " + segment.Label, segment => segment.SpectreColor)
                .AddItems(segments, segment => "Style " + segment.Label, segment => "bold #abcdef")
                .AddItem("Current");

            console.Write(breadcrumb);

            string output = writer.ToString();
            Assert.Contains("\u001b[3;38;2;119;136;153mHome", output);
            Assert.Contains("\u001b[4;38;2;51;102;153m / ", output);
            Assert.Contains("\u001b[38;2;17;34;51mRGB Services", output);
            Assert.Contains("\u001b[38;2;68;85;102mColor Services", output);
            Assert.Contains("\u001b[1;38;2;171;205;239mStyle Services", output);
            Assert.Contains("\u001b[1;38;2;34;197;94mCurrent", output);
            Assert.DoesNotContain("[#abcdef", output);
            Assert.DoesNotContain("[bold", output);
        }

        /// <summary>
        /// Breadcrumb labels treat raw markup as styled visible text during elision and text export.
        /// </summary>
        /// <param name="width">The configured breadcrumb width.</param>
        [Theory]
        [InlineData(24)]
        [InlineData(60)]
        public void Breadcrumb_RawMarkupItems_DoNotLeakTagsOrOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                var options = new BreadcrumbOptions
                {
                    Width = width,
                    Separator = " / ",
                    ItemScheme = Scheme(ConsoleColor.DarkGray),
                    CurrentScheme = Scheme(ConsoleColor.White)
                };

                Consoul.Breadcrumb(
                    options,
                    "[cyan]Home[/]",
                    "[green]Services[/]",
                    "[yellow]Deployments[/]",
                    "[white on navy]Prod [[hot]][/]");

                VisualAssert.ContainsText(scope.Driver, "Home");
                VisualAssert.ContainsText(scope.Driver, "Prod");
                VisualAssert.ContainsText(scope.Driver, "[hot]");
                Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                Assert.DoesNotContain("[green]", scope.Driver.Text);
                Assert.DoesNotContain("[yellow]", scope.Driver.Text);
                Assert.DoesNotContain("[white on navy]", scope.Driver.Text);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Home") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Prod") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= width, "Raw markup breadcrumb line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);

                string text = Consoul.BreadcrumbText(
                    new[] { "[cyan]Home[/]", "[green]Services[/]", "[white on navy]Prod [[hot]][/]" },
                    options,
                    width);
                Assert.Contains("Home", text);
                Assert.Contains("Prod", text);
                Assert.DoesNotContain("[cyan]", text);
                Assert.DoesNotContain("[green]", text);
                Assert.DoesNotContain("[white on navy]", text);
                Assert.True(text.Length <= width, "Raw markup breadcrumb text exceeded configured width: " + text);
            }
        }

        /// <summary>
        /// Breadcrumb text generation creates bounded strings without writing to the console.
        /// </summary>
        [Fact]
        public void BreadcrumbText_ReturnsBoundedPlainText()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                string text = Consoul.BreadcrumbText(
                    new[] { "Home", "Operations", "Deployments", "Production" },
                    new BreadcrumbOptions { Separator = " > " },
                    width: 18);

                Assert.True(text.Length <= 18, "Breadcrumb text exceeded requested width: " + text);
                Assert.Contains("...", text);
                Assert.Empty(scope.Driver.Writes);
            }
        }

        /// <summary>
        /// Breadcrumb builders can project model-backed navigation segments without adapter loops.
        /// </summary>
        [Fact]
        public void Breadcrumb_AddItems_ProjectsModelTrail()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                var segments = new[]
                {
                    new BreadcrumbSegment("Operations"),
                    new BreadcrumbSegment("Services"),
                    new BreadcrumbSegment("Deployments")
                };

                new Breadcrumb()
                    .AddItem("Home")
                    .AddItems(segments, segment => segment.Label)
                    .Separator(" / ")
                    .Width(48)
                    .Render();

                VisualAssert.WroteText(scope.Driver, "Home");
                VisualAssert.WroteText(scope.Driver, "Operations");
                VisualAssert.WroteText(scope.Driver, "Services");
                VisualAssert.WroteText(scope.Driver, "Deployments");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("/"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Breadcrumb builders accept quick params labels for small MVC navigation hints.
        /// </summary>
        [Fact]
        public void Breadcrumb_AddItems_AcceptsQuickParamsLabels()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                new Breadcrumb()
                    .Width(40)
                    .Separator(" > ")
                    .AddItems("Home", "[green]Deploy[/]", "Review")
                    .Render();

                VisualAssert.WroteText(scope.Driver, "Home");
                VisualAssert.WroteText(scope.Driver, "Deploy");
                VisualAssert.WroteText(scope.Driver, "Review");
                Assert.DoesNotContain("[green]", scope.Driver.Text);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Breadcrumbs can be built fluently and composed inside captured panels.
        /// </summary>
        [Fact]
        public void Breadcrumb_ComposesInsidePanel()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Consoul.Panel(
                    () => Consoul.Breadcrumb(breadcrumb => breadcrumb
                        .Add("Home")
                        .Add("Settings")
                        .Add("Editor"),
                        new BreadcrumbOptions { Width = 32 }),
                    title: "Navigation",
                    width: 40);

                VisualAssert.WroteText(scope.Driver, "Navigation");
                VisualAssert.WroteText(scope.Driver, "Home");
                VisualAssert.WroteText(scope.Driver, "Editor");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed dynamic views can render breadcrumbs as navigation context.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderBreadcrumbSections()
        {
            using (var scope = new ConsoleDriverScope(44))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var view = Consoul.View("Settings", new NavigationModel { Area = "Operations", Page = "Editor" })
                        .RenderModel(model =>
                        {
                            Consoul.Breadcrumb("Home", model.Area, model.Page);
                            Consoul.Callout("Editing " + model.Area + " settings.", CalloutKind.Info, new CalloutOptions { Width = 36 });
                        })
                        .Option("Refresh", model => model.Page = "Review");

                    view.Render();

                    VisualAssert.WroteText(scope.Driver, "Home");
                    VisualAssert.WroteText(scope.Driver, "Editor");
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

        private sealed class BreadcrumbSegment
        {
            public BreadcrumbSegment(string label)
            {
                Label = label;
            }

            public string Label { get; private set; }
        }

        private sealed class StyledBreadcrumbSegment
        {
            public StyledBreadcrumbSegment(string label, RgbColor rgbColor, SpectreColor spectreColor)
            {
                Label = label;
                RgbColor = rgbColor;
                SpectreColor = spectreColor;
            }

            public string Label { get; }

            public RgbColor RgbColor { get; }

            public SpectreColor SpectreColor { get; }
        }

        private sealed class NavigationModel
        {
            public string Area { get; set; } = string.Empty;

            public string Page { get; set; } = string.Empty;
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
