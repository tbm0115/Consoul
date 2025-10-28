using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Options = ConsoulLibrary.RenderOptions;
namespace ConsoulLibrary {
    public delegate void PromptChoiceCallback<TTarget>(TTarget choice);
    /// <summary>
    /// Renders a new indexable list of options to choose from.
    /// </summary>
    public class Prompt
    {
        private List<PromptOption> _options { get; set; }

        /// <summary>
        /// Gets a <see cref="PromptOption.Label"/> by index.
        /// </summary>
        /// <param name="index">Zero-based index of the <see cref="PromptOption"/></param>
        /// <returns><see cref="PromptOption.Label"/></returns>
        public string this[int index] => _options[index].Label;

        /// <summary>
        /// Display message for the prompt
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Flags whether to clear the console window upon prompt (and reprompt)
        /// </summary>
        public bool ClearConsole { get; set; }

        /// <summary>
        /// Collection of <see cref="PromptOption"/>s
        /// </summary>
        public IEnumerable<PromptOption> Options => _options;

        /// <summary>
        /// Number of <see cref="PromptOption"/>s
        /// </summary>
        public int Count => _options.Count;


        public Prompt(string message, bool clear = false)
        {
            Message = message;
            ClearConsole = clear;
            _options = new List<PromptOption>();
        }

        public Prompt(string message, bool clear = false, params string[] options) : this(message, clear)
        {
            foreach (string option in options)
            {
                Add(option);
            }
        }

        public Prompt(string message, bool clear = false, params PromptOption[] options) : this(message, clear)
        {
            int i = 0;
            foreach (PromptOption option in options)
            {
                option.Index = i;
                _options.Add(option);
                i++;
            }
        }

        /// <summary>
        /// Adds a new <see cref="PromptOption"/>
        /// </summary>
        /// <param name="label"><see cref="PromptOption.Label"/></param>
        /// <param name="color"><see cref="PromptOption.Color"/></param>
        /// <param name="isDefault">Flag for whether this is the default selected option</param>
        /// <param name="renderStyle">Sets the style for which the item is rendered</param>
        /// <returns>Index of the new item</returns>
        public int Add(string label, ConsoleColor? color = null, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            if (color == null)
                color = RenderOptions.OptionColor;
            var option = new PromptOption(_options.Count, label, (ConsoleColor)color, isDefault, renderStyle);
            option.Index = _options.Count;
            _options.Add(option);
            return option.Index;
        }

        /// <summary>
        /// Clears the list of <see cref="PromptOption"/>s
        /// </summary>
        public void Clear()
        {
            _options.Clear();
        }

        /// <summary>
        /// Displays the options for this prompt. Loops until the user "selects" the appropriate option.
        /// </summary>
        /// <returns>Zero-based index of the selected option.</returns>
        public int Run(CancellationToken cancellationToken = default)
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
            PromptOption defaultOption = _options.FirstOrDefault(o => o.IsDefault);
            if (defaultOption != null)
                _options.Where(o => o.Index != defaultOption.Index && o.IsDefault).ToList().ForEach(o => o.IsDefault = false);
            do
            {
                if (ClearConsole)
                {
                    Console.Clear();
                }
                Consoul._write(Message, RenderOptions.PromptColor);
                Consoul._write("Choose the corresponding number from the options below:", RenderOptions.SubnoteColor);
                int i = 0;

                Routines.RegisterOptions(this);
                foreach (PromptOption option in Options)
                {
                    Consoul._write(option.ToString(), option.Color);
                    i++;
                }
                Console.ForegroundColor = RenderOptions.DefaultColor;
                input = Consoul.Read(cancellationToken);
                if (string.IsNullOrEmpty(input) && defaultOption != null)
                {
                    selection = defaultOption.Index;
                }
                else if (escapePhrases.Any(o => input.Equals(o, StringComparison.OrdinalIgnoreCase)))
                {
                    return Consoul.EscapeIndex;
                }
                else 
                {
                    Int32.TryParse(input, out selection);
                    if (selection <= 0 || selection > (_options.Count + 1)) {
                        Consoul._write("Invalid selection!", RenderOptions.InvalidColor);
                        selection = -1;
                    } else {
                        selection--;
                    }
                }
            } while (selection < 0);

            _options[selection].Selected = true;

            return selection;
        }
    }

}
