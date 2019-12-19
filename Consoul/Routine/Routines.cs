using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary {
    public static class Routines {
        public static Queue<string> InputBuffer { get; private set; } = new Queue<string>();

        public static void InitializeRoutine(Routine routine) => InputBuffer = routine;

        public static void InitializeRoutine(string[] args)
        {
            int idxRoutineFlag = args.ToList().IndexOf("-Routine");
            if (idxRoutineFlag >= 0 && (idxRoutineFlag + 1) > args.Length - 1) {
                _loadRoutine(args[idxRoutineFlag + 1]);
                return; // Continue without error
            }

            int idxXmlRoutineFlag = args.ToList().IndexOf("-XmlRoutine");
            if (idxXmlRoutineFlag >= 0 && (idxXmlRoutineFlag + 1) > args.Length - 1) {
                _loadXmlRoutine(args[idxXmlRoutineFlag + 1]);
                return;
            }

        }
        private static void _loadXmlRoutine(string filepath){
            if (!System.IO.File.Exists(filepath))
                return; // Continue without error
            XmlRoutine xRoutine = new XmlRoutine(filepath);
            Routines.InitializeRoutine(xRoutine);
        }
        private static void _loadRoutine(string assemblyName) {
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
        }

        public static string Next()
        {
            if (InputBuffer.Count <= 0)
                throw new IndexOutOfRangeException();
            return InputBuffer.Dequeue();
        }

        public static string Peek()
        {
            if (InputBuffer.Count <= 0)
                throw new IndexOutOfRangeException();
            return InputBuffer.Peek();
        }

        public static bool HasBuffer() => InputBuffer.Any();

        public static bool MonitorInputs { get; set; } = false;

        public static Stack<string> UserInputs { get; private set; } = new Stack<string>();
    }
}
