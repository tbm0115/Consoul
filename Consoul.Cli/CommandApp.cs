using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides a small CommandApp-style runner for Consoul command-line proof-of-concept applications.
    /// </summary>
    public class CommandApp : ICommandApp
    {
        private readonly CommandAppConfigurator _configurator = new CommandAppConfigurator();
        private readonly Type _defaultCommandType;
        private readonly IAnsiConsole _console;

        private IAnsiConsole Console => _console ?? AnsiConsole.Console;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandApp"/> class.
        /// </summary>
        public CommandApp() : this((Type)null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandApp"/> class.
        /// </summary>
        /// <param name="console">The console facade used for help and errors.</param>
        public CommandApp(IAnsiConsole console) : this((Type)null, console)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandApp"/> class.
        /// </summary>
        /// <param name="registrar">The type registrar used to resolve command and view instances.</param>
        public CommandApp(ITypeRegistrar registrar) : this((Type)null, null)
        {
            SetTypeRegistrar(registrar);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandApp"/> class.
        /// </summary>
        /// <param name="registrar">The type registrar used to resolve command and view instances.</param>
        /// <param name="frontend">The frontend that configures the registrar.</param>
        public CommandApp(ITypeRegistrar registrar, ITypeRegistrarFrontend frontend) : this((Type)null, null)
        {
            SetTypeRegistrar(registrar, frontend);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandApp"/> class.
        /// </summary>
        /// <param name="registrar">The type registrar used to resolve command and view instances.</param>
        /// <param name="console">The console facade used for help and errors.</param>
        public CommandApp(ITypeRegistrar registrar, IAnsiConsole console) : this((Type)null, console)
        {
            SetTypeRegistrar(registrar);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandApp"/> class.
        /// </summary>
        /// <param name="registrar">The type registrar used to resolve command and view instances.</param>
        /// <param name="frontend">The frontend that configures the registrar.</param>
        /// <param name="console">The console facade used for help and errors.</param>
        public CommandApp(ITypeRegistrar registrar, ITypeRegistrarFrontend frontend, IAnsiConsole console) : this((Type)null, console)
        {
            SetTypeRegistrar(registrar, frontend);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandApp"/> class.
        /// </summary>
        /// <param name="defaultCommandType">Optional command type to run when no command name is supplied.</param>
        /// <param name="console">The console facade used for help and errors.</param>
        protected CommandApp(Type defaultCommandType, IAnsiConsole console)
        {
            _defaultCommandType = defaultCommandType;
            _console = console;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandApp"/> class.
        /// </summary>
        /// <param name="defaultCommandType">Optional command type to run when no command name is supplied.</param>
        /// <param name="registrar">The type registrar used to resolve command and view instances.</param>
        /// <param name="console">The console facade used for help and errors.</param>
        protected CommandApp(Type defaultCommandType, ITypeRegistrar registrar, IAnsiConsole console)
            : this(defaultCommandType, console)
        {
            SetTypeRegistrar(registrar);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandApp"/> class.
        /// </summary>
        /// <param name="defaultCommandType">Optional command type to run when no command name is supplied.</param>
        /// <param name="registrar">The type registrar used to resolve command and view instances.</param>
        /// <param name="frontend">The frontend that configures the registrar.</param>
        /// <param name="console">The console facade used for help and errors.</param>
        protected CommandApp(Type defaultCommandType, ITypeRegistrar registrar, ITypeRegistrarFrontend frontend, IAnsiConsole console)
            : this(defaultCommandType, console)
        {
            SetTypeRegistrar(registrar, frontend);
        }

        /// <summary>
        /// Configures the command app.
        /// </summary>
        /// <param name="configure">The configuration callback.</param>
        public void Configure(Action<CommandAppConfigurator> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            configure(_configurator);
            RegisterDefaultCommandType();
        }

        /// <summary>
        /// Runs the command app.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>The process-style exit code.</returns>
        public int Run(string[] args)
        {
            return RunAsync(args).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Runs the command app.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>The process-style exit code.</returns>
        public int Run(IEnumerable<string> args)
        {
            return RunAsync(args).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Runs the command app asynchronously.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that resolves to the process-style exit code.</returns>
        public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
        {
            args = args ?? new string[0];

            try
            {
                return await RunCoreAsync(args, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                if (_configurator.Settings.PropagateExceptions)
                {
                    throw;
                }

                return _configurator.Settings.CancellationExitCode;
            }
            catch (Exception ex)
            {
                if (_configurator.Settings.PropagateExceptions)
                {
                    throw;
                }

                if (_configurator.ExceptionHandler != null)
                {
                    var context = new CommandExceptionContext(ex, args, Console, _configurator.Settings);
                    return _configurator.ExceptionHandler(context);
                }

                Console.WriteLine("ERROR: " + ex.Message);
                return 1;
            }
        }

        /// <summary>
        /// Runs the command app asynchronously.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that resolves to the process-style exit code.</returns>
        public Task<int> RunAsync(IEnumerable<string> args, CancellationToken cancellationToken = default)
        {
            return RunAsync(args == null ? null : args.ToArray(), cancellationToken);
        }

        /// <summary>
        /// Gets command-line completion suggestions for the supplied argument tokens.
        /// </summary>
        /// <param name="args">The current command-line arguments, including the token being completed when available.</param>
        /// <returns>The matching completion suggestions.</returns>
        public IReadOnlyList<CommandCompletionItem> GetCompletions(string[] args)
        {
            return CommandCompletionProvider.GetCompletions(_configurator, GetDefaultRegistration(), args);
        }

        /// <summary>
        /// Gets command-line completion suggestions for the supplied argument tokens.
        /// </summary>
        /// <param name="args">The current command-line arguments, including the token being completed when available.</param>
        /// <returns>The matching completion suggestions.</returns>
        public IReadOnlyList<CommandCompletionItem> GetCompletions(IEnumerable<string> args)
        {
            return GetCompletions(args == null ? null : args.ToArray());
        }

        private async Task<int> RunCoreAsync(string[] args, CancellationToken cancellationToken)
        {
            CommandRegistration defaultRegistration = GetDefaultRegistration();
            if (_configurator.Settings.ValidateExamples)
            {
                _configurator.ValidateRegisteredExamples(defaultRegistration);
            }

            if (args.Length == 0)
            {
                if (defaultRegistration != null)
                {
                    return await ExecuteAsync(
                        defaultRegistration,
                        string.Empty,
                        new string[0],
                        args,
                        cancellationToken).ConfigureAwait(false);
                }

                WriteHelp();
                return 0;
            }

            string commandName = args[0] ?? string.Empty;
            if (IsVersionOption(commandName))
            {
                WriteVersion();
                return 0;
            }

            if (IsHelp(commandName))
            {
                if (args.Length > 1)
                {
                    string[] helpPath = args.Skip(1).ToArray();
                    if (TryWritePathHelp(helpPath))
                    {
                        return 0;
                    }

                    WriteUnknownHelpPath(helpPath);
                    return 1;
                }

                WriteHelp();
                return 0;
            }

            CommandRegistration registration = _configurator.Find(commandName);
            if (registration != null)
            {
                string[] commandArguments = args.Skip(1).ToArray();
                if (commandArguments.Any(IsHelp))
                {
                    WriteCommandHelp(registration, new[] { commandName });
                    return 0;
                }

                return await ExecuteAsync(registration, commandName, commandArguments, args, cancellationToken).ConfigureAwait(false);
            }

            CommandBranchRegistration branch = _configurator.FindBranch(commandName);
            if (branch != null)
            {
                return await RunBranchAsync(
                    branch,
                    new[] { commandName },
                    args,
                    1,
                    new List<string>(),
                    new Type[0],
                    cancellationToken).ConfigureAwait(false);
            }

            if (IsVersionCommand(commandName))
            {
                WriteVersion();
                return 0;
            }

            if (defaultRegistration != null)
            {
                return await ExecuteAsync(
                    defaultRegistration,
                    string.Empty,
                    args,
                    args,
                    cancellationToken).ConfigureAwait(false);
            }

            WriteUnknownCommand(commandName, commandName, _configurator.Commands, _configurator.Branches);
            WriteHelp();
            return 1;
        }

        private async Task<int> RunBranchAsync(
            CommandBranchRegistration branch,
            IReadOnlyList<string> path,
            string[] rawArguments,
            int index,
            List<string> inheritedArguments,
            IReadOnlyList<Type> inheritedSettingsTypes,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<Type> activeSettingsTypes = AppendSettingsType(inheritedSettingsTypes, branch.SettingsType);

            while (index < rawArguments.Length)
            {
                string token = rawArguments[index] ?? string.Empty;
                if (IsHelp(token))
                {
                    WriteBranchHelp(branch, path);
                    return 0;
                }

                int nextIndex;
                if (!TryConsumeBranchOption(branch, rawArguments, index, inheritedArguments, out nextIndex))
                {
                    break;
                }

                index = nextIndex;
            }

            if (index >= rawArguments.Length)
            {
                WriteBranchHelp(branch, path);
                return 0;
            }

            string commandName = rawArguments[index] ?? string.Empty;
            CommandBranchRegistration childBranch = branch.FindBranch(commandName, _configurator.Settings.CaseSensitive);
            if (childBranch != null)
            {
                return await RunBranchAsync(
                    childBranch,
                    path.Concat(new[] { commandName }).ToArray(),
                    rawArguments,
                    index + 1,
                    inheritedArguments,
                    activeSettingsTypes,
                    cancellationToken).ConfigureAwait(false);
            }

            CommandRegistration command = branch.FindCommand(commandName, _configurator.Settings.CaseSensitive);
            if (command == null)
            {
                string unknownPath = string.Join(" ", path.Concat(new[] { commandName }));
                WriteUnknownCommand(unknownPath, commandName, branch.Commands, branch.Branches);
                WriteBranchHelp(branch, path);
                return 1;
            }

            string[] commandArguments = inheritedArguments.Concat(rawArguments.Skip(index + 1)).ToArray();
            string[] commandPath = path.Concat(new[] { commandName }).ToArray();
            if (commandArguments.Any(IsHelp))
            {
                WriteCommandHelp(command, commandPath, activeSettingsTypes);
                return 0;
            }

            return await ExecuteAsync(command, string.Join(" ", commandPath), commandArguments, rawArguments, cancellationToken).ConfigureAwait(false);
        }

        private async Task<int> ExecuteAsync(CommandRegistration registration, string commandName, string[] arguments, string[] rawArguments, CancellationToken cancellationToken)
        {
            CommandSettings settings = null;

            if (registration.SettingsType != null)
            {
                settings = CommandSettingsBinder.Bind(registration.SettingsType, arguments, _configurator.Settings);
                CommandValidationResult validation = settings.Validate();
                if (validation != null && !validation.Successful)
                {
                    throw new CommandParameterException(validation.Message);
                }

                _configurator.RegisterCommandSettings(settings);
            }

            object command = registration.CreateCommand();
            var context = new CommandContext(commandName, arguments, rawArguments, Console, cancellationToken);

            _configurator.Interceptor?.Intercept(context, settings);

            int result;
            if (registration.SettingsType != null)
            {
                result = await ExecuteSettingsCommandAsync(command, registration.SettingsType, context, settings, cancellationToken).ConfigureAwait(false);
                _configurator.Interceptor?.InterceptResult(context, settings, ref result);
                return result;
            }

            var asyncCommand = command as IAsyncCommand;
            if (asyncCommand != null)
            {
                result = await asyncCommand.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
                _configurator.Interceptor?.InterceptResult(context, settings, ref result);
                return result;
            }

            var syncCommand = command as ICommand;
            if (syncCommand != null)
            {
                result = syncCommand.Execute(context);
                _configurator.Interceptor?.InterceptResult(context, settings, ref result);
                return result;
            }

            throw new CommandRuntimeException("Command '" + registration.Name + "' must implement ICommand or IAsyncCommand.");
        }

        private static async Task<int> ExecuteSettingsCommandAsync(object command, Type settingsType, CommandContext context, CommandSettings settings, CancellationToken cancellationToken)
        {
            Type commandType = command.GetType();
            Type asyncInterface = typeof(IAsyncCommand<>).MakeGenericType(settingsType);
            if (asyncInterface.IsAssignableFrom(commandType))
            {
                MethodInfo method = asyncInterface.GetMethod(nameof(IAsyncCommand<CommandSettings>.ExecuteAsync));
                var task = (Task<int>)InvokeCommandMethod(method, command, new object[] { context, settings, cancellationToken });
                return await task.ConfigureAwait(false);
            }

            Type syncInterface = typeof(ICommand<>).MakeGenericType(settingsType);
            if (syncInterface.IsAssignableFrom(commandType))
            {
                MethodInfo method = syncInterface.GetMethod(nameof(ICommand<CommandSettings>.Execute));
                return (int)InvokeCommandMethod(method, command, new object[] { context, settings });
            }

            throw new CommandRuntimeException("Command must implement ICommand<TSettings> or IAsyncCommand<TSettings>.");
        }

        private static object InvokeCommandMethod(MethodInfo method, object command, object[] arguments)
        {
            try
            {
                return method.Invoke(command, arguments);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                }

                throw;
            }
        }

        private CommandRegistration CreateDefaultRegistration()
        {
            RegisterDefaultCommandType();
            return _defaultCommandType == null
                ? null
                : new CommandRegistration("__default", _defaultCommandType, () => _configurator.CreateCommand(_defaultCommandType));
        }

        private CommandRegistration GetDefaultRegistration()
        {
            return _configurator.DefaultCommand ?? CreateDefaultRegistration();
        }

        private void SetTypeRegistrar(ITypeRegistrar registrar)
        {
            _configurator.SetTypeRegistrar(registrar, () => Console);
            RegisterDefaultCommandType();
        }

        private void SetTypeRegistrar(ITypeRegistrar registrar, ITypeRegistrarFrontend frontend)
        {
            _configurator.SetTypeRegistrar(registrar, frontend, () => Console);
            RegisterDefaultCommandType();
        }

        private void RegisterDefaultCommandType()
        {
            if (_defaultCommandType != null)
            {
                _configurator.RegisterCommandType(_defaultCommandType);
            }
        }

        private void WriteHelp()
        {
            CommandRegistration defaultRegistration = GetDefaultRegistration();
            if (_configurator.HelpProvider != null)
            {
                _configurator.HelpProvider.WriteApplicationHelp(
                    Console,
                    new CommandAppHelpContext(_configurator.Settings, _configurator.Commands, _configurator.Branches, _configurator.Examples, defaultRegistration));
                return;
            }

            string appName = _configurator.Settings.ApplicationName;
            string version = _configurator.Settings.ApplicationVersion;

            Console.WriteLine(string.IsNullOrWhiteSpace(version) ? appName : appName + " " + version);
            if (!string.IsNullOrWhiteSpace(_configurator.Settings.ApplicationDescription))
            {
                Console.WriteLine(FormatDescription(_configurator.Settings.ApplicationDescription));
            }

            Console.WriteLine();
            Console.WriteLine("USAGE:");
            bool hasVisibleCommands = _configurator.Commands.Any(command => !command.Hidden);
            bool hasVisibleBranches = _configurator.Branches.Any(branch => !branch.Hidden);
            bool hasDefaultCommand = defaultRegistration != null;
            if (hasVisibleCommands || hasVisibleBranches)
            {
                Console.WriteLine("    " + appName + " [COMMAND] [ARGUMENTS]");
            }

            if (hasDefaultCommand)
            {
                Console.WriteLine("    " + appName + BuildUsageSuffix(defaultRegistration, null));
            }

            if (!hasVisibleCommands && !hasVisibleBranches && !hasDefaultCommand)
            {
                Console.WriteLine("    " + appName + " [COMMAND] [ARGUMENTS]");
            }

            WriteDefaultCommandHelp(defaultRegistration, appName);

            if (!hasVisibleCommands)
            {
                if (!hasVisibleBranches)
                {
                    if (!hasDefaultCommand)
                    {
                        Console.WriteLine();
                        Console.WriteLine("COMMANDS:");
                        Console.WriteLine("    No commands registered.");
                    }

                    WriteTopLevelExamples(appName);
                    return;
                }
            }

            Console.WriteLine();
            Console.WriteLine("COMMANDS:");
            foreach (CommandBranchRegistration branch in _configurator.Branches.Where(branch => !branch.Hidden))
            {
                string aliases = branch.Aliases.Count == 0
                    ? string.Empty
                    : " (" + string.Join(", ", branch.Aliases) + ")";
                string label = "    " + branch.Name + aliases;
                string description = FormatDescription(branch.Description);
                Console.WriteLine(PadRight(label, 28) + description);
            }

            if (!hasVisibleCommands)
            {
                WriteTopLevelExamples(appName);
                return;
            }

            foreach (CommandRegistration command in _configurator.Commands.Where(command => !command.Hidden))
            {
                string aliases = command.Aliases.Count == 0
                    ? string.Empty
                    : " (" + string.Join(", ", command.Aliases) + ")";
                string label = "    " + command.Name + aliases;
                string description = FormatDescription(command.Description);
                Console.WriteLine(PadRight(label, 28) + description);
            }

            WriteTopLevelExamples(appName);
        }

        private void WriteDefaultCommandHelp(CommandRegistration defaultRegistration, string appName)
        {
            if (defaultRegistration == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(defaultRegistration.Description))
            {
                Console.WriteLine();
                Console.WriteLine("DEFAULT COMMAND:");
                Console.WriteLine("    " + FormatDescription(defaultRegistration.Description));
            }

            if (defaultRegistration.Examples.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("DEFAULT EXAMPLES:");
                foreach (IReadOnlyList<string> example in defaultRegistration.Examples)
                {
                    string suffix = example == null || example.Count == 0
                        ? string.Empty
                        : " " + string.Join(" ", example);
                    Console.WriteLine("    " + appName + suffix);
                }
            }

            WriteSettingsHelp(defaultRegistration.SettingsType);
        }

        private void WriteTopLevelExamples(string appName)
        {
            if (_configurator.Examples.Count == 0)
            {
                return;
            }

            Console.WriteLine();
            Console.WriteLine("EXAMPLES:");
            foreach (IReadOnlyList<string> example in _configurator.Examples)
            {
                string suffix = example == null || example.Count == 0
                    ? string.Empty
                    : " " + string.Join(" ", example);
                Console.WriteLine("    " + appName + suffix);
            }
        }

        private void WriteVersion()
        {
            string appName = _configurator.Settings.ApplicationName;
            string version = _configurator.Settings.ApplicationVersion;
            Console.WriteLine(string.IsNullOrWhiteSpace(version) ? appName : appName + " " + version);
        }

        private void WriteUnknownCommand(
            string commandPath,
            string commandName,
            IEnumerable<CommandRegistration> commands,
            IEnumerable<CommandBranchRegistration> branches)
        {
            Console.WriteLine("Unknown command: " + commandPath);

            IReadOnlyList<string> suggestions = FindCommandSuggestions(commandName, commands, branches);
            if (suggestions.Count == 1)
            {
                Console.WriteLine("Did you mean '" + suggestions[0] + "'?");
            }
            else if (suggestions.Count > 1)
            {
                Console.WriteLine("Did you mean one of these? " + string.Join(", ", suggestions.Select(suggestion => "'" + suggestion + "'")));
            }
        }

        private void WriteUnknownHelpPath(string[] path)
        {
            if (path == null || path.Length == 0)
            {
                WriteHelp();
                return;
            }

            CommandRegistration rootCommand = _configurator.Find(path[0]);
            if (rootCommand != null)
            {
                Console.WriteLine("Unknown command: " + string.Join(" ", path));
                WriteCommandHelp(rootCommand, new[] { path[0] });
                return;
            }

            string unknownName = path[0] ?? string.Empty;
            IEnumerable<CommandRegistration> commands = _configurator.Commands;
            IEnumerable<CommandBranchRegistration> branches = _configurator.Branches;
            CommandBranchRegistration matchedBranch = null;
            List<string> matchedBranchPath = null;
            IReadOnlyList<Type> inheritedSettingsTypes = new Type[0];

            CommandBranchRegistration branch = _configurator.FindBranch(path[0]);
            if (branch != null)
            {
                matchedBranch = branch;
                matchedBranchPath = new List<string> { path[0] };
                inheritedSettingsTypes = AppendSettingsType(inheritedSettingsTypes, branch.SettingsType);
                commands = branch.Commands;
                branches = branch.Branches;

                for (int index = 1; index < path.Length; index++)
                {
                    string segment = path[index] ?? string.Empty;
                    CommandBranchRegistration childBranch = branch.FindBranch(segment, _configurator.Settings.CaseSensitive);
                    if (childBranch != null)
                    {
                        branch = childBranch;
                        matchedBranch = branch;
                        matchedBranchPath.Add(segment);
                        inheritedSettingsTypes = AppendSettingsType(inheritedSettingsTypes, branch.SettingsType);
                        commands = branch.Commands;
                        branches = branch.Branches;
                        continue;
                    }

                    CommandRegistration childCommand = branch.FindCommand(segment, _configurator.Settings.CaseSensitive);
                    if (childCommand != null)
                    {
                        Console.WriteLine("Unknown command: " + string.Join(" ", path));
                        WriteCommandHelp(childCommand, matchedBranchPath.Concat(new[] { segment }).ToArray(), inheritedSettingsTypes);
                        return;
                    }

                    unknownName = segment;
                    commands = branch.Commands;
                    branches = branch.Branches;
                    break;
                }
            }

            WriteUnknownCommand(string.Join(" ", path), unknownName, commands, branches);
            if (matchedBranch != null)
            {
                WriteBranchHelp(matchedBranch, matchedBranchPath);
                return;
            }

            WriteHelp();
        }

        private static IReadOnlyList<string> FindCommandSuggestions(
            string commandName,
            IEnumerable<CommandRegistration> commands,
            IEnumerable<CommandBranchRegistration> branches)
        {
            return SuggestionHelper.FindSuggestions(commandName, EnumerateSuggestionCandidates(commands, branches));
        }

        private static IEnumerable<SuggestionHelper.Candidate> EnumerateSuggestionCandidates(
            IEnumerable<CommandRegistration> commands,
            IEnumerable<CommandBranchRegistration> branches)
        {
            foreach (CommandBranchRegistration branch in (branches ?? Enumerable.Empty<CommandBranchRegistration>()).Where(branch => !branch.Hidden))
            {
                yield return new SuggestionHelper.Candidate(branch.Name, branch.Aliases);
            }

            foreach (CommandRegistration command in (commands ?? Enumerable.Empty<CommandRegistration>()).Where(command => !command.Hidden))
            {
                yield return new SuggestionHelper.Candidate(command.Name, command.Aliases);
            }
        }

        private void WriteBranchHelp(CommandBranchRegistration branch, IReadOnlyList<string> path)
        {
            if (_configurator.HelpProvider != null)
            {
                _configurator.HelpProvider.WriteBranchHelp(
                    Console,
                    new CommandBranchHelpContext(_configurator.Settings, path, branch));
                return;
            }

            string appName = _configurator.Settings.ApplicationName;
            string branchPath = string.Join(" ", path);
            Console.WriteLine(branchPath);
            if (!string.IsNullOrWhiteSpace(branch.Description))
            {
                Console.WriteLine(FormatDescription(branch.Description));
            }

            Console.WriteLine();
            Console.WriteLine("USAGE:");
            Console.WriteLine("    " + appName + " " + branchPath + " [COMMAND] [ARGUMENTS]");

            if (branch.Aliases.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("ALIASES:");
                Console.WriteLine("    " + string.Join(", ", branch.Aliases));
            }

            WriteSettingsHelp(branch.SettingsType);

            Console.WriteLine();
            Console.WriteLine("COMMANDS:");
            bool hasVisibleBranches = branch.Branches.Any(childBranch => !childBranch.Hidden);
            bool hasVisibleCommands = branch.Commands.Any(command => !command.Hidden);
            if (!hasVisibleBranches && !hasVisibleCommands)
            {
                Console.WriteLine("    No commands registered.");
                return;
            }

            foreach (CommandBranchRegistration childBranch in branch.Branches.Where(childBranch => !childBranch.Hidden))
            {
                string aliases = childBranch.Aliases.Count == 0
                    ? string.Empty
                    : " (" + string.Join(", ", childBranch.Aliases) + ")";
                string label = "    " + childBranch.Name + aliases;
                string description = FormatDescription(childBranch.Description);
                Console.WriteLine(PadRight(label, 28) + description);
            }

            foreach (CommandRegistration command in branch.Commands.Where(command => !command.Hidden))
            {
                string aliases = command.Aliases.Count == 0
                    ? string.Empty
                    : " (" + string.Join(", ", command.Aliases) + ")";
                string label = "    " + command.Name + aliases;
                string description = FormatDescription(command.Description);
                Console.WriteLine(PadRight(label, 28) + description);
            }
        }

        private void WriteCommandHelp(CommandRegistration command, IReadOnlyList<string> path)
        {
            WriteCommandHelp(command, path, null);
        }

        private void WriteCommandHelp(CommandRegistration command, IReadOnlyList<string> path, IReadOnlyList<Type> inheritedSettingsTypes)
        {
            if (_configurator.HelpProvider != null)
            {
                _configurator.HelpProvider.WriteCommandHelp(
                    Console,
                    new CommandHelpContext(_configurator.Settings, path, command, inheritedSettingsTypes));
                return;
            }

            string appName = _configurator.Settings.ApplicationName;
            string commandPath = path == null || path.Count == 0 ? command.Name : string.Join(" ", path);
            Console.WriteLine(commandPath);
            if (!string.IsNullOrWhiteSpace(command.Description))
            {
                Console.WriteLine(FormatDescription(command.Description));
            }

            Console.WriteLine();
            Console.WriteLine("USAGE:");
            Console.WriteLine("    " + appName + " " + commandPath + BuildUsageSuffix(command, inheritedSettingsTypes));

            if (command.Aliases.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("ALIASES:");
                Console.WriteLine("    " + string.Join(", ", command.Aliases));
            }

            if (command.Examples.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("EXAMPLES:");
                foreach (var example in command.Examples)
                {
                    Console.WriteLine("    " + appName + " " + string.Join(" ", example));
                }
            }

            WriteSettingsHelp(command.SettingsType);
            WriteInheritedOptionsHelp(command.SettingsType, inheritedSettingsTypes);
        }

        private void WriteSettingsHelp(Type settingsType)
        {
            CommandSettingsDescriptor descriptor = CommandSettingsBinder.Describe(settingsType);
            if (descriptor.Arguments.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("ARGUMENTS:");
                foreach (CommandArgumentDescriptor argument in descriptor.Arguments)
                {
                    Console.WriteLine(PadRight("    " + argument.Template, 28) + BuildDescription(argument.Description, argument.DefaultValue, false, true));
                }
            }

            if (descriptor.Options.Any(option => !option.Attribute.IsHidden))
            {
                WriteOptionsHelp(descriptor.Options.Where(option => !option.Attribute.IsHidden), "OPTIONS:");
            }
        }

        private void WriteInheritedOptionsHelp(Type commandSettingsType, IReadOnlyList<Type> inheritedSettingsTypes)
        {
            IReadOnlyList<CommandOptionDescriptor> options = GetVisibleInheritedOptions(commandSettingsType, inheritedSettingsTypes);
            if (options.Count == 0)
            {
                return;
            }

            WriteOptionsHelp(options, "INHERITED OPTIONS:");
        }

        private void WriteOptionsHelp(IEnumerable<CommandOptionDescriptor> options, string heading)
        {
            CommandOptionDescriptor[] visibleOptions = options.ToArray();
            if (visibleOptions.Length == 0)
            {
                return;
            }

            Console.WriteLine();
            Console.WriteLine(heading);
            foreach (CommandOptionDescriptor option in visibleOptions)
            {
                string valueName = option.IsBoolean && !option.IsFlagValue
                    ? string.Empty
                    : option.Attribute.ValueIsOptional
                        ? " [" + option.ValueName + "]"
                        : " <" + option.ValueName + ">";
                string aliases = string.Join(", ", option.Aliases) + valueName;
                Console.WriteLine(PadRight("    " + aliases, 32) + BuildDescription(option.Description, option.DefaultValue, option.Attribute.IsRequired, !_configurator.Settings.HideOptionDefaultValues));
            }
        }

        private bool TryWritePathHelp(string[] path)
        {
            if (path == null || path.Length == 0)
            {
                return false;
            }

            CommandRegistration command = _configurator.Find(path[0]);
            if (command != null)
            {
                if (path.Length > 1)
                {
                    return false;
                }

                WriteCommandHelp(command, new[] { path[0] });
                return true;
            }

            CommandBranchRegistration branch = _configurator.FindBranch(path[0]);
            if (branch == null)
            {
                return false;
            }

            var branchPath = new List<string> { path[0] };
            IReadOnlyList<Type> inheritedSettingsTypes = AppendSettingsType(new Type[0], branch.SettingsType);
            for (int index = 1; index < path.Length; index++)
            {
                CommandBranchRegistration childBranch = branch.FindBranch(path[index], _configurator.Settings.CaseSensitive);
                if (childBranch != null)
                {
                    branch = childBranch;
                    branchPath.Add(path[index]);
                    inheritedSettingsTypes = AppendSettingsType(inheritedSettingsTypes, branch.SettingsType);
                    continue;
                }

                CommandRegistration childCommand = branch.FindCommand(path[index], _configurator.Settings.CaseSensitive);
                if (childCommand != null)
                {
                    if (index < path.Length - 1)
                    {
                        return false;
                    }

                    WriteCommandHelp(childCommand, branchPath.Concat(new[] { path[index] }).ToArray(), inheritedSettingsTypes);
                    return true;
                }

                return false;
            }

            WriteBranchHelp(branch, branchPath);
            return true;
        }

        private bool TryConsumeBranchOption(CommandBranchRegistration branch, string[] rawArguments, int index, List<string> inheritedArguments, out int nextIndex)
        {
            nextIndex = index;
            if (branch.SettingsType == null || index >= rawArguments.Length)
            {
                return false;
            }

            string token = rawArguments[index] ?? string.Empty;
            if (!LooksLikeOption(token))
            {
                return false;
            }

            CommandOptionDescriptor option = FindOption(branch.SettingsType, token);
            if (option == null)
            {
                if (_configurator.Settings.StrictParsing)
                {
                    CommandSettingsDescriptor descriptor = CommandSettingsBinder.Describe(branch.SettingsType);
                    throw new CommandParameterException(CommandSettingsBinder.BuildUnknownOptionMessage(token, descriptor.Options));
                }

                return false;
            }

            inheritedArguments.Add(token);
            nextIndex = index + 1;
            if (option.IsBoolean || token.IndexOf('=') >= 0)
            {
                return true;
            }

            if (nextIndex >= rawArguments.Length || LooksLikeOption(rawArguments[nextIndex]))
            {
                if (option.Attribute.ValueIsOptional)
                {
                    return true;
                }

                throw new CommandParameterException(CommandSettingsBinder.BuildMissingOptionValueMessage(token, option));
            }

            inheritedArguments.Add(rawArguments[nextIndex]);
            nextIndex++;
            return true;
        }

        private CommandOptionDescriptor FindOption(Type settingsType, string token)
        {
            string optionName = token;
            int equalsIndex = token.IndexOf('=');
            if (equalsIndex >= 0)
            {
                optionName = token.Substring(0, equalsIndex);
            }

            StringComparison comparison = _configurator.Settings.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            CommandSettingsDescriptor descriptor = CommandSettingsBinder.Describe(settingsType);
            return descriptor.Options.FirstOrDefault(option => option.Aliases.Any(alias => string.Equals(alias, optionName, comparison)));
        }

        private static string BuildUsageSuffix(CommandRegistration command, IReadOnlyList<Type> inheritedSettingsTypes)
        {
            CommandSettingsDescriptor descriptor = CommandSettingsBinder.Describe(command.SettingsType);
            string arguments = string.Join(" ", descriptor.Arguments.Select(argument => argument.Template));
            bool hasOptions = descriptor.Options.Count > 0 || GetVisibleInheritedOptions(command.SettingsType, inheritedSettingsTypes).Count > 0;
            string options = hasOptions ? "[OPTIONS]" : string.Empty;
            string suffix = string.Join(" ", new[] { arguments, options }.Where(value => !string.IsNullOrWhiteSpace(value)));
            return string.IsNullOrWhiteSpace(suffix) ? " [ARGUMENTS]" : " " + suffix;
        }

        private static IReadOnlyList<Type> AppendSettingsType(IReadOnlyList<Type> settingsTypes, Type settingsType)
        {
            if (settingsType == null)
            {
                return settingsTypes ?? new Type[0];
            }

            Type[] currentTypes = settingsTypes == null ? new Type[0] : settingsTypes.ToArray();
            if (currentTypes.Contains(settingsType))
            {
                return currentTypes;
            }

            return currentTypes.Concat(new[] { settingsType }).ToArray();
        }

        private static IReadOnlyList<CommandOptionDescriptor> GetVisibleInheritedOptions(Type commandSettingsType, IReadOnlyList<Type> inheritedSettingsTypes)
        {
            if (inheritedSettingsTypes == null || inheritedSettingsTypes.Count == 0)
            {
                return new CommandOptionDescriptor[0];
            }

            var knownAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (CommandOptionDescriptor option in CommandSettingsBinder.Describe(commandSettingsType).Options)
            {
                AddOptionAliases(knownAliases, option);
            }

            var result = new List<CommandOptionDescriptor>();
            foreach (Type settingsType in inheritedSettingsTypes.Where(type => type != null))
            {
                foreach (CommandOptionDescriptor option in CommandSettingsBinder.Describe(settingsType).Options.Where(option => !option.Attribute.IsHidden))
                {
                    if (option.Aliases.Any(alias => knownAliases.Contains(alias)))
                    {
                        continue;
                    }

                    result.Add(option);
                    AddOptionAliases(knownAliases, option);
                }
            }

            return result;
        }

        private static void AddOptionAliases(HashSet<string> aliases, CommandOptionDescriptor option)
        {
            foreach (string alias in option.Aliases)
            {
                aliases.Add(alias);
            }
        }

        private string BuildDescription(string description, object defaultValue, bool required, bool includeDefaultValue)
        {
            string result = FormatDescription(description);
            if (required)
            {
                result = AppendNote(result, "required");
            }

            if (includeDefaultValue && defaultValue != null)
            {
                result = AppendNote(result, "default: " + Convert.ToString(defaultValue, System.Globalization.CultureInfo.InvariantCulture));
            }

            return result;
        }

        private string FormatDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return string.Empty;
            }

            string result = description;
            if (_configurator.Settings.TrimTrailingPeriods)
            {
                result = result.TrimEnd();
                while (result.EndsWith(".", StringComparison.Ordinal))
                {
                    result = result.Substring(0, result.Length - 1).TrimEnd();
                }
            }

            return result;
        }

        private static string AppendNote(string value, string note)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "[" + note + "]";
            }

            return value + " [" + note + "]";
        }

        private static bool IsHelp(string value)
        {
            return string.Equals(value, "help", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "--help", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "-h", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsVersionOption(string value)
        {
            return string.Equals(value, "--version", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "-v", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsVersionCommand(string value)
        {
            return string.Equals(value, "version", StringComparison.OrdinalIgnoreCase);
        }

        private static bool LooksLikeOption(string value)
        {
            return !string.IsNullOrEmpty(value) && value.Length > 1 && value[0] == '-';
        }

        private static string PadRight(string value, int totalWidth)
        {
            value = value ?? string.Empty;
            return value.Length >= totalWidth ? value + "  " : value.PadRight(totalWidth);
        }
    }

    /// <summary>
    /// Provides a command app with a typed default command.
    /// </summary>
    /// <typeparam name="TDefaultCommand">The command type to run when no command name is supplied.</typeparam>
    public class CommandApp<TDefaultCommand> : CommandApp where TDefaultCommand : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandApp{TDefaultCommand}"/> class.
        /// </summary>
        public CommandApp() : base(typeof(TDefaultCommand), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandApp{TDefaultCommand}"/> class.
        /// </summary>
        /// <param name="console">The console facade used for help and errors.</param>
        public CommandApp(IAnsiConsole console) : base(typeof(TDefaultCommand), console)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandApp{TDefaultCommand}"/> class.
        /// </summary>
        /// <param name="registrar">The type registrar used to resolve command and view instances.</param>
        public CommandApp(ITypeRegistrar registrar) : base(typeof(TDefaultCommand), registrar, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandApp{TDefaultCommand}"/> class.
        /// </summary>
        /// <param name="registrar">The type registrar used to resolve command and view instances.</param>
        /// <param name="frontend">The frontend that configures the registrar.</param>
        public CommandApp(ITypeRegistrar registrar, ITypeRegistrarFrontend frontend) : base(typeof(TDefaultCommand), registrar, frontend, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandApp{TDefaultCommand}"/> class.
        /// </summary>
        /// <param name="registrar">The type registrar used to resolve command and view instances.</param>
        /// <param name="console">The console facade used for help and errors.</param>
        public CommandApp(ITypeRegistrar registrar, IAnsiConsole console) : base(typeof(TDefaultCommand), registrar, console)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandApp{TDefaultCommand}"/> class.
        /// </summary>
        /// <param name="registrar">The type registrar used to resolve command and view instances.</param>
        /// <param name="frontend">The frontend that configures the registrar.</param>
        /// <param name="console">The console facade used for help and errors.</param>
        public CommandApp(ITypeRegistrar registrar, ITypeRegistrarFrontend frontend, IAnsiConsole console) : base(typeof(TDefaultCommand), registrar, frontend, console)
        {
        }
    }
}
