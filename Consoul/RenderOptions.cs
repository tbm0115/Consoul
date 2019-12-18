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
    }
    public abstract class Routine : Queue<string>
    {
        public Routine()
        {

        }

        public Routine(IEnumerable<string> collection) : base(collection)
        {
        }
    }

    public static class RenderOptions
    {
        /// <summary>
        /// Default color for general the Write method(s)
        /// </summary>
        public static ConsoleColor DefaultColor { get; set; } = ConsoleColor.White;

        /// <summary>
        /// Default color of main Prompt messages
        /// </summary>
        public static ConsoleColor PromptColor { get; set; } = ConsoleColor.Yellow;

        /// <summary>
        /// Default color for supplementary or help text
        /// </summary>
        public static ConsoleColor SubnoteColor { get; set; } = ConsoleColor.Gray;

        /// <summary>
        /// Default color for Invalid Operation messages
        /// </summary>
        public static ConsoleColor InvalidColor { get; set; } = ConsoleColor.Red;

        /// <summary>
        /// Default color for PromptOptions
        /// </summary>
        public static ConsoleColor OptionColor { get; set; } = ConsoleColor.DarkYellow;

        /// <summary>
        /// Default color for rendering Routine input values
        /// </summary>
        public static ConsoleColor RoutineInputColor { get; set; } = ConsoleColor.Cyan;

        /// <summary>
        /// Messages colored with this will not be rendered
        /// </summary>
        public static List<ConsoleColor> BlacklistColors { get; set; } = new List<ConsoleColor>();

        /// <summary>
        /// Default Write mode for the library
        /// </summary>
        public static WriteModes WriteMode { get; set; } = WriteModes.WriteAll;

        /// <summary>
        /// Default message for the 'Go Back' selection
        /// </summary>
        public static string DefaultGoBackMessage { get; set; } = "<==\tGo Back";

        public static string ContinueMessage { get; set; } = "Press enter to continue...";

        public enum WriteModes
        {
            WriteAll,
            SuppressAll,
            SuppressBlacklist
        }

        public static ConsoleColor GetColor(ConsoleColor? color = null)
        {
            if (color == null)
                color = DefaultColor;
            return (ConsoleColor)color;
        }
    }
}
