using System;
using System.Collections.Generic;

namespace ConsoulLibrary
{
    /// <summary>
    /// Configures metadata for a registered command.
    /// </summary>
    public interface ICommandConfigurator
    {
        /// <summary>
        /// Gets the primary command name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the command implementation type.
        /// </summary>
        Type CommandType { get; }

        /// <summary>
        /// Gets the command settings type when the command uses strongly-typed settings.
        /// </summary>
        Type SettingsType { get; }

        /// <summary>
        /// Gets the command description shown in help output.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets a value indicating whether the command is hidden from command listings.
        /// </summary>
        bool Hidden { get; }

        /// <summary>
        /// Gets the aliases that can invoke the command.
        /// </summary>
        IReadOnlyList<string> Aliases { get; }

        /// <summary>
        /// Gets example argument lists shown in command help output.
        /// </summary>
        IReadOnlyList<IReadOnlyList<string>> Examples { get; }

        /// <summary>
        /// Sets the command description shown in help output.
        /// </summary>
        /// <param name="description">The command description.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration WithDescription(string description);

        /// <summary>
        /// Adds an alias that can invoke the command.
        /// </summary>
        /// <param name="alias">The alias to add.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration WithAlias(string alias);

        /// <summary>
        /// Adds aliases that can invoke the command.
        /// </summary>
        /// <param name="aliases">The aliases to add.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration WithAliases(params string[] aliases);

        /// <summary>
        /// Adds an example shown in command help output.
        /// </summary>
        /// <param name="arguments">The example arguments, including the command name if desired.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration WithExample(params string[] arguments);

        /// <summary>
        /// Adds examples shown in command help output.
        /// </summary>
        /// <param name="examples">The example argument lists, including the command name if desired.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration WithExamples(params string[][] examples);

        /// <summary>
        /// Hides or shows the command in command listings while keeping it invokable.
        /// </summary>
        /// <param name="isHidden">A value indicating whether the command should be hidden.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration WithHidden(bool isHidden = true);

        /// <summary>
        /// Hides the command in command listings while keeping it invokable.
        /// </summary>
        /// <returns>The command registration.</returns>
        CommandRegistration IsHiddenCommand();

        /// <summary>
        /// Hides the command in command listings while keeping it invokable.
        /// </summary>
        /// <returns>The command registration.</returns>
        CommandRegistration IsHidden();
    }
}
