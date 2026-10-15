using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ConsoulLibrary.Color;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for sparkline view helpers.
    /// </summary>
    public class SparklineRenderingTests
    {
        /// <summary>
        /// Sparklines render labels and trend characters within the active terminal width.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(12)]
        [InlineData(32)]
        [InlineData(80)]
        public void Sparkline_TrendValues_DoNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Sparkline(
                    new[] { 1d, 3d, 2d, 5d, 8d, 13d, 21d },
                    new SparklineOptions
                    {
                        Label = "Build duration trend",
                        Width = width,
                        ShowRange = true,
                        Characters = "123456789"
                    });

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("B"));
                Assert.Contains(scope.Driver.Writes, write => write.Text.Any(char.IsDigit));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Sparkline options honor custom colors, explicit ranges, and value formatting.
        /// </summary>
        [Fact]
        public void Sparkline_CustomOptions_RenderColorsAndRange()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Consoul.Sparkline(new[] { -2d, 0d, 2d }, new SparklineOptions
                {
                    Width = 32,
                    Label = "Temp",
                    MinimumValue = -4,
                    MaximumValue = 4,
                    ShowRange = true,
                    Characters = "abc",
                    ValueFormatter = value => value.ToString("0.0"),
                    LabelScheme = Scheme(ConsoleColor.Yellow),
                    LineScheme = Scheme(ConsoleColor.Cyan),
                    RangeScheme = Scheme(ConsoleColor.DarkGray)
                });

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Temp") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("c") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("-4.0..4.0") && write.Color == ConsoleColor.DarkGray);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Sparkline labels treat raw markup as styled visible text.
        /// </summary>
        /// <param name="width">The configured sparkline width.</param>
        [Theory]
        [InlineData(24)]
        [InlineData(40)]
        public void Sparkline_RawMarkupLabel_DoesNotLeakTagsOrOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Sparkline(new[] { 1d, 3d, 2d, 5d, 8d }, new SparklineOptions
                {
                    Width = width,
                    Label = "[cyan]Duration [[prod]][/]",
                    LabelWidth = Math.Min(18, width - 4),
                    Characters = "123456789",
                    LabelScheme = Scheme(ConsoleColor.DarkGray),
                    LineScheme = Scheme(ConsoleColor.Green)
                });

                VisualAssert.ContainsText(scope.Driver, "Duration [prod]");
                Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Duration [prod]") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Any(char.IsDigit) && write.Color == ConsoleColor.Green);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= width, "Raw markup sparkline line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Sparkline style helpers preserve RGB and decoration metadata for true-color terminals.
        /// </summary>
        [Fact]
        public void Sparkline_StyleHelpersEmitTrueColorSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 12, isTerminal: true),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });

            var sparkline = new Sparkline(new[] { 1d, 3d, 2d, 5d, 8d })
                .Width(60)
                .Label("Duration")
                .LabelColor("bold #22c55e")
                .LineColor("#336699")
                .RangeColor("underline #112233")
                .ShowRange()
                .Characters("123456789");

            console.Write(sparkline);

            string output = writer.ToString();
            Assert.Contains("\u001b[1;38;2;34;197;94mDuration", output);
            Assert.Contains("\u001b[38;2;51;102;153m", output);
            Assert.Contains("\u001b[4;38;2;17;34;51m1..8", output);
        }

        /// <summary>
        /// Sparkline RGB helpers use 256-color SGR when the terminal supports eight-bit color.
        /// </summary>
        [Fact]
        public void Sparkline_RgbHelpersEmitEightBitSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 12, isTerminal: true),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.EightBit
            });

            var sparkline = new Sparkline(new[] { 1d, 3d, 2d, 5d, 8d })
                .Width(60)
                .Label("API")
                .LabelColor(RgbColor.FromHex("#445566"))
                .LineColor(RgbColor.FromHex("#336699"))
                .RangeColor(RgbColor.FromHex("#112233"))
                .ShowRange()
                .Characters("123456789");

            console.Write(sparkline);

            string output = writer.ToString();
            Assert.Contains("\u001b[38;5;239mAPI", output);
            Assert.Contains("\u001b[38;5;60m", output);
            Assert.Contains("\u001b[38;5;234m1..8", output);
            Assert.DoesNotContain("\u001b[38;2", output);
        }

        /// <summary>
        /// Common Spectre-style sparkline aliases map to the existing range and formatter options.
        /// </summary>
        [Fact]
        public void Sparkline_SpectreStyleAliases_MapToExistingOptions()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var sparkline = new Sparkline(new[] { 1d, 5d, 9d })
                    .Width(40)
                    .Label("Latency")
                    .WithMinValue(0)
                    .MinValue(1)
                    .WithMaxValue(10)
                    .MaxValue(9)
                    .ShowRange()
                    .Characters("123")
                    .UseValueFormatter(value => value.ToString("0", CultureInfo.InvariantCulture) + " ms");

                AnsiConsole.Write(sparkline);

                VisualAssert.ContainsText(scope.Driver, "Latency");
                VisualAssert.ContainsText(scope.Driver, "1 ms..9 ms");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("3") && write.Color == RenderOptions.OptionColor);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Sparkline rendering ignores non-finite values and handles flat data.
        /// </summary>
        [Fact]
        public void Sparkline_NonFiniteAndFlatValues_RenderSafely()
        {
            using (var scope = new ConsoleDriverScope(24))
            {
                Consoul.Sparkline(new[] { double.NaN, 5d, double.PositiveInfinity, 5d }, new SparklineOptions
                {
                    Width = 20,
                    Characters = ".#"
                });

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Builder overloads add values fluently before rendering.
        /// </summary>
        [Fact]
        public void Sparkline_BuilderOverload_AddsValues()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                Consoul.Sparkline(line =>
                {
                    line.Add(1).Add(2).Add(3);
                }, new SparklineOptions { Label = "Rate", Characters = "abc" });

                VisualAssert.WroteText(scope.Driver, "Rate");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("c"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Builder callbacks accept immediate value params without allocating a separate sequence.
        /// </summary>
        [Fact]
        public void Sparkline_BuilderCallback_AddRangeAcceptsParams()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                SparklineEntry captured = new SparklineEntry();

                Consoul.Sparkline(line =>
                {
                    captured = line.AddRange(1, double.NaN, 2, double.PositiveInfinity, 3);
                }, new SparklineOptions { Label = "Rate", ShowRange = true, Characters = "abc" });

                Assert.Equal(new[] { 1d, 2d, 3d }, captured.Values);
                VisualAssert.ContainsText(scope.Driver, "Rate");
                VisualAssert.ContainsText(scope.Driver, "1..3");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("c"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Fluent sparkline builders accept immediate value params for compact MVC-style dashboard snippets.
        /// </summary>
        [Fact]
        public void Sparkline_FluentBuilder_AddValuesAcceptsParams()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var sparkline = new Sparkline()
                    .Width(32)
                    .Label("Rate")
                    .ShowRange()
                    .Characters("abc")
                    .AddValues(1, double.NaN, 2, double.PositiveInfinity, 3);

                AnsiConsole.Write(sparkline);

                Assert.Equal(new[] { 1d, 2d, 3d }, sparkline.Values);
                VisualAssert.ContainsText(scope.Driver, "Rate");
                VisualAssert.ContainsText(scope.Driver, "1..3");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("c"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed dynamic views can render sparkline sections.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderSparklineSections()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var model = new DashboardModel
                    {
                        Durations = new List<double> { 12, 10, 11, 8, 7, 9 }
                    };

                    var view = Consoul.View("Dashboard", model)
                        .RenderModel(dashboard =>
                        {
                            Consoul.Sparkline(dashboard.Durations, "Duration", width: 36, color: ConsoleColor.Green);
                        })
                        .Option("Refresh", dashboard => dashboard.Durations.Add(6));

                    view.Render();

                    VisualAssert.WroteText(scope.Driver, "Duration");
                    Assert.Contains(scope.Driver.Writes, write => write.Color == ConsoleColor.Green);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Enumerable.Empty<string>()));
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

        private sealed class DashboardModel
        {
            public List<double> Durations { get; set; } = new List<double>();
        }

        private sealed class TestRoutine : Routine
        {
            public TestRoutine(IEnumerable<string> inputs)
            {
                foreach (string input in inputs)
                {
                    Enqueue(new RoutineInput { Value = input });
                }
            }
        }
    }
}
