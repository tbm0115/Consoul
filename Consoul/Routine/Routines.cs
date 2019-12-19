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
            if (idxRoutineFlag < 0)
                return; // Continue without error

            if ((idxRoutineFlag + 1) > args.Length - 1)
                throw new IndexOutOfRangeException();

            Assembly assembly = Assembly.GetCallingAssembly();

            Type[] allTypes = assembly.GetTypes();

            Type[] routineTypes = allTypes.Where(o => o.BaseType == typeof(Routine)).ToArray();
            if (!routineTypes.Any())
                throw new NotImplementedException();

            Type routineType = routineTypes.FirstOrDefault(o => o.Name.Equals(args[idxRoutineFlag + 1], StringComparison.OrdinalIgnoreCase));
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
