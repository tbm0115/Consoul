using ConsoulLibrary.Color;
using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for a bordered panel.
    /// </summary>
    public class PanelOptions
    {
        /// <summary>
        /// Gets or sets the optional title rendered in the top border.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the optional footer rendered in the bottom border.
        /// </summary>
        public string Footer { get; set; }

        /// <summary>
        /// Gets or sets the requested panel width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the panel should expand to the active terminal width when no explicit <see cref="Width"/> is set.
        /// </summary>
        public bool Expand { get; set; } = true;

        /// <summary>
        /// Gets or sets the horizontal content padding inside the panel.
        /// </summary>
        public int Padding { get; set; } = 1;

        /// <summary>
        /// Gets or sets explicit four-side content padding inside the panel.
        /// </summary>
        /// <remarks>
        /// When set, this value takes precedence over the legacy horizontal <see cref="Padding"/> value.
        /// </remarks>
        public Padding ContentPadding { get; set; }

        /// <summary>
        /// Gets or sets the named border preset used for this panel.
        /// </summary>
        public PanelBorderStyle BorderStyle { get; set; } = PanelBorderStyle.Custom;

        /// <summary>
        /// Gets or sets the alignment used for the title inside the top border.
        /// </summary>
        public TextBlockAlignment TitleAlignment { get; set; } = TextBlockAlignment.Left;

        /// <summary>
        /// Gets or sets the alignment used for the footer inside the bottom border.
        /// </summary>
        public TextBlockAlignment FooterAlignment { get; set; } = TextBlockAlignment.Left;

        /// <summary>
        /// Gets or sets the top-left corner character.
        /// </summary>
        public char TopLeft { get; set; } = '+';

        /// <summary>
        /// Gets or sets the top-right corner character.
        /// </summary>
        public char TopRight { get; set; } = '+';

        /// <summary>
        /// Gets or sets the bottom-left corner character.
        /// </summary>
        public char BottomLeft { get; set; } = '+';

        /// <summary>
        /// Gets or sets the bottom-right corner character.
        /// </summary>
        public char BottomRight { get; set; } = '+';

        /// <summary>
        /// Gets or sets the horizontal border character.
        /// </summary>
        public char Horizontal { get; set; } = '-';

        /// <summary>
        /// Gets or sets the vertical border character.
        /// </summary>
        public char Vertical { get; set; } = '|';

        /// <summary>
        /// Gets or sets the color scheme used for the border.
        /// </summary>
        public ColorScheme? BorderScheme { get; set; }

        /// <summary>
        /// Gets or sets the text decoration applied to border characters on ANSI-capable terminals.
        /// </summary>
        public Decoration BorderDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional 24-bit border foreground color used on true-color terminals.
        /// </summary>
        public RgbColor? BorderForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional 24-bit border background color used on true-color terminals.
        /// </summary>
        public RgbColor? BorderBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for content text.
        /// </summary>
        public ColorScheme? ContentScheme { get; set; }

        /// <summary>
        /// Gets or sets the text decoration applied to untagged content on ANSI-capable terminals.
        /// </summary>
        public Decoration ContentDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional 24-bit content foreground color used for untagged content on true-color terminals.
        /// </summary>
        public RgbColor? ContentForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional 24-bit content background color used for untagged content on true-color terminals.
        /// </summary>
        public RgbColor? ContentBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for the title.
        /// </summary>
        public ColorScheme? TitleScheme { get; set; }

        /// <summary>
        /// Gets or sets the text decoration applied to the title on ANSI-capable terminals.
        /// </summary>
        public Decoration TitleDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional 24-bit title foreground color used on true-color terminals.
        /// </summary>
        public RgbColor? TitleForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional 24-bit title background color used on true-color terminals.
        /// </summary>
        public RgbColor? TitleBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for the footer.
        /// </summary>
        public ColorScheme? FooterScheme { get; set; }

        /// <summary>
        /// Gets or sets the text decoration applied to the footer on ANSI-capable terminals.
        /// </summary>
        public Decoration FooterDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional 24-bit footer foreground color used on true-color terminals.
        /// </summary>
        public RgbColor? FooterForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional 24-bit footer background color used on true-color terminals.
        /// </summary>
        public RgbColor? FooterBackgroundRgb { get; set; }

        /// <summary>
        /// Creates a copy of the current panel options.
        /// </summary>
        /// <returns>A new <see cref="PanelOptions"/> instance with the same values.</returns>
        public PanelOptions Clone()
        {
            return new PanelOptions
            {
                Title = Title,
                Footer = Footer,
                Width = Width,
                Expand = Expand,
                Padding = Padding,
                ContentPadding = ContentPadding != null ? ContentPadding.Clone() : null,
                BorderStyle = BorderStyle,
                TitleAlignment = TitleAlignment,
                FooterAlignment = FooterAlignment,
                TopLeft = TopLeft,
                TopRight = TopRight,
                BottomLeft = BottomLeft,
                BottomRight = BottomRight,
                Horizontal = Horizontal,
                Vertical = Vertical,
                BorderScheme = BorderScheme,
                BorderDecoration = BorderDecoration,
                BorderForegroundRgb = BorderForegroundRgb,
                BorderBackgroundRgb = BorderBackgroundRgb,
                ContentScheme = ContentScheme,
                ContentDecoration = ContentDecoration,
                ContentForegroundRgb = ContentForegroundRgb,
                ContentBackgroundRgb = ContentBackgroundRgb,
                TitleScheme = TitleScheme,
                TitleDecoration = TitleDecoration,
                TitleForegroundRgb = TitleForegroundRgb,
                TitleBackgroundRgb = TitleBackgroundRgb,
                FooterScheme = FooterScheme,
                FooterDecoration = FooterDecoration,
                FooterForegroundRgb = FooterForegroundRgb,
                FooterBackgroundRgb = FooterBackgroundRgb
            };
        }

        internal Padding ResolveContentPadding()
        {
            if (ContentPadding != null)
            {
                return ContentPadding.Clone();
            }

            int horizontalPadding = Math.Max(0, Padding);
            return new Padding(horizontalPadding, 0, horizontalPadding, 0);
        }

        internal ColorScheme ResolveBorderScheme()
        {
            return BorderScheme ?? new ColorScheme
            {
                Color = RenderOptions.SubnoteColor,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        internal ColorScheme ResolveContentScheme()
        {
            return ContentScheme ?? RenderOptions.DefaultScheme;
        }

        internal ColorScheme ResolveTitleScheme()
        {
            return TitleScheme ?? ResolveBorderScheme();
        }

        internal ColorScheme ResolveFooterScheme()
        {
            return FooterScheme ?? ResolveBorderScheme();
        }

        internal PanelBorderCharacters GetBorderCharacters()
        {
            switch (BorderStyle)
            {
                case PanelBorderStyle.Square:
                    return new PanelBorderCharacters('┌', '┐', '└', '┘', '─', '│');
                case PanelBorderStyle.Rounded:
                    return new PanelBorderCharacters('╭', '╮', '╰', '╯', '─', '│');
                case PanelBorderStyle.Ascii:
                    return new PanelBorderCharacters('+', '+', '+', '+', '-', '|');
                case PanelBorderStyle.Heavy:
                    return new PanelBorderCharacters('┏', '┓', '┗', '┛', '━', '┃');
                case PanelBorderStyle.Double:
                    return new PanelBorderCharacters('╔', '╗', '╚', '╝', '═', '║');
                case PanelBorderStyle.None:
                    return new PanelBorderCharacters(' ', ' ', ' ', ' ', ' ', ' ');
                default:
                    return new PanelBorderCharacters(TopLeft, TopRight, BottomLeft, BottomRight, Horizontal, Vertical);
            }
        }
    }

    internal struct PanelBorderCharacters
    {
        public PanelBorderCharacters(char topLeft, char topRight, char bottomLeft, char bottomRight, char horizontal, char vertical)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
            Horizontal = horizontal;
            Vertical = vertical;
        }

        public char TopLeft { get; }

        public char TopRight { get; }

        public char BottomLeft { get; }

        public char BottomRight { get; }

        public char Horizontal { get; }

        public char Vertical { get; }
    }
}
