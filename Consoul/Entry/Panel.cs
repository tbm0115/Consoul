using System;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent panel construction over <see cref="PanelEntry"/>.
    /// </summary>
    /// <remarks>
    /// The panel uses the same responsive renderer as <see cref="PanelEntry"/> and can be passed anywhere an
    /// <see cref="IConsoulRenderable"/> is accepted.
    /// </remarks>
    public class Panel : PanelEntry
    {
        private readonly Action _renderContent;
        private readonly int? _captureWidth;
        private readonly int _captureHeight;

        /// <summary>
        /// Initializes a new instance of the <see cref="Panel"/> class.
        /// </summary>
        /// <param name="content">The panel content to render.</param>
        public Panel(string content) : base(content)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Panel"/> class with explicit options.
        /// </summary>
        /// <param name="content">The panel content to render.</param>
        /// <param name="options">Optional rendering options for the panel.</param>
        public Panel(string content, PanelOptions options) : base(content, options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Panel"/> class with captured renderable content.
        /// </summary>
        /// <param name="renderable">The renderable content to capture and render inside the panel.</param>
        public Panel(IConsoulRenderable renderable) : this(renderable, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Panel"/> class with captured renderable content.
        /// </summary>
        /// <param name="renderable">The renderable content to capture and render inside the panel.</param>
        /// <param name="options">Optional rendering options for the panel.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the renderable content.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the renderable content.</param>
        public Panel(IConsoulRenderable renderable, PanelOptions options, int? captureWidth = null, int captureHeight = 200)
            : this(ToRenderAction(renderable), options, captureWidth, captureHeight)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Panel"/> class with captured action content.
        /// </summary>
        /// <param name="render">The action to capture and render inside the panel.</param>
        public Panel(Action render) : this(render, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Panel"/> class with captured action content.
        /// </summary>
        /// <param name="render">The action to capture and render inside the panel.</param>
        /// <param name="options">Optional rendering options for the panel.</param>
        /// <param name="captureWidth">Optional virtual console width used while capturing the action.</param>
        /// <param name="captureHeight">Virtual console height used while capturing the action.</param>
        public Panel(Action render, PanelOptions options, int? captureWidth = null, int captureHeight = 200)
            : base(string.Empty, options)
        {
            _renderContent = render ?? throw new ArgumentNullException(nameof(render));
            _captureWidth = captureWidth;
            _captureHeight = Math.Max(1, captureHeight);
        }

        /// <summary>
        /// Captures deferred content when needed and renders the panel to the active console driver.
        /// </summary>
        public override void Render()
        {
            if (_renderContent != null)
            {
                Content = RenderActionContent.Capture(_renderContent, _captureWidth ?? ResolveCaptureWidth(Options), _captureHeight);
            }

            base.Render();
        }

        /// <summary>
        /// Sets the panel title/header rendered in the top border.
        /// </summary>
        /// <param name="header">The title/header text.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Header(string header)
        {
            return ApplyHeader(header, null, null);
        }

        /// <summary>
        /// Sets the panel title/header and alignment rendered in the top border.
        /// </summary>
        /// <param name="header">The title/header text.</param>
        /// <param name="alignment">The title/header justification.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Header(string header, Justify alignment)
        {
            return ApplyHeader(header, alignment, null);
        }

        /// <summary>
        /// Sets the panel title/header and foreground color rendered in the top border.
        /// </summary>
        /// <param name="header">The title/header text.</param>
        /// <param name="color">The title/header foreground color.</param>
        /// <param name="alignment">Optional title/header justification.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Header(string header, ConsoleColor color, Justify? alignment = null)
        {
            return ApplyHeader(header, alignment, new Style(color));
        }

        /// <summary>
        /// Sets the panel title/header and foreground color rendered in the top border from a Spectre-style color facade.
        /// </summary>
        /// <param name="header">The title/header text.</param>
        /// <param name="color">The title/header foreground color.</param>
        /// <param name="alignment">Optional title/header justification.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Header(string header, SpectreColor color, Justify? alignment = null)
        {
            return ApplyHeader(header, alignment, new Style(color));
        }

        /// <summary>
        /// Sets the panel title/header and foreground color rendered in the top border from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="header">The title/header text.</param>
        /// <param name="color">The title/header foreground color.</param>
        /// <param name="alignment">Optional title/header justification.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Header(string header, RgbColor color, Justify? alignment = null)
        {
            return ApplyHeader(header, alignment, new Style(color));
        }

        /// <summary>
        /// Sets the panel title/header and style rendered in the top border.
        /// </summary>
        /// <param name="header">The title/header text.</param>
        /// <param name="style">The title/header style.</param>
        /// <param name="alignment">Optional title/header justification.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Header(string header, Style style, Justify? alignment = null)
        {
            return ApplyHeader(header, alignment, style);
        }

        /// <summary>
        /// Sets the panel title/header and style rendered in the top border from Spectre-style style text.
        /// </summary>
        /// <param name="header">The title/header text.</param>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <param name="alignment">Optional title/header justification.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Header(string header, string style, Justify? alignment = null)
        {
            return ApplyHeader(header, alignment, Style.Parse(style));
        }

        /// <summary>
        /// Sets the panel title/header and alignment rendered in the top border.
        /// </summary>
        /// <param name="header">The title/header text.</param>
        /// <param name="alignment">The title/header alignment.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Header(string header, TextBlockAlignment alignment)
        {
            Options.Title = header;
            Options.TitleAlignment = alignment;
            return this;
        }

        /// <summary>
        /// Sets the panel title/header from a Spectre-style header object.
        /// </summary>
        /// <param name="header">The header configuration.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Header(PanelHeader header)
        {
            if (header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }

            return ApplyHeader(header.Text, header.Justification, null);
        }

        /// <summary>
        /// Sets the panel title/header from captured renderable content.
        /// </summary>
        /// <param name="header">The renderable component used to produce the title/header text.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Header(IConsoulRenderable header)
        {
            return ApplyHeader(FormatHeaderFooter(header), null, null);
        }

        /// <summary>
        /// Sets the panel title/header from captured renderable content and alignment.
        /// </summary>
        /// <param name="header">The renderable component used to produce the title/header text.</param>
        /// <param name="alignment">The title/header justification.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Header(IConsoulRenderable header, Justify alignment)
        {
            return ApplyHeader(FormatHeaderFooter(header), alignment, null);
        }

        /// <summary>
        /// Sets the panel title/header from captured renderable content and alignment.
        /// </summary>
        /// <param name="header">The renderable component used to produce the title/header text.</param>
        /// <param name="alignment">The title/header alignment.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Header(IConsoulRenderable header, TextBlockAlignment alignment)
        {
            Options.Title = FormatHeaderFooter(header);
            Options.TitleAlignment = alignment;
            return this;
        }

        /// <summary>
        /// Sets the panel title rendered in the top border.
        /// </summary>
        /// <param name="title">The title text.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Title(string title)
        {
            return Header(title);
        }

        /// <summary>
        /// Sets the panel title and alignment rendered in the top border.
        /// </summary>
        /// <param name="title">The title text.</param>
        /// <param name="alignment">The title justification.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Title(string title, Justify alignment)
        {
            return Header(title, alignment);
        }

        /// <summary>
        /// Sets the panel title and alignment rendered in the top border.
        /// </summary>
        /// <param name="title">The title text.</param>
        /// <param name="alignment">The title alignment.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Title(string title, TextBlockAlignment alignment)
        {
            return Header(title, alignment);
        }

        /// <summary>
        /// Sets the panel title and foreground color rendered in the top border.
        /// </summary>
        /// <param name="title">The title text.</param>
        /// <param name="color">The title foreground color.</param>
        /// <param name="alignment">Optional title justification.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Title(string title, ConsoleColor color, Justify? alignment = null)
        {
            return Header(title, color, alignment);
        }

        /// <summary>
        /// Sets the panel title and foreground color rendered in the top border from a Spectre-style color facade.
        /// </summary>
        /// <param name="title">The title text.</param>
        /// <param name="color">The title foreground color.</param>
        /// <param name="alignment">Optional title justification.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Title(string title, SpectreColor color, Justify? alignment = null)
        {
            return Header(title, color, alignment);
        }

        /// <summary>
        /// Sets the panel title and foreground color rendered in the top border from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="title">The title text.</param>
        /// <param name="color">The title foreground color.</param>
        /// <param name="alignment">Optional title justification.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Title(string title, RgbColor color, Justify? alignment = null)
        {
            return Header(title, color, alignment);
        }

        /// <summary>
        /// Sets the panel title and style rendered in the top border from Spectre-style style text.
        /// </summary>
        /// <param name="title">The title text.</param>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <param name="alignment">Optional title justification.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Title(string title, string style, Justify? alignment = null)
        {
            return Header(title, style, alignment);
        }

        /// <summary>
        /// Sets the panel title and style rendered in the top border.
        /// </summary>
        /// <param name="title">The title text.</param>
        /// <param name="style">The title style.</param>
        /// <param name="alignment">Optional title justification.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Title(string title, Style style, Justify? alignment = null)
        {
            return Header(title, style, alignment);
        }

        /// <summary>
        /// Sets the panel title from captured renderable content.
        /// </summary>
        /// <param name="title">The renderable component used to produce the title text.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Title(IConsoulRenderable title)
        {
            return Header(title);
        }

        /// <summary>
        /// Sets the panel title from captured renderable content and alignment.
        /// </summary>
        /// <param name="title">The renderable component used to produce the title text.</param>
        /// <param name="alignment">The title justification.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Title(IConsoulRenderable title, Justify alignment)
        {
            return Header(title, alignment);
        }

        /// <summary>
        /// Sets the panel title from captured renderable content and alignment.
        /// </summary>
        /// <param name="title">The renderable component used to produce the title text.</param>
        /// <param name="alignment">The title alignment.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Title(IConsoulRenderable title, TextBlockAlignment alignment)
        {
            return Header(title, alignment);
        }

        /// <summary>
        /// Sets the panel footer rendered in the bottom border.
        /// </summary>
        /// <param name="footer">The footer text.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Footer(string footer)
        {
            return ApplyFooter(footer, null, null);
        }

        /// <summary>
        /// Sets the panel footer and alignment rendered in the bottom border.
        /// </summary>
        /// <param name="footer">The footer text.</param>
        /// <param name="alignment">The footer justification.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Footer(string footer, Justify alignment)
        {
            return ApplyFooter(footer, alignment, null);
        }

        /// <summary>
        /// Sets the panel footer and alignment rendered in the bottom border.
        /// </summary>
        /// <param name="footer">The footer text.</param>
        /// <param name="alignment">The footer alignment.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Footer(string footer, TextBlockAlignment alignment)
        {
            Options.Footer = footer;
            Options.FooterAlignment = alignment;
            return this;
        }

        /// <summary>
        /// Sets the panel footer and foreground color rendered in the bottom border.
        /// </summary>
        /// <param name="footer">The footer text.</param>
        /// <param name="color">The footer foreground color.</param>
        /// <param name="alignment">Optional footer justification.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Footer(string footer, ConsoleColor color, Justify? alignment = null)
        {
            return ApplyFooter(footer, alignment, new Style(color));
        }

        /// <summary>
        /// Sets the panel footer and foreground color rendered in the bottom border from a Spectre-style color facade.
        /// </summary>
        /// <param name="footer">The footer text.</param>
        /// <param name="color">The footer foreground color.</param>
        /// <param name="alignment">Optional footer justification.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Footer(string footer, SpectreColor color, Justify? alignment = null)
        {
            return ApplyFooter(footer, alignment, new Style(color));
        }

        /// <summary>
        /// Sets the panel footer and foreground color rendered in the bottom border from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="footer">The footer text.</param>
        /// <param name="color">The footer foreground color.</param>
        /// <param name="alignment">Optional footer justification.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Footer(string footer, RgbColor color, Justify? alignment = null)
        {
            return ApplyFooter(footer, alignment, new Style(color));
        }

        /// <summary>
        /// Sets the panel footer and style rendered in the bottom border.
        /// </summary>
        /// <param name="footer">The footer text.</param>
        /// <param name="style">The footer style.</param>
        /// <param name="alignment">Optional footer justification.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Footer(string footer, Style style, Justify? alignment = null)
        {
            return ApplyFooter(footer, alignment, style);
        }

        /// <summary>
        /// Sets the panel footer and style rendered in the bottom border from Spectre-style style text.
        /// </summary>
        /// <param name="footer">The footer text.</param>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <param name="alignment">Optional footer justification.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Footer(string footer, string style, Justify? alignment = null)
        {
            return ApplyFooter(footer, alignment, Style.Parse(style));
        }

        /// <summary>
        /// Sets the panel footer from reusable header/footer metadata.
        /// </summary>
        /// <param name="footer">The footer metadata.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Footer(PanelHeader footer)
        {
            if (footer == null)
            {
                throw new ArgumentNullException(nameof(footer));
            }

            return ApplyFooter(footer.Text, footer.Justification, null);
        }

        /// <summary>
        /// Sets the panel footer from captured renderable content.
        /// </summary>
        /// <param name="footer">The renderable component used to produce the footer text.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Footer(IConsoulRenderable footer)
        {
            return ApplyFooter(FormatHeaderFooter(footer), null, null);
        }

        /// <summary>
        /// Sets the panel footer from captured renderable content and alignment.
        /// </summary>
        /// <param name="footer">The renderable component used to produce the footer text.</param>
        /// <param name="alignment">The footer justification.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Footer(IConsoulRenderable footer, Justify alignment)
        {
            return ApplyFooter(FormatHeaderFooter(footer), alignment, null);
        }

        /// <summary>
        /// Sets the panel footer from captured renderable content and alignment.
        /// </summary>
        /// <param name="footer">The renderable component used to produce the footer text.</param>
        /// <param name="alignment">The footer alignment.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Footer(IConsoulRenderable footer, TextBlockAlignment alignment)
        {
            Options.Footer = FormatHeaderFooter(footer);
            Options.FooterAlignment = alignment;
            return this;
        }

        /// <summary>
        /// Sets the requested panel width.
        /// </summary>
        /// <param name="width">The requested width. The actual width is clamped to the active terminal.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Width(int? width)
        {
            Options.Width = width;
            return this;
        }

        /// <summary>
        /// Expands the panel to the active terminal width.
        /// </summary>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Expand()
        {
            Options.Width = null;
            Options.Expand = true;
            return this;
        }

        /// <summary>
        /// Collapses the panel width to its content when no explicit width is set.
        /// </summary>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Collapse()
        {
            Options.Width = null;
            Options.Expand = false;
            return this;
        }

        /// <summary>
        /// Sets horizontal content padding inside the panel.
        /// </summary>
        /// <param name="padding">The horizontal padding amount.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Padding(int padding)
        {
            Options.Padding = Math.Max(0, padding);
            Options.ContentPadding = null;
            return this;
        }

        /// <summary>
        /// Sets horizontal and vertical content padding inside the panel.
        /// </summary>
        /// <param name="horizontal">Padding added to the left and right sides.</param>
        /// <param name="vertical">Padding added above and below the content.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Padding(int horizontal, int vertical)
        {
            return Padding(new Padding(horizontal, vertical));
        }

        /// <summary>
        /// Sets explicit four-side content padding inside the panel.
        /// </summary>
        /// <param name="padding">The padding to apply.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Padding(Padding padding)
        {
            Options.ContentPadding = padding != null ? padding.Clone() : new Padding(0);
            return this;
        }

        /// <summary>
        /// Sets explicit four-side content padding inside the panel.
        /// </summary>
        /// <param name="left">Padding added to the left side.</param>
        /// <param name="top">Padding added above the content.</param>
        /// <param name="right">Padding added to the right side.</param>
        /// <param name="bottom">Padding added below the content.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Padding(int left, int top, int right, int bottom)
        {
            return Padding(new Padding(left, top, right, bottom));
        }

        /// <summary>
        /// Sets the left content padding inside the panel.
        /// </summary>
        /// <param name="left">Padding added to the left side.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel PadLeft(int left)
        {
            return MutatePadding(padding => padding.Left = Math.Max(0, left));
        }

        /// <summary>
        /// Sets the top content padding inside the panel.
        /// </summary>
        /// <param name="top">Padding added above the content.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel PadTop(int top)
        {
            return MutatePadding(padding => padding.Top = Math.Max(0, top));
        }

        /// <summary>
        /// Sets the right content padding inside the panel.
        /// </summary>
        /// <param name="right">Padding added to the right side.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel PadRight(int right)
        {
            return MutatePadding(padding => padding.Right = Math.Max(0, right));
        }

        /// <summary>
        /// Sets the bottom content padding inside the panel.
        /// </summary>
        /// <param name="bottom">Padding added below the content.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel PadBottom(int bottom)
        {
            return MutatePadding(padding => padding.Bottom = Math.Max(0, bottom));
        }

        /// <summary>
        /// Sets the panel border style.
        /// </summary>
        /// <param name="style">The named border style.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Border(PanelBorderStyle style)
        {
            Options.BorderStyle = style;
            return this;
        }

        /// <summary>
        /// Sets the panel border preset using Spectre-style box border objects.
        /// </summary>
        /// <param name="border">The border preset to use.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Border(BoxBorder border)
        {
            if (border == null)
            {
                throw new ArgumentNullException(nameof(border));
            }

            Options.BorderStyle = border.Style;
            return this;
        }

        /// <summary>
        /// Uses a rounded border preset.
        /// </summary>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel RoundedBorder()
        {
            return Border(PanelBorderStyle.Rounded);
        }

        /// <summary>
        /// Uses a square border preset.
        /// </summary>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel SquareBorder()
        {
            return Border(PanelBorderStyle.Square);
        }

        /// <summary>
        /// Uses an ASCII border preset.
        /// </summary>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel AsciiBorder()
        {
            return Border(PanelBorderStyle.Ascii);
        }

        /// <summary>
        /// Uses a heavy border preset.
        /// </summary>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel HeavyBorder()
        {
            return Border(PanelBorderStyle.Heavy);
        }

        /// <summary>
        /// Uses a double-line border preset.
        /// </summary>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel DoubleBorder()
        {
            return Border(PanelBorderStyle.Double);
        }

        /// <summary>
        /// Removes the visible panel border while preserving padding, header, footer, and content rendering.
        /// </summary>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel NoBorder()
        {
            return Border(PanelBorderStyle.None);
        }

        /// <summary>
        /// Uses the simple heavy border preset.
        /// </summary>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel SimpleHeavyBorder()
        {
            return Border(BoxBorder.SimpleHeavy);
        }

        /// <summary>
        /// Uses a minimal border preset with no visible frame.
        /// </summary>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel MinimalBorder()
        {
            return Border(BoxBorder.Minimal);
        }

        /// <summary>
        /// Uses the closest available minimal heavy-header border preset.
        /// </summary>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel MinimalHeavyHeadBorder()
        {
            return Border(BoxBorder.MinimalHeavyHead);
        }

        /// <summary>
        /// Uses the closest available minimal double-header border preset.
        /// </summary>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel MinimalDoubleHeadBorder()
        {
            return Border(BoxBorder.MinimalDoubleHead);
        }

        /// <summary>
        /// Uses the closest available horizontal border preset.
        /// </summary>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel HorizontalBorder()
        {
            return Border(BoxBorder.Horizontal);
        }

        /// <summary>
        /// Uses the alternate ASCII border preset.
        /// </summary>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel Ascii2Border()
        {
            return Border(BoxBorder.Ascii2);
        }

        /// <summary>
        /// Uses the closest available ASCII double-header border preset.
        /// </summary>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel AsciiDoubleHeadBorder()
        {
            return Border(BoxBorder.AsciiDoubleHead);
        }

        /// <summary>
        /// Uses the markdown-friendly ASCII border preset.
        /// </summary>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel MarkdownBorder()
        {
            return Border(BoxBorder.Markdown);
        }

        /// <summary>
        /// Uses the closest available heavy edge border preset.
        /// </summary>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel HeavyEdgeBorder()
        {
            return Border(BoxBorder.HeavyEdge);
        }

        /// <summary>
        /// Uses the closest available heavy-header border preset.
        /// </summary>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel HeavyHeadBorder()
        {
            return Border(BoxBorder.HeavyHead);
        }

        /// <summary>
        /// Uses the closest available double edge border preset.
        /// </summary>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel DoubleEdgeBorder()
        {
            return Border(BoxBorder.DoubleEdge);
        }

        /// <summary>
        /// Uses a terminal-safe ASCII border preset.
        /// </summary>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel SafeBorder()
        {
            return Border(BoxBorder.Safe);
        }

        /// <summary>
        /// Sets custom border characters.
        /// </summary>
        /// <param name="topLeft">The top-left corner character.</param>
        /// <param name="topRight">The top-right corner character.</param>
        /// <param name="bottomLeft">The bottom-left corner character.</param>
        /// <param name="bottomRight">The bottom-right corner character.</param>
        /// <param name="horizontal">The horizontal border character.</param>
        /// <param name="vertical">The vertical border character.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel BorderCharacters(char topLeft, char topRight, char bottomLeft, char bottomRight, char horizontal, char vertical)
        {
            Options.BorderStyle = PanelBorderStyle.Custom;
            Options.TopLeft = topLeft;
            Options.TopRight = topRight;
            Options.BottomLeft = bottomLeft;
            Options.BottomRight = bottomRight;
            Options.Horizontal = horizontal;
            Options.Vertical = vertical;
            return this;
        }

        /// <summary>
        /// Sets the title/header alignment.
        /// </summary>
        /// <param name="alignment">The title/header alignment.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel HeaderAlignment(TextBlockAlignment alignment)
        {
            Options.TitleAlignment = alignment;
            return this;
        }

        /// <summary>
        /// Sets the title/header alignment using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="justify">The title/header justification.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel HeaderAlignment(Justify justify)
        {
            Options.TitleAlignment = JustifyMap.ToTextBlockAlignment(justify);
            return this;
        }

        /// <summary>
        /// Sets the title alignment.
        /// </summary>
        /// <param name="alignment">The title alignment.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel TitleAlignment(TextBlockAlignment alignment)
        {
            return HeaderAlignment(alignment);
        }

        /// <summary>
        /// Sets the title alignment using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="justify">The title justification.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel TitleAlignment(Justify justify)
        {
            return HeaderAlignment(justify);
        }

        /// <summary>
        /// Sets the footer alignment.
        /// </summary>
        /// <param name="alignment">The footer alignment.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel FooterAlignment(TextBlockAlignment alignment)
        {
            Options.FooterAlignment = alignment;
            return this;
        }

        /// <summary>
        /// Sets the footer alignment using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="justify">The footer justification.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel FooterAlignment(Justify justify)
        {
            Options.FooterAlignment = JustifyMap.ToTextBlockAlignment(justify);
            return this;
        }

        /// <summary>
        /// Sets the content foreground color.
        /// </summary>
        /// <param name="color">The content foreground color.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel ContentColor(ConsoleColor color)
        {
            Options.ContentScheme = Scheme(color);
            Options.ContentDecoration = Decoration.None;
            Options.ContentForegroundRgb = null;
            Options.ContentBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the content foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The content foreground color.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel ContentColor(SpectreColor color)
        {
            Options.ContentScheme = Scheme(color);
            Options.ContentDecoration = Decoration.None;
            Options.ContentForegroundRgb = color.ToRgbColor();
            Options.ContentBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the content foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The content foreground color.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel ContentColor(RgbColor color)
        {
            Options.ContentScheme = Scheme(color);
            Options.ContentDecoration = Decoration.None;
            Options.ContentForegroundRgb = color;
            Options.ContentBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the content color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel ContentColor(string style)
        {
            ApplyContentStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the content color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel ContentColor(Style style)
        {
            ApplyContentStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the border foreground color.
        /// </summary>
        /// <param name="color">The border foreground color.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel BorderColor(ConsoleColor color)
        {
            Options.BorderScheme = Scheme(color);
            Options.BorderDecoration = Decoration.None;
            Options.BorderForegroundRgb = null;
            Options.BorderBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the border foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The border foreground color.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel BorderColor(SpectreColor color)
        {
            Options.BorderScheme = Scheme(color);
            Options.BorderDecoration = Decoration.None;
            Options.BorderForegroundRgb = color.ToRgbColor();
            Options.BorderBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the border foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The border foreground color.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel BorderColor(RgbColor color)
        {
            Options.BorderScheme = Scheme(color);
            Options.BorderDecoration = Decoration.None;
            Options.BorderForegroundRgb = color;
            Options.BorderBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the border color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel BorderColor(string style)
        {
            ApplyBorderStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the border color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel BorderColor(Style style)
        {
            ApplyBorderStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the title/header foreground color.
        /// </summary>
        /// <param name="color">The title/header foreground color.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel HeaderColor(ConsoleColor color)
        {
            Options.TitleScheme = Scheme(color);
            Options.TitleDecoration = Decoration.None;
            Options.TitleForegroundRgb = null;
            Options.TitleBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the title/header foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The title/header foreground color.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel HeaderColor(SpectreColor color)
        {
            Options.TitleScheme = Scheme(color);
            Options.TitleDecoration = Decoration.None;
            Options.TitleForegroundRgb = color.ToRgbColor();
            Options.TitleBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the title/header foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The title/header foreground color.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel HeaderColor(RgbColor color)
        {
            Options.TitleScheme = Scheme(color);
            Options.TitleDecoration = Decoration.None;
            Options.TitleForegroundRgb = color;
            Options.TitleBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the title/header color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel HeaderColor(string style)
        {
            ApplyHeaderStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the title/header color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel HeaderColor(Style style)
        {
            ApplyHeaderStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the footer foreground color.
        /// </summary>
        /// <param name="color">The footer foreground color.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel FooterColor(ConsoleColor color)
        {
            Options.FooterScheme = Scheme(color);
            Options.FooterDecoration = Decoration.None;
            Options.FooterForegroundRgb = null;
            Options.FooterBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the footer foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The footer foreground color.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel FooterColor(SpectreColor color)
        {
            Options.FooterScheme = Scheme(color);
            Options.FooterDecoration = Decoration.None;
            Options.FooterForegroundRgb = color.ToRgbColor();
            Options.FooterBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the footer foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The footer foreground color.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel FooterColor(RgbColor color)
        {
            Options.FooterScheme = Scheme(color);
            Options.FooterDecoration = Decoration.None;
            Options.FooterForegroundRgb = color;
            Options.FooterBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the footer color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel FooterColor(string style)
        {
            ApplyFooterStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the footer color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Panel"/> instance.</returns>
        public Panel FooterColor(Style style)
        {
            ApplyFooterStyle(style);
            return this;
        }

        private static Action ToRenderAction(IConsoulRenderable renderable)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable));
            }

            return renderable.Render;
        }

        private static string FormatHeaderFooter(IConsoulRenderable renderable)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable));
            }

            var markup = renderable as MarkupEntry;
            if (markup != null)
            {
                return markup.Markup ?? string.Empty;
            }

            var textBlock = renderable as TextBlockEntry;
            if (textBlock != null)
            {
                return textBlock.Content ?? string.Empty;
            }

            return RenderActionContent.Capture(renderable, 80, 20).Trim();
        }

        private static int ResolveCaptureWidth(PanelOptions options)
        {
            int width = PanelEntry.ResolveWidth(options?.Width);
            if (width < 4)
            {
                return width;
            }

            Padding padding = options != null ? options.ResolveContentPadding() : new Padding(0);
            int borderWidth = options != null && options.BorderStyle == PanelBorderStyle.None ? 0 : 2;
            return Math.Max(1, width - borderWidth - padding.Horizontal);
        }

        private Panel ApplyHeader(string header, Justify? alignment, Style style)
        {
            Options.Title = header;

            if (alignment.HasValue)
            {
                Options.TitleAlignment = JustifyMap.ToTextBlockAlignment(alignment.Value);
            }

            if (style != null)
            {
                ApplyHeaderStyle(style);
            }

            return this;
        }

        private Panel ApplyFooter(string footer, Justify? alignment, Style style)
        {
            Options.Footer = footer;

            if (alignment.HasValue)
            {
                Options.FooterAlignment = JustifyMap.ToTextBlockAlignment(alignment.Value);
            }

            if (style != null)
            {
                ApplyFooterStyle(style);
            }

            return this;
        }

        private void ApplyBorderStyle(Style style)
        {
            Options.BorderScheme = Scheme(style, Options.BorderScheme ?? RenderOptions.DefaultScheme);
            Options.BorderDecoration = style?.Decoration ?? Decoration.None;
            Options.BorderForegroundRgb = style?.ForegroundRgb;
            Options.BorderBackgroundRgb = style?.BackgroundRgb;
        }

        private void ApplyContentStyle(Style style)
        {
            Options.ContentScheme = Scheme(style, Options.ContentScheme ?? RenderOptions.DefaultScheme);
            Options.ContentDecoration = style?.Decoration ?? Decoration.None;
            Options.ContentForegroundRgb = style?.ForegroundRgb;
            Options.ContentBackgroundRgb = style?.BackgroundRgb;
        }

        private void ApplyHeaderStyle(Style style)
        {
            Options.TitleScheme = Scheme(style, Options.TitleScheme ?? RenderOptions.DefaultScheme);
            Options.TitleDecoration = style?.Decoration ?? Decoration.None;
            Options.TitleForegroundRgb = style?.ForegroundRgb;
            Options.TitleBackgroundRgb = style?.BackgroundRgb;
        }

        private void ApplyFooterStyle(Style style)
        {
            Options.FooterScheme = Scheme(style, Options.FooterScheme ?? RenderOptions.DefaultScheme);
            Options.FooterDecoration = style?.Decoration ?? Decoration.None;
            Options.FooterForegroundRgb = style?.ForegroundRgb;
            Options.FooterBackgroundRgb = style?.BackgroundRgb;
        }

        private Panel MutatePadding(Action<Padding> mutate)
        {
            Padding padding = Options.ResolveContentPadding();
            mutate(padding);
            Options.ContentPadding = padding;
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

        private static ColorScheme Scheme(Style style, ColorScheme fallback)
        {
            return (style ?? Style.Plain).ToColorScheme(fallback);
        }
    }
}
