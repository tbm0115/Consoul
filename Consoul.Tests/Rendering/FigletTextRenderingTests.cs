using System;
using System.Globalization;
using System.IO;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for figlet-style text helpers.
    /// </summary>
    public class FigletTextRenderingTests
    {
        /// <summary>
        /// Figlet text wraps glyph groups and stays inside the active terminal width.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(12)]
        [InlineData(32)]
        [InlineData(80)]
        public void FigletText_DoesNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.FigletText("Consoul 2", width: width);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Figlet text convenience overloads honor color and glyph configuration.
        /// </summary>
        [Fact]
        public void FigletText_ConvenienceOverload_RendersCustomColorAndCharacter()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Consoul.FigletText(
                    "Go",
                    width: 24,
                    color: ConsoleColor.Cyan,
                    backgroundColor: ConsoleColor.Black,
                    character: '*');

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("*") && write.Color == ConsoleColor.Cyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style layout mode helpers compact inter-glyph spacing for migration-friendly splash text.
        /// </summary>
        [Fact]
        public void FigletText_LayoutModeHelpers_CompactGlyphSpacing()
        {
            int fullWidth;
            using (var fullScope = new ConsoleDriverScope(40))
            {
                new FigletText("HI").Width(40).LeftJustified().FullSize().Render();
                fullWidth = MaxWrittenLineLength(fullScope.Driver);
                VisualAssert.NoOverflow(fullScope.Driver);
            }

            int fittedWidth;
            using (var fittedScope = new ConsoleDriverScope(40))
            {
                new FigletText("HI").Width(40).LeftJustified().Fitted().Render();
                fittedWidth = MaxWrittenLineLength(fittedScope.Driver);
                VisualAssert.NoOverflow(fittedScope.Driver);
            }

            Assert.True(fittedWidth < fullWidth, $"Expected fitted width {fittedWidth} to be less than full width {fullWidth}.");
        }

        /// <summary>
        /// Figlet text builders accept aligned aliases alongside justified aliases.
        /// </summary>
        [Fact]
        public void FigletText_AlignedAliases_MapToExpectedJustification()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                new FigletText("I").Width(12).LeftAligned().Pad(true).Render();
                new FigletText("I").Width(12).RightAligned().Pad(true).Render();

                Assert.Contains(scope.Driver.Lines, line => line == "###         ");
                Assert.Contains(scope.Driver.Lines, line => line == "         ###");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style padding control avoids expanding each rendered glyph line to the requested width.
        /// </summary>
        [Fact]
        public void FigletText_PadFalse_DoesNotPadLinesToFullWidth()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                new FigletText("HI")
                    .Width(32)
                    .LeftJustified()
                    .Pad(false)
                    .Render();

                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length < 32, "Unpadded figlet line should not expand to the configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Figlet color helpers preserve RGB and decoration metadata for true-color terminals.
        /// </summary>
        [Fact]
        public void FigletText_StyleHelpersEmitTrueColorSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 48, height: 20, isTerminal: true),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });

            console.Write(new FigletText("I").Width(12).LeftJustified().Pad(false).Color("italic #112233 on #445566"));
            console.Write(new FigletText("I").Width(12).LeftJustified().Pad(false).Color(RgbColor.FromHex("#778899")));
            console.Write(new FigletText("I").Width(12).LeftJustified().Pad(false).Color(SpectreColor.FromHex("#22c55e")));
            console.Write(new FigletText("I").Width(12).LeftJustified().Pad(false).Color(Style.Parse("bold #abcdef")));

            string output = writer.ToString();
            Assert.Contains("\u001b[3;38;2;17;34;51;48;2;68;85;102m", output);
            Assert.Contains("\u001b[38;2;119;136;153m", output);
            Assert.Contains("\u001b[38;2;34;197;94m", output);
            Assert.Contains("\u001b[1;38;2;171;205;239m", output);
            Assert.DoesNotContain("[#112233", output);
            Assert.DoesNotContain("[italic", output);
        }

        /// <summary>
        /// Object-initialized options expose layout and padding controls for reusable figlet renderables.
        /// </summary>
        [Fact]
        public void FigletTextOptions_LayoutAndPadding_AreHonored()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var entry = new FigletTextEntry(
                    "HI",
                    new FigletTextOptions
                    {
                        Width = 32,
                        Alignment = TextBlockAlignment.Left,
                        Character = '@',
                        LayoutMode = FigletLayoutMode.Smushed,
                        Pad = false,
                        Scheme = new ColorScheme { Color = ConsoleColor.Yellow, BackgroundColor = ConsoleColor.Black }
                    });

                entry.Render();

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("@") && write.Color == ConsoleColor.Yellow);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length < 32, "Unpadded figlet options should not expand to the configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Figlet text options are cloned before rendering so callers can reuse option instances safely.
        /// </summary>
        [Fact]
        public void FigletText_Options_AreCloned()
        {
            var options = new FigletTextOptions
            {
                Width = 24,
                Alignment = TextBlockAlignment.Right,
                Character = '@',
                Scheme = new ColorScheme
                {
                    Color = ConsoleColor.Yellow,
                    BackgroundColor = ConsoleColor.Black
                }
            };

            var entry = new FigletTextEntry("MVC", options);
            options.Character = '*';

            using (var scope = new ConsoleDriverScope(48))
            {
                entry.Render();

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("@") && write.Color == ConsoleColor.Yellow);
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("*"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed dynamic views can use figlet text as a splash or section title.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderFigletTextSections()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var view = Consoul.View("Dashboard", new DashboardModel { Name = "Mvc" })
                        .RenderModel(model =>
                        {
                            Consoul.FigletText(model.Name, width: 32, color: ConsoleColor.Green);
                        })
                        .Option("Refresh", model => model.Name = "Refreshed");

                    view.Render();

                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#") && write.Color == ConsoleColor.Green);
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

        private static int MaxWrittenLineLength(FakeConsoleDriver driver)
        {
            int max = 0;
            foreach (string line in driver.Lines)
            {
                if (line.Length > max)
                {
                    max = line.Length;
                }
            }

            return max;
        }
    }
}
