using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a node in a tree rendered by <see cref="TreeEntry"/>.
    /// </summary>
    public class TreeNode
    {
        private readonly List<TreeNode> _children = new List<TreeNode>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNode"/> class.
        /// </summary>
        /// <param name="text">The text rendered for the node.</param>
        /// <param name="scheme">Optional color scheme used for this node text.</param>
        public TreeNode(string text, ColorScheme? scheme = null)
        {
            Text = text ?? string.Empty;
            Scheme = scheme;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNode"/> class with plain object or renderable content.
        /// </summary>
        /// <param name="content">The content rendered for the node.</param>
        /// <param name="scheme">Optional color scheme used for this node text.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing renderable content.</param>
        public TreeNode(object content, ColorScheme? scheme = null, int? captureWidth = null, int captureHeight = 20)
        {
            var renderable = content as IConsoulRenderable;
            if (renderable != null)
            {
                ColorScheme? capturedScheme;
                Decoration? capturedDecoration;
                RgbColor? capturedForegroundRgb;
                RgbColor? capturedBackgroundRgb;
                Text = CaptureRenderable(
                    renderable,
                    captureWidth,
                    captureHeight,
                    out capturedScheme,
                    out capturedDecoration,
                    out capturedForegroundRgb,
                    out capturedBackgroundRgb);
                Scheme = scheme ?? capturedScheme;
                Decoration = capturedDecoration;
                ForegroundRgb = scheme.HasValue ? null : capturedForegroundRgb;
                BackgroundRgb = scheme.HasValue ? null : capturedBackgroundRgb;
                return;
            }

            Text = RenderActionContent.FormatValue(content);
            Scheme = scheme;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNode"/> class with captured renderable content.
        /// </summary>
        /// <param name="renderable">The renderable content used as the node label.</param>
        /// <param name="scheme">Optional color scheme used for this node text.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable.</param>
        public TreeNode(IConsoulRenderable renderable, ColorScheme? scheme = null, int? captureWidth = null, int captureHeight = 20)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable));
            }

            ColorScheme? capturedScheme;
            Decoration? capturedDecoration;
            RgbColor? capturedForegroundRgb;
            RgbColor? capturedBackgroundRgb;
            Text = CaptureRenderable(
                renderable,
                captureWidth,
                captureHeight,
                out capturedScheme,
                out capturedDecoration,
                out capturedForegroundRgb,
                out capturedBackgroundRgb);
            Scheme = scheme ?? capturedScheme;
            Decoration = capturedDecoration;
            ForegroundRgb = scheme.HasValue ? null : capturedForegroundRgb;
            BackgroundRgb = scheme.HasValue ? null : capturedBackgroundRgb;
        }

        /// <summary>
        /// Gets or sets the text rendered for the node.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the optional color scheme used for this node text.
        /// </summary>
        public ColorScheme? Scheme { get; set; }

        /// <summary>
        /// Gets or sets the optional ANSI decoration used for this node text.
        /// </summary>
        public Decoration? Decoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for this node text on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for this node text on ANSI-capable terminals.
        /// </summary>
        public RgbColor? BackgroundRgb { get; set; }

        /// <summary>
        /// Gets the child nodes rendered below this node.
        /// </summary>
        public IList<TreeNode> Children => _children;

        /// <summary>
        /// Gets or sets a value indicating whether this node should render its children.
        /// </summary>
        public bool Expanded { get; set; } = true;

        /// <summary>
        /// Adds a child node with the provided text.
        /// </summary>
        /// <param name="text">The text rendered for the child node.</param>
        /// <param name="color">Optional foreground color used for the child node.</param>
        /// <returns>The added child node.</returns>
        public TreeNode Add(string text, ConsoleColor? color = null)
        {
            ColorScheme? scheme = null;
            if (color.HasValue)
            {
                scheme = new ColorScheme
                {
                    Color = color.Value,
                    BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
                };
            }

            return Add(new TreeNode(text, scheme));
        }

        /// <summary>
        /// Adds a child node with a Spectre-style foreground color.
        /// </summary>
        /// <param name="text">The text rendered for the child node.</param>
        /// <param name="color">The foreground color used for the child node.</param>
        /// <returns>The added child node.</returns>
        public TreeNode Add(string text, SpectreColor color)
        {
            TreeNode node = new TreeNode(text, CreateScheme(color));
            node.ForegroundRgb = color.ToRgbColor();
            return Add(node);
        }

        /// <summary>
        /// Adds a child node with an RGB foreground color.
        /// </summary>
        /// <param name="text">The text rendered for the child node.</param>
        /// <param name="color">The RGB foreground color used for the child node.</param>
        /// <returns>The added child node.</returns>
        public TreeNode Add(string text, RgbColor color)
        {
            TreeNode node = new TreeNode(text, CreateScheme(color));
            node.ForegroundRgb = color;
            return Add(node);
        }

        /// <summary>
        /// Adds a child node using Spectre-style style text.
        /// </summary>
        /// <param name="text">The child node text.</param>
        /// <param name="style">Optional style used for the child node, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <returns>The added child node.</returns>
        public TreeNode Add(string text, string style)
        {
            Style parsed = string.IsNullOrWhiteSpace(style) ? null : Style.Parse(style);
            return Add(CreateNode(text, parsed));
        }

        /// <summary>
        /// Adds a child node using a reusable style object.
        /// </summary>
        /// <param name="text">The child node text.</param>
        /// <param name="style">Optional style used for the child node.</param>
        /// <returns>The added child node.</returns>
        public TreeNode Add(string text, Style style)
        {
            return Add(CreateNode(text, style));
        }

        /// <summary>
        /// Adds a child node, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The child node content.</param>
        /// <param name="color">Optional foreground color used for the child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing renderable content.</param>
        /// <returns>The added child node.</returns>
        public TreeNode Add(object content, ConsoleColor? color = null, int? captureWidth = null, int captureHeight = 20)
        {
            var child = content as TreeNode;
            if (child != null)
            {
                return Add(child);
            }

            var renderable = content as IConsoulRenderable;
            if (renderable != null)
            {
                return Add(renderable, color, captureWidth, captureHeight);
            }

            return Add(RenderActionContent.FormatValue(content), color);
        }

        /// <summary>
        /// Adds a child node with a Spectre-style foreground color, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The child node content.</param>
        /// <param name="color">The foreground color used for the child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing renderable content.</param>
        /// <returns>The added child node.</returns>
        public TreeNode Add(object content, SpectreColor color, int? captureWidth = null, int captureHeight = 20)
        {
            var child = content as TreeNode;
            if (child != null)
            {
                return Add(child);
            }

            var renderable = content as IConsoulRenderable;
            if (renderable != null)
            {
                return Add(renderable, color, captureWidth, captureHeight);
            }

            return Add(RenderActionContent.FormatValue(content), color);
        }

        /// <summary>
        /// Adds a child node with an RGB foreground color, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The child node content.</param>
        /// <param name="color">The RGB foreground color used for the child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing renderable content.</param>
        /// <returns>The added child node.</returns>
        public TreeNode Add(object content, RgbColor color, int? captureWidth = null, int captureHeight = 20)
        {
            var child = content as TreeNode;
            if (child != null)
            {
                return Add(child);
            }

            var renderable = content as IConsoulRenderable;
            if (renderable != null)
            {
                return Add(renderable, color, captureWidth, captureHeight);
            }

            return Add(RenderActionContent.FormatValue(content), color);
        }

        /// <summary>
        /// Adds a child node using Spectre-style style text, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The child node content.</param>
        /// <param name="style">Optional style used for the child node, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing renderable content.</param>
        /// <returns>The added child node.</returns>
        public TreeNode Add(object content, string style, int? captureWidth = null, int captureHeight = 20)
        {
            var child = content as TreeNode;
            if (child != null)
            {
                return Add(child);
            }

            var renderable = content as IConsoulRenderable;
            if (renderable != null)
            {
                return Add(renderable, style, captureWidth, captureHeight);
            }

            return Add(RenderActionContent.FormatValue(content), style);
        }

        /// <summary>
        /// Adds a child node using a reusable style object, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The child node content.</param>
        /// <param name="style">Optional style used for the child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing renderable content.</param>
        /// <returns>The added child node.</returns>
        public TreeNode Add(object content, Style style, int? captureWidth = null, int captureHeight = 20)
        {
            var child = content as TreeNode;
            if (child != null)
            {
                return Add(child);
            }

            var renderable = content as IConsoulRenderable;
            if (renderable != null)
            {
                return Add(renderable, style, captureWidth, captureHeight);
            }

            return Add(RenderActionContent.FormatValue(content), style);
        }

        /// <summary>
        /// Adds multiple child nodes with the provided text labels.
        /// </summary>
        /// <param name="nodes">The node labels to add.</param>
        /// <returns>The current <see cref="TreeNode"/> instance.</returns>
        public TreeNode AddNodes(params string[] nodes)
        {
            return AddNodes((IEnumerable<string>)nodes);
        }

        /// <summary>
        /// Adds multiple child nodes with the provided text labels.
        /// </summary>
        /// <param name="nodes">The node labels to add.</param>
        /// <returns>The current <see cref="TreeNode"/> instance.</returns>
        public TreeNode AddNodes(IEnumerable<string> nodes)
        {
            if (nodes == null)
            {
                return this;
            }

            foreach (string node in nodes)
            {
                Add(node);
            }

            return this;
        }

        /// <summary>
        /// Adds multiple existing child nodes.
        /// </summary>
        /// <param name="nodes">The child nodes to add.</param>
        /// <returns>The current <see cref="TreeNode"/> instance.</returns>
        public TreeNode AddNodes(params TreeNode[] nodes)
        {
            return AddNodes((IEnumerable<TreeNode>)nodes);
        }

        /// <summary>
        /// Adds multiple existing child nodes.
        /// </summary>
        /// <param name="nodes">The child nodes to add.</param>
        /// <returns>The current <see cref="TreeNode"/> instance.</returns>
        public TreeNode AddNodes(IEnumerable<TreeNode> nodes)
        {
            if (nodes == null)
            {
                return this;
            }

            foreach (TreeNode node in nodes)
            {
                if (node != null)
                {
                    Add(node);
                }
            }

            return this;
        }

        /// <summary>
        /// Adds multiple child nodes from mixed plain values, existing nodes, and reusable renderable components.
        /// </summary>
        /// <param name="nodes">The node values to add.</param>
        /// <returns>The current <see cref="TreeNode"/> instance.</returns>
        public TreeNode AddNodes(params object[] nodes)
        {
            return AddNodes((IEnumerable<object>)nodes);
        }

        /// <summary>
        /// Adds multiple child nodes from mixed plain values, existing nodes, and reusable renderable components.
        /// </summary>
        /// <param name="nodes">The node values to add.</param>
        /// <returns>The current <see cref="TreeNode"/> instance.</returns>
        public TreeNode AddNodes(IEnumerable<object> nodes)
        {
            if (nodes == null)
            {
                return this;
            }

            foreach (object node in nodes)
            {
                Add(node);
            }

            return this;
        }

        /// <summary>
        /// Adds child nodes projected from a model collection.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into child nodes.</param>
        /// <param name="contentSelector">Selects the child node content. Values may be plain objects, existing nodes, or reusable renderables.</param>
        /// <param name="colorSelector">Optionally selects the foreground color used for each child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing renderable content.</param>
        /// <returns>The current <see cref="TreeNode"/> instance.</returns>
        public TreeNode AddNodes<T>(
            IEnumerable<T> items,
            Func<T, object> contentSelector,
            Func<T, ConsoleColor?> colorSelector = null,
            int? captureWidth = null,
            int captureHeight = 20)
        {
            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                Add(
                    contentSelector(item),
                    colorSelector != null ? colorSelector(item) : null,
                    captureWidth,
                    captureHeight);
            }

            return this;
        }

        /// <summary>
        /// Adds child nodes projected from a model collection with Spectre-style node colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into child nodes.</param>
        /// <param name="contentSelector">Selects the child node content. Values may be plain objects, existing nodes, or reusable renderables.</param>
        /// <param name="colorSelector">Selects the foreground color used for each child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing renderable content.</param>
        /// <returns>The current <see cref="TreeNode"/> instance.</returns>
        public TreeNode AddNodes<T>(
            IEnumerable<T> items,
            Func<T, object> contentSelector,
            Func<T, SpectreColor> colorSelector,
            int? captureWidth = null,
            int captureHeight = 20)
        {
            if (colorSelector == null)
            {
                throw new ArgumentNullException(nameof(colorSelector));
            }

            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                Add(contentSelector(item), colorSelector(item), captureWidth, captureHeight);
            }

            return this;
        }

        /// <summary>
        /// Adds child nodes projected from a model collection with RGB node colors.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into child nodes.</param>
        /// <param name="contentSelector">Selects the child node content. Values may be plain objects, existing nodes, or reusable renderables.</param>
        /// <param name="colorSelector">Selects the foreground color used for each child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing renderable content.</param>
        /// <returns>The current <see cref="TreeNode"/> instance.</returns>
        public TreeNode AddNodes<T>(
            IEnumerable<T> items,
            Func<T, object> contentSelector,
            Func<T, RgbColor> colorSelector,
            int? captureWidth = null,
            int captureHeight = 20)
        {
            if (colorSelector == null)
            {
                throw new ArgumentNullException(nameof(colorSelector));
            }

            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                Add(contentSelector(item), colorSelector(item), captureWidth, captureHeight);
            }

            return this;
        }

        /// <summary>
        /// Adds child nodes projected from a model collection using Spectre-style style text.
        /// </summary>
        /// <typeparam name="T">The model item type.</typeparam>
        /// <param name="items">The model items to project into child nodes.</param>
        /// <param name="contentSelector">Selects the child node content. Values may be plain objects, existing nodes, or reusable renderables.</param>
        /// <param name="styleSelector">Optionally selects style text such as <c>green</c>, <c>#22c55e</c>, or <c>white on blue</c> for each child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing renderable content.</param>
        /// <returns>The current <see cref="TreeNode"/> instance.</returns>
        public TreeNode AddNodes<T>(
            IEnumerable<T> items,
            Func<T, object> contentSelector,
            Func<T, string> styleSelector,
            int? captureWidth = null,
            int captureHeight = 20)
        {
            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }

            if (items == null)
            {
                return this;
            }

            foreach (T item in items)
            {
                Add(
                    contentSelector(item),
                    styleSelector != null ? styleSelector(item) : null,
                    captureWidth,
                    captureHeight);
            }

            return this;
        }

        /// <summary>
        /// Adds a child node with captured renderable content.
        /// </summary>
        /// <param name="renderable">The renderable content used as the child node label.</param>
        /// <param name="color">Optional foreground color used for the child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable.</param>
        /// <returns>The added child node.</returns>
        public TreeNode Add(IConsoulRenderable renderable, ConsoleColor? color = null, int? captureWidth = null, int captureHeight = 20)
        {
            ColorScheme? scheme = null;
            if (color.HasValue)
            {
                scheme = new ColorScheme
                {
                    Color = color.Value,
                    BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
                };
            }

            return Add(new TreeNode(renderable, scheme, captureWidth, captureHeight));
        }

        /// <summary>
        /// Adds a child node with captured renderable content and a Spectre-style foreground color.
        /// </summary>
        /// <param name="renderable">The renderable content used as the child node label.</param>
        /// <param name="color">The foreground color used for the child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable.</param>
        /// <returns>The added child node.</returns>
        public TreeNode Add(IConsoulRenderable renderable, SpectreColor color, int? captureWidth = null, int captureHeight = 20)
        {
            TreeNode node = new TreeNode(renderable, CreateScheme(color), captureWidth, captureHeight);
            node.ForegroundRgb = color.ToRgbColor();
            return Add(node);
        }

        /// <summary>
        /// Adds a child node with captured renderable content and an RGB foreground color.
        /// </summary>
        /// <param name="renderable">The renderable content used as the child node label.</param>
        /// <param name="color">The RGB foreground color used for the child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable.</param>
        /// <returns>The added child node.</returns>
        public TreeNode Add(IConsoulRenderable renderable, RgbColor color, int? captureWidth = null, int captureHeight = 20)
        {
            TreeNode node = new TreeNode(renderable, CreateScheme(color), captureWidth, captureHeight);
            node.ForegroundRgb = color;
            return Add(node);
        }

        /// <summary>
        /// Adds a child node with captured renderable content and Spectre-style style text.
        /// </summary>
        /// <param name="renderable">The renderable content used as the child node label.</param>
        /// <param name="style">Optional style used for the child node, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable.</param>
        /// <returns>The added child node.</returns>
        public TreeNode Add(IConsoulRenderable renderable, string style, int? captureWidth = null, int captureHeight = 20)
        {
            Style parsed = string.IsNullOrWhiteSpace(style) ? null : Style.Parse(style);
            return Add(CreateNode(renderable, parsed, captureWidth, captureHeight));
        }

        /// <summary>
        /// Adds a child node with captured renderable content and a reusable style object.
        /// </summary>
        /// <param name="renderable">The renderable content used as the child node label.</param>
        /// <param name="style">Optional style used for the child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable.</param>
        /// <returns>The added child node.</returns>
        public TreeNode Add(IConsoulRenderable renderable, Style style, int? captureWidth = null, int captureHeight = 20)
        {
            return Add(CreateNode(renderable, style, captureWidth, captureHeight));
        }

        /// <summary>
        /// Adds a child node with the provided text.
        /// </summary>
        /// <param name="text">The text rendered for the child node.</param>
        /// <param name="color">Optional foreground color used for the child node.</param>
        /// <returns>The added child node.</returns>
        public TreeNode AddNode(string text, ConsoleColor? color = null)
        {
            return Add(text, color);
        }

        /// <summary>
        /// Adds a child node with a Spectre-style foreground color.
        /// </summary>
        /// <param name="text">The text rendered for the child node.</param>
        /// <param name="color">The foreground color used for the child node.</param>
        /// <returns>The added child node.</returns>
        public TreeNode AddNode(string text, SpectreColor color)
        {
            return Add(text, color);
        }

        /// <summary>
        /// Adds a child node with an RGB foreground color.
        /// </summary>
        /// <param name="text">The text rendered for the child node.</param>
        /// <param name="color">The RGB foreground color used for the child node.</param>
        /// <returns>The added child node.</returns>
        public TreeNode AddNode(string text, RgbColor color)
        {
            return Add(text, color);
        }

        /// <summary>
        /// Adds a child node using Spectre-style style text.
        /// </summary>
        /// <param name="text">The child node text.</param>
        /// <param name="style">Optional style used for the child node, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <returns>The added child node.</returns>
        public TreeNode AddNode(string text, string style)
        {
            return Add(text, style);
        }

        /// <summary>
        /// Adds a child node using a reusable style object.
        /// </summary>
        /// <param name="text">The child node text.</param>
        /// <param name="style">Optional style used for the child node.</param>
        /// <returns>The added child node.</returns>
        public TreeNode AddNode(string text, Style style)
        {
            return Add(text, style);
        }

        /// <summary>
        /// Adds a child node, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The child node content.</param>
        /// <param name="color">Optional foreground color used for the child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing renderable content.</param>
        /// <returns>The added child node.</returns>
        public TreeNode AddNode(object content, ConsoleColor? color = null, int? captureWidth = null, int captureHeight = 20)
        {
            return Add(content, color, captureWidth, captureHeight);
        }

        /// <summary>
        /// Adds a child node with a Spectre-style foreground color, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The child node content.</param>
        /// <param name="color">The foreground color used for the child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing renderable content.</param>
        /// <returns>The added child node.</returns>
        public TreeNode AddNode(object content, SpectreColor color, int? captureWidth = null, int captureHeight = 20)
        {
            return Add(content, color, captureWidth, captureHeight);
        }

        /// <summary>
        /// Adds a child node with an RGB foreground color, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The child node content.</param>
        /// <param name="color">The RGB foreground color used for the child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing renderable content.</param>
        /// <returns>The added child node.</returns>
        public TreeNode AddNode(object content, RgbColor color, int? captureWidth = null, int captureHeight = 20)
        {
            return Add(content, color, captureWidth, captureHeight);
        }

        /// <summary>
        /// Adds a child node using Spectre-style style text, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The child node content.</param>
        /// <param name="style">Optional style used for the child node, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing renderable content.</param>
        /// <returns>The added child node.</returns>
        public TreeNode AddNode(object content, string style, int? captureWidth = null, int captureHeight = 20)
        {
            return Add(content, style, captureWidth, captureHeight);
        }

        /// <summary>
        /// Adds a child node using a reusable style object, formatting plain object values with the current culture and capturing renderable values.
        /// </summary>
        /// <param name="content">The child node content.</param>
        /// <param name="style">Optional style used for the child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing renderable content.</param>
        /// <returns>The added child node.</returns>
        public TreeNode AddNode(object content, Style style, int? captureWidth = null, int captureHeight = 20)
        {
            return Add(content, style, captureWidth, captureHeight);
        }

        /// <summary>
        /// Adds a child node with captured renderable content.
        /// </summary>
        /// <param name="renderable">The renderable content used as the child node label.</param>
        /// <param name="color">Optional foreground color used for the child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable.</param>
        /// <returns>The added child node.</returns>
        public TreeNode AddNode(IConsoulRenderable renderable, ConsoleColor? color = null, int? captureWidth = null, int captureHeight = 20)
        {
            return Add(renderable, color, captureWidth, captureHeight);
        }

        /// <summary>
        /// Adds a child node with captured renderable content and a Spectre-style foreground color.
        /// </summary>
        /// <param name="renderable">The renderable content used as the child node label.</param>
        /// <param name="color">The foreground color used for the child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable.</param>
        /// <returns>The added child node.</returns>
        public TreeNode AddNode(IConsoulRenderable renderable, SpectreColor color, int? captureWidth = null, int captureHeight = 20)
        {
            return Add(renderable, color, captureWidth, captureHeight);
        }

        /// <summary>
        /// Adds a child node with captured renderable content and an RGB foreground color.
        /// </summary>
        /// <param name="renderable">The renderable content used as the child node label.</param>
        /// <param name="color">The RGB foreground color used for the child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable.</param>
        /// <returns>The added child node.</returns>
        public TreeNode AddNode(IConsoulRenderable renderable, RgbColor color, int? captureWidth = null, int captureHeight = 20)
        {
            return Add(renderable, color, captureWidth, captureHeight);
        }

        /// <summary>
        /// Adds a child node with captured renderable content and Spectre-style style text.
        /// </summary>
        /// <param name="renderable">The renderable content used as the child node label.</param>
        /// <param name="style">Optional style used for the child node, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable.</param>
        /// <returns>The added child node.</returns>
        public TreeNode AddNode(IConsoulRenderable renderable, string style, int? captureWidth = null, int captureHeight = 20)
        {
            return Add(renderable, style, captureWidth, captureHeight);
        }

        /// <summary>
        /// Adds a child node with captured renderable content and a reusable style object.
        /// </summary>
        /// <param name="renderable">The renderable content used as the child node label.</param>
        /// <param name="style">Optional style used for the child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable.</param>
        /// <returns>The added child node.</returns>
        public TreeNode AddNode(IConsoulRenderable renderable, Style style, int? captureWidth = null, int captureHeight = 20)
        {
            return Add(renderable, style, captureWidth, captureHeight);
        }

        /// <summary>
        /// Adds a child node with the provided text and color scheme.
        /// </summary>
        /// <param name="text">The text rendered for the child node.</param>
        /// <param name="scheme">The color scheme used for the child node.</param>
        /// <returns>The added child node.</returns>
        public TreeNode Add(string text, ColorScheme scheme)
        {
            return Add(new TreeNode(text, scheme));
        }

        /// <summary>
        /// Adds a child node with captured renderable content and color scheme.
        /// </summary>
        /// <param name="renderable">The renderable content used as the child node label.</param>
        /// <param name="scheme">The color scheme used for the child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable.</param>
        /// <returns>The added child node.</returns>
        public TreeNode Add(IConsoulRenderable renderable, ColorScheme scheme, int? captureWidth = null, int captureHeight = 20)
        {
            return Add(new TreeNode(renderable, scheme, captureWidth, captureHeight));
        }

        /// <summary>
        /// Adds a child node with the provided text and color scheme.
        /// </summary>
        /// <param name="text">The text rendered for the child node.</param>
        /// <param name="scheme">The color scheme used for the child node.</param>
        /// <returns>The added child node.</returns>
        public TreeNode AddNode(string text, ColorScheme scheme)
        {
            return Add(text, scheme);
        }

        /// <summary>
        /// Adds a child node with captured renderable content and color scheme.
        /// </summary>
        /// <param name="renderable">The renderable content used as the child node label.</param>
        /// <param name="scheme">The color scheme used for the child node.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable.</param>
        /// <returns>The added child node.</returns>
        public TreeNode AddNode(IConsoulRenderable renderable, ColorScheme scheme, int? captureWidth = null, int captureHeight = 20)
        {
            return Add(renderable, scheme, captureWidth, captureHeight);
        }

        /// <summary>
        /// Adds an existing child node.
        /// </summary>
        /// <param name="child">The child node to add.</param>
        /// <returns>The added child node.</returns>
        public TreeNode Add(TreeNode child)
        {
            if (child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            _children.Add(child);
            return child;
        }

        /// <summary>
        /// Adds an existing child node.
        /// </summary>
        /// <param name="child">The child node to add.</param>
        /// <returns>The added child node.</returns>
        public TreeNode AddNode(TreeNode child)
        {
            return Add(child);
        }

        private static TreeNode CreateNode(string text, Style style)
        {
            TreeNode node = new TreeNode(text, style?.ToColorScheme(RenderOptions.DefaultScheme));
            ApplyStyle(node, style);
            return node;
        }

        private static TreeNode CreateNode(IConsoulRenderable renderable, Style style, int? captureWidth, int captureHeight)
        {
            TreeNode node = new TreeNode(
                renderable,
                style?.ToColorScheme(RenderOptions.DefaultScheme),
                captureWidth,
                captureHeight);
            ApplyStyle(node, style);
            return node;
        }

        private static void ApplyStyle(TreeNode node, Style style)
        {
            if (node == null || style == null)
            {
                return;
            }

            node.Decoration = style.Decoration;
            node.ForegroundRgb = style.ForegroundRgb;
            node.BackgroundRgb = style.BackgroundRgb;
        }

        private static ColorScheme CreateScheme(SpectreColor color)
        {
            return new ColorScheme
            {
                Color = color.ToConsoleColor(),
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        private static ColorScheme CreateScheme(RgbColor color)
        {
            return new ColorScheme
            {
                Color = color.ToConsoleColor(),
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        /// <summary>
        /// Hides this node's child nodes.
        /// </summary>
        /// <returns>The current <see cref="TreeNode"/> instance.</returns>
        public TreeNode Collapse()
        {
            Expanded = false;
            return this;
        }

        /// <summary>
        /// Shows this node's child nodes.
        /// </summary>
        /// <returns>The current <see cref="TreeNode"/> instance.</returns>
        public TreeNode Expand()
        {
            Expanded = true;
            return this;
        }

        private static string CaptureRenderable(
            IConsoulRenderable renderable,
            int? captureWidth,
            int captureHeight,
            out ColorScheme? scheme,
            out Decoration? decoration,
            out RgbColor? foregroundRgb,
            out RgbColor? backgroundRgb)
        {
            scheme = null;
            decoration = null;
            foregroundRgb = null;
            backgroundRgb = null;

            var markup = renderable as MarkupEntry;
            if (markup != null)
            {
                return CaptureMarkup(markup, out scheme, out decoration, out foregroundRgb, out backgroundRgb);
            }

            var textBlock = renderable as TextBlockEntry;
            if (textBlock != null && textBlock.Options.Scheme.HasValue)
            {
                scheme = textBlock.Options.Scheme.Value;
                decoration = textBlock.Options.Decoration;
                foregroundRgb = textBlock.Options.ForegroundRgb;
                backgroundRgb = textBlock.Options.BackgroundRgb;
            }

            return RenderActionContent.Capture(
                renderable,
                captureWidth ?? Consoul.TerminalWidth,
                Math.Max(1, captureHeight)).TrimEnd('\r', '\n');
        }

        private static string CaptureMarkup(
            MarkupEntry markup,
            out ColorScheme? scheme,
            out Decoration? decoration,
            out RgbColor? foregroundRgb,
            out RgbColor? backgroundRgb)
        {
            scheme = null;
            decoration = null;
            foregroundRgb = null;
            backgroundRgb = null;
            var builder = new System.Text.StringBuilder();
            foreach (MarkupSegment segment in MarkupEntry.Parse(markup.Markup, RenderOptions.DefaultScheme))
            {
                if (segment.Text.Length == 0)
                {
                    continue;
                }

                if (!scheme.HasValue)
                {
                    scheme = segment.Scheme;
                    decoration = segment.Decoration;
                    foregroundRgb = segment.ForegroundRgb;
                    backgroundRgb = segment.BackgroundRgb;
                }

                builder.Append(segment.Text);
            }

            return builder.ToString();
        }
    }
}
