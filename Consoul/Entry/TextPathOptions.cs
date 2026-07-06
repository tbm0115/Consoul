using ConsoulLibrary.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for responsive, colorized terminal paths.
    /// </summary>
    public class TextPathOptions
    {
        /// <summary>
        /// Gets or sets the requested render width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the separator used when rendering path segments. When omitted, Consoul infers it from the path.
        /// </summary>
        public string Separator { get; set; }

        /// <summary>
        /// Gets or sets the text used when middle path segments or long file names are elided.
        /// </summary>
        public string Ellipsis { get; set; } = "...";

        /// <summary>
        /// Gets or sets the color scheme used for root path segments such as <c>C:</c>, <c>/</c>, or UNC roots.
        /// </summary>
        public ColorScheme? RootScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for directory path segments.
        /// </summary>
        public ColorScheme? DirectoryScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for the file name or final path segment.
        /// </summary>
        public ColorScheme? FileNameScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for the final path segment extension.
        /// </summary>
        public ColorScheme? ExtensionScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for path separators and elision markers.
        /// </summary>
        public ColorScheme? SeparatorScheme { get; set; }

        /// <summary>
        /// Creates a copy of the current text path options.
        /// </summary>
        /// <returns>A new <see cref="TextPathOptions"/> instance with the same values.</returns>
        public TextPathOptions Clone()
        {
            return new TextPathOptions
            {
                Width = Width,
                Separator = Separator,
                Ellipsis = Ellipsis,
                RootScheme = RootScheme,
                DirectoryScheme = DirectoryScheme,
                FileNameScheme = FileNameScheme,
                ExtensionScheme = ExtensionScheme,
                SeparatorScheme = SeparatorScheme
            };
        }

        internal ColorScheme ResolveRootScheme()
        {
            return RootScheme ?? RenderOptions.SubnoteScheme;
        }

        internal ColorScheme ResolveDirectoryScheme()
        {
            return DirectoryScheme ?? RenderOptions.DefaultScheme;
        }

        internal ColorScheme ResolveFileNameScheme()
        {
            return FileNameScheme ?? RenderOptions.PromptScheme;
        }

        internal ColorScheme ResolveExtensionScheme()
        {
            return ExtensionScheme ?? RenderOptions.OptionScheme;
        }

        internal ColorScheme ResolveSeparatorScheme()
        {
            return SeparatorScheme ?? RenderOptions.SubnoteScheme;
        }
    }
}
