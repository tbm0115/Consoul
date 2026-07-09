using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoulLibrary
{
    /// <summary>
    /// Converts text input into a typed prompt value.
    /// </summary>
    /// <typeparam name="T">The parsed value type.</typeparam>
    /// <param name="text">The input text.</param>
    /// <param name="value">The parsed value when parsing succeeds.</param>
    /// <returns><c>true</c> when parsing succeeds; otherwise, <c>false</c>.</returns>
    public delegate bool TextPromptParser<T>(string text, out T value);

    /// <summary>
    /// Represents a typed text prompt with parsing, defaults, validation, and cancellation support.
    /// </summary>
    /// <typeparam name="T">The parsed value type.</typeparam>
    public class TextPrompt<T> : IPrompt<TextPromptResult<T>>
    {
        private readonly List<Func<T, string>> _validators = new List<Func<T, string>>();
        private readonly List<Func<string, string>> _inputNormalizers = new List<Func<string, string>>();
        private readonly List<T> _acceptedValues = new List<T>();
        private readonly StringComparer _choiceComparer;
        private TextPromptParser<T> _parser;
        private IEqualityComparer<T> _acceptedValuesComparer = EqualityComparer<T>.Default;
        private string _acceptedValuesValidationMessage;
        private bool _hasCustomAcceptedValuesValidationMessage;
        private bool _acceptedValuesValidatorAdded;
        private bool _useInvalidChoiceMessageForAcceptedValueMismatch;
        private bool _hasDefaultValue;
        private T _defaultValue;
        private ConsoleColor _promptColor = RenderOptions.PromptColor;
        private ConsoleColor _invalidColor = RenderOptions.InvalidColor;
        private ConsoleColor _maskColor = RenderOptions.SubnoteColor;
        private Style _promptTextStyle;
        private Style _invalidTextStyle;
        private Style _maskTextStyle;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextPrompt{T}"/> class.
        /// </summary>
        /// <param name="message">The message to display when the prompt is shown.</param>
        /// <param name="clear">Indicates whether to clear the console when the prompt is displayed.</param>
        public TextPrompt(string message, bool clear = false)
        {
            Message = message;
            ClearConsole = clear;
            _choiceComparer = StringComparer.OrdinalIgnoreCase;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextPrompt{T}"/> class with a comparer used for accepted choice text.
        /// </summary>
        /// <param name="message">The message to display when the prompt is shown.</param>
        /// <param name="comparer">The comparer used when matching typed input to accepted choice display text.</param>
        public TextPrompt(string message, StringComparer comparer)
            : this(message, false)
        {
            _choiceComparer = comparer ?? StringComparer.OrdinalIgnoreCase;
        }

        /// <summary>
        /// Shows the prompt through the specified console facade.
        /// </summary>
        /// <param name="console">The console facade used to render the prompt.</param>
        /// <returns>The text prompt result.</returns>
        public TextPromptResult<T> Show(IAnsiConsole console)
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
        /// <returns>A task that resolves to the text prompt result.</returns>
        public Task<TextPromptResult<T>> ShowAsync(IAnsiConsole console, CancellationToken cancellationToken = default)
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
        /// Gets or sets a value indicating whether empty input is a valid value.
        /// </summary>
        public bool AllowEmpty { get; set; }

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
        /// Gets or sets the message rendered when parsing fails.
        /// </summary>
        public string InvalidMessage { get; set; } = "Invalid input!";

        /// <summary>
        /// Gets or sets the message rendered when the prompt has known choices and the entered value cannot be matched to one.
        /// </summary>
        public string InvalidChoiceMessage { get; set; } = "Please select one of the available options.";

        /// <summary>
        /// Gets or sets the culture used by the built-in parser and default value formatter.
        /// </summary>
        public CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;

        /// <summary>
        /// Gets or sets a value indicating whether accepted values are rendered in the prompt message.
        /// </summary>
        public bool ShowChoices { get; set; } = true;

        /// <summary>
        /// Gets or sets the separator used when accepted values are rendered in the prompt message.
        /// </summary>
        public string ChoicesSeparator { get; set; } = "/";

        /// <summary>
        /// Gets or sets a value indicating whether the configured default value is rendered in the prompt message.
        /// </summary>
        public bool ShowDefaultValue { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the rendered prompt line is cleared after a successful response.
        /// </summary>
        public bool ClearOnFinish { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the default value is seeded into the editable input buffer.
        /// </summary>
        public bool EditableDefaultValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether input is read without echoing typed characters.
        /// </summary>
        public bool IsSecret { get; set; }

        /// <summary>
        /// Gets or sets the character used to mask typed secret input. Set to <c>null</c> to hide input without mask characters.
        /// </summary>
        public char? Mask { get; set; } = '*';

        /// <summary>
        /// Gets or sets the color used for rendered secret mask characters.
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
        /// Gets or sets the converter used to display typed choices and default values.
        /// </summary>
        public Func<T, string> Converter { get; set; }

        /// <summary>
        /// Gets or sets the optional style used when accepted values are rendered in the prompt message.
        /// </summary>
        public Style ChoicesStyle { get; set; }

        /// <summary>
        /// Gets or sets the optional style used when the default value is rendered in the prompt message.
        /// </summary>
        public Style DefaultValueStyle { get; set; }

        /// <summary>
        /// Sets the prompt title/message using builder-style vocabulary.
        /// </summary>
        /// <param name="title">The prompt title shown to the user.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> Title(string title)
        {
            Message = title ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message.
        /// </summary>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> PromptStyle(ConsoleColor color)
        {
            PromptColor = color;
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> PromptStyle(Style style)
        {
            _promptColor = PromptRenderHelpers.ResolveStyleColor(style, PromptColor);
            _promptTextStyle = style;
            return this;
        }

        /// <summary>
        /// Sets the color used for the prompt message from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> PromptStyle(string style)
        {
            return PromptStyle(ConsoulLibrary.Style.Parse(style));
        }

        /// <summary>
        /// Sets the color used for the prompt message.
        /// </summary>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> Style(ConsoleColor color)
        {
            return PromptStyle(color);
        }

        /// <summary>
        /// Sets the color used for the prompt message from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> Style(Style style)
        {
            return PromptStyle(style);
        }

        /// <summary>
        /// Sets the color used for the prompt message from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> Style(string style)
        {
            return PromptStyle(style);
        }

        /// <summary>
        /// Sets the color used for invalid input messages.
        /// </summary>
        /// <param name="color">The invalid input message color.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> InvalidStyle(ConsoleColor color)
        {
            InvalidColor = color;
            return this;
        }

        /// <summary>
        /// Sets the color used for invalid input messages from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> InvalidStyle(Style style)
        {
            _invalidColor = PromptRenderHelpers.ResolveStyleColor(style, InvalidColor);
            _invalidTextStyle = style;
            return this;
        }

        /// <summary>
        /// Sets the color used for invalid input messages from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>red</c>, <c>grey</c>, <c>#ef4444</c>, or <c>white on red</c>.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> InvalidStyle(string style)
        {
            return InvalidStyle(ConsoulLibrary.Style.Parse(style));
        }

        /// <summary>
        /// Sets the style used when secret text prompt mask characters are rendered.
        /// </summary>
        /// <param name="color">The mask character color.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> MaskStyle(ConsoleColor color)
        {
            MaskColor = color;
            return this;
        }

        /// <summary>
        /// Sets the style used when secret text prompt mask characters are rendered from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> MaskStyle(Style style)
        {
            _maskColor = PromptRenderHelpers.ResolveStyleColor(style, MaskColor);
            _maskTextStyle = style;
            return this;
        }

        /// <summary>
        /// Sets the style used when secret text prompt mask characters are rendered from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>cyan</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> MaskStyle(string style)
        {
            return MaskStyle(ConsoulLibrary.Style.Parse(style));
        }

        /// <summary>
        /// Uses a default value when the user presses Enter without input.
        /// </summary>
        /// <param name="value">The default value.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> WithDefault(T value)
        {
            _defaultValue = value;
            _hasDefaultValue = true;
            return this;
        }

        /// <summary>
        /// Uses a default value when the user presses Enter without input.
        /// </summary>
        /// <param name="value">The default value.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> DefaultValue(T value)
        {
            return WithDefault(value);
        }

        /// <summary>
        /// Sets the converter used to display typed choices and default values.
        /// </summary>
        /// <param name="converter">The converter used to render values.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> UseConverter(Func<T, string> converter)
        {
            Converter = converter ?? throw new ArgumentNullException(nameof(converter));
            return this;
        }

        /// <summary>
        /// Allows empty input as a valid result.
        /// </summary>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> AllowEmptyInput()
        {
            AllowEmpty = true;
            return this;
        }

        /// <summary>
        /// Uses a custom parser for text input.
        /// </summary>
        /// <param name="parser">The parser to use.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> ParseWith(TextPromptParser<T> parser)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            return this;
        }

        /// <summary>
        /// Uses a custom parser for text input.
        /// </summary>
        /// <param name="parser">The parser to use.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> UseParser(TextPromptParser<T> parser)
        {
            return ParseWith(parser);
        }

        /// <summary>
        /// Sets the message rendered when parsing fails.
        /// </summary>
        /// <param name="message">The invalid input message.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> WithInvalidMessage(string message)
        {
            InvalidMessage = message ?? throw new ArgumentNullException(nameof(message));
            return this;
        }

        /// <summary>
        /// Sets the message rendered when parsing fails.
        /// </summary>
        /// <param name="message">The invalid input message.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> ValidationErrorMessage(string message)
        {
            return WithInvalidMessage(message);
        }

        /// <summary>
        /// Sets the message rendered when the prompt has known choices and the entered value cannot be matched to one.
        /// </summary>
        /// <param name="message">The invalid choice message.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> WithInvalidChoiceMessage(string message)
        {
            InvalidChoiceMessage = message ?? throw new ArgumentNullException(nameof(message));
            return this;
        }

        /// <summary>
        /// Sets the culture used by the built-in parser and default value formatter.
        /// </summary>
        /// <param name="culture">The culture to use.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> UseCulture(CultureInfo culture)
        {
            Culture = culture ?? throw new ArgumentNullException(nameof(culture));
            return this;
        }

        /// <summary>
        /// Adds a text normalizer that runs before parsing, validation, and cancellation phrase checks.
        /// </summary>
        /// <param name="normalizer">The normalizer to apply to submitted text.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> NormalizeInput(Func<string, string> normalizer)
        {
            if (normalizer == null)
            {
                throw new ArgumentNullException(nameof(normalizer));
            }

            _inputNormalizers.Add(normalizer);
            return this;
        }

        /// <summary>
        /// Trims leading and trailing white space before parsing and validation.
        /// </summary>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> TrimInput()
        {
            return NormalizeInput(text => (text ?? string.Empty).Trim());
        }

        /// <summary>
        /// Requires the parsed value to be present, rejecting <c>null</c> and empty or white-space strings.
        /// </summary>
        /// <param name="message">Optional message rendered when the value is missing.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> Required(string message = null)
        {
            return Validate(value => IsMissingValue(value) ? (message ?? "A value is required.") : null);
        }

        /// <summary>
        /// Adds a validator. Return <c>null</c> or an empty string when the value is valid; otherwise return the message to render.
        /// </summary>
        /// <param name="validator">The validator to add.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> Validate(Func<T, string> validator)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            _validators.Add(validator);
            return this;
        }

        /// <summary>
        /// Adds a result-shaped validator. Return <see cref="PromptValidationResult.Success"/> when the value is valid,
        /// or <see cref="PromptValidationResult.Error(string)"/> with the message to render when the value should be rejected.
        /// </summary>
        /// <param name="validator">The validator to add.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> Validate(Func<T, PromptValidationResult> validator)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            return Validate(value =>
            {
                PromptValidationResult result = validator(value);
                if (result == null || result.Successful)
                {
                    return null;
                }

                return string.IsNullOrEmpty(result.Message) ? InvalidMessage : result.Message;
            });
        }

        /// <summary>
        /// Adds a predicate validator with a fixed validation message.
        /// </summary>
        /// <param name="predicate">Predicate that returns <c>true</c> when the value is valid.</param>
        /// <param name="message">Message rendered when the predicate rejects the value.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> Validate(Func<T, bool> predicate, string message)
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
        /// Adds a predicate validator with a value-aware validation message.
        /// </summary>
        /// <param name="predicate">Predicate that returns <c>true</c> when the value is valid.</param>
        /// <param name="messageFactory">Factory that creates the rejection message for invalid values.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> Validate(Func<T, bool> predicate, Func<T, string> messageFactory)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (messageFactory == null)
            {
                throw new ArgumentNullException(nameof(messageFactory));
            }

            return Validate(value => predicate(value) ? null : messageFactory(value));
        }

        /// <summary>
        /// Requires comparable values to be greater than or equal to the specified minimum.
        /// </summary>
        /// <param name="minimum">The inclusive minimum value.</param>
        /// <param name="message">Optional message rendered when the value is too small.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> Minimum(T minimum, string message = null)
        {
            EnsureComparable();
            return Validate(value =>
            {
                if (IsMissingValue(value))
                {
                    return null;
                }

                return CompareValues(value, minimum) >= 0
                    ? null
                    : message ?? "Value must be at least " + FormatValue(minimum) + ".";
            });
        }

        /// <summary>
        /// Requires comparable values to be less than or equal to the specified maximum.
        /// </summary>
        /// <param name="maximum">The inclusive maximum value.</param>
        /// <param name="message">Optional message rendered when the value is too large.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> Maximum(T maximum, string message = null)
        {
            EnsureComparable();
            return Validate(value =>
            {
                if (IsMissingValue(value))
                {
                    return null;
                }

                return CompareValues(value, maximum) <= 0
                    ? null
                    : message ?? "Value must be at most " + FormatValue(maximum) + ".";
            });
        }

        /// <summary>
        /// Requires comparable values to be within the inclusive range.
        /// </summary>
        /// <param name="minimum">The inclusive minimum value.</param>
        /// <param name="maximum">The inclusive maximum value.</param>
        /// <param name="message">Optional message rendered when the value is outside the range.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> Range(T minimum, T maximum, string message = null)
        {
            EnsureComparable();
            if (CompareValues(minimum, maximum) > 0)
            {
                throw new ArgumentException("Minimum cannot be greater than maximum.", nameof(minimum));
            }

            return Validate(value =>
            {
                if (IsMissingValue(value))
                {
                    return null;
                }

                return CompareValues(value, minimum) >= 0 && CompareValues(value, maximum) <= 0
                    ? null
                    : message ?? "Value must be between " + FormatValue(minimum) + " and " + FormatValue(maximum) + ".";
            });
        }

        /// <summary>
        /// Restricts accepted values to a known set.
        /// </summary>
        /// <param name="allowedValues">Allowed values.</param>
        /// <param name="validationMessage">Optional message rendered when the parsed value is not allowed.</param>
        /// <param name="comparer">Optional equality comparer used to compare parsed values.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> AcceptOnly(IEnumerable<T> allowedValues, string validationMessage = null, IEqualityComparer<T> comparer = null)
        {
            if (allowedValues == null)
            {
                throw new ArgumentNullException(nameof(allowedValues));
            }

            var values = new List<T>(allowedValues);
            if (values.Count == 0)
            {
                throw new ArgumentException("At least one allowed value is required.", nameof(allowedValues));
            }

            _acceptedValues.Clear();
            _acceptedValues.AddRange(values);
            _acceptedValuesComparer = comparer ?? EqualityComparer<T>.Default;
            _acceptedValuesValidationMessage = validationMessage;
            _hasCustomAcceptedValuesValidationMessage = validationMessage != null;
            _useInvalidChoiceMessageForAcceptedValueMismatch = false;
            EnsureAcceptedValuesValidator();
            return this;
        }

        /// <summary>
        /// Adds a single accepted choice using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="choice">The choice to add.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> AddChoice(T choice)
        {
            _acceptedValues.Add(choice);
            EnsureAcceptedValuesValidator();
            return this;
        }

        /// <summary>
        /// Adds accepted choices using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="choices">The choices to add.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> AddChoices(params T[] choices)
        {
            return AddChoices((IEnumerable<T>)choices);
        }

        /// <summary>
        /// Adds accepted choices using Spectre-style builder vocabulary.
        /// </summary>
        /// <param name="choices">The choices to add.</param>
        /// <returns>The current <see cref="TextPrompt{T}"/> instance.</returns>
        public TextPrompt<T> AddChoices(IEnumerable<T> choices)
        {
            if (choices == null)
            {
                throw new ArgumentNullException(nameof(choices));
            }

            foreach (T choice in choices)
            {
                _acceptedValues.Add(choice);
            }

            EnsureAcceptedValuesValidator();
            return this;
        }

        internal TextPrompt<T> UseInvalidChoiceMessageForAcceptedValueMismatch()
        {
            _useInvalidChoiceMessageForAcceptedValueMismatch = true;
            return this;
        }

        /// <summary>
        /// Displays the prompt and waits for typed input.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="TextPromptResult{T}"/> describing the parsed value or cancellation state.</returns>
        public TextPromptResult<T> Render(CancellationToken cancellationToken = default)
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

                int promptTop = Consoul.ConsoleDriver.CursorTop;
                WritePromptMessage();
                int promptBottom = Math.Max(promptTop, Consoul.ConsoleDriver.CursorTop - 1);
                string rawInput = ReadPromptInput(cancellationToken);
                int promptBottomAfterInput = Math.Max(promptBottom, Consoul.ConsoleDriver.CursorTop - 1);
                string input = NormalizeInputText(rawInput);
                if (cancellationToken.IsCancellationRequested)
                {
                    return TextPromptResult<T>.Canceled();
                }

                if (Array.Exists(escapePhrases, phrase => input.Equals(phrase, StringComparison.OrdinalIgnoreCase)))
                {
                    return TextPromptResult<T>.Canceled();
                }

                T value;
                if (TryResolveValue(input, out value))
                {
                    string validationMessage = ValidateValue(value);
                    if (string.IsNullOrEmpty(validationMessage))
                    {
                        ClearRenderedPrompt(promptTop, promptBottomAfterInput);
                        return TextPromptResult<T>.FromValue(value, rawInput);
                    }

                    WriteValidationMessage(validationMessage);
                }
                else
                {
                    WriteUnresolvedInputMessage();
                }
            } while (true);
        }

        private string ReadPromptInput(CancellationToken cancellationToken)
        {
            if (EditableDefaultValue && _hasDefaultValue && !Routines.HasBuffer())
            {
                return ReadEditableDefaultInput(FormatValue(_defaultValue), cancellationToken);
            }

            return IsSecret
                ? Consoul.ReadPassword(ResolveMaskStyle(), Mask ?? '\0', cancellationToken)
                : Consoul.Read(cancellationToken);
        }

        private string ReadEditableDefaultInput(string defaultText, CancellationToken cancellationToken)
        {
            var input = new RoutineInput();
            var buffer = new StringBuilder(defaultText ?? string.Empty);
            int cursor = buffer.Length;
            int inputTop = Consoul.ConsoleDriver.CursorTop;
            RenderEditableBuffer(buffer.ToString(), cursor, inputTop);

            while (!cancellationToken.IsCancellationRequested)
            {
                ConsoleKeyInfo key = Consoul.ConsoleDriver.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Consoul.ConsoleDriver.WriteLine(string.Empty);
                    break;
                }

                switch (key.Key)
                {
                    case ConsoleKey.Backspace:
                        if (cursor > 0)
                        {
                            buffer.Remove(cursor - 1, 1);
                            cursor--;
                            RenderEditableBuffer(buffer.ToString(), cursor, inputTop);
                        }
                        break;
                    case ConsoleKey.Delete:
                        if (cursor < buffer.Length)
                        {
                            buffer.Remove(cursor, 1);
                            RenderEditableBuffer(buffer.ToString(), cursor, inputTop);
                        }
                        break;
                    case ConsoleKey.LeftArrow:
                        if (cursor > 0)
                        {
                            cursor--;
                            RenderEditableBuffer(buffer.ToString(), cursor, inputTop);
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        if (cursor < buffer.Length)
                        {
                            cursor++;
                            RenderEditableBuffer(buffer.ToString(), cursor, inputTop);
                        }
                        break;
                    case ConsoleKey.Home:
                        cursor = 0;
                        RenderEditableBuffer(buffer.ToString(), cursor, inputTop);
                        break;
                    case ConsoleKey.End:
                        cursor = buffer.Length;
                        RenderEditableBuffer(buffer.ToString(), cursor, inputTop);
                        break;
                    default:
                        if (!char.IsControl(key.KeyChar))
                        {
                            buffer.Insert(cursor, key.KeyChar);
                            cursor++;
                            RenderEditableBuffer(buffer.ToString(), cursor, inputTop);
                        }
                        break;
                }
            }

            input.Value = buffer.ToString();
            if (Routines.MonitorInputs)
            {
                Routines.UserInputs.Push(input);
            }

            return Routines.ResolveInputValue(input);
        }

        private void RenderEditableBuffer(string value, int cursor, int top)
        {
            string display = FormatEditableInput(value);
            int start = ResolveVisibleStart(display, cursor);
            int width = Consoul.ConsoleBufferWidth;
            int maxVisibleLength = Math.Max(0, width - 1);
            string visible = maxVisibleLength == 0
                ? string.Empty
                : display.Substring(start, Math.Min(maxVisibleLength, Math.Max(0, display.Length - start)));
            int cursorLeft = Math.Max(0, Math.Min(cursor - start, width - 1));

            Consoul.ConsoleDriver.ClearLine(top);
            Consoul.ConsoleDriver.SetCursorPosition(0, top);
            if (visible.Length > 0)
            {
                if (IsSecret)
                {
                    PromptRenderHelpers.WriteText(visible, ResolveMaskStyle(), writeLine: false);
                }
                else
                {
                    Consoul.WriteCore(visible, PromptColor, writeLine: false);
                }
            }

            Consoul.ConsoleDriver.SetCursorPosition(cursorLeft, top);
        }

        private string FormatEditableInput(string value)
        {
            if (!IsSecret)
            {
                return value ?? string.Empty;
            }

            return Mask.HasValue
                ? new string(Mask.Value, (value ?? string.Empty).Length)
                : string.Empty;
        }

        private string FormatDefaultHintValue(T value)
        {
            string formatted = FormatValue(value);
            return IsSecret ? FormatEditableInput(formatted) : formatted;
        }

        private static int ResolveVisibleStart(string display, int cursor)
        {
            int maxVisibleLength = Math.Max(0, Consoul.ConsoleBufferWidth - 1);
            if (maxVisibleLength == 0 || string.IsNullOrEmpty(display) || display.Length <= maxVisibleLength)
            {
                return 0;
            }

            int clampedCursor = Math.Max(0, Math.Min(cursor, display.Length));
            return Math.Max(0, Math.Min(clampedCursor, display.Length - maxVisibleLength));
        }

        private void ClearRenderedPrompt(int promptTop, int promptBottom)
        {
            if (!ClearOnFinish)
            {
                return;
            }

            int height = Consoul.ConsoleBufferHeight;
            int firstLine = Math.Max(0, Math.Min(promptTop, height - 1));
            int lastLine = Math.Max(firstLine, Math.Min(promptBottom, height - 1));
            for (int line = firstLine; line <= lastLine; line++)
            {
                Consoul.ConsoleDriver.ClearLine(line);
            }

            Consoul.ConsoleDriver.SetCursorPosition(0, firstLine);
        }

        private void WritePromptMessage()
        {
            List<PromptHint> hints = BuildPromptHints();
            if (!PromptRenderHelpers.ContainsMarkup(Message) && CanUsePlainPromptLine(hints))
            {
                Consoul.WriteCore(BuildPromptText(hints), PromptColor);
                return;
            }

            PromptRenderHelpers.StyledText promptStyle = ResolvePromptStyle();
            if (hints.Count == 0)
            {
                PromptRenderHelpers.WriteMarkupLine(Message, promptStyle);
                return;
            }

            PromptRenderHelpers.WriteMarkup(Message, promptStyle, writeLine: false);
            PromptRenderHelpers.WriteMarkup(" (", promptStyle, writeLine: false);
            for (int i = 0; i < hints.Count; i++)
            {
                if (i > 0)
                {
                    PromptRenderHelpers.WriteMarkup(", ", promptStyle, writeLine: false);
                }

                PromptRenderHelpers.WriteMarkup(hints[i].Text, hints[i].Style, writeLine: false);
            }

            PromptRenderHelpers.WriteMarkup(")", promptStyle, writeLine: true);
        }

        private string BuildPromptText(IList<PromptHint> hints)
        {
            return hints.Count == 0 ? Message : Message + " (" + string.Join(", ", hints.Select(hint => hint.Text)) + ")";
        }

        private bool CanUsePlainPromptLine(IList<PromptHint> hints)
        {
            return !HasStyleMetadata(_promptTextStyle) &&
                hints.All(hint => hint.Color == PromptColor && !hint.HasStyleMetadata);
        }

        private List<PromptHint> BuildPromptHints()
        {
            var hints = new List<string>();
            if (ShowChoices && _acceptedValues.Count > 0)
            {
                string separator = string.IsNullOrEmpty(ChoicesSeparator) ? "/" : ChoicesSeparator;
                hints.Add(string.Join(separator, _acceptedValues.Select(FormatValue)));
            }

            var promptHints = new List<PromptHint>();
            if (hints.Count > 0)
            {
                promptHints.Add(new PromptHint(
                    string.Join(string.Empty, hints),
                    ResolvePromptHintStyle(ChoicesStyle)));
            }

            if (ShowDefaultValue && _hasDefaultValue)
            {
                promptHints.Add(new PromptHint(
                    "default: " + FormatDefaultHintValue(_defaultValue),
                    ResolvePromptHintStyle(DefaultValueStyle)));
            }

            return promptHints;
        }

        private string NormalizeInputText(string input)
        {
            string value = input ?? string.Empty;
            foreach (Func<string, string> normalizer in _inputNormalizers)
            {
                value = normalizer(value) ?? string.Empty;
            }

            return value;
        }

        private bool TryResolveValue(string input, out T value)
        {
            input = input ?? string.Empty;
            if (input.Length == 0)
            {
                if (_hasDefaultValue)
                {
                    value = _defaultValue;
                    return true;
                }

                if (AllowEmpty)
                {
                    value = ResolveEmptyValue();
                    return true;
                }
            }

            if (TryResolveAcceptedValue(input, out value))
            {
                return true;
            }

            if (_acceptedValues.Count > 0)
            {
                value = default(T);
                return false;
            }

            TextPromptParser<T> parser = _parser ?? DefaultParse;
            return parser(input, out value);
        }

        private bool TryResolveAcceptedValue(string input, out T value)
        {
            foreach (T acceptedValue in _acceptedValues)
            {
                if (MatchesAcceptedValue(input, acceptedValue))
                {
                    value = acceptedValue;
                    return true;
                }
            }

            value = default(T);
            return false;
        }

        private bool MatchesAcceptedValue(string input, T acceptedValue)
        {
            return _choiceComparer.Equals(input, FormatValue(acceptedValue))
                || _choiceComparer.Equals(input, DefaultFormatValue(acceptedValue, Culture));
        }

        private string GetUnresolvedInputMessage()
        {
            if (_acceptedValues.Count == 0)
            {
                return InvalidMessage;
            }

            return _useInvalidChoiceMessageForAcceptedValueMismatch
                ? InvalidChoiceMessage
                : GetAcceptedValuesValidationMessage();
        }

        private void WriteUnresolvedInputMessage()
        {
            string message = GetUnresolvedInputMessage();
            if (IsGeneratedAcceptedValuesValidationMessage(message))
            {
                PromptRenderHelpers.WriteMarkupOrPlainLine(message, ResolveInvalidStyle(), parseMarkup: false);
                return;
            }

            PromptRenderHelpers.WriteMarkupOrPlainLine(message, ResolveInvalidStyle());
        }

        private T ResolveEmptyValue()
        {
            if (typeof(T) == typeof(string))
            {
                return (T)(object)string.Empty;
            }

            return default(T);
        }

        private string ValidateValue(T value)
        {
            foreach (Func<T, string> validator in _validators)
            {
                string message = validator(value);
                if (!string.IsNullOrEmpty(message))
                {
                    return message;
                }
            }

            return null;
        }

        private void WriteValidationMessage(string message)
        {
            if (IsGeneratedAcceptedValuesValidationMessage(message))
            {
                PromptRenderHelpers.WriteMarkupOrPlainLine(message, ResolveInvalidStyle(), parseMarkup: false);
                return;
            }

            PromptRenderHelpers.WriteMarkupOrPlainLine(message, ResolveInvalidStyle());
        }

        private void EnsureAcceptedValuesValidator()
        {
            if (_acceptedValuesValidatorAdded)
            {
                return;
            }

            _acceptedValuesValidatorAdded = true;
            Validate(value => _acceptedValues.Count == 0 || _acceptedValues.Contains(value, _acceptedValuesComparer)
                ? null
                : GetAcceptedValuesValidationMessage());
        }

        private string GetAcceptedValuesValidationMessage()
        {
            return _hasCustomAcceptedValuesValidationMessage
                ? _acceptedValuesValidationMessage
                : BuildAllowedValuesMessage(_acceptedValues);
        }

        private bool IsGeneratedAcceptedValuesValidationMessage(string message)
        {
            return !_hasCustomAcceptedValuesValidationMessage &&
                _acceptedValues.Count > 0 &&
                string.Equals(message, BuildAllowedValuesMessage(_acceptedValues), StringComparison.Ordinal);
        }

        private static bool IsMissingValue(T value)
        {
            if (value == null)
            {
                return true;
            }

            string text = value as string;
            return text != null && string.IsNullOrWhiteSpace(text);
        }

        private static void EnsureComparable()
        {
            Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            if (typeof(IComparable).IsAssignableFrom(targetType))
            {
                return;
            }

            Type comparableType = typeof(IComparable<>).MakeGenericType(targetType);
            if (comparableType.IsAssignableFrom(targetType))
            {
                return;
            }

            throw new InvalidOperationException("Range validation requires values of type " + typeof(T).Name + " to implement IComparable.");
        }

        private static int CompareValues(T value, T boundary)
        {
            if (value == null || boundary == null)
            {
                return Comparer<T>.Default.Compare(value, boundary);
            }

            var typedComparable = value as IComparable<T>;
            if (typedComparable != null)
            {
                return typedComparable.CompareTo(boundary);
            }

            var comparable = value as IComparable;
            if (comparable != null)
            {
                return comparable.CompareTo(boundary);
            }

            return Comparer<T>.Default.Compare(value, boundary);
        }

        private bool DefaultParse(string text, out T value)
        {
            Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            CultureInfo culture = Culture ?? CultureInfo.CurrentCulture;

            if (typeof(T) == typeof(string))
            {
                value = (T)(object)text;
                return true;
            }

            try
            {
                object parsed;
                if (targetType.IsEnum)
                {
                    parsed = Enum.Parse(targetType, text, ignoreCase: true);
                }
                else if (targetType == typeof(bool))
                {
                    bool boolValue;
                    if (!TryParseBoolean(text, out boolValue))
                    {
                        value = default(T);
                        return false;
                    }

                    parsed = boolValue;
                }
                else if (targetType == typeof(Guid))
                {
                    Guid guidValue;
                    if (!Guid.TryParse(text, out guidValue))
                    {
                        value = default(T);
                        return false;
                    }

                    parsed = guidValue;
                }
                else if (targetType == typeof(TimeSpan))
                {
                    TimeSpan timeSpanValue;
                    if (!TimeSpan.TryParse(text, culture, out timeSpanValue))
                    {
                        value = default(T);
                        return false;
                    }

                    parsed = timeSpanValue;
                }
                else if (targetType == typeof(DateTime))
                {
                    DateTime dateTimeValue;
                    if (!DateTime.TryParse(text, culture, DateTimeStyles.None, out dateTimeValue))
                    {
                        value = default(T);
                        return false;
                    }

                    parsed = dateTimeValue;
                }
                else if (targetType == typeof(DateTimeOffset))
                {
                    DateTimeOffset dateTimeOffsetValue;
                    if (!DateTimeOffset.TryParse(text, culture, DateTimeStyles.None, out dateTimeOffsetValue))
                    {
                        value = default(T);
                        return false;
                    }

                    parsed = dateTimeOffsetValue;
                }
                else if (targetType == typeof(Uri))
                {
                    Uri uriValue;
                    if (!Uri.TryCreate(text, UriKind.RelativeOrAbsolute, out uriValue))
                    {
                        value = default(T);
                        return false;
                    }

                    parsed = uriValue;
                }
                else if (targetType == typeof(Version))
                {
                    Version versionValue;
                    if (!Version.TryParse(text, out versionValue))
                    {
                        value = default(T);
                        return false;
                    }

                    parsed = versionValue;
                }
                else
                {
                    parsed = Convert.ChangeType(text, targetType, culture);
                }

                value = (T)parsed;
                return true;
            }
            catch
            {
                value = default(T);
                return false;
            }
        }

        private static bool TryParseBoolean(string text, out bool value)
        {
            if (bool.TryParse(text, out value))
            {
                return true;
            }

            switch ((text ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "y":
                case "yes":
                case "1":
                    value = true;
                    return true;
                case "n":
                case "no":
                case "0":
                    value = false;
                    return true;
                default:
                    value = false;
                    return false;
            }
        }

        private string FormatValue(T value)
        {
            return Converter == null ? DefaultFormatValue(value, Culture) : Converter(value) ?? string.Empty;
        }

        private static string DefaultFormatValue(T value, IFormatProvider formatProvider)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var formattable = value as IFormattable;
            return formattable == null
                ? value.ToString()
                : formattable.ToString(null, formatProvider ?? CultureInfo.CurrentCulture);
        }

        private string BuildAllowedValuesMessage(IEnumerable<T> allowedValues)
        {
            return "Value must be one of: " + string.Join(", ", allowedValues.Select(FormatValue)) + ".";
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

        private PromptRenderHelpers.StyledText ResolvePromptHintStyle(Style style)
        {
            return PromptRenderHelpers.ResolveStyle(style, PromptColor);
        }

        private static bool HasStyleMetadata(Style style)
        {
            return style != null &&
                (style.Decoration != Decoration.None ||
                 style.ForegroundRgb.HasValue ||
                 style.BackgroundRgb.HasValue);
        }

        private struct PromptHint
        {
            public PromptHint(string text, PromptRenderHelpers.StyledText style)
            {
                Text = text;
                Style = style;
            }

            public string Text { get; }

            public PromptRenderHelpers.StyledText Style { get; }

            public ConsoleColor Color => Style.Scheme.Color;

            public bool HasStyleMetadata =>
                Style.Decoration != Decoration.None ||
                Style.ForegroundRgb.HasValue ||
                Style.BackgroundRgb.HasValue;
        }
    }
}
