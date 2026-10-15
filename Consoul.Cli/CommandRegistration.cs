using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents one command registered with a <see cref="CommandApp"/>.
    /// </summary>
    public sealed class CommandRegistration : ICommandConfigurator
    {
        private readonly List<string> _aliases = new List<string>();
        private readonly List<string[]> _examples = new List<string[]>();
        private readonly Func<object> _factory;

        internal CommandRegistration(string name, Type commandType, Func<object> factory)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Command name is required.", nameof(name));
            }

            Name = name;
            CommandType = commandType ?? throw new ArgumentNullException(nameof(commandType));
            SettingsType = CommandSettingsBinder.GetSettingsType(CommandType);
            Description = CommandDescriptionMetadata.FromTypes(CommandType, SettingsType);
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Gets the primary command name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the command implementation type.
        /// </summary>
        public Type CommandType { get; }

        /// <summary>
        /// Gets the command settings type when the command uses strongly-typed settings.
        /// </summary>
        public Type SettingsType { get; }

        /// <summary>
        /// Gets the command description shown in help output.
        /// </summary>
        public string Description { get; private set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the command is hidden from command listings.
        /// </summary>
        public bool Hidden { get; private set; }

        /// <summary>
        /// Gets the aliases that can invoke the command.
        /// </summary>
        public IReadOnlyList<string> Aliases => _aliases.AsReadOnly();

        /// <summary>
        /// Gets example argument lists shown in command help output.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<string>> Examples => _examples.Select(example => (IReadOnlyList<string>)example).ToArray();

        /// <summary>
        /// Sets the command description shown in help output.
        /// </summary>
        /// <param name="description">The command description.</param>
        /// <returns>The current <see cref="CommandRegistration"/> instance.</returns>
        public CommandRegistration WithDescription(string description)
        {
            Description = description ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Adds an alias that can invoke the command.
        /// </summary>
        /// <param name="alias">The alias to add.</param>
        /// <returns>The current <see cref="CommandRegistration"/> instance.</returns>
        public CommandRegistration WithAlias(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                throw new ArgumentException("Command alias is required.", nameof(alias));
            }

            if (!_aliases.Contains(alias))
            {
                _aliases.Add(alias);
            }

            return this;
        }

        /// <summary>
        /// Adds aliases that can invoke the command.
        /// </summary>
        /// <param name="aliases">The aliases to add.</param>
        /// <returns>The current <see cref="CommandRegistration"/> instance.</returns>
        public CommandRegistration WithAliases(params string[] aliases)
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
        /// Adds an example shown in command help output.
        /// </summary>
        /// <param name="arguments">The example arguments, including the command name if desired.</param>
        /// <returns>The current <see cref="CommandRegistration"/> instance.</returns>
        public CommandRegistration WithExample(params string[] arguments)
        {
            _examples.Add((arguments ?? new string[0]).ToArray());
            return this;
        }

        /// <summary>
        /// Adds examples shown in command help output.
        /// </summary>
        /// <param name="examples">The example argument lists, including the command name if desired.</param>
        /// <returns>The current <see cref="CommandRegistration"/> instance.</returns>
        public CommandRegistration WithExamples(params string[][] examples)
        {
            if (examples == null)
            {
                throw new ArgumentNullException(nameof(examples));
            }

            foreach (string[] example in examples)
            {
                WithExample(example);
            }

            return this;
        }

        /// <summary>
        /// Hides or shows the command in command listings while keeping it invokable.
        /// </summary>
        /// <param name="isHidden">A value indicating whether the command should be hidden.</param>
        /// <returns>The current <see cref="CommandRegistration"/> instance.</returns>
        public CommandRegistration WithHidden(bool isHidden = true)
        {
            Hidden = isHidden;
            return this;
        }

        /// <summary>
        /// Hides the command in command listings while keeping it invokable.
        /// </summary>
        /// <returns>The current <see cref="CommandRegistration"/> instance.</returns>
        public CommandRegistration IsHiddenCommand()
        {
            return WithHidden();
        }

        /// <summary>
        /// Hides the command in command listings while keeping it invokable.
        /// </summary>
        /// <returns>The current <see cref="CommandRegistration"/> instance.</returns>
        public CommandRegistration IsHidden()
        {
            return WithHidden();
        }

        internal object CreateCommand()
        {
            object command = _factory();
            if (command == null)
            {
                throw new CommandRuntimeException("Command factory returned null for '" + CommandType.FullName + "'.");
            }

            if (!CommandType.IsInstanceOfType(command))
            {
                throw new CommandRuntimeException("Command factory for '" + CommandType.FullName + "' returned '" + command.GetType().FullName + "'.");
            }

            return command;
        }

        internal bool Matches(string value, bool caseSensitive)
        {
            StringComparison comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            if (string.Equals(Name, value, comparison))
            {
                return true;
            }

            return _aliases.Any(alias => string.Equals(alias, value, comparison));
        }
    }
}
