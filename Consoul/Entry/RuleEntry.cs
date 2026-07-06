using ConsoulLibrary.Color;
using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Renders a responsive horizontal rule for separating sections inside console views.
    /// </summary>
    public class RuleEntry : IRenderable
    {
        private readonly RuleOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleEntry"/> class.
        /// </summary>
        /// <param name="title">Optional title rendered within the rule.</param>
        /// <param name="options">Optional rendering options for the rule.</param>
        public RuleEntry(string title = null, RuleOptions options = null)
        {
            Title = title;
            _options = options != null ? options.Clone() : new RuleOptions();
        }

        /// <summary>
        /// Gets or sets the optional title rendered within the rule.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets the options used to render the rule.
        /// </summary>
        public RuleOptions Options => _options;

        /// <summary>
        /// Renders the rule to the active console driver.
        /// </summary>
        public void Render()
        {
            int width = PanelEntry.ResolveWidth(_options.Width);
            ColorScheme ruleScheme = _options.ResolveRuleScheme();
            ColorScheme titleScheme = _options.ResolveTitleScheme();

            if (width <= 0)
            {
                return;
            }

            string title = string.IsNullOrWhiteSpace(Title) ? null : Title.Trim();
            string visibleTitle = title == null ? null : MarkupRenderText.GetPlainText(title);
            int titleWidth = string.IsNullOrEmpty(visibleTitle) ? 0 : visibleTitle.Length + 2;
            if (string.IsNullOrEmpty(title) || titleWidth >= width)
            {
                Consoul.Write(new string(_options.Character, width), ruleScheme);
                return;
            }

            int remaining = width - titleWidth;
            int leftWidth;
            int rightWidth;
            switch (_options.Alignment)
            {
                case RuleTitleAlignment.Left:
                    leftWidth = Math.Min(2, remaining);
                    rightWidth = remaining - leftWidth;
                    break;
                case RuleTitleAlignment.Right:
                    rightWidth = Math.Min(2, remaining);
                    leftWidth = remaining - rightWidth;
                    break;
                default:
                    leftWidth = remaining / 2;
                    rightWidth = remaining - leftWidth;
                    break;
            }

            Consoul.Write(new string(_options.Character, leftWidth), ruleScheme, false);
            Consoul.Write(" ", titleScheme, false);
            MarkupRenderText.WriteSegments(MarkupRenderText.ParseSegments(title, titleScheme), titleScheme, false);
            Consoul.Write(" ", titleScheme, false);
            Consoul.Write(new string(_options.Character, rightWidth), ruleScheme);
        }

        /// <summary>
        /// Renders a temporary rule to the active console driver.
        /// </summary>
        /// <param name="title">Optional title rendered within the rule.</param>
        /// <param name="width">Optional requested width. The actual width is clamped to the current terminal width.</param>
        /// <param name="color">Optional foreground color for the rule.</param>
        /// <param name="alignment">Title alignment when a title is supplied.</param>
        public static void Render(string title = null, int? width = null, ConsoleColor? color = null, RuleTitleAlignment alignment = RuleTitleAlignment.Center)
        {
            var options = new RuleOptions
            {
                Width = width,
                Alignment = alignment
            };

            if (color.HasValue)
            {
                options.RuleScheme = new ColorScheme
                {
                    Color = color.Value,
                    BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
                };
            }

            new RuleEntry(title, options).Render();
        }
    }
}
