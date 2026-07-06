using System;
using ConsoulLibrary.Color;
using Xunit;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for compact badge view helpers.
    /// </summary>
    public class BadgeRenderingTests
    {
        /// <summary>
        /// Badges trim to the active width instead of overflowing.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(4)]
        [InlineData(8)]
        [InlineData(20)]
        public void Badge_LongLabel_DoesNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Badge("preview-environment-with-a-very-long-name", BadgeKind.Info);

                Assert.NotEmpty(scope.Driver.Writes);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Badges honor custom width, casing, frame, and color schemes.
        /// </summary>
        [Fact]
        public void Badge_CustomOptions_RendersConfiguredTextAndColors()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var options = new BadgeOptions
                {
                    MaxWidth = 14,
                    Uppercase = false,
                    LeftEdge = "<",
                    RightEdge = ">",
                    HorizontalPadding = 0,
                    SuccessScheme = new ColorScheme
                    {
                        Color = ConsoleColor.White,
                        BackgroundColor = ConsoleColor.DarkGreen
                    }
                };

                Consoul.Badge("ready", BadgeKind.Success, options);

                Assert.Contains(scope.Driver.Writes, write => write.Text == "<ready>" && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkGreen);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Badge color helpers accept shared palette and RGB colors.
        /// </summary>
        [Fact]
        public void Badge_ColorHelpers_AcceptSharedPaletteColors()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                AnsiConsole.Write(new Badge("palette").Success().Color(SpectreColor.Cyan));
                AnsiConsole.Write(new Badge("rgb").Info().Color(RgbColor.Yellow));

                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("PALETTE") &&
                    write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("RGB") &&
                    write.Color == ConsoleColor.Yellow);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Badges treat raw markup labels as styled visible text during rendering and text export.
        /// </summary>
        [Fact]
        public void Badge_RawMarkupLabel_DoesNotLeakTagsOrOverflow()
        {
            using (var scope = new ConsoleDriverScope(24))
            {
                Consoul.Badge("[green]ready [[prod]][/]", BadgeKind.Info);

                VisualAssert.ContainsText(scope.Driver, "[ READY [PROD] ]");
                Assert.DoesNotContain("[green]", scope.Driver.Text);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("READY [PROD]") && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);

                string text = Consoul.BadgeText(
                    "[green]ready [[prod]][/]",
                    BadgeKind.Info,
                    new BadgeOptions { MaxWidth = 16 });
                Assert.Equal("[ READY [PROD] ]", text);
                Assert.DoesNotContain("[green]", text);
            }
        }

        /// <summary>
        /// Badges can be written inline with other console output.
        /// </summary>
        [Fact]
        public void Badge_InlineWrite_ComposesWithText()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                Consoul.Badge("ok", BadgeKind.Success, writeLine: false);
                Consoul.Write(" Worker", ConsoleColor.Cyan);

                VisualAssert.ContainsText(scope.Driver, "[ OK ] Worker");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Badge text generation creates bounded strings without writing to the console.
        /// </summary>
        [Fact]
        public void BadgeText_ReturnsBoundedPlainText()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                string text = Consoul.BadgeText(
                    "production-preview",
                    BadgeKind.Warning,
                    new BadgeOptions { MaxWidth = 10 });

                Assert.Equal(10, text.Length);
                Assert.Empty(scope.Driver.Writes);
            }
        }

        /// <summary>
        /// Badge entries can be reused as renderable components in captured layouts.
        /// </summary>
        [Fact]
        public void BadgeEntry_Renderable_CanBeCapturedAndComposed()
        {
            IConsoulRenderable badge = new BadgeEntry(
                "ready",
                BadgeKind.Success,
                new BadgeOptions { MaxWidth = 12 });

            string captured = Consoul.Capture(badge, width: 20);

            Assert.Contains("[ READY ]", captured);

            using (var scope = new ConsoleDriverScope(32))
            {
                Consoul.Panel(badge, title: "State", width: 24, captureWidth: 20);

                VisualAssert.WroteText(scope.Driver, "State");
                VisualAssert.WroteText(scope.Driver, "[ READY ]");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed dynamic views can render badges as compact state labels.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderBadgeSections()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var view = Consoul.View("Deploy", new DeployModel { Environment = "prod", Ready = true })
                        .RenderModel(model =>
                        {
                            Consoul.Badge(model.Environment, BadgeKind.Info, writeLine: false);
                            Consoul.Write(" ", writeLine: false);
                            Consoul.Badge(model.Ready ? "ready" : "blocked", model.Ready ? BadgeKind.Success : BadgeKind.Error);
                        })
                        .Option("Toggle", model => model.Ready = !model.Ready);

                    view.Render();

                    VisualAssert.WroteText(scope.Driver, "[ PROD ]");
                    VisualAssert.WroteText(scope.Driver, "[ READY ]");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        private sealed class DeployModel
        {
            public string Environment { get; set; } = string.Empty;

            public bool Ready { get; set; }
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
