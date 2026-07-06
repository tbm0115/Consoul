using System;
using System.Collections.Generic;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent tree construction over <see cref="TreeEntry"/>.
    /// </summary>
    /// <remarks>
    /// The tree uses the same responsive renderer as <see cref="TreeEntry"/> and can be passed anywhere an
    /// <see cref="IConsoulRenderable"/> is accepted.
    /// </remarks>
    public class Tree : TreeEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Tree"/> class.
        /// </summary>
        /// <param name="rootText">The text rendered for the root node.</param>
        public Tree(string rootText) : this(new TreeNode(rootText))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tree"/> class with captured renderable root content.
        /// </summary>
        /// <param name="root">The renderable content used as the root node label.</param>
        public Tree(IConsoulRenderable root) : this(new TreeNode(root))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tree"/> class with plain object or renderable root content.
        /// </summary>
        /// <param name="root">The content rendered for the root node.</param>
        public Tree(object root) : this(new TreeNode(root))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tree"/> class.
        /// </summary>
        /// <param name="root">The root node to render.</param>
        public Tree(TreeNode root) : base(root)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tree"/> class with explicit options.
        /// </summary>
        /// <param name="root">The root node to render.</param>
        /// <param name="options">Optional rendering options for the tree.</param>
        public Tree(TreeNode root, TreeOptions options) : base(root, options)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether tree nodes should render their children.
        /// </summary>
        public bool Expanded
        {
            get => Options.Expanded;
            set => Options.Expanded = value;
        }

        /// <summary>
        /// Adds a child node to the root node.
        /// </summary>
        /// <param name="text">The text rendered for the child node.</param>
        /// <param name="color">Optional foreground color used for the child node.</param>
        /// <returns>The added child node.</returns>
        public TreeNode AddNode(string text, ConsoleColor? color = null)
        {
            return Root.AddNode(text, color);
        }

        /// <summary>
        /// Adds a child node to the root node using Spectre-style style text.
        /// </summary>
        /// <param name="text">The text rendered for the child node.</param>
        /// <param name="style">Optional style used for the child node, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <returns>The added child node.</returns>
        public TreeNode AddNode(string text, string style)
        {
            return Root.AddNode(text, style);
        }

        /// <summary>
        /// Adds a child node to the root node, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The child node content.</param>
        /// <param name="color">Optional foreground color used for the child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing renderable content.</param>
        /// <returns>The added child node.</returns>
        public TreeNode AddNode(object content, ConsoleColor? color = null, int? captureWidth = null, int captureHeight = 20)
        {
            return Root.AddNode(content, color, captureWidth, captureHeight);
        }

        /// <summary>
        /// Adds a child node to the root node using Spectre-style style text, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The child node content.</param>
        /// <param name="style">Optional style used for the child node, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing renderable content.</param>
        /// <returns>The added child node.</returns>
        public TreeNode AddNode(object content, string style, int? captureWidth = null, int captureHeight = 20)
        {
            return Root.AddNode(content, style, captureWidth, captureHeight);
        }

        /// <summary>
        /// Adds multiple child nodes to the root node.
        /// </summary>
        /// <param name="nodes">The node labels to add.</param>
        /// <returns>The current <see cref="Tree"/> instance.</returns>
        public Tree AddNodes(params string[] nodes)
        {
            Root.AddNodes(nodes);
            return this;
        }

        /// <summary>
        /// Adds multiple child nodes to the root node.
        /// </summary>
        /// <param name="nodes">The node labels to add.</param>
        /// <returns>The current <see cref="Tree"/> instance.</returns>
        public Tree AddNodes(System.Collections.Generic.IEnumerable<string> nodes)
        {
            Root.AddNodes(nodes);
            return this;
        }

        /// <summary>
        /// Adds multiple existing child nodes to the root node.
        /// </summary>
        /// <param name="nodes">The child nodes to add.</param>
        /// <returns>The current <see cref="Tree"/> instance.</returns>
        public Tree AddNodes(params TreeNode[] nodes)
        {
            Root.AddNodes(nodes);
            return this;
        }

        /// <summary>
        /// Adds multiple existing child nodes to the root node.
        /// </summary>
        /// <param name="nodes">The child nodes to add.</param>
        /// <returns>The current <see cref="Tree"/> instance.</returns>
        public Tree AddNodes(System.Collections.Generic.IEnumerable<TreeNode> nodes)
        {
            Root.AddNodes(nodes);
            return this;
        }

        /// <summary>
        /// Adds multiple child nodes to the root node from mixed plain values, existing nodes, and reusable renderable components.
        /// </summary>
        /// <param name="nodes">The node values to add.</param>
        /// <returns>The current <see cref="Tree"/> instance.</returns>
        public Tree AddNodes(params object[] nodes)
        {
            Root.AddNodes(nodes);
            return this;
        }

        /// <summary>
        /// Adds multiple child nodes to the root node from mixed plain values, existing nodes, and reusable renderable components.
        /// </summary>
        /// <param name="nodes">The node values to add.</param>
        /// <returns>The current <see cref="Tree"/> instance.</returns>
        public Tree AddNodes(IEnumerable<object> nodes)
        {
            Root.AddNodes(nodes);
            return this;
        }

        /// <summary>
        /// Adds child nodes to the root node projected from a model collection.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into child nodes.</param>
        /// <param name="contentSelector">Selects the child node content. Values may be plain objects, existing nodes, or reusable renderables.</param>
        /// <param name="colorSelector">Optionally selects the foreground color used for each child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing renderable content.</param>
        /// <returns>The current <see cref="Tree"/> instance.</returns>
        public Tree AddNodes<T>(
            IEnumerable<T> items,
            Func<T, object> contentSelector,
            Func<T, ConsoleColor?> colorSelector = null,
            int? captureWidth = null,
            int captureHeight = 20)
        {
            Root.AddNodes(items, contentSelector, colorSelector, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds child nodes to the root node projected from a model collection with Spectre-style node colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into child nodes.</param>
        /// <param name="contentSelector">Selects the child node content. Values may be plain objects, existing nodes, or reusable renderables.</param>
        /// <param name="colorSelector">Selects the foreground color used for each child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing renderable content.</param>
        /// <returns>The current <see cref="Tree"/> instance.</returns>
        public Tree AddNodes<T>(
            IEnumerable<T> items,
            Func<T, object> contentSelector,
            Func<T, SpectreColor> colorSelector,
            int? captureWidth = null,
            int captureHeight = 20)
        {
            Root.AddNodes(items, contentSelector, colorSelector, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds child nodes to the root node projected from a model collection with RGB node colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into child nodes.</param>
        /// <param name="contentSelector">Selects the child node content. Values may be plain objects, existing nodes, or reusable renderables.</param>
        /// <param name="colorSelector">Selects the foreground color used for each child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing renderable content.</param>
        /// <returns>The current <see cref="Tree"/> instance.</returns>
        public Tree AddNodes<T>(
            IEnumerable<T> items,
            Func<T, object> contentSelector,
            Func<T, RgbColor> colorSelector,
            int? captureWidth = null,
            int captureHeight = 20)
        {
            Root.AddNodes(items, contentSelector, colorSelector, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds child nodes to the root node projected from a model collection using Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into child nodes.</param>
        /// <param name="contentSelector">Selects the child node content. Values may be plain objects, existing nodes, or reusable renderables.</param>
        /// <param name="styleSelector">Optionally selects style text such as <c>green</c>, <c>#22c55e</c>, or <c>white on blue</c> for each child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing renderable content.</param>
        /// <returns>The current <see cref="Tree"/> instance.</returns>
        public Tree AddNodes<T>(
            IEnumerable<T> items,
            Func<T, object> contentSelector,
            Func<T, string> styleSelector,
            int? captureWidth = null,
            int captureHeight = 20)
        {
            Root.AddNodes(items, contentSelector, styleSelector, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Adds a child node with captured renderable content to the root node.
        /// </summary>
        /// <param name="renderable">The renderable content used as the child node label.</param>
        /// <param name="color">Optional foreground color used for the child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable.</param>
        /// <returns>The added child node.</returns>
        public TreeNode AddNode(IConsoulRenderable renderable, ConsoleColor? color = null, int? captureWidth = null, int captureHeight = 20)
        {
            return Root.AddNode(renderable, color, captureWidth, captureHeight);
        }

        /// <summary>
        /// Adds a child node with captured renderable content and Spectre-style style text to the root node.
        /// </summary>
        /// <param name="renderable">The renderable content used as the child node label.</param>
        /// <param name="style">Optional style used for the child node, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable.</param>
        /// <returns>The added child node.</returns>
        public TreeNode AddNode(IConsoulRenderable renderable, string style, int? captureWidth = null, int captureHeight = 20)
        {
            return Root.AddNode(renderable, style, captureWidth, captureHeight);
        }

        /// <summary>
        /// Adds an existing child node to the root node.
        /// </summary>
        /// <param name="child">The child node to add.</param>
        /// <returns>The added child node.</returns>
        public TreeNode AddNode(TreeNode child)
        {
            return Root.AddNode(child);
        }

        /// <summary>
        /// Sets the requested tree width.
        /// </summary>
        /// <param name="width">The requested width. The actual width is clamped to the active terminal.</param>
        /// <returns>The current <see cref="Tree"/> instance.</returns>
        public Tree Width(int? width)
        {
            Options.Width = width;
            return this;
        }

        /// <summary>
        /// Expands the tree to the active terminal width.
        /// </summary>
        /// <returns>The current <see cref="Tree"/> instance.</returns>
        public Tree Expand()
        {
            Options.Width = null;
            return this;
        }

        /// <summary>
        /// Collapses the tree so only the root node is rendered.
        /// </summary>
        /// <returns>The current <see cref="Tree"/> instance.</returns>
        public Tree Collapse()
        {
            Options.Expanded = false;
            return this;
        }

        /// <summary>
        /// Shows or hides the root node.
        /// </summary>
        /// <param name="includeRoot">Whether the root node should render.</param>
        /// <returns>The current <see cref="Tree"/> instance.</returns>
        public Tree ShowRoot(bool includeRoot = true)
        {
            Options.IncludeRoot = includeRoot;
            return this;
        }

        /// <summary>
        /// Hides the root node and renders its children as top-level nodes.
        /// </summary>
        /// <returns>The current <see cref="Tree"/> instance.</returns>
        public Tree HideRoot()
        {
            Options.IncludeRoot = false;
            return this;
        }

        /// <summary>
        /// Sets ASCII branch characters for the tree.
        /// </summary>
        /// <returns>The current <see cref="Tree"/> instance.</returns>
        public Tree AsciiGuide()
        {
            return Guide(TreeGuide.Ascii);
        }

        /// <summary>
        /// Sets rounded Unicode branch characters for the tree.
        /// </summary>
        /// <returns>The current <see cref="Tree"/> instance.</returns>
        public Tree RoundedGuide()
        {
            return Guide(TreeGuide.Line);
        }

        /// <summary>
        /// Sets the tree guide preset.
        /// </summary>
        /// <param name="guide">The guide preset to apply.</param>
        /// <returns>The current <see cref="Tree"/> instance.</returns>
        public Tree Guide(TreeGuide guide)
        {
            Options.Guide = guide;
            return this;
        }

        /// <summary>
        /// Sets the branch and indentation strings used by the tree renderer.
        /// </summary>
        /// <param name="branch">Prefix for a non-final child node.</param>
        /// <param name="lastBranch">Prefix for the final child node.</param>
        /// <param name="indent">Continuation prefix below a non-final child node.</param>
        /// <param name="lastIndent">Continuation prefix below the final child node.</param>
        /// <returns>The current <see cref="Tree"/> instance.</returns>
        public Tree Guide(string branch, string lastBranch, string indent, string lastIndent)
        {
            Options.Branch = branch ?? string.Empty;
            Options.LastBranch = lastBranch ?? string.Empty;
            Options.Indent = indent ?? string.Empty;
            Options.LastIndent = lastIndent ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the default node text foreground color.
        /// </summary>
        /// <param name="color">The default node text foreground color.</param>
        /// <returns>The current <see cref="Tree"/> instance.</returns>
        public Tree TextColor(ConsoleColor color)
        {
            Options.TextScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the default node text color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Tree"/> instance.</returns>
        public Tree TextColor(string style)
        {
            Options.TextScheme = Style.ParseScheme(style, Options.TextScheme ?? RenderOptions.DefaultScheme);
            return this;
        }

        /// <summary>
        /// Sets the default node text color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Tree"/> instance.</returns>
        public Tree TextColor(Style style)
        {
            Options.TextScheme = Scheme(style, Options.TextScheme ?? RenderOptions.DefaultScheme);
            return this;
        }

        /// <summary>
        /// Sets the branch prefix foreground color.
        /// </summary>
        /// <param name="color">The branch prefix foreground color.</param>
        /// <returns>The current <see cref="Tree"/> instance.</returns>
        public Tree BranchColor(ConsoleColor color)
        {
            Options.BranchScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the branch prefix color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Tree"/> instance.</returns>
        public Tree BranchColor(string style)
        {
            Options.BranchScheme = Style.ParseScheme(style, Options.BranchScheme ?? RenderOptions.DefaultScheme);
            return this;
        }

        /// <summary>
        /// Sets the branch prefix color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Tree"/> instance.</returns>
        public Tree BranchColor(Style style)
        {
            Options.BranchScheme = Scheme(style, Options.BranchScheme ?? RenderOptions.DefaultScheme);
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

        private static ColorScheme Scheme(Style style, ColorScheme fallback)
        {
            return (style ?? Style.Plain).ToColorScheme(fallback);
        }
    }
}
