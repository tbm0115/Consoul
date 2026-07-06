namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a named panel border preset for Spectre-style panel builder snippets.
    /// </summary>
    public sealed class BoxBorder
    {
        private BoxBorder(PanelBorderStyle style)
        {
            Style = style;
        }

        /// <summary>
        /// Gets a square Unicode border preset.
        /// </summary>
        public static BoxBorder Square { get; } = new BoxBorder(PanelBorderStyle.Square);

        /// <summary>
        /// Gets the default simple panel border preset.
        /// </summary>
        public static BoxBorder Simple { get; } = Square;

        /// <summary>
        /// Gets a simple heavy-line panel border alias used by common Spectre-style snippets.
        /// </summary>
        public static BoxBorder SimpleHeavy { get; } = new BoxBorder(PanelBorderStyle.Heavy);

        /// <summary>
        /// Gets a minimal panel border preset that renders panel content without a visible border.
        /// </summary>
        public static BoxBorder Minimal { get; } = new BoxBorder(PanelBorderStyle.None);

        /// <summary>
        /// Gets a minimal panel border preset with the closest available heavy-line rendering.
        /// </summary>
        public static BoxBorder MinimalHeavyHead { get; } = SimpleHeavy;

        /// <summary>
        /// Gets a minimal panel border preset with the closest available double-line rendering.
        /// </summary>
        public static BoxBorder MinimalDoubleHead { get; } = new BoxBorder(PanelBorderStyle.Double);

        /// <summary>
        /// Gets a horizontal-line panel border alias mapped to Consoul's square border renderer.
        /// </summary>
        public static BoxBorder Horizontal { get; } = Square;

        /// <summary>
        /// Gets a rounded Unicode border preset.
        /// </summary>
        public static BoxBorder Rounded { get; } = new BoxBorder(PanelBorderStyle.Rounded);

        /// <summary>
        /// Gets an ASCII border preset.
        /// </summary>
        public static BoxBorder Ascii { get; } = new BoxBorder(PanelBorderStyle.Ascii);

        /// <summary>
        /// Gets an alternate ASCII border preset alias used by common Spectre-style snippets.
        /// </summary>
        public static BoxBorder Ascii2 { get; } = Ascii;

        /// <summary>
        /// Gets an ASCII border preset with the closest available emphasized header rendering.
        /// </summary>
        public static BoxBorder AsciiDoubleHead { get; } = Ascii;

        /// <summary>
        /// Gets a markdown-friendly ASCII panel border alias.
        /// </summary>
        public static BoxBorder Markdown { get; } = Ascii;

        /// <summary>
        /// Gets a heavy Unicode border preset.
        /// </summary>
        public static BoxBorder Heavy { get; } = new BoxBorder(PanelBorderStyle.Heavy);

        /// <summary>
        /// Gets a heavy Unicode border preset with the closest available edge emphasis.
        /// </summary>
        public static BoxBorder HeavyEdge { get; } = Heavy;

        /// <summary>
        /// Gets a heavy Unicode border preset with the closest available header emphasis.
        /// </summary>
        public static BoxBorder HeavyHead { get; } = Heavy;

        /// <summary>
        /// Gets a double-line Unicode border preset.
        /// </summary>
        public static BoxBorder Double { get; } = new BoxBorder(PanelBorderStyle.Double);

        /// <summary>
        /// Gets a double-line Unicode border preset with the closest available edge emphasis.
        /// </summary>
        public static BoxBorder DoubleEdge { get; } = Double;

        /// <summary>
        /// Gets a border preset that favors terminal-safe ASCII rendering.
        /// </summary>
        public static BoxBorder Safe { get; } = Ascii;

        /// <summary>
        /// Gets a border preset that renders panel content without a visible border.
        /// </summary>
        public static BoxBorder None { get; } = new BoxBorder(PanelBorderStyle.None);

        internal PanelBorderStyle Style { get; }

        /// <summary>
        /// Converts a box border preset into the underlying Consoul panel border style.
        /// </summary>
        /// <param name="border">The panel border preset.</param>
        public static implicit operator PanelBorderStyle(BoxBorder border)
        {
            return border != null ? border.Style : PanelBorderStyle.Custom;
        }
    }
}
