using System;
using System.Globalization;
using System.IO;
using System.Linq;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for fixed-position progress bars.
    /// </summary>
    public class ProgressBarRenderingTests
    {
        /// <summary>
        /// Progress bars clamp progress values and recalculate their rendered width when the terminal narrows.
        /// </summary>
        [Fact]
        public void ProgressBar_Update_ClampsProgressAndUsesCurrentTerminalWidth()
        {
            using (var scope = new ConsoleDriverScope(24))
            {
                using (var progress = new ProgressBar("Starting responsive render")
                {
                    BarWidth = 80,
                    BlockCharacter = '#',
                    RefreshRate = 0
                })
                {
                    progress.Update(double.PositiveInfinity, "Complete");

                    Assert.Equal(1.0, progress.Progress);
                    Assert.Contains(scope.Driver.Writes, write => write.Text == new string('#', 23));
                    VisualAssert.NoOverflow(scope.Driver);

                    scope.Driver.Resize(12);
                    int writeCount = scope.Driver.Writes.Count;

                    progress.Update(1.0, "Narrow");

                    Assert.Equal(1.0, progress.Progress);
                    Assert.Contains(scope.Driver.Writes.Skip(writeCount), write => write.Text == new string('#', 11));
                    VisualAssert.NoOverflow(scope.Driver);

                    progress.Update(double.NaN, "Reset");

                    Assert.Equal(0.0, progress.Progress);
                    VisualAssert.NoOverflow(scope.Driver);
                }
            }
        }

        /// <summary>
        /// Optional track and percentage rendering reserve space before writing to narrow terminals.
        /// </summary>
        [Fact]
        public void ProgressBar_TrackAndPercentage_StayWithinTerminalWidth()
        {
            using (var scope = new ConsoleDriverScope(30))
            {
                using (var progress = new ProgressBar("Halfway")
                {
                    BarWidth = 80,
                    BlockCharacter = '#',
                    EmptyCharacter = '-',
                    ShowTrack = true,
                    ShowPercentage = true,
                    RefreshRate = 0
                })
                {
                    progress.Update(0.5, "Halfway through a long running task");
                }

                string row = scope.Driver.Writes
                    .Select(write => write.Text)
                    .LastOrDefault(text => text.Contains("50%")) ?? string.Empty;

                Assert.False(string.IsNullOrEmpty(row));
                Assert.Contains("#", row);
                Assert.Contains("-", row);
                Assert.True(row.Length <= scope.Driver.BufferWidth, $"Progress row exceeded width {scope.Driver.BufferWidth}: '{row}'");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// The default progress bar remains filled-only without an empty track or percentage suffix.
        /// </summary>
        [Fact]
        public void ProgressBar_DefaultRendering_RemainsFilledOnly()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                using (var progress = new ProgressBar("Default")
                {
                    BarWidth = 10,
                    BlockCharacter = '=',
                    RefreshRate = 0
                })
                {
                    progress.Update(0.45, "Almost half");
                }

                Assert.Contains(scope.Driver.Writes, write => write.Text == "====");
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("%"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Styled progress updates apply reusable foreground and background styles to both message and bar rows.
        /// </summary>
        [Fact]
        public void ProgressBar_UpdateStyled_RendersReusableStyles()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                using (var progress = new ProgressBar("Styled")
                {
                    BarWidth = 8,
                    BlockCharacter = '#',
                    RefreshRate = 0
                })
                {
                    progress.UpdateStyled(
                        0.5,
                        "Styled progress",
                        new Style(ConsoleColor.Cyan, ConsoleColor.DarkBlue),
                        new Style(ConsoleColor.Magenta, ConsoleColor.DarkGreen));
                }

                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("Styled progress") &&
                    write.Color == ConsoleColor.Cyan &&
                    write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text == "####" &&
                    write.Color == ConsoleColor.Magenta &&
                    write.BackgroundColor == ConsoleColor.DarkGreen);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Direct shared-palette progress updates map Spectre-style and RGB values through the same renderer.
        /// </summary>
        [Fact]
        public void ProgressBar_Update_AcceptsDirectPaletteValues()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                using (var progress = new ProgressBar("Palette")
                {
                    BarWidth = 8,
                    BlockCharacter = '#',
                    RefreshRate = 0
                })
                {
                    progress.Update(0.5, "Color progress", SpectreColor.Aqua, SpectreColor.Green);
                    progress.Update(0.75, "RGB progress", RgbColor.Yellow, RgbColor.Red);
                }

                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("Color progress") &&
                    write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text == "####" &&
                    write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("RGB progress") &&
                    write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text == "######" &&
                    write.Color == ConsoleColor.Red);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// RGB and parsed style progress updates preserve true-color and decoration metadata on ANSI-capable terminals.
        /// </summary>
        [Fact]
        public void ProgressBar_RgbAndStyledUpdatesEmitTrueColorSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            var output = new AnsiConsoleOutput(writer, width: 80, height: 12, isTerminal: true);
            var settings = new AnsiConsoleSettings
            {
                Out = output,
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            };

            using (Consoul.UseConsoleDriver(new TextWriterConsoleDriver(output, settings)))
            using (var progress = new ProgressBar("Initial")
            {
                BarWidth = 8,
                BlockCharacter = '#',
                RefreshRate = 0
            })
            {
                progress.Update(
                    0.5,
                    "RGB progress",
                    RgbColor.FromHex("#336699"),
                    RgbColor.FromHex("#22c55e"));
                progress.UpdateStyled(
                    0.75,
                    "Styled progress",
                    Style.Parse("italic #778899"),
                    Style.Parse("bold #abcdef"));
            }

            string rendered = writer.ToString();
            Assert.Contains("\u001b[38;2;51;102;153mRGB progress", rendered);
            Assert.Contains("\u001b[38;2;34;197;94m####", rendered);
            Assert.Contains("\u001b[3;38;2;119;136;153mStyled progress", rendered);
            Assert.Contains("\u001b[1;38;2;171;205;239m######", rendered);
        }

        /// <summary>
        /// Progress bar messages parse raw markup while the bar row remains literal progress text.
        /// </summary>
        [Fact]
        public void ProgressBar_MarkupMessages_RenderVisibleTextAndColors()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                using (var progress = new ProgressBar("[green]Starting[/] [[prod]]")
                {
                    BarWidth = 8,
                    BlockCharacter = '#',
                    RefreshRate = 0
                })
                {
                    progress.UpdateStyled(
                        0.5,
                        "[cyan]Halfway[/] [[prod]]",
                        new Style(ConsoleColor.Yellow, ConsoleColor.DarkBlue),
                        new Style(ConsoleColor.Magenta, ConsoleColor.DarkGreen));
                }

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Starting") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Halfway") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains(" [prod]") &&
                    write.Color == ConsoleColor.Yellow &&
                    write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text == "####" &&
                    write.Color == ConsoleColor.Magenta &&
                    write.BackgroundColor == ConsoleColor.DarkGreen);
                Assert.DoesNotContain(scope.Driver.Writes, write =>
                    write.Text.Contains("[green]") ||
                    write.Text.Contains("[cyan]") ||
                    write.Text.Contains("[/]"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Percentage rendering is suppressed when the terminal is too narrow to show both a bar and suffix.
        /// </summary>
        [Fact]
        public void ProgressBar_Percentage_HidesWhenTerminalIsTooNarrow()
        {
            using (var scope = new ConsoleDriverScope(5))
            {
                using (var progress = new ProgressBar("Tiny")
                {
                    BarWidth = 80,
                    BlockCharacter = '#',
                    EmptyCharacter = '-',
                    ShowTrack = true,
                    ShowPercentage = true,
                    RefreshRate = 0
                })
                {
                    progress.Update(1.0, "Done");
                }

                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("%"));
                Assert.Contains(scope.Driver.Writes, write => write.Text == "####");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }
    }
}
