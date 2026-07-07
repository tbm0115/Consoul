using System;
using System.Threading;
using System.Threading.Tasks;

using System.Collections.Generic;
using System.Linq;
using ConsoulLibrary.Color;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a dynamic option for a console view, which can hold a customizable entry of type <typeparamref name="T"/>.
    /// This class is designed for dynamically created options, allowing flexible configuration of display and action behavior.
    /// </summary>
    /// <typeparam name="T">The type of data represented by the dynamic entry.</typeparam>
    public class DynamicOption<T> : IViewOption
    {
        private readonly List<string> _searchTerms = new List<string>();
        private Func<bool> _isEnabled = () => true;
        private Func<string> _disabledReason = () => null;
        private Func<string> _description = () => null;
        private Func<string> _shortcut = () => null;
        private Func<IEnumerable<string>> _dynamicSearchTerms = () => Enumerable.Empty<string>();
        private Func<PromptRenderHelpers.StyledText> _style;

        /// <summary>
        /// Gets or sets the dynamic entry associated with this option.
        /// The entry includes properties such as message and color that can be updated dynamically.
        /// </summary>
        public DynamicEntry<T> Entry { get; set; }

        /// <summary>
        /// Gets or sets the callback action that will be executed when this option is selected.
        /// </summary>
        public ViewOptionCallback Action { get; set; }

        /// <summary>
        /// Gets or sets the asynchronous callback action that will be executed when this option is selected.
        /// </summary>
        public Func<CancellationToken, Task> AsyncAction { get; set; }

        /// <summary>
        /// Gets a value indicating whether the option can be selected in the current render pass.
        /// </summary>
        public bool IsEnabled => _isEnabled == null || _isEnabled();

        /// <summary>
        /// Gets text explaining why the option cannot be selected in the current render pass.
        /// </summary>
        public string DisabledReason => _disabledReason != null ? _disabledReason() : null;

        /// <summary>
        /// Gets optional helper text rendered under the option in view prompts.
        /// </summary>
        public string Description => _description != null ? _description() : null;

        /// <summary>
        /// Gets optional shortcut text that can select this option in view prompts.
        /// </summary>
        public string Shortcut => _shortcut != null ? _shortcut() : null;

        /// <summary>
        /// Gets hidden search terms that can match prompt filtering without changing the rendered label.
        /// </summary>
        public IEnumerable<string> SearchTerms => _searchTerms.Concat(_dynamicSearchTerms() ?? Enumerable.Empty<string>());

        /// <summary>
        /// Gets or sets a value indicating whether this option is a display-only separator.
        /// </summary>
        public bool IsSeparator { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicOption{T}"/> class with specified message and action.
        /// Optionally, the color expression for the entry can also be set.
        /// </summary>
        /// <param name="messageExpression">The delegate expression to define the display message for this option.</param>
        /// <param name="action">The callback action to execute when this option is selected.</param>
        /// <param name="colorExpression">Optional parameter to define the color of the entry dynamically.</param>
        public DynamicOption(SetViewOptionMessage messageExpression, ViewOptionCallback action, SetViewOptionColor colorExpression = null)
        {
            // Initialize a new dynamic entry with the given message and optional color expression
            Entry = new DynamicEntry<T>(messageExpression, colorExpression);

            // Assign the provided action to execute when the option is selected
            Action = action ?? delegate { };
            AsyncAction = cancellationToken =>
            {
                Action.Invoke();
                return Task.CompletedTask;
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicOption{T}"/> class with an asynchronous action.
        /// </summary>
        /// <param name="messageExpression">The delegate expression to define the display message for this option.</param>
        /// <param name="action">The asynchronous callback action to execute when this option is selected.</param>
        /// <param name="colorExpression">Optional parameter to define the color of the entry dynamically.</param>
        public DynamicOption(SetViewOptionMessage messageExpression, Func<CancellationToken, Task> action, SetViewOptionColor colorExpression = null)
        {
            Entry = new DynamicEntry<T>(messageExpression, colorExpression);
            AsyncAction = action ?? (cancellationToken => Task.CompletedTask);
            Action = () => AsyncAction(CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Configures a predicate that controls whether the option is selectable.
        /// </summary>
        /// <param name="isEnabled">Predicate that returns <c>true</c> when the option can be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <returns>The current <see cref="DynamicOption{T}"/> instance.</returns>
        public DynamicOption<T> EnableWhen(Func<bool> isEnabled, Func<string> disabledReason = null)
        {
            _isEnabled = isEnabled ?? throw new ArgumentNullException(nameof(isEnabled));
            _disabledReason = disabledReason ?? (() => null);
            return this;
        }

        /// <summary>
        /// Configures a predicate that controls when the option is displayed as disabled.
        /// </summary>
        /// <param name="isDisabled">Predicate that returns <c>true</c> when the option cannot be selected.</param>
        /// <param name="disabledReason">Optional reason rendered when the option is unavailable.</param>
        /// <returns>The current <see cref="DynamicOption{T}"/> instance.</returns>
        public DynamicOption<T> DisableWhen(Func<bool> isDisabled, Func<string> disabledReason = null)
        {
            if (isDisabled == null)
            {
                throw new ArgumentNullException(nameof(isDisabled));
            }

            return EnableWhen(() => !isDisabled(), disabledReason);
        }

        /// <summary>
        /// Configures helper text rendered under the option in view prompts.
        /// </summary>
        /// <param name="description">The helper text to render under the option.</param>
        /// <returns>The current <see cref="DynamicOption{T}"/> instance.</returns>
        public DynamicOption<T> Describe(string description)
        {
            _description = () => description;
            return this;
        }

        /// <summary>
        /// Configures dynamic helper text rendered under the option in view prompts.
        /// </summary>
        /// <param name="description">Function that creates helper text for the current render pass.</param>
        /// <returns>The current <see cref="DynamicOption{T}"/> instance.</returns>
        public DynamicOption<T> Describe(Func<string> description)
        {
            _description = description ?? throw new ArgumentNullException(nameof(description));
            return this;
        }

        /// <summary>
        /// Configures shortcut text that can select this option in view prompts.
        /// </summary>
        /// <param name="shortcut">Shortcut text for this option.</param>
        /// <returns>The current <see cref="DynamicOption{T}"/> instance.</returns>
        public DynamicOption<T> UseShortcut(string shortcut)
        {
            _shortcut = () => shortcut;
            return this;
        }

        /// <summary>
        /// Configures dynamic shortcut text that can select this option in view prompts.
        /// </summary>
        /// <param name="shortcut">Function that creates shortcut text for the current render pass.</param>
        /// <returns>The current <see cref="DynamicOption{T}"/> instance.</returns>
        public DynamicOption<T> UseShortcut(Func<string> shortcut)
        {
            _shortcut = shortcut ?? throw new ArgumentNullException(nameof(shortcut));
            return this;
        }

        /// <summary>
        /// Adds hidden search terms that can match prompt filtering without changing the rendered label.
        /// </summary>
        /// <param name="searchTerms">Search terms or aliases for this option.</param>
        /// <returns>The current <see cref="DynamicOption{T}"/> instance.</returns>
        public DynamicOption<T> AddSearchTerms(params string[] searchTerms)
        {
            return AddSearchTerms((IEnumerable<string>)searchTerms);
        }

        /// <summary>
        /// Adds hidden search terms that can match prompt filtering without changing the rendered label.
        /// </summary>
        /// <param name="searchTerms">Search terms or aliases for this option.</param>
        /// <returns>The current <see cref="DynamicOption{T}"/> instance.</returns>
        public DynamicOption<T> AddSearchTerms(IEnumerable<string> searchTerms)
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
        /// Configures dynamic hidden search terms that can match prompt filtering without changing the rendered label.
        /// </summary>
        /// <param name="searchTerms">Function that creates search terms for the current render pass.</param>
        /// <returns>The current <see cref="DynamicOption{T}"/> instance.</returns>
        public DynamicOption<T> UseSearchTerms(Func<IEnumerable<string>> searchTerms)
        {
            _dynamicSearchTerms = searchTerms ?? throw new ArgumentNullException(nameof(searchTerms));
            return this;
        }

        /// <summary>
        /// Configures the option color using style text.
        /// </summary>
        /// <param name="style">A style string such as <c>green</c>, <c>grey</c>, <c>aqua</c>, or <c>#22c55e</c>.</param>
        /// <returns>The current <see cref="DynamicOption{T}"/> instance.</returns>
        public DynamicOption<T> UseStyle(string style)
        {
            Style parsed = ParseStyle(style);
            ApplyStyle(parsed, RenderOptions.OptionColor);
            return this;
        }

        /// <summary>
        /// Configures the option color using a reusable style.
        /// </summary>
        /// <param name="style">Optional foreground style for the option label.</param>
        /// <returns>The current <see cref="DynamicOption{T}"/> instance.</returns>
        public DynamicOption<T> UseStyle(Style style)
        {
            ApplyStyle(style, RenderOptions.OptionColor);
            return this;
        }

        /// <summary>
        /// Configures the option color using a Spectre-style color facade.
        /// </summary>
        /// <param name="color">Color facade mapped to the nearest console color for the option label.</param>
        /// <returns>The current <see cref="DynamicOption{T}"/> instance.</returns>
        public DynamicOption<T> UseColor(SpectreColor color)
        {
            return UseColor(color.ToRgbColor());
        }

        /// <summary>
        /// Configures the option color using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="color">RGB color mapped to the nearest console color for the option label.</param>
        /// <returns>The current <see cref="DynamicOption{T}"/> instance.</returns>
        public DynamicOption<T> UseColor(RgbColor color)
        {
            ApplyColor(color);
            return this;
        }

        /// <summary>
        /// Configures the option color using a Spectre-style color facade evaluated each render pass.
        /// </summary>
        /// <param name="color">Function that creates the option color for the current render pass.</param>
        /// <returns>The current <see cref="DynamicOption{T}"/> instance.</returns>
        public DynamicOption<T> UseColor(Func<SpectreColor> color)
        {
            if (color == null)
            {
                throw new ArgumentNullException(nameof(color));
            }

            return UseColor(() => color().ToRgbColor());
        }

        /// <summary>
        /// Configures the option color using an RGB color evaluated each render pass.
        /// </summary>
        /// <param name="color">Function that creates the option color for the current render pass.</param>
        /// <returns>The current <see cref="DynamicOption{T}"/> instance.</returns>
        public DynamicOption<T> UseColor(Func<RgbColor> color)
        {
            if (color == null)
            {
                throw new ArgumentNullException(nameof(color));
            }

            Entry.SetColor = () => color().ToConsoleColor();
            _style = () => ResolveStyledText(new Style(color()), RenderOptions.OptionColor);
            return this;
        }

        /// <summary>
        /// Configures the option color using style text evaluated each render pass.
        /// </summary>
        /// <param name="style">Function that creates style text for the current render pass.</param>
        /// <returns>The current <see cref="DynamicOption{T}"/> instance.</returns>
        public DynamicOption<T> UseStyle(Func<string> style)
        {
            if (style == null)
            {
                throw new ArgumentNullException(nameof(style));
            }

            Entry.SetColor = () => ResolveStyleColor(ParseStyle(style()), RenderOptions.OptionColor);
            _style = () => ResolveStyledText(ParseStyle(style()), RenderOptions.OptionColor);
            return this;
        }

        /// <summary>
        /// Configures the option color using a reusable style evaluated each render pass.
        /// </summary>
        /// <param name="style">Function that creates a style for the current render pass.</param>
        /// <returns>The current <see cref="DynamicOption{T}"/> instance.</returns>
        public DynamicOption<T> UseStyle(Func<Style> style)
        {
            if (style == null)
            {
                throw new ArgumentNullException(nameof(style));
            }

            Entry.SetColor = () => ResolveStyleColor(style(), RenderOptions.OptionColor);
            _style = () => ResolveStyledText(style(), RenderOptions.OptionColor);
            return this;
        }

        /// <summary>
        /// Creates a display-only separator for grouping dynamic view options.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="color">Optional separator foreground color.</param>
        /// <returns>A configured separator option.</returns>
        public static DynamicOption<T> Separator(string label = null, ConsoleColor? color = null)
        {
            return new DynamicOption<T>(
                () => label ?? string.Empty,
                cancellationToken => Task.CompletedTask,
                () => color ?? RenderOptions.SubnoteColor)
            {
                IsSeparator = true
            };
        }

        /// <summary>
        /// Creates a display-only separator for grouping dynamic view options using style text.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="style">Optional separator foreground style.</param>
        /// <returns>A configured separator option.</returns>
        public static DynamicOption<T> Separator(string label, string style)
        {
            return Separator(label, ParseStyle(style));
        }

        /// <summary>
        /// Creates a display-only separator for grouping dynamic view options using a reusable style.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="style">Optional separator foreground style.</param>
        /// <returns>A configured separator option.</returns>
        public static DynamicOption<T> Separator(string label, Style style)
        {
            DynamicOption<T> option = Separator(label, ResolveStyleColor(style, RenderOptions.SubnoteColor));
            option.ApplyStyle(style, RenderOptions.SubnoteColor);
            return option;
        }

        /// <summary>
        /// Creates a display-only separator for grouping dynamic view options using a Spectre-style color facade.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="color">Color facade mapped to the nearest console color for the separator label.</param>
        /// <returns>A configured separator option.</returns>
        public static DynamicOption<T> Separator(string label, SpectreColor color)
        {
            return Separator(label, color.ToRgbColor());
        }

        /// <summary>
        /// Creates a display-only separator for grouping dynamic view options using an RGB color mapped to Consoul's console palette.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="color">RGB color mapped to the nearest console color for the separator label.</param>
        /// <returns>A configured separator option.</returns>
        public static DynamicOption<T> Separator(string label, RgbColor color)
        {
            DynamicOption<T> option = Separator(label, (ConsoleColor?)color.ToConsoleColor());
            option.ApplyColor(color);
            return option;
        }

        /// <summary>
        /// Creates a display-only separator for grouping dynamic view options using style text.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="style">Optional separator foreground style.</param>
        /// <returns>A configured separator option.</returns>
        public static DynamicOption<T> SeparatorStyled(string label = null, string style = null)
        {
            return Separator(label, ParseStyle(style));
        }

        /// <summary>
        /// Invokes the option action.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the option action.</returns>
        public Task InvokeAsync(CancellationToken cancellationToken = default)
        {
            return AsyncAction != null ? AsyncAction(cancellationToken) : Task.CompletedTask;
        }

        internal ViewHelpItem ToHelpItem(string label, string disabledReason = null)
        {
            PromptRenderHelpers.StyledText style = ResolveStyle();
            return new ViewHelpItem(
                label,
                Description,
                Shortcut,
                disabledReason,
                IsSeparator,
                style.Scheme,
                style.Decoration,
                style.ForegroundRgb,
                style.BackgroundRgb);
        }

        internal void ApplyStyleTo(SelectOption option, bool disabled = false)
        {
            if (option == null)
            {
                return;
            }

            PromptRenderHelpers.StyledText style = ResolveStyle();
            option.Color = style.Scheme.Color;
            option.Decoration = style.Decoration;
            option.ForegroundRgb = style.ForegroundRgb;
            option.BackgroundRgb = style.BackgroundRgb;

            if (disabled || option.IsDisabled)
            {
                option.DisabledColor = style.Scheme.Color;
                option.DisabledDecoration = style.Decoration;
                option.DisabledForegroundRgb = style.ForegroundRgb;
                option.DisabledBackgroundRgb = style.BackgroundRgb;
            }
        }

        internal PromptRenderHelpers.StyledText ResolveStyle()
        {
            return _style == null ? PromptRenderHelpers.FromColor(Entry.SetColor()) : _style();
        }

        private void ApplyColor(RgbColor color)
        {
            Entry.SetColor = () => color.ToConsoleColor();
            _style = () => PromptRenderHelpers.FromComponents(
                color.ToConsoleColor(),
                global::ConsoulLibrary.Decoration.None,
                color,
                null);
        }

        private void ApplyStyle(Style style, ConsoleColor fallback)
        {
            Entry.SetColor = () => ResolveStyleColor(style, fallback);
            _style = () => ResolveStyledText(style, fallback);
        }

        private static Style ParseStyle(string style)
        {
            return string.IsNullOrWhiteSpace(style) ? null : Style.Parse(style);
        }

        private static ConsoleColor ResolveStyleColor(Style style, ConsoleColor fallback)
        {
            return style == null ? fallback : style.ToForegroundColor(fallback);
        }

        private static PromptRenderHelpers.StyledText ResolveStyledText(Style style, ConsoleColor fallback)
        {
            return PromptRenderHelpers.ResolveStyle(style, fallback);
        }
    }
}
