using ConsoulLibrary.Color;

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
        /// Gets or sets the color scheme used for action labels.
        /// </summary>
        public ColorScheme? LabelScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for shortcut text.
        /// </summary>
        public ColorScheme? ShortcutScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for descriptions.
        /// </summary>
        public ColorScheme? DescriptionScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for disabled reasons.
        /// </summary>
        public ColorScheme? DisabledScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for separators.
        /// </summary>
        public ColorScheme? SeparatorScheme { get; set; }

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
                LabelScheme = LabelScheme,
                ShortcutScheme = ShortcutScheme,
                DescriptionScheme = DescriptionScheme,
                DisabledScheme = DisabledScheme,
                SeparatorScheme = SeparatorScheme
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
    }
}
