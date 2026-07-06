using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsoulLibrary.Tests.Rendering;
using Xunit;

namespace ConsoulLibrary.Tests.Compatibility
{
    /// <summary>
    /// Tests migration-friendly Spectre-style entry points.
    /// </summary>
    public class AnsiConsoleFacadeTests
    {
        /// <summary>
        /// Markup facade methods delegate to Consoul markup rendering.
        /// </summary>
        [Fact]
        public void MarkupLine_DelegatesToConsoulMarkup()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                AnsiConsole.MarkupLine("Deploy [green]ready[/].");

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("ready") && write.Color == ConsoleColor.Green);
                VisualAssert.ContainsText(scope.Driver, "Deploy ready.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Formatted markup facade overloads support common Spectre-style composite-format snippets.
        /// </summary>
        [Fact]
        public void MarkupLine_FormatOverloadEscapesArguments()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                AnsiConsole.MarkupLine(
                    CultureInfo.InvariantCulture,
                    "Deploy [green]{0}[/] after [cyan]{1:0.0}[/] checks.",
                    "Production [red]hot[/]",
                    7.5);

                AnsiConsole.Markup(" State: [yellow]{0}[/]", "Needs [approval]");

                VisualAssert.ContainsText(scope.Driver, "Deploy Production [red]hot[/] after 7.5 checks.");
                VisualAssert.ContainsText(scope.Driver, "State: Needs [approval]");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Production [red]hot[/]") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("7.5") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Needs [approval]") && write.Color == ConsoleColor.Yellow);
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text.Contains("hot") && write.Color == ConsoleColor.Red);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style plain output overloads render through the same Consoul console driver.
        /// </summary>
        [Fact]
        public void WriteOverloads_RenderFormattedValuesAndAnsiActions()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                AnsiConsole.Write(42);
                AnsiConsole.Write('|');
                AnsiConsole.Write(new[] { 'O', 'K' });
                AnsiConsole.Write('|');
                AnsiConsole.Write((object)new OutputLabel("object"));
                AnsiConsole.Write('|');
                AnsiConsole.Write((object)new Markup("[green]boxed[/]"));
                AnsiConsole.Write('|');
                AnsiConsole.Write(CultureInfo.InvariantCulture, "|{0:0.0}|", 3.5);
                AnsiConsole.WriteLine((object)new OutputLabel("line-object"));
                AnsiConsole.WriteLine(true);
                AnsiConsole.WriteLine(CultureInfo.InvariantCulture, "{0:0.00}", 12.5m);
                AnsiConsole.WriteAnsi(() => AnsiConsole.Write("raw-ansi"));

                VisualAssert.ContainsText(scope.Driver, "42|OK|object|boxed||3.5|line-object");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("boxed") && write.Color == ConsoleColor.Green);
                VisualAssert.ContainsText(scope.Driver, "True");
                VisualAssert.ContainsText(scope.Driver, "12.50");
                VisualAssert.ContainsText(scope.Driver, "raw-ansi");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// AnsiConsole.Create builds an isolated writer-backed facade for Spectre-style test setup.
        /// </summary>
        [Fact]
        public void Create_WithTextWriterOutput_RendersThroughConfiguredWriter()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 40, height: 12)
            });

            console.MarkupLine("Deploy [green]ready[/].");
            console.Panel("Writer panel", new PanelOptions { Width = 28, Title = "Created" });

            string output = writer.ToString();

            Assert.Equal(40, console.Profile.Width);
            Assert.Equal(12, console.Profile.Height);
            Assert.True(console.Profile.IsVirtual);
            Assert.True(console.Profile.IsOutputRedirected);
            Assert.Contains("Deploy ready.", output);
            Assert.Contains("Created", output);
            Assert.Contains("Writer panel", output);
        }

        /// <summary>
        /// Created consoles can be assigned to the static facade without leaking output to the process console driver.
        /// </summary>
        [Fact]
        public void Create_AssignedFacadeRoutesStaticCallsToConfiguredWriter()
        {
            IAnsiConsole previous = AnsiConsole.Console;
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole created = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 36, height: 10)
            });

            using (var scope = new ConsoleDriverScope(80))
            {
                try
                {
                    AnsiConsole.Console = created;

                    AnsiConsole.MarkupLine("[cyan]Created console[/]");
                    AnsiConsole.Cursor.SetPosition(2, 1);
                    AnsiConsole.WriteLine("Writer only");

                    Assert.Contains("Created console", writer.ToString());
                    Assert.Contains("Writer only", writer.ToString());
                    Assert.DoesNotContain("Created console", scope.Driver.Text);
                    Assert.Equal(0, scope.Driver.CursorLeft);
                    Assert.Equal(0, scope.Driver.CursorTop);
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    AnsiConsole.Console = previous;
                }
            }
        }

        /// <summary>
        /// Assigned scoped facades preserve Spectre-style clear home intent through the static facade.
        /// </summary>
        [Fact]
        public void Create_AssignedScopedFacadePreservesClearHomeIntent()
        {
            IAnsiConsole previous = AnsiConsole.Console;
            var driver = new NonHomingClearConsoleDriver();
            IAnsiConsole created = new ConsoulAnsiConsole(driver);

            try
            {
                AnsiConsole.Console = created;

                driver.SetCursorPosition(5, 2);
                AnsiConsole.Clear(false);

                Assert.Equal(1, driver.ClearCount);
                Assert.Equal(5, driver.CursorLeft);
                Assert.Equal(2, driver.CursorTop);

                AnsiConsole.Clear(true);

                Assert.Equal(2, driver.ClearCount);
                Assert.Equal(0, driver.CursorLeft);
                Assert.Equal(0, driver.CursorTop);
            }
            finally
            {
                AnsiConsole.Console = previous;
            }
        }

        /// <summary>
        /// Alternate-screen helpers emit enter and exit sequences around scoped dashboard output.
        /// </summary>
        [Fact]
        public void AlternateScreen_RunsActionAndRestoresMainScreen()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                bool ran = false;

                int result = AnsiConsole.AlternateScreen(() =>
                {
                    ran = true;
                    AnsiConsole.MarkupLine("[green]Dashboard ready[/]");
                    return 42;
                });

                Assert.True(ran);
                Assert.Equal(42, result);
                Assert.True(scope.Driver.ClearCount >= 1);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("\u001b[?1049h"));
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("\u001b[?1049l"));
                VisualAssert.ContainsText(scope.Driver, "Dashboard ready");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Alternate-screen helpers run as no-op wrappers when ANSI support is explicitly disabled.
        /// </summary>
        [Fact]
        public void AlternateScreen_DisabledAnsiRunsWithoutEscapeSequences()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 48, height: 12, isTerminal: true),
                Ansi = AnsiSupport.No,
                Interactive = InteractionSupport.No
            });

            using (AlternateScreenScope scope = console.AlternateScreen())
            {
                Assert.False(scope.IsEnabled);
                console.MarkupLine("[cyan]Fallback dashboard[/]");
            }

            string output = writer.ToString();
            Assert.Contains("Fallback dashboard", output);
            Assert.DoesNotContain("\u001b[?1049h", output);
            Assert.DoesNotContain("\u001b[?1049l", output);
        }

        /// <summary>
        /// Async alternate-screen helpers dispose their scope when the action fails.
        /// </summary>
        [Fact]
        public async Task AlternateScreenAsync_DisposesScopeWhenActionFails()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    AnsiConsole.AlternateScreenAsync(async cancellationToken =>
                    {
                        await Task.Delay(1, cancellationToken);
                        AnsiConsole.WriteLine("Before failure");
                        throw new InvalidOperationException("alternate failed");
                    }, CancellationToken.None));

                Assert.Equal("alternate failed", exception.Message);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("\u001b[?1049h"));
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("\u001b[?1049l"));
                VisualAssert.ContainsText(scope.Driver, "Before failure");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Created consoles record their own writer-backed output without relying on global recording state.
        /// </summary>
        [Fact]
        public void Create_RecordExportsConfiguredConsoleOutput()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 40, height: 12)
            });

            console.Record();
            console.MarkupLine("Recorded [green]ready[/].");

            string text = console.ExportText();
            string html = console.ExportHtml();

            Assert.Contains("Recorded ready.", text);
            Assert.Contains("Recorded ready.", writer.ToString());
            Assert.Contains("<pre", html);
            Assert.Contains("Recorded ready.", html);
        }

        /// <summary>
        /// Created consoles honor explicit Spectre-style profile capability settings.
        /// </summary>
        [Fact]
        public void Create_ProfileSettingsOverrideDetectedCapabilities()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 50, height: 8, isTerminal: false),
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.TrueColor,
                Interactive = InteractionSupport.Yes
            });

            TerminalProfile profile = console.Profile;

            Assert.Equal(50, profile.Width);
            Assert.Equal(8, profile.Height);
            Assert.True(profile.IsVirtual);
            Assert.False(profile.IsInputRedirected);
            Assert.False(profile.IsOutputRedirected);
            Assert.True(profile.IsInteractive);
            Assert.True(profile.SupportsColor);
            Assert.True(profile.Capabilities.Ansi);
            Assert.True(profile.Capabilities.Unicode);
            Assert.True(profile.Capabilities.Links);
            Assert.True(profile.Capabilities.Interactive);
            Assert.False(profile.Capabilities.Legacy);
            Assert.Equal(ColorSystem.TrueColor, profile.Capabilities.ColorSystem);

            console.Record();
            Assert.Equal(ColorSystem.TrueColor, console.Profile.Capabilities.ColorSystem);
            Assert.True(console.Profile.Capabilities.Interactive);
        }

        /// <summary>
        /// Created consoles can be configured as no-color, non-ANSI, non-interactive targets for deterministic tests.
        /// </summary>
        [Fact]
        public void Create_ProfileSettingsCanDisableAnsiColorAndInteraction()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(writer, width: 44, height: 6, isTerminal: true),
                Ansi = AnsiSupport.No,
                ColorSystem = ColorSystemSupport.NoColors,
                Interactive = InteractionSupport.No
            });

            TerminalProfile profile = console.Profile;

            Assert.Equal(44, profile.Width);
            Assert.Equal(6, profile.Height);
            Assert.True(profile.IsVirtual);
            Assert.True(profile.IsInputRedirected);
            Assert.False(profile.IsOutputRedirected);
            Assert.False(profile.IsInteractive);
            Assert.False(profile.SupportsColor);
            Assert.False(profile.Capabilities.Ansi);
            Assert.False(profile.Capabilities.Unicode);
            Assert.False(profile.Capabilities.Links);
            Assert.False(profile.Capabilities.Interactive);
            Assert.True(profile.Capabilities.Legacy);
            Assert.Equal(ColorSystem.NoColors, profile.Capabilities.ColorSystem);
        }

        /// <summary>
        /// Created consoles consume configured text-reader input for typed prompts without routine setup.
        /// </summary>
        [Fact]
        public void Create_WithTextReaderInput_AnswersTextPromptThroughConfiguredConsole()
        {
            Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
            try
            {
                var writer = new StringWriter(CultureInfo.InvariantCulture);
                IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
                {
                    Out = new AnsiConsoleOutput(writer, width: 52, height: 12),
                    In = new StringReader("Production\n")
                });

                TextPromptResult<string> result = console.Prompt(
                    new TextPrompt<string>("Target")
                        .AddChoices("Development", "Production"));

                Assert.True(result.HasValue);
                Assert.Equal("Production", result.Value);
                Assert.False(console.Profile.IsInputRedirected);
                Assert.Contains("Target", writer.ToString());
                Assert.Contains("Development/Production", writer.ToString());
            }
            finally
            {
                Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
            }
        }

        /// <summary>
        /// Created-console input streams keep unread input available across prompt re-renders.
        /// </summary>
        [Fact]
        public void Create_WithTextReaderInput_ReusesRemainingInputAfterInvalidSelection()
        {
            Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
            try
            {
                var writer = new StringWriter(CultureInfo.InvariantCulture);
                IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
                {
                    Out = new AnsiConsoleOutput(writer, width: 52, height: 12),
                    In = new StringReader("oops\n2\n")
                });
                var prompt = new SelectionPrompt("Pick an action")
                    .AddChoices("Build", "Deploy");

                PromptResult result = console.Prompt(prompt);

                Assert.True(result.HasSelection);
                Assert.Equal(1, result.Index);
                Assert.Contains("Invalid selection!", writer.ToString());
                Assert.Contains("Deploy", writer.ToString());
            }
            finally
            {
                Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
            }
        }

        /// <summary>
        /// Created consoles provide key-style input for editable prompt buffers as well as stream-based reads.
        /// </summary>
        [Fact]
        public void Create_WithTextReaderInput_ProvidesReadKeyInputForEditablePrompts()
        {
            Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
            try
            {
                var writer = new StringWriter(CultureInfo.InvariantCulture);
                IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
                {
                    Out = new AnsiConsoleOutput(writer, width: 52, height: 12),
                    In = new StringReader("ops\n")
                });
                var prompt = new TextPrompt<string>("Service")
                    .DefaultValue("dev");
                prompt.EditableDefaultValue = true;

                TextPromptResult<string> result = console.Prompt(prompt);

                Assert.True(result.HasValue);
                Assert.Equal("devops", result.Value);
                Assert.Contains("Service", writer.ToString());
                Assert.Contains("devops", writer.ToString());
            }
            finally
            {
                Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
            }
        }

        /// <summary>
        /// Selection prompt wrappers preserve Consoul's result-first return type.
        /// </summary>
        [Fact]
        public void Prompt_SelectionPrompt_ReturnsPromptResult()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2" }));

                try
                {
                    var prompt = new SelectionPrompt("Pick an action");
                    prompt.Add("Build");
                    prompt.Add("Deploy");

                    PromptResult result = AnsiConsole.Prompt(prompt);

                    Assert.True(result.HasSelection);
                    Assert.Equal(1, result.Index);
                    VisualAssert.ContainsText(scope.Driver, "Pick an action");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Typed text prompt wrappers return explicit typed prompt results.
        /// </summary>
        [Fact]
        public void Prompt_TextPrompt_ReturnsTypedResult()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "42" }));

                try
                {
                    TextPromptResult<int> result = AnsiConsole.Prompt(Consoul.Text<int>("Port"));

                    Assert.True(result.HasValue);
                    Assert.Equal(42, result.Value);
                    VisualAssert.ContainsText(scope.Driver, "Port");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Prompt abstractions and async convenience methods keep Consoul's result-first prompt model.
        /// </summary>
        [Fact]
        public async Task PromptInterfaceAndAsyncWrappers_PreserveResultFirstTypes()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                Routines.InitializeRoutine(new TestRoutine(new[] { "2", "", "yes" }));

                try
                {
                    IPrompt<SelectionResult<string>> targetPrompt = new SelectionPrompt<string>("Pick target")
                        .AddChoices("Dev", "Prod");

                    SelectionResult<string> target = await AnsiConsole.PromptAsync(targetPrompt);
                    TextPromptResult<int> retries = AnsiConsole.Ask("Retries", 3);
                    ConfirmResult deploy = await AnsiConsole.ConfirmAsync("Deploy?", defaultValue: false);

                    Assert.True(target.HasSelection);
                    Assert.Equal("Prod", target.SelectedItem);
                    Assert.True(retries.HasValue);
                    Assert.Equal(3, retries.Value);
                    Assert.True(deploy.HasValue);
                    Assert.True(deploy.Value);
                    VisualAssert.ContainsText(scope.Driver, "Pick target");
                    VisualAssert.ContainsText(scope.Driver, "Retries");
                    VisualAssert.ContainsText(scope.Driver, "Deploy?");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Visual facade methods render through existing responsive components.
        /// </summary>
        [Fact]
        public void VisualWrappers_RenderThroughConsoulComponents()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                AnsiConsole.Rule("Migration");
                AnsiConsole.Panel("Compatibility entry points", new PanelOptions { Width = 42, Title = "AnsiConsole" });
                AnsiConsole.Table(
                    new[]
                    {
                        new Job("Build", "Ready"),
                        new Job("Deploy", "Waiting")
                    },
                    job => job.Name,
                    job => job.State);

                VisualAssert.ContainsText(scope.Driver, "Migration");
                VisualAssert.ContainsText(scope.Driver, "AnsiConsole");
                VisualAssert.ContainsText(scope.Driver, "Build");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Legacy Render aliases Write for reusable renderable components.
        /// </summary>
        [Fact]
        public void Render_RendersReusableComponents()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                AnsiConsole.Render(new Markup("[green]Ready[/]").NewLine());
                AnsiConsole.Render(new Panel("Renderable alias").Header("Render").Width(36));
                AnsiConsole.Console.Render(new Markup("[cyan]Injected alias[/]").NewLine());

                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.Green);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Injected alias") && write.Color == ConsoleColor.Cyan);
                VisualAssert.ContainsText(scope.Driver, "Renderable alias");
                VisualAssert.ContainsText(scope.Driver, "Render");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Expanded visual facade methods delegate to Consoul's MVC-style render helpers.
        /// </summary>
        [Fact]
        public void AdditionalVisualWrappers_RenderThroughConsoulComponents()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                AnsiConsole.Badge("ready", BadgeKind.Success);
                AnsiConsole.Headline("POC", width: 32, color: ConsoleColor.Cyan, style: HeadlineStyle.Rule);
                AnsiConsole.FigletText("OK", width: 32, color: ConsoleColor.Green);
                AnsiConsole.Calendar(new DateTime(2026, 6, 1), calendar =>
                {
                    calendar.Add(new DateTime(2026, 6, 15), "#22c55e", "Release");
                }, new CalendarOptions { Width = 32 });
                AnsiConsole.Callout("Approval required.", CalloutKind.Warning, new CalloutOptions { Width = 40 });
                AnsiConsole.Breadcrumb("Home", "Deployments", "Production");
                AnsiConsole.ShortcutBar(bar =>
                {
                    bar.AddStyled("Enter", "Select", keyStyle: "aqua", descriptionStyle: "grey");
                    bar.AddStyled("Esc", "Back", keyStyle: "yellow");
                }, new ShortcutBarOptions { Width = 40 });
                AnsiConsole.BulletList(list =>
                {
                    BulletListItem checks = list.AddStyled("Checks", "white", "yellow");
                    checks.AddStyled("Build", "aqua");
                }, new BulletListOptions { Width = 40 });
                AnsiConsole.Checklist(list =>
                {
                    list.AddStyled("Build", ChecklistItemState.Complete, labelStyle: "aqua");
                    list.AddStyled("Approval", ChecklistItemState.Pending, "Waiting", labelStyle: "yellow", descriptionStyle: "grey");
                }, new ChecklistOptions { Width = 40 });
                AnsiConsole.Timeline(timeline =>
                {
                    timeline.AddStyled("Build", TimelineItemState.Complete, titleStyle: "aqua");
                    timeline.AddStyled("Deploy", TimelineItemState.Active, titleStyle: "yellow");
                }, new TimelineOptions { Width = 40 });
                AnsiConsole.PropertyList(list =>
                {
                    list.Add("Owner", "Platform", ConsoleColor.Cyan);
                    list.Add("State", "Waiting", ConsoleColor.Yellow);
                }, new PropertyListOptions { Width = 40 });
                AnsiConsole.Metrics(metrics =>
                {
                    metrics.AddStyled("Passing", 8, "grey", "#22c55e", unit: "jobs", kind: MetricKind.Success);
                    metrics.AddStyled("Waiting", 2, "grey", "yellow", kind: MetricKind.Warning);
                }, new MetricsOptions { Width = 40 });
                AnsiConsole.Sparkline(new[] { 1d, 3d, 2d, 5d }, "Trend", width: 40, color: ConsoleColor.Green, showRange: true);
                AnsiConsole.BarChart(chart =>
                {
                    chart.Add("Ready", 3, "#22c55e");
                }, new BarChartOptions { Width = 40, MaximumValue = 5 });
                AnsiConsole.BreakdownChart(chart =>
                {
                    chart.Add("Queued", 2, "yellow");
                    chart.Add("Running", 1, "#22c55e");
                }, new BreakdownChartOptions { Width = 40 });
                AnsiConsole.Grid(grid =>
                {
                    grid.Add("Build", "Job", "grey");
                    grid.Add("Ready", "State", "aqua");
                }, new GridOptions { Width = 40, Columns = 2 });
                AnsiConsole.Rows(rows =>
                {
                    rows.Add("Summary", "grey");
                    rows.Add(new Markup("[green]Ready[/]"), "aqua");
                }, new RowsOptions { Width = 40 });
                AnsiConsole.Layout(layout =>
                {
                    layout.Add("Ready", "API", "#22c55e");
                    layout.Add("Waiting", "Worker", "yellow");
                }, new LayoutOptions { Width = 48, Columns = 2 });
                AnsiConsole.Canvas(12, 3, canvas =>
                {
                    canvas.Rectangle(0, 0, 11, 2, '#', "grey");
                    canvas.Text(3, 1, "API", "aqua");
                });
                AnsiConsole.TextBlock("Wrapped deployment summary", width: 32, alignment: TextBlockAlignment.Center, color: ConsoleColor.White);
                AnsiConsole.TextPathStyled(@"C:\deployments\release\notes.txt", width: 40, fileNameStyle: "aqua");
                AnsiConsole.Pad("Indented", left: 2, width: 20);
                AnsiConsole.Align("Centered", TextBlockAlignment.Center, width: 20);
                ValidationSummaryResult validation = AnsiConsole.ValidationSummary(
                    new InvalidSettings(),
                    new ValidationSummaryOptions { Width = 40 });

                Assert.False(validation.IsValid);
                Assert.Contains("READY", AnsiConsole.BadgeText("ready", BadgeKind.Success));
                Assert.Contains("Home", AnsiConsole.BreadcrumbText("Home", "Deployments"));
                Assert.Contains("[Enter]", AnsiConsole.ShortcutBarText(new ShortcutItem("Enter", "Select")));
                VisualAssert.ContainsText(scope.Driver, "Approval required");
                VisualAssert.ContainsText(scope.Driver, "Production");
                VisualAssert.ContainsText(scope.Driver, "Passing");
                VisualAssert.ContainsText(scope.Driver, "Trend");
                VisualAssert.ContainsText(scope.Driver, "Owner");
                VisualAssert.ContainsText(scope.Driver, "API");
                VisualAssert.ContainsText(scope.Driver, "Name");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("A") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Build") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Summary") && write.Color == ConsoleColor.Gray);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Ready") && write.Color == ConsoleColor.DarkCyan);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Status and progress facade methods keep the scoped helper behavior intact.
        /// </summary>
        [Fact]
        public void ScopedWrappers_RunExistingStatusAndProgressHelpers()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                AnsiConsole.Status("Preparing", status => status.Succeed("Ready"));
                AnsiConsole.Progress(progress =>
                {
                    ProgressTask task = progress.AddTask("Package", 2);
                    task.Increment(1);
                    task.Complete();
                });

                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.ContainsText(scope.Driver, "Package");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Static composition helpers accept mixed plain values and renderables for quick POC dashboards.
        /// </summary>
        [Fact]
        public void StaticCompositionHelpers_AcceptMixedObjectRowsAndColumns()
        {
            using (var scope = new ConsoleDriverScope(64))
            {
                AnsiConsole.Rows(
                    new RowsOptions { Width = 36, Spacing = 0 },
                    "Summary",
                    new Markup("[green]Ready[/]"),
                    2);

                AnsiConsole.Columns(
                    new ColumnsOptions { Width = 48, Columns = 3, MinimumColumnWidth = 4, Gutter = 1 },
                    "API",
                    new Markup("[green]Ready[/]"),
                    3);

                AnsiConsole.Grid(
                    new GridOptions { Width = 48, Columns = 3, MinimumColumnWidth = 4, Gutter = 1 },
                    "Worker",
                    new Markup("[yellow]Queued[/]"),
                    4);

                VisualAssert.ContainsText(scope.Driver, "Summary");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.ContainsText(scope.Driver, "2");
                VisualAssert.ContainsText(scope.Driver, "API");
                VisualAssert.ContainsText(scope.Driver, "3");
                VisualAssert.ContainsText(scope.Driver, "Worker");
                VisualAssert.ContainsText(scope.Driver, "Queued");
                VisualAssert.ContainsText(scope.Driver, "4");
                Assert.All(scope.Driver.Lines, line => Assert.True(line.Length <= 48, "Mixed facade composition line exceeded configured width: " + line));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style start wrappers run the same scoped progress helpers without changing Consoul's MVC-style view model flow.
        /// </summary>
        [Fact]
        public async Task SpectreStyleStartWrappers_RunScopedHelpers()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                void AssertWritten(string expected)
                {
                    Assert.Contains(scope.Driver.Writes, write => write.Text.Contains(expected));
                }

                IAnsiConsole console = AnsiConsole.Console;

                int statusResult = AnsiConsole.Status()
                    .Spinner(Spinner.Known.Star)
                    .SpinnerStyle("yellow")
                    .AutoRefresh(false)
                    .Start("Preparing", status =>
                    {
                        status.Status("Builder ready");
                        status.Refresh();
                        status.Spinner(Spinner.Known.Dots);
                        status.Status("Builder finalizing");
                        status.Refresh();
                        return 42;
                    });
                string asyncStatusResult = await AnsiConsole.Status().StartAsync("Async preparing", async status =>
                {
                    status.Succeed("Builder async ready");
                    await Task.Delay(1);
                    return "ready";
                });

                string spinnerResult = AnsiConsole.Spinner()
                    .Spinner(Spinner.Known.Dots)
                    .SpinnerStyle("aqua")
                    .AutoRefresh(false)
                    .Start("Packaging", spinner =>
                    {
                        spinner.Tick("Builder packaged");
                        spinner.Refresh();
                        return "packaged";
                    });
                string asyncSpinnerResult = await AnsiConsole.Spinner().StartAsync("Async packaging", async spinner =>
                {
                    spinner.Succeed("Builder async packaged");
                    await Task.Delay(1);
                    return "packaged";
                });

                AnsiConsole.Progress(new ProgressContextOptions { Width = 64, LabelWidth = 28 }).Start(progress =>
                {
                    ProgressTask task = progress.AddTask("Builder upload", 2);
                    task.Complete();
                });

                await AnsiConsole.Progress(new ProgressContextOptions { Width = 64, LabelWidth = 28 }).StartAsync((progress, cancellationToken) =>
                {
                    ProgressTask task = progress.AddTask("Builder async upload", 2);
                    task.Complete();
                    return Task.CompletedTask;
                });

                await AnsiConsole.ProgressAsync((progress, cancellationToken) =>
                {
                    ProgressTask task = progress.AddTask("Facade async upload", 2);
                    task.Complete();
                    return Task.CompletedTask;
                }, new ProgressContextOptions { Width = 64, LabelWidth = 28 });
                await AnsiConsole.ProgressAsync(async progress =>
                {
                    await Task.Yield();
                    ProgressTask task = progress.AddTask("Facade quick async upload", 2);
                    task.Complete();
                }, new ProgressContextOptions { Width = 64, LabelWidth = 28 });

                AnsiConsole.Live(new LiveDisplayOptions
                {
                    Title = "Builder live",
                    ClearBeforeRender = false,
                    ClearOnDispose = false
                }).Start(live =>
                {
                    live.Refresh(() => AnsiConsole.MarkupLine("[cyan]Live start summary[/]"));
                    live.Succeed("Live start ready");
                });

                await AnsiConsole.Live(new LiveDisplayOptions
                {
                    Title = "Builder async live",
                    ClearBeforeRender = false,
                    ClearOnDispose = false
                }).StartAsync((live, cancellationToken) =>
                {
                    live.Refresh(() => AnsiConsole.MarkupLine("[cyan]Live async start summary[/]"));
                    live.Succeed("Live async start ready");
                    return Task.CompletedTask;
                });

                await AnsiConsole.LiveAsync((live, cancellationToken) =>
                {
                    live.Refresh(() => AnsiConsole.MarkupLine("[cyan]Facade live summary[/]"));
                    live.Succeed("Facade live ready");
                    return Task.CompletedTask;
                }, new LiveDisplayOptions
                {
                    Title = "Facade live",
                    ClearBeforeRender = false,
                    ClearOnDispose = false
                });
                await AnsiConsole.LiveAsync(async live =>
                {
                    await Task.Yield();
                    live.Refresh(() => AnsiConsole.MarkupLine("[cyan]Facade quick live summary[/]"));
                    live.Succeed("Facade quick live ready");
                }, new LiveDisplayOptions
                {
                    Title = "Facade quick live",
                    ClearBeforeRender = false,
                    ClearOnDispose = false
                });

                console.Status().Start("Injected status", status => status.Succeed("Injected status ready"));
                await console.StatusAsync("Injected async status", async status =>
                {
                    await Task.Yield();
                    status.Succeed("Injected quick status ready");
                });
                console.Spinner().Start("Injected spinner", spinner => spinner.Succeed("Injected spinner ready"));
                await console.SpinnerAsync("Injected async spinner", async spinner =>
                {
                    await Task.Yield();
                    spinner.Succeed("Injected quick spinner ready");
                });
                await console.ProgressAsync((progress, cancellationToken) =>
                {
                    ProgressTask task = progress.AddTask("Injected upload", 2);
                    task.Complete();
                    return Task.CompletedTask;
                }, new ProgressContextOptions { Width = 64, LabelWidth = 28 });
                await console.ProgressAsync(async progress =>
                {
                    await Task.Yield();
                    ProgressTask task = progress.AddTask("Injected quick upload", 2);
                    task.Complete();
                }, new ProgressContextOptions { Width = 64, LabelWidth = 28 });
                await console.LiveAsync((live, cancellationToken) =>
                {
                    live.Refresh(() => console.MarkupLine("[cyan]Injected live summary[/]"));
                    live.Succeed("Injected live ready");
                    return Task.CompletedTask;
                }, new LiveDisplayOptions
                {
                    Title = "Injected live",
                    ClearBeforeRender = false,
                    ClearOnDispose = false
                });
                await console.LiveAsync(async live =>
                {
                    await Task.Yield();
                    live.Refresh(() => console.MarkupLine("[cyan]Injected quick live summary[/]"));
                    live.Succeed("Injected quick live ready");
                }, new LiveDisplayOptions
                {
                    Title = "Injected quick live",
                    ClearBeforeRender = false,
                    ClearOnDispose = false
                });

                Assert.Equal(42, statusResult);
                Assert.Equal("ready", asyncStatusResult);
                Assert.Equal("packaged", spinnerResult);
                Assert.Equal("packaged", asyncSpinnerResult);
                AssertWritten("Builder ready");
                AssertWritten("Builder finalizing");
                AssertWritten("Builder async ready");
                AssertWritten("Builder packaged");
                AssertWritten("Builder async packaged");
                AssertWritten("Builder upload");
                AssertWritten("Builder async upload");
                AssertWritten("Facade async upload");
                AssertWritten("Facade quick async upload");
                AssertWritten("Live start summary");
                AssertWritten("Live async start summary");
                AssertWritten("Facade live summary");
                AssertWritten("Facade quick live summary");
                AssertWritten("Injected status ready");
                AssertWritten("Injected quick status ready");
                AssertWritten("Injected spinner ready");
                AssertWritten("Injected quick spinner ready");
                AssertWritten("Injected upload");
                AssertWritten("Injected quick upload");
                AssertWritten("Injected live summary");
                AssertWritten("Injected quick live summary");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Builder ready") && write.Color == ConsoleColor.Yellow);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Result-returning async scoped helpers keep quick controller code value-oriented without leaving scoped rendering.
        /// </summary>
        [Fact]
        public async Task ResultReturningScopedAsyncHelpers_ReturnValuesThroughStaticCoreAndInjectedFacades()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                IAnsiConsole console = AnsiConsole.Console;

                int coreStatus = await ConsoulLibrary.Consoul.StatusAsync("Core status", async status =>
                {
                    await Task.Yield();
                    status.Succeed("Core status ready");
                    return 11;
                });
                string coreSpinner = await ConsoulLibrary.Consoul.SpinnerAsync("Core spinner", (spinner, cancellationToken) =>
                {
                    spinner.Succeed("Core spinner ready");
                    return Task.FromResult("spinner");
                });
                int coreProgress = await ConsoulLibrary.Consoul.ProgressAsync(async progress =>
                {
                    await Task.Yield();
                    ProgressTask task = progress.AddTask("Core progress", 1);
                    task.Complete();
                    return 12;
                }, new ProgressContextOptions { Width = 64, LabelWidth = 24 });
                string coreLive = await ConsoulLibrary.Consoul.LiveAsync(new Text("Core target"), async live =>
                {
                    await Task.Yield();
                    live.Refresh(() => ConsoulLibrary.Consoul.MarkupLine("[green]Core live summary[/]"));
                    live.Succeed("Core live ready");
                    return "live";
                }, new LiveDisplayOptions
                {
                    ClearBeforeRender = false,
                    ClearOnDispose = false
                });

                int facadeStatus = await AnsiConsole.StatusAsync("Facade status", async status =>
                {
                    await Task.Yield();
                    status.Succeed("Facade status ready");
                    return 21;
                });
                int facadeProgress = await AnsiConsole.ProgressAsync((progress, cancellationToken) =>
                {
                    ProgressTask task = progress.AddTask("Facade result progress", 1);
                    task.Complete();
                    return Task.FromResult(22);
                }, new ProgressContextOptions { Width = 64, LabelWidth = 24 });
                string injectedLive = await console.LiveAsync(new Text("Injected target"), async live =>
                {
                    await Task.Yield();
                    live.Refresh(() => console.MarkupLine("[cyan]Injected result live summary[/]"));
                    live.Succeed("Injected result live ready");
                    return "injected";
                }, new LiveDisplayOptions
                {
                    ClearBeforeRender = false,
                    ClearOnDispose = false
                });

                Assert.Equal(11, coreStatus);
                Assert.Equal("spinner", coreSpinner);
                Assert.Equal(12, coreProgress);
                Assert.Equal("live", coreLive);
                Assert.Equal(21, facadeStatus);
                Assert.Equal(22, facadeProgress);
                Assert.Equal("injected", injectedLive);
                VisualAssert.ContainsText(scope.Driver, "Core status ready");
                VisualAssert.ContainsText(scope.Driver, "Core spinner ready");
                VisualAssert.ContainsText(scope.Driver, "Core progress");
                VisualAssert.ContainsText(scope.Driver, "Core live summary");
                VisualAssert.ContainsText(scope.Driver, "Facade status ready");
                VisualAssert.ContainsText(scope.Driver, "Facade result progress");
                VisualAssert.ContainsText(scope.Driver, "Injected result live summary");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// The injectable facade exposes the active terminal profile and delegates output and capture calls.
        /// </summary>
        [Fact]
        public void ConsoleProperty_ProvidesInjectableFacade()
        {
            using (var scope = new ConsoleDriverScope(72, 18))
            {
                IAnsiConsole console = AnsiConsole.Console;

                console.MarkupLine("Deploy [green]ready[/].");
                string captured = console.Capture(() => console.MarkupLine("[cyan]Captured[/]"), width: 24);
                IRenderable migratedRenderable = new Markup("[yellow]Captured alias[/]");
                string capturedRenderable = console.Capture(migratedRenderable, width: 24);

                Assert.IsType<ConsoulAnsiConsole>(console);
                Assert.Equal(72, console.Profile.Width);
                Assert.Equal(18, console.Profile.Height);
                Assert.True(console.Profile.IsVirtual);
                Assert.Equal(72, AnsiConsole.Profile.Width);
                Assert.Equal(18, AnsiConsole.Profile.Height);
                Assert.True(AnsiConsole.Profile.IsVirtual);
                Assert.NotNull(console.Cursor);
                Assert.NotNull(AnsiConsole.Cursor);
                Assert.Contains("Captured", captured);
                Assert.Contains("Captured alias", capturedRenderable);
                VisualAssert.ContainsText(scope.Driver, "Deploy ready.");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("ready") && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style recording captures output while still forwarding it to the active console driver.
        /// </summary>
        [Fact]
        public void Recording_ExportsTextFromLiveOutput()
        {
            AnsiConsole.ResetRecording();

            using (var scope = new ConsoleDriverScope(72, 18))
            {
                try
                {
                    AnsiConsole.Record();

                    AnsiConsole.MarkupLine("Deploy [green]ready[/].");
                    AnsiConsole.Panel("Recorded panel", new PanelOptions { Width = 32, Title = "Recorder" });

                    string exported = AnsiConsole.ExportText();

                    Assert.Contains("Deploy ready.", exported);
                    Assert.Contains("Recorder", exported);
                    Assert.Contains("Recorded panel", exported);
                    VisualAssert.ContainsText(scope.Driver, "Deploy ready.");
                    VisualAssert.ContainsText(scope.Driver, "Recorded panel");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    AnsiConsole.ResetRecording();
                }
            }
        }

        /// <summary>
        /// Exporting before recording starts fails clearly instead of returning stale output.
        /// </summary>
        [Fact]
        public void ExportText_ThrowsWhenRecordingHasNotStarted()
        {
            AnsiConsole.ResetRecording();

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => AnsiConsole.ExportText());

            Assert.Contains("recording has not been started", exception.Message);
        }

        /// <summary>
        /// Recorded HTML output is escaped so captured text can be embedded in docs or reports.
        /// </summary>
        [Fact]
        public void ExportHtml_EscapesRecordedOutput()
        {
            AnsiConsole.ResetRecording();

            using (new ConsoleDriverScope(80))
            {
                try
                {
                    AnsiConsole.Record();

                    AnsiConsole.WriteLine("<deploy> & \"release\"");

                    string html = AnsiConsole.ExportHtml();

                    Assert.StartsWith("<pre", html);
                    Assert.Contains("&lt;deploy&gt; &amp; &quot;release&quot;", html);
                    Assert.EndsWith("</pre>", html);
                }
                finally
                {
                    AnsiConsole.ResetRecording();
                }
            }
        }

        /// <summary>
        /// Scoped capture remains isolated from active recording because it uses a virtual console driver.
        /// </summary>
        [Fact]
        public void Capture_DoesNotLeakIntoActiveRecording()
        {
            AnsiConsole.ResetRecording();

            using (new ConsoleDriverScope(80))
            {
                try
                {
                    AnsiConsole.Record();

                    string captured = AnsiConsole.Capture(() => AnsiConsole.MarkupLine("[cyan]Captured only[/]"));
                    AnsiConsole.WriteLine("Recorded only");

                    string exported = AnsiConsole.ExportText();

                    Assert.Contains("Captured only", captured);
                    Assert.DoesNotContain("Captured only", exported);
                    Assert.Contains("Recorded only", exported);
                }
                finally
                {
                    AnsiConsole.ResetRecording();
                }
            }
        }

        /// <summary>
        /// Recording methods participate in the injectable facade for testable POC services.
        /// </summary>
        [Fact]
        public void ConsoleProperty_AssignedFacadeReceivesRecordingCalls()
        {
            IAnsiConsole previous = AnsiConsole.Console;
            IAnsiConsole proxyConsole;
            RecordingAnsiConsoleProxy recorder = RecordingAnsiConsoleProxy.Create(out proxyConsole);

            try
            {
                AnsiConsole.Console = proxyConsole;

                AnsiConsole.Record();
                string text = AnsiConsole.ExportText();
                string html = AnsiConsole.ExportHtml();

                Assert.Equal("proxy:ExportText", text);
                Assert.Equal("proxy:ExportHtml", html);
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Record));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.ExportText));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.ExportHtml));
            }
            finally
            {
                AnsiConsole.Console = previous;
            }
        }

        /// <summary>
        /// Assigned facades receive prompt abstraction and async prompt convenience calls.
        /// </summary>
        [Fact]
        public async Task ConsoleProperty_AssignedFacadeReceivesPromptAsyncCalls()
        {
            IAnsiConsole previous = AnsiConsole.Console;
            IAnsiConsole proxyConsole;
            RecordingAnsiConsoleProxy recorder = RecordingAnsiConsoleProxy.Create(out proxyConsole);

            try
            {
                AnsiConsole.Console = proxyConsole;

                await AnsiConsole.PromptAsync(new SelectionPrompt("Pick").AddChoices("A"));
                await AnsiConsole.AskAsync<int>("Port");
                await AnsiConsole.AskAsync("Retries", 3);
                await AnsiConsole.ConfirmAsync("Deploy?");

                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.PromptAsync));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.AskAsync));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.ConfirmAsync));
            }
            finally
            {
                AnsiConsole.Console = previous;
            }
        }

        /// <summary>
        /// The Spectre-style cursor facade controls the active Consoul console driver safely.
        /// </summary>
        [Fact]
        public void CursorProperty_ControlsActiveConsoleDriver()
        {
            using (var scope = new ConsoleDriverScope(12, 6))
            {
                IAnsiConsoleCursor cursor = AnsiConsole.Cursor;

                cursor.Hide();
                Assert.False(scope.Driver.CursorVisible);

                cursor.Show();
                Assert.True(scope.Driver.CursorVisible);

                cursor.SetPosition(3, 2);
                Assert.Equal(3, scope.Driver.CursorLeft);
                Assert.Equal(2, scope.Driver.CursorTop);

                cursor.MoveRight(4);
                cursor.MoveDown(2);
                Assert.Equal(7, scope.Driver.CursorLeft);
                Assert.Equal(4, scope.Driver.CursorTop);

                cursor.MoveLeft(99);
                cursor.MoveUp(99);
                Assert.Equal(0, scope.Driver.CursorLeft);
                Assert.Equal(0, scope.Driver.CursorTop);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// The static profile alias follows an assigned facade for Spectre-style test setup.
        /// </summary>
        [Fact]
        public void ProfileProperty_UsesAssignedFacade()
        {
            IAnsiConsole previous = AnsiConsole.Console;
            IAnsiConsole proxyConsole;
            RecordingAnsiConsoleProxy recorder = RecordingAnsiConsoleProxy.Create(out proxyConsole);

            try
            {
                AnsiConsole.Console = proxyConsole;

                TerminalProfile profile = AnsiConsole.Profile;
                IAnsiConsoleCursor cursor = AnsiConsole.Cursor;
                cursor.MoveRight(2);
                cursor.Hide();

                Assert.Equal(80, profile.Width);
                Assert.Equal(25, profile.Height);
                Assert.True(profile.IsVirtual);
                Assert.Contains(recorder.Calls, call => call.Name == "get_" + nameof(IAnsiConsole.Profile));
                Assert.Contains(recorder.Calls, call => call.Name == "get_" + nameof(IAnsiConsole.Cursor));
                Assert.Contains(recorder.Cursor.Calls, call => call.Name == nameof(IAnsiConsoleCursor.Move) && Equals(call.Arguments[0], CursorDirection.Right));
                Assert.Contains(recorder.Cursor.Calls, call => call.Name == nameof(IAnsiConsoleCursor.Show) && Equals(call.Arguments[0], false));
            }
            finally
            {
                AnsiConsole.Console = previous;
            }
        }

        /// <summary>
        /// Assigning AnsiConsole.Console lets common static entry points route through an injected facade.
        /// </summary>
        [Fact]
        public void ConsoleProperty_AssignedFacadeReceivesCoreStaticCalls()
        {
            IAnsiConsole previous = AnsiConsole.Console;
            IAnsiConsole proxyConsole;
            RecordingAnsiConsoleProxy recorder = RecordingAnsiConsoleProxy.Create(out proxyConsole);

            try
            {
                AnsiConsole.Console = proxyConsole;

                AnsiConsole.Clear(false);
                AnsiConsole.Write("plain");
                AnsiConsole.Write(42);
                AnsiConsole.Write((object)new OutputLabel("object"));
                AnsiConsole.Write(CultureInfo.InvariantCulture, "{0:0.0}", 3.5);
                AnsiConsole.WriteAnsi(() => AnsiConsole.Write("ansi"));
                AnsiConsole.WriteLine("line");
                AnsiConsole.WriteLine(false);
                AnsiConsole.WriteLine((object)new OutputLabel("object-line"));
                AnsiConsole.WriteLine(CultureInfo.InvariantCulture, "{0:0.00}", 12.5m);
                AnsiConsole.Markup("[green]markup[/]");
                AnsiConsole.MarkupLine("[cyan]line[/]");
                string escaped = AnsiConsole.EscapeMarkup("Owner [prod]");
                string stripped = AnsiConsole.RemoveMarkup("[green]Owner[/]");
                AnsiConsole.Rule("Injected rule");
                AnsiConsole.Panel("Injected panel");
                AnsiConsole.Columns("One", "Two");
                AnsiConsole.Columns("Service", new Markup("[green]Ready[/]"), 2);
                AnsiConsole.Rows("Summary", new Markup("[green]Ready[/]"), 3);
                AnsiConsole.Grid("Worker", new Markup("[yellow]Queued[/]"), 4);

                Assert.Equal("escaped:Owner [prod]", escaped);
                Assert.Equal("proxy:RemoveMarkup", stripped);
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Clear));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Write) && call.FirstArgumentEquals("plain"));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Write) && call.FirstArgumentEquals("42"));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Write) && call.FirstArgumentEquals("object"));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Write) && call.FirstArgumentEquals("3.5"));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.WriteAnsi));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.WriteLine) && call.FirstArgumentEquals("line"));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.WriteLine) && call.FirstArgumentEquals("False"));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.WriteLine) && call.FirstArgumentEquals("object-line"));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.WriteLine) && call.FirstArgumentEquals("12.50"));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Markup) && call.FirstArgumentEquals("[green]markup[/]"));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.MarkupLine) && call.FirstArgumentEquals("[cyan]line[/]"));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.EscapeMarkup));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.RemoveMarkup));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Rule) && call.FirstArgumentEquals("Injected rule"));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Panel) && call.FirstArgumentEquals("Injected panel"));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Columns));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Columns) && call.Arguments.Length > 0 && call.Arguments[0] is object[] columns && columns.Length == 3 && Equals(columns[2], 2));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Rows) && call.Arguments.Length > 0 && call.Arguments[0] is object[] rows && rows.Length == 3 && Equals(rows[2], 3));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Grid) && call.Arguments.Length > 0 && call.Arguments[0] is object[] grid && grid.Length == 3 && Equals(grid[2], 4));
            }
            finally
            {
                AnsiConsole.Console = previous;
            }
        }

        /// <summary>
        /// Assigned facades receive visual, dashboard, scoped, and capture helpers beyond the core prompt/output surface.
        /// </summary>
        [Fact]
        public void ConsoleProperty_AssignedFacadeReceivesVisualDashboardAndScopedStaticCalls()
        {
            IAnsiConsole previous = AnsiConsole.Console;
            IAnsiConsole proxyConsole;
            RecordingAnsiConsoleProxy recorder = RecordingAnsiConsoleProxy.Create(out proxyConsole);

            try
            {
                AnsiConsole.Console = proxyConsole;

                AnsiConsole.Badge("ready", BadgeKind.Success);
                string badge = AnsiConsole.BadgeText("ready");
                AnsiConsole.Headline("Deploy");
                AnsiConsole.FigletText("OK");
                AnsiConsole.Calendar(new DateTime(2026, 6, 1));
                AnsiConsole.Callout("Approval required.", CalloutKind.Warning);
                AnsiConsole.WriteException(new InvalidOperationException("Preview failed"), ExceptionFormats.NoStackTrace);
                AnsiConsole.WriteException(new InvalidOperationException("Settings failed"), new ExceptionSettings { Format = ExceptionFormats.ShortenEverything });
                ValidationSummaryResult validation = AnsiConsole.ValidateModel(new InvalidSettings());
                ValidationSummaryResult summary = AnsiConsole.ValidationSummary(new InvalidSettings());
                AnsiConsole.Timeline(timeline => timeline.Add("Queued"));
                AnsiConsole.BulletList(list => list.Add("Build"));
                AnsiConsole.Grid(grid =>
                {
                    grid.Add("Name");
                    grid.Add("Ready");
                });
                AnsiConsole.Canvas(4, 2, canvas => canvas.Set(0, 0, 'x'));
                AnsiConsole.CanvasImage(new[,]
                {
                    { ConsoleColor.DarkBlue, ConsoleColor.Blue },
                    { ConsoleColor.DarkGreen, ConsoleColor.Green }
                });
                AnsiConsole.Rows("one", "two");
                AnsiConsole.Layout(layout => layout.Add("Status", "Ready"));
                AnsiConsole.Pad("Indented", left: 2);
                AnsiConsole.Align("Centered", TextBlockAlignment.Center);
                AnsiConsole.TextBlock("Wrapped");
                AnsiConsole.TextPathStyled(@"C:\deploy\release.txt", fileNameStyle: "aqua");
                AnsiConsole.Breadcrumb("Home", "Deploy");
                string breadcrumb = AnsiConsole.BreadcrumbText("Home", "Deploy");
                AnsiConsole.ShortcutBar(new ShortcutItem("Enter", "Select"));
                string shortcuts = AnsiConsole.ShortcutBarText(new ShortcutItem("Esc", "Back"));
                AnsiConsole.Checklist(list => list.Add("Approved"));
                AnsiConsole.PropertyList(new InvalidSettings());
                AnsiConsole.Metrics(metrics => metrics.Add("Jobs", "3"));
                AnsiConsole.BarChart(chart => chart.Add("Ready", 3));
                AnsiConsole.BreakdownChart(chart => chart.Add("Ready", 3));
                AnsiConsole.Sparkline(new[] { 1d, 3d, 2d }, new SparklineOptions());
                AnsiConsole.Status("Preparing", status => { });
                AnsiConsole.Spinner("Packaging", spinner => { });
                AnsiConsole.Progress(progress => { });
                AnsiConsole.Live(live => { });
                string captured = AnsiConsole.Capture(() => AnsiConsole.MarkupLine("[green]Captured[/]"));

                Assert.Equal("proxy:BadgeText", badge);
                Assert.Equal("proxy:BreadcrumbText", breadcrumb);
                Assert.Equal("proxy:ShortcutBarText", shortcuts);
                Assert.Equal("proxy:Capture", captured);
                Assert.NotNull(validation);
                Assert.NotNull(summary);
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Badge));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Headline));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.FigletText));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Calendar));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Callout));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.WriteException) && call.Arguments.Length > 1 && Equals(call.Arguments[1], ExceptionFormats.NoStackTrace));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.WriteException) && call.Arguments.Length > 1 && call.Arguments[1] is ExceptionSettings);
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.ValidateModel));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.ValidationSummary));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Grid));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.CanvasImage));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Layout));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Metrics));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Progress));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Live));
                Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Capture));
            }
            finally
            {
                AnsiConsole.Console = previous;
            }
        }

        /// <summary>
        /// IAnsiConsole accepts Spectre-style Clear(home) calls without expanding the interface contract.
        /// </summary>
        [Fact]
        public void ConsoleExtensions_ClearHome_DelegatesToFacade()
        {
            IAnsiConsole proxyConsole;
            RecordingAnsiConsoleProxy recorder = RecordingAnsiConsoleProxy.Create(out proxyConsole);

            proxyConsole.Clear(false);

            Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Clear));
        }

        /// <summary>
        /// IAnsiConsole accepts quick async scoped helpers without expanding the interface contract.
        /// </summary>
        [Fact]
        public async Task ConsoleExtensions_QuickAsyncScopedHelpers_DelegateToTokenAwareFacade()
        {
            IAnsiConsole proxyConsole;
            RecordingAnsiConsoleProxy recorder = RecordingAnsiConsoleProxy.Create(out proxyConsole);

            await proxyConsole.StatusAsync("Status", status => Task.CompletedTask);
            await proxyConsole.SpinnerAsync("Spinner", spinner => Task.CompletedTask);
            await proxyConsole.ProgressAsync(progress => Task.CompletedTask);
            await proxyConsole.LiveAsync(live => Task.CompletedTask);
            await proxyConsole.LiveAsync(new Text("Target"), live => Task.CompletedTask);
            await proxyConsole.StatusAsync("Status result", status => Task.FromResult(1));
            await proxyConsole.SpinnerAsync("Spinner result", spinner => Task.FromResult(2));
            await proxyConsole.ProgressAsync(progress => Task.FromResult(3));
            await proxyConsole.LiveAsync(live => Task.FromResult(4));
            await proxyConsole.LiveAsync(new Text("Target result"), live => Task.FromResult(5));

            Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.StatusAsync));
            Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.SpinnerAsync));
            Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.ProgressAsync));
            Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.LiveAsync));
        }

        /// <summary>
        /// IAnsiConsole accepts primitive and formatted write overloads through additive extension methods.
        /// </summary>
        [Fact]
        public void ConsoleExtensions_PrimitiveWriteOverloads_FormatThroughFacade()
        {
            IAnsiConsole proxyConsole;
            RecordingAnsiConsoleProxy recorder = RecordingAnsiConsoleProxy.Create(out proxyConsole);

            proxyConsole.Write(42);
            proxyConsole.Write(CultureInfo.InvariantCulture, 3.5);
            proxyConsole.Write(new[] { 'O', 'K' });
            proxyConsole.Write((object)new OutputLabel("object"));
            proxyConsole.Write((object)new Markup("[green]renderable[/]"));
            proxyConsole.Write(CultureInfo.InvariantCulture, " value {0:0.0}", 1.25);
            proxyConsole.WriteLine(false);
            proxyConsole.WriteLine((object)new OutputLabel("object-line"));
            proxyConsole.WriteLine((IConsoulRenderable)new Panel("panel-line").NoBorder());
            proxyConsole.WriteLine((IRenderable)new Markup("[green]markup-line[/]"));
            proxyConsole.WriteLine(CultureInfo.InvariantCulture, " done {0:0.0}", 2.5);

            Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Write) && call.FirstArgumentEquals("42"));
            Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Write) && call.FirstArgumentEquals("3.5"));
            Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Write) && call.FirstArgumentEquals("OK"));
            Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Write) && call.FirstArgumentEquals("object"));
            Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Write) && call.Arguments.Length > 0 && call.Arguments[0] is IConsoulRenderable);
            Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Write) && call.FirstArgumentEquals(" value 1.3"));
            Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.WriteLine) && call.FirstArgumentEquals("False"));
            Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.WriteLine) && call.FirstArgumentEquals("object-line"));
            Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.WriteLine) && call.FirstArgumentEquals(" done 2.5"));
            Assert.True(
                recorder.Calls.Count(call => call.Name == nameof(IAnsiConsole.Write) && call.Arguments.Length > 0 && call.Arguments[0] is IConsoulRenderable) >= 3,
                "Expected boxed and strongly typed renderable write calls to reach the facade.");
            Assert.True(
                recorder.Calls.Count(call => call.Name == nameof(IAnsiConsole.WriteLine) && call.Arguments.Length == 0) >= 2,
                "Expected renderable WriteLine overloads to append blank line calls.");
        }

        /// <summary>
        /// Static and injectable renderable WriteLine overloads render visual components instead of falling back to type names.
        /// </summary>
        [Fact]
        public void RenderableWriteLineOverloads_RenderThroughConsoleDriver()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                IAnsiConsole console = AnsiConsole.Console;

                AnsiConsole.WriteLine((IConsoulRenderable)new Text("Static renderable line", ConsoleColor.Cyan));
                console.WriteLine((IRenderable)new Markup("[green]Injected renderable line[/]"));

                VisualAssert.ContainsText(scope.Driver, "Static renderable line");
                VisualAssert.ContainsText(scope.Driver, "Injected renderable line");
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Static renderable line") && write.Color == ConsoleColor.Cyan);
                Assert.Contains(scope.Driver.Writes, write => write.Text.Contains("Injected renderable line") && write.Color == ConsoleColor.Green);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// IAnsiConsole accepts formatted markup overloads through additive extension methods.
        /// </summary>
        [Fact]
        public void ConsoleExtensions_FormattedMarkupOverloads_EscapeArgumentsThroughFacade()
        {
            IAnsiConsole proxyConsole;
            RecordingAnsiConsoleProxy recorder = RecordingAnsiConsoleProxy.Create(out proxyConsole);

            proxyConsole.Markup("Target [green]{0}[/]", "prod [hot]");
            proxyConsole.MarkupLine(CultureInfo.InvariantCulture, "Cost [cyan]{0:0.0}[/]", 1.25);

            Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.Markup) && call.FirstArgumentEquals("Target [green]prod [[hot]][/]"));
            Assert.Contains(recorder.Calls, call => call.Name == nameof(IAnsiConsole.MarkupLine) && call.FirstArgumentEquals("Cost [cyan]1.3[/]"));
        }

        /// <summary>
        /// The injectable facade keeps method-name parity with the static migration facade.
        /// </summary>
        [Fact]
        public void ConsoleProperty_ExposesStaticFacadeMethodNames()
        {
            string[] staticNames = typeof(AnsiConsole)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(method => !method.IsSpecialName)
                .Where(method => method.Name != nameof(AnsiConsole.Create))
                .Select(method => method.Name)
                .Distinct()
                .OrderBy(name => name)
                .ToArray();

            string[] instanceNames = typeof(IAnsiConsole)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(method => !method.IsSpecialName)
                .Select(method => method.Name)
                .Concat(typeof(AnsiConsoleExtensions)
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(method => !method.IsSpecialName)
                    .Where(method =>
                    {
                        ParameterInfo[] parameters = method.GetParameters();
                        return parameters.Length > 0 && parameters[0].ParameterType == typeof(IAnsiConsole);
                    })
                    .Select(method => method.Name))
                .Distinct()
                .OrderBy(name => name)
                .ToArray();

            string[] missing = staticNames.Except(instanceNames).ToArray();

            Assert.Empty(missing);
        }

        /// <summary>
        /// Prompt calls through the injectable facade keep Consoul's result-first prompt behavior.
        /// </summary>
        [Fact]
        public void ConsoleProperty_PromptMethodsPreserveResultTypes()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                IAnsiConsole console = AnsiConsole.Console;
                Routines.InitializeRoutine(new TestRoutine(new[] { "2", "8080", string.Empty }));

                try
                {
                    SelectionResult<string> target = console.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Pick target")
                            .AddChoices("Development", "Production"));
                    TextPromptResult<int> port = console.Ask<int>("Port");
                    ConfirmResult deploy = console.Confirm("Deploy?", defaultValue: false);

                    Assert.True(target.HasSelection);
                    Assert.Equal("Production", target.SelectedItem);
                    Assert.True(port.HasValue);
                    Assert.Equal(8080, port.Value);
                    Assert.True(deploy.HasValue);
                    Assert.False(deploy.Value);
                    VisualAssert.ContainsText(scope.Driver, "Pick target");
                    VisualAssert.ContainsText(scope.Driver, "Port");
                    VisualAssert.ContainsText(scope.Driver, "Deploy?");
                    VisualAssert.NoOverflow(scope.Driver);
                }
                finally
                {
                    Routines.InitializeRoutine(new TestRoutine(Array.Empty<string>()));
                }
            }
        }

        /// <summary>
        /// Visual and scoped helpers are available through the injectable facade for reusable POC services.
        /// </summary>
        [Fact]
        public async Task ConsoleProperty_VisualAndScopedMethodsDelegateToConsoul()
        {
            using (var scope = new ConsoleDriverScope(96))
            {
                IAnsiConsole console = AnsiConsole.Console;

                console.Rule("Injected");
                console.Panel("Panel body", new PanelOptions { Title = "Panel", Width = 40 });
                console.Panel(new Markup("[green]Renderable panel[/]"), new PanelOptions { Title = "Renderable", Width = 44 });
                console.Badge("ready", BadgeKind.Success);
                string escaped = console.EscapeMarkup("Owner [prod]");
                string stripped = console.RemoveMarkup("[green]Owner[/] [[prod]]");
                console.MarkupLine();
                console.Table(
                    new[]
                    {
                        new Job("Build", "Ready"),
                        new Job("Deploy", "Waiting")
                    },
                    job => job.Name,
                    job => job.State);
                console.Tree("Injected tree", root => root.Add("Leaf", "aqua"), new TreeOptions { Width = 40 });
                console.Columns(new ColumnsOptions { Width = 40, Columns = 2 }, "Left", "Right");
                console.Headline("Injected headline", width: 40, color: ConsoleColor.Cyan, style: HeadlineStyle.Rule);
                console.FigletText("OK", width: 32, color: ConsoleColor.Green);
                console.Calendar(new DateTime(2026, 6, 1), calendar =>
                {
                    calendar.Add(new DateTime(2026, 6, 15), "aqua", "Release");
                }, new CalendarOptions { Width = 32 });
                console.Callout("Approval required.", CalloutKind.Warning, new CalloutOptions { Width = 40 });
                console.Timeline(timeline =>
                {
                    timeline.AddStyled("Build", TimelineItemState.Complete, titleStyle: "aqua");
                }, new TimelineOptions { Width = 40 });
                console.BulletList(list =>
                {
                    list.AddStyled("Checks", "aqua");
                }, new BulletListOptions { Width = 40 });
                console.Checklist(list =>
                {
                    list.AddStyled("Approval", ChecklistItemState.Pending, "Waiting", labelStyle: "yellow");
                }, new ChecklistOptions { Width = 40 });
                console.Grid(grid =>
                {
                    grid.Add("Job", "Build", "grey");
                    grid.Add("State", "Ready", "aqua");
                }, new GridOptions { Width = 40, Columns = 2 });
                console.Rows(rows =>
                {
                    rows.Add("Row summary", "grey");
                }, new RowsOptions { Width = 40 });
                console.Layout(layout =>
                {
                    layout.Add("API", "Ready", "aqua");
                    layout.Add("Worker", "Waiting", "yellow");
                }, new LayoutOptions { Width = 48, Columns = 2 });
                console.Canvas(12, 3, canvas =>
                {
                    canvas.Text(2, 1, "API", "aqua");
                });
                console.Pad("Indented", left: 2, width: 20);
                console.Align("Centered", TextBlockAlignment.Center, width: 20);
                console.TextBlock("Wrapped injected summary", width: 32, alignment: TextBlockAlignment.Center);
                console.TextPathStyled(@"C:\deployments\release\notes.txt", width: 40, fileNameStyle: "aqua");
                console.Breadcrumb("Home", "Injected");
                console.ShortcutBar(bar =>
                {
                    bar.AddStyled("Esc", "Back", keyStyle: "yellow");
                }, new ShortcutBarOptions { Width = 40 });
                console.PropertyList(list =>
                {
                    list.Add("Owner", "Platform", ConsoleColor.Cyan);
                    list.Add("State", "Waiting", ConsoleColor.Yellow);
                }, new PropertyListOptions { Width = 40 });
                console.Metrics(metrics =>
                {
                    metrics.Add("Passing", 8, "jobs", kind: MetricKind.Success);
                }, new MetricsOptions { Width = 40 });
                console.BarChart(chart =>
                {
                    chart.Add("Ready", 3, ConsoleColor.Green);
                }, new BarChartOptions { Width = 40, MaximumValue = 5 });
                console.BreakdownChart(chart =>
                {
                    chart.Add("Queued", 2, ConsoleColor.Yellow);
                    chart.Add("Running", 1, ConsoleColor.Green);
                }, new BreakdownChartOptions { Width = 40 });
                console.Sparkline(new[] { 1d, 3d, 2d, 5d }, new SparklineOptions
                {
                    Width = 40,
                    Label = "Trend",
                    ShowRange = true
                });
                ValidationSummaryResult validation = console.ValidationSummary(
                    new InvalidSettings(),
                    new ValidationSummaryOptions { Width = 40 });
                ValidationSummaryResult validationOnly = console.ValidateModel(new InvalidSettings());
                console.WriteException(
                    new InvalidOperationException("Preview failed"),
                    "Deployment preview failed",
                    new ExceptionRenderOptions { Width = 48, MaxStackFrames = 1 });
                console.Status("Preparing", status => status.Succeed("Ready"));
                await console.StatusAsync("Async preparing", (status, cancellationToken) =>
                {
                    status.Succeed("Async ready");
                    return Task.CompletedTask;
                });
                console.Spinner("Packaging", spinner => spinner.Succeed("Packaged"));
                await console.SpinnerAsync("Async packaging", (spinner, cancellationToken) =>
                {
                    spinner.Succeed("Async packaged");
                    return Task.CompletedTask;
                });
                console.Progress(progress =>
                {
                    ProgressTask task = progress.AddTask("Upload", 2);
                    task.Complete();
                }, new ProgressContextOptions { Width = 40 });
                console.Live(live =>
                {
                    live.Refresh(() => console.MarkupLine("[cyan]Live summary[/]"));
                    live.Succeed("Live ready");
                }, new LiveDisplayOptions
                {
                    Title = "Live",
                    ClearBeforeRender = false,
                    ClearOnDispose = false
                });

                Assert.False(validation.IsValid);
                Assert.False(validationOnly.IsValid);
                Assert.Equal("Owner [[prod]]", escaped);
                Assert.Equal("Owner [prod]", stripped);
                VisualAssert.ContainsText(scope.Driver, "Injected");
                VisualAssert.ContainsText(scope.Driver, "Panel body");
                VisualAssert.ContainsText(scope.Driver, "Renderable panel");
                VisualAssert.ContainsText(scope.Driver, "READY");
                VisualAssert.ContainsText(scope.Driver, "Build");
                VisualAssert.ContainsText(scope.Driver, "Injected tree");
                VisualAssert.ContainsText(scope.Driver, "Leaf");
                VisualAssert.ContainsText(scope.Driver, "Left");
                VisualAssert.ContainsText(scope.Driver, "Injected headline");
                VisualAssert.ContainsText(scope.Driver, "Approval required");
                VisualAssert.ContainsText(scope.Driver, "Checks");
                VisualAssert.ContainsText(scope.Driver, "Approval");
                VisualAssert.ContainsText(scope.Driver, "Row summary");
                VisualAssert.ContainsText(scope.Driver, "Centered");
                VisualAssert.ContainsText(scope.Driver, "notes.txt");
                VisualAssert.ContainsText(scope.Driver, "Home");
                VisualAssert.ContainsText(scope.Driver, "[Esc]");
                VisualAssert.ContainsText(scope.Driver, "Owner");
                VisualAssert.ContainsText(scope.Driver, "Passing");
                VisualAssert.ContainsText(scope.Driver, "Trend");
                VisualAssert.ContainsText(scope.Driver, "Name");
                VisualAssert.ContainsText(scope.Driver, "Deployment preview failed");
                VisualAssert.ContainsText(scope.Driver, "Ready");
                VisualAssert.ContainsText(scope.Driver, "Async ready");
                VisualAssert.ContainsText(scope.Driver, "Packaged");
                VisualAssert.ContainsText(scope.Driver, "Async packaged");
                VisualAssert.ContainsText(scope.Driver, "Upload");
                VisualAssert.ContainsText(scope.Driver, "Live summary");
                VisualAssert.NoOverflow(scope.Driver);
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

        private sealed class Job
        {
            public Job(string name, string state)
            {
                Name = name;
                State = state;
            }

            public string Name { get; }

            public string State { get; }
        }

        private sealed class OutputLabel
        {
            private readonly string _value;

            public OutputLabel(string value)
            {
                _value = value;
            }

            public override string ToString()
            {
                return _value;
            }
        }

        private sealed class InvalidSettings
        {
            [Required]
            public string Name { get; set; } = string.Empty;
        }

        private class RecordingAnsiConsoleProxy : DispatchProxy
        {
            public List<RecordedCall> Calls { get; } = new List<RecordedCall>();

            public RecordingCursor Cursor { get; } = new RecordingCursor();

            public static RecordingAnsiConsoleProxy Create(out IAnsiConsole console)
            {
                console = DispatchProxy.Create<IAnsiConsole, RecordingAnsiConsoleProxy>();
                return (RecordingAnsiConsoleProxy)(object)console;
            }

            protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
            {
                if (targetMethod == null)
                {
                    return null;
                }

                object?[] safeArguments = args ?? Array.Empty<object?>();
                Calls.Add(new RecordedCall(targetMethod.Name, safeArguments));

                if (targetMethod.ReturnType == typeof(string) && targetMethod.Name == nameof(IAnsiConsole.EscapeMarkup))
                {
                    return "escaped:" + (safeArguments.Length > 0 ? safeArguments[0] : string.Empty);
                }

                if (targetMethod.ReturnType == typeof(string))
                {
                    return "proxy:" + targetMethod.Name;
                }

                if (targetMethod.ReturnType == typeof(TerminalProfile))
                {
                    return new TerminalProfile(80, 25, isVirtual: true);
                }

                if (targetMethod.ReturnType == typeof(IAnsiConsoleCursor))
                {
                    return Cursor;
                }

                if (targetMethod.ReturnType == typeof(ValidationSummaryResult))
                {
                    return new ValidationSummaryResult(Array.Empty<ValidationIssue>());
                }

                if (targetMethod.ReturnType == typeof(Task))
                {
                    return Task.CompletedTask;
                }

                if (targetMethod.ReturnType.IsGenericType && targetMethod.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    Type resultType = targetMethod.ReturnType.GetGenericArguments()[0];
                    MethodInfo fromResult = typeof(Task).GetMethods()
                        .Single(method => method.Name == nameof(Task.FromResult) && method.IsGenericMethodDefinition);

                    object? defaultValue = resultType.IsValueType ? Activator.CreateInstance(resultType) : null;
                    return fromResult.MakeGenericMethod(resultType).Invoke(null, new[] { defaultValue });
                }

                if (targetMethod.ReturnType == typeof(void))
                {
                    return null;
                }

                return targetMethod.ReturnType.IsValueType ? Activator.CreateInstance(targetMethod.ReturnType) : null;
            }
        }

        private sealed class RecordedCall
        {
            public RecordedCall(string name, object?[] arguments)
            {
                Name = name;
                Arguments = arguments;
            }

            public string Name { get; }

            public object?[] Arguments { get; }

            public bool FirstArgumentEquals(object? expected)
            {
                return Arguments.Length > 0 && Equals(Arguments[0], expected);
            }
        }

        private sealed class RecordingCursor : IAnsiConsoleCursor
        {
            public List<RecordedCall> Calls { get; } = new List<RecordedCall>();

            public void Show(bool show)
            {
                Calls.Add(new RecordedCall(nameof(Show), new object?[] { show }));
            }

            public void SetPosition(int column, int line)
            {
                Calls.Add(new RecordedCall(nameof(SetPosition), new object?[] { column, line }));
            }

            public void Move(CursorDirection direction, int steps)
            {
                Calls.Add(new RecordedCall(nameof(Move), new object?[] { direction, steps }));
            }
        }

        private sealed class NonHomingClearConsoleDriver : IConsoleDriver
        {
            public int BufferWidth => 80;

            public int BufferHeight => 25;

            public int CursorLeft { get; private set; }

            public int CursorTop { get; private set; }

            public bool CursorVisible { get; set; } = true;

            public ConsoleColor ForegroundColor { get; set; } = RenderOptions.DefaultColor;

            public ConsoleColor BackgroundColor { get; set; } = RenderOptions.DefaultScheme.BackgroundColor;

            public Encoding InputEncoding => Encoding.UTF8;

            public string Title { get; set; } = string.Empty;

            public bool IsInputRedirected => true;

            public bool IsOutputRedirected => true;

            public bool IsErrorRedirected => true;

            public bool IsVirtual => true;

            public int ClearCount { get; private set; }

            public void Write(string value)
            {
                CursorLeft = Math.Min(BufferWidth - 1, CursorLeft + (value ?? string.Empty).Length);
            }

            public void WriteLine(string value)
            {
                CursorLeft = 0;
                CursorTop = Math.Min(BufferHeight - 1, CursorTop + 1);
            }

            public void WriteControl(string value)
            {
            }

            public void Clear()
            {
                ClearCount++;
            }

            public void ClearLine(int top)
            {
                CursorLeft = 0;
                CursorTop = Math.Max(0, Math.Min(top, BufferHeight - 1));
            }

            public void SetCursorPosition(int left, int top)
            {
                CursorLeft = Math.Max(0, Math.Min(left, BufferWidth - 1));
                CursorTop = Math.Max(0, Math.Min(top, BufferHeight - 1));
            }

            public ConsoleKeyInfo ReadKey(bool intercept)
            {
                return new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false);
            }

            public Stream OpenStandardInput()
            {
                return Stream.Null;
            }
        }
    }
}
