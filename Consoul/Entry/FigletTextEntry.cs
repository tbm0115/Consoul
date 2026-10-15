namespace ConsoulLibrary
{
    /// <summary>
    /// Renders large ASCII text for application titles, splash screens, and section headers.
    /// </summary>
    public class FigletTextEntry : IRenderable
    {
        private readonly FigletTextOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="FigletTextEntry"/> class.
        /// </summary>
        /// <param name="text">The text to render as large ASCII glyphs.</param>
        /// <param name="options">Optional rendering options for the text.</param>
        public FigletTextEntry(string text, FigletTextOptions options = null)
        {
            Text = text ?? string.Empty;
            _options = options != null ? options.Clone() : new FigletTextOptions();
        }

        /// <summary>
        /// Gets or sets the text rendered as large ASCII glyphs.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets the options used to render the text.
        /// </summary>
        public FigletTextOptions Options => _options;

        /// <summary>
        /// Renders the text to the active console driver.
        /// </summary>
        public void Render()
        {
            new HeadlineEntry(Text, _options.ToHeadlineOptions()).Render();
        }

        /// <summary>
        /// Renders temporary large ASCII text to the active console driver.
        /// </summary>
        /// <param name="text">The text to render as large ASCII glyphs.</param>
        /// <param name="options">Optional rendering options for the text.</param>
        public static void Render(string text, FigletTextOptions options = null)
        {
            new FigletTextEntry(text, options).Render();
        }
    }
}
