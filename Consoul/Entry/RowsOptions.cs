using ConsoulLibrary.Color;
using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for vertically stacked rows.
    /// </summary>
    public class RowsOptions
    {
        /// <summary>
        /// Gets or sets the requested row width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether rows should expand to the active terminal width when no explicit <see cref="Width"/> is set.
        /// </summary>
        public bool Expand { get; set; } = true;

        /// <summary>
        /// Gets or sets the number of blank lines between rows.
        /// </summary>
        public int Spacing { get; set; } = 1;

        /// <summary>
        /// Gets or sets the default color scheme used for row content.
        /// </summary>
        public ColorScheme? ContentScheme { get; set; }

        /// <summary>
        /// Gets or sets the text decoration used for default row content on ANSI-capable terminals.
        /// </summary>
        public Decoration ContentDecoration { get; set; }

        /// <summary>
        /// Gets or sets the original RGB foreground used for default row content on true-color terminals.
        /// </summary>
        public RgbColor? ContentForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the original RGB background used for default row content on true-color terminals.
        /// </summary>
        public RgbColor? ContentBackgroundRgb { get; set; }

        /// <summary>
        /// Creates a copy of the current row options.
        /// </summary>
        /// <returns>A new <see cref="RowsOptions"/> instance with the same values.</returns>
        public RowsOptions Clone()
        {
            return new RowsOptions
            {
                Width = Width,
                Expand = Expand,
                Spacing = Spacing,
                ContentScheme = ContentScheme,
                ContentDecoration = ContentDecoration,
                ContentForegroundRgb = ContentForegroundRgb,
                ContentBackgroundRgb = ContentBackgroundRgb
            };
        }

        internal int ResolveSpacing()
        {
            return Math.Max(0, Spacing);
        }

        internal ColorScheme ResolveContentScheme()
        {
            return ContentScheme ?? RenderOptions.DefaultScheme;
        }
    }
}
