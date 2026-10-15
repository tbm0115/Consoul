using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary
{
    /// <summary>
    /// Configures commands, aliases, views, and help metadata for a <see cref="CommandApp"/>.
    /// </summary>
    public interface IConfigurator
    {
        /// <summary>
        /// Gets command app settings.
        /// </summary>
        CommandAppSettings Settings { get; }

        /// <summary>
        /// Gets the registered commands in display order.
        /// </summary>
        IReadOnlyList<CommandRegistration> Commands { get; }

        /// <summary>
        /// Gets the registered command branches in display order.
        /// </summary>
        IReadOnlyList<CommandBranchRegistration> Branches { get; }

        /// <summary>
        /// Gets example argument lists shown in top-level help output.
        /// </summary>
        IReadOnlyList<IReadOnlyList<string>> Examples { get; }

        /// <summary>
        /// Gets the command that runs when no command name is supplied.
        /// </summary>
        CommandRegistration DefaultCommand { get; }

        /// <summary>
        /// Gets the custom help provider used to render command app help.
        /// </summary>
        IHelpProvider HelpProvider { get; }

        /// <summary>
        /// Sets the application name shown in help output.
        /// </summary>
        /// <param name="name">The application name.</param>
        /// <returns>The current configurator.</returns>
        CommandAppConfigurator SetApplicationName(string name);

        /// <summary>
        /// Sets the application version shown in help output.
        /// </summary>
        /// <param name="version">The application version.</param>
        /// <returns>The current configurator.</returns>
        CommandAppConfigurator SetApplicationVersion(string version);

        /// <summary>
        /// Sets the application description shown in help output.
        /// </summary>
        /// <param name="description">The application description.</param>
        /// <returns>The current configurator.</returns>
        CommandAppConfigurator SetApplicationDescription(string description);

        /// <summary>
        /// Rethrows command exceptions instead of rendering them as command errors.
        /// </summary>
        /// <returns>The current configurator.</returns>
        CommandAppConfigurator PropagateExceptions();

        /// <summary>
        /// Sets the exit code returned when command execution is canceled.
        /// </summary>
        /// <param name="exitCode">The cancellation exit code.</param>
        /// <returns>The current configurator.</returns>
        CommandAppConfigurator CancellationExitCode(int exitCode);

        /// <summary>
        /// Hides option default values from generated help output.
        /// </summary>
        /// <returns>The current configurator.</returns>
        CommandAppConfigurator HideOptionDefaultValues();

        /// <summary>
        /// Sets whether generated help descriptions should trim trailing periods.
        /// </summary>
        /// <param name="trim">A value indicating whether trailing periods should be trimmed.</param>
        /// <returns>The current configurator.</returns>
        CommandAppConfigurator TrimTrailingPeriods(bool trim = true);

        /// <summary>
        /// Sets whether command and branch lookup should be case-sensitive.
        /// </summary>
        /// <param name="caseSensitive">A value indicating whether command lookup should be case-sensitive.</param>
        /// <returns>The current configurator.</returns>
        CommandAppConfigurator SetCaseSensitive(bool caseSensitive = true);

        /// <summary>
        /// Sets whether unknown options should be rejected during settings binding.
        /// </summary>
        /// <param name="strictParsing">A value indicating whether unknown options should be rejected.</param>
        /// <returns>The current configurator.</returns>
        CommandAppConfigurator SetStrictParsing(bool strictParsing = true);

        /// <summary>
        /// Allows unknown option-shaped tokens to flow through as positional arguments.
        /// </summary>
        /// <returns>The current configurator.</returns>
        CommandAppConfigurator IgnoreUnknownOptions();

        /// <summary>
        /// Validates registered command examples before commands run.
        /// </summary>
        /// <returns>The current configurator.</returns>
        CommandAppConfigurator ValidateExamples();

        /// <summary>
        /// Adds an example shown in top-level command app help output.
        /// </summary>
        /// <param name="arguments">The example arguments.</param>
        /// <returns>The current configurator.</returns>
        CommandAppConfigurator AddExample(params string[] arguments);

        /// <summary>
        /// Adds examples shown in top-level command app help output.
        /// </summary>
        /// <param name="examples">The example argument lists.</param>
        /// <returns>The current configurator.</returns>
        CommandAppConfigurator AddExamples(params string[][] examples);

        /// <summary>
        /// Sets the interceptor that runs before and after command execution.
        /// </summary>
        /// <param name="interceptor">The interceptor to use.</param>
        /// <returns>The current configurator.</returns>
        CommandAppConfigurator SetInterceptor(ICommandInterceptor interceptor);

        /// <summary>
        /// Sets the provider used to render command app help.
        /// </summary>
        /// <param name="provider">The help provider to use.</param>
        /// <returns>The current configurator.</returns>
        CommandAppConfigurator SetHelpProvider(IHelpProvider provider);

        /// <summary>
        /// Sets the handler that renders command app exceptions.
        /// </summary>
        /// <param name="handler">The exception handler to use.</param>
        /// <returns>The current configurator.</returns>
        CommandAppConfigurator SetExceptionHandler(ICommandExceptionHandler handler);

        /// <summary>
        /// Sets the handler that renders command app exceptions.
        /// </summary>
        /// <param name="handler">The exception handler delegate to use.</param>
        /// <returns>The current configurator.</returns>
        CommandAppConfigurator SetExceptionHandler(Func<CommandExceptionContext, int> handler);

        /// <summary>
        /// Sets the activator used to create command and view instances.
        /// </summary>
        /// <param name="activator">The command activator.</param>
        /// <returns>The current configurator.</returns>
        CommandAppConfigurator SetCommandActivator(ICommandActivator activator);

        /// <summary>
        /// Sets the activator used to create command and view instances.
        /// </summary>
        /// <param name="activator">The activator delegate.</param>
        /// <returns>The current configurator.</returns>
        CommandAppConfigurator SetCommandActivator(Func<Type, object> activator);

        /// <summary>
        /// Sets the type registrar used to create command and MVC-style view instances.
        /// </summary>
        /// <param name="registrar">The type registrar.</param>
        /// <returns>The current configurator.</returns>
        CommandAppConfigurator SetTypeRegistrar(ITypeRegistrar registrar);

        /// <summary>
        /// Sets the type registrar used to create command and MVC-style view instances, then configures it with a frontend.
        /// </summary>
        /// <param name="registrar">The type registrar.</param>
        /// <param name="frontend">The frontend that configures the registrar.</param>
        /// <returns>The current configurator.</returns>
        CommandAppConfigurator SetTypeRegistrar(ITypeRegistrar registrar, ITypeRegistrarFrontend frontend);

        /// <summary>
        /// Sets the command that runs when no command name is supplied.
        /// </summary>
        /// <typeparam name="TCommand">The command implementation type.</typeparam>
        /// <returns>The default command registration.</returns>
        CommandRegistration SetDefaultCommand<TCommand>() where TCommand : class;

        /// <summary>
        /// Sets the command that runs when no command name is supplied.
        /// </summary>
        /// <typeparam name="TCommand">The command implementation type.</typeparam>
        /// <param name="factory">The command factory.</param>
        /// <returns>The default command registration.</returns>
        CommandRegistration SetDefaultCommand<TCommand>(Func<TCommand> factory) where TCommand : class;

        /// <summary>
        /// Adds a typed command.
        /// </summary>
        /// <typeparam name="TCommand">The command implementation type.</typeparam>
        /// <param name="name">The command name.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddCommand<TCommand>(string name) where TCommand : class;

        /// <summary>
        /// Adds a typed command with an explicit factory.
        /// </summary>
        /// <typeparam name="TCommand">The command implementation type.</typeparam>
        /// <param name="name">The command name.</param>
        /// <param name="factory">The command factory.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddCommand<TCommand>(string name, Func<TCommand> factory) where TCommand : class;

        /// <summary>
        /// Adds a command that renders an MVC-style view.
        /// </summary>
        /// <typeparam name="TView">The view type to render.</typeparam>
        /// <param name="name">The command name.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddView<TView>(string name) where TView : IView;

        /// <summary>
        /// Adds a delegate command for quick proof-of-concept command apps.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddCommand(string name, Action<CommandContext> execute);

        /// <summary>
        /// Adds a delegate command for quick proof-of-concept command apps.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddCommand(string name, Func<CommandContext, int> execute);

        /// <summary>
        /// Adds a Spectre-style delegate command for quick proof-of-concept command apps.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddDelegate(string name, Func<CommandContext, CancellationToken, int> execute);

        /// <summary>
        /// Adds an asynchronous delegate command for quick proof-of-concept command apps.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddCommand(string name, Func<CommandContext, CancellationToken, Task<int>> execute);

        /// <summary>
        /// Adds a Spectre-style asynchronous delegate command for quick proof-of-concept command apps.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddAsyncDelegate(string name, Func<CommandContext, CancellationToken, Task<int>> execute);

        /// <summary>
        /// Adds a delegate command with strongly-typed settings for quick proof-of-concept command apps.
        /// </summary>
        /// <typeparam name="TSettings">The command settings type.</typeparam>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddCommand<TSettings>(string name, Action<CommandContext, TSettings> execute) where TSettings : CommandSettings;

        /// <summary>
        /// Adds a delegate command with strongly-typed settings for quick proof-of-concept command apps.
        /// </summary>
        /// <typeparam name="TSettings">The command settings type.</typeparam>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddCommand<TSettings>(string name, Func<CommandContext, TSettings, int> execute) where TSettings : CommandSettings;

        /// <summary>
        /// Adds a Spectre-style delegate command with strongly-typed settings for quick proof-of-concept command apps.
        /// </summary>
        /// <typeparam name="TSettings">The command settings type.</typeparam>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddDelegate<TSettings>(string name, Func<CommandContext, TSettings, CancellationToken, int> execute) where TSettings : CommandSettings;

        /// <summary>
        /// Adds an asynchronous delegate command with strongly-typed settings for quick proof-of-concept command apps.
        /// </summary>
        /// <typeparam name="TSettings">The command settings type.</typeparam>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddCommand<TSettings>(string name, Func<CommandContext, TSettings, CancellationToken, Task<int>> execute) where TSettings : CommandSettings;

        /// <summary>
        /// Adds a Spectre-style asynchronous delegate command with strongly-typed settings for quick proof-of-concept command apps.
        /// </summary>
        /// <typeparam name="TSettings">The command settings type.</typeparam>
        /// <param name="name">The command name.</param>
        /// <param name="execute">The delegate to execute.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddAsyncDelegate<TSettings>(string name, Func<CommandContext, TSettings, CancellationToken, Task<int>> execute) where TSettings : CommandSettings;

        /// <summary>
        /// Adds a branch of related subcommands.
        /// </summary>
        /// <param name="name">The branch name.</param>
        /// <param name="configure">The branch configuration callback.</param>
        /// <returns>The branch registration.</returns>
        CommandBranchRegistration AddBranch(string name, Action<CommandBranchRegistration> configure);

        /// <summary>
        /// Adds a branch of related subcommands with shared settings.
        /// </summary>
        /// <typeparam name="TSettings">The branch settings type.</typeparam>
        /// <param name="name">The branch name.</param>
        /// <param name="configure">The branch configuration callback.</param>
        /// <returns>The branch registration.</returns>
        CommandBranchRegistration AddBranch<TSettings>(string name, Action<CommandBranchRegistration> configure) where TSettings : CommandSettings;
    }
}
