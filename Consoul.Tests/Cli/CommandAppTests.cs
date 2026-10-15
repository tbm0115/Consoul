using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ConsoulLibrary.Tests.Rendering;
using ConsoulLibrary.Unsafe;
using ConsoulLibrary.Views;
using Xunit;

namespace ConsoulLibrary.Tests.Cli
{
    /// <summary>
    /// Tests for the optional Consoul.Cli command app surface.
    /// </summary>
    public class CommandAppTests
    {
        private const string EnvEnvironmentVariable = "CONSOUL_TEST_CLI_ENVIRONMENT";
        private const string EnvReplicasVariable = "CONSOUL_TEST_CLI_REPLICAS";
        private const string EnvDryRunVariable = "CONSOUL_TEST_CLI_DRY_RUN";
        private const string EnvModeVariable = "CONSOUL_TEST_CLI_MODE";
        private const string EnvNameVariable = "CONSOUL_TEST_CLI_NAME";
        private const string EnvCodeVariable = "CONSOUL_TEST_CLI_CODE";
        private const string EnvMetadataVariable = "CONSOUL_TEST_CLI_METADATA";
        private const string EnvTagVariable = "CONSOUL_TEST_CLI_TAG";

        /// <summary>
        /// Delegate commands receive command-specific arguments and return their exit code.
        /// </summary>
        [Fact]
        public async Task CommandApp_RunsDelegateCommandAndPassesArguments()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                string[] received = Array.Empty<string>();
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("tool");
                    config.AddCommand("deploy", context =>
                    {
                        received = context.Arguments.ToArray();
                        context.Console.WriteLine("Deploying " + string.Join(",", context.Arguments));
                        return 7;
                    }).WithDescription("Deploy a target");
                });

                int exitCode = await app.RunAsync(new[] { "deploy", "api", "--dry-run" });

                Assert.Equal(7, exitCode);
                Assert.Equal(new[] { "api", "--dry-run" }, received);
                VisualAssert.ContainsText(scope.Driver, "Deploying api,--dry-run");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Typed delegate commands bind settings without requiring a one-off command class.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedDelegateCommand_BindsSettingsWithoutCommandClass()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                CopySettings? received = null;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("files");
                    config.AddCommand<CopySettings>("copy", (context, settings) =>
                    {
                        received = settings;
                        context.Console.WriteLine("Delegate copied " + settings.Source + " -> " + settings.Destination);
                        return 9;
                    })
                    .WithDescription("Copy files");
                });

                int exitCode = await app.RunAsync(new[] { "copy", "source.txt", "dest.txt", "--force" });

                Assert.Equal(9, exitCode);
                Assert.NotNull(received);
                Assert.Equal("source.txt", received!.Source);
                Assert.Equal("dest.txt", received.Destination);
                Assert.True(received.Force);
                VisualAssert.ContainsText(scope.Driver, "Delegate copied source.txt -> dest.txt");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style delegate aliases work through the root configurator interface.
        /// </summary>
        [Fact]
        public async Task CommandApp_AddDelegate_RunsThroughConfiguratorInterface()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                CancellationToken observedToken = default;
                var cts = new CancellationTokenSource();
                var app = new CommandApp();
                app.Configure(config =>
                {
                    IConfigurator configurator = config;
                    configurator.AddDelegate("status", (context, cancellationToken) =>
                    {
                        observedToken = cancellationToken;
                        context.Console.WriteLine("Delegate alias status");
                        return 11;
                    })
                    .WithDescription("Show status");
                });

                int exitCode = await app.RunAsync(new[] { "status" }, cts.Token);

                Assert.Equal(11, exitCode);
                Assert.Equal(cts.Token, observedToken);
                VisualAssert.ContainsText(scope.Driver, "Delegate alias status");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style async delegate aliases bind typed settings through the root configurator interface.
        /// </summary>
        [Fact]
        public async Task CommandApp_AddAsyncDelegate_BindsTypedSettingsThroughConfiguratorInterface()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                CopySettings? received = null;
                CancellationToken observedToken = default;
                var cts = new CancellationTokenSource();
                var app = new CommandApp();
                app.Configure(config =>
                {
                    IConfigurator configurator = config;
                    configurator.AddAsyncDelegate<CopySettings>("copy", async (context, settings, cancellationToken) =>
                    {
                        await Task.Delay(1, cancellationToken);
                        received = settings;
                        observedToken = cancellationToken;
                        context.Console.WriteLine("Async delegate alias copied " + settings.Source);
                        return 12;
                    })
                    .WithDescription("Copy files");
                });

                int exitCode = await app.RunAsync(new[] { "copy", "source.txt", "dest.txt", "--force" }, cts.Token);

                Assert.Equal(12, exitCode);
                Assert.NotNull(received);
                Assert.Equal("source.txt", received!.Source);
                Assert.Equal("dest.txt", received.Destination);
                Assert.True(received.Force);
                Assert.Equal(cts.Token, observedToken);
                VisualAssert.ContainsText(scope.Driver, "Async delegate alias copied source.txt");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Aliases invoke the same command and top-level help lists aliases, descriptions, and usage.
        /// </summary>
        [Fact]
        public async Task CommandApp_AliasAndTopLevelHelp_RenderExpectedOutput()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                bool ran = false;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("tasks");
                    config.SetApplicationVersion("1.2.3");
                    config.SetApplicationDescription("Task automation console");
                    config.AddCommand("remove", context =>
                    {
                        ran = true;
                        return 0;
                    })
                    .WithDescription("Remove an item")
                    .WithAlias("rm")
                    .WithExample("remove", "old-file.txt");
                });

                int aliasExit = await app.RunAsync(new[] { "rm", "old-file.txt" });
                int helpExit = await app.RunAsync(new[] { "--help" });

                Assert.Equal(0, aliasExit);
                Assert.Equal(0, helpExit);
                Assert.True(ran);
                VisualAssert.ContainsText(scope.Driver, "tasks 1.2.3");
                VisualAssert.ContainsText(scope.Driver, "Task automation console");
                VisualAssert.ContainsText(scope.Driver, "USAGE:");
                VisualAssert.ContainsText(scope.Driver, "remove (rm)");
                VisualAssert.ContainsText(scope.Driver, "Remove an item");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Root version options render the configured application version without executing a command.
        /// </summary>
        [Fact]
        public async Task CommandApp_VersionOptions_RenderConfiguredVersion()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                bool ran = false;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("tasks");
                    config.SetApplicationVersion("2.4.6");
                    config.AddCommand("status", context =>
                    {
                        ran = true;
                        return 0;
                    });
                });

                int longExit = await app.RunAsync(new[] { "--version" });
                int shortExit = await app.RunAsync(new[] { "-v" });

                Assert.Equal(0, longExit);
                Assert.Equal(0, shortExit);
                Assert.False(ran);
                Assert.True(scope.Driver.Writes.Count(write => write.Text.Contains("tasks 2.4.6")) >= 2);
                Assert.DoesNotContain(scope.Driver.Text, "Unknown command");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// A registered version command takes precedence over the built-in version word.
        /// </summary>
        [Fact]
        public async Task CommandApp_VersionCommand_DoesNotOverrideRegisteredCommand()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("tasks");
                    config.SetApplicationVersion("2.4.6");
                    config.AddCommand("version", context =>
                    {
                        context.Console.WriteLine("Custom version command");
                        return 5;
                    });
                });

                int exitCode = await app.RunAsync(new[] { "version" });

                Assert.Equal(5, exitCode);
                VisualAssert.ContainsText(scope.Driver, "Custom version command");
                Assert.DoesNotContain(scope.Driver.Writes, write => write.Text == "tasks 2.4.6");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Command-specific help includes aliases and examples.
        /// </summary>
        [Fact]
        public async Task CommandApp_CommandHelp_RendersAliasesAndExamples()
        {
            using (var scope = new ConsoleDriverScope(60))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("tasks");
                    config.AddCommand("list", context => 0)
                        .WithDescription("List items")
                        .WithAlias("ls")
                        .WithExample("list", "--all");
                });

                int exitCode = await app.RunAsync(new[] { "help", "list" });

                Assert.Equal(0, exitCode);
                VisualAssert.ContainsText(scope.Driver, "list");
                VisualAssert.ContainsText(scope.Driver, "List items");
                VisualAssert.ContainsText(scope.Driver, "ALIASES:");
                VisualAssert.ContainsText(scope.Driver, "ls");
                VisualAssert.ContainsText(scope.Driver, "tasks list --all");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Description attributes seed command, settings, branch, default-command, and MVC-style view help metadata.
        /// </summary>
        [Fact]
        public async Task CommandApp_DescriptionAttributes_SeedHelpMetadata()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("poc");
                    config.SetDefaultCommand<AttributedDefaultCommand>();
                    config.AddCommand<AttributedCommand>("annotated");
                    config.AddCommand<AttributedDelegateSettings>("delegate", (context, settings) => 0);
                    config.AddBranch<AttributedBranchSettings>("ops", branch =>
                    {
                        branch.AddCommand<AttributedCommand>("run");
                    });
                    config.AddView<AttributedView>("dashboard");
                });

                int exitCode = await app.RunAsync(new[] { "--help" });

                Assert.Equal(0, exitCode);
                VisualAssert.ContainsText(scope.Driver, "Run default from attributes");
                VisualAssert.ContainsText(scope.Driver, "Run annotated command");
                VisualAssert.ContainsText(scope.Driver, "Run delegate settings command");
                VisualAssert.ContainsText(scope.Driver, "Manage attributed operations");
                VisualAssert.ContainsText(scope.Driver, "Open annotated dashboard");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Explicit fluent descriptions continue to override attribute-inferred descriptions.
        /// </summary>
        [Fact]
        public async Task CommandApp_DescriptionAttributes_AllowExplicitOverrides()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("poc");
                    config.AddCommand<AttributedCommand>("annotated")
                        .WithDescription("Explicit command description");
                    config.AddBranch<AttributedBranchSettings>("ops", branch => { })
                        .WithDescription("Explicit branch description");
                    config.AddView<AttributedView>("dashboard")
                        .WithDescription("Explicit view description");
                });

                int exitCode = await app.RunAsync(new[] { "--help" });

                Assert.Equal(0, exitCode);
                VisualAssert.ContainsText(scope.Driver, "Explicit command description");
                VisualAssert.ContainsText(scope.Driver, "Explicit branch description");
                VisualAssert.ContainsText(scope.Driver, "Explicit view description");
                Assert.DoesNotContain("Run annotated command", scope.Driver.Text);
                Assert.DoesNotContain("Manage attributed operations", scope.Driver.Text);
                Assert.DoesNotContain("Open annotated dashboard", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Invalid top-level help paths return an error with nearest visible command suggestions.
        /// </summary>
        [Fact]
        public async Task CommandApp_HelpUnknownRootPath_ReturnsErrorAndSuggestion()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("tasks");
                    config.AddCommand("status", context => 0)
                        .WithDescription("Show status");
                    config.AddCommand("status-internal", context => 0)
                        .WithDescription("Hidden status")
                        .IsHidden();
                });

                int exitCode = await app.RunAsync(new[] { "help", "stauts" });

                Assert.Equal(1, exitCode);
                VisualAssert.ContainsText(scope.Driver, "Unknown command: stauts");
                VisualAssert.ContainsText(scope.Driver, "Did you mean 'status'?");
                VisualAssert.ContainsText(scope.Driver, "COMMANDS:");
                VisualAssert.ContainsText(scope.Driver, "status");
                Assert.DoesNotContain("status-internal", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Invalid branch help paths return an error with nearest visible child suggestions and branch help.
        /// </summary>
        [Fact]
        public async Task CommandApp_HelpUnknownBranchPath_ReturnsErrorAndSuggestion()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("gitish");
                    config.AddBranch("remote", remote =>
                    {
                        remote.SetDescription("Manage remotes");
                        remote.AddCommand("add", context => 0)
                            .WithDescription("Add a remote");
                        remote.AddCommand("apply", context => 0)
                            .WithDescription("Hidden apply")
                            .IsHidden();
                    });
                });

                int exitCode = await app.RunAsync(new[] { "help", "remote", "aad" });

                Assert.Equal(1, exitCode);
                VisualAssert.ContainsText(scope.Driver, "Unknown command: remote aad");
                VisualAssert.ContainsText(scope.Driver, "Did you mean 'add'?");
                VisualAssert.ContainsText(scope.Driver, "gitish remote [COMMAND] [ARGUMENTS]");
                VisualAssert.ContainsText(scope.Driver, "Manage remotes");
                VisualAssert.ContainsText(scope.Driver, "add");
                Assert.DoesNotContain("apply", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Extra path segments after a top-level command are treated as invalid help paths.
        /// </summary>
        [Fact]
        public async Task CommandApp_HelpTopLevelCommandWithExtraPath_ReturnsErrorAndCommandHelp()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("tasks");
                    config.AddCommand("list", context => 0)
                        .WithDescription("List items");
                });

                int exitCode = await app.RunAsync(new[] { "help", "list", "details" });

                Assert.Equal(1, exitCode);
                VisualAssert.ContainsText(scope.Driver, "Unknown command: list details");
                VisualAssert.ContainsText(scope.Driver, "tasks list [ARGUMENTS]");
                VisualAssert.ContainsText(scope.Driver, "List items");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Extra path segments after a branch command are treated as invalid help paths while preserving command context.
        /// </summary>
        [Fact]
        public async Task CommandApp_HelpBranchCommandWithExtraPath_ReturnsErrorAndCommandHelp()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("gitish");
                    config.AddBranch<RemoteSettings>("remote", remote =>
                    {
                        remote.AddCommand("status", context => 0)
                            .WithDescription("Show remote status");
                    });
                });

                int exitCode = await app.RunAsync(new[] { "help", "remote", "status", "details" });

                Assert.Equal(1, exitCode);
                VisualAssert.ContainsText(scope.Driver, "Unknown command: remote status details");
                VisualAssert.ContainsText(scope.Driver, "gitish remote status [OPTIONS]");
                VisualAssert.ContainsText(scope.Driver, "Show remote status");
                VisualAssert.ContainsText(scope.Driver, "INHERITED OPTIONS:");
                VisualAssert.ContainsText(scope.Driver, "-v, --verbose");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Batch aliases and examples reduce ceremony when porting larger command surfaces.
        /// </summary>
        [Fact]
        public async Task CommandApp_CommandBatchAliasesAndExamples_RenderExpectedOutput()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                int runCount = 0;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    ICommandConfigurator command = config.AddCommand("remove", context =>
                    {
                        runCount++;
                        context.Console.WriteLine("Batch remove");
                        return 0;
                    });

                    command.WithDescription("Remove an item")
                        .WithAliases("rm", "del")
                        .WithExamples(
                            new[] { "remove", "old-file.txt" },
                            new[] { "rm", "old-file.txt" });
                });

                int firstAliasExit = await app.RunAsync(new[] { "rm", "old-file.txt" });
                int secondAliasExit = await app.RunAsync(new[] { "del", "old-file.txt" });
                int helpExit = await app.RunAsync(new[] { "remove", "--help" });

                Assert.Equal(0, firstAliasExit);
                Assert.Equal(0, secondAliasExit);
                Assert.Equal(0, helpExit);
                Assert.Equal(2, runCount);
                VisualAssert.ContainsText(scope.Driver, "Batch remove");
                VisualAssert.ContainsText(scope.Driver, "rm, del");
                VisualAssert.ContainsText(scope.Driver, "app remove old-file.txt");
                VisualAssert.ContainsText(scope.Driver, "app rm old-file.txt");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Completion metadata exposes visible root commands, branches, aliases, and built-ins without running commands.
        /// </summary>
        [Fact]
        public void CommandApp_Completions_SuggestRootCommandsBranchesAliasesAndBuiltIns()
        {
            var app = new CommandApp();
            app.Configure(config =>
            {
                config.SetApplicationVersion("1.2.3");
                config.AddCommand("status", context => 0)
                    .WithDescription("Show status")
                    .WithAliases("s", "stat");
                config.AddCommand("secret", context => 0)
                    .IsHidden();
                config.AddBranch("remote", remote =>
                {
                    remote.SetDescription("Manage remotes");
                })
                .WithAlias("r");
            });

            IReadOnlyList<CommandCompletionItem> completions = app.GetCompletions(Array.Empty<string>());

            Assert.Contains(completions, item => item.Value == "status" && item.Kind == CommandCompletionKind.Command);
            Assert.Contains(completions, item => item.Value == "s" && item.Kind == CommandCompletionKind.Command);
            Assert.Contains(completions, item => item.Value == "stat" && item.Kind == CommandCompletionKind.Command);
            Assert.Contains(completions, item => item.Value == "remote" && item.Kind == CommandCompletionKind.Branch);
            Assert.Contains(completions, item => item.Value == "r" && item.Kind == CommandCompletionKind.Branch);
            Assert.Contains(completions, item => item.Value == "help" && item.Kind == CommandCompletionKind.BuiltIn);
            Assert.Contains(completions, item => item.Value == "version" && item.Kind == CommandCompletionKind.BuiltIn);
            Assert.DoesNotContain(completions, item => item.Value == "secret");
        }

        /// <summary>
        /// Completion metadata follows branch aliases and exposes branch options before child command selection.
        /// </summary>
        [Fact]
        public void CommandApp_Completions_SuggestBranchCommandsAliasesAndOptions()
        {
            var app = new CommandApp();
            app.Configure(config =>
            {
                config.AddBranch<RemoteSettings>("remote", remote =>
                {
                    remote.WithAlias("r");
                    remote.AddCommand("add", context => 0)
                        .WithAlias("a");
                    remote.AddCommand("list", context => 0)
                        .WithAlias("ls");
                    remote.AddCommand("secret", context => 0)
                        .IsHidden();
                });
            });

            IReadOnlyList<CommandCompletionItem> branchCompletions = app.GetCompletions(new[] { "r", string.Empty });
            IReadOnlyList<CommandCompletionItem> optionCompletions = app.GetCompletions(new[] { "remote", "--v" });

            Assert.Contains(branchCompletions, item => item.Value == "add" && item.Kind == CommandCompletionKind.Command);
            Assert.Contains(branchCompletions, item => item.Value == "a" && item.Kind == CommandCompletionKind.Command);
            Assert.Contains(branchCompletions, item => item.Value == "list" && item.Kind == CommandCompletionKind.Command);
            Assert.Contains(branchCompletions, item => item.Value == "ls" && item.Kind == CommandCompletionKind.Command);
            Assert.Contains(branchCompletions, item => item.Value == "--verbose" && item.Kind == CommandCompletionKind.Option);
            Assert.Contains(branchCompletions, item => item.Value == "-v" && item.Kind == CommandCompletionKind.Option);
            Assert.DoesNotContain(branchCompletions, item => item.Value == "secret");
            Assert.Contains(optionCompletions, item => item.Value == "--verbose" && item.Kind == CommandCompletionKind.Option);
            Assert.DoesNotContain(optionCompletions, item => item.Value == "add");
        }

        /// <summary>
        /// Completion metadata includes command options and friendly enum value spellings.
        /// </summary>
        [Fact]
        public void CommandApp_Completions_SuggestCommandOptionsAndEnumValues()
        {
            var app = new CommandApp();
            app.Configure(config => config.AddCommand<CopyCommand>("copy"));

            IReadOnlyList<CommandCompletionItem> optionCompletions = app.GetCompletions(new[] { "copy", "source.txt", "--m" });
            IReadOnlyList<CommandCompletionItem> valueCompletions = app.GetCompletions(new[] { "copy", "source.txt", "--mode", "release-" });
            IReadOnlyList<CommandCompletionItem> inlineValueCompletions = app.GetCompletions(new[] { "copy", "source.txt", "--mode=release-" });

            Assert.Contains(optionCompletions, item => item.Value == "--mode" && item.Kind == CommandCompletionKind.Option);
            Assert.Contains(valueCompletions, item => item.Value == "release-candidate" && item.Kind == CommandCompletionKind.Value);
            Assert.Contains(valueCompletions, item => item.Value == "release-train" && item.Kind == CommandCompletionKind.Value);
            Assert.Contains(inlineValueCompletions, item => item.Value == "--mode=release-candidate" && item.Kind == CommandCompletionKind.Value);
            Assert.Contains(inlineValueCompletions, item => item.Value == "--mode=release-train" && item.Kind == CommandCompletionKind.Value);
        }

        /// <summary>
        /// Completion metadata exposes default-command options at the root without requiring a command name.
        /// </summary>
        [Fact]
        public void CommandApp_Completions_SuggestDefaultCommandOptions()
        {
            var app = new CommandApp();
            app.Configure(config => config.SetDefaultCommand<CopyCommand>());

            IReadOnlyList<CommandCompletionItem> completions = app.GetCompletions(new[] { "--b" });

            Assert.Contains(completions, item => item.Value == "--buffer-size" && item.Kind == CommandCompletionKind.Option);
        }

        /// <summary>
        /// Top-level help includes application examples configured with AddExamples.
        /// </summary>
        [Fact]
        public async Task CommandApp_TopLevelHelp_RendersApplicationExamples()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("deploy");
                    config.AddExamples(
                        new[] { "production" },
                        new[] { "staging", "--force" });
                    config.AddCommand("status", context => 0)
                        .WithDescription("Show status");
                });

                int exitCode = await app.RunAsync(new[] { "--help" });

                Assert.Equal(0, exitCode);
                VisualAssert.ContainsText(scope.Driver, "EXAMPLES:");
                VisualAssert.ContainsText(scope.Driver, "deploy production");
                VisualAssert.ContainsText(scope.Driver, "deploy staging --force");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// AddExample is available through the Spectre-style configurator interface.
        /// </summary>
        [Fact]
        public async Task CommandApp_TopLevelExamples_AreConfigurableThroughInterface()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    IConfigurator configurator = config;
                    configurator.SetApplicationName("tool");
                    configurator.AddExample("doctor", "--verbose");
                });

                int exitCode = await app.RunAsync(new[] { "--help" });

                Assert.Equal(0, exitCode);
                VisualAssert.ContainsText(scope.Driver, "No commands registered.");
                VisualAssert.ContainsText(scope.Driver, "tool doctor --verbose");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Hidden top-level commands and branches stay invokable while being omitted from top-level help.
        /// </summary>
        [Fact]
        public async Task CommandApp_HiddenCommandsAndBranches_AreOmittedFromTopLevelHelpButInvokable()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("tasks");
                    config.AddCommand("status", context =>
                    {
                        context.Console.WriteLine("Visible status");
                        return 0;
                    })
                    .WithDescription("Show status");

                    config.AddCommand("diagnostics", context =>
                    {
                        context.Console.WriteLine("Diagnostics ran");
                        return 6;
                    })
                    .WithDescription("Internal diagnostics")
                    .IsHidden();

                    config.AddBranch("internal", branch =>
                    {
                        branch.AddCommand("ping", context =>
                        {
                            context.Console.WriteLine("Internal ping");
                            return 7;
                        });
                    })
                    .WithDescription("Internal tools")
                    .IsHidden();
                });

                int helpExit = await app.RunAsync(new[] { "--help" });
                string helpText = scope.Driver.Text;
                int diagnosticsExit = await app.RunAsync(new[] { "diagnostics" });
                int branchExit = await app.RunAsync(new[] { "internal", "ping" });

                Assert.Equal(0, helpExit);
                Assert.Contains("status", helpText);
                Assert.DoesNotContain("diagnostics", helpText);
                Assert.DoesNotContain("Internal diagnostics", helpText);
                Assert.DoesNotContain("internal", helpText);
                Assert.DoesNotContain("Internal tools", helpText);
                Assert.Equal(6, diagnosticsExit);
                Assert.Equal(7, branchExit);
                VisualAssert.ContainsText(scope.Driver, "Diagnostics ran");
                VisualAssert.ContainsText(scope.Driver, "Internal ping");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Hidden branch children stay invokable while being omitted from branch help.
        /// </summary>
        [Fact]
        public async Task CommandApp_HiddenBranchChildren_AreOmittedFromBranchHelpButInvokable()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("gitish");
                    config.AddBranch("remote", remote =>
                    {
                        remote.AddCommand("list", context =>
                        {
                            context.Console.WriteLine("Listed remotes");
                            return 0;
                        })
                        .WithDescription("List remotes");

                        remote.AddCommand("prune-cache", context =>
                        {
                            context.Console.WriteLine("Pruned cache");
                            return 8;
                        })
                        .WithDescription("Internal cache cleanup")
                        .IsHidden();

                        remote.AddBranch("internal", branch =>
                        {
                            branch.AddCommand("audit", context =>
                            {
                                context.Console.WriteLine("Audited remotes");
                                return 9;
                            });
                        })
                        .WithDescription("Internal remote tools")
                        .IsHidden();
                    });
                });

                int helpExit = await app.RunAsync(new[] { "remote", "--help" });
                string helpText = scope.Driver.Text;
                int hiddenCommandExit = await app.RunAsync(new[] { "remote", "prune-cache" });
                int hiddenBranchExit = await app.RunAsync(new[] { "remote", "internal", "audit" });

                Assert.Equal(0, helpExit);
                Assert.Contains("list", helpText);
                Assert.DoesNotContain("prune-cache", helpText);
                Assert.DoesNotContain("Internal cache cleanup", helpText);
                Assert.DoesNotContain("internal", helpText);
                Assert.DoesNotContain("Internal remote tools", helpText);
                Assert.Equal(8, hiddenCommandExit);
                Assert.Equal(9, hiddenBranchExit);
                VisualAssert.ContainsText(scope.Driver, "Pruned cache");
                VisualAssert.ContainsText(scope.Driver, "Audited remotes");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Example validation accepts full-path, relative, and branch examples without executing commands during validation.
        /// </summary>
        [Fact]
        public async Task CommandApp_ValidateExamples_AcceptsRelativeAndBranchExamples()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                CopyCommand.LastSettings = null;
                RemoteAddCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.ValidateExamples();
                    config.AddCommand<CopyCommand>("copy")
                        .WithExample("source.txt", "dest.txt", "--force");

                    config.AddBranch<RemoteSettings>("remote", remote =>
                    {
                        remote.AddCommand<RemoteAddCommand>("add")
                            .WithExample("remote", "--verbose", "add", "origin", "https://example.test/repo.git");
                    });
                });

                int exitCode = await app.RunAsync(new[] { "copy", "source.txt", "dest.txt" });

                Assert.Equal(0, exitCode);
                Assert.NotNull(CopyCommand.LastSettings);
                Assert.Null(RemoteAddCommand.LastSettings);
                VisualAssert.ContainsText(scope.Driver, "Copied source.txt -> dest.txt");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Example validation catches invalid typed command examples before command execution.
        /// </summary>
        [Fact]
        public async Task CommandApp_ValidateExamples_InvalidExampleReturnsCleanError()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                CopyCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.ValidateExamples();
                    config.AddCommand<CopyCommand>("copy")
                        .WithExample("copy", "--unknown");
                });

                int exitCode = await app.RunAsync(new[] { "copy", "source.txt" });

                Assert.Equal(1, exitCode);
                Assert.Null(CopyCommand.LastSettings);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Example for command 'copy' is invalid");
                VisualAssert.ContainsText(scope.Driver, "Unknown option '--unknown'.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Example validation includes expected option value names when an example omits an option value.
        /// </summary>
        [Fact]
        public async Task CommandApp_ValidateExamples_MissingOptionValueShowsExpectedValueName()
        {
            using (var scope = new ConsoleDriverScope(240))
            {
                CopyCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.ValidateExamples();
                    config.AddCommand<CopyCommand>("copy")
                        .WithExample("copy", "source.txt", "--buffer-size");
                });

                int exitCode = await app.RunAsync(new[] { "copy", "source.txt" });

                Assert.Equal(1, exitCode);
                Assert.Null(CopyCommand.LastSettings);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Example for command 'copy' is invalid");
                VisualAssert.ContainsText(scope.Driver, "Missing value for option '--buffer-size'. Expected <INT>.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Example validation also checks top-level application examples.
        /// </summary>
        [Fact]
        public async Task CommandApp_ValidateExamples_InvalidApplicationExampleReturnsCleanError()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                CopyCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.ValidateExamples();
                    config.AddExample("copy", "--unknown");
                    config.AddCommand<CopyCommand>("copy");
                });

                int exitCode = await app.RunAsync(new[] { "--help" });

                Assert.Equal(1, exitCode);
                Assert.Null(CopyCommand.LastSettings);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Application example is invalid: copy --unknown.");
                VisualAssert.ContainsText(scope.Driver, "Unknown option '--unknown'.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Default-command examples are validated through the same settings binder used by execution.
        /// </summary>
        [Fact]
        public async Task CommandApp_ValidateExamples_InvalidDefaultCommandExampleReturnsCleanError()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                CopyCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.ValidateExamples();
                    config.SetDefaultCommand<CopyCommand>()
                        .WithExample("--unknown");
                });

                int exitCode = await app.RunAsync(Array.Empty<string>());

                Assert.Equal(1, exitCode);
                Assert.Null(CopyCommand.LastSettings);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Example for default command is invalid: --unknown.");
                VisualAssert.ContainsText(scope.Driver, "Unknown option '--unknown'.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Exception handlers can render command failures and choose the exit code.
        /// </summary>
        [Fact]
        public async Task CommandApp_ExceptionHandler_RendersCustomErrorAndExitCode()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                CommandExceptionContext? observed = null;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("tasks");
                    config.SetExceptionHandler(context =>
                    {
                        observed = context;
                        context.Console.WriteLine("CUSTOM ERROR: " + context.Exception.Message);
                        return 42;
                    });

                    config.AddCommand("boom", new Action<CommandContext>(context =>
                    {
                        throw new InvalidOperationException("Boom");
                    }));
                });

                int exitCode = await app.RunAsync(new[] { "boom", "--dry-run" });

                Assert.Equal(42, exitCode);
                Assert.NotNull(observed);
                Assert.IsType<InvalidOperationException>(observed!.Exception);
                Assert.Equal(new[] { "boom", "--dry-run" }, observed.Arguments);
                Assert.Equal("tasks", observed.Settings.ApplicationName);
                VisualAssert.ContainsText(scope.Driver, "CUSTOM ERROR: Boom");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Exception handlers also receive parser and settings binding errors.
        /// </summary>
        [Fact]
        public async Task CommandApp_ExceptionHandler_HandlesParameterBindingErrors()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var handler = new RecordingExceptionHandler(17);
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetExceptionHandler(handler);
                    config.AddCommand<CopyCommand>("copy");
                });

                int exitCode = await app.RunAsync(new[] { "copy" });

                Assert.Equal(17, exitCode);
                Assert.NotNull(handler.Context);
                Assert.IsType<CommandParameterException>(handler.Context!.Exception);
                Assert.IsAssignableFrom<CommandParseException>(handler.Context.Exception);
                Assert.Equal(new[] { "copy" }, handler.Context.Arguments);
                VisualAssert.ContainsText(scope.Driver, "HANDLED: CommandParameterException: Missing required argument 'source'.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Exception handlers receive the original exception from typed command execution, not a reflection wrapper.
        /// </summary>
        [Fact]
        public async Task CommandApp_ExceptionHandler_ReceivesOriginalTypedCommandException()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var handler = new RecordingExceptionHandler(23);
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetExceptionHandler(handler);
                    config.AddCommand<ThrowingTypedCommand>("boom");
                });

                int exitCode = await app.RunAsync(new[] { "boom", "source.txt" });

                Assert.Equal(23, exitCode);
                Assert.NotNull(handler.Context);
                Assert.IsType<InvalidOperationException>(handler.Context!.Exception);
                Assert.Equal("Typed boom: source.txt", handler.Context.Exception.Message);
                VisualAssert.ContainsText(scope.Driver, "HANDLED: InvalidOperationException: Typed boom: source.txt");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Configuration mistakes use a typed command app exception.
        /// </summary>
        [Fact]
        public void CommandApp_ConfigurationErrors_UseCommandConfigurationException()
        {
            var app = new CommandApp();

            CommandConfigurationException exception = Assert.Throws<CommandConfigurationException>(() =>
            {
                app.Configure(config =>
                {
                    config.AddCommand("status", context => 0);
                    config.AddCommand("status", context => 0);
                });
            });

            Assert.Contains("already been registered", exception.Message);
        }

        /// <summary>
        /// Propagated parser errors can be caught through the Spectre-style parse exception base type.
        /// </summary>
        [Fact]
        public async Task CommandApp_ParserErrors_AreCommandParseExceptionsWhenPropagated()
        {
            using (new ConsoleDriverScope(100))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.PropagateExceptions();
                    config.AddCommand<CopyCommand>("copy");
                });

                Exception exception = await Record.ExceptionAsync(() => app.RunAsync(new[] { "copy" }));

                Assert.IsAssignableFrom<CommandParseException>(exception);
                Assert.IsType<CommandParameterException>(exception);
                Assert.Contains("Missing required argument 'source'", exception.Message);
            }
        }

        /// <summary>
        /// Propagated typed command failures preserve the original command exception type.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedCommandExceptions_PropagateOriginalException()
        {
            using (new ConsoleDriverScope(100))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.PropagateExceptions();
                    config.AddCommand<ThrowingTypedCommand>("boom");
                });

                InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
                    () => app.RunAsync(new[] { "boom", "source.txt" }));

                Assert.Equal("Typed boom: source.txt", exception.Message);
            }
        }

        /// <summary>
        /// Invalid option templates fail fast with the dedicated template exception when propagation is requested.
        /// </summary>
        [Fact]
        public async Task CommandApp_InvalidOptionTemplate_ThrowsCommandTemplateExceptionWhenPropagated()
        {
            using (new ConsoleDriverScope(100))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.PropagateExceptions();
                    config.AddCommand<InvalidOptionAliasSettings>("broken", (context, settings) => 0);
                });

                CommandTemplateException exception = await Assert.ThrowsAsync<CommandTemplateException>(
                    () => app.RunAsync(new[] { "broken" }));

                Assert.Contains("Option alias 'force' must start with '-' or '--'.", exception.Message);
            }
        }

        /// <summary>
        /// Invalid argument templates render clean command errors without exposing reflection wrappers.
        /// </summary>
        [Fact]
        public async Task CommandApp_InvalidArgumentTemplate_ReturnsCleanTemplateError()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.AddCommand<InvalidArgumentTemplateSettings>("broken", (context, settings) => 0);
                });

                int exitCode = await app.RunAsync(new[] { "broken", "source.txt" });

                Assert.Equal(1, exitCode);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Argument template 'source' must use <name> or [name] syntax.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Invalid option value templates are rejected before command execution.
        /// </summary>
        [Fact]
        public async Task CommandApp_InvalidOptionValueTemplate_ReturnsCleanTemplateError()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.AddCommand<InvalidOptionValueTemplateSettings>("broken", (context, settings) => 0);
                });

                int exitCode = await app.RunAsync(new[] { "broken", "--count", "3" });

                Assert.Equal(1, exitCode);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Option value template '<>' must contain a value name.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Valid templates still preserve domain-friendly value names in help output.
        /// </summary>
        [Fact]
        public async Task CommandApp_ValidTemplates_PreserveOptionalAndDomainValueNames()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("deploy");
                    config.AddCommand<TemplateShapeSettings>("shape", (context, settings) => 0);
                });

                int exitCode = await app.RunAsync(new[] { "shape", "--help" });

                Assert.Equal(0, exitCode);
                VisualAssert.ContainsText(scope.Driver, "<source>");
                VisualAssert.ContainsText(scope.Driver, "--env <KEY=VALUE>");
                VisualAssert.ContainsText(scope.Driver, "--port [PORT]");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Duplicate option aliases are rejected before a command can bind unpredictably.
        /// </summary>
        [Fact]
        public async Task CommandApp_DuplicateOptionAlias_ReturnsCleanTemplateError()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.AddCommand<DuplicateOptionAliasSettings>("broken", (context, settings) => 0);
                });

                int exitCode = await app.RunAsync(new[] { "broken", "--name", "Ada" });

                Assert.Equal(1, exitCode);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Option alias '--name' is used by multiple properties");
                VisualAssert.ContainsText(scope.Driver, "DuplicateOptionAliasSettings");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Duplicate positional argument slots use the template exception path when propagation is enabled.
        /// </summary>
        [Fact]
        public async Task CommandApp_DuplicateArgumentPosition_ThrowsCommandTemplateExceptionWhenPropagated()
        {
            using (new ConsoleDriverScope(100))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.PropagateExceptions();
                    config.AddCommand<DuplicateArgumentPositionSettings>("broken", (context, settings) => 0);
                });

                CommandTemplateException exception = await Assert.ThrowsAsync<CommandTemplateException>(
                    () => app.RunAsync(new[] { "broken", "source.txt", "dest.txt" }));

                Assert.Contains("Argument position '0' is used by multiple properties on 'DuplicateArgumentPositionSettings': Source, Destination.", exception.Message);
            }
        }

        /// <summary>
        /// Required positional arguments cannot follow optional ones because binding would be ambiguous.
        /// </summary>
        [Fact]
        public async Task CommandApp_RequiredArgumentAfterOptional_ReturnsCleanTemplateError()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.AddCommand<RequiredArgumentAfterOptionalSettings>("broken", (context, settings) => 0);
                });

                int exitCode = await app.RunAsync(new[] { "broken", "value" });

                Assert.Equal(1, exitCode);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Required argument '<destination>' cannot appear after optional argument '[source]'");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Propagated command creation or command-shape failures use a typed runtime exception.
        /// </summary>
        [Fact]
        public async Task CommandApp_RuntimeErrors_UseCommandRuntimeExceptionWhenCommandShapeIsInvalid()
        {
            using (new ConsoleDriverScope(100))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.PropagateExceptions();
                    config.AddCommand<NotACommand>("broken");
                });

                CommandRuntimeException exception = await Assert.ThrowsAsync<CommandRuntimeException>(
                    () => app.RunAsync(new[] { "broken" }));

                Assert.Contains("must implement ICommand or IAsyncCommand", exception.Message);
            }
        }

        /// <summary>
        /// Propagating exceptions remains the highest-priority behavior when configured.
        /// </summary>
        [Fact]
        public async Task CommandApp_PropagateExceptions_TakesPrecedenceOverExceptionHandler()
        {
            using (new ConsoleDriverScope(100))
            {
                bool handlerCalled = false;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.PropagateExceptions();
                    config.SetExceptionHandler(context =>
                    {
                        handlerCalled = true;
                        return 99;
                    });

                    config.AddCommand("boom", new Action<CommandContext>(context =>
                    {
                        throw new InvalidOperationException("Boom");
                    }));
                });

                InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
                    () => app.RunAsync(new[] { "boom" }));

                Assert.Equal("Boom", exception.Message);
                Assert.False(handlerCalled);
            }
        }

        /// <summary>
        /// Unknown commands return a non-zero exit code and render bounded help output.
        /// </summary>
        [Fact]
        public async Task CommandApp_UnknownCommand_ReturnsErrorAndHelp()
        {
            using (var scope = new ConsoleDriverScope(48))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("tasks");
                    config.AddCommand("list", context => 0).WithDescription("List items");
                });

                int exitCode = await app.RunAsync(new[] { "missing" });

                Assert.Equal(1, exitCode);
                VisualAssert.ContainsText(scope.Driver, "Unknown command: missing");
                VisualAssert.ContainsText(scope.Driver, "COMMANDS:");
                VisualAssert.ContainsText(scope.Driver, "list");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Unknown root commands suggest the nearest visible command before rendering bounded help output.
        /// </summary>
        [Fact]
        public async Task CommandApp_UnknownCommand_SuggestsNearestVisibleRootCommand()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("tasks");
                    config.AddCommand("status", context => 0).WithDescription("Show status");
                    config.AddCommand("stats-internal", context => 0).WithDescription("Internal stats").IsHidden();
                });

                int exitCode = await app.RunAsync(new[] { "stats" });

                Assert.Equal(1, exitCode);
                VisualAssert.ContainsText(scope.Driver, "Unknown command: stats");
                VisualAssert.ContainsText(scope.Driver, "Did you mean 'status'?");
                VisualAssert.ContainsText(scope.Driver, "COMMANDS:");
                VisualAssert.ContainsText(scope.Driver, "status");
                Assert.DoesNotContain("stats-internal", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Unknown root commands also suggest visible branches and can match through visible aliases.
        /// </summary>
        [Fact]
        public async Task CommandApp_UnknownCommand_SuggestsBranchesAndAliasMatches()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("deploy");
                    config.AddCommand("publish", context => 0)
                        .WithAlias("deploy")
                        .WithDescription("Publish services");
                    config.AddBranch("remote", remote =>
                    {
                        remote.SetDescription("Manage remotes");
                    });
                });

                int aliasExitCode = await app.RunAsync(new[] { "deply" });
                int branchExitCode = await app.RunAsync(new[] { "remtoe" });

                Assert.Equal(1, aliasExitCode);
                Assert.Equal(1, branchExitCode);
                VisualAssert.ContainsText(scope.Driver, "Unknown command: deply");
                VisualAssert.ContainsText(scope.Driver, "Did you mean 'publish'?");
                VisualAssert.ContainsText(scope.Driver, "Unknown command: remtoe");
                VisualAssert.ContainsText(scope.Driver, "Did you mean 'remote'?");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Unknown branch commands suggest the nearest visible child command without exposing hidden entries.
        /// </summary>
        [Fact]
        public async Task CommandApp_UnknownBranchCommand_SuggestsNearestVisibleChild()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("gitish");
                    config.AddBranch("remote", remote =>
                    {
                        remote.AddCommand("add", context => 0).WithDescription("Add a remote");
                        remote.AddCommand("apply", context => 0).WithDescription("Internal apply").IsHidden();
                    });
                });

                int exitCode = await app.RunAsync(new[] { "remote", "aad" });

                Assert.Equal(1, exitCode);
                VisualAssert.ContainsText(scope.Driver, "Unknown command: remote aad");
                VisualAssert.ContainsText(scope.Driver, "Did you mean 'add'?");
                VisualAssert.ContainsText(scope.Driver, "COMMANDS:");
                VisualAssert.ContainsText(scope.Driver, "add");
                Assert.DoesNotContain("apply", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Hidden commands remain invokable by exact name but are not used as typo suggestions.
        /// </summary>
        [Fact]
        public async Task CommandApp_UnknownCommand_DoesNotSuggestHiddenCommands()
        {
            using (var scope = new ConsoleDriverScope(72))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("tasks");
                    config.AddCommand("secret", context => 0).IsHidden();
                });

                int exitCode = await app.RunAsync(new[] { "secre" });

                Assert.Equal(1, exitCode);
                VisualAssert.ContainsText(scope.Driver, "Unknown command: secre");
                Assert.DoesNotContain("Did you mean", scope.Driver.Text);
                Assert.DoesNotContain("secret", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Case-sensitive parsing rejects differently cased command names while accepting exact matches.
        /// </summary>
        [Fact]
        public async Task CommandApp_SetCaseSensitive_ControlsCommandLookup()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("tasks");
                    config.SetCaseSensitive();
                    config.AddCommand("deploy", context =>
                    {
                        context.Console.WriteLine("Deploy exact");
                        return 0;
                    });
                });

                int mismatchExit = await app.RunAsync(new[] { "DEPLOY" });
                int exactExit = await app.RunAsync(new[] { "deploy" });

                Assert.Equal(1, mismatchExit);
                Assert.Equal(0, exactExit);
                VisualAssert.ContainsText(scope.Driver, "Unknown command: DEPLOY");
                VisualAssert.ContainsText(scope.Driver, "Deploy exact");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style CaseSensitivity settings map to Consoul command and option lookup behavior.
        /// </summary>
        [Fact]
        public async Task CommandApp_SettingsCaseSensitivity_ControlsCommandAndOptionLookup()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                CopyCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.Settings.CaseSensitivity = CaseSensitivity.All;
                    config.AddCommand<CopyCommand>("copy");
                });

                int commandMismatchExit = await app.RunAsync(new[] { "COPY", "source.txt" });
                int optionMismatchExit = await app.RunAsync(new[] { "copy", "source.txt", "--FORCE" });

                app.Configure(config => config.Settings.CaseSensitivity = CaseSensitivity.None);
                int insensitiveExit = await app.RunAsync(new[] { "COPY", "source.txt", "--FORCE" });

                Assert.Equal(1, commandMismatchExit);
                Assert.Equal(1, optionMismatchExit);
                Assert.Equal(0, insensitiveExit);
                Assert.NotNull(CopyCommand.LastSettings);
                Assert.True(CopyCommand.LastSettings!.Force);
                VisualAssert.ContainsText(scope.Driver, "Unknown command: COPY");
                VisualAssert.ContainsText(scope.Driver, "ERROR: Unknown option '--FORCE'.");
                VisualAssert.ContainsText(scope.Driver, "Copied source.txt -> .");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Configurator extensions apply Spectre-style settings aliases and culture-aware binding.
        /// </summary>
        [Fact]
        public async Task CommandApp_ConfiguratorExtensions_ApplySpectreStyleSettingsAndCulture()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                decimal observedAmount = 0m;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    IConfigurator configurator = config;
                    IConfigurator returned = configurator
                        .And(current => current.SetApplicationName("culture"))
                        .SetApplicationCulture(new CultureInfo("fr-FR"))
                        .CaseSensitivity(CaseSensitivity.All)
                        .UseStrictParsing();

                    Assert.Same(configurator, returned);
                    configurator.AddCommand<CultureAmountSettings>("price", (context, settings) =>
                    {
                        observedAmount = settings.Amount;
                        context.Console.WriteLine("Amount " + settings.Amount.ToString(CultureInfo.InvariantCulture));
                        return 24;
                    });
                });

                int commandMismatchExit = await app.RunAsync(new[] { "PRICE", "--amount", "1,5" });
                int strictParsingExit = await app.RunAsync(new[] { "price", "--amount", "1,5", "--extra" });
                int successExit = await app.RunAsync(new[] { "price", "--amount", "1,5" });

                Assert.Equal(1, commandMismatchExit);
                Assert.Equal(1, strictParsingExit);
                Assert.Equal(24, successExit);
                Assert.Equal(1.5m, observedAmount);
                VisualAssert.ContainsText(scope.Driver, "Unknown command: PRICE");
                VisualAssert.ContainsText(scope.Driver, "ERROR: Unknown option '--extra'.");
                VisualAssert.ContainsText(scope.Driver, "Amount 1.5");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// UseAssemblyInformationalVersion sets a non-empty version from assembly metadata.
        /// </summary>
        [Fact]
        public void CommandApp_UseAssemblyInformationalVersion_SetsConfiguredVersion()
        {
            var configurator = new CommandAppConfigurator();

            IConfigurator returned = configurator.UseAssemblyInformationalVersion();

            Assert.Same(configurator, returned);
            Assert.False(string.IsNullOrWhiteSpace(configurator.Settings.ApplicationVersion));
        }

        /// <summary>
        /// Spectre-style compatibility interfaces expose the existing configurator and app behavior.
        /// </summary>
        [Fact]
        public async Task CommandApp_CompatibilityInterfaces_ExposeConfigurationAndEnumerableRunSurface()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                ICommandApp app = new CommandApp();
                app.Configure(config =>
                {
                    IConfigurator configurator = config;
                    ICommandAppSettings settings = configurator.Settings;
                    settings.ApplicationName = "compat";
                    settings.CaseSensitivity = CaseSensitivity.None;

                    ICommandConfigurator command = configurator
                        .AddCommand("status", context =>
                        {
                            context.Console.WriteLine("Compatibility status");
                            return 15;
                        })
                        .WithDescription("Show compatibility status");

                    Assert.Equal("status", command.Name);
                    Assert.Equal("Show compatibility status", command.Description);

                    IBranchConfigurator branch = configurator.AddBranch("ops", ops =>
                    {
                        IBranchConfigurator branchConfigurator = ops;
                        branchConfigurator
                            .AddCommand("ping", context =>
                            {
                                context.Console.WriteLine("Compatibility ping");
                                return 16;
                            })
                            .WithDescription("Ping compatibility branch");
                    });

                    branch.WithDescription("Compatibility operations");
                });

                int statusExit = await app.RunAsync((IEnumerable<string>)new[] { "STATUS" });
                int branchExit = await app.RunAsync(new List<string> { "OPS", "PING" });

                Assert.Equal(15, statusExit);
                Assert.Equal(16, branchExit);
                VisualAssert.ContainsText(scope.Driver, "Compatibility status");
                VisualAssert.ContainsText(scope.Driver, "Compatibility ping");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// EmptyCommandSettings provides an explicit empty settings type for source-portable command classes.
        /// </summary>
        [Fact]
        public async Task CommandApp_EmptyCommandSettings_BindsExplicitEmptySettingsType()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                EmptySettingsCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.AddCommand<EmptySettingsCommand>("empty");
                });

                int exitCode = await app.RunAsync(new[] { "empty" });

                Assert.Equal(17, exitCode);
                Assert.NotNull(EmptySettingsCommand.LastSettings);
                VisualAssert.ContainsText(scope.Driver, "Empty settings");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// FlagValue options distinguish omitted flags, flags without values, and flags with explicit values.
        /// </summary>
        [Fact]
        public async Task CommandApp_FlagValueOptions_DistinguishUnsetDefaultAndExplicitValues()
        {
            using (var scope = new ConsoleDriverScope(120))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.AddCommand<FlagValueCommand>("listen");
                });

                int unsetExit = await app.RunAsync(new[] { "listen" });
                FlagValueSettings unset = FlagValueCommand.LastSettings!;
                int defaultExit = await app.RunAsync(new[] { "listen", "--port" });
                FlagValueSettings defaulted = FlagValueCommand.LastSettings!;
                int explicitExit = await app.RunAsync(new[] { "listen", "--port", "8080", "--timeout" });
                FlagValueSettings explicitPort = FlagValueCommand.LastSettings!;
                int nullableExit = await app.RunAsync(new[] { "listen", "--timeout", "15" });
                FlagValueSettings explicitTimeout = FlagValueCommand.LastSettings!;

                Assert.Equal(18, unsetExit);
                Assert.False(unset.Port.IsSet);
                Assert.Equal(0, unset.Port.Value);
                Assert.False(unset.Timeout.IsSet);
                Assert.Null(unset.Timeout.Value);

                Assert.Equal(18, defaultExit);
                Assert.True(defaulted.Port.IsSet);
                Assert.Equal(3000, defaulted.Port.Value);

                Assert.Equal(18, explicitExit);
                Assert.True(explicitPort.Port.IsSet);
                Assert.Equal(8080, explicitPort.Port.Value);
                Assert.True(explicitPort.Timeout.IsSet);
                Assert.Null(explicitPort.Timeout.Value);

                Assert.Equal(18, nullableExit);
                Assert.True(explicitTimeout.Timeout.IsSet);
                Assert.Equal(15, explicitTimeout.Timeout.Value);
                VisualAssert.ContainsText(scope.Driver, "port:not-set");
                VisualAssert.ContainsText(scope.Driver, "port:3000");
                VisualAssert.ContainsText(scope.Driver, "port:8080");
                VisualAssert.ContainsText(scope.Driver, "timeout:15");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Optional value templates render with square brackets in command help.
        /// </summary>
        [Fact]
        public async Task CommandApp_FlagValueHelp_RendersOptionalValueTemplate()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("server");
                    config.AddCommand<FlagValueCommand>("listen");
                });

                int exitCode = await app.RunAsync(new[] { "listen", "--help" });

                Assert.Equal(0, exitCode);
                VisualAssert.ContainsText(scope.Driver, "--port [PORT]");
                VisualAssert.ContainsText(scope.Driver, "--timeout [SECONDS]");
                VisualAssert.ContainsText(scope.Driver, "--host <HOST>");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Dictionary and lookup options bind repeated KEY=VALUE option values.
        /// </summary>
        [Fact]
        public async Task CommandApp_DictionaryAndLookupOptions_BindRepeatedKeyValuePairs()
        {
            using (var scope = new ConsoleDriverScope(120))
            {
                MetadataCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.AddCommand<MetadataCommand>("deploy");
                });

                int exitCode = await app.RunAsync(new[]
                {
                    "deploy",
                    "--env",
                    "region=us",
                    "--env",
                    "region=eu",
                    "--env",
                    "replicas=3",
                    "--tag",
                    "tier=api",
                    "--tag",
                    "tier=worker",
                    "--tag=region:eu",
                    "--limit",
                    "replicas=4"
                });

                Assert.Equal(20, exitCode);
                Assert.NotNull(MetadataCommand.LastSettings);
                Assert.Equal("eu", MetadataCommand.LastSettings!.Environment["region"]);
                Assert.Equal("3", MetadataCommand.LastSettings.Environment["replicas"]);
                Assert.Equal(4, MetadataCommand.LastSettings.Limits["replicas"]);
                Assert.Equal(new[] { "api", "worker" }, MetadataCommand.LastSettings.Tags["tier"].ToArray());
                Assert.Equal(new[] { "eu" }, MetadataCommand.LastSettings.Tags["region"].ToArray());
                VisualAssert.ContainsText(scope.Driver, "Metadata env:region=eu,replicas=3 tags:tier=api|worker");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Dictionary and lookup options report invalid key-value input as parser errors.
        /// </summary>
        [Fact]
        public async Task CommandApp_DictionaryOptions_InvalidPairReturnsCleanError()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                MetadataCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.AddCommand<MetadataCommand>("deploy");
                });

                int exitCode = await app.RunAsync(new[] { "deploy", "--env", "region" });

                Assert.Equal(1, exitCode);
                Assert.Null(MetadataCommand.LastSettings);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Value 'region' for option '--env' must use KEY=VALUE syntax.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// PairDeconstructor supports custom dictionary and lookup option syntaxes.
        /// </summary>
        [Fact]
        public async Task CommandApp_PairDeconstructor_BindsDictionaryAndLookupOptions()
        {
            using (var scope = new ConsoleDriverScope(120))
            {
                WeightedMetadataCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.AddCommand<WeightedMetadataCommand>("deploy");
                });

                int exitCode = await app.RunAsync(new[]
                {
                    "deploy",
                    "--weight",
                    "api/2",
                    "--weight",
                    "api/5",
                    "--weight=worker/3",
                    "--audit",
                    "api/1",
                    "--audit",
                    "api/2"
                });

                Assert.Equal(21, exitCode);
                Assert.NotNull(WeightedMetadataCommand.LastSettings);
                Assert.Equal(5, WeightedMetadataCommand.LastSettings!.Weights["api"]);
                Assert.Equal(3, WeightedMetadataCommand.LastSettings.Weights["worker"]);
                Assert.Equal(new[] { 1, 2 }, WeightedMetadataCommand.LastSettings.Audits["api"].ToArray());
                VisualAssert.ContainsText(scope.Driver, "Weighted api=5,worker=3 audits:api=1|2");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// PairDeconstructor failures return clean parser errors.
        /// </summary>
        [Fact]
        public async Task CommandApp_PairDeconstructor_InvalidPairReturnsCleanError()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                WeightedMetadataCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.AddCommand<WeightedMetadataCommand>("deploy");
                });

                int exitCode = await app.RunAsync(new[] { "deploy", "--weight", "api=2" });

                Assert.Equal(1, exitCode);
                Assert.Null(WeightedMetadataCommand.LastSettings);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Value 'api=2' for option '--weight' could not be deconstructed by 'SlashPairDeconstructor'.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Strict parsing rejects unknown options by default for typed settings commands.
        /// </summary>
        [Fact]
        public async Task CommandApp_StrictParsing_RejectsUnknownOptionsByDefault()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                ForwardCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<ForwardCommand>("forward"));

                int exitCode = await app.RunAsync(new[] { "forward", "tool", "--passthrough" });

                Assert.Equal(1, exitCode);
                Assert.Null(ForwardCommand.LastSettings);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Unknown option '--passthrough'.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Strict parsing suggests nearby visible options for typed command option typos.
        /// </summary>
        [Fact]
        public async Task CommandApp_StrictParsing_SuggestsNearestVisibleOption()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                CopyCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<CopyCommand>("copy"));

                int exitCode = await app.RunAsync(new[] { "copy", "source.txt", "--froce" });

                Assert.Equal(1, exitCode);
                Assert.Null(CopyCommand.LastSettings);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Unknown option '--froce'. Did you mean '--force'?");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Hidden options remain invokable by exact alias but are not used as typo suggestions.
        /// </summary>
        [Fact]
        public async Task CommandApp_StrictParsing_DoesNotSuggestHiddenOptions()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.AddCommand<HiddenOptionSettings>("config", (context, settings) => 0);
                });

                int exitCode = await app.RunAsync(new[] { "config", "service", "--secre" });

                Assert.Equal(1, exitCode);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Unknown option '--secre'.");
                Assert.DoesNotContain("Did you mean", scope.Driver.Text);
                Assert.DoesNotContain("--secret", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Branch shared settings reject mistyped branch options with option suggestions instead of command lookup noise.
        /// </summary>
        [Fact]
        public async Task CommandApp_BranchSharedOptionTypo_SuggestsNearestVisibleOption()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                RemoteAddCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("gitish");
                    config.AddBranch<RemoteSettings>("remote", remote =>
                    {
                        remote.AddCommand<RemoteAddCommand>("add");
                    });
                });

                int exitCode = await app.RunAsync(new[] { "remote", "--verbse", "add", "origin", "https://example.test/repo.git" });

                Assert.Equal(1, exitCode);
                Assert.Null(RemoteAddCommand.LastSettings);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Unknown option '--verbse'. Did you mean '--verbose'?");
                Assert.DoesNotContain("Unknown command: remote --verbse", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Branch shared settings report the expected value name when a branch option value is missing.
        /// </summary>
        [Fact]
        public async Task CommandApp_BranchSharedOptionMissingValueShowsExpectedValueName()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("work");
                    config.AddBranch<WorkspaceSettings>("workspace", workspace =>
                    {
                        workspace.AddCommand("status", context => 0);
                    });
                });

                int exitCode = await app.RunAsync(new[] { "workspace", "--path" });

                Assert.Equal(1, exitCode);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Missing value for option '--path'. Expected <PATH>.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Permissive parsing forwards unknown option-shaped tokens into positional catch-all arguments.
        /// </summary>
        [Fact]
        public async Task CommandApp_IgnoreUnknownOptions_ForwardsUnknownOptionsAsArguments()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                ForwardCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.IgnoreUnknownOptions();
                    config.AddCommand<ForwardCommand>("forward");
                });

                int exitCode = await app.RunAsync(new[] { "forward", "tool", "--passthrough", "value", "-x" });

                Assert.Equal(0, exitCode);
                Assert.NotNull(ForwardCommand.LastSettings);
                Assert.Equal("tool", ForwardCommand.LastSettings!.Target);
                Assert.Equal(new[] { "--passthrough", "value", "-x" }, ForwardCommand.LastSettings.Arguments);
                VisualAssert.ContainsText(scope.Driver, "Forward tool --passthrough|value|-x");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Remaining arguments capture forwarded raw tokens and provide an option lookup.
        /// </summary>
        [Fact]
        public async Task CommandApp_RemainingArguments_CapturesRawAndParsedForwardedTokens()
        {
            using (var scope = new ConsoleDriverScope(120))
            {
                RemainingForwardCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.IgnoreUnknownOptions();
                    config.AddCommand<RemainingForwardCommand>("wrap");
                });

                int exitCode = await app.RunAsync(new[]
                {
                    "wrap",
                    "tool",
                    "--passthrough",
                    "value",
                    "-x",
                    "--level=debug",
                    "tail"
                });

                Assert.Equal(19, exitCode);
                Assert.NotNull(RemainingForwardCommand.LastSettings);
                IRemainingArguments remaining = RemainingForwardCommand.LastSettings!.Remaining;
                Assert.Equal(new[] { "--passthrough", "value", "-x", "--level=debug", "tail" }, remaining.Raw);
                Assert.Equal("value", remaining.Parsed["--passthrough"].Single());
                Assert.Null(remaining.Parsed["-x"].Single());
                Assert.Equal("debug", remaining.Parsed["--level"].Single());
                VisualAssert.ContainsText(scope.Driver, "Remaining tool --passthrough|value|-x|--level=debug|tail");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// The -- delimiter sends subsequent tokens into remaining arguments without option parsing by Consoul.
        /// </summary>
        [Fact]
        public async Task CommandApp_RemainingArguments_CapturesDelimiterTokens()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                RemainingForwardCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<RemainingForwardCommand>("wrap"));

                int exitCode = await app.RunAsync(new[] { "wrap", "tool", "--", "--literal", "two" });

                Assert.Equal(19, exitCode);
                Assert.NotNull(RemainingForwardCommand.LastSettings);
                Assert.Equal(new[] { "--literal", "two" }, RemainingForwardCommand.LastSettings!.Remaining.Raw);
                Assert.Equal("two", RemainingForwardCommand.LastSettings.Remaining.Parsed["--literal"].Single());
                VisualAssert.ContainsText(scope.Driver, "Remaining tool --literal|two");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Async commands receive the cancellation token and can return custom exit codes.
        /// </summary>
        [Fact]
        public async Task CommandApp_AsyncCommand_ReceivesCancellationToken()
        {
            using (new ConsoleDriverScope(80))
            {
                AsyncTokenCommand.ObservedToken = default;
                var cts = new CancellationTokenSource();
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<AsyncTokenCommand>("async"));

                int exitCode = await app.RunAsync(new[] { "async" }, cts.Token);

                Assert.Equal(5, exitCode);
                Assert.Equal(cts.Token, AsyncTokenCommand.ObservedToken);
            }
        }

        /// <summary>
        /// Canceled command execution returns the Spectre-style default cancellation exit code.
        /// </summary>
        [Fact]
        public async Task CommandApp_CanceledCommand_ReturnsDefaultCancellationExitCode()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var cts = new CancellationTokenSource();
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.AddAsyncDelegate("wait", (context, cancellationToken) =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        return Task.FromResult(0);
                    });
                });

                cts.Cancel();
                int exitCode = await app.RunAsync(new[] { "wait" }, cts.Token);

                Assert.Equal(130, exitCode);
                Assert.DoesNotContain("ERROR:", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// CancellationExitCode customizes the code returned for canceled command execution.
        /// </summary>
        [Fact]
        public async Task CommandApp_CancellationExitCode_CustomizesCanceledCommandExitCode()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var cts = new CancellationTokenSource();
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.CancellationExitCode(99);
                    config.AddAsyncDelegate("wait", (context, cancellationToken) =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        return Task.FromResult(0);
                    });
                });

                cts.Cancel();
                int exitCode = await app.RunAsync(new[] { "wait" }, cts.Token);

                Assert.Equal(99, exitCode);
                Assert.DoesNotContain("ERROR:", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// PropagateExceptions keeps cancellation visible to host applications that request raw exceptions.
        /// </summary>
        [Fact]
        public async Task CommandApp_CanceledCommand_PropagatesWhenConfigured()
        {
            using (new ConsoleDriverScope(80))
            {
                var cts = new CancellationTokenSource();
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.PropagateExceptions();
                    config.CancellationExitCode(99);
                    config.AddAsyncDelegate("wait", (context, cancellationToken) =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        return Task.FromResult(0);
                    });
                });

                cts.Cancel();
                await Assert.ThrowsAsync<OperationCanceledException>(() => app.RunAsync(new[] { "wait" }, cts.Token));
            }
        }

        /// <summary>
        /// CommandContext exposes the cancellation token to synchronous commands.
        /// </summary>
        [Fact]
        public async Task CommandApp_CommandContext_ExposesCancellationTokenToSyncCommands()
        {
            using (new ConsoleDriverScope(80))
            {
                SyncTokenCommand.ObservedToken = default;
                var cts = new CancellationTokenSource();
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<SyncTokenCommand>("sync"));

                int exitCode = await app.RunAsync(new[] { "sync" }, cts.Token);

                Assert.Equal(6, exitCode);
                Assert.Equal(cts.Token, SyncTokenCommand.ObservedToken);
            }
        }

        /// <summary>
        /// Spectre-style synchronous command base classes run through the Consoul command app pipeline.
        /// </summary>
        [Fact]
        public async Task CommandApp_CommandBase_RunsWithoutSettingsAndReceivesCancellationToken()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                BaseStatusCommand.ObservedToken = default;
                var cts = new CancellationTokenSource();
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<BaseStatusCommand>("status"));

                int exitCode = await app.RunAsync(new[] { "status" }, cts.Token);

                Assert.Equal(7, exitCode);
                Assert.Equal(cts.Token, BaseStatusCommand.ObservedToken);
                VisualAssert.ContainsText(scope.Driver, "Base status");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style typed command base classes bind settings and receive cancellation tokens.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedCommandBase_BindsSettingsAndReceivesCancellationToken()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                BaseCopyCommand.LastSettings = null;
                BaseCopyCommand.ObservedToken = default;
                var cts = new CancellationTokenSource();
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<BaseCopyCommand>("copy"));

                int exitCode = await app.RunAsync(new[] { "copy", "source.txt", "--count", "3" }, cts.Token);

                Assert.Equal(3, exitCode);
                Assert.NotNull(BaseCopyCommand.LastSettings);
                Assert.Equal("source.txt", BaseCopyCommand.LastSettings!.Source);
                Assert.Equal(3, BaseCopyCommand.LastSettings.Count);
                Assert.Equal(cts.Token, BaseCopyCommand.ObservedToken);
                VisualAssert.ContainsText(scope.Driver, "Base copy source.txt x3");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style asynchronous command base classes run through the Consoul command app pipeline.
        /// </summary>
        [Fact]
        public async Task CommandApp_AsyncCommandBase_RunsWithoutSettingsAndReceivesCancellationToken()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                BaseAsyncStatusCommand.ObservedToken = default;
                var cts = new CancellationTokenSource();
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<BaseAsyncStatusCommand>("async-status"));

                int exitCode = await app.RunAsync(new[] { "async-status" }, cts.Token);

                Assert.Equal(8, exitCode);
                Assert.Equal(cts.Token, BaseAsyncStatusCommand.ObservedToken);
                VisualAssert.ContainsText(scope.Driver, "Async base status");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style typed asynchronous command base classes bind settings and receive cancellation tokens.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedAsyncCommandBase_BindsSettingsAndReceivesCancellationToken()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                BasePublishCommand.LastSettings = null;
                BasePublishCommand.ObservedToken = default;
                var cts = new CancellationTokenSource();
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<BasePublishCommand>("publish"));

                int exitCode = await app.RunAsync(new[] { "publish", "artifact.zip", "--environment", "prod" }, cts.Token);

                Assert.Equal(9, exitCode);
                Assert.NotNull(BasePublishCommand.LastSettings);
                Assert.Equal("artifact.zip", BasePublishCommand.LastSettings!.Artifact);
                Assert.Equal("prod", BasePublishCommand.LastSettings.Environment);
                Assert.Equal(cts.Token, BasePublishCommand.ObservedToken);
                VisualAssert.ContainsText(scope.Driver, "Async publish artifact.zip to prod");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Interceptors wrap typed command execution and can adjust the final exit code.
        /// </summary>
        [Fact]
        public async Task CommandApp_Interceptor_WrapsTypedCommandAndCanAdjustExitCode()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var interceptor = new RecordingInterceptor(resultOverride: 12);
                CopyCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetInterceptor(interceptor);
                    config.AddCommand<CopyCommand>("copy");
                });

                int exitCode = await app.RunAsync(new[] { "copy", "source.txt", "dest.txt" });

                Assert.Equal(12, exitCode);
                Assert.Equal(new[] { "before:copy", "after:copy:0" }, interceptor.Events);
                Assert.Same(CopyCommand.LastSettings, interceptor.Settings);
                Assert.Equal("source.txt", ((CopySettings)interceptor.Settings!).Source);
                VisualAssert.ContainsText(scope.Driver, "Copied source.txt -> dest.txt");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Interceptors also wrap untyped delegate commands and receive null settings.
        /// </summary>
        [Fact]
        public async Task CommandApp_Interceptor_WrapsUntypedDelegateCommand()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var interceptor = new RecordingInterceptor();
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetInterceptor(interceptor);
                    config.AddCommand("status", context =>
                    {
                        context.Console.WriteLine("Status ran");
                        return 4;
                    });
                });

                int exitCode = await app.RunAsync(new[] { "status" });

                Assert.Equal(4, exitCode);
                Assert.Equal(new[] { "before:status", "after:status:4" }, interceptor.Events);
                Assert.Null(interceptor.Settings);
                VisualAssert.ContainsText(scope.Driver, "Status ran");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Interceptors can observe cancellation through CommandContext.
        /// </summary>
        [Fact]
        public async Task CommandApp_Interceptor_SeesContextCancellationToken()
        {
            using (new ConsoleDriverScope(80))
            {
                var interceptor = new RecordingInterceptor();
                var cts = new CancellationTokenSource();
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetInterceptor(interceptor);
                    config.AddCommand("status", context => 0);
                });

                int exitCode = await app.RunAsync(new[] { "status" }, cts.Token);

                Assert.Equal(0, exitCode);
                Assert.Equal(cts.Token, interceptor.ObservedToken);
            }
        }

        /// <summary>
        /// CommandContext exposes Spectre-style aliases and a shared data bag for POC services.
        /// </summary>
        [Fact]
        public async Task CommandApp_CommandContext_ExposesNameRemainingAndSharedData()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var interceptor = new ContextDataInterceptor();
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetInterceptor(interceptor);
                    config.AddCommand("wrap", context =>
                    {
                        Assert.Equal(context.CommandName, context.Name);
                        Assert.Same(context.Arguments, context.Remaining);
                        Assert.Equal(new[] { "tool", "--level", "debug" }, context.Remaining);
                        context.Console.WriteLine(context.Name + " " + string.Join("|", context.Remaining));
                        context.Console.WriteLine((string)context.Data["trace"]);
                        context.Data["handler"] = "ran";
                        return 6;
                    });
                });

                int exitCode = await app.RunAsync(new[] { "wrap", "tool", "--level", "debug" });

                Assert.Equal(6, exitCode);
                Assert.Equal(new[] { "before:wrap", "after:wrap:ran:6" }, interceptor.Events);
                VisualAssert.ContainsText(scope.Driver, "wrap tool|--level|debug");
                VisualAssert.ContainsText(scope.Driver, "intercepted:wrap");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// The base interceptor class lets POC commands override only the lifecycle hook they need.
        /// </summary>
        [Fact]
        public async Task CommandApp_CommandInterceptorBase_AllowsPartialOverrides()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var interceptor = new BeforeOnlyInterceptor();
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetInterceptor(interceptor);
                    config.AddCommand("status", context =>
                    {
                        context.Console.WriteLine("Status ran");
                        return 4;
                    });
                });

                int exitCode = await app.RunAsync(new[] { "status" });

                Assert.Equal(4, exitCode);
                Assert.Equal("status", interceptor.CommandName);
                Assert.Null(interceptor.Settings);
                VisualAssert.ContainsText(scope.Driver, "Status ran");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Typed interceptor base classes receive matching settings without requiring manual casts.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedCommandInterceptorBase_ReceivesMatchingSettingsAndCanAdjustExitCode()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var interceptor = new CopySettingsInterceptor(resultOverride: 23);
                CopyCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetInterceptor(interceptor);
                    config.AddCommand<CopyCommand>("copy");
                });

                int exitCode = await app.RunAsync(new[] { "copy", "source.txt", "dest.txt", "--force" });

                Assert.Equal(23, exitCode);
                Assert.Equal(new[] { "before:copy:source.txt", "after:copy:0" }, interceptor.Events);
                Assert.Same(CopyCommand.LastSettings, interceptor.Settings);
                Assert.True(interceptor.Settings!.Force);
                VisualAssert.ContainsText(scope.Driver, "Copied source.txt -> dest.txt");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Typed interceptors ignore commands that bind another settings type.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedCommandInterceptorBase_IgnoresOtherSettingsTypes()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var interceptor = new CopySettingsInterceptor(resultOverride: 23);
                BasePublishCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetInterceptor(interceptor);
                    config.AddCommand<BasePublishCommand>("publish");
                });

                int exitCode = await app.RunAsync(new[] { "publish", "artifact.zip", "--environment", "prod" });

                Assert.Equal(9, exitCode);
                Assert.Empty(interceptor.Events);
                Assert.Null(interceptor.Settings);
                Assert.NotNull(BasePublishCommand.LastSettings);
                VisualAssert.ContainsText(scope.Driver, "Async publish artifact.zip to prod");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Command activators create command classes that use constructor-injected dependencies.
        /// </summary>
        [Fact]
        public async Task CommandApp_CommandActivator_CreatesConstructorInjectedCommand()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var service = new MessageService("Injected command");
                var activator = new TestCommandActivator(type =>
                {
                    if (type == typeof(InjectedCommand))
                    {
                        return new InjectedCommand(service);
                    }

                    return Activator.CreateInstance(type)!;
                });

                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetCommandActivator(activator);
                    config.AddCommand<InjectedCommand>("injected");
                });

                int exitCode = await app.RunAsync(new[] { "injected" });

                Assert.Equal(0, exitCode);
                Assert.Contains(typeof(InjectedCommand), activator.CreatedTypes);
                VisualAssert.ContainsText(scope.Driver, "Injected command");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Type registrar constructors register command types and injectable console services before resolving commands.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypeRegistrarConstructor_ResolvesConstructorInjectedCommand()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var registrar = new TestTypeRegistrar();
                registrar.RegisterInstance(typeof(MessageService), new MessageService("Registrar command"));
                var app = new CommandApp(registrar);
                app.Configure(config =>
                {
                    config.AddCommand<RegistrarInjectedCommand>("injected");
                });

                int exitCode = await app.RunAsync(new[] { "injected" });

                Assert.Equal(12, exitCode);
                Assert.Contains(typeof(IAnsiConsole), registrar.RegisteredServices);
                Assert.Contains(typeof(RegistrarInjectedCommand), registrar.RegisteredServices);
                Assert.Equal(1, registrar.BuildCount);
                VisualAssert.ContainsText(scope.Driver, "Registrar command");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Type registrar frontends can configure reusable service registrations before command resolution.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypeRegistrarFrontendConstructor_ConfiguresRegistrarBeforeResolution()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var registrar = new TestTypeRegistrar();
                var frontend = new TestTypeRegistrarFrontend("Frontend command");
                var app = new CommandApp(registrar, frontend);
                app.Configure(config =>
                {
                    config.AddCommand<RegistrarInjectedCommand>("injected");
                });

                int exitCode = await app.RunAsync(new[] { "injected" });

                Assert.Equal(12, exitCode);
                Assert.Equal(1, frontend.ConfigureCount);
                Assert.Contains(typeof(MessageService), registrar.RegisteredServices);
                Assert.Contains(typeof(RegistrarInjectedCommand), registrar.RegisteredServices);
                VisualAssert.ContainsText(scope.Driver, "Frontend command");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Configurator interfaces expose the registrar frontend path for shared setup helpers.
        /// </summary>
        [Fact]
        public async Task CommandApp_ConfiguratorSetTypeRegistrarFrontend_ConfiguresRegistrarBeforeResolution()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var registrar = new TestTypeRegistrar();
                var frontend = new TestTypeRegistrarFrontend("Configurator frontend command");
                var app = new CommandApp();
                app.Configure(config =>
                {
                    IConfigurator configurator = config;
                    configurator.SetTypeRegistrar(registrar, frontend);
                    configurator.AddCommand<RegistrarInjectedCommand>("injected");
                });

                int exitCode = await app.RunAsync(new[] { "injected" });

                Assert.Equal(12, exitCode);
                Assert.Equal(1, frontend.ConfigureCount);
                Assert.Contains(typeof(MessageService), registrar.RegisteredServices);
                Assert.Contains(typeof(RegistrarInjectedCommand), registrar.RegisteredServices);
                VisualAssert.ContainsText(scope.Driver, "Configurator frontend command");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Type registrars also resolve typed default commands.
        /// </summary>
        [Fact]
        public async Task CommandApp_GenericTypeRegistrarConstructor_ResolvesDefaultCommand()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var registrar = new TestTypeRegistrar();
                registrar.RegisterInstance(typeof(MessageService), new MessageService("Registrar default"));
                var app = new CommandApp<RegistrarDefaultCommand>(registrar);

                int exitCode = await app.RunAsync(Array.Empty<string>());

                Assert.Equal(13, exitCode);
                Assert.Contains(typeof(RegistrarDefaultCommand), registrar.RegisteredServices);
                VisualAssert.ContainsText(scope.Driver, "Registrar default");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Branch registrations flow through the same registrar path as top-level commands.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypeRegistrar_ResolvesNestedBranchCommand()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var registrar = new TestTypeRegistrar();
                registrar.RegisterInstance(typeof(MessageService), new MessageService("Registrar branch"));
                var app = new CommandApp(registrar);
                app.Configure(config =>
                {
                    config.AddBranch("tools", tools =>
                    {
                        tools.AddCommand<RegistrarInjectedCommand>("injected");
                    });
                });

                int exitCode = await app.RunAsync(new[] { "tools", "injected" });

                Assert.Equal(12, exitCode);
                Assert.Contains(typeof(RegistrarInjectedCommand), registrar.RegisteredServices);
                VisualAssert.ContainsText(scope.Driver, "Registrar branch");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Parsed settings are registered before command resolution so lightweight DI bridges can inject them.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypeRegistrar_RegistersParsedSettingsBeforeCommandResolution()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var registrar = new TestTypeRegistrar();
                var app = new CommandApp(registrar);
                app.Configure(config =>
                {
                    config.AddCommand<SettingsInjectedCommand>("show");
                });

                int exitCode = await app.RunAsync(new[] { "show", "--name", "Ada" });

                Assert.Equal(14, exitCode);
                Assert.Contains(typeof(SettingsInjectedSettings), registrar.RegisteredServices);
                VisualAssert.ContainsText(scope.Driver, "Settings Ada");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Explicit command factories support branch commands with local POC dependencies.
        /// </summary>
        [Fact]
        public async Task CommandApp_CommandFactory_CreatesConstructorInjectedBranchCommand()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var service = new MessageService("Branch factory command");
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.AddBranch("tools", tools =>
                    {
                        tools.AddCommand("injected", () => new InjectedCommand(service));
                    });
                });

                int exitCode = await app.RunAsync(new[] { "tools", "injected" });

                Assert.Equal(0, exitCode);
                VisualAssert.ContainsText(scope.Driver, "Branch factory command");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// View commands use the command activator so MVC-style views can have constructor dependencies.
        /// </summary>
        [Fact]
        public async Task CommandApp_AddView_UsesCommandActivatorForMvcView()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var service = new MessageService("Injected view");
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetCommandActivator(type =>
                    {
                        if (type == typeof(InjectedView))
                        {
                            return new InjectedView(service);
                        }

                        return Activator.CreateInstance(type)!;
                    });

                    config.AddView<InjectedView>("dashboard");
                });

                int exitCode = await app.RunAsync(new[] { "dashboard" });

                Assert.Equal(0, exitCode);
                VisualAssert.ContainsText(scope.Driver, "Injected view");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// CommandApp with a default command runs it when no command name is supplied.
        /// </summary>
        [Fact]
        public async Task CommandApp_DefaultCommand_RunsWhenNoArgs()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                DefaultCommand.Ran = false;
                var app = new CommandApp<DefaultCommand>();

                int exitCode = await app.RunAsync(Array.Empty<string>());

                Assert.Equal(3, exitCode);
                Assert.True(DefaultCommand.Ran);
                VisualAssert.ContainsText(scope.Driver, "Default command");
            }
        }

        /// <summary>
        /// Generic default commands bind arguments when no registered command name is supplied.
        /// </summary>
        [Fact]
        public async Task CommandApp_GenericDefaultCommand_BindsArgumentsWhenNoCommandMatches()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                CopyCommand.LastSettings = null;
                var app = new CommandApp<CopyCommand>();

                int exitCode = await app.RunAsync(new[] { "source.txt", "dest.txt", "--force" });

                Assert.Equal(0, exitCode);
                Assert.NotNull(CopyCommand.LastSettings);
                Assert.Equal("source.txt", CopyCommand.LastSettings!.Source);
                Assert.Equal("dest.txt", CopyCommand.LastSettings.Destination);
                Assert.True(CopyCommand.LastSettings.Force);
                VisualAssert.ContainsText(scope.Driver, "Copied source.txt -> dest.txt");
                Assert.DoesNotContain("Unknown command", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Default-only command apps show default command arguments and options in root help.
        /// </summary>
        [Fact]
        public async Task CommandApp_GenericDefaultCommandHelp_RendersDefaultSettings()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var app = new CommandApp<CopyCommand>();
                app.Configure(config => config.SetApplicationName("files"));

                int exitCode = await app.RunAsync(new[] { "--help" });

                Assert.Equal(0, exitCode);
                VisualAssert.ContainsText(scope.Driver, "files <source> [destination] [OPTIONS]");
                VisualAssert.ContainsText(scope.Driver, "ARGUMENTS:");
                VisualAssert.ContainsText(scope.Driver, "<source>");
                VisualAssert.ContainsText(scope.Driver, "[destination]");
                VisualAssert.ContainsText(scope.Driver, "OPTIONS:");
                VisualAssert.ContainsText(scope.Driver, "-f, --force");
                Assert.DoesNotContain("No commands registered.", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Configured default commands run when no command name is supplied.
        /// </summary>
        [Fact]
        public async Task CommandApp_ConfiguredDefaultCommand_RunsWhenNoArgs()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                ConfiguredDefaultCommand.Ran = false;
                var app = new CommandApp();
                app.Configure(config => config.SetDefaultCommand<ConfiguredDefaultCommand>());

                int exitCode = await app.RunAsync(Array.Empty<string>());

                Assert.Equal(10, exitCode);
                Assert.True(ConfiguredDefaultCommand.Ran);
                VisualAssert.ContainsText(scope.Driver, "Configured default command");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Configured default commands accept arguments while named commands still take precedence.
        /// </summary>
        [Fact]
        public async Task CommandApp_ConfiguredDefaultCommand_BindsArgumentsAfterNamedCommandsHavePriority()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                CopyCommand.LastSettings = null;
                bool namedCommandRan = false;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.ValidateExamples();
                    config.AddExample("source.txt", "--force");
                    config.SetDefaultCommand<CopyCommand>();
                    config.AddCommand("status", context =>
                    {
                        namedCommandRan = true;
                        context.Console.WriteLine("Named status");
                        return 4;
                    });
                });

                int defaultExit = await app.RunAsync(new[] { "source.txt", "--force" });
                int namedExit = await app.RunAsync(new[] { "status" });

                Assert.Equal(0, defaultExit);
                Assert.Equal(4, namedExit);
                Assert.NotNull(CopyCommand.LastSettings);
                Assert.Equal("source.txt", CopyCommand.LastSettings!.Source);
                Assert.True(CopyCommand.LastSettings.Force);
                Assert.True(namedCommandRan);
                VisualAssert.ContainsText(scope.Driver, "Copied source.txt -> .");
                VisualAssert.ContainsText(scope.Driver, "Named status");
                Assert.DoesNotContain("Unknown command: source.txt", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Root help includes default-command usage while preserving named command listings.
        /// </summary>
        [Fact]
        public async Task CommandApp_ConfiguredDefaultCommandHelp_RendersDefaultUsageAndNamedCommands()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("tool");
                    config.SetDefaultCommand<CopyCommand>()
                        .WithDescription("Copy by default")
                        .WithExample("source.txt", "dest.txt", "--force");
                    config.AddCommand("status", context => 0)
                        .WithDescription("Show status");
                });

                int exitCode = await app.RunAsync(new[] { "--help" });

                Assert.Equal(0, exitCode);
                VisualAssert.ContainsText(scope.Driver, "tool [COMMAND] [ARGUMENTS]");
                VisualAssert.ContainsText(scope.Driver, "tool <source> [destination] [OPTIONS]");
                VisualAssert.ContainsText(scope.Driver, "DEFAULT COMMAND:");
                VisualAssert.ContainsText(scope.Driver, "Copy by default");
                VisualAssert.ContainsText(scope.Driver, "DEFAULT EXAMPLES:");
                VisualAssert.ContainsText(scope.Driver, "tool source.txt dest.txt --force");
                VisualAssert.ContainsText(scope.Driver, "COMMANDS:");
                VisualAssert.ContainsText(scope.Driver, "status");
                VisualAssert.ContainsText(scope.Driver, "Show status");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Default command factories allow constructor dependencies without requiring the generic app type.
        /// </summary>
        [Fact]
        public async Task CommandApp_ConfiguredDefaultCommandFactory_CreatesInjectedDefaultCommand()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                var service = new MessageService("Injected default command");
                var app = new CommandApp();
                app.Configure(config => config.SetDefaultCommand(() => new InjectedCommand(service)));

                int exitCode = await app.RunAsync(Array.Empty<string>());

                Assert.Equal(0, exitCode);
                VisualAssert.ContainsText(scope.Driver, "Injected default command");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// A configured default command overrides the generic app default command.
        /// </summary>
        [Fact]
        public async Task CommandApp_ConfiguredDefaultCommand_OverridesGenericDefaultCommand()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                DefaultCommand.Ran = false;
                ConfiguredDefaultCommand.Ran = false;
                var app = new CommandApp<DefaultCommand>();
                app.Configure(config => config.SetDefaultCommand<ConfiguredDefaultCommand>());

                int exitCode = await app.RunAsync(Array.Empty<string>());

                Assert.Equal(10, exitCode);
                Assert.False(DefaultCommand.Ran);
                Assert.True(ConfiguredDefaultCommand.Ran);
                VisualAssert.ContainsText(scope.Driver, "Configured default command");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// View commands launch existing MVC-style views without a separate command abstraction.
        /// </summary>
        [Fact]
        public async Task CommandApp_AddView_RendersMvcView()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                TestView.Rendered = false;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("tool");
                    config.AddView<TestView>("dashboard")
                        .WithDescription("Open dashboard")
                        .WithAlias("dash");
                });

                int exitCode = await app.RunAsync(new[] { "dash" });

                Assert.Equal(0, exitCode);
                Assert.True(TestView.Rendered);
                VisualAssert.ContainsText(scope.Driver, "Rendered dashboard");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// View commands enter the normal view renderer so MVC navigation requests are honored.
        /// </summary>
        [Fact]
        public async Task CommandApp_AddView_UsesViewRendererNavigation()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                RendererAwareView.Reset();
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("tool");
                    config.AddView<RendererAwareView>("dashboard");
                });

                int exitCode = await app.RunAsync(new[] { "dashboard" });

                Assert.Equal(0, exitCode);
                Assert.True(RendererAwareView.Rendered);
                Assert.True(RendererAwareView.ReceivedNavigationContext);
                Assert.Equal(new[] { "Renderer Aware" }, RendererAwareView.BreadcrumbTrail);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Typed settings bind positional arguments, named options, flags, arrays, enums, and default values.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedSettings_BindsArgumentsAndOptions()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                CopyCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("files");
                    config.AddCommand<CopyCommand>("copy")
                        .WithDescription("Copy files");
                });

                int exitCode = await app.RunAsync(new[]
                {
                    "copy",
                    "source.txt",
                    "dest.txt",
                    "--force",
                    "-b",
                    "128",
                    "--mode",
                    "Release",
                    "--tag",
                    "api",
                    "--tag=prod"
                });

                Assert.Equal(0, exitCode);
                Assert.NotNull(CopyCommand.LastSettings);
                Assert.Equal("source.txt", CopyCommand.LastSettings!.Source);
                Assert.Equal("dest.txt", CopyCommand.LastSettings.Destination);
                Assert.True(CopyCommand.LastSettings.Force);
                Assert.Equal(128, CopyCommand.LastSettings.BufferSize);
                Assert.Equal(BuildMode.Release, CopyCommand.LastSettings.Mode);
                Assert.Equal(new[] { "api", "prod" }, CopyCommand.LastSettings.Tags);
                VisualAssert.ContainsText(scope.Driver, "Copied source.txt -> dest.txt");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Typed settings accept clustered single-character boolean flags while leaving valued short options separate.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedSettings_BindsClusteredShortBooleanFlags()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                ClusteredFlagCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("files");
                    config.AddCommand<ClusteredFlagCommand>("copy")
                        .WithDescription("Copy files");
                });

                int exitCode = await app.RunAsync(new[]
                {
                    "copy",
                    "source.txt",
                    "dest.txt",
                    "-vf",
                    "-b",
                    "128"
                });

                Assert.Equal(0, exitCode);
                Assert.NotNull(ClusteredFlagCommand.LastSettings);
                Assert.True(ClusteredFlagCommand.LastSettings!.Verbose);
                Assert.True(ClusteredFlagCommand.LastSettings.Force);
                Assert.Equal(128, ClusteredFlagCommand.LastSettings.BufferSize);
                VisualAssert.ContainsText(scope.Driver, "Copied source.txt -> dest.txt");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Typed settings bind repeated options and remaining positional values into generic collection properties.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedSettings_BindsGenericCollectionArgumentsAndOptions()
        {
            using (var scope = new ConsoleDriverScope(120))
            {
                CollectionCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("collect");
                    config.AddCommand<CollectionCommand>("copy")
                        .WithDescription("Copy collection-backed settings");
                });

                int exitCode = await app.RunAsync(new[]
                {
                    "copy",
                    "1",
                    "2",
                    "--name",
                    "api",
                    "--name=worker",
                    "--level",
                    "3",
                    "--level",
                    "5",
                    "--code",
                    "8",
                    "--code=13"
                });

                int helpExitCode = await app.RunAsync(new[] { "copy", "--help" });

                Assert.Equal(4, exitCode);
                Assert.Equal(0, helpExitCode);
                Assert.NotNull(CollectionCommand.LastSettings);
                Assert.Equal(new[] { 1, 2 }, CollectionCommand.LastSettings!.Items);
                Assert.Equal(new[] { "api", "worker" }, CollectionCommand.LastSettings.Names);
                Assert.Equal(new[] { 3, 5 }, CollectionCommand.LastSettings.Levels);
                Assert.Equal(new[] { 8, 13 }, CollectionCommand.LastSettings.Codes);
                VisualAssert.ContainsText(scope.Driver, "Collections items:1|2 names:api|worker levels:3|5 codes:8|13");
                VisualAssert.ContainsText(scope.Driver, "--code <INT32>");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Environment variables seed option values and satisfy required options without adding configuration packages.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedSettings_BindsOptionEnvironmentVariables()
        {
            using (var env = new EnvironmentVariableScope(new Dictionary<string, string?>
            {
                [EnvEnvironmentVariable] = "staging",
                [EnvReplicasVariable] = "4",
                [EnvDryRunVariable] = "true",
                [EnvModeVariable] = "release-candidate"
            }))
            using (var scope = new ConsoleDriverScope(120))
            {
                EnvCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<EnvCommand>("deploy"));

                int exitCode = await app.RunAsync(new[] { "deploy", "api" });

                Assert.Equal(0, exitCode);
                Assert.NotNull(EnvCommand.LastSettings);
                Assert.Equal("api", EnvCommand.LastSettings!.Service);
                Assert.Equal("staging", EnvCommand.LastSettings.Environment);
                Assert.Equal(4, EnvCommand.LastSettings.Replicas);
                Assert.True(EnvCommand.LastSettings.DryRun);
                Assert.Equal(BuildMode.ReleaseCandidate, EnvCommand.LastSettings.Mode);
                VisualAssert.ContainsText(scope.Driver, "Env deploy api staging x4 dry:True mode:ReleaseCandidate");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Invalid environment values render parameter errors with the environment variable name.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedSettings_InvalidEnvironmentVariableReturnsCleanError()
        {
            using (var env = new EnvironmentVariableScope(new Dictionary<string, string?>
            {
                [EnvEnvironmentVariable] = "staging",
                [EnvReplicasVariable] = "many"
            }))
            using (var scope = new ConsoleDriverScope(120))
            {
                EnvCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<EnvCommand>("deploy"));

                int exitCode = await app.RunAsync(new[] { "deploy", "api" });

                Assert.Equal(1, exitCode);
                Assert.Null(EnvCommand.LastSettings);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Could not bind value 'many' to environment variable '" + EnvReplicasVariable + "'. Expected <COUNT>.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Explicit CLI values override env-seeded repeated options instead of appending stale ambient defaults.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedSettings_CommandLineValuesOverrideEnvironmentCollections()
        {
            using (var env = new EnvironmentVariableScope(new Dictionary<string, string?>
            {
                [EnvNameVariable] = "env-name",
                [EnvCodeVariable] = "1",
                [EnvMetadataVariable] = "region=env",
                [EnvTagVariable] = "tier=env"
            }))
            using (var scope = new ConsoleDriverScope(120))
            {
                EnvCollectionCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<EnvCollectionCommand>("collect"));

                int exitCode = await app.RunAsync(new[]
                {
                    "collect",
                    "--name",
                    "cli-name",
                    "--code",
                    "2",
                    "--env",
                    "region=cli",
                    "--tag",
                    "tier=cli",
                    "--tag",
                    "role=worker"
                });

                Assert.Equal(0, exitCode);
                Assert.NotNull(EnvCollectionCommand.LastSettings);
                Assert.Equal(new[] { "cli-name" }, EnvCollectionCommand.LastSettings!.Names);
                Assert.Equal(new[] { 2 }, EnvCollectionCommand.LastSettings.Codes);
                Assert.Equal("cli", EnvCollectionCommand.LastSettings.Environment["region"]);
                Assert.Equal(new[] { "cli" }, EnvCollectionCommand.LastSettings.Tags["tier"]);
                Assert.Equal(new[] { "worker" }, EnvCollectionCommand.LastSettings.Tags["role"]);
                VisualAssert.ContainsText(scope.Driver, "Env collections names:cli-name codes:2 env:region=cli tags:tier=cli,role=worker");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Enum options accept CLI-friendly kebab-case spellings for multi-word enum members.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedSettings_EnumOptionsAcceptKebabCaseValues()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                CopyCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<CopyCommand>("copy"));

                int exitCode = await app.RunAsync(new[]
                {
                    "copy",
                    "source.txt",
                    "--mode",
                    "release-candidate"
                });

                Assert.Equal(0, exitCode);
                Assert.NotNull(CopyCommand.LastSettings);
                Assert.Equal(BuildMode.ReleaseCandidate, CopyCommand.LastSettings!.Mode);
                VisualAssert.ContainsText(scope.Driver, "Copied source.txt -> .");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Enum options accept common metadata labels so POC settings can keep domain-friendly names.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedSettings_EnumOptionsAcceptMetadataLabels()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                CopyCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<CopyCommand>("copy"));

                int descriptionExitCode = await app.RunAsync(new[]
                {
                    "copy",
                    "nightly.txt",
                    "--mode",
                    "nightly-build"
                });

                Assert.Equal(0, descriptionExitCode);
                Assert.NotNull(CopyCommand.LastSettings);
                Assert.Equal(BuildMode.Nightly, CopyCommand.LastSettings!.Mode);

                CopyCommand.LastSettings = null;
                int displayExitCode = await app.RunAsync(new[]
                {
                    "copy",
                    "train.txt",
                    "--mode",
                    "release-train"
                });

                Assert.Equal(0, displayExitCode);
                Assert.NotNull(CopyCommand.LastSettings);
                Assert.Equal(BuildMode.ReleaseTrain, CopyCommand.LastSettings!.Mode);
                VisualAssert.ContainsText(scope.Driver, "Copied nightly.txt -> .");
                VisualAssert.ContainsText(scope.Driver, "Copied train.txt -> .");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Boolean options with long aliases accept generated --no-* negated aliases for default-enabled POC settings.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedSettings_BooleanOptionsAcceptNegatedLongAliases()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                BoolNegationCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<BoolNegationCommand>("build"));

                int exitCode = await app.RunAsync(new[]
                {
                    "build",
                    "--no-cache",
                    "--no-color",
                    "--verbose"
                });

                Assert.Equal(0, exitCode);
                Assert.NotNull(BoolNegationCommand.LastSettings);
                Assert.False(BoolNegationCommand.LastSettings!.Cache);
                Assert.False(BoolNegationCommand.LastSettings.Color);
                Assert.True(BoolNegationCommand.LastSettings.Verbose);
                VisualAssert.ContainsText(scope.Driver, "cache:False color:False verbose:True");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Explicit --no-* aliases remain exact options and are not swallowed by generated boolean negation.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedSettings_ExplicitNoAliasTakesPrecedenceOverGeneratedNegation()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                ExplicitNoAliasCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<ExplicitNoAliasCommand>("build"));

                int exitCode = await app.RunAsync(new[] { "build", "--no-cache" });

                Assert.Equal(0, exitCode);
                Assert.NotNull(ExplicitNoAliasCommand.LastSettings);
                Assert.True(ExplicitNoAliasCommand.LastSettings!.Cache);
                Assert.True(ExplicitNoAliasCommand.LastSettings.NoCacheAlias);
                VisualAssert.ContainsText(scope.Driver, "cache:True no-cache:True");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Settings command help includes positional arguments, options, defaults, descriptions, and required notes.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedSettingsHelp_RendersArgumentsOptionsAndDefaults()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("files");
                    config.AddCommand<CopyCommand>("copy")
                        .WithDescription("Copy files")
                        .WithAlias("cp");
                });

                int exitCode = await app.RunAsync(new[] { "copy", "--help" });

                Assert.Equal(0, exitCode);
                VisualAssert.ContainsText(scope.Driver, "files copy <source> [destination] [OPTIONS]");
                VisualAssert.ContainsText(scope.Driver, "ARGUMENTS:");
                VisualAssert.ContainsText(scope.Driver, "<source>");
                VisualAssert.ContainsText(scope.Driver, "Source path");
                VisualAssert.ContainsText(scope.Driver, "-b, --buffer-size <INT>");
                VisualAssert.ContainsText(scope.Driver, "Buffer size in KB");
                VisualAssert.ContainsText(scope.Driver, "default: 64");
                VisualAssert.ContainsText(scope.Driver, "--mode <VALUE>");
                VisualAssert.ContainsText(scope.Driver, "--tag <TAG>");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// HideOptionDefaultValues suppresses option defaults without changing argument help.
        /// </summary>
        [Fact]
        public async Task CommandApp_HideOptionDefaultValues_SuppressesOptionDefaultsInHelp()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("files");
                    config.HideOptionDefaultValues();
                    config.AddCommand<CopyCommand>("copy")
                        .WithDescription("Copy files");
                });

                int exitCode = await app.RunAsync(new[] { "copy", "--help" });

                Assert.Equal(0, exitCode);
                VisualAssert.ContainsText(scope.Driver, "Destination path [default: .]");
                VisualAssert.ContainsText(scope.Driver, "Buffer size in KB");
                Assert.DoesNotContain("default: 64", scope.Driver.Text);
                Assert.DoesNotContain("default: Debug", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// TrimTrailingPeriods normalizes generated help descriptions without changing parser metadata.
        /// </summary>
        [Fact]
        public async Task CommandApp_TrimTrailingPeriods_NormalizesHelpDescriptions()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("periods");
                    config.SetApplicationDescription("Utility app.");
                    config.TrimTrailingPeriods();
                    config.AddCommand<PeriodHelpCommand>("check")
                        .WithDescription("Run checks.");
                });

                int rootHelpExitCode = await app.RunAsync(new[] { "--help" });
                int commandHelpExitCode = await app.RunAsync(new[] { "check", "--help" });

                Assert.Equal(0, rootHelpExitCode);
                Assert.Equal(0, commandHelpExitCode);
                VisualAssert.ContainsText(scope.Driver, "Utility app");
                VisualAssert.ContainsText(scope.Driver, "Run checks");
                VisualAssert.ContainsText(scope.Driver, "Input path");
                VisualAssert.ContainsText(scope.Driver, "Output path");
                Assert.DoesNotContain("Utility app.", scope.Driver.Text);
                Assert.DoesNotContain("Run checks.", scope.Driver.Text);
                Assert.DoesNotContain("Input path.", scope.Driver.Text);
                Assert.DoesNotContain("Output path.", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Missing required positional arguments return a clean error instead of invoking the command.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedSettings_MissingRequiredArgumentReturnsError()
        {
            using (var scope = new ConsoleDriverScope(80))
            {
                CopyCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<CopyCommand>("copy"));

                int exitCode = await app.RunAsync(new[] { "copy" });

                Assert.Equal(1, exitCode);
                Assert.Null(CopyCommand.LastSettings);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Missing required argument 'source'.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Missing multiple required positional arguments are reported together before command execution.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedSettings_MultipleMissingRequiredArgumentsReturnSingleError()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                RemoteAddCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<RemoteAddCommand>("add"));

                int exitCode = await app.RunAsync(new[] { "add" });

                Assert.Equal(1, exitCode);
                Assert.Null(RemoteAddCommand.LastSettings);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Missing required arguments 'name', 'url'.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Missing option values include the expected value name from the option template.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedSettings_MissingOptionValueShowsExpectedValueName()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                CopyCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<CopyCommand>("copy"));

                int exitCode = await app.RunAsync(new[] { "copy", "source.txt", "--buffer-size" });

                Assert.Equal(1, exitCode);
                Assert.Null(CopyCommand.LastSettings);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Missing value for option '--buffer-size'. Expected <INT>.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Invalid option values report the command-line option and expected value name instead of only the property name.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedSettings_InvalidOptionValueShowsParameterAndExpectedValueName()
        {
            using (var scope = new ConsoleDriverScope(180))
            {
                CopyCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<CopyCommand>("copy"));

                int exitCode = await app.RunAsync(new[] { "copy", "source.txt", "--buffer-size", "large" });

                Assert.Equal(1, exitCode);
                Assert.Null(CopyCommand.LastSettings);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Could not bind value 'large' to option '--buffer-size'. Expected <INT>.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Invalid enum option values show accepted CLI spellings instead of raw enum parser text.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedSettings_InvalidEnumOptionValueShowsAllowedValues()
        {
            using (var scope = new ConsoleDriverScope(240))
            {
                CopyCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<CopyCommand>("copy"));

                int exitCode = await app.RunAsync(new[] { "copy", "source.txt", "--mode", "ship-it" });

                Assert.Equal(1, exitCode);
                Assert.Null(CopyCommand.LastSettings);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Could not bind value 'ship-it' to option '--mode'. Expected <VALUE>. Allowed values:");
                VisualAssert.ContainsText(scope.Driver, "release-candidate");
                VisualAssert.ContainsText(scope.Driver, "nightly-build");
                VisualAssert.ContainsText(scope.Driver, "release-train");
                Assert.DoesNotContain("Requested value", scope.Driver.Text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Required options and validation failures return a clean error before command execution.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedSettings_RequiredOptionsAndValidationFailBeforeExecute()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                PublishCommand.Executed = false;
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<PublishCommand>("publish"));

                int missingExit = await app.RunAsync(new[] { "publish", "api" });
                int validationExit = await app.RunAsync(new[] { "publish", "api", "--environment", "prod", "--replicas", "0" });

                Assert.Equal(1, missingExit);
                Assert.Equal(1, validationExit);
                Assert.False(PublishCommand.Executed);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Missing required option '--environment'.");
                VisualAssert.ContainsText(scope.Driver, "ERROR: Replicas must be at least 1.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Missing multiple required options are reported together so POC command authors get one actionable error.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedSettings_MultipleMissingRequiredOptionsReturnSingleError()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                AliasValidationCommand.Executed = false;
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<AliasValidationCommand>("alias"));

                int exitCode = await app.RunAsync(new[] { "alias" });

                Assert.Equal(1, exitCode);
                Assert.False(AliasValidationCommand.Executed);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Missing required options '--name', '--count'.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Parameter validation attributes fail fast before command execution and can reuse validation logic across settings.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedSettings_ParameterValidationAttributeFailsBeforeExecute()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                ListenCommand.Executed = false;
                ListenCommand.LastPort = 0;
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<ListenCommand>("listen"));

                int invalidExit = await app.RunAsync(new[] { "listen", "--port", "0" });

                Assert.Equal(1, invalidExit);
                Assert.False(ListenCommand.Executed);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Port must be between 1 and 65535.");

                int validExit = await app.RunAsync(new[] { "listen", "--port", "443" });

                Assert.Equal(0, validExit);
                Assert.True(ListenCommand.Executed);
                Assert.Equal(443, ListenCommand.LastPort);
                VisualAssert.ContainsText(scope.Driver, "Listening on 443");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// DataAnnotations validation attributes on command settings fail before command execution.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedSettings_DataAnnotationsFailBeforeExecute()
        {
            using (var scope = new ConsoleDriverScope(120))
            {
                DataAnnotationsDeployCommand.Executed = false;
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<DataAnnotationsDeployCommand>("deploy"));

                int missingEnvironmentExit = await app.RunAsync(new[] { "deploy", "api", "--replicas", "2" });
                int rangeExit = await app.RunAsync(new[] { "deploy", "api", "--environment", "prod", "--replicas", "12" });
                int validExit = await app.RunAsync(new[] { "deploy", "api", "--environment", "prod", "--replicas", "3" });

                Assert.Equal(1, missingEnvironmentExit);
                Assert.Equal(1, rangeExit);
                Assert.Equal(0, validExit);
                Assert.True(DataAnnotationsDeployCommand.Executed);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Environment is required.");
                VisualAssert.ContainsText(scope.Driver, "ERROR: Replicas must be between 1 and 10.");
                VisualAssert.ContainsText(scope.Driver, "Annotated deploy api to prod x3");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Example validation includes DataAnnotations failures without executing the command.
        /// </summary>
        [Fact]
        public async Task CommandApp_ValidateExamples_DataAnnotationsFailuresReturnCleanErrors()
        {
            using (var scope = new ConsoleDriverScope(180))
            {
                DataAnnotationsDeployCommand.Executed = false;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.ValidateExamples();
                    config.AddCommand<DataAnnotationsDeployCommand>("deploy")
                        .WithExample("deploy", "api", "--environment", "prod", "--replicas", "12");
                });

                int exitCode = await app.RunAsync(new[] { "deploy", "api", "--environment", "prod", "--replicas", "3" });

                Assert.Equal(1, exitCode);
                Assert.False(DataAnnotationsDeployCommand.Executed);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Example for command 'deploy' is invalid");
                VisualAssert.ContainsText(scope.Driver, "Replicas must be between 1 and 10.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// IValidatableObject settings can express model-level command validation rules.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedSettings_IValidatableObjectFailsBeforeExecute()
        {
            using (var scope = new ConsoleDriverScope(120))
            {
                ValidatableDeployCommand.Executed = false;
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<ValidatableDeployCommand>("deploy"));

                int invalidExit = await app.RunAsync(new[] { "deploy", "api", "--environment", "api" });
                int validExit = await app.RunAsync(new[] { "deploy", "api", "--environment", "prod" });

                Assert.Equal(1, invalidExit);
                Assert.Equal(0, validExit);
                Assert.True(ValidatableDeployCommand.Executed);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Environment must differ from service.");
                VisualAssert.ContainsText(scope.Driver, "Validatable deploy api to prod");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Parameter validation contexts expose Spectre-style parameter metadata.
        /// </summary>
        [Fact]
        public async Task CommandApp_ParameterValidationContext_ExposesParameterInfo()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                MetadataCaptureAttribute.Snapshots.Clear();
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<ParameterInfoCommand>("inspect"));

                int exitCode = await app.RunAsync(new[] { "inspect", "input.txt", "--count", "5" });

                Assert.Equal(0, exitCode);
                Assert.Equal(2, MetadataCaptureAttribute.Snapshots.Count);

                ParameterSnapshot argument = MetadataCaptureAttribute.Snapshots.Single(snapshot => !snapshot.IsOption);
                Assert.Equal("path", argument.Name);
                Assert.Equal("<path>", argument.Template);
                Assert.Equal("Input path", argument.Description);
                Assert.True(argument.IsRequired);
                Assert.Equal(nameof(ParameterInfoSettings.Path), argument.PropertyName);
                Assert.Equal(typeof(string), argument.ValueType);
                Assert.Equal("input.txt", argument.Value);
                Assert.Equal(argument.Name, argument.LegacyParameterName);

                ParameterSnapshot option = MetadataCaptureAttribute.Snapshots.Single(snapshot => snapshot.IsOption);
                Assert.Equal("--count", option.Name);
                Assert.Equal("--count <COUNT>", option.Template);
                Assert.Equal("Item count", option.Description);
                Assert.True(option.IsRequired);
                Assert.Equal(nameof(ParameterInfoSettings.Count), option.PropertyName);
                Assert.Equal(typeof(int), option.ValueType);
                Assert.Equal(5, option.Value);
                Assert.Equal(option.Name, option.LegacyParameterName);
                VisualAssert.ContainsText(scope.Driver, "Inspect input.txt x5");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style ValidationResult aliases work for settings and parameter validators.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedSettings_AcceptsValidationResultAlias()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                AliasValidationCommand.Executed = false;
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<AliasValidationCommand>("alias"));

                int settingsExit = await app.RunAsync(new[] { "alias", "--name", "bad", "--count", "1" });
                int parameterExit = await app.RunAsync(new[] { "alias", "--name", "ok", "--count", "0" });
                int successExit = await app.RunAsync(new[] { "alias", "--name", "ok", "--count", "2" });

                Assert.Equal(1, settingsExit);
                Assert.Equal(1, parameterExit);
                Assert.Equal(0, successExit);
                Assert.True(AliasValidationCommand.Executed);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Name cannot be bad.");
                VisualAssert.ContainsText(scope.Driver, "ERROR: Count must be at least 1.");
                VisualAssert.ContainsText(scope.Driver, "Alias ok x2");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Typed settings bind custom value objects through the standard TypeConverter pattern.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedSettings_UsesTypeConverterForCustomValues()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                ConnectCommand.LastEndpoint = null;
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<ConnectCommand>("connect"));

                int exitCode = await app.RunAsync(new[] { "connect", "--endpoint", "api.local:443" });

                Assert.Equal(0, exitCode);
                Assert.NotNull(ConnectCommand.LastEndpoint);
                Assert.Equal("api.local", ConnectCommand.LastEndpoint!.Host);
                Assert.Equal(443, ConnectCommand.LastEndpoint.Port);
                VisualAssert.ContainsText(scope.Driver, "Connected api.local:443");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Invalid custom values keep the converter's domain hint and identify the failing option.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedSettings_InvalidTypeConverterValueShowsOptionAndConverterMessage()
        {
            using (var scope = new ConsoleDriverScope(180))
            {
                ConnectCommand.LastEndpoint = null;
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<ConnectCommand>("connect"));

                int exitCode = await app.RunAsync(new[] { "connect", "--endpoint", "api.local" });

                Assert.Equal(1, exitCode);
                Assert.Null(ConnectCommand.LastEndpoint);
                VisualAssert.ContainsText(scope.Driver, "ERROR: Could not bind value 'api.local' to option '--endpoint'. Expected <ENDPOINT>.");
                VisualAssert.ContainsText(scope.Driver, "Endpoint values must use host:port.");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Typed settings can bind custom values through a TypeConverterAttribute on the settings property.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedSettings_UsesPropertyTypeConverterForCustomValues()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                ConnectWithPropertyConverterCommand.LastEndpoint = null;
                var app = new CommandApp();
                app.Configure(config => config.AddCommand<ConnectWithPropertyConverterCommand>("connect"));

                int exitCode = await app.RunAsync(new[] { "connect", "--endpoint", "api.local:8443" });

                Assert.Equal(22, exitCode);
                Assert.NotNull(ConnectWithPropertyConverterCommand.LastEndpoint);
                Assert.Equal("api.local", ConnectWithPropertyConverterCommand.LastEndpoint!.Host);
                Assert.Equal(8443, ConnectWithPropertyConverterCommand.LastEndpoint.Port);
                VisualAssert.ContainsText(scope.Driver, "Property converter api.local:8443");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Branches route to nested commands and include the full command path in the context.
        /// </summary>
        [Fact]
        public async Task CommandApp_Branch_RoutesNestedCommand()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                RemoteAddCommand.LastSettings = null;
                RemoteAddCommand.LastCommandName = null;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("gitish");
                    config.AddBranch("remote", remote =>
                    {
                        remote.SetDescription("Manage remotes");
                        remote.AddCommand<RemoteAddCommand>("add")
                            .WithDescription("Add a remote");
                    });
                });

                int exitCode = await app.RunAsync(new[] { "remote", "add", "origin", "https://example.test/repo.git" });

                Assert.Equal(0, exitCode);
                Assert.NotNull(RemoteAddCommand.LastSettings);
                Assert.Equal("origin", RemoteAddCommand.LastSettings!.Name);
                Assert.Equal("https://example.test/repo.git", RemoteAddCommand.LastSettings.Url);
                Assert.False(RemoteAddCommand.LastSettings.Verbose);
                Assert.Equal("remote add", RemoteAddCommand.LastCommandName);
                VisualAssert.ContainsText(scope.Driver, "Added origin");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Branch settings options can appear before the subcommand and bind to inherited command settings.
        /// </summary>
        [Fact]
        public async Task CommandApp_Branch_SharedOptionsCanAppearBeforeSubcommand()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                RemoteAddCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.AddBranch<RemoteSettings>("remote", remote =>
                    {
                        remote.AddCommand<RemoteAddCommand>("add");
                    });
                });

                int exitCode = await app.RunAsync(new[] { "remote", "--verbose", "add", "origin", "https://example.test/repo.git" });

                Assert.Equal(0, exitCode);
                Assert.NotNull(RemoteAddCommand.LastSettings);
                Assert.True(RemoteAddCommand.LastSettings!.Verbose);
                VisualAssert.ContainsText(scope.Driver, "Added origin");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Branch typed delegate commands bind inherited settings and receive cancellation tokens.
        /// </summary>
        [Fact]
        public async Task CommandApp_Branch_TypedDelegateCommand_BindsInheritedSettings()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                RemoteAddSettings? received = null;
                CancellationToken observedToken = default;
                var cts = new CancellationTokenSource();
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.AddBranch<RemoteSettings>("remote", remote =>
                    {
                        remote.AddCommand<RemoteAddSettings>("add", async (context, settings, cancellationToken) =>
                        {
                            await Task.Delay(1, cancellationToken);
                            received = settings;
                            observedToken = cancellationToken;
                            context.Console.WriteLine("Delegate added " + settings.Name);
                            return 8;
                        });
                    });
                });

                int exitCode = await app.RunAsync(
                    new[] { "remote", "--verbose", "add", "origin", "https://example.test/repo.git" },
                    cts.Token);

                Assert.Equal(8, exitCode);
                Assert.NotNull(received);
                Assert.Equal("origin", received!.Name);
                Assert.Equal("https://example.test/repo.git", received.Url);
                Assert.True(received.Verbose);
                Assert.Equal(cts.Token, observedToken);
                VisualAssert.ContainsText(scope.Driver, "Delegate added origin");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Spectre-style delegate aliases work through branch configurator interfaces.
        /// </summary>
        [Fact]
        public async Task CommandApp_BranchAddDelegateAliases_RunThroughBranchInterface()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                CancellationToken statusToken = default;
                RemoteAddSettings? received = null;
                CancellationToken addToken = default;
                var cts = new CancellationTokenSource();
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.AddBranch<RemoteSettings>("remote", remote =>
                    {
                        IBranchConfigurator branch = remote;
                        branch.AddDelegate("status", (context, cancellationToken) =>
                        {
                            statusToken = cancellationToken;
                            context.Console.WriteLine("Branch delegate alias status");
                            return 13;
                        });

                        branch.AddAsyncDelegate<RemoteAddSettings>("add", async (context, settings, cancellationToken) =>
                        {
                            await Task.Delay(1, cancellationToken);
                            received = settings;
                            addToken = cancellationToken;
                            context.Console.WriteLine("Branch async delegate alias added " + settings.Name);
                            return 14;
                        });
                    });
                });

                int statusExitCode = await app.RunAsync(new[] { "remote", "status" }, cts.Token);
                int addExitCode = await app.RunAsync(
                    new[] { "remote", "--verbose", "add", "origin", "https://example.test/repo.git" },
                    cts.Token);

                Assert.Equal(13, statusExitCode);
                Assert.Equal(14, addExitCode);
                Assert.Equal(cts.Token, statusToken);
                Assert.Equal(cts.Token, addToken);
                Assert.NotNull(received);
                Assert.Equal("origin", received!.Name);
                Assert.Equal("https://example.test/repo.git", received.Url);
                Assert.True(received.Verbose);
                VisualAssert.ContainsText(scope.Driver, "Branch delegate alias status");
                VisualAssert.ContainsText(scope.Driver, "Branch async delegate alias added origin");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Typed branch configurator callbacks can be used when porting Spectre-style branch helpers.
        /// </summary>
        [Fact]
        public async Task CommandApp_TypedBranchConfiguratorCallback_ConfiguresSharedOptionsAndNestedBranches()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                RemoteAddCommand.LastSettings = null;
                RemoteListCommand.Ran = false;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("gitish");
                    IConfigurator configurator = config;
                    Action<IConfigurator<RemoteSettings>> configureRemote = ConfigureTypedRemoteBranch;
                    configurator.AddBranch<RemoteSettings>("remote", configureRemote);
                });

                int addExitCode = await app.RunAsync(new[] { "remote", "--verbose", "add", "origin", "https://example.test/repo.git" });
                int nestedExitCode = await app.RunAsync(new[] { "remote", "audit", "list" });
                int helpExitCode = await app.RunAsync(new[] { "remote", "--help" });

                Assert.Equal(0, addExitCode);
                Assert.Equal(0, nestedExitCode);
                Assert.Equal(0, helpExitCode);
                Assert.NotNull(RemoteAddCommand.LastSettings);
                Assert.True(RemoteAddCommand.LastSettings!.Verbose);
                Assert.True(RemoteListCommand.Ran);
                VisualAssert.ContainsText(scope.Driver, "Added origin");
                VisualAssert.ContainsText(scope.Driver, "Listed remotes");
                VisualAssert.ContainsText(scope.Driver, "Manage typed remotes");
                VisualAssert.ContainsText(scope.Driver, "audit");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// ICommandLimiter supports Spectre-style generic helpers that constrain command settings compatibility.
        /// </summary>
        [Fact]
        public async Task CommandApp_CommandLimiter_AllowsSpectreStyleGenericRegistrationHelpers()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                RemoteAddCommand.LastSettings = null;
                BasePublishCommand.LastSettings = null;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.AddBranch<RemoteSettings>("remote", remote =>
                    {
                        AddLimitedCommand<RemoteSettings, RemoteAddCommand>(remote, "add");
                    });

                    AddLimitedCommand<BasePublishSettings, BasePublishCommand>(config, "publish");
                });

                int branchExitCode = await app.RunAsync(new[] { "remote", "--verbose", "add", "origin", "https://example.test/repo.git" });
                int commandExitCode = await app.RunAsync(new[] { "publish", "artifact.zip", "--environment", "prod" });

                Assert.Equal(0, branchExitCode);
                Assert.Equal(9, commandExitCode);
                Assert.NotNull(RemoteAddCommand.LastSettings);
                Assert.True(RemoteAddCommand.LastSettings!.Verbose);
                Assert.NotNull(BasePublishCommand.LastSettings);
                Assert.Equal("artifact.zip", BasePublishCommand.LastSettings!.Artifact);
                Assert.Equal("prod", BasePublishCommand.LastSettings.Environment);
                VisualAssert.ContainsText(scope.Driver, "Added origin");
                VisualAssert.ContainsText(scope.Driver, "Async publish artifact.zip to prod");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Unsafe configurators support runtime command and branch types for plugin-style POC command discovery.
        /// </summary>
        [Fact]
        public async Task CommandApp_UnsafeConfiguratorExtensions_RegisterRuntimeTypes()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                RemoteAddCommand.LastSettings = null;
                RemoteListCommand.Ran = false;
                BaseStatusCommand.ObservedToken = default;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("ops");
                    IUnsafeConfigurator unsafeConfig = config.AsUnsafe();
                    unsafeConfig.SetDefaultCommand(typeof(BaseStatusCommand))
                        .WithDescription("Run default status");
                    unsafeConfig.AddCommand("list", typeof(RemoteListCommand))
                        .WithDescription("List remotes");

                    config.AddBranch("remote", typeof(RemoteSettings), remote =>
                    {
                        remote.AddCommand("add", typeof(RemoteAddCommand))
                            .WithDescription("Add a remote");
                    })
                    .WithDescription("Manage remotes");
                });

                int defaultExitCode = await app.RunAsync(Array.Empty<string>());
                int listExitCode = await app.RunAsync(new[] { "list" });
                int branchExitCode = await app.RunAsync(new[] { "remote", "--verbose", "add", "origin", "https://example.test/repo.git" });
                int helpExitCode = await app.RunAsync(new[] { "remote", "--help" });

                Assert.Equal(7, defaultExitCode);
                Assert.Equal(0, listExitCode);
                Assert.Equal(0, branchExitCode);
                Assert.Equal(0, helpExitCode);
                Assert.True(RemoteListCommand.Ran);
                Assert.NotNull(RemoteAddCommand.LastSettings);
                Assert.True(RemoteAddCommand.LastSettings!.Verbose);
                Assert.Equal("origin", RemoteAddCommand.LastSettings.Name);
                VisualAssert.ContainsText(scope.Driver, "Base status");
                VisualAssert.ContainsText(scope.Driver, "Listed remotes");
                VisualAssert.ContainsText(scope.Driver, "Added origin");
                VisualAssert.ContainsText(scope.Driver, "Manage remotes");
                VisualAssert.ContainsText(scope.Driver, "-v, --verbose");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Branch aliases and command aliases route through nested command lookup.
        /// </summary>
        [Fact]
        public async Task CommandApp_Branch_AliasesRouteNestedCommand()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                RemoteListCommand.Ran = false;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.AddBranch("remote", remote =>
                    {
                        remote.WithAliases("r", "repo");
                        remote.AddCommand<RemoteListCommand>("list")
                            .WithAliases("ls", "show");
                    });
                });

                int firstAliasExitCode = await app.RunAsync(new[] { "r", "ls" });
                int secondAliasExitCode = await app.RunAsync(new[] { "repo", "show" });

                Assert.Equal(0, firstAliasExitCode);
                Assert.Equal(0, secondAliasExitCode);
                Assert.True(RemoteListCommand.Ran);
                VisualAssert.ContainsText(scope.Driver, "Listed remotes");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Branch help lists nested branches, commands, aliases, and shared options.
        /// </summary>
        [Fact]
        public async Task CommandApp_BranchHelp_RendersSubcommandsAndSharedOptions()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("gitish");
                    config.AddBranch<RemoteSettings>("remote", remote =>
                    {
                        remote.SetDescription("Manage remotes");
                        remote.WithAlias("r");
                        remote.AddCommand<RemoteAddCommand>("add")
                            .WithDescription("Add a remote");
                        remote.AddCommand<RemoteListCommand>("list")
                            .WithDescription("List remotes")
                            .WithAlias("ls");
                    });
                });

                int exitCode = await app.RunAsync(new[] { "remote", "--help" });

                Assert.Equal(0, exitCode);
                VisualAssert.ContainsText(scope.Driver, "gitish remote [COMMAND] [ARGUMENTS]");
                VisualAssert.ContainsText(scope.Driver, "Manage remotes");
                VisualAssert.ContainsText(scope.Driver, "ALIASES:");
                VisualAssert.ContainsText(scope.Driver, "r");
                VisualAssert.ContainsText(scope.Driver, "-v, --verbose");
                VisualAssert.ContainsText(scope.Driver, "Show detailed output");
                VisualAssert.ContainsText(scope.Driver, "add");
                VisualAssert.ContainsText(scope.Driver, "list (ls)");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Command help under a branch includes inherited branch options even for untyped commands.
        /// </summary>
        [Fact]
        public async Task CommandApp_BranchCommandHelp_RendersInheritedOptions()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("gitish");
                    config.AddBranch<RemoteSettings>("remote", remote =>
                    {
                        remote.AddCommand("status", context => 0)
                            .WithDescription("Show remote status");
                    });
                });

                int exitCode = await app.RunAsync(new[] { "remote", "status", "--help" });

                Assert.Equal(0, exitCode);
                VisualAssert.ContainsText(scope.Driver, "gitish remote status [OPTIONS]");
                VisualAssert.ContainsText(scope.Driver, "Show remote status");
                VisualAssert.ContainsText(scope.Driver, "INHERITED OPTIONS:");
                VisualAssert.ContainsText(scope.Driver, "-v, --verbose");
                VisualAssert.ContainsText(scope.Driver, "Show detailed output");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Command help reached by path lookup does not duplicate options already inherited by typed settings.
        /// </summary>
        [Fact]
        public async Task CommandApp_BranchCommandHelp_DeduplicatesTypedInheritedOptions()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("gitish");
                    config.AddBranch<RemoteSettings>("remote", remote =>
                    {
                        remote.AddCommand<RemoteAddCommand>("add")
                            .WithDescription("Add a remote");
                    });
                });

                int exitCode = await app.RunAsync(new[] { "help", "remote", "add" });
                string helpText = scope.Driver.Text;

                Assert.Equal(0, exitCode);
                VisualAssert.ContainsText(scope.Driver, "gitish remote add <name> <url> [OPTIONS]");
                VisualAssert.ContainsText(scope.Driver, "Add a remote");
                VisualAssert.ContainsText(scope.Driver, "OPTIONS:");
                VisualAssert.ContainsText(scope.Driver, "-v, --verbose");
                Assert.DoesNotContain("INHERITED OPTIONS:", helpText);
                Assert.Equal(1, helpText.Split(new[] { "-v, --verbose" }, StringSplitOptions.None).Length - 1);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Built-in HelpProvider renders root, branch, and command help from the command model with configurable styles.
        /// </summary>
        [Fact]
        public async Task CommandApp_BuiltInHelpProvider_RendersStyledModelHelp()
        {
            using (var scope = new ConsoleDriverScope(120))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("ops");
                    config.SetHelpProvider(new HelpProvider(new HelpProviderStyle
                    {
                        Command = new CommandStyle { Name = "yellow" },
                        Argument = new ArgumentStyle { Template = "magenta" },
                        Option = new OptionStyle { Name = "cyan" },
                        Example = new ExampleStyle { Command = "dim" }
                    }));
                    config.AddExample("remote", "add", "origin", "https://example.test/repo.git");
                    config.AddBranch<RemoteSettings>("remote", remote =>
                    {
                        remote.SetDescription("Manage remotes");
                        remote.AddCommand<RemoteAddCommand>("add")
                            .WithDescription("Add a remote")
                            .WithExample("remote", "add", "origin", "https://example.test/repo.git");
                    });
                });

                int rootExitCode = await app.RunAsync(new[] { "--help" });
                int branchExitCode = await app.RunAsync(new[] { "remote", "--help" });
                int commandExitCode = await app.RunAsync(new[] { "remote", "add", "--help" });
                string text = scope.Driver.Text;

                Assert.Equal(0, rootExitCode);
                Assert.Equal(0, branchExitCode);
                Assert.Equal(0, commandExitCode);
                VisualAssert.ContainsText(scope.Driver, "ops [COMMAND] [ARGUMENTS]");
                VisualAssert.ContainsText(scope.Driver, "ops remote [COMMAND] [ARGUMENTS]");
                VisualAssert.ContainsText(scope.Driver, "ops remote add <name> <url> [OPTIONS]");
                VisualAssert.ContainsText(scope.Driver, "Manage remotes");
                VisualAssert.ContainsText(scope.Driver, "ARGUMENTS:");
                VisualAssert.ContainsText(scope.Driver, "<name>");
                VisualAssert.ContainsText(scope.Driver, "<url>");
                VisualAssert.ContainsText(scope.Driver, "OPTIONS:");
                VisualAssert.ContainsText(scope.Driver, "-v, --verbose");
                VisualAssert.ContainsText(scope.Driver, "EXAMPLES:");
                VisualAssert.ContainsText(scope.Driver, "ops remote add origin https://example.test/repo.git");
                Assert.DoesNotContain("[yellow]", text);
                Assert.DoesNotContain("[cyan]", text);
                Assert.Contains(scope.Driver.Writes, write => write.Color == ConsoleColor.Yellow && write.Text.Contains("remote"));
                Assert.Contains(scope.Driver.Writes, write => write.Color == ConsoleColor.Cyan && write.Text.Contains("-v, --verbose"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Built-in HelpProvider renders default-command metadata in root help.
        /// </summary>
        [Fact]
        public async Task CommandApp_BuiltInHelpProvider_RendersDefaultCommandHelp()
        {
            using (var scope = new ConsoleDriverScope(120))
            {
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.SetApplicationName("files");
                    config.SetHelpProvider(new HelpProvider(new HelpProviderStyle
                    {
                        Command = new CommandStyle { Name = "yellow" },
                        Argument = new ArgumentStyle { Template = "magenta" },
                        Option = new OptionStyle { Name = "cyan" },
                        Example = new ExampleStyle { Command = "dim" }
                    }));
                    config.SetDefaultCommand<CopyCommand>()
                        .WithDescription("Copy by default")
                        .WithExample("source.txt", "dest.txt", "--force");
                    config.AddCommand("status", context => 0)
                        .WithDescription("Show file status");
                });

                int exitCode = await app.RunAsync(new[] { "--help" });
                string text = scope.Driver.Text;

                Assert.Equal(0, exitCode);
                VisualAssert.ContainsText(scope.Driver, "files [COMMAND] [ARGUMENTS]");
                VisualAssert.ContainsText(scope.Driver, "files <source> [destination] [OPTIONS]");
                VisualAssert.ContainsText(scope.Driver, "DEFAULT COMMAND:");
                VisualAssert.ContainsText(scope.Driver, "Copy by default");
                VisualAssert.ContainsText(scope.Driver, "DEFAULT EXAMPLES:");
                VisualAssert.ContainsText(scope.Driver, "files source.txt dest.txt --force");
                VisualAssert.ContainsText(scope.Driver, "ARGUMENTS:");
                VisualAssert.ContainsText(scope.Driver, "<source>");
                VisualAssert.ContainsText(scope.Driver, "[destination]");
                VisualAssert.ContainsText(scope.Driver, "OPTIONS:");
                VisualAssert.ContainsText(scope.Driver, "-f, --force");
                VisualAssert.ContainsText(scope.Driver, "COMMANDS:");
                VisualAssert.ContainsText(scope.Driver, "status");
                Assert.DoesNotContain("No commands registered.", text);
                Assert.Contains(scope.Driver.Writes, write => write.Color == ConsoleColor.Magenta && write.Text.Contains("<source>"));
                Assert.Contains(scope.Driver.Writes, write => write.Color == ConsoleColor.Cyan && write.Text.Contains("-f, --force"));
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Custom help providers can render root, branch, and command help from context objects.
        /// </summary>
        [Fact]
        public async Task CommandApp_CustomHelpProvider_RendersApplicationBranchAndCommandHelp()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                var provider = new RecordingHelpProvider();
                var app = new CommandApp();
                app.Configure(config =>
                {
                    IConfigurator configurator = config;
                    configurator.SetApplicationName("ops");
                    configurator.SetApplicationVersion("2.0");
                    configurator.SetApplicationDescription("Operational MVC console.");
                    configurator.SetHelpProvider(provider);
                    configurator.AddExample("status");
                    config.SetDefaultCommand<CopyCommand>()
                        .WithDescription("Copy by default");
                    configurator.AddCommand("status", context => 0)
                        .WithDescription("Show service status");
                    configurator.AddBranch<RemoteSettings>("remote", remote =>
                    {
                        remote.SetDescription("Manage remotes");
                        remote.AddCommand<RemoteAddCommand>("add")
                            .WithDescription("Add a remote");
                    });
                });

                int appExitCode = await app.RunAsync(new[] { "--help" });
                int branchExitCode = await app.RunAsync(new[] { "remote", "--help" });
                int commandExitCode = await app.RunAsync(new[] { "remote", "add", "--help" });
                string text = scope.Driver.Text;

                Assert.Equal(0, appExitCode);
                Assert.Equal(0, branchExitCode);
                Assert.Equal(0, commandExitCode);
                Assert.NotNull(provider.ApplicationContext);
                Assert.NotNull(provider.BranchContext);
                Assert.NotNull(provider.CommandContext);
                Assert.Equal("ops", provider.ApplicationContext!.ApplicationName);
                Assert.Equal("2.0", provider.ApplicationContext.ApplicationVersion);
                Assert.Equal("Operational MVC console.", provider.ApplicationContext.ApplicationDescription);
                Assert.Equal("status", provider.ApplicationContext.Commands.Single().Name);
                Assert.Equal("remote", provider.ApplicationContext.Branches.Single().Name);
                Assert.Equal(new[] { "status" }, provider.ApplicationContext.Examples.Single());
                Assert.NotNull(provider.ApplicationContext.DefaultCommand);
                Assert.NotNull(provider.ApplicationContext.DefaultCommandInfo);
                Assert.Equal("__default", provider.ApplicationContext.DefaultCommand!.Name);
                Assert.Equal("Copy by default", provider.ApplicationContext.DefaultCommandInfo!.Description);
                Assert.Equal(new[] { "<source>", "[destination]" }, provider.ApplicationContext.DefaultCommandInfo.Arguments.Select(argument => argument.Template));
                Assert.Equal("ops", provider.ApplicationContext.Model.ApplicationName);
                Assert.Equal("status", provider.ApplicationContext.Model.Commands.Single().Name);
                Assert.Equal("remote", provider.ApplicationContext.Model.Branches.Single().Name);
                Assert.Equal("add", provider.ApplicationContext.Model.Branches.Single().Commands.Single().Name);
                Assert.Equal("remote", provider.BranchContext!.BranchPath);
                Assert.Equal(typeof(RemoteSettings), provider.BranchContext.Branch.SettingsType);
                Assert.Equal("remote", provider.BranchContext.BranchInfo.Name);
                Assert.Equal("-v", provider.BranchContext.Options.Single().Aliases.First());
                Assert.Equal("remote add", provider.CommandContext!.CommandPath);
                Assert.Equal("add", provider.CommandContext.Command.Name);
                Assert.Equal("add", provider.CommandContext.CommandInfo.Name);
                Assert.Equal(new[] { "<name>", "<url>" }, provider.CommandContext.Arguments.Select(argument => argument.Template));
                Assert.Contains(provider.CommandContext.Options, option => option.Aliases.Contains("--verbose"));
                Assert.Equal(new[] { typeof(RemoteSettings) }, provider.CommandContext.InheritedSettingsTypes);
                Assert.Contains("CUSTOM APP ops 2.0 commands:1 branches:1 examples:1 default:2", text);
                Assert.Contains("CUSTOM BRANCH remote commands:1 settings:RemoteSettings options:1", text);
                Assert.Contains("CUSTOM COMMAND remote add args:2 options:1 inherited:RemoteSettings", text);
                Assert.DoesNotContain("USAGE:", text);
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        /// <summary>
        /// Multiple branch levels route to deeply nested commands.
        /// </summary>
        [Fact]
        public async Task CommandApp_NestedBranch_RoutesDeepCommand()
        {
            using (var scope = new ConsoleDriverScope(100))
            {
                UploadCommand.Ran = false;
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.AddBranch("cloud", cloud =>
                    {
                        cloud.AddBranch("storage", storage =>
                        {
                            storage.AddCommand<UploadCommand>("upload");
                        });
                    });
                });

                int exitCode = await app.RunAsync(new[] { "cloud", "storage", "upload" });

                Assert.Equal(0, exitCode);
                Assert.True(UploadCommand.Ran);
                VisualAssert.ContainsText(scope.Driver, "Uploaded");
                VisualAssert.NoOverflow(scope.Driver);
            }
        }

        private sealed class EnvironmentVariableScope : IDisposable
        {
            private readonly Dictionary<string, string?> _previousValues = new Dictionary<string, string?>();

            public EnvironmentVariableScope(IDictionary<string, string?> values)
            {
                foreach (KeyValuePair<string, string?> value in values)
                {
                    _previousValues[value.Key] = Environment.GetEnvironmentVariable(value.Key);
                    Environment.SetEnvironmentVariable(value.Key, value.Value);
                }
            }

            public void Dispose()
            {
                foreach (KeyValuePair<string, string?> value in _previousValues)
                {
                    Environment.SetEnvironmentVariable(value.Key, value.Value);
                }
            }
        }

        private sealed class AsyncTokenCommand : IAsyncCommand
        {
            public static CancellationToken ObservedToken { get; set; }

            public Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
            {
                ObservedToken = cancellationToken;
                return Task.FromResult(5);
            }
        }

        private sealed class SyncTokenCommand : ICommand
        {
            public static CancellationToken ObservedToken { get; set; }

            public int Execute(CommandContext context)
            {
                ObservedToken = context.CancellationToken;
                return 6;
            }
        }

        private sealed class BaseStatusCommand : Command
        {
            public static CancellationToken ObservedToken { get; set; }

            protected override int Execute(CommandContext context, CancellationToken cancellationToken)
            {
                ObservedToken = cancellationToken;
                context.Console.MarkupLine("[green]Base status[/]");
                return 7;
            }
        }

        private sealed class BaseCopyCommand : Command<BaseCopySettings>
        {
            public static BaseCopySettings? LastSettings { get; set; }

            public static CancellationToken ObservedToken { get; set; }

            protected override int Execute(CommandContext context, BaseCopySettings settings, CancellationToken cancellationToken)
            {
                LastSettings = settings;
                ObservedToken = cancellationToken;
                context.Console.MarkupLineInterpolated($"Base copy [cyan]{settings.Source}[/] x{settings.Count}");
                return settings.Count;
            }
        }

        private sealed class BaseAsyncStatusCommand : AsyncCommand
        {
            public static CancellationToken ObservedToken { get; set; }

            protected override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                await Task.Delay(1, cancellationToken);
                ObservedToken = cancellationToken;
                context.Console.MarkupLine("[green]Async base status[/]");
                return 8;
            }
        }

        private sealed class BasePublishCommand : AsyncCommand<BasePublishSettings>
        {
            public static BasePublishSettings? LastSettings { get; set; }

            public static CancellationToken ObservedToken { get; set; }

            protected override async Task<int> ExecuteAsync(CommandContext context, BasePublishSettings settings, CancellationToken cancellationToken)
            {
                await Task.Delay(1, cancellationToken);
                LastSettings = settings;
                ObservedToken = cancellationToken;
                context.Console.MarkupLineInterpolated($"Async publish [cyan]{settings.Artifact}[/] to [green]{settings.Environment}[/]");
                return 9;
            }
        }

        private sealed class BaseCopySettings : CommandSettings
        {
            [CommandArgument(0, "<source>")]
            public string Source { get; set; } = string.Empty;

            [CommandOption("--count <COUNT>")]
            public int Count { get; set; }
        }

        private sealed class BasePublishSettings : CommandSettings
        {
            [CommandArgument(0, "<artifact>")]
            public string Artifact { get; set; } = string.Empty;

            [CommandOption("--environment <ENVIRONMENT>", true)]
            public string Environment { get; set; } = string.Empty;
        }

        private sealed class RecordingInterceptor : ICommandInterceptor
        {
            private readonly int? _resultOverride;

            public RecordingInterceptor(int? resultOverride = null)
            {
                _resultOverride = resultOverride;
            }

            public string[] Events { get; private set; } = Array.Empty<string>();

            public CommandSettings? Settings { get; private set; }

            public CancellationToken ObservedToken { get; private set; }

            public void Intercept(CommandContext context, CommandSettings settings)
            {
                Settings = settings;
                ObservedToken = context.CancellationToken;
                Events = Events.Concat(new[] { "before:" + context.CommandName }).ToArray();
            }

            public void InterceptResult(CommandContext context, CommandSettings settings, ref int result)
            {
                Events = Events.Concat(new[] { "after:" + context.CommandName + ":" + result.ToString(CultureInfo.InvariantCulture) }).ToArray();
                if (_resultOverride.HasValue)
                {
                    result = _resultOverride.Value;
                }
            }
        }

        private sealed class BeforeOnlyInterceptor : CommandInterceptor
        {
            public string CommandName { get; private set; } = string.Empty;

            public CommandSettings? Settings { get; private set; }

            public override void Intercept(CommandContext context, CommandSettings settings)
            {
                CommandName = context.CommandName;
                Settings = settings;
            }
        }

        private sealed class CopySettingsInterceptor : CommandInterceptor<CopySettings>
        {
            private readonly int? _resultOverride;

            public CopySettingsInterceptor(int? resultOverride = null)
            {
                _resultOverride = resultOverride;
            }

            public string[] Events { get; private set; } = Array.Empty<string>();

            public CopySettings? Settings { get; private set; }

            public override void Intercept(CommandContext context, CopySettings settings)
            {
                Settings = settings;
                Events = Events.Concat(new[] { "before:" + context.CommandName + ":" + settings.Source }).ToArray();
            }

            public override void InterceptResult(CommandContext context, CopySettings settings, ref int result)
            {
                Events = Events.Concat(new[] { "after:" + context.CommandName + ":" + result.ToString(CultureInfo.InvariantCulture) }).ToArray();
                if (_resultOverride.HasValue)
                {
                    result = _resultOverride.Value;
                }
            }
        }

        private sealed class ContextDataInterceptor : CommandInterceptor
        {
            public string[] Events { get; private set; } = Array.Empty<string>();

            public override void Intercept(CommandContext context, CommandSettings settings)
            {
                context.Data["trace"] = "intercepted:" + context.Name;
                Events = Events.Concat(new[] { "before:" + context.Name }).ToArray();
            }

            public override void InterceptResult(CommandContext context, CommandSettings settings, ref int result)
            {
                Events = Events.Concat(new[] { "after:" + context.Name + ":" + context.Data["handler"] + ":" + result.ToString(CultureInfo.InvariantCulture) }).ToArray();
            }
        }

        private sealed class RecordingExceptionHandler : ICommandExceptionHandler
        {
            private readonly int _exitCode;

            public RecordingExceptionHandler(int exitCode)
            {
                _exitCode = exitCode;
            }

            public CommandExceptionContext? Context { get; private set; }

            public int Handle(CommandExceptionContext context)
            {
                Context = context;
                context.Console.WriteLine("HANDLED: " + context.Exception.GetType().Name + ": " + context.Exception.Message);
                return _exitCode;
            }
        }

        private sealed class RecordingHelpProvider : IHelpProvider
        {
            public CommandAppHelpContext? ApplicationContext { get; private set; }

            public CommandBranchHelpContext? BranchContext { get; private set; }

            public CommandHelpContext? CommandContext { get; private set; }

            public void WriteApplicationHelp(IAnsiConsole console, CommandAppHelpContext context)
            {
                ApplicationContext = context;
                console.WriteLine(
                    "CUSTOM APP " +
                    context.ApplicationName +
                    " " +
                    context.ApplicationVersion +
                    " commands:" +
                    context.Commands.Count.ToString(CultureInfo.InvariantCulture) +
                    " branches:" +
                    context.Branches.Count.ToString(CultureInfo.InvariantCulture) +
                    " examples:" +
                    context.Examples.Count.ToString(CultureInfo.InvariantCulture) +
                    " default:" +
                    (context.DefaultCommandInfo == null
                        ? "none"
                        : context.DefaultCommandInfo.Arguments.Count.ToString(CultureInfo.InvariantCulture)));
            }

            public void WriteBranchHelp(IAnsiConsole console, CommandBranchHelpContext context)
            {
                BranchContext = context;
                string settingsName = context.Branch.SettingsType == null ? "none" : context.Branch.SettingsType.Name;
                console.WriteLine(
                    "CUSTOM BRANCH " +
                    context.BranchPath +
                    " commands:" +
                    context.BranchInfo.Commands.Count.ToString(CultureInfo.InvariantCulture) +
                    " settings:" +
                    settingsName +
                    " options:" +
                    context.Options.Count.ToString(CultureInfo.InvariantCulture));
            }

            public void WriteCommandHelp(IAnsiConsole console, CommandHelpContext context)
            {
                CommandContext = context;
                string inherited = context.InheritedSettingsTypes.Count == 0
                    ? "none"
                    : string.Join(",", context.InheritedSettingsTypes.Select(type => type.Name));
                console.WriteLine(
                    "CUSTOM COMMAND " +
                    context.CommandPath +
                    " args:" +
                    context.Arguments.Count.ToString(CultureInfo.InvariantCulture) +
                    " options:" +
                    context.Options.Count.ToString(CultureInfo.InvariantCulture) +
                    " inherited:" +
                    inherited);
            }
        }

        private sealed class TestCommandActivator : ICommandActivator
        {
            private readonly Func<Type, object> _factory;

            public TestCommandActivator(Func<Type, object> factory)
            {
                _factory = factory;
            }

            public Type[] CreatedTypes { get; private set; } = Array.Empty<Type>();

            public object Create(Type commandType)
            {
                CreatedTypes = CreatedTypes.Concat(new[] { commandType }).ToArray();
                return _factory(commandType);
            }
        }

        private sealed class TestTypeRegistrarFrontend : ITypeRegistrarFrontend
        {
            private readonly string _message;

            public TestTypeRegistrarFrontend(string message)
            {
                _message = message;
            }

            public int ConfigureCount { get; private set; }

            public void Configure(ITypeRegistrar registrar)
            {
                ConfigureCount++;
                registrar.RegisterInstance(typeof(MessageService), new MessageService(_message));
            }
        }

        private sealed class TestTypeRegistrar : ITypeRegistrar
        {
            private readonly Dictionary<Type, Func<object?>> _factories = new Dictionary<Type, Func<object?>>();

            public Type[] RegisteredServices { get; private set; } = Array.Empty<Type>();

            public int BuildCount { get; private set; }

            public ITypeResolver Build()
            {
                BuildCount++;
                return new TestTypeResolver(this);
            }

            public void Register(Type service, Type implementation)
            {
                if (service == null)
                {
                    throw new ArgumentNullException(nameof(service));
                }

                if (implementation == null)
                {
                    throw new ArgumentNullException(nameof(implementation));
                }

                RegisteredServices = RegisteredServices.Concat(new[] { service }).ToArray();
                _factories[service] = () => Create(implementation);
            }

            public void RegisterInstance(Type service, object implementation)
            {
                if (service == null)
                {
                    throw new ArgumentNullException(nameof(service));
                }

                RegisteredServices = RegisteredServices.Concat(new[] { service }).ToArray();
                _factories[service] = () => implementation;
            }

            public void RegisterLazy(Type service, Func<object> factory)
            {
                if (service == null)
                {
                    throw new ArgumentNullException(nameof(service));
                }

                if (factory == null)
                {
                    throw new ArgumentNullException(nameof(factory));
                }

                RegisteredServices = RegisteredServices.Concat(new[] { service }).ToArray();
                _factories[service] = factory;
            }

            internal object? Resolve(Type type)
            {
                if (type == null)
                {
                    return null;
                }

                if (_factories.TryGetValue(type, out Func<object?>? factory))
                {
                    return factory();
                }

                if (type.IsAbstract || type.IsInterface)
                {
                    return null;
                }

                return Create(type);
            }

            private object Create(Type implementation)
            {
                ConstructorInfo? constructor = implementation
                    .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .OrderByDescending(candidate => candidate.GetParameters().Length)
                    .FirstOrDefault();

                if (constructor == null)
                {
                    object? instance = Activator.CreateInstance(implementation);
                    if (instance == null)
                    {
                        throw new InvalidOperationException("Could not create " + implementation.FullName + ".");
                    }

                    return instance;
                }

                object?[] arguments = constructor
                    .GetParameters()
                    .Select(parameter => Resolve(parameter.ParameterType))
                    .ToArray();

                return constructor.Invoke(arguments);
            }
        }

        private sealed class TestTypeResolver : ITypeResolver
        {
            private readonly TestTypeRegistrar _registrar;

            public TestTypeResolver(TestTypeRegistrar registrar)
            {
                _registrar = registrar;
            }

            public object? Resolve(Type type)
            {
                return _registrar.Resolve(type);
            }
        }

        private sealed class MessageService
        {
            public MessageService(string message)
            {
                Message = message;
            }

            public string Message { get; }
        }

        private sealed class InjectedCommand : ICommand
        {
            private readonly MessageService _service;

            public InjectedCommand(MessageService service)
            {
                _service = service;
            }

            public int Execute(CommandContext context)
            {
                context.Console.WriteLine(_service.Message);
                return 0;
            }
        }

        private sealed class RegistrarInjectedCommand : ICommand
        {
            private readonly MessageService _service;
            private readonly IAnsiConsole _console;

            public RegistrarInjectedCommand(MessageService service, IAnsiConsole console)
            {
                _service = service;
                _console = console;
            }

            public int Execute(CommandContext context)
            {
                _console.WriteLine(_service.Message);
                return 12;
            }
        }

        private sealed class RegistrarDefaultCommand : ICommand
        {
            private readonly MessageService _service;

            public RegistrarDefaultCommand(MessageService service)
            {
                _service = service;
            }

            public int Execute(CommandContext context)
            {
                context.Console.WriteLine(_service.Message);
                return 13;
            }
        }

        private sealed class SettingsInjectedCommand : ICommand<SettingsInjectedSettings>
        {
            private readonly SettingsInjectedSettings _settings;

            public SettingsInjectedCommand(SettingsInjectedSettings settings)
            {
                _settings = settings;
            }

            public int Execute(CommandContext context, SettingsInjectedSettings settings)
            {
                Assert.Same(settings, _settings);
                context.Console.WriteLine("Settings " + _settings.Name);
                return 14;
            }
        }

        private sealed class SettingsInjectedSettings : CommandSettings
        {
            [CommandOption("--name <NAME>", true)]
            public string Name { get; set; } = string.Empty;
        }

        private sealed class InjectedView : IView
        {
            private readonly MessageService _service;

            public InjectedView(MessageService service)
            {
                _service = service;
            }

            public string Title { get; set; } = "Injected";

            public bool GoBackRequested { get; private set; }

            public Task RenderAsync(CancellationToken cancellationToken = default)
            {
                Consoul.Write(_service.Message);
                return Task.CompletedTask;
            }

            public void Render()
            {
                RenderAsync().GetAwaiter().GetResult();
            }

            public void GoBack()
            {
                GoBackRequested = true;
            }
        }

        private sealed class DefaultCommand : ICommand
        {
            public static bool Ran { get; set; }

            public int Execute(CommandContext context)
            {
                Ran = true;
                context.Console.WriteLine("Default command");
                return 3;
            }
        }

        private sealed class ConfiguredDefaultCommand : ICommand
        {
            public static bool Ran { get; set; }

            public int Execute(CommandContext context)
            {
                Ran = true;
                context.Console.WriteLine("Configured default command");
                return 10;
            }
        }

        private sealed class TestView : IView
        {
            public static bool Rendered { get; set; }

            public string Title { get; set; } = "Dashboard";

            public bool GoBackRequested { get; private set; }

            public Task RenderAsync(CancellationToken cancellationToken = default)
            {
                Rendered = true;
                Consoul.Write("Rendered dashboard");
                return Task.CompletedTask;
            }

            public void Render()
            {
                RenderAsync().GetAwaiter().GetResult();
            }

            public void GoBack()
            {
                GoBackRequested = true;
            }
        }

        [Description("Run default from attributes")]
        private sealed class AttributedDefaultCommand : ICommand
        {
            public int Execute(CommandContext context)
            {
                context.Console.WriteLine("Attributed default");
                return 0;
            }
        }

        [Description("Run annotated command")]
        private sealed class AttributedCommand : ICommand<AttributedCommandSettings>
        {
            public int Execute(CommandContext context, AttributedCommandSettings settings)
            {
                context.Console.WriteLine("Attributed command");
                return 0;
            }
        }

        [Description("Fallback command settings description")]
        private sealed class AttributedCommandSettings : CommandSettings
        {
        }

        [Description("Run delegate settings command")]
        private sealed class AttributedDelegateSettings : CommandSettings
        {
        }

        [Description("Manage attributed operations")]
        private sealed class AttributedBranchSettings : CommandSettings
        {
        }

        [Description("Open annotated dashboard")]
        private sealed class AttributedView : IView
        {
            public string Title { get; set; } = "Attributed";

            public bool GoBackRequested { get; private set; }

            public Task RenderAsync(CancellationToken cancellationToken = default)
            {
                Consoul.Write("Attributed dashboard");
                return Task.CompletedTask;
            }

            public void Render()
            {
                RenderAsync().GetAwaiter().GetResult();
            }

            public void GoBack()
            {
                GoBackRequested = true;
            }
        }

        private sealed class CopyCommand : ICommand<CopySettings>
        {
            public static CopySettings? LastSettings { get; set; }

            public int Execute(CommandContext context, CopySettings settings)
            {
                LastSettings = settings;
                context.Console.WriteLine("Copied " + settings.Source + " -> " + settings.Destination);
                return 0;
            }
        }

        private sealed class ClusteredFlagCommand : ICommand<ClusteredFlagSettings>
        {
            public static ClusteredFlagSettings? LastSettings { get; set; }

            public int Execute(CommandContext context, ClusteredFlagSettings settings)
            {
                LastSettings = settings;
                context.Console.WriteLine("Copied " + settings.Source + " -> " + settings.Destination);
                return 0;
            }
        }

        private sealed class ThrowingTypedCommand : ICommand<CopySettings>
        {
            public int Execute(CommandContext context, CopySettings settings)
            {
                throw new InvalidOperationException("Typed boom: " + settings.Source);
            }
        }

        private sealed class BoolNegationCommand : ICommand<BoolNegationSettings>
        {
            public static BoolNegationSettings? LastSettings { get; set; }

            public int Execute(CommandContext context, BoolNegationSettings settings)
            {
                LastSettings = settings;
                context.Console.WriteLine(
                    "cache:" +
                    settings.Cache +
                    " color:" +
                    settings.Color +
                    " verbose:" +
                    settings.Verbose);
                return 0;
            }
        }

        private sealed class ExplicitNoAliasCommand : ICommand<ExplicitNoAliasSettings>
        {
            public static ExplicitNoAliasSettings? LastSettings { get; set; }

            public int Execute(CommandContext context, ExplicitNoAliasSettings settings)
            {
                LastSettings = settings;
                context.Console.WriteLine(
                    "cache:" +
                    settings.Cache +
                    " no-cache:" +
                    settings.NoCacheAlias);
                return 0;
            }
        }

        private sealed class PeriodHelpCommand : ICommand<PeriodHelpSettings>
        {
            public int Execute(CommandContext context, PeriodHelpSettings settings)
            {
                context.Console.WriteLine(settings.Input + " -> " + settings.Output);
                return 0;
            }
        }

        private sealed class EmptySettingsCommand : ICommand<EmptyCommandSettings>
        {
            public static EmptyCommandSettings? LastSettings { get; set; }

            public int Execute(CommandContext context, EmptyCommandSettings settings)
            {
                LastSettings = settings;
                context.Console.WriteLine("Empty settings");
                return 17;
            }
        }

        private sealed class FlagValueCommand : ICommand<FlagValueSettings>
        {
            public static FlagValueSettings? LastSettings { get; set; }

            public int Execute(CommandContext context, FlagValueSettings settings)
            {
                LastSettings = settings;
                string port = settings.Port.IsSet
                    ? settings.Port.Value.ToString(CultureInfo.InvariantCulture)
                    : "not-set";
                string timeout = settings.Timeout.IsSet
                    ? (settings.Timeout.Value.HasValue ? settings.Timeout.Value.Value.ToString(CultureInfo.InvariantCulture) : "default")
                    : "disabled";
                context.Console.WriteLine("port:" + port + " timeout:" + timeout + " host:" + settings.Host);
                return 18;
            }
        }

        private sealed class FlagValueSettings : CommandSettings
        {
            [CommandOption("--port [PORT]")]
            [Description("The port to listen on")]
            [DefaultValue(3000)]
            public FlagValue<int> Port { get; set; }

            [CommandOption("--timeout [SECONDS]")]
            [Description("Optional timeout")]
            public FlagValue<int?> Timeout { get; set; }

            [CommandOption("--host <HOST>")]
            [DefaultValue("localhost")]
            public string Host { get; set; } = string.Empty;
        }

        private sealed class MetadataCommand : ICommand<MetadataSettings>
        {
            public static MetadataSettings? LastSettings { get; set; }

            public int Execute(CommandContext context, MetadataSettings settings)
            {
                LastSettings = settings;
                string env = string.Join(",", settings.Environment.Select(pair => pair.Key + "=" + pair.Value));
                string tags = string.Join(",", settings.Tags.Select(group => group.Key + "=" + string.Join("|", group)));
                context.Console.WriteLine("Metadata env:" + env + " tags:" + tags);
                return 20;
            }
        }

        private sealed class MetadataSettings : CommandSettings
        {
            [CommandOption("--env <KEY=VALUE>")]
            public IDictionary<string, string> Environment { get; set; } = new Dictionary<string, string>();

            [CommandOption("--tag <KEY=VALUE>")]
            public ILookup<string, string> Tags { get; set; } = Enumerable.Empty<KeyValuePair<string, string>>()
                .ToLookup(pair => pair.Key, pair => pair.Value);

            [CommandOption("--limit <KEY=VALUE>")]
            public IReadOnlyDictionary<string, int> Limits { get; set; } = new Dictionary<string, int>();
        }

        private sealed class EnvCommand : ICommand<EnvSettings>
        {
            public static EnvSettings? LastSettings { get; set; }

            public int Execute(CommandContext context, EnvSettings settings)
            {
                LastSettings = settings;
                context.Console.WriteLine(
                    "Env deploy " +
                    settings.Service +
                    " " +
                    settings.Environment +
                    " x" +
                    settings.Replicas.ToString(CultureInfo.InvariantCulture) +
                    " dry:" +
                    settings.DryRun +
                    " mode:" +
                    settings.Mode);
                return 0;
            }
        }

        private sealed class EnvSettings : CommandSettings
        {
            [CommandArgument(0, "<service>")]
            public string Service { get; set; } = string.Empty;

            [CommandOption("--environment <ENVIRONMENT>", isRequired: true, EnvironmentVariable = EnvEnvironmentVariable)]
            public string Environment { get; set; } = string.Empty;

            [CommandOption("--replicas <COUNT>", EnvironmentVariable = EnvReplicasVariable)]
            public int Replicas { get; set; }

            [CommandOption("--dry-run", EnvironmentVariable = EnvDryRunVariable)]
            public bool DryRun { get; set; }

            [CommandOption("--mode <MODE>", EnvironmentVariable = EnvModeVariable)]
            public BuildMode Mode { get; set; }
        }

        private sealed class EnvCollectionCommand : ICommand<EnvCollectionSettings>
        {
            public static EnvCollectionSettings? LastSettings { get; set; }

            public int Execute(CommandContext context, EnvCollectionSettings settings)
            {
                LastSettings = settings;
                string env = string.Join(",", settings.Environment.Select(pair => pair.Key + "=" + pair.Value));
                string tags = string.Join(",", settings.Tags.Select(group => group.Key + "=" + string.Join("|", group)));
                context.Console.WriteLine(
                    "Env collections names:" +
                    string.Join("|", settings.Names) +
                    " codes:" +
                    string.Join("|", settings.Codes) +
                    " env:" +
                    env +
                    " tags:" +
                    tags);
                return 0;
            }
        }

        private sealed class EnvCollectionSettings : CommandSettings
        {
            [CommandOption("--name <NAME>", EnvironmentVariable = EnvNameVariable)]
            public List<string> Names { get; set; } = new List<string>();

            [CommandOption("--code <CODE>", EnvironmentVariable = EnvCodeVariable)]
            public IReadOnlyList<int> Codes { get; set; } = Array.Empty<int>();

            [CommandOption("--env <KEY=VALUE>", EnvironmentVariable = EnvMetadataVariable)]
            public IDictionary<string, string> Environment { get; set; } = new Dictionary<string, string>();

            [CommandOption("--tag <KEY=VALUE>", EnvironmentVariable = EnvTagVariable)]
            public ILookup<string, string> Tags { get; set; } = Enumerable.Empty<KeyValuePair<string, string>>()
                .ToLookup(pair => pair.Key, pair => pair.Value);
        }

        private sealed class WeightedMetadataCommand : ICommand<WeightedMetadataSettings>
        {
            public static WeightedMetadataSettings? LastSettings { get; set; }

            public int Execute(CommandContext context, WeightedMetadataSettings settings)
            {
                LastSettings = settings;
                string weights = string.Join(",", settings.Weights.Select(pair => pair.Key + "=" + pair.Value.ToString(CultureInfo.InvariantCulture)));
                string audits = string.Join(",", settings.Audits.Select(group => group.Key + "=" + string.Join("|", group.Select(value => value.ToString(CultureInfo.InvariantCulture)))));
                context.Console.WriteLine("Weighted " + weights + " audits:" + audits);
                return 21;
            }
        }

        private sealed class WeightedMetadataSettings : CommandSettings
        {
            [CommandOption("--weight <NAME/WEIGHT>")]
            [TypeConverter(typeof(SlashPairDeconstructor))]
            public IDictionary<string, int> Weights { get; set; } = new Dictionary<string, int>();

            [CommandOption("--audit <NAME/WEIGHT>")]
            [TypeConverter(typeof(SlashPairDeconstructor))]
            public ILookup<string, int> Audits { get; set; } = Enumerable.Empty<KeyValuePair<string, int>>()
                .ToLookup(pair => pair.Key, pair => pair.Value);
        }

        private sealed class SlashPairDeconstructor : PairDeconstructor<string, int>
        {
            public override bool TryDeconstruct(string input, out string key, out int value)
            {
                key = string.Empty;
                value = 0;
                string[] parts = (input ?? string.Empty).Split('/');
                if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]))
                {
                    return false;
                }

                key = parts[0];
                return int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
            }
        }

        private sealed class NotACommand
        {
        }

        private sealed class ForwardCommand : ICommand<ForwardSettings>
        {
            public static ForwardSettings? LastSettings { get; set; }

            public int Execute(CommandContext context, ForwardSettings settings)
            {
                LastSettings = settings;
                context.Console.WriteLine("Forward " + settings.Target + " " + string.Join("|", settings.Arguments));
                return 0;
            }
        }

        private sealed class ForwardSettings : CommandSettings
        {
            [CommandArgument(0, "<target>")]
            public string Target { get; set; } = string.Empty;

            [CommandArgument(1, "[arguments]")]
            public string[] Arguments { get; set; } = Array.Empty<string>();
        }

        private sealed class WorkspaceSettings : CommandSettings
        {
            [CommandOption("--path <PATH>")]
            public string Path { get; set; } = string.Empty;
        }

        private sealed class RemainingForwardCommand : ICommand<RemainingForwardSettings>
        {
            public static RemainingForwardSettings? LastSettings { get; set; }

            public int Execute(CommandContext context, RemainingForwardSettings settings)
            {
                LastSettings = settings;
                context.Console.WriteLine("Remaining " + settings.Target + " " + string.Join("|", settings.Remaining.Raw));
                return 19;
            }
        }

        private sealed class RemainingForwardSettings : CommandSettings
        {
            [CommandArgument(0, "<target>")]
            public string Target { get; set; } = string.Empty;

            [CommandArgument(1, "[arguments]")]
            public IRemainingArguments Remaining { get; set; } = RemainingArguments.Empty;
        }

        private sealed class CollectionCommand : ICommand<CollectionSettings>
        {
            public static CollectionSettings? LastSettings { get; set; }

            public int Execute(CommandContext context, CollectionSettings settings)
            {
                LastSettings = settings;
                context.Console.WriteLine(
                    "Collections items:" +
                    string.Join("|", settings.Items) +
                    " names:" +
                    string.Join("|", settings.Names) +
                    " levels:" +
                    string.Join("|", settings.Levels) +
                    " codes:" +
                    string.Join("|", settings.Codes));
                return settings.Items.Count + settings.Codes.Count;
            }
        }

        private sealed class CollectionSettings : CommandSettings
        {
            [CommandArgument(0, "[items]")]
            public IReadOnlyList<int> Items { get; set; } = Array.Empty<int>();

            [CommandOption("--name <NAME>")]
            public List<string> Names { get; set; } = new List<string>();

            [CommandOption("--level <LEVEL>")]
            public ICollection<int> Levels { get; } = new List<int>();

            [CommandOption("--code")]
            public IReadOnlyList<int> Codes { get; set; } = Array.Empty<int>();
        }

        private sealed class CopySettings : CommandSettings
        {
            [CommandArgument(0, "<source>")]
            [Description("Source path")]
            public string Source { get; set; } = string.Empty;

            [CommandArgument(1, "[destination]")]
            [Description("Destination path")]
            [DefaultValue(".")]
            public string Destination { get; set; } = ".";

            [CommandOption("-f|--force")]
            [Description("Overwrite existing files")]
            public bool Force { get; set; }

            [CommandOption("-b|--buffer-size <INT>")]
            [Description("Buffer size in KB")]
            [DefaultValue(64)]
            public int BufferSize { get; set; }

            [CommandOption("--mode")]
            [Description("Build mode")]
            [DefaultValue(BuildMode.Debug)]
            public BuildMode Mode { get; set; }

            [CommandOption("--tag <TAG>")]
            [Description("Tags to apply")]
            public string[] Tags { get; set; } = Array.Empty<string>();
        }

        private sealed class ClusteredFlagSettings : CommandSettings
        {
            [CommandArgument(0, "<source>")]
            public string Source { get; set; } = string.Empty;

            [CommandArgument(1, "[destination]")]
            [DefaultValue(".")]
            public string Destination { get; set; } = ".";

            [CommandOption("-v|--verbose")]
            public bool Verbose { get; set; }

            [CommandOption("-f|--force")]
            public bool Force { get; set; }

            [CommandOption("-b|--buffer-size <INT>")]
            [DefaultValue(64)]
            public int BufferSize { get; set; }
        }

        private sealed class BoolNegationSettings : CommandSettings
        {
            [CommandOption("--cache")]
            [DefaultValue(true)]
            public bool Cache { get; set; } = true;

            [CommandOption("--color")]
            [DefaultValue(true)]
            public bool Color { get; set; } = true;

            [CommandOption("-v|--verbose")]
            public bool Verbose { get; set; }
        }

        private sealed class ExplicitNoAliasSettings : CommandSettings
        {
            [CommandOption("--cache")]
            [DefaultValue(true)]
            public bool Cache { get; set; } = true;

            [CommandOption("--no-cache")]
            public bool NoCacheAlias { get; set; }
        }

        private sealed class HiddenOptionSettings : CommandSettings
        {
            [CommandArgument(0, "<name>")]
            public string Name { get; set; } = string.Empty;

            [CommandOption("--secret", IsHidden = true)]
            public bool Secret { get; set; }
        }

        private sealed class InvalidOptionAliasSettings : CommandSettings
        {
            [CommandOption("force")]
            public bool Force { get; set; }
        }

        private sealed class InvalidOptionValueTemplateSettings : CommandSettings
        {
            [CommandOption("--count <>")]
            public int Count { get; set; }
        }

        private sealed class InvalidArgumentTemplateSettings : CommandSettings
        {
            [CommandArgument(0, "source")]
            public string Source { get; set; } = string.Empty;
        }

        private sealed class TemplateShapeSettings : CommandSettings
        {
            [CommandArgument(0, "<source>")]
            public string Source { get; set; } = string.Empty;

            [CommandOption("--env <KEY=VALUE>")]
            public IDictionary<string, string> Environment { get; set; } = new Dictionary<string, string>();

            [CommandOption("--port [PORT]")]
            public FlagValue<int> Port { get; set; }
        }

        private sealed class DuplicateOptionAliasSettings : CommandSettings
        {
            [CommandOption("--name <NAME>")]
            public string Name { get; set; } = string.Empty;

            [CommandOption("-n|--name <DISPLAY_NAME>")]
            public string DisplayName { get; set; } = string.Empty;
        }

        private sealed class DuplicateArgumentPositionSettings : CommandSettings
        {
            [CommandArgument(0, "<source>")]
            public string Source { get; set; } = string.Empty;

            [CommandArgument(0, "<destination>")]
            public string Destination { get; set; } = string.Empty;
        }

        private sealed class RequiredArgumentAfterOptionalSettings : CommandSettings
        {
            [CommandArgument(0, "[source]")]
            public string Source { get; set; } = string.Empty;

            [CommandArgument(1, "<destination>")]
            public string Destination { get; set; } = string.Empty;
        }

        private sealed class CultureAmountSettings : CommandSettings
        {
            [CommandOption("--amount <AMOUNT>", true)]
            public decimal Amount { get; set; }
        }

        private enum BuildMode
        {
            Debug,
            Release,
            ReleaseCandidate,
            [Description("nightly build")]
            Nightly,
            [Display(Name = "release train")]
            ReleaseTrain
        }

        private sealed class PeriodHelpSettings : CommandSettings
        {
            [CommandArgument(0, "<input>")]
            [Description("Input path.")]
            public string Input { get; set; } = string.Empty;

            [CommandOption("--output <OUTPUT>")]
            [Description("Output path.")]
            [DefaultValue("out.txt")]
            public string Output { get; set; } = "out.txt";
        }

        private sealed class PublishCommand : ICommand<PublishSettings>
        {
            public static bool Executed { get; set; }

            public int Execute(CommandContext context, PublishSettings settings)
            {
                Executed = true;
                return 0;
            }
        }

        private sealed class PublishSettings : CommandSettings
        {
            [CommandArgument(0, "<service>")]
            public string Service { get; set; } = string.Empty;

            [CommandOption("--environment", isRequired: true)]
            public string Environment { get; set; } = string.Empty;

            [CommandOption("--replicas")]
            [DefaultValue(1)]
            public int Replicas { get; set; } = 1;

            public override CommandValidationResult Validate()
            {
                return Replicas < 1
                    ? CommandValidationResult.Error("Replicas must be at least 1.")
                    : CommandValidationResult.Success();
            }
        }

        private sealed class DataAnnotationsDeployCommand : ICommand<DataAnnotationsDeploySettings>
        {
            public static bool Executed { get; set; }

            public int Execute(CommandContext context, DataAnnotationsDeploySettings settings)
            {
                Executed = true;
                context.Console.WriteLine(
                    "Annotated deploy " +
                    settings.Service +
                    " to " +
                    settings.Environment +
                    " x" +
                    settings.Replicas.ToString(CultureInfo.InvariantCulture));
                return 0;
            }
        }

        private sealed class DataAnnotationsDeploySettings : CommandSettings
        {
            [CommandArgument(0, "<service>")]
            public string Service { get; set; } = string.Empty;

            [CommandOption("--environment <ENVIRONMENT>")]
            [Required(ErrorMessage = "Environment is required.")]
            public string Environment { get; set; } = string.Empty;

            [CommandOption("--replicas <COUNT>")]
            [Range(1, 10, ErrorMessage = "Replicas must be between 1 and 10.")]
            public int Replicas { get; set; } = 1;
        }

        private sealed class ValidatableDeployCommand : ICommand<ValidatableDeploySettings>
        {
            public static bool Executed { get; set; }

            public int Execute(CommandContext context, ValidatableDeploySettings settings)
            {
                Executed = true;
                context.Console.WriteLine("Validatable deploy " + settings.Service + " to " + settings.Environment);
                return 0;
            }
        }

        private sealed class ValidatableDeploySettings : CommandSettings, IValidatableObject
        {
            [CommandArgument(0, "<service>")]
            public string Service { get; set; } = string.Empty;

            [CommandOption("--environment <ENVIRONMENT>")]
            public string Environment { get; set; } = string.Empty;

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (string.Equals(Service, Environment, StringComparison.OrdinalIgnoreCase))
                {
                    yield return new ValidationResult("Environment must differ from service.", new[] { nameof(Environment) });
                }
            }
        }

        private sealed class ListenCommand : ICommand<ListenSettings>
        {
            public static bool Executed { get; set; }

            public static int LastPort { get; set; }

            public int Execute(CommandContext context, ListenSettings settings)
            {
                Executed = true;
                LastPort = settings.Port;
                context.Console.WriteLine("Listening on " + settings.Port.ToString(CultureInfo.InvariantCulture));
                return 0;
            }
        }

        private sealed class ListenSettings : CommandSettings
        {
            [CommandOption("--port <PORT>", isRequired: true)]
            [PortRange]
            public int Port { get; set; }
        }

        private sealed class ParameterInfoCommand : ICommand<ParameterInfoSettings>
        {
            public int Execute(CommandContext context, ParameterInfoSettings settings)
            {
                context.Console.WriteLine("Inspect " + settings.Path + " x" + settings.Count.ToString(CultureInfo.InvariantCulture));
                return 0;
            }
        }

        private sealed class ParameterInfoSettings : CommandSettings
        {
            [CommandArgument(0, "<path>")]
            [Description("Input path")]
            [MetadataCapture]
            public string Path { get; set; } = string.Empty;

            [CommandOption("--count <COUNT>", true)]
            [Description("Item count")]
            [MetadataCapture]
            public int Count { get; set; }
        }

        private sealed class MetadataCaptureAttribute : ParameterValidationAttribute
        {
            public static List<ParameterSnapshot> Snapshots { get; } = new List<ParameterSnapshot>();

            public override CommandValidationResult Validate(CommandParameterContext context)
            {
                ICommandParameterInfo parameter = context.Parameter;
                Snapshots.Add(new ParameterSnapshot(
                    parameter.Name,
                    parameter.Template,
                    parameter.Description,
                    parameter.DefaultValue,
                    parameter.IsRequired,
                    parameter.IsOption,
                    parameter.Property.Name,
                    parameter.ValueType,
                    context.Value,
                    context.ParameterName));
                return CommandValidationResult.Success();
            }
        }

        private sealed class ParameterSnapshot
        {
            public ParameterSnapshot(
                string name,
                string template,
                string description,
                object defaultValue,
                bool isRequired,
                bool isOption,
                string propertyName,
                Type valueType,
                object value,
                string legacyParameterName)
            {
                Name = name;
                Template = template;
                Description = description;
                DefaultValue = defaultValue;
                IsRequired = isRequired;
                IsOption = isOption;
                PropertyName = propertyName;
                ValueType = valueType;
                Value = value;
                LegacyParameterName = legacyParameterName;
            }

            public string Name { get; }

            public string Template { get; }

            public string Description { get; }

            public object DefaultValue { get; }

            public bool IsRequired { get; }

            public bool IsOption { get; }

            public string PropertyName { get; }

            public Type ValueType { get; }

            public object Value { get; }

            public string LegacyParameterName { get; }
        }

        private sealed class PortRangeAttribute : ParameterValidationAttribute
        {
            public override CommandValidationResult Validate(CommandParameterContext context)
            {
                int port = context.Value is int value ? value : 0;
                return port >= 1 && port <= 65535
                    ? CommandValidationResult.Success()
                    : CommandValidationResult.Error("Port must be between 1 and 65535.");
            }
        }

        private sealed class AliasValidationCommand : Command<AliasValidationSettings>
        {
            public static bool Executed { get; set; }

            protected override int Execute(CommandContext context, AliasValidationSettings settings)
            {
                Executed = true;
                context.Console.MarkupLineInterpolated($"Alias [green]{settings.Name}[/] x{settings.Count}");
                return 0;
            }
        }

        private sealed class AliasValidationSettings : CommandSettings
        {
            [CommandOption("--name <NAME>", true)]
            public string Name { get; set; } = string.Empty;

            [CommandOption("--count <COUNT>", true)]
            [PositiveCount]
            public int Count { get; set; }

            public override ConsoulLibrary.Cli.ValidationResult Validate()
            {
                return string.Equals(Name, "bad", StringComparison.OrdinalIgnoreCase)
                    ? ConsoulLibrary.Cli.ValidationResult.Error("Name cannot be bad.")
                    : ConsoulLibrary.Cli.ValidationResult.Success();
            }
        }

        private sealed class PositiveCountAttribute : ParameterValidationAttribute
        {
            public override ConsoulLibrary.Cli.ValidationResult Validate(CommandParameterContext context)
            {
                int count = context.Value is int value ? value : 0;
                return count >= 1
                    ? ConsoulLibrary.Cli.ValidationResult.Success()
                    : ConsoulLibrary.Cli.ValidationResult.Error("Count must be at least 1.");
            }
        }

        private sealed class ConnectCommand : ICommand<EndpointSettings>
        {
            public static Endpoint? LastEndpoint { get; set; }

            public int Execute(CommandContext context, EndpointSettings settings)
            {
                LastEndpoint = settings.Endpoint;
                context.Console.WriteLine("Connected " + settings.Endpoint.Host + ":" + settings.Endpoint.Port.ToString(CultureInfo.InvariantCulture));
                return 0;
            }
        }

        private sealed class EndpointSettings : CommandSettings
        {
            [CommandOption("--endpoint <ENDPOINT>", isRequired: true)]
            [Description("Host and port endpoint")]
            public Endpoint Endpoint { get; set; } = new Endpoint(string.Empty, 0);
        }

        [TypeConverter(typeof(EndpointConverter))]
        private sealed class Endpoint
        {
            public Endpoint(string host, int port)
            {
                Host = host;
                Port = port;
            }

            public string Host { get; }

            public int Port { get; }
        }

        private sealed class EndpointConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
            {
                return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
            }

            public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
            {
                var text = value as string;
                if (text == null)
                {
                    return base.ConvertFrom(context, culture, value);
                }

                string[] parts = text.Split(':');
                if (parts.Length != 2 || !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int port))
                {
                    throw new FormatException("Endpoint values must use host:port.");
                }

                return new Endpoint(parts[0], port);
            }
        }

        private sealed class ConnectWithPropertyConverterCommand : ICommand<PropertyEndpointSettings>
        {
            public static PropertyEndpoint? LastEndpoint { get; set; }

            public int Execute(CommandContext context, PropertyEndpointSettings settings)
            {
                LastEndpoint = settings.Endpoint;
                context.Console.WriteLine("Property converter " + settings.Endpoint.Host + ":" + settings.Endpoint.Port.ToString(CultureInfo.InvariantCulture));
                return 22;
            }
        }

        private sealed class PropertyEndpointSettings : CommandSettings
        {
            [CommandOption("--endpoint <ENDPOINT>", isRequired: true)]
            [TypeConverter(typeof(PropertyEndpointConverter))]
            public PropertyEndpoint Endpoint { get; set; } = new PropertyEndpoint(string.Empty, 0);
        }

        private sealed class PropertyEndpoint
        {
            public PropertyEndpoint(string host, int port)
            {
                Host = host;
                Port = port;
            }

            public string Host { get; }

            public int Port { get; }
        }

        private sealed class PropertyEndpointConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
            {
                return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
            }

            public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
            {
                var text = value as string;
                if (text == null)
                {
                    return base.ConvertFrom(context, culture, value);
                }

                string[] parts = text.Split(':');
                if (parts.Length != 2 || !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int port))
                {
                    throw new FormatException("Endpoint values must use host:port.");
                }

                return new PropertyEndpoint(parts[0], port);
            }
        }

        private static void ConfigureTypedRemoteBranch(IConfigurator<RemoteSettings> remote)
        {
            remote.SetDescription("Manage typed remotes");
            remote.AddCommand<RemoteAddCommand>("add");
            remote.AddBranch<RemoteAuditSettings>("audit", ConfigureTypedAuditBranch);
        }

        private static void ConfigureTypedAuditBranch(IConfigurator<RemoteAuditSettings> audit)
        {
            audit.AddCommand<RemoteListCommand>("list")
                .WithDescription("List remote audit entries");
        }

        private static CommandRegistration AddLimitedCommand<TSettings, TCommand>(IConfigurator configurator, string name)
            where TSettings : CommandSettings
            where TCommand : class, ICommandLimiter<TSettings>
        {
            return configurator.AddCommand<TCommand>(name);
        }

        private static CommandRegistration AddLimitedCommand<TSettings, TCommand>(IBranchConfigurator configurator, string name)
            where TSettings : CommandSettings
            where TCommand : class, ICommandLimiter<TSettings>
        {
            return configurator.AddCommand<TCommand>(name);
        }

        private sealed class RendererAwareView : IView, INavigationAwareView
        {
            private ViewNavigationContext _navigationContext = new ViewNavigationContext();
            private bool _navigationContextAssigned;

            public static bool Rendered { get; private set; }

            public static bool ReceivedNavigationContext { get; private set; }

            public static IReadOnlyList<string> BreadcrumbTrail { get; private set; } = Array.Empty<string>();

            public string Title { get; set; } = "Renderer Aware";

            public bool GoBackRequested { get; private set; }

            ViewNavigationContext INavigationAwareView.NavigationContext
            {
                get => _navigationContext;
                set
                {
                    _navigationContext = value ?? new ViewNavigationContext();
                    _navigationContextAssigned = true;
                }
            }

            public static void Reset()
            {
                Rendered = false;
                ReceivedNavigationContext = false;
                BreadcrumbTrail = Array.Empty<string>();
            }

            public Task RenderAsync(CancellationToken cancellationToken = default)
            {
                Rendered = true;
                ReceivedNavigationContext = _navigationContextAssigned;
                BreadcrumbTrail = _navigationContextAssigned
                    ? _navigationContext.BreadcrumbTrail.ToArray()
                    : Array.Empty<string>();
                GoBack();
                return Task.CompletedTask;
            }

            public void Render()
            {
                RenderAsync().GetAwaiter().GetResult();
            }

            public void GoBack()
            {
                GoBackRequested = true;
            }
        }

        private class RemoteSettings : CommandSettings
        {
            [CommandOption("-v|--verbose")]
            [Description("Show detailed output")]
            public bool Verbose { get; set; }
        }

        private sealed class RemoteAuditSettings : CommandSettings
        {
        }

        private sealed class RemoteAddSettings : RemoteSettings
        {
            [CommandArgument(0, "<name>")]
            public string Name { get; set; } = string.Empty;

            [CommandArgument(1, "<url>")]
            public string Url { get; set; } = string.Empty;
        }

        private sealed class RemoteAddCommand : ICommand<RemoteAddSettings>
        {
            public static RemoteAddSettings? LastSettings { get; set; }

            public static string? LastCommandName { get; set; }

            public int Execute(CommandContext context, RemoteAddSettings settings)
            {
                LastSettings = settings;
                LastCommandName = context.CommandName;
                context.Console.WriteLine("Added " + settings.Name);
                return 0;
            }
        }

        private sealed class RemoteListCommand : ICommand
        {
            public static bool Ran { get; set; }

            public int Execute(CommandContext context)
            {
                Ran = true;
                context.Console.WriteLine("Listed remotes");
                return 0;
            }
        }

        private sealed class UploadCommand : ICommand
        {
            public static bool Ran { get; set; }

            public int Execute(CommandContext context)
            {
                Ran = true;
                context.Console.WriteLine("Uploaded");
                return 0;
            }
        }
    }
}
