using ConsoulLibrary.Color;
using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for a responsive grid.
    /// </summary>
    public class GridOptions
    {
        /// <summary>
        /// Gets or sets the requested grid width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the grid should expand to the active terminal width when no explicit <see cref="Width"/> is set.
        /// </summary>
        public bool Expand { get; set; } = true;

        /// <summary>
        /// Gets or sets the preferred number of columns. The renderer may use fewer columns when the terminal is narrow.
        /// </summary>
        public int? Columns { get; set; }

        /// <summary>
        /// Gets or sets the minimum width of each column before the grid stacks into fewer columns.
        /// </summary>
        public int MinimumColumnWidth { get; set; } = 12;

        /// <summary>
        /// Gets or sets the number of spaces between columns.
        /// </summary>
        public int Gutter { get; set; } = 2;

        /// <summary>
        /// Gets or sets the color scheme used for headers.
        /// </summary>
        public ColorScheme? HeaderScheme { get; set; }

        /// <summary>
        /// Gets or sets the text decoration used for headers on ANSI-capable terminals.
        /// </summary>
        public Decoration HeaderDecoration { get; set; }

        /// <summary>
        /// Gets or sets the original RGB foreground used for headers on true-color terminals.
        /// </summary>
        public RgbColor? HeaderForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the original RGB background used for headers on true-color terminals.
        /// </summary>
        public RgbColor? HeaderBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the default color scheme used for cell content.
        /// </summary>
        public ColorScheme? ContentScheme { get; set; }

        /// <summary>
        /// Gets or sets the text decoration used for default cell content on ANSI-capable terminals.
        /// </summary>
        public Decoration ContentDecoration { get; set; }

        /// <summary>
        /// Gets or sets the original RGB foreground used for default cell content on true-color terminals.
        /// </summary>
        public RgbColor? ContentForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the original RGB background used for default cell content on true-color terminals.
        /// </summary>
        public RgbColor? ContentBackgroundRgb { get; set; }

        /// <summary>
        /// Creates a copy of the current grid options.
        /// </summary>
        /// <returns>A new <see cref="GridOptions"/> instance with the same values.</returns>
        public GridOptions Clone()
        {
            return new GridOptions
            {
                Width = Width,
                Expand = Expand,
                Columns = Columns,
                MinimumColumnWidth = MinimumColumnWidth,
                Gutter = Gutter,
                HeaderScheme = HeaderScheme,
                HeaderDecoration = HeaderDecoration,
                HeaderForegroundRgb = HeaderForegroundRgb,
                HeaderBackgroundRgb = HeaderBackgroundRgb,
                ContentScheme = ContentScheme,
                ContentDecoration = ContentDecoration,
                ContentForegroundRgb = ContentForegroundRgb,
                ContentBackgroundRgb = ContentBackgroundRgb
            };
        }

        internal int ResolveGutter()
        {
            return Math.Max(0, Gutter);
        }

        internal int ResolveMinimumColumnWidth()
        {
            return Math.Max(1, MinimumColumnWidth);
        }

        internal ColorScheme ResolveHeaderScheme()
        {
            return HeaderScheme ?? RenderOptions.SubnoteScheme;
        }

        internal ColorScheme ResolveContentScheme()
        {
            return ContentScheme ?? RenderOptions.DefaultScheme;
        }
    }
}
