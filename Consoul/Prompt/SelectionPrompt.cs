using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;
using Options = ConsoulLibrary.RenderOptions;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a callback invoked when a typed prompt option is selected.
    /// </summary>
    /// <typeparam name="TTarget">The type of the selected target object.</typeparam>
    /// <param name="choice">The selected target object.</param>
    public delegate void PromptChoiceCallback<TTarget>(TTarget choice);
    /// <summary>
    /// Represents a selection prompt with generic entity type <typeparamref name="T"/>, allowing customized label selection for each item.
    /// </summary>
    /// <typeparam name="T">The type of the entities to be represented as options in the prompt.</typeparam>
    public class SelectionPrompt<T> : SelectionPrompt, IPrompt<SelectionResult<T>>
    {
        private readonly Dictionary<int, T> _itemsByIndex = new Dictionary<int, T>();
        private readonly Dictionary<SelectOption, T> _separatorItemsByOption = new Dictionary<SelectOption, T>();
        private bool _hasDefaultValue;
        private T _defaultValue;

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
        /// Gets or sets the fallback item returned on <see cref="SelectionResult{T}.SelectedItem"/> when the prompt is canceled.
        /// </summary>
        public new Func<T> CancelResult { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectionPrompt{T}"/> class with a message, an optional clear flag, and a label selector function.
        /// </summary>
        /// <param name="message">The message to display when the prompt is shown.</param>
        /// <param name="clear">Indicates whether to clear the console when the prompt is displayed.</param>
        /// <param name="labelSelector">The function used to extract the label from each entity of type <typeparamref name="T"/>.</param>
        public SelectionPrompt(string message, bool clear = false, Func<T, string> labelSelector = null) : base(message, clear)
        {
            LabelSelector = labelSelector ?? DefaultLabelSelector;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectionPrompt{T}"/> class with an empty title and default label conversion.
        /// </summary>
        public SelectionPrompt() : this(string.Empty, false, null)
        {
        }

        /// <summary>
        /// Shows the prompt through the specified console facade.
        /// </summary>
        /// <param name="console">The console facade used to render the prompt.</param>
        /// <returns>The typed selection result.</returns>
        SelectionResult<T> IPrompt<SelectionResult<T>>.Show(IAnsiConsole console)
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
        /// <returns>A task that resolves to the typed selection result.</returns>
        Task<SelectionResult<T>> IPrompt<SelectionResult<T>>.ShowAsync(IAnsiConsole console, CancellationToken cancellationToken)
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
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> Title(string title)
        {
            base.Title(title);
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message.
        /// </summary>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> PromptStyle(ConsoleColor color)
        {
            base.PromptStyle(color);
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> PromptStyle(SpectreColor color)
        {
            base.PromptStyle(color);
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message from an RGB foreground color.
        /// </summary>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> PromptStyle(RgbColor color)
        {
            base.PromptStyle(color);
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> PromptStyle(Style style)
        {
            base.PromptStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> PromptStyle(string style)
        {
            base.PromptStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message.
        /// </summary>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> Style(ConsoleColor color)
        {
            base.Style(color);
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> Style(Style style)
        {
            base.Style(style);
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> Style(string style)
        {
            base.Style(style);
            return this;
        }

        /// <summary>
        /// Sets the helper text shown above the options when line-based input is used.
        /// </summary>
        /// <param name="text">The instruction text.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> InstructionsText(string text)
        {
            base.InstructionsText(text);
            return this;
        }

        /// <summary>
        /// Sets the color used for prompt instruction text.
        /// </summary>
        /// <param name="color">The instruction text color.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> InstructionStyle(ConsoleColor color)
        {
            base.InstructionStyle(color);
            return this;
        }

        /// <summary>
        /// Sets the color used for prompt instruction text from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The instruction text color.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> InstructionStyle(SpectreColor color)
        {
            base.InstructionStyle(color);
            return this;
        }

        /// <summary>
        /// Sets the color used for prompt instruction text from an RGB foreground color.
        /// </summary>
        /// <param name="color">The instruction text color.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> InstructionStyle(RgbColor color)
        {
            base.InstructionStyle(color);
            return this;
        }

        /// <summary>
        /// Sets the color used for prompt instruction text from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> InstructionStyle(Style style)
        {
            base.InstructionStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the color used for prompt instruction text from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#94a3b8</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> InstructionStyle(string style)
        {
            base.InstructionStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the color used for option descriptions.
        /// </summary>
        /// <param name="color">The option description foreground color.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> OptionDescriptionStyle(ConsoleColor color)
        {
            base.OptionDescriptionStyle(color);
            return this;
        }

        /// <summary>
        /// Sets the color used for option descriptions from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The option description foreground color.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> OptionDescriptionStyle(SpectreColor color)
        {
            base.OptionDescriptionStyle(color);
            return this;
        }

        /// <summary>
        /// Sets the color used for option descriptions from an RGB foreground color.
        /// </summary>
        /// <param name="color">The option description foreground color.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> OptionDescriptionStyle(RgbColor color)
        {
            base.OptionDescriptionStyle(color);
            return this;
        }

        /// <summary>
        /// Sets the style used for option descriptions.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> OptionDescriptionStyle(Style style)
        {
            base.OptionDescriptionStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the style used for option descriptions from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>italic</c>, <c>#94a3b8</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> OptionDescriptionStyle(string style)
        {
            base.OptionDescriptionStyle(style);
            return this;
        }

        /// <summary>
        /// Configures the converter used to render typed choices as labels.
        /// </summary>
        /// <param name="converter">The converter used to render typed choices.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public SelectionPrompt<T> UseConverter(Func<T, string> converter)
        {
            LabelSelector = converter ?? throw new ArgumentNullException(nameof(converter));
            RefreshOptionLabels();
            return this;
        }

        /// <summary>
        /// Adds multiple typed choices using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="choices">The choices to add.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public SelectionPrompt<T> AddChoices(params T[] choices)
        {
            return AddChoices((IEnumerable<T>)choices);
        }

        /// <summary>
        /// Adds multiple typed choices using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="choices">The choices to add.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public SelectionPrompt<T> AddChoices(IEnumerable<T> choices)
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
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public SelectionPrompt<T> AddChoiceGroup(T group, params T[] choices)
        {
            return AddChoiceGroup(group, (IEnumerable<T>)choices);
        }

        /// <summary>
        /// Adds a display-only group heading followed by typed choices using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="group">The item used to render the group heading.</param>
        /// <param name="choices">The choices to add under the group.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public SelectionPrompt<T> AddChoiceGroup(T group, IEnumerable<T> choices)
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
        /// <param name="isDefault">Whether the group is the default selection when groups are independently selectable.</param>
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
        /// <param name="isDefault">Whether the child is the default selection.</param>
        /// <param name="renderStyle">The render style used for the child option.</param>
        /// <returns>The added child option.</returns>
        public SelectOption AddChild(SelectOption parent, T item, ConsoleColor? color = null, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
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
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> SearchEnabled(bool enabled = true)
        {
            base.SearchEnabled(enabled);
            return this;
        }

        /// <summary>
        /// Enables slash-command filtering for this prompt using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="enabled">Whether filtering should be enabled.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> EnableSearch(bool enabled = true)
        {
            return SearchEnabled(enabled);
        }

        /// <summary>
        /// Enables keyboard navigation for this prompt.
        /// </summary>
        /// <param name="enabled">Whether keyboard navigation should be enabled.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> UseKeyboardNavigation(bool enabled = true)
        {
            base.UseKeyboardNavigation(enabled);
            return this;
        }

        /// <summary>
        /// Sets whether keyboard navigation wraps at the first and last available options.
        /// </summary>
        /// <param name="enabled">Whether arrow-key navigation should wrap around.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> UseWrapAround(bool enabled = true)
        {
            base.UseWrapAround(enabled);
            return this;
        }

        /// <summary>
        /// Sets the maximum number of choices shown per page.
        /// </summary>
        /// <param name="pageSize">The maximum number of choices per page, or <see langword="null"/> to render all choices.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> WithPageSize(int? pageSize)
        {
            base.WithPageSize(pageSize);
            return this;
        }

        /// <summary>
        /// Sets the paging helper text.
        /// </summary>
        /// <param name="text">The paging helper text.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> WithMoreChoicesText(string text)
        {
            base.WithMoreChoicesText(text);
            return this;
        }

        /// <summary>
        /// Sets the keyboard search placeholder text shown before any filter text has been entered.
        /// </summary>
        /// <param name="text">The placeholder text to render in keyboard search mode.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> WithSearchPlaceholderText(string text)
        {
            base.WithSearchPlaceholderText(text);
            return this;
        }

        /// <summary>
        /// Sets the message rendered when submitted input does not match an available choice.
        /// </summary>
        /// <param name="message">The invalid selection message.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> WithInvalidSelectionMessage(string message)
        {
            base.WithInvalidSelectionMessage(message);
            return this;
        }

        /// <summary>
        /// Marks the matching item as the default selection when the user presses Enter without input.
        /// </summary>
        /// <param name="value">The item that should be treated as the default selection.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public SelectionPrompt<T> DefaultValue(T value)
        {
            _hasDefaultValue = true;
            _defaultValue = value;
            ApplyDefaultValue();
            return this;
        }

        /// <summary>
        /// Configures the fallback item exposed on <see cref="SelectionResult{T}.SelectedItem"/> when the prompt is canceled.
        /// </summary>
        /// <param name="cancelResult">The fallback item factory.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public SelectionPrompt<T> OnCancel(Func<T> cancelResult)
        {
            CancelResult = cancelResult ?? throw new ArgumentNullException(nameof(cancelResult));
            return this;
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice.
        /// </summary>
        /// <param name="color">The highlight color.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> HighlightColor(ConsoleColor color)
        {
            base.HighlightColor(color);
            return this;
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The highlight color.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> HighlightColor(SpectreColor color)
        {
            base.HighlightColor(color);
            return this;
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice from an RGB foreground color.
        /// </summary>
        /// <param name="color">The highlight color.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> HighlightColor(RgbColor color)
        {
            base.HighlightColor(color);
            return this;
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> HighlightColor(Style style)
        {
            base.HighlightColor(style);
            return this;
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> HighlightColor(string style)
        {
            base.HighlightColor(style);
            return this;
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice.
        /// </summary>
        /// <param name="color">The highlight color.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> HighlightStyle(ConsoleColor color)
        {
            base.HighlightStyle(color);
            return this;
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> HighlightStyle(Style style)
        {
            base.HighlightStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public new SelectionPrompt<T> HighlightStyle(string style)
        {
            base.HighlightStyle(style);
            return this;
        }

        /// <summary>
        /// Configures helper text rendered under each typed option.
        /// </summary>
        /// <param name="descriptionSelector">The function used to extract helper text from each entity of type <typeparamref name="T"/>.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public SelectionPrompt<T> WithDescriptions(Func<T, string> descriptionSelector)
        {
            DescriptionSelector = descriptionSelector ?? throw new ArgumentNullException(nameof(descriptionSelector));
            RefreshOptionDescriptions();
            return this;
        }

        /// <summary>
        /// Configures hidden search terms used when filtering typed options.
        /// </summary>
        /// <param name="searchTermsSelector">The function used to extract search terms from each entity.</param>
        /// <returns>The current <see cref="SelectionPrompt{T}"/> instance.</returns>
        public SelectionPrompt<T> WithSearchTerms(Func<T, IEnumerable<string>> searchTermsSelector)
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
        /// Initializes a new typed prompt with an initial set of options.
        /// </summary>
        /// <param name="message">The message to display when the prompt is shown.</param>
        /// <param name="clear">Indicates whether to clear the console when the prompt is displayed.</param>
        /// <param name="labelSelector">The function used to extract the label from each entity of type <typeparamref name="T"/>.</param>
        /// <param name="options">Initial options to add to the prompt.</param>
        public SelectionPrompt(string message, bool clear = false, Func<T, string> labelSelector = null, params T[] options) : this(message, clear, labelSelector)
        {
            foreach (var item in options)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Adds a new option to the prompt using an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="item">The instance of <typeparamref name="T"/> to add as an option.</param>
        /// <param name="color">The color of the option text in the console.</param>
        /// <param name="isDefault">Indicates whether this option is the default selection.</param>
        /// <param name="renderStyle">Specifies the rendering style for this option.</param>
        /// <returns>The index of the newly added option.</returns>
        public int Add(T item, ConsoleColor? color = null, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            if (LabelSelector == null)
            {
                throw new InvalidOperationException("LabelSelector function must be set before adding items of type T.");
            }

            int index = base.Add(LabelSelector(item), color, isDefault || IsDefaultValue(item), renderStyle);
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
        /// <param name="item">The instance of <typeparamref name="T"/> to add as an unavailable option.</param>
        /// <param name="reason">Optional text explaining why the option cannot be selected.</param>
        /// <param name="color">Optional foreground color used to render the disabled option.</param>
        /// <param name="renderStyle">Specifies the rendering style for this option.</param>
        /// <returns>The index of the newly added option.</returns>
        public int AddDisabled(T item, string reason = null, ConsoleColor? color = null, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            if (LabelSelector == null)
            {
                throw new InvalidOperationException("LabelSelector function must be set before adding items of type T.");
            }

            int index = base.AddDisabled(LabelSelector(item), reason, color, renderStyle);
            GetOptionByIndex(index).Description = DescriptionSelector != null ? DescriptionSelector(item) : null;
            if (SearchTermsSelector != null)
            {
                GetOptionByIndex(index).AddSearchTerms(SearchTermsSelector(item));
            }

            _itemsByIndex[index] = item;
            return index;
        }

        /// <summary>
        /// Displays the prompt and maps the selected index back to the typed item added through <see cref="Add(T, ConsoleColor?, bool, OptionRenderStyle)"/>.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A typed selection result with the underlying prompt result and selected item.</returns>
        public SelectionResult<T> RenderSelection(CancellationToken cancellationToken = default)
        {
            PromptResult result = Render(cancellationToken);
            T selectedItem = default(T);
            if (result.HasSelection)
            {
                _itemsByIndex.TryGetValue(result.Index, out selectedItem);
            }
            else if (result.IsCanceled && CancelResult != null)
            {
                selectedItem = CancelResult();
            }

            return new SelectionResult<T>(result, selectedItem);
        }

        private bool IsDefaultValue(T item)
        {
            return _hasDefaultValue && EqualityComparer<T>.Default.Equals(item, _defaultValue);
        }

        private void ApplyDefaultValue()
        {
            if (!_hasDefaultValue || _itemsByIndex.Count == 0)
            {
                return;
            }

            foreach (SelectOption option in Options.Where(option => !option.IsSeparator))
            {
                option.IsDefault = false;
            }

            foreach (KeyValuePair<int, T> item in _itemsByIndex)
            {
                if (EqualityComparer<T>.Default.Equals(item.Value, _defaultValue))
                {
                    GetOptionByIndex(item.Key).IsDefault = true;
                    return;
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

        /// <summary>
        /// Creates and shows a new prompt with the given message and options, then returns the selected item of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="message">The message to display for the prompt.</param>
        /// <param name="labelSelector">A function to extract the label for each option.</param>
        /// <param name="clear">Indicates whether to clear the console when the prompt is displayed.</param>
        /// <param name="options">The options to present for selection.</param>
        /// <returns>The selected object of type <typeparamref name="T"/>, or null if no valid selection was made.</returns>
        public static T Render(string message, Func<T, string> labelSelector, bool clear = false, params T[] options)
        {
            var prompt = new SelectionPrompt<T>(message, clear, labelSelector, options);
            return prompt.RenderSelection().SelectedItem;
        }
    }

    /// <summary>
    /// Represents a prompt that renders an indexable list of options for the user to choose from.
    /// </summary>
    public class SelectionPrompt : IPrompt<PromptResult>
    {
        private ConsoleColor _optionDescriptionColor = RenderOptions.SubnoteColor;

        /// <summary>
        /// Stores the mutable option list used by derived prompt types.
        /// </summary>
        protected List<SelectOption> _options { get; set; }

        /// <summary>
        /// Gets the label of the selectable <see cref="SelectOption"/> at the specified index.
        /// </summary>
        /// <param name="index">Zero-based selectable index of the <see cref="SelectOption"/></param>
        /// <returns>The label of the <see cref="SelectOption"/> at the specified index.</returns>
        public string this[int index] => GetOptionByIndex(index).Label;

        /// <summary>
        /// Gets or sets the message to be displayed to the user when the prompt is shown.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Indicates whether to clear the console window upon showing or re-showing the prompt.
        /// </summary>
        public bool ClearConsole { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of selectable options rendered per page. Set to <see langword="null"/> to render all options.
        /// </summary>
        public int? PageSize { get; set; }

        /// <summary>
        /// Gets or sets the paging helper text. Placeholders are first option, last option, total options, current page, and total pages.
        /// </summary>
        public string MoreChoicesText { get; set; } = "Showing options {0}-{1} of {2}. Type next/prev to change pages.";

        /// <summary>
        /// Gets or sets the color used for the prompt message.
        /// </summary>
        public ConsoleColor PromptColor { get; set; } = RenderOptions.PromptColor;

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
        public string InstructionsMessage { get; set; } = "Choose the corresponding number from the options below:";

        /// <summary>
        /// Gets or sets the color used for prompt instruction text.
        /// </summary>
        public ConsoleColor InstructionColor { get; set; } = RenderOptions.SubnoteColor;

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
        /// Gets or sets a value indicating whether configured option shortcuts can select prompt options.
        /// </summary>
        public bool ShortcutsEnabled { get; set; } = true;

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
        /// Gets or sets the message rendered when submitted input does not match an available choice.
        /// </summary>
        public string InvalidSelectionMessage { get; set; } = "Invalid selection!";

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
        public string KeyboardNavigationHelpText { get; set; } = "Use Up/Down to move, Enter to select, Esc to cancel.";

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
        public ConsoleColor KeyboardSelectionColor { get; set; } = RenderOptions.PromptColor;

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
        /// Gets or sets the style used for disabled options.
        /// </summary>
        public Style DisabledStyle { get; set; }

        /// <summary>
        /// Gets or sets the style used for visible text that matches the active search filter.
        /// </summary>
        public Style SearchHighlightStyle { get; set; }

        /// <summary>
        /// Gets or sets how grouped choices participate in selection.
        /// </summary>
        public SelectionMode Mode { get; set; } = SelectionMode.Leaf;

        /// <summary>
        /// Gets or sets a fallback result returned when the prompt is canceled.
        /// </summary>
        public Func<PromptResult> CancelResult { get; set; }

        /// <summary>
        /// Gets the collection of <see cref="SelectOption"/> objects that represent the selectable options.
        /// </summary>
        public IEnumerable<SelectOption> Options => _options;

        /// <summary>
        /// Gets the number of selectable or disabled options in the prompt, excluding separators.
        /// </summary>
        public int Count => _options.Count(option => !option.IsSeparator);

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectionPrompt"/> class with a message and an optional clear flag.
        /// </summary>
        /// <param name="message">The message to display when the prompt is shown.</param>
        /// <param name="clear">Indicates whether to clear the console when the prompt is displayed.</param>
        public SelectionPrompt(string message, bool clear = false)
        {
            Message = message;
            ClearConsole = clear;
            _options = new List<SelectOption>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectionPrompt"/> class with a message, an optional clear flag, and a list of options.
        /// </summary>
        /// <param name="message">The message to display when the prompt is shown.</param>
        /// <param name="clear">Indicates whether to clear the console when the prompt is displayed.</param>
        /// <param name="options">The list of string options to be added to the prompt.</param>
        public SelectionPrompt(string message, bool clear = false, params string[] options) : this(message, clear)
        {
            foreach (string option in options)
            {
                Add(option);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectionPrompt"/> class with a message, an optional clear flag, and a list of <see cref="SelectOption"/> objects.
        /// </summary>
        /// <param name="message">The message to display when the prompt is shown.</param>
        /// <param name="clear">Indicates whether to clear the console when the prompt is displayed.</param>
        /// <param name="options">The list of <see cref="SelectOption"/> objects to be added to the prompt.</param>
        public SelectionPrompt(string message, bool clear = false, params SelectOption[] options) : this(message, clear)
        {
            int i = 0;
            foreach (SelectOption option in options)
            {
                option.Index = option.IsSeparator ? -1 : i;
                _options.Add(option);
                if (!option.IsSeparator)
                {
                    i++;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectionPrompt"/> class with an empty title.
        /// </summary>
        public SelectionPrompt() : this(string.Empty, false)
        {
        }

        /// <summary>
        /// Shows the prompt through the specified console facade.
        /// </summary>
        /// <param name="console">The console facade used to render the prompt.</param>
        /// <returns>The selection result.</returns>
        public PromptResult Show(IAnsiConsole console)
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
        /// <returns>A task that resolves to the selection result.</returns>
        public Task<PromptResult> ShowAsync(IAnsiConsole console, CancellationToken cancellationToken = default)
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
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt Title(string title)
        {
            Message = title ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message.
        /// </summary>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt PromptStyle(ConsoleColor color)
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
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt PromptStyle(SpectreColor color)
        {
            return PromptStyle(color.ToRgbColor());
        }

        /// <summary>
        /// Sets the color used for the prompt message from an RGB foreground color.
        /// </summary>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt PromptStyle(RgbColor color)
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
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt PromptStyle(Style style)
        {
            ApplyPromptStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt PromptStyle(string style)
        {
            return PromptStyle(ConsoulLibrary.Style.Parse(style));
        }

        /// <summary>
        /// Sets the color used for the prompt message.
        /// </summary>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt Style(ConsoleColor color)
        {
            return PromptStyle(color);
        }

        /// <summary>
        /// Sets the color used for the prompt message from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt Style(Style style)
        {
            return PromptStyle(style);
        }

        /// <summary>
        /// Sets the color used for the prompt message from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt Style(string style)
        {
            return PromptStyle(style);
        }

        /// <summary>
        /// Sets the helper text shown above the options when line-based input is used.
        /// </summary>
        /// <param name="text">The instruction text.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt InstructionsText(string text)
        {
            InstructionsMessage = text ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the color used for prompt instruction text.
        /// </summary>
        /// <param name="color">The instruction text color.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt InstructionStyle(ConsoleColor color)
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
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt InstructionStyle(SpectreColor color)
        {
            return InstructionStyle(color.ToRgbColor());
        }

        /// <summary>
        /// Sets the color used for prompt instruction text from an RGB foreground color.
        /// </summary>
        /// <param name="color">The instruction text color.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt InstructionStyle(RgbColor color)
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
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt InstructionStyle(Style style)
        {
            ApplyInstructionStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the color used for prompt instruction text from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>aqua</c>, <c>#94a3b8</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt InstructionStyle(string style)
        {
            return InstructionStyle(ConsoulLibrary.Style.Parse(style));
        }

        /// <summary>
        /// Sets the color used for option descriptions.
        /// </summary>
        /// <param name="color">The option description foreground color.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt OptionDescriptionStyle(ConsoleColor color)
        {
            OptionDescriptionColor = color;
            return this;
        }

        /// <summary>
        /// Sets the color used for option descriptions from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The option description foreground color.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt OptionDescriptionStyle(SpectreColor color)
        {
            return OptionDescriptionStyle(color.ToRgbColor());
        }

        /// <summary>
        /// Sets the color used for option descriptions from an RGB foreground color.
        /// </summary>
        /// <param name="color">The option description foreground color.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt OptionDescriptionStyle(RgbColor color)
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
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt OptionDescriptionStyle(Style style)
        {
            ApplyOptionDescriptionStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the style used for option descriptions from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>grey</c>, <c>italic</c>, <c>#94a3b8</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt OptionDescriptionStyle(string style)
        {
            return OptionDescriptionStyle(ConsoulLibrary.Style.Parse(style));
        }

        /// <summary>
        /// Adds multiple string choices using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="choices">The choices to add.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt AddChoices(params string[] choices)
        {
            return AddChoices((IEnumerable<string>)choices);
        }

        /// <summary>
        /// Adds multiple string choices using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="choices">The choices to add.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt AddChoices(IEnumerable<string> choices)
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
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt AddChoiceGroup(string group, params string[] choices)
        {
            return AddChoiceGroup(group, (IEnumerable<string>)choices);
        }

        /// <summary>
        /// Adds a display-only group heading followed by string choices using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="group">The group heading text.</param>
        /// <param name="choices">The choices to add under the group.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt AddChoiceGroup(string group, IEnumerable<string> choices)
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
        /// <param name="isDefault">Whether the group is the default selection when groups are independently selectable.</param>
        /// <returns>The added group option.</returns>
        public SelectOption AddGroup(string label, ConsoleColor? color = null, bool isDefault = false)
        {
            SelectOption group = AddChoice(label);
            group.Color = color ?? group.Color;
            group.IsDefault = isDefault;
            group.IsGroup = true;
            return group;
        }

        /// <summary>
        /// Adds a child choice beneath an existing grouped option.
        /// </summary>
        /// <param name="parent">The prompt-owned group option that should contain the child.</param>
        /// <param name="label">The child choice label.</param>
        /// <param name="color">Optional child foreground color.</param>
        /// <param name="isDefault">Whether the child is the default selection.</param>
        /// <param name="renderStyle">The render style used for the child option.</param>
        /// <returns>The added child option.</returns>
        public SelectOption AddChild(SelectOption parent, string label, ConsoleColor? color = null, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            if (!_options.Contains(parent) || parent.IsSeparator)
            {
                throw new ArgumentException("Child options can only be added to prompt-owned selectable options.", nameof(parent));
            }

            var child = new SelectOption(GetNextOptionIndex(), label, color ?? RenderOptions.OptionColor, isDefault, renderStyle);
            AttachChildFactory(child);
            parent.AddChildOption(child);
            _options.Add(child);
            return child;
        }

        /// <summary>
        /// Enables slash-command filtering for this prompt.
        /// </summary>
        /// <param name="enabled">Whether filtering should be enabled.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt SearchEnabled(bool enabled = true)
        {
            FilterEnabled = enabled;
            return this;
        }

        /// <summary>
        /// Enables slash-command filtering for this prompt using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="enabled">Whether filtering should be enabled.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt EnableSearch(bool enabled = true)
        {
            return SearchEnabled(enabled);
        }

        /// <summary>
        /// Enables keyboard navigation for this prompt.
        /// </summary>
        /// <param name="enabled">Whether keyboard navigation should be enabled.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt UseKeyboardNavigation(bool enabled = true)
        {
            KeyboardNavigationEnabled = enabled;
            return this;
        }

        /// <summary>
        /// Sets whether keyboard navigation wraps at the first and last available options.
        /// </summary>
        /// <param name="enabled">Whether arrow-key navigation should wrap around.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt UseWrapAround(bool enabled = true)
        {
            WrapAround = enabled;
            return this;
        }

        /// <summary>
        /// Sets the maximum number of choices shown per page.
        /// </summary>
        /// <param name="pageSize">The maximum number of choices per page, or <see langword="null"/> to render all choices.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt WithPageSize(int? pageSize)
        {
            PageSize = pageSize.HasValue ? Math.Max(1, pageSize.Value) : (int?)null;
            return this;
        }

        /// <summary>
        /// Sets the paging helper text.
        /// </summary>
        /// <param name="text">The paging helper text.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt WithMoreChoicesText(string text)
        {
            MoreChoicesText = text ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the keyboard search placeholder text shown before any filter text has been entered.
        /// </summary>
        /// <param name="text">The placeholder text to render in keyboard search mode.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt WithSearchPlaceholderText(string text)
        {
            SearchPlaceholderText = text;
            return this;
        }

        /// <summary>
        /// Sets the message rendered when submitted input does not match an available choice.
        /// </summary>
        /// <param name="message">The invalid selection message.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt WithInvalidSelectionMessage(string message)
        {
            InvalidSelectionMessage = message ?? throw new ArgumentNullException(nameof(message));
            return this;
        }

        /// <summary>
        /// Marks the option with the matching label as the default selection when the user presses Enter without input.
        /// </summary>
        /// <param name="label">The option label that should be treated as the default selection.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt DefaultValue(string label)
        {
            foreach (SelectOption option in Options.Where(option => !option.IsSeparator))
            {
                option.IsDefault = string.Equals(option.Label, label, StringComparison.Ordinal);
            }

            return this;
        }

        /// <summary>
        /// Configures the prompt result returned when the prompt is canceled.
        /// </summary>
        /// <param name="cancelResult">The fallback result factory.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt OnCancel(Func<PromptResult> cancelResult)
        {
            CancelResult = cancelResult ?? throw new ArgumentNullException(nameof(cancelResult));
            return this;
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice.
        /// </summary>
        /// <param name="color">The highlight color.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt HighlightColor(ConsoleColor color)
        {
            KeyboardSelectionColor = color;
            KeyboardSelectionDecoration = Decoration.None;
            KeyboardSelectionForegroundRgb = null;
            KeyboardSelectionBackgroundRgb = null;
            return this;
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The highlight color.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt HighlightColor(SpectreColor color)
        {
            return HighlightColor(color.ToRgbColor());
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice from an RGB foreground color.
        /// </summary>
        /// <param name="color">The highlight color.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt HighlightColor(RgbColor color)
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
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt HighlightColor(Style style)
        {
            ApplyKeyboardSelectionStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt HighlightColor(string style)
        {
            return HighlightColor(ConsoulLibrary.Style.Parse(style));
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice.
        /// </summary>
        /// <param name="color">The highlight color.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt HighlightStyle(ConsoleColor color)
        {
            return HighlightColor(color);
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt HighlightStyle(Style style)
        {
            return HighlightColor(style);
        }

        /// <summary>
        /// Sets the color used for the currently highlighted keyboard choice from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt HighlightStyle(string style)
        {
            return HighlightColor(style);
        }

        /// <summary>
        /// Adds a new <see cref="SelectOption"/> to the prompt.
        /// </summary>
        /// <param name="label">The label of the option.</param>
        /// <param name="color">The color of the option text in the console.</param>
        /// <param name="isDefault">Indicates whether this option is the default selection.</param>
        /// <param name="renderStyle">Sets the style for how the item is rendered in the console.</param>
        /// <returns>The index of the newly added option.</returns>
        public int Add(string label, ConsoleColor? color = null, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            if (color == null)
                color = RenderOptions.OptionColor;
            var option = new SelectOption(GetNextOptionIndex(), label, (ConsoleColor)color, isDefault, renderStyle);
            AttachChildFactory(option);
            _options.Add(option);
            return option.Index;
        }

        /// <summary>
        /// Adds a new <see cref="SelectOption"/> with helper text rendered under the option.
        /// </summary>
        /// <param name="label">The label of the option.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <param name="color">The color of the option text in the console.</param>
        /// <param name="isDefault">Indicates whether this option is the default selection.</param>
        /// <param name="renderStyle">Sets the style for how the item is rendered in the console.</param>
        /// <returns>The index of the newly added option.</returns>
        public int AddDescribed(string label, string description, ConsoleColor? color = null, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            int index = Add(label, color, isDefault, renderStyle);
            GetOptionByIndex(index).Description = description;
            return index;
        }

        /// <summary>
        /// Adds a new <see cref="SelectOption"/> with shortcut text that can select the option.
        /// </summary>
        /// <param name="label">The label of the option.</param>
        /// <param name="shortcut">Shortcut text that can select the option.</param>
        /// <param name="color">The color of the option text in the console.</param>
        /// <param name="isDefault">Indicates whether this option is the default selection.</param>
        /// <param name="renderStyle">Sets the style for how the item is rendered in the console.</param>
        /// <returns>The index of the newly added option.</returns>
        public int AddShortcut(string label, string shortcut, ConsoleColor? color = null, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            int index = Add(label, color, isDefault, renderStyle);
            GetOptionByIndex(index).UseShortcut(shortcut);
            return index;
        }

        /// <summary>
        /// Adds a new <see cref="SelectOption"/> with helper text and shortcut text that can select the option.
        /// </summary>
        /// <param name="label">The label of the option.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <param name="shortcut">Shortcut text that can select the option.</param>
        /// <param name="color">The color of the option text in the console.</param>
        /// <param name="isDefault">Indicates whether this option is the default selection.</param>
        /// <param name="renderStyle">Sets the style for how the item is rendered in the console.</param>
        /// <returns>The index of the newly added option.</returns>
        public int AddDescribedShortcut(string label, string description, string shortcut, ConsoleColor? color = null, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            int index = AddDescribed(label, description, color, isDefault, renderStyle);
            GetOptionByIndex(index).UseShortcut(shortcut);
            return index;
        }

        /// <summary>
        /// Adds hidden search terms to an existing option.
        /// </summary>
        /// <param name="index">Zero-based selectable option index.</param>
        /// <param name="searchTerms">Search terms or aliases for the option.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt AddSearchTerms(int index, params string[] searchTerms)
        {
            GetOptionByIndex(index).AddSearchTerms(searchTerms);
            return this;
        }

        /// <summary>
        /// Adds hidden search terms to an existing option.
        /// </summary>
        /// <param name="index">Zero-based selectable option index.</param>
        /// <param name="searchTerms">Search terms or aliases for the option.</param>
        /// <returns>The current <see cref="SelectionPrompt"/> instance.</returns>
        public SelectionPrompt AddSearchTerms(int index, IEnumerable<string> searchTerms)
        {
            GetOptionByIndex(index).AddSearchTerms(searchTerms);
            return this;
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
        /// <param name="renderStyle">Sets the style for how the item is rendered in the console.</param>
        /// <returns>The index of the newly added option.</returns>
        public int AddDisabled(string label, string reason = null, ConsoleColor? color = null, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            var option = new SelectOption(GetNextOptionIndex(), label, color ?? ConsoleColor.DarkGray, false, renderStyle)
            {
                DisabledColor = color ?? ConsoleColor.DarkGray
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
        /// <param name="renderStyle">Sets the style for how the item is rendered in the console.</param>
        /// <returns>The index of the newly added option.</returns>
        public int AddDisabledDescribed(string label, string description, string reason = null, ConsoleColor? color = null, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            int index = AddDisabled(label, reason, color, renderStyle);
            GetOptionByIndex(index).Description = description;
            return index;
        }

        /// <summary>
        /// Adds an unavailable option with shortcut text that can attempt to select the option.
        /// </summary>
        /// <param name="label">The label of the disabled option.</param>
        /// <param name="shortcut">Shortcut text that can attempt to select the option.</param>
        /// <param name="reason">Optional text explaining why the option cannot be selected.</param>
        /// <param name="color">Optional foreground color used to render the disabled option.</param>
        /// <param name="renderStyle">Sets the style for how the item is rendered in the console.</param>
        /// <returns>The index of the newly added option.</returns>
        public int AddDisabledShortcut(string label, string shortcut, string reason = null, ConsoleColor? color = null, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            int index = AddDisabled(label, reason, color, renderStyle);
            GetOptionByIndex(index).UseShortcut(shortcut);
            return index;
        }

        /// <summary>
        /// Adds an unavailable option with helper text and shortcut text that can attempt to select the option.
        /// </summary>
        /// <param name="label">The label of the disabled option.</param>
        /// <param name="description">Optional helper text rendered under the option.</param>
        /// <param name="shortcut">Shortcut text that can attempt to select the option.</param>
        /// <param name="reason">Optional text explaining why the option cannot be selected.</param>
        /// <param name="color">Optional foreground color used to render the disabled option.</param>
        /// <param name="renderStyle">Sets the style for how the item is rendered in the console.</param>
        /// <returns>The index of the newly added option.</returns>
        public int AddDisabledDescribedShortcut(string label, string description, string shortcut, string reason = null, ConsoleColor? color = null, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            int index = AddDisabledDescribed(label, description, reason, color, renderStyle);
            GetOptionByIndex(index).UseShortcut(shortcut);
            return index;
        }

        /// <summary>
        /// Clears all <see cref="SelectOption"/> objects from the prompt.
        /// </summary>
        public void Clear()
        {
            _options.Clear();
        }

        /// <summary>
        /// Displays the prompt options to the user and waits for the user to make a selection.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="PromptResult"/> that describes the user's selection or cancellation.</returns>
        public PromptResult Render(CancellationToken cancellationToken = default)
        {
            string[] escapePhrases = new string[]
            {
                "go back",
                "back",
                "exit",
                "goback"
            };
            string input = "";
            int selection = -1;
            SelectOption defaultOption = _options.FirstOrDefault(o => o.IsDefault && CanSelectOption(o));
            if (defaultOption != null)
                _options.Where(o => o.Index != defaultOption.Index && o.IsDefault).ToList().ForEach(o => o.IsDefault = false);
            if (KeyboardNavigationEnabled && !Routines.HasBuffer())
            {
                return RenderKeyboard(defaultOption, cancellationToken);
            }

            string filterText = string.Empty;
            int currentPage = GetInitialPage(defaultOption != null ? defaultOption.Index : 0, filterText);
            do
            {
                if (ClearConsole)
                {
                    Consoul.ConsoleDriver.Clear();
                }
                int matchCount = GetMatchingOptionCount(filterText);
                int? pageSize = GetEffectivePageSize(filterText);
                currentPage = ClampPage(currentPage, pageSize, filterText);
                Routines.RegisterOptions(this);
                RenderPromptFrame(pageSize, currentPage, filterText, matchCount);
                Consoul.ConsoleDriver.ForegroundColor = RenderOptions.DefaultColor;
                input = Consoul.Read(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
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
                if (string.IsNullOrEmpty(input) && defaultOption != null)
                {
                    selection = defaultOption.Index;
                }
                else if (escapePhrases.Any(o => input.Equals(o, StringComparison.OrdinalIgnoreCase)))
                {
                    return ResolveCancelResult();
                }
                else if (TryResolveShortcut(input, filterText, out SelectOption shortcutOption))
                {
                    if (!CanSelectOption(shortcutOption))
                    {
                        WriteUnavailableSelectionMessage(shortcutOption);
                        selection = -1;
                    }
                    else
                    {
                        selection = shortcutOption.Index;
                    }
                }
                else
                {
                    Int32.TryParse(input, out selection);
                    if (selection <= 0 || selection > Count)
                    {
                        WriteInvalidSelectionMessage();
                        selection = -1;
                    }
                    else
                    {
                        selection--;
                        SelectOption selectedOption = GetOptionByIndex(selection);
                        if (!CanSelectOption(selectedOption))
                        {
                            WriteUnavailableSelectionMessage(selectedOption);
                            selection = -1;
                        }
                        else if (!string.IsNullOrWhiteSpace(filterText) && !MatchesFilter(selectedOption, filterText))
                        {
                            WriteInvalidSelectionMessage();
                            selection = -1;
                        }
                    }
                }
            } while (selection < 0);

            GetOptionByIndex(selection).Selected = true;

            return PromptResult.FromSelection(selection);
        }

        private PromptResult RenderKeyboard(SelectOption defaultOption, CancellationToken cancellationToken)
        {
            string filterText = string.Empty;
            int highlightedIndex = ResolveInitialKeyboardIndex(defaultOption != null ? defaultOption.Index : -1, filterText);
            int currentPage = GetInitialPage(highlightedIndex >= 0 ? highlightedIndex : 0, filterText);
            bool hasRendered = false;
            bool previousCursorVisible = Consoul.ConsoleDriver.CursorVisible;
            Consoul.ConsoleDriver.CursorVisible = false;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (ClearConsole || hasRendered)
                    {
                        Consoul.ConsoleDriver.Clear();
                    }

                    int matchCount = GetMatchingOptionCount(filterText);
                    highlightedIndex = EnsureKeyboardHighlight(highlightedIndex, filterText);
                    int? pageSize = GetEffectivePageSize(filterText);
                    currentPage = GetInitialPage(highlightedIndex >= 0 ? highlightedIndex : 0, filterText);
                    currentPage = ClampPage(currentPage, pageSize, filterText);

                    Routines.RegisterOptions(this);
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
                        if (highlightedIndex >= 0)
                        {
                            GetOptionByIndex(highlightedIndex).Selected = true;
                            return PromptResult.FromSelection(highlightedIndex);
                        }

                        continue;
                    }

                    if (TryResolveShortcut(keyInfo.KeyChar.ToString(), filterText, out SelectOption shortcutOption))
                    {
                        if (!CanSelectOption(shortcutOption))
                        {
                            WriteUnavailableSelectionMessage(shortcutOption);
                            continue;
                        }

                        shortcutOption.Selected = true;
                        return PromptResult.FromSelection(shortcutOption.Index);
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
                }
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
                .FirstOrDefault(option => option.Index == preferredIndex && CanSelectOption(option));
            return preferred != null ? preferred.Index : GetFirstEnabledMatchIndex(filterText);
        }

        private int EnsureKeyboardHighlight(int highlightedIndex, string filterText)
        {
            SelectOption highlighted = GetMatchingOptions(filterText)
                .FirstOrDefault(option => option.Index == highlightedIndex && CanSelectOption(option));
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
                .Where(CanSelectOption)
                .ToArray();
        }

        private static bool MatchesFilter(SelectOption option, string filterText)
        {
            return string.IsNullOrWhiteSpace(filterText) ||
                PromptRenderHelpers.ContainsVisibleText(option.Label ?? string.Empty, filterText) ||
                (option.Shortcut ?? string.Empty).IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                PromptRenderHelpers.ContainsVisibleText(option.Description ?? string.Empty, filterText) ||
                PromptRenderHelpers.ContainsVisibleText(option.DisabledReason ?? string.Empty, filterText) ||
                option.SearchTerms.Any(term => (term ?? string.Empty).IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private bool TryResolveShortcut(string input, string filterText, out SelectOption option)
        {
            option = null;
            if (!ShortcutsEnabled || string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            string shortcut = input.Trim();
            option = _options.FirstOrDefault(item =>
                !item.IsSeparator &&
                !string.IsNullOrWhiteSpace(item.Shortcut) &&
                item.Shortcut.Trim().Equals(shortcut, StringComparison.OrdinalIgnoreCase) &&
                (string.IsNullOrWhiteSpace(filterText) || MatchesFilter(item, filterText)));
            return option != null;
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

        private ConsoleColor ResolveOptionColor(SelectOption option)
        {
            if (option.IsSeparator)
            {
                return option.Color;
            }

            return !CanSelectOption(option) ? PromptRenderHelpers.ResolveStyleColor(DisabledStyle, option.DisabledColor) : option.Color;
        }

        private PromptRenderHelpers.StyledText ResolveOptionStyle(SelectOption option)
        {
            if (option.IsSeparator)
            {
                return option.ResolveStyle();
            }

            return !CanSelectOption(option)
                ? option.ResolveDisabledStyle(DisabledStyle)
                : option.ResolveStyle();
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

        private bool CanSelectOption(SelectOption option)
        {
            return option != null &&
                !option.IsSeparator &&
                !option.IsDisabled &&
                !(option.IsGroup && Mode == SelectionMode.Leaf);
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

        private PromptResult ResolveCancelResult()
        {
            return CancelResult == null ? PromptResult.Canceled() : CancelResult();
        }

        private string ResolveSearchPlaceholderText()
        {
            return string.IsNullOrWhiteSpace(SearchPlaceholderText) ? KeyboardFilterHelpText : SearchPlaceholderText;
        }

        /// <summary>
        /// Creates and shows a new prompt with the given message and options, then returns the selected option.
        /// </summary>
        /// <param name="message">The message to display for the prompt.</param>
        /// <param name="clear">Indicates whether to clear the console when the prompt is displayed.</param>
        /// <param name="options">The options to present for selection.</param>
        /// <returns>The selected <see cref="SelectOption"/>, or null if no valid selection was made.</returns>
        public static SelectOption Render(string message, bool clear = false, params SelectOption[] options)
        {
            var prompt = new SelectionPrompt(message, clear, options);
            var result = prompt.Render();
            if (result.HasSelection)
            {
                return options.FirstOrDefault(option => !option.IsSeparator && option.Index == result.Index);
            }

            return null;
        }

        /// <summary>
        /// Creates and shows a new prompt with the given message and options, then returns the selected item of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the objects to present as options.</typeparam>
        /// <param name="message">The message to display for the prompt.</param>
        /// <param name="labelSelector">A function to extract the label for each option.</param>
        /// <param name="clear">Indicates whether to clear the console when the prompt is displayed.</param>
        /// <param name="options">The options to present for selection.</param>
        /// <returns>The selected object of type <typeparamref name="T"/>, or null if no valid selection was made.</returns>
        public static T Render<T>(string message, Func<T, string> labelSelector, bool clear = false, params T[] options) where T : class
        {
            return SelectionPrompt<T>.Render(message, labelSelector, clear, options);
        }
    }
}
