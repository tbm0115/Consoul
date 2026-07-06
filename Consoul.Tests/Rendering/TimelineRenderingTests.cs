using System;
using ConsoulLibrary.Color;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for timeline view helpers.
    /// </summary>
    public class TimelineRenderingTests
    {
        /// <summary>
        /// Timelines wrap long titles and descriptions without overflowing.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(16)]
        [InlineData(32)]
        [InlineData(80)]
        public void Timeline_LongContent_DoesNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Consoul.Timeline(timeline =>
                {
                    timeline.Add(
                        "Restore dependencies with a verylongtokenwithoutnaturalbreaks",
                        TimelineItemState.Complete,
                        "Package cache restored and lock file validated.");
                    timeline.Add(
                        "Run smoke checks",
                        TimelineItemState.Active,
                        "Executing the narrow terminal validation path before publishing.");
                    timeline.Add("Deploy preview", TimelineItemState.Pending);
                }, new TimelineOptions { Width = width });

                VisualAssert.WroteText(scope.Driver, "Restore");
                VisualAssert.WroteText(scope.Driver, "Deploy");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Timelines honor custom markers, timestamps, and color schemes.
        /// </summary>
        [Fact]
        public void Timeline_CustomOptions_RendersConfiguredMarkersAndColors()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var options = new TimelineOptions
                {
                    Width = 36,
                    TimestampFormat = "HH:mm",
                    CompleteMarker = "done",
                    Connector = ":",
                    CompleteScheme = new ColorScheme
                    {
                        Color = ConsoleColor.Green,
                        BackgroundColor = ConsoleColor.Black
                    },
                    TitleScheme = new ColorScheme
                    {
                        Color = ConsoleColor.Cyan,
                        BackgroundColor = ConsoleColor.Black
                    }
                };

                Consoul.Timeline(new[]
                {
                    new TimelineItem(
                        "Build",
                        TimelineItemState.Complete,
                        "Artifacts ready.",
                        new DateTimeOffset(2026, 6, 23, 9, 30, 0, TimeSpan.Zero)),
                    new TimelineItem("Release", TimelineItemState.Pending)
                }, options);

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("done") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[09:30] Build") && write.Color == ConsoleColor.Cyan);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 36, "Timeline line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Timeline titles and descriptions treat raw markup as styled visible text.
        /// </summary>
        /// <param name="width">The configured timeline width.</param>
        [Theory]
        [InlineData(20)]
        [InlineData(36)]
        public void Timeline_RawMarkupTitlesAndDescriptions_WrapByVisibleTextAndDoNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width + 4))
            {
                Consoul.Timeline(new[]
                {
                    new TimelineItem(
                        "[green]Build artifacts[/]",
                        TimelineItemState.Complete,
                        "[cyan]Package[/] ready for [[prod]]",
                        new DateTimeOffset(2026, 6, 23, 9, 30, 0, TimeSpan.Zero)),
                    new TimelineItem(
                        "[yellow]ApprovalClusterWithVeryLongNameWithoutNaturalBreaks[/]",
                        TimelineItemState.Pending,
                        "[white on navy]Owner approval[/] required")
                }, new TimelineOptions
                {
                    Width = width,
                    TimestampFormat = "HH:mm",
                    TitleScheme = Scheme(ConsoleColor.White),
                    DescriptionScheme = Scheme(ConsoleColor.DarkGray)
                });

                VisualAssert.ContainsText(scope.Driver, "[09:30]");
                VisualAssert.ContainsText(scope.Driver, "Build");
                VisualAssert.ContainsText(scope.Driver, "artifacts");
                VisualAssert.ContainsText(scope.Driver, "Package");
                VisualAssert.ContainsText(scope.Driver, "[prod]");
                VisualAssert.ContainsText(scope.Driver, "Approval");
                VisualAssert.ContainsText(scope.Driver, "Owner approval");
                Assert.DoesNotContain("[green]", scope.Driver.Text);
                Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                Assert.DoesNotContain("[white on navy]", scope.Driver.Text);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("artifacts") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Package") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[prod]") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Approval") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Owner approval") && write.Color == ConsoleColor.White && write.BackgroundColor == ConsoleColor.DarkBlue);
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= width, "Raw markup timeline line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Timelines can be composed inside captured panels.
        /// </summary>
        [Fact]
        public void Timeline_ComposesInsidePanel()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Consoul.Panel(
                    () => Consoul.Timeline(timeline =>
                    {
                        timeline.Add("Queued", TimelineItemState.Complete);
                        timeline.Add("Running", TimelineItemState.Active);
                        timeline.Add("Publish", TimelineItemState.Pending);
                    }, new TimelineOptions { Width = 32 }),
                    title: "Workflow",
                    width: 40);

                VisualAssert.WroteText(scope.Driver, "Workflow");
                VisualAssert.WroteText(scope.Driver, "Queued");
                VisualAssert.WroteText(scope.Driver, "Publish");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Timelines can hide descriptions and connectors for compact summaries.
        /// </summary>
        [Fact]
        public void Timeline_CompactOptions_HideDescriptionsAndConnectors()
        {
            using (var scope = new ConsoleDriverScope(24))
            {
                Consoul.Timeline(timeline =>
                {
                    timeline.Add("Build", TimelineItemState.Complete, "Hidden description");
                    timeline.Add("Test", TimelineItemState.Warning, "Hidden description");
                }, new TimelineOptions
                {
                    ShowDescriptions = false,
                    ShowConnectors = false,
                    Width = 24
                });

                VisualAssert.WroteText(scope.Driver, "Build");
                VisualAssert.WroteText(scope.Driver, "Test");
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("Hidden"));
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("|"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Model-backed dynamic views can render timeline sections from model state.
        /// </summary>
        [Fact]
        public void DynamicModelView_CanRenderTimelineSections()
        {
            using (var scope = new ConsoleDriverScope(44))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var view = Consoul.View("Release", new ReleaseModel { Name = "Worker", Ready = true })
                        .RenderModel(model =>
                        {
                            Consoul.Timeline(timeline =>
                            {
                                timeline.Add("Build " + model.Name, TimelineItemState.Complete);
                                timeline.Add(model.Ready ? "Ready for deploy" : "Waiting", model.Ready ? TimelineItemState.Active : TimelineItemState.Warning);
                                timeline.Add("Publish", TimelineItemState.Pending);
                            }, new TimelineOptions { Width = 40 });
                        })
                        .Option("Toggle readiness", model => model.Ready = !model.Ready);

                    view.Render();

                    VisualAssert.WroteText(scope.Driver, "Build Worker");
                    VisualAssert.WroteText(scope.Driver, "Ready for deploy");
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
            public string Name { get; set; } = string.Empty;

            public bool Ready { get; set; }
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
