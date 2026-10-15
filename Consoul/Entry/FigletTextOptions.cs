using ConsoulLibrary.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for large ASCII text.
    /// </summary>
    public class FigletTextOptions
    {
        /// <summary>
        /// Gets or sets the requested render width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the alignment used for rendered lines.
        /// </summary>
        public TextBlockAlignment Alignment { get; set; } = TextBlockAlignment.Center;

        /// <summary>
        /// Gets or sets the character used to fill the large text glyphs.
        /// </summary>
        public char Character { get; set; } = '#';

        /// <summary>
        /// Gets or sets a value indicating whether rendered lines should be padded to the full render width.
        /// </summary>
        public bool Pad { get; set; }

        /// <summary>
        /// Gets or sets the figlet layout mode used between glyphs.
        /// </summary>
        public FigletLayoutMode LayoutMode { get; set; } = FigletLayoutMode.FullSize;

        /// <summary>
        /// Gets or sets the color scheme used for the rendered text.
        /// </summary>
        public ColorScheme? Scheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for rendered glyphs.
        /// </summary>
        public Decoration Decoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for rendered glyphs on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for rendered glyphs on ANSI-capable terminals.
        /// </summary>
        public RgbColor? BackgroundRgb { get; set; }

        /// <summary>
        /// Creates a copy of the current figlet text options.
        /// </summary>
        /// <returns>A new <see cref="FigletTextOptions"/> instance with the same values.</returns>
        public FigletTextOptions Clone()
        {
            return new FigletTextOptions
            {
                Width = Width,
                Alignment = Alignment,
                Character = Character,
                Pad = Pad,
                LayoutMode = LayoutMode,
                Scheme = Scheme,
                Decoration = Decoration,
                ForegroundRgb = ForegroundRgb,
                BackgroundRgb = BackgroundRgb
            };
        }

        internal ColorScheme ResolveScheme()
        {
            return Scheme ?? RenderOptions.PromptScheme;
        }

        internal HeadlineOptions ToHeadlineOptions()
        {
            return new HeadlineOptions
            {
                Width = Width,
                Style = HeadlineStyle.Block,
                Alignment = Alignment,
                BlockCharacter = Character,
                Pad = Pad,
                BlockGap = LayoutMode == FigletLayoutMode.FullSize ? 1 : 0,
                Scheme = ResolveScheme(),
                Decoration = Decoration,
                ForegroundRgb = ForegroundRgb,
                BackgroundRgb = BackgroundRgb
            };
        }
    }
}
