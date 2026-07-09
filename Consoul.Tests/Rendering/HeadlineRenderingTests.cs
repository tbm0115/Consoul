using System;
using System.Globalization;
using System.IO;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for headline view helpers.
    /// </summary>
    public class HeadlineRenderingTests
    {
        /// <summary>
        /// Block headlines wrap character groups and stay inside the active terminal width.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(12)]
        [InlineData(32)]
        [InlineData(80)]
        public void Headline_BlockStyle_DoesNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Headline("Consoul 2", new HeadlineOptions
                {
                    Width = width,
                    Style = HeadlineStyle.Block,
                    BlockCharacter = '#'
                });

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Headline styles reuse the expected underlying structured output helpers.
        /// </summary>
        [Fact]
        public void Headline_AlternateStyles_RenderReadableText()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Consoul.Headline("Plain", new HeadlineOptions { Width = 20, Style = HeadlineStyle.Plain });
                Consoul.Headline("Box", new HeadlineOptions { Width = 20, Style = HeadlineStyle.Box });
                Consoul.Headline("Rule", new HeadlineOptions { Width = 20, Style = HeadlineStyle.Rule });

                VisualAssert.WroteText(scope.Driver, "Plain");
                VisualAssert.WroteText(scope.Driver, "Box");
                VisualAssert.WroteText(scope.Driver, "Rule");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Headline helpers honor configured colors.
        /// </summary>
        [Fact]
        public void Headline_CustomOptions_RenderColors()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Consoul.Headline("Go", new HeadlineOptions
                {
                    Width = 24,
                    Style = HeadlineStyle.Block,
                    Scheme = new ColorScheme
                    {
                        Color = ConsoleColor.Cyan,
                        BackgroundColor = ConsoleColor.Black
                    }
                });

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#") && write.Color == ConsoleColor.Cyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Headline options preserve RGB and decoration metadata across headline styles.
        /// </summary>
        [Fact]
        public void HeadlineOptions_TextMetadataEmitsTrueColorSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 48, height: 20, isTerminal: true),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });

            var options = new[]
            {
                new HeadlineOptions { Width = 24, Style = HeadlineStyle.Plain },
                new HeadlineOptions { Width = 24, Style = HeadlineStyle.Box },
                new HeadlineOptions { Width = 24, Style = HeadlineStyle.Rule },
                new HeadlineOptions { Width = 24, Style = HeadlineStyle.Block, Alignment = TextBlockAlignment.Left, BlockCharacter = '@', Pad = false }
            };

            foreach (HeadlineOptions option in options)
            {
                option.Scheme = new ColorScheme
                {
                    Color = ConsoleColor.Green,
                    BackgroundColor = ConsoleColor.Black
                };
                option.Decoration = Decoration.Bold;
                option.ForegroundRgb = RgbColor.FromHex("#22c55e");
            }

            console.Write(new HeadlineEntry("Plain", options[0]));
            console.Write(new HeadlineEntry("Box", options[1]));
            console.Write(new HeadlineEntry("Rule", options[2]));
            console.Write(new HeadlineEntry("I", options[3]));

            string output = writer.ToString();
            Assert.Contains("\u001b[1;38;2;34;197;94mPlain", output);
            Assert.Contains("\u001b[1;38;2;34;197;94mBox", output);
            Assert.Contains("\u001b[1;38;2;34;197;94mRule", output);
            Assert.Contains("\u001b[1;38;2;34;197;94m@@@", output);
        }

        /// <summary>
        /// Model-backed dynamic views can render headlines as section openers.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderHeadlineSections()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var view = Consoul.View("Dashboard", new DashboardModel { Name = "Build" })
                        .RenderModel(model =>
                        {
                            Consoul.Headline(model.Name, new HeadlineOptions
                            {
                                Width = 32,
                                Style = HeadlineStyle.Rule
                            });
                        })
                        .Option("Refresh", model => model.Name = "Refreshed");

                    view.Render();

                    VisualAssert.WroteText(scope.Driver, "Build");
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
    }
}
