using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary {
    public abstract class Routine : Queue<RoutineInput> {
        public bool UseDelays { get; set; } = false;

        public Routine()
        {

        }

        public Routine(IEnumerable<string> collection) : base(collection.Select(o => new RoutineInput() { Value = o }))
        {
        }
    }
}
