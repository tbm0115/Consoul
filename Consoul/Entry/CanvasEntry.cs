using ConsoulLibrary.Color;
using System;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders a small ASCII canvas for diagrams, maps, and proof-of-concept visual sketches.
    /// </summary>
    public class CanvasEntry : IRenderable
    {
        private readonly CanvasCell?[,] _cells;
        private readonly CanvasImagePixel?[,] _pixels;
        private readonly CanvasOptions _options;
        private bool _containsCells;
        private bool _containsPixels;

        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasEntry"/> class.
        /// </summary>
        /// <param name="width">The logical canvas width.</param>
        /// <param name="height">The logical canvas height.</param>
        /// <param name="options">Optional rendering options for the canvas.</param>
        public CanvasEntry(int width, int height, CanvasOptions options = null)
        {
            Width = Math.Max(1, width);
            Height = Math.Max(1, height);
            _options = options != null ? options.Clone() : new CanvasOptions();
            _cells = new CanvasCell?[Height, Width];
            _pixels = new CanvasImagePixel?[Height, Width];
        }

        /// <summary>
        /// Gets the logical canvas width.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the logical canvas height.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets the options used to render the canvas.
        /// </summary>
        public CanvasOptions Options => _options;

        /// <summary>
        /// Gets or sets the maximum rendered width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? MaxWidth
        {
            get => _options.MaxWidth;
            set => _options.MaxWidth = value;
        }

        /// <summary>
        /// Gets or sets the maximum rendered height.
        /// </summary>
        public int? MaxHeight
        {
            get => _options.MaxHeight;
            set => _options.MaxHeight = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether pixel canvases are scaled to fit the configured render width.
        /// </summary>
        public bool Scale
        {
            get => _options.Scale;
            set => _options.Scale = value;
        }

        /// <summary>
        /// Sets one canvas cell.
        /// </summary>
        /// <param name="x">The zero-based horizontal position.</param>
        /// <param name="y">The zero-based vertical position.</param>
        /// <param name="character">The character to render.</param>
        /// <param name="color">Optional foreground color for the cell.</param>
        /// <param name="backgroundColor">Optional background color for the cell.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry Set(int x, int y, char character, ConsoleColor? color = null, ConsoleColor? backgroundColor = null)
        {
            return Set(x, y, character, CreateScheme(color, backgroundColor));
        }

        /// <summary>
        /// Sets one canvas cell.
        /// </summary>
        /// <param name="x">The zero-based horizontal position.</param>
        /// <param name="y">The zero-based vertical position.</param>
        /// <param name="character">The character to render.</param>
        /// <param name="color">The foreground color for the cell.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry Set(int x, int y, char character, SpectreColor color)
        {
            return Set(x, y, character, CreateScheme(color));
        }

        /// <summary>
        /// Sets one canvas cell.
        /// </summary>
        /// <param name="x">The zero-based horizontal position.</param>
        /// <param name="y">The zero-based vertical position.</param>
        /// <param name="character">The character to render.</param>
        /// <param name="color">The foreground color for the cell.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry Set(int x, int y, char character, RgbColor color)
        {
            return Set(x, y, character, CreateScheme(color));
        }

        /// <summary>
        /// Sets one canvas cell.
        /// </summary>
        /// <param name="x">The zero-based horizontal position.</param>
        /// <param name="y">The zero-based vertical position.</param>
        /// <param name="character">The character to render.</param>
        /// <param name="scheme">Optional color scheme for the cell.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry Set(int x, int y, char character, ColorScheme? scheme)
        {
            if (!IsInside(x, y))
            {
                return this;
            }

            _cells[y, x] = new CanvasCell(character, scheme ?? _options.ResolveDefaultScheme());
            _containsCells = true;
            return this;
        }

        /// <summary>
        /// Sets one canvas cell using Spectre-style style text.
        /// </summary>
        /// <param name="x">The zero-based horizontal position.</param>
        /// <param name="y">The zero-based vertical position.</param>
        /// <param name="character">The character to render.</param>
        /// <param name="style">A markup style such as <c>green</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry Set(int x, int y, char character, string style)
        {
            return Set(x, y, character, CreateScheme(style));
        }

        /// <summary>
        /// Sets one canvas cell using a reusable style.
        /// </summary>
        /// <param name="x">The zero-based horizontal position.</param>
        /// <param name="y">The zero-based vertical position.</param>
        /// <param name="character">The character to render.</param>
        /// <param name="style">The style used for the cell.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry Set(int x, int y, char character, Style style)
        {
            return Set(x, y, character, CreateScheme(style));
        }

        /// <summary>
        /// Sets one colored pixel using the nearest console color.
        /// </summary>
        /// <param name="x">The zero-based horizontal pixel position.</param>
        /// <param name="y">The zero-based vertical pixel position.</param>
        /// <param name="color">The pixel color.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry SetPixel(int x, int y, ConsoleColor color)
        {
            return SetPixel(x, y, CanvasImagePixel.FromConsoleColor(color));
        }

        /// <summary>
        /// Sets one colored pixel using the nearest console color.
        /// </summary>
        /// <param name="x">The zero-based horizontal pixel position.</param>
        /// <param name="y">The zero-based vertical pixel position.</param>
        /// <param name="color">The pixel color.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry SetPixel(int x, int y, SpectreColor color)
        {
            return SetPixel(x, y, ToPixel(color));
        }

        /// <summary>
        /// Sets one colored pixel using the nearest console color.
        /// </summary>
        /// <param name="x">The zero-based horizontal pixel position.</param>
        /// <param name="y">The zero-based vertical pixel position.</param>
        /// <param name="color">The pixel color.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry SetPixel(int x, int y, RgbColor color)
        {
            return SetPixel(x, y, ToPixel(color));
        }

        /// <summary>
        /// Sets one colored pixel using Spectre-style style text.
        /// </summary>
        /// <param name="x">The zero-based horizontal pixel position.</param>
        /// <param name="y">The zero-based vertical pixel position.</param>
        /// <param name="style">A markup style such as <c>green</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry SetPixel(int x, int y, string style)
        {
            return SetPixel(x, y, Style.ParseForeground(style, RenderOptions.DefaultColor));
        }

        /// <summary>
        /// Sets one colored pixel using a parsed style.
        /// </summary>
        /// <param name="x">The zero-based horizontal pixel position.</param>
        /// <param name="y">The zero-based vertical pixel position.</param>
        /// <param name="style">The style whose foreground color is used for the pixel.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry SetPixel(int x, int y, Style style)
        {
            ConsoleColor color = (style ?? new Style()).ToForegroundColor(RenderOptions.DefaultColor);
            return SetPixel(x, y, color);
        }

        /// <summary>
        /// Sets one colored pixel.
        /// </summary>
        /// <param name="x">The zero-based horizontal pixel position.</param>
        /// <param name="y">The zero-based vertical pixel position.</param>
        /// <param name="pixel">The source pixel to render.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry SetPixel(int x, int y, CanvasImagePixel pixel)
        {
            if (!IsInside(x, y))
            {
                return this;
            }

            _pixels[y, x] = pixel;
            _containsPixels = true;

            if (pixel.Alpha == 0)
            {
                _cells[y, x] = null;
                return this;
            }

            _cells[y, x] = new CanvasCell(
                ' ',
                new ColorScheme
                {
                    Color = _options.ResolveDefaultScheme().Color,
                    BackgroundColor = pixel.ToConsoleColor()
                });
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
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry Text(int x, int y, string text, ConsoleColor? color = null, ConsoleColor? backgroundColor = null)
        {
            ColorScheme? scheme = CreateScheme(color, backgroundColor);
            text = text ?? string.Empty;
            for (int index = 0; index < text.Length; index++)
            {
                Set(x + index, y, text[index], scheme);
            }

            return this;
        }

        /// <summary>
        /// Draws text starting at the specified canvas cell.
        /// </summary>
        /// <param name="x">The zero-based horizontal position.</param>
        /// <param name="y">The zero-based vertical position.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="color">The foreground color for the text.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry Text(int x, int y, string text, SpectreColor color)
        {
            return Text(x, y, text, CreateScheme(color));
        }

        /// <summary>
        /// Draws text starting at the specified canvas cell.
        /// </summary>
        /// <param name="x">The zero-based horizontal position.</param>
        /// <param name="y">The zero-based vertical position.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="color">The foreground color for the text.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry Text(int x, int y, string text, RgbColor color)
        {
            return Text(x, y, text, CreateScheme(color));
        }

        /// <summary>
        /// Draws text starting at the specified canvas cell using Spectre-style style text.
        /// </summary>
        /// <param name="x">The zero-based horizontal position.</param>
        /// <param name="y">The zero-based vertical position.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="style">A markup style such as <c>green</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry Text(int x, int y, string text, string style)
        {
            ColorScheme? scheme = CreateScheme(style);
            text = text ?? string.Empty;
            for (int index = 0; index < text.Length; index++)
            {
                Set(x + index, y, text[index], scheme);
            }

            return this;
        }

        /// <summary>
        /// Draws text starting at the specified canvas cell using a reusable style.
        /// </summary>
        /// <param name="x">The zero-based horizontal position.</param>
        /// <param name="y">The zero-based vertical position.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="style">The style used for the text.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry Text(int x, int y, string text, Style style)
        {
            ColorScheme? scheme = CreateScheme(style);
            text = text ?? string.Empty;
            for (int index = 0; index < text.Length; index++)
            {
                Set(x + index, y, text[index], scheme);
            }

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
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry Line(int x1, int y1, int x2, int y2, char character = '#', ConsoleColor? color = null, ConsoleColor? backgroundColor = null)
        {
            ColorScheme? scheme = CreateScheme(color, backgroundColor);
            return Line(x1, y1, x2, y2, character, scheme);
        }

        /// <summary>
        /// Draws a line between two canvas cells.
        /// </summary>
        /// <param name="x1">The starting zero-based horizontal position.</param>
        /// <param name="y1">The starting zero-based vertical position.</param>
        /// <param name="x2">The ending zero-based horizontal position.</param>
        /// <param name="y2">The ending zero-based vertical position.</param>
        /// <param name="color">The foreground color for the line.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry Line(int x1, int y1, int x2, int y2, SpectreColor color)
        {
            return Line(x1, y1, x2, y2, '#', CreateScheme(color));
        }

        /// <summary>
        /// Draws a line between two canvas cells.
        /// </summary>
        /// <param name="x1">The starting zero-based horizontal position.</param>
        /// <param name="y1">The starting zero-based vertical position.</param>
        /// <param name="x2">The ending zero-based horizontal position.</param>
        /// <param name="y2">The ending zero-based vertical position.</param>
        /// <param name="color">The foreground color for the line.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry Line(int x1, int y1, int x2, int y2, RgbColor color)
        {
            return Line(x1, y1, x2, y2, '#', CreateScheme(color));
        }

        /// <summary>
        /// Draws a line between two canvas cells.
        /// </summary>
        /// <param name="x1">The starting zero-based horizontal position.</param>
        /// <param name="y1">The starting zero-based vertical position.</param>
        /// <param name="x2">The ending zero-based horizontal position.</param>
        /// <param name="y2">The ending zero-based vertical position.</param>
        /// <param name="character">The character used for the line.</param>
        /// <param name="color">The foreground color for the line.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry Line(int x1, int y1, int x2, int y2, char character, SpectreColor color)
        {
            return Line(x1, y1, x2, y2, character, CreateScheme(color));
        }

        /// <summary>
        /// Draws a line between two canvas cells.
        /// </summary>
        /// <param name="x1">The starting zero-based horizontal position.</param>
        /// <param name="y1">The starting zero-based vertical position.</param>
        /// <param name="x2">The ending zero-based horizontal position.</param>
        /// <param name="y2">The ending zero-based vertical position.</param>
        /// <param name="character">The character used for the line.</param>
        /// <param name="color">The foreground color for the line.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry Line(int x1, int y1, int x2, int y2, char character, RgbColor color)
        {
            return Line(x1, y1, x2, y2, character, CreateScheme(color));
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
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry Line(int x1, int y1, int x2, int y2, char character, string style)
        {
            return Line(x1, y1, x2, y2, character, CreateScheme(style));
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
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry Line(int x1, int y1, int x2, int y2, char character, Style style)
        {
            return Line(x1, y1, x2, y2, character, CreateScheme(style));
        }

        private CanvasEntry Line(int x1, int y1, int x2, int y2, char character, ColorScheme? scheme)
        {
            int dx = Math.Abs(x2 - x1);
            int sx = x1 < x2 ? 1 : -1;
            int dy = -Math.Abs(y2 - y1);
            int sy = y1 < y2 ? 1 : -1;
            int error = dx + dy;

            while (true)
            {
                Set(x1, y1, character, scheme);
                if (x1 == x2 && y1 == y2)
                {
                    break;
                }

                int twiceError = 2 * error;
                if (twiceError >= dy)
                {
                    error += dy;
                    x1 += sx;
                }

                if (twiceError <= dx)
                {
                    error += dx;
                    y1 += sy;
                }
            }

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
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry Rectangle(int x, int y, int width, int height, char character = '#', ConsoleColor? color = null, ConsoleColor? backgroundColor = null)
        {
            width = Math.Max(1, width);
            height = Math.Max(1, height);
            Line(x, y, x + width - 1, y, character, color, backgroundColor);
            Line(x, y + height - 1, x + width - 1, y + height - 1, character, color, backgroundColor);
            Line(x, y, x, y + height - 1, character, color, backgroundColor);
            Line(x + width - 1, y, x + width - 1, y + height - 1, character, color, backgroundColor);
            return this;
        }

        /// <summary>
        /// Draws a rectangle outline.
        /// </summary>
        /// <param name="x">The zero-based horizontal position of the left side.</param>
        /// <param name="y">The zero-based vertical position of the top side.</param>
        /// <param name="width">The rectangle width.</param>
        /// <param name="height">The rectangle height.</param>
        /// <param name="color">The foreground color for the outline.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry Rectangle(int x, int y, int width, int height, SpectreColor color)
        {
            return Rectangle(x, y, width, height, '#', color);
        }

        /// <summary>
        /// Draws a rectangle outline.
        /// </summary>
        /// <param name="x">The zero-based horizontal position of the left side.</param>
        /// <param name="y">The zero-based vertical position of the top side.</param>
        /// <param name="width">The rectangle width.</param>
        /// <param name="height">The rectangle height.</param>
        /// <param name="color">The foreground color for the outline.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry Rectangle(int x, int y, int width, int height, RgbColor color)
        {
            return Rectangle(x, y, width, height, '#', color);
        }

        /// <summary>
        /// Draws a rectangle outline.
        /// </summary>
        /// <param name="x">The zero-based horizontal position of the left side.</param>
        /// <param name="y">The zero-based vertical position of the top side.</param>
        /// <param name="width">The rectangle width.</param>
        /// <param name="height">The rectangle height.</param>
        /// <param name="character">The character used for the outline.</param>
        /// <param name="color">The foreground color for the outline.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry Rectangle(int x, int y, int width, int height, char character, SpectreColor color)
        {
            width = Math.Max(1, width);
            height = Math.Max(1, height);
            ColorScheme scheme = CreateScheme(color);
            Line(x, y, x + width - 1, y, character, scheme);
            Line(x, y + height - 1, x + width - 1, y + height - 1, character, scheme);
            Line(x, y, x, y + height - 1, character, scheme);
            Line(x + width - 1, y, x + width - 1, y + height - 1, character, scheme);
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
        /// <param name="color">The foreground color for the outline.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry Rectangle(int x, int y, int width, int height, char character, RgbColor color)
        {
            width = Math.Max(1, width);
            height = Math.Max(1, height);
            ColorScheme scheme = CreateScheme(color);
            Line(x, y, x + width - 1, y, character, scheme);
            Line(x, y + height - 1, x + width - 1, y + height - 1, character, scheme);
            Line(x, y, x, y + height - 1, character, scheme);
            Line(x + width - 1, y, x + width - 1, y + height - 1, character, scheme);
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
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry Rectangle(int x, int y, int width, int height, char character, string style)
        {
            width = Math.Max(1, width);
            height = Math.Max(1, height);
            ColorScheme? scheme = CreateScheme(style);
            Line(x, y, x + width - 1, y, character, scheme);
            Line(x, y + height - 1, x + width - 1, y + height - 1, character, scheme);
            Line(x, y, x, y + height - 1, character, scheme);
            Line(x + width - 1, y, x + width - 1, y + height - 1, character, scheme);
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
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry Rectangle(int x, int y, int width, int height, char character, Style style)
        {
            width = Math.Max(1, width);
            height = Math.Max(1, height);
            ColorScheme? scheme = CreateScheme(style);
            Line(x, y, x + width - 1, y, character, scheme);
            Line(x, y + height - 1, x + width - 1, y + height - 1, character, scheme);
            Line(x, y, x, y + height - 1, character, scheme);
            Line(x + width - 1, y, x + width - 1, y + height - 1, character, scheme);
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
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry FillRectangle(int x, int y, int width, int height, char character = '#', ConsoleColor? color = null, ConsoleColor? backgroundColor = null)
        {
            ColorScheme? scheme = CreateScheme(color, backgroundColor);
            return FillRectangle(x, y, width, height, character, scheme);
        }

        /// <summary>
        /// Fills a rectangle.
        /// </summary>
        /// <param name="x">The zero-based horizontal position of the left side.</param>
        /// <param name="y">The zero-based vertical position of the top side.</param>
        /// <param name="width">The rectangle width.</param>
        /// <param name="height">The rectangle height.</param>
        /// <param name="color">The foreground color for the fill.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry FillRectangle(int x, int y, int width, int height, SpectreColor color)
        {
            return FillRectangle(x, y, width, height, '#', color);
        }

        /// <summary>
        /// Fills a rectangle.
        /// </summary>
        /// <param name="x">The zero-based horizontal position of the left side.</param>
        /// <param name="y">The zero-based vertical position of the top side.</param>
        /// <param name="width">The rectangle width.</param>
        /// <param name="height">The rectangle height.</param>
        /// <param name="color">The foreground color for the fill.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry FillRectangle(int x, int y, int width, int height, RgbColor color)
        {
            return FillRectangle(x, y, width, height, '#', color);
        }

        /// <summary>
        /// Fills a rectangle.
        /// </summary>
        /// <param name="x">The zero-based horizontal position of the left side.</param>
        /// <param name="y">The zero-based vertical position of the top side.</param>
        /// <param name="width">The rectangle width.</param>
        /// <param name="height">The rectangle height.</param>
        /// <param name="character">The character used for the fill.</param>
        /// <param name="color">The foreground color for the fill.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry FillRectangle(int x, int y, int width, int height, char character, SpectreColor color)
        {
            return FillRectangle(x, y, width, height, character, CreateScheme(color));
        }

        /// <summary>
        /// Fills a rectangle.
        /// </summary>
        /// <param name="x">The zero-based horizontal position of the left side.</param>
        /// <param name="y">The zero-based vertical position of the top side.</param>
        /// <param name="width">The rectangle width.</param>
        /// <param name="height">The rectangle height.</param>
        /// <param name="character">The character used for the fill.</param>
        /// <param name="color">The foreground color for the fill.</param>
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry FillRectangle(int x, int y, int width, int height, char character, RgbColor color)
        {
            return FillRectangle(x, y, width, height, character, CreateScheme(color));
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
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry FillRectangle(int x, int y, int width, int height, char character, string style)
        {
            return FillRectangle(x, y, width, height, character, CreateScheme(style));
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
        /// <returns>The current <see cref="CanvasEntry"/> instance.</returns>
        public CanvasEntry FillRectangle(int x, int y, int width, int height, char character, Style style)
        {
            return FillRectangle(x, y, width, height, character, CreateScheme(style));
        }

        private CanvasEntry FillRectangle(int x, int y, int width, int height, char character, ColorScheme? scheme)
        {
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    Set(x + column, y + row, character, scheme);
                }
            }

            return this;
        }

        /// <summary>
        /// Renders the canvas to the active console driver.
        /// </summary>
        public void Render()
        {
            if (_containsPixels && !_containsCells)
            {
                RenderPixels();
                return;
            }

            int renderWidth = Math.Min(Width, PanelEntry.ResolveWidth(_options.MaxWidth ?? Width));
            int renderHeight = Math.Min(Height, Math.Max(1, _options.MaxHeight ?? Height));
            ColorScheme defaultScheme = _options.ResolveDefaultScheme();

            for (int y = 0; y < renderHeight; y++)
            {
                for (int x = 0; x < renderWidth; x++)
                {
                    CanvasCell cell = _cells[y, x] ?? new CanvasCell(_options.EmptyCharacter, defaultScheme);
                    Consoul.Write(cell.Character.ToString(), cell.Scheme, false);
                }

                Consoul.Write(string.Empty, defaultScheme, true);
            }
        }

        /// <summary>
        /// Renders a temporary canvas to the active console driver.
        /// </summary>
        /// <param name="width">The logical canvas width.</param>
        /// <param name="height">The logical canvas height.</param>
        /// <param name="configure">Action used to draw on the canvas before rendering.</param>
        /// <param name="options">Optional rendering options for the canvas.</param>
        public static void Render(int width, int height, Action<CanvasEntry> configure, CanvasOptions options = null)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var canvas = new CanvasEntry(width, height, options);
            configure(canvas);
            canvas.Render();
        }

        private void RenderPixels()
        {
            var pixels = new CanvasImagePixel[Height, Width];
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    pixels[y, x] = _pixels[y, x] ?? CanvasImagePixel.Transparent;
                }
            }

            var options = new CanvasImageOptions
            {
                MaxWidth = _options.MaxWidth,
                MaxHeight = _options.MaxHeight,
                Scale = _options.Scale,
                Resampler = CanvasImageResampler.NearestNeighbor
            };

            new CanvasImageEntry(pixels, options).Render();
        }

        private bool IsInside(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        private static ColorScheme? CreateScheme(ConsoleColor? color, ConsoleColor? backgroundColor)
        {
            if (!color.HasValue && !backgroundColor.HasValue)
            {
                return null;
            }

            return new ColorScheme
            {
                Color = RenderOptions.GetColorOrDefault(color),
                BackgroundColor = RenderOptions.GetBackgroundColorOrDefault(backgroundColor)
            };
        }

        private static ColorScheme CreateScheme(string style)
        {
            return Style.Parse(style).ToColorScheme(RenderOptions.DefaultScheme);
        }

        private static ColorScheme CreateScheme(Style style)
        {
            return (style ?? Style.Plain).ToColorScheme(RenderOptions.DefaultScheme);
        }

        private CanvasEntry Text(int x, int y, string text, ColorScheme? scheme)
        {
            text = text ?? string.Empty;
            for (int index = 0; index < text.Length; index++)
            {
                Set(x + index, y, text[index], scheme);
            }

            return this;
        }

        private static ColorScheme CreateScheme(SpectreColor color)
        {
            return new ColorScheme
            {
                Color = color.ToConsoleColor(),
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        private static ColorScheme CreateScheme(RgbColor color)
        {
            return new ColorScheme
            {
                Color = color.ToConsoleColor(),
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        private static CanvasImagePixel ToPixel(SpectreColor color)
        {
            return CanvasImagePixel.FromConsoleColor(color.ToConsoleColor());
        }

        private static CanvasImagePixel ToPixel(RgbColor color)
        {
            return CanvasImagePixel.FromConsoleColor(color.ToConsoleColor());
        }
    }
}
