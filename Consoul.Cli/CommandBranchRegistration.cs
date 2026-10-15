using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a branch of related commands in a <see cref="CommandApp"/>.
    /// </summary>
    public sealed class CommandBranchRegistration : IBranchConfigurator, IConfigurator<CommandSettings>
    {
        private readonly List<string> _aliases = new List<string>();
        private readonly List<CommandRegistration> _commands = new List<CommandRegistration>();
        private readonly List<CommandBranchRegistration> _branches = new List<CommandBranchRegistration>();
        private readonly Func<Type, object> _commandActivator;
        private readonly Action<Type> _registerCommandType;

        internal CommandBranchRegistration(string name, Type settingsType = null, Func<Type, object> commandActivator = null, Action<Type> registerCommandType = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Branch name is required.", nameof(name));
            }

            if (settingsType != null && !typeof(CommandSettings).IsAssignableFrom(settingsType))
            {
                throw new ArgumentException("Branch settings must inherit from CommandSettings.", nameof(settingsType));
            }

            Name = name;
            SettingsType = settingsType;
            Description = CommandDescriptionMetadata.FromType(SettingsType);
            _commandActivator = commandActivator ?? Activator.CreateInstance;
            _registerCommandType = registerCommandType ?? (_ => { });
        }

        /// <summary>
        /// Gets the branch name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the branch settings type when shared branch options are configured.
        /// </summary>
        public Type SettingsType { get; }

        /// <summary>
        /// Gets the branch description shown in help output.
        /// </summary>
        public string Description { get; private set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the branch is hidden from command listings.
        /// </summary>
        public bool Hidden { get; private set; }

        /// <summary>
        /// Gets aliases that can invoke the branch.
        /// </summary>
        public IReadOnlyList<string> Aliases => _aliases.AsReadOnly();

        /// <summary>
        /// Gets child commands registered under the branch.
        /// </summary>
        public IReadOnlyList<CommandRegistration> Commands => _commands.AsReadOnly();

        /// <summary>
        /// Gets nested branches registered under the branch.
        /// </summary>
        public IReadOnlyList<CommandBranchRegistration> Branches => _branches.AsReadOnly();

        /// <summary>
        /// Sets the branch description shown in help output.
        /// </summary>
        /// <param name="description">The branch description.</param>
        /// <returns>The current <see cref="CommandBranchRegistration"/> instance.</returns>
        public CommandBranchRegistration SetDescription(string description)
        {
            Description = description ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the branch description shown in help output.
        /// </summary>
        /// <param name="description">The branch description.</param>
        /// <returns>The current <see cref="CommandBranchRegistration"/> instance.</returns>
        public CommandBranchRegistration WithDescription(string description)
        {
            return SetDescription(description);
        }

        /// <summary>
        /// Adds an alias that can invoke the branch.
        /// </summary>
        /// <param name="alias">The alias to add.</param>
        /// <returns>The current <see cref="CommandBranchRegistration"/> instance.</returns>
        public CommandBranchRegistration WithAlias(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                throw new ArgumentException("Branch alias is required.", nameof(alias));
            }

            if (!_aliases.Contains(alias))
            {
                _aliases.Add(alias);
            }

            return this;
        }

        /// <summary>
        /// Adds aliases that can invoke the branch.
        /// </summary>
        /// <param name="aliases">The aliases to add.</param>
        /// <returns>The current <see cref="CommandBranchRegistration"/> instance.</returns>
        public CommandBranchRegistration WithAliases(params string[] aliases)
        {
            if (aliases == null)
            {
                throw new ArgumentNullException(nameof(aliases));
            }

            foreach (string alias in aliases)
            {
                WithAlias(alias);
            }

            return this;
        }

        /// <summary>
        /// Hides or shows the branch in command listings while keeping it invokable.
        /// </summary>
        /// <param name="isHidden">A value indicating whether the branch should be hidden.</param>
        /// <returns>The current <see cref="CommandBranchRegistration"/> instance.</returns>
        public CommandBranchRegistration WithHidden(bool isHidden = true)
        {
            Hidden = isHidden;
            return this;
        }

        /// <summary>
        /// Hides the branch in command listings while keeping it invokable.
        /// </summary>
        /// <returns>The current <see cref="CommandBranchRegistration"/> instance.</returns>
        public CommandBranchRegistration IsHiddenBranch()
        {
            return WithHidden();
        }

        /// <summary>
        /// Hides the branch in command listings while keeping it invokable.
        /// </summary>
        /// <returns>The current <see cref="CommandBranchRegistration"/> instance.</returns>
        public CommandBranchRegistration IsHidden()
        {
            return WithHidden();
        }

        /// <summary>
        /// Adds a typed command to the branch.
        /// </summary>
        /// <typeparam name="TCommand">The command implementation type.</typeparam>
        /// <param name="name">The command name.</param>
        /// <returns>The command registration.</returns>
        public CommandRegistration AddCommand<TCommand>(string name) where TCommand : class
        {
            return AddCommand(name, typeof(TCommand), () => CreateCommand(typeof(TCommand)));
        }

        /// <summary>
        /// Adds a typed command with an explicit factory to the branch.
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
        /// Adds a delegate command to the branch.
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
        /// Adds a delegate command to the branch.
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
        /// Adds a Spectre-style delegate command to the branch.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        public CommandRegistration AddDelegate(string name, Func<CommandContext, CancellationToken, int> execute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            return AddCommand(name, (context, cancellationToken) => Task.FromResult(execute(context, cancellationToken)));
        }

        /// <summary>
        /// Adds an asynchronous delegate command to the branch.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        public CommandRegistration AddCommand(string name, Func<CommandContext, CancellationToken, Task<int>> execute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            return AddCommand(name, typeof(DelegateCommand), () => DelegateCommand.FromAsyncFunc(execute));
        }

        /// <summary>
        /// Adds a Spectre-style asynchronous delegate command to the branch.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        public CommandRegistration AddAsyncDelegate(string name, Func<CommandContext, CancellationToken, Task<int>> execute)
        {
            return AddCommand(name, execute);
        }

        /// <summary>
        /// Adds a delegate command with strongly-typed settings to the branch.
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
        /// Adds a delegate command with strongly-typed settings to the branch.
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
        /// Adds a Spectre-style delegate command with strongly-typed settings to the branch.
        /// </summary>
        /// <typeparam name="TSettings">The command settings type.</typeparam>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        public CommandRegistration AddDelegate<TSettings>(string name, Func<CommandContext, TSettings, CancellationToken, int> execute) where TSettings : CommandSettings
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            return AddCommand<TSettings>(name, (context, settings, cancellationToken) => Task.FromResult(execute(context, settings, cancellationToken)));
        }

        /// <summary>
        /// Adds an asynchronous delegate command with strongly-typed settings to the branch.
        /// </summary>
        /// <typeparam name="TSettings">The command settings type.</typeparam>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        public CommandRegistration AddCommand<TSettings>(string name, Func<CommandContext, TSettings, CancellationToken, Task<int>> execute) where TSettings : CommandSettings
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            return AddCommand(name, typeof(DelegateCommand<TSettings>), () => DelegateCommand<TSettings>.FromAsyncFunc(execute));
        }

        /// <summary>
        /// Adds a Spectre-style asynchronous delegate command with strongly-typed settings to the branch.
        /// </summary>
        /// <typeparam name="TSettings">The command settings type.</typeparam>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        public CommandRegistration AddAsyncDelegate<TSettings>(string name, Func<CommandContext, TSettings, CancellationToken, Task<int>> execute) where TSettings : CommandSettings
        {
            return AddCommand<TSettings>(name, execute);
        }

        /// <summary>
        /// Adds a command that renders an MVC-style view.
        /// </summary>
        /// <typeparam name="TView">The view type to render.</typeparam>
        /// <param name="name">The command name.</param>
        /// <returns>The command registration.</returns>
        public CommandRegistration AddView<TView>(string name) where TView : IView
        {
            _registerCommandType(typeof(TView));
            string description = CommandDescriptionMetadata.FromType(typeof(TView));
            return AddCommand(name, typeof(ViewCommand<TView>), () => new ViewCommand<TView>(() => CreateView<TView>()))
                .WithDescription(string.IsNullOrWhiteSpace(description) ? "Open the " + typeof(TView).Name + " view" : description);
        }

        /// <summary>
        /// Adds a nested branch.
        /// </summary>
        /// <param name="name">The branch name.</param>
        /// <param name="configure">The branch configuration callback.</param>
        /// <returns>The branch registration.</returns>
        public CommandBranchRegistration AddBranch(string name, Action<CommandBranchRegistration> configure)
        {
            return AddBranch(name, null, configure);
        }

        /// <summary>
        /// Adds a nested branch with shared settings.
        /// </summary>
        /// <typeparam name="TSettings">The branch settings type.</typeparam>
        /// <param name="name">The branch name.</param>
        /// <param name="configure">The branch configuration callback.</param>
        /// <returns>The branch registration.</returns>
        public CommandBranchRegistration AddBranch<TSettings>(string name, Action<CommandBranchRegistration> configure) where TSettings : CommandSettings
        {
            return AddBranch(name, typeof(TSettings), configure);
        }

        internal CommandRegistration FindCommand(string value, bool caseSensitive)
        {
            return _commands.FirstOrDefault(command => command.Matches(value, caseSensitive));
        }

        internal CommandBranchRegistration FindBranch(string value, bool caseSensitive)
        {
            StringComparison comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            return _branches.FirstOrDefault(branch => branch.Matches(value, comparison));
        }

        internal bool Matches(string value, StringComparison comparison)
        {
            if (string.Equals(Name, value, comparison))
            {
                return true;
            }

            return _aliases.Any(alias => string.Equals(alias, value, comparison));
        }

        internal CommandRegistration AddCommandUnsafe(string name, Type commandType)
        {
            return AddCommand(name, commandType, () => CreateCommand(commandType));
        }

        internal CommandBranchRegistration AddBranchUnsafe(string name, Type settingsType, Action<CommandBranchRegistration> configure)
        {
            return AddBranch(name, settingsType, configure);
        }

        private CommandRegistration AddCommand(string name, Type commandType, Func<object> factory)
        {
            if (FindCommand(name, false) != null || FindBranch(name, false) != null)
            {
                throw new CommandConfigurationException("A command or branch named '" + name + "' has already been registered.");
            }

            _registerCommandType(commandType);
            var registration = new CommandRegistration(name, commandType, factory);
            _commands.Add(registration);
            return registration;
        }

        private CommandBranchRegistration AddBranch(string name, Type settingsType, Action<CommandBranchRegistration> configure)
        {
            if (FindCommand(name, false) != null || FindBranch(name, false) != null)
            {
                throw new CommandConfigurationException("A command or branch named '" + name + "' has already been registered.");
            }

            var branch = new CommandBranchRegistration(name, settingsType, _commandActivator, _registerCommandType);
            _branches.Add(branch);
            configure?.Invoke(branch);
            return branch;
        }

        private object CreateCommand(Type commandType)
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
    }
}
