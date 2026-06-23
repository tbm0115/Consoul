using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary {
    /// <summary>
    /// Base queue for scripted Consoul routine inputs.
    /// </summary>
    public abstract class Routine : Queue<RoutineInput> {
        /// <summary>
        /// Gets or sets whether playback should honor delays recorded between routine inputs.
        /// </summary>
        public bool UseDelays { get; set; } = false;

        /// <summary>
        /// Initializes an empty routine.
        /// </summary>
        public Routine()
        {

        }

        /// <summary>
        /// Initializes a routine from a sequence of raw input values.
        /// </summary>
        /// <param name="collection">Raw input values to enqueue.</param>
        public Routine(IEnumerable<string> collection) : base(collection.Select(o => new RoutineInput() { Value = o }))
        {
        }
    }
}
