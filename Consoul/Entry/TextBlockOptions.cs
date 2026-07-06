using ConsoulLibrary.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for a responsive text block.
    /// </summary>
    public class TextBlockOptions
    {
        /// <summary>
        /// Gets or sets the requested text block width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the alignment used for each wrapped line.
        /// </summary>
        public TextBlockAlignment Alignment { get; set; } = TextBlockAlignment.Left;

        /// <summary>
        /// Gets or sets how content wider than the available line width should be handled.
        /// </summary>
        public Overflow Overflow { get; set; } = Overflow.Wrap;

        /// <summary>
        /// Gets or sets the number of spaces written before each line.
        /// </summary>
        public int LeftPadding { get; set; }

        /// <summary>
        /// Gets or sets the number of spaces written after each line.
        /// </summary>
        public int RightPadding { get; set; }

        /// <summary>
        /// Gets or sets the number of blank lines written before the content.
        /// </summary>
        public int TopPadding { get; set; }

        /// <summary>
        /// Gets or sets the number of blank lines written after the content.
        /// </summary>
        public int BottomPadding { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for text block content.
        /// </summary>
        public ColorScheme? Scheme { get; set; }

        /// <summary>
        /// Creates a copy of the current text block options.
        /// </summary>
        /// <returns>A new <see cref="TextBlockOptions"/> instance with the same values.</returns>
        public TextBlockOptions Clone()
        {
            return new TextBlockOptions
            {
                Width = Width,
                Alignment = Alignment,
                Overflow = Overflow,
                LeftPadding = LeftPadding,
                RightPadding = RightPadding,
                TopPadding = TopPadding,
                BottomPadding = BottomPadding,
                Scheme = Scheme
            };
        }

        internal ColorScheme ResolveScheme()
        {
            return Scheme ?? RenderOptions.DefaultScheme;
        }
    }

    /// <summary>
    /// Defines alignment for text block lines.
    /// </summary>
    public enum TextBlockAlignment
    {
        /// <summary>
        /// Aligns text to the left side of the text block.
        /// </summary>
        Left,

        /// <summary>
        /// Centers text inside the text block.
        /// </summary>
        Center,

        /// <summary>
        /// Aligns text to the right side of the text block.
        /// </summary>
        Right
    }
}
