using System;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Describes a reusable table title with optional Spectre-style justification and style metadata.
    /// </summary>
    /// <remarks>
    /// Consoul projects this value into <see cref="TableRenderOptions.Title"/> so manual POC tables and MVC-style
    /// table views continue to share the same renderer.
    /// </remarks>
    public sealed class TableTitle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableTitle"/> class.
        /// </summary>
        /// <param name="text">The title text.</param>
        /// <param name="justification">Optional title justification.</param>
        public TableTitle(string text, Justify? justification = null)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Justification = justification;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableTitle"/> class with a console foreground color.
        /// </summary>
        /// <param name="text">The title text.</param>
        /// <param name="color">The title foreground color.</param>
        /// <param name="justification">Optional title justification.</param>
        public TableTitle(string text, ConsoleColor color, Justify? justification = null)
            : this(text, new Style(color), justification)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableTitle"/> class.
        /// </summary>
        /// <param name="text">The title text.</param>
        /// <param name="color">The title foreground color.</param>
        /// <param name="justification">Optional title justification.</param>
        public TableTitle(string text, SpectreColor color, Justify? justification = null)
            : this(text, new Style(color), justification)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableTitle"/> class.
        /// </summary>
        /// <param name="text">The title text.</param>
        /// <param name="color">The title foreground color.</param>
        /// <param name="justification">Optional title justification.</param>
        public TableTitle(string text, RgbColor color, Justify? justification = null)
            : this(text, new Style(color), justification)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableTitle"/> class with Spectre-style style text.
        /// </summary>
        /// <param name="text">The title text.</param>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <param name="justification">Optional title justification.</param>
        public TableTitle(string text, string style, Justify? justification = null)
            : this(text, ConsoulLibrary.Style.Parse(style), justification)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableTitle"/> class with a reusable style object.
        /// </summary>
        /// <param name="text">The title text.</param>
        /// <param name="style">The title style.</param>
        /// <param name="justification">Optional title justification.</param>
        public TableTitle(string text, Style style, Justify? justification = null)
            : this(text, justification)
        {
            Style = style;
        }

        /// <summary>
        /// Gets the title text.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets or sets the optional title justification.
        /// </summary>
        public Justify? Justification { get; set; }

        /// <summary>
        /// Gets or sets the optional title style.
        /// </summary>
        public Style Style { get; set; }

        /// <summary>
        /// Sets the title justification.
        /// </summary>
        /// <param name="justification">The title justification.</param>
        /// <returns>The current <see cref="TableTitle"/> instance.</returns>
        public TableTitle SetAlignment(Justify justification)
        {
            Justification = justification;
            return this;
        }

        /// <summary>
        /// Sets the title style.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="TableTitle"/> instance.</returns>
        public TableTitle SetStyle(Style style)
        {
            Style = style;
            return this;
        }

        /// <summary>
        /// Sets the title foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TableTitle"/> instance.</returns>
        public TableTitle Color(ConsoleColor color)
        {
            return SetStyle(new Style(color));
        }

        /// <summary>
        /// Sets the title foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TableTitle"/> instance.</returns>
        public TableTitle Color(SpectreColor color)
        {
            return SetStyle(new Style(color));
        }

        /// <summary>
        /// Sets the title foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TableTitle"/> instance.</returns>
        public TableTitle Color(RgbColor color)
        {
            return SetStyle(new Style(color));
        }

        /// <summary>
        /// Sets the title style from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="TableTitle"/> instance.</returns>
        public TableTitle Color(string style)
        {
            return SetStyle(ConsoulLibrary.Style.Parse(style));
        }
    }
}
