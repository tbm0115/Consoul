using System;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;
using TextStyle = ConsoulLibrary.Style;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent text construction over <see cref="TextBlockEntry"/>.
    /// </summary>
    public class Text : TextBlockEntry
    {
        /// <summary>
        /// Gets an empty text renderable.
        /// </summary>
        public static Text Empty => new Text(string.Empty);

        /// <summary>
        /// Gets a text renderable containing a new line.
        /// </summary>
        public static Text NewLine => new Text(Environment.NewLine);

        /// <summary>
        /// Initializes a new instance of the <see cref="Text"/> class.
        /// </summary>
        /// <param name="content">The text content to render.</param>
        public Text(string content) : base(content)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Text"/> class with explicit options.
        /// </summary>
        /// <param name="content">The text content to render.</param>
        /// <param name="options">Optional rendering options for the text block.</param>
        public Text(string content, TextBlockOptions options) : base(content, options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Text"/> class with a foreground color.
        /// </summary>
        /// <param name="content">The text content to render.</param>
        /// <param name="color">The foreground color used when rendering the text.</param>
        public Text(string content, ConsoleColor color) : this(content, color, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Text"/> class with a foreground color.
        /// </summary>
        /// <param name="content">The text content to render.</param>
        /// <param name="color">The foreground color used when rendering the text.</param>
        public Text(string content, SpectreColor color) : base(content, new TextBlockOptions
        {
            Scheme = Scheme(color)
        })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Text"/> class with a foreground color.
        /// </summary>
        /// <param name="content">The text content to render.</param>
        /// <param name="color">The foreground color used when rendering the text.</param>
        public Text(string content, RgbColor color) : base(content, new TextBlockOptions
        {
            Scheme = Scheme(color)
        })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Text"/> class with foreground and background colors.
        /// </summary>
        /// <param name="content">The text content to render.</param>
        /// <param name="color">The foreground color used when rendering the text.</param>
        /// <param name="backgroundColor">The optional background color used when rendering the text.</param>
        public Text(string content, ConsoleColor color, ConsoleColor? backgroundColor) : base(content, new TextBlockOptions
        {
            Scheme = Scheme(color, backgroundColor)
        })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Text"/> class with Spectre-style style text.
        /// </summary>
        /// <param name="content">The text content to render.</param>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        public Text(string content, string style) : base(content, new TextBlockOptions
        {
            Scheme = TextStyle.ParseScheme(style, RenderOptions.DefaultScheme)
        })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Text"/> class with Spectre-style color metadata.
        /// </summary>
        /// <param name="content">The text content to render.</param>
        /// <param name="style">The foreground/background style used when rendering the text.</param>
        public Text(string content, TextStyle style) : base(content, new TextBlockOptions
        {
            Scheme = style?.ToColorScheme(RenderOptions.DefaultScheme)
        })
        {
        }

        /// <summary>
        /// Gets or sets the foreground/background style used when rendering the text.
        /// </summary>
        public TextStyle Style
        {
            get
            {
                if (!Options.Scheme.HasValue)
                {
                    return null;
                }

                ColorScheme scheme = Options.Scheme.Value;
                return new TextStyle(scheme.Color, scheme.BackgroundColor);
            }
            set
            {
                Options.Scheme = value != null
                    ? Scheme(value, Options.ResolveScheme())
                    : (ColorScheme?)null;
            }
        }

        /// <summary>
        /// Replaces the text content rendered by this instance.
        /// </summary>
        /// <param name="content">The text content to render.</param>
        /// <returns>The current <see cref="Text"/> instance.</returns>
        public Text ContentText(string content)
        {
            Content = content ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the requested text block width.
        /// </summary>
        /// <param name="width">The requested width. The actual width is clamped to the active terminal.</param>
        /// <returns>The current <see cref="Text"/> instance.</returns>
        public Text Width(int? width)
        {
            Options.Width = width;
            return this;
        }

        /// <summary>
        /// Expands the text block to the active terminal width.
        /// </summary>
        /// <returns>The current <see cref="Text"/> instance.</returns>
        public Text Expand()
        {
            Options.Width = null;
            return this;
        }

        /// <summary>
        /// Aligns text to the left side of the text block.
        /// </summary>
        /// <returns>The current <see cref="Text"/> instance.</returns>
        public Text LeftJustified()
        {
            Options.Alignment = TextBlockAlignment.Left;
            return this;
        }

        /// <summary>
        /// Centers text inside the text block.
        /// </summary>
        /// <returns>The current <see cref="Text"/> instance.</returns>
        public Text Centered()
        {
            Options.Alignment = TextBlockAlignment.Center;
            return this;
        }

        /// <summary>
        /// Aligns text to the right side of the text block.
        /// </summary>
        /// <returns>The current <see cref="Text"/> instance.</returns>
        public Text RightJustified()
        {
            Options.Alignment = TextBlockAlignment.Right;
            return this;
        }

        /// <summary>
        /// Sets text justification using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="justify">The justification to apply.</param>
        /// <returns>The current <see cref="Text"/> instance.</returns>
        public Text Justify(Justify justify)
        {
            Options.Alignment = JustifyMap.ToTextBlockAlignment(justify);
            return this;
        }

        /// <summary>
        /// Sets how content wider than the available line width should be handled.
        /// </summary>
        /// <param name="overflow">The overflow behavior.</param>
        /// <returns>The current <see cref="Text"/> instance.</returns>
        public Text OverflowMode(Overflow overflow)
        {
            Options.Overflow = overflow;
            return this;
        }

        /// <summary>
        /// Sets horizontal padding on both sides of each rendered line.
        /// </summary>
        /// <param name="padding">The horizontal padding amount.</param>
        /// <returns>The current <see cref="Text"/> instance.</returns>
        public Text Padding(int padding)
        {
            Options.LeftPadding = padding;
            Options.RightPadding = padding;
            return this;
        }

        /// <summary>
        /// Sets all padding values.
        /// </summary>
        /// <param name="left">Spaces written before each line.</param>
        /// <param name="right">Spaces written after each line.</param>
        /// <param name="top">Blank lines written before the content.</param>
        /// <param name="bottom">Blank lines written after the content.</param>
        /// <returns>The current <see cref="Text"/> instance.</returns>
        public Text Padding(int left, int right, int top = 0, int bottom = 0)
        {
            Options.LeftPadding = left;
            Options.RightPadding = right;
            Options.TopPadding = top;
            Options.BottomPadding = bottom;
            return this;
        }

        /// <summary>
        /// Sets the content foreground color.
        /// </summary>
        /// <param name="color">The content foreground color.</param>
        /// <returns>The current <see cref="Text"/> instance.</returns>
        public Text Color(ConsoleColor color)
        {
            Options.Scheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the content foreground color.
        /// </summary>
        /// <param name="color">The content foreground color.</param>
        /// <returns>The current <see cref="Text"/> instance.</returns>
        public Text Color(SpectreColor color)
        {
            Options.Scheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the content foreground color.
        /// </summary>
        /// <param name="color">The content foreground color.</param>
        /// <returns>The current <see cref="Text"/> instance.</returns>
        public Text Color(RgbColor color)
        {
            Options.Scheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the content foreground/background colors from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Text"/> instance.</returns>
        public Text Color(string style)
        {
            Options.Scheme = TextStyle.ParseScheme(style, Options.ResolveScheme());
            return this;
        }

        /// <summary>
        /// Sets the content foreground/background colors from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Text"/> instance.</returns>
        public Text Color(TextStyle style)
        {
            Options.Scheme = Scheme(style, Options.ResolveScheme());
            return this;
        }

        private static ColorScheme Scheme(TextStyle style, ColorScheme fallback)
        {
            return (style ?? TextStyle.Plain).ToColorScheme(fallback);
        }

        private static ColorScheme Scheme(ConsoleColor color)
        {
            return Scheme(color, null);
        }

        private static ColorScheme Scheme(SpectreColor color)
        {
            return Scheme(color.ToConsoleColor());
        }

        private static ColorScheme Scheme(RgbColor color)
        {
            return Scheme(color.ToConsoleColor());
        }

        private static ColorScheme Scheme(ConsoleColor color, ConsoleColor? backgroundColor)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = backgroundColor ?? RenderOptions.DefaultScheme.BackgroundColor
            };
        }
    }
}
