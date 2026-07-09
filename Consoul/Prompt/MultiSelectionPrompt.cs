using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a prompt that lets the user select multiple options by number, comma-separated lists, or ranges.
    /// </summary>
    public class MultiSelectionPrompt : IPrompt<MultiSelectionResult>
    {
        private readonly List<SelectOption> _options = new List<SelectOption>();
        private readonly HashSet<string> _defaultLabels = new HashSet<string>(StringComparer.Ordinal);
        private bool _selectAllByDefault;
        private ConsoleColor _promptColor = RenderOptions.PromptColor;
        private ConsoleColor _instructionColor = RenderOptions.SubnoteColor;
        private ConsoleColor _optionDescriptionColor = RenderOptions.SubnoteColor;
        private ConsoleColor _keyboardSelectionColor = RenderOptions.PromptColor;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiSelectionPrompt"/> class with a message and optional behavior flags.
        /// </summary>
        /// <param name="message">The message to display when the prompt is shown.</param>
        /// <param name="clear">Indicates whether to clear the console when the prompt is displayed.</param>
        /// <param name="allowEmpty">Indicates whether an empty selection is valid.</param>
        public MultiSelectionPrompt(string message, bool clear = false, bool allowEmpty = false)
        {
            Message = message;
            ClearConsole = clear;
            AllowEmpty = allowEmpty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiSelectionPrompt"/> class with initial string options.
        /// </summary>
        /// <param name="message">The message to display when the prompt is shown.</param>
        /// <param name="clear">Indicates whether to clear the console when the prompt is displayed.</param>
        /// <param name="allowEmpty">Indicates whether an empty selection is valid.</param>
        /// <param name="options">Initial options to add to the prompt.</param>
        public MultiSelectionPrompt(string message, bool clear = false, bool allowEmpty = false, params string[] options) : this(message, clear, allowEmpty)
        {
            foreach (string option in options ?? new string[0])
            {
                Add(option);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiSelectionPrompt"/> class with an empty title.
        /// </summary>
        public MultiSelectionPrompt() : this(string.Empty, false, false)
        {
        }

        /// <summary>
        /// Shows the prompt through the specified console facade.
        /// </summary>
        /// <param name="console">The console facade used to render the prompt.</param>
        /// <returns>The multi-selection result.</returns>
        public MultiSelectionResult Show(IAnsiConsole console)
        {
            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            return console.Prompt(this);
        }

        /// <summary>
        /// Shows the prompt asynchronously through the specified console facade.
        /// </summary>
        /// <param name="console">The console facade used to render the prompt.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that resolves to the multi-selection result.</returns>
        public Task<MultiSelectionResult> ShowAsync(IAnsiConsole console, CancellationToken cancellationToken = default)
        {
            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            return Task.FromResult(console.Prompt(this, cancellationToken));
        }

        /// <summary>
        /// Sets the prompt title/message using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="title">The prompt title shown to the user.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt Title(string title)
        {
            Message = title ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message.
        /// </summary>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt PromptStyle(ConsoleColor color)
        {
            PromptColor = color;
            PromptDecoration = Decoration.None;
            PromptForegroundRgb = null;
            PromptBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt PromptStyle(SpectreColor color)
        {
            return PromptStyle(color.ToRgbColor());
        }

        /// <summary>
        /// Sets the color used for the prompt message from an RGB foreground color.
        /// </summary>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt PromptStyle(RgbColor color)
        {
            PromptColor = color.ToConsoleColor();
            PromptDecoration = Decoration.None;
            PromptForegroundRgb = color;
            PromptBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt PromptStyle(Style style)
        {
            ApplyPromptStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt PromptStyle(string style)
        {
            return PromptStyle(ConsoulLibrary.Style.Parse(style));
        }

        /// <summary>
        /// Sets the color used for the prompt message.
        /// </summary>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt Style(ConsoleColor color)
        {
            return PromptStyle(color);
        }

        /// <summary>
        /// Sets the color used for the prompt message from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt Style(Style style)
        {
            return PromptStyle(style);
        }

        /// <summary>
        /// Sets the color used for the prompt message from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt Style(string style)
        {
            return PromptStyle(style);
        }

        /// <summary>
        /// Sets the helper text shown above the options when line-based input is used.
        /// </summary>
        /// <param name="text">The instruction text.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt InstructionsText(string text)
        {
            InstructionsMessage = text ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the color used for prompt instruction text.
        /// </summary>
        /// <param name="color">The instruction text color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt InstructionStyle(ConsoleColor color)
        {
            InstructionColor = color;
            InstructionDecoration = Decoration.None;
            InstructionForegroundRgb = null;
            InstructionBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the color used for prompt instruction text from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The instruction text color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt InstructionStyle(SpectreColor color)
        {
            return InstructionStyle(color.ToRgbColor());
        }

        /// <summary>
        /// Sets the color used for prompt instruction text from an RGB foreground color.
        /// </summary>
        /// <param name="color">The instruction text color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt InstructionStyle(RgbColor color)
        {
            InstructionColor = color.ToConsoleColor();
            InstructionDecoration = Decoration.None;
            InstructionForegroundRgb = color;
            InstructionBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the color used for prompt instruction text from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt InstructionStyle(Style style)
        {
            ApplyInstructionStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the color used for prompt instruction text from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#94a3b8</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt InstructionStyle(string style)
        {
            return InstructionStyle(ConsoulLibrary.Style.Parse(style));
        }

        /// <summary>
        /// Sets the color used for option descriptions.
        /// </summary>
        /// <param name="color">The option description foreground color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt OptionDescriptionStyle(ConsoleColor color)
        {
            OptionDescriptionColor = color;
            return this;
        }

        /// <summary>
        /// Sets the color used for option descriptions from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The option description foreground color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt OptionDescriptionStyle(SpectreColor color)
        {
            return OptionDescriptionStyle(color.ToRgbColor());
        }

        /// <summary>
        /// Sets the color used for option descriptions from an RGB foreground color.
        /// </summary>
        /// <param name="color">The option description foreground color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt OptionDescriptionStyle(RgbColor color)
        {
            OptionDescriptionColor = color.ToConsoleColor();
            OptionDescriptionDecoration = Decoration.None;
            OptionDescriptionForegroundRgb = color;
            OptionDescriptionBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the style used for option descriptions.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt OptionDescriptionStyle(Style style)
        {
            ApplyOptionDescriptionStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the style used for option descriptions from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>italic</c>, <c>#94a3b8</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt OptionDescriptionStyle(string style)
        {
            return OptionDescriptionStyle(ConsoulLibrary.Style.Parse(style));
        }

        /// <summary>
        /// Adds multiple string choices using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="choices">The choices to add.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt AddChoices(params string[] choices)
        {
            return AddChoices((IEnumerable<string>)choices);
        }

        /// <summary>
        /// Adds multiple string choices using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="choices">The choices to add.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt AddChoices(IEnumerable<string> choices)
        {
            foreach (string choice in choices ?? new string[0])
            {
                Add(choice);
            }

            return this;
        }

        /// <summary>
        /// Adds a display-only group heading followed by string choices using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="group">The group heading text.</param>
        /// <param name="choices">The choices to add under the group.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt AddChoiceGroup(string group, params string[] choices)
        {
            return AddChoiceGroup(group, (IEnumerable<string>)choices);
        }

        /// <summary>
        /// Adds a display-only group heading followed by string choices using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="group">The group heading text.</param>
        /// <param name="choices">The choices to add under the group.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt AddChoiceGroup(string group, IEnumerable<string> choices)
        {
            AddSeparator(group);
            return AddChoices(choices);
        }

        /// <summary>
        /// Adds one string choice using Spectre-style prompt vocabulary.
        /// </summary>
        /// <param name="choice">The choice label to add.</param>
        /// <returns>The added <see cref="SelectOption"/>.</returns>
        public SelectOption AddChoice(string choice)
        {
            int index = Add(choice);
            return GetOptionByIndex(index);
        }

        /// <summary>
        /// Adds a grouped choice that can contain child options.
        /// </summary>
        /// <param name="label">The group label.</param>
        /// <param name="color">Optional group foreground color.</param>
        /// <param name="isDefault">Whether the group is selected by default when groups are independently selectable.</param>
        /// <returns>The added group option.</returns>
        public SelectOption AddGroup(string label, ConsoleColor? color = null, bool isDefault = false)
        {
            SelectOption group = AddChoice(label);
            group.Color = color ?? group.Color;
            group.IsDefault = isDefault;
            group.Selected = isDefault;
            group.IsGroup = true;
            return group;
        }

        /// <summary>
        /// Adds a child choice beneath an existing grouped option.
        /// </summary>
        /// <param name="parent">The prompt-owned group option that should contain the child.</param>
        /// <param name="label">The child choice label.</param>
        /// <param name="color">Optional child foreground color.</param>
        /// <param name="isDefault">Whether the child is selected by default.</param>
        /// <param name="renderStyle">The render style used for the child option.</param>
        /// <returns>The added child option.</returns>
        public SelectOption AddChild(SelectOption parent, string label, ConsoleColor? color = null, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Checkbox)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            if (!_options.Contains(parent) || parent.IsSeparator)
            {
                throw new ArgumentException("Child options can only be added to prompt-owned selectable options.", nameof(parent));
            }

            bool selectedByDefault = isDefault || ShouldSelectOptionByDefault(label);
            var child = new SelectOption(GetNextOptionIndex(), label, color ?? RenderOptions.OptionColor, selectedByDefault, renderStyle)
            {
                Selected = selectedByDefault
            };
            AttachChildFactory(child);
            parent.AddChildOption(child);
            _options.Add(child);
            return child;
        }

        /// <summary>
        /// Enables slash-command filtering for this prompt.
        /// </summary>
        /// <param name="enabled">Whether filtering should be enabled.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt SearchEnabled(bool enabled = true)
        {
            FilterEnabled = enabled;
            return this;
        }

        /// <summary>
        /// Enables slash-command filtering for this prompt using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="enabled">Whether filtering should be enabled.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt EnableSearch(bool enabled = true)
        {
            return SearchEnabled(enabled);
        }

        /// <summary>
        /// Enables keyboard navigation for this prompt.
        /// </summary>
        /// <param name="enabled">Whether keyboard navigation should be enabled.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt UseKeyboardNavigation(bool enabled = true)
        {
            KeyboardNavigationEnabled = enabled;
            return this;
        }

        /// <summary>
        /// Sets whether keyboard navigation wraps at the first and last available options.
        /// </summary>
        /// <param name="enabled">Whether arrow-key navigation should wrap around.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt UseWrapAround(bool enabled = true)
        {
            WrapAround = enabled;
            return this;
        }

        /// <summary>
        /// Sets whether the prompt accepts an empty selection.
        /// </summary>
        /// <param name="allowEmpty">Whether an empty selection should be accepted.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt AllowEmptySelection(bool allowEmpty = true)
        {
            AllowEmpty = allowEmpty;
            return this;
        }

        /// <summary>
        /// Sets whether this prompt requires at least one selected option.
        /// </summary>
        /// <param name="required">Whether at least one selected option is required.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt Required(bool required = true)
        {
            AllowEmpty = !required;
            return this;
        }

        /// <summary>
        /// Allows this prompt to return an empty selection.
        /// </summary>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt NotRequired()
        {
            return Required(false);
        }

        /// <summary>
        /// Sets the maximum number of choices shown per page.
        /// </summary>
        /// <param name="pageSize">The maximum number of choices per page, or <see langword="null"/> to render all choices.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt WithPageSize(int? pageSize)
        {
            PageSize = pageSize.HasValue ? Math.Max(1, pageSize.Value) : (int?)null;
            return this;
        }

        /// <summary>
        /// Sets the paging helper text.
        /// </summary>
        /// <param name="text">The paging helper text.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt WithMoreChoicesText(string text)
        {
            MoreChoicesText = text ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the keyboard search placeholder text shown before any filter text has been entered.
        /// </summary>
        /// <param name="text">The placeholder text to render in keyboard search mode.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt WithSearchPlaceholderText(string text)
        {
            SearchPlaceholderText = text;
            return this;
        }

        /// <summary>
        /// Sets the message rendered when submitted input does not match available choices.
        /// </summary>
        /// <param name="message">The invalid selection message.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt WithInvalidSelectionMessage(string message)
        {
            InvalidSelectionMessage = message ?? throw new ArgumentNullException(nameof(message));
            return this;
        }

        /// <summary>
        /// Marks the option with the matching label as the default selection when the user presses Enter without input.
        /// </summary>
        /// <param name="label">The option label that should be treated as the default selection.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt DefaultValue(string label)
        {
            ResetDefaultSelectionState();
            if (label != null)
            {
                _defaultLabels.Add(label);
            }

            foreach (SelectOption option in Options.Where(option => !option.IsSeparator))
            {
                bool isDefault = string.Equals(option.Label, label, StringComparison.Ordinal);
                option.IsDefault = isDefault;
                option.Selected = isDefault;
            }

            return this;
        }

        /// <summary>
        /// Marks one option label as selected by default without clearing existing default selections.
        /// </summary>
        /// <param name="label">The option label that should be selected by default.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt Select(string label)
        {
            return Select(new[] { label });
        }

        /// <summary>
        /// Marks matching option labels as selected by default without clearing existing default selections.
        /// </summary>
        /// <param name="labels">The option labels that should be selected by default.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt Select(params string[] labels)
        {
            return Select((IEnumerable<string>)labels);
        }

        /// <summary>
        /// Marks matching option labels as selected by default without clearing existing default selections.
        /// </summary>
        /// <param name="labels">The option labels that should be selected by default.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt Select(IEnumerable<string> labels)
        {
            if (labels == null)
            {
                return this;
            }

            foreach (string label in labels.Where(label => label != null))
            {
                _defaultLabels.Add(label);
            }

            foreach (SelectOption option in Options.Where(option => ShouldSelectOptionByDefault(option.Label)))
            {
                MarkOptionSelectedByDefault(option);
            }

            SyncGroupSelectionStates();
            return this;
        }

        /// <summary>
        /// Marks every selectable option as selected by default.
        /// </summary>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt SelectAll()
        {
            _selectAllByDefault = true;
            foreach (SelectOption option in Options)
            {
                MarkOptionSelectedByDefault(option);
            }

            SyncGroupSelectionStates();
            return this;
        }

        /// <summary>
        /// Configures the multi-selection result returned when the prompt is canceled.
        /// </summary>
        /// <param name="cancelResult">The fallback result factory.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt OnCancel(Func<MultiSelectionResult> cancelResult)
        {
            CancelResult = cancelResult ?? throw new ArgumentNullException(nameof(cancelResult));
            return this;
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice.
        /// </summary>
        /// <param name="color">The highlight color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt HighlightColor(ConsoleColor color)
        {
            KeyboardSelectionColor = color;
            return this;
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The highlight color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt HighlightColor(SpectreColor color)
        {
            return HighlightColor(color.ToRgbColor());
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice from an RGB foreground color.
        /// </summary>
        /// <param name="color">The highlight color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt HighlightColor(RgbColor color)
        {
            KeyboardSelectionColor = color.ToConsoleColor();
            KeyboardSelectionDecoration = Decoration.None;
            KeyboardSelectionForegroundRgb = color;
            KeyboardSelectionBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt HighlightColor(Style style)
        {
            ApplyKeyboardSelectionStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt HighlightColor(string style)
        {
            return HighlightColor(ConsoulLibrary.Style.Parse(style));
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice.
        /// </summary>
        /// <param name="color">The highlight color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt HighlightStyle(ConsoleColor color)
        {
            return HighlightColor(color);
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt HighlightStyle(Style style)
        {
            return HighlightColor(style);
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt HighlightStyle(string style)
        {
            return HighlightColor(style);
        }

        /// <summary>
        /// Gets or sets the message displayed to the user when the prompt is shown.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to clear the console before each prompt render.
        /// </summary>
        public bool ClearConsole { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an empty selection is valid.
        /// </summary>
        public bool AllowEmpty { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of selectable options rendered per page. Set to <see langword="null"/> to render all options.
        /// </summary>
        public int? PageSize { get; set; }

        /// <summary>
        /// Gets or sets the paging helper text. Placeholders are first option, last option, total options, current page, and total pages.
        /// </summary>
        public string MoreChoicesText { get; set; } = "Showing options {0}-{1} of {2}. Type next/prev to change pages.";

        /// <summary>
        /// Gets or sets the message rendered when submitted input does not match available choices.
        /// </summary>
        public string InvalidSelectionMessage { get; set; } = "Invalid selection!";

        /// <summary>
        /// Gets or sets the color used for the prompt message.
        /// </summary>
        public ConsoleColor PromptColor
        {
            get { return _promptColor; }
            set
            {
                _promptColor = value;
                PromptDecoration = Decoration.None;
                PromptForegroundRgb = null;
                PromptBackgroundRgb = null;
            }
        }

        /// <summary>
        /// Gets or sets the ANSI decoration used for the prompt message.
        /// </summary>
        public Decoration PromptDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for the prompt message on ANSI-capable terminals.
        /// </summary>
        public RgbColor? PromptForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for the prompt message on ANSI-capable terminals.
        /// </summary>
        public RgbColor? PromptBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the helper text shown above the options when line-based input is used.
        /// </summary>
        public string InstructionsMessage { get; set; } = "Choose one or more numbers separated by commas or ranges (for example 1,3-5):";

        /// <summary>
        /// Gets or sets the color used for prompt instruction text.
        /// </summary>
        public ConsoleColor InstructionColor
        {
            get { return _instructionColor; }
            set
            {
                _instructionColor = value;
                InstructionDecoration = Decoration.None;
                InstructionForegroundRgb = null;
                InstructionBackgroundRgb = null;
            }
        }

        /// <summary>
        /// Gets or sets the ANSI decoration used for prompt instruction text.
        /// </summary>
        public Decoration InstructionDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for prompt instruction text on ANSI-capable terminals.
        /// </summary>
        public RgbColor? InstructionForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for prompt instruction text on ANSI-capable terminals.
        /// </summary>
        public RgbColor? InstructionBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether slash commands can filter the rendered options.
        /// </summary>
        public bool FilterEnabled { get; set; }

        /// <summary>
        /// Gets or sets the command prefix used for filtering. The default prefix is <c>/</c>.
        /// </summary>
        public string FilterCommandPrefix { get; set; } = "/";

        /// <summary>
        /// Gets or sets the helper text shown when filtering is enabled. Use <c>{0}</c> for the command prefix.
        /// </summary>
        public string FilterHelpText { get; set; } = "Type {0}text to filter options, or {0}clear to reset.";

        /// <summary>
        /// Gets or sets the text shown when a filter is active. Placeholders are filter text and match count.
        /// </summary>
        public string ActiveFilterText { get; set; } = "Filter: {0} ({1} matches).";

        /// <summary>
        /// Gets or sets the text shown when a filter has no matches. Placeholders are filter text and match count.
        /// </summary>
        public string NoFilterMatchesText { get; set; } = "No options match \"{0}\".";

        /// <summary>
        /// Gets or sets a value indicating whether option descriptions are rendered when present.
        /// </summary>
        public bool ShowOptionDescriptions { get; set; } = true;

        /// <summary>
        /// Gets or sets the text prefix written before option descriptions.
        /// </summary>
        public string OptionDescriptionPrefix { get; set; } = "    ";

        /// <summary>
        /// Gets or sets the color used for option descriptions.
        /// </summary>
        public ConsoleColor OptionDescriptionColor
        {
            get { return _optionDescriptionColor; }
            set
            {
                _optionDescriptionColor = value;
                OptionDescriptionDecoration = Decoration.None;
                OptionDescriptionForegroundRgb = null;
                OptionDescriptionBackgroundRgb = null;
            }
        }

        /// <summary>
        /// Gets or sets the ANSI decoration used for option descriptions.
        /// </summary>
        public Decoration OptionDescriptionDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for option descriptions on ANSI-capable terminals.
        /// </summary>
        public RgbColor? OptionDescriptionForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for option descriptions on ANSI-capable terminals.
        /// </summary>
        public RgbColor? OptionDescriptionBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the prompt uses arrow-key navigation when routine input is not queued.
        /// </summary>
        public bool KeyboardNavigationEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether keyboard navigation wraps at the first and last available options.
        /// </summary>
        public bool WrapAround { get; set; } = true;

        /// <summary>
        /// Gets or sets the helper text shown for keyboard navigation.
        /// </summary>
        public string KeyboardNavigationHelpText { get; set; } = "Use Up/Down to move, Space to toggle, Enter to accept, Esc to cancel.";

        /// <summary>
        /// Gets or sets the helper text shown before keyboard filter text has been entered.
        /// </summary>
        public string KeyboardFilterHelpText { get; set; } = "Type to filter options; Backspace edits the filter.";

        /// <summary>
        /// Gets or sets the keyboard search placeholder text shown before any filter text has been entered.
        /// </summary>
        public string SearchPlaceholderText { get; set; }

        /// <summary>
        /// Gets or sets the color used for the currently highlighted keyboard option.
        /// </summary>
        public ConsoleColor KeyboardSelectionColor
        {
            get { return _keyboardSelectionColor; }
            set
            {
                _keyboardSelectionColor = value;
                KeyboardSelectionDecoration = Decoration.None;
                KeyboardSelectionForegroundRgb = null;
                KeyboardSelectionBackgroundRgb = null;
            }
        }

        /// <summary>
        /// Gets or sets the ANSI decoration used for the currently highlighted keyboard option.
        /// </summary>
        public Decoration KeyboardSelectionDecoration { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB foreground color used for the currently highlighted keyboard option on ANSI-capable terminals.
        /// </summary>
        public RgbColor? KeyboardSelectionForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the optional RGB background color used for the currently highlighted keyboard option on ANSI-capable terminals.
        /// </summary>
        public RgbColor? KeyboardSelectionBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the style used for visible text that matches the active search filter.
        /// </summary>
        public Style SearchHighlightStyle { get; set; }

        /// <summary>
        /// Gets or sets how grouped choices participate in selection results.
        /// </summary>
        public SelectionMode Mode { get; set; } = SelectionMode.Leaf;

        /// <summary>
        /// Gets or sets a fallback result returned when the prompt is canceled.
        /// </summary>
        public Func<MultiSelectionResult> CancelResult { get; set; }

        /// <summary>
        /// Gets the collection of selectable options.
        /// </summary>
        public IEnumerable<SelectOption> Options => _options;

        /// <summary>
        /// Gets the number of selectable or disabled options in the prompt, excluding separators.
        /// </summary>
        public int Count => _options.Count(option => !option.IsSeparator);

        /// <summary>
        /// Gets the label of the selectable <see cref="SelectOption"/> at the specified index.
        /// </summary>
        /// <param name="index">Zero-based selectable index of the <see cref="SelectOption"/>.</param>
        /// <returns>The label of the <see cref="SelectOption"/> at the specified index.</returns>
        public string this[int index] => GetOptionByIndex(index).Label;

        /// <summary>
        /// Adds a new option to the prompt.
        /// </summary>
        /// <param name="label">The label of the option.</param>
        /// <param name="color">The color of the option text in the console.</param>
        /// <param name="isDefault">Indicates whether this option is selected when the user presses Enter without input.</param>
        /// <returns>The index of the newly added option.</returns>
        public int Add(string label, ConsoleColor? color = null, bool isDefault = false)
        {
            bool selectedByDefault = isDefault || ShouldSelectOptionByDefault(label);
            var option = new SelectOption(GetNextOptionIndex(), label, color ?? RenderOptions.OptionColor, selectedByDefault, OptionRenderStyle.Checkbox)
            {
                Selected = selectedByDefault
            };
            AttachChildFactory(option);
            _options.Add(option);
            return option.Index;
        }

        /// <summary>
        /// Adds a new option with helper text rendered under the option.
        /// </summary>
        /// <param name="label">The label of the option.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <param name="color">The color of the option text in the console.</param>
        /// <param name="isDefault">Indicates whether this option is selected when the user presses Enter without input.</param>
        /// <returns>The index of the newly added option.</returns>
        public int AddDescribed(string label, string description, ConsoleColor? color = null, bool isDefault = false)
        {
            int index = Add(label, color, isDefault);
            GetOptionByIndex(index).Description = description;
            return index;
        }

        /// <summary>
        /// Adds a display-only separator to visually group prompt options.
        /// </summary>
        /// <param name="label">Optional separator label.</param>
        /// <param name="color">Optional separator foreground color.</param>
        /// <returns>The added separator.</returns>
        public SelectOption AddSeparator(string label = null, ConsoleColor? color = null)
        {
            SelectOption separator = SelectOption.Separator(label, color);
            _options.Add(separator);
            return separator;
        }

        /// <summary>
        /// Adds an unavailable option that is displayed but cannot be selected.
        /// </summary>
        /// <param name="label">The label of the disabled option.</param>
        /// <param name="reason">Optional text explaining why the option cannot be selected.</param>
        /// <param name="color">Optional foreground color used to render the disabled option.</param>
        /// <returns>The index of the newly added option.</returns>
        public int AddDisabled(string label, string reason = null, ConsoleColor? color = null)
        {
            var option = new SelectOption(GetNextOptionIndex(), label, color ?? ConsoleColor.DarkGray, false, OptionRenderStyle.Checkbox)
            {
                DisabledColor = color ?? ConsoleColor.DarkGray,
                Selected = false
            }.Disable(reason);

            AttachChildFactory(option);
            _options.Add(option);
            return option.Index;
        }

        /// <summary>
        /// Adds an unavailable option with helper text rendered under the option.
        /// </summary>
        /// <param name="label">The label of the disabled option.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <param name="reason">Optional text explaining why the option cannot be selected.</param>
        /// <param name="color">Optional foreground color used to render the disabled option.</param>
        /// <returns>The index of the newly added option.</returns>
        public int AddDisabledDescribed(string label, string description, string reason = null, ConsoleColor? color = null)
        {
            int index = AddDisabled(label, reason, color);
            GetOptionByIndex(index).Description = description;
            return index;
        }

        /// <summary>
        /// Adds hidden search terms to an existing option.
        /// </summary>
        /// <param name="index">Zero-based selectable option index.</param>
        /// <param name="searchTerms">Search terms or aliases for the option.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt AddSearchTerms(int index, params string[] searchTerms)
        {
            GetOptionByIndex(index).AddSearchTerms(searchTerms);
            return this;
        }

        /// <summary>
        /// Adds hidden search terms to an existing option.
        /// </summary>
        /// <param name="index">Zero-based selectable option index.</param>
        /// <param name="searchTerms">Search terms or aliases for the option.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt"/> instance.</returns>
        public MultiSelectionPrompt AddSearchTerms(int index, IEnumerable<string> searchTerms)
        {
            GetOptionByIndex(index).AddSearchTerms(searchTerms);
            return this;
        }

        /// <summary>
        /// Clears all options from the prompt.
        /// </summary>
        public void Clear()
        {
            _options.Clear();
        }

        /// <summary>
        /// Displays the prompt options to the user and waits for one or more selections.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="MultiSelectionResult"/> that describes selected indexes or cancellation.</returns>
        public MultiSelectionResult Render(CancellationToken cancellationToken = default)
        {
            string[] escapePhrases = new string[]
            {
                "go back",
                "back",
                "exit",
                "goback"
            };
            int currentPage = GetInitialPage();
            string filterText = string.Empty;
            if (KeyboardNavigationEnabled && !Routines.HasBuffer())
            {
                return RenderKeyboard(cancellationToken);
            }

            do
            {
                if (ClearConsole)
                {
                    Consoul.ConsoleDriver.Clear();
                }
                int matchCount = GetMatchingOptionCount(filterText);
                int? pageSize = GetEffectivePageSize(filterText);
                currentPage = ClampPage(currentPage, pageSize, filterText);
                RenderPromptFrame(pageSize, currentPage, filterText, matchCount);

                Consoul.ConsoleDriver.ForegroundColor = RenderOptions.DefaultColor;
                string input = Consoul.Read(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return ResolveCancelResult();
                }

                if (escapePhrases.Any(phrase => input.Equals(phrase, StringComparison.OrdinalIgnoreCase)))
                {
                    return ResolveCancelResult();
                }

                if (TryHandleFilterCommand(input, ref filterText, ref currentPage))
                {
                    continue;
                }

                if (TryHandlePagingCommand(input, pageSize, filterText, ref currentPage))
                {
                    continue;
                }

                int[] indexes;
                if (TryParseInput(input, filterText, out indexes))
                {
                    foreach (SelectOption option in _options)
                    {
                        option.Selected = CanSelectOption(option) && indexes.Contains(option.Index);
                    }

                    SyncGroupSelectionStates();

                    return MultiSelectionResult.FromSelections(indexes);
                }

                WriteInvalidSelectionMessage();
                SelectOption unavailableOption;
                if (TryResolveUnavailableSelection(input, filterText, out unavailableOption))
                {
                    WriteUnavailableSelectionMessage(unavailableOption);
                }
            } while (true);
        }

        private MultiSelectionResult RenderKeyboard(CancellationToken cancellationToken)
        {
            string filterText = string.Empty;
            int highlightedIndex = ResolveInitialKeyboardIndex(GetFirstSelectedIndex(), filterText);
            bool previousCursorVisible = Consoul.ConsoleDriver.CursorVisible;
            bool hasRendered = false;
            Consoul.ConsoleDriver.CursorVisible = false;

            try
            {
                do
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return ResolveCancelResult();
                    }

                    if (ClearConsole || hasRendered)
                    {
                        Consoul.ConsoleDriver.Clear();
                    }

                    int matchCount = GetMatchingOptionCount(filterText);
                    highlightedIndex = EnsureKeyboardHighlight(highlightedIndex, filterText);
                    int? pageSize = GetEffectivePageSize(filterText);
                    int currentPage = GetInitialPage(highlightedIndex, filterText);
                    currentPage = ClampPage(currentPage, pageSize, filterText);
                    RenderPromptFrame(pageSize, currentPage, filterText, matchCount, highlightedIndex, keyboardMode: true);
                    hasRendered = true;

                    ConsoleKeyInfo keyInfo = Consoul.ConsoleDriver.ReadKey(true);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        return ResolveCancelResult();
                    }

                    if (keyInfo.Key == ConsoleKey.Enter)
                    {
                        int[] selected = GetSelectedIndexes();
                        if (selected.Length == 0 && highlightedIndex >= 0 && !AllowEmpty)
                        {
                            SelectForSubmit(highlightedIndex);
                            selected = GetSelectedIndexes();
                        }

                        if (selected.Length > 0 || AllowEmpty)
                        {
                            return MultiSelectionResult.FromSelections(selected);
                        }

                        WriteInvalidSelectionMessage();
                        continue;
                    }

                    if (keyInfo.Key == ConsoleKey.Spacebar)
                    {
                        ToggleKeyboardSelection(highlightedIndex);
                        continue;
                    }

                    if (keyInfo.Key == ConsoleKey.UpArrow || keyInfo.Key == ConsoleKey.LeftArrow)
                    {
                        highlightedIndex = MoveKeyboardHighlight(highlightedIndex, filterText, -1);
                        continue;
                    }

                    if (keyInfo.Key == ConsoleKey.DownArrow || keyInfo.Key == ConsoleKey.RightArrow || keyInfo.Key == ConsoleKey.Tab)
                    {
                        highlightedIndex = MoveKeyboardHighlight(highlightedIndex, filterText, 1);
                        continue;
                    }

                    if (keyInfo.Key == ConsoleKey.Home)
                    {
                        highlightedIndex = GetFirstEnabledMatchIndex(filterText);
                        continue;
                    }

                    if (keyInfo.Key == ConsoleKey.End)
                    {
                        highlightedIndex = GetLastEnabledMatchIndex(filterText);
                        continue;
                    }

                    if (keyInfo.Key == ConsoleKey.Backspace)
                    {
                        if (FilterEnabled && filterText.Length > 0)
                        {
                            filterText = filterText.Substring(0, filterText.Length - 1);
                            highlightedIndex = ResolveInitialKeyboardIndex(-1, filterText);
                        }

                        continue;
                    }

                    if (FilterEnabled && !char.IsControl(keyInfo.KeyChar))
                    {
                        filterText += keyInfo.KeyChar;
                        highlightedIndex = ResolveInitialKeyboardIndex(-1, filterText);
                    }
                } while (true);
            }
            finally
            {
                Consoul.ConsoleDriver.CursorVisible = previousCursorVisible;
            }

            return ResolveCancelResult();
        }

        private void RenderPromptFrame(int? pageSize, int currentPage, string filterText, int matchCount, int? highlightedIndex = null, bool keyboardMode = false)
        {
            PromptRenderHelpers.WriteMarkupLine(Message, ResolvePromptStyle());
            PromptRenderHelpers.WriteMarkupLine(
                keyboardMode ? KeyboardNavigationHelpText : InstructionsMessage,
                ResolveInstructionStyle());

            foreach (SelectOption option in GetVisibleOptions(pageSize, currentPage, filterText))
            {
                WriteOption(option, highlightedIndex.HasValue && option.Index == highlightedIndex.Value, keyboardMode, filterText);
            }

            WriteFilterHelp(filterText, matchCount, keyboardMode);
            WritePageHelp(pageSize, currentPage, filterText);
        }

        private void WriteOption(SelectOption option, bool highlighted, bool keyboardMode, string filterText)
        {
            string text = option.ToString();
            if (keyboardMode)
            {
                text = (highlighted ? "> " : "  ") + text;
            }

            PromptRenderHelpers.WriteSearchHighlightedText(
                text,
                filterText,
                SearchHighlightStyle,
                highlighted ? ResolveKeyboardSelectionStyle() : ResolveOptionStyle(option));
            WriteOptionDescription(option, keyboardMode, filterText);
        }

        private void WriteOptionDescription(SelectOption option, bool keyboardMode, string filterText)
        {
            if (!ShowOptionDescriptions || option.IsSeparator || string.IsNullOrWhiteSpace(option.Description))
            {
                return;
            }

            string prefix = OptionDescriptionPrefix ?? string.Empty;
            if (keyboardMode)
            {
                prefix = "  " + prefix;
            }

            PromptRenderHelpers.WriteSearchHighlightedText(prefix + option.Description.Trim(), filterText, SearchHighlightStyle, ResolveOptionDescriptionStyle(option));
        }

        private void AttachChildFactory(SelectOption option)
        {
            option.AttachChildFactory(AddChild);
        }

        private bool TryParseInput(string input, string filterText, out int[] indexes)
        {
            input = (input ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(input))
            {
                indexes = _options.Where(option => option.IsDefault && CanSelectOption(option)).Select(option => option.Index).ToArray();
                return indexes.Length > 0 || AllowEmpty;
            }

            if (input.Equals("all", StringComparison.OrdinalIgnoreCase) || input == "*")
            {
                indexes = GetMatchingOptions(filterText).Where(CanSelectOption).Select(option => option.Index).ToArray();
                return indexes.Length > 0 || AllowEmpty;
            }

            if (AllowEmpty && input.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                indexes = new int[0];
                return true;
            }

            var selected = new List<int>();
            string[] tokens = input.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string token in tokens)
            {
                if (token.IndexOf('-') >= 0)
                {
                    string[] rangeParts = token.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    if (rangeParts.Length != 2)
                    {
                        indexes = new int[0];
                        return false;
                    }

                    int start;
                    int end;
                    if (!TryParseOneBasedIndex(rangeParts[0], out start) || !TryParseOneBasedIndex(rangeParts[1], out end))
                    {
                        indexes = new int[0];
                        return false;
                    }

                    int min = Math.Min(start, end);
                    int max = Math.Max(start, end);
                    for (int value = min; value <= max; value++)
                    {
                        AddSelectionIndex(selected, value);
                    }

                    continue;
                }

                int index;
                if (!TryParseOneBasedIndex(token, out index))
                {
                    indexes = new int[0];
                    return false;
                }

                AddSelectionIndex(selected, index);
            }

            if (selected.Any(index => !CanSelectOption(GetOptionByIndex(index))))
            {
                indexes = new int[0];
                return false;
            }

            if (!string.IsNullOrWhiteSpace(filterText) && selected.Any(index => !MatchesFilter(GetOptionByIndex(index), filterText)))
            {
                indexes = new int[0];
                return false;
            }

            indexes = selected.ToArray();
            return indexes.Length > 0 || AllowEmpty;
        }

        private bool TryParseOneBasedIndex(string value, out int zeroBasedIndex)
        {
            int parsed;
            if (!int.TryParse(value, out parsed) || parsed <= 0 || parsed > Count)
            {
                zeroBasedIndex = -1;
                return false;
            }

            zeroBasedIndex = parsed - 1;
            return true;
        }

        private bool TryResolveUnavailableSelection(string input, string filterText, out SelectOption unavailableOption)
        {
            unavailableOption = null;
            input = (input ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(input) ||
                input.Equals("all", StringComparison.OrdinalIgnoreCase) ||
                input == "*" ||
                input.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string[] tokens = input.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string token in tokens)
            {
                if (token.IndexOf('-') >= 0)
                {
                    string[] rangeParts = token.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    if (rangeParts.Length != 2)
                    {
                        return false;
                    }

                    int start;
                    int end;
                    if (!TryParseOneBasedIndex(rangeParts[0], out start) || !TryParseOneBasedIndex(rangeParts[1], out end))
                    {
                        return false;
                    }

                    int min = Math.Min(start, end);
                    int max = Math.Max(start, end);
                    for (int value = min; value <= max; value++)
                    {
                        if (TryResolveUnavailableSelection(value, filterText, out unavailableOption))
                        {
                            return true;
                        }
                    }

                    continue;
                }

                int index;
                if (!TryParseOneBasedIndex(token, out index))
                {
                    return false;
                }

                if (TryResolveUnavailableSelection(index, filterText, out unavailableOption))
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryResolveUnavailableSelection(int index, string filterText, out SelectOption unavailableOption)
        {
            unavailableOption = null;
            SelectOption option = GetOptionByIndex(index);
            if (!string.IsNullOrWhiteSpace(filterText) && !MatchesFilter(option, filterText))
            {
                return false;
            }

            if (option.IsDisabled)
            {
                unavailableOption = option;
                return true;
            }

            if (option.IsGroup && Mode == SelectionMode.Leaf && !GetSelectableDescendants(option).Any())
            {
                unavailableOption = option;
                return true;
            }

            return false;
        }

        private void WriteUnavailableSelectionMessage(SelectOption option)
        {
            string reason = option.IsGroup && Mode == SelectionMode.Leaf
                ? "Choose one of the group's child options."
                : string.IsNullOrWhiteSpace(option.DisabledReason)
                ? "Option is disabled."
                : option.DisabledReason;
            if (!PromptRenderHelpers.ContainsMarkup(reason))
            {
                Consoul.WriteCore("Option unavailable: " + reason, RenderOptions.InvalidColor);
                return;
            }

            Consoul.WriteCore("Option unavailable: ", RenderOptions.InvalidColor, writeLine: false);
            PromptRenderHelpers.WriteMarkupLine(reason, RenderOptions.InvalidColor);
        }

        private static void AddIfMissing(List<int> selected, int index)
        {
            if (!selected.Contains(index))
            {
                selected.Add(index);
            }
        }

        private void AddSelectionIndex(List<int> selected, int index)
        {
            SelectOption option = GetOptionByIndex(index);
            if (option.IsGroup && Mode == SelectionMode.Leaf)
            {
                foreach (SelectOption child in GetSelectableDescendants(option))
                {
                    AddIfMissing(selected, child.Index);
                }

                return;
            }

            AddIfMissing(selected, index);
        }

        private int GetNextOptionIndex()
        {
            return Count;
        }

        private int? GetEffectivePageSize(string filterText)
        {
            int count = GetMatchingOptionCount(filterText);
            if (!PageSize.HasValue || PageSize.Value <= 0 || count <= PageSize.Value)
            {
                return null;
            }

            return PageSize.Value;
        }

        private int GetInitialPage()
        {
            SelectOption firstDefault = _options.FirstOrDefault(option => option.IsDefault && !option.IsDisabled && !option.IsSeparator);
            return firstDefault == null ? 0 : GetInitialPage(firstDefault.Index, string.Empty);
        }

        private int GetInitialPage(int optionIndex, string filterText)
        {
            int? pageSize = GetEffectivePageSize(filterText);
            if (!pageSize.HasValue || optionIndex < 0)
            {
                return 0;
            }

            SelectOption option = GetMatchingOptions(filterText).FirstOrDefault(item => item.Index == optionIndex);
            if (option == null)
            {
                return 0;
            }

            int matchIndex = GetMatchingOptions(filterText).ToList().IndexOf(option);
            return matchIndex < 0 ? 0 : matchIndex / pageSize.Value;
        }

        private int ClampPage(int currentPage, int? pageSize, string filterText)
        {
            if (!pageSize.HasValue)
            {
                return 0;
            }

            int pageCount = GetPageCount(pageSize.Value, filterText);
            return Math.Max(0, Math.Min(currentPage, pageCount - 1));
        }

        private int GetPageCount(int pageSize, string filterText)
        {
            return Math.Max(1, (int)Math.Ceiling(GetMatchingOptionCount(filterText) / (double)pageSize));
        }

        private IEnumerable<SelectOption> GetVisibleOptions(int? pageSize, int currentPage, string filterText)
        {
            SelectOption[] matches = GetMatchingOptions(filterText);
            IEnumerable<SelectOption> visibleMatches = matches;
            if (!pageSize.HasValue)
            {
                return IncludeSeparators(visibleMatches.ToArray());
            }

            int startIndex = currentPage * pageSize.Value;
            visibleMatches = matches.Skip(startIndex).Take(pageSize.Value);
            return IncludeSeparators(visibleMatches.ToArray());
        }

        private IEnumerable<SelectOption> IncludeSeparators(IReadOnlyCollection<SelectOption> visibleOptions)
        {
            var visible = new HashSet<SelectOption>(visibleOptions);
            foreach (SelectOption option in _options)
            {
                if (!option.IsSeparator)
                {
                    if (visible.Contains(option))
                    {
                        yield return option;
                    }

                    continue;
                }

                if (ShouldRenderSeparator(option, visible))
                {
                    yield return option;
                }
            }
        }

        private bool ShouldRenderSeparator(SelectOption option, ISet<SelectOption> visibleOptions)
        {
            int optionPosition = _options.IndexOf(option);
            for (int index = optionPosition + 1; index < _options.Count; index++)
            {
                SelectOption nextSelectable = _options[index];
                if (nextSelectable.IsSeparator)
                {
                    break;
                }

                if (visibleOptions.Contains(nextSelectable))
                {
                    return true;
                }
            }

            return false;
        }

        private SelectOption[] GetMatchingOptions(string filterText)
        {
            return _options
                .Where(option => !option.IsSeparator && MatchesFilter(option, filterText))
                .ToArray();
        }

        private int GetMatchingOptionCount(string filterText)
        {
            return GetMatchingOptions(filterText).Length;
        }

        private int ResolveInitialKeyboardIndex(int preferredIndex, string filterText)
        {
            SelectOption preferred = GetMatchingOptions(filterText)
                .FirstOrDefault(option => option.Index == preferredIndex && CanToggleOption(option));
            return preferred != null ? preferred.Index : GetFirstEnabledMatchIndex(filterText);
        }

        private int EnsureKeyboardHighlight(int highlightedIndex, string filterText)
        {
            SelectOption highlighted = GetMatchingOptions(filterText)
                .FirstOrDefault(option => option.Index == highlightedIndex && CanToggleOption(option));
            return highlighted != null ? highlighted.Index : GetFirstEnabledMatchIndex(filterText);
        }

        private int MoveKeyboardHighlight(int highlightedIndex, string filterText, int direction)
        {
            SelectOption[] matches = GetEnabledMatchingOptions(filterText);
            if (matches.Length == 0)
            {
                return -1;
            }

            int currentPosition = Array.FindIndex(matches, option => option.Index == highlightedIndex);
            if (currentPosition < 0)
            {
                return WrapAround && direction < 0 ? matches[matches.Length - 1].Index : matches[0].Index;
            }

            int nextPosition = currentPosition + direction;
            if (WrapAround)
            {
                nextPosition = (nextPosition + matches.Length) % matches.Length;
            }
            else
            {
                nextPosition = Math.Max(0, Math.Min(matches.Length - 1, nextPosition));
            }

            return matches[nextPosition].Index;
        }

        private int GetFirstEnabledMatchIndex(string filterText)
        {
            SelectOption option = GetEnabledMatchingOptions(filterText).FirstOrDefault();
            return option != null ? option.Index : -1;
        }

        private int GetLastEnabledMatchIndex(string filterText)
        {
            SelectOption[] matches = GetEnabledMatchingOptions(filterText);
            return matches.Length == 0 ? -1 : matches[matches.Length - 1].Index;
        }

        private SelectOption[] GetEnabledMatchingOptions(string filterText)
        {
            return GetMatchingOptions(filterText)
                .Where(CanToggleOption)
                .ToArray();
        }

        private int GetFirstSelectedIndex()
        {
            SelectOption option = _options.FirstOrDefault(item => CanSelectOption(item) && item.Selected);
            return option != null ? option.Index : -1;
        }

        private int[] GetSelectedIndexes()
        {
            SyncGroupSelectionStates();
            return _options
                .Where(option => CanSelectOption(option) && option.Selected)
                .Select(option => option.Index)
                .ToArray();
        }

        private void ToggleKeyboardSelection(int highlightedIndex)
        {
            if (highlightedIndex < 0)
            {
                return;
            }

            SelectOption option = GetOptionByIndex(highlightedIndex);
            if (!CanToggleOption(option))
            {
                return;
            }

            if (option.IsGroup && Mode == SelectionMode.Leaf)
            {
                SelectGroupChildren(option);
                return;
            }

            option.Selected = !option.Selected;
            SyncGroupSelectionStates();
        }

        private void SelectForSubmit(int highlightedIndex)
        {
            SelectOption option = GetOptionByIndex(highlightedIndex);
            if (!CanToggleOption(option))
            {
                return;
            }

            if (option.IsGroup && Mode == SelectionMode.Leaf)
            {
                foreach (SelectOption child in GetSelectableDescendants(option))
                {
                    child.Selected = true;
                }

                SyncGroupSelectionStates();
                return;
            }

            if (CanSelectOption(option))
            {
                option.Selected = true;
            }

            SyncGroupSelectionStates();
        }

        private bool CanToggleOption(SelectOption option)
        {
            return option != null && !option.IsSeparator && !option.IsDisabled;
        }

        private bool CanSelectOption(SelectOption option)
        {
            return CanToggleOption(option) && !(option.IsGroup && Mode == SelectionMode.Leaf);
        }

        private bool ShouldSelectOptionByDefault(string label)
        {
            return _selectAllByDefault || (label != null && _defaultLabels.Contains(label));
        }

        /// <summary>
        /// Clears remembered label and select-all defaults used for future options.
        /// </summary>
        protected void ResetDefaultSelectionState()
        {
            _defaultLabels.Clear();
            _selectAllByDefault = false;
        }

        private void MarkOptionSelectedByDefault(SelectOption option)
        {
            if (option == null || option.IsSeparator || option.IsDisabled)
            {
                return;
            }

            if (option.IsGroup && Mode == SelectionMode.Leaf)
            {
                foreach (SelectOption child in GetSelectableDescendants(option))
                {
                    child.IsDefault = true;
                    child.Selected = true;
                }

                return;
            }

            if (CanSelectOption(option))
            {
                option.IsDefault = true;
                option.Selected = true;
            }
        }

        private IEnumerable<SelectOption> GetSelectableDescendants(SelectOption option)
        {
            foreach (SelectOption child in option.Children)
            {
                if (CanSelectOption(child))
                {
                    yield return child;
                }

                foreach (SelectOption descendant in GetSelectableDescendants(child))
                {
                    yield return descendant;
                }
            }
        }

        private void SelectGroupChildren(SelectOption option)
        {
            SelectOption[] descendants = GetSelectableDescendants(option).ToArray();
            if (descendants.Length == 0)
            {
                return;
            }

            bool select = descendants.Any(child => !child.Selected);
            foreach (SelectOption child in descendants)
            {
                child.Selected = select;
            }

            SyncGroupSelectionStates();
        }

        private void SyncGroupSelectionStates()
        {
            foreach (SelectOption group in _options.Where(option => option.IsGroup).OrderByDescending(option => option.Depth))
            {
                SelectOption[] descendants = GetSelectableDescendants(group).ToArray();
                group.Selected = descendants.Length > 0 && descendants.All(child => child.Selected);
            }
        }

        private static bool MatchesFilter(SelectOption option, string filterText)
        {
            return string.IsNullOrWhiteSpace(filterText) ||
                PromptRenderHelpers.ContainsVisibleText(option.Label ?? string.Empty, filterText) ||
                PromptRenderHelpers.ContainsVisibleText(option.Description ?? string.Empty, filterText) ||
                PromptRenderHelpers.ContainsVisibleText(option.DisabledReason ?? string.Empty, filterText) ||
                option.SearchTerms.Any(term => (term ?? string.Empty).IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private bool TryHandleFilterCommand(string input, ref string filterText, ref int currentPage)
        {
            if (!FilterEnabled || string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            string prefix = string.IsNullOrEmpty(FilterCommandPrefix) ? "/" : FilterCommandPrefix;
            string command = input.Trim();
            if (!command.StartsWith(prefix, StringComparison.Ordinal))
            {
                return false;
            }

            string value = command.Substring(prefix.Length).Trim();
            filterText = IsClearFilterCommand(value) ? string.Empty : value;
            currentPage = 0;
            return true;
        }

        private static bool IsClearFilterCommand(string value)
        {
            return string.IsNullOrWhiteSpace(value) ||
                value.Equals("clear", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("reset", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("all", StringComparison.OrdinalIgnoreCase);
        }

        private bool TryHandlePagingCommand(string input, int? pageSize, string filterText, ref int currentPage)
        {
            if (!pageSize.HasValue || string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            string command = input.Trim();
            int pageCount = GetPageCount(pageSize.Value, filterText);
            if (command.Equals("next", StringComparison.OrdinalIgnoreCase) ||
                command.Equals("n", StringComparison.OrdinalIgnoreCase))
            {
                currentPage = Math.Min(currentPage + 1, pageCount - 1);
                return true;
            }

            if (command.Equals("prev", StringComparison.OrdinalIgnoreCase) ||
                command.Equals("previous", StringComparison.OrdinalIgnoreCase) ||
                command.Equals("p", StringComparison.OrdinalIgnoreCase))
            {
                currentPage = Math.Max(currentPage - 1, 0);
                return true;
            }

            return false;
        }

        private void WriteFilterHelp(string filterText, int matchCount, bool keyboardMode = false)
        {
            if (!FilterEnabled)
            {
                return;
            }

            string prefix = string.IsNullOrEmpty(FilterCommandPrefix) ? "/" : FilterCommandPrefix;
            if (string.IsNullOrWhiteSpace(filterText))
            {
                WriteFormattedPromptHelp(keyboardMode ? ResolveSearchPlaceholderText() : FilterHelpText, RenderOptions.SubnoteColor, prefix);
                return;
            }

            WriteFormattedPromptHelp(ActiveFilterText, RenderOptions.SubnoteColor, filterText, matchCount);
            if (matchCount == 0)
            {
                WriteFormattedPromptHelp(NoFilterMatchesText, RenderOptions.InvalidColor, filterText, matchCount);
            }
        }

        private static void WriteFormattedPromptHelp(string format, ConsoleColor color, params object[] args)
        {
            PromptRenderHelpers.WriteFormattedMarkupOrPlainLine(format, color, args);
        }

        private void WriteInvalidSelectionMessage()
        {
            PromptRenderHelpers.WriteMarkupOrPlainLine(InvalidSelectionMessage ?? string.Empty, RenderOptions.InvalidColor);
        }

        private void WritePageHelp(int? pageSize, int currentPage, string filterText)
        {
            if (!pageSize.HasValue)
            {
                return;
            }

            int count = GetMatchingOptionCount(filterText);
            int firstOption = currentPage * pageSize.Value + 1;
            int lastOption = Math.Min(count, firstOption + pageSize.Value - 1);
            int pageCount = GetPageCount(pageSize.Value, filterText);
            string format = MoreChoicesText ?? "Showing options {0}-{1} of {2}. Type next/prev to change pages.";
            string message;
            bool parseMarkup;
            if (!PromptRenderHelpers.TryFormatMarkupOrPlain(format, out message, out parseMarkup, firstOption, lastOption, count, currentPage + 1, pageCount))
            {
                message = "Showing options " + firstOption + "-" + lastOption + " of " + count + ". Type next/prev to change pages.";
                parseMarkup = false;
            }

            PromptRenderHelpers.WriteMarkupOrPlainLine(message, RenderOptions.SubnoteColor, parseMarkup);
        }

        /// <summary>
        /// Gets the selectable option with the specified zero-based index.
        /// </summary>
        /// <param name="index">Zero-based selectable index of the option.</param>
        /// <returns>The matching <see cref="SelectOption"/>.</returns>
        protected SelectOption GetOptionByIndex(int index)
        {
            SelectOption option = _options.FirstOrDefault(item => !item.IsSeparator && item.Index == index);
            if (option == null)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return option;
        }

        private static PromptRenderHelpers.StyledText ResolveOptionStyle(SelectOption option)
        {
            if (option.IsSeparator)
            {
                return option.ResolveStyle();
            }

            return option.IsDisabled ? option.ResolveDisabledStyle(null) : option.ResolveStyle();
        }

        private PromptRenderHelpers.StyledText ResolvePromptStyle()
        {
            return PromptRenderHelpers.FromComponents(
                PromptColor,
                PromptDecoration,
                PromptForegroundRgb,
                PromptBackgroundRgb);
        }

        private PromptRenderHelpers.StyledText ResolveInstructionStyle()
        {
            return PromptRenderHelpers.FromComponents(
                InstructionColor,
                InstructionDecoration,
                InstructionForegroundRgb,
                InstructionBackgroundRgb);
        }

        private PromptRenderHelpers.StyledText ResolveOptionDescriptionStyle(SelectOption option)
        {
            PromptRenderHelpers.StyledText fallback = PromptRenderHelpers.FromComponents(
                OptionDescriptionColor,
                OptionDescriptionDecoration,
                OptionDescriptionForegroundRgb,
                OptionDescriptionBackgroundRgb);
            return option == null ? fallback : option.ResolveDescriptionStyle(fallback);
        }

        private PromptRenderHelpers.StyledText ResolveKeyboardSelectionStyle()
        {
            return PromptRenderHelpers.FromComponents(
                KeyboardSelectionColor,
                KeyboardSelectionDecoration,
                KeyboardSelectionForegroundRgb,
                KeyboardSelectionBackgroundRgb);
        }

        private void ApplyPromptStyle(Style style)
        {
            if (style == null)
            {
                return;
            }

            PromptColor = style.ToForegroundColor(PromptColor);
            PromptDecoration = style.Decoration;
            PromptForegroundRgb = style.ForegroundRgb;
            PromptBackgroundRgb = style.BackgroundRgb;
        }

        private void ApplyInstructionStyle(Style style)
        {
            if (style == null)
            {
                return;
            }

            InstructionColor = style.ToForegroundColor(InstructionColor);
            InstructionDecoration = style.Decoration;
            InstructionForegroundRgb = style.ForegroundRgb;
            InstructionBackgroundRgb = style.BackgroundRgb;
        }

        private void ApplyOptionDescriptionStyle(Style style)
        {
            if (style == null)
            {
                return;
            }

            OptionDescriptionColor = style.ToForegroundColor(OptionDescriptionColor);
            OptionDescriptionDecoration = style.Decoration;
            OptionDescriptionForegroundRgb = style.ForegroundRgb;
            OptionDescriptionBackgroundRgb = style.BackgroundRgb;
        }

        private void ApplyKeyboardSelectionStyle(Style style)
        {
            if (style == null)
            {
                return;
            }

            KeyboardSelectionColor = style.ToForegroundColor(KeyboardSelectionColor);
            KeyboardSelectionDecoration = style.Decoration;
            KeyboardSelectionForegroundRgb = style.ForegroundRgb;
            KeyboardSelectionBackgroundRgb = style.BackgroundRgb;
        }

        private MultiSelectionResult ResolveCancelResult()
        {
            return CancelResult == null ? MultiSelectionResult.Canceled() : CancelResult();
        }

        private string ResolveSearchPlaceholderText()
        {
            return string.IsNullOrWhiteSpace(SearchPlaceholderText) ? KeyboardFilterHelpText : SearchPlaceholderText;
        }
    }

    /// <summary>
    /// Represents a multi-selection prompt with generic entity type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the entities represented as options in the prompt.</typeparam>
    public class MultiSelectionPrompt<T> : MultiSelectionPrompt, IPrompt<MultiSelectionResult<T>>
    {
        private readonly Dictionary<int, T> _itemsByIndex = new Dictionary<int, T>();
        private readonly Dictionary<SelectOption, T> _separatorItemsByOption = new Dictionary<SelectOption, T>();
        private readonly List<T> _defaultValues = new List<T>();

        /// <summary>
        /// Initializes a new typed multi-selection prompt.
        /// </summary>
        /// <param name="message">The message to display when the prompt is shown.</param>
        /// <param name="clear">Indicates whether to clear the console when the prompt is displayed.</param>
        /// <param name="allowEmpty">Indicates whether an empty selection is valid.</param>
        /// <param name="labelSelector">The function used to extract the label from each entity.</param>
        public MultiSelectionPrompt(string message, bool clear = false, bool allowEmpty = false, Func<T, string> labelSelector = null) : base(message, clear, allowEmpty)
        {
            LabelSelector = labelSelector ?? DefaultLabelSelector;
        }

        /// <summary>
        /// Initializes a new typed multi-selection prompt with an empty title and default label conversion.
        /// </summary>
        public MultiSelectionPrompt() : this(string.Empty, false, false, null)
        {
        }

        /// <summary>
        /// Shows the prompt through the specified console facade.
        /// </summary>
        /// <param name="console">The console facade used to render the prompt.</param>
        /// <returns>The typed multi-selection result.</returns>
        MultiSelectionResult<T> IPrompt<MultiSelectionResult<T>>.Show(IAnsiConsole console)
        {
            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            return console.Prompt(this);
        }

        /// <summary>
        /// Shows the prompt asynchronously through the specified console facade.
        /// </summary>
        /// <param name="console">The console facade used to render the prompt.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that resolves to the typed multi-selection result.</returns>
        Task<MultiSelectionResult<T>> IPrompt<MultiSelectionResult<T>>.ShowAsync(IAnsiConsole console, CancellationToken cancellationToken)
        {
            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            return Task.FromResult(console.Prompt(this, cancellationToken));
        }

        /// <summary>
        /// Initializes a new typed multi-selection prompt with an initial set of options.
        /// </summary>
        /// <param name="message">The message to display when the prompt is shown.</param>
        /// <param name="clear">Indicates whether to clear the console when the prompt is displayed.</param>
        /// <param name="allowEmpty">Indicates whether an empty selection is valid.</param>
        /// <param name="labelSelector">The function used to extract the label from each entity.</param>
        /// <param name="options">Initial options to add to the prompt.</param>
        public MultiSelectionPrompt(string message, bool clear = false, bool allowEmpty = false, Func<T, string> labelSelector = null, params T[] options) : this(message, clear, allowEmpty, labelSelector)
        {
            foreach (T item in options ?? new T[0])
            {
                Add(item);
            }
        }

        /// <summary>
        /// Gets or sets the function used to extract the label for each entity of type <typeparamref name="T"/>.
        /// </summary>
        public Func<T, string> LabelSelector { get; set; }

        /// <summary>
        /// Gets or sets the function used to extract optional helper text for each entity of type <typeparamref name="T"/>.
        /// </summary>
        public Func<T, string> DescriptionSelector { get; set; }

        /// <summary>
        /// Gets or sets the function used to extract hidden search terms for each entity of type <typeparamref name="T"/>.
        /// </summary>
        public Func<T, IEnumerable<string>> SearchTermsSelector { get; set; }

        /// <summary>
        /// Gets or sets the fallback items returned on <see cref="MultiSelectionResult{T}.SelectedItems"/> when the prompt is canceled.
        /// </summary>
        public new Func<IReadOnlyList<T>> CancelResult { get; set; }

        /// <summary>
        /// Sets the prompt title/message using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="title">The prompt title shown to the user.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> Title(string title)
        {
            base.Title(title);
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message.
        /// </summary>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> PromptStyle(ConsoleColor color)
        {
            base.PromptStyle(color);
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> PromptStyle(SpectreColor color)
        {
            base.PromptStyle(color);
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message from an RGB foreground color.
        /// </summary>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> PromptStyle(RgbColor color)
        {
            base.PromptStyle(color);
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> PromptStyle(Style style)
        {
            base.PromptStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> PromptStyle(string style)
        {
            base.PromptStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message.
        /// </summary>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> Style(ConsoleColor color)
        {
            base.Style(color);
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> Style(Style style)
        {
            base.Style(style);
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> Style(string style)
        {
            base.Style(style);
            return this;
        }

        /// <summary>
        /// Sets the helper text shown above the options when line-based input is used.
        /// </summary>
        /// <param name="text">The instruction text.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> InstructionsText(string text)
        {
            base.InstructionsText(text);
            return this;
        }

        /// <summary>
        /// Sets the color used for prompt instruction text.
        /// </summary>
        /// <param name="color">The instruction text color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> InstructionStyle(ConsoleColor color)
        {
            base.InstructionStyle(color);
            return this;
        }

        /// <summary>
        /// Sets the color used for prompt instruction text from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The instruction text color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> InstructionStyle(SpectreColor color)
        {
            base.InstructionStyle(color);
            return this;
        }

        /// <summary>
        /// Sets the color used for prompt instruction text from an RGB foreground color.
        /// </summary>
        /// <param name="color">The instruction text color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> InstructionStyle(RgbColor color)
        {
            base.InstructionStyle(color);
            return this;
        }

        /// <summary>
        /// Sets the color used for prompt instruction text from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> InstructionStyle(Style style)
        {
            base.InstructionStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the color used for prompt instruction text from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#94a3b8</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> InstructionStyle(string style)
        {
            base.InstructionStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the color used for option descriptions.
        /// </summary>
        /// <param name="color">The option description foreground color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> OptionDescriptionStyle(ConsoleColor color)
        {
            base.OptionDescriptionStyle(color);
            return this;
        }

        /// <summary>
        /// Sets the color used for option descriptions from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The option description foreground color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> OptionDescriptionStyle(SpectreColor color)
        {
            base.OptionDescriptionStyle(color);
            return this;
        }

        /// <summary>
        /// Sets the color used for option descriptions from an RGB foreground color.
        /// </summary>
        /// <param name="color">The option description foreground color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> OptionDescriptionStyle(RgbColor color)
        {
            base.OptionDescriptionStyle(color);
            return this;
        }

        /// <summary>
        /// Sets the style used for option descriptions.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> OptionDescriptionStyle(Style style)
        {
            base.OptionDescriptionStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the style used for option descriptions from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>italic</c>, <c>#94a3b8</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> OptionDescriptionStyle(string style)
        {
            base.OptionDescriptionStyle(style);
            return this;
        }

        /// <summary>
        /// Configures the converter used to render typed choices as labels.
        /// </summary>
        /// <param name="converter">The converter used to render typed choices.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public MultiSelectionPrompt<T> UseConverter(Func<T, string> converter)
        {
            LabelSelector = converter ?? throw new ArgumentNullException(nameof(converter));
            RefreshOptionLabels();
            return this;
        }

        /// <summary>
        /// Adds multiple typed choices using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="choices">The choices to add.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public MultiSelectionPrompt<T> AddChoices(params T[] choices)
        {
            return AddChoices((IEnumerable<T>)choices);
        }

        /// <summary>
        /// Adds multiple typed choices using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="choices">The choices to add.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public MultiSelectionPrompt<T> AddChoices(IEnumerable<T> choices)
        {
            foreach (T item in choices ?? new T[0])
            {
                Add(item);
            }

            return this;
        }

        /// <summary>
        /// Adds a display-only group heading followed by typed choices using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="group">The item used to render the group heading.</param>
        /// <param name="choices">The choices to add under the group.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public MultiSelectionPrompt<T> AddChoiceGroup(T group, params T[] choices)
        {
            return AddChoiceGroup(group, (IEnumerable<T>)choices);
        }

        /// <summary>
        /// Adds a display-only group heading followed by typed choices using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="group">The item used to render the group heading.</param>
        /// <param name="choices">The choices to add under the group.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public MultiSelectionPrompt<T> AddChoiceGroup(T group, IEnumerable<T> choices)
        {
            SelectOption separator = AddSeparator(LabelSelector(group));
            _separatorItemsByOption[separator] = group;
            return AddChoices(choices);
        }

        /// <summary>
        /// Adds one typed choice using Spectre-style prompt vocabulary.
        /// </summary>
        /// <param name="choice">The choice to add.</param>
        /// <returns>The added <see cref="SelectOption"/>.</returns>
        public SelectOption AddChoice(T choice)
        {
            int index = Add(choice);
            return GetOptionByIndex(index);
        }

        /// <summary>
        /// Adds a grouped typed choice that can contain child options.
        /// </summary>
        /// <param name="group">The item used to render and map the group option.</param>
        /// <param name="color">Optional group foreground color.</param>
        /// <param name="isDefault">Whether the group is selected by default when groups are independently selectable.</param>
        /// <returns>The added group option.</returns>
        public SelectOption AddGroup(T group, ConsoleColor? color = null, bool isDefault = false)
        {
            int index = Add(group, color, isDefault);
            SelectOption option = GetOptionByIndex(index);
            option.IsGroup = true;
            return option;
        }

        /// <summary>
        /// Adds a typed child choice beneath an existing grouped option.
        /// </summary>
        /// <param name="parent">The prompt-owned group option that should contain the child.</param>
        /// <param name="item">The child item to render and map.</param>
        /// <param name="color">Optional child foreground color.</param>
        /// <param name="isDefault">Whether the child is selected by default.</param>
        /// <param name="renderStyle">The render style used for the child option.</param>
        /// <returns>The added child option.</returns>
        public SelectOption AddChild(SelectOption parent, T item, ConsoleColor? color = null, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Checkbox)
        {
            if (LabelSelector == null)
            {
                throw new InvalidOperationException("LabelSelector function must be set before adding items of type T.");
            }

            SelectOption option = base.AddChild(parent, LabelSelector(item), color, isDefault || IsDefaultValue(item), renderStyle);
            option.Description = DescriptionSelector != null ? DescriptionSelector(item) : null;
            if (SearchTermsSelector != null)
            {
                option.AddSearchTerms(SearchTermsSelector(item));
            }

            _itemsByIndex[option.Index] = item;
            return option;
        }

        /// <summary>
        /// Enables slash-command filtering for this prompt.
        /// </summary>
        /// <param name="enabled">Whether filtering should be enabled.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> SearchEnabled(bool enabled = true)
        {
            base.SearchEnabled(enabled);
            return this;
        }

        /// <summary>
        /// Enables slash-command filtering for this prompt using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="enabled">Whether filtering should be enabled.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> EnableSearch(bool enabled = true)
        {
            return SearchEnabled(enabled);
        }

        /// <summary>
        /// Enables keyboard navigation for this prompt.
        /// </summary>
        /// <param name="enabled">Whether keyboard navigation should be enabled.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> UseKeyboardNavigation(bool enabled = true)
        {
            base.UseKeyboardNavigation(enabled);
            return this;
        }

        /// <summary>
        /// Sets whether keyboard navigation wraps at the first and last available options.
        /// </summary>
        /// <param name="enabled">Whether arrow-key navigation should wrap around.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> UseWrapAround(bool enabled = true)
        {
            base.UseWrapAround(enabled);
            return this;
        }

        /// <summary>
        /// Sets whether the prompt accepts an empty selection.
        /// </summary>
        /// <param name="allowEmpty">Whether an empty selection should be accepted.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> AllowEmptySelection(bool allowEmpty = true)
        {
            base.AllowEmptySelection(allowEmpty);
            return this;
        }

        /// <summary>
        /// Sets whether this prompt requires at least one selected option.
        /// </summary>
        /// <param name="required">Whether at least one selected option is required.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> Required(bool required = true)
        {
            base.Required(required);
            return this;
        }

        /// <summary>
        /// Allows this prompt to return an empty selection.
        /// </summary>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> NotRequired()
        {
            base.NotRequired();
            return this;
        }

        /// <summary>
        /// Sets the maximum number of choices shown per page.
        /// </summary>
        /// <param name="pageSize">The maximum number of choices per page, or <see langword="null"/> to render all choices.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> WithPageSize(int? pageSize)
        {
            base.WithPageSize(pageSize);
            return this;
        }

        /// <summary>
        /// Sets the paging helper text.
        /// </summary>
        /// <param name="text">The paging helper text.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> WithMoreChoicesText(string text)
        {
            base.WithMoreChoicesText(text);
            return this;
        }

        /// <summary>
        /// Sets the keyboard search placeholder text shown before any filter text has been entered.
        /// </summary>
        /// <param name="text">The placeholder text to render in keyboard search mode.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> WithSearchPlaceholderText(string text)
        {
            base.WithSearchPlaceholderText(text);
            return this;
        }

        /// <summary>
        /// Sets the message rendered when submitted input does not match available choices.
        /// </summary>
        /// <param name="message">The invalid selection message.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> WithInvalidSelectionMessage(string message)
        {
            base.WithInvalidSelectionMessage(message);
            return this;
        }

        /// <summary>
        /// Marks the matching item as the default selected item when the user presses Enter without input.
        /// </summary>
        /// <param name="value">The item that should be treated as selected by default.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public MultiSelectionPrompt<T> DefaultValue(T value)
        {
            ResetDefaultSelectionState();
            _defaultValues.Clear();
            _defaultValues.Add(value);
            ApplyDefaultValues(clearExisting: true);
            return this;
        }

        /// <summary>
        /// Marks one typed choice as selected by default without clearing existing default selections.
        /// </summary>
        /// <param name="value">The item that should be selected by default.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public MultiSelectionPrompt<T> Select(T value)
        {
            return Select(new[] { value });
        }

        /// <summary>
        /// Marks typed choices as selected by default without clearing existing default selections.
        /// </summary>
        /// <param name="values">The items that should be selected by default.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public MultiSelectionPrompt<T> Select(params T[] values)
        {
            return Select((IEnumerable<T>)values);
        }

        /// <summary>
        /// Marks typed choices as selected by default without clearing existing default selections.
        /// </summary>
        /// <param name="values">The items that should be selected by default.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public MultiSelectionPrompt<T> Select(IEnumerable<T> values)
        {
            if (values == null)
            {
                return this;
            }

            foreach (T value in values)
            {
                if (!_defaultValues.Any(existing => EqualityComparer<T>.Default.Equals(existing, value)))
                {
                    _defaultValues.Add(value);
                }
            }

            ApplyDefaultValues(clearExisting: false);
            return this;
        }

        /// <summary>
        /// Marks every current and future typed choice as selected by default.
        /// </summary>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> SelectAll()
        {
            base.SelectAll();
            foreach (KeyValuePair<int, T> item in _itemsByIndex)
            {
                if (!_defaultValues.Any(existing => EqualityComparer<T>.Default.Equals(existing, item.Value)))
                {
                    _defaultValues.Add(item.Value);
                }
            }

            return this;
        }

        /// <summary>
        /// Configures the fallback items exposed on <see cref="MultiSelectionResult{T}.SelectedItems"/> when the prompt is canceled.
        /// </summary>
        /// <param name="cancelResult">The fallback items factory.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public MultiSelectionPrompt<T> OnCancel(Func<IReadOnlyList<T>> cancelResult)
        {
            CancelResult = cancelResult ?? throw new ArgumentNullException(nameof(cancelResult));
            return this;
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice.
        /// </summary>
        /// <param name="color">The highlight color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> HighlightColor(ConsoleColor color)
        {
            base.HighlightColor(color);
            return this;
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The highlight color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> HighlightColor(SpectreColor color)
        {
            base.HighlightColor(color);
            return this;
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice from an RGB foreground color.
        /// </summary>
        /// <param name="color">The highlight color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> HighlightColor(RgbColor color)
        {
            base.HighlightColor(color);
            return this;
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> HighlightColor(Style style)
        {
            base.HighlightColor(style);
            return this;
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> HighlightColor(string style)
        {
            base.HighlightColor(style);
            return this;
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice.
        /// </summary>
        /// <param name="color">The highlight color.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> HighlightStyle(ConsoleColor color)
        {
            base.HighlightStyle(color);
            return this;
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> HighlightStyle(Style style)
        {
            base.HighlightStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public new MultiSelectionPrompt<T> HighlightStyle(string style)
        {
            base.HighlightStyle(style);
            return this;
        }

        /// <summary>
        /// Configures helper text rendered under each typed option.
        /// </summary>
        /// <param name="descriptionSelector">The function used to extract helper text from each entity of type <typeparamref name="T"/>.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public MultiSelectionPrompt<T> WithDescriptions(Func<T, string> descriptionSelector)
        {
            DescriptionSelector = descriptionSelector ?? throw new ArgumentNullException(nameof(descriptionSelector));
            RefreshOptionDescriptions();
            return this;
        }

        /// <summary>
        /// Configures hidden search terms used when filtering typed options.
        /// </summary>
        /// <param name="searchTermsSelector">The function used to extract search terms from each entity.</param>
        /// <returns>The current <see cref="MultiSelectionPrompt{T}"/> instance.</returns>
        public MultiSelectionPrompt<T> WithSearchTerms(Func<T, IEnumerable<string>> searchTermsSelector)
        {
            SearchTermsSelector = searchTermsSelector ?? throw new ArgumentNullException(nameof(searchTermsSelector));
            RefreshOptionSearchTerms();
            return this;
        }

        private static string DefaultLabelSelector(T item)
        {
            return item == null ? string.Empty : item.ToString();
        }

        /// <summary>
        /// Adds a new option to the prompt using an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="item">The item to add as an option.</param>
        /// <param name="color">The color of the option text in the console.</param>
        /// <param name="isDefault">Indicates whether this option is selected when the user presses Enter without input.</param>
        /// <returns>The index of the newly added option.</returns>
        public int Add(T item, ConsoleColor? color = null, bool isDefault = false)
        {
            if (LabelSelector == null)
            {
                throw new InvalidOperationException("LabelSelector function must be set before adding items of type T.");
            }

            int index = base.Add(LabelSelector(item), color, isDefault || IsDefaultValue(item));
            GetOptionByIndex(index).Description = DescriptionSelector != null ? DescriptionSelector(item) : null;
            if (SearchTermsSelector != null)
            {
                GetOptionByIndex(index).AddSearchTerms(SearchTermsSelector(item));
            }

            _itemsByIndex[index] = item;
            return index;
        }

        /// <summary>
        /// Adds an unavailable typed option that is displayed but cannot be selected.
        /// </summary>
        /// <param name="item">The item to add as an unavailable option.</param>
        /// <param name="reason">Optional text explaining why the option cannot be selected.</param>
        /// <param name="color">Optional foreground color used to render the disabled option.</param>
        /// <returns>The index of the newly added option.</returns>
        public int AddDisabled(T item, string reason = null, ConsoleColor? color = null)
        {
            if (LabelSelector == null)
            {
                throw new InvalidOperationException("LabelSelector function must be set before adding items of type T.");
            }

            int index = base.AddDisabled(LabelSelector(item), reason, color);
            GetOptionByIndex(index).Description = DescriptionSelector != null ? DescriptionSelector(item) : null;
            if (SearchTermsSelector != null)
            {
                GetOptionByIndex(index).AddSearchTerms(SearchTermsSelector(item));
            }

            _itemsByIndex[index] = item;
            return index;
        }

        /// <summary>
        /// Displays the prompt and maps selected indexes back to typed items added through <see cref="Add(T, ConsoleColor?, bool)"/>.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A typed multi-selection result with the underlying prompt result and selected items.</returns>
        public MultiSelectionResult<T> RenderSelection(CancellationToken cancellationToken = default)
        {
            MultiSelectionResult result = Render(cancellationToken);
            T[] selectedItems = result.Indexes
                .Where(index => _itemsByIndex.ContainsKey(index))
                .Select(index => _itemsByIndex[index])
                .ToArray();
            if (result.IsCanceled && CancelResult != null)
            {
                selectedItems = (CancelResult() ?? new T[0]).ToArray();
            }

            return new MultiSelectionResult<T>(result, selectedItems);
        }

        private bool IsDefaultValue(T item)
        {
            return _defaultValues.Any(value => EqualityComparer<T>.Default.Equals(value, item));
        }

        private void ApplyDefaultValues(bool clearExisting)
        {
            if (_defaultValues.Count == 0 || _itemsByIndex.Count == 0)
            {
                return;
            }

            if (clearExisting)
            {
                foreach (SelectOption option in Options.Where(option => !option.IsSeparator))
                {
                    option.IsDefault = false;
                    option.Selected = false;
                }
            }

            foreach (KeyValuePair<int, T> item in _itemsByIndex)
            {
                if (IsDefaultValue(item.Value))
                {
                    SelectOption option = GetOptionByIndex(item.Key);
                    if (!option.IsDisabled)
                    {
                        option.IsDefault = true;
                        option.Selected = true;
                    }
                }
            }
        }

        private void RefreshOptionDescriptions()
        {
            foreach (KeyValuePair<int, T> item in _itemsByIndex)
            {
                GetOptionByIndex(item.Key).Description = DescriptionSelector != null ? DescriptionSelector(item.Value) : null;
            }
        }

        private void RefreshOptionLabels()
        {
            foreach (KeyValuePair<int, T> item in _itemsByIndex)
            {
                GetOptionByIndex(item.Key).Label = LabelSelector(item.Value);
            }

            foreach (KeyValuePair<SelectOption, T> separator in _separatorItemsByOption)
            {
                separator.Key.Label = LabelSelector(separator.Value);
            }
        }

        private void RefreshOptionSearchTerms()
        {
            foreach (KeyValuePair<int, T> item in _itemsByIndex)
            {
                SelectOption option = GetOptionByIndex(item.Key);
                option.SearchTerms.Clear();
                if (SearchTermsSelector != null)
                {
                    option.AddSearchTerms(SearchTermsSelector(item.Value));
                }
            }
        }
    }
}
