using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent terminal image construction over <see cref="CanvasImageEntry"/>.
    /// </summary>
    public class CanvasImage : CanvasImageEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasImage"/> class from a row-major pixel matrix.
        /// </summary>
        /// <param name="pixels">The source pixels, indexed as <c>[y, x]</c>.</param>
        public CanvasImage(CanvasImagePixel[,] pixels)
            : base(pixels)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasImage"/> class from a row-major pixel matrix.
        /// </summary>
        /// <param name="pixels">The source pixels, indexed as <c>[y, x]</c>.</param>
        /// <param name="options">Optional rendering options.</param>
        public CanvasImage(CanvasImagePixel[,] pixels, CanvasImageOptions options)
            : base(pixels, options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasImage"/> class from a row-major console color matrix.
        /// </summary>
        /// <param name="pixels">The source colors, indexed as <c>[y, x]</c>.</param>
        public CanvasImage(ConsoleColor[,] pixels)
            : base(pixels)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasImage"/> class from a row-major console color matrix.
        /// </summary>
        /// <param name="pixels">The source colors, indexed as <c>[y, x]</c>.</param>
        /// <param name="options">Optional rendering options.</param>
        public CanvasImage(ConsoleColor[,] pixels, CanvasImageOptions options)
            : base(pixels, options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasImage"/> class from a pixel factory.
        /// </summary>
        /// <param name="width">The source image width.</param>
        /// <param name="height">The source image height.</param>
        /// <param name="pixelFactory">A factory that returns the pixel for each <c>x</c> and <c>y</c> coordinate.</param>
        public CanvasImage(int width, int height, Func<int, int, CanvasImagePixel> pixelFactory)
            : base(width, height, pixelFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasImage"/> class from a pixel factory.
        /// </summary>
        /// <param name="width">The source image width.</param>
        /// <param name="height">The source image height.</param>
        /// <param name="pixelFactory">A factory that returns the pixel for each <c>x</c> and <c>y</c> coordinate.</param>
        /// <param name="options">Optional rendering options.</param>
        public CanvasImage(int width, int height, Func<int, int, CanvasImagePixel> pixelFactory, CanvasImageOptions options)
            : base(width, height, pixelFactory, options)
        {
        }

        /// <summary>
        /// Sets the maximum rendered cell width.
        /// </summary>
        /// <param name="maxWidth">The maximum rendered width.</param>
        /// <returns>The current <see cref="CanvasImage"/> instance.</returns>
        public new CanvasImage MaxWidth(int? maxWidth)
        {
            if (maxWidth.HasValue && maxWidth.Value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxWidth), "Maximum width must be at least 1.");
            }

            Options.MaxWidth = maxWidth;
            return this;
        }

        /// <summary>
        /// Removes the explicit maximum rendered cell width.
        /// </summary>
        /// <returns>The current <see cref="CanvasImage"/> instance.</returns>
        public CanvasImage NoMaxWidth()
        {
            Options.MaxWidth = null;
            return this;
        }

        /// <summary>
        /// Sets the exact rendered pixel width before terminal half-block composition.
        /// </summary>
        /// <param name="width">The rendered pixel width.</param>
        /// <returns>The current <see cref="CanvasImage"/> instance.</returns>
        public new CanvasImage PixelWidth(int width)
        {
            if (width < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Pixel width must be at least 1.");
            }

            Options.PixelWidth = width;
            return this;
        }

        /// <summary>
        /// Sets the maximum rendered terminal row height.
        /// </summary>
        /// <param name="height">The maximum rendered terminal row height.</param>
        /// <returns>The current <see cref="CanvasImage"/> instance.</returns>
        public CanvasImage MaxHeight(int? height)
        {
            if (height.HasValue && height.Value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Maximum height must be at least 1.");
            }

            Options.MaxHeight = height;
            return this;
        }

        /// <summary>
        /// Sets whether the image should be resampled to fit the requested size.
        /// </summary>
        /// <param name="enabled">A value indicating whether scaling is enabled.</param>
        /// <returns>The current <see cref="CanvasImage"/> instance.</returns>
        public CanvasImage Scale(bool enabled = true)
        {
            Options.Scale = enabled;
            return this;
        }

        /// <summary>
        /// Disables image scaling so rendering clips the top-left source pixels to the configured bounds.
        /// </summary>
        /// <returns>The current <see cref="CanvasImage"/> instance.</returns>
        public CanvasImage NoScale()
        {
            Options.Scale = false;
            return this;
        }

        /// <summary>
        /// Uses nearest-neighbor sampling when scaling.
        /// </summary>
        /// <returns>The current <see cref="CanvasImage"/> instance.</returns>
        public CanvasImage NearestNeighborResampler()
        {
            Options.Resampler = CanvasImageResampler.NearestNeighbor;
            return this;
        }

        /// <summary>
        /// Uses bilinear sampling when scaling.
        /// </summary>
        /// <returns>The current <see cref="CanvasImage"/> instance.</returns>
        public CanvasImage BilinearResampler()
        {
            Options.Resampler = CanvasImageResampler.Bilinear;
            return this;
        }

        /// <summary>
        /// Uses high-quality compatibility sampling when scaling.
        /// </summary>
        /// <returns>The current <see cref="CanvasImage"/> instance.</returns>
        public CanvasImage BicubicResampler()
        {
            Options.Resampler = CanvasImageResampler.Bicubic;
            return this;
        }

        /// <summary>
        /// Sets the alpha threshold below which pixels render as transparent.
        /// </summary>
        /// <param name="threshold">The minimum alpha value that should be visible.</param>
        /// <returns>The current <see cref="CanvasImage"/> instance.</returns>
        public CanvasImage TransparentThreshold(byte threshold)
        {
            Options.TransparentThreshold = threshold;
            return this;
        }
    }
}
