namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for dependency-free terminal canvas images.
    /// </summary>
    public class CanvasImageOptions
    {
        /// <summary>
        /// Gets or sets the maximum rendered cell width. The active terminal width is always respected.
        /// </summary>
        public int? MaxWidth { get; set; }

        /// <summary>
        /// Gets or sets the exact rendered pixel width before terminal half-block composition.
        /// </summary>
        public int? PixelWidth { get; set; }

        /// <summary>
        /// Gets or sets the maximum rendered terminal row height.
        /// </summary>
        public int? MaxHeight { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the source pixels are sampled to fit the resolved render size.
        /// </summary>
        public bool Scale { get; set; } = true;

        /// <summary>
        /// Gets or sets the resampler used when the image is scaled.
        /// </summary>
        public CanvasImageResampler Resampler { get; set; } = CanvasImageResampler.Bicubic;

        /// <summary>
        /// Gets or sets the minimum alpha value that should be considered visible.
        /// </summary>
        public byte TransparentThreshold { get; set; } = 1;

        /// <summary>
        /// Creates a copy of the current options.
        /// </summary>
        /// <returns>A new <see cref="CanvasImageOptions"/> instance with the same values.</returns>
        public CanvasImageOptions Clone()
        {
            return new CanvasImageOptions
            {
                MaxWidth = MaxWidth,
                PixelWidth = PixelWidth,
                MaxHeight = MaxHeight,
                Scale = Scale,
                Resampler = Resampler,
                TransparentThreshold = TransparentThreshold
            };
        }
    }
}
