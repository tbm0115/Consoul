using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for <see cref="ViewHelpEntry"/>.
    /// </summary>
    public class ViewHelpOptions
    {
        /// <summary>
        /// Gets or sets the requested help width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the preferred width used for action labels.
        /// </summary>
        public int? LabelWidth { get; set; }

        /// <summary>
        /// Gets or sets the preferred width used for rendered shortcut text.
        /// </summary>
        public int? ShortcutWidth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether shortcuts should be rendered.
        /// </summary>
        public bool ShowShortcuts { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether descriptions should be rendered.
        /// </summary>
        public bool ShowDescriptions { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether disabled reasons should be rendered.
        /// </summary>
        public bool ShowDisabledReasons { get; set; } = true;

        /// <summary>
        /// Gets or sets text prepended to shortcut values.
        /// </summary>
        public string ShortcutPrefix { get; set; } = "[";

        /// <summary>
        /// Gets or sets text appended to shortcut values.
        /// </summary>
        public string ShortcutSuffix { get; set; } = "]";

        /// <summary>
        /// Gets or sets text prepended to disabled reasons.
        /// </summary>
        public string DisabledPrefix { get; set; } = "disabled: ";

        /// <summary>
        /// Gets or sets the color scheme used for the title rule.
        /// </summary>
        public ColorScheme? TitleScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for the title rule.
        /// </summary>
        public Decoration TitleDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for the title rule on ANSI-capable terminals.
        /// </summary>
        public RgbColor? TitleForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for the title rule on ANSI-capable terminals.
        /// </summary>
        public RgbColor? TitleBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for action labels.
        /// </summary>
        public ColorScheme? LabelScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for action labels.
        /// </summary>
        public Decoration LabelDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for action labels on ANSI-capable terminals.
        /// </summary>
        public RgbColor? LabelForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for action labels on ANSI-capable terminals.
        /// </summary>
        public RgbColor? LabelBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for shortcut text.
        /// </summary>
        public ColorScheme? ShortcutScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for shortcut text.
        /// </summary>
        public Decoration ShortcutDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for shortcut text on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ShortcutForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for shortcut text on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ShortcutBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for descriptions.
        /// </summary>
        public ColorScheme? DescriptionScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for descriptions.
        /// </summary>
        public Decoration DescriptionDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for descriptions on ANSI-capable terminals.
        /// </summary>
        public RgbColor? DescriptionForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for descriptions on ANSI-capable terminals.
        /// </summary>
        public RgbColor? DescriptionBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for disabled reasons.
        /// </summary>
        public ColorScheme? DisabledScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for disabled reasons.
        /// </summary>
        public Decoration DisabledDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for disabled reasons on ANSI-capable terminals.
        /// </summary>
        public RgbColor? DisabledForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for disabled reasons on ANSI-capable terminals.
        /// </summary>
        public RgbColor? DisabledBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for separators.
        /// </summary>
        public ColorScheme? SeparatorScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for separators.
        /// </summary>
        public Decoration SeparatorDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for separators on ANSI-capable terminals.
        /// </summary>
        public RgbColor? SeparatorForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for separators on ANSI-capable terminals.
        /// </summary>
        public RgbColor? SeparatorBackgroundRgb { get; set; }

        /// <summary>
        /// Creates a copy of the current view help options.
        /// </summary>
        /// <returns>A new <see cref="ViewHelpOptions"/> instance with the same values.</returns>
        public ViewHelpOptions Clone()
        {
            return new ViewHelpOptions
            {
                Width = Width,
                LabelWidth = LabelWidth,
                ShortcutWidth = ShortcutWidth,
                ShowShortcuts = ShowShortcuts,
                ShowDescriptions = ShowDescriptions,
                ShowDisabledReasons = ShowDisabledReasons,
                ShortcutPrefix = ShortcutPrefix,
                ShortcutSuffix = ShortcutSuffix,
                DisabledPrefix = DisabledPrefix,
                TitleScheme = TitleScheme,
                TitleDecoration = TitleDecoration,
                TitleForegroundRgb = TitleForegroundRgb,
                TitleBackgroundRgb = TitleBackgroundRgb,
                LabelScheme = LabelScheme,
                LabelDecoration = LabelDecoration,
                LabelForegroundRgb = LabelForegroundRgb,
                LabelBackgroundRgb = LabelBackgroundRgb,
                ShortcutScheme = ShortcutScheme,
                ShortcutDecoration = ShortcutDecoration,
                ShortcutForegroundRgb = ShortcutForegroundRgb,
                ShortcutBackgroundRgb = ShortcutBackgroundRgb,
                DescriptionScheme = DescriptionScheme,
                DescriptionDecoration = DescriptionDecoration,
                DescriptionForegroundRgb = DescriptionForegroundRgb,
                DescriptionBackgroundRgb = DescriptionBackgroundRgb,
                DisabledScheme = DisabledScheme,
                DisabledDecoration = DisabledDecoration,
                DisabledForegroundRgb = DisabledForegroundRgb,
                DisabledBackgroundRgb = DisabledBackgroundRgb,
                SeparatorScheme = SeparatorScheme,
                SeparatorDecoration = SeparatorDecoration,
                SeparatorForegroundRgb = SeparatorForegroundRgb,
                SeparatorBackgroundRgb = SeparatorBackgroundRgb
            };
        }

        internal ColorScheme ResolveTitleScheme()
        {
            return TitleScheme ?? RenderOptions.PromptScheme;
        }

        internal ColorScheme ResolveLabelScheme()
        {
            return LabelScheme ?? RenderOptions.OptionScheme;
        }

        internal ColorScheme ResolveShortcutScheme()
        {
            return ShortcutScheme ?? RenderOptions.SubnoteScheme;
        }

        internal ColorScheme ResolveDescriptionScheme()
        {
            return DescriptionScheme ?? RenderOptions.DefaultScheme;
        }

        internal ColorScheme ResolveDisabledScheme()
        {
            return DisabledScheme ?? RenderOptions.InvalidScheme;
        }

        internal ColorScheme ResolveSeparatorScheme()
        {
            return SeparatorScheme ?? RenderOptions.SubnoteScheme;
        }

        internal StyledText ResolveTitleStyle()
        {
            return new StyledText(ResolveTitleScheme(), TitleDecoration, TitleForegroundRgb, TitleBackgroundRgb);
        }

        internal StyledText ResolveLabelStyle()
        {
            return new StyledText(ResolveLabelScheme(), LabelDecoration, LabelForegroundRgb, LabelBackgroundRgb);
        }

        internal StyledText ResolveShortcutStyle()
        {
            return new StyledText(ResolveShortcutScheme(), ShortcutDecoration, ShortcutForegroundRgb, ShortcutBackgroundRgb);
        }

        internal StyledText ResolveDescriptionStyle()
        {
            return new StyledText(ResolveDescriptionScheme(), DescriptionDecoration, DescriptionForegroundRgb, DescriptionBackgroundRgb);
        }

        internal StyledText ResolveDisabledStyle()
        {
            return new StyledText(ResolveDisabledScheme(), DisabledDecoration, DisabledForegroundRgb, DisabledBackgroundRgb);
        }

        internal StyledText ResolveSeparatorStyle()
        {
            return new StyledText(ResolveSeparatorScheme(), SeparatorDecoration, SeparatorForegroundRgb, SeparatorBackgroundRgb);
        }

        internal readonly struct StyledText
        {
            public StyledText(ColorScheme scheme, Decoration decoration, RgbColor? foregroundRgb, RgbColor? backgroundRgb)
            {
                Scheme = scheme;
                Decoration = decoration;
                ForegroundRgb = foregroundRgb;
                BackgroundRgb = backgroundRgb;
            }

            public ColorScheme Scheme { get; }

            public Decoration Decoration { get; }

            public RgbColor? ForegroundRgb { get; }

            public RgbColor? BackgroundRgb { get; }
        }
    }
}
