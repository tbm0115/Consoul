using ConsoulLibrary.Color;
using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for responsive text columns.
    /// </summary>
    public class ColumnsOptions
    {
        /// <summary>
        /// Gets or sets the requested column layout width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether columns should expand to the active terminal width when no explicit <see cref="Width"/> is set.
        /// </summary>
        public bool Expand { get; set; } = true;

        /// <summary>
        /// Gets or sets the preferred number of columns. The renderer may use fewer columns when the terminal is narrow.
        /// </summary>
        public int? Columns { get; set; }

        /// <summary>
        /// Gets or sets the minimum width of each column before the layout stacks into fewer columns.
        /// </summary>
        public int MinimumColumnWidth { get; set; } = 16;

        /// <summary>
        /// Gets or sets the number of spaces between columns.
        /// </summary>
        public int Gutter { get; set; } = 3;

        /// <summary>
        /// Gets or sets the color scheme used for optional column headers.
        /// </summary>
        public ColorScheme? HeaderScheme { get; set; }

        /// <summary>
        /// Gets or sets the text decoration used for optional column headers on ANSI-capable terminals.
        /// </summary>
        public Decoration HeaderDecoration { get; set; }

        /// <summary>
        /// Gets or sets the original RGB foreground used for optional column headers on true-color terminals.
        /// </summary>
        public RgbColor? HeaderForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the original RGB background used for optional column headers on true-color terminals.
        /// </summary>
        public RgbColor? HeaderBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the default color scheme used for column content.
        /// </summary>
        public ColorScheme? ContentScheme { get; set; }

        /// <summary>
        /// Gets or sets the text decoration used for default column content on ANSI-capable terminals.
        /// </summary>
        public Decoration ContentDecoration { get; set; }

        /// <summary>
        /// Gets or sets the original RGB foreground used for default column content on true-color terminals.
        /// </summary>
        public RgbColor? ContentForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the original RGB background used for default column content on true-color terminals.
        /// </summary>
        public RgbColor? ContentBackgroundRgb { get; set; }

        /// <summary>
        /// Creates a copy of the current column options.
        /// </summary>
        /// <returns>A new <see cref="ColumnsOptions"/> instance with the same values.</returns>
        public ColumnsOptions Clone()
        {
            return new ColumnsOptions
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

        internal GridOptions ToGridOptions()
        {
            return new GridOptions
            {
                Width = Width,
                Expand = Expand,
                Columns = Columns,
                MinimumColumnWidth = Math.Max(1, MinimumColumnWidth),
                Gutter = Math.Max(0, Gutter),
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
    }
}
