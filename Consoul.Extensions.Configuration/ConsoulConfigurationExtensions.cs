using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace ConsoulLibrary
{
    /// <summary>
    /// Optional Microsoft.Extensions.Configuration helpers for Consoul routines.
    /// </summary>
    public static class ConsoulConfigurationExtensions
    {
        /// <summary>
        /// Converts the Consoul section of an <see cref="IConfiguration"/> into routine settings.
        /// </summary>
        /// <param name="configuration">Configuration root or section.</param>
        /// <returns>Routine settings.</returns>
        public static RoutineSettingsSection GetConsoulRoutineSettings(this IConfiguration configuration)
        {
            if (configuration == null)
            {
                return RoutineSettingsSection.Empty;
            }

            var configurationSection = configuration as IConfigurationSection;
            IConfiguration section = configurationSection != null && string.Equals(configurationSection.Key, "Consoul", System.StringComparison.OrdinalIgnoreCase)
                ? configuration
                : configuration.GetSection("Consoul");

            var values = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            CopyChildren(section, string.Empty, values);
            return new RoutineSettingsSection(values);
        }

        /// <summary>
        /// Configures Consoul routine transforms from the Consoul section of an <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="configuration">Configuration root or section.</param>
        public static void ConfigureConsoulRoutines(this IConfiguration configuration)
        {
            Routines.ConfigureAppSettings(configuration.GetConsoulRoutineSettings());
        }

        /// <summary>
        /// Configures Consoul routine transforms and initializes a routine from command-line arguments.
        /// </summary>
        /// <param name="configuration">Configuration root or section.</param>
        /// <param name="args">Command-line arguments.</param>
        public static void InitializeConsoulRoutine(this IConfiguration configuration, string[] args)
        {
            configuration.ConfigureConsoulRoutines();
            Routines.InitializeRoutine(args);
        }

        /// <summary>
        /// Loads Consoul routine settings from a JSON file.
        /// </summary>
        /// <param name="path">JSON file path.</param>
        /// <param name="optional">Whether the file is optional.</param>
        /// <returns>Routine settings.</returns>
        public static RoutineSettingsSection LoadConsoulRoutineSettingsFromJsonFile(string path, bool optional = false)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(path, optional)
                .Build();

            return configuration.GetConsoulRoutineSettings();
        }

        private static void CopyChildren(IConfiguration section, string path, IDictionary<string, string> values)
        {
            foreach (IConfigurationSection child in section.GetChildren())
            {
                string childPath = string.IsNullOrEmpty(path) ? child.Key : path + ":" + child.Key;
                if (child.Value != null)
                {
                    values[childPath] = child.Value;
                }

                CopyChildren(child, childPath, values);
            }
        }
    }
}
