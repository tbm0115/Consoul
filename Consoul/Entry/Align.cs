using System;
using System.Collections.Generic;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style renderable alignment over <see cref="AlignEntry"/>.
    /// </summary>
    public sealed class Align : IRenderable
    {
        private readonly string _content;
        private readonly IConsoulRenderable _renderable;
        private readonly Action _render;

        /// <summary>
        /// Initializes a new instance of the <see cref="Align"/> class with text content.
        /// </summary>
        /// <param name="content">The text content to align.</param>
        /// <param name="horizontal">The horizontal alignment applied to each content line.</param>
        /// <param name="vertical">The vertical alignment applied when a height is configured.</param>
        public Align(
            string content,
            HorizontalAlignment horizontal = HorizontalAlignment.Left,
            VerticalAlignment vertical = VerticalAlignment.Top)
        {
            _content = content ?? string.Empty;
            Options = CreateOptions(horizontal, vertical);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Align"/> class with text content.
        /// </summary>
        /// <param name="content">The text content to align.</param>
        /// <param name="alignment">The horizontal alignment applied to each content line.</param>
        public Align(string content, TextBlockAlignment alignment)
        {
            _content = content ?? string.Empty;
            Options = new AlignOptions { Alignment = alignment };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Align"/> class with renderable content.
        /// </summary>
        /// <param name="renderable">The renderable content to capture and align.</param>
        /// <param name="horizontal">The horizontal alignment applied to each captured line.</param>
        /// <param name="vertical">The vertical alignment applied when a height is configured.</param>
        public Align(
            IConsoulRenderable renderable,
            HorizontalAlignment horizontal = HorizontalAlignment.Left,
            VerticalAlignment vertical = VerticalAlignment.Top)
        {
            _renderable = renderable ?? throw new ArgumentNullException(nameof(renderable));
            Options = CreateOptions(horizontal, vertical);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Align"/> class with renderable content.
        /// </summary>
        /// <param name="renderable">The renderable content to capture and align.</param>
        /// <param name="alignment">The horizontal alignment applied to each captured line.</param>
        public Align(IConsoulRenderable renderable, TextBlockAlignment alignment)
        {
            _renderable = renderable ?? throw new ArgumentNullException(nameof(renderable));
            Options = new AlignOptions { Alignment = alignment };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Align"/> class with renderable rows.
        /// </summary>
        /// <param name="renderables">The renderable rows to capture and align.</param>
        /// <param name="horizontal">The horizontal alignment applied to each captured line.</param>
        /// <param name="vertical">The vertical alignment applied when a height is configured.</param>
        public Align(
            IEnumerable<IConsoulRenderable> renderables,
            HorizontalAlignment horizontal = HorizontalAlignment.Left,
            VerticalAlignment vertical = VerticalAlignment.Top)
            : this(new Rows(renderables ?? Array.Empty<IConsoulRenderable>()), horizontal, vertical)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Align"/> class with text rows.
        /// </summary>
        /// <param name="rows">The text rows to align.</param>
        /// <param name="horizontal">The horizontal alignment applied to each captured line.</param>
        /// <param name="vertical">The vertical alignment applied when a height is configured.</param>
        public Align(
            IEnumerable<string> rows,
            HorizontalAlignment horizontal = HorizontalAlignment.Left,
            VerticalAlignment vertical = VerticalAlignment.Top)
            : this(new Rows(rows ?? Array.Empty<string>()), horizontal, vertical)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Align"/> class with a render action.
        /// </summary>
        /// <param name="render">The render action to capture and align.</param>
        /// <param name="horizontal">The horizontal alignment applied to each captured line.</param>
        /// <param name="vertical">The vertical alignment applied when a height is configured.</param>
        public Align(
            Action render,
            HorizontalAlignment horizontal = HorizontalAlignment.Left,
            VerticalAlignment vertical = VerticalAlignment.Top)
        {
            _render = render ?? throw new ArgumentNullException(nameof(render));
            Options = CreateOptions(horizontal, vertical);
        }

        /// <summary>
        /// Gets the options used to render the aligned block.
        /// </summary>
        public AlignOptions Options { get; }

        /// <summary>
        /// Renders the aligned content to the active console driver.
        /// </summary>
        public void Render()
        {
            if (_content != null)
            {
                AlignEntry.Render(_content, Options);
                return;
            }

            int captureWidth = Options.Width ?? Consoul.ConsoleBufferWidth;
            if (_render != null)
            {
                AlignEntry.Render(_render, Options, captureWidth);
                return;
            }

            AlignEntry.Render(_renderable, Options, captureWidth);
        }

        /// <summary>
        /// Sets the requested alignment width.
        /// </summary>
        /// <param name="width">The requested width, or <see langword="null"/> to use the active terminal width.</param>
        /// <returns>The current <see cref="Align"/> instance.</returns>
        public Align Width(int? width)
        {
            if (width.HasValue && width.Value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than zero.");
            }

            Options.Width = width;
            return this;
        }

        /// <summary>
        /// Sets the optional alignment height.
        /// </summary>
        /// <param name="height">The requested height, or <see langword="null"/> to use the content height.</param>
        /// <returns>The current <see cref="Align"/> instance.</returns>
        public Align Height(int? height)
        {
            if (height.HasValue && height.Value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Height must be greater than zero.");
            }

            Options.Height = height;
            return this;
        }

        /// <summary>
        /// Aligns content to the left side.
        /// </summary>
        /// <returns>The current <see cref="Align"/> instance.</returns>
        public Align LeftAligned()
        {
            Options.Alignment = TextBlockAlignment.Left;
            return this;
        }

        /// <summary>
        /// Aligns content to the left side using text-builder justification vocabulary.
        /// </summary>
        /// <returns>The current <see cref="Align"/> instance.</returns>
        public Align LeftJustified()
        {
            return LeftAligned();
        }

        /// <summary>
        /// Centers content horizontally.
        /// </summary>
        /// <returns>The current <see cref="Align"/> instance.</returns>
        public Align Centered()
        {
            Options.Alignment = TextBlockAlignment.Center;
            return this;
        }

        /// <summary>
        /// Aligns content to the right side.
        /// </summary>
        /// <returns>The current <see cref="Align"/> instance.</returns>
        public Align RightAligned()
        {
            Options.Alignment = TextBlockAlignment.Right;
            return this;
        }

        /// <summary>
        /// Aligns content to the right side using text-builder justification vocabulary.
        /// </summary>
        /// <returns>The current <see cref="Align"/> instance.</returns>
        public Align RightJustified()
        {
            return RightAligned();
        }

        /// <summary>
        /// Sets horizontal alignment using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="justify">The justification to apply.</param>
        /// <returns>The current <see cref="Align"/> instance.</returns>
        public Align Justify(Justify justify)
        {
            Options.Alignment = JustifyMap.ToTextBlockAlignment(justify);
            return this;
        }

        /// <summary>
        /// Sets horizontal alignment using Spectre-style alignment vocabulary.
        /// </summary>
        /// <param name="alignment">The horizontal alignment to apply.</param>
        /// <returns>The current <see cref="Align"/> instance.</returns>
        public Align Horizontal(HorizontalAlignment alignment)
        {
            Options.Alignment = ToTextBlockAlignment(alignment);
            return this;
        }

        /// <summary>
        /// Sets vertical alignment for bounded-height rendering.
        /// </summary>
        /// <param name="alignment">The vertical alignment to apply.</param>
        /// <returns>The current <see cref="Align"/> instance.</returns>
        public Align Vertical(VerticalAlignment alignment)
        {
            Options.VerticalAlignment = alignment;
            return this;
        }

        /// <summary>
        /// Aligns content to the top of the configured height.
        /// </summary>
        /// <returns>The current <see cref="Align"/> instance.</returns>
        public Align TopAligned()
        {
            Options.VerticalAlignment = VerticalAlignment.Top;
            return this;
        }

        /// <summary>
        /// Centers content vertically inside the configured height.
        /// </summary>
        /// <returns>The current <see cref="Align"/> instance.</returns>
        public Align MiddleAligned()
        {
            Options.VerticalAlignment = VerticalAlignment.Middle;
            return this;
        }

        /// <summary>
        /// Aligns content to the bottom of the configured height.
        /// </summary>
        /// <returns>The current <see cref="Align"/> instance.</returns>
        public Align BottomAligned()
        {
            Options.VerticalAlignment = VerticalAlignment.Bottom;
            return this;
        }

        /// <summary>
        /// Sets the foreground color used for aligned content.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="Align"/> instance.</returns>
        public Align Color(ConsoleColor color)
        {
            Options.Scheme = new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
            return this;
        }

        /// <summary>
        /// Sets the foreground color used for aligned content from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="Align"/> instance.</returns>
        public Align Color(SpectreColor color)
        {
            return Color(color.ToConsoleColor());
        }

        /// <summary>
        /// Sets the foreground color used for aligned content from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="Align"/> instance.</returns>
        public Align Color(RgbColor color)
        {
            return Color(color.ToConsoleColor());
        }

        /// <summary>
        /// Sets the color used for aligned content from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Align"/> instance.</returns>
        public Align Color(Style style)
        {
            Options.Scheme = (style ?? Style.Plain).ToColorScheme(Options.ResolveScheme());
            return this;
        }

        /// <summary>
        /// Sets the color used for aligned content from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Align"/> instance.</returns>
        public Align Color(string style)
        {
            Options.Scheme = Style.ParseScheme(style, Options.ResolveScheme());
            return this;
        }

        private static AlignOptions CreateOptions(HorizontalAlignment horizontal, VerticalAlignment vertical)
        {
            return new AlignOptions
            {
                Alignment = ToTextBlockAlignment(horizontal),
                VerticalAlignment = vertical
            };
        }

        private static TextBlockAlignment ToTextBlockAlignment(HorizontalAlignment alignment)
        {
            switch (alignment)
            {
                case HorizontalAlignment.Center:
                    return TextBlockAlignment.Center;
                case HorizontalAlignment.Right:
                    return TextBlockAlignment.Right;
                default:
                    return TextBlockAlignment.Left;
            }
        }
    }
}
