using System;
using System.Linq;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for dependency-free terminal canvas images.
    /// </summary>
    public class CanvasImageRenderingTests
    {
        /// <summary>
        /// Console-color matrices render as terminal half-block pairs with top foreground and bottom background colors.
        /// </summary>
        [Fact]
        public void CanvasImage_ColorMatrix_RendersHalfBlocksWithForegroundAndBackground()
        {
            using (var scope = new ConsoleDriverScope(12))
            {
                var pixels = new[,]
                {
                    { ConsoleColor.Red, ConsoleColor.Green, ConsoleColor.Blue, ConsoleColor.Yellow },
                    { ConsoleColor.Cyan, ConsoleColor.Magenta, ConsoleColor.White, ConsoleColor.Black }
                };

                AnsiConsole.Write(new CanvasImage(pixels).NearestNeighborResampler().NoMaxWidth());

                Assert.Contains(scope.Driver.Writes, write => write.Text == "▀" && write.Color == ConsoleColor.Red && write.BackgroundColor == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text == "▀" && write.Color == ConsoleColor.Green && write.BackgroundColor == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text == "▀" && write.Color == ConsoleColor.Blue && write.BackgroundColor == ConsoleColor.White);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Canvas images respect terminal width and configured max width when scaled from larger source pixels.
        /// </summary>
        /// <param name="terminalWidth">The fake terminal width.</param>
        [Theory]
        [InlineData(8)]
        [InlineData(20)]
        [InlineData(80)]
        public void CanvasImage_ResponsiveScaling_DoesNotOverflow(int terminalWidth)
        {
            using (var scope = new ConsoleDriverScope(terminalWidth))
            {
                var image = new CanvasImage(
                        40,
                        20,
                        (x, y) => new CanvasImagePixel(
                            (byte)Math.Min(255, x * 6),
                            (byte)Math.Min(255, y * 12),
                            (byte)Math.Min(255, (x + y) * 4)))
                    .PixelWidth(60)
                    .MaxWidth(24)
                    .MaxHeight(8)
                    .BilinearResampler();

                AnsiConsole.Write(image);

                int expectedWidth = Math.Min(terminalWidth, 24);
                Assert.All(scope.Driver.Lines.Where(line => line.Length > 0), line => Assert.True(line.Length <= expectedWidth, $"Line length {line.Length} exceeded expected render width {expectedWidth}: '{line}'"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Fluent scale aliases expose the existing clipping mode for small POC thumbnails.
        /// </summary>
        [Fact]
        public void CanvasImage_ScaleAliases_ClipInsteadOfResamplingWhenDisabled()
        {
            using (var scope = new ConsoleDriverScope(16))
            {
                var pixels = new[,]
                {
                    { ConsoleColor.Red, ConsoleColor.Green, ConsoleColor.Blue, ConsoleColor.Yellow, ConsoleColor.DarkYellow, ConsoleColor.DarkRed },
                    { ConsoleColor.Cyan, ConsoleColor.Magenta, ConsoleColor.White, ConsoleColor.Gray, ConsoleColor.DarkGray, ConsoleColor.Black },
                    { ConsoleColor.DarkBlue, ConsoleColor.DarkGreen, ConsoleColor.DarkCyan, ConsoleColor.DarkMagenta, ConsoleColor.Red, ConsoleColor.Green },
                    { ConsoleColor.Blue, ConsoleColor.Yellow, ConsoleColor.Cyan, ConsoleColor.Magenta, ConsoleColor.White, ConsoleColor.Gray }
                };
                var image = new CanvasImage(pixels)
                    .PixelWidth(12)
                    .MaxWidth(3)
                    .MaxHeight(1)
                    .NoScale()
                    .NearestNeighborResampler();

                AnsiConsole.Write(image);

                Assert.False(image.Options.Scale);
                image.Scale();
                Assert.True(image.Options.Scale);
                image.Scale(false);
                Assert.False(image.Options.Scale);
                Assert.Contains(scope.Driver.Lines, line => line.Length == 3);
                Assert.All(scope.Driver.Lines.Where(line => line.Length > 0), line => Assert.True(line.Length <= 3, "No-scale image line exceeded clipped width: " + line));
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Color == ConsoleColor.Yellow);
                Assert.DoesNotContain(scope.Driver.Writes, write => write.BackgroundColor == ConsoleColor.Gray);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Transparent source pixels use upper or lower half-blocks instead of forcing a background color.
        /// </summary>
        [Fact]
        public void CanvasImage_TransparentPixels_RenderUpperAndLowerBlocks()
        {
            using (var scope = new ConsoleDriverScope(8))
            {
                var pixels = new[,]
                {
                    { CanvasImagePixel.Transparent, CanvasImagePixel.FromConsoleColor(ConsoleColor.Red) },
                    { CanvasImagePixel.FromConsoleColor(ConsoleColor.Green), CanvasImagePixel.Transparent }
                };

                AnsiConsole.Write(new CanvasImage(pixels).NearestNeighborResampler());

                Assert.Contains(scope.Driver.Writes, write => write.Text == "▄" && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text == "▀" && write.Color == ConsoleColor.Red);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Static facade overloads render canvas images without requiring a real console handle.
        /// </summary>
        [Fact]
        public void CanvasImage_FacadeOverloads_RenderThroughVirtualConsole()
        {
            using (var scope = new ConsoleDriverScope(10))
            {
                var pixels = new[,]
                {
                    { ConsoleColor.DarkBlue, ConsoleColor.DarkCyan },
                    { ConsoleColor.DarkGreen, ConsoleColor.DarkYellow }
                };

                Consoul.CanvasImage(pixels, new CanvasImageOptions { PixelWidth = 2 });
                AnsiConsole.CanvasImage(pixels, new CanvasImageOptions { PixelWidth = 2 });

                Assert.True(scope.Driver.Writes.Count(write => write.Text == "▀") >= 4);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Canvas images compose inside other renderables through the existing renderable surface.
        /// </summary>
        [Fact]
        public void CanvasImage_ComposesInsidePanel()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                var image = new CanvasImage(8, 4, (x, y) =>
                {
                    return (x + y) % 2 == 0
                        ? CanvasImagePixel.FromConsoleColor(ConsoleColor.Cyan)
                        : CanvasImagePixel.FromConsoleColor(ConsoleColor.DarkBlue);
                }).MaxWidth(8);

                AnsiConsole.Write(new Panel(image).Header("Preview").Width(16));

                VisualAssert.WroteText(scope.Driver, "Preview");
                VisualAssert.ContainsText(scope.Driver, "▀");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }
    }
}
