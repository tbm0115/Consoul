using System;
using System.Collections.Generic;
using System.Linq;
using ConsoulLibrary;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace Consoul.Tests.Prompt
{
    /// <summary>
    /// Tests multi-selection prompt behavior.
    /// </summary>
    public class MultiSelectionPromptTests
    {
        /// <summary>
        /// Multi-selection prompts parse comma-separated indexes and ranges.
        /// </summary>
        [Fact]
        public void Render_ParsesCommaSeparatedIndexesAndRanges()
        {
            using (new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "1,3-4" }));

                try
                {
                    var prompt = new MultiSelectionPrompt("Pick deployments", false, false, "Build", "Test", "Package", "Deploy");

                    MultiSelectionResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 0, 2, 3 }, result.Indexes.ToArray());
                    Assert.Equal(new[] { true, false, true, true }, prompt.Options.Select(option => option.Selected).ToArray());
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Default multi-selection options are accepted when the user presses Enter.
        /// </summary>
        [Fact]
        public void Render_EmptyInputSelectsDefaults()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                try
                {
                    var prompt = new MultiSelectionPrompt("Pick defaults");
                    prompt.Add("Alpha", isDefault: true);
                    prompt.Add("Beta");
                    prompt.Add("Gamma", isDefault: true);

                    MultiSelectionResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 0, 2 }, result.Indexes.ToArray());
                    VisualAssert.WroteText(scope.Driver, "[x]");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Spectre-style multi-selection helpers can preselect multiple defaults by label.
        /// </summary>
        [Fact]
        public void Render_SelectLabelsPreselectsMultipleDefaults()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                try
                {
                    var prompt = new MultiSelectionPrompt("Pick defaults")
                        .AddChoices("Build", "Test", "Deploy")
                        .Select("Build", "Deploy");

                    MultiSelectionResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 0, 2 }, result.Indexes.ToArray());
                    Assert.Equal(new[] { true, false, true }, prompt.Options.Select(option => option.Selected).ToArray());
                    Assert.Equal(new[] { true, false, true }, prompt.Options.Select(option => option.IsDefault).ToArray());
                    VisualAssert.WroteText(scope.Driver, "[x]");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Spectre-style select-all defaults skip disabled choices.
        /// </summary>
        [Fact]
        public void Render_SelectAllPreselectsOnlySelectableDefaults()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                try
                {
                    var prompt = new MultiSelectionPrompt("Pick defaults")
                        .AddChoices("Build", "Test");
                    prompt.AddDisabled("Deploy", "approval required");
                    prompt.SelectAll();

                    MultiSelectionResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 0, 1 }, result.Indexes.ToArray());
                    Assert.Equal(new[] { true, true, false }, prompt.Options.Select(option => option.Selected).ToArray());
                    VisualAssert.WroteText(scope.Driver, "disabled: approval required");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Spectre-style multi-selection default labels can be declared before choices are added.
        /// </summary>
        [Fact]
        public void Render_SelectBeforeAddChoicesPreselectsFutureLabelDefaults()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                try
                {
                    var prompt = new MultiSelectionPrompt("Pick defaults")
                        .Select("Build", "Deploy")
                        .AddChoices("Build", "Test", "Deploy");

                    MultiSelectionResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 0, 2 }, result.Indexes.ToArray());
                    Assert.Equal(new[] { true, false, true }, prompt.Options.Select(option => option.Selected).ToArray());
                    Assert.Equal(new[] { true, false, true }, prompt.Options.Select(option => option.IsDefault).ToArray());
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Spectre-style select-all can be declared before choices are added.
        /// </summary>
        [Fact]
        public void Render_SelectAllBeforeAddChoicesPreselectsFutureOptions()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                try
                {
                    var prompt = new MultiSelectionPrompt("Pick defaults")
                        .SelectAll()
                        .AddChoices("Build", "Test", "Deploy");

                    MultiSelectionResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 0, 1, 2 }, result.Indexes.ToArray());
                    Assert.Equal(new[] { true, true, true }, prompt.Options.Select(option => option.Selected).ToArray());
                    Assert.Equal(new[] { true, true, true }, prompt.Options.Select(option => option.IsDefault).ToArray());
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Invalid multi-selection input re-renders without overflowing narrow consoles.
        /// </summary>
        [Fact]
        public void Render_InvalidThenValidSelection_DoesNotOverflow()
        {
            using (var scope = new ConsoleDriverScope(24))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "9", "2,3" }));

                try
                {
                    var prompt = new MultiSelectionPrompt(
                        "Choose deployment tasks with a long prompt message",
                        clear: true,
                        allowEmpty: false,
                        "Alpha task with a long label",
                        "Beta task",
                        "Gamma task");

                    MultiSelectionResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 1, 2 }, result.Indexes.ToArray());
                    Assert.True(scope.Driver.ClearCount >= 2);
                    VisualAssert.WroteText(scope.Driver, "Invalid selection!");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Multi-selection prompts support all, none, and cancellation inputs.
        /// </summary>
        [Fact]
        public void Render_AllNoneAndEscapeInputs_PreserveResultState()
        {
            using (new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "all" }));

                try
                {
                    MultiSelectionResult all = new MultiSelectionPrompt("Pick all", false, false, "One", "Two").Render();
                    Assert.Equal(new[] { 0, 1 }, all.Indexes.ToArray());

                    Routines.InitializeRoutine(new TestRoutine(new[] { "none" }));
                    MultiSelectionResult none = new MultiSelectionPrompt("Pick none", false, true, "One", "Two").Render();
                    Assert.False(none.IsCanceled);
                    Assert.False(none.HasSelection);
                    Assert.Empty(none.Indexes);

                    Routines.InitializeRoutine(new TestRoutine(new[] { "exit" }));
                    MultiSelectionResult escaped = new MultiSelectionPrompt("Pick maybe", false, false, "One", "Two").Render();
                    Assert.True(escaped.IsCanceled);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Multi-selection all command selects only enabled options.
        /// </summary>
        [Fact]
        public void Render_AllCommandSkipsDisabledOptions()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "all" }));

                try
                {
                    var prompt = new MultiSelectionPrompt("Pick tasks");
                    prompt.Add("Build");
                    prompt.AddDisabled("Deploy", "approval required");
                    prompt.Add("Report");

                    MultiSelectionResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 0, 2 }, result.Indexes.ToArray());
                    Assert.Equal(new[] { true, false, true }, prompt.Options.Select(option => option.Selected).ToArray());
                    VisualAssert.WroteText(scope.Driver, "disabled: approval required");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Multi-selection prompts reject explicit disabled option indexes and repeat.
        /// </summary>
        [Fact]
        public void Render_DisabledOptionSelectionRepeats()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2", "1,3" }));

                try
                {
                    var prompt = new MultiSelectionPrompt("Pick tasks");
                    prompt.Add("Build");
                    prompt.AddDisabled("Deploy", "approval required");
                    prompt.Add("Report");

                    MultiSelectionResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 0, 2 }, result.Indexes.ToArray());
                    Assert.Equal(new[] { true, false, true }, prompt.Options.Select(option => option.Selected).ToArray());
                    VisualAssert.WroteText(scope.Driver, "Invalid selection!");
                    VisualAssert.WroteText(scope.Driver, "Option unavailable: approval required");
                    VisualAssert.WroteText(scope.Driver, "disabled: approval required");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Separators group multi-selection options visually without consuming selection indexes.
        /// </summary>
        [Fact]
        public void Render_SeparatorsDoNotConsumeSelectionIndexes()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "1,3" }));

                try
                {
                    var prompt = new MultiSelectionPrompt("Pick tasks");
                    prompt.AddSeparator("Build");
                    prompt.Add("Compile");
                    prompt.Add("Test");
                    prompt.AddSeparator("Release");
                    prompt.Add("Deploy");

                    MultiSelectionResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 0, 2 }, result.Indexes.ToArray());
                    Assert.Equal(
                        new[] { true, false, true },
                        prompt.Options.Where(option => !option.IsSeparator).Select(option => option.Selected).ToArray());
                    VisualAssert.WroteText(scope.Driver, "-- Build --");
                    VisualAssert.WroteText(scope.Driver, "-- Release --");
                    Assert.DoesNotContain(scope.Driver.Lines, line => line.Contains("1) Build"));
                    Assert.DoesNotContain(scope.Driver.Lines, line => line.Contains("2) Release"));
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Paged multi-selection prompts keep long checklists bounded and allow next/previous navigation.
        /// </summary>
        [Fact]
        public void Render_PageSize_NextPreviousCommandsShowPagedOptions()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "next", "prev", "1,3" }));

                try
                {
                    var prompt = new MultiSelectionPrompt("Pick tasks", clear: true)
                    {
                        PageSize = 2
                    };
                    prompt.AddSeparator("Build");
                    prompt.Add("Compile");
                    prompt.Add("Test");
                    prompt.AddSeparator("Release");
                    prompt.Add("Package");
                    prompt.Add("Deploy");

                    MultiSelectionResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 0, 2 }, result.Indexes.ToArray());
                    VisualAssert.ContainsText(scope.Driver, "[ ] - 1) Compile");
                    VisualAssert.ContainsText(scope.Driver, "[ ] - 2) Test");
                    VisualAssert.ContainsText(scope.Driver, "Showing options 1-2 of 4");
                    Assert.DoesNotContain("[ ] - 3) Package", scope.Driver.Text);
                    Assert.DoesNotContain("[ ] - 4) Deploy", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Filter-enabled multi-selection prompts narrow rendered choices and let all select matching options.
        /// </summary>
        [Fact]
        public void Render_FilterCommandNarrowsVisibleOptionsAndAllSelectsMatches()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "/deploy", "all" }));

                try
                {
                    var prompt = new MultiSelectionPrompt("Pick tasks", clear: true, allowEmpty: false)
                    {
                        FilterEnabled = true
                    };
                    prompt.AddSeparator("Build");
                    prompt.Add("Compile");
                    prompt.Add("Test");
                    prompt.AddSeparator("Release");
                    prompt.Add("Deploy API");
                    prompt.Add("Deploy database");
                    prompt.Add("Promote");

                    MultiSelectionResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 2, 3 }, result.Indexes.ToArray());
                    Assert.Equal(new[] { false, false, true, true, false }, prompt.Options.Where(option => !option.IsSeparator).Select(option => option.Selected).ToArray());
                    VisualAssert.ContainsText(scope.Driver, "3) Deploy API");
                    VisualAssert.ContainsText(scope.Driver, "4) Deploy database");
                    VisualAssert.ContainsText(scope.Driver, "Filter: deploy (2 matches)");
                    Assert.DoesNotContain("1) Compile", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Keyboard-enabled multi-selection prompts toggle highlighted options and accept checked selections.
        /// </summary>
        [Fact]
        public void Render_KeyboardNavigation_TogglesAndAcceptsSelections()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                scope.Driver.EnqueueKey(Key(ConsoleKey.DownArrow));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Spacebar, ' '));
                scope.Driver.EnqueueKey(Key(ConsoleKey.DownArrow));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Spacebar, ' '));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Enter, '\r'));

                var prompt = new MultiSelectionPrompt("Pick tasks")
                {
                    KeyboardNavigationEnabled = true,
                    KeyboardSelectionColor = ConsoleColor.Cyan
                };
                prompt.Add("Build");
                prompt.Add("Test");
                prompt.Add("Deploy");

                MultiSelectionResult result = prompt.Render();

                Assert.True(result.HasSelection);
                Assert.Equal(new[] { 1, 2 }, result.Indexes.ToArray());
                Assert.Equal(new[] { false, true, true }, prompt.Options.Select(option => option.Selected).ToArray());
                VisualAssert.WroteText(scope.Driver, "> [ ] - 2) Test");
                VisualAssert.WroteText(scope.Driver, "> [x] - 3) Deploy");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("> [x] - 3) Deploy") && write.Color == ConsoleColor.Cyan);
                Assert.True(scope.Driver.CursorVisible);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Keyboard movement skips disabled multi-selection options.
        /// </summary>
        [Fact]
        public void Render_KeyboardNavigation_SkipsDisabledOptions()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                scope.Driver.EnqueueKey(Key(ConsoleKey.DownArrow));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Enter, '\r'));

                var prompt = new MultiSelectionPrompt("Pick tasks")
                {
                    KeyboardNavigationEnabled = true
                };
                prompt.Add("Build");
                prompt.AddDisabled("Deploy", "approval required");
                prompt.Add("Report");

                MultiSelectionResult result = prompt.Render();

                Assert.True(result.HasSelection);
                Assert.Equal(new[] { 2 }, result.Indexes.ToArray());
                Assert.Equal(new[] { false, false, true }, prompt.Options.Select(option => option.Selected).ToArray());
                VisualAssert.WroteText(scope.Driver, "disabled: approval required");
                VisualAssert.WroteText(scope.Driver, "> [ ] - 3) Report");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Keyboard filtering narrows multi-selection options while preserving selected indexes.
        /// </summary>
        [Fact]
        public void Render_KeyboardNavigation_FilterTextNarrowsOptions()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                scope.Driver.EnqueueKey(Key(ConsoleKey.D, 'd'));
                scope.Driver.EnqueueKey(Key(ConsoleKey.E, 'e'));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Spacebar, ' '));
                scope.Driver.EnqueueKey(Key(ConsoleKey.DownArrow));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Spacebar, ' '));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Enter, '\r'));

                var prompt = new MultiSelectionPrompt("Pick tasks", clear: true)
                {
                    FilterEnabled = true,
                    KeyboardNavigationEnabled = true
                };
                prompt.AddSeparator("Build");
                prompt.Add("Compile");
                prompt.Add("Test");
                prompt.AddSeparator("Release");
                prompt.Add("Deploy API");
                prompt.Add("Deploy database");
                prompt.Add("Promote");

                MultiSelectionResult result = prompt.Render();

                Assert.True(result.HasSelection);
                Assert.Equal(new[] { 2, 3 }, result.Indexes.ToArray());
                Assert.Equal(new[] { false, false, true, true, false }, prompt.Options.Where(option => !option.IsSeparator).Select(option => option.Selected).ToArray());
                VisualAssert.ContainsText(scope.Driver, "Filter: de (2 matches)");
                VisualAssert.ContainsText(scope.Driver, "> [x] - 4) Deploy database");
                Assert.DoesNotContain("1) Compile", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Typed multi-selection returns both selected indexes and selected objects.
        /// </summary>
        [Fact]
        public void ConsoulMultiSelectTyped_ReturnsSelectedItems()
        {
            using (new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "1,3" }));

                try
                {
                    var tasks = new[]
                    {
                        new DeployTask("Build", 1),
                        new DeployTask("Test", 2),
                        new DeployTask("Deploy", 3)
                    };

                    MultiSelectionResult<DeployTask> result = ConsoulLibrary.Consoul.MultiSelect(
                        "Pick tasks",
                        task => task.Name,
                        false,
                        tasks);

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 0, 2 }, result.Indexes.ToArray());
                    Assert.Equal(new[] { 1, 3 }, result.SelectedItems.Select(task => task.Id).ToArray());
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Typed multi-selection prompts can return selected objects directly while skipping disabled typed options.
        /// </summary>
        [Fact]
        public void MultiSelectionPromptTyped_RenderSelection_ReturnsSelectedItemsWithSeparatorsAndDisabledOptions()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "all" }));

                try
                {
                    var prompt = new MultiSelectionPrompt<DeployTask>("Pick tasks", false, false, task => task.Name);
                    prompt.AddSeparator("Build");
                    prompt.Add(new DeployTask("Compile", 1));
                    prompt.AddDisabled(new DeployTask("Production approval", 2), "approval required");
                    prompt.AddSeparator("Release");
                    prompt.Add(new DeployTask("Deploy API", 3));
                    prompt.Add(new DeployTask("Deploy database", 4));

                    MultiSelectionResult<DeployTask> result = prompt.RenderSelection();

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 0, 2, 3 }, result.Indexes.ToArray());
                    Assert.Equal(new[] { 1, 3, 4 }, result.SelectedItems.Select(task => task.Id).ToArray());
                    VisualAssert.WroteText(scope.Driver, "disabled: approval required");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Simple multi-select overloads can mark default options without constructing a prompt manually.
        /// </summary>
        [Fact]
        public void ConsoulMultiSelect_EnumerableOptions_EmptyInputUsesDefaultOptions()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                try
                {
                    MultiSelectionResult result = ConsoulLibrary.Consoul.MultiSelect(
                        "Pick stages",
                        new[] { "Build", "Test", "Deploy" },
                        defaultOptions: new[] { "Build", "Deploy" });

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 0, 2 }, result.Indexes.ToArray());
                    VisualAssert.WroteText(scope.Driver, "[x]");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Typed multi-select overloads can mark default objects by equality.
        /// </summary>
        [Fact]
        public void ConsoulMultiSelectTyped_EmptyInputUsesDefaultItems()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                var tasks = new[]
                {
                    new DeployTask("Build", 1),
                    new DeployTask("Test", 2),
                    new DeployTask("Deploy", 3)
                };

                try
                {
                    MultiSelectionResult<DeployTask> result = ConsoulLibrary.Consoul.MultiSelect(
                        "Pick tasks",
                        tasks,
                        task => task.Name,
                        new[] { tasks[0], tasks[2] });

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 0, 2 }, result.Indexes.ToArray());
                    Assert.Equal(new[] { 1, 3 }, result.SelectedItems.Select(task => task.Id).ToArray());
                    VisualAssert.WroteText(scope.Driver, "[x]");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Typed multi-select overloads can mark defaults with a predicate when equality is not enough.
        /// </summary>
        [Fact]
        public void ConsoulMultiSelectTyped_DefaultPredicateMarksExpectedItems()
        {
            using (new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                var tasks = new[]
                {
                    new DeployTask("Build", 1),
                    new DeployTask("Test", 2),
                    new DeployTask("Deploy", 3)
                };

                try
                {
                    MultiSelectionResult<DeployTask> result = ConsoulLibrary.Consoul.MultiSelect(
                        "Pick tasks",
                        tasks,
                        task => task.Name,
                        task => task.Id != 2);

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 1, 3 }, result.SelectedItems.Select(task => task.Id).ToArray());
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Described multi-selection options render helper text and can be selected normally.
        /// </summary>
        [Fact]
        public void Render_DescribedOptionsRenderHelperText()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "1" }));

                try
                {
                    var prompt = new MultiSelectionPrompt("Pick tasks");
                    prompt.AddDescribed(
                        "Build",
                        "Compiles the solution before packaging artifacts.",
                        ConsoleColor.Green);
                    prompt.Add("Deploy");

                    MultiSelectionResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 0 }, result.Indexes.ToArray());
                    VisualAssert.WroteText(scope.Driver, "Build");
                    VisualAssert.WroteText(scope.Driver, "Compiles the solution");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Multi-selection filtering matches option descriptions as well as labels.
        /// </summary>
        [Fact]
        public void Render_FilterCommandMatchesDescriptionText()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "/approval", "all" }));

                try
                {
                    var prompt = new MultiSelectionPrompt("Pick tasks", clear: true)
                    {
                        FilterEnabled = true
                    };
                    prompt.Add("Build");
                    prompt.AddDescribed("Deploy", "Requires approval before production release.", ConsoleColor.Green);
                    prompt.Add("Report");

                    MultiSelectionResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 1 }, result.Indexes.ToArray());
                    VisualAssert.ContainsText(scope.Driver, "2) Deploy");
                    VisualAssert.ContainsText(scope.Driver, "Requires approval");
                    VisualAssert.ContainsText(scope.Driver, "Filter: approval (1 matches)");
                    Assert.DoesNotContain("1) Build", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Multi-selection filtering can match hidden search terms, and all selects only the matching aliases.
        /// </summary>
        [Fact]
        public void Render_FilterCommandMatchesHiddenSearchTermsAndAllSelectsMatches()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "/ship", "all" }));

                try
                {
                    var prompt = new MultiSelectionPrompt("Pick tasks", clear: true, allowEmpty: false)
                    {
                        FilterEnabled = true
                    };
                    prompt.Add("Build");
                    int releaseIndex = prompt.Add("Release");
                    prompt.AddSearchTerms(releaseIndex, "ship", "promote");
                    prompt.Add("Report");

                    MultiSelectionResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 1 }, result.Indexes.ToArray());
                    Assert.Equal(new[] { false, true, false }, prompt.Options.Select(option => option.Selected).ToArray());
                    VisualAssert.ContainsText(scope.Driver, "2) Release");
                    VisualAssert.ContainsText(scope.Driver, "Filter: ship (1 matches)");
                    Assert.DoesNotContain("1) Build", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Typed multi-selection prompts can derive helper text from the underlying domain object.
        /// </summary>
        [Fact]
        public void MultiSelectionPromptTyped_WithDescriptions_RendersDomainHelperText()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "1,2" }));

                try
                {
                    var prompt = new MultiSelectionPrompt<DeployTask>("Pick tasks", false, false, task => task.Name)
                        .WithDescriptions(task => "Task id: " + task.Id);
                    prompt.Add(new DeployTask("Build", 1));
                    prompt.Add(new DeployTask("Deploy", 2));

                    MultiSelectionResult<DeployTask> result = prompt.RenderSelection();

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 1, 2 }, result.SelectedItems.Select(task => task.Id).ToArray());
                    VisualAssert.WroteText(scope.Driver, "Task id: 1");
                    VisualAssert.WroteText(scope.Driver, "Task id: 2");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Typed multi-selection prompts can derive hidden filter aliases from domain objects.
        /// </summary>
        [Fact]
        public void MultiSelectionPromptTyped_WithSearchTerms_FiltersByDomainAliases()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "/release", "all" }));

                try
                {
                    var prompt = new MultiSelectionPrompt<DeployTask>("Pick tasks", true, false, task => task.Name)
                    {
                        FilterEnabled = true
                    }.WithSearchTerms(task => new[] { "task-" + task.Id, task.Id == 2 ? "release" : "build" });
                    prompt.Add(new DeployTask("Compile", 1));
                    prompt.Add(new DeployTask("Deploy", 2));

                    MultiSelectionResult<DeployTask> result = prompt.RenderSelection();

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 2 }, result.SelectedItems.Select(task => task.Id).ToArray());
                    VisualAssert.ContainsText(scope.Driver, "2) Deploy");
                    VisualAssert.ContainsText(scope.Driver, "Filter: release (1 matches)");
                    Assert.DoesNotContain("1) Compile", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Typed multi-selection metadata selectors refresh choices that were already added.
        /// </summary>
        [Fact]
        public void MultiSelectionPromptTyped_MetadataSelectorsApplyToExistingChoices()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "/release", "all" }));

                try
                {
                    var prompt = new MultiSelectionPrompt<DeployTask>("Pick tasks", true, false, task => task.Name)
                    {
                        FilterEnabled = true
                    };
                    prompt.Add(new DeployTask("Build", 1));
                    prompt.Add(new DeployTask("Deploy", 2));

                    prompt
                        .WithDescriptions(task => "Task id: " + task.Id)
                        .WithSearchTerms(task => new[] { "task-" + task.Id, task.Id == 2 ? "release" : "build" });

                    MultiSelectionResult<DeployTask> result = prompt.RenderSelection();

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 2 }, result.SelectedItems.Select(task => task.Id).ToArray());
                    VisualAssert.WroteText(scope.Driver, "Task id: 2");
                    VisualAssert.ContainsText(scope.Driver, "Filter: release (1 matches)");
                    Assert.DoesNotContain("1) Build", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Typed multi-selection defaults can be selected before choices are projected into the prompt.
        /// </summary>
        [Fact]
        public void MultiSelectionPromptTyped_SelectBeforeAddChoicesPreselectsDefaults()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                try
                {
                    var build = new DeployTask("Build", 1);
                    var test = new DeployTask("Test", 2);
                    var deploy = new DeployTask("Deploy", 3);
                    var prompt = new MultiSelectionPrompt<DeployTask>("Pick tasks")
                        .UseConverter(task => task.Name)
                        .Select(build, deploy)
                        .AddChoices(build, test, deploy);

                    MultiSelectionResult<DeployTask> result = prompt.RenderSelection();

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 1, 3 }, result.SelectedItems.Select(task => task.Id).ToArray());
                    Assert.Equal(new[] { true, false, true }, prompt.Options.Select(option => option.Selected).ToArray());
                    Assert.Equal(new[] { true, false, true }, prompt.Options.Select(option => option.IsDefault).ToArray());
                    VisualAssert.WroteText(scope.Driver, "Build");
                    VisualAssert.WroteText(scope.Driver, "Deploy");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Typed select-all defaults can be declared before choices are projected into the prompt.
        /// </summary>
        [Fact]
        public void MultiSelectionPromptTyped_SelectAllBeforeAddChoicesPreselectsFutureDefaults()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                try
                {
                    var build = new DeployTask("Build", 1);
                    var test = new DeployTask("Test", 2);
                    var deploy = new DeployTask("Deploy", 3);
                    var prompt = new MultiSelectionPrompt<DeployTask>("Pick tasks")
                        .UseConverter(task => task.Name)
                        .SelectAll()
                        .AddChoices(build, test, deploy);

                    MultiSelectionResult<DeployTask> result = prompt.RenderSelection();

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 1, 2, 3 }, result.SelectedItems.Select(task => task.Id).ToArray());
                    Assert.Equal(new[] { true, true, true }, prompt.Options.Select(option => option.Selected).ToArray());
                    Assert.Equal(new[] { true, true, true }, prompt.Options.Select(option => option.IsDefault).ToArray());
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        /// <summary>
        /// Typed multi-selection converters refresh choices that were already added.
        /// </summary>
        [Fact]
        public void MultiSelectionPromptTyped_ConverterAppliesToExistingChoices()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "1,2" }));

                try
                {
                    var prompt = new MultiSelectionPrompt<DeployTask>("Pick tasks");
                    prompt.AddChoiceGroup(
                        new DeployTask("Release", 0),
                        new DeployTask("Build", 1),
                        new DeployTask("Deploy", 2));

                    prompt.UseConverter(task => task.Name);

                    MultiSelectionResult<DeployTask> result = prompt.RenderSelection();

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 1, 2 }, result.SelectedItems.Select(task => task.Id).ToArray());
                    VisualAssert.WroteText(scope.Driver, "-- Release --");
                    VisualAssert.WroteText(scope.Driver, "Build");
                    VisualAssert.WroteText(scope.Driver, "Deploy");
                    Assert.DoesNotContain("DeployTask", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    ClearRoutine();
                }
            }
        }

        private static void ClearRoutine()
        {
            Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
        }

        private static ConsoleKeyInfo Key(ConsoleKey key, char keyChar = '\0')
        {
            return new ConsoleKeyInfo(keyChar, key, false, false, false);
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

        private sealed class DeployTask
        {
            public DeployTask(string name, int id)
            {
                Name = name;
                Id = id;
            }

            public string Name { get; }

            public int Id { get; }
        }
    }
}
