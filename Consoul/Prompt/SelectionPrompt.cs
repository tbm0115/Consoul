using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Options = ConsoulLibrary.RenderOptions;

namespace ConsoulLibrary
{
    public delegate void PromptChoiceCallback<TTarget>(TTarget choice);
    /// <summary>
    /// Represents a selection prompt with generic entity type <typeparamref name="T"/>, allowing customized label selection for each item.
    /// </summary>
    /// <typeparam name="T">The type of the entities to be represented as options in the prompt.</typeparam>
    public class SelectionPrompt<T> : SelectionPrompt
    {
        /// <summary>
        /// Gets or sets the function used to extract the label for each entity of type <typeparamref name="T"/>.
        /// </summary>
        public Func<T, string> LabelSelector { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectionPrompt{T}"/> class with a message, an optional clear flag, and a label selector function.
        /// </summary>
        /// <param name="message">The message to display when the prompt is shown.</param>
        /// <param name="clear">Indicates whether to clear the console when the prompt is displayed.</param>
        /// <param name="labelSelector">The function used to extract the label from each entity of type <typeparamref name="T"/>.</param>
        public SelectionPrompt(string message, bool clear = false, Func<T, string> labelSelector = null) : base(message, clear)
        {
            LabelSelector = labelSelector;
        }

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

            string label = LabelSelector(item);
            var option = new SelectOption(_options.Count, label, color ?? RenderOptions.OptionColor, isDefault, renderStyle);
            _options.Add(option);
            return option.Index;
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
        public static T Render(string message, Func<T, string> labelSelector, bool clear = false, params T[] options)
        {
            var prompt = new SelectionPrompt<T>(message, clear, labelSelector, options);
            var result = prompt.Render();
            if (result.HasSelection && result.Index >= 0 && result.Index < options.Length)
            {
                return options[result.Index];
            }
            return default(T);
        }
    }

    /// <summary>
    /// Represents a prompt that renders an indexable list of options for the user to choose from.
    /// </summary>
    public class SelectionPrompt
    {
        protected List<SelectOption> _options { get; set; }

        /// <summary>
        /// Gets the label of the <see cref="SelectOption"/> at the specified index.
        /// </summary>
        /// <param name="index">Zero-based index of the <see cref="SelectOption"/></param>
        /// <returns>The label of the <see cref="SelectOption"/> at the specified index.</returns>
        public string this[int index] => _options[index].Label;

        /// <summary>
        /// Gets or sets the message to be displayed to the user when the prompt is shown.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Indicates whether to clear the console window upon showing or re-showing the prompt.
        /// </summary>
        public bool ClearConsole { get; set; }

        /// <summary>
        /// Gets the collection of <see cref="SelectOption"/> objects that represent the selectable options.
        /// </summary>
        public IEnumerable<SelectOption> Options => _options;

        /// <summary>
        /// Gets the number of selectable <see cref="SelectOption"/> objects in the prompt.
        /// </summary>
        public int Count => _options.Count;

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
                option.Index = i;
                _options.Add(option);
                i++;
            }
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
            var option = new SelectOption(_options.Count, label, (ConsoleColor)color, isDefault, renderStyle);
            option.Index = _options.Count;
            _options.Add(option);
            return option.Index;
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
            SelectOption defaultOption = _options.FirstOrDefault(o => o.IsDefault);
            if (defaultOption != null)
                _options.Where(o => o.Index != defaultOption.Index && o.IsDefault).ToList().ForEach(o => o.IsDefault = false);
            do
            {
                if (ClearConsole)
                {
                    Console.Clear();
                }
                Consoul.WriteCore(Message, RenderOptions.PromptColor);
                Consoul.WriteCore("Choose the corresponding number from the options below:", RenderOptions.SubnoteColor);
                int i = 0;

                Routines.RegisterOptions(this);
                foreach (SelectOption option in Options)
                {
                    Consoul.WriteCore(option.ToString(), option.Color);
                    i++;
                }
                Console.ForegroundColor = RenderOptions.DefaultColor;
                input = Consoul.Read(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return PromptResult.Canceled();
                }
                if (string.IsNullOrEmpty(input) && defaultOption != null)
                {
                    selection = defaultOption.Index;
                }
                else if (escapePhrases.Any(o => input.Equals(o, StringComparison.OrdinalIgnoreCase)))
                {
                    return PromptResult.Canceled();
                }
                else
                {
                    Int32.TryParse(input, out selection);
                    if (selection <= 0 || selection > (_options.Count + 1))
                    {
                        Consoul.WriteCore("Invalid selection!", RenderOptions.InvalidColor);
                        selection = -1;
                    }
                    else
                    {
                        selection--;
                    }
                }
            } while (selection < 0);

            _options[selection].Selected = true;

            return PromptResult.FromSelection(selection);
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
            if (result.HasSelection && result.Index >= 0 && result.Index < options.Length)
            {
                return options[result.Index];
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
