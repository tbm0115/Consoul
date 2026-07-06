using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides data used to render help for a command.
    /// </summary>
    public sealed class CommandHelpContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHelpContext"/> class.
        /// </summary>
        /// <param name="settings">The command app settings.</param>
        /// <param name="path">The command tokens that identify the command.</param>
        /// <param name="command">The command registration.</param>
        /// <param name="inheritedSettingsTypes">Settings types inherited from parent branches.</param>
        public CommandHelpContext(
            CommandAppSettings settings,
            IReadOnlyList<string> path,
            CommandRegistration command,
            IReadOnlyList<Type> inheritedSettingsTypes)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Path = path == null ? Array.Empty<string>() : path.ToArray();
            Command = command ?? throw new ArgumentNullException(nameof(command));
            InheritedSettingsTypes = inheritedSettingsTypes == null ? Array.Empty<Type>() : inheritedSettingsTypes.ToArray();
            CommandInfo = CommandHelpModelFactory.CreateCommand(Command);
            InheritedOptions = CommandHelpModelFactory.GetVisibleInheritedOptions(Command.SettingsType, InheritedSettingsTypes);
        }

        /// <summary>
        /// Gets the command app settings.
        /// </summary>
        public CommandAppSettings Settings { get; }

        /// <summary>
        /// Gets the Spectre-shaped command model for custom help renderers.
        /// </summary>
        public ICommandInfo CommandInfo { get; }

        /// <summary>
        /// Gets the application name shown in help output.
        /// </summary>
        public string ApplicationName => Settings.ApplicationName;

        /// <summary>
        /// Gets the command tokens that identify the command.
        /// </summary>
        public IReadOnlyList<string> Path { get; }

        /// <summary>
        /// Gets the command path as a display string.
        /// </summary>
        public string CommandPath => Path.Count == 0 ? Command.Name : string.Join(" ", Path);

        /// <summary>
        /// Gets the command registration.
        /// </summary>
        public CommandRegistration Command { get; }

        /// <summary>
        /// Gets positional arguments declared by the command settings type.
        /// </summary>
        public IReadOnlyList<ICommandArgument> Arguments => CommandInfo.Arguments;

        /// <summary>
        /// Gets options declared by the command settings type.
        /// </summary>
        public IReadOnlyList<ICommandOption> Options => CommandInfo.Options;

        /// <summary>
        /// Gets settings types inherited from parent branches.
        /// </summary>
        public IReadOnlyList<Type> InheritedSettingsTypes { get; }

        /// <summary>
        /// Gets visible options inherited from parent branches and not already declared by the command settings type.
        /// </summary>
        public IReadOnlyList<ICommandOption> InheritedOptions { get; }
    }
}
