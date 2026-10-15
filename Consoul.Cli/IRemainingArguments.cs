using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents command-line arguments captured after known arguments and options are bound.
    /// </summary>
    public interface IRemainingArguments
    {
        /// <summary>
        /// Gets the captured arguments exactly as they appeared after command parsing.
        /// </summary>
        IReadOnlyList<string> Raw { get; }

        /// <summary>
        /// Gets option-shaped captured arguments grouped by option name.
        /// </summary>
        ILookup<string, string> Parsed { get; }
    }
}
