using System.Collections.Generic;

namespace ConsoulLibrary {
    public abstract class Routine : Queue<string>
    {
        public Routine()
        {

        }

        public Routine(IEnumerable<string> collection) : base(collection)
        {
        }
    }
}
