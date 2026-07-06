using System;
using ConsoulLibrary.Color;
using Xunit;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for intent-colored callout view helpers.
    /// </summary>
    public class CalloutRenderingTests
    {
        /// <summary>
        /// Callouts render common message kinds at multiple widths without overflowing.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        /// <param name="kind">The callout kind to render.</param>
        [Theory]
        [InlineData(18, CalloutKind.Info)]
        [InlineData(24, CalloutKind.Success)]
        [InlineData(32, CalloutKind.Warning)]
        [InlineData(80, CalloutKind.Error)]
        public void Callout_Kinds_DoNotOverflow(int width, CalloutKind kind)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Callout(
                    "This proof-of-concept view message includes a verylongtokenwithoutnaturalbreaks and should wrap safely.",
                    kind);

                VisualAssert.WroteText(scope.Driver, "This");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Callouts honor custom titles, widths, markers, and color schemes.
        /// </summary>
        [Fact]
        public void Callout_CustomOptions_RendersConfiguredTitleAndColors()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var options = new CalloutOptions
                {
                    Title = "Deploy",
                    Width = 28,
                    Padding = 2,
                    WarningMarker = "WARN",
                    BorderScheme = new ColorScheme
                    {
                        Color = ConsoleColor.DarkYellow,
                        BackgroundColor = ConsoleColor.Black
                    },
                    TitleScheme = new ColorScheme
                    {
                        Color = ConsoleColor.DarkYellow,
                        BackgroundColor = ConsoleColor.Black
                    },
                    ContentScheme = new ColorScheme
                    {
                        Color = ConsoleColor.Cyan,
                        BackgroundColor = ConsoleColor.Black
                    }
                };

                Consoul.Callout("Waiting for owner approval before continuing.", CalloutKind.Warning, options);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("WARN Deploy") && write.Color == ConsoleColor.DarkYellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Waiting") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 28, "Callout line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Callout color helpers accept shared palette and RGB colors.
        /// </summary>
        [Fact]
        public void Callout_ColorHelpers_AcceptSharedPaletteColors()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                AnsiConsole.Write(new Callout("Palette body message.", CalloutKind.Info)
                    .Width(40)
                    .Title("Palette")
                    .BorderColor(SpectreColor.DarkGray)
                    .TitleColor(RgbColor.Yellow)
                    .ContentColor(SpectreColor.Cyan));

                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("Palette") &&
                    write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("Palette body") &&
                    write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write =>
                    !string.IsNullOrWhiteSpace(write.Text) &&
                    write.Color == ConsoleColor.DarkGray);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Callouts can capture existing render helpers as content without overflowing.
        /// </summary>
        [Fact]
        public void Callout_RenderAction_CapturesStructuredContent()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Consoul.Callout(
                    () => Consoul.PropertyList(list =>
                    {
                        list.Add("Service", "Billing", ConsoleColor.Cyan);
                        list.Add("Status", "Degraded", ConsoleColor.Yellow);
                    }, new PropertyListOptions { LabelWidth = 8 }),
                    CalloutKind.Warning,
                    new CalloutOptions
                    {
                        Title = "Health",
                        Width = 42
                    });

                VisualAssert.WroteText(scope.Driver, "Health");
                VisualAssert.WroteText(scope.Driver, "Service");
                VisualAssert.WroteText(scope.Driver, "Degraded");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed dynamic views can render callout sections as part of their MVC-style output.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderCalloutSections()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var view = Consoul.View("Release", new ReleaseModel { Service = "Worker", IsReady = true })
                        .RenderModel(model =>
                        {
                            Consoul.Callout(
                                model.Service + " is ready for release.",
                                model.IsReady ? CalloutKind.Success : CalloutKind.Warning,
                                new CalloutOptions { Width = 36 });
                        })
                        .Option("Toggle readiness", model => model.IsReady = !model.IsReady);

                    view.Render();

                    VisualAssert.WroteText(scope.Driver, "Release");
                    VisualAssert.WroteText(scope.Driver, "Worker is ready");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        private sealed class ReleaseModel
        {
            public string Service { get; set; } = string.Empty;

            public bool IsReady { get; set; }
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
