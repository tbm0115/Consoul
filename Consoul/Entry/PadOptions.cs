using ConsoulLibrary.Color;
using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for padded text or captured render content.
    /// </summary>
    public class PadOptions
    {
        /// <summary>
        /// Gets or sets the requested padded width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the number of spaces added to the left of each content line.
        /// </summary>
        public int Left { get; set; }

        /// <summary>
        /// Gets or sets the number of spaces added to the right of each content line.
        /// </summary>
        public int Right { get; set; }

        /// <summary>
        /// Gets or sets the number of blank lines added before the content.
        /// </summary>
        public int Top { get; set; }

        /// <summary>
        /// Gets or sets the number of blank lines added after the content.
        /// </summary>
        public int Bottom { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for padded content.
        /// </summary>
        public ColorScheme? Scheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for padded content.
        /// </summary>
        public Decoration Decoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for padded content on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for padded content on ANSI-capable terminals.
        /// </summary>
        public RgbColor? BackgroundRgb { get; set; }

        /// <summary>
        /// Creates a copy of the current padding options.
        /// </summary>
        /// <returns>A new <see cref="PadOptions"/> instance with the same values.</returns>
        public PadOptions Clone()
        {
            return new PadOptions
            {
                Width = Width,
                Left = Left,
                Right = Right,
                Top = Top,
                Bottom = Bottom,
                Scheme = Scheme,
                Decoration = Decoration,
                ForegroundRgb = ForegroundRgb,
                BackgroundRgb = BackgroundRgb
            };
        }

        internal int ResolveLeft()
        {
            return Math.Max(0, Left);
        }

        internal int ResolveRight()
        {
            return Math.Max(0, Right);
        }

        internal int ResolveTop()
        {
            return Math.Max(0, Top);
        }

        internal int ResolveBottom()
        {
            return Math.Max(0, Bottom);
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
