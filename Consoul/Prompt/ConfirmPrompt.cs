using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a yes/no confirmation prompt with defaults and cancellation support.
    /// </summary>
    public class ConfirmPrompt : IPrompt<ConfirmResult>
    {
        private bool _hasDefaultValue;
        private bool _defaultValue;
        private ConsoleColor _promptColor = RenderOptions.PromptColor;
        private ConsoleColor _invalidColor = RenderOptions.InvalidColor;
        private Style _promptTextStyle;
        private Style _invalidTextStyle;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmPrompt"/> class.
        /// </summary>
        /// <param name="message">The message to display when the prompt is shown.</param>
        /// <param name="clear">Indicates whether to clear the console when the prompt is displayed.</param>
        public ConfirmPrompt(string message, bool clear = false)
        {
            Message = message;
            ClearConsole = clear;
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

            return Task.FromResult(console.Prompt(this, cancellationToken));
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
        /// Gets or sets the color used for the prompt message.
        /// </summary>
        public ConsoleColor PromptColor
        {
            get => _promptColor;
            set
            {
                _promptColor = value;
                _promptTextStyle = null;
            }
        }

        /// <summary>
        /// Gets or sets the color used for invalid input messages.
        /// </summary>
        public ConsoleColor InvalidColor
        {
            get => _invalidColor;
            set
            {
                _invalidColor = value;
                _invalidTextStyle = null;
            }
        }

        /// <summary>
        /// Gets or sets the message rendered when the user enters an invalid confirmation value.
        /// </summary>
        public string InvalidMessage { get; set; } = "Please answer yes or no.";

        /// <summary>
        /// Sets the prompt title/message using builder-style vocabulary.
        /// </summary>
        /// <param name="title">The prompt title shown to the user.</param>
        /// <returns>The current <see cref="ConfirmPrompt"/> instance.</returns>
        public ConfirmPrompt Title(string title)
        {
            Message = title ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message.
        /// </summary>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The current <see cref="ConfirmPrompt"/> instance.</returns>
        public ConfirmPrompt PromptStyle(ConsoleColor color)
        {
            PromptColor = color;
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="ConfirmPrompt"/> instance.</returns>
        public ConfirmPrompt PromptStyle(Style style)
        {
            _promptColor = PromptRenderHelpers.ResolveStyleColor(style, PromptColor);
            _promptTextStyle = style;
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="ConfirmPrompt"/> instance.</returns>
        public ConfirmPrompt PromptStyle(string style)
        {
            return PromptStyle(ConsoulLibrary.Style.Parse(style));
        }

        /// <summary>
        /// Sets the color used for the prompt message.
        /// </summary>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The current <see cref="ConfirmPrompt"/> instance.</returns>
        public ConfirmPrompt Style(ConsoleColor color)
        {
            return PromptStyle(color);
        }

        /// <summary>
        /// Sets the color used for the prompt message from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="ConfirmPrompt"/> instance.</returns>
        public ConfirmPrompt Style(Style style)
        {
            return PromptStyle(style);
        }

        /// <summary>
        /// Sets the color used for the prompt message from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="ConfirmPrompt"/> instance.</returns>
        public ConfirmPrompt Style(string style)
        {
            return PromptStyle(style);
        }

        /// <summary>
        /// Sets the color used for invalid input messages.
        /// </summary>
        /// <param name="color">The invalid input message color.</param>
        /// <returns>The current <see cref="ConfirmPrompt"/> instance.</returns>
        public ConfirmPrompt InvalidStyle(ConsoleColor color)
        {
            InvalidColor = color;
            return this;
        }

        /// <summary>
        /// Sets the color used for invalid input messages from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="ConfirmPrompt"/> instance.</returns>
        public ConfirmPrompt InvalidStyle(Style style)
        {
            _invalidColor = PromptRenderHelpers.ResolveStyleColor(style, InvalidColor);
            _invalidTextStyle = style;
            return this;
        }

        /// <summary>
        /// Sets the color used for invalid input messages from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>red</c>, <c>grey</c>, <c>#ef4444</c>, or <c>white on red</c>.</param>
        /// <returns>The current <see cref="ConfirmPrompt"/> instance.</returns>
        public ConfirmPrompt InvalidStyle(string style)
        {
            return InvalidStyle(ConsoulLibrary.Style.Parse(style));
        }

        /// <summary>
        /// Uses a default value when the user presses Enter without input.
        /// </summary>
        /// <param name="value">The default confirmation value.</param>
        /// <returns>The current <see cref="ConfirmPrompt"/> instance.</returns>
        public ConfirmPrompt WithDefault(bool value)
        {
            _defaultValue = value;
            _hasDefaultValue = true;
            return this;
        }

        /// <summary>
        /// Uses a default value when the user presses Enter without input.
        /// </summary>
        /// <param name="value">The default confirmation value.</param>
        /// <returns>The current <see cref="ConfirmPrompt"/> instance.</returns>
        public ConfirmPrompt DefaultValue(bool value)
        {
            return WithDefault(value);
        }

        /// <summary>
        /// Sets the message rendered when the user enters an invalid confirmation value.
        /// </summary>
        /// <param name="message">The invalid confirmation message.</param>
        /// <returns>The current <see cref="ConfirmPrompt"/> instance.</returns>
        public ConfirmPrompt ValidationErrorMessage(string message)
        {
            InvalidMessage = message ?? throw new ArgumentNullException(nameof(message));
            return this;
        }

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
                if (ClearConsole)
                {
                    Consoul.ConsoleDriver.Clear();
                }

                WritePromptMessage();
                string input = Consoul.Read(cancellationToken);
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

                PromptRenderHelpers.WriteMarkupOrPlainLine(InvalidMessage, ResolveInvalidStyle());
            } while (true);
        }

        private void WritePromptMessage()
        {
            PromptRenderHelpers.StyledText promptStyle = ResolvePromptStyle();
            if (!PromptRenderHelpers.ContainsMarkup(Message) && !HasStyleMetadata(_promptTextStyle))
            {
                Consoul.WriteCore(BuildPromptMessage(), PromptColor);
                return;
            }

            PromptRenderHelpers.WriteMarkup(Message, promptStyle, writeLine: false);
            PromptRenderHelpers.WriteMarkup(_hasDefaultValue
                ? _defaultValue ? " [Y/n]" : " [y/N]"
                : " [y/n]", promptStyle, writeLine: true);
        }

        private string BuildPromptMessage()
        {
            if (!_hasDefaultValue)
            {
                return Message + " [y/n]";
            }

            return Message + (_defaultValue ? " [Y/n]" : " [y/N]");
        }

        private bool TryResolveValue(string input, out bool value)
        {
            input = (input ?? string.Empty).Trim();
            if (input.Length == 0 && _hasDefaultValue)
            {
                value = _defaultValue;
                return true;
            }

            switch (input.ToLowerInvariant())
            {
                case "y":
                case "yes":
                case "true":
                case "1":
                case "on":
                    value = true;
                    return true;
                case "n":
                case "no":
                case "false":
                case "0":
                case "off":
                    value = false;
                    return true;
                default:
                    value = false;
                    return false;
            }
        }

        private PromptRenderHelpers.StyledText ResolvePromptStyle()
        {
            return PromptRenderHelpers.ResolveStyle(_promptTextStyle, PromptColor);
        }

        private PromptRenderHelpers.StyledText ResolveInvalidStyle()
        {
            return PromptRenderHelpers.ResolveStyle(_invalidTextStyle, InvalidColor);
        }

        private static bool HasStyleMetadata(Style style)
        {
            return style != null &&
                (style.Decoration != Decoration.None ||
                 style.ForegroundRgb.HasValue ||
                 style.BackgroundRgb.HasValue);
        }
    }
}
