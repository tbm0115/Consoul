using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for spinner scopes.
    /// </summary>
    public class SpinnerRenderingTests
    {
        /// <summary>
        /// Spinner scopes tick, update, and complete in place without overflowing narrow terminals.
        /// </summary>
        [Fact]
        public void SpinnerScope_TickAndSucceed_DoesNotOverflow()
        {
            using (var scope = new ConsoleDriverScope(24))
            {
                using (SpinnerScope spinner = Consoul.Spinner("Starting", new SpinnerOptions { MaxWidth = 24 }))
                {
                    spinner.Tick("Loading a very long spinner message");
                    spinner.Tick("Still working");
                    spinner.Succeed("Done");
                }

                VisualAssert.WroteText(scope.Driver, "| Starting");
                VisualAssert.WroteText(scope.Driver, "/ Loading");
                VisualAssert.WroteText(scope.Driver, "- Still working");
                VisualAssert.WroteText(scope.Driver, "[OK] Done");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spinners honor custom frames and colors.
        /// </summary>
        [Fact]
        public void SpinnerScope_CustomOptions_RenderFramesAndColors()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var options = new SpinnerOptions
                {
                    MaxWidth = 32,
                    Frames = new[] { ".", "o", "O" },
                    ActiveColor = ConsoleColor.Cyan,
                    SuccessColor = ConsoleColor.Green
                };

                using (SpinnerScope spinner = Consoul.Spinner("Scanning", options))
                {
                    spinner.Tick("Reading");
                    spinner.Tick("Finishing");
                    spinner.Succeed();
                }

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(". Scanning") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("o Reading") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("O Finishing") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[OK] Finishing") && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spinner messages parse raw markup in message text while keeping frames and completion prefixes literal.
        /// </summary>
        [Fact]
        public void SpinnerScope_MarkupMessages_RenderVisibleTextAndColors()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                var options = new SpinnerOptions
                {
                    MaxWidth = 40,
                    Frames = new[] { "*" },
                    ActiveColor = ConsoleColor.Yellow,
                    SuccessColor = ConsoleColor.Blue
                };

                using (SpinnerScope spinner = Consoul.Spinner("[green]Starting[/] [[prod]]", options))
                {
                    spinner.Tick("[cyan]Loading[/] [[prod]]");
                    spinner.Succeed("[green]Ready[/] [[prod]]");
                }

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("* ") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Starting") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Loading") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(" [prod]") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(" [prod]") && write.Color == ConsoleColor.Blue);
                Assert.DoesNotContain(scope.Driver.Writes, write =>
                    write.Text.Contains("[green]") ||
                    write.Text.Contains("[cyan]") ||
                    write.Text.Contains("[/]"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spinner builders keep parsed spinner-style metadata until the started scope renders it.
        /// </summary>
        [Fact]
        public void SpinnerBuilder_SpinnerStylePreservesTrueColorSgrWhenSupported()
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
            {
                AnsiConsole.Spinner()
                    .Spinner(Spinner.Known.Line)
                    .SpinnerStyle("italic #778899")
                    .RefreshRate(0)
                    .Start("Builder initial", spinner =>
                    {
                        spinner.Tick("Builder styled");
                    });
            }

            string rendered = writer.ToString();
            Assert.Contains("\u001b[3;38;2;119;136;153m", rendered);
            Assert.Contains("Builder styled", rendered);
        }

        /// <summary>
        /// Common Spectre-style spinner aliases can be used by spinner and status builders.
        /// </summary>
        [Fact]
        public void SpinnerKnown_CommonAliases_RenderThroughBuilders()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Assert.Same(Spinner.Known.Line, Spinner.Known.Pipe);
                Assert.Same(Spinner.Known.Dots, Spinner.Known.SimpleDots);
                Assert.NotEmpty(Spinner.Known.SimpleDotsScrolling.Frames);
                Assert.NotEmpty(Spinner.Known.Dots2.Frames);
                Assert.NotEmpty(Spinner.Known.Flip.Frames);
                Assert.NotEmpty(Spinner.Known.Bounce.Frames);
                Assert.NotEmpty(Spinner.Known.BouncingBall.Frames);
                Assert.NotEmpty(Spinner.Known.Arrow.Frames);
                Assert.NotEmpty(Spinner.Known.Line2.Frames);
                Assert.NotEmpty(Spinner.Known.Dots3.Frames);
                Assert.NotEmpty(Spinner.Known.Dots12.Frames);
                Assert.NotEmpty(Spinner.Known.Star2.Frames);
                Assert.NotEmpty(Spinner.Known.Hamburger.Frames);
                Assert.NotEmpty(Spinner.Known.GrowVertical.Frames);
                Assert.NotEmpty(Spinner.Known.GrowHorizontal.Frames);
                Assert.NotEmpty(Spinner.Known.BoxBounce.Frames);
                Assert.NotEmpty(Spinner.Known.BoxBounce2.Frames);
                Assert.NotEmpty(Spinner.Known.Triangle.Frames);
                Assert.NotEmpty(Spinner.Known.Arc.Frames);
                Assert.NotEmpty(Spinner.Known.Circle.Frames);
                Assert.NotEmpty(Spinner.Known.Toggle.Frames);
                Assert.NotEmpty(Spinner.Known.Toggle13.Frames);
                Assert.NotEmpty(Spinner.Known.Clock.Frames);
                Assert.NotEmpty(Spinner.Known.Moon.Frames);
                Assert.NotEmpty(Spinner.Known.Pong.Frames);
                Assert.NotEmpty(Spinner.Known.BetaWave.Frames);

                AnsiConsole.Spinner()
                    .Spinner(Spinner.Known.Pipe)
                    .Start("Loading", spinner =>
                    {
                        spinner.Tick("Queued");
                    });

                AnsiConsole.Status()
                    .Spinner(Spinner.Known.SimpleDots)
                    .Start("Checking", status =>
                    {
                        status.Status("Ready");
                    });

                AnsiConsole.Status()
                    .Spinner(Spinner.Known.Triangle)
                    .Start("Triangulating", status =>
                    {
                        status.Status("Aligned");
                    });

                VisualAssert.WroteText(scope.Driver, "| Loading");
                VisualAssert.WroteText(scope.Driver, "/ Queued");
                VisualAssert.WroteText(scope.Driver, ". Checking");
                VisualAssert.WroteText(scope.Driver, ".. Ready");
                VisualAssert.WroteText(scope.Driver, "/\\ Triangulating");
                VisualAssert.WroteText(scope.Driver, "|> Aligned");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Reusable spinners accept generated frame collections for MVC-style status palettes.
        /// </summary>
        [Fact]
        public void Spinner_Constructors_AcceptEnumerableFrames()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                IEnumerable<string> frames = new List<string> { "G1", "G2" };
                IEnumerable<string> namedFrames = new List<string> { "N1", "N2" };

                var generated = new Spinner(frames);
                var named = new Spinner("Generated", namedFrames);

                Assert.Equal(string.Empty, generated.Name);
                Assert.Equal("Generated", named.Name);
                Assert.Equal(new[] { "G1", "G2" }, generated.Frames);
                Assert.Equal(new[] { "N1", "N2" }, named.Frames);

                AnsiConsole.Spinner()
                    .Spinner(generated)
                    .Start("Generated spinner", spinner =>
                    {
                        spinner.Tick("Generated spinner tick");
                    });

                AnsiConsole.Status()
                    .Spinner(named)
                    .Start("Generated status", status =>
                    {
                        status.Status("Generated status tick");
                    });

                VisualAssert.WroteText(scope.Driver, "G1 Generated spinner");
                VisualAssert.WroteText(scope.Driver, "G2 Generated spinner tick");
                VisualAssert.WroteText(scope.Driver, "N1 Generated status");
                VisualAssert.WroteText(scope.Driver, "N2 Generated status tick");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spinner action helpers automatically mark successful operations as complete.
        /// </summary>
        [Fact]
        public void Spinner_ActionHelper_MarksSuccess()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                Consoul.Spinner("Running", spinner =>
                {
                    spinner.Tick("Working");
                });

                VisualAssert.WroteText(scope.Driver, "| Running");
                VisualAssert.WroteText(scope.Driver, "/ Working");
                VisualAssert.WroteText(scope.Driver, "[OK] Working");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spinner action helpers mark failed operations and rethrow the original exception.
        /// </summary>
        [Fact]
        public void Spinner_ActionHelper_MarksFailureAndRethrows()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                    Consoul.Spinner("Running", spinner =>
                    {
                        spinner.Tick("About to fail");
                        throw new InvalidOperationException("Nope");
                    }));

                Assert.Equal("Nope", exception.Message);
                VisualAssert.WroteText(scope.Driver, "[FAIL] About to fail");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Asynchronous spinner action helpers pass cancellation tokens and mark success.
        /// </summary>
        [Fact]
        public async Task SpinnerAsync_ActionHelper_MarksSuccess()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                bool sawToken = false;
                using (var cancellation = new CancellationTokenSource())
                {
                    await Consoul.SpinnerAsync("Running async", (spinner, token) =>
                    {
                        sawToken = token == cancellation.Token;
                        spinner.Tick("Async work");
                        return Task.CompletedTask;
                    }, cancellationToken: cancellation.Token);
                }

                Assert.True(sawToken);
                VisualAssert.WroteText(scope.Driver, "[OK] Async work");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed dynamic views can render spinner sections from option actions.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRunSpinnerActions()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "1", "2" }));

                try
                {
                    var view = Consoul.View("Dashboard", new DashboardModel())
                        .Option("Refresh", model =>
                        {
                            Consoul.Spinner("Refreshing", spinner =>
                            {
                                spinner.Tick("Loading state");
                                model.Refreshed = true;
                            });
                        });

                    view.Render();

                    VisualAssert.WroteText(scope.Driver, "Refreshing");
                    VisualAssert.WroteText(scope.Driver, "[OK] Loading state");
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
            public bool Refreshed { get; set; }
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
