using ConsoulLibrary.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for a horizontal rule.
    /// </summary>
    public class RuleOptions
    {
        /// <summary>
        /// Gets or sets the requested rule width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the character used to draw the rule.
        /// </summary>
        public char Character { get; set; } = '-';

        /// <summary>
        /// Gets or sets the title alignment when a title is supplied.
        /// </summary>
        public RuleTitleAlignment Alignment { get; set; } = RuleTitleAlignment.Center;

        /// <summary>
        /// Gets or sets the color scheme used for the rule line.
        /// </summary>
        public ColorScheme? RuleScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for the title.
        /// </summary>
        public ColorScheme? TitleScheme { get; set; }

        /// <summary>
        /// Creates a copy of the current rule options.
        /// </summary>
        /// <returns>A new <see cref="RuleOptions"/> instance with the same values.</returns>
        public RuleOptions Clone()
        {
            return new RuleOptions
            {
                Width = Width,
                Character = Character,
                Alignment = Alignment,
                RuleScheme = RuleScheme,
                TitleScheme = TitleScheme
            };
        }

        internal ColorScheme ResolveRuleScheme()
        {
            return RuleScheme ?? new ColorScheme
            {
                Color = RenderOptions.SubnoteColor,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        internal ColorScheme ResolveTitleScheme()
        {
            return TitleScheme ?? ResolveRuleScheme();
        }
    }

    /// <summary>
    /// Defines title alignment for a horizontal rule.
    /// </summary>
    public enum RuleTitleAlignment
    {
        /// <summary>
        /// Renders the title near the left side of the rule.
        /// </summary>
        Left,

        /// <summary>
        /// Renders the title centered in the rule.
        /// </summary>
        Center,

        /// <summary>
        /// Renders the title near the right side of the rule.
        /// </summary>
        Right
    }
}
