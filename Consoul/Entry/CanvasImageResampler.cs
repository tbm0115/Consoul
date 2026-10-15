namespace ConsoulLibrary
{
    /// <summary>
    /// Specifies the resampling method used when a canvas image is scaled for the terminal.
    /// </summary>
    public enum CanvasImageResampler
    {
        /// <summary>
        /// Samples the nearest source pixel. This is fastest and keeps a pixel-art appearance.
        /// </summary>
        NearestNeighbor,

        /// <summary>
        /// Blends neighboring pixels before mapping the result to the nearest console color.
        /// </summary>
        Bilinear,

        /// <summary>
        /// Uses the high-quality compatibility mode. In the dependency-free core renderer this maps to bilinear sampling.
        /// </summary>
        Bicubic
    }
}
