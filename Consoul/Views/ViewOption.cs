using System;
using System.Linq.Expressions;

using System.Collections.Generic;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a selectable option in a view, along with an associated action to execute when the option is chosen.
    /// </summary>
    public class ViewOption : IViewOption
    {
        private readonly List<string> _searchTerms = new List<string>();

        /// <summary>
        /// Gets or sets the <see cref="LineEntry"/> that represents the text displayed for this option.
        /// </summary>
        public LineEntry Entry { get; set; }

        /// <summary>
        /// Gets or sets the callback action to be invoked when this option is selected.
        /// </summary>
        public ViewOptionCallback Action { get; set; }

        /// <summary>
        /// Gets or sets the color of the option text.
        /// </summary>
        public ConsoleColor Color { get; set; }

        /// <summary>
        /// Gets or sets optional ANSI text decoration used for the option label.
        /// </summary>
        public Decoration? Decoration { get; set; }

        /// <summary>
        /// Gets or sets an optional RGB foreground color used for the option label on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets an optional RGB background color used for the option label on ANSI-capable terminals.
        /// </summary>
        public RgbColor? BackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets optional helper text rendered under this option in view prompts.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets optional shortcut text that can select this option in view prompts.
        /// </summary>
        public string Shortcut { get; set; }

        /// <summary>
        /// Gets hidden search terms that can match prompt filtering without changing the rendered label.
        /// </summary>
        public IList<string> SearchTerms => _searchTerms;

        /// <summary>
        /// Gets or sets a value indicating whether this option is a display-only separator.
        /// </summary>
        public bool IsSeparator { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewOption"/> class with the specified message, action, and optional color.
        /// </summary>
        /// <param name="message">The message to display for this view option.</param>
        /// <param name="action">The callback action to invoke when this option is selected.</param>
        /// <param name="color">The color to display for the message text. If null, defaults to the <see cref="RenderOptions.DefaultColor"/>.</param>
        public ViewOption(string message, ViewOptionCallback action, ConsoleColor? color = null)
        {
            if (color == null)
                color = RenderOptions.DefaultScheme.Color;

            // Create a new LineEntry to represent the view option.
            Entry = new LineEntry(message, (ConsoleColor)color);

            Action = action;
            Color = Entry.Color;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewOption"/> class with a Spectre-style color facade.
        /// </summary>
        /// <param name="message">The message to display for this view option.</param>
        /// <param name="action">The callback action to invoke when this option is selected.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the message text.</param>
        public ViewOption(string message, ViewOptionCallback action, SpectreColor color)
            : this(message, action, color.ToRgbColor())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewOption"/> class with an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="message">The message to display for this view option.</param>
        /// <param name="action">The callback action to invoke when this option is selected.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the message text.</param>
        public ViewOption(string message, ViewOptionCallback action, RgbColor color)
            : this(message, action, (ConsoleColor?)color.ToConsoleColor())
        {
            ApplyColor(color);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewOption"/> class with the specified message, action, and style text.
        /// </summary>
        /// <param name="message">The message to display for this view option.</param>
        /// <param name="action">The callback action to invoke when this option is selected.</param>
        /// <param name="style">A style string such as <c>green</c>, <c>grey</c>, <c>aqua</c>, or <c>#22c55e</c>.</param>
        public ViewOption(string message, ViewOptionCallback action, string style)
            : this(message, action, RenderOptions.DefaultScheme.Color)
        {
            ApplyStyle(ParseStyle(style), RenderOptions.DefaultScheme.Color);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewOption"/> class with the specified message, action, and style.
        /// </summary>
        /// <param name="message">The message to display for this view option.</param>
        /// <param name="action">The callback action to invoke when this option is selected.</param>
        /// <param name="style">Optional foreground style for the message text.</param>
        public ViewOption(string message, ViewOptionCallback action, Style style)
            : this(message, action, RenderOptions.DefaultScheme.Color)
        {
            ApplyStyle(style, RenderOptions.DefaultScheme.Color);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewOption"/> class with optional helper text.
        /// </summary>
        /// <param name="message">The message to display for this view option.</param>
        /// <param name="action">The callback action to invoke when this option is selected.</param>
        /// <param name="color">The color to display for the message text. If null, defaults to the <see cref="RenderOptions.DefaultColor"/>.</param>
        /// <param name="description">Optional helper text rendered under this option.</param>
        public ViewOption(string message, ViewOptionCallback action, ConsoleColor? color, string description)
            : this(message, action, color)
        {
            Description = description;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewOption"/> class with a Spectre-style color facade and optional helper text.
        /// </summary>
        /// <param name="message">The message to display for this view option.</param>
        /// <param name="action">The callback action to invoke when this option is selected.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the message text.</param>
        /// <param name="description">Optional helper text rendered under this option.</param>
        public ViewOption(string message, ViewOptionCallback action, SpectreColor color, string description)
            : this(message, action, color)
        {
            Description = description;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewOption"/> class with an RGB color and optional helper text.
        /// </summary>
        /// <param name="message">The message to display for this view option.</param>
        /// <param name="action">The callback action to invoke when this option is selected.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the message text.</param>
        /// <param name="description">Optional helper text rendered under this option.</param>
        public ViewOption(string message, ViewOptionCallback action, RgbColor color, string description)
            : this(message, action, color)
        {
            Description = description;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewOption"/> class with style text and optional helper text.
        /// </summary>
        /// <param name="message">The message to display for this view option.</param>
        /// <param name="action">The callback action to invoke when this option is selected.</param>
        /// <param name="style">A style string such as <c>green</c>, <c>grey</c>, <c>aqua</c>, or <c>#22c55e</c>.</param>
        /// <param name="description">Optional helper text rendered under this option.</param>
        public ViewOption(string message, ViewOptionCallback action, string style, string description)
            : this(message, action, style)
        {
            Description = description;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewOption"/> class with a style and optional helper text.
        /// </summary>
        /// <param name="message">The message to display for this view option.</param>
        /// <param name="action">The callback action to invoke when this option is selected.</param>
        /// <param name="style">Optional foreground style for the message text.</param>
        /// <param name="description">Optional helper text rendered under this option.</param>
        public ViewOption(string message, ViewOptionCallback action, Style style, string description)
            : this(message, action, style)
        {
            Description = description;
        }

        /// <summary>
        /// Sets helper text rendered under this option in view prompts.
        /// </summary>
        /// <param name="description">Optional helper text rendered under this option.</param>
        /// <returns>The current <see cref="ViewOption"/> instance.</returns>
        public ViewOption Describe(string description)
        {
            Description = description;
            return this;
        }

        /// <summary>
        /// Sets shortcut text that can select this option in view prompts.
        /// </summary>
        /// <param name="shortcut">Shortcut text for this option.</param>
        /// <returns>The current <see cref="ViewOption"/> instance.</returns>
        public ViewOption UseShortcut(string shortcut)
        {
            Shortcut = shortcut;
            return this;
        }

        /// <summary>
        /// Adds hidden search terms that can match prompt filtering without changing the rendered label.
        /// </summary>
        /// <param name="searchTerms">Search terms or aliases for this option.</param>
        /// <returns>The current <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddSearchTerms(params string[] searchTerms)
        {
            return AddSearchTerms((IEnumerable<string>)searchTerms);
        }

        /// <summary>
        /// Adds hidden search terms that can match prompt filtering without changing the rendered label.
        /// </summary>
        /// <param name="searchTerms">Search terms or aliases for this option.</param>
        /// <returns>The current <see cref="ViewOption"/> instance.</returns>
        public ViewOption AddSearchTerms(IEnumerable<string> searchTerms)
        {
            if (searchTerms == null)
            {
                return this;
            }

            foreach (string term in searchTerms)
            {
                if (!string.IsNullOrWhiteSpace(term))
                {
                    _searchTerms.Add(term.Trim());
                }
            }

            return this;
        }

        /// <summary>
        /// Sets the option text color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ViewOption"/> instance.</returns>
        public ViewOption UseColor(ConsoleColor color)
        {
            Entry.Color = color;
            Color = color;
            Decoration = null;
            ForegroundRgb = null;
            BackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the option text color from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ViewOption"/> instance.</returns>
        public ViewOption UseColor(SpectreColor color)
        {
            return UseColor(color.ToRgbColor());
        }

        /// <summary>
        /// Sets the option text color from an RGB value mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ViewOption"/> instance.</returns>
        public ViewOption UseColor(RgbColor color)
        {
            ApplyColor(color);
            return this;
        }

        /// <summary>
        /// Sets the option text style from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply to the option label.</param>
        /// <returns>The current <see cref="ViewOption"/> instance.</returns>
        public ViewOption UseStyle(Style style)
        {
            ApplyStyle(style, Color);
            return this;
        }

        /// <summary>
        /// Sets the option text style from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>bold #22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="ViewOption"/> instance.</returns>
        public ViewOption UseStyle(string style)
        {
            return UseStyle(ParseStyle(style));
        }

        /// <summary>
        /// Creates a display-only separator for grouping view options.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="color">Optional separator foreground color.</param>
        /// <returns>A configured separator option.</returns>
        public static ViewOption Separator(string label = null, ConsoleColor? color = null)
        {
            return new ViewOption(label ?? string.Empty, delegate { }, color ?? RenderOptions.SubnoteColor)
            {
                IsSeparator = true
            };
        }

        /// <summary>
        /// Creates a display-only separator for grouping view options using a Spectre-style color facade.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the separator.</param>
        /// <returns>A configured separator option.</returns>
        public static ViewOption Separator(string label, SpectreColor color)
        {
            return Separator(label, color.ToRgbColor());
        }

        /// <summary>
        /// Creates a display-only separator for grouping view options using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the separator.</param>
        /// <returns>A configured separator option.</returns>
        public static ViewOption Separator(string label, RgbColor color)
        {
            ViewOption option = Separator(label, (ConsoleColor?)color.ToConsoleColor());
            option.ApplyColor(color);
            return option;
        }

        /// <summary>
        /// Creates a display-only separator for grouping view options using style text.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="style">Optional separator foreground style.</param>
        /// <returns>A configured separator option.</returns>
        public static ViewOption Separator(string label, string style)
        {
            return Separator(label, ParseStyle(style));
        }

        /// <summary>
        /// Creates a display-only separator for grouping view options using a reusable style.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="style">Optional separator foreground style.</param>
        /// <returns>A configured separator option.</returns>
        public static ViewOption Separator(string label, Style style)
        {
            ViewOption option = Separator(label, PromptRenderHelpers.ResolveStyleColor(style, RenderOptions.SubnoteColor));
            option.ApplyStyle(style, RenderOptions.SubnoteColor);
            return option;
        }

        /// <summary>
        /// Creates a display-only separator for grouping view options using style text.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="style">Optional separator foreground style.</param>
        /// <returns>A configured separator option.</returns>
        public static ViewOption SeparatorStyled(string label = null, string style = null)
        {
            return Separator(label, ParseStyle(style));
        }

        /// <summary>
        /// Builds the message to be displayed for this view option, allowing for an optional template transformation.
        /// </summary>
        /// <param name="template">
        /// An optional expression used to customize the message. The expression should take a string as input and return a string.
        /// </param>
        /// <returns>The formatted message for this view option.</returns>
        public string Render(Expression<Func<string, string>> template = null)
        {
            if (template != null)
            {
                // If a template is provided, compile and apply it to the existing message.
                return template.Compile().Invoke(Entry.Message);
            }
            return Entry.Message;
        }

        internal ViewHelpItem ToHelpItem(string label, string disabledReason = null)
        {
            return new ViewHelpItem(
                label,
                Description,
                Shortcut,
                disabledReason,
                IsSeparator,
                CreateScheme(Color),
                Decoration,
                ForegroundRgb,
                BackgroundRgb);
        }

        internal void ApplyStyleTo(SelectOption option)
        {
            if (option == null)
            {
                return;
            }

            option.Color = Color;
            option.Decoration = Decoration;
            option.ForegroundRgb = ForegroundRgb;
            option.BackgroundRgb = BackgroundRgb;
        }

        internal void ApplyStyleTo(ShortcutItem item)
        {
            if (item == null)
            {
                return;
            }

            item.DescriptionScheme = CreateScheme(Color);
            item.DescriptionDecoration = Decoration;
            item.DescriptionForegroundRgb = ForegroundRgb;
            item.DescriptionBackgroundRgb = BackgroundRgb;
        }

        private void ApplyColor(RgbColor color)
        {
            Entry.Color = color.ToConsoleColor();
            Color = Entry.Color;
            Decoration = global::ConsoulLibrary.Decoration.None;
            ForegroundRgb = color;
            BackgroundRgb = null;
        }

        private void ApplyStyle(Style style, ConsoleColor fallback)
        {
            if (style == null)
            {
                UseColor(fallback);
                return;
            }

            ColorScheme scheme = style.ToColorScheme(CreateScheme(fallback));
            Entry.Color = scheme.Color;
            Color = Entry.Color;
            Decoration = style.Decoration;
            ForegroundRgb = style.ForegroundRgb;
            BackgroundRgb = style.BackgroundRgb;
        }

        private static Style ParseStyle(string style)
        {
            return string.IsNullOrWhiteSpace(style) ? null : Style.Parse(style);
        }

        private static ColorScheme CreateScheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }
    }
}
