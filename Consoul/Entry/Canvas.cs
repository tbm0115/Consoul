using System;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent canvas construction over <see cref="CanvasEntry"/>.
    /// </summary>
    public class Canvas : CanvasEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Canvas"/> class.
        /// </summary>
        /// <param name="width">The logical canvas width.</param>
        /// <param name="height">The logical canvas height.</param>
        public Canvas(int width, int height) : base(width, height)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Canvas"/> class with explicit options.
        /// </summary>
        /// <param name="width">The logical canvas width.</param>
        /// <param name="height">The logical canvas height.</param>
        /// <param name="options">Optional rendering options for the canvas.</param>
        public Canvas(int width, int height, CanvasOptions options) : base(width, height, options)
        {
        }

        /// <summary>
        /// Sets one canvas cell.
        /// </summary>
        /// <param name="x">The zero-based horizontal position.</param>
        /// <param name="y">The zero-based vertical position.</param>
        /// <param name="character">The character to render.</param>
        /// <param name="color">Optional foreground color for the cell.</param>
        /// <param name="backgroundColor">Optional background color for the cell.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas Set(int x, int y, char character, ConsoleColor? color = null, ConsoleColor? backgroundColor = null)
        {
            base.Set(x, y, character, color, backgroundColor);
            return this;
        }

        /// <summary>
        /// Sets one canvas cell.
        /// </summary>
        /// <param name="x">The zero-based horizontal position.</param>
        /// <param name="y">The zero-based vertical position.</param>
        /// <param name="character">The character to render.</param>
        /// <param name="scheme">Optional color scheme for the cell.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas Set(int x, int y, char character, ColorScheme? scheme)
        {
            base.Set(x, y, character, scheme);
            return this;
        }

        /// <summary>
        /// Sets one canvas cell using a shared palette color.
        /// </summary>
        /// <param name="x">The zero-based horizontal position.</param>
        /// <param name="y">The zero-based vertical position.</param>
        /// <param name="character">The character to render.</param>
        /// <param name="color">The foreground color for the cell.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas Set(int x, int y, char character, SpectreColor color)
        {
            base.Set(x, y, character, color);
            return this;
        }

        /// <summary>
        /// Sets one canvas cell using an RGB palette color.
        /// </summary>
        /// <param name="x">The zero-based horizontal position.</param>
        /// <param name="y">The zero-based vertical position.</param>
        /// <param name="character">The character to render.</param>
        /// <param name="color">The foreground color for the cell.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas Set(int x, int y, char character, RgbColor color)
        {
            base.Set(x, y, character, color);
            return this;
        }

        /// <summary>
        /// Sets one canvas cell using Spectre-style style text.
        /// </summary>
        /// <param name="x">The zero-based horizontal position.</param>
        /// <param name="y">The zero-based vertical position.</param>
        /// <param name="character">The character to render.</param>
        /// <param name="style">A markup style such as <c>green</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas Set(int x, int y, char character, string style)
        {
            base.Set(x, y, character, Scheme(style));
            return this;
        }

        /// <summary>
        /// Sets one canvas cell using a reusable style.
        /// </summary>
        /// <param name="x">The zero-based horizontal position.</param>
        /// <param name="y">The zero-based vertical position.</param>
        /// <param name="character">The character to render.</param>
        /// <param name="style">The style used for the cell.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas Set(int x, int y, char character, Style style)
        {
            base.Set(x, y, character, Scheme(style));
            return this;
        }

        /// <summary>
        /// Sets one colored pixel using the nearest console color.
        /// </summary>
        /// <param name="x">The zero-based horizontal pixel position.</param>
        /// <param name="y">The zero-based vertical pixel position.</param>
        /// <param name="color">The pixel color.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas SetPixel(int x, int y, ConsoleColor color)
        {
            base.SetPixel(x, y, color);
            return this;
        }

        /// <summary>
        /// Sets one colored pixel using a shared palette color.
        /// </summary>
        /// <param name="x">The zero-based horizontal pixel position.</param>
        /// <param name="y">The zero-based vertical pixel position.</param>
        /// <param name="color">The pixel color.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas SetPixel(int x, int y, SpectreColor color)
        {
            base.SetPixel(x, y, color);
            return this;
        }

        /// <summary>
        /// Sets one colored pixel using an RGB palette color.
        /// </summary>
        /// <param name="x">The zero-based horizontal pixel position.</param>
        /// <param name="y">The zero-based vertical pixel position.</param>
        /// <param name="color">The pixel color.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas SetPixel(int x, int y, RgbColor color)
        {
            base.SetPixel(x, y, color);
            return this;
        }

        /// <summary>
        /// Sets one colored pixel using Spectre-style style text.
        /// </summary>
        /// <param name="x">The zero-based horizontal pixel position.</param>
        /// <param name="y">The zero-based vertical pixel position.</param>
        /// <param name="style">A markup style such as <c>green</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas SetPixel(int x, int y, string style)
        {
            base.SetPixel(x, y, style);
            return this;
        }

        /// <summary>
        /// Sets one colored pixel using a parsed style.
        /// </summary>
        /// <param name="x">The zero-based horizontal pixel position.</param>
        /// <param name="y">The zero-based vertical pixel position.</param>
        /// <param name="style">The style whose foreground color is used for the pixel.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas SetPixel(int x, int y, Style style)
        {
            base.SetPixel(x, y, style);
            return this;
        }

        /// <summary>
        /// Sets one colored pixel.
        /// </summary>
        /// <param name="x">The zero-based horizontal pixel position.</param>
        /// <param name="y">The zero-based vertical pixel position.</param>
        /// <param name="pixel">The source pixel to render.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas SetPixel(int x, int y, CanvasImagePixel pixel)
        {
            base.SetPixel(x, y, pixel);
            return this;
        }

        /// <summary>
        /// Draws text starting at the specified canvas cell.
        /// </summary>
        /// <param name="x">The zero-based horizontal position.</param>
        /// <param name="y">The zero-based vertical position.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="color">Optional foreground color for the text.</param>
        /// <param name="backgroundColor">Optional background color for the text.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas Text(int x, int y, string text, ConsoleColor? color = null, ConsoleColor? backgroundColor = null)
        {
            base.Text(x, y, text, color, backgroundColor);
            return this;
        }

        /// <summary>
        /// Draws text starting at the specified canvas cell using a shared palette color.
        /// </summary>
        /// <param name="x">The zero-based horizontal position.</param>
        /// <param name="y">The zero-based vertical position.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="color">The foreground color for the text.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas Text(int x, int y, string text, SpectreColor color)
        {
            base.Text(x, y, text, color);
            return this;
        }

        /// <summary>
        /// Draws text starting at the specified canvas cell using an RGB palette color.
        /// </summary>
        /// <param name="x">The zero-based horizontal position.</param>
        /// <param name="y">The zero-based vertical position.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="color">The foreground color for the text.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas Text(int x, int y, string text, RgbColor color)
        {
            base.Text(x, y, text, color);
            return this;
        }

        /// <summary>
        /// Draws text starting at the specified canvas cell using Spectre-style style text.
        /// </summary>
        /// <param name="x">The zero-based horizontal position.</param>
        /// <param name="y">The zero-based vertical position.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="style">A markup style such as <c>green</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas Text(int x, int y, string text, string style)
        {
            base.Text(x, y, text, style);
            return this;
        }

        /// <summary>
        /// Draws text starting at the specified canvas cell using a reusable style.
        /// </summary>
        /// <param name="x">The zero-based horizontal position.</param>
        /// <param name="y">The zero-based vertical position.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="style">The style used for the text.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas Text(int x, int y, string text, Style style)
        {
            base.Text(x, y, text, style);
            return this;
        }

        /// <summary>
        /// Draws a line between two canvas cells.
        /// </summary>
        /// <param name="x1">The starting zero-based horizontal position.</param>
        /// <param name="y1">The starting zero-based vertical position.</param>
        /// <param name="x2">The ending zero-based horizontal position.</param>
        /// <param name="y2">The ending zero-based vertical position.</param>
        /// <param name="character">The character used for the line.</param>
        /// <param name="color">Optional foreground color for the line.</param>
        /// <param name="backgroundColor">Optional background color for the line.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas Line(int x1, int y1, int x2, int y2, char character = '#', ConsoleColor? color = null, ConsoleColor? backgroundColor = null)
        {
            base.Line(x1, y1, x2, y2, character, color, backgroundColor);
            return this;
        }

        /// <summary>
        /// Draws a line between two canvas cells using a shared palette color.
        /// </summary>
        /// <param name="x1">The starting zero-based horizontal position.</param>
        /// <param name="y1">The starting zero-based vertical position.</param>
        /// <param name="x2">The ending zero-based horizontal position.</param>
        /// <param name="y2">The ending zero-based vertical position.</param>
        /// <param name="color">The foreground color for the line.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas Line(int x1, int y1, int x2, int y2, SpectreColor color)
        {
            base.Line(x1, y1, x2, y2, color);
            return this;
        }

        /// <summary>
        /// Draws a line between two canvas cells using an RGB palette color.
        /// </summary>
        /// <param name="x1">The starting zero-based horizontal position.</param>
        /// <param name="y1">The starting zero-based vertical position.</param>
        /// <param name="x2">The ending zero-based horizontal position.</param>
        /// <param name="y2">The ending zero-based vertical position.</param>
        /// <param name="color">The foreground color for the line.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas Line(int x1, int y1, int x2, int y2, RgbColor color)
        {
            base.Line(x1, y1, x2, y2, color);
            return this;
        }

        /// <summary>
        /// Draws a line between two canvas cells using a shared palette color.
        /// </summary>
        /// <param name="x1">The starting zero-based horizontal position.</param>
        /// <param name="y1">The starting zero-based vertical position.</param>
        /// <param name="x2">The ending zero-based horizontal position.</param>
        /// <param name="y2">The ending zero-based vertical position.</param>
        /// <param name="character">The character used for the line.</param>
        /// <param name="color">The foreground color for the line.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas Line(int x1, int y1, int x2, int y2, char character, SpectreColor color)
        {
            base.Line(x1, y1, x2, y2, character, color);
            return this;
        }

        /// <summary>
        /// Draws a line between two canvas cells using an RGB palette color.
        /// </summary>
        /// <param name="x1">The starting zero-based horizontal position.</param>
        /// <param name="y1">The starting zero-based vertical position.</param>
        /// <param name="x2">The ending zero-based horizontal position.</param>
        /// <param name="y2">The ending zero-based vertical position.</param>
        /// <param name="character">The character used for the line.</param>
        /// <param name="color">The foreground color for the line.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas Line(int x1, int y1, int x2, int y2, char character, RgbColor color)
        {
            base.Line(x1, y1, x2, y2, character, color);
            return this;
        }

        /// <summary>
        /// Draws a line between two canvas cells using Spectre-style style text.
        /// </summary>
        /// <param name="x1">The starting zero-based horizontal position.</param>
        /// <param name="y1">The starting zero-based vertical position.</param>
        /// <param name="x2">The ending zero-based horizontal position.</param>
        /// <param name="y2">The ending zero-based vertical position.</param>
        /// <param name="character">The character used for the line.</param>
        /// <param name="style">A markup style such as <c>green</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas Line(int x1, int y1, int x2, int y2, char character, string style)
        {
            base.Line(x1, y1, x2, y2, character, style);
            return this;
        }

        /// <summary>
        /// Draws a line between two canvas cells using a reusable style.
        /// </summary>
        /// <param name="x1">The starting zero-based horizontal position.</param>
        /// <param name="y1">The starting zero-based vertical position.</param>
        /// <param name="x2">The ending zero-based horizontal position.</param>
        /// <param name="y2">The ending zero-based vertical position.</param>
        /// <param name="character">The character used for the line.</param>
        /// <param name="style">The style used for the line.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas Line(int x1, int y1, int x2, int y2, char character, Style style)
        {
            base.Line(x1, y1, x2, y2, character, style);
            return this;
        }

        /// <summary>
        /// Draws a rectangle outline.
        /// </summary>
        /// <param name="x">The zero-based horizontal position of the left side.</param>
        /// <param name="y">The zero-based vertical position of the top side.</param>
        /// <param name="width">The rectangle width.</param>
        /// <param name="height">The rectangle height.</param>
        /// <param name="character">The character used for the outline.</param>
        /// <param name="color">Optional foreground color for the outline.</param>
        /// <param name="backgroundColor">Optional background color for the outline.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas Rectangle(int x, int y, int width, int height, char character = '#', ConsoleColor? color = null, ConsoleColor? backgroundColor = null)
        {
            base.Rectangle(x, y, width, height, character, color, backgroundColor);
            return this;
        }

        /// <summary>
        /// Draws a rectangle outline using a shared palette color.
        /// </summary>
        /// <param name="x">The zero-based horizontal position of the left side.</param>
        /// <param name="y">The zero-based vertical position of the top side.</param>
        /// <param name="width">The rectangle width.</param>
        /// <param name="height">The rectangle height.</param>
        /// <param name="color">The foreground color for the outline.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas Rectangle(int x, int y, int width, int height, SpectreColor color)
        {
            base.Rectangle(x, y, width, height, color);
            return this;
        }

        /// <summary>
        /// Draws a rectangle outline using an RGB palette color.
        /// </summary>
        /// <param name="x">The zero-based horizontal position of the left side.</param>
        /// <param name="y">The zero-based vertical position of the top side.</param>
        /// <param name="width">The rectangle width.</param>
        /// <param name="height">The rectangle height.</param>
        /// <param name="color">The foreground color for the outline.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas Rectangle(int x, int y, int width, int height, RgbColor color)
        {
            base.Rectangle(x, y, width, height, color);
            return this;
        }

        /// <summary>
        /// Draws a rectangle outline using a shared palette color.
        /// </summary>
        /// <param name="x">The zero-based horizontal position of the left side.</param>
        /// <param name="y">The zero-based vertical position of the top side.</param>
        /// <param name="width">The rectangle width.</param>
        /// <param name="height">The rectangle height.</param>
        /// <param name="character">The character used for the outline.</param>
        /// <param name="color">The foreground color for the outline.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas Rectangle(int x, int y, int width, int height, char character, SpectreColor color)
        {
            base.Rectangle(x, y, width, height, character, color);
            return this;
        }

        /// <summary>
        /// Draws a rectangle outline using an RGB palette color.
        /// </summary>
        /// <param name="x">The zero-based horizontal position of the left side.</param>
        /// <param name="y">The zero-based vertical position of the top side.</param>
        /// <param name="width">The rectangle width.</param>
        /// <param name="height">The rectangle height.</param>
        /// <param name="character">The character used for the outline.</param>
        /// <param name="color">The foreground color for the outline.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas Rectangle(int x, int y, int width, int height, char character, RgbColor color)
        {
            base.Rectangle(x, y, width, height, character, color);
            return this;
        }

        /// <summary>
        /// Draws a rectangle outline using Spectre-style style text.
        /// </summary>
        /// <param name="x">The zero-based horizontal position of the left side.</param>
        /// <param name="y">The zero-based vertical position of the top side.</param>
        /// <param name="width">The rectangle width.</param>
        /// <param name="height">The rectangle height.</param>
        /// <param name="character">The character used for the outline.</param>
        /// <param name="style">A markup style such as <c>green</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas Rectangle(int x, int y, int width, int height, char character, string style)
        {
            base.Rectangle(x, y, width, height, character, style);
            return this;
        }

        /// <summary>
        /// Draws a rectangle outline using a reusable style.
        /// </summary>
        /// <param name="x">The zero-based horizontal position of the left side.</param>
        /// <param name="y">The zero-based vertical position of the top side.</param>
        /// <param name="width">The rectangle width.</param>
        /// <param name="height">The rectangle height.</param>
        /// <param name="character">The character used for the outline.</param>
        /// <param name="style">The style used for the outline.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas Rectangle(int x, int y, int width, int height, char character, Style style)
        {
            base.Rectangle(x, y, width, height, character, style);
            return this;
        }

        /// <summary>
        /// Fills a rectangle.
        /// </summary>
        /// <param name="x">The zero-based horizontal position of the left side.</param>
        /// <param name="y">The zero-based vertical position of the top side.</param>
        /// <param name="width">The rectangle width.</param>
        /// <param name="height">The rectangle height.</param>
        /// <param name="character">The character used for the fill.</param>
        /// <param name="color">Optional foreground color for the fill.</param>
        /// <param name="backgroundColor">Optional background color for the fill.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas FillRectangle(int x, int y, int width, int height, char character = '#', ConsoleColor? color = null, ConsoleColor? backgroundColor = null)
        {
            base.FillRectangle(x, y, width, height, character, color, backgroundColor);
            return this;
        }

        /// <summary>
        /// Fills a rectangle using a shared palette color.
        /// </summary>
        /// <param name="x">The zero-based horizontal position of the left side.</param>
        /// <param name="y">The zero-based vertical position of the top side.</param>
        /// <param name="width">The rectangle width.</param>
        /// <param name="height">The rectangle height.</param>
        /// <param name="color">The foreground color for the fill.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas FillRectangle(int x, int y, int width, int height, SpectreColor color)
        {
            base.FillRectangle(x, y, width, height, color);
            return this;
        }

        /// <summary>
        /// Fills a rectangle using an RGB palette color.
        /// </summary>
        /// <param name="x">The zero-based horizontal position of the left side.</param>
        /// <param name="y">The zero-based vertical position of the top side.</param>
        /// <param name="width">The rectangle width.</param>
        /// <param name="height">The rectangle height.</param>
        /// <param name="color">The foreground color for the fill.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas FillRectangle(int x, int y, int width, int height, RgbColor color)
        {
            base.FillRectangle(x, y, width, height, color);
            return this;
        }

        /// <summary>
        /// Fills a rectangle using a shared palette color.
        /// </summary>
        /// <param name="x">The zero-based horizontal position of the left side.</param>
        /// <param name="y">The zero-based vertical position of the top side.</param>
        /// <param name="width">The rectangle width.</param>
        /// <param name="height">The rectangle height.</param>
        /// <param name="character">The character used for the fill.</param>
        /// <param name="color">The foreground color for the fill.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas FillRectangle(int x, int y, int width, int height, char character, SpectreColor color)
        {
            base.FillRectangle(x, y, width, height, character, color);
            return this;
        }

        /// <summary>
        /// Fills a rectangle using an RGB palette color.
        /// </summary>
        /// <param name="x">The zero-based horizontal position of the left side.</param>
        /// <param name="y">The zero-based vertical position of the top side.</param>
        /// <param name="width">The rectangle width.</param>
        /// <param name="height">The rectangle height.</param>
        /// <param name="character">The character used for the fill.</param>
        /// <param name="color">The foreground color for the fill.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas FillRectangle(int x, int y, int width, int height, char character, RgbColor color)
        {
            base.FillRectangle(x, y, width, height, character, color);
            return this;
        }

        /// <summary>
        /// Fills a rectangle using Spectre-style style text.
        /// </summary>
        /// <param name="x">The zero-based horizontal position of the left side.</param>
        /// <param name="y">The zero-based vertical position of the top side.</param>
        /// <param name="width">The rectangle width.</param>
        /// <param name="height">The rectangle height.</param>
        /// <param name="character">The character used for the fill.</param>
        /// <param name="style">A markup style such as <c>green</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas FillRectangle(int x, int y, int width, int height, char character, string style)
        {
            base.FillRectangle(x, y, width, height, character, style);
            return this;
        }

        /// <summary>
        /// Fills a rectangle using a reusable style.
        /// </summary>
        /// <param name="x">The zero-based horizontal position of the left side.</param>
        /// <param name="y">The zero-based vertical position of the top side.</param>
        /// <param name="width">The rectangle width.</param>
        /// <param name="height">The rectangle height.</param>
        /// <param name="character">The character used for the fill.</param>
        /// <param name="style">The style used for the fill.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas FillRectangle(int x, int y, int width, int height, char character, Style style)
        {
            base.FillRectangle(x, y, width, height, character, style);
            return this;
        }

        /// <summary>
        /// Sets the maximum rendered width.
        /// </summary>
        /// <param name="width">The maximum rendered width.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas MaxWidth(int? width)
        {
            Options.MaxWidth = width;
            return this;
        }

        /// <summary>
        /// Sets the maximum rendered height.
        /// </summary>
        /// <param name="height">The maximum rendered height.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas MaxHeight(int? height)
        {
            Options.MaxHeight = height;
            return this;
        }

        /// <summary>
        /// Sets whether pixel canvases are scaled to fit the configured render width.
        /// </summary>
        /// <param name="enabled"><c>true</c> to scale pixel canvases; otherwise, <c>false</c> to clip.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public new Canvas Scale(bool enabled = true)
        {
            Options.Scale = enabled;
            return this;
        }

        /// <summary>
        /// Sets the character used for empty cells.
        /// </summary>
        /// <param name="character">The empty-cell character.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public Canvas EmptyCharacter(char character)
        {
            Options.EmptyCharacter = character;
            return this;
        }

        /// <summary>
        /// Sets the default foreground color for empty cells and uncolored drawing operations.
        /// </summary>
        /// <param name="color">The default foreground color.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public Canvas DefaultColor(ConsoleColor color)
        {
            Options.DefaultScheme = new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
            return this;
        }

        /// <summary>
        /// Sets the default foreground color for empty cells and uncolored drawing operations from a shared palette color.
        /// </summary>
        /// <param name="color">The default foreground color.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public Canvas DefaultColor(SpectreColor color)
        {
            Options.DefaultScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the default foreground color for empty cells and uncolored drawing operations from an RGB palette color.
        /// </summary>
        /// <param name="color">The default foreground color.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public Canvas DefaultColor(RgbColor color)
        {
            Options.DefaultScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the default foreground/background colors for empty cells and uncolored drawing operations from Spectre-style style text.
        /// </summary>
        /// <param name="style">A markup style such as <c>green</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public Canvas DefaultColor(string style)
        {
            Options.DefaultScheme = Scheme(style);
            return this;
        }

        /// <summary>
        /// Sets the default foreground/background colors for empty cells and uncolored drawing operations from a reusable style.
        /// </summary>
        /// <param name="style">The style used for empty cells and uncolored drawing operations.</param>
        /// <returns>The current <see cref="Canvas"/> instance.</returns>
        public Canvas DefaultColor(Style style)
        {
            Options.DefaultScheme = Scheme(style);
            return this;
        }

        private static ColorScheme Scheme(string style)
        {
            return Style.Parse(style).ToColorScheme(RenderOptions.DefaultScheme);
        }

        private static ColorScheme Scheme(Style style)
        {
            return (style ?? Style.Plain).ToColorScheme(RenderOptions.DefaultScheme);
        }

        private static ColorScheme Scheme(SpectreColor color)
        {
            return new ColorScheme
            {
                Color = color.ToConsoleColor(),
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        private static ColorScheme Scheme(RgbColor color)
        {
            return new ColorScheme
            {
                Color = color.ToConsoleColor(),
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }
    }
}
