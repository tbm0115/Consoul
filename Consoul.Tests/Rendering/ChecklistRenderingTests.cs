using System;
using ConsoulLibrary.Color;
using Xunit;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for checklist view helpers.
    /// </summary>
    public class ChecklistRenderingTests
    {
        /// <summary>
        /// Checklists wrap item text and stay inside the active terminal width.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(16)]
        [InlineData(32)]
        [InlineData(80)]
        public void Checklist_LongItems_DoNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Checklist(list =>
                {
                    list.Add("Restore packages", ChecklistItemState.Complete);
                    list.Add("Run responsive rendering tests with a very long descriptive label", ChecklistItemState.Warning);
                    list.Add("Publish preview package", ChecklistItemState.Pending, "Waiting for approval from the release owner.");
                }, new ChecklistOptions { Width = width });

                VisualAssert.WroteText(scope.Driver, "Restore");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[x]"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Checklist states use distinct default marker colors.
        /// </summary>
        [Fact]
        public void Checklist_States_RenderDefaultMarkerColors()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Consoul.Checklist(list =>
                {
                    list.Add("Done", ChecklistItemState.Complete);
                    list.Add("Pending", ChecklistItemState.Pending);
                    list.Add("Warning", ChecklistItemState.Warning);
                    list.Add("Error", ChecklistItemState.Error);
                    list.Add("Skipped", ChecklistItemState.Skipped);
                });

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[x]") && write.Color == RenderOptions.OptionColor);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[ ]") && write.Color == RenderOptions.SubnoteColor);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[!]") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[X]") && write.Color == RenderOptions.InvalidColor);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[-]") && write.Color == RenderOptions.SubnoteColor);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Checklist options and item-level schemes can override rendered colors and markers.
        /// </summary>
        [Fact]
        public void Checklist_CustomOptions_RenderCustomMarkersAndColors()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Consoul.Checklist(
                    new ChecklistOptions
                    {
                        CompleteMarker = "OK",
                        PendingMarker = "..",
                        LabelScheme = Scheme(ConsoleColor.Cyan),
                        CompleteScheme = Scheme(ConsoleColor.Green),
                        DescriptionScheme = Scheme(ConsoleColor.DarkGray)
                    },
                    ChecklistItem.Create("Build package", ChecklistItemState.Complete, "Package is ready."));

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("OK") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Package is ready") && write.Color == ConsoleColor.DarkGray);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Checklist fluent color helpers accept shared palette color values directly.
        /// </summary>
        [Fact]
        public void Checklist_ColorHelpers_AcceptSharedPaletteColors()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                new Checklist()
                    .Width(40)
                    .LabelColor(SpectreColor.Aqua)
                    .DescriptionColor(RgbColor.Yellow)
                    .AddItem("Build package", ChecklistItemState.Complete, "Package is ready.")
                    .Render();

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build package") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Package is ready.") && write.Color == ConsoleColor.Yellow);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Checklist labels and descriptions treat raw markup as styled visible text.
        /// </summary>
        /// <param name="width">The configured checklist width.</param>
        [Theory]
        [InlineData(20)]
        [InlineData(36)]
        public void Checklist_RawMarkupLabelsAndDescriptions_WrapByVisibleTextAndDoNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width + 4))
            {
                Consoul.Checklist(list =>
                {
                    list.Add(
                        "[green]Build artifacts[/]",
                        ChecklistItemState.Complete,
                        "[cyan]Package[/] ready for [[prod]]");
                    list.Add(
                        "[yellow]ApprovalClusterWithVeryLongNameWithoutNaturalBreaks[/]",
                        ChecklistItemState.Pending,
                        "[white on navy]Owner approval[/] required");
                }, new ChecklistOptions
                {
                    Width = width,
                    LabelScheme = Scheme(ConsoleColor.White),
                    DescriptionScheme = Scheme(ConsoleColor.DarkGray)
                });

                VisualAssert.ContainsText(scope.Driver, "Build artifacts");
                VisualAssert.ContainsText(scope.Driver, "Package");
                VisualAssert.ContainsText(scope.Driver, "[prod]");
                VisualAssert.ContainsText(scope.Driver, "Approval");
                VisualAssert.ContainsText(scope.Driver, "Owner approval");
                Assert.DoesNotContain("[green]", scope.Driver.Text);
                Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                Assert.DoesNotContain("[white on navy]", scope.Driver.Text);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build artifacts") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Package") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[prod]") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Approval") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Owner approval") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= width, "Raw markup checklist line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Checklists compose inside captured panels without overflowing.
        /// </summary>
        [Fact]
        public void Checklist_ComposesInsidePanel()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                Consoul.Panel(
                    () => Consoul.Checklist(list =>
                    {
                        list.Add("Build", ChecklistItemState.Complete);
                        list.Add("Test", ChecklistItemState.Complete);
                        list.Add("Deploy", ChecklistItemState.Pending);
                    }),
                    title: "Release",
                    width: 36);

                VisualAssert.WroteText(scope.Driver, "Release");
                VisualAssert.WroteText(scope.Driver, "Deploy");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed dynamic views can render checklist sections.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderChecklistSections()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var view = Consoul.View("Release", new ReleaseModel { TestsPassed = true, Approved = false })
                        .RenderModel(model =>
                        {
                            Consoul.Checklist(list =>
                            {
                                list.Add("Tests", model.TestsPassed ? ChecklistItemState.Complete : ChecklistItemState.Error);
                                list.Add("Approval", model.Approved ? ChecklistItemState.Complete : ChecklistItemState.Pending);
                            }, new ChecklistOptions { Width = 32 });
                        })
                        .Option("Approve", model => model.Approved = true);

                    view.Render();

                    VisualAssert.WroteText(scope.Driver, "Tests");
                    VisualAssert.WroteText(scope.Driver, "Approval");
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

        private sealed class ReleaseModel
        {
            public bool TestsPassed { get; set; }

            public bool Approved { get; set; }
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
