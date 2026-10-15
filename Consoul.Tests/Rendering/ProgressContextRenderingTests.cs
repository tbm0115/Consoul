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
    /// Responsive rendering tests for multi-task progress contexts.
    /// </summary>
    public class ProgressContextRenderingTests
    {
        /// <summary>
        /// Progress contexts render multiple task rows within narrow and normal terminal widths.
        /// </summary>
        /// <param name="width">The fake terminal width.</param>
        [Theory]
        [InlineData(18)]
        [InlineData(40)]
        [InlineData(80)]
        public void ProgressContext_MultipleTasks_DoNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                using (ProgressContext progress = Consoul.Progress(new ProgressContextOptions { Width = width, LabelWidth = 12 }))
                {
                    ProgressTask build = progress.AddTask("Build project", 100);
                    ProgressTask test = progress.AddTask("Run tests", 10);

                    build.Update(50);
                    test.Increment(5);
                    build.Complete("Build done");
                    test.Complete("Tests done");
                }

                VisualAssert.WroteText(scope.Driver, "Build");
                VisualAssert.WroteText(scope.Driver, "Tests");
                if (width >= 40)
                {
                    VisualAssert.WroteText(scope.Driver, "100%");
                }

                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Completed progress rows use the configured completion color.
        /// </summary>
        [Fact]
        public void ProgressTask_Complete_UsesCompletionColor()
        {
            using (var scope = new ConsoleDriverScope(50))
            {
                var options = new ProgressContextOptions
                {
                    CompleteColor = ConsoleColor.Green,
                    ActiveColor = ConsoleColor.Yellow
                };

                using (ProgressContext progress = Consoul.Progress(options))
                {
                    ProgressTask task = progress.AddTask("Package", 4);
                    task.Update(2);
                    task.Complete();
                }

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("50%") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("100%") && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Failed progress rows use the configured failure color and update context state counts.
        /// </summary>
        [Fact]
        public void ProgressTask_Fail_UsesFailureColorAndUpdatesContextState()
        {
            using (var scope = new ConsoleDriverScope(60))
            {
                var options = new ProgressContextOptions
                {
                    ActiveColor = ConsoleColor.Yellow,
                    CompleteColor = ConsoleColor.Green,
                    FailedColor = ConsoleColor.Magenta
                };

                using (ProgressContext progress = Consoul.Progress(options))
                {
                    ProgressTask build = progress.AddTask("Build", 2);
                    ProgressTask deploy = progress.AddTask("Deploy", 2);

                    build.Complete("Build done");
                    deploy.Update(1);
                    deploy.Fail("Deploy failed");

                    Assert.Equal(ProgressTaskState.Completed, build.State);
                    Assert.Equal(ProgressTaskState.Failed, deploy.State);
                    Assert.True(build.IsCompleted);
                    Assert.True(deploy.IsFailed);
                    Assert.True(deploy.IsFinished);
                    Assert.Equal(1, progress.CompletedTaskCount);
                    Assert.Equal(1, progress.FailedTaskCount);
                    Assert.True(progress.HasFailures);
                    Assert.True(progress.IsFinished);
                    Assert.False(progress.IsCompleted);
                }

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build done") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy failed") && write.Color == ConsoleColor.Magenta);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Progress row palette helpers preserve RGB and decoration metadata for ANSI true-color terminals.
        /// </summary>
        [Fact]
        public void ProgressContext_RowPaletteHelpersEmitTrueColorSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 16, isTerminal: true),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });

            console.Progress(new ProgressContextOptions { Width = 56, LabelWidth = 16 })
                .ActiveStyle("bold #112233")
                .CompleteColor(RgbColor.FromHex("#22c55e"))
                .FailureColor(SpectreColor.FromHex("#ef4444"))
                .Start(progress =>
                {
                    ProgressTask build = progress.AddTask("Build package", 2);
                    build.Update(1);
                    build.Complete("Build ready");

                    ProgressTask deploy = progress.AddTask("Deploy package", 1);
                    deploy.Fail("Deploy failed");
                });

            string output = writer.ToString();
            Assert.Contains("\u001b[1;38;2;17;34;51mBuild package", output);
            Assert.Contains("\u001b[38;2;34;197;94mBuild ready", output);
            Assert.Contains("\u001b[38;2;239;68;68mDeploy failed", output);
        }

        /// <summary>
        /// Column-mode progress rows inherit row styles while explicit column styles can override them.
        /// </summary>
        [Fact]
        public void ProgressContext_ColumnRowsInheritRowPaletteUntilColumnOverrides()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 16, isTerminal: true),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });

            console.Progress(new ProgressContextOptions { Width = 64, LabelWidth = 18 })
                .ActiveColor(RgbColor.FromHex("#445566"))
                .Columns(
                    new TaskDescriptionColumn(),
                    new TextColumn("{task.Percentage}") { Width = 4 },
                    new TextColumn("override").TextColor(RgbColor.FromHex("#abcdef")))
                .Start(progress =>
                {
                    ProgressTask task = progress.AddTask("Column package", 4);
                    task.Update(2);
                });

            string output = writer.ToString();
            Assert.Contains("\u001b[38;2;68;85;102mColumn package", output);
            Assert.Contains("\u001b[38;2;68;85;102m50%", output);
            Assert.Contains("\u001b[38;2;171;205;239moverride", output);
        }

        /// <summary>
        /// Aggregate completion remains false for empty, active, and failed contexts, and true only when every task succeeds.
        /// </summary>
        [Fact]
        public void ProgressContext_IsCompletedTracksSuccessfulAggregateState()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                using (ProgressContext progress = Consoul.Progress(new ProgressContextOptions { Width = 60, LabelWidth = 16 }))
                {
                    Assert.False(progress.IsFinished);
                    Assert.False(progress.IsCompleted);

                    ProgressTask build = progress.AddTask("Build", 2);
                    ProgressTask deploy = progress.AddTask("Deploy", 2);

                    Assert.False(progress.IsFinished);
                    Assert.False(progress.IsCompleted);

                    build.Complete("Build done");

                    Assert.False(progress.IsFinished);
                    Assert.False(progress.IsCompleted);

                    deploy.Fail("Deploy failed");

                    Assert.True(progress.IsFinished);
                    Assert.False(progress.IsCompleted);
                }

                using (ProgressContext progress = Consoul.Progress(new ProgressContextOptions { Width = 60, LabelWidth = 16 }))
                {
                    ProgressTask compile = progress.AddTask("Compile", 1);
                    ProgressTask package = progress.AddTask("Package", 1);

                    compile.Complete("Compile done");
                    package.Complete("Package done");

                    Assert.True(progress.IsFinished);
                    Assert.True(progress.IsCompleted);
                }

                VisualAssert.WroteText(scope.Driver, "Build done");
                VisualAssert.WroteText(scope.Driver, "Deploy failed");
                VisualAssert.WroteText(scope.Driver, "Compile done");
                VisualAssert.WroteText(scope.Driver, "Package done");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style loop snippets can stop on the aggregate finished state without inspecting each task.
        /// </summary>
        [Fact]
        public void ProgressContext_IsFinishedSupportsLoopStyleSnippets()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                AnsiConsole.Progress(new ProgressContextOptions { Width = 60, LabelWidth = 20 }).Start(progress =>
                {
                    ProgressTask task = progress.AddTask("Loop upload", 3);
                    int iterations = 0;

                    while (!progress.IsFinished)
                    {
                        task.Increment(1);
                        iterations++;
                        Assert.True(iterations <= 3, "Progress loop should finish once the task reaches its maximum value.");
                    }

                    Assert.Equal(3, iterations);
                    Assert.True(progress.IsCompleted);
                });

                VisualAssert.WroteText(scope.Driver, "Loop upload");
                VisualAssert.WroteText(scope.Driver, "100%");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Failed progress rows can be marked active again for retry-style workflows.
        /// </summary>
        [Fact]
        public void ProgressTask_Start_ReactivatesFailedTask()
        {
            using (var scope = new ConsoleDriverScope(60))
            {
                using (ProgressContext progress = Consoul.Progress(new ProgressContextOptions { FailedColor = ConsoleColor.Red }))
                {
                    ProgressTask task = progress.AddTask("Package", 3);

                    task.Update(1);
                    task.Fail("Package failed");
                    task.Start("Retry package");
                    task.Increment(1);

                    Assert.Equal(ProgressTaskState.Active, task.State);
                    Assert.False(task.IsFailed);
                    Assert.False(task.IsFinished);
                    Assert.Equal(0, progress.FailedTaskCount);

                    task.Complete("Package ready");

                    Assert.True(task.IsCompleted);
                    Assert.True(progress.IsFinished);
                    Assert.True(progress.IsCompleted);
                }

                VisualAssert.WroteText(scope.Driver, "Retry package");
                VisualAssert.WroteText(scope.Driver, "Package ready");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style auto-start overloads and task aliases map to Consoul task lifecycle behavior.
        /// </summary>
        [Fact]
        public void ProgressContext_AddTask_WithAutoStartOverload_AndTaskAliases_Work()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                using (ProgressContext progress = AnsiConsole.Progress(new ProgressContextOptions { Width = 60, LabelWidth = 18 }))
                {
                    ProgressTask pending = progress.AddTask("Download package", autoStart: false, maxValue: 2048);
                    ProgressTask active = progress.AddTask("Publish package", autoStart: true, maxValue: 4);

                    Assert.Equal(ProgressTaskState.Pending, pending.State);
                    Assert.False(pending.IsStarted);
                    Assert.Equal(2048, pending.MaxValue);
                    Assert.Equal(ProgressTaskState.Active, active.State);
                    Assert.True(active.IsStarted);
                    Assert.Equal(4, active.MaxValue);

                    pending.StartTask();
                    pending.Update(1024);

                    Assert.Equal(ProgressTaskState.Active, pending.State);
                    Assert.True(pending.IsStarted);

                    pending.StopTask();
                    active.StopTask();

                    Assert.True(pending.IsCompleted);
                    Assert.True(active.IsCompleted);
                    Assert.True(progress.IsFinished);
                    Assert.True(progress.IsCompleted);
                }

                VisualAssert.WroteText(scope.Driver, "Download package");
                VisualAssert.WroteText(scope.Driver, "Publish package");
                VisualAssert.WroteText(scope.Driver, "50%");
                VisualAssert.WroteText(scope.Driver, "100%");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style progress task settings configure max value, pending state, and indeterminate rendering.
        /// </summary>
        [Fact]
        public void ProgressContext_AddTask_WithTaskSettings_ConfiguresLifecycle()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                using (ProgressContext progress = AnsiConsole.Progress(new ProgressContextOptions { Width = 60, LabelWidth = 18 }))
                {
                    ProgressTask task = progress.AddTask(
                        "Download package",
                        new ProgressTaskSettings
                        {
                            AutoStart = false,
                            MaxValue = 2048,
                            IsIndeterminate = true
                        });

                    Assert.Equal(ProgressTaskState.Pending, task.State);
                    Assert.False(task.IsStarted);
                    Assert.True(task.IsIndeterminateValue);
                    Assert.Equal(2048, task.MaxValue);

                    task.Start();
                    task.IsIndeterminate(false);
                    task.Update(1024);

                    Assert.Equal(ProgressTaskState.Active, task.State);
                    Assert.True(task.IsStarted);
                    Assert.False(task.IsIndeterminateValue);

                    task.Complete("Package ready");

                    Assert.Equal(ProgressTaskState.Completed, task.State);
                    Assert.True(task.IsCompleted);
                }

                VisualAssert.WroteText(scope.Driver, "Download package");
                VisualAssert.WroteText(scope.Driver, "50%");
                VisualAssert.WroteText(scope.Driver, "Package ready");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Batch helpers add model-backed progress task rows from descriptions and prepared task settings.
        /// </summary>
        [Fact]
        public void ProgressContext_AddTasks_CreatesDescriptionAndSettingsBatches()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                using (ProgressContext progress = AnsiConsole.Progress(new ProgressContextOptions { Width = 64, LabelWidth = 20 })
                    .AutoRefresh(false))
                {
                    IReadOnlyList<ProgressTask> descriptionTasks = progress.AddTasks("Restore packages", "Build app");
                    IEnumerable<string> queuedDescriptions = new[] { "Package app" };
                    IReadOnlyList<ProgressTask> queuedTasks = progress.AddTasks(
                        queuedDescriptions,
                        autoStart: false,
                        maxValue: 3);
                    var configuredTaskSettings = new[]
                    {
                        new ProgressTaskSettings
                        {
                            Description = "Publish app",
                            MaxValue = 2,
                            Tag = "release",
                            HideWhenCompleted = true
                        },
                        new ProgressTaskSettings
                        {
                            Description = "Smoke tests",
                            AutoStart = false,
                            MaxValue = 4,
                            IsIndeterminate = true,
                            Tag = "qa"
                        }
                    };
                    IReadOnlyList<ProgressTask> configuredTasks = progress.AddTasks(configuredTaskSettings);

                    Assert.Equal(new[] { descriptionTasks[0], descriptionTasks[1], queuedTasks[0], configuredTasks[0], configuredTasks[1] }, progress.Tasks);
                    Assert.All(descriptionTasks, task => Assert.Equal(ProgressTaskState.Active, task.State));
                    Assert.Equal(ProgressTaskState.Pending, queuedTasks[0].State);
                    Assert.Equal(3, queuedTasks[0].MaxValue);
                    Assert.Equal("release", configuredTasks[0].Tag);
                    Assert.True(configuredTasks[0].HideWhenCompleted.HasValue);
                    Assert.True(configuredTasks[0].HideWhenCompleted.GetValueOrDefault());
                    Assert.Equal(2, configuredTasks[0].MaxValue);
                    Assert.Equal("qa", configuredTasks[1].Tag);
                    Assert.Equal(ProgressTaskState.Pending, configuredTasks[1].State);
                    Assert.True(configuredTasks[1].IsIndeterminateValue);

                    descriptionTasks[0].Complete("Restore ready");
                    descriptionTasks[1].Complete("Build ready");
                    queuedTasks[0].StartTask();
                    queuedTasks[0].Complete("Package ready");
                    configuredTasks[0].Complete("Publish hidden");
                    configuredTasks[1].StartTask();
                    configuredTasks[1].IsIndeterminate(false);
                    configuredTasks[1].Complete("Smoke ready");
                    progress.Refresh();

                    Assert.True(progress.IsFinished);
                    Assert.Equal(5, progress.CompletedTaskCount);
                }

                string screen = string.Join(Environment.NewLine, scope.Driver.Lines);

                VisualAssert.WroteText(scope.Driver, "Restore packages");
                VisualAssert.WroteText(scope.Driver, "Build app");
                VisualAssert.WroteText(scope.Driver, "Package app");
                VisualAssert.WroteText(scope.Driver, "Publish app");
                VisualAssert.WroteText(scope.Driver, "Smoke tests");
                Assert.DoesNotContain("Publish hidden", screen);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style task metadata exposes identifiers, tags, timing, percentage, and estimated runtime values.
        /// </summary>
        [Fact]
        public void ProgressTask_SpectreStyleMetadata_TracksTimingPercentageAndTag()
        {
            using (new ConsoleDriverScope(64))
            {
                using (ProgressContext progress = AnsiConsole.Progress(new ProgressContextOptions { Width = 60, LabelWidth = 18 })
                    .AutoRefresh(false))
                {
                    ProgressTask task = progress.AddTask("Download package", autoStart: false, maxValue: 200);

                    Assert.True(task.Id > 0);
                    Assert.Null(task.StartTime);
                    Assert.Null(task.StopTime);
                    Assert.Null(task.ElapsedTime);
                    Assert.Equal(0, task.Percentage);

                    task.Tag = "artifact";
                    task.MaxTimeForSpeedCache = TimeSpan.FromSeconds(2);
                    task.MaxSamplingAge = TimeSpan.FromSeconds(10);
                    task.MaxSamplesKept = 4;

                    task.StartTask();
                    Thread.Sleep(10);
                    task.Value(50);

                    Assert.Equal("artifact", task.Tag);
                    Assert.True(task.StartTime.HasValue);
                    Assert.Null(task.StopTime);
                    Assert.InRange(task.Percentage, 24.9, 25.1);
                    Assert.True(task.ElapsedTime.HasValue);
                    Assert.True(task.Speed.GetValueOrDefault() > 0);
                    Assert.True(task.RemainingTime.HasValue);
                    Assert.Equal(TimeSpan.FromSeconds(2), task.MaxTimeForSpeedCache);
                    Assert.Equal(TimeSpan.FromSeconds(10), task.MaxSamplingAge);
                    Assert.Equal(4, task.MaxSamplesKept);

                    task.Complete("Download ready");

                    Assert.Equal(100, task.Percentage);
                    Assert.True(task.StopTime.HasValue);
                    Assert.Equal(TimeSpan.Zero, task.RemainingTime);
                    Assert.True(task.ElapsedTime.HasValue);
                    Assert.True(task.IsFinished);
                }
            }
        }

        /// <summary>
        /// Per-task hide settings override the context default for completed progress rows.
        /// </summary>
        [Fact]
        public void ProgressTask_HideWhenCompleted_OverridesContextDefault()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                using (ProgressContext progress = AnsiConsole.Progress(new ProgressContextOptions { Width = 64, LabelWidth = 20 })
                    .AutoRefresh(false)
                    .HideCompleted(false))
                {
                    ProgressTask hidden = progress.AddTask("Hidden package", 1);
                    ProgressTask visible = progress.AddTask("Visible package", 1);

                    hidden.HideWhenCompleted = true;
                    visible.HideWhenCompleted = false;

                    hidden.Complete("Hidden complete");
                    visible.Complete("Visible complete");
                    progress.Refresh();
                }

                string screen = string.Join(Environment.NewLine, scope.Driver.Lines);

                Assert.DoesNotContain("Hidden package", screen);
                Assert.DoesNotContain("Hidden complete", screen);
                Assert.Contains("Visible complete", screen);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style ordered task insertion keeps task ordering and rendered rows bounded.
        /// </summary>
        [Fact]
        public void ProgressContext_AddTaskAtBeforeAfterAndRemoveTask_PreservesOrder()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                using (ProgressContext progress = AnsiConsole.Progress(new ProgressContextOptions { Width = 64, LabelWidth = 18 })
                    .AutoRefresh(false))
                {
                    ProgressTask build = progress.AddTask("Build", autoStart: false, maxValue: 4);
                    ProgressTask deploy = progress.AddTaskAfter("Deploy", build, autoStart: false, maxValue: 1);
                    ProgressTask restore = progress.AddTaskBefore("Restore", build, autoStart: false, maxValue: 2);
                    ProgressTask test = progress.AddTaskAt("Test", 2, autoStart: false, maxValue: 3);

                    Assert.Equal(new[] { restore, build, test, deploy }, progress.Tasks);
                    Assert.All(progress.Tasks, task => Assert.Equal(ProgressTaskState.Pending, task.State));

                    restore.StartTask();
                    build.StartTask();
                    test.StartTask();
                    deploy.StartTask();
                    progress.Refresh();

                    Assert.True(progress.RemoveTask(build));
                    Assert.False(progress.RemoveTask(build));
                    Assert.Equal(new[] { restore, test, deploy }, progress.Tasks);
                    Assert.Throws<ObjectDisposedException>(() => build.Update(1));

                    progress.Refresh();
                }

                VisualAssert.WroteText(scope.Driver, "Restore");
                VisualAssert.WroteText(scope.Driver, "Build");
                VisualAssert.WroteText(scope.Driver, "Test");
                VisualAssert.WroteText(scope.Driver, "Deploy");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Ordered insertion overloads accept task settings without starting or determinate-rendering the row.
        /// </summary>
        [Fact]
        public void ProgressContext_OrderedTaskInsertion_WithTaskSettings_ConfiguresLifecycle()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                using (ProgressContext progress = AnsiConsole.Progress(new ProgressContextOptions { Width = 64, LabelWidth = 20 }))
                {
                    ProgressTask publish = progress.AddTask("Publish", 5);
                    ProgressTask restore = progress.AddTaskAt(
                        "Restore packages",
                        new ProgressTaskSettings
                        {
                            AutoStart = false,
                            MaxValue = 2048,
                            IsIndeterminate = true
                        },
                        0);
                    ProgressTask test = progress.AddTaskAfter(
                        "Run tests",
                        new ProgressTaskSettings { AutoStart = false, MaxValue = 10 },
                        restore);
                    ProgressTask pack = progress.AddTaskBefore(
                        "Pack artifacts",
                        new ProgressTaskSettings { AutoStart = false, MaxValue = 3 },
                        publish);

                    Assert.Equal(new[] { restore, test, pack, publish }, progress.Tasks);
                    Assert.Equal(ProgressTaskState.Pending, restore.State);
                    Assert.Equal(2048, restore.MaxValue);
                    Assert.True(restore.IsIndeterminateValue);
                    Assert.Equal(ProgressTaskState.Pending, test.State);
                    Assert.Equal(10, test.MaxValue);
                    Assert.Equal(ProgressTaskState.Pending, pack.State);
                    Assert.Equal(3, pack.MaxValue);
                    Assert.Equal(ProgressTaskState.Active, publish.State);
                }

                VisualAssert.WroteText(scope.Driver, "Restore packages");
                VisualAssert.WroteText(scope.Driver, "Run tests");
                VisualAssert.WroteText(scope.Driver, "Pack artifacts");
                VisualAssert.WroteText(scope.Driver, "Publish");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Progress action helpers dispose owned task rows after the action completes.
        /// </summary>
        [Fact]
        public void Progress_ActionHelper_DisposesTasks()
        {
            using (new ConsoleDriverScope(50))
            {
                ProgressTask captured = null!;

                Consoul.Progress(progress =>
                {
                    captured = progress.AddTask("Build", 1);
                    captured.Complete();
                });

                Assert.NotNull(captured);
                Assert.Throws<ObjectDisposedException>(() => captured.Update(0));
            }
        }

        /// <summary>
        /// Asynchronous progress helpers pass cancellation tokens to the action.
        /// </summary>
        [Fact]
        public async Task ProgressAsync_ActionHelper_PassesCancellationToken()
        {
            using (var scope = new ConsoleDriverScope(50))
            {
                bool sawToken = false;
                using (var cancellation = new CancellationTokenSource())
                {
                    await Consoul.ProgressAsync((progress, token) =>
                    {
                        sawToken = token == cancellation.Token;
                        ProgressTask task = progress.AddTask("Async", 2);
                        task.Update(1);
                        task.Complete();
                        return Task.CompletedTask;
                    }, cancellationToken: cancellation.Token);
                }

                Assert.True(sawToken);
                VisualAssert.WroteText(scope.Driver, "Async");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style progress task properties and context configuration helpers render through the existing task rows.
        /// </summary>
        [Fact]
        public async Task ProgressContext_SpectreStyleTaskPropertiesAndConfigurationHelpers_Work()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                int processed = AnsiConsole.Progress(new ProgressContextOptions { Width = 64, LabelWidth = 24 })
                    .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn())
                    .RefreshRate(TimeSpan.FromMilliseconds(0))
                    .AutoRefresh(false)
                    .HideCompleted(true)
                    .ExcludeVerticalPadding()
                    .UseRenderHook((renderable, tasks) => renderable)
                    .Start(progress =>
                    {
                        ProgressTask task = progress.AddTask("Processing records");

                        Assert.Equal(100, task.MaxValue);

                        task.IsIndeterminate();
                        Assert.True(task.IsIndeterminateValue);
                        task.Description = "Resolving total";
                        progress.Refresh();

                        task.IsIndeterminate(false);
                        task.MaxValue = 10;
                        task.Value = 5;
                        task.Description = "Processing files";
                        progress.Refresh();

                        task.Value = 10;
                        Assert.True(task.IsFinished);
                        progress.Refresh();
                        return 10;
                    });

                string asyncResult = await AnsiConsole.Progress(new ProgressContextOptions { Width = 64, LabelWidth = 24 })
                    .AutoClear(true)
                    .StartAsync(async progress =>
                    {
                        ProgressTask task = progress.AddTask("Async records");
                        task.MaxValue = 2;
                        task.Value = 2;
                        await Task.Delay(1);
                        return "done";
                    });

                Assert.Equal(10, processed);
                Assert.Equal("done", asyncResult);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Resolving total"));
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Processing files") && write.Text.Contains("50%"));
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Async records") && write.Text.Contains("100%"));
                Assert.Contains(scope.Driver.Writes, write => string.IsNullOrWhiteSpace(write.Text));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Progress context refresh-rate helpers update options and existing task rows without requiring options-object setup.
        /// </summary>
        [Fact]
        public void ProgressContext_RefreshRateHelpers_UpdateOptionsAndExistingTasks()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                using (ProgressContext progress = AnsiConsole.Progress(new ProgressContextOptions { Width = 56, LabelWidth = 18 })
                    .AutoRefresh(false)
                    .RefreshRate(TimeSpan.FromMilliseconds(2.5)))
                {
                    Assert.Equal(3, progress.Options.RefreshRate);

                    ProgressTask task = progress.AddTask("Refresh tuned", maxValue: 2);
                    progress.RefreshRate(-5);
                    Assert.Equal(0, progress.Options.RefreshRate);

                    task.Update(1);
                    progress.Refresh();
                    task.Complete("Refresh done");
                    progress.Refresh();
                }

                VisualAssert.WroteText(scope.Driver, "Refresh tuned");
                VisualAssert.WroteText(scope.Driver, "Refresh done");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// No-argument toggles and append-only column helpers make quick progress views readable without replacing prior configuration.
        /// </summary>
        [Fact]
        public void ProgressContext_TogglesAndAddColumns_UpdateOptionsAndRenderBoundedRows()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                using (ProgressContext progress = AnsiConsole.Progress(new ProgressContextOptions { Width = 64, LabelWidth = 18 })
                    .Columns(new TaskDescriptionColumn())
                    .AddColumn(new TextColumn("[grey]{task.Tag}[/]"))
                    .AddColumns(new ProgressColumn[] { null!, new PercentageColumn() })
                    .AutoRefresh()
                    .ManualRefresh()
                    .AutoClear()
                    .HideCompleted()
                    .ShowCompleted())
                {
                    IReadOnlyList<ProgressColumn> columns = progress.Options.GetColumns();

                    Assert.False(progress.Options.AutoRefresh);
                    Assert.True(progress.Options.AutoClear);
                    Assert.False(progress.Options.HideCompleted);
                    Assert.Collection(
                        columns,
                        column => Assert.IsType<TaskDescriptionColumn>(column),
                        column => Assert.IsType<TextColumn>(column),
                        column => Assert.IsType<PercentageColumn>(column));

                    ProgressTask task = progress.AddTask("Package artifacts", maxValue: 4)
                        .Tag("release");

                    task.Update(2);
                    progress.Refresh();
                    task.Complete("Package ready");
                    progress.Refresh();
                }

                VisualAssert.WroteText(scope.Driver, "Package artifacts");
                VisualAssert.WroteText(scope.Driver, "release");
                VisualAssert.WroteText(scope.Driver, "50%");
                VisualAssert.WroteText(scope.Driver, "100%");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Progress render hooks can replace generated rows while the fixed row renderer keeps output bounded.
        /// </summary>
        [Fact]
        public void ProgressContext_RenderHook_ReplacesGeneratedRowWithinBounds()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                int calls = 0;
                int taskCount = 0;
                string generatedSnapshot = string.Empty;

                using (ProgressContext progress = AnsiConsole.Progress(new ProgressContextOptions { Width = 36, LabelWidth = 12 })
                    .AutoRefresh(false)
                    .UseRenderHook((renderable, tasks) =>
                    {
                        calls++;
                        taskCount = tasks.Count;
                        generatedSnapshot = Consoul.Capture(renderable, width: 37, height: 20).TrimEnd();
                        return new Markup("[cyan]Hooked progress row that is intentionally long[/]\nSecond line is ignored");
                    }))
                {
                    ProgressTask task = progress.AddTask("Original task", 4);
                    task.Update(2);
                    progress.Refresh();

                    string formatted = progress.FormatRow(task);

                    Assert.True(formatted.Length <= 36);
                    Assert.StartsWith("Hooked progress row", formatted.TrimEnd(), StringComparison.Ordinal);
                    Assert.DoesNotContain("Second line", formatted);
                }

                Assert.True(calls >= 2);
                Assert.Equal(1, taskCount);
                Assert.Contains("Original", generatedSnapshot);
                VisualAssert.WroteText(scope.Driver, "Hooked progress row");
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("Second line"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Default progress task descriptions render raw markup by visible width without leaking tags.
        /// </summary>
        [Fact]
        public void ProgressContext_DefaultTaskDescriptionMarkup_RendersVisibleTextAndColors()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                using (ProgressContext progress = Consoul.Progress(new ProgressContextOptions
                {
                    Width = 52,
                    LabelWidth = 24,
                    ActiveColor = ConsoleColor.Yellow,
                    CompleteColor = ConsoleColor.Blue
                }))
                {
                    ProgressTask task = progress.AddTask("[green]Build[/] for [[prod]]", 4);
                    task.Update(2);

                    string row = progress.FormatRow(task);

                    Assert.Contains("Build for [prod]", row);
                    Assert.DoesNotContain("[green]", row);
                    Assert.DoesNotContain("[/]", row);

                    task.Complete("[cyan]Build done[/] for [[prod]]");
                }

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(" for [prod]") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build done") && write.Color == ConsoleColor.Cyan);
                Assert.DoesNotContain(scope.Driver.Writes, write =>
                    write.Text.Contains("[green]") ||
                    write.Text.Contains("[cyan]") ||
                    write.Text.Contains("[/]"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// TaskDescriptionColumn renders raw markup by visible width while other progress columns remain exact.
        /// </summary>
        [Fact]
        public void ProgressContext_TaskDescriptionColumnMarkup_RendersVisibleTextAndColors()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                using (ProgressContext progress = AnsiConsole.Progress(new ProgressContextOptions
                {
                    Width = 52,
                    LabelWidth = 22,
                    ActiveColor = ConsoleColor.Yellow
                })
                    .Columns(new TaskDescriptionColumn(), new PercentageColumn()))
                {
                    ProgressTask task = progress.AddTask("[green]Package[/] [[prod]]", 4);
                    task.Update(1);

                    string row = progress.FormatRow(task);

                    Assert.Contains("Package [prod]", row);
                    Assert.Contains("25%", row);
                    Assert.DoesNotContain("[green]", row);
                    Assert.DoesNotContain("[/]", row);
                    task.Complete();
                }

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Package") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(" [prod]") && write.Color == ConsoleColor.Yellow);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("25%"));
                Assert.DoesNotContain(scope.Driver.Writes, write =>
                    write.Text.Contains("[green]") ||
                    write.Text.Contains("[/]"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Configured progress columns control which parts of a row are rendered.
        /// </summary>
        [Fact]
        public void ProgressContext_ConfiguredColumns_RenderOnlyRequestedCoreColumns()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                using (ProgressContext progress = AnsiConsole.Progress(new ProgressContextOptions { Width = 60, LabelWidth = 16 })
                    .Columns(new TaskDescriptionColumn(), new PercentageColumn()))
                {
                    ProgressTask task = progress.AddTask("Package assets", 4);
                    task.Update(1);
                    task.Complete("Package ready");
                }

                VisualAssert.WroteText(scope.Driver, "Package assets");
                VisualAssert.WroteText(scope.Driver, "25%");
                VisualAssert.WroteText(scope.Driver, "100%");
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("["));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Configured progress columns can come from generated MVC-style column collections.
        /// </summary>
        [Fact]
        public void ProgressContext_ConfiguredColumns_AcceptsEnumerableColumns()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                IEnumerable<ProgressColumn> columns = new List<ProgressColumn>
                {
                    new TaskDescriptionColumn(),
                    new PercentageColumn()
                };

                using (ProgressContext progress = AnsiConsole.Progress(new ProgressContextOptions { Width = 60, LabelWidth = 16 })
                    .Columns(columns))
                {
                    Assert.Equal(2, progress.Options.GetColumns().Count);
                    Assert.True(progress.Options.ShowPercentage);

                    ProgressTask task = progress.AddTask("Generated", 4);
                    task.Update(2);
                    task.Complete("Generated ready");
                }

                VisualAssert.WroteText(scope.Driver, "Generated");
                VisualAssert.WroteText(scope.Driver, "50%");
                VisualAssert.WroteText(scope.Driver, "100%");
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("["));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// ProgressBarColumn width and common style aliases are accepted by configured column rows.
        /// </summary>
        [Fact]
        public void ProgressContext_ProgressBarColumnWidth_RendersPreferredWidth()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                var completedStyle = new Style(ConsoleColor.Green);
                var column = new ProgressBarColumn
                {
                    Width = 6,
                    CompleteStyle = completedStyle,
                    FinishedStyle = new Style(ConsoleColor.Cyan),
                    RemainingStyle = new Style(ConsoleColor.DarkGray),
                    IndeterminateStyle = new Style(ConsoleColor.Yellow)
                };

                Assert.Same(completedStyle, column.CompletedStyle);

                using (ProgressContext progress = AnsiConsole.Progress(new ProgressContextOptions { Width = 40, LabelWidth = 8, MinimumBarWidth = 2 })
                    .Columns(new TaskDescriptionColumn(), column, new PercentageColumn()))
                {
                    ProgressTask task = progress.AddTask("Package", 10);
                    task.Update(5);

                    string row = progress.FormatRow(task);

                    Assert.Contains("[###---]", row);
                    Assert.Contains("50%", row);
                    task.Update(10);
                }

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[") && write.Color != ConsoleColor.Green && write.Color != ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("50%"));
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("###") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("---") && write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("######") && write.Color == ConsoleColor.Cyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Progress column palette helpers accept direct Spectre-style and RGB color values.
        /// </summary>
        [Fact]
        public void ProgressContext_ColumnPaletteHelpers_AcceptDirectColors()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                var spinner = new Spinner("Test", "A", "B");
                ProgressBarColumn bar = new ProgressBarColumn { Width = 6 }
                    .CompleteColor(SpectreColor.Green)
                    .FinishedColor(RgbColor.Cyan)
                    .RemainingColor(SpectreColor.DarkGray)
                    .IndeterminateColor(RgbColor.Yellow);
                SpinnerColumn spinnerColumn = new SpinnerColumn { Spinner = spinner }
                    .SpinnerStyle(SpectreColor.Magenta);
                TextColumn textColumn = new TextColumn("=>", RgbColor.Aqua)
                    .TextColor(SpectreColor.Aqua);

                Assert.Equal(ConsoleColor.Green, bar.CompletedStyle.Foreground);
                Assert.Equal(ConsoleColor.Cyan, bar.FinishedStyle.Foreground);
                Assert.Equal(ConsoleColor.DarkGray, bar.RemainingStyle.Foreground);
                Assert.Equal(ConsoleColor.Yellow, bar.IndeterminateStyle.Foreground);
                Assert.Equal(ConsoleColor.Magenta, spinnerColumn.Style.Foreground);
                Assert.Equal(ConsoleColor.Cyan, textColumn.Style.Foreground);

                using (ProgressContext progress = AnsiConsole.Progress(new ProgressContextOptions { Width = 60, LabelWidth = 10 })
                    .Columns(spinnerColumn, new TaskDescriptionColumn(), bar, textColumn, new PercentageColumn()))
                {
                    ProgressTask task = progress.AddTask("Package", 10);
                    task.Update(5);
                    progress.Refresh();
                    task.Update(10);
                }

                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.TrimStart().StartsWith("A", StringComparison.Ordinal) &&
                    write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("###") &&
                    write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("---") &&
                    write.Color == ConsoleColor.DarkGray);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("######") &&
                    write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("=>") &&
                    write.Color == ConsoleColor.Cyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Progress column palette helpers accept Spectre-style style strings for quick POC column wiring.
        /// </summary>
        [Fact]
        public void ProgressContext_ColumnPaletteHelpers_AcceptStyleStrings()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                var spinner = new Spinner("Test", "A", "B");
                ProgressBarColumn bar = new ProgressBarColumn { Width = 6 }
                    .Complete("green")
                    .Finished("white on blue")
                    .Remaining("grey")
                    .Indeterminate("yellow");
                SpinnerColumn spinnerColumn = new SpinnerColumn { Spinner = spinner }
                    .SpinnerStyle("magenta");
                TextColumn textColumn = new TextColumn("=>", "red")
                    .TextColor("white on blue");

                Assert.Equal(ConsoleColor.Green, bar.CompletedStyle.Foreground);
                Assert.Equal(ConsoleColor.White, bar.FinishedStyle.Foreground);
                Assert.Equal(ConsoleColor.Blue, bar.FinishedStyle.Background);
                Assert.Equal(ConsoleColor.Gray, bar.RemainingStyle.Foreground);
                Assert.Equal(ConsoleColor.Yellow, bar.IndeterminateStyle.Foreground);
                Assert.Equal(ConsoleColor.Magenta, spinnerColumn.Style.Foreground);
                Assert.Equal(ConsoleColor.White, textColumn.Style.Foreground);
                Assert.Equal(ConsoleColor.Blue, textColumn.Style.Background);

                using (ProgressContext progress = AnsiConsole.Progress(new ProgressContextOptions { Width = 60, LabelWidth = 10 })
                    .Columns(spinnerColumn, new TaskDescriptionColumn(), bar, textColumn, new PercentageColumn()))
                {
                    ProgressTask task = progress.AddTask("Package", 10);
                    task.Update(5);
                    progress.Refresh();
                    task.Update(10);
                }

                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.TrimStart().StartsWith("A", StringComparison.Ordinal) &&
                    write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("###") &&
                    write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("---") &&
                    write.Color == ConsoleColor.Gray);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("######") &&
                    write.Color == ConsoleColor.White &&
                    write.BackgroundColor == ConsoleColor.Blue);
                Assert.Contains(scope.Driver.Writes, write =>
                    write.Text.Contains("=>") &&
                    write.Color == ConsoleColor.White &&
                    write.BackgroundColor == ConsoleColor.Blue);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// ProgressBarColumn width remains a preference and shrinks inside narrow terminals.
        /// </summary>
        [Fact]
        public void ProgressContext_ProgressBarColumnWidth_ShrinksWithinTerminalWidth()
        {
            using (var scope = new ConsoleDriverScope(14))
            {
                using (ProgressContext progress = AnsiConsole.Progress(new ProgressContextOptions { Width = 14, LabelWidth = 10, MinimumBarWidth = 2 })
                    .Columns(new TaskDescriptionColumn(), new ProgressBarColumn { Width = 40 }, new PercentageColumn()))
                {
                    ProgressTask task = progress.AddTask("Package", 10);
                    task.Update(5);

                    string row = progress.FormatRow(task);

                    Assert.True(row.Length <= 14, "Expected the formatted row to stay within the terminal width.");
                    Assert.Contains("[", row);
                    Assert.Contains("50%", row);
                    Assert.DoesNotContain("[####################", row);
                }

                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Operational progress columns render elapsed time, remaining time, downloaded quantity, transfer speed, and spinner frames.
        /// </summary>
        [Fact]
        public void ProgressContext_ConfiguredColumns_RenderOperationalMetrics()
        {
            using (var scope = new ConsoleDriverScope(96))
            {
                var spinner = new Spinner("Test", "A", "B");
                using (ProgressContext progress = AnsiConsole.Progress(new ProgressContextOptions { Width = 90, LabelWidth = 18 })
                    .Columns(
                        new SpinnerColumn { Spinner = spinner, Style = new Style(ConsoleColor.Magenta) },
                        new TaskDescriptionColumn(),
                        new DownloadedColumn(),
                        new TransferSpeedColumn(),
                        new RemainingTimeColumn(),
                        new ElapsedTimeColumn()))
                {
                    ProgressTask task = progress.AddTask("Download package", 2048);
                    Thread.Sleep(10);
                    task.Update(1024);
                    progress.Refresh();
                }

                VisualAssert.WroteText(scope.Driver, "Download package");
                VisualAssert.WroteText(scope.Driver, "1 KB");
                VisualAssert.WroteText(scope.Driver, "/s");
                Assert.Contains(scope.Driver.Writes, write => write.Text.TrimStart().StartsWith("A", StringComparison.Ordinal) && write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.TrimStart().StartsWith("B", StringComparison.Ordinal) && write.Color == ConsoleColor.Magenta);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(":"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// TextColumn renders literal and task-template text with markup-aware styling.
        /// </summary>
        [Fact]
        public void ProgressContext_TextColumn_RendersTemplatesAndStyles()
        {
            using (var scope = new ConsoleDriverScope(88))
            {
                using (ProgressContext progress = AnsiConsole.Progress(new ProgressContextOptions { Width = 80, LabelWidth = 18 })
                    .Columns(
                        new TextColumn("[cyan]{task.Description}[/]"),
                        new TextColumn("=>", new Style(ConsoleColor.DarkGray)),
                        new TextColumn("{task.Value}/{task.MaxValue}") { Width = 7 },
                        new TextColumn("{task.Percentage} {task.State}")))
                {
                    ProgressTask task = progress.AddTask("Build [[prod]]", 10);
                    task.Update(5);
                    task.Complete("Build done");
                }

                VisualAssert.WroteText(scope.Driver, "Build [prod]");
                VisualAssert.WroteText(scope.Driver, "5/10");
                VisualAssert.WroteText(scope.Driver, "50% Active");
                VisualAssert.WroteText(scope.Driver, "100% Completed");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build [prod]") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("=>") && write.Color == ConsoleColor.DarkGray);
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("[cyan]") || write.Text.Contains("[/]"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// TextColumn shrinks within narrow terminals so literal separators and templates remain bounded.
        /// </summary>
        [Fact]
        public void ProgressContext_TextColumn_ShrinksWithinTerminalWidth()
        {
            using (var scope = new ConsoleDriverScope(24))
            {
                using (ProgressContext progress = AnsiConsole.Progress(new ProgressContextOptions { Width = 24 })
                    .Columns(
                        new TextColumn("{task.Description}") { Width = 18 },
                        new TextColumn("|"),
                        new PercentageColumn()))
                {
                    ProgressTask task = progress.AddTask("Very long package description", 4);
                    task.Update(2);
                }

                VisualAssert.WroteText(scope.Driver, "50%");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// A progress-bar-only column configuration omits task descriptions and percentages.
        /// </summary>
        [Fact]
        public void ProgressContext_ProgressBarColumnOnly_OmitsDescriptionAndPercentage()
        {
            using (var scope = new ConsoleDriverScope(40))
            {
                using (ProgressContext progress = AnsiConsole.Progress(new ProgressContextOptions { Width = 32 })
                    .Columns(new ProgressBarColumn()))
                {
                    ProgressTask task = progress.AddTask("Hidden description", 2);
                    task.Update(1);
                }

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("["));
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("Hidden description"));
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("%"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }
    }
}
