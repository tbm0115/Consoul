using ConsoulLibrary.Color;
using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a row rendered by <see cref="RowsEntry"/>.
    /// </summary>
    public class RowsItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RowsItem"/> class.
        /// </summary>
        /// <param name="content">The row content.</param>
        /// <param name="scheme">Optional color scheme used for the row content.</param>
        public RowsItem(string content, ColorScheme? scheme = null)
            : this(content, scheme, Decoration.None, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RowsItem"/> class.
        /// </summary>
        /// <param name="content">The row content.</param>
        /// <param name="scheme">Optional color scheme used for the row content.</param>
        /// <param name="decoration">Optional text decoration metadata used for the row content.</param>
        /// <param name="foregroundRgb">Optional RGB foreground metadata used for true-color terminals.</param>
        /// <param name="backgroundRgb">Optional RGB background metadata used for true-color terminals.</param>
        public RowsItem(
            string content,
            ColorScheme? scheme,
            Decoration decoration,
            RgbColor? foregroundRgb,
            RgbColor? backgroundRgb)
        {
            Content = content ?? string.Empty;
            Scheme = scheme;
            Decoration = decoration;
            ForegroundRgb = foregroundRgb;
            BackgroundRgb = backgroundRgb;
        }

        /// <summary>
        /// Gets or sets the row content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the optional color scheme used for the row content.
        /// </summary>
        public ColorScheme? Scheme { get; set; }

        /// <summary>
        /// Gets or sets optional decoration metadata used for the row content.
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
        /// Creates a row item with an optional foreground color.
        /// </summary>
        /// <param name="content">The row content.</param>
        /// <param name="color">Optional foreground color used for the row content.</param>
        /// <returns>A configured <see cref="RowsItem"/>.</returns>
        public static RowsItem Create(string content, ConsoleColor? color = null)
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

            return new RowsItem(content, scheme);
        }

        /// <summary>
        /// Creates a row item with Spectre-style style text.
        /// </summary>
        /// <param name="content">The row content.</param>
        /// <param name="style">Optional style used for the row content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <returns>A configured <see cref="RowsItem"/>.</returns>
        public static RowsItem Create(string content, string style)
        {
            return Create(content, string.IsNullOrWhiteSpace(style) ? null : Style.Parse(style));
        }

        /// <summary>
        /// Creates a row item with a reusable style.
        /// </summary>
        /// <param name="content">The row content.</param>
        /// <param name="style">Optional style used for the row content.</param>
        /// <returns>A configured <see cref="RowsItem"/>.</returns>
        public static RowsItem Create(string content, Style style)
        {
            if (style == null)
            {
                return new RowsItem(content);
            }

            return new RowsItem(
                content,
                style.ToColorScheme(RenderOptions.DefaultScheme),
                style.Decoration,
                style.ForegroundRgb,
                style.BackgroundRgb);
        }
    }
}
