using ConsoulLibrary.Color;
using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a titled text pane rendered by <see cref="LayoutEntry"/>.
    /// </summary>
    public class LayoutPane
    {
        private string _content;
        private Action _renderContent;
        private int? _captureWidth;
        private int _captureHeight = 200;

        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutPane"/> class.
        /// </summary>
        /// <param name="content">The pane content.</param>
        /// <param name="title">Optional title rendered above the pane content.</param>
        /// <param name="scheme">Optional color scheme used for pane content.</param>
        public LayoutPane(string content, string title = null, ColorScheme? scheme = null)
        {
            _content = content ?? string.Empty;
            Title = title;
            Scheme = scheme;
        }

        /// <summary>
        /// Gets or sets the optional title rendered above the pane content.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the pane content.
        /// </summary>
        public string Content
        {
            get { return _content; }
            set
            {
                _content = value ?? string.Empty;
                _renderContent = null;
                ChildLayout = null;
            }
        }

        /// <summary>
        /// Gets or sets the optional content color scheme for this pane.
        /// </summary>
        public ColorScheme? Scheme { get; set; }

        /// <summary>
        /// Gets or sets a fixed pane size hint. In column layouts this is a width in cells.
        /// </summary>
        public int? SizeValue { get; set; }

        /// <summary>
        /// Gets or sets the relative size used when remaining space is shared between flexible panes.
        /// </summary>
        public int RatioValue { get; set; } = 1;

        /// <summary>
        /// Gets or sets the minimum size needed before this pane is laid out beside another pane.
        /// </summary>
        public int? MinimumSizeValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this pane is included during rendering.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        internal Layout ChildLayout { get; private set; }

        /// <summary>
        /// Replaces the pane content with plain text.
        /// </summary>
        /// <param name="content">The pane content.</param>
        /// <param name="color">Optional foreground color used for pane content.</param>
        /// <returns>The current <see cref="LayoutPane"/> instance.</returns>
        public LayoutPane Update(string content, ConsoleColor? color = null)
        {
            Content = content;
            if (color.HasValue)
            {
                Scheme = CreateScheme(color.Value);
            }

            return this;
        }

        /// <summary>
        /// Replaces the pane content with plain text using Spectre-style style text.
        /// </summary>
        /// <param name="content">The pane content.</param>
        /// <param name="style">Optional style used for pane content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="LayoutPane"/> instance.</returns>
        public LayoutPane Update(string content, string style)
        {
            Content = content;
            if (!string.IsNullOrWhiteSpace(style))
            {
                Scheme = Style.Parse(style).ToColorScheme(RenderOptions.DefaultScheme);
            }

            return this;
        }

        /// <summary>
        /// Replaces the pane content with plain text using a reusable style.
        /// </summary>
        /// <param name="content">The pane content.</param>
        /// <param name="style">Optional style used for pane content.</param>
        /// <returns>The current <see cref="LayoutPane"/> instance.</returns>
        public LayoutPane Update(string content, Style style)
        {
            Content = content;
            if (style != null)
            {
                Scheme = style.ToColorScheme(RenderOptions.DefaultScheme);
            }

            return this;
        }

        /// <summary>
        /// Replaces the pane content with a deferred render action captured each time the layout renders.
        /// </summary>
        /// <param name="render">The render action used to produce pane content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the render action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the render action.</param>
        /// <returns>The current <see cref="LayoutPane"/> instance.</returns>
        public LayoutPane Update(Action render, int? captureWidth = null, int captureHeight = 200)
        {
            if (render == null)
            {
                throw new ArgumentNullException(nameof(render));
            }

            SetDeferredContent(render, null, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Replaces the pane content with a deferred renderable component captured each time the layout renders.
        /// </summary>
        /// <param name="renderable">The renderable component used to produce pane content.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable component.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable component.</param>
        /// <returns>The current <see cref="LayoutPane"/> instance.</returns>
        public LayoutPane Update(IConsoulRenderable renderable, int? captureWidth = null, int captureHeight = 200)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable));
            }

            SetDeferredContent(renderable.Render, renderable as Layout, captureWidth, captureHeight);
            return this;
        }

        /// <summary>
        /// Sets a fixed pane size hint. In column layouts this is a width in cells.
        /// </summary>
        /// <param name="size">The fixed size, or <see langword="null"/> to use flexible sizing.</param>
        /// <returns>The current <see cref="LayoutPane"/> instance.</returns>
        public LayoutPane Size(int? size)
        {
            if (size.HasValue && size.Value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "Size must be greater than zero.");
            }

            SizeValue = size;
            return this;
        }

        /// <summary>
        /// Sets the relative size used when remaining space is shared between flexible panes.
        /// </summary>
        /// <param name="ratio">The relative ratio. Values less than one are treated as one.</param>
        /// <returns>The current <see cref="LayoutPane"/> instance.</returns>
        public LayoutPane Ratio(int ratio)
        {
            RatioValue = Math.Max(1, ratio);
            return this;
        }

        /// <summary>
        /// Sets the minimum size needed before this pane is laid out beside another pane.
        /// </summary>
        /// <param name="size">The minimum size, or <see langword="null"/> to use the layout default.</param>
        /// <returns>The current <see cref="LayoutPane"/> instance.</returns>
        public LayoutPane MinimumSize(int? size)
        {
            if (size.HasValue && size.Value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "Minimum size must be greater than zero.");
            }

            MinimumSizeValue = size;
            return this;
        }

        /// <summary>
        /// Includes or excludes this pane during rendering.
        /// </summary>
        /// <param name="visible">Whether the pane should be rendered.</param>
        /// <returns>The current <see cref="LayoutPane"/> instance.</returns>
        public LayoutPane Visible(bool visible = true)
        {
            IsVisible = visible;
            return this;
        }

        /// <summary>
        /// Excludes this pane during rendering.
        /// </summary>
        /// <returns>The current <see cref="LayoutPane"/> instance.</returns>
        public LayoutPane Invisible()
        {
            return Visible(false);
        }

        /// <summary>
        /// Creates a pane with an optional foreground color.
        /// </summary>
        /// <param name="content">The pane content.</param>
        /// <param name="title">Optional title rendered above the pane content.</param>
        /// <param name="color">Optional foreground color used for pane content.</param>
        /// <returns>A configured <see cref="LayoutPane"/>.</returns>
        public static LayoutPane Create(string content, string title = null, ConsoleColor? color = null)
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

            return new LayoutPane(content, title, scheme);
        }

        /// <summary>
        /// Creates a pane with Spectre-style style text.
        /// </summary>
        /// <param name="content">The pane content.</param>
        /// <param name="title">Optional title rendered above the pane content.</param>
        /// <param name="style">Optional style used for pane content, such as <c>green</c> or <c>white on blue</c>.</param>
        /// <returns>A configured <see cref="LayoutPane"/>.</returns>
        public static LayoutPane Create(string content, string title, string style)
        {
            ColorScheme? scheme = string.IsNullOrWhiteSpace(style)
                ? (ColorScheme?)null
                : Style.Parse(style).ToColorScheme(RenderOptions.DefaultScheme);

            return new LayoutPane(content, title, scheme);
        }

        /// <summary>
        /// Creates a pane with a reusable style.
        /// </summary>
        /// <param name="content">The pane content.</param>
        /// <param name="title">Optional title rendered above the pane content.</param>
        /// <param name="style">Optional style used for pane content.</param>
        /// <returns>A configured <see cref="LayoutPane"/>.</returns>
        public static LayoutPane Create(string content, string title, Style style)
        {
            ColorScheme? scheme = style == null
                ? (ColorScheme?)null
                : style.ToColorScheme(RenderOptions.DefaultScheme);

            return new LayoutPane(content, title, scheme);
        }

        internal static LayoutPane CreateLive(IConsoulRenderable renderable, string title = null, ConsoleColor? color = null, int? captureWidth = null, int captureHeight = 200)
        {
            ColorScheme? scheme = null;
            if (color.HasValue)
            {
                scheme = CreateScheme(color.Value);
            }

            return CreateLive(renderable, title, scheme, captureWidth, captureHeight);
        }

        internal static LayoutPane CreateLive(IConsoulRenderable renderable, string title, ColorScheme? scheme, int? captureWidth = null, int captureHeight = 200)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable));
            }

            var pane = new LayoutPane(string.Empty, title, scheme);
            pane.SetDeferredContent(renderable.Render, renderable as Layout, captureWidth, captureHeight);
            return pane;
        }

        internal string ResolveContent(int width)
        {
            if (_renderContent == null)
            {
                return Content;
            }

            return RenderActionContent.Capture(_renderContent, _captureWidth ?? width, _captureHeight);
        }

        private void SetDeferredContent(Action render, Layout childLayout, int? captureWidth, int captureHeight)
        {
            _content = string.Empty;
            _renderContent = render;
            _captureWidth = captureWidth;
            _captureHeight = Math.Max(1, captureHeight);
            ChildLayout = childLayout;
        }

        private static ColorScheme CreateScheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }
    }
}
