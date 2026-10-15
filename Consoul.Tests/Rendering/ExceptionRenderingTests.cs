using System;
using ConsoulLibrary.Color;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for structured exception output.
    /// </summary>
    public class ExceptionRenderingTests
    {
        /// <summary>
        /// Exception panels wrap long messages and stay inside the active terminal width.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(20)]
        [InlineData(32)]
        [InlineData(80)]
        public void WriteException_LongMessage_DoesNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                var exception = new InvalidOperationException("Deployment failed because verylongtokenwithoutnaturalbreaks could not be processed.");

                Consoul.WriteException(
                    exception,
                    "While rendering dashboard",
                    new ExceptionRenderOptions
                    {
                        Width = width,
                        IncludeStackTrace = false
                    });

                VisualAssert.WroteText(scope.Driver, "While");
                VisualAssert.WroteText(scope.Driver, "Message:");
                VisualAssert.WroteText(scope.Driver, "Deployment");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Exception panels can render inner exceptions and bounded stack trace details.
        /// </summary>
        [Fact]
        public void WriteException_InnerExceptionAndStackTrace_RenderInBounds()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                Exception exception = CreateNestedException();

                Consoul.WriteException(
                    exception,
                    new ExceptionRenderOptions
                    {
                        Width = 64,
                        IncludeStackTrace = true,
                        IncludeInnerExceptions = true,
                        MaxStackFrames = 1
                    });

                VisualAssert.WroteText(scope.Driver, "Type:");
                VisualAssert.WroteText(scope.Driver, "Stack trace:");
                VisualAssert.WroteText(scope.Driver, "Inner 1:");
                VisualAssert.WroteText(scope.Driver, "Outer failure");
                VisualAssert.WroteText(scope.Driver, "Inner failure");
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 64, "Exception line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Exception panels honor custom color schemes.
        /// </summary>
        [Fact]
        public void WriteException_CustomOptions_RenderColors()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var options = new ExceptionRenderOptions
                {
                    Title = "Failure",
                    Width = 48,
                    IncludeStackTrace = false,
                    BorderScheme = Scheme(ConsoleColor.Green),
                    TitleScheme = Scheme(ConsoleColor.Yellow),
                    ContextScheme = Scheme(ConsoleColor.Magenta),
                    TypeScheme = Scheme(ConsoleColor.Cyan),
                    MessageScheme = Scheme(ConsoleColor.Red)
                };

                Consoul.WriteException(new InvalidOperationException("Custom colors"), "Context", options);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Failure") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("|") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Context") && write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Type:") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Custom colors") && write.Color == ConsoleColor.Red);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Exception panels parse raw markup in human-authored title, context, and message text.
        /// </summary>
        [Fact]
        public void WriteException_MarkupTitleContextAndMessage_RenderVisibleTextAndColors()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var options = new ExceptionRenderOptions
                {
                    Title = "[yellow]Deploy failure[/]",
                    Width = 44,
                    IncludeStackTrace = false,
                    ContextScheme = Scheme(ConsoleColor.Magenta),
                    MessageScheme = Scheme(ConsoleColor.Blue)
                };

                Consoul.WriteException(
                    new InvalidOperationException("[green]Package rejected[/] for [[prod]]"),
                    "[cyan]Deploying[/] [[prod]]",
                    options);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy failure") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploying") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Package rejected") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(" [prod]") && write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(" for [prod]") && write.Color == ConsoleColor.Blue);
                Assert.DoesNotContain(scope.Driver.Writes, write =>
                    write.Text.Contains("[yellow]") ||
                    write.Text.Contains("[cyan]") ||
                    write.Text.Contains("[green]") ||
                    write.Text.Contains("[/]"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style exception format flags adapt to Consoul's structured renderer.
        /// </summary>
        [Fact]
        public void WriteException_ExceptionFormats_ShortenStackDetails()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var exception = new StackTraceException(
                    "Preview failed",
                    "   at Example.App.Services.DeploymentRunner.Execute() in C:\\repo\\src\\DeploymentRunner.cs:line 42");

                Consoul.WriteException(
                    exception,
                    ExceptionFormats.ShortenEverything | ExceptionFormats.ShowLinks);

                string output = string.Join("\n", scope.Driver.Lines);
                Assert.Contains("Type: StackTraceException", output);
                Assert.Contains("DeploymentRunner.Execute()", output);
                Assert.Contains("DeploymentRunner.cs:line 42", output);
                Assert.DoesNotContain("C:\\repo\\src", output);
                Assert.DoesNotContain("Example.App.Services", output);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style exception settings map to Consoul options and styles.
        /// </summary>
        [Fact]
        public void WriteException_ExceptionSettings_ApplyFormatAndStyle()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var exception = new StackTraceException(
                    "Settings failed",
                    "   at Example.App.Services.DeploymentRunner.Execute() in C:\\repo\\src\\DeploymentRunner.cs:line 42");

                AnsiConsole.WriteException(exception, new ExceptionSettings
                {
                    Format = ExceptionFormats.NoStackTrace | ExceptionFormats.ShortenTypes,
                    Style = new ExceptionStyle
                    {
                        Exception = new Style(ConsoleColor.Blue),
                        Message = new Style(ConsoleColor.Yellow)
                    }
                });

                string output = string.Join("\n", scope.Driver.Lines);
                Assert.Contains("Type: StackTraceException", output);
                Assert.DoesNotContain("Stack trace:", output);
                Assert.DoesNotContain("ConsoulLibrary.Tests.Rendering.ExceptionRenderingTests", output);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Type:") && write.Color == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Settings failed") && write.Color == ConsoleColor.Yellow);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Aggregate exceptions flatten child failures and can bound the number of rendered children.
        /// </summary>
        [Fact]
        public void WriteException_AggregateException_FlattensAndLimitsChildren()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                var exception = new AggregateException(
                    "Parallel deployment failed",
                    new InvalidOperationException("API failed"),
                    new ArgumentException("Worker failed"),
                    new AggregateException(
                        "Nested queue failures",
                        new TimeoutException("Queue timed out")));

                Consoul.WriteException(
                    exception,
                    new ExceptionRenderOptions
                    {
                        Width = 64,
                        IncludeStackTrace = false,
                        Format = ExceptionFormats.ShortenTypes,
                        MaxAggregateExceptions = 2
                    });

                string output = string.Join("\n", scope.Driver.Lines);

                Assert.Contains("Type: AggregateException", output);
                Assert.Contains("Message: Parallel deployment failed", output);
                Assert.Contains("Inner 1: InvalidOperationException", output);
                Assert.Contains("API failed", output);
                Assert.Contains("Inner 2: ArgumentException", output);
                Assert.Contains("Worker failed", output);
                Assert.Contains("Inner exceptions truncated: 1 not rendered", output);
                Assert.DoesNotContain("Queue timed out", output);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 64, "Aggregate exception line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style exception settings can opt out of aggregate flattening.
        /// </summary>
        [Fact]
        public void WriteException_ExceptionSettings_CanDisableAggregateFlattening()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                var exception = new AggregateException(
                    "Parallel deployment failed",
                    new InvalidOperationException("API failed"),
                    new ArgumentException("Worker failed"));

                AnsiConsole.WriteException(exception, new ExceptionSettings
                {
                    Format = ExceptionFormats.NoStackTrace | ExceptionFormats.ShortenTypes,
                    FlattenAggregateExceptions = false
                });

                string output = string.Join("\n", scope.Driver.Lines);

                Assert.Contains("Type: AggregateException", output);
                Assert.Contains("Message: Parallel deployment failed", output);
                Assert.Contains("Inner 1: InvalidOperationException", output);
                Assert.Contains("API failed", output);
                Assert.DoesNotContain("ArgumentException", output);
                Assert.DoesNotContain("Inner 2:", output);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed dynamic views can render structured exceptions as diagnostic sections.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderExceptionSections()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var view = Consoul.View("Dashboard", new DashboardModel())
                        .RenderModel(model =>
                        {
                            Consoul.WriteException(
                                new InvalidOperationException("Render preview failed"),
                                "Latest diagnostic",
                                new ExceptionRenderOptions
                                {
                                    Width = 42,
                                    IncludeStackTrace = false
                                });
                        })
                        .Option("Refresh", model => model.Refreshed = true);

                    view.Render();

                    VisualAssert.WroteText(scope.Driver, "Latest diagnostic");
                    VisualAssert.WroteText(scope.Driver, "Render preview");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        private static Exception CreateNestedException()
        {
            try
            {
                ThrowOuter();
            }
            catch (Exception exception)
            {
                return exception;
            }

            throw new InvalidOperationException("Expected nested exception was not thrown.");
        }

        private static void ThrowOuter()
        {
            try
            {
                ThrowInner();
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Outer failure", exception);
            }
        }

        private static void ThrowInner()
        {
            throw new ArgumentException("Inner failure");
        }

        private static ColorScheme Scheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = ConsoleColor.Black
            };
        }

        private sealed class StackTraceException : Exception
        {
            private readonly string _stackTrace;

            public StackTraceException(string message, string stackTrace) : base(message)
            {
                _stackTrace = stackTrace;
            }

            public override string StackTrace => _stackTrace;
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
