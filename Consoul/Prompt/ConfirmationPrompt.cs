using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a Spectre-shaped yes/no confirmation prompt that returns a Consoul <see cref="ConfirmResult"/>.
    /// </summary>
    public class ConfirmationPrompt : IPrompt<ConfirmResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmationPrompt"/> class.
        /// </summary>
        /// <param name="message">The message to display when the prompt is shown.</param>
        public ConfirmationPrompt(string message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        /// <summary>
        /// Shows the prompt through the specified console facade.
        /// </summary>
        /// <param name="console">The console facade used to render the prompt.</param>
        /// <returns>The confirmation result.</returns>
        public ConfirmResult Show(IAnsiConsole console)
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
        /// <returns>A task that resolves to the confirmation result.</returns>
        public Task<ConfirmResult> ShowAsync(IAnsiConsole console, CancellationToken cancellationToken = default)
        {
            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            return Task.FromResult(Render(cancellationToken));
        }

        /// <summary>
        /// Gets or sets the message shown to the user.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the character that represents an affirmative answer.
        /// </summary>
        public char Yes { get; set; } = 'y';

        /// <summary>
        /// Gets or sets the character that represents a negative answer.
        /// </summary>
        public char No { get; set; } = 'n';

        /// <summary>
        /// Gets or sets a value indicating whether the affirmative answer is the default.
        /// </summary>
        public bool DefaultValue { get; set; } = true;

        /// <summary>
        /// Gets or sets the message rendered when input cannot be matched to a confirmation choice.
        /// </summary>
        public string InvalidChoiceMessage { get; set; } = "Please select one of the available options.";

        /// <summary>
        /// Gets or sets a value indicating whether yes/no choices should be shown in the prompt text.
        /// </summary>
        public bool ShowChoices { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the default answer should be emphasized in the prompt text.
        /// </summary>
        public bool ShowDefaultValue { get; set; } = true;

        /// <summary>
        /// Gets or sets the style used for non-default yes/no choices.
        /// </summary>
        public Style ChoicesStyle { get; set; }

        /// <summary>
        /// Gets or sets the style used for the default yes/no choice.
        /// </summary>
        public Style DefaultValueStyle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user must press Enter after typing a choice.
        /// </summary>
        public bool RequireEnter { get; set; } = true;

        /// <summary>
        /// Gets or sets the comparer used to match typed input to yes/no choices.
        /// </summary>
        public StringComparer Comparer { get; set; } = StringComparer.CurrentCultureIgnoreCase;

        /// <summary>
        /// Gets or sets the color used for the prompt message.
        /// </summary>
        public ConsoleColor PromptColor { get; set; } = RenderOptions.PromptColor;

        /// <summary>
        /// Gets or sets the color used for invalid input messages.
        /// </summary>
        public ConsoleColor InvalidColor { get; set; } = RenderOptions.InvalidColor;

        /// <summary>
        /// Displays the prompt and waits for confirmation input.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="ConfirmResult"/> describing the confirmed value or cancellation state.</returns>
        public ConfirmResult Render(CancellationToken cancellationToken = default)
        {
            string[] escapePhrases = new string[]
            {
                "go back",
                "back",
                "exit",
                "goback"
            };

            do
            {
                WritePromptMessage();
                string input = ReadInput(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return ConfirmResult.Canceled();
                }

                if (Array.Exists(escapePhrases, phrase => input.Equals(phrase, StringComparison.OrdinalIgnoreCase)))
                {
                    return ConfirmResult.Canceled();
                }

                bool value;
                if (TryResolveValue(input, out value))
                {
                    return ConfirmResult.FromValue(value, input ?? string.Empty);
                }

                PromptRenderHelpers.WriteMarkupOrPlainLine(InvalidChoiceMessage, InvalidColor);
            } while (true);
        }

        private string ReadInput(CancellationToken cancellationToken)
        {
            if (RequireEnter || Routines.HasBuffer())
            {
                return Consoul.Read(cancellationToken);
            }

            ConsoleKeyInfo key = Consoul.ConsoleDriver.ReadKey(true);
            return key.Key == ConsoleKey.Enter ? string.Empty : key.KeyChar.ToString();
        }

        private void WritePromptMessage()
        {
            if (ChoicesStyle == null && DefaultValueStyle == null)
            {
                if (PromptRenderHelpers.ContainsMarkup(Message))
                {
                    WritePromptMessageWithLiteralChoices();
                }
                else
                {
                    Consoul.WriteCore(BuildPromptMessage(), PromptColor);
                }

                return;
            }

            if (!ShowChoices)
            {
                WritePromptWithoutChoices();
                return;
            }

            PromptRenderHelpers.WriteMarkup(Message, PromptColor, writeLine: false);
            Consoul.WriteCore(" [", PromptColor, writeLine: false);
            WriteChoice(Yes, DefaultValue);
            Consoul.WriteCore("/", ResolveStyleColor(ChoicesStyle, PromptColor), writeLine: false);
            WriteChoice(No, !DefaultValue);
            Consoul.WriteCore("]", PromptColor);
        }

        private void WritePromptWithoutChoices()
        {
            if (!ShowDefaultValue)
            {
                PromptRenderHelpers.WriteMarkupLine(Message, PromptColor);
                return;
            }

            PromptRenderHelpers.WriteMarkup(Message, PromptColor, writeLine: false);
            Consoul.WriteCore(" [", PromptColor, writeLine: false);
            Consoul.WriteCore(ResolveDefaultCharacter(), ResolveStyleColor(DefaultValueStyle, PromptColor), writeLine: false);
            Consoul.WriteCore("]", PromptColor);
        }

        private void WritePromptMessageWithLiteralChoices()
        {
            PromptRenderHelpers.WriteMarkup(Message, PromptColor, writeLine: false);
            if (!ShowChoices)
            {
                if (ShowDefaultValue)
                {
                    Consoul.WriteCore(" [" + ResolveDefaultCharacter() + "]", PromptColor);
                    return;
                }

                Consoul.WriteCore(string.Empty, PromptColor);
                return;
            }

            string yes = FormatChoice(Yes, DefaultValue);
            string no = FormatChoice(No, !DefaultValue);
            Consoul.WriteCore(" [" + yes + "/" + no + "]", PromptColor);
        }

        private void WriteChoice(char choice, bool isDefault)
        {
            Style style = ShowDefaultValue && isDefault ? DefaultValueStyle : ChoicesStyle;
            Consoul.WriteCore(FormatChoice(choice, isDefault), ResolveStyleColor(style, PromptColor), writeLine: false);
        }

        private string BuildPromptMessage()
        {
            if (!ShowChoices)
            {
                return ShowDefaultValue ? Message + " [" + ResolveDefaultCharacter() + "]" : Message;
            }

            string yes = FormatChoice(Yes, DefaultValue);
            string no = FormatChoice(No, !DefaultValue);
            return Message + " [" + yes + "/" + no + "]";
        }

        private string FormatChoice(char choice, bool isDefault)
        {
            return ShowDefaultValue && isDefault
                ? char.ToUpperInvariant(choice).ToString()
                : char.ToLowerInvariant(choice).ToString();
        }

        private string ResolveDefaultCharacter()
        {
            return char.ToUpperInvariant(DefaultValue ? Yes : No).ToString();
        }

        private bool TryResolveValue(string input, out bool value)
        {
            input = (input ?? string.Empty).Trim();
            if (input.Length == 0)
            {
                value = DefaultValue;
                return true;
            }

            StringComparer comparer = Comparer ?? StringComparer.CurrentCultureIgnoreCase;
            if (comparer.Equals(input, Yes.ToString()))
            {
                value = true;
                return true;
            }

            if (comparer.Equals(input, No.ToString()))
            {
                value = false;
                return true;
            }

            value = false;
            return false;
        }

        private static ConsoleColor ResolveStyleColor(Style style, ConsoleColor fallback)
        {
            return style == null ? fallback : style.ToForegroundColor(fallback);
        }
    }
}
