using System;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent terminal path construction over <see cref="TextPathEntry"/>.
    /// </summary>
    public class TextPath : TextPathEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextPath"/> class.
        /// </summary>
        /// <param name="path">The path text to render.</param>
        public TextPath(string path) : base(path)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextPath"/> class with explicit options.
        /// </summary>
        /// <param name="path">The path text to render.</param>
        /// <param name="options">Optional rendering options for the path.</param>
        public TextPath(string path, TextPathOptions options) : base(path, options)
        {
        }

        /// <summary>
        /// Sets the requested render width.
        /// </summary>
        /// <param name="width">The requested width, or <see langword="null"/> to use the active terminal width.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath Width(int? width)
        {
            if (width.HasValue && width.Value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than zero.");
            }

            Options.Width = width;
            return this;
        }

        /// <summary>
        /// Sets the separator used when rendering path segments.
        /// </summary>
        /// <param name="separator">The separator text, or <see langword="null"/> to infer it from the path.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath Separator(string separator)
        {
            Options.Separator = separator;
            return this;
        }

        /// <summary>
        /// Sets the text used when middle path segments or long file names are elided.
        /// </summary>
        /// <param name="ellipsis">The ellipsis text.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath Ellipsis(string ellipsis)
        {
            Options.Ellipsis = string.IsNullOrEmpty(ellipsis) ? "..." : ellipsis;
            return this;
        }

        /// <summary>
        /// Sets the root segment color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath RootColor(ConsoleColor color)
        {
            Options.RootScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the root segment color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath RootColor(SpectreColor color)
        {
            Options.RootScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the root segment color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath RootColor(RgbColor color)
        {
            Options.RootScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the root segment color from a reusable style.
        /// </summary>
        /// <param name="style">The root segment style.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath RootColor(Style style)
        {
            return RootStyle(style);
        }

        /// <summary>
        /// Sets the root segment color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath RootColor(string style)
        {
            Options.RootScheme = ParseScheme(style, Options.ResolveRootScheme());
            return this;
        }

        /// <summary>
        /// Sets the root segment style.
        /// </summary>
        /// <param name="style">The root segment style.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath RootStyle(Style style)
        {
            Options.RootScheme = ToScheme(style, Options.ResolveRootScheme());
            return this;
        }

        /// <summary>
        /// Sets the directory segment color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath DirectoryColor(ConsoleColor color)
        {
            Options.DirectoryScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the directory segment color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath DirectoryColor(SpectreColor color)
        {
            Options.DirectoryScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the directory segment color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath DirectoryColor(RgbColor color)
        {
            Options.DirectoryScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the directory segment color from a reusable style.
        /// </summary>
        /// <param name="style">The directory segment style.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath DirectoryColor(Style style)
        {
            return DirectoryStyle(style);
        }

        /// <summary>
        /// Sets the directory segment color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath DirectoryColor(string style)
        {
            Options.DirectoryScheme = ParseScheme(style, Options.ResolveDirectoryScheme());
            return this;
        }

        /// <summary>
        /// Sets the directory segment style.
        /// </summary>
        /// <param name="style">The directory segment style.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath DirectoryStyle(Style style)
        {
            Options.DirectoryScheme = ToScheme(style, Options.ResolveDirectoryScheme());
            return this;
        }

        /// <summary>
        /// Sets the file name or final segment color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath FileNameColor(ConsoleColor color)
        {
            Options.FileNameScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the file name or final segment color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath FileNameColor(SpectreColor color)
        {
            Options.FileNameScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the file name or final segment color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath FileNameColor(RgbColor color)
        {
            Options.FileNameScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the file name or final segment color from a reusable style.
        /// </summary>
        /// <param name="style">The file name style.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath FileNameColor(Style style)
        {
            return FileNameStyle(style);
        }

        /// <summary>
        /// Sets the file name or final segment color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath FileNameColor(string style)
        {
            Options.FileNameScheme = ParseScheme(style, Options.ResolveFileNameScheme());
            return this;
        }

        /// <summary>
        /// Sets the file name or final segment style.
        /// </summary>
        /// <param name="style">The file name style.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath FileNameStyle(Style style)
        {
            Options.FileNameScheme = ToScheme(style, Options.ResolveFileNameScheme());
            return this;
        }

        /// <summary>
        /// Sets the stem color for the final path segment before an extension.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath StemColor(ConsoleColor color)
        {
            return FileNameColor(color);
        }

        /// <summary>
        /// Sets the stem color for the final path segment before an extension.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath StemColor(SpectreColor color)
        {
            return FileNameColor(color);
        }

        /// <summary>
        /// Sets the stem color for the final path segment before an extension.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath StemColor(RgbColor color)
        {
            return FileNameColor(color);
        }

        /// <summary>
        /// Sets the stem color for the final path segment before an extension.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath StemColor(string style)
        {
            return FileNameColor(style);
        }

        /// <summary>
        /// Sets the stem color for the final path segment before an extension from a reusable style.
        /// </summary>
        /// <param name="style">The stem style.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath StemColor(Style style)
        {
            return StemStyle(style);
        }

        /// <summary>
        /// Sets the stem style for the final path segment before an extension.
        /// </summary>
        /// <param name="style">The stem style.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath StemStyle(Style style)
        {
            return FileNameStyle(style);
        }

        /// <summary>
        /// Sets the extension color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath ExtensionColor(ConsoleColor color)
        {
            Options.ExtensionScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the extension color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath ExtensionColor(SpectreColor color)
        {
            Options.ExtensionScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the extension color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath ExtensionColor(RgbColor color)
        {
            Options.ExtensionScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the extension color from a reusable style.
        /// </summary>
        /// <param name="style">The extension style.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath ExtensionColor(Style style)
        {
            return ExtensionStyle(style);
        }

        /// <summary>
        /// Sets the extension color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>yellow</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath ExtensionColor(string style)
        {
            Options.ExtensionScheme = ParseScheme(style, Options.ResolveExtensionScheme());
            return this;
        }

        /// <summary>
        /// Sets the extension style.
        /// </summary>
        /// <param name="style">The extension style.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath ExtensionStyle(Style style)
        {
            Options.ExtensionScheme = ToScheme(style, Options.ResolveExtensionScheme());
            return this;
        }

        /// <summary>
        /// Sets the final path segment color, including both stem and extension.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath LeafColor(ConsoleColor color)
        {
            Options.FileNameScheme = Scheme(color);
            Options.ExtensionScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the final path segment color, including both stem and extension.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath LeafColor(SpectreColor color)
        {
            ColorScheme scheme = Scheme(color);
            Options.FileNameScheme = scheme;
            Options.ExtensionScheme = scheme;
            return this;
        }

        /// <summary>
        /// Sets the final path segment color, including both stem and extension.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath LeafColor(RgbColor color)
        {
            ColorScheme scheme = Scheme(color);
            Options.FileNameScheme = scheme;
            Options.ExtensionScheme = scheme;
            return this;
        }

        /// <summary>
        /// Sets the final path segment color from a reusable style, including both stem and extension.
        /// </summary>
        /// <param name="style">The final segment style.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath LeafColor(Style style)
        {
            return LeafStyle(style);
        }

        /// <summary>
        /// Sets the final path segment color, including both stem and extension.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>aqua</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath LeafColor(string style)
        {
            ColorScheme scheme = ParseScheme(style, Options.ResolveFileNameScheme());
            Options.FileNameScheme = scheme;
            Options.ExtensionScheme = scheme;
            return this;
        }

        /// <summary>
        /// Sets the final path segment style, including both stem and extension.
        /// </summary>
        /// <param name="style">The final segment style.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath LeafStyle(Style style)
        {
            ColorScheme scheme = ToScheme(style, Options.ResolveFileNameScheme());
            Options.FileNameScheme = scheme;
            Options.ExtensionScheme = scheme;
            return this;
        }

        /// <summary>
        /// Sets the path separator and ellipsis marker color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath SeparatorColor(ConsoleColor color)
        {
            Options.SeparatorScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the path separator and ellipsis marker color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath SeparatorColor(SpectreColor color)
        {
            Options.SeparatorScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the path separator and ellipsis marker color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath SeparatorColor(RgbColor color)
        {
            Options.SeparatorScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the path separator and ellipsis marker color from a reusable style.
        /// </summary>
        /// <param name="style">The separator style.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath SeparatorColor(Style style)
        {
            return SeparatorStyle(style);
        }

        /// <summary>
        /// Sets the path separator and ellipsis marker color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>yellow</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath SeparatorColor(string style)
        {
            Options.SeparatorScheme = ParseScheme(style, Options.ResolveSeparatorScheme());
            return this;
        }

        /// <summary>
        /// Sets the path separator and ellipsis marker style.
        /// </summary>
        /// <param name="style">The separator style.</param>
        /// <returns>The current <see cref="TextPath"/> instance.</returns>
        public TextPath SeparatorStyle(Style style)
        {
            Options.SeparatorScheme = ToScheme(style, Options.ResolveSeparatorScheme());
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

        private static ColorScheme Scheme(SpectreColor color)
        {
            return Scheme(color.ToConsoleColor());
        }

        private static ColorScheme Scheme(RgbColor color)
        {
            return Scheme(color.ToConsoleColor());
        }

        private static ColorScheme ParseScheme(string style, ColorScheme fallback)
        {
            return Style.Parse(style).ToColorScheme(fallback);
        }

        private static ColorScheme ToScheme(Style style, ColorScheme fallback)
        {
            if (style == null)
            {
                throw new ArgumentNullException(nameof(style));
            }

            return style.ToColorScheme(fallback);
        }
    }
}
