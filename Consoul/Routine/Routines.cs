using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace ConsoulLibrary {
    public static class Routines {
        public static List<RegisteredOption> PromptRegistry { get; set; } = new List<RegisteredOption>();

        public static Queue<RoutineInput> InputBuffer { get; private set; } = new Queue<RoutineInput>();

        private static IConfigurationSection appSettings = null;
        public static IConfigurationSection getAppSettings()
        {
            if (appSettings == null)
            {
                try
                {
                    var assemblyLoc = Assembly.GetExecutingAssembly().Location;
                    var directoryPath = Path.GetDirectoryName(assemblyLoc);

                    var configFilePath = Path.Combine(directoryPath, "appsettings.json");

                    IConfigurationBuilder builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile(configFilePath);

                    var configRoot = builder.Build();
                    appSettings = configRoot.GetSection("Consoul");
                }
                catch (Exception ex)
                {
                    return null;
                }
            }

            return appSettings;
        }

        public static void InitializeRoutine(Routine routine, string name = null) {
            InputBuffer = routine;
            UseDelays = routine.UseDelays;
            if (!string.IsNullOrEmpty(name))
            {
                Console.Title = $"Routine: {name} - " + Console.Title;
            }
        }

        public static void InitializeRoutine(string[] args, IConfigurationRoot configuration = null)
        {
            if (configuration != null)
                appSettings = configuration.GetSection("Consoul");

            if (_checkRoutine(args))
                return;
            if (_checkXmlRoutine(args))
                return;

        }
        private static bool _checkXmlRoutine(string[] args) {
            string filePath = string.Empty;
            int idxXmlRoutineFlag = args.ToList().IndexOf("-XmlRoutine");
            if (idxXmlRoutineFlag < 0 || (idxXmlRoutineFlag + 1) >= args.Length)
                return false;
            filePath = args[idxXmlRoutineFlag + 1];
            if (!System.IO.File.Exists(filePath))
                throw new System.IO.FileNotFoundException("Cannot find file.", filePath);

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
        private static bool _checkRoutine(string[] args) {
            string assemblyName = string.Empty;
            int idxRoutineFlag = args.ToList().IndexOf("-Routine");
            if (idxRoutineFlag < 0 || (idxRoutineFlag + 1) >= args.Length)
                return false; // Continue without error
            assemblyName = args[idxRoutineFlag + 1];
            Assembly assembly = Assembly.GetCallingAssembly();

            Type[] allTypes = assembly.GetTypes();

            Type[] routineTypes = allTypes.Where(o => o.BaseType == typeof(Routine)).ToArray();
            if (!routineTypes.Any())
                throw new NotImplementedException();

            Type routineType = routineTypes.FirstOrDefault(o => o.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase));
            if (routineType == null)
                throw new KeyNotFoundException();

            Routine routine = routineType.GetConstructor(new Type[0]).Invoke(new object[0]) as Routine;
            if (routine == null)
                throw new TypeLoadException();
            InitializeRoutine(routine, routineType.Name);

            return true;
        }

        public static RoutineInput Next()
        {
            if (InputBuffer.Count <= 0)
                throw new IndexOutOfRangeException();
            return InputBuffer.Dequeue();
        }

        public static string Peek()
        {
            if (InputBuffer.Count <= 0)
                throw new IndexOutOfRangeException();
            return InputBuffer.Peek().Value;
        }

        public static bool HasBuffer() => InputBuffer.Any();

        public static void RegisterOptions(Prompt prompt)
        {
            ClearRegisteredOptions();
            PromptRegistry
            .AddRange(
                prompt
                .Options
                .Select(o =>
                    new RegisteredOption()
                    {
                        Index = o.Index,
                        Prompt = prompt.Message,
                        Text = o.Label
                    }
                )
            );
        }

        public static void ClearRegisteredOptions() => PromptRegistry.Clear();

        public static bool MonitorInputs { get; set; } = false;

        public static bool UseDelays { get; set; } = false;

        public static Stack<RoutineInput> UserInputs { get; private set; } = new Stack<RoutineInput>();
    }
}
