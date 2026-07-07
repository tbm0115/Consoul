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
        /// Gets or sets the text decoration applied to root path segments on ANSI-capable terminals.
        /// </summary>
        public Decoration RootDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional 24-bit root foreground color used on extended-color terminals.
        /// </summary>
        public RgbColor? RootForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional 24-bit root background color used on extended-color terminals.
        /// </summary>
        public RgbColor? RootBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for directory path segments.
        /// </summary>
        public ColorScheme? DirectoryScheme { get; set; }

        /// <summary>
        /// Gets or sets the text decoration applied to directory path segments on ANSI-capable terminals.
        /// </summary>
        public Decoration DirectoryDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional 24-bit directory foreground color used on extended-color terminals.
        /// </summary>
        public RgbColor? DirectoryForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional 24-bit directory background color used on extended-color terminals.
        /// </summary>
        public RgbColor? DirectoryBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for the file name or final path segment.
        /// </summary>
        public ColorScheme? FileNameScheme { get; set; }

        /// <summary>
        /// Gets or sets the text decoration applied to file name segments on ANSI-capable terminals.
        /// </summary>
        public Decoration FileNameDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional 24-bit file name foreground color used on extended-color terminals.
        /// </summary>
        public RgbColor? FileNameForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional 24-bit file name background color used on extended-color terminals.
        /// </summary>
        public RgbColor? FileNameBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for the final path segment extension.
        /// </summary>
        public ColorScheme? ExtensionScheme { get; set; }

        /// <summary>
        /// Gets or sets the text decoration applied to extension segments on ANSI-capable terminals.
        /// </summary>
        public Decoration ExtensionDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional 24-bit extension foreground color used on extended-color terminals.
        /// </summary>
        public RgbColor? ExtensionForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional 24-bit extension background color used on extended-color terminals.
        /// </summary>
        public RgbColor? ExtensionBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for path separators and elision markers.
        /// </summary>
        public ColorScheme? SeparatorScheme { get; set; }

        /// <summary>
        /// Gets or sets the text decoration applied to path separators and elision markers on ANSI-capable terminals.
        /// </summary>
        public Decoration SeparatorDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional 24-bit separator foreground color used on extended-color terminals.
        /// </summary>
        public RgbColor? SeparatorForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional 24-bit separator background color used on extended-color terminals.
        /// </summary>
        public RgbColor? SeparatorBackgroundRgb { get; set; }

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
                RootDecoration = RootDecoration,
                RootForegroundRgb = RootForegroundRgb,
                RootBackgroundRgb = RootBackgroundRgb,
                DirectoryScheme = DirectoryScheme,
                DirectoryDecoration = DirectoryDecoration,
                DirectoryForegroundRgb = DirectoryForegroundRgb,
                DirectoryBackgroundRgb = DirectoryBackgroundRgb,
                FileNameScheme = FileNameScheme,
                FileNameDecoration = FileNameDecoration,
                FileNameForegroundRgb = FileNameForegroundRgb,
                FileNameBackgroundRgb = FileNameBackgroundRgb,
                ExtensionScheme = ExtensionScheme,
                ExtensionDecoration = ExtensionDecoration,
                ExtensionForegroundRgb = ExtensionForegroundRgb,
                ExtensionBackgroundRgb = ExtensionBackgroundRgb,
                SeparatorScheme = SeparatorScheme,
                SeparatorDecoration = SeparatorDecoration,
                SeparatorForegroundRgb = SeparatorForegroundRgb,
                SeparatorBackgroundRgb = SeparatorBackgroundRgb
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
