using System;
using System.Linq;
using ConsoulLibrary.Color;
using Xunit;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for ASCII canvas view helpers.
    /// </summary>
    public class CanvasRenderingTests
    {
        /// <summary>
        /// Canvas rendering clips to the configured or terminal width without overflowing.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(12)]
        [InlineData(32)]
        [InlineData(80)]
        public void Canvas_DrawingPrimitives_DoNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Canvas(40, 8, canvas =>
                {
                    canvas.Rectangle(0, 0, 39, 7, '#', ConsoleColor.Cyan);
                    canvas.Line(1, 6, 38, 1, '*', ConsoleColor.Green);
                    canvas.Text(3, 3, "Consoul canvas", ConsoleColor.Yellow);
                }, new CanvasOptions { MaxWidth = width });

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#"));
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("*") || write.Text.Contains("C"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Canvas cells honor colors and filled rectangles.
        /// </summary>
        [Fact]
        public void Canvas_FillAndText_RenderColors()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                Consoul.Canvas(12, 4, canvas =>
                {
                    canvas.FillRectangle(0, 0, 4, 2, '.', ConsoleColor.DarkGray);
                    canvas.Text(1, 1, "OK", ConsoleColor.Green);
                    canvas.Set(10, 3, '!', new ColorScheme
                    {
                        Color = ConsoleColor.Red,
                        BackgroundColor = ConsoleColor.Black
                    });
                });

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(".") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("O") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("!") && write.Color == ConsoleColor.Red);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Canvas drawing helpers accept the shared palette types without forcing style strings or console colors.
        /// </summary>
        [Fact]
        public void Canvas_ColorHelpers_AcceptSharedPaletteColors()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                AnsiConsole.Write(new Canvas(16, 4)
                    .DefaultColor(RgbColor.DarkGray)
                    .Set(15, 3, 'D')
                    .Set(0, 0, 'S', SpectreColor.Yellow)
                    .Text(2, 0, "API", RgbColor.Cyan)
                    .Line(0, 1, 5, 1, '-', SpectreColor.Green)
                    .Rectangle(7, 0, 4, 3, '#', RgbColor.Magenta)
                    .FillRectangle(12, 0, 3, 2, '.', SpectreColor.Blue));

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("S") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("A") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("-") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#") && write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(".") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("D") && write.Color == ConsoleColor.DarkGray);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style pixel drawing renders compact half-block pairs when no character cells are present.
        /// </summary>
        [Fact]
        public void Canvas_SetPixel_RendersHalfBlockPixels()
        {
            using (var scope = new ConsoleDriverScope(12))
            {
                AnsiConsole.Write(new Canvas(2, 2)
                    .SetPixel(0, 0, ConsoleColor.Red)
                    .SetPixel(0, 1, ConsoleColor.Green)
                    .SetPixel(1, 0, "blue")
                    .SetPixel(1, 1, new Style(ConsoleColor.Yellow)));

                Assert.Contains(scope.Driver.Writes, write => write.Text == "▀" && write.Color == ConsoleColor.Red && write.BackgroundColor == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text == "▀" && write.Color == ConsoleColor.Blue && write.BackgroundColor == ConsoleColor.Yellow);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Pixel drawing helpers accept the shared palette types and preserve half-block foreground/background colors.
        /// </summary>
        [Fact]
        public void Canvas_SetPixelColorHelpers_AcceptSharedPaletteColors()
        {
            using (var scope = new ConsoleDriverScope(12))
            {
                AnsiConsole.Write(new Canvas(2, 2)
                    .SetPixel(0, 0, SpectreColor.Red)
                    .SetPixel(0, 1, RgbColor.Green)
                    .SetPixel(1, 0, RgbColor.Blue)
                    .SetPixel(1, 1, SpectreColor.Yellow));

                Assert.Contains(scope.Driver.Writes, write => write.Text == "▀" && write.Color == ConsoleColor.Red && write.BackgroundColor == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text == "▀" && write.Color == ConsoleColor.Blue && write.BackgroundColor == ConsoleColor.Yellow);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Pixel canvas scaling can be disabled to clip to the configured render width instead of resampling.
        /// </summary>
        [Fact]
        public void Canvas_SetPixel_CanDisableScaling()
        {
            using (var scope = new ConsoleDriverScope(8))
            {
                var canvas = new Canvas(8, 2)
                    .Scale(false)
                    .MaxWidth(4);

                for (int x = 0; x < 8; x++)
                {
                    canvas.SetPixel(x, 0, x < 4 ? ConsoleColor.Cyan : ConsoleColor.Red);
                    canvas.SetPixel(x, 1, x < 4 ? ConsoleColor.Blue : ConsoleColor.Yellow);
                }

                AnsiConsole.Write(canvas);

                FakeConsoleWrite[] pixels = scope.Driver.Writes.Where(write => write.Text == "▀").ToArray();
                Assert.Equal(4, pixels.Length);
                Assert.All(pixels, write => Assert.Equal(ConsoleColor.Cyan, write.Color));
                Assert.All(pixels, write => Assert.Equal(ConsoleColor.Blue, write.BackgroundColor));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Pixel operations can be mixed with diagram primitives without losing text labels.
        /// </summary>
        [Fact]
        public void Canvas_SetPixel_MixesWithDiagramPrimitives()
        {
            using (var scope = new ConsoleDriverScope(16))
            {
                AnsiConsole.Write(new Canvas(8, 3)
                    .SetPixel(0, 0, ConsoleColor.DarkBlue)
                    .Rectangle(1, 0, 6, 3, '#', ConsoleColor.Cyan)
                    .Text(2, 1, "OK", ConsoleColor.Yellow));

                Assert.Contains(scope.Driver.Writes, write => write.Text == " " && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#") && write.Color == ConsoleColor.Cyan);
                VisualAssert.ContainsText(scope.Driver, "OK");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Base canvas entries expose property-based render settings for compatibility with initializer-style code.
        /// </summary>
        [Fact]
        public void CanvasEntry_RenderSettings_CanBeConfiguredWithProperties()
        {
            using (var scope = new ConsoleDriverScope(8))
            {
                var canvas = new CanvasEntry(4, 2)
                {
                    MaxWidth = 2,
                    Scale = false
                };

                canvas.SetPixel(0, 0, ConsoleColor.Green)
                    .SetPixel(0, 1, ConsoleColor.DarkGreen)
                    .SetPixel(1, 0, ConsoleColor.Green)
                    .SetPixel(1, 1, ConsoleColor.DarkGreen)
                    .SetPixel(2, 0, ConsoleColor.Red)
                    .SetPixel(2, 1, ConsoleColor.DarkRed);

                AnsiConsole.Write(canvas);

                FakeConsoleWrite[] pixels = scope.Driver.Writes.Where(write => write.Text == "▀").ToArray();
                Assert.Equal(2, pixels.Length);
                Assert.All(pixels, write => Assert.Equal(ConsoleColor.Green, write.Color));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Existing canvas instances can be rendered directly.
        /// </summary>
        [Fact]
        public void Canvas_InstanceOverload_RendersExistingCanvas()
        {
            using (var scope = new ConsoleDriverScope(24))
            {
                var canvas = new CanvasEntry(10, 3);
                canvas.Text(0, 0, "Node")
                    .Line(4, 0, 8, 2, '-')
                    .Set(9, 2, '>');

                Consoul.Canvas(canvas);

                VisualAssert.ContainsText(scope.Driver, "Node");
                VisualAssert.ContainsText(scope.Driver, ">");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Canvas content composes inside captured panels.
        /// </summary>
        [Fact]
        public void Canvas_ComposesInsidePanel()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Consoul.Panel(
                    () => Consoul.Canvas(16, 4, canvas =>
                    {
                        canvas.Rectangle(0, 0, 15, 3, '#');
                        canvas.Text(3, 1, "API");
                    }),
                    title: "Diagram",
                    width: 24);

                VisualAssert.WroteText(scope.Driver, "Diagram");
                VisualAssert.WroteText(scope.Driver, "API");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed dynamic views can render canvas sections.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderCanvasSections()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var view = Consoul.View("Flow", new FlowModel { Name = "Build" })
                        .RenderModel(model =>
                        {
                            Consoul.Canvas(20, 5, canvas =>
                            {
                                canvas.Rectangle(0, 1, 8, 3, '#', ConsoleColor.Cyan);
                                canvas.Text(1, 2, model.Name, ConsoleColor.Cyan);
                                canvas.Line(7, 2, 13, 2, '-');
                                canvas.Rectangle(13, 1, 7, 3, '#', ConsoleColor.Green);
                                canvas.Text(14, 2, "Ship", ConsoleColor.Green);
                            }, new CanvasOptions { MaxWidth = 32 });
                        })
                        .Option("Rename", model => model.Name = "Test");

                    view.Render();

                    VisualAssert.ContainsText(scope.Driver, "Build");
                    VisualAssert.ContainsText(scope.Driver, "Ship");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        private sealed class FlowModel
        {
            public string Name { get; set; } = string.Empty;
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
