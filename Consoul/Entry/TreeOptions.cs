using ConsoulLibrary.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for a responsive tree.
    /// </summary>
    public class TreeOptions
    {
        /// <summary>
        /// Gets or sets the requested tree width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the root node should be rendered.
        /// </summary>
        public bool IncludeRoot { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether tree nodes should render their children.
        /// </summary>
        public bool Expanded { get; set; } = true;

        /// <summary>
        /// Gets or sets the tree guide preset used for branch and indentation prefixes.
        /// </summary>
        public TreeGuide Guide
        {
            get => _guide;
            set
            {
                _guide = value;
                if (value == null)
                {
                    return;
                }

                Branch = value.Branch;
                LastBranch = value.LastBranch;
                Indent = value.Indent;
                LastIndent = value.LastIndent;
            }
        }

        /// <summary>
        /// Gets or sets the prefix used for a non-final child node.
        /// </summary>
        public string Branch { get; set; } = "|-- ";

        /// <summary>
        /// Gets or sets the prefix used for the final child node.
        /// </summary>
        public string LastBranch { get; set; } = "`-- ";

        /// <summary>
        /// Gets or sets the continuation prefix used below a non-final child node.
        /// </summary>
        public string Indent { get; set; } = "|   ";

        /// <summary>
        /// Gets or sets the continuation prefix used below a final child node.
        /// </summary>
        public string LastIndent { get; set; } = "    ";

        /// <summary>
        /// Gets or sets the color scheme used for branch prefixes.
        /// </summary>
        public ColorScheme? BranchScheme { get; set; }

        /// <summary>
        /// Gets or sets the default color scheme used for node text.
        /// </summary>
        public ColorScheme? TextScheme { get; set; }

        private TreeGuide _guide;

        /// <summary>
        /// Creates a copy of the current tree options.
        /// </summary>
        /// <returns>A new <see cref="TreeOptions"/> instance with the same values.</returns>
        public TreeOptions Clone()
        {
            return new TreeOptions
            {
                Width = Width,
                IncludeRoot = IncludeRoot,
                Expanded = Expanded,
                Guide = Guide,
                Branch = Branch,
                LastBranch = LastBranch,
                Indent = Indent,
                LastIndent = LastIndent,
                BranchScheme = BranchScheme,
                TextScheme = TextScheme
            };
        }

        internal ColorScheme ResolveBranchScheme()
        {
            return BranchScheme ?? RenderOptions.SubnoteScheme;
        }

        internal ColorScheme ResolveTextScheme()
        {
            return TextScheme ?? RenderOptions.DefaultScheme;
        }

        internal string ResolveBranch(bool isLast)
        {
            return isLast ? LastBranch ?? string.Empty : Branch ?? string.Empty;
        }

        internal string ResolveIndent(bool isLast)
        {
            return isLast ? LastIndent ?? string.Empty : Indent ?? string.Empty;
        }
    }
}
