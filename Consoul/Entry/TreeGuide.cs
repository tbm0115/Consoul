namespace ConsoulLibrary
{
    /// <summary>
    /// Defines reusable tree guide line presets for Spectre-style tree builders.
    /// </summary>
    public sealed class TreeGuide
    {
        private TreeGuide(string branch, string lastBranch, string indent, string lastIndent)
        {
            Branch = branch ?? string.Empty;
            LastBranch = lastBranch ?? string.Empty;
            Indent = indent ?? string.Empty;
            LastIndent = lastIndent ?? string.Empty;
        }

        /// <summary>
        /// Gets an ASCII guide preset for maximum terminal compatibility.
        /// </summary>
        public static TreeGuide Ascii { get; } = new TreeGuide("|-- ", "`-- ", "|   ", "    ");

        /// <summary>
        /// Gets a Unicode line guide preset.
        /// </summary>
        public static TreeGuide Line { get; } = new TreeGuide("\u251c\u2500 ", "\u2570\u2500 ", "\u2502  ", "   ");

        /// <summary>
        /// Gets a double-line Unicode guide preset.
        /// </summary>
        public static TreeGuide DoubleLine { get; } = new TreeGuide("\u2560\u2550 ", "\u255a\u2550 ", "\u2551  ", "   ");

        /// <summary>
        /// Gets a heavy Unicode guide preset.
        /// </summary>
        public static TreeGuide BoldLine { get; } = new TreeGuide("\u2523\u2501 ", "\u2517\u2501 ", "\u2503  ", "   ");

        /// <summary>
        /// Gets the prefix used for a non-final child node.
        /// </summary>
        public string Branch { get; }

        /// <summary>
        /// Gets the prefix used for the final child node.
        /// </summary>
        public string LastBranch { get; }

        /// <summary>
        /// Gets the continuation prefix used below a non-final child node.
        /// </summary>
        public string Indent { get; }

        /// <summary>
        /// Gets the continuation prefix used below the final child node.
        /// </summary>
        public string LastIndent { get; }
    }
}
