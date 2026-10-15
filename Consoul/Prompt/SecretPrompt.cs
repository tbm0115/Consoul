using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a masked secret prompt for passwords, API keys, and other sensitive values.
    /// </summary>
    public class SecretPrompt : IPrompt<SecretResult>
    {
        private readonly List<Func<string, string>> _validators = new List<Func<string, string>>();
        private ConsoleColor _promptColor = RenderOptions.PromptColor;
        private ConsoleColor _invalidColor = RenderOptions.InvalidColor;
        private ConsoleColor _maskColor = RenderOptions.SubnoteColor;
        private Style _promptTextStyle;
        private Style _invalidTextStyle;
        private Style _maskTextStyle;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecretPrompt"/> class.
        /// </summary>
        /// <param name="message">The message to display when the prompt is shown.</param>
        /// <param name="clear">Indicates whether to clear the console when the prompt is displayed.</param>
        public SecretPrompt(string message, bool clear = false)
        {
            Message = message;
            ClearConsole = clear;
        }

        /// <summary>
        /// Shows the prompt through the specified console facade.
        /// </summary>
        /// <param name="console">The console facade used to render the prompt.</param>
        /// <returns>The secret prompt result.</returns>
        public SecretResult Show(IAnsiConsole console)
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
        /// <returns>A task that resolves to the secret prompt result.</returns>
        public Task<SecretResult> ShowAsync(IAnsiConsole console, CancellationToken cancellationToken = default)
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
        /// Gets or sets a value indicating whether empty input is a valid secret value.
        /// </summary>
        public bool AllowEmpty { get; set; }

        /// <summary>
        /// Gets or sets the character used to mask typed input. Set to <c>null</c> to hide input without mask characters.
        /// </summary>
        public char? MaskCharacter { get; set; } = '*';

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
        /// Gets or sets the color used for rendered mask characters.
        /// </summary>
        public ConsoleColor MaskColor
        {
            get => _maskColor;
            set
            {
                _maskColor = value;
                _maskTextStyle = null;
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
        /// Gets or sets the message rendered when a non-empty value is required.
        /// </summary>
        public string RequiredMessage { get; set; } = "A value is required.";

        /// <summary>
        /// Sets the prompt title/message using builder-style vocabulary.
        /// </summary>
        /// <param name="title">The prompt title shown to the user.</param>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt Title(string title)
        {
            Message = title ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message.
        /// </summary>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt PromptStyle(ConsoleColor color)
        {
            PromptColor = color;
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt PromptStyle(SpectreColor color)
        {
            return PromptStyle(new Style(color));
        }

        /// <summary>
        /// Sets the color used for the prompt message from an RGB value.
        /// </summary>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt PromptStyle(RgbColor color)
        {
            return PromptStyle(new Style(color));
        }

        /// <summary>
        /// Sets the color used for the prompt message from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt PromptStyle(Style style)
        {
            _promptColor = PromptRenderHelpers.ResolveStyleColor(style, PromptColor);
            _promptTextStyle = style;
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt PromptStyle(string style)
        {
            return PromptStyle(ConsoulLibrary.Style.Parse(style));
        }

        /// <summary>
        /// Sets the color used for the prompt message.
        /// </summary>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt Style(ConsoleColor color)
        {
            return PromptStyle(color);
        }

        /// <summary>
        /// Sets the color used for the prompt message from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt Style(SpectreColor color)
        {
            return PromptStyle(color);
        }

        /// <summary>
        /// Sets the color used for the prompt message from an RGB value.
        /// </summary>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt Style(RgbColor color)
        {
            return PromptStyle(color);
        }

        /// <summary>
        /// Sets the color used for the prompt message from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt Style(Style style)
        {
            return PromptStyle(style);
        }

        /// <summary>
        /// Sets the color used for the prompt message from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt Style(string style)
        {
            return PromptStyle(style);
        }

        /// <summary>
        /// Sets the character used to mask typed input.
        /// </summary>
        /// <param name="character">The mask character, or <see langword="null"/> to hide input without rendering mask characters.</param>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt Mask(char? character = '*')
        {
            MaskCharacter = character;
            return this;
        }

        /// <summary>
        /// Sets the color used for rendered mask characters.
        /// </summary>
        /// <param name="color">The mask character color.</param>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt MaskStyle(ConsoleColor color)
        {
            MaskColor = color;
            return this;
        }

        /// <summary>
        /// Sets the color used for rendered mask characters from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The mask character color.</param>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt MaskStyle(SpectreColor color)
        {
            return MaskStyle(new Style(color));
        }

        /// <summary>
        /// Sets the color used for rendered mask characters from an RGB value.
        /// </summary>
        /// <param name="color">The mask character color.</param>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt MaskStyle(RgbColor color)
        {
            return MaskStyle(new Style(color));
        }

        /// <summary>
        /// Sets the color used for rendered mask characters from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt MaskStyle(Style style)
        {
            _maskColor = PromptRenderHelpers.ResolveStyleColor(style, MaskColor);
            _maskTextStyle = style;
            return this;
        }

        /// <summary>
        /// Sets the color used for rendered mask characters from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>cyan</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt MaskStyle(string style)
        {
            return MaskStyle(ConsoulLibrary.Style.Parse(style));
        }

        /// <summary>
        /// Sets the color used for invalid input messages.
        /// </summary>
        /// <param name="color">The invalid input message color.</param>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt InvalidStyle(ConsoleColor color)
        {
            InvalidColor = color;
            return this;
        }

        /// <summary>
        /// Sets the color used for invalid input messages from a Spectre-style color facade.
        /// </summary>
        /// <param name="color">The invalid input message color.</param>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt InvalidStyle(SpectreColor color)
        {
            return InvalidStyle(new Style(color));
        }

        /// <summary>
        /// Sets the color used for invalid input messages from an RGB value.
        /// </summary>
        /// <param name="color">The invalid input message color.</param>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt InvalidStyle(RgbColor color)
        {
            return InvalidStyle(new Style(color));
        }

        /// <summary>
        /// Sets the color used for invalid input messages from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt InvalidStyle(Style style)
        {
            _invalidColor = PromptRenderHelpers.ResolveStyleColor(style, InvalidColor);
            _invalidTextStyle = style;
            return this;
        }

        /// <summary>
        /// Sets the color used for invalid input messages from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>red</c>, <c>grey</c>, <c>#ef4444</c>, or <c>white on red</c>.</param>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt InvalidStyle(string style)
        {
            return InvalidStyle(ConsoulLibrary.Style.Parse(style));
        }

        /// <summary>
        /// Sets the message rendered when a non-empty value is required.
        /// </summary>
        /// <param name="message">The required value message.</param>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt RequiredErrorMessage(string message)
        {
            RequiredMessage = message ?? throw new ArgumentNullException(nameof(message));
            return this;
        }

        /// <summary>
        /// Allows empty input as a valid secret value.
        /// </summary>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt AllowEmptyInput()
        {
            AllowEmpty = true;
            return this;
        }

        /// <summary>
        /// Adds a validator. Return <c>null</c> or an empty string when the value is valid; otherwise return the message to render.
        /// </summary>
        /// <param name="validator">The validator to add.</param>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt Validate(Func<string, string> validator)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            _validators.Add(validator);
            return this;
        }

        /// <summary>
        /// Adds a predicate validator with a fixed validation message.
        /// </summary>
        /// <param name="predicate">Predicate that returns <c>true</c> when the value is valid.</param>
        /// <param name="message">Message rendered when the predicate rejects the value.</param>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt Validate(Func<string, bool> predicate, string message)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return Validate(value => predicate(value) ? null : message);
        }

        /// <summary>
        /// Requires the secret value to contain at least the specified number of characters.
        /// </summary>
        /// <param name="minimumLength">Minimum accepted secret length.</param>
        /// <param name="message">Optional message rendered when the value is too short.</param>
        /// <returns>The current <see cref="SecretPrompt"/> instance.</returns>
        public SecretPrompt MinimumLength(int minimumLength, string message = null)
        {
            if (minimumLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumLength), "Minimum length cannot be negative.");
            }

            string validationMessage = message ?? "Value must contain at least " + minimumLength + " characters.";
            return Validate(value => (value ?? string.Empty).Length >= minimumLength, validationMessage);
        }

        /// <summary>
        /// Displays the prompt and waits for masked input.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="SecretResult"/> describing the secret value, or cancellation when input ends or is canceled.</returns>
        public SecretResult Render(CancellationToken cancellationToken = default)
        {
            do
            {
                if (ClearConsole)
                {
                    Consoul.ConsoleDriver.Clear();
                }

                PromptRenderHelpers.WriteMarkupLine(Message, ResolvePromptStyle());
                string value = Consoul.ReadPassword(ResolveMaskStyle(), MaskCharacter ?? '\0', cancellationToken);
                if (value == null || cancellationToken.IsCancellationRequested)
                {
                    return SecretResult.Canceled();
                }

                if (string.IsNullOrEmpty(value))
                {
                    if (AllowEmpty)
                    {
                        return SecretResult.FromValue(value);
                    }

                    PromptRenderHelpers.WriteMarkupOrPlainLine(RequiredMessage, ResolveInvalidStyle());
                    continue;
                }

                string validationMessage = ValidateValue(value);
                if (string.IsNullOrEmpty(validationMessage))
                {
                    return SecretResult.FromValue(value);
                }

                PromptRenderHelpers.WriteMarkupOrPlainLine(validationMessage, ResolveInvalidStyle());
            } while (true);
        }

        private PromptRenderHelpers.StyledText ResolvePromptStyle()
        {
            return PromptRenderHelpers.ResolveStyle(_promptTextStyle, PromptColor);
        }

        private PromptRenderHelpers.StyledText ResolveInvalidStyle()
        {
            return PromptRenderHelpers.ResolveStyle(_invalidTextStyle, InvalidColor);
        }

        private PromptRenderHelpers.StyledText ResolveMaskStyle()
        {
            return PromptRenderHelpers.ResolveStyle(_maskTextStyle, MaskColor);
        }

        private string ValidateValue(string value)
        {
            foreach (Func<string, string> validator in _validators)
            {
                string message = validator(value);
                if (!string.IsNullOrEmpty(message))
                {
                    return message;
                }
            }

            return null;
        }
    }
}
