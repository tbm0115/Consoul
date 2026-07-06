using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ConsoulLibrary
{
    /// <summary>
    /// Manages scripted routine input playback and recording state.
    /// </summary>
    public static class Routines
    {
        private static RoutineSettingsSection appSettings = null;

        /// <summary>
        /// Gets or sets the prompt options registered for the current prompt.
        /// </summary>
        public static List<RegisteredOption> PromptRegistry { get; set; } = new List<RegisteredOption>();

        /// <summary>
        /// Gets the queued routine inputs waiting to be consumed.
        /// </summary>
        public static Queue<RoutineInput> InputBuffer { get; private set; } = new Queue<RoutineInput>();

        /// <summary>
        /// Gets the configured routine settings, loading default settings on first use.
        /// </summary>
        /// <returns>The active routine settings section.</returns>
        public static RoutineSettingsSection getAppSettings()
        {
            if (appSettings == null)
            {
                appSettings = LoadDefaultAppSettings();
            }

            return appSettings;
        }

        /// <summary>
        /// Configures routine settings used by input transforms.
        /// </summary>
        /// <param name="settings">Routine settings to use.</param>
        public static void ConfigureAppSettings(RoutineSettingsSection settings)
        {
            appSettings = settings ?? RoutineSettingsSection.Empty;
        }

        /// <summary>
        /// Configures transform values under the routine settings Transforms section.
        /// </summary>
        /// <param name="transforms">Transform values keyed by transform name.</param>
        public static void ConfigureTransforms(IReadOnlyDictionary<string, string> transforms)
        {
            appSettings = RoutineSettingsSection.FromTransforms(transforms);
        }

        /// <summary>
        /// Sets the active routine input queue.
        /// </summary>
        /// <param name="routine">Routine to activate.</param>
        /// <param name="name">Optional routine name shown in the console title.</param>
        public static void InitializeRoutine(Routine routine, string name = null)
        {
            InputBuffer = routine;
            UseDelays = routine.UseDelays;
            if (!string.IsNullOrEmpty(name))
            {
                string title = Consoul.ConsoleDriver.Title;
                Consoul.ConsoleDriver.Title = string.IsNullOrEmpty(title)
                    ? $"Routine: {name}"
                    : $"Routine: {name} - {title}";
            }
        }

        /// <summary>
        /// Initializes a routine from command-line arguments.
        /// </summary>
        /// <param name="args">Command-line arguments used to locate routine options.</param>
        /// <param name="configuration">Optional configuration object used to load routine settings.</param>
        public static void InitializeRoutine(string[] args, object configuration = null)
        {
            if (configuration != null)
            {
                ConfigureAppSettings(RoutineSettingsSection.FromConfigurationObject(configuration));
            }

            if (_checkRoutine(args))
            {
                return;
            }

            if (_checkXmlRoutine(args))
            {
                return;
            }
        }

        private static RoutineSettingsSection LoadDefaultAppSettings()
        {
            try
            {
                var assemblyLoc = Assembly.GetExecutingAssembly().Location;
                var assemblyDirectory = Path.GetDirectoryName(assemblyLoc);
                var candidates = new[]
                {
                    Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"),
                    string.IsNullOrEmpty(assemblyDirectory) ? null : Path.Combine(assemblyDirectory, "appsettings.json")
                };

                foreach (var candidate in candidates.Where(path => !string.IsNullOrEmpty(path)).Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    if (File.Exists(candidate))
                    {
                        return RoutineSettingsSection.FromJsonFile(candidate);
                    }
                }
            }
            catch
            {
            }

            return RoutineSettingsSection.Empty;
        }

        private static bool _checkXmlRoutine(string[] args)
        {
            string filePath = string.Empty;
            int idxXmlRoutineFlag = args.ToList().IndexOf("-XmlRoutine");
            if (idxXmlRoutineFlag < 0 || (idxXmlRoutineFlag + 1) >= args.Length)
            {
                return false;
            }

            filePath = args[idxXmlRoutineFlag + 1];
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Cannot find file.", filePath);
            }

            XmlRoutine xRoutine;
            string routineName = string.Empty;
            int idxXmlRoutineNameFlag = args.ToList().IndexOf("-Name");
            if (idxXmlRoutineNameFlag == (idxXmlRoutineFlag + 2) && (idxXmlRoutineNameFlag + 1) < args.Length)
            {
                routineName = args[idxXmlRoutineNameFlag + 1];
                xRoutine = new XmlRoutine(filePath, routineName);
                routineName = $"{xRoutine.Name}\\{routineName}";
            }
            else
            {
                xRoutine = new XmlRoutine(filePath);
                routineName = xRoutine.Name;
            }

            InitializeRoutine(xRoutine, routineName);
            return true;
        }

        private static bool _checkRoutine(string[] args)
        {
            string assemblyName = string.Empty;
            int idxRoutineFlag = args.ToList().IndexOf("-Routine");
            if (idxRoutineFlag < 0 || (idxRoutineFlag + 1) >= args.Length)
            {
                return false;
            }

            assemblyName = args[idxRoutineFlag + 1];
            Assembly assembly = Assembly.GetCallingAssembly();

            Type[] routineTypes = assembly.GetTypes()
                .Where(o => typeof(Routine).IsAssignableFrom(o) && !o.IsAbstract && o.GetConstructor(Type.EmptyTypes) != null)
                .ToArray();

            if (!routineTypes.Any())
            {
                throw new InvalidOperationException("No Routine types with parameterless constructors were found in assembly '" + assembly.GetName().Name + "'.");
            }

            Type routineType = routineTypes.FirstOrDefault(o => o.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase));
            if (routineType == null)
            {
                string availableRoutines = string.Join(", ", routineTypes.Select(o => o.Name).OrderBy(o => o));
                throw new KeyNotFoundException("Routine '" + assemblyName + "' was not found in assembly '" + assembly.GetName().Name + "'. Available routines: " + availableRoutines + ".");
            }

            Routine routine = routineType.GetConstructor(Type.EmptyTypes).Invoke(new object[0]) as Routine;
            if (routine == null)
            {
                throw new TypeLoadException("Routine '" + routineType.FullName + "' could not be created.");
            }

            InitializeRoutine(routine, routineType.Name);
            return true;
        }

        /// <summary>
        /// Dequeues the next routine input.
        /// </summary>
        /// <returns>The next routine input.</returns>
        public static RoutineInput Next()
        {
            if (InputBuffer.Count <= 0)
            {
                throw new IndexOutOfRangeException();
            }

            return InputBuffer.Dequeue();
        }

        /// <summary>
        /// Gets the value of the next routine input without removing it.
        /// </summary>
        /// <returns>The next routine input value.</returns>
        public static string Peek()
        {
            if (InputBuffer.Count <= 0)
            {
                throw new IndexOutOfRangeException();
            }

            return InputBuffer.Peek().Value;
        }

        /// <summary>
        /// Determines whether routine input is queued.
        /// </summary>
        /// <returns><c>true</c> when routine input is available; otherwise, <c>false</c>.</returns>
        public static bool HasBuffer() => InputBuffer.Any();

        /// <summary>
        /// Registers the options from the active selection prompt for routine matching.
        /// </summary>
        /// <param name="prompt">Prompt whose options should be registered.</param>
        public static void RegisterOptions(SelectionPrompt prompt)
        {
            ClearRegisteredOptions();
            PromptRegistry.AddRange(
                prompt
                    .Options
                    .Where(o => !o.IsSeparator)
                    .Select(o =>
                    {
                        var registered = new RegisteredOption()
                        {
                            Index = o.Index,
                            Prompt = prompt.Message,
                            Text = o.Label,
                            Description = o.Description,
                            Shortcut = o.Shortcut,
                            IsDisabled = o.IsDisabled,
                            DisabledReason = o.DisabledReason
                        };

                        foreach (string term in o.SearchTerms.Where(term => !string.IsNullOrWhiteSpace(term)))
                        {
                            registered.SearchTerms.Add(term.Trim());
                        }

                        return registered;
                    }));
        }

        /// <summary>
        /// Resolves a routine input against registered prompt options without mutating the original recorded value.
        /// </summary>
        /// <param name="input">Routine input to resolve.</param>
        /// <returns>The value that should be used by prompt parsing.</returns>
        internal static string ResolveInputValue(RoutineInput input)
        {
            if (input == null)
            {
                return string.Empty;
            }

            string value = input.Value;
            if (!PromptRegistry.Any())
            {
                return value;
            }

            RegisteredOption option = input.Method == RoutineInput.InputMethod.OptionText
                ? FindOptionByText(value)
                : FindOptionByUserInput(value);

            if (option != null)
            {
                input.OptionReference = option;
            }

            if (input.Method == RoutineInput.InputMethod.OptionText && option != null)
            {
                return (option.Index + 1).ToString(CultureInfo.InvariantCulture);
            }

            return value;
        }

        private static RegisteredOption FindOptionByUserInput(string value)
        {
            RegisteredOption numericOption = PromptRegistry.FirstOrDefault(o => (o.Index + 1).ToString(CultureInfo.InvariantCulture) == value);
            if (numericOption != null)
            {
                return numericOption;
            }

            return PromptRegistry.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Shortcut) &&
                string.Equals(o.Shortcut.Trim(), value, StringComparison.OrdinalIgnoreCase));
        }

        private static RegisteredOption FindOptionByText(string value)
        {
            return PromptRegistry.FirstOrDefault(o =>
                string.Equals(o.Text, value, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(o.Shortcut, value, StringComparison.OrdinalIgnoreCase) ||
                o.SearchTerms.Any(term => string.Equals(term, value, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Clears all registered prompt options.
        /// </summary>
        public static void ClearRegisteredOptions() => PromptRegistry.Clear();

        /// <summary>
        /// Gets or sets whether user inputs should be captured for routine recording.
        /// </summary>
        public static bool MonitorInputs { get; set; } = false;

        /// <summary>
        /// Gets or sets whether routine playback should honor recorded delays.
        /// </summary>
        public static bool UseDelays { get; set; } = false;

        /// <summary>
        /// Gets recorded user inputs.
        /// </summary>
        public static Stack<RoutineInput> UserInputs { get; private set; } = new Stack<RoutineInput>();
    }
}
