using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders a responsive tree for nested console view content.
    /// </summary>
    public class TreeEntry : IRenderable
    {
        private readonly TreeOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeEntry"/> class.
        /// </summary>
        /// <param name="root">The root node to render.</param>
        /// <param name="options">Optional rendering options for the tree.</param>
        public TreeEntry(TreeNode root, TreeOptions options = null)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));
            _options = options != null ? options.Clone() : new TreeOptions();
        }

        /// <summary>
        /// Gets the root node rendered by the tree.
        /// </summary>
        public TreeNode Root { get; }

        /// <summary>
        /// Gets the options used to render the tree.
        /// </summary>
        public TreeOptions Options => _options;

        /// <summary>
        /// Renders the tree to the active console driver.
        /// </summary>
        public void Render()
        {
            int width = PanelEntry.ResolveWidth(_options.Width);
            if (width <= 0)
            {
                return;
            }

            if (_options.IncludeRoot)
            {
                RenderNode(Root, string.Empty, string.Empty, width);
                return;
            }

            RenderChildren(Root.Children, string.Empty, width);
        }

        /// <summary>
        /// Renders a temporary tree to the active console driver.
        /// </summary>
        /// <param name="root">The root node to render.</param>
        /// <param name="options">Optional rendering options for the tree.</param>
        public static void Render(TreeNode root, TreeOptions options = null)
        {
            new TreeEntry(root, options).Render();
        }

        private void RenderChildren(IList<TreeNode> children, string continuationPrefix, int width)
        {
            if (children == null)
            {
                return;
            }

            int visibleCount = 0;
            foreach (TreeNode child in children)
            {
                if (child != null)
                {
                    visibleCount++;
                }
            }

            int visibleIndex = 0;
            foreach (TreeNode child in children)
            {
                if (child == null)
                {
                    continue;
                }

                visibleIndex++;
                bool isLast = visibleIndex == visibleCount;
                string branchPrefix = continuationPrefix + _options.ResolveBranch(isLast);
                string childContinuationPrefix = continuationPrefix + _options.ResolveIndent(isLast);
                RenderNode(child, branchPrefix, childContinuationPrefix, width);
            }
        }

        private void RenderNode(TreeNode node, string prefix, string continuationPrefix, int width)
        {
            WriteWrappedNode(prefix, continuationPrefix, node, width);
            if (_options.Expanded && node.Expanded)
            {
                RenderChildren(node.Children, continuationPrefix, width);
            }
        }

        private void WriteWrappedNode(string firstPrefix, string continuationPrefix, TreeNode node, int width)
        {
            string text = node.Text ?? string.Empty;
            ColorScheme textScheme = node.Scheme ?? _options.ResolveTextScheme();
            Decoration textDecoration = node.Decoration ?? _options.TextDecoration;
            RgbColor? textForegroundRgb = node.ForegroundRgb ?? (node.Scheme.HasValue ? null : _options.TextForegroundRgb);
            RgbColor? textBackgroundRgb = node.BackgroundRgb ?? (node.Scheme.HasValue ? null : _options.TextBackgroundRgb);
            List<List<FixedMessageSegment>> sourceLines = MarkupRenderText.GetLogicalLines(
                text,
                textScheme,
                textDecoration,
                textForegroundRgb,
                textBackgroundRgb);
            if (sourceLines.Count == 0)
            {
                WritePrefixedLine(firstPrefix, new List<FixedMessageSegment>(), textScheme, width);
                return;
            }

            bool isFirstRenderedLine = true;
            foreach (List<FixedMessageSegment> sourceLine in sourceLines)
            {
                List<FixedMessageSegment> remaining = sourceLine;
                if (MarkupRenderText.GetLength(remaining) == 0)
                {
                    WritePrefixedLine(
                        isFirstRenderedLine ? firstPrefix : continuationPrefix,
                        new List<FixedMessageSegment>(),
                        textScheme,
                        width);
                    isFirstRenderedLine = false;
                    continue;
                }

                while (MarkupRenderText.GetLength(remaining) > 0)
                {
                    string prefix = isFirstRenderedLine ? firstPrefix : continuationPrefix;
                    int contentWidth = GetContentWidth(prefix, width);
                    List<FixedMessageSegment> segment = MarkupRenderText.TakeWrappedSegments(
                        remaining,
                        contentWidth,
                        out remaining);
                    WritePrefixedLine(prefix, segment, textScheme, width);
                    isFirstRenderedLine = false;
                }
            }
        }

        private void WritePrefixedLine(
            string prefix,
            IList<FixedMessageSegment> text,
            ColorScheme textScheme,
            int width)
        {
            string visiblePrefix = ClipPrefix(prefix, width);
            int available = Math.Max(0, width - visiblePrefix.Length);
            List<FixedMessageSegment> visibleText = MarkupRenderText.TruncateSegments(text, available);

            ColorScheme branchScheme = _options.ResolveBranchScheme();
            if (visiblePrefix.Length > 0)
            {
                TerminalLinkWriter.WriteText(
                    visiblePrefix,
                    branchScheme,
                    null,
                    _options.BranchDecoration,
                    _options.BranchForegroundRgb,
                    _options.BranchBackgroundRgb);
            }

            MarkupRenderText.WriteSegments(visibleText, textScheme, true);
        }

        private static int GetContentWidth(string prefix, int width)
        {
            string visiblePrefix = ClipPrefix(prefix, width);
            return Math.Max(1, width - visiblePrefix.Length);
        }

        private static string ClipPrefix(string prefix, int width)
        {
            prefix = prefix ?? string.Empty;
            int maximumPrefixLength = Math.Max(0, width - 1);
            return prefix.Length > maximumPrefixLength
                ? prefix.Substring(0, maximumPrefixLength)
                : prefix;
        }
    }
}
