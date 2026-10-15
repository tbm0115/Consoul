using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Describes the command surface available to a help renderer.
    /// </summary>
    public interface ICommandModel
    {
        /// <summary>
        /// Gets the application name shown in help output.
        /// </summary>
        string ApplicationName { get; }

        /// <summary>
        /// Gets the application version shown in help output.
        /// </summary>
        string ApplicationVersion { get; }

        /// <summary>
        /// Gets the application description shown in help output.
        /// </summary>
        string ApplicationDescription { get; }

        /// <summary>
        /// Gets the top-level commands in display order.
        /// </summary>
        IReadOnlyList<ICommandInfo> Commands { get; }

        /// <summary>
        /// Gets the top-level branches in display order.
        /// </summary>
        IReadOnlyList<ICommandContainer> Branches { get; }

        /// <summary>
        /// Gets application examples in display order.
        /// </summary>
        IReadOnlyList<IReadOnlyList<string>> Examples { get; }
    }

    /// <summary>
    /// Describes a command container such as a branch.
    /// </summary>
    public interface ICommandContainer
    {
        /// <summary>
        /// Gets the container name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the container description shown in help output.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets a value indicating whether the container is hidden from command listings.
        /// </summary>
        bool Hidden { get; }

        /// <summary>
        /// Gets aliases that can invoke the container.
        /// </summary>
        IReadOnlyList<string> Aliases { get; }

        /// <summary>
        /// Gets the shared settings type for the container.
        /// </summary>
        Type SettingsType { get; }

        /// <summary>
        /// Gets shared options declared by the container settings type.
        /// </summary>
        IReadOnlyList<ICommandOption> Options { get; }

        /// <summary>
        /// Gets child commands in display order.
        /// </summary>
        IReadOnlyList<ICommandInfo> Commands { get; }

        /// <summary>
        /// Gets nested containers in display order.
        /// </summary>
        IReadOnlyList<ICommandContainer> Branches { get; }
    }

    /// <summary>
    /// Describes an executable command.
    /// </summary>
    public interface ICommandInfo
    {
        /// <summary>
        /// Gets the command name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the command description shown in help output.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets a value indicating whether the command is hidden from command listings.
        /// </summary>
        bool Hidden { get; }

        /// <summary>
        /// Gets aliases that can invoke the command.
        /// </summary>
        IReadOnlyList<string> Aliases { get; }

        /// <summary>
        /// Gets example argument lists shown in command help output.
        /// </summary>
        IReadOnlyList<IReadOnlyList<string>> Examples { get; }

        /// <summary>
        /// Gets the command implementation type.
        /// </summary>
        Type CommandType { get; }

        /// <summary>
        /// Gets the command settings type.
        /// </summary>
        Type SettingsType { get; }

        /// <summary>
        /// Gets positional arguments declared by the command settings type.
        /// </summary>
        IReadOnlyList<ICommandArgument> Arguments { get; }

        /// <summary>
        /// Gets options declared by the command settings type.
        /// </summary>
        IReadOnlyList<ICommandOption> Options { get; }
    }

    /// <summary>
    /// Describes a command parameter.
    /// </summary>
    public interface ICommandParameter : ICommandParameterInfo
    {
    }

    /// <summary>
    /// Describes a positional command argument.
    /// </summary>
    public interface ICommandArgument : ICommandParameter
    {
        /// <summary>
        /// Gets the zero-based argument position.
        /// </summary>
        int Position { get; }
    }

    /// <summary>
    /// Describes a named command option.
    /// </summary>
    public interface ICommandOption : ICommandParameter
    {
        /// <summary>
        /// Gets all option aliases.
        /// </summary>
        IReadOnlyList<string> Aliases { get; }

        /// <summary>
        /// Gets the primary option name.
        /// </summary>
        string PrimaryName { get; }

        /// <summary>
        /// Gets the value name shown in help output.
        /// </summary>
        string ValueName { get; }

        /// <summary>
        /// Gets a value indicating whether the option should be hidden from help output.
        /// </summary>
        bool IsHidden { get; }

        /// <summary>
        /// Gets a value indicating whether the option value may be omitted.
        /// </summary>
        bool ValueIsOptional { get; }
    }

    internal static class CommandHelpModelFactory
    {
        public static ICommandModel CreateModel(
            CommandAppSettings settings,
            IReadOnlyList<CommandRegistration> commands,
            IReadOnlyList<CommandBranchRegistration> branches,
            IReadOnlyList<IReadOnlyList<string>> examples)
        {
            return new CommandModel(settings, commands, branches, examples);
        }

        public static ICommandInfo CreateCommand(CommandRegistration command)
        {
            return new CommandInfoModel(command);
        }

        public static ICommandContainer CreateContainer(CommandBranchRegistration branch)
        {
            return new CommandContainerModel(branch);
        }

        public static IReadOnlyList<ICommandOption> GetOptions(Type settingsType)
        {
            return CommandSettingsBinder.Describe(settingsType).Options.Cast<ICommandOption>().ToArray();
        }

        public static IReadOnlyList<ICommandOption> GetVisibleInheritedOptions(Type commandSettingsType, IReadOnlyList<Type> inheritedSettingsTypes)
        {
            if (inheritedSettingsTypes == null || inheritedSettingsTypes.Count == 0)
            {
                return Array.Empty<ICommandOption>();
            }

            var knownAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (ICommandOption option in GetOptions(commandSettingsType))
            {
                AddOptionAliases(knownAliases, option);
            }

            var result = new List<ICommandOption>();
            foreach (Type settingsType in inheritedSettingsTypes.Where(type => type != null))
            {
                foreach (ICommandOption option in GetOptions(settingsType).Where(option => !option.IsHidden))
                {
                    if (option.Aliases.Any(alias => knownAliases.Contains(alias)))
                    {
                        continue;
                    }

                    result.Add(option);
                    AddOptionAliases(knownAliases, option);
                }
            }

            return result.ToArray();
        }

        private static void AddOptionAliases(HashSet<string> aliases, ICommandOption option)
        {
            foreach (string alias in option.Aliases)
            {
                aliases.Add(alias);
            }
        }

        private sealed class CommandModel : ICommandModel
        {
            public CommandModel(
                CommandAppSettings settings,
                IReadOnlyList<CommandRegistration> commands,
                IReadOnlyList<CommandBranchRegistration> branches,
                IReadOnlyList<IReadOnlyList<string>> examples)
            {
                settings = settings ?? new CommandAppSettings();
                ApplicationName = settings.ApplicationName;
                ApplicationVersion = settings.ApplicationVersion;
                ApplicationDescription = settings.ApplicationDescription;
                Commands = (commands ?? Array.Empty<CommandRegistration>()).Select(CreateCommand).ToArray();
                Branches = (branches ?? Array.Empty<CommandBranchRegistration>()).Select(CreateContainer).ToArray();
                Examples = examples == null
                    ? Array.Empty<IReadOnlyList<string>>()
                    : examples.Select(example => (IReadOnlyList<string>)(example == null ? Array.Empty<string>() : example.ToArray())).ToArray();
            }

            public string ApplicationName { get; }

            public string ApplicationVersion { get; }

            public string ApplicationDescription { get; }

            public IReadOnlyList<ICommandInfo> Commands { get; }

            public IReadOnlyList<ICommandContainer> Branches { get; }

            public IReadOnlyList<IReadOnlyList<string>> Examples { get; }
        }

        private sealed class CommandContainerModel : ICommandContainer
        {
            public CommandContainerModel(CommandBranchRegistration branch)
            {
                branch = branch ?? throw new ArgumentNullException(nameof(branch));
                Name = branch.Name;
                Description = branch.Description;
                Hidden = branch.Hidden;
                Aliases = branch.Aliases.ToArray();
                SettingsType = branch.SettingsType;
                Options = GetOptions(branch.SettingsType);
                Commands = branch.Commands.Select(CreateCommand).ToArray();
                Branches = branch.Branches.Select(CreateContainer).ToArray();
            }

            public string Name { get; }

            public string Description { get; }

            public bool Hidden { get; }

            public IReadOnlyList<string> Aliases { get; }

            public Type SettingsType { get; }

            public IReadOnlyList<ICommandOption> Options { get; }

            public IReadOnlyList<ICommandInfo> Commands { get; }

            public IReadOnlyList<ICommandContainer> Branches { get; }
        }

        private sealed class CommandInfoModel : ICommandInfo
        {
            public CommandInfoModel(CommandRegistration command)
            {
                command = command ?? throw new ArgumentNullException(nameof(command));
                Name = command.Name;
                Description = command.Description;
                Hidden = command.Hidden;
                Aliases = command.Aliases.ToArray();
                Examples = command.Examples.Select(example => (IReadOnlyList<string>)(example == null ? Array.Empty<string>() : example.ToArray())).ToArray();
                CommandType = command.CommandType;
                SettingsType = command.SettingsType;
                CommandSettingsDescriptor descriptor = CommandSettingsBinder.Describe(command.SettingsType);
                Arguments = descriptor.Arguments.Cast<ICommandArgument>().ToArray();
                Options = descriptor.Options.Cast<ICommandOption>().ToArray();
            }

            public string Name { get; }

            public string Description { get; }

            public bool Hidden { get; }

            public IReadOnlyList<string> Aliases { get; }

            public IReadOnlyList<IReadOnlyList<string>> Examples { get; }

            public Type CommandType { get; }

            public Type SettingsType { get; }

            public IReadOnlyList<ICommandArgument> Arguments { get; }

            public IReadOnlyList<ICommandOption> Options { get; }
        }
    }
}
