using System;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Describes a reusable table caption with optional Spectre-style justification and style metadata.
    /// </summary>
    /// <remarks>
    /// Consoul projects this value into <see cref="TableRenderOptions.Caption"/> so captions stay on the existing
    /// responsive table rendering path.
    /// </remarks>
    public sealed class TableCaption
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableCaption"/> class.
        /// </summary>
        /// <param name="text">The caption text.</param>
        /// <param name="justification">Optional caption justification.</param>
        public TableCaption(string text, Justify? justification = null)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Justification = justification;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableCaption"/> class from a renderable component.
        /// </summary>
        /// <param name="caption">Renderable caption captured or adapted for the table caption.</param>
        /// <param name="justification">Optional caption justification.</param>
        public TableCaption(IConsoulRenderable caption, Justify? justification = null)
            : this(TableColumn.Capture(caption), justification)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableCaption"/> class with a console foreground color.
        /// </summary>
        /// <param name="text">The caption text.</param>
        /// <param name="color">The caption foreground color.</param>
        /// <param name="justification">Optional caption justification.</param>
        public TableCaption(string text, ConsoleColor color, Justify? justification = null)
            : this(text, new Style(color), justification)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableCaption"/> class from a renderable component with a console foreground color.
        /// </summary>
        /// <param name="caption">Renderable caption captured or adapted for the table caption.</param>
        /// <param name="color">The caption foreground color.</param>
        /// <param name="justification">Optional caption justification.</param>
        public TableCaption(IConsoulRenderable caption, ConsoleColor color, Justify? justification = null)
            : this(caption, new Style(color), justification)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableCaption"/> class.
        /// </summary>
        /// <param name="text">The caption text.</param>
        /// <param name="color">The caption foreground color.</param>
        /// <param name="justification">Optional caption justification.</param>
        public TableCaption(string text, SpectreColor color, Justify? justification = null)
            : this(text, new Style(color), justification)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableCaption"/> class from a renderable component.
        /// </summary>
        /// <param name="caption">Renderable caption captured or adapted for the table caption.</param>
        /// <param name="color">The caption foreground color.</param>
        /// <param name="justification">Optional caption justification.</param>
        public TableCaption(IConsoulRenderable caption, SpectreColor color, Justify? justification = null)
            : this(caption, new Style(color), justification)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableCaption"/> class.
        /// </summary>
        /// <param name="text">The caption text.</param>
        /// <param name="color">The caption foreground color.</param>
        /// <param name="justification">Optional caption justification.</param>
        public TableCaption(string text, RgbColor color, Justify? justification = null)
            : this(text, new Style(color), justification)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableCaption"/> class from a renderable component.
        /// </summary>
        /// <param name="caption">Renderable caption captured or adapted for the table caption.</param>
        /// <param name="color">The caption foreground color.</param>
        /// <param name="justification">Optional caption justification.</param>
        public TableCaption(IConsoulRenderable caption, RgbColor color, Justify? justification = null)
            : this(caption, new Style(color), justification)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableCaption"/> class with Spectre-style style text.
        /// </summary>
        /// <param name="text">The caption text.</param>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <param name="justification">Optional caption justification.</param>
        public TableCaption(string text, string style, Justify? justification = null)
            : this(text, ConsoulLibrary.Style.Parse(style), justification)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableCaption"/> class from a renderable component with Spectre-style style text.
        /// </summary>
        /// <param name="caption">Renderable caption captured or adapted for the table caption.</param>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <param name="justification">Optional caption justification.</param>
        public TableCaption(IConsoulRenderable caption, string style, Justify? justification = null)
            : this(caption, ConsoulLibrary.Style.Parse(style), justification)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableCaption"/> class with a reusable style object.
        /// </summary>
        /// <param name="text">The caption text.</param>
        /// <param name="style">The caption style.</param>
        /// <param name="justification">Optional caption justification.</param>
        public TableCaption(string text, Style style, Justify? justification = null)
            : this(text, justification)
        {
            Style = style;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableCaption"/> class from a renderable component with a reusable style object.
        /// </summary>
        /// <param name="caption">Renderable caption captured or adapted for the table caption.</param>
        /// <param name="style">The caption style.</param>
        /// <param name="justification">Optional caption justification.</param>
        public TableCaption(IConsoulRenderable caption, Style style, Justify? justification = null)
            : this(caption, justification)
        {
            Style = style;
        }

        /// <summary>
        /// Gets the caption text.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets or sets the optional caption justification.
        /// </summary>
        public Justify? Justification { get; set; }

        /// <summary>
        /// Gets or sets the optional caption style.
        /// </summary>
        public Style Style { get; set; }

        /// <summary>
        /// Sets the caption justification.
        /// </summary>
        /// <param name="justification">The caption justification.</param>
        /// <returns>The current <see cref="TableCaption"/> instance.</returns>
        public TableCaption SetAlignment(Justify justification)
        {
            Justification = justification;
            return this;
        }

        /// <summary>
        /// Sets the caption style.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="TableCaption"/> instance.</returns>
        public TableCaption SetStyle(Style style)
        {
            Style = style;
            return this;
        }

        /// <summary>
        /// Sets the caption foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TableCaption"/> instance.</returns>
        public TableCaption Color(ConsoleColor color)
        {
            return SetStyle(new Style(color));
        }

        /// <summary>
        /// Sets the caption foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TableCaption"/> instance.</returns>
        public TableCaption Color(SpectreColor color)
        {
            return SetStyle(new Style(color));
        }

        /// <summary>
        /// Sets the caption foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TableCaption"/> instance.</returns>
        public TableCaption Color(RgbColor color)
        {
            return SetStyle(new Style(color));
        }

        /// <summary>
        /// Sets the caption style from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="TableCaption"/> instance.</returns>
        public TableCaption Color(string style)
        {
            return SetStyle(ConsoulLibrary.Style.Parse(style));
        }
    }
}
