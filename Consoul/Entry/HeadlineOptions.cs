using ConsoulLibrary.Color;
using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for responsive headline text.
    /// </summary>
    public class HeadlineOptions
    {
        /// <summary>
        /// Gets or sets the requested headline width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the headline rendering style.
        /// </summary>
        public HeadlineStyle Style { get; set; } = HeadlineStyle.Block;

        /// <summary>
        /// Gets or sets the alignment used for rendered headline lines.
        /// </summary>
        public TextBlockAlignment Alignment { get; set; } = TextBlockAlignment.Center;

        /// <summary>
        /// Gets or sets the character used to fill block-style headline glyphs.
        /// </summary>
        public char BlockCharacter { get; set; } = '#';

        /// <summary>
        /// Gets or sets the number of spaces inserted between block-style headline glyphs.
        /// </summary>
        public int BlockGap { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether rendered block-style lines should be padded to the full render width.
        /// </summary>
        public bool Pad { get; set; } = true;

        /// <summary>
        /// Gets or sets the character used for rule-style headline separators.
        /// </summary>
        public char RuleCharacter { get; set; } = '-';

        /// <summary>
        /// Gets or sets the color scheme used for headline text.
        /// </summary>
        public ColorScheme? Scheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for borders in boxed and rule styles.
        /// </summary>
        public ColorScheme? BorderScheme { get; set; }

        /// <summary>
        /// Creates a copy of the current headline options.
        /// </summary>
        /// <returns>A new <see cref="HeadlineOptions"/> instance with the same values.</returns>
        public HeadlineOptions Clone()
        {
            return new HeadlineOptions
            {
                Width = Width,
                Style = Style,
                Alignment = Alignment,
                BlockCharacter = BlockCharacter,
                BlockGap = BlockGap,
                Pad = Pad,
                RuleCharacter = RuleCharacter,
                Scheme = Scheme,
                BorderScheme = BorderScheme
            };
        }

        internal ColorScheme ResolveScheme()
        {
            return Scheme ?? RenderOptions.PromptScheme;
        }

        internal ColorScheme ResolveBorderScheme()
        {
            return BorderScheme ?? RenderOptions.SubnoteScheme;
        }
    }

    /// <summary>
    /// Defines available headline rendering styles.
    /// </summary>
    public enum HeadlineStyle
    {
        /// <summary>
        /// Renders ordinary aligned text.
        /// </summary>
        Plain,

        /// <summary>
        /// Renders text inside a bordered panel.
        /// </summary>
        Box,

        /// <summary>
        /// Renders text as a titled horizontal rule.
        /// </summary>
        Rule,

        /// <summary>
        /// Renders text as large ASCII block letters.
        /// </summary>
        Block
    }
}
