using System;
using System.Linq;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style padding around text or reusable Consoul renderables.
    /// </summary>
    public sealed class Padder : IRenderable
    {
        private readonly string _content;
        private readonly IConsoulRenderable _renderable;
        private int? _width;
        private ColorScheme? _scheme;

        /// <summary>
        /// Initializes a new instance of the <see cref="Padder"/> class with text content.
        /// </summary>
        /// <param name="content">The text content to pad.</param>
        /// <param name="padding">Optional padding. Defaults to one cell on every side.</param>
        public Padder(string content, Padding padding = null)
        {
            _content = content ?? string.Empty;
            Padding = padding != null ? padding.Clone() : new Padding(1);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Padder"/> class with renderable content.
        /// </summary>
        /// <param name="renderable">The renderable component to pad.</param>
        /// <param name="padding">Optional padding. Defaults to one cell on every side.</param>
        public Padder(IConsoulRenderable renderable, Padding padding = null)
        {
            _renderable = renderable ?? throw new ArgumentNullException(nameof(renderable));
            Padding = padding != null ? padding.Clone() : new Padding(1);
        }

        /// <summary>
        /// Gets or sets the padding applied around the content.
        /// </summary>
        public Padding Padding { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the padded block expands to the available width.
        /// </summary>
        public bool Expand { get; set; }

        /// <summary>
        /// Renders the padded content to the active console driver.
        /// </summary>
        public void Render()
        {
            Padding padding = Padding ?? new Padding(0);
            string content = ResolveContent(padding);
            PadOptions options = padding.ToPadOptions();
            options.Width = ResolveWidth(content, padding);
            options.Scheme = _scheme;
            PadEntry.Render(content, options);
        }

        internal Padder SetWidth(int? width)
        {
            if (width.HasValue && width.Value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than zero.");
            }

            _width = width;
            return this;
        }

        internal Padder SetColor(ColorScheme scheme)
        {
            _scheme = scheme;
            return this;
        }

        private string ResolveContent(Padding padding)
        {
            if (_renderable == null)
            {
                return _content;
            }

            int captureWidth = Math.Max(1, (_width ?? Consoul.ConsoleBufferWidth) - padding.Horizontal);
            return RenderActionContent.Capture(_renderable, captureWidth, 200);
        }

        private int? ResolveWidth(string content, Padding padding)
        {
            if (_width.HasValue)
            {
                return _width.Value;
            }

            if (Expand)
            {
                return null;
            }

            int contentWidth = GetMaximumLineLength(content, parseMarkup: _renderable == null);
            return Math.Max(1, Math.Min(Consoul.ConsoleBufferWidth, contentWidth + padding.Horizontal));
        }

        private static int GetMaximumLineLength(string content, bool parseMarkup)
        {
            if (parseMarkup)
            {
                int maximum = 0;
                foreach (var line in MarkupRenderText.GetLogicalLines(content, RenderOptions.DefaultScheme))
                {
                    maximum = Math.Max(maximum, MarkupRenderText.GetLength(line));
                }

                return maximum;
            }

            content = (content ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');
            string[] lines = content.Split('\n');
            return lines.Length == 0 ? 0 : lines.Max(line => line.Length);
        }
    }

    /// <summary>
    /// Provides fluent helpers for <see cref="Padder"/> instances.
    /// </summary>
    public static class PadderExtensions
    {
        /// <summary>
        /// Sets all padding values.
        /// </summary>
        /// <param name="padder">The padder to configure.</param>
        /// <param name="padding">The padding to apply.</param>
        /// <returns>The configured <see cref="Padder"/> instance.</returns>
        public static Padder Padding(this Padder padder, Padding padding)
        {
            if (padder == null)
            {
                throw new ArgumentNullException(nameof(padder));
            }

            padder.Padding = padding != null ? padding.Clone() : new Padding(0);
            return padder;
        }

        /// <summary>
        /// Sets horizontal and vertical padding values.
        /// </summary>
        /// <param name="padder">The padder to configure.</param>
        /// <param name="horizontal">Padding added to the left and right sides.</param>
        /// <param name="vertical">Padding added above and below the content.</param>
        /// <returns>The configured <see cref="Padder"/> instance.</returns>
        public static Padder Padding(this Padder padder, int horizontal, int vertical)
        {
            return Padding(padder, new Padding(horizontal, vertical));
        }

        /// <summary>
        /// Sets all padding values.
        /// </summary>
        /// <param name="padder">The padder to configure.</param>
        /// <param name="left">Padding added to the left side.</param>
        /// <param name="top">Padding added above the content.</param>
        /// <param name="right">Padding added to the right side.</param>
        /// <param name="bottom">Padding added below the content.</param>
        /// <returns>The configured <see cref="Padder"/> instance.</returns>
        public static Padder Padding(this Padder padder, int left, int top, int right, int bottom)
        {
            return Padding(padder, new Padding(left, top, right, bottom));
        }

        /// <summary>
        /// Sets the left padding.
        /// </summary>
        /// <param name="padder">The padder to configure.</param>
        /// <param name="left">Padding added to the left side.</param>
        /// <returns>The configured <see cref="Padder"/> instance.</returns>
        public static Padder PadLeft(this Padder padder, int left)
        {
            return MutatePadding(padder, padding => padding.Left = Math.Max(0, left));
        }

        /// <summary>
        /// Sets the right padding.
        /// </summary>
        /// <param name="padder">The padder to configure.</param>
        /// <param name="right">Padding added to the right side.</param>
        /// <returns>The configured <see cref="Padder"/> instance.</returns>
        public static Padder PadRight(this Padder padder, int right)
        {
            return MutatePadding(padder, padding => padding.Right = Math.Max(0, right));
        }

        /// <summary>
        /// Sets the top padding.
        /// </summary>
        /// <param name="padder">The padder to configure.</param>
        /// <param name="top">Padding added above the content.</param>
        /// <returns>The configured <see cref="Padder"/> instance.</returns>
        public static Padder PadTop(this Padder padder, int top)
        {
            return MutatePadding(padder, padding => padding.Top = Math.Max(0, top));
        }

        /// <summary>
        /// Sets the bottom padding.
        /// </summary>
        /// <param name="padder">The padder to configure.</param>
        /// <param name="bottom">Padding added below the content.</param>
        /// <returns>The configured <see cref="Padder"/> instance.</returns>
        public static Padder PadBottom(this Padder padder, int bottom)
        {
            return MutatePadding(padder, padding => padding.Bottom = Math.Max(0, bottom));
        }

        /// <summary>
        /// Sets the padded block width.
        /// </summary>
        /// <param name="padder">The padder to configure.</param>
        /// <param name="width">The requested width, or <see langword="null"/> to use automatic width.</param>
        /// <returns>The configured <see cref="Padder"/> instance.</returns>
        public static Padder Width(this Padder padder, int? width)
        {
            if (padder == null)
            {
                throw new ArgumentNullException(nameof(padder));
            }

            return padder.SetWidth(width);
        }

        /// <summary>
        /// Sets whether the padded block expands to the active terminal width.
        /// </summary>
        /// <param name="padder">The padder to configure.</param>
        /// <param name="enabled">Whether expansion should be enabled.</param>
        /// <returns>The configured <see cref="Padder"/> instance.</returns>
        public static Padder Expand(this Padder padder, bool enabled = true)
        {
            if (padder == null)
            {
                throw new ArgumentNullException(nameof(padder));
            }

            padder.Expand = enabled;
            return padder;
        }

        /// <summary>
        /// Sets the foreground color used for padded content.
        /// </summary>
        /// <param name="padder">The padder to configure.</param>
        /// <param name="color">The foreground color.</param>
        /// <returns>The configured <see cref="Padder"/> instance.</returns>
        public static Padder Color(this Padder padder, ConsoleColor color)
        {
            if (padder == null)
            {
                throw new ArgumentNullException(nameof(padder));
            }

            return padder.SetColor(new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            });
        }

        /// <summary>
        /// Sets the foreground color used for padded content from a Spectre-style color facade.
        /// </summary>
        /// <param name="padder">The padder to configure.</param>
        /// <param name="color">The foreground color.</param>
        /// <returns>The configured <see cref="Padder"/> instance.</returns>
        public static Padder Color(this Padder padder, SpectreColor color)
        {
            return Color(padder, color.ToConsoleColor());
        }

        /// <summary>
        /// Sets the foreground color used for padded content from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="padder">The padder to configure.</param>
        /// <param name="color">The foreground color.</param>
        /// <returns>The configured <see cref="Padder"/> instance.</returns>
        public static Padder Color(this Padder padder, RgbColor color)
        {
            return Color(padder, color.ToConsoleColor());
        }

        /// <summary>
        /// Sets the color used for padded content from a reusable style object.
        /// </summary>
        /// <param name="padder">The padder to configure.</param>
        /// <param name="style">The style to apply.</param>
        /// <returns>The configured <see cref="Padder"/> instance.</returns>
        public static Padder Color(this Padder padder, Style style)
        {
            if (padder == null)
            {
                throw new ArgumentNullException(nameof(padder));
            }

            return padder.SetColor((style ?? Style.Plain).ToColorScheme(RenderOptions.DefaultScheme));
        }

        /// <summary>
        /// Sets the color used for padded content from Spectre-style style text.
        /// </summary>
        /// <param name="padder">The padder to configure.</param>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The configured <see cref="Padder"/> instance.</returns>
        public static Padder Color(this Padder padder, string style)
        {
            if (padder == null)
            {
                throw new ArgumentNullException(nameof(padder));
            }

            return padder.SetColor(Style.ParseScheme(style, RenderOptions.DefaultScheme));
        }

        private static Padder MutatePadding(Padder padder, Action<Padding> mutate)
        {
            if (padder == null)
            {
                throw new ArgumentNullException(nameof(padder));
            }

            Padding padding = padder.Padding ?? new Padding(0);
            mutate(padding);
            padder.Padding = padding;
            return padder;
        }
    }
}
