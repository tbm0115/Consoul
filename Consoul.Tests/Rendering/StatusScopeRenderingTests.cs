using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ConsoulLibrary.Tests.Rendering;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for status message scopes.
    /// </summary>
    public class StatusScopeRenderingTests
    {
        /// <summary>
        /// Status scopes update and complete in place without overflowing narrow terminals.
        /// </summary>
        [Fact]
        public void StatusScope_UpdateAndSucceed_DoesNotOverflow()
        {
            using (var scope = new ConsoleDriverScope(24))
            {
                using (StatusScope status = Consoul.Status("Starting", new StatusOptions { MaxWidth = 24 }))
                {
                    status.Update("Processing a very long status message");
                    status.Succeed("Done");
                }

                VisualAssert.WroteText(scope.Driver, "... Starting");
                VisualAssert.WroteText(scope.Driver, "Processing");
                VisualAssert.WroteText(scope.Driver, "[OK] Done");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Status action helpers automatically mark successful operations as complete.
        /// </summary>
        [Fact]
        public void Status_ActionHelper_MarksSuccess()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                Consoul.Status("Running", status =>
                {
                    status.Update("Working");
                });

                VisualAssert.WroteText(scope.Driver, "... Running");
                VisualAssert.WroteText(scope.Driver, "... Working");
                VisualAssert.WroteText(scope.Driver, "[OK] Working");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Status action helpers mark failed operations and rethrow the original exception.
        /// </summary>
        [Fact]
        public void Status_ActionHelper_MarksFailureAndRethrows()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                    Consoul.Status("Running", status =>
                    {
                        status.Update("About to fail");
                        throw new InvalidOperationException("Nope");
                    }));

                Assert.Equal("Nope", exception.Message);
                VisualAssert.WroteText(scope.Driver, "[FAIL] About to fail");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Styled status updates apply reusable foreground and background styles without changing legacy color overloads.
        /// </summary>
        [Fact]
        public void StatusScope_UpdateStyled_RendersReusableStyle()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                using (StatusScope status = Consoul.Status("Starting", new StatusOptions { MaxWidth = 40 }))
                {
                    status.UpdateStyled("Styled status", new Style(ConsoleColor.Cyan, ConsoleColor.DarkBlue));
                    status.StatusStyled("Styled fluent", new Style(ConsoleColor.Yellow, ConsoleColor.DarkGreen));
                }

                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("... Styled status") &&
                    write.Color == ConsoleColor.Cyan &&
                    write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("... Styled fluent") &&
                    write.Color == ConsoleColor.Yellow &&
                    write.BackgroundColor == ConsoleColor.DarkGreen);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Status message helpers accept direct Spectre-style and RGB palette values without adapter code.
        /// </summary>
        [Fact]
        public void StatusScope_StatusHelpers_AcceptDirectPaletteValues()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                using (StatusScope status = Consoul.Status("Palette", new StatusOptions { MaxWidth = 56 }))
                {
                    status.Update("Color update", SpectreColor.Aqua);
                    status.Status("RGB fluent", RgbColor.Yellow);
                    status.UpdateStyled("Color styled", SpectreColor.Green);
                    status.StatusStyled("RGB styled", RgbColor.Red);
                }

                using (StatusContext status = AnsiConsole.Status().StartContext("Context palette"))
                {
                    status.Status("Context color", SpectreColor.Magenta);
                    status.Status("Context rgb", RgbColor.Cyan);
                    status.Succeed("Context done");
                }

                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("... Color update") &&
                    write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("... RGB fluent") &&
                    write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("... Color styled") &&
                    write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("... RGB styled") &&
                    write.Color == ConsoleColor.Red);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("... Context color") &&
                    write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("... Context rgb") &&
                    write.Color == ConsoleColor.Cyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// RGB, parsed status styles, and spinner styles preserve true-color and decoration metadata on ANSI-capable terminals.
        /// </summary>
        [Fact]
        public void StatusScope_RgbAndSpinnerStylesEmitTrueColorSgrWhenSupported()
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
            using (StatusScope status = Consoul.Status("Initial", new StatusOptions
            {
                MaxWidth = 80,
                RefreshRate = 0
            }))
            {
                status.Update("RGB update", RgbColor.FromHex("#336699"));
                status.StatusStyled("Styled update", Style.Parse("italic #778899"));
                status.Spinner(Spinner.Known.Line)
                    .SpinnerStyle("bold #22c55e")
                    .Status("Spinner styled")
                    .Refresh();
            }

            string rendered = writer.ToString();
            Assert.Contains("\u001b[38;2;51;102;153m... RGB update", rendered);
            Assert.Contains("\u001b[3;38;2;119;136;153m... Styled update", rendered);
            Assert.Contains("\u001b[1;38;2;34;197;94m", rendered);
            Assert.Contains("Spinner styled", rendered);
        }

        /// <summary>
        /// Status builders keep parsed spinner-style metadata until the started scope renders it.
        /// </summary>
        [Fact]
        public void StatusBuilder_SpinnerStylePreservesTrueColorSgrWhenSupported()
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
                AnsiConsole.Status()
                    .Spinner(Spinner.Known.Line)
                    .SpinnerStyle("bold #22c55e")
                    .RefreshRate(0)
                    .Start("Builder initial", status =>
                    {
                        status.Status("Builder styled");
                        status.Refresh();
                    });
            }

            string rendered = writer.ToString();
            Assert.Contains("\u001b[1;38;2;34;197;94m", rendered);
            Assert.Contains("Builder styled", rendered);
        }

        /// <summary>
        /// Status messages parse raw markup in message text while keeping status prefixes literal.
        /// </summary>
        [Fact]
        public void StatusScope_MarkupMessages_RenderVisibleTextAndColors()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                using (StatusScope status = Consoul.Status("[green]Starting[/] [[prod]]", new StatusOptions
                {
                    MaxWidth = 40,
                    ActiveColor = ConsoleColor.Yellow,
                    SuccessColor = ConsoleColor.Blue
                }))
                {
                    status.Update("[cyan]Checking[/] [[prod]]");
                    status.Succeed("[green]Ready[/] [[prod]]");
                }

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Starting") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Checking") && write.Color == ConsoleColor.Cyan);
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
        /// Asynchronous status action helpers pass cancellation tokens and mark success.
        /// </summary>
        [Fact]
        public async Task StatusAsync_ActionHelper_MarksSuccess()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                bool sawToken = false;
                using (var cancellation = new CancellationTokenSource())
                {
                    await Consoul.StatusAsync("Running async", (status, token) =>
                    {
                        sawToken = token == cancellation.Token;
                        status.Update("Async work");
                        return Task.CompletedTask;
                    }, cancellationToken: cancellation.Token);
                }

                Assert.True(sawToken);
                VisualAssert.WroteText(scope.Driver, "[OK] Async work");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style status contexts support property-based status, spinner, and spinner-style mutation.
        /// </summary>
        [Fact]
        public void StatusContext_PropertyStyleMutation_RendersSpinnerStatusAndStyle()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                using (StatusContext status = AnsiConsole.Status()
                    .Spinner(Spinner.Known.Star)
                    .StartContext("Preparing"))
                {
                    Assert.Same(Spinner.Known.Star, status.Spinner);

                    var spinner = new Spinner("Test", "A", "B");
                    status.Spinner = spinner;
                    status.SpinnerStyle = new Style(ConsoleColor.Magenta);
                    status.Status = "Downloading package";
                    status.Refresh();
                    status.Refresh();

                    Assert.Same(spinner, status.Spinner);
                    Assert.Equal("Downloading package", status.Status);
                    Assert.Equal(ConsoleColor.Magenta, status.SpinnerStyle.Foreground);

                    status.Succeed("Package ready");
                }

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("* Preparing"));
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("A Downloading package") && write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("B Downloading package") && write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[OK] Package ready"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Status and spinner style helpers accept direct console, Spectre-style, and RGB color values.
        /// </summary>
        [Fact]
        public void StatusAndSpinner_SpinnerStyleHelpers_AcceptDirectPaletteValues()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                AnsiConsole.Status()
                    .Spinner(Spinner.Known.Line)
                    .RefreshRate(TimeSpan.FromMilliseconds(0))
                    .SpinnerStyle(ConsoleColor.Yellow)
                    .Start("Status builder", status =>
                    {
                        status.Status("Status builder direct color");
                        status.Refresh();
                    });

                using (StatusScope status = Consoul.Status("Status scope", new StatusOptions { AutoRefresh = false }))
                {
                    status.Spinner(Spinner.Known.Line)
                        .RefreshRate(-10)
                        .SpinnerStyle(RgbColor.Green)
                        .Status("Status scope rgb")
                        .Refresh();
                    status.Succeed("Status scope done");
                }

                using (StatusContext status = AnsiConsole.Status()
                    .Spinner(Spinner.Known.Line)
                    .StartContext("Status context"))
                {
                    status.SpinnerStyle(SpectreColor.Aqua)
                        .Status("Status context color")
                        .Refresh();
                    status.Succeed("Status context done");
                }

                AnsiConsole.Spinner()
                    .Spinner(Spinner.Known.Line)
                    .RefreshRate(TimeSpan.FromMilliseconds(0))
                    .SpinnerStyle(RgbColor.Yellow)
                    .Start("Spinner builder", spinner =>
                    {
                        spinner.Tick("Spinner builder rgb");
                    });

                using (SpinnerScope spinner = Consoul.Spinner("Spinner scope", new SpinnerOptions { AutoRefresh = false }))
                {
                    spinner.Spinner(Spinner.Known.Line)
                        .RefreshRate(-10)
                        .SpinnerStyle(SpectreColor.Red)
                        .Tick("Spinner scope color");
                    spinner.Refresh();
                    spinner.Succeed("Spinner scope done");
                }

                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("Status builder direct color") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("Status scope rgb") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("Status context color") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("Spinner builder rgb") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("Spinner scope color") && write.Color == ConsoleColor.Red);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Status and spinner frame helpers accept generated frame collections.
        /// </summary>
        [Fact]
        public void StatusAndSpinner_FrameHelpers_AcceptEnumerableFrames()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                IEnumerable<string> statusFrames = new List<string> { "S1", "S2" };
                IEnumerable<string> spinnerFrames = new List<string> { "P1", "P2" };

                AnsiConsole.Status()
                    .Frames(statusFrames)
                    .Start("Status builder", status =>
                    {
                        status.Status("Status builder generated");
                        status.Refresh();
                    });

                using (StatusScope status = Consoul.Status("Status scope", new StatusOptions { AutoRefresh = false }))
                {
                    status.Frames(statusFrames)
                        .Status("Status scope generated")
                        .Refresh();
                    status.Succeed("Status scope done");
                }

                AnsiConsole.Spinner()
                    .Frames(spinnerFrames)
                    .Start("Spinner builder", spinner =>
                    {
                        spinner.Tick("Spinner builder generated");
                    });

                using (SpinnerScope spinner = Consoul.Spinner("Spinner scope", new SpinnerOptions { AutoRefresh = false }))
                {
                    spinner.Frames(spinnerFrames);
                    spinner.Tick("Spinner scope generated");
                    spinner.Refresh();
                    spinner.Succeed("Spinner scope done");
                }

                VisualAssert.WroteText(scope.Driver, "S1 Status builder");
                VisualAssert.WroteText(scope.Driver, "S2 Status builder generated");
                VisualAssert.WroteText(scope.Driver, "S1 Status scope generated");
                VisualAssert.WroteText(scope.Driver, "P1 Spinner builder");
                VisualAssert.WroteText(scope.Driver, "P2 Spinner builder generated");
                VisualAssert.WroteText(scope.Driver, "P2 Spinner scope generated");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style status context builders preserve success lifecycle behavior for synchronous and asynchronous POC flows.
        /// </summary>
        [Fact]
        public async Task StatusBuilder_StartContext_AutoCompletesSyncAndAsyncOperations()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                int syncResult = AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .StartContext("Checking", status =>
                    {
                        status.Status = "Sync ready";
                        return 7;
                    });

                string asyncResult = await AnsiConsole.Status()
                    .StartContextAsync("Async checking", async status =>
                    {
                        status.Spinner = Spinner.Known.Line;
                        status.SpinnerStyle("cyan").Status("Async ready");
                        await Task.Delay(1);
                        return "done";
                    });

                Assert.Equal(7, syncResult);
                Assert.Equal("done", asyncResult);
                VisualAssert.WroteText(scope.Driver, "[OK] Sync ready");
                VisualAssert.WroteText(scope.Driver, "[OK] Async ready");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style status contexts can complete success and failure directly without accessing the underlying scope.
        /// </summary>
        [Fact]
        public void StatusContext_CompletionHelpers_RenderSuccessAndFailure()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                using (StatusContext status = AnsiConsole.Status().StartContext("Preparing"))
                {
                    status.Status = "Package ready";
                    status.Succeed();
                }

                using (StatusContext status = AnsiConsole.Status().StartContext("Publishing"))
                {
                    status.Fail("Publish failed");
                }

                VisualAssert.WroteText(scope.Driver, "[OK] Package ready");
                VisualAssert.WroteText(scope.Driver, "[FAIL] Publish failed");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }
    }
}
