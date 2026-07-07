using System;
using System.Collections.Generic;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents an individual selection option for a prompt, including properties for label, color, index, and selection state.
    /// </summary>
    public class SelectOption
    {
        private readonly List<string> _searchTerms = new List<string>();
        private readonly List<SelectOption> _children = new List<SelectOption>();
        private Func<SelectOption, string, ConsoleColor?, bool, OptionRenderStyle, SelectOption> _childFactory;

        /// <summary>
        /// Gets or sets the display label for this option.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets optional helper text rendered under this option.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets optional shortcut text that can be typed or pressed to select this option.
        /// </summary>
        public string Shortcut { get; set; }

        /// <summary>
        /// Gets hidden search terms that can match filter text without changing the rendered label.
        /// </summary>
        public IList<string> SearchTerms => _searchTerms;

        /// <summary>
        /// Gets the parent option when this option belongs to a grouped prompt choice.
        /// </summary>
        public SelectOption Parent { get; private set; }

        /// <summary>
        /// Gets the child options nested under this option.
        /// </summary>
        public IReadOnlyList<SelectOption> Children => _children;

        /// <summary>
        /// Gets the nesting depth used when rendering grouped prompt choices.
        /// </summary>
        public int Depth { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this option is a grouped choice that can contain child options.
        /// </summary>
        public bool IsGroup { get; set; }

        /// <summary>
        /// Gets a value indicating whether this option has child options.
        /// </summary>
        public bool HasChildren => _children.Count > 0;

        /// <summary>
        /// Gets or sets the display color for this option.
        /// </summary>
        public ConsoleColor Color { get; set; }

        /// <summary>
        /// Gets or sets optional ANSI decoration used for this option label.
        /// </summary>
        public Decoration? Decoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for this option label on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for this option label on ANSI-capable terminals.
        /// </summary>
        public RgbColor? BackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the zero-based index reference for this option.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this option is the default selection.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this option has been selected.
        /// </summary>
        public bool Selected { get; set; } = false;

        /// <summary>
        /// Gets or sets the rendering style for this option.
        /// </summary>
        public OptionRenderStyle Style { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this option is displayed but cannot be selected.
        /// </summary>
        public bool IsDisabled { get; set; }

        /// <summary>
        /// Gets or sets optional text explaining why the option cannot be selected.
        /// </summary>
        public string DisabledReason { get; set; }

        /// <summary>
        /// Gets or sets the display color used when <see cref="IsDisabled"/> is <c>true</c>.
        /// </summary>
        public ConsoleColor DisabledColor { get; set; } = ConsoleColor.DarkGray;

        /// <summary>
        /// Gets or sets optional ANSI decoration used when this option is disabled.
        /// </summary>
        public Decoration? DisabledDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used when this option is disabled on ANSI-capable terminals.
        /// </summary>
        public RgbColor? DisabledForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used when this option is disabled on ANSI-capable terminals.
        /// </summary>
        public RgbColor? DisabledBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this item is a display-only separator.
        /// </summary>
        public bool IsSeparator { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectOption"/> class with the specified label, color, default flag, and render style.
        /// </summary>
        /// <param name="label">The label for this option.</param>
        /// <param name="color">The color of the option text in the console. Defaults to the standard option color if not provided.</param>
        /// <param name="isDefault">Indicates whether this option is the default selection.</param>
        /// <param name="renderStyle">Specifies the rendering style for this option.</param>
        public SelectOption(string label, ConsoleColor? color = null, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            Label = label;
            Color = color ?? RenderOptions.OptionColor;
            IsDefault = isDefault;
            Style = renderStyle;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectOption"/> class with an optional description.
        /// </summary>
        /// <param name="label">The label for this option.</param>
        /// <param name="description">Optional helper text rendered under this option.</param>
        /// <param name="color">The color of the option text in the console. Defaults to the standard option color if not provided.</param>
        /// <param name="isDefault">Indicates whether this option is the default selection.</param>
        /// <param name="renderStyle">Specifies the rendering style for this option.</param>
        public SelectOption(string label, string description, ConsoleColor? color = null, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
            : this(label, color, isDefault, renderStyle)
        {
            Description = description;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectOption"/> class with the specified index, label, color, default flag, and render style.
        /// </summary>
        /// <param name="index">The zero-based index of this option.</param>
        /// <param name="label">The label for this option.</param>
        /// <param name="color">The color of the option text in the console.</param>
        /// <param name="isDefault">Indicates whether this option is the default selection.</param>
        /// <param name="renderStyle">Specifies the rendering style for this option.</param>
        public SelectOption(int index, string label, ConsoleColor color, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable) : this(label, color, isDefault, renderStyle)
        {
            Index = index;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectOption"/> class with the specified index and optional description.
        /// </summary>
        /// <param name="index">The zero-based index of this option.</param>
        /// <param name="label">The label for this option.</param>
        /// <param name="color">The color of the option text in the console.</param>
        /// <param name="description">Optional helper text rendered under this option.</param>
        /// <param name="isDefault">Indicates whether this option is the default selection.</param>
        /// <param name="renderStyle">Specifies the rendering style for this option.</param>
        public SelectOption(int index, string label, ConsoleColor color, string description, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
            : this(index, label, color, isDefault, renderStyle)
        {
            Description = description;
        }

        /// <summary>
        /// Sets helper text rendered under this option.
        /// </summary>
        /// <param name="description">Optional helper text rendered under this option.</param>
        /// <returns>The current <see cref="SelectOption"/> instance.</returns>
        public SelectOption Describe(string description)
        {
            Description = description;
            return this;
        }

        /// <summary>
        /// Sets shortcut text that can be typed or pressed to select this option.
        /// </summary>
        /// <param name="shortcut">Shortcut text for this option.</param>
        /// <returns>The current <see cref="SelectOption"/> instance.</returns>
        public SelectOption UseShortcut(string shortcut)
        {
            Shortcut = shortcut;
            return this;
        }

        /// <summary>
        /// Sets the display color for this option.
        /// </summary>
        /// <param name="color">The option foreground color.</param>
        /// <returns>The current <see cref="SelectOption"/> instance.</returns>
        public SelectOption UseColor(ConsoleColor color)
        {
            Color = color;
            Decoration = null;
            ForegroundRgb = null;
            BackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the display color for this option from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The option foreground color.</param>
        /// <returns>The current <see cref="SelectOption"/> instance.</returns>
        public SelectOption UseColor(SpectreColor color)
        {
            return UseColor(color.ToRgbColor());
        }

        /// <summary>
        /// Sets the display color for this option from an RGB foreground color.
        /// </summary>
        /// <param name="color">The option foreground color.</param>
        /// <returns>The current <see cref="SelectOption"/> instance.</returns>
        public SelectOption UseColor(RgbColor color)
        {
            Color = color.ToConsoleColor();
            Decoration = ConsoulLibrary.Decoration.None;
            ForegroundRgb = color;
            BackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the display color for this option from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="SelectOption"/> instance.</returns>
        public SelectOption UseStyle(Style style)
        {
            ApplyStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the display color for this option from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="SelectOption"/> instance.</returns>
        public SelectOption UseStyle(string style)
        {
            return UseStyle(ConsoulLibrary.Style.Parse(style));
        }

        /// <summary>
        /// Sets the display color used when this option is disabled.
        /// </summary>
        /// <param name="color">The disabled option foreground color.</param>
        /// <returns>The current <see cref="SelectOption"/> instance.</returns>
        public SelectOption UseDisabledColor(ConsoleColor color)
        {
            DisabledColor = color;
            DisabledDecoration = null;
            DisabledForegroundRgb = null;
            DisabledBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the display color used when this option is disabled from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The disabled option foreground color.</param>
        /// <returns>The current <see cref="SelectOption"/> instance.</returns>
        public SelectOption UseDisabledColor(SpectreColor color)
        {
            return UseDisabledColor(color.ToRgbColor());
        }

        /// <summary>
        /// Sets the display color used when this option is disabled from an RGB foreground color.
        /// </summary>
        /// <param name="color">The disabled option foreground color.</param>
        /// <returns>The current <see cref="SelectOption"/> instance.</returns>
        public SelectOption UseDisabledColor(RgbColor color)
        {
            DisabledColor = color.ToConsoleColor();
            DisabledDecoration = ConsoulLibrary.Decoration.None;
            DisabledForegroundRgb = color;
            DisabledBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the display color used when this option is disabled from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="SelectOption"/> instance.</returns>
        public SelectOption UseDisabledStyle(Style style)
        {
            ApplyDisabledStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the display color used when this option is disabled from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>red</c>, <c>grey</c>, <c>#ef4444</c>, or <c>white on red</c>.</param>
        /// <returns>The current <see cref="SelectOption"/> instance.</returns>
        public SelectOption UseDisabledStyle(string style)
        {
            return UseDisabledStyle(ConsoulLibrary.Style.Parse(style));
        }

        /// <summary>
        /// Adds hidden search terms that can match filter text without changing the rendered label.
        /// </summary>
        /// <param name="searchTerms">Search terms or aliases for this option.</param>
        /// <returns>The current <see cref="SelectOption"/> instance.</returns>
        public SelectOption AddSearchTerms(params string[] searchTerms)
        {
            return AddSearchTerms((IEnumerable<string>)searchTerms);
        }

        /// <summary>
        /// Adds hidden search terms that can match filter text without changing the rendered label.
        /// </summary>
        /// <param name="searchTerms">Search terms or aliases for this option.</param>
        /// <returns>The current <see cref="SelectOption"/> instance.</returns>
        public SelectOption AddSearchTerms(IEnumerable<string> searchTerms)
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
        /// Adds a child choice beneath this option when the option is owned by a prompt.
        /// </summary>
        /// <param name="label">The child choice label.</param>
        /// <param name="color">Optional child choice foreground color.</param>
        /// <param name="isDefault">Whether the child should be treated as the default selection.</param>
        /// <param name="renderStyle">The render style used for the child option.</param>
        /// <returns>The added child option.</returns>
        public SelectOption AddChild(string label, ConsoleColor? color = null, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            if (_childFactory == null)
            {
                throw new InvalidOperationException("Child options can only be added to options returned by a prompt.");
            }

            return _childFactory(this, label, color, isDefault, renderStyle);
        }

        /// <summary>
        /// Marks this option as displayed but unavailable for selection.
        /// </summary>
        /// <param name="reason">Optional text explaining why the option cannot be selected.</param>
        /// <returns>The current <see cref="SelectOption"/> instance.</returns>
        public SelectOption Disable(string reason = null)
        {
            IsDisabled = true;
            DisabledReason = reason;
            IsDefault = false;
            Selected = false;
            return this;
        }

        /// <summary>
        /// Creates a display-only separator for grouped prompt options.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="color">Optional separator foreground color.</param>
        /// <returns>A configured separator option.</returns>
        public static SelectOption Separator(string label = null, ConsoleColor? color = null)
        {
            return new SelectOption(label ?? string.Empty, color ?? RenderOptions.SubnoteColor, false, OptionRenderStyle.Separator)
            {
                Index = -1,
                IsSeparator = true
            };
        }

        /// <summary>
        /// Creates a display-only separator for grouped prompt options using a Spectre-style color facade.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="color">The separator foreground color.</param>
        /// <returns>A configured separator option.</returns>
        public static SelectOption Separator(string label, SpectreColor color)
        {
            return Separator(label, color.ToRgbColor());
        }

        /// <summary>
        /// Creates a display-only separator for grouped prompt options using an RGB foreground color.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="color">The separator foreground color.</param>
        /// <returns>A configured separator option.</returns>
        public static SelectOption Separator(string label, RgbColor color)
        {
            return Separator(label, (ConsoleColor?)color.ToConsoleColor()).UseColor(color);
        }

        /// <summary>
        /// Creates a display-only separator for grouped prompt options using a reusable style object.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="style">Optional separator style.</param>
        /// <returns>A configured separator option.</returns>
        public static SelectOption Separator(string label, Style style)
        {
            SelectOption option = Separator(label, style == null ? (ConsoleColor?)null : style.ToForegroundColor(RenderOptions.SubnoteColor));
            option.ApplyStyle(style);
            return option;
        }

        /// <summary>
        /// Creates a display-only separator for grouped prompt options using Spectre-style style text.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="style">Optional separator style text.</param>
        /// <returns>A configured separator option.</returns>
        public static SelectOption SeparatorStyled(string label = null, string style = null)
        {
            return Separator(label, string.IsNullOrWhiteSpace(style) ? null : ConsoulLibrary.Style.Parse(style));
        }

        internal void AttachChildFactory(Func<SelectOption, string, ConsoleColor?, bool, OptionRenderStyle, SelectOption> childFactory)
        {
            _childFactory = childFactory;
        }

        internal PromptRenderHelpers.StyledText ResolveStyle()
        {
            return PromptRenderHelpers.FromComponents(
                Color,
                Decoration ?? ConsoulLibrary.Decoration.None,
                ForegroundRgb,
                BackgroundRgb);
        }

        internal PromptRenderHelpers.StyledText ResolveDisabledStyle(Style fallbackStyle)
        {
            PromptRenderHelpers.StyledText style = PromptRenderHelpers.FromComponents(
                DisabledColor,
                DisabledDecoration ?? ConsoulLibrary.Decoration.None,
                DisabledForegroundRgb,
                DisabledBackgroundRgb);

            return fallbackStyle == null
                ? style
                : PromptRenderHelpers.ResolveStyle(fallbackStyle, style.Scheme.Color);
        }

        internal void AddChildOption(SelectOption child)
        {
            if (child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            IsGroup = true;
            child.Parent = this;
            child.Depth = Depth + 1;
            _children.Add(child);
        }

        private void ApplyStyle(Style style)
        {
            if (style == null)
            {
                return;
            }

            Color = style.ToForegroundColor(Color);
            Decoration = style.Decoration;
            ForegroundRgb = style.ForegroundRgb;
            BackgroundRgb = style.BackgroundRgb;
        }

        private void ApplyDisabledStyle(Style style)
        {
            if (style == null)
            {
                return;
            }

            DisabledColor = style.ToForegroundColor(DisabledColor);
            DisabledDecoration = style.Decoration;
            DisabledForegroundRgb = style.ForegroundRgb;
            DisabledBackgroundRgb = style.BackgroundRgb;
        }

        /// <summary>
        /// Returns a string that represents the current <see cref="SelectOption"/>, formatted based on the rendering style.
        /// </summary>
        /// <returns>A string representation of the current <see cref="SelectOption"/>.</returns>
        public override string ToString()
        {
            if (IsSeparator)
            {
                return string.IsNullOrWhiteSpace(Label)
                    ? string.Empty
                    : "-- " + Label.Trim() + " --";
            }

            string suffix = (IsDefault ? "\t(default) " : string.Empty);
            if (!string.IsNullOrWhiteSpace(Shortcut))
            {
                suffix += "\t[" + Shortcut.Trim() + "] ";
            }

            if (IsDisabled)
            {
                suffix += string.IsNullOrWhiteSpace(DisabledReason)
                    ? "\t(disabled) "
                    : "\t(disabled: " + DisabledReason + ") ";
            }

            string indent = Depth <= 0 ? string.Empty : new string(' ', Depth * 2);
            string formattedLabel = $"{Index + 1}) {Label}{suffix}";
            switch (Style)
            {
                case OptionRenderStyle.Checkbox:
                    return $"{indent}[{(Selected ? "x" : " ")}] - {formattedLabel}";
                case OptionRenderStyle.Separator:
                    return string.IsNullOrWhiteSpace(Label) ? string.Empty : "-- " + Label.Trim() + " --";
                default:
                    return indent + formattedLabel;
            }
        }
    }

    /// <summary>
    /// Specifies the expected rendering style of each item in a selection list.
    /// </summary>
    public enum OptionRenderStyle
    {
        /// <summary>
        /// Item rendered as an incrementing number.
        /// </summary>
        Indexable = 0,

        /// <summary>
        /// Item rendered as a checkbox, indicating whether it is selected.
        /// </summary>
        Checkbox = 1,

        /// <summary>
        /// Item rendered as a non-selectable separator.
        /// </summary>
        Separator = 2
    }
}
