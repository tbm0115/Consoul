using ConsoulLibrary.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for aligned text or captured render content.
    /// </summary>
    public class AlignOptions
    {
        /// <summary>
        /// Gets or sets the requested alignment width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the optional alignment height. When unset, the content renders at its natural height.
        /// </summary>
        public int? Height { get; set; }

        /// <summary>
        /// Gets or sets how each content line is aligned inside the requested width.
        /// </summary>
        public TextBlockAlignment Alignment { get; set; } = TextBlockAlignment.Left;

        /// <summary>
        /// Gets or sets how content is aligned vertically when <see cref="Height"/> is set.
        /// </summary>
        public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Top;

        /// <summary>
        /// Gets or sets the color scheme used for aligned content.
        /// </summary>
        public ColorScheme? Scheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for aligned content.
        /// </summary>
        public Decoration Decoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for aligned content on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for aligned content on ANSI-capable terminals.
        /// </summary>
        public RgbColor? BackgroundRgb { get; set; }

        /// <summary>
        /// Creates a copy of the current alignment options.
        /// </summary>
        /// <returns>A new <see cref="AlignOptions"/> instance with the same values.</returns>
        public AlignOptions Clone()
        {
            return new AlignOptions
            {
                Width = Width,
                Height = Height,
                Alignment = Alignment,
                VerticalAlignment = VerticalAlignment,
                Scheme = Scheme,
                Decoration = Decoration,
                ForegroundRgb = ForegroundRgb,
                BackgroundRgb = BackgroundRgb
            };
        }

        internal ColorScheme ResolveScheme()
        {
            return Scheme ?? RenderOptions.DefaultScheme;
        }

        internal StyledText ResolveStyle()
        {
            return new StyledText(ResolveScheme(), Decoration, ForegroundRgb, BackgroundRgb);
        }

        internal readonly struct StyledText
        {
            public StyledText(ColorScheme scheme, Decoration decoration, RgbColor? foregroundRgb, RgbColor? backgroundRgb)
            {
                Scheme = scheme;
                Decoration = decoration;
                ForegroundRgb = foregroundRgb;
                BackgroundRgb = backgroundRgb;
            }

            public ColorScheme Scheme { get; }

            public Decoration Decoration { get; }

            public RgbColor? ForegroundRgb { get; }

            public RgbColor? BackgroundRgb { get; }
        }
    }
}
