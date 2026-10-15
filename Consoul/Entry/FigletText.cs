using System;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent figlet text construction over <see cref="FigletTextEntry"/>.
    /// </summary>
    public class FigletText : FigletTextEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FigletText"/> class.
        /// </summary>
        /// <param name="text">The text to render as large ASCII glyphs.</param>
        public FigletText(string text) : base(text)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FigletText"/> class with explicit options.
        /// </summary>
        /// <param name="text">The text to render as large ASCII glyphs.</param>
        /// <param name="options">Optional rendering options for the text.</param>
        public FigletText(string text, FigletTextOptions options) : base(text, options)
        {
        }

        /// <summary>
        /// Sets the text rendered as large ASCII glyphs.
        /// </summary>
        /// <param name="text">The text to render.</param>
        /// <returns>The current <see cref="FigletText"/> instance.</returns>
        public FigletText TextValue(string text)
        {
            Text = text ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the requested render width.
        /// </summary>
        /// <param name="width">The requested width. The actual width is clamped to the active terminal.</param>
        /// <returns>The current <see cref="FigletText"/> instance.</returns>
        public FigletText Width(int? width)
        {
            Options.Width = width;
            return this;
        }

        /// <summary>
        /// Aligns rendered glyph lines to the left.
        /// </summary>
        /// <returns>The current <see cref="FigletText"/> instance.</returns>
        public FigletText LeftJustified()
        {
            Options.Alignment = TextBlockAlignment.Left;
            return this;
        }

        /// <summary>
        /// Aligns rendered glyph lines to the left.
        /// </summary>
        /// <returns>The current <see cref="FigletText"/> instance.</returns>
        public FigletText LeftAligned()
        {
            return LeftJustified();
        }

        /// <summary>
        /// Centers rendered glyph lines.
        /// </summary>
        /// <returns>The current <see cref="FigletText"/> instance.</returns>
        public FigletText Centered()
        {
            Options.Alignment = TextBlockAlignment.Center;
            return this;
        }

        /// <summary>
        /// Aligns rendered glyph lines to the right.
        /// </summary>
        /// <returns>The current <see cref="FigletText"/> instance.</returns>
        public FigletText RightJustified()
        {
            Options.Alignment = TextBlockAlignment.Right;
            return this;
        }

        /// <summary>
        /// Aligns rendered glyph lines to the right.
        /// </summary>
        /// <returns>The current <see cref="FigletText"/> instance.</returns>
        public FigletText RightAligned()
        {
            return RightJustified();
        }

        /// <summary>
        /// Sets glyph justification using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="justify">The justification to apply.</param>
        /// <returns>The current <see cref="FigletText"/> instance.</returns>
        public FigletText Justify(Justify justify)
        {
            Options.Alignment = JustifyMap.ToTextBlockAlignment(justify);
            return this;
        }

        /// <summary>
        /// Sets the character used to fill glyphs.
        /// </summary>
        /// <param name="character">The glyph fill character.</param>
        /// <returns>The current <see cref="FigletText"/> instance.</returns>
        public FigletText Character(char character)
        {
            Options.Character = character;
            return this;
        }

        /// <summary>
        /// Sets whether rendered lines should be padded to the full render width.
        /// </summary>
        /// <param name="enabled"><c>true</c> to pad rendered lines; otherwise, <c>false</c>.</param>
        /// <returns>The current <see cref="FigletText"/> instance.</returns>
        public FigletText Pad(bool enabled = true)
        {
            Options.Pad = enabled;
            return this;
        }

        /// <summary>
        /// Sets the figlet layout mode used between glyphs.
        /// </summary>
        /// <param name="mode">The layout mode to use.</param>
        /// <returns>The current <see cref="FigletText"/> instance.</returns>
        public FigletText LayoutMode(FigletLayoutMode mode)
        {
            Options.LayoutMode = mode;
            return this;
        }

        /// <summary>
        /// Preserves the normal spacing between rendered glyphs.
        /// </summary>
        /// <returns>The current <see cref="FigletText"/> instance.</returns>
        public FigletText FullSize()
        {
            return LayoutMode(FigletLayoutMode.FullSize);
        }

        /// <summary>
        /// Removes inter-glyph spacing when glyphs can fit more tightly.
        /// </summary>
        /// <returns>The current <see cref="FigletText"/> instance.</returns>
        public FigletText Fitted()
        {
            return LayoutMode(FigletLayoutMode.Fitted);
        }

        /// <summary>
        /// Uses the most compact spacing supported by Consoul's dependency-free glyph renderer.
        /// </summary>
        /// <returns>The current <see cref="FigletText"/> instance.</returns>
        public FigletText Smushed()
        {
            return LayoutMode(FigletLayoutMode.Smushed);
        }

        /// <summary>
        /// Sets the glyph foreground color.
        /// </summary>
        /// <param name="color">The glyph foreground color.</param>
        /// <returns>The current <see cref="FigletText"/> instance.</returns>
        public FigletText Color(ConsoleColor color)
        {
            Options.Scheme = Scheme(color);
            Options.Decoration = Decoration.None;
            Options.ForegroundRgb = null;
            Options.BackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the glyph foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The glyph foreground color.</param>
        /// <returns>The current <see cref="FigletText"/> instance.</returns>
        public FigletText Color(SpectreColor color)
        {
            return Color(new Style(color));
        }

        /// <summary>
        /// Sets the glyph foreground color from an RGB value.
        /// </summary>
        /// <param name="color">The glyph foreground color.</param>
        /// <returns>The current <see cref="FigletText"/> instance.</returns>
        public FigletText Color(RgbColor color)
        {
            return Color(new Style(color));
        }

        /// <summary>
        /// Sets the rendered glyph foreground/background colors from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="FigletText"/> instance.</returns>
        public FigletText Color(Style style)
        {
            ApplyStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the rendered glyph foreground/background colors from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="FigletText"/> instance.</returns>
        public FigletText Color(string style)
        {
            ApplyStyle(Style.Parse(style));
            return this;
        }

        private static ColorScheme Scheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        private void ApplyStyle(Style style)
        {
            Style resolvedStyle = style ?? Style.Plain;
            Options.Scheme = resolvedStyle.ToColorScheme(Options.ResolveScheme());
            Options.Decoration = resolvedStyle.Decoration;
            Options.ForegroundRgb = resolvedStyle.ForegroundRgb;
            Options.BackgroundRgb = resolvedStyle.BackgroundRgb;
        }
    }
}
