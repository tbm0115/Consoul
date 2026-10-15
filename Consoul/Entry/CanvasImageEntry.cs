using System;
using ConsoulLibrary.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders an in-memory pixel image as colored terminal half-blocks without adding image decoding dependencies.
    /// </summary>
    public class CanvasImageEntry : IRenderable
    {
        private readonly CanvasImagePixel[,] _pixels;
        private readonly CanvasImageOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasImageEntry"/> class from a row-major pixel matrix.
        /// </summary>
        /// <param name="pixels">The source pixels, indexed as <c>[y, x]</c>.</param>
        /// <param name="options">Optional rendering options.</param>
        public CanvasImageEntry(CanvasImagePixel[,] pixels, CanvasImageOptions options = null)
        {
            if (pixels == null)
            {
                throw new ArgumentNullException(nameof(pixels));
            }

            Height = Math.Max(1, pixels.GetLength(0));
            Width = Math.Max(1, pixels.GetLength(1));
            _pixels = new CanvasImagePixel[Height, Width];
            Array.Copy(pixels, _pixels, pixels.Length);
            _options = options != null ? options.Clone() : new CanvasImageOptions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasImageEntry"/> class from a row-major console color matrix.
        /// </summary>
        /// <param name="pixels">The source colors, indexed as <c>[y, x]</c>.</param>
        /// <param name="options">Optional rendering options.</param>
        public CanvasImageEntry(ConsoleColor[,] pixels, CanvasImageOptions options = null)
            : this(ConvertPixels(pixels), options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasImageEntry"/> class from a pixel factory.
        /// </summary>
        /// <param name="width">The source image width.</param>
        /// <param name="height">The source image height.</param>
        /// <param name="pixelFactory">A factory that returns the pixel for each <c>x</c> and <c>y</c> coordinate.</param>
        /// <param name="options">Optional rendering options.</param>
        public CanvasImageEntry(int width, int height, Func<int, int, CanvasImagePixel> pixelFactory, CanvasImageOptions options = null)
        {
            if (pixelFactory == null)
            {
                throw new ArgumentNullException(nameof(pixelFactory));
            }

            Width = Math.Max(1, width);
            Height = Math.Max(1, height);
            _pixels = new CanvasImagePixel[Height, Width];
            _options = options != null ? options.Clone() : new CanvasImageOptions();

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    _pixels[y, x] = pixelFactory(x, y);
                }
            }
        }

        /// <summary>
        /// Gets the source image width.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the source image height.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets the options used to render the image.
        /// </summary>
        public CanvasImageOptions Options => _options;

        /// <summary>
        /// Gets or sets the maximum rendered cell width.
        /// </summary>
        public int? MaxWidth
        {
            get => _options.MaxWidth;
            set => _options.MaxWidth = value;
        }

        /// <summary>
        /// Gets or sets the exact rendered pixel width before terminal half-block composition.
        /// </summary>
        public int? PixelWidth
        {
            get => _options.PixelWidth;
            set => _options.PixelWidth = value;
        }

        /// <summary>
        /// Renders the image to the active console driver.
        /// </summary>
        public void Render()
        {
            RenderSize size = ResolveRenderSize();
            if (size.Width <= 0 || size.Height <= 0)
            {
                return;
            }

            ColorScheme defaultScheme = RenderOptions.DefaultScheme;
            for (int y = 0; y < size.Height; y += 2)
            {
                for (int x = 0; x < size.Width; x++)
                {
                    CanvasImagePixel top = Sample(x, y, size.Width, size.Height);
                    CanvasImagePixel bottom = y + 1 < size.Height
                        ? Sample(x, y + 1, size.Width, size.Height)
                        : CanvasImagePixel.Transparent;

                    WritePixelPair(top, bottom, defaultScheme);
                }

                Consoul.Write(string.Empty, defaultScheme, true);
            }
        }

        /// <summary>
        /// Renders a canvas image to the active console driver.
        /// </summary>
        /// <param name="image">The image to render.</param>
        public static void Render(CanvasImageEntry image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            image.Render();
        }

        private RenderSize ResolveRenderSize()
        {
            int terminalWidth = Math.Max(1, Consoul.ConsoleBufferWidth);
            int requestedWidth;

            if (_options.PixelWidth.HasValue)
            {
                requestedWidth = Math.Max(1, _options.PixelWidth.Value);
            }
            else
            {
                requestedWidth = Width;
            }

            if (_options.MaxWidth.HasValue)
            {
                requestedWidth = Math.Min(requestedWidth, Math.Max(1, _options.MaxWidth.Value));
            }

            if (!_options.Scale)
            {
                int clippedWidth = Math.Min(Math.Min(requestedWidth, terminalWidth), Width);
                int clippedHeight = Height;

                if (_options.MaxHeight.HasValue)
                {
                    clippedHeight = Math.Min(clippedHeight, Math.Max(1, _options.MaxHeight.Value) * 2);
                }

                return new RenderSize(Math.Max(1, clippedWidth), Math.Max(1, clippedHeight));
            }

            int renderWidth = Math.Min(requestedWidth, terminalWidth);
            renderWidth = Math.Max(1, renderWidth);

            double scale = renderWidth / (double)Width;
            int renderHeight = Math.Max(1, (int)Math.Round(Height * scale));

            if (_options.MaxHeight.HasValue)
            {
                int maxPixelHeight = Math.Max(1, _options.MaxHeight.Value) * 2;
                if (renderHeight > maxPixelHeight)
                {
                    double heightScale = maxPixelHeight / (double)renderHeight;
                    renderHeight = maxPixelHeight;
                    renderWidth = Math.Max(1, (int)Math.Round(renderWidth * heightScale));
                }
            }

            return new RenderSize(Math.Min(renderWidth, terminalWidth), renderHeight);
        }

        private CanvasImagePixel Sample(int targetX, int targetY, int targetWidth, int targetHeight)
        {
            if (!_options.Scale)
            {
                return _pixels[targetY, targetX];
            }

            if (_options.Resampler == CanvasImageResampler.NearestNeighbor)
            {
                return SampleNearest(targetX, targetY, targetWidth, targetHeight);
            }

            return SampleBilinear(targetX, targetY, targetWidth, targetHeight);
        }

        private CanvasImagePixel SampleNearest(int targetX, int targetY, int targetWidth, int targetHeight)
        {
            double sourceX = ((targetX + 0.5) * Width / Math.Max(1.0, targetWidth)) - 0.5;
            double sourceY = ((targetY + 0.5) * Height / Math.Max(1.0, targetHeight)) - 0.5;
            int x = Clamp((int)Math.Round(sourceX), 0, Width - 1);
            int y = Clamp((int)Math.Round(sourceY), 0, Height - 1);
            return _pixels[y, x];
        }

        private CanvasImagePixel SampleBilinear(int targetX, int targetY, int targetWidth, int targetHeight)
        {
            double sourceX = targetWidth <= 1
                ? 0
                : targetX * (Width - 1) / (double)(targetWidth - 1);
            double sourceY = targetHeight <= 1
                ? 0
                : targetY * (Height - 1) / (double)(targetHeight - 1);

            int x0 = Clamp((int)Math.Floor(sourceX), 0, Width - 1);
            int y0 = Clamp((int)Math.Floor(sourceY), 0, Height - 1);
            int x1 = Clamp(x0 + 1, 0, Width - 1);
            int y1 = Clamp(y0 + 1, 0, Height - 1);
            double xAmount = sourceX - x0;
            double yAmount = sourceY - y0;

            CanvasImagePixel topLeft = _pixels[y0, x0];
            CanvasImagePixel topRight = _pixels[y0, x1];
            CanvasImagePixel bottomLeft = _pixels[y1, x0];
            CanvasImagePixel bottomRight = _pixels[y1, x1];

            return new CanvasImagePixel(
                Blend(Blend(topLeft.Red, topRight.Red, xAmount), Blend(bottomLeft.Red, bottomRight.Red, xAmount), yAmount),
                Blend(Blend(topLeft.Green, topRight.Green, xAmount), Blend(bottomLeft.Green, bottomRight.Green, xAmount), yAmount),
                Blend(Blend(topLeft.Blue, topRight.Blue, xAmount), Blend(bottomLeft.Blue, bottomRight.Blue, xAmount), yAmount),
                Blend(Blend(topLeft.Alpha, topRight.Alpha, xAmount), Blend(bottomLeft.Alpha, bottomRight.Alpha, xAmount), yAmount));
        }

        private void WritePixelPair(CanvasImagePixel top, CanvasImagePixel bottom, ColorScheme defaultScheme)
        {
            bool hasTop = IsVisible(top);
            bool hasBottom = IsVisible(bottom);

            if (hasTop && hasBottom)
            {
                Consoul.Write(
                    "\u2580",
                    new ColorScheme
                    {
                        Color = top.ToConsoleColor(),
                        BackgroundColor = bottom.ToConsoleColor()
                    },
                    false);
                return;
            }

            if (hasTop)
            {
                Consoul.Write(
                    "\u2580",
                    new ColorScheme
                    {
                        Color = top.ToConsoleColor(),
                        BackgroundColor = defaultScheme.BackgroundColor
                    },
                    false);
                return;
            }

            if (hasBottom)
            {
                Consoul.Write(
                    "\u2584",
                    new ColorScheme
                    {
                        Color = bottom.ToConsoleColor(),
                        BackgroundColor = defaultScheme.BackgroundColor
                    },
                    false);
                return;
            }

            Consoul.Write(" ", defaultScheme, false);
        }

        private bool IsVisible(CanvasImagePixel pixel)
        {
            return pixel.Alpha >= _options.TransparentThreshold;
        }

        private static CanvasImagePixel[,] ConvertPixels(ConsoleColor[,] colors)
        {
            if (colors == null)
            {
                throw new ArgumentNullException(nameof(colors));
            }

            int height = Math.Max(1, colors.GetLength(0));
            int width = Math.Max(1, colors.GetLength(1));
            var pixels = new CanvasImagePixel[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pixels[y, x] = CanvasImagePixel.FromConsoleColor(colors[y, x]);
                }
            }

            return pixels;
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        private static byte Blend(byte start, byte end, double amount)
        {
            return (byte)Clamp((int)Math.Round(start + ((end - start) * amount)), 0, 255);
        }

        private readonly struct RenderSize
        {
            public RenderSize(int width, int height)
            {
                Width = width;
                Height = height;
            }

            public int Width { get; }

            public int Height { get; }
        }
    }
}
