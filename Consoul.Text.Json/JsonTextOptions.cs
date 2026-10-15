using System;
using ConsoulLibrary.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for syntax-colored JSON text.
    /// </summary>
    public class JsonTextOptions
    {
        /// <summary>
        /// Gets or sets the requested render width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether valid JSON should be normalized with indentation before rendering.
        /// </summary>
        public bool PrettyPrint { get; set; } = true;

        /// <summary>
        /// Gets or sets the indentation text used when valid JSON is pretty-printed.
        /// </summary>
        public string Indentation { get; set; } = "   ";

        /// <summary>
        /// Gets or sets a value indicating whether invalid JSON should throw instead of rendering a friendly error.
        /// </summary>
        public bool ThrowOnInvalidJson { get; set; }

        /// <summary>
        /// Gets or sets the message rendered before invalid JSON content.
        /// </summary>
        public string InvalidJsonMessage { get; set; } = "Invalid JSON";

        /// <summary>
        /// Gets or sets the color scheme used for property names.
        /// </summary>
        public ColorScheme? PropertyNameScheme { get; set; }

        /// <summary>
        /// Gets or sets the style used for member/property names.
        /// </summary>
        public Style MemberStyle
        {
            get => ToStyle(PropertyNameScheme);
            set => PropertyNameScheme = ToScheme(value);
        }

        /// <summary>
        /// Gets or sets the color scheme used for string values.
        /// </summary>
        public ColorScheme? StringScheme { get; set; }

        /// <summary>
        /// Gets or sets the style used for string values.
        /// </summary>
        public Style StringStyle
        {
            get => ToStyle(StringScheme);
            set => StringScheme = ToScheme(value);
        }

        /// <summary>
        /// Gets or sets the color scheme used for numeric values.
        /// </summary>
        public ColorScheme? NumberScheme { get; set; }

        /// <summary>
        /// Gets or sets the style used for numeric values.
        /// </summary>
        public Style NumberStyle
        {
            get => ToStyle(NumberScheme);
            set => NumberScheme = ToScheme(value);
        }

        /// <summary>
        /// Gets or sets the color scheme used for boolean values.
        /// </summary>
        public ColorScheme? BooleanScheme { get; set; }

        /// <summary>
        /// Gets or sets the style used for boolean values.
        /// </summary>
        public Style BooleanStyle
        {
            get => ToStyle(BooleanScheme);
            set => BooleanScheme = ToScheme(value);
        }

        /// <summary>
        /// Gets or sets the color scheme used for null values.
        /// </summary>
        public ColorScheme? NullScheme { get; set; }

        /// <summary>
        /// Gets or sets the style used for null values.
        /// </summary>
        public Style NullStyle
        {
            get => ToStyle(NullScheme);
            set => NullScheme = ToScheme(value);
        }

        /// <summary>
        /// Gets or sets the color scheme used for JSON punctuation.
        /// </summary>
        public ColorScheme? PunctuationScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for JSON object braces.
        /// </summary>
        public ColorScheme? BracesScheme { get; set; }

        /// <summary>
        /// Gets or sets the style used for JSON object braces.
        /// </summary>
        public Style BracesStyle
        {
            get => ToStyle(BracesScheme);
            set => BracesScheme = ToScheme(value);
        }

        /// <summary>
        /// Gets or sets the color scheme used for JSON array brackets.
        /// </summary>
        public ColorScheme? BracketScheme { get; set; }

        /// <summary>
        /// Gets or sets the style used for JSON array brackets.
        /// </summary>
        public Style BracketsStyle
        {
            get => ToStyle(BracketScheme);
            set => BracketScheme = ToScheme(value);
        }

        /// <summary>
        /// Gets or sets the color scheme used for JSON colons.
        /// </summary>
        public ColorScheme? ColonScheme { get; set; }

        /// <summary>
        /// Gets or sets the style used for JSON colons.
        /// </summary>
        public Style ColonStyle
        {
            get => ToStyle(ColonScheme);
            set => ColonScheme = ToScheme(value);
        }

        /// <summary>
        /// Gets or sets the color scheme used for JSON commas.
        /// </summary>
        public ColorScheme? CommaScheme { get; set; }

        /// <summary>
        /// Gets or sets the style used for JSON commas.
        /// </summary>
        public Style CommaStyle
        {
            get => ToStyle(CommaScheme);
            set => CommaScheme = ToScheme(value);
        }

        /// <summary>
        /// Gets or sets the color scheme used for whitespace and unclassified content.
        /// </summary>
        public ColorScheme? PlainScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used when invalid JSON is rendered.
        /// </summary>
        public ColorScheme? InvalidScheme { get; set; }

        /// <summary>
        /// Creates a copy of the current JSON text options.
        /// </summary>
        /// <returns>A new <see cref="JsonTextOptions"/> instance with the same values.</returns>
        public JsonTextOptions Clone()
        {
            return new JsonTextOptions
            {
                Width = Width,
                PrettyPrint = PrettyPrint,
                Indentation = Indentation,
                ThrowOnInvalidJson = ThrowOnInvalidJson,
                InvalidJsonMessage = InvalidJsonMessage,
                PropertyNameScheme = PropertyNameScheme,
                StringScheme = StringScheme,
                NumberScheme = NumberScheme,
                BooleanScheme = BooleanScheme,
                NullScheme = NullScheme,
                PunctuationScheme = PunctuationScheme,
                BracesScheme = BracesScheme,
                BracketScheme = BracketScheme,
                ColonScheme = ColonScheme,
                CommaScheme = CommaScheme,
                PlainScheme = PlainScheme,
                InvalidScheme = InvalidScheme
            };
        }

        internal ColorScheme ResolvePropertyNameScheme() => PropertyNameScheme ?? RenderOptions.PromptScheme;

        internal ColorScheme ResolveStringScheme() => StringScheme ?? RenderOptions.OptionScheme;

        internal ColorScheme ResolveNumberScheme() => NumberScheme ?? Scheme(ConsoleColor.Cyan);

        internal ColorScheme ResolveBooleanScheme() => BooleanScheme ?? Scheme(ConsoleColor.Yellow);

        internal ColorScheme ResolveNullScheme() => NullScheme ?? RenderOptions.SubnoteScheme;

        internal ColorScheme ResolvePunctuationScheme() => PunctuationScheme ?? RenderOptions.SubnoteScheme;

        internal ColorScheme ResolveBracesScheme() => BracesScheme ?? ResolvePunctuationScheme();

        internal ColorScheme ResolveBracketScheme() => BracketScheme ?? ResolvePunctuationScheme();

        internal ColorScheme ResolveColonScheme() => ColonScheme ?? ResolvePunctuationScheme();

        internal ColorScheme ResolveCommaScheme() => CommaScheme ?? ResolvePunctuationScheme();

        internal ColorScheme ResolvePlainScheme() => PlainScheme ?? RenderOptions.DefaultScheme;

        internal ColorScheme ResolveInvalidScheme() => InvalidScheme ?? RenderOptions.InvalidScheme;

        private static ColorScheme Scheme(ConsoleColor color)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.DefaultScheme.BackgroundColor
            };
        }

        private static Style ToStyle(ColorScheme? scheme)
        {
            if (!scheme.HasValue)
            {
                return null;
            }

            return new Style((ConsoleColor?)scheme.Value.Color, scheme.Value.BackgroundColor);
        }

        private static ColorScheme? ToScheme(Style style)
        {
            if (style == null)
            {
                return null;
            }

            return style.ToColorScheme(RenderOptions.DefaultScheme);
        }
    }
}
