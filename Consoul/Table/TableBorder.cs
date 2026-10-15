namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a named table border preset for Spectre-style table builder snippets.
    /// </summary>
    /// <remarks>
    /// The preset maps onto Consoul's existing <see cref="TableBorderStyle"/> renderer and may also carry line-visibility
    /// settings for presets such as <see cref="None"/>.
    /// </remarks>
    public sealed class TableBorder
    {
        private TableBorder(
            TableBorderStyle style,
            bool? headerHorizontal = null,
            bool? headerVertical = null,
            bool? contentHorizontal = null,
            bool? contentVertical = null)
        {
            Style = style;
            HeaderHorizontal = headerHorizontal;
            HeaderVertical = headerVertical;
            ContentHorizontal = contentHorizontal;
            ContentVertical = contentVertical;
        }

        /// <summary>
        /// Gets a preset that hides all table border and separator lines.
        /// </summary>
        public static TableBorder None { get; } = new TableBorder(
            TableBorderStyle.Simple,
            headerHorizontal: false,
            headerVertical: false,
            contentHorizontal: false,
            contentVertical: false);

        /// <summary>
        /// Gets the legacy Consoul repeated-line table style.
        /// </summary>
        public static TableBorder Simple { get; } = new TableBorder(TableBorderStyle.Simple);

        /// <summary>
        /// Gets a simple heavy-line table style alias used by common Spectre-style snippets.
        /// </summary>
        public static TableBorder SimpleHeavy { get; } = new TableBorder(TableBorderStyle.Heavy);

        /// <summary>
        /// Gets a minimal table style that hides all border and separator lines.
        /// </summary>
        public static TableBorder Minimal { get; } = None;

        /// <summary>
        /// Gets a minimal table style with the closest available heavy header separator.
        /// </summary>
        public static TableBorder MinimalHeavyHead { get; } = new TableBorder(
            TableBorderStyle.Heavy,
            headerHorizontal: true,
            headerVertical: false,
            contentHorizontal: false,
            contentVertical: false);

        /// <summary>
        /// Gets a minimal table style with the closest available double header separator.
        /// </summary>
        public static TableBorder MinimalDoubleHead { get; } = new TableBorder(
            TableBorderStyle.Double,
            headerHorizontal: true,
            headerVertical: false,
            contentHorizontal: false,
            contentVertical: false);

        /// <summary>
        /// Gets a horizontal-line-only table style.
        /// </summary>
        public static TableBorder Horizontal { get; } = new TableBorder(
            TableBorderStyle.Simple,
            headerVertical: false,
            contentVertical: false);

        /// <summary>
        /// Gets a square Unicode border preset.
        /// </summary>
        public static TableBorder Square { get; } = new TableBorder(TableBorderStyle.Square);

        /// <summary>
        /// Gets a rounded Unicode border preset.
        /// </summary>
        public static TableBorder Rounded { get; } = new TableBorder(TableBorderStyle.Rounded);

        /// <summary>
        /// Gets an ASCII border preset.
        /// </summary>
        public static TableBorder Ascii { get; } = new TableBorder(TableBorderStyle.Ascii);

        /// <summary>
        /// Gets an alternate ASCII border preset alias used by common Spectre-style snippets.
        /// </summary>
        public static TableBorder Ascii2 { get; } = Ascii;

        /// <summary>
        /// Gets an ASCII border preset with the closest available emphasized header separator.
        /// </summary>
        public static TableBorder AsciiDoubleHead { get; } = new TableBorder(
            TableBorderStyle.Ascii,
            contentHorizontal: false);

        /// <summary>
        /// Gets a markdown-friendly ASCII table style.
        /// </summary>
        public static TableBorder Markdown { get; } = new TableBorder(
            TableBorderStyle.Ascii,
            contentHorizontal: false);

        /// <summary>
        /// Gets a heavy Unicode border preset.
        /// </summary>
        public static TableBorder Heavy { get; } = new TableBorder(TableBorderStyle.Heavy);

        /// <summary>
        /// Gets a heavy Unicode border preset with the closest available edge emphasis.
        /// </summary>
        public static TableBorder HeavyEdge { get; } = Heavy;

        /// <summary>
        /// Gets a heavy Unicode border preset with the closest available header emphasis.
        /// </summary>
        public static TableBorder HeavyHead { get; } = new TableBorder(
            TableBorderStyle.Heavy,
            contentHorizontal: false);

        /// <summary>
        /// Gets a double-line Unicode border preset.
        /// </summary>
        public static TableBorder Double { get; } = new TableBorder(TableBorderStyle.Double);

        /// <summary>
        /// Gets a double-line Unicode border preset with the closest available edge emphasis.
        /// </summary>
        public static TableBorder DoubleEdge { get; } = Double;

        /// <summary>
        /// Gets a border preset that favors terminal-safe ASCII rendering.
        /// </summary>
        public static TableBorder Safe { get; } = Ascii;

        internal TableBorderStyle Style { get; }

        internal bool? HeaderHorizontal { get; }

        internal bool? HeaderVertical { get; }

        internal bool? ContentHorizontal { get; }

        internal bool? ContentVertical { get; }

        /// <summary>
        /// Converts a table border preset into the underlying Consoul table border style.
        /// </summary>
        /// <param name="border">The table border preset.</param>
        public static implicit operator TableBorderStyle(TableBorder border)
        {
            return border != null ? border.Style : TableBorderStyle.Simple;
        }
    }
}
