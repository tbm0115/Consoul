using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides data used to render top-level command app help.
    /// </summary>
    public sealed class CommandAppHelpContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAppHelpContext"/> class.
        /// </summary>
        /// <param name="settings">The command app settings.</param>
        /// <param name="commands">The top-level command registrations.</param>
        /// <param name="branches">The top-level branch registrations.</param>
        /// <param name="examples">The application examples.</param>
        public CommandAppHelpContext(
            CommandAppSettings settings,
            IReadOnlyList<CommandRegistration> commands,
            IReadOnlyList<CommandBranchRegistration> branches,
            IReadOnlyList<IReadOnlyList<string>> examples)
            : this(settings, commands, branches, examples, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAppHelpContext"/> class.
        /// </summary>
        /// <param name="settings">The command app settings.</param>
        /// <param name="commands">The top-level command registrations.</param>
        /// <param name="branches">The top-level branch registrations.</param>
        /// <param name="examples">The application examples.</param>
        /// <param name="defaultCommand">The default command registration, when one is configured.</param>
        public CommandAppHelpContext(
            CommandAppSettings settings,
            IReadOnlyList<CommandRegistration> commands,
            IReadOnlyList<CommandBranchRegistration> branches,
            IReadOnlyList<IReadOnlyList<string>> examples,
            CommandRegistration defaultCommand)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Commands = commands == null ? Array.Empty<CommandRegistration>() : commands.ToArray();
            Branches = branches == null ? Array.Empty<CommandBranchRegistration>() : branches.ToArray();
            Examples = examples == null
                ? Array.Empty<IReadOnlyList<string>>()
                : examples.Select(example => (IReadOnlyList<string>)(example == null ? Array.Empty<string>() : example.ToArray())).ToArray();
            DefaultCommand = defaultCommand;
            DefaultCommandInfo = defaultCommand == null ? null : CommandHelpModelFactory.CreateCommand(defaultCommand);
            Model = CommandHelpModelFactory.CreateModel(Settings, Commands, Branches, Examples);
        }

        /// <summary>
        /// Gets the command app settings.
        /// </summary>
        public CommandAppSettings Settings { get; }

        /// <summary>
        /// Gets the Spectre-shaped command model for custom help renderers.
        /// </summary>
        public ICommandModel Model { get; }

        /// <summary>
        /// Gets the application name shown in help output.
        /// </summary>
        public string ApplicationName => Settings.ApplicationName;

        /// <summary>
        /// Gets the application version shown in help output.
        /// </summary>
        public string ApplicationVersion => Settings.ApplicationVersion;

        /// <summary>
        /// Gets the application description shown in help output.
        /// </summary>
        public string ApplicationDescription => Settings.ApplicationDescription;

        /// <summary>
        /// Gets the top-level command registrations in display order.
        /// </summary>
        public IReadOnlyList<CommandRegistration> Commands { get; }

        /// <summary>
        /// Gets the default command registration, when one is configured.
        /// </summary>
        public CommandRegistration DefaultCommand { get; }

        /// <summary>
        /// Gets the read-only default command model, when one is configured.
        /// </summary>
        public ICommandInfo DefaultCommandInfo { get; }

        /// <summary>
        /// Gets the top-level branch registrations in display order.
        /// </summary>
        public IReadOnlyList<CommandBranchRegistration> Branches { get; }

        /// <summary>
        /// Gets application examples in display order.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<string>> Examples { get; }
    }
}
