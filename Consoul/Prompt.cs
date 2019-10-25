using System;
using System.Collections.Generic;
using Options = Consoul.RenderOptions;
namespace Consoul
{
    public delegate void PromptChoiceCallback<TTarget>(TTarget choice);
    public class Prompt
    {
        public string Message { get; set; }
        public bool ClearConsole { get; set; }
        private List<PromptOption> _options { get; set; }
        public IEnumerable<PromptOption> Options => _options;
        public string this[int index] => _options[index].Label;
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

        public void Add(string label, ConsoleColor? color = null)
        {
            if (color == null)
                color = RenderOptions.OptionColor;
            _options.Add(new PromptOption(_options.Count, label, (ConsoleColor)color));
        }

        public void Clear()
        {
            _options.Clear();
        }

        /// <summary>
        /// Displays the options for this prompt. Loops until the user "selects" the appropriate option.
        /// </summary>
        /// <returns>Zero-based index of the selected option.</returns>
        public int Run()
        {
            string input = "";
            int selection = -1;
            do
            {
                if (ClearConsole)
                {
                    Console.Clear();
                }
                Consoul._write(Message, RenderOptions.PromptColor);
                Consoul._write("Choose the corresponding number from the options below:", RenderOptions.SubnoteColor);
                int i = 0;
                foreach (PromptOption option in Options)
                {
                    Consoul._write(option.ToString(), option.Color);
                    i++;
                }
                Console.ForegroundColor = RenderOptions.DefaultColor;
                input = Console.ReadLine();
                Int32.TryParse(input, out selection);
                if (selection <= 0 || selection > (_options.Count + 1))
                {
                    Consoul._write("Invalid selection!", RenderOptions.InvalidColor);
                    selection = -1;
                }
                else
                {
                    selection--;
                }
            } while (selection < 0);
            return selection;
        }
    }
    public class PromptOption
    {
        public string Label { get; set; }
        public ConsoleColor Color { get; set; }
        public int Index { get; set; }

        public override string ToString()
        {
            return $"{Index + 1}) {Label}";
        }

        public PromptOption(string label, ConsoleColor? color = null)
        {
            Label = label;
            Color = color ?? RenderOptions.OptionColor;
        }
        public PromptOption(int index, string label, ConsoleColor color) : this(label, color)
        {
            Index = index;
        }
    }

}
