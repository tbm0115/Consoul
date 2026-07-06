using ConsoulLibrary.Color;
using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Controls how <see cref="LayoutEntry"/> arranges panes.
    /// </summary>
    public class LayoutOptions
    {
        /// <summary>
        /// Gets or sets the requested layout width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the primary layout direction.
        /// </summary>
        public LayoutDirection Direction { get; set; } = LayoutDirection.Columns;

        /// <summary>
        /// Gets or sets the preferred number of columns when <see cref="Direction"/> is <see cref="LayoutDirection.Columns"/>.
        /// </summary>
        public int? Columns { get; set; }

        /// <summary>
        /// Gets or sets the minimum pane width before the layout uses fewer columns.
        /// </summary>
        public int MinimumPaneWidth { get; set; } = 18;

        /// <summary>
        /// Gets or sets the number of spaces between side-by-side panes.
        /// </summary>
        public int Gutter { get; set; } = 2;

        /// <summary>
        /// Gets or sets the number of blank lines between row-oriented panes.
        /// </summary>
        public int RowSpacing { get; set; } = 1;

        /// <summary>
        /// Gets or sets the color scheme used for pane titles.
        /// </summary>
        public ColorScheme? TitleScheme { get; set; }

        /// <summary>
        /// Gets or sets the default color scheme used for pane content.
        /// </summary>
        public ColorScheme? ContentScheme { get; set; }

        /// <summary>
        /// Creates a copy of the current layout options.
        /// </summary>
        /// <returns>A new <see cref="LayoutOptions"/> instance with the same values.</returns>
        public LayoutOptions Clone()
        {
            return new LayoutOptions
            {
                Width = Width,
                Direction = Direction,
                Columns = Columns,
                MinimumPaneWidth = MinimumPaneWidth,
                Gutter = Gutter,
                RowSpacing = RowSpacing,
                TitleScheme = TitleScheme,
                ContentScheme = ContentScheme
            };
        }

        internal int ResolveGutter()
        {
            return Math.Max(0, Gutter);
        }

        internal int ResolveMinimumPaneWidth()
        {
            return Math.Max(1, MinimumPaneWidth);
        }

        internal int ResolveRowSpacing()
        {
            return Math.Max(0, RowSpacing);
        }

        internal ColorScheme ResolveTitleScheme()
        {
            return TitleScheme ?? RenderOptions.SubnoteScheme;
        }

        internal ColorScheme ResolveContentScheme()
        {
            return ContentScheme ?? RenderOptions.DefaultScheme;
        }
    }

    /// <summary>
    /// Describes the primary direction used by <see cref="LayoutEntry"/>.
    /// </summary>
    public enum LayoutDirection
    {
        /// <summary>
        /// Render panes side by side when the terminal is wide enough.
        /// </summary>
        Columns,

        /// <summary>
        /// Render panes one after another.
        /// </summary>
        Rows
    }
}
