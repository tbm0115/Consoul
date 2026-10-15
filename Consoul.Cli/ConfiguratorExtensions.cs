using System;
using System.Globalization;
using System.Reflection;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style extension methods for configuring command applications.
    /// </summary>
    public static class ConfiguratorExtensions
    {
        /// <summary>
        /// Applies an inline configuration callback and returns the original configurator.
        /// </summary>
        /// <typeparam name="TConfigurator">The configurator type.</typeparam>
        /// <param name="configurator">The configurator to update.</param>
        /// <param name="configure">The configuration callback.</param>
        /// <returns>The original configurator.</returns>
        public static TConfigurator And<TConfigurator>(this TConfigurator configurator, Action<TConfigurator> configure)
            where TConfigurator : IConfigurator
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            configure(configurator);
            return configurator;
        }

        /// <summary>
        /// Configures case sensitivity for command names, branches, aliases, and options.
        /// </summary>
        /// <param name="configurator">The configurator to update.</param>
        /// <param name="caseSensitivity">The case-sensitivity behavior.</param>
        /// <returns>The original configurator.</returns>
        public static IConfigurator CaseSensitivity(this IConfigurator configurator, ConsoulLibrary.CaseSensitivity caseSensitivity)
        {
            EnsureConfigurator(configurator).Settings.CaseSensitivity = caseSensitivity;
            return configurator;
        }

        /// <summary>
        /// Sets the culture used when binding command setting values.
        /// </summary>
        /// <param name="configurator">The configurator to update.</param>
        /// <param name="culture">The application culture.</param>
        /// <returns>The original configurator.</returns>
        public static IConfigurator SetApplicationCulture(this IConfigurator configurator, CultureInfo culture)
        {
            EnsureConfigurator(configurator).Settings.ApplicationCulture = culture ?? CultureInfo.InvariantCulture;
            return configurator;
        }

        /// <summary>
        /// Uses the entry assembly informational version as the application version.
        /// </summary>
        /// <param name="configurator">The configurator to update.</param>
        /// <returns>The original configurator.</returns>
        public static IConfigurator UseAssemblyInformationalVersion(this IConfigurator configurator)
        {
            EnsureConfigurator(configurator);
            Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
            string version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (string.IsNullOrWhiteSpace(version))
            {
                version = assembly.GetName().Version?.ToString();
            }

            configurator.SetApplicationVersion(version ?? string.Empty);
            return configurator;
        }

        /// <summary>
        /// Enables strict parsing of unknown option-shaped tokens.
        /// </summary>
        /// <param name="configurator">The configurator to update.</param>
        /// <returns>The original configurator.</returns>
        public static IConfigurator UseStrictParsing(this IConfigurator configurator)
        {
            EnsureConfigurator(configurator).SetStrictParsing();
            return configurator;
        }

        /// <summary>
        /// Adds a branch of related subcommands with shared settings using a typed branch configurator callback.
        /// </summary>
        /// <typeparam name="TSettings">The branch settings type.</typeparam>
        /// <param name="configurator">The root configurator to update.</param>
        /// <param name="name">The branch name.</param>
        /// <param name="configure">The branch configuration callback.</param>
        /// <returns>The branch registration.</returns>
        public static CommandBranchRegistration AddBranch<TSettings>(
            this IConfigurator configurator,
            string name,
            Action<IConfigurator<TSettings>> configure)
            where TSettings : CommandSettings
        {
            EnsureConfigurator(configurator);
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            return configurator.AddBranch<TSettings>(name, branch => configure(ToTypedConfigurator<TSettings>(branch)));
        }

        /// <summary>
        /// Adds a nested branch with shared settings using a typed branch configurator callback.
        /// </summary>
        /// <typeparam name="TSettings">The branch settings type.</typeparam>
        /// <param name="configurator">The parent branch configurator to update.</param>
        /// <param name="name">The branch name.</param>
        /// <param name="configure">The branch configuration callback.</param>
        /// <returns>The branch registration.</returns>
        public static CommandBranchRegistration AddBranch<TSettings>(
            this IBranchConfigurator configurator,
            string name,
            Action<IConfigurator<TSettings>> configure)
            where TSettings : CommandSettings
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            return configurator.AddBranch<TSettings>(name, branch => configure(ToTypedConfigurator<TSettings>(branch)));
        }

        private static IConfigurator EnsureConfigurator(IConfigurator configurator)
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            return configurator;
        }

        private static IConfigurator<TSettings> ToTypedConfigurator<TSettings>(CommandBranchRegistration branch)
            where TSettings : CommandSettings
        {
            return (IConfigurator<TSettings>)(IConfigurator<CommandSettings>)branch;
        }
    }
}
