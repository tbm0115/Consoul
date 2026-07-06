using System;
using System.Linq;
using ConsoulLibrary.Color;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for shortcut bar view helpers.
    /// </summary>
    public class ShortcutBarRenderingTests
    {
        /// <summary>
        /// Shortcut bars wrap available action hints without overflowing.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(12)]
        [InlineData(24)]
        [InlineData(80)]
        public void ShortcutBar_ManyItems_DoNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.ShortcutBar(
                    new ShortcutBarOptions { Width = width },
                    new ShortcutItem("Enter", "Select current option"),
                    new ShortcutItem("Esc", "Go back"),
                    new ShortcutItem("F5", "Refresh preview"),
                    new ShortcutItem("?", "Help"));

                VisualAssert.WroteText(scope.Driver, "[Enter]");
                VisualAssert.WroteText(scope.Driver, "[Esc]");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Shortcut bars honor custom colors and disabled item rendering.
        /// </summary>
        [Fact]
        public void ShortcutBar_CustomOptions_ColorizeSegments()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var options = new ShortcutBarOptions
                {
                    Width = 60,
                    ItemSeparator = " | ",
                    KeyScheme = Scheme(ConsoleColor.Cyan),
                    DescriptionScheme = Scheme(ConsoleColor.White),
                    SeparatorScheme = Scheme(ConsoleColor.DarkGray),
                    DisabledScheme = Scheme(ConsoleColor.DarkGray)
                };

                Consoul.ShortcutBar(options,
                    new ShortcutItem("Enter", "Select"),
                    new ShortcutItem("F5", "Refresh", disabled: true));

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[Enter]") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Select") && write.Color == ConsoleColor.White);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("|") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[F5]") && write.Color == ConsoleColor.DarkGray);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Shortcut bars treat raw markup in keys and descriptions as styled visible text.
        /// </summary>
        /// <param name="width">The configured shortcut bar width.</param>
        [Theory]
        [InlineData(24)]
        [InlineData(40)]
        public void ShortcutBar_RawMarkupKeysAndDescriptions_DoNotLeakTagsOrOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                var options = new ShortcutBarOptions
                {
                    Width = width,
                    DisabledScheme = Scheme(ConsoleColor.DarkGray)
                };

                Consoul.ShortcutBar(options,
                    new ShortcutItem("[green]Enter[/]", "[white on navy]Select[/] [[prod]]"),
                    new ShortcutItem("[red]F5[/]", "[green]Refresh[/]", disabled: true));

                VisualAssert.ContainsText(scope.Driver, "[Enter]");
                VisualAssert.ContainsText(scope.Driver, "Select");
                VisualAssert.ContainsText(scope.Driver, "[prod]");
                VisualAssert.ContainsText(scope.Driver, "[F5]");
                Assert.DoesNotContain("[green]", scope.Driver.Text);
                Assert.DoesNotContain("[white on navy]", scope.Driver.Text);
                Assert.DoesNotContain("[red]", scope.Driver.Text);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Enter") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Select") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[prod]"));
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("F5") && write.Color == ConsoleColor.DarkGray);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= width, "Raw markup shortcut bar line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);

                string text = Consoul.ShortcutBarText(
                    new[] { new ShortcutItem("[green]Enter[/]", "[white on navy]Select[/] [[prod]]") },
                    width: width);
                Assert.Contains("[Enter]", text);
                Assert.Contains("Select", text);
                Assert.DoesNotContain("[green]", text);
                Assert.DoesNotContain("[white on navy]", text);
            }
        }

        /// <summary>
        /// Disabled shortcut items can be hidden for compact views.
        /// </summary>
        [Fact]
        public void ShortcutBar_HideDisabled_OmitsDisabledItems()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Consoul.ShortcutBar(
                    new ShortcutBarOptions { ShowDisabled = false },
                    new ShortcutItem("Enter", "Select"),
                    new ShortcutItem("F5", "Refresh", disabled: true));

                VisualAssert.WroteText(scope.Driver, "[Enter]");
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("F5"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Shortcut bar text generation creates bounded text without writing to the console.
        /// </summary>
        [Fact]
        public void ShortcutBarText_ReturnsBoundedPlainText()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                string text = Consoul.ShortcutBarText(
                    new[]
                    {
                        new ShortcutItem("Enter", "Select current option"),
                        new ShortcutItem("Esc", "Go back"),
                        new ShortcutItem("F5", "Refresh preview")
                    },
                    width: 18);

                Assert.All(text.Split(new[] { Environment.NewLine }, StringSplitOptions.None), line => Assert.True(line.Length <= 18, "Shortcut line exceeded requested width: " + line));
                Assert.Empty(scope.Driver.Writes);
            }
        }

        /// <summary>
        /// Shortcut bars can be built fluently and composed inside captured panels.
        /// </summary>
        [Fact]
        public void ShortcutBar_ComposesInsidePanel()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Consoul.Panel(
                    () => Consoul.ShortcutBar(bar =>
                    {
                        bar.Add("Enter", "Select");
                        bar.Add("Esc", "Back");
                    }, new ShortcutBarOptions { Width = 32 }),
                    title: "Keys",
                    width: 40);

                VisualAssert.WroteText(scope.Driver, "Keys");
                VisualAssert.WroteText(scope.Driver, "[Enter]");
                VisualAssert.WroteText(scope.Driver, "Back");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed dynamic views can render shortcut bars as view action hints.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderShortcutBarSections()
        {
            using (var scope = new ConsoleDriverScope(44))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var view = Consoul.View("Deploy", new ShortcutModel { CanRefresh = true })
                        .RenderModel(model =>
                        {
                            Consoul.ShortcutBar(bar =>
                            {
                                bar.Add("Enter", "Run selected action");
                                bar.Add("F5", "Refresh", disabled: !model.CanRefresh);
                                bar.Add("Esc", "Back");
                            }, new ShortcutBarOptions { Width = 40 });
                        })
                        .Option("Toggle refresh", model => model.CanRefresh = !model.CanRefresh);

                    view.Render();

                    VisualAssert.WroteText(scope.Driver, "[Enter]");
                    VisualAssert.WroteText(scope.Driver, "[F5]");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
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

        private sealed class ShortcutModel
        {
            public bool CanRefresh { get; set; }
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
