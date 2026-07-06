using System;
using System.Collections.Generic;
using System.Linq;
using ConsoulLibrary;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace Consoul.Tests.Prompt
{
    /// <summary>
    /// Tests selection prompt behavior.
    /// </summary>
    public class SelectionPromptTests
    {
        /// <summary>
        /// Verifies that prompts repeat when the selected number is larger than the option count.
        /// </summary>
        [Fact]
        public void Render_RepeatsWhenSelectionExceedsOptionCount()
        {
            var routine = new TestRoutine(new[]
            {
                new RoutineInput { Value = "4" },
                new RoutineInput { Value = "1" }
            });

            Routines.InitializeRoutine(routine);

            try
            {
                var prompt = new SelectionPrompt("Pick an option", clear: false, "Alpha", "Beta", "Gamma");

                PromptResult result = prompt.Render();

                Assert.True(result.HasSelection);
                Assert.Equal(0, result.Index);
            }
            finally
            {
                Routines.InitializeRoutine(new TestRoutine(Array.Empty<RoutineInput>()));
            }
        }

        /// <summary>
        /// Simple selection overloads can mark a default option without constructing <see cref="SelectOption"/> instances.
        /// </summary>
        [Fact]
        public void Select_EnumerableOptions_EmptyInputUsesDefaultOption()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    new RoutineInput { Value = string.Empty }
                }));

                try
                {
                    PromptResult result = ConsoulLibrary.Consoul.Select(
                        "Pick environment",
                        new[] { "Development", "Staging", "Production" },
                        defaultOption: "Staging");

                    Assert.True(result.HasSelection);
                    Assert.Equal(1, result.Index);
                    VisualAssert.WroteText(scope.Driver, "Staging");
                    VisualAssert.WroteText(scope.Driver, "(default)");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<RoutineInput>()));
                }
            }
        }

        /// <summary>
        /// Typed selection overloads can mark a default object by equality.
        /// </summary>
        [Fact]
        public void Select_TypedOptions_EmptyInputUsesDefaultItem()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    new RoutineInput { Value = string.Empty }
                }));

                var targets = new[]
                {
                    new DeployTarget("Development", "dev"),
                    new DeployTarget("Production", "prod")
                };

                try
                {
                    SelectionResult<DeployTarget> result = ConsoulLibrary.Consoul.Select(
                        "Pick target",
                        targets,
                        target => target.Name,
                        targets[1]);

                    Assert.True(result.HasSelection);
                    Assert.Equal(1, result.Index);
                    Assert.Equal("prod", result.SelectedItem.Slug);
                    VisualAssert.WroteText(scope.Driver, "Production");
                    VisualAssert.WroteText(scope.Driver, "(default)");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<RoutineInput>()));
                }
            }
        }

        /// <summary>
        /// Typed selection overloads can mark defaults with a predicate when object equality is not enough.
        /// </summary>
        [Fact]
        public void Select_TypedOptions_DefaultPredicateMarksExpectedItem()
        {
            using (new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    new RoutineInput { Value = string.Empty }
                }));

                var targets = new[]
                {
                    new DeployTarget("Development", "dev"),
                    new DeployTarget("Production", "prod")
                };

                try
                {
                    SelectionResult<DeployTarget> result = ConsoulLibrary.Consoul.Select(
                        "Pick target",
                        targets,
                        target => target.Name,
                        target => target.Slug == "prod");

                    Assert.True(result.HasSelection);
                    Assert.Equal("prod", result.SelectedItem.Slug);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<RoutineInput>()));
                }
            }
        }

        /// <summary>
        /// Typed prompts can return a result-first selected object even when separators and disabled typed options are present.
        /// </summary>
        [Fact]
        public void SelectionPromptTyped_RenderSelection_ReturnsSelectedItemWithSeparatorsAndDisabledOptions()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    new RoutineInput { Value = "2" },
                    new RoutineInput { Value = "3" }
                }));

                try
                {
                    var prompt = new SelectionPrompt<DeployTarget>("Pick target", false, target => target.Name);
                    prompt.AddSeparator("Internal");
                    prompt.Add(new DeployTarget("Development", "dev"));
                    prompt.AddDisabled(new DeployTarget("Production", "prod"), "approval required");
                    prompt.Add(new DeployTarget("Staging", "staging"));

                    SelectionResult<DeployTarget> result = prompt.RenderSelection();

                    Assert.True(result.HasSelection);
                    Assert.Equal(2, result.Index);
                    Assert.Equal("staging", result.SelectedItem.Slug);
                    VisualAssert.WroteText(scope.Driver, "disabled: approval required");
                    VisualAssert.WroteText(scope.Driver, "Option unavailable: approval required");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<RoutineInput>()));
                }
            }
        }

        /// <summary>
        /// Disabled options are rendered but cannot be selected.
        /// </summary>
        [Fact]
        public void Render_DisabledOptionRejectsSelectionAndRepeats()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    new RoutineInput { Value = "2" },
                    new RoutineInput { Value = "1" }
                }));

                try
                {
                    var prompt = new SelectionPrompt("Pick deployment action");
                    prompt.Add("Preview", ConsoleColor.Green);
                    prompt.AddDisabled("Deploy", "approval required");

                    PromptResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(0, result.Index);
                    Assert.True(prompt.Options.ElementAt(1).IsDisabled);
                    Assert.False(prompt.Options.ElementAt(1).Selected);
                    VisualAssert.WroteText(scope.Driver, "Deploy");
                    VisualAssert.WroteText(scope.Driver, "disabled: approval required");
                    VisualAssert.WroteText(scope.Driver, "Option unavailable: approval required");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.DarkGray);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<RoutineInput>()));
                }
            }
        }

        /// <summary>
        /// Shortcut input selects the configured option without requiring its numeric index.
        /// </summary>
        [Fact]
        public void Render_ShortcutInputSelectsOption()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    new RoutineInput { Value = "d" }
                }));

                try
                {
                    var prompt = new SelectionPrompt("Pick deployment action");
                    prompt.AddShortcut("Preview", "p", ConsoleColor.Cyan);
                    prompt.AddDescribedShortcut("Deploy", "Deploys the selected target.", "d", ConsoleColor.Green);

                    PromptResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(1, result.Index);
                    VisualAssert.WroteText(scope.Driver, "[d]");
                    VisualAssert.WroteText(scope.Driver, "Deploys the selected target.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<RoutineInput>()));
                }
            }
        }

        /// <summary>
        /// Shortcut input honors disabled option reasons and repeats the prompt.
        /// </summary>
        [Fact]
        public void Render_DisabledShortcutRejectsSelectionAndRepeats()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    new RoutineInput { Value = "d" },
                    new RoutineInput { Value = "p" }
                }));

                try
                {
                    var prompt = new SelectionPrompt("Pick deployment action");
                    prompt.AddShortcut("Preview", "p", ConsoleColor.Cyan);
                    prompt.AddDisabledShortcut("Deploy", "d", "approval required");

                    PromptResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(0, result.Index);
                    VisualAssert.WroteText(scope.Driver, "[d]");
                    VisualAssert.WroteText(scope.Driver, "Option unavailable: approval required");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<RoutineInput>()));
                }
            }
        }

        /// <summary>
        /// Keyboard-enabled prompts can select configured shortcuts directly.
        /// </summary>
        [Fact]
        public void Render_KeyboardShortcutSelectsOption()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(Array.Empty<RoutineInput>()));
                scope.Driver.EnqueueKey(new ConsoleKeyInfo('d', ConsoleKey.D, false, false, false));

                try
                {
                    var prompt = new SelectionPrompt("Pick deployment action")
                    {
                        KeyboardNavigationEnabled = true,
                        FilterEnabled = true
                    };
                    prompt.AddShortcut("Preview", "p", ConsoleColor.Cyan);
                    prompt.AddShortcut("Deploy", "d", ConsoleColor.Green);

                    PromptResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(1, result.Index);
                    VisualAssert.WroteText(scope.Driver, "[d]");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<RoutineInput>()));
                }
            }
        }

        /// <summary>
        /// Disabled default options are ignored when the user presses Enter.
        /// </summary>
        [Fact]
        public void Render_DisabledDefaultIsIgnored()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    new RoutineInput { Value = string.Empty },
                    new RoutineInput { Value = "1" }
                }));

                try
                {
                    var disabledDefault = new SelectOption("Deploy", isDefault: true).Disable("approval required");
                    var prompt = new SelectionPrompt("Pick deployment action", false, new SelectOption("Preview"), disabledDefault);

                    PromptResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(0, result.Index);
                    VisualAssert.WroteText(scope.Driver, "Invalid selection!");
                    VisualAssert.WroteText(scope.Driver, "disabled: approval required");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<RoutineInput>()));
                }
            }
        }

        /// <summary>
        /// Separators group options visually without consuming selection indexes.
        /// </summary>
        [Fact]
        public void Render_SeparatorsDoNotConsumeSelectionIndexes()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    new RoutineInput { Value = "2" }
                }));

                try
                {
                    var prompt = new SelectionPrompt("Pick deployment action");
                    prompt.AddSeparator("Safe actions");
                    prompt.Add("Preview", ConsoleColor.Green);
                    prompt.AddSeparator("Release actions");
                    prompt.Add("Deploy", ConsoleColor.Yellow);

                    PromptResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(1, result.Index);
                    Assert.Equal(2, prompt.Count);
                    Assert.Equal("Deploy", prompt[1]);
                    VisualAssert.WroteText(scope.Driver, "-- Safe actions --");
                    VisualAssert.WroteText(scope.Driver, "-- Release actions --");
                    Assert.DoesNotContain(scope.Driver.Lines, line => line.Contains("1) Safe actions"));
                    Assert.DoesNotContain(scope.Driver.Lines, line => line.Contains("2) Release actions"));
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<RoutineInput>()));
                }
            }
        }

        /// <summary>
        /// Paged selection prompts keep long menus bounded and allow next-page navigation.
        /// </summary>
        [Fact]
        public void Render_PageSize_NextCommandShowsNextPageAndSelectsGlobalIndex()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    new RoutineInput { Value = "next" },
                    new RoutineInput { Value = "5" }
                }));

                try
                {
                    var prompt = new SelectionPrompt("Pick deployment action", clear: true)
                    {
                        PageSize = 3
                    };
                    prompt.AddSeparator("Safe actions");
                    prompt.Add("Preview");
                    prompt.Add("Plan");
                    prompt.Add("Validate");
                    prompt.AddSeparator("Release actions");
                    prompt.Add("Deploy");
                    prompt.Add("Promote");
                    prompt.Add("Archive");

                    PromptResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(4, result.Index);
                    VisualAssert.ContainsText(scope.Driver, "4) Deploy");
                    VisualAssert.ContainsText(scope.Driver, "5) Promote");
                    VisualAssert.ContainsText(scope.Driver, "Showing options 4-6 of 6");
                    Assert.DoesNotContain("1) Preview", scope.Driver.Text);
                    Assert.DoesNotContain("2) Plan", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<RoutineInput>()));
                }
            }
        }

        /// <summary>
        /// Filter-enabled prompts narrow long menus while preserving original option indexes.
        /// </summary>
        [Fact]
        public void Render_FilterCommandNarrowsVisibleOptionsAndSelectsStableIndex()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    new RoutineInput { Value = "/deploy" },
                    new RoutineInput { Value = "4" }
                }));

                try
                {
                    var prompt = new SelectionPrompt("Pick deployment action", clear: true)
                    {
                        FilterEnabled = true
                    };
                    prompt.AddSeparator("Plan");
                    prompt.Add("Preview");
                    prompt.Add("Validate");
                    prompt.Add("Deploy database");
                    prompt.AddSeparator("Release");
                    prompt.Add("Deploy API");
                    prompt.Add("Promote");

                    PromptResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(3, result.Index);
                    VisualAssert.ContainsText(scope.Driver, "4) Deploy API");
                    VisualAssert.ContainsText(scope.Driver, "Filter: deploy (2 matches)");
                    Assert.DoesNotContain("1) Preview", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<RoutineInput>()));
                }
            }
        }

        /// <summary>
        /// Filter clear commands restore the full option list before selection.
        /// </summary>
        [Fact]
        public void Render_FilterClearRestoresFullOptionList()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    new RoutineInput { Value = "/deploy" },
                    new RoutineInput { Value = "/clear" },
                    new RoutineInput { Value = "1" }
                }));

                try
                {
                    var prompt = new SelectionPrompt("Pick deployment action", clear: true)
                    {
                        FilterEnabled = true
                    };
                    prompt.Add("Preview");
                    prompt.Add("Validate");
                    prompt.Add("Deploy API");

                    PromptResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(0, result.Index);
                    VisualAssert.ContainsText(scope.Driver, "1) Preview");
                    VisualAssert.ContainsText(scope.Driver, "Type /text to filter options");
                    Assert.DoesNotContain("Filter: deploy", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<RoutineInput>()));
                }
            }
        }

        /// <summary>
        /// Keyboard-enabled prompts let users move with arrow keys and confirm the highlighted option.
        /// </summary>
        [Fact]
        public void Render_KeyboardNavigation_SelectsHighlightedOption()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                scope.Driver.EnqueueKey(Key(ConsoleKey.DownArrow));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Enter, '\r'));

                var prompt = new SelectionPrompt("Pick deployment action", clear: true)
                {
                    KeyboardNavigationEnabled = true,
                    KeyboardSelectionColor = ConsoleColor.Cyan
                };
                prompt.Add("Preview");
                prompt.Add("Deploy");
                prompt.Add("Promote");

                PromptResult result = prompt.Render();

                Assert.True(result.HasSelection);
                Assert.Equal(1, result.Index);
                Assert.True(prompt.Options.ElementAt(1).Selected);
                VisualAssert.ContainsText(scope.Driver, "> 2) Deploy");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("> 2) Deploy") && write.Color == ConsoleColor.Cyan);
                Assert.True(scope.Driver.CursorVisible);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Keyboard navigation skips disabled options while still rendering them as unavailable.
        /// </summary>
        [Fact]
        public void Render_KeyboardNavigation_SkipsDisabledOptions()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                scope.Driver.EnqueueKey(Key(ConsoleKey.DownArrow));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Enter, '\r'));

                var prompt = new SelectionPrompt("Pick deployment action", clear: true)
                {
                    KeyboardNavigationEnabled = true
                };
                prompt.Add("Preview");
                prompt.AddDisabled("Deploy", "approval required");
                prompt.Add("Promote");

                PromptResult result = prompt.Render();

                Assert.True(result.HasSelection);
                Assert.Equal(2, result.Index);
                Assert.False(prompt.Options.ElementAt(1).Selected);
                VisualAssert.ContainsText(scope.Driver, "> 3) Promote");
                VisualAssert.ContainsText(scope.Driver, "disabled: approval required");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Keyboard prompts can type-to-filter before confirming the first matching option.
        /// </summary>
        [Fact]
        public void Render_KeyboardNavigation_FilterTextNarrowsOptions()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                scope.Driver.EnqueueKey(Key(ConsoleKey.D, 'd'));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Enter, '\r'));

                var prompt = new SelectionPrompt("Pick deployment action", clear: true)
                {
                    KeyboardNavigationEnabled = true,
                    FilterEnabled = true
                };
                prompt.Add("Preview");
                prompt.Add("Deploy API");
                prompt.Add("Deploy database");
                prompt.Add("Promote");

                PromptResult result = prompt.Render();

                Assert.True(result.HasSelection);
                Assert.Equal(1, result.Index);
                VisualAssert.ContainsText(scope.Driver, "> 2) Deploy API");
                VisualAssert.ContainsText(scope.Driver, "Filter: d (2 matches)");
                Assert.DoesNotContain("1) Preview", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Static SelectOption rendering maps selectable indexes back to the selected option when separators are present.
        /// </summary>
        [Fact]
        public void StaticRender_SeparatorsReturnSelectedOption()
        {
            using (new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    new RoutineInput { Value = "2" }
                }));

                try
                {
                    SelectOption selected = SelectionPrompt.Render(
                        "Pick deployment action",
                        false,
                        SelectOption.Separator("Safe actions"),
                        new SelectOption("Preview"),
                        SelectOption.Separator("Release actions"),
                        new SelectOption("Deploy"));

                    Assert.NotNull(selected);
                    Assert.Equal("Deploy", selected.Label);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<RoutineInput>()));
                }
            }
        }

        /// <summary>
        /// Described selection options render helper text and can be selected normally.
        /// </summary>
        [Fact]
        public void Render_DescribedOptionsRenderHelperText()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    new RoutineInput { Value = "1" }
                }));

                try
                {
                    var prompt = new SelectionPrompt("Pick deployment action");
                    prompt.AddDescribed(
                        "Preview",
                        "Runs a dry run before any release state is changed.",
                        ConsoleColor.Green);
                    prompt.Add("Exit");

                    PromptResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(0, result.Index);
                    VisualAssert.WroteText(scope.Driver, "Preview");
                    VisualAssert.WroteText(scope.Driver, "Runs a dry run");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Runs a dry run") && write.Color == RenderOptions.SubnoteColor);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<RoutineInput>()));
                }
            }
        }

        /// <summary>
        /// Filtering matches option descriptions as well as labels.
        /// </summary>
        [Fact]
        public void Render_FilterCommandMatchesDescriptionText()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    new RoutineInput { Value = "/approval" },
                    new RoutineInput { Value = "2" }
                }));

                try
                {
                    var prompt = new SelectionPrompt("Pick deployment action", clear: true)
                    {
                        FilterEnabled = true
                    };
                    prompt.Add("Preview");
                    prompt.AddDescribed("Deploy", "Requires approval before production release.", ConsoleColor.Green);
                    prompt.Add("Report");

                    PromptResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(1, result.Index);
                    VisualAssert.ContainsText(scope.Driver, "2) Deploy");
                    VisualAssert.ContainsText(scope.Driver, "Requires approval");
                    VisualAssert.ContainsText(scope.Driver, "Filter: approval (1 matches)");
                    Assert.DoesNotContain("1) Preview", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<RoutineInput>()));
                }
            }
        }

        /// <summary>
        /// Filtering can match hidden search terms without rendering those terms as option text.
        /// </summary>
        [Fact]
        public void Render_FilterCommandMatchesHiddenSearchTerms()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    new RoutineInput { Value = "/ship" },
                    new RoutineInput { Value = "2" }
                }));

                try
                {
                    var prompt = new SelectionPrompt("Pick deployment action", clear: true)
                    {
                        FilterEnabled = true
                    };
                    prompt.Add("Preview");
                    int deployIndex = prompt.Add("Deploy", ConsoleColor.Green);
                    prompt.AddSearchTerms(deployIndex, "ship", "release");
                    prompt.Add("Report");

                    PromptResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(1, result.Index);
                    VisualAssert.ContainsText(scope.Driver, "2) Deploy");
                    VisualAssert.ContainsText(scope.Driver, "Filter: ship (1 matches)");
                    Assert.DoesNotContain("1) Preview", scope.Driver.Text);
                    Assert.DoesNotContain("ship", scope.Driver.Text.Replace("Filter: ship (1 matches)", string.Empty));
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<RoutineInput>()));
                }
            }
        }

        /// <summary>
        /// Typed prompts can derive helper text from the underlying domain object.
        /// </summary>
        [Fact]
        public void SelectionPromptTyped_WithDescriptions_RendersDomainHelperText()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    new RoutineInput { Value = "2" }
                }));

                try
                {
                    var prompt = new SelectionPrompt<DeployTarget>("Pick target", false, target => target.Name)
                        .WithDescriptions(target => "Deploy slug: " + target.Slug);
                    prompt.Add(new DeployTarget("Development", "dev"));
                    prompt.Add(new DeployTarget("Production", "prod"));

                    SelectionResult<DeployTarget> result = prompt.RenderSelection();

                    Assert.True(result.HasSelection);
                    Assert.Equal("prod", result.SelectedItem.Slug);
                    VisualAssert.WroteText(scope.Driver, "Deploy slug: dev");
                    VisualAssert.WroteText(scope.Driver, "Deploy slug: prod");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<RoutineInput>()));
                }
            }
        }

        /// <summary>
        /// Typed prompts can derive hidden filter aliases from the underlying domain object.
        /// </summary>
        [Fact]
        public void SelectionPromptTyped_WithSearchTerms_FiltersByDomainAliases()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    new RoutineInput { Value = "/prod" },
                    new RoutineInput { Value = "2" }
                }));

                try
                {
                    var prompt = new SelectionPrompt<DeployTarget>("Pick target", true, target => target.Name)
                    {
                        FilterEnabled = true
                    }.WithSearchTerms(target => new[] { target.Slug });
                    prompt.Add(new DeployTarget("Development", "dev"));
                    prompt.Add(new DeployTarget("Production", "prod"));

                    SelectionResult<DeployTarget> result = prompt.RenderSelection();

                    Assert.True(result.HasSelection);
                    Assert.Equal("prod", result.SelectedItem.Slug);
                    VisualAssert.ContainsText(scope.Driver, "2) Production");
                    VisualAssert.ContainsText(scope.Driver, "Filter: prod (1 matches)");
                    Assert.DoesNotContain("1) Development", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<RoutineInput>()));
                }
            }
        }

        /// <summary>
        /// Typed prompt metadata selectors refresh choices that were already added.
        /// </summary>
        [Fact]
        public void SelectionPromptTyped_MetadataSelectorsApplyToExistingChoices()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    new RoutineInput { Value = "/prod" },
                    new RoutineInput { Value = "2" }
                }));

                try
                {
                    var prompt = new SelectionPrompt<DeployTarget>("Pick target", true, target => target.Name)
                    {
                        FilterEnabled = true
                    };
                    prompt.Add(new DeployTarget("Development", "dev"));
                    prompt.Add(new DeployTarget("Production", "prod"));

                    prompt
                        .WithDescriptions(target => "Deploy slug: " + target.Slug)
                        .WithSearchTerms(target => new[] { target.Slug });

                    SelectionResult<DeployTarget> result = prompt.RenderSelection();

                    Assert.True(result.HasSelection);
                    Assert.Equal("prod", result.SelectedItem.Slug);
                    VisualAssert.WroteText(scope.Driver, "Deploy slug: prod");
                    VisualAssert.ContainsText(scope.Driver, "Filter: prod (1 matches)");
                    Assert.DoesNotContain("1) Development", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<RoutineInput>()));
                }
            }
        }

        /// <summary>
        /// Typed prompt converters refresh choices that were already added.
        /// </summary>
        [Fact]
        public void SelectionPromptTyped_ConverterAppliesToExistingChoices()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    new RoutineInput { Value = "2" }
                }));

                try
                {
                    var prompt = new SelectionPrompt<DeployTarget>("Pick target");
                    prompt.AddChoiceGroup(
                        new DeployTarget("Release", "release"),
                        new DeployTarget("Development", "dev"),
                        new DeployTarget("Production", "prod"));

                    prompt.UseConverter(target => target.Name);

                    SelectionResult<DeployTarget> result = prompt.RenderSelection();

                    Assert.True(result.HasSelection);
                    Assert.Equal("prod", result.SelectedItem.Slug);
                    VisualAssert.WroteText(scope.Driver, "-- Release --");
                    VisualAssert.WroteText(scope.Driver, "Production");
                    Assert.DoesNotContain("DeployTarget", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<RoutineInput>()));
                }
            }
        }

        private static ConsoleKeyInfo Key(ConsoleKey key, char keyChar = '\0')
        {
            return new ConsoleKeyInfo(keyChar, key, false, false, false);
        }

        private sealed class TestRoutine : Routine
        {
            public TestRoutine(IEnumerable<RoutineInput> inputs)
            {
                foreach (var input in inputs)
                {
                    Enqueue(input);
                }
            }
        }

        private sealed class DeployTarget
        {
            public DeployTarget(string name, string slug)
            {
                Name = name;
                Slug = slug;
            }

            public string Name { get; }

            public string Slug { get; }
        }
    }
}
