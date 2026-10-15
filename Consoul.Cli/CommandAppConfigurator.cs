using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Configures commands, aliases, and help metadata for a <see cref="CommandApp"/>.
    /// </summary>
    public sealed class CommandAppConfigurator : IConfigurator
    {
        private readonly List<CommandRegistration> _commands = new List<CommandRegistration>();
        private readonly List<CommandBranchRegistration> _branches = new List<CommandBranchRegistration>();
        private readonly List<string[]> _examples = new List<string[]>();
        private readonly HashSet<Type> _registeredTypes = new HashSet<Type>();
        private Func<Type, object> _commandActivator = DefaultCreate;
        private ITypeRegistrar _typeRegistrar;
        private ITypeResolver _typeResolver;
        private Func<IAnsiConsole> _consoleFactory = () => AnsiConsole.Console;

        /// <summary>
        /// Gets command app settings.
        /// </summary>
        public CommandAppSettings Settings { get; } = new CommandAppSettings();

        /// <summary>
        /// Gets the registered commands in display order.
        /// </summary>
        public IReadOnlyList<CommandRegistration> Commands => _commands.AsReadOnly();

        /// <summary>
        /// Gets the registered command branches in display order.
        /// </summary>
        public IReadOnlyList<CommandBranchRegistration> Branches => _branches.AsReadOnly();

        /// <summary>
        /// Gets example argument lists shown in top-level help output.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<string>> Examples => _examples.Select(example => (IReadOnlyList<string>)example).ToArray();

        /// <summary>
        /// Gets the command that runs when no command name is supplied.
        /// </summary>
        public CommandRegistration DefaultCommand { get; private set; }

        /// <summary>
        /// Gets the interceptor that runs before and after command execution.
        /// </summary>
        public ICommandInterceptor Interceptor { get; private set; }

        /// <summary>
        /// Gets the custom help provider used to render command app help.
        /// </summary>
        public IHelpProvider HelpProvider { get; private set; }

        /// <summary>
        /// Gets the handler that renders command app exceptions.
        /// </summary>
        public Func<CommandExceptionContext, int> ExceptionHandler { get; private set; }

        /// <summary>
        /// Sets the application name shown in help output.
        /// </summary>
        /// <param name="name">The application name.</param>
        /// <returns>The current <see cref="CommandAppConfigurator"/> instance.</returns>
        public CommandAppConfigurator SetApplicationName(string name)
        {
            Settings.ApplicationName = string.IsNullOrWhiteSpace(name) ? "app" : name;
            return this;
        }

        /// <summary>
        /// Sets the application version shown in help output.
        /// </summary>
        /// <param name="version">The application version.</param>
        /// <returns>The current <see cref="CommandAppConfigurator"/> instance.</returns>
        public CommandAppConfigurator SetApplicationVersion(string version)
        {
            Settings.ApplicationVersion = version;
            return this;
        }

        /// <summary>
        /// Sets the application description shown in help output.
        /// </summary>
        /// <param name="description">The application description.</param>
        /// <returns>The current <see cref="CommandAppConfigurator"/> instance.</returns>
        public CommandAppConfigurator SetApplicationDescription(string description)
        {
            Settings.ApplicationDescription = description ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Rethrows command exceptions instead of rendering them as command errors.
        /// </summary>
        /// <returns>The current <see cref="CommandAppConfigurator"/> instance.</returns>
        public CommandAppConfigurator PropagateExceptions()
        {
            Settings.PropagateExceptions = true;
            return this;
        }

        /// <summary>
        /// Sets the exit code returned when command execution is canceled.
        /// </summary>
        /// <param name="exitCode">The cancellation exit code.</param>
        /// <returns>The current <see cref="CommandAppConfigurator"/> instance.</returns>
        public CommandAppConfigurator CancellationExitCode(int exitCode)
        {
            Settings.CancellationExitCode = exitCode;
            return this;
        }

        /// <summary>
        /// Hides option default values from generated help output.
        /// </summary>
        /// <returns>The current <see cref="CommandAppConfigurator"/> instance.</returns>
        public CommandAppConfigurator HideOptionDefaultValues()
        {
            Settings.HideOptionDefaultValues = true;
            return this;
        }

        /// <summary>
        /// Sets whether generated help descriptions should trim trailing periods.
        /// </summary>
        /// <param name="trim">A value indicating whether trailing periods should be trimmed.</param>
        /// <returns>The current <see cref="CommandAppConfigurator"/> instance.</returns>
        public CommandAppConfigurator TrimTrailingPeriods(bool trim = true)
        {
            Settings.TrimTrailingPeriods = trim;
            return this;
        }

        /// <summary>
        /// Sets whether command and branch lookup should be case-sensitive.
        /// </summary>
        /// <param name="caseSensitive">A value indicating whether command lookup should be case-sensitive.</param>
        /// <returns>The current <see cref="CommandAppConfigurator"/> instance.</returns>
        public CommandAppConfigurator SetCaseSensitive(bool caseSensitive = true)
        {
            Settings.CaseSensitive = caseSensitive;
            return this;
        }

        /// <summary>
        /// Sets whether unknown options should be rejected during settings binding.
        /// </summary>
        /// <param name="strictParsing">A value indicating whether unknown options should be rejected.</param>
        /// <returns>The current <see cref="CommandAppConfigurator"/> instance.</returns>
        public CommandAppConfigurator SetStrictParsing(bool strictParsing = true)
        {
            Settings.StrictParsing = strictParsing;
            return this;
        }

        /// <summary>
        /// Allows unknown option-shaped tokens to flow through as positional arguments.
        /// </summary>
        /// <returns>The current <see cref="CommandAppConfigurator"/> instance.</returns>
        public CommandAppConfigurator IgnoreUnknownOptions()
        {
            Settings.StrictParsing = false;
            return this;
        }

        /// <summary>
        /// Validates registered command examples before commands run.
        /// </summary>
        /// <returns>The current <see cref="CommandAppConfigurator"/> instance.</returns>
        public CommandAppConfigurator ValidateExamples()
        {
            Settings.ValidateExamples = true;
            return this;
        }

        /// <summary>
        /// Adds an example shown in top-level command app help output.
        /// </summary>
        /// <param name="arguments">The example arguments.</param>
        /// <returns>The current <see cref="CommandAppConfigurator"/> instance.</returns>
        public CommandAppConfigurator AddExample(params string[] arguments)
        {
            _examples.Add((arguments ?? new string[0]).ToArray());
            return this;
        }

        /// <summary>
        /// Adds examples shown in top-level command app help output.
        /// </summary>
        /// <param name="examples">The example argument lists.</param>
        /// <returns>The current <see cref="CommandAppConfigurator"/> instance.</returns>
        public CommandAppConfigurator AddExamples(params string[][] examples)
        {
            if (examples == null)
            {
                throw new ArgumentNullException(nameof(examples));
            }

            foreach (string[] example in examples)
            {
                AddExample(example);
            }

            return this;
        }

        /// <summary>
        /// Sets the interceptor that runs before and after command execution.
        /// </summary>
        /// <param name="interceptor">The interceptor to use.</param>
        /// <returns>The current <see cref="CommandAppConfigurator"/> instance.</returns>
        public CommandAppConfigurator SetInterceptor(ICommandInterceptor interceptor)
        {
            Interceptor = interceptor ?? throw new ArgumentNullException(nameof(interceptor));
            return this;
        }

        /// <summary>
        /// Sets the provider used to render command app help.
        /// </summary>
        /// <param name="provider">The help provider to use.</param>
        /// <returns>The current <see cref="CommandAppConfigurator"/> instance.</returns>
        public CommandAppConfigurator SetHelpProvider(IHelpProvider provider)
        {
            HelpProvider = provider ?? throw new ArgumentNullException(nameof(provider));
            return this;
        }

        /// <summary>
        /// Sets the handler that renders command app exceptions.
        /// </summary>
        /// <param name="handler">The exception handler to use.</param>
        /// <returns>The current <see cref="CommandAppConfigurator"/> instance.</returns>
        public CommandAppConfigurator SetExceptionHandler(ICommandExceptionHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            return SetExceptionHandler(handler.Handle);
        }

        /// <summary>
        /// Sets the handler that renders command app exceptions.
        /// </summary>
        /// <param name="handler">The exception handler delegate to use.</param>
        /// <returns>The current <see cref="CommandAppConfigurator"/> instance.</returns>
        public CommandAppConfigurator SetExceptionHandler(Func<CommandExceptionContext, int> handler)
        {
            ExceptionHandler = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <summary>
        /// Sets the activator used to create command and view instances.
        /// </summary>
        /// <param name="activator">The command activator.</param>
        /// <returns>The current <see cref="CommandAppConfigurator"/> instance.</returns>
        public CommandAppConfigurator SetCommandActivator(ICommandActivator activator)
        {
            if (activator == null)
            {
                throw new ArgumentNullException(nameof(activator));
            }

            return SetCommandActivator(activator.Create);
        }

        /// <summary>
        /// Sets the activator used to create command and view instances.
        /// </summary>
        /// <param name="activator">The activator delegate.</param>
        /// <returns>The current <see cref="CommandAppConfigurator"/> instance.</returns>
        public CommandAppConfigurator SetCommandActivator(Func<Type, object> activator)
        {
            _commandActivator = activator ?? throw new ArgumentNullException(nameof(activator));
            return this;
        }

        /// <summary>
        /// Sets the type registrar used to create command and MVC-style view instances.
        /// </summary>
        /// <param name="registrar">The type registrar.</param>
        /// <returns>The current <see cref="CommandAppConfigurator"/> instance.</returns>
        public CommandAppConfigurator SetTypeRegistrar(ITypeRegistrar registrar)
        {
            return SetTypeRegistrar(registrar, () => AnsiConsole.Console);
        }

        /// <summary>
        /// Sets the type registrar used to create command and MVC-style view instances, then configures it with a frontend.
        /// </summary>
        /// <param name="registrar">The type registrar.</param>
        /// <param name="frontend">The frontend that configures the registrar.</param>
        /// <returns>The current <see cref="CommandAppConfigurator"/> instance.</returns>
        public CommandAppConfigurator SetTypeRegistrar(ITypeRegistrar registrar, ITypeRegistrarFrontend frontend)
        {
            return SetTypeRegistrar(registrar, frontend, () => AnsiConsole.Console);
        }

        /// <summary>
        /// Sets the command that runs when no command name is supplied.
        /// </summary>
        /// <typeparam name="TCommand">The command implementation type.</typeparam>
        /// <returns>The default command registration.</returns>
        public CommandRegistration SetDefaultCommand<TCommand>() where TCommand : class
        {
            return SetDefaultCommand(typeof(TCommand), () => CreateCommand(typeof(TCommand)));
        }

        /// <summary>
        /// Sets the command that runs when no command name is supplied.
        /// </summary>
        /// <typeparam name="TCommand">The command implementation type.</typeparam>
        /// <param name="factory">The command factory.</param>
        /// <returns>The default command registration.</returns>
        public CommandRegistration SetDefaultCommand<TCommand>(Func<TCommand> factory) where TCommand : class
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            return SetDefaultCommand(typeof(TCommand), () => factory());
        }

        /// <summary>
        /// Adds a typed command.
        /// </summary>
        /// <typeparam name="TCommand">The command implementation type.</typeparam>
        /// <param name="name">The command name.</param>
        /// <returns>The command registration.</returns>
        public CommandRegistration AddCommand<TCommand>(string name) where TCommand : class
        {
            return AddCommand(name, typeof(TCommand), () => CreateCommand(typeof(TCommand)));
        }

        /// <summary>
        /// Adds a typed command with an explicit factory.
        /// </summary>
        /// <typeparam name="TCommand">The command implementation type.</typeparam>
        /// <param name="name">The command name.</param>
        /// <param name="factory">The command factory.</param>
        /// <returns>The command registration.</returns>
        public CommandRegistration AddCommand<TCommand>(string name, Func<TCommand> factory) where TCommand : class
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            return AddCommand(name, typeof(TCommand), () => factory());
        }

        /// <summary>
        /// Adds a command that renders an MVC-style view.
        /// </summary>
        /// <typeparam name="TView">The view type to render.</typeparam>
        /// <param name="name">The command name.</param>
        /// <returns>The command registration.</returns>
        public CommandRegistration AddView<TView>(string name) where TView : IView
        {
            RegisterCommandType(typeof(TView));
            string description = CommandDescriptionMetadata.FromType(typeof(TView));
            return AddCommand(name, typeof(ViewCommand<TView>), () => new ViewCommand<TView>(() => CreateView<TView>()))
                .WithDescription(string.IsNullOrWhiteSpace(description) ? "Open the " + typeof(TView).Name + " view" : description);
        }

        /// <summary>
        /// Adds a delegate command for quick proof-of-concept command apps.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        public CommandRegistration AddCommand(string name, Action<CommandContext> execute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            return AddCommand(name, typeof(DelegateCommand), () => DelegateCommand.FromAction(execute));
        }

        /// <summary>
        /// Adds a delegate command for quick proof-of-concept command apps.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        public CommandRegistration AddCommand(string name, Func<CommandContext, int> execute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            return AddCommand(name, typeof(DelegateCommand), () => DelegateCommand.FromFunc(execute));
        }

        /// <summary>
        /// Adds a Spectre-style delegate command for quick proof-of-concept command apps.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        public CommandRegistration AddDelegate(string name, Func<CommandContext, System.Threading.CancellationToken, int> execute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            return AddCommand(name, (context, cancellationToken) => System.Threading.Tasks.Task.FromResult(execute(context, cancellationToken)));
        }

        /// <summary>
        /// Adds an asynchronous delegate command for quick proof-of-concept command apps.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        public CommandRegistration AddCommand(string name, Func<CommandContext, System.Threading.CancellationToken, System.Threading.Tasks.Task<int>> execute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            return AddCommand(name, typeof(DelegateCommand), () => DelegateCommand.FromAsyncFunc(execute));
        }

        /// <summary>
        /// Adds a Spectre-style asynchronous delegate command for quick proof-of-concept command apps.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        public CommandRegistration AddAsyncDelegate(string name, Func<CommandContext, System.Threading.CancellationToken, System.Threading.Tasks.Task<int>> execute)
        {
            return AddCommand(name, execute);
        }

        /// <summary>
        /// Adds a delegate command with strongly-typed settings for quick proof-of-concept command apps.
        /// </summary>
        /// <typeparam name="TSettings">The command settings type.</typeparam>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        public CommandRegistration AddCommand<TSettings>(string name, Action<CommandContext, TSettings> execute) where TSettings : CommandSettings
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            return AddCommand(name, typeof(DelegateCommand<TSettings>), () => DelegateCommand<TSettings>.FromAction(execute));
        }

        /// <summary>
        /// Adds a delegate command with strongly-typed settings for quick proof-of-concept command apps.
        /// </summary>
        /// <typeparam name="TSettings">The command settings type.</typeparam>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        public CommandRegistration AddCommand<TSettings>(string name, Func<CommandContext, TSettings, int> execute) where TSettings : CommandSettings
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            return AddCommand(name, typeof(DelegateCommand<TSettings>), () => DelegateCommand<TSettings>.FromFunc(execute));
        }

        /// <summary>
        /// Adds a Spectre-style delegate command with strongly-typed settings for quick proof-of-concept command apps.
        /// </summary>
        /// <typeparam name="TSettings">The command settings type.</typeparam>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        public CommandRegistration AddDelegate<TSettings>(string name, Func<CommandContext, TSettings, System.Threading.CancellationToken, int> execute) where TSettings : CommandSettings
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            return AddCommand<TSettings>(name, (context, settings, cancellationToken) => System.Threading.Tasks.Task.FromResult(execute(context, settings, cancellationToken)));
        }

        /// <summary>
        /// Adds an asynchronous delegate command with strongly-typed settings for quick proof-of-concept command apps.
        /// </summary>
        /// <typeparam name="TSettings">The command settings type.</typeparam>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        public CommandRegistration AddCommand<TSettings>(string name, Func<CommandContext, TSettings, System.Threading.CancellationToken, System.Threading.Tasks.Task<int>> execute) where TSettings : CommandSettings
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            return AddCommand(name, typeof(DelegateCommand<TSettings>), () => DelegateCommand<TSettings>.FromAsyncFunc(execute));
        }

        /// <summary>
        /// Adds a Spectre-style asynchronous delegate command with strongly-typed settings for quick proof-of-concept command apps.
        /// </summary>
        /// <typeparam name="TSettings">The command settings type.</typeparam>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        public CommandRegistration AddAsyncDelegate<TSettings>(string name, Func<CommandContext, TSettings, System.Threading.CancellationToken, System.Threading.Tasks.Task<int>> execute) where TSettings : CommandSettings
        {
            return AddCommand<TSettings>(name, execute);
        }

        /// <summary>
        /// Adds a branch of related subcommands.
        /// </summary>
        /// <param name="name">The branch name.</param>
        /// <param name="configure">The branch configuration callback.</param>
        /// <returns>The branch registration.</returns>
        public CommandBranchRegistration AddBranch(string name, Action<CommandBranchRegistration> configure)
        {
            return AddBranch(name, null, configure);
        }

        /// <summary>
        /// Adds a branch of related subcommands with shared settings.
        /// </summary>
        /// <typeparam name="TSettings">The branch settings type.</typeparam>
        /// <param name="name">The branch name.</param>
        /// <param name="configure">The branch configuration callback.</param>
        /// <returns>The branch registration.</returns>
        public CommandBranchRegistration AddBranch<TSettings>(string name, Action<CommandBranchRegistration> configure) where TSettings : CommandSettings
        {
            return AddBranch(name, typeof(TSettings), configure);
        }

        internal CommandRegistration Find(string value)
        {
            return _commands.FirstOrDefault(command => command.Matches(value, Settings.CaseSensitive));
        }

        internal CommandBranchRegistration FindBranch(string value)
        {
            StringComparison comparison = Settings.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            return _branches.FirstOrDefault(branch => branch.Matches(value, comparison));
        }

        internal void ValidateRegisteredExamples(CommandRegistration defaultCommand = null)
        {
            foreach (CommandRegistration command in _commands)
            {
                ValidateCommandExamples(command, new[] { command.Name });
            }

            foreach (CommandBranchRegistration branch in _branches)
            {
                ValidateBranchExamples(branch, new[] { branch.Name });
            }

            if (defaultCommand != null)
            {
                ValidateDefaultCommandExamples(defaultCommand);
            }

            foreach (IReadOnlyList<string> example in _examples)
            {
                ValidateApplicationExample(example == null ? new string[0] : example.ToArray(), defaultCommand);
            }
        }

        internal CommandAppConfigurator SetTypeRegistrar(ITypeRegistrar registrar, Func<IAnsiConsole> consoleFactory)
        {
            _typeRegistrar = registrar ?? throw new ArgumentNullException(nameof(registrar));
            _consoleFactory = consoleFactory ?? throw new ArgumentNullException(nameof(consoleFactory));
            _typeResolver = null;
            _registeredTypes.Clear();
            _commandActivator = ResolveFromTypeRegistrar;

            _typeRegistrar.RegisterInstance(typeof(IAnsiConsole), _consoleFactory());
            RegisterExistingTypes();
            return this;
        }

        internal CommandAppConfigurator SetTypeRegistrar(ITypeRegistrar registrar, ITypeRegistrarFrontend frontend, Func<IAnsiConsole> consoleFactory)
        {
            if (registrar == null)
            {
                throw new ArgumentNullException(nameof(registrar));
            }

            if (frontend == null)
            {
                throw new ArgumentNullException(nameof(frontend));
            }

            frontend.Configure(registrar);
            return SetTypeRegistrar(registrar, consoleFactory);
        }

        internal void RegisterCommandSettings(CommandSettings settings)
        {
            if (_typeRegistrar == null || settings == null)
            {
                return;
            }

            _typeRegistrar.RegisterInstance(settings.GetType(), settings);
            ResetTypeResolver();
        }

        internal void RegisterCommandType(Type commandType)
        {
            if (_typeRegistrar == null || commandType == null || _registeredTypes.Contains(commandType))
            {
                return;
            }

            _typeRegistrar.Register(commandType, commandType);
            _registeredTypes.Add(commandType);
            ResetTypeResolver();
        }

        internal CommandRegistration AddCommandUnsafe(string name, Type commandType)
        {
            return AddCommand(name, commandType, () => CreateCommand(commandType));
        }

        internal CommandBranchRegistration AddBranchUnsafe(string name, Type settingsType, Action<CommandBranchRegistration> configure)
        {
            return AddBranch(name, settingsType, configure);
        }

        internal CommandRegistration SetDefaultCommandUnsafe(Type commandType)
        {
            return SetDefaultCommand(commandType, () => CreateCommand(commandType));
        }

        private CommandRegistration AddCommand(string name, Type commandType, Func<object> factory)
        {
            if (Find(name) != null || FindBranch(name) != null)
            {
                throw new CommandConfigurationException("A command or branch named '" + name + "' has already been registered.");
            }

            RegisterCommandType(commandType);
            var registration = new CommandRegistration(name, commandType, factory);
            _commands.Add(registration);
            return registration;
        }

        private CommandBranchRegistration AddBranch(string name, Type settingsType, Action<CommandBranchRegistration> configure)
        {
            if (Find(name) != null || FindBranch(name) != null)
            {
                throw new CommandConfigurationException("A command or branch named '" + name + "' has already been registered.");
            }

            var branch = new CommandBranchRegistration(name, settingsType, CreateCommand, RegisterCommandType);
            _branches.Add(branch);
            configure?.Invoke(branch);
            return branch;
        }

        private CommandRegistration SetDefaultCommand(Type commandType, Func<object> factory)
        {
            RegisterCommandType(commandType);
            DefaultCommand = new CommandRegistration("__default", commandType, factory);
            return DefaultCommand;
        }

        internal object CreateCommand(Type commandType)
        {
            if (commandType == null)
            {
                throw new ArgumentNullException(nameof(commandType));
            }

            object command = _commandActivator(commandType);
            if (command == null)
            {
                throw new CommandRuntimeException("Command activator returned null for '" + commandType.FullName + "'.");
            }

            if (!commandType.IsInstanceOfType(command))
            {
                throw new CommandRuntimeException("Command activator for '" + commandType.FullName + "' returned '" + command.GetType().FullName + "'.");
            }

            return command;
        }

        private TView CreateView<TView>() where TView : IView
        {
            return (TView)CreateCommand(typeof(TView));
        }

        private object ResolveFromTypeRegistrar(Type commandType)
        {
            if (_typeResolver == null)
            {
                _typeResolver = _typeRegistrar.Build();
                if (_typeResolver == null)
                {
                    throw new CommandRuntimeException("Type registrar returned null from Build().");
                }
            }

            object command = _typeResolver.Resolve(commandType);
            return command ?? DefaultCreate(commandType);
        }

        private void RegisterExistingTypes()
        {
            foreach (CommandRegistration command in _commands)
            {
                RegisterCommandType(command.CommandType);
            }

            if (DefaultCommand != null)
            {
                RegisterCommandType(DefaultCommand.CommandType);
            }

            foreach (CommandBranchRegistration branch in _branches)
            {
                RegisterBranchTypes(branch);
            }
        }

        private void RegisterBranchTypes(CommandBranchRegistration branch)
        {
            foreach (CommandRegistration command in branch.Commands)
            {
                RegisterCommandType(command.CommandType);
            }

            foreach (CommandBranchRegistration childBranch in branch.Branches)
            {
                RegisterBranchTypes(childBranch);
            }
        }

        private void ResetTypeResolver()
        {
            _typeResolver = null;
        }

        private static object DefaultCreate(Type commandType)
        {
            return Activator.CreateInstance(commandType);
        }

        private void ValidateBranchExamples(CommandBranchRegistration branch, IReadOnlyList<string> path)
        {
            foreach (CommandRegistration command in branch.Commands)
            {
                ValidateCommandExamples(command, path.Concat(new[] { command.Name }).ToArray());
            }

            foreach (CommandBranchRegistration childBranch in branch.Branches)
            {
                ValidateBranchExamples(childBranch, path.Concat(new[] { childBranch.Name }).ToArray());
            }
        }

        private void ValidateCommandExamples(CommandRegistration command, IReadOnlyList<string> path)
        {
            foreach (IReadOnlyList<string> example in command.Examples)
            {
                ValidateCommandExample(command, path, example == null ? new string[0] : example.ToArray());
            }
        }

        private void ValidateCommandExample(CommandRegistration expectedCommand, IReadOnlyList<string> path, string[] example)
        {
            ExampleResolution resolution;
            string error;
            if (!TryResolveExample(example, out resolution, out error) || !ReferenceEquals(resolution.Command, expectedCommand))
            {
                string[] prefixedExample = path.Concat(example ?? new string[0]).ToArray();
                if (!TryResolveExample(prefixedExample, out resolution, out error) || !ReferenceEquals(resolution.Command, expectedCommand))
                {
                    string message = ReferenceEquals(resolution.Command, null)
                        ? error
                        : "Routes to command '" + resolution.Command.Name + "' instead.";
                    throw CreateInvalidExampleException(path, example, message);
                }
            }

            try
            {
                ValidateBoundSettings(expectedCommand, resolution.Arguments);
            }
            catch (Exception ex)
            {
                throw CreateInvalidExampleException(path, example, ex.Message, ex);
            }
        }

        private void ValidateDefaultCommandExamples(CommandRegistration defaultCommand)
        {
            foreach (IReadOnlyList<string> example in defaultCommand.Examples)
            {
                ValidateDefaultCommandExample(defaultCommand, example == null ? new string[0] : example.ToArray());
            }
        }

        private void ValidateDefaultCommandExample(CommandRegistration defaultCommand, string[] example)
        {
            try
            {
                ValidateBoundSettings(defaultCommand, example);
            }
            catch (Exception ex)
            {
                throw CreateInvalidDefaultCommandExampleException(example, ex.Message, ex);
            }
        }

        private void ValidateApplicationExample(string[] example, CommandRegistration defaultCommand)
        {
            ExampleResolution resolution;
            string error;
            if (TryResolveExample(example, out resolution, out error))
            {
                try
                {
                    ValidateBoundSettings(resolution.Command, resolution.Arguments);
                    return;
                }
                catch (Exception ex)
                {
                    throw CreateInvalidApplicationExampleException(example, ex.Message, ex);
                }
            }

            if (defaultCommand != null)
            {
                try
                {
                    ValidateBoundSettings(defaultCommand, example);
                    return;
                }
                catch (Exception ex)
                {
                    throw CreateInvalidApplicationExampleException(example, ex.Message, ex);
                }
            }

            throw CreateInvalidApplicationExampleException(example, error);
        }

        private void ValidateBoundSettings(CommandRegistration command, string[] arguments)
        {
            if (command.SettingsType == null)
            {
                return;
            }

            CommandSettings settings = CommandSettingsBinder.Bind(command.SettingsType, arguments, Settings);
            CommandValidationResult validation = settings.Validate();
            if (validation != null && !validation.Successful)
            {
                throw new CommandParameterException(validation.Message);
            }
        }

        private bool TryResolveExample(string[] example, out ExampleResolution resolution, out string error)
        {
            resolution = default(ExampleResolution);
            error = null;
            if (example == null || example.Length == 0)
            {
                error = "Example does not include a command name.";
                return false;
            }

            string commandName = example[0] ?? string.Empty;
            CommandRegistration command = Find(commandName);
            if (command != null)
            {
                resolution = new ExampleResolution(command, example.Skip(1).ToArray());
                return true;
            }

            CommandBranchRegistration branch = FindBranch(commandName);
            if (branch != null)
            {
                return TryResolveBranchExample(branch, new[] { commandName }, example, 1, new List<string>(), out resolution, out error);
            }

            error = "Unknown command: " + commandName;
            return false;
        }

        private bool TryResolveBranchExample(
            CommandBranchRegistration branch,
            IReadOnlyList<string> path,
            string[] example,
            int index,
            List<string> inheritedArguments,
            out ExampleResolution resolution,
            out string error)
        {
            resolution = default(ExampleResolution);
            error = null;

            while (index < example.Length)
            {
                int nextIndex;
                string optionError;
                if (!TryConsumeBranchOption(branch, example, index, inheritedArguments, out nextIndex, out optionError))
                {
                    if (!string.IsNullOrWhiteSpace(optionError))
                    {
                        error = optionError;
                        return false;
                    }

                    break;
                }

                index = nextIndex;
            }

            if (index >= example.Length)
            {
                error = "Example stops at branch '" + string.Join(" ", path) + "' without a command.";
                return false;
            }

            string commandName = example[index] ?? string.Empty;
            CommandBranchRegistration childBranch = branch.FindBranch(commandName, Settings.CaseSensitive);
            if (childBranch != null)
            {
                return TryResolveBranchExample(
                    childBranch,
                    path.Concat(new[] { commandName }).ToArray(),
                    example,
                    index + 1,
                    inheritedArguments,
                    out resolution,
                    out error);
            }

            CommandRegistration command = branch.FindCommand(commandName, Settings.CaseSensitive);
            if (command == null)
            {
                error = "Unknown command: " + string.Join(" ", path.Concat(new[] { commandName }));
                return false;
            }

            resolution = new ExampleResolution(command, inheritedArguments.Concat(example.Skip(index + 1)).ToArray());
            return true;
        }

        private bool TryConsumeBranchOption(
            CommandBranchRegistration branch,
            string[] example,
            int index,
            List<string> inheritedArguments,
            out int nextIndex,
            out string error)
        {
            nextIndex = index;
            error = null;
            if (branch.SettingsType == null || index >= example.Length)
            {
                return false;
            }

            string token = example[index] ?? string.Empty;
            if (!LooksLikeOption(token))
            {
                return false;
            }

            CommandOptionDescriptor option = FindBranchOption(branch.SettingsType, token);
            if (option == null)
            {
                return false;
            }

            inheritedArguments.Add(token);
            nextIndex = index + 1;
            if (option.IsBoolean || token.IndexOf('=') >= 0)
            {
                return true;
            }

            if (nextIndex >= example.Length || LooksLikeOption(example[nextIndex]))
            {
                if (option.Attribute.ValueIsOptional)
                {
                    return true;
                }

                error = CommandSettingsBinder.BuildMissingOptionValueMessage(token, option);
                return false;
            }

            inheritedArguments.Add(example[nextIndex]);
            nextIndex++;
            return true;
        }

        private CommandOptionDescriptor FindBranchOption(Type settingsType, string token)
        {
            string optionName = token;
            int equalsIndex = token.IndexOf('=');
            if (equalsIndex >= 0)
            {
                optionName = token.Substring(0, equalsIndex);
            }

            StringComparison comparison = Settings.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            CommandSettingsDescriptor descriptor = CommandSettingsBinder.Describe(settingsType);
            return descriptor.Options.FirstOrDefault(option => option.Aliases.Any(alias => string.Equals(alias, optionName, comparison)));
        }

        private static CommandConfigurationException CreateInvalidExampleException(IReadOnlyList<string> path, string[] example, string message, Exception innerException = null)
        {
            string commandPath = string.Join(" ", path ?? new string[0]);
            string formattedExample = FormatExample(example, commandPath);
            return new CommandConfigurationException(
                "Example for command '" + commandPath + "' is invalid: " + formattedExample + ". " + message,
                innerException);
        }

        private static CommandConfigurationException CreateInvalidDefaultCommandExampleException(string[] example, string message, Exception innerException = null)
        {
            return new CommandConfigurationException(
                "Example for default command is invalid: " + FormatExample(example, "<empty>") + ". " + message,
                innerException);
        }

        private static CommandConfigurationException CreateInvalidApplicationExampleException(string[] example, string message, Exception innerException = null)
        {
            return new CommandConfigurationException(
                "Application example is invalid: " + FormatExample(example, "<empty>") + ". " + message,
                innerException);
        }

        private static string FormatExample(string[] example, string fallback)
        {
            return example == null || example.Length == 0
                ? fallback
                : string.Join(" ", example);
        }

        private static bool LooksLikeOption(string value)
        {
            return !string.IsNullOrEmpty(value) && value.Length > 1 && value[0] == '-';
        }

        private readonly struct ExampleResolution
        {
            public ExampleResolution(CommandRegistration command, string[] arguments)
            {
                Command = command;
                Arguments = arguments ?? new string[0];
            }

            public CommandRegistration Command { get; }

            public string[] Arguments { get; }
        }
    }
}
