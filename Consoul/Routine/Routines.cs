using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary {
    public static class Routines {
        public static List<RegisteredOption> PromptRegistry { get; set; } = new List<RegisteredOption>();

        public static Queue<RoutineInput> InputBuffer { get; private set; } = new Queue<RoutineInput>();

        public static void InitializeRoutine(Routine routine, string name = null) {
            InputBuffer = routine;
            UseDelays = routine.UseDelays;
            if (!string.IsNullOrEmpty(name))
            {
                Console.Title = $"Routine: {name} - " + Console.Title;
            }
        }

        public static void InitializeRoutine(string[] args)
        {
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

            int idxXmlRoutineNameFlag = args.ToList().IndexOf("-Name");
            if (idxXmlRoutineNameFlag == (idxXmlRoutineFlag + 2) && (idxXmlRoutineNameFlag + 1) < args.Length)
                xRoutine = new XmlRoutine(filePath, args[idxXmlRoutineNameFlag + 1]);
            else
                xRoutine = new XmlRoutine(filePath);
            
            InitializeRoutine(xRoutine);
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
            InitializeRoutine(routine);

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
