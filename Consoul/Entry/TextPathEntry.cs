using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders responsive, colorized terminal paths for console views and diagnostics.
    /// </summary>
    public class TextPathEntry : IRenderable
    {
        private readonly TextPathOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextPathEntry"/> class.
        /// </summary>
        /// <param name="path">The path text to render.</param>
        /// <param name="options">Optional rendering options for the path.</param>
        public TextPathEntry(string path, TextPathOptions options = null)
        {
            Path = path ?? string.Empty;
            _options = options != null ? options.Clone() : new TextPathOptions();
        }

        /// <summary>
        /// Gets or sets the path text rendered by this entry.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets the options used to render the path.
        /// </summary>
        public TextPathOptions Options => _options;

        /// <summary>
        /// Renders the path to the active console driver.
        /// </summary>
        public void Render()
        {
            int width = PanelEntry.ResolveWidth(_options.Width);
            string separator = ResolveSeparator(Path, _options.Separator);
            string ellipsis = string.IsNullOrEmpty(_options.Ellipsis) ? "..." : _options.Ellipsis;
            PathModel model = Shorten(Parse(Path, separator), width, separator, ellipsis);
            RenderModel(model, ellipsis);
            Consoul.Write(string.Empty, RenderOptions.DefaultScheme, true);
        }

        /// <summary>
        /// Renders a temporary text path to the active console driver.
        /// </summary>
        /// <param name="path">The path text to render.</param>
        /// <param name="options">Optional rendering options for the path.</param>
        public static void Render(string path, TextPathOptions options = null)
        {
            new TextPathEntry(path, options).Render();
        }

        private static string ResolveSeparator(string path, string configuredSeparator)
        {
            if (!string.IsNullOrEmpty(configuredSeparator))
            {
                return configuredSeparator;
            }

            path = path ?? string.Empty;
            bool hasForward = path.IndexOf('/') >= 0;
            bool hasBack = path.IndexOf('\\') >= 0;
            if (hasForward && !hasBack)
            {
                return "/";
            }

            return "\\";
        }

        private static PathModel Parse(string path, string separator)
        {
            path = path ?? string.Empty;
            int index = 0;
            string root = string.Empty;
            bool rootEndsWithSeparator = false;

            if (path.Length >= 2 && IsSeparator(path[0]) && IsSeparator(path[1]))
            {
                string[] uncParts = path.Substring(2).Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (uncParts.Length >= 2)
                {
                    root = separator + separator + uncParts[0] + separator + uncParts[1];
                    index = FindIndexAfterParts(path, 2, 2);
                }
                else if (uncParts.Length == 1)
                {
                    root = separator + separator + uncParts[0];
                    index = path.Length;
                }
                else
                {
                    root = separator + separator;
                    rootEndsWithSeparator = true;
                    index = path.Length;
                }
            }
            else if (path.Length >= 2 && path[1] == ':')
            {
                root = path.Substring(0, 2);
                index = 2;
                if (index < path.Length && IsSeparator(path[index]))
                {
                    index++;
                }
            }
            else if (path.Length > 0 && IsSeparator(path[0]))
            {
                root = separator;
                rootEndsWithSeparator = true;
                index = 1;
            }

            string remainder = index < path.Length ? path.Substring(index) : string.Empty;
            var parts = remainder
                .Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            string leaf = parts.Count > 0 ? parts[parts.Count - 1] : string.Empty;
            if (parts.Count > 0)
            {
                parts.RemoveAt(parts.Count - 1);
            }

            if (leaf.Length == 0 && root.Length == 0 && path.Length > 0)
            {
                leaf = path;
            }

            return new PathModel(root, rootEndsWithSeparator, parts, leaf, false);
        }

        private static int FindIndexAfterParts(string path, int start, int partCount)
        {
            int index = start;
            int found = 0;
            bool inPart = false;
            while (index < path.Length)
            {
                bool separator = IsSeparator(path[index]);
                if (separator)
                {
                    if (inPart)
                    {
                        found++;
                        inPart = false;
                        if (found == partCount)
                        {
                            return index + 1;
                        }
                    }
                }
                else
                {
                    inPart = true;
                }

                index++;
            }

            return path.Length;
        }

        private static bool IsSeparator(char value)
        {
            return value == '\\' || value == '/';
        }

        private static PathModel Shorten(PathModel model, int width, string separator, string ellipsis)
        {
            width = Math.Max(1, width);
            if (Measure(model, separator, ellipsis) <= width)
            {
                return model;
            }

            var directories = new List<string>(model.Directories);
            bool omitted = model.OmittedDirectories;
            while (directories.Count > 0)
            {
                directories.RemoveAt(0);
                omitted = true;
                var candidate = new PathModel(model.Root, model.RootEndsWithSeparator, directories, model.Leaf, omitted);
                if (Measure(candidate, separator, ellipsis) <= width)
                {
                    return candidate;
                }
            }

            var leafCandidate = new PathModel(model.Root, model.RootEndsWithSeparator, directories, model.Leaf, omitted);
            int prefixWidth = MeasurePrefix(leafCandidate, separator, ellipsis);
            int leafWidth = width - prefixWidth;
            if (leafWidth > 0)
            {
                leafCandidate = new PathModel(
                    model.Root,
                    model.RootEndsWithSeparator,
                    directories,
                    TrimMiddle(model.Leaf, leafWidth, ellipsis),
                    omitted);

                if (Measure(leafCandidate, separator, ellipsis) <= width)
                {
                    return leafCandidate;
                }
            }

            string normalized = BuildPlainText(model, separator, ellipsis);
            return new PathModel(string.Empty, false, new List<string>(), TrimMiddle(normalized, width, ellipsis), false);
        }

        private static int Measure(PathModel model, string separator, string ellipsis)
        {
            return MeasurePrefix(model, separator, ellipsis) + (model.Leaf ?? string.Empty).Length;
        }

        private static int MeasurePrefix(PathModel model, string separator, string ellipsis)
        {
            int width = (model.Root ?? string.Empty).Length;
            bool hasContent = width > 0;

            if (model.OmittedDirectories)
            {
                if (hasContent && !model.RootEndsWithSeparator)
                {
                    width += separator.Length;
                }

                width += ellipsis.Length;
                hasContent = true;
            }

            foreach (string directory in model.Directories)
            {
                if (hasContent && !EndsWithSeparator(width, model.RootEndsWithSeparator, model.OmittedDirectories, model.Directories, directory))
                {
                    width += separator.Length;
                }

                width += (directory ?? string.Empty).Length;
                hasContent = true;
            }

            if (!string.IsNullOrEmpty(model.Leaf) && hasContent && !EndsWithSeparatorBeforeLeaf(model))
            {
                width += separator.Length;
            }

            return width;
        }

        private static bool EndsWithSeparatorBeforeLeaf(PathModel model)
        {
            return model.RootEndsWithSeparator
                && !model.OmittedDirectories
                && model.Directories.Count == 0;
        }

        private static bool EndsWithSeparator(
            int currentWidth,
            bool rootEndsWithSeparator,
            bool omittedDirectories,
            IList<string> directories,
            string currentDirectory)
        {
            if (currentWidth == 0)
            {
                return true;
            }

            if (rootEndsWithSeparator && !omittedDirectories && directories.Count > 0 && directories[0] == currentDirectory)
            {
                return true;
            }

            return false;
        }

        private static string BuildPlainText(PathModel model, string separator, string ellipsis)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(model.Root))
            {
                parts.Add(model.Root);
            }

            if (model.OmittedDirectories)
            {
                parts.Add(ellipsis);
            }

            parts.AddRange(model.Directories);
            if (!string.IsNullOrEmpty(model.Leaf))
            {
                parts.Add(model.Leaf);
            }

            if (parts.Count == 0)
            {
                return string.Empty;
            }

            string text = parts[0];
            for (int index = 1; index < parts.Count; index++)
            {
                bool previousRootEndsWithSeparator = index == 1 && model.RootEndsWithSeparator && !model.OmittedDirectories;
                text += previousRootEndsWithSeparator ? parts[index] : separator + parts[index];
            }

            return text;
        }

        private static string TrimMiddle(string text, int width, string ellipsis)
        {
            text = text ?? string.Empty;
            width = Math.Max(1, width);
            ellipsis = string.IsNullOrEmpty(ellipsis) ? "..." : ellipsis;

            if (text.Length <= width)
            {
                return text;
            }

            if (width <= ellipsis.Length)
            {
                return ellipsis.Substring(0, width);
            }

            int remaining = width - ellipsis.Length;
            int left = Math.Max(1, remaining / 2);
            int right = remaining - left;
            return text.Substring(0, left) + ellipsis + text.Substring(text.Length - right);
        }

        private void RenderModel(PathModel model, string ellipsis)
        {
            bool hasContent = false;
            if (!string.IsNullOrEmpty(model.Root))
            {
                WriteRoot(model.Root);
                hasContent = true;
            }

            if (model.OmittedDirectories)
            {
                WriteSeparatorIfNeeded(hasContent, model.RootEndsWithSeparator);
                WriteSeparator(ellipsis);
                hasContent = true;
            }

            for (int index = 0; index < model.Directories.Count; index++)
            {
                WriteSeparatorIfNeeded(hasContent, model.RootEndsWithSeparator && index == 0 && !model.OmittedDirectories);
                WriteDirectory(model.Directories[index]);
                hasContent = true;
            }

            if (!string.IsNullOrEmpty(model.Leaf))
            {
                WriteSeparatorIfNeeded(hasContent, EndsWithSeparatorBeforeLeaf(model));
                WriteLeaf(model.Leaf);
            }
        }

        private void WriteSeparatorIfNeeded(bool hasContent, bool previousAlreadyEndedWithSeparator)
        {
            if (hasContent && !previousAlreadyEndedWithSeparator)
            {
                WriteSeparator(ResolveSeparator(Path, _options.Separator));
            }
        }

        private void WriteLeaf(string leaf)
        {
            int extensionIndex = leaf.LastIndexOf('.');
            if (extensionIndex > 0 && extensionIndex < leaf.Length - 1)
            {
                WriteFileName(leaf.Substring(0, extensionIndex));
                WriteExtension(leaf.Substring(extensionIndex));
                return;
            }

            WriteFileName(leaf);
        }

        private void WriteRoot(string value)
        {
            WriteStyled(
                value,
                _options.ResolveRootScheme(),
                _options.RootDecoration,
                _options.RootForegroundRgb,
                _options.RootBackgroundRgb);
        }

        private void WriteDirectory(string value)
        {
            WriteStyled(
                value,
                _options.ResolveDirectoryScheme(),
                _options.DirectoryDecoration,
                _options.DirectoryForegroundRgb,
                _options.DirectoryBackgroundRgb);
        }

        private void WriteFileName(string value)
        {
            WriteStyled(
                value,
                _options.ResolveFileNameScheme(),
                _options.FileNameDecoration,
                _options.FileNameForegroundRgb,
                _options.FileNameBackgroundRgb);
        }

        private void WriteExtension(string value)
        {
            WriteStyled(
                value,
                _options.ResolveExtensionScheme(),
                _options.ExtensionDecoration,
                _options.ExtensionForegroundRgb,
                _options.ExtensionBackgroundRgb);
        }

        private void WriteSeparator(string value)
        {
            WriteStyled(
                value,
                _options.ResolveSeparatorScheme(),
                _options.SeparatorDecoration,
                _options.SeparatorForegroundRgb,
                _options.SeparatorBackgroundRgb);
        }

        private static void WriteStyled(
            string value,
            ColorScheme scheme,
            Decoration decoration,
            RgbColor? foregroundRgb,
            RgbColor? backgroundRgb)
        {
            TerminalLinkWriter.WriteText(value, scheme, null, decoration, foregroundRgb, backgroundRgb);
        }

        private readonly struct PathModel
        {
            public PathModel(string root, bool rootEndsWithSeparator, IList<string> directories, string leaf, bool omittedDirectories)
            {
                Root = root ?? string.Empty;
                RootEndsWithSeparator = rootEndsWithSeparator;
                Directories = directories ?? new List<string>();
                Leaf = leaf ?? string.Empty;
                OmittedDirectories = omittedDirectories;
            }

            public string Root { get; }

            public bool RootEndsWithSeparator { get; }

            public IList<string> Directories { get; }

            public string Leaf { get; }

            public bool OmittedDirectories { get; }
        }
    }
}
