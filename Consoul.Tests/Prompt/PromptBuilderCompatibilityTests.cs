using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ConsoulLibrary;
using ConsoulLibrary.Color;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace Consoul.Tests.Prompt
{
    /// <summary>
    /// Tests Spectre-style prompt builder aliases over Consoul prompt types.
    /// </summary>
    public class PromptBuilderCompatibilityTests
    {
        /// <summary>
        /// Parameterless string selection prompts can use Title and AddChoices aliases.
        /// </summary>
        [Fact]
        public void SelectionPrompt_StringBuilderAliases_SelectChoice()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var prompt = new SelectionPrompt<string>()
                        .Title("Choose environment")
                        .AddChoices("Development", "Production");

                    SelectionResult<string> result = AnsiConsole.Prompt(prompt);

                    Assert.True(result.HasSelection);
                    Assert.Equal("Production", result.SelectedItem);
                    VisualAssert.ContainsText(scope.Driver, "Choose environment");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Prompt result objects can be assigned or enumerated in common Spectre-style value snippets.
        /// </summary>
        [Fact]
        public void PromptResults_ImplicitConversionsAndEnumeration_SupportSpectreStyleAssignments()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2", "8080", "yes", "token-value", "1,2" }));

                try
                {
                    var development = new DeployTarget("Development", "dev");
                    var production = new DeployTarget("Production", "prod");

                    DeployTarget selectedTarget = AnsiConsole.Prompt(new SelectionPrompt<DeployTarget>()
                        .Title("Target")
                        .UseConverter(target => target.Name)
                        .AddChoices(development, production));
                    int port = AnsiConsole.Ask<int>("Port");
                    bool deploy = AnsiConsole.Confirm("Deploy?");
                    string token = new SecretPrompt("Token").Render();
                    List<DeployTarget> selectedTargets = AnsiConsole.Prompt(new MultiSelectionPrompt<DeployTarget>()
                        .Title("Targets")
                        .UseConverter(target => target.Name)
                        .AddChoices(development, production));

                    PromptResult rawSelection = PromptResult.FromSelection(2);
                    int selectedIndex = rawSelection;
                    MultiSelectionResult rawSelections = MultiSelectionResult.FromSelections(new[] { 1, 3, 3 });
                    List<int> selectedIndexes = rawSelections;
                    var typedSelections = new MultiSelectionResult<DeployTarget>(
                        MultiSelectionResult.FromSelections(new[] { 0, 1 }),
                        new[] { development, production });

                    Assert.Equal("prod", selectedTarget.Slug);
                    Assert.Equal(8080, port);
                    Assert.True(deploy);
                    Assert.Equal("token-value", token);
                    Assert.Equal(new[] { "dev", "prod" }, selectedTargets.Select(target => target.Slug).ToArray());
                    Assert.Equal(2, selectedIndex);
                    Assert.Equal(new[] { 1, 3 }, selectedIndexes.ToArray());
                    Assert.Equal(2, rawSelections.Count);
                    Assert.Equal(3, rawSelections[1]);
                    Assert.Equal(2, typedSelections.Count);
                    Assert.Equal("prod", typedSelections[1].Slug);
                    Assert.Equal(new[] { "dev", "prod" }, typedSelections.Select(target => target.Slug).ToArray());
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Typed selection prompts use converters, search, and paging aliases while returning domain objects.
        /// </summary>
        [Fact]
        public void SelectionPrompt_TypedBuilderAliases_ReturnSelectedDomainObject()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "/prod", "2" }));

                try
                {
                    var prompt = new SelectionPrompt<DeployTarget>()
                        .Title("Choose target")
                        .UseConverter(target => target.Name)
                        .WithDescriptions(target => target.Slug)
                        .WithSearchTerms(target => new[] { target.Slug })
                        .EnableSearch()
                        .WithPageSize(1)
                        .WithMoreChoicesText("Page {3}/{4}")
                        .AddChoices(
                            new DeployTarget("Development", "dev"),
                            new DeployTarget("Production", "prod"));

                    SelectionResult<DeployTarget> result = AnsiConsole.Prompt(prompt);

                    Assert.True(result.HasSelection);
                    Assert.Equal("prod", result.SelectedItem.Slug);
                    VisualAssert.ContainsText(scope.Driver, "Filter: prod (1 matches)");
                    VisualAssert.ContainsText(scope.Driver, "Production");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Selection and multi-selection prompts expose Spectre-shaped aliases for invalid input copy.
        /// </summary>
        [Fact]
        public void SelectionPrompt_InvalidSelectionAliases_RenderCustomCopy()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "9", "1", "9", "1" }));

                try
                {
                    PromptResult selection = new SelectionPrompt("Choose task", false, "Build")
                        .InvalidSelectionMessage("Choose a listed task.")
                        .Render();

                    MultiSelectionResult multiSelection = new MultiSelectionPrompt("Choose tasks", false, false, "Build")
                        .InvalidChoiceMessage("Choose listed task numbers.")
                        .Render();

                    Assert.True(selection.HasSelection);
                    Assert.True(multiSelection.HasSelection);
                    VisualAssert.WroteText(scope.Driver, "Choose a listed task.");
                    VisualAssert.WroteText(scope.Driver, "Choose listed task numbers.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Typed multi-selection prompts use the same builder aliases and map selected indexes back to items.
        /// </summary>
        [Fact]
        public void MultiSelectionPrompt_TypedBuilderAliases_ReturnSelectedDomainObjects()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "1,3" }));

                try
                {
                    var prompt = new MultiSelectionPrompt<DeployTarget>()
                        .Title("Choose targets")
                        .UseConverter(target => target.Name)
                        .WithDescriptions(target => target.Slug)
                        .EnableSearch()
                        .WithPageSize(2)
                        .AllowEmptySelection()
                        .AddChoices(
                            new DeployTarget("Development", "dev"),
                            new DeployTarget("Staging", "stage"),
                            new DeployTarget("Production", "prod"));

                    MultiSelectionResult<DeployTarget> result = AnsiConsole.Prompt(prompt);

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { "dev", "prod" }, result.SelectedItems.Select(target => target.Slug).ToArray());
                    VisualAssert.ContainsText(scope.Driver, "Choose targets");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Typed selection prompts can add Spectre-style choice groups without changing selectable indexes.
        /// </summary>
        [Fact]
        public void SelectionPrompt_TypedChoiceGroups_RenderSeparatorsAndSelectDomainObject()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "3" }));

                try
                {
                    var safe = new DeployTarget("Safe migrations", "safe");
                    var restricted = new DeployTarget("Restricted migrations", "restricted");
                    var prompt = new SelectionPrompt<DeployTarget>()
                        .Title("Choose migration")
                        .UseConverter(target => target.Name)
                        .AddChoiceGroup(
                            safe,
                            new DeployTarget("Preview", "preview"),
                            new DeployTarget("Validate", "validate"))
                        .AddChoiceGroup(
                            restricted,
                            new DeployTarget("Deploy", "deploy"));

                    SelectionResult<DeployTarget> result = AnsiConsole.Prompt(prompt);

                    Assert.True(result.HasSelection);
                    Assert.Equal(2, result.Index);
                    Assert.Equal("deploy", result.SelectedItem.Slug);
                    VisualAssert.WroteText(scope.Driver, "-- Safe migrations --");
                    VisualAssert.WroteText(scope.Driver, "-- Restricted migrations --");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Multi-selection prompts can add Spectre-style choice groups without making headings selectable.
        /// </summary>
        [Fact]
        public void MultiSelectionPrompt_ChoiceGroups_SelectOnlyGroupedChoices()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "1,3" }));

                try
                {
                    var prompt = new MultiSelectionPrompt()
                        .Title("Pick checks")
                        .AddChoiceGroup("Build", "Restore", "Compile")
                        .AddChoiceGroup("Release", "Pack", "Publish");

                    MultiSelectionResult result = AnsiConsole.Prompt(prompt);

                    Assert.False(result.IsCanceled);
                    Assert.Equal(new[] { 0, 2 }, result.Indexes.ToArray());
                    VisualAssert.WroteText(scope.Driver, "-- Build --");
                    VisualAssert.WroteText(scope.Driver, "-- Release --");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Structured single-selection groups render as parent/child choices and leaf mode requires selecting a child.
        /// </summary>
        [Fact]
        public void SelectionPrompt_GroupedChoices_LeafModeRequiresChildSelection()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "1", "2" }));

                try
                {
                    var prompt = new SelectionPrompt("Choose task");
                    SelectOption group = prompt.AddGroup("Build");
                    group.AddChild("Compile");

                    PromptResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(1, result.Index);
                    VisualAssert.WroteText(scope.Driver, "1) Build");
                    VisualAssert.WroteText(scope.Driver, "  2) Compile");
                    VisualAssert.WroteText(scope.Driver, "Option unavailable: Choose one of the group's child options.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Structured single-selection groups can be selected directly in independent mode.
        /// </summary>
        [Fact]
        public void SelectionPrompt_GroupedChoices_IndependentModeCanSelectGroup()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "1" }));

                try
                {
                    var prompt = new SelectionPrompt("Choose task")
                    {
                        Mode = SelectionMode.Independent
                    };
                    prompt.AddGroup("Build").AddChild("Compile");

                    PromptResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(0, result.Index);
                    VisualAssert.WroteText(scope.Driver, "1) Build");
                    VisualAssert.WroteText(scope.Driver, "  2) Compile");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Structured multi-selection groups expand to leaf children when leaf mode is active.
        /// </summary>
        [Fact]
        public void MultiSelectionPrompt_GroupedChoices_LeafModeGroupInputSelectsChildren()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "1" }));

                try
                {
                    var prompt = new MultiSelectionPrompt("Pick tasks");
                    SelectOption group = prompt.AddGroup("Build");
                    group.AddChild("Restore");
                    group.AddChild("Compile");

                    MultiSelectionResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 1, 2 }, result.Indexes.ToArray());
                    VisualAssert.WroteText(scope.Driver, "[ ] - 1) Build");
                    VisualAssert.WroteText(scope.Driver, "2) Restore");
                    VisualAssert.WroteText(scope.Driver, "3) Compile");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Structured multi-selection groups can be returned as independent selections.
        /// </summary>
        [Fact]
        public void MultiSelectionPrompt_GroupedChoices_IndependentModeCanSelectGroup()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "1" }));

                try
                {
                    var prompt = new MultiSelectionPrompt("Pick tasks")
                    {
                        Mode = SelectionMode.Independent
                    };
                    prompt.AddGroup("Build").AddChild("Compile");

                    MultiSelectionResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 0 }, result.Indexes.ToArray());
                    VisualAssert.WroteText(scope.Driver, "[ ] - 1) Build");
                    VisualAssert.WroteText(scope.Driver, "2) Compile");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Text prompts expose builder aliases for title, default values, styling, custom parsing, and parse error text.
        /// </summary>
        [Fact]
        public void TextPrompt_BuilderAliases_ParseAndReturnDefaultValue()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "bad", string.Empty }));

                try
                {
                    TextPromptResult<int> result = new TextPrompt<int>("Ignored")
                        .Title("Retry count")
                        .PromptStyle("green")
                        .InvalidStyle("maroon")
                        .DefaultValue(4)
                        .ValidationErrorMessage("Enter a whole number.")
                        .UseParser((string text, out int value) => int.TryParse(text, out value))
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal(4, result.Value);
                    VisualAssert.WroteText(scope.Driver, "Enter a whole number.");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Retry count") && write.Color == ConsoleColor.Green);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Enter a whole number.") && write.Color == ConsoleColor.DarkRed);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Text prompt extension aliases accept Spectre-style choices, choice display, and empty-input method syntax.
        /// </summary>
        [Fact]
        public void TextPrompt_ChoiceAndEmptyAliases_ReturnResultFirstValues()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "qa", string.Empty, string.Empty }));

                try
                {
                    TextPromptResult<string> environment = new TextPrompt<string>("Environment")
                        .Choices("dev", "qa", "prod")
                        .ChoicesSeparator(" | ")
                        .ShowChoices()
                        .Render();

                    TextPromptResult<string> optionalLabel = new TextPrompt<string>("Optional label")
                        .AllowEmpty()
                        .HideChoices()
                        .Render();

                    TextPromptResult<string> optionalOwner = new TextPrompt<string>("Optional owner")
                        .NotRequired()
                        .Render();

                    Assert.True(environment.HasValue);
                    Assert.Equal("qa", environment.Value);
                    Assert.True(optionalLabel.HasValue);
                    Assert.Equal(string.Empty, optionalLabel.Value);
                    Assert.True(optionalOwner.HasValue);
                    Assert.Equal(string.Empty, optionalOwner.Value);
                    VisualAssert.WroteText(scope.Driver, "Environment (dev | qa | prod)");
                    VisualAssert.WroteText(scope.Driver, "Optional label");
                    VisualAssert.WroteText(scope.Driver, "Optional owner");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Text prompts can add accepted choices incrementally using Spectre-style instance methods.
        /// </summary>
        [Fact]
        public void TextPrompt_AddChoiceAliases_AddAcceptedChoicesIncrementally()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "Production" }));

                try
                {
                    var development = new DeployTarget("Development", "dev");
                    var staging = new DeployTarget("Staging", "stage");
                    var production = new DeployTarget("Production", "prod");

                    TextPromptResult<DeployTarget> result = new TextPrompt<DeployTarget>("Target")
                        .UseConverter(target => target.Name)
                        .AddChoice(development)
                        .AddChoices(staging, production)
                        .ChoicesSeparator(" | ")
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal("prod", result.Value.Slug);
                    VisualAssert.WroteText(scope.Driver, "Target (Development | Staging | Production)");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Text prompts use converters for displayed choices, default hints, validation copy, and accepted choice lookup.
        /// </summary>
        [Fact]
        public void TextPrompt_ConverterAndHintStyleAliases_SelectDomainObjectFromDisplayText()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "Production" }));

                try
                {
                    var development = new DeployTarget("Development", "dev");
                    var production = new DeployTarget("Production", "prod");

                    TextPromptResult<DeployTarget> result = new TextPrompt<DeployTarget>("Target")
                        .UseConverter(target => target.Name)
                        .Choices(development, production)
                        .ChoicesSeparator(" | ")
                        .DefaultValue(production)
                        .ChoicesStyle("aqua")
                        .DefaultValueStyle("green")
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal("prod", result.Value.Slug);
                    VisualAssert.WroteText(scope.Driver, "Development | Production");
                    VisualAssert.WroteText(scope.Driver, "default: Production");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Development | Production") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("default: Production") && write.Color == ConsoleColor.Green);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Text prompts can hide default-value hints while preserving Enter-to-accept behavior.
        /// </summary>
        [Fact]
        public void TextPrompt_HideDefaultValueAlias_KeepsDefaultSelection()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                try
                {
                    TextPromptResult<string> result = new TextPrompt<string>("Environment")
                        .DefaultValue("prod")
                        .HideDefaultValue()
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal("prod", result.Value);
                    VisualAssert.WroteText(scope.Driver, "Environment");
                    Assert.DoesNotContain("default: prod", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Text prompts can clear their rendered prompt line after successful input.
        /// </summary>
        [Fact]
        public void TextPrompt_ClearOnFinishAlias_ClearsWrappedPromptAfterSuccessfulInput()
        {
            using (var scope = new ConsoleDriverScope(24))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "prod" }));

                try
                {
                    TextPromptResult<string> result = new TextPrompt<string>("Transient deployment target")
                        .Choices("dev", "prod")
                        .ClearOnFinish()
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal("prod", result.Value);
                    VisualAssert.WroteText(scope.Driver, "Transient deployment target");
                    Assert.True(scope.Driver.ClearLineCount >= 2);
                    Assert.DoesNotContain("Transient", scope.Driver.Text);
                    Assert.DoesNotContain("deployment", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Text prompts can use a constructor comparer to control accepted choice display-text matching.
        /// </summary>
        [Fact]
        public void TextPrompt_StringComparerConstructor_ControlsChoiceMatching()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "PROD", "prod" }));

                try
                {
                    TextPromptResult<string> result = new TextPrompt<string>("Environment", StringComparer.Ordinal)
                        .Choices("prod")
                        .InvalidChoiceMessage("Use exact casing.")
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal("prod", result.Value);
                    VisualAssert.WroteText(scope.Driver, "Use exact casing.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Text prompts can seed defaults into an editable keyboard input buffer.
        /// </summary>
        [Fact]
        public void TextPrompt_EditableDefaultValueAlias_AllowsKeyboardEditingDefault()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Backspace, '\b'));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Backspace, '\b'));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Backspace, '\b'));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Backspace, '\b'));
                EnqueueText(scope.Driver, "-new");
                scope.Driver.EnqueueKey(Key(ConsoleKey.Enter, '\r'));

                try
                {
                    TextPromptResult<string> result = new TextPrompt<string>("Branch")
                        .DefaultValue("api-old")
                        .EditableDefaultValue()
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal("api-new", result.Value);
                    VisualAssert.WroteText(scope.Driver, "api-old");
                    VisualAssert.WroteText(scope.Driver, "api-new");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Secret text prompts can seed editable defaults without writing the default value to the console.
        /// </summary>
        [Fact]
        public void TextPrompt_EditableDefaultValueWithSecret_MasksSeededDefault()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Enter, '\r'));

                try
                {
                    TextPromptResult<string> result = new TextPrompt<string>("Token")
                        .DefaultValue("secret")
                        .Secret('#')
                        .EditableDefaultValue()
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal("secret", result.Value);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("######"));
                    Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("secret"));
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Text prompts can read secret input while preserving typed parsing and masking routine-backed values.
        /// </summary>
        [Fact]
        public void TextPrompt_SecretAliases_MaskRoutineInputAndParseTypedValues()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "token-value", "42" }));

                try
                {
                    TextPromptResult<string> token = new TextPrompt<string>("Token")
                        .Secret()
                        .Mask('#')
                        .MaskStyle("aqua")
                        .Render();

                    TextPromptResult<int> pin = new TextPrompt<int>("PIN")
                        .Secret()
                        .Render();

                    Assert.True(token.HasValue);
                    Assert.Equal("token-value", token.Value);
                    Assert.True(pin.HasValue);
                    Assert.Equal(42, pin.Value);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("###########") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("**"));
                    Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("token-value"));
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Text prompts use configured culture for built-in parsing and default hint formatting.
        /// </summary>
        [Fact]
        public void TextPrompt_CultureAlias_ParsesAndFormatsWithConfiguredCulture()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty, "2,75" }));

                try
                {
                    CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");

                    TextPromptResult<decimal> defaultResult = new TextPrompt<decimal>("Default ratio")
                        .Culture(culture)
                        .DefaultValue(1.5m)
                        .Render();

                    TextPromptResult<decimal> parsedResult = new TextPrompt<decimal>("Ratio")
                        .Culture(culture)
                        .Render();

                    Assert.True(defaultResult.HasValue);
                    Assert.Equal(1.5m, defaultResult.Value);
                    Assert.True(parsedResult.HasValue);
                    Assert.Equal(2.75m, parsedResult.Value);
                    VisualAssert.WroteText(scope.Driver, "default: 1,5");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Text prompts with object choices render invalid choice copy when display text cannot map to a choice.
        /// </summary>
        [Fact]
        public void TextPrompt_InvalidChoiceMessageAlias_RendersForUnmatchedObjectChoice()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "Sandbox", "Production" }));

                try
                {
                    var development = new DeployTarget("Development", "dev");
                    var production = new DeployTarget("Production", "prod");

                    TextPromptResult<DeployTarget> result = new TextPrompt<DeployTarget>("Target")
                        .UseConverter(target => target.Name)
                        .Choices(development, production)
                        .InvalidChoiceMessage("Choose a listed target.")
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal("prod", result.Value.Slug);
                    VisualAssert.WroteText(scope.Driver, "Choose a listed target.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Text prompt not-empty aliases reuse the existing required-value validation path.
        /// </summary>
        [Fact]
        public void TextPrompt_NotEmptyAlias_RePromptsUntilValueIsPresent()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty, "service-owner" }));

                try
                {
                    TextPromptResult<string> result = new TextPrompt<string>("Service owner")
                        .NotEmpty("Owner is required.")
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal("service-owner", result.Value);
                    VisualAssert.WroteText(scope.Driver, "Owner is required.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Text prompt length aliases keep common POC form validation declarative.
        /// </summary>
        [Fact]
        public void TextPrompt_LengthAliases_RePromptUntilValueLengthIsValid()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[]
                {
                    "ab",
                    "abc",
                    "abcdef",
                    "short",
                    "xy",
                    "toolongvalue",
                    "valid"
                }));

                try
                {
                    TextPromptResult<string> minimum = new TextPrompt<string>("Service key")
                        .MinimumLength(3, "Use at least three characters.")
                        .Render();

                    TextPromptResult<string> maximum = new TextPrompt<string>("Code")
                        .MaximumLength(5, "Use at most five characters.")
                        .Render();

                    TextPromptResult<string> range = new TextPrompt<string>("Slug")
                        .LengthBetween(3, 6, "Use three to six characters.")
                        .Render();

                    Assert.True(minimum.HasValue);
                    Assert.Equal("abc", minimum.Value);
                    Assert.True(maximum.HasValue);
                    Assert.Equal("short", maximum.Value);
                    Assert.True(range.HasValue);
                    Assert.Equal("valid", range.Value);
                    VisualAssert.WroteText(scope.Driver, "Use at least three characters.");
                    VisualAssert.WroteText(scope.Driver, "Use at most five characters.");
                    VisualAssert.WroteText(scope.Driver, "Use three to six characters.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Confirmation prompts expose builder aliases for title, defaults, colors, and invalid input copy.
        /// </summary>
        [Fact]
        public void ConfirmPrompt_BuilderAliases_ReturnDefaultAfterInvalidInput()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "maybe", string.Empty }));

                try
                {
                    ConfirmResult result = new ConfirmPrompt("Ignored")
                        .Title("Deploy now?")
                        .Style("aqua")
                        .InvalidStyle("olive")
                        .DefaultValue(false)
                        .ValidationErrorMessage("Use yes or no.")
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.False(result.Value);
                    VisualAssert.WroteText(scope.Driver, "Use yes or no.");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy now? [y/N]") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Use yes or no.") && write.Color == ConsoleColor.DarkYellow);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Spectre-shaped confirmation prompts render through the generic prompt facade while returning Consoul results.
        /// </summary>
        [Fact]
        public void ConfirmationPrompt_SpectreAlias_ReturnsConfirmResultWithCustomChoices()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "later", "d" }));

                try
                {
                    ConfirmationPrompt prompt = new ConfirmationPrompt("Deploy now?")
                    {
                        DefaultValue = false,
                        Comparer = StringComparer.OrdinalIgnoreCase
                    }
                    .Yes('d')
                    .No('s')
                    .InvalidChoiceMessage("Choose d or s.")
                    .PromptStyle("aqua")
                    .InvalidStyle("olive")
                    .ShowDefaultValue()
                    .RequireEnter();

                    ConfirmResult result = AnsiConsole.Prompt(prompt);

                    Assert.True(result.HasValue);
                    Assert.True(result.Value);
                    VisualAssert.WroteText(scope.Driver, "Deploy now? [d/S]");
                    VisualAssert.WroteText(scope.Driver, "Choose d or s.");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy now? [d/S]") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Choose d or s.") && write.Color == ConsoleColor.DarkYellow);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Spectre-shaped confirmation prompts accept reusable styles for prompt and invalid-message colors.
        /// </summary>
        [Fact]
        public void ConfirmationPrompt_StyleObjects_ColorPromptAndInvalidMessages()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "maybe", "y" }));

                try
                {
                    ConfirmationPrompt prompt = new ConfirmationPrompt("Deploy now?")
                    {
                        DefaultValue = false
                    }
                    .Style(new Style(ConsoleColor.Magenta))
                    .InvalidStyle(new Style(ConsoleColor.Red))
                    .InvalidChoiceMessage("Use y or n.");

                    ConfirmResult result = AnsiConsole.Prompt(prompt);

                    Assert.True(result.HasValue);
                    Assert.True(result.Value);
                    Assert.Contains("Deploy now? [y/N]", scope.Driver.Text);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy now?") && write.Color == ConsoleColor.Magenta);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[y/N]") && write.Color == ConsoleColor.Magenta);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Use y or n.") && write.Color == ConsoleColor.Red);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Spectre-shaped confirmation prompts apply separate styles to default and non-default choices.
        /// </summary>
        [Fact]
        public void ConfirmationPrompt_ChoiceStyles_ColorDefaultAndNonDefaultChoices()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                try
                {
                    ConfirmationPrompt prompt = new ConfirmationPrompt("Deploy now?")
                    {
                        DefaultValue = false
                    }
                    .ChoicesStyle("blue")
                    .DefaultValueStyle("green");

                    ConfirmResult result = AnsiConsole.Prompt(prompt);

                    Assert.True(result.HasValue);
                    Assert.False(result.Value);
                    Assert.Contains("Deploy now? [y/N]", scope.Driver.Text);
                    Assert.Contains(scope.Driver.Writes, write => write.Text == "y" && write.Color == ConsoleColor.Blue);
                    Assert.Contains(scope.Driver.Writes, write => write.Text == "N" && write.Color == ConsoleColor.Green);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Secret prompts expose builder aliases for title, colors, masks, and required input copy.
        /// </summary>
        [Fact]
        public void SecretPrompt_BuilderAliases_RenderMaskAndCustomRequiredMessage()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty, "token-value" }));

                try
                {
                    SecretResult result = new SecretPrompt("Ignored")
                        .Title("API token")
                        .PromptStyle("blue")
                        .Mask('#')
                        .MaskStyle("navy")
                        .InvalidStyle("red")
                        .RequiredErrorMessage("Token is required.")
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal("token-value", result.Value);
                    VisualAssert.WroteText(scope.Driver, "Token is required.");
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("API token") && write.Color == ConsoleColor.Blue);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Token is required.") && write.Color == ConsoleColor.Red);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#") && write.Color == ConsoleColor.DarkBlue);
                    Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("token-value"));
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Secret prompts expose optional-input builder aliases without requiring real console input.
        /// </summary>
        [Fact]
        public void SecretPrompt_BuilderAliases_AllowEmptyAndNotRequiredReturnEmptySecret()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                scope.Driver.EnqueueKey(Key(ConsoleKey.Enter, '\r'));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Enter, '\r'));

                SecretResult optionalApiKey = new SecretPrompt("Optional API key")
                    .AllowEmpty()
                    .Render();

                SecretResult optionalToken = new SecretPrompt("Optional token")
                    .NotRequired()
                    .Render();

                Assert.True(optionalApiKey.HasValue);
                Assert.Equal(string.Empty, optionalApiKey.Value);
                Assert.True(optionalToken.HasValue);
                Assert.Equal(string.Empty, optionalToken.Value);
                VisualAssert.WroteText(scope.Driver, "Optional API key");
                VisualAssert.WroteText(scope.Driver, "Optional token");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// String selection prompts expose builder aliases for prompt copy, instruction copy, and colors.
        /// </summary>
        [Fact]
        public void SelectionPrompt_StringBuilderStyleAliases_RenderInstructionText()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "1" }));

                try
                {
                    var prompt = new SelectionPrompt()
                        .Title("Pick action")
                        .PromptStyle("fuchsia")
                        .InstructionsText("Pick by number.")
                        .InstructionStyle("purple")
                        .HighlightStyle("green")
                        .EnableSearch()
                        .AddChoices("Preview", "Deploy");

                    PromptResult result = AnsiConsole.Prompt(prompt);

                    Assert.True(result.HasSelection);
                    Assert.Equal(0, result.Index);
                    Assert.Equal(ConsoleColor.Green, prompt.KeyboardSelectionColor);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Pick action") && write.Color == ConsoleColor.Magenta);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Pick by number.") && write.Color == ConsoleColor.DarkMagenta);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Selection prompt filter helper aliases configure line-based filtering copy and prefix text.
        /// </summary>
        [Fact]
        public void SelectionPrompt_FilterHelperAliases_RenderCustomCopy()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "?none", "?clear", "1" }));

                try
                {
                    var prompt = new SelectionPrompt()
                        .Title("Pick action")
                        .EnableSearch()
                        .FilterCommandPrefix("?")
                        .FilterHelpText("Find with {0}term")
                        .ActiveFilterText("Filtering {0} ({1})")
                        .NoFilterMatchesText("No actions for {0}")
                        .AddChoices("Build", "Deploy");

                    PromptResult result = AnsiConsole.Prompt(prompt);

                    Assert.True(result.HasSelection);
                    Assert.Equal(0, result.Index);
                    VisualAssert.WroteText(scope.Driver, "Find with ?term");
                    VisualAssert.WroteText(scope.Driver, "Filtering none (0)");
                    VisualAssert.WroteText(scope.Driver, "No actions for none");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Selection prompt keyboard helper aliases configure keyboard navigation and filter copy.
        /// </summary>
        [Fact]
        public void SelectionPrompt_KeyboardHelperAliases_RenderCustomCopy()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Enter, '\r'));

                try
                {
                    var prompt = new SelectionPrompt()
                        .Title("Pick action")
                        .UseKeyboardNavigation()
                        .EnableSearch()
                        .KeyboardNavigationHelpText("Move, filter, accept, or cancel.")
                        .KeyboardFilterHelpText("Type to narrow actions.")
                        .AddChoices("Build", "Deploy");

                    PromptResult result = AnsiConsole.Prompt(prompt);

                    Assert.True(result.HasSelection);
                    Assert.Equal(0, result.Index);
                    VisualAssert.WroteText(scope.Driver, "Move, filter, accept, or cancel.");
                    VisualAssert.WroteText(scope.Driver, "Type to narrow actions.");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Multi-selection prompt filter helper aliases configure line-based filtering copy and prefix text.
        /// </summary>
        [Fact]
        public void MultiSelectionPrompt_FilterHelperAliases_RenderCustomCopy()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "@none", "@clear", "all" }));

                try
                {
                    var prompt = new MultiSelectionPrompt()
                        .Title("Pick checks")
                        .EnableSearch()
                        .FilterCommandPrefix("@")
                        .FilterHelpText("Filter with {0}term")
                        .ActiveFilterText("Checks filter {0} ({1})")
                        .NoFilterMatchesText("No checks for {0}")
                        .AddChoices("Build", "Deploy");

                    MultiSelectionResult result = AnsiConsole.Prompt(prompt);

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 0, 1 }, result.Indexes.ToArray());
                    VisualAssert.WroteText(scope.Driver, "Filter with @term");
                    VisualAssert.WroteText(scope.Driver, "Checks filter none (0)");
                    VisualAssert.WroteText(scope.Driver, "No checks for none");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Typed prompt helper aliases configure typed selection and multi-selection prompt properties.
        /// </summary>
        [Fact]
        public void TypedSelectionPrompt_HelperTextAliases_ConfigureProperties()
        {
            var selection = new SelectionPrompt<DeployTarget>()
                .FilterCommandPrefix("!")
                .FilterHelpText("Selection filter {0}")
                .ActiveFilterText("Selection active {0}")
                .NoFilterMatchesText("Selection none {0}")
                .KeyboardNavigationHelpText("Selection keys")
                .KeyboardFilterHelpText("Selection filter keys");

            var multiSelection = new MultiSelectionPrompt<DeployTarget>()
                .FilterCommandPrefix("~")
                .FilterHelpText("Multi filter {0}")
                .ActiveFilterText("Multi active {0}")
                .NoFilterMatchesText("Multi none {0}")
                .KeyboardNavigationHelpText("Multi keys")
                .KeyboardFilterHelpText("Multi filter keys");

            Assert.Equal("!", selection.FilterCommandPrefix);
            Assert.Equal("Selection filter {0}", selection.FilterHelpText);
            Assert.Equal("Selection active {0}", selection.ActiveFilterText);
            Assert.Equal("Selection none {0}", selection.NoFilterMatchesText);
            Assert.Equal("Selection keys", selection.KeyboardNavigationHelpText);
            Assert.Equal("Selection filter keys", selection.KeyboardFilterHelpText);
            Assert.Equal("~", multiSelection.FilterCommandPrefix);
            Assert.Equal("Multi filter {0}", multiSelection.FilterHelpText);
            Assert.Equal("Multi active {0}", multiSelection.ActiveFilterText);
            Assert.Equal("Multi none {0}", multiSelection.NoFilterMatchesText);
            Assert.Equal("Multi keys", multiSelection.KeyboardNavigationHelpText);
            Assert.Equal("Multi filter keys", multiSelection.KeyboardFilterHelpText);
        }

        /// <summary>
        /// Selection prompt choices can be styled directly through returned option objects.
        /// </summary>
        [Fact]
        public void SelectionPrompt_SelectOptionStyleHelpers_ColorChoicesAndDisabledChoices()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "1" }));

                try
                {
                    var prompt = new SelectionPrompt()
                        .Title("Pick action");
                    prompt.AddSeparator("Build").UseStyle("grey");
                    prompt.AddChoice("Preview").UseStyle(new Style(ConsoleColor.Cyan));
                    prompt.AddChoice("Deploy")
                        .UseDisabledStyle(new Style(ConsoleColor.Red))
                        .Disable("approval required");

                    PromptResult result = AnsiConsole.Prompt(prompt);

                    Assert.True(result.HasSelection);
                    Assert.Equal(0, result.Index);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Preview") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.Red);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Multi-selection prompt choices can be styled directly through returned option objects.
        /// </summary>
        [Fact]
        public void MultiSelectionPrompt_SelectOptionStyleHelpers_ColorChoices()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                try
                {
                    var prompt = new MultiSelectionPrompt()
                        .Title("Pick checks")
                        .NotRequired();
                    prompt.AddChoice("Build").UseStyle(new Style(ConsoleColor.Yellow));
                    prompt.AddChoice("Deploy").UseStyle("aqua");

                    MultiSelectionResult result = AnsiConsole.Prompt(prompt);

                    Assert.False(result.IsCanceled);
                    Assert.Empty(result.Indexes);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build") && write.Color == ConsoleColor.Yellow);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.Cyan);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Selection prompts can apply Spectre-shaped disabled choice styles without changing disabled behavior.
        /// </summary>
        [Fact]
        public void SelectionPrompt_DisabledStyleAlias_ColorsDisabledChoices()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "1" }));

                try
                {
                    var prompt = new SelectionPrompt()
                        .Title("Pick action")
                        .DisabledStyle("red");
                    prompt.Add("Preview");
                    prompt.AddDisabled("Deploy", "approval required");

                    PromptResult result = AnsiConsole.Prompt(prompt);

                    Assert.True(result.HasSelection);
                    Assert.Equal(0, result.Index);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.Red);
                    VisualAssert.WroteText(scope.Driver, "disabled: approval required");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Selection prompts can apply Spectre-shaped search highlight styles to visible filter matches.
        /// </summary>
        [Fact]
        public void SelectionPrompt_SearchHighlightStyleAlias_ColorsVisibleFilterMatches()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "/dep", "2" }));

                try
                {
                    var prompt = new SelectionPrompt()
                        .Title("Pick action")
                        .EnableSearch()
                        .SearchHighlightStyle("yellow")
                        .AddChoices("Preview", "Deploy API", "Promote");

                    PromptResult result = AnsiConsole.Prompt(prompt);

                    Assert.True(result.HasSelection);
                    Assert.Equal(1, result.Index);
                    Assert.Contains(scope.Driver.Writes, write => write.Text == "Dep" && write.Color == ConsoleColor.Yellow);
                    VisualAssert.WroteText(scope.Driver, "Filter: dep");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Multi-selection prompts can apply Spectre-shaped search highlight styles to visible filter matches.
        /// </summary>
        [Fact]
        public void MultiSelectionPrompt_SearchHighlightStyleAlias_ColorsVisibleFilterMatches()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "/dep", "all" }));

                try
                {
                    var prompt = new MultiSelectionPrompt()
                        .Title("Pick tasks")
                        .EnableSearch()
                        .SearchHighlightStyle("yellow")
                        .AddChoices("Build", "Deploy API", "Promote");

                    MultiSelectionResult result = AnsiConsole.Prompt(prompt);

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 1 }, result.Indexes.ToArray());
                    Assert.Contains(scope.Driver.Writes, write => write.Text == "Dep" && write.Color == ConsoleColor.Yellow);
                    VisualAssert.WroteText(scope.Driver, "Filter: dep");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Prompt palette extensions apply shared Color and RgbColor values to scalar prompt rendering.
        /// </summary>
        [Fact]
        public void PromptPaletteExtensions_ColorScalarPrompts()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "bad", "7", "maybe", "yes", string.Empty, "token-value", string.Empty }));

                try
                {
                    TextPromptResult<int> retryCount = new TextPrompt<int>("Retry count")
                        .PromptStyle(Color.Green)
                        .InvalidStyle(RgbColor.Red)
                        .UseParser((string text, out int value) => int.TryParse(text, out value))
                        .Render();

                    ConfirmResult deploy = new ConfirmPrompt("Deploy now?")
                        .Style(RgbColor.Cyan)
                        .InvalidStyle(Color.Yellow)
                        .DefaultValue(false)
                        .Render();

                    SecretResult token = new SecretPrompt("API token")
                        .PromptStyle(Color.Blue)
                        .Mask('#')
                        .MaskStyle(RgbColor.DarkBlue)
                        .InvalidStyle(Color.Red)
                        .RequiredErrorMessage("Token is required.")
                        .Render();

                    ConfirmResult continueResult = AnsiConsole.Prompt(new ConfirmationPrompt("Continue?")
                    {
                        DefaultValue = true
                    }
                    .PromptStyle(Color.Cyan)
                    .ChoicesStyle(Color.Magenta)
                    .DefaultValueStyle(RgbColor.Green)
                    .InvalidStyle(RgbColor.Red));

                    Assert.True(retryCount.HasValue);
                    Assert.Equal(7, retryCount.Value);
                    Assert.True(deploy.HasValue);
                    Assert.True(deploy.Value);
                    Assert.True(token.HasValue);
                    Assert.Equal("token-value", token.Value);
                    Assert.True(continueResult.HasValue);
                    Assert.True(continueResult.Value);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Retry count") && write.Color == ConsoleColor.Green);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Invalid input!") && write.Color == ConsoleColor.Red);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy now? [y/N]") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Please answer yes or no.") && write.Color == ConsoleColor.Yellow);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("API token") && write.Color == ConsoleColor.Blue);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#") && write.Color == ConsoleColor.DarkBlue);
                    Assert.Contains(scope.Driver.Writes, write => write.Text == "Y" && write.Color == ConsoleColor.Green);
                    Assert.Contains(scope.Driver.Writes, write => write.Text == "n" && write.Color == ConsoleColor.Magenta);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Prompt palette extensions preserve fluent chaining across selection, multi-selection, typed prompts, and select options.
        /// </summary>
        [Fact]
        public void PromptPaletteExtensions_ColorSelectionPromptSurface()
        {
            var prompt = new SelectionPrompt()
                .Title("Action")
                .PromptStyle(Color.Cyan)
                .InstructionStyle(RgbColor.DarkGray)
                .HighlightColor(Color.Yellow)
                .DisabledStyle(RgbColor.Red)
                .SearchHighlightStyle(Color.Green);

            SelectOption group = prompt.AddGroup("Build", Color.DarkGray);
            prompt.AddChild(group, "Compile", RgbColor.Cyan);
            prompt.Add("Preview", Color.Green);
            prompt.AddDescribed("Deploy", "Publishes the release.", RgbColor.Yellow);
            prompt.AddSeparator("Disabled", Color.DarkGray);
            prompt.AddDisabled("Archive", "approval required", RgbColor.Red);
            prompt.AddDisabledDescribed("Delete", "Removes old releases.", "locked", Color.Red);
            prompt.AddChoice("Inspect")
                .UseColor(Color.Cyan)
                .UseStyle(RgbColor.Green)
                .UseDisabledStyle(Color.Red)
                .AddChild("Nested", RgbColor.Yellow);

            var typedPrompt = new SelectionPrompt<DeployTarget>()
                .UseConverter(target => target.Name)
                .PromptStyle(Color.Magenta)
                .Style(RgbColor.Cyan)
                .InstructionStyle(Color.DarkGray)
                .HighlightStyle(RgbColor.Yellow)
                .DisabledStyle(Color.Red)
                .SearchHighlightStyle(RgbColor.Green);

            var development = new DeployTarget("Development", "dev");
            var production = new DeployTarget("Production", "prod");
            SelectOption typedGroup = typedPrompt.AddGroup(development, Color.DarkGray);
            typedPrompt.AddChild(typedGroup, production, RgbColor.Cyan);
            typedPrompt.Add(development, Color.Green);
            typedPrompt.AddDisabled(production, "approval required", RgbColor.Red);

            var multiPrompt = new MultiSelectionPrompt()
                .Title("Checks")
                .PromptStyle(Color.Yellow)
                .Style(RgbColor.Cyan)
                .InstructionStyle(Color.DarkGray)
                .HighlightColor(RgbColor.Green)
                .SearchHighlightStyle(Color.Magenta);

            SelectOption multiGroup = multiPrompt.AddGroup("Build", Color.DarkGray);
            multiPrompt.AddChild(multiGroup, "Compile", RgbColor.Cyan);
            multiPrompt.Add("Unit tests", Color.Green, isDefault: true);
            multiPrompt.AddDescribed("Deploy", "Runs deploy checks.", RgbColor.Yellow);
            multiPrompt.AddSeparator("Disabled", Color.DarkGray);
            multiPrompt.AddDisabled("Archive", "approval required", RgbColor.Red);
            multiPrompt.AddDisabledDescribed("Delete", "Removes old releases.", "locked", Color.Red);

            var typedMultiPrompt = new MultiSelectionPrompt<DeployTarget>()
                .UseConverter(target => target.Name)
                .PromptStyle(Color.Blue)
                .Style(RgbColor.Cyan)
                .InstructionStyle(Color.DarkGray)
                .HighlightStyle(RgbColor.Yellow)
                .SearchHighlightStyle(Color.Green);

            SelectOption typedMultiGroup = typedMultiPrompt.AddGroup(development, Color.DarkGray);
            typedMultiPrompt.AddChild(typedMultiGroup, production, RgbColor.Cyan);
            typedMultiPrompt.Add(development, Color.Green);
            typedMultiPrompt.AddDisabled(production, "approval required", RgbColor.Red);

            Assert.Equal(ConsoleColor.Cyan, prompt.PromptColor);
            Assert.Equal(ConsoleColor.DarkGray, prompt.InstructionColor);
            Assert.Equal(ConsoleColor.Yellow, prompt.KeyboardSelectionColor);
            Assert.Equal(ConsoleColor.Cyan, typedPrompt.PromptColor);
            Assert.Equal(ConsoleColor.DarkGray, typedPrompt.InstructionColor);
            Assert.Equal(ConsoleColor.Yellow, typedPrompt.KeyboardSelectionColor);
            Assert.Equal(ConsoleColor.Cyan, multiPrompt.PromptColor);
            Assert.Equal(ConsoleColor.DarkGray, multiPrompt.InstructionColor);
            Assert.Equal(ConsoleColor.Green, multiPrompt.KeyboardSelectionColor);
            Assert.Equal(ConsoleColor.Cyan, typedMultiPrompt.PromptColor);
            Assert.Equal(ConsoleColor.DarkGray, typedMultiPrompt.InstructionColor);
            Assert.Equal(ConsoleColor.Yellow, typedMultiPrompt.KeyboardSelectionColor);
            Assert.Contains(prompt.Options, option => option.Label == "Compile" && option.Color == ConsoleColor.Cyan);
            Assert.Contains(prompt.Options, option => option.Label == "Preview" && option.Color == ConsoleColor.Green);
            Assert.Contains(prompt.Options, option => option.Label == "Deploy" && option.Color == ConsoleColor.Yellow);
            Assert.Contains(prompt.Options, option => option.Label == "Archive" && option.Color == ConsoleColor.Red);
            Assert.Contains(multiPrompt.Options, option => option.Label == "Unit tests" && option.Color == ConsoleColor.Green);
            Assert.Contains(multiPrompt.Options, option => option.Label == "Deploy" && option.Color == ConsoleColor.Yellow);
            Assert.Contains(multiPrompt.Options, option => option.Label == "Archive" && option.Color == ConsoleColor.Red);
        }

        /// <summary>
        /// Multi-selection prompts expose aliases for required behavior and rendered instructions.
        /// </summary>
        [Fact]
        public void MultiSelectionPrompt_BuilderAliases_NotRequiredAllowsEmptySelection()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                try
                {
                    var prompt = new MultiSelectionPrompt()
                        .Title("Pick checks")
                        .PromptStyle("yellow")
                        .InstructionsText("Choose checks or press Enter.")
                        .InstructionStyle("olive")
                        .NotRequired()
                        .HighlightStyle("green")
                        .AddChoices("Build", "Test");

                    MultiSelectionResult result = AnsiConsole.Prompt(prompt);

                    Assert.False(result.IsCanceled);
                    Assert.Empty(result.Indexes);
                    Assert.Equal(ConsoleColor.Green, prompt.KeyboardSelectionColor);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Pick checks") && write.Color == ConsoleColor.Yellow);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Choose checks or press Enter.") && write.Color == ConsoleColor.DarkYellow);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Typed selection prompts can declare their default value before choices are added.
        /// </summary>
        [Fact]
        public void SelectionPrompt_TypedDefaultValueBeforeChoices_SelectsDefaultOnEmptyInput()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                try
                {
                    var development = new DeployTarget("Development", "dev");
                    var production = new DeployTarget("Production", "prod");
                    var prompt = new SelectionPrompt<DeployTarget>()
                        .Title("Choose target")
                        .UseConverter(target => target.Name)
                        .DefaultValue(production)
                        .AddChoices(development, production);

                    SelectionResult<DeployTarget> result = AnsiConsole.Prompt(prompt);

                    Assert.True(result.HasSelection);
                    Assert.Equal(1, result.Index);
                    Assert.Equal("prod", result.SelectedItem.Slug);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Keyboard selection can clamp at the first option and render custom search placeholder text.
        /// </summary>
        [Fact]
        public void SelectionPrompt_KeyboardWrapAroundDisabled_ClampsAtFirstOption()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                scope.Driver.EnqueueKey(Key(ConsoleKey.UpArrow));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Enter, '\r'));

                try
                {
                    var prompt = new SelectionPrompt()
                        .Title("Pick action")
                        .UseKeyboardNavigation()
                        .UseWrapAround(false)
                        .EnableSearch()
                        .WithSearchPlaceholderText("Search actions")
                        .AddChoices("Preview", "Deploy");

                    PromptResult result = AnsiConsole.Prompt(prompt);

                    Assert.True(result.HasSelection);
                    Assert.Equal(0, result.Index);
                    VisualAssert.WroteText(scope.Driver, "Search actions");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Typed selection prompts can expose a domain fallback while still reporting cancellation.
        /// </summary>
        [Fact]
        public void SelectionPrompt_TypedCancelResult_ReturnsFallbackItemWithCanceledState()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Escape));

                try
                {
                    var fallback = new DeployTarget("Canceled", "cancel");
                    var prompt = new SelectionPrompt<DeployTarget>()
                        .Title("Choose target")
                        .UseConverter(target => target.Name)
                        .UseKeyboardNavigation()
                        .OnCancel(() => fallback)
                        .AddChoices(
                            new DeployTarget("Development", "dev"),
                            new DeployTarget("Production", "prod"));

                    SelectionResult<DeployTarget> result = AnsiConsole.Prompt(prompt);

                    Assert.True(result.IsCanceled);
                    Assert.Equal("cancel", result.SelectedItem.Slug);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Multi-selection keyboard navigation can clamp at the first option instead of wrapping.
        /// </summary>
        [Fact]
        public void MultiSelectionPrompt_KeyboardWrapAroundDisabled_ClampsAtFirstOption()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                scope.Driver.EnqueueKey(Key(ConsoleKey.UpArrow));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Spacebar, ' '));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Enter, '\r'));

                try
                {
                    var prompt = new MultiSelectionPrompt()
                        .Title("Pick checks")
                        .UseKeyboardNavigation()
                        .UseWrapAround(false)
                        .NotRequired()
                        .AddChoices("Build", "Test");

                    MultiSelectionResult result = AnsiConsole.Prompt(prompt);

                    Assert.False(result.IsCanceled);
                    Assert.Equal(new[] { 0 }, result.Indexes.ToArray());
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Typed multi-selection prompts can use default values and cancellation fallbacks without sentinel indexes.
        /// </summary>
        [Fact]
        public void MultiSelectionPrompt_TypedDefaultValueAndCancelResult_ReturnDomainObjects()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                try
                {
                    var development = new DeployTarget("Development", "dev");
                    var production = new DeployTarget("Production", "prod");
                    var prompt = new MultiSelectionPrompt<DeployTarget>()
                        .Title("Choose targets")
                        .UseConverter(target => target.Name)
                        .DefaultValue(production)
                        .OnCancel(() => new[] { development })
                        .AddChoices(development, production);

                    MultiSelectionResult<DeployTarget> result = AnsiConsole.Prompt(prompt);

                    Assert.False(result.IsCanceled);
                    Assert.Equal(new[] { "prod" }, result.SelectedItems.Select(target => target.Slug).ToArray());
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Typed multi-selection cancellation fallbacks preserve canceled state while exposing fallback items.
        /// </summary>
        [Fact]
        public void MultiSelectionPrompt_TypedCancelResult_ReturnsFallbackItemsWithCanceledState()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Escape));

                try
                {
                    var fallback = new DeployTarget("Canceled", "cancel");
                    var prompt = new MultiSelectionPrompt<DeployTarget>()
                        .Title("Choose targets")
                        .UseConverter(target => target.Name)
                        .UseKeyboardNavigation()
                        .OnCancel(() => new[] { fallback })
                        .AddChoices(
                            new DeployTarget("Development", "dev"),
                            new DeployTarget("Production", "prod"));

                    MultiSelectionResult<DeployTarget> result = AnsiConsole.Prompt(prompt);

                    Assert.True(result.IsCanceled);
                    Assert.Equal(new[] { "cancel" }, result.SelectedItems.Select(target => target.Slug).ToArray());
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Property-named prompt builder extensions support Spectre-style paging calls.
        /// </summary>
        [Fact]
        public void SelectionPrompt_PropertyNamedBuilderExtensions_PageAndSelectChoice()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "next", "2" }));

                try
                {
                    var prompt = new SelectionPrompt()
                        .Title("Pick environment")
                        .PageSize(1)
                        .MoreChoicesText("Page {3}/{4}");

                    SelectOption development = prompt.AddChoice("Development");
                    SelectOption production = prompt.AddChoice("Production");

                    PromptResult result = AnsiConsole.Prompt(prompt);

                    Assert.Equal("Development", development.Label);
                    Assert.Equal("Production", production.Label);
                    Assert.True(result.HasSelection);
                    Assert.Equal(1, result.Index);
                    VisualAssert.WroteText(scope.Driver, "Page 2/2");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Property-named prompt builder extensions support keyboard wrap and search placeholder calls.
        /// </summary>
        [Fact]
        public void SelectionPrompt_PropertyNamedBuilderExtensions_ClampAndRenderPlaceholder()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                scope.Driver.EnqueueKey(Key(ConsoleKey.UpArrow));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Enter, '\r'));

                try
                {
                    var prompt = new SelectionPrompt()
                        .Title("Pick action")
                        .EnableSearch()
                        .UseKeyboardNavigation()
                        .WrapAround(false)
                        .SearchPlaceholderText("Find actions");

                    prompt.AddChoice("Preview");
                    prompt.AddChoice("Deploy");

                    PromptResult result = AnsiConsole.Prompt(prompt);

                    Assert.True(result.HasSelection);
                    Assert.Equal(0, result.Index);
                    VisualAssert.WroteText(scope.Driver, "Find actions");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Typed prompts support AddChoice and Spectre-style CancelResult property fallback shapes.
        /// </summary>
        [Fact]
        public void TypedPrompt_AddChoiceAndCancelResultProperty_ReturnCancellationFallbacks()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Escape));
                scope.Driver.EnqueueKey(Key(ConsoleKey.Escape));

                try
                {
                    var fallback = new DeployTarget("Canceled", "cancel");
                    var selection = new SelectionPrompt<DeployTarget>()
                        .Title("Pick target")
                        .UseConverter(target => target.Name)
                        .UseKeyboardNavigation()
                        .WrapAround(false);
                    selection.CancelResult = () => fallback;

                    SelectOption development = selection.AddChoice(new DeployTarget("Development", "dev"));
                    selection.AddChoice(new DeployTarget("Production", "prod"));

                    SelectionResult<DeployTarget> selected = AnsiConsole.Prompt(selection);

                    var multi = new MultiSelectionPrompt<DeployTarget>()
                        .Title("Pick targets")
                        .UseConverter(target => target.Name)
                        .UseKeyboardNavigation()
                        .PageSize(1)
                        .MoreChoicesText("Page {3}/{4}")
                        .WrapAround(false);
                    multi.CancelResult = () => new[] { fallback };

                    multi.AddChoice(new DeployTarget("Development", "dev"));
                    multi.AddChoice(new DeployTarget("Production", "prod"));

                    MultiSelectionResult<DeployTarget> selectedMany = AnsiConsole.Prompt(multi);

                    Assert.Equal("Development", development.Label);
                    Assert.True(selected.IsCanceled);
                    Assert.Equal("cancel", selected.SelectedItem.Slug);
                    Assert.True(selectedMany.IsCanceled);
                    Assert.Equal(new[] { "cancel" }, selectedMany.SelectedItems.Select(target => target.Slug).ToArray());
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Scalar prompt builder color aliases accept reusable style objects.
        /// </summary>
        [Fact]
        public void ScalarPromptBuilders_ColorAliases_AcceptStyleObjects()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "bad", string.Empty, "maybe", string.Empty, string.Empty, "token-value" }));

                try
                {
                    TextPromptResult<int> retryCount = new TextPrompt<int>("Ignored")
                        .Title("Retry count")
                        .Style(new Style(ConsoleColor.Green))
                        .InvalidStyle(new Style(ConsoleColor.Red))
                        .DefaultValue(4)
                        .ValidationErrorMessage("Enter a whole number.")
                        .UseParser((string text, out int value) => int.TryParse(text, out value))
                        .Render();

                    ConfirmResult deploy = new ConfirmPrompt("Ignored")
                        .Title("Deploy now?")
                        .PromptStyle(new Style(ConsoleColor.Cyan))
                        .InvalidStyle(new Style(ConsoleColor.DarkYellow))
                        .DefaultValue(false)
                        .ValidationErrorMessage("Use yes or no.")
                        .Render();

                    SecretResult token = new SecretPrompt("Ignored")
                        .Title("API token")
                        .Style(new Style(ConsoleColor.Blue))
                        .Mask('#')
                        .MaskStyle(new Style(ConsoleColor.DarkBlue))
                        .InvalidStyle(new Style(ConsoleColor.Red))
                        .RequiredErrorMessage("Token is required.")
                        .Render();

                    Assert.True(retryCount.HasValue);
                    Assert.Equal(4, retryCount.Value);
                    Assert.True(deploy.HasValue);
                    Assert.False(deploy.Value);
                    Assert.True(token.HasValue);
                    Assert.Equal("token-value", token.Value);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Retry count") && write.Color == ConsoleColor.Green);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Enter a whole number.") && write.Color == ConsoleColor.Red);
                    Assert.Contains("Deploy now? [y/N]", scope.Driver.Text);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy now?") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("[y/N]") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Use yes or no.") && write.Color == ConsoleColor.DarkYellow);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("API token") && write.Color == ConsoleColor.Blue);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Token is required.") && write.Color == ConsoleColor.Red);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("#") && write.Color == ConsoleColor.DarkBlue);
                    Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("token-value"));
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Selection prompt builder color aliases accept reusable style objects while preserving fluent return types.
        /// </summary>
        [Fact]
        public void SelectionPromptBuilders_ColorAliases_AcceptStyleObjects()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "1", "1", string.Empty, "all" }));

                try
                {
                    PromptResult action = new SelectionPrompt()
                        .Title("Pick action")
                        .PromptStyle(new Style(ConsoleColor.Magenta))
                        .InstructionsText("Pick by number.")
                        .InstructionStyle(new Style(ConsoleColor.DarkMagenta))
                        .HighlightStyle(new Style(ConsoleColor.Green))
                        .AddChoices("Preview", "Deploy")
                        .Render();

                    var development = new DeployTarget("Development", "dev");
                    var production = new DeployTarget("Production", "prod");
                    SelectionResult<DeployTarget> target = new SelectionPrompt<DeployTarget>()
                        .Title("Target")
                        .Style(new Style(ConsoleColor.Cyan))
                        .InstructionsText("Pick a target.")
                        .InstructionStyle(new Style(ConsoleColor.DarkGray))
                        .HighlightColor(new Style(ConsoleColor.Yellow))
                        .UseConverter(item => item.Name)
                        .AddChoices(development, production)
                        .RenderSelection();

                    MultiSelectionResult checks = new MultiSelectionPrompt()
                        .Title("Pick checks")
                        .Style(new Style(ConsoleColor.Yellow))
                        .InstructionsText("Choose checks or press Enter.")
                        .InstructionStyle(new Style(ConsoleColor.DarkYellow))
                        .HighlightColor(new Style(ConsoleColor.Green))
                        .NotRequired()
                        .AddChoices("Build", "Test")
                        .Render();

                    MultiSelectionResult<DeployTarget> targets = new MultiSelectionPrompt<DeployTarget>()
                        .Title("Deploy targets")
                        .PromptStyle(new Style(ConsoleColor.Blue))
                        .InstructionsText("Choose targets.")
                        .InstructionStyle(new Style(ConsoleColor.DarkCyan))
                        .HighlightStyle(new Style(ConsoleColor.Green))
                        .UseConverter(item => item.Name)
                        .AddChoices(development, production)
                        .RenderSelection();

                    Assert.True(action.HasSelection);
                    Assert.Equal(0, action.Index);
                    Assert.True(target.HasSelection);
                    Assert.Equal("dev", target.SelectedItem.Slug);
                    Assert.False(checks.IsCanceled);
                    Assert.Empty(checks.Indexes);
                    Assert.True(targets.HasSelection);
                    Assert.Equal(new[] { "dev", "prod" }, targets.SelectedItems.Select(item => item.Slug).ToArray());
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Pick action") && write.Color == ConsoleColor.Magenta);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Pick by number.") && write.Color == ConsoleColor.DarkMagenta);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Target") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Pick a target.") && write.Color == ConsoleColor.DarkGray);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Pick checks") && write.Color == ConsoleColor.Yellow);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Choose checks or press Enter.") && write.Color == ConsoleColor.DarkYellow);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy targets") && write.Color == ConsoleColor.Blue);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Choose targets.") && write.Color == ConsoleColor.DarkCyan);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        private static ConsoleKeyInfo Key(ConsoleKey key, char keyChar = '\0')
        {
            return new ConsoleKeyInfo(keyChar, key, false, false, false);
        }

        private static void EnqueueText(FakeConsoleDriver driver, string text)
        {
            foreach (char ch in text)
            {
                driver.EnqueueKey(Key(ConsoleKey.A, ch));
            }
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
