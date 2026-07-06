using ConsoulLibrary.Color;
using System;
using System.Reflection;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines options for model validation summaries.
    /// </summary>
    public class ValidationSummaryOptions
    {
        /// <summary>
        /// Gets or sets the requested validation summary width. The actual width is clamped to the current terminal width.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the title used when validation issues are present.
        /// </summary>
        public string InvalidTitle { get; set; } = "Validation";

        /// <summary>
        /// Gets or sets the title used when a valid model is rendered.
        /// </summary>
        public string ValidTitle { get; set; } = "Validation";

        /// <summary>
        /// Gets or sets the message rendered when a valid model is shown.
        /// </summary>
        public string ValidMessage { get; set; } = "No validation issues.";

        /// <summary>
        /// Gets or sets a value indicating whether a success callout should be rendered when the model is valid.
        /// </summary>
        public bool ShowValidMessage { get; set; }

        /// <summary>
        /// Gets or sets binding flags used to discover properties for validation.
        /// </summary>
        public BindingFlags BindingAttr { get; set; } = BindingFlags.Public | BindingFlags.Instance;

        /// <summary>
        /// Gets or sets the color scheme used for the invalid summary border and title.
        /// </summary>
        public ColorScheme? InvalidScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for invalid summary issue content.
        /// </summary>
        public ColorScheme? IssueScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for the valid summary border and title.
        /// </summary>
        public ColorScheme? ValidScheme { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for the valid summary message.
        /// </summary>
        public ColorScheme? ValidMessageScheme { get; set; }

        /// <summary>
        /// Creates a copy of the current validation summary options.
        /// </summary>
        /// <returns>A new <see cref="ValidationSummaryOptions"/> instance with the same values.</returns>
        public ValidationSummaryOptions Clone()
        {
            return new ValidationSummaryOptions
            {
                Width = Width,
                InvalidTitle = InvalidTitle,
                ValidTitle = ValidTitle,
                ValidMessage = ValidMessage,
                ShowValidMessage = ShowValidMessage,
                BindingAttr = BindingAttr,
                InvalidScheme = InvalidScheme,
                IssueScheme = IssueScheme,
                ValidScheme = ValidScheme,
                ValidMessageScheme = ValidMessageScheme
            };
        }

        /// <summary>
        /// Sets the invalid summary border and title color scheme.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions InvalidColor(ConsoleColor color, ConsoleColor? backgroundColor = null)
        {
            InvalidScheme = Scheme(color, backgroundColor);
            return this;
        }

        /// <summary>
        /// Sets the invalid summary border and title foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions InvalidColor(SpectreColor color)
        {
            InvalidScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the invalid summary border and title foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions InvalidColor(RgbColor color)
        {
            InvalidScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the invalid summary border and title color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions InvalidColor(Style style)
        {
            InvalidScheme = Scheme(style);
            return this;
        }

        /// <summary>
        /// Sets the invalid summary border and title color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>red</c>, <c>grey</c>, <c>#dc2626</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions InvalidColor(string style)
        {
            InvalidScheme = Style.ParseScheme(style, RenderOptions.DefaultScheme);
            return this;
        }

        /// <summary>
        /// Sets the invalid summary issue content color scheme.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions IssueColor(ConsoleColor color, ConsoleColor? backgroundColor = null)
        {
            IssueScheme = Scheme(color, backgroundColor);
            return this;
        }

        /// <summary>
        /// Sets the invalid summary issue content foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions IssueColor(SpectreColor color)
        {
            IssueScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the invalid summary issue content foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions IssueColor(RgbColor color)
        {
            IssueScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the invalid summary issue content color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions IssueColor(Style style)
        {
            IssueScheme = Scheme(style);
            return this;
        }

        /// <summary>
        /// Sets the invalid summary issue content color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>yellow</c>, <c>grey</c>, <c>#f59e0b</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions IssueColor(string style)
        {
            IssueScheme = Style.ParseScheme(style, RenderOptions.DefaultScheme);
            return this;
        }

        /// <summary>
        /// Sets the valid summary border and title color scheme.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions ValidColor(ConsoleColor color, ConsoleColor? backgroundColor = null)
        {
            ValidScheme = Scheme(color, backgroundColor);
            return this;
        }

        /// <summary>
        /// Sets the valid summary border and title foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions ValidColor(SpectreColor color)
        {
            ValidScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the valid summary border and title foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions ValidColor(RgbColor color)
        {
            ValidScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the valid summary border and title color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions ValidColor(Style style)
        {
            ValidScheme = Scheme(style);
            return this;
        }

        /// <summary>
        /// Sets the valid summary border and title color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions ValidColor(string style)
        {
            ValidScheme = Style.ParseScheme(style, RenderOptions.DefaultScheme);
            return this;
        }

        /// <summary>
        /// Sets the valid summary message color scheme.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions ValidMessageColor(ConsoleColor color, ConsoleColor? backgroundColor = null)
        {
            ValidMessageScheme = Scheme(color, backgroundColor);
            return this;
        }

        /// <summary>
        /// Sets the valid summary message foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions ValidMessageColor(SpectreColor color)
        {
            ValidMessageScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the valid summary message foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions ValidMessageColor(RgbColor color)
        {
            ValidMessageScheme = Scheme(color);
            return this;
        }

        /// <summary>
        /// Sets the valid summary message color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions ValidMessageColor(Style style)
        {
            ValidMessageScheme = Scheme(style);
            return this;
        }

        /// <summary>
        /// Sets the valid summary message color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions ValidMessageColor(string style)
        {
            ValidMessageScheme = Style.ParseScheme(style, RenderOptions.DefaultScheme);
            return this;
        }

        private static ColorScheme Scheme(ConsoleColor color, ConsoleColor? backgroundColor)
        {
            return new ColorScheme
            {
                Color = color,
                BackgroundColor = RenderOptions.GetBackgroundColorOrDefault(backgroundColor)
            };
        }

        private static ColorScheme Scheme(SpectreColor color)
        {
            return Scheme(color.ToConsoleColor(), null);
        }

        private static ColorScheme Scheme(RgbColor color)
        {
            return Scheme(color.ToConsoleColor(), null);
        }

        private static ColorScheme Scheme(Style style)
        {
            return (style ?? Style.Plain).ToColorScheme(RenderOptions.DefaultScheme);
        }
    }
}
