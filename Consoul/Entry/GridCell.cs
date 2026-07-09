using ConsoulLibrary.Color;
using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a cell rendered by <see cref="GridEntry"/>.
    /// </summary>
    public class GridCell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GridCell"/> class.
        /// </summary>
        /// <param name="content">The cell content.</param>
        /// <param name="header">Optional header rendered above the content.</param>
        /// <param name="scheme">Optional color scheme used for the content.</param>
        public GridCell(string content, string header = null, ColorScheme? scheme = null)
            : this(content, header, scheme, Decoration.None, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridCell"/> class.
        /// </summary>
        /// <param name="content">The cell content.</param>
        /// <param name="header">Optional header rendered above the content.</param>
        /// <param name="scheme">Optional color scheme used for the content.</param>
        /// <param name="decoration">Optional text decoration metadata used for the content.</param>
        /// <param name="foregroundRgb">Optional RGB foreground metadata used for true-color terminals.</param>
        /// <param name="backgroundRgb">Optional RGB background metadata used for true-color terminals.</param>
        public GridCell(
            string content,
            string header,
            ColorScheme? scheme,
            Decoration decoration,
            RgbColor? foregroundRgb,
            RgbColor? backgroundRgb)
        {
            Content = content ?? string.Empty;
            Header = header;
            Scheme = scheme;
            Decoration = decoration;
            ForegroundRgb = foregroundRgb;
            BackgroundRgb = backgroundRgb;
        }

        /// <summary>
        /// Gets or sets the cell content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the optional cell header.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Gets or sets the optional color scheme used for the content.
        /// </summary>
        public ColorScheme? Scheme { get; set; }

        /// <summary>
        /// Gets or sets optional decoration metadata used for the content.
        /// </summary>
        public Decoration Decoration { get; set; }

        /// <summary>
        /// Gets or sets optional RGB foreground metadata used for true-color terminals.
        /// </summary>
        public RgbColor? ForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets optional RGB background metadata used for true-color terminals.
        /// </summary>
        public RgbColor? BackgroundRgb { get; set; }

        /// <summary>
        /// Creates a grid cell with a foreground color.
        /// </summary>
        /// <param name="content">The cell content.</param>
        /// <param name="header">Optional header rendered above the content.</param>
        /// <param name="color">Optional foreground color used for the content.</param>
        /// <returns>A configured <see cref="GridCell"/>.</returns>
        public static GridCell Create(string content, string header = null, ConsoleColor? color = null)
        {
            ColorScheme? scheme = null;
            if (color.HasValue)
            {
                scheme = new ColorScheme
                {
                    Color = color.Value,
                    BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
                };
            }

            return new GridCell(content, header, scheme);
        }

        /// <summary>
        /// Creates a grid cell with Spectre-style style text.
        /// </summary>
        /// <param name="content">The cell content.</param>
        /// <param name="header">Optional header rendered above the content.</param>
        /// <param name="style">Optional style used for the content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <returns>A configured <see cref="GridCell"/>.</returns>
        public static GridCell Create(string content, string header, string style)
        {
            return Create(content, header, string.IsNullOrWhiteSpace(style) ? null : Style.Parse(style));
        }

        /// <summary>
        /// Creates a grid cell with a reusable style.
        /// </summary>
        /// <param name="content">The cell content.</param>
        /// <param name="header">Optional header rendered above the content.</param>
        /// <param name="style">Optional style used for the content.</param>
        /// <returns>A configured <see cref="GridCell"/>.</returns>
        public static GridCell Create(string content, string header, Style style)
        {
            if (style == null)
            {
                return new GridCell(content, header);
            }

            return new GridCell(
                content,
                header,
                style.ToColorScheme(RenderOptions.DefaultScheme),
                style.Decoration,
                style.ForegroundRgb,
                style.BackgroundRgb);
        }
    }
}
