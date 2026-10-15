using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ConsoulLibrary.Color;
using ConsoulLibrary.Tests.Rendering;
using SpectreColor = ConsoulLibrary.Color.Color;
using Xunit;

namespace ConsoulLibrary.Tests.Rendering
{
    /// <summary>
    /// Responsive rendering tests for selection prompts.
    /// </summary>
    public class PromptResponsiveRenderingTests
    {
        /// <summary>
        /// Invalid prompt input re-renders under narrow widths without overflowing.
        /// </summary>
        [Theory]
        [InlineData(24)]
        [InlineData(80)]
        public void Render_InvalidSelectionThenValidSelection_DoesNotOverflow(int width)
        {
            using (var scope = new ConsoleDriverScope(width))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "9", "2" }));

                try
                {
                    var prompt = new SelectionPrompt(
                        "Choose a deployment target with a long prompt message",
                        clear: true,
                        "Alpha target with extended descriptive text",
                        "Beta target",
                        "Gamma target");

                    PromptResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(1, result.Index);
                    Assert.True(scope.Driver.ClearCount >= 2);
                    VisualAssert.WroteText(scope.Driver, "Invalid selection!");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Selection prompts can customize invalid selection copy with raw markup.
        /// </summary>
        [Fact]
        public void SelectionPrompt_CustomInvalidSelectionMessageMarkup_RendersVisibleText()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "9", "1" }));

                try
                {
                    var prompt = new SelectionPrompt("Pick action", clear: false, "Preview", "Deploy")
                        .WithInvalidSelectionMessage("[yellow]Choose an available action[/] [[prod]]");

                    PromptResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(0, result.Index);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Choose an available action") && write.Color == ConsoleColor.Yellow);
                    Assert.Contains("[prod]", scope.Driver.Text);
                    Assert.DoesNotContain("Invalid selection!", scope.Driver.Text);
                    Assert.DoesNotContain("[yellow]", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Default and escape prompt inputs preserve behavior while rendering headlessly.
        /// </summary>
        [Fact]
        public void Render_DefaultAndEscapeInputs_DoNotRequireRealConsole()
        {
            using (var scope = new ConsoleDriverScope(32))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty }));

                try
                {
                    var prompt = new SelectionPrompt(
                        "Pick a default",
                        false,
                        new SelectOption(0, "Default option with a long label", ConsoleColor.Green, isDefault: true),
                        new SelectOption(1, "Other option", ConsoleColor.White));

                    PromptResult defaultResult = prompt.Render();

                    Assert.True(defaultResult.HasSelection);
                    Assert.Equal(0, defaultResult.Index);

                    Routines.InitializeRoutine(new TestRoutine(new[] { "exit" }));
                    PromptResult escapeResult = new SelectionPrompt("Pick anything", false, "One", "Two").Render();

                    Assert.True(escapeResult.IsCanceled);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Selection prompt titles, options, and descriptions parse raw markup without leaking tags.
        /// </summary>
        [Fact]
        public void SelectionPrompt_RawMarkupText_RendersVisibleTextAndFiltersByVisibleContent()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "/Deploy", "1" }));

                try
                {
                    var prompt = new SelectionPrompt("[cyan]Pick target[/] [[prod]]", clear: false)
                        .SearchEnabled();
                    prompt.AddDescribed("[green]Deploy[/] [[prod]]", "[white on navy]Ships release[/] to [[prod]]");
                    prompt.AddChoices("[yellow]Preview[/]");

                    PromptResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(0, result.Index);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Pick target") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.Green);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ships release") && write.Color == ConsoleColor.White);
                    Assert.Contains("[prod]", scope.Driver.Text);
                    Assert.DoesNotContain("[green]", scope.Driver.Text);
                    Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                    Assert.DoesNotContain("[white on navy]", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Selection prompt filter and paging helper templates parse markup while formatted filter values stay literal.
        /// </summary>
        [Fact]
        public void SelectionPrompt_FormattedHelpMarkup_RendersTemplateMarkupAndEscapesFilterValues()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "/[green]missing[/]", "/clear", "next", "2" }));

                try
                {
                    var prompt = new SelectionPrompt("[cyan]Pick target[/]", clear: false)
                    {
                        FilterEnabled = true,
                        PageSize = 1,
                        FilterHelpText = "[cyan]Type {0}find[/]",
                        ActiveFilterText = "[yellow]Filter[/]: {0} ({1})",
                        NoFilterMatchesText = "[red]No matches for {0}[/]",
                        MoreChoicesText = "[green]Page {3}/{4}[/]"
                    };
                    prompt.AddChoices("Development", "Production");

                    PromptResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(1, result.Index);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Type /find") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Filter") && write.Color == ConsoleColor.Yellow);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("No matches for [green]missing[/]") && write.Color == ConsoleColor.Red);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Page 2/2") && write.Color == ConsoleColor.Green);
                    Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("missing") && write.Color == ConsoleColor.Green);
                    Assert.Contains("[green]missing[/]", scope.Driver.Text);
                    Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                    Assert.DoesNotContain("[yellow]", scope.Driver.Text);
                    Assert.DoesNotContain("[red]", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Keyboard search placeholder text parses raw markup without requiring a real console.
        /// </summary>
        [Fact]
        public void SelectionPrompt_KeyboardSearchPlaceholderMarkup_RendersVisibleText()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                scope.Driver.EnqueueKey(Enter());

                var prompt = new SelectionPrompt("Pick action", clear: false)
                {
                    FilterEnabled = true,
                    KeyboardNavigationEnabled = true,
                    SearchPlaceholderText = "[cyan]Search actions[/] [[prod]]"
                };
                prompt.AddChoices("Preview", "Deploy");

                PromptResult result = prompt.Render();

                Assert.True(result.HasSelection);
                Assert.Equal(0, result.Index);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Search actions") && write.Color == ConsoleColor.Cyan);
                Assert.Contains("[prod]", scope.Driver.Text);
                Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Disabled option reasons parse raw markup and unavailable feedback keeps the generated prefix literal.
        /// </summary>
        [Fact]
        public void SelectionPrompt_DisabledReasonMarkup_RendersVisibleReasonAndUnavailableMessage()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "/Approval", "2", "/clear", "1" }));

                try
                {
                    var prompt = new SelectionPrompt("[cyan]Pick action[/]", clear: false)
                    {
                        FilterEnabled = true
                    };
                    prompt.Add("Preview");
                    prompt.AddDisabled("[green]Deploy[/]", "[yellow]Approval required[/] [[prod]]");

                    PromptResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(0, result.Index);
                    Assert.Contains(scope.Driver.Writes, write => write.Text == "Option unavailable: " && write.Color == RenderOptions.InvalidColor);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Approval required") && write.Color == ConsoleColor.Yellow);
                    Assert.Contains("[prod]", scope.Driver.Text);
                    Assert.Contains("Filter: Approval", scope.Driver.Text);
                    Assert.DoesNotContain("[yellow]", scope.Driver.Text);
                    Assert.DoesNotContain("[green]", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Selection prompt style helpers preserve RGB and decoration metadata for true-color terminals.
        /// </summary>
        [Fact]
        public void SelectionPrompt_StyleHelpersEmitTrueColorSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 16, isTerminal: true),
                In = new StringReader("1\n"),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });

            SelectOption deploy = new SelectOption("Deploy").UseStyle("bold #112233").Describe("Ship release");
            SelectOption separator = SelectOption.SeparatorStyled("Danger", "italic #778899");
            SelectOption archive = new SelectOption("Archive").UseDisabledStyle("italic #abcdef").Disable("locked");
            var prompt = new SelectionPrompt("Pick action", false, deploy, separator, archive)
                .PromptStyle("bold #22c55e")
                .InstructionStyle("italic #778899");
            prompt.OptionDescriptionDecoration = Decoration.Underline;
            prompt.OptionDescriptionForegroundRgb = RgbColor.FromHex("#336699");

            PromptResult result = console.Prompt(prompt);

            Assert.True(result.HasSelection);
            Assert.Equal(0, result.Index);

            string output = writer.ToString();
            Assert.Contains("\u001b[1;38;2;34;197;94mPick action", output);
            Assert.Contains("\u001b[3;38;2;119;136;153mChoose", output);
            Assert.Contains("\u001b[1;38;2;17;34;51m1) Deploy", output);
            Assert.Contains("\u001b[4;38;2;51;102;153m    Ship release", output);
            Assert.Contains("\u001b[3;38;2;119;136;153m-- Danger --", output);
            Assert.Contains("\u001b[3;38;2;171;205;239m2) Archive", output);
        }

        /// <summary>
        /// Individual selection option descriptions can override or compose with prompt-level description styles.
        /// </summary>
        [Fact]
        public void SelectionPrompt_OptionDescriptionStyleOverridesPromptDefault()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 16, isTerminal: true),
                In = new StringReader("1\n"),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });

            SelectOption deploy = new SelectOption("Deploy")
                .Describe("Ship release")
                .UseDescriptionStyle("italic");
            SelectOption rollback = new SelectOption("Rollback")
                .Describe("Use previous version")
                .UseDescriptionStyle("#445566");
            SelectOption audit = new SelectOption("Audit")
                .Describe("Review only");
            var prompt = new SelectionPrompt("Pick action", false, deploy, rollback, audit)
                .OptionDescriptionStyle("underline #336699");
            Assert.Equal(RgbColor.FromHex("#336699"), prompt.OptionDescriptionForegroundRgb);

            PromptResult result = console.Prompt(prompt);

            Assert.True(result.HasSelection);
            Assert.Equal(0, result.Index);

            string output = writer.ToString();
            string deployStyle = AnsiPrefixBefore(output, "Ship release");
            string rollbackStyle = AnsiPrefixBefore(output, "Use previous version");
            string auditStyle = AnsiPrefixBefore(output, "Review only");
            AssertAnsiCode(deployStyle, "3");
            AssertAnsiRgb(deployStyle, 51, 102, 153);
            AssertAnsiRgb(rollbackStyle, 68, 85, 102);
            Assert.DoesNotContain("4", GetAnsiCodes(rollbackStyle));
            AssertAnsiCode(auditStyle, "4");
            AssertAnsiRgb(auditStyle, 51, 102, 153);
        }

        /// <summary>
        /// Plain selection prompt description color helpers clear previously configured RGB and decoration metadata.
        /// </summary>
        [Fact]
        public void SelectionPrompt_OptionDescriptionColorHelperClearsPriorRichStyleMetadata()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 16, isTerminal: true),
                In = new StringReader("1\n"),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });

            SelectOption deploy = new SelectOption("Deploy")
                .Describe("Ship release")
                .UseDescriptionStyle("bold #112233")
                .UseDescriptionColor(ConsoleColor.Green);
            var prompt = new SelectionPrompt("Pick action", false, deploy)
                .OptionDescriptionStyle("underline #336699")
                .OptionDescriptionStyle(ConsoleColor.Yellow);

            PromptResult result = console.Prompt(prompt);

            Assert.True(result.HasSelection);
            Assert.Equal(0, result.Index);

            string output = writer.ToString();
            Assert.Contains("Ship release", output);
            Assert.DoesNotContain("\u001b[1;", output);
            Assert.DoesNotContain("\u001b[4;", output);
            Assert.DoesNotContain("\u001b[38;2", output);
            Assert.DoesNotContain("[#112233", output);
            Assert.DoesNotContain("[#336699", output);
        }

        /// <summary>
        /// Selection prompt palette extensions preserve RGB metadata instead of reducing colors to the nearest console color.
        /// </summary>
        [Fact]
        public void SelectionPrompt_PaletteExtensionsEmitTrueColorSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 16, isTerminal: true),
                In = new StringReader("1\n"),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });

            var prompt = new SelectionPrompt("Pick action", false);
            PromptPaletteExtensions.AddDescribed(prompt, "Deploy", "Ship release", RgbColor.FromHex("#112233"));
            PromptPaletteExtensions.AddSeparator(prompt, "Danger", RgbColor.FromHex("#778899"));
            PromptPaletteExtensions.AddDisabledDescribed(prompt, "Archive", "Needs approval", "locked", RgbColor.FromHex("#abcdef"));
            PromptPaletteExtensions.Style(prompt, RgbColor.FromHex("#22c55e"));
            PromptPaletteExtensions.InstructionStyle(prompt, RgbColor.FromHex("#778899"));
            prompt.OptionDescriptionForegroundRgb = RgbColor.FromHex("#336699");

            PromptResult result = console.Prompt(prompt);

            Assert.True(result.HasSelection);
            Assert.Equal(0, result.Index);

            string output = writer.ToString();
            Assert.Contains("\u001b[38;2;34;197;94mPick action", output);
            Assert.Contains("\u001b[38;2;119;136;153mChoose", output);
            Assert.Contains("\u001b[38;2;17;34;51m1) Deploy", output);
            Assert.Contains("\u001b[38;2;51;102;153m    Ship release", output);
            Assert.Contains("\u001b[38;2;119;136;153m-- Danger --", output);
            Assert.Contains("\u001b[38;2;171;205;239m2) Archive", output);
        }

        /// <summary>
        /// Multi-selection prompt style helpers preserve RGB and decoration metadata for true-color terminals.
        /// </summary>
        [Fact]
        public void MultiSelectionPrompt_StyleHelpersEmitTrueColorSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 16, isTerminal: true),
                In = new StringReader("1\n"),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });

            var prompt = new MultiSelectionPrompt("Pick checks", false, false)
                .PromptStyle("bold #22c55e")
                .InstructionStyle("italic #778899");
            int buildIndex = PromptPaletteExtensions.AddDescribed(prompt, "Build", "Compile release", RgbColor.FromHex("#112233"));
            prompt.Options.First(option => !option.IsSeparator && option.Index == buildIndex).AddSearchTerms("compile");
            PromptPaletteExtensions.AddSeparator(prompt, "Stage", RgbColor.FromHex("#778899")).UseStyle("italic #778899");
            PromptPaletteExtensions.AddDisabledDescribed(prompt, "Deploy", "Needs approval", "locked", RgbColor.FromHex("#abcdef"));
            prompt.OptionDescriptionDecoration = Decoration.Underline;
            prompt.OptionDescriptionForegroundRgb = RgbColor.FromHex("#336699");

            MultiSelectionResult result = console.Prompt(prompt);

            Assert.True(result.HasSelection);
            Assert.Equal(new[] { 0 }, result.Indexes);

            string output = writer.ToString();
            Assert.Contains("\u001b[1;38;2;34;197;94mPick checks", output);
            Assert.Contains("\u001b[3;38;2;119;136;153mChoose", output);
            Assert.Contains("\u001b[38;2;17;34;51m[ ] - 1) Build", output);
            Assert.Contains("\u001b[4;38;2;51;102;153m    Compile release", output);
            Assert.Contains("\u001b[3;38;2;119;136;153m-- Stage --", output);
            Assert.Contains("\u001b[38;2;171;205;239m[ ] - 2) Deploy", output);
        }

        /// <summary>
        /// Individual multi-selection option descriptions can override or compose with prompt-level description styles.
        /// </summary>
        [Fact]
        public void MultiSelectionPrompt_OptionDescriptionStyleOverridesPromptDefault()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 16, isTerminal: true),
                In = new StringReader("1\n"),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });

            var prompt = new MultiSelectionPrompt("Pick checks", false, false)
                .OptionDescriptionStyle("underline #336699");
            Assert.Equal(RgbColor.FromHex("#336699"), prompt.OptionDescriptionForegroundRgb);
            int buildIndex = prompt.AddDescribed("Build", "Compile release");
            int testIndex = prompt.AddDescribed("Test", "Run verification");
            prompt.AddDescribed("Audit", "Review only");
            prompt.Options.First(option => option.Index == buildIndex).UseDescriptionStyle("italic");
            prompt.Options.First(option => option.Index == testIndex).UseDescriptionColor(RgbColor.FromHex("#445566"));

            MultiSelectionResult result = console.Prompt(prompt);

            Assert.True(result.HasSelection);
            Assert.Equal(new[] { 0 }, result.Indexes);

            string output = writer.ToString();
            string buildStyle = AnsiPrefixBefore(output, "Compile release");
            string testStyle = AnsiPrefixBefore(output, "Run verification");
            string auditStyle = AnsiPrefixBefore(output, "Review only");
            AssertAnsiCode(buildStyle, "3");
            AssertAnsiRgb(buildStyle, 51, 102, 153);
            AssertAnsiRgb(testStyle, 68, 85, 102);
            Assert.DoesNotContain("4", GetAnsiCodes(testStyle));
            AssertAnsiCode(auditStyle, "4");
            AssertAnsiRgb(auditStyle, 51, 102, 153);
        }

        /// <summary>
        /// Text prompt title, accepted value, default value, and invalid styles preserve RGB and decorations.
        /// </summary>
        [Fact]
        public void TextPrompt_StyleHelpersEmitTrueColorSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 16, isTerminal: true),
                In = new StringReader("qa\n\n"),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });

            TextPromptResult<string> result = console.Prompt(
                new TextPrompt<string>("Deploy environment")
                    .PromptStyle("bold #112233")
                    .InvalidStyle("italic #445566")
                    .DefaultValue("dev")
                    .AddChoices("dev", "prod")
                    .ChoicesStyle("underline #778899")
                    .DefaultValueStyle("bold #abcdef"));

            Assert.True(result.HasValue);
            Assert.Equal("dev", result.Value);

            string output = writer.ToString();
            Assert.Contains("\u001b[1;38;2;17;34;51mDeploy environment", output);
            Assert.Contains("\u001b[4;38;2;119;136;153mdev/prod", output);
            Assert.Contains("\u001b[1;38;2;171;205;239mdefault: dev", output);
            Assert.Contains("\u001b[3;38;2;68;85;102mValue must be one of", output);
        }

        /// <summary>
        /// Direct text prompt palette overloads preserve RGB metadata instead of falling back through ConsoleColor conversions.
        /// </summary>
        [Fact]
        public void TextPrompt_DirectPaletteStyleHelpersEmitTrueColorSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 16, isTerminal: true),
                In = new StringReader("qa\nprod\n"),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });

            TextPromptResult<string> result = console.Prompt(
                new TextPrompt<string>("Deploy environment")
                    .PromptStyle(SpectreColor.FromHex("#112233"))
                    .InvalidStyle(RgbColor.FromHex("#445566"))
                    .AddChoices("dev", "prod"));

            Assert.True(result.HasValue);
            Assert.Equal("prod", result.Value);

            string output = writer.ToString();
            Assert.Contains("\u001b[38;2;17;34;51mDeploy environment", output);
            Assert.Contains("\u001b[38;2;68;85;102mValue must be one of", output);
        }

        /// <summary>
        /// Secret text prompt mask styling preserves RGB and decoration metadata for true-color terminals.
        /// </summary>
        [Fact]
        public void TextPrompt_MaskStyleHelpersEmitTrueColorSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 16, isTerminal: true),
                In = new StringReader(string.Empty),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });
            Routines.InitializeRoutine(new TestRoutine(new[] { "token-value" }));

            try
            {
                TextPromptResult<string> result = console.Prompt(
                    new TextPrompt<string>("API token")
                        .Secret()
                        .Mask('*')
                        .MaskStyle("bold #112233"));

                Assert.True(result.HasValue);
                Assert.Equal("token-value", result.Value);

                string output = writer.ToString();
                Assert.Contains("\u001b[1;38;2;17;34;51m***********", output);
                Assert.DoesNotContain("token-value", output);
            }
            finally
            {
                Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
            }
        }

        /// <summary>
        /// Direct text prompt mask palette overloads preserve RGB metadata instead of falling back through ConsoleColor conversions.
        /// </summary>
        [Fact]
        public void TextPrompt_DirectPaletteMaskStyleHelperEmitsTrueColorSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 16, isTerminal: true),
                In = new StringReader(string.Empty),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });
            Routines.InitializeRoutine(new TestRoutine(new[] { "token-value" }));

            try
            {
                TextPromptResult<string> result = console.Prompt(
                    new TextPrompt<string>("API token")
                        .Secret()
                        .Mask('*')
                        .MaskStyle(RgbColor.FromHex("#112233")));

                Assert.True(result.HasValue);
                Assert.Equal("token-value", result.Value);

                string output = writer.ToString();
                Assert.Contains("\u001b[38;2;17;34;51m***********", output);
                Assert.DoesNotContain("token-value", output);
            }
            finally
            {
                Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
            }
        }

        /// <summary>
        /// Secret prompt palette mask styling preserves RGB metadata instead of reducing colors to the nearest console color.
        /// </summary>
        [Fact]
        public void SecretPrompt_PaletteMaskStyleEmitsTrueColorSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 16, isTerminal: true),
                In = new StringReader(string.Empty),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });
            Routines.InitializeRoutine(new TestRoutine(new[] { "secret-value" }));

            try
            {
                SecretPrompt prompt = PromptPaletteExtensions.MaskStyle(
                    new SecretPrompt("Secret value").Mask('#'),
                    RgbColor.FromHex("#445566"));

                SecretResult result = console.Prompt(prompt);

                Assert.True(result.HasValue);
                Assert.Equal("secret-value", result.Value);

                string output = writer.ToString();
                Assert.Contains("\u001b[38;2;68;85;102m############", output);
                Assert.DoesNotContain("secret-value", output);
            }
            finally
            {
                Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
            }
        }

        /// <summary>
        /// Direct secret prompt palette overloads preserve RGB metadata instead of falling back through ConsoleColor conversions.
        /// </summary>
        [Fact]
        public void SecretPrompt_DirectPaletteStyleHelpersEmitTrueColorSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 16, isTerminal: true),
                In = new StringReader(string.Empty),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });
            Routines.InitializeRoutine(new TestRoutine(new[] { string.Empty, "secret-value" }));

            try
            {
                SecretResult result = console.Prompt(
                    new SecretPrompt("Secret value")
                        .PromptStyle(SpectreColor.FromHex("#112233"))
                        .Mask('#')
                        .MaskStyle(RgbColor.FromHex("#445566"))
                        .InvalidStyle(RgbColor.FromHex("#778899")));

                Assert.True(result.HasValue);
                Assert.Equal("secret-value", result.Value);

                string output = writer.ToString();
                Assert.Contains("\u001b[38;2;17;34;51mSecret value", output);
                Assert.Contains("\u001b[38;2;119;136;153mA value is required.", output);
                Assert.Contains("\u001b[38;2;68;85;102m############", output);
                Assert.DoesNotContain("secret-value", output);
            }
            finally
            {
                Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
            }
        }

        /// <summary>
        /// Confirmation prompt title, choices, default value, and invalid styles preserve RGB and decorations.
        /// </summary>
        [Fact]
        public void ConfirmationPrompt_StyleHelpersEmitTrueColorSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 16, isTerminal: true),
                In = new StringReader("maybe\n\n"),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });

            ConfirmResult result = console.Prompt(
                new ConfirmationPrompt("Continue?")
                    .PromptStyle("bold #223344")
                    .ChoicesStyle("italic #445566")
                    .DefaultValueStyle("underline #778899")
                    .InvalidStyle("bold #abcdef"));

            Assert.True(result.HasValue);
            Assert.True(result.Value);

            string output = writer.ToString();
            Assert.Contains("\u001b[1;38;2;34;51;68mContinue?", output);
            Assert.Contains("\u001b[4;38;2;119;136;153mY", output);
            Assert.Contains("\u001b[3;38;2;68;85;102mn", output);
            Assert.Contains("\u001b[1;38;2;171;205;239mPlease select one of the available options.", output);
        }

        /// <summary>
        /// Direct confirmation prompt palette overloads preserve RGB metadata instead of falling back through ConsoleColor conversions.
        /// </summary>
        [Fact]
        public void ConfirmPrompt_DirectPaletteStyleHelpersEmitTrueColorSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 16, isTerminal: true),
                In = new StringReader("maybe\nyes\n"),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor
            });

            ConfirmResult result = console.Prompt(
                new ConfirmPrompt("Deploy?")
                    .PromptStyle(SpectreColor.FromHex("#223344"))
                    .InvalidStyle(RgbColor.FromHex("#abcdef")));

            Assert.True(result.HasValue);
            Assert.True(result.Value);

            string output = writer.ToString();
            Assert.Contains("\u001b[38;2;34;51;68mDeploy?", output);
            Assert.Contains("\u001b[38;2;171;205;239mPlease answer yes or no.", output);
        }

        /// <summary>
        /// Selection prompt RGB helpers emit 256-color SGR when the terminal supports eight-bit color.
        /// </summary>
        [Fact]
        public void SelectionPrompt_RgbHelpersEmitEightBitSgrWhenSupported()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 80, height: 16, isTerminal: true),
                In = new StringReader("1\n"),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.EightBit
            });

            SelectOption deploy = new SelectOption("Deploy")
                .UseColor(RgbColor.FromHex("#445566"))
                .Describe("Ship release");
            SelectOption separator = SelectOption.Separator("Danger", RgbColor.FromHex("#336699"));
            SelectOption archive = new SelectOption("Archive").UseColor(RgbColor.FromHex("#778899"));
            var prompt = new SelectionPrompt("Pick action", false, deploy, separator, archive)
            {
                OptionDescriptionForegroundRgb = RgbColor.FromHex("#336699")
            };

            PromptResult result = console.Prompt(prompt);

            Assert.True(result.HasSelection);
            Assert.Equal(0, result.Index);

            string output = writer.ToString();
            Assert.Contains("\u001b[38;5;239m1) Deploy", output);
            Assert.Contains("\u001b[38;5;60m    Ship release", output);
            Assert.Contains("\u001b[38;5;60m-- Danger --", output);
            Assert.DoesNotContain("\u001b[38;2", output);
        }

        /// <summary>
        /// Multi-selection prompt titles and checkbox options parse raw markup without leaking tags.
        /// </summary>
        [Fact]
        public void MultiSelectionPrompt_RawMarkupText_RendersVisibleText()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "1" }));

                try
                {
                    var prompt = new MultiSelectionPrompt("[cyan]Pick tasks[/] [[prod]]", clear: false, allowEmpty: false);
                    prompt.AddDescribed("[green]Build[/] [[prod]]", "[white on navy]Compile artifacts[/]");
                    prompt.AddChoices("[yellow]Test[/]");

                    MultiSelectionResult result = prompt.Render();

                    Assert.False(result.IsCanceled);
                    Assert.Contains(0, result.Indexes);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Pick tasks") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build") && write.Color == ConsoleColor.Green);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Compile artifacts") && write.Color == ConsoleColor.White);
                    Assert.Contains("[prod]", scope.Driver.Text);
                    Assert.DoesNotContain("[green]", scope.Driver.Text);
                    Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                    Assert.DoesNotContain("[white on navy]", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Multi-selection filter and paging helper templates parse markup while formatted filter values stay literal.
        /// </summary>
        [Fact]
        public void MultiSelectionPrompt_FormattedHelpMarkup_RendersTemplateMarkupAndEscapesFilterValues()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "/[green]none[/]", "/clear", "all" }));

                try
                {
                    var prompt = new MultiSelectionPrompt("[cyan]Pick checks[/]", clear: false, allowEmpty: false)
                    {
                        FilterEnabled = true,
                        PageSize = 1,
                        FilterHelpText = "[cyan]Use {0}query[/]",
                        ActiveFilterText = "[blue]Filter[/]: {0}",
                        NoFilterMatchesText = "[red]Nothing for {0}[/]",
                        MoreChoicesText = "[magenta]Step {3}/{4}[/]"
                    };
                    prompt.AddChoices("Build", "Test");

                    MultiSelectionResult result = prompt.Render();

                    Assert.False(result.IsCanceled);
                    Assert.Equal(new[] { 0, 1 }, result.Indexes);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Use /query") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Filter") && write.Color == ConsoleColor.Blue);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Nothing for [green]none[/]") && write.Color == ConsoleColor.Red);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Step 1/2") && write.Color == ConsoleColor.Magenta);
                    Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("none") && write.Color == ConsoleColor.Green);
                    Assert.Contains("[green]none[/]", scope.Driver.Text);
                    Assert.DoesNotContain("[blue]", scope.Driver.Text);
                    Assert.DoesNotContain("[red]", scope.Driver.Text);
                    Assert.DoesNotContain("[magenta]", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Multi-selection disabled reasons parse raw markup and filter by visible reason text.
        /// </summary>
        [Fact]
        public void MultiSelectionPrompt_DisabledReasonMarkup_RendersVisibleTextAndFiltersByReason()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "/Approval", "2", "/clear", "all" }));

                try
                {
                    var prompt = new MultiSelectionPrompt("[cyan]Pick tasks[/]", clear: false, allowEmpty: false)
                    {
                        FilterEnabled = true
                    };
                    prompt.Add("Build");
                    prompt.AddDisabled("[green]Deploy[/]", "[yellow]Approval required[/] [[prod]]");
                    prompt.Add("Report");

                    MultiSelectionResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 0, 2 }, result.Indexes);
                    Assert.Contains(scope.Driver.Writes, write => write.Text == "Option unavailable: " && write.Color == RenderOptions.InvalidColor);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Approval required") && write.Color == ConsoleColor.Yellow);
                    Assert.Contains("[prod]", scope.Driver.Text);
                    Assert.Contains("Filter: Approval", scope.Driver.Text);
                    Assert.DoesNotContain("[yellow]", scope.Driver.Text);
                    Assert.DoesNotContain("[green]", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Multi-selection prompts can customize invalid selection copy while preserving disabled reason feedback.
        /// </summary>
        [Fact]
        public void MultiSelectionPrompt_CustomInvalidSelectionMessageMarkup_RendersVisibleTextAndDisabledReason()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "9", "2", "1" }));

                try
                {
                    var prompt = new MultiSelectionPrompt("Pick tasks", clear: false, allowEmpty: false)
                    {
                        InvalidSelectionMessage = "[yellow]Choose task indexes[/] [[prod]]"
                    };
                    prompt.Add("Build");
                    prompt.AddDisabled("Deploy", "[cyan]Approval required[/] [[prod]]");

                    MultiSelectionResult result = prompt.Render();

                    Assert.True(result.HasSelection);
                    Assert.Equal(new[] { 0 }, result.Indexes);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Choose task indexes") && write.Color == ConsoleColor.Yellow);
                    Assert.Contains(scope.Driver.Writes, write => write.Text == "Option unavailable: " && write.Color == RenderOptions.InvalidColor);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Approval required") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains("[prod]", scope.Driver.Text);
                    Assert.DoesNotContain("Invalid selection!", scope.Driver.Text);
                    Assert.DoesNotContain("[yellow]", scope.Driver.Text);
                    Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Text prompt titles parse raw markup while generated hints remain literal.
        /// </summary>
        [Fact]
        public void TextPrompt_RawMarkupMessage_RendersVisibleTextAndLiteralHints()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "3" }));

                try
                {
                    TextPromptResult<int> result = Consoul.Text<int>("[cyan]Retry count[/] [[prod]]")
                        .WithDefault(5)
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal(3, result.Value);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Retry count") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains("[prod]", scope.Driver.Text);
                    Assert.Contains("default: 5", scope.Driver.Text);
                    Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Secret prompt titles parse raw markup without leaking the typed value.
        /// </summary>
        [Fact]
        public void SecretPrompt_RawMarkupMessage_RendersVisibleTextWithoutLeakingSecret()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                EnqueueText(scope.Driver, "token-value");
                scope.Driver.EnqueueKey(Enter());

                SecretResult result = new SecretPrompt("[cyan]API key[/] [[prod]]").Render();

                Assert.True(result.HasValue);
                Assert.Equal("token-value", result.Value);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("API key") && write.Color == ConsoleColor.Cyan);
                Assert.Contains("[prod]", scope.Driver.Text);
                Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                Assert.DoesNotContain("token-value", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Legacy confirmation prompts parse raw markup in the message while yes/no suffixes remain literal.
        /// </summary>
        [Fact]
        public void ConfirmPrompt_RawMarkupMessage_RendersVisibleTextAndLiteralChoices()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "y" }));

                try
                {
                    ConfirmResult result = new ConfirmPrompt("[green]Deploy[/] [[prod]]").Render();

                    Assert.True(result.HasValue);
                    Assert.True(result.Value);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.Green);
                    Assert.Contains("[prod]", scope.Driver.Text);
                    Assert.Contains("[y/n]", scope.Driver.Text);
                    Assert.DoesNotContain("[green]", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Spectre-shaped confirmation prompts parse raw markup in the message while custom choice text remains literal.
        /// </summary>
        [Fact]
        public void ConfirmationPrompt_RawMarkupMessage_RendersVisibleTextAndLiteralChoices()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "o" }));

                try
                {
                    var prompt = new ConfirmationPrompt("[green]Deploy[/] [[prod]]")
                    {
                        Yes = 'o',
                        No = 'x',
                        DefaultValue = false
                    };

                    ConfirmResult result = prompt.Render();

                    Assert.True(result.HasValue);
                    Assert.True(result.Value);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Deploy") && write.Color == ConsoleColor.Green);
                    Assert.Contains("[prod]", scope.Driver.Text);
                    Assert.Contains("[o/X]", scope.Driver.Text);
                    Assert.DoesNotContain("[green]", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Text prompt validation copy parses raw markup while generated accepted-value messages keep model values literal.
        /// </summary>
        [Fact]
        public void TextPrompt_RawMarkupValidationMessages_RenderVisibleTextAndGeneratedValuesStayLiteral()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "abc", "-1", "3" }));

                try
                {
                    TextPromptResult<int> result = Consoul.Text<int>("Retry count")
                        .WithInvalidMessage("[cyan]Enter a whole number[/] [[prod]]")
                        .Validate(value => value >= 0 ? null : "[yellow]Retry count cannot be negative[/] [[prod]]")
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal(3, result.Value);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Enter a whole number") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Retry count cannot be negative") && write.Color == ConsoleColor.Yellow);
                    Assert.Contains("[prod]", scope.Driver.Text);
                    Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                    Assert.DoesNotContain("[yellow]", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }

            using (var scope = new ConsoleDriverScope(64))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "qa", "[green]prod[/]" }));

                try
                {
                    TextPromptResult<string> result = Consoul.Text<string>("Environment")
                        .AcceptOnly(new[] { "[green]prod[/]" })
                        .Render();

                    Assert.True(result.HasValue);
                    Assert.Equal("[green]prod[/]", result.Value);
                    Assert.Contains("[green]prod[/]", scope.Driver.Text);
                    Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("prod") && write.Color == ConsoleColor.Green);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Secret prompt required and validation messages parse raw markup without leaking the typed value.
        /// </summary>
        [Fact]
        public void SecretPrompt_RawMarkupValidationMessages_RenderVisibleTextWithoutLeakingSecret()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                scope.Driver.EnqueueKey(Enter());
                EnqueueText(scope.Driver, "tiny");
                scope.Driver.EnqueueKey(Enter());
                EnqueueText(scope.Driver, "long-secret");
                scope.Driver.EnqueueKey(Enter());

                SecretResult result = new SecretPrompt("Token")
                    .RequiredErrorMessage("[cyan]Token is required[/] [[prod]]")
                    .Validate(value => value.Length >= 8 ? null : "[yellow]Token length invalid[/] [[prod]]")
                    .Render();

                Assert.True(result.HasValue);
                Assert.Equal("long-secret", result.Value);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Token is required") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Token length invalid") && write.Color == ConsoleColor.Yellow);
                Assert.Contains("[prod]", scope.Driver.Text);
                Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                Assert.DoesNotContain("[yellow]", scope.Driver.Text);
                Assert.DoesNotContain("tiny", scope.Driver.Text);
                Assert.DoesNotContain("long-secret", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Confirmation invalid-input messages parse raw markup for both Consoul and Spectre-shaped prompts.
        /// </summary>
        [Fact]
        public void ConfirmationPrompts_RawMarkupInvalidMessages_RenderVisibleTextAndColors()
        {
            using (var scope = new ConsoleDriverScope(56))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "maybe", "y", "later", "o" }));

                try
                {
                    ConfirmResult confirm = new ConfirmPrompt("Deploy?")
                        .ValidationErrorMessage("[cyan]Use yes or no[/] [[prod]]")
                        .Render();

                    var confirmationPrompt = new ConfirmationPrompt("Deploy with custom choice?")
                    {
                        Yes = 'o',
                        No = 'x'
                    }
                    .InvalidChoiceMessage("[yellow]Choose o or x[/] [[prod]]");

                    ConfirmResult confirmation = confirmationPrompt.Render();

                    Assert.True(confirm.HasValue);
                    Assert.True(confirm.Value);
                    Assert.True(confirmation.HasValue);
                    Assert.True(confirmation.Value);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Use yes or no") && write.Color == ConsoleColor.Cyan);
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Choose o or x") && write.Color == ConsoleColor.Yellow);
                    Assert.Contains("[prod]", scope.Driver.Text);
                    Assert.DoesNotContain("[cyan]", scope.Driver.Text);
                    Assert.DoesNotContain("[yellow]", scope.Driver.Text);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        private static void EnqueueText(FakeConsoleDriver driver, string text)
        {
            foreach (char ch in text)
            {
                driver.EnqueueKey(new ConsoleKeyInfo(ch, ConsoleKey.A, false, false, false));
            }
        }

        private static ConsoleKeyInfo Enter()
        {
            return new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false);
        }

        private static string AnsiPrefixBefore(string output, string text)
        {
            int textIndex = output.IndexOf(text, StringComparison.Ordinal);
            Assert.True(textIndex >= 0, "Expected rendered output to contain \"" + text + "\".");

            int lineStart = Math.Max(output.LastIndexOf('\n', textIndex), output.LastIndexOf('\r', textIndex));
            string prefix = output.Substring(lineStart + 1, textIndex - lineStart - 1);
            Assert.Contains("\u001b[", prefix);

            return prefix;
        }

        private static void AssertAnsiCode(string sequence, string code)
        {
            Assert.Contains(code, GetAnsiCodes(sequence));
        }

        private static void AssertAnsiRgb(string sequence, byte red, byte green, byte blue)
        {
            Assert.Contains("38;2;" + red + ";" + green + ";" + blue, sequence);
        }

        private static IEnumerable<string> GetAnsiCodes(string sequence)
        {
            int searchIndex = 0;
            while (searchIndex < sequence.Length)
            {
                int escapeIndex = sequence.IndexOf("\u001b[", searchIndex, StringComparison.Ordinal);
                if (escapeIndex < 0)
                {
                    yield break;
                }

                int endIndex = sequence.IndexOf('m', escapeIndex);
                Assert.True(endIndex >= 0, "Expected an ANSI SGR sequence to end with 'm'.");
                string[] codes = sequence.Substring(escapeIndex + 2, endIndex - escapeIndex - 2).Split(';');
                foreach (string code in codes)
                {
                    yield return code;
                }

                searchIndex = endIndex + 1;
            }
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
    }
}
