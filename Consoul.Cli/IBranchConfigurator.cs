using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary
{
    /// <summary>
    /// Configures a branch of related commands in a <see cref="CommandApp"/>.
    /// </summary>
    public interface IBranchConfigurator
    {
        /// <summary>
        /// Gets the branch name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the branch settings type when shared branch options are configured.
        /// </summary>
        Type SettingsType { get; }

        /// <summary>
        /// Gets the branch description shown in help output.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets a value indicating whether the branch is hidden from command listings.
        /// </summary>
        bool Hidden { get; }

        /// <summary>
        /// Gets aliases that can invoke the branch.
        /// </summary>
        IReadOnlyList<string> Aliases { get; }

        /// <summary>
        /// Gets child commands registered under the branch.
        /// </summary>
        IReadOnlyList<CommandRegistration> Commands { get; }

        /// <summary>
        /// Gets nested branches registered under the branch.
        /// </summary>
        IReadOnlyList<CommandBranchRegistration> Branches { get; }

        /// <summary>
        /// Sets the branch description shown in help output.
        /// </summary>
        /// <param name="description">The branch description.</param>
        /// <returns>The branch registration.</returns>
        CommandBranchRegistration SetDescription(string description);

        /// <summary>
        /// Sets the branch description shown in help output.
        /// </summary>
        /// <param name="description">The branch description.</param>
        /// <returns>The branch registration.</returns>
        CommandBranchRegistration WithDescription(string description);

        /// <summary>
        /// Adds an alias that can invoke the branch.
        /// </summary>
        /// <param name="alias">The alias to add.</param>
        /// <returns>The branch registration.</returns>
        CommandBranchRegistration WithAlias(string alias);

        /// <summary>
        /// Adds aliases that can invoke the branch.
        /// </summary>
        /// <param name="aliases">The aliases to add.</param>
        /// <returns>The branch registration.</returns>
        CommandBranchRegistration WithAliases(params string[] aliases);

        /// <summary>
        /// Hides or shows the branch in command listings while keeping it invokable.
        /// </summary>
        /// <param name="isHidden">A value indicating whether the branch should be hidden.</param>
        /// <returns>The branch registration.</returns>
        CommandBranchRegistration WithHidden(bool isHidden = true);

        /// <summary>
        /// Hides the branch in command listings while keeping it invokable.
        /// </summary>
        /// <returns>The branch registration.</returns>
        CommandBranchRegistration IsHiddenBranch();

        /// <summary>
        /// Hides the branch in command listings while keeping it invokable.
        /// </summary>
        /// <returns>The branch registration.</returns>
        CommandBranchRegistration IsHidden();

        /// <summary>
        /// Adds a typed command to the branch.
        /// </summary>
        /// <typeparam name="TCommand">The command implementation type.</typeparam>
        /// <param name="name">The command name.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddCommand<TCommand>(string name) where TCommand : class;

        /// <summary>
        /// Adds a typed command with an explicit factory to the branch.
        /// </summary>
        /// <typeparam name="TCommand">The command implementation type.</typeparam>
        /// <param name="name">The command name.</param>
        /// <param name="factory">The command factory.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddCommand<TCommand>(string name, Func<TCommand> factory) where TCommand : class;

        /// <summary>
        /// Adds a delegate command to the branch.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddCommand(string name, Action<CommandContext> execute);

        /// <summary>
        /// Adds a delegate command to the branch.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddCommand(string name, Func<CommandContext, int> execute);

        /// <summary>
        /// Adds a Spectre-style delegate command to the branch.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddDelegate(string name, Func<CommandContext, CancellationToken, int> execute);

        /// <summary>
        /// Adds an asynchronous delegate command to the branch.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddCommand(string name, Func<CommandContext, CancellationToken, Task<int>> execute);

        /// <summary>
        /// Adds a Spectre-style asynchronous delegate command to the branch.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddAsyncDelegate(string name, Func<CommandContext, CancellationToken, Task<int>> execute);

        /// <summary>
        /// Adds a delegate command with strongly-typed settings to the branch.
        /// </summary>
        /// <typeparam name="TSettings">The command settings type.</typeparam>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddCommand<TSettings>(string name, Action<CommandContext, TSettings> execute) where TSettings : CommandSettings;

        /// <summary>
        /// Adds a delegate command with strongly-typed settings to the branch.
        /// </summary>
        /// <typeparam name="TSettings">The command settings type.</typeparam>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddCommand<TSettings>(string name, Func<CommandContext, TSettings, int> execute) where TSettings : CommandSettings;

        /// <summary>
        /// Adds a Spectre-style delegate command with strongly-typed settings to the branch.
        /// </summary>
        /// <typeparam name="TSettings">The command settings type.</typeparam>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddDelegate<TSettings>(string name, Func<CommandContext, TSettings, CancellationToken, int> execute) where TSettings : CommandSettings;

        /// <summary>
        /// Adds an asynchronous delegate command with strongly-typed settings to the branch.
        /// </summary>
        /// <typeparam name="TSettings">The command settings type.</typeparam>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddCommand<TSettings>(string name, Func<CommandContext, TSettings, CancellationToken, Task<int>> execute) where TSettings : CommandSettings;

        /// <summary>
        /// Adds a Spectre-style asynchronous delegate command with strongly-typed settings to the branch.
        /// </summary>
        /// <typeparam name="TSettings">The command settings type.</typeparam>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddAsyncDelegate<TSettings>(string name, Func<CommandContext, TSettings, CancellationToken, Task<int>> execute) where TSettings : CommandSettings;

        /// <summary>
        /// Adds a command that renders an MVC-style view.
        /// </summary>
        /// <typeparam name="TView">The view type to render.</typeparam>
        /// <param name="name">The command name.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddView<TView>(string name) where TView : IView;

        /// <summary>
        /// Adds a nested branch.
        /// </summary>
        /// <param name="name">The branch name.</param>
        /// <param name="configure">The branch configuration callback.</param>
        /// <returns>The branch registration.</returns>
        CommandBranchRegistration AddBranch(string name, Action<CommandBranchRegistration> configure);

        /// <summary>
        /// Adds a nested branch with shared settings.
        /// </summary>
        /// <typeparam name="TSettings">The branch settings type.</typeparam>
        /// <param name="name">The branch name.</param>
        /// <param name="configure">The branch configuration callback.</param>
        /// <returns>The branch registration.</returns>
        CommandBranchRegistration AddBranch<TSettings>(string name, Action<CommandBranchRegistration> configure) where TSettings : CommandSettings;
    }
}
