using System;

namespace ConsoulLibrary.Unsafe
{
    /// <summary>
    /// Represents unsafe command app configuration that accepts runtime <see cref="Type"/> values.
    /// </summary>
    public interface IUnsafeConfigurator
    {
        /// <summary>
        /// Adds a command by runtime type.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="commandType">The command implementation type.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddCommand(string name, Type commandType);

        /// <summary>
        /// Adds an untyped branch by runtime type configuration.
        /// </summary>
        /// <param name="name">The branch name.</param>
        /// <param name="configure">The branch configuration callback.</param>
        /// <returns>The branch registration.</returns>
        CommandBranchRegistration AddBranch(string name, Action<IUnsafeBranchConfigurator> configure);

        /// <summary>
        /// Adds a branch by runtime settings type.
        /// </summary>
        /// <param name="name">The branch name.</param>
        /// <param name="settingsType">The branch settings type, or <c>null</c> for an untyped branch.</param>
        /// <param name="configure">The branch configuration callback.</param>
        /// <returns>The branch registration.</returns>
        CommandBranchRegistration AddBranch(string name, Type settingsType, Action<IUnsafeBranchConfigurator> configure);

        /// <summary>
        /// Sets the default command by runtime type.
        /// </summary>
        /// <param name="commandType">The command implementation type.</param>
        /// <returns>The default command registration.</returns>
        CommandRegistration SetDefaultCommand(Type commandType);
    }

    /// <summary>
    /// Represents unsafe branch configuration that accepts runtime <see cref="Type"/> values.
    /// </summary>
    public interface IUnsafeBranchConfigurator
    {
        /// <summary>
        /// Adds a command by runtime type.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="commandType">The command implementation type.</param>
        /// <returns>The command registration.</returns>
        CommandRegistration AddCommand(string name, Type commandType);

        /// <summary>
        /// Adds an untyped nested branch by runtime type configuration.
        /// </summary>
        /// <param name="name">The branch name.</param>
        /// <param name="configure">The branch configuration callback.</param>
        /// <returns>The branch registration.</returns>
        CommandBranchRegistration AddBranch(string name, Action<IUnsafeBranchConfigurator> configure);

        /// <summary>
        /// Adds a nested branch by runtime settings type.
        /// </summary>
        /// <param name="name">The branch name.</param>
        /// <param name="settingsType">The branch settings type, or <c>null</c> for an untyped branch.</param>
        /// <param name="configure">The branch configuration callback.</param>
        /// <returns>The branch registration.</returns>
        CommandBranchRegistration AddBranch(string name, Type settingsType, Action<IUnsafeBranchConfigurator> configure);
    }

    /// <summary>
    /// Provides Spectre-style unsafe configuration helpers for dynamic command discovery.
    /// </summary>
    public static class UnsafeConfiguratorExtensions
    {
        /// <summary>
        /// Gets an unsafe configurator for runtime type registration.
        /// </summary>
        /// <param name="configurator">The command app configurator.</param>
        /// <returns>An unsafe configurator wrapper.</returns>
        public static IUnsafeConfigurator AsUnsafe(this IConfigurator configurator)
        {
            return new UnsafeConfigurator(RequireConfigurator(configurator));
        }

        /// <summary>
        /// Gets an unsafe configurator for runtime type registration.
        /// </summary>
        /// <param name="configurator">The command app configurator.</param>
        /// <returns>An unsafe configurator wrapper.</returns>
        public static IUnsafeConfigurator Unsafe(this IConfigurator configurator)
        {
            return AsUnsafe(configurator);
        }

        /// <summary>
        /// Gets an unsafe branch configurator for runtime type registration.
        /// </summary>
        /// <param name="configurator">The branch configurator.</param>
        /// <returns>An unsafe branch configurator wrapper.</returns>
        public static IUnsafeBranchConfigurator AsUnsafe(this IBranchConfigurator configurator)
        {
            return new UnsafeBranchConfigurator(RequireBranchConfigurator(configurator));
        }

        /// <summary>
        /// Gets an unsafe branch configurator for runtime type registration.
        /// </summary>
        /// <param name="configurator">The branch configurator.</param>
        /// <returns>An unsafe branch configurator wrapper.</returns>
        public static IUnsafeBranchConfigurator Unsafe(this IBranchConfigurator configurator)
        {
            return AsUnsafe(configurator);
        }

        /// <summary>
        /// Adds a command by runtime type.
        /// </summary>
        /// <param name="configurator">The command app configurator.</param>
        /// <param name="name">The command name.</param>
        /// <param name="commandType">The command implementation type.</param>
        /// <returns>The command registration.</returns>
        public static CommandRegistration AddCommand(this IConfigurator configurator, string name, Type commandType)
        {
            return RequireConfigurator(configurator).AddCommandUnsafe(name, commandType);
        }

        /// <summary>
        /// Adds a command by runtime type.
        /// </summary>
        /// <param name="configurator">The branch configurator.</param>
        /// <param name="name">The command name.</param>
        /// <param name="commandType">The command implementation type.</param>
        /// <returns>The command registration.</returns>
        public static CommandRegistration AddCommand(this IBranchConfigurator configurator, string name, Type commandType)
        {
            return RequireBranchConfigurator(configurator).AddCommandUnsafe(name, commandType);
        }

