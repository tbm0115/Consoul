using System;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style fluent rule construction over <see cref="RuleEntry"/>.
    /// </summary>
    /// <remarks>
    /// The rule uses the same responsive renderer as <see cref="RuleEntry"/> and can be passed anywhere an
    /// <see cref="IConsoulRenderable"/> is accepted.
    /// </remarks>
    public class Rule : RuleEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rule"/> class.
        /// </summary>
        public Rule() : this((string)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rule"/> class.
        /// </summary>
        /// <param name="title">Optional title rendered within the rule.</param>
        public Rule(string title) : base(title)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rule"/> class with a renderable title.
        /// </summary>
        /// <param name="title">Renderable title captured or adapted for the rule title.</param>
        public Rule(IConsoulRenderable title) : this(FormatTitle(title))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rule"/> class with a title and justification.
        /// </summary>
        /// <param name="title">Optional title rendered within the rule.</param>
        /// <param name="justification">Title justification to apply.</param>
        public Rule(string title, Justify justification) : this(title)
        {
            Options.Alignment = JustifyMap.ToRuleTitleAlignment(justification);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rule"/> class with a renderable title and justification.
        /// </summary>
        /// <param name="title">Renderable title captured or adapted for the rule title.</param>
        /// <param name="justification">Title justification to apply.</param>
        public Rule(IConsoulRenderable title, Justify justification) : this(title)
        {
            Options.Alignment = JustifyMap.ToRuleTitleAlignment(justification);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rule"/> class with a title color and optional justification.
        /// </summary>
        /// <param name="title">Optional title rendered within the rule.</param>
        /// <param name="titleColor">Title foreground color.</param>
        /// <param name="justification">Optional title justification.</param>
        public Rule(string title, ConsoleColor titleColor, Justify? justification = null) : this(title)
        {
            Options.TitleScheme = Scheme(titleColor);

            if (justification.HasValue)
            {
                Options.Alignment = JustifyMap.ToRuleTitleAlignment(justification.Value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rule"/> class with a renderable title color and optional justification.
        /// </summary>
        /// <param name="title">Renderable title captured or adapted for the rule title.</param>
        /// <param name="titleColor">Title foreground color.</param>
        /// <param name="justification">Optional title justification.</param>
        public Rule(IConsoulRenderable title, ConsoleColor titleColor, Justify? justification = null) : this(title)
        {
            Options.TitleScheme = Scheme(titleColor);

            if (justification.HasValue)
            {
                Options.Alignment = JustifyMap.ToRuleTitleAlignment(justification.Value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rule"/> class with a Spectre-style title color and optional justification.
        /// </summary>
        /// <param name="title">Optional title rendered within the rule.</param>
        /// <param name="titleColor">Title foreground color.</param>
        /// <param name="justification">Optional title justification.</param>
        public Rule(string title, SpectreColor titleColor, Justify? justification = null)
            : this(title)
        {
            ApplyTitleStyle(new Style(titleColor));

            if (justification.HasValue)
            {
                Options.Alignment = JustifyMap.ToRuleTitleAlignment(justification.Value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rule"/> class with a renderable title, Spectre-style title color, and optional justification.
        /// </summary>
        /// <param name="title">Renderable title captured or adapted for the rule title.</param>
        /// <param name="titleColor">Title foreground color.</param>
        /// <param name="justification">Optional title justification.</param>
        public Rule(IConsoulRenderable title, SpectreColor titleColor, Justify? justification = null)
            : this(title)
        {
            ApplyTitleStyle(new Style(titleColor));

            if (justification.HasValue)
            {
                Options.Alignment = JustifyMap.ToRuleTitleAlignment(justification.Value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rule"/> class with an RGB title color and optional justification.
        /// </summary>
        /// <param name="title">Optional title rendered within the rule.</param>
        /// <param name="titleColor">Title foreground color mapped to Consoul's console palette.</param>
        /// <param name="justification">Optional title justification.</param>
        public Rule(string title, RgbColor titleColor, Justify? justification = null)
            : this(title)
        {
            ApplyTitleStyle(new Style(titleColor));

            if (justification.HasValue)
            {
                Options.Alignment = JustifyMap.ToRuleTitleAlignment(justification.Value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rule"/> class with a renderable title, RGB title color, and optional justification.
        /// </summary>
        /// <param name="title">Renderable title captured or adapted for the rule title.</param>
        /// <param name="titleColor">Title foreground color mapped to Consoul's console palette.</param>
        /// <param name="justification">Optional title justification.</param>
        public Rule(IConsoulRenderable title, RgbColor titleColor, Justify? justification = null)
            : this(title)
        {
            ApplyTitleStyle(new Style(titleColor));

            if (justification.HasValue)
            {
                Options.Alignment = JustifyMap.ToRuleTitleAlignment(justification.Value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rule"/> class with explicit render options.
        /// </summary>
        /// <param name="title">Optional title rendered within the rule.</param>
        /// <param name="options">Optional rendering options for the rule.</param>
        public Rule(string title, RuleOptions options) : base(title, options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rule"/> class with a renderable title and explicit render options.
        /// </summary>
        /// <param name="title">Renderable title captured or adapted for the rule title.</param>
        /// <param name="options">Optional rendering options for the rule.</param>
        public Rule(IConsoulRenderable title, RuleOptions options) : base(FormatTitle(title), options)
        {
        }

        /// <summary>
        /// Gets or sets the rule title justification using Spectre-style vocabulary.
        /// </summary>
        public Justify? Justification
        {
            get { return JustifyMap.FromRuleTitleAlignment(Options.Alignment); }
            set
            {
                if (value.HasValue)
                {
                    Options.Alignment = JustifyMap.ToRuleTitleAlignment(value.Value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the rule line style for Spectre-style object initializer snippets.
        /// </summary>
        public Style Style
        {
            get
            {
                if (!Options.RuleScheme.HasValue)
                {
                    return null;
                }

                ColorScheme scheme = Options.RuleScheme.Value;
                Style style = new Style(Options.RuleDecoration, scheme.Color, scheme.BackgroundColor);
                if (Options.RuleForegroundRgb.HasValue)
                {
                    style = style.WithForeground(Options.RuleForegroundRgb.Value);
                }

                if (Options.RuleBackgroundRgb.HasValue)
                {
                    style = style.WithBackground(Options.RuleBackgroundRgb.Value);
                }

                return style;
            }
            set
            {
                if (value == null)
                {
                    Options.RuleScheme = null;
                    Options.RuleDecoration = Decoration.None;
                    Options.RuleForegroundRgb = null;
                    Options.RuleBackgroundRgb = null;
                    return;
                }

                ApplyRuleStyle(value);
            }
        }

        /// <summary>
        /// Sets the rule title.
        /// </summary>
        /// <param name="title">The rule title.</param>
        /// <returns>The current <see cref="Rule"/> instance.</returns>
        public Rule TitleText(string title)
        {
            Title = title;
            return this;
        }

        /// <summary>
        /// Sets the rule title from a renderable component.
        /// </summary>
        /// <param name="title">Renderable title captured or adapted for the rule title.</param>
        /// <returns>The current <see cref="Rule"/> instance.</returns>
        public Rule TitleText(IConsoulRenderable title)
        {
            Title = FormatTitle(title);
            return this;
        }

        /// <summary>
        /// Sets the rule title using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="title">The rule title.</param>
        /// <returns>The current <see cref="Rule"/> instance.</returns>
        public Rule RuleTitle(string title)
        {
            return TitleText(title);
        }

        /// <summary>
        /// Sets the rule title from a renderable component using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="title">Renderable title captured or adapted for the rule title.</param>
        /// <returns>The current <see cref="Rule"/> instance.</returns>
        public Rule RuleTitle(IConsoulRenderable title)
        {
            return TitleText(title);
        }

        /// <summary>
        /// Sets the requested rule width.
        /// </summary>
        /// <param name="width">The requested width. The actual width is clamped to the active terminal.</param>
        /// <returns>The current <see cref="Rule"/> instance.</returns>
        public Rule Width(int? width)
        {
            Options.Width = width;
            return this;
        }

        /// <summary>
        /// Expands the rule to the active terminal width.
        /// </summary>
        /// <returns>The current <see cref="Rule"/> instance.</returns>
        public Rule Expand()
        {
            Options.Width = null;
            return this;
        }

        /// <summary>
        /// Sets the character used to draw the rule line.
        /// </summary>
        /// <param name="character">The rule line character.</param>
        /// <returns>The current <see cref="Rule"/> instance.</returns>
        public Rule Character(char character)
        {
            Options.Character = character;
            return this;
        }

        /// <summary>
        /// Aligns the title near the left side of the rule.
        /// </summary>
        /// <returns>The current <see cref="Rule"/> instance.</returns>
        public Rule LeftJustified()
        {
            Options.Alignment = RuleTitleAlignment.Left;
            return this;
        }

        /// <summary>
        /// Aligns the title near the left side of the rule using table/text alignment vocabulary.
        /// </summary>
        /// <returns>The current <see cref="Rule"/> instance.</returns>
        public Rule LeftAligned()
        {
            return LeftJustified();
        }

        /// <summary>
        /// Centers the title inside the rule.
        /// </summary>
        /// <returns>The current <see cref="Rule"/> instance.</returns>
        public Rule Centered()
        {
            Options.Alignment = RuleTitleAlignment.Center;
            return this;
        }

        /// <summary>
        /// Aligns the title near the right side of the rule.
        /// </summary>
        /// <returns>The current <see cref="Rule"/> instance.</returns>
        public Rule RightJustified()
        {
            Options.Alignment = RuleTitleAlignment.Right;
            return this;
        }

        /// <summary>
        /// Aligns the title near the right side of the rule using table/text alignment vocabulary.
        /// </summary>
        /// <returns>The current <see cref="Rule"/> instance.</returns>
        public Rule RightAligned()
        {
            return RightJustified();
        }

        /// <summary>
        /// Sets the rule title justification using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="justify">The justification to apply.</param>
        /// <returns>The current <see cref="Rule"/> instance.</returns>
        public Rule Justify(Justify justify)
        {
            Options.Alignment = JustifyMap.ToRuleTitleAlignment(justify);
            return this;
        }

        /// <summary>
        /// Sets the rule line foreground color.
        /// </summary>
        /// <param name="color">The rule line foreground color.</param>
        /// <returns>The current <see cref="Rule"/> instance.</returns>
        public Rule RuleColor(ConsoleColor color)
        {
            Options.RuleScheme = Scheme(color);
            Options.RuleDecoration = Decoration.None;
            Options.RuleForegroundRgb = null;
            Options.RuleBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the rule line foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The rule line foreground color.</param>
        /// <returns>The current <see cref="Rule"/> instance.</returns>
        public Rule RuleColor(SpectreColor color)
        {
            Options.RuleScheme = Scheme(color);
            Options.RuleDecoration = Decoration.None;
            Options.RuleForegroundRgb = color.ToRgbColor();
            Options.RuleBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the rule line foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The rule line foreground color.</param>
        /// <returns>The current <see cref="Rule"/> instance.</returns>
        public Rule RuleColor(RgbColor color)
        {
            Options.RuleScheme = Scheme(color);
            Options.RuleDecoration = Decoration.None;
            Options.RuleForegroundRgb = color;
            Options.RuleBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the rule title foreground color.
        /// </summary>
        /// <param name="color">The rule title foreground color.</param>
        /// <returns>The current <see cref="Rule"/> instance.</returns>
        public Rule TitleColor(ConsoleColor color)
        {
            Options.TitleScheme = Scheme(color);
            Options.TitleDecoration = Decoration.None;
            Options.TitleForegroundRgb = null;
            Options.TitleBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the rule title foreground color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The rule title foreground color.</param>
        /// <returns>The current <see cref="Rule"/> instance.</returns>
        public Rule TitleColor(SpectreColor color)
        {
            Options.TitleScheme = Scheme(color);
            Options.TitleDecoration = Decoration.None;
            Options.TitleForegroundRgb = color.ToRgbColor();
            Options.TitleBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the rule title foreground color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The rule title foreground color.</param>
        /// <returns>The current <see cref="Rule"/> instance.</returns>
        public Rule TitleColor(RgbColor color)
        {
            Options.TitleScheme = Scheme(color);
            Options.TitleDecoration = Decoration.None;
            Options.TitleForegroundRgb = color;
            Options.TitleBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the rule line style using Consoul markup color syntax.
        /// </summary>
        /// <param name="style">A markup style such as <c>gray</c>, <c>cyan</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Rule"/> instance.</returns>
        public Rule RuleStyle(string style)
        {
            ApplyRuleStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the rule line style from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Rule"/> instance.</returns>
        public Rule RuleStyle(Style style)
        {
            ApplyRuleStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the rule title style using Consoul markup color syntax.
        /// </summary>
        /// <param name="style">A markup style such as <c>yellow</c>, <c>prompt</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="Rule"/> instance.</returns>
        public Rule TitleStyle(string style)
        {
            ApplyTitleStyle(Style.Parse(style));
            return this;
        }

        /// <summary>
        /// Sets the rule title style from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="Rule"/> instance.</returns>
        public Rule TitleStyle(Style style)
        {
            ApplyTitleStyle(style);
            return this;
        }

        private void ApplyRuleStyle(Style style)
        {
            Options.RuleScheme = Scheme(style, Options.RuleScheme ?? RenderOptions.DefaultScheme);
            Options.RuleDecoration = style?.Decoration ?? Decoration.None;
            Options.RuleForegroundRgb = style?.ForegroundRgb;
            Options.RuleBackgroundRgb = style?.BackgroundRgb;
        }

        private void ApplyTitleStyle(Style style)
        {
            Options.TitleScheme = Scheme(style, Options.TitleScheme ?? Options.ResolveRuleScheme());
            Options.TitleDecoration = style?.Decoration ?? Decoration.None;
            Options.TitleForegroundRgb = style?.ForegroundRgb;
            Options.TitleBackgroundRgb = style?.BackgroundRgb;
        }

        private static string FormatTitle(IConsoulRenderable title)
        {
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            if (title is MarkupEntry markup)
            {
                return markup.Markup;
            }

            if (title is TextBlockEntry textBlock)
            {
                return textBlock.Content;
            }

            return RenderActionContent.Capture(title, 80, 20).Trim();
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