        /// <summary>
        /// Adds an untyped branch by runtime type configuration.
        /// </summary>
        /// <param name="configurator">The command app configurator.</param>
        /// <param name="name">The branch name.</param>
        /// <param name="configure">The branch configuration callback.</param>
        /// <returns>The branch registration.</returns>
        public static CommandBranchRegistration AddBranch(this IConfigurator configurator, string name, Action<IUnsafeBranchConfigurator> configure)
        {
            return AddBranch(configurator, name, null, configure);
        }

        /// <summary>
        /// Adds a branch by runtime settings type.
        /// </summary>
        /// <param name="configurator">The command app configurator.</param>
        /// <param name="name">The branch name.</param>
        /// <param name="settingsType">The branch settings type, or <c>null</c> for an untyped branch.</param>
        /// <param name="configure">The branch configuration callback.</param>
        /// <returns>The branch registration.</returns>
        public static CommandBranchRegistration AddBranch(this IConfigurator configurator, string name, Type settingsType, Action<IUnsafeBranchConfigurator> configure)
        {
            return RequireConfigurator(configurator).AddBranchUnsafe(
                name,
                settingsType,
                branch => configure?.Invoke(new UnsafeBranchConfigurator(branch)));
        }

        /// <summary>
        /// Adds an untyped nested branch by runtime type configuration.
        /// </summary>
        /// <param name="configurator">The branch configurator.</param>
        /// <param name="name">The branch name.</param>
        /// <param name="configure">The branch configuration callback.</param>
        /// <returns>The branch registration.</returns>
        public static CommandBranchRegistration AddBranch(this IBranchConfigurator configurator, string name, Action<IUnsafeBranchConfigurator> configure)
        {
            return AddBranch(configurator, name, null, configure);
        }

        /// <summary>
        /// Adds a nested branch by runtime settings type.
        /// </summary>
        /// <param name="configurator">The branch configurator.</param>
        /// <param name="name">The branch name.</param>
        /// <param name="settingsType">The branch settings type, or <c>null</c> for an untyped branch.</param>
        /// <param name="configure">The branch configuration callback.</param>
        /// <returns>The branch registration.</returns>
        public static CommandBranchRegistration AddBranch(this IBranchConfigurator configurator, string name, Type settingsType, Action<IUnsafeBranchConfigurator> configure)
        {
            return RequireBranchConfigurator(configurator).AddBranchUnsafe(
                name,
                settingsType,
                branch => configure?.Invoke(new UnsafeBranchConfigurator(branch)));
        }

        /// <summary>
        /// Sets the default command by runtime type.
        /// </summary>
        /// <param name="configurator">The command app configurator.</param>
        /// <param name="commandType">The command implementation type.</param>
        /// <returns>The default command registration.</returns>
        public static CommandRegistration SetDefaultCommand(this IConfigurator configurator, Type commandType)
        {
            return RequireConfigurator(configurator).SetDefaultCommandUnsafe(commandType);
        }

        private static CommandAppConfigurator RequireConfigurator(IConfigurator configurator)
        {
            var concrete = configurator as CommandAppConfigurator;
            if (concrete == null)
            {
                throw new CommandConfigurationException("Unsafe configuration requires a CommandAppConfigurator instance.");
            }

            return concrete;
        }

        private static CommandBranchRegistration RequireBranchConfigurator(IBranchConfigurator configurator)
        {
            var concrete = configurator as CommandBranchRegistration;
            if (concrete == null)
            {
                throw new CommandConfigurationException("Unsafe branch configuration requires a CommandBranchRegistration instance.");
            }

            return concrete;
        }

        private sealed class UnsafeConfigurator : IUnsafeConfigurator
        {
            private readonly CommandAppConfigurator _configurator;

            public UnsafeConfigurator(CommandAppConfigurator configurator)
            {
                _configurator = configurator ?? throw new ArgumentNullException(nameof(configurator));
            }

            public CommandRegistration AddCommand(string name, Type commandType)
            {
                return _configurator.AddCommandUnsafe(name, commandType);
            }

            public CommandBranchRegistration AddBranch(string name, Action<IUnsafeBranchConfigurator> configure)
            {
                return AddBranch(name, null, configure);
            }

            public CommandBranchRegistration AddBranch(string name, Type settingsType, Action<IUnsafeBranchConfigurator> configure)
            {
                return _configurator.AddBranchUnsafe(
                    name,
                    settingsType,
                    branch => configure?.Invoke(new UnsafeBranchConfigurator(branch)));
            }

            public CommandRegistration SetDefaultCommand(Type commandType)
            {
                return _configurator.SetDefaultCommandUnsafe(commandType);
            }
        }

        private sealed class UnsafeBranchConfigurator : IUnsafeBranchConfigurator
        {
            private readonly CommandBranchRegistration _configurator;

            public UnsafeBranchConfigurator(CommandBranchRegistration configurator)
            {
                _configurator = configurator ?? throw new ArgumentNullException(nameof(configurator));
            }

            public CommandRegistration AddCommand(string name, Type commandType)
            {
                return _configurator.AddCommandUnsafe(name, commandType);
            }

            public CommandBranchRegistration AddBranch(string name, Action<IUnsafeBranchConfigurator> configure)
            {
                return AddBranch(name, null, configure);
            }

            public CommandBranchRegistration AddBranch(string name, Type settingsType, Action<IUnsafeBranchConfigurator> configure)
            {
                return _configurator.AddBranchUnsafe(
                    name,
                    settingsType,
                    branch => configure?.Invoke(new UnsafeBranchConfigurator(branch)));
            }
        }
    }
}
