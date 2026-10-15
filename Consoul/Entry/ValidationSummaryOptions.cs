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
        /// Gets or sets the ANSI decoration used for the invalid summary border and title.
        /// </summary>
        public Decoration InvalidDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for the invalid summary border and title on ANSI-capable terminals.
        /// </summary>
        public RgbColor? InvalidForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for the invalid summary border and title on ANSI-capable terminals.
        /// </summary>
        public RgbColor? InvalidBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for invalid summary issue content.
        /// </summary>
        public ColorScheme? IssueScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for invalid summary issue content.
        /// </summary>
        public Decoration IssueDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for invalid summary issue content on ANSI-capable terminals.
        /// </summary>
        public RgbColor? IssueForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for invalid summary issue content on ANSI-capable terminals.
        /// </summary>
        public RgbColor? IssueBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for the valid summary border and title.
        /// </summary>
        public ColorScheme? ValidScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for the valid summary border and title.
        /// </summary>
        public Decoration ValidDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for the valid summary border and title on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ValidForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for the valid summary border and title on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ValidBackgroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the color scheme used for the valid summary message.
        /// </summary>
        public ColorScheme? ValidMessageScheme { get; set; }

        /// <summary>
        /// Gets or sets the ANSI decoration used for the valid summary message.
        /// </summary>
        public Decoration ValidMessageDecoration { get; set; }

        /// <summary>
        /// Gets or sets the RGB foreground color used for the valid summary message on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ValidMessageForegroundRgb { get; set; }

        /// <summary>
        /// Gets or sets the RGB background color used for the valid summary message on ANSI-capable terminals.
        /// </summary>
        public RgbColor? ValidMessageBackgroundRgb { get; set; }

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
                InvalidDecoration = InvalidDecoration,
                InvalidForegroundRgb = InvalidForegroundRgb,
                InvalidBackgroundRgb = InvalidBackgroundRgb,
                IssueScheme = IssueScheme,
                IssueDecoration = IssueDecoration,
                IssueForegroundRgb = IssueForegroundRgb,
                IssueBackgroundRgb = IssueBackgroundRgb,
                ValidScheme = ValidScheme,
                ValidDecoration = ValidDecoration,
                ValidForegroundRgb = ValidForegroundRgb,
                ValidBackgroundRgb = ValidBackgroundRgb,
                ValidMessageScheme = ValidMessageScheme,
                ValidMessageDecoration = ValidMessageDecoration,
                ValidMessageForegroundRgb = ValidMessageForegroundRgb,
                ValidMessageBackgroundRgb = ValidMessageBackgroundRgb
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
            SetInvalidStyle(Scheme(color, backgroundColor), Decoration.None, null, null);
            return this;
        }

        /// <summary>
        /// Sets the invalid summary border and title foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions InvalidColor(SpectreColor color)
        {
            SetInvalidStyle(Scheme(color), Decoration.None, color.ToRgbColor(), null);
            return this;
        }

        /// <summary>
        /// Sets the invalid summary border and title foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions InvalidColor(RgbColor color)
        {
            SetInvalidStyle(Scheme(color), Decoration.None, color, null);
            return this;
        }

        /// <summary>
        /// Sets the invalid summary border and title color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions InvalidColor(Style style)
        {
            ApplyInvalidStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the invalid summary border and title color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>red</c>, <c>grey</c>, <c>#dc2626</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions InvalidColor(string style)
        {
            return InvalidColor(Style.Parse(style));
        }

        /// <summary>
        /// Sets the invalid summary issue content color scheme.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions IssueColor(ConsoleColor color, ConsoleColor? backgroundColor = null)
        {
            SetIssueStyle(Scheme(color, backgroundColor), Decoration.None, null, null);
            return this;
        }

        /// <summary>
        /// Sets the invalid summary issue content foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions IssueColor(SpectreColor color)
        {
            SetIssueStyle(Scheme(color), Decoration.None, color.ToRgbColor(), null);
            return this;
        }

        /// <summary>
        /// Sets the invalid summary issue content foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions IssueColor(RgbColor color)
        {
            SetIssueStyle(Scheme(color), Decoration.None, color, null);
            return this;
        }

        /// <summary>
        /// Sets the invalid summary issue content color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions IssueColor(Style style)
        {
            ApplyIssueStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the invalid summary issue content color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>yellow</c>, <c>grey</c>, <c>#f59e0b</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions IssueColor(string style)
        {
            return IssueColor(Style.Parse(style));
        }

        /// <summary>
        /// Sets the valid summary border and title color scheme.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions ValidColor(ConsoleColor color, ConsoleColor? backgroundColor = null)
        {
            SetValidStyle(Scheme(color, backgroundColor), Decoration.None, null, null);
            return this;
        }

        /// <summary>
        /// Sets the valid summary border and title foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions ValidColor(SpectreColor color)
        {
            SetValidStyle(Scheme(color), Decoration.None, color.ToRgbColor(), null);
            return this;
        }

        /// <summary>
        /// Sets the valid summary border and title foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions ValidColor(RgbColor color)
        {
            SetValidStyle(Scheme(color), Decoration.None, color, null);
            return this;
        }

        /// <summary>
        /// Sets the valid summary border and title color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions ValidColor(Style style)
        {
            ApplyValidStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the valid summary border and title color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions ValidColor(string style)
        {
            return ValidColor(Style.Parse(style));
        }

        /// <summary>
        /// Sets the valid summary message color scheme.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions ValidMessageColor(ConsoleColor color, ConsoleColor? backgroundColor = null)
        {
            SetValidMessageStyle(Scheme(color, backgroundColor), Decoration.None, null, null);
            return this;
        }

        /// <summary>
        /// Sets the valid summary message foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions ValidMessageColor(SpectreColor color)
        {
            SetValidMessageStyle(Scheme(color), Decoration.None, color.ToRgbColor(), null);
            return this;
        }

        /// <summary>
        /// Sets the valid summary message foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions ValidMessageColor(RgbColor color)
        {
            SetValidMessageStyle(Scheme(color), Decoration.None, color, null);
            return this;
        }

        /// <summary>
        /// Sets the valid summary message color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions ValidMessageColor(Style style)
        {
            ApplyValidMessageStyle(style);
            return this;
        }

        /// <summary>
        /// Sets the valid summary message color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="ValidationSummaryOptions"/> instance.</returns>
        public ValidationSummaryOptions ValidMessageColor(string style)
        {
            return ValidMessageColor(Style.Parse(style));
        }

        private void ApplyInvalidStyle(Style style)
        {
            Style resolvedStyle = style ?? Style.Plain;
            SetInvalidStyle(resolvedStyle.ToColorScheme(RenderOptions.DefaultScheme), resolvedStyle.Decoration, resolvedStyle.ForegroundRgb, resolvedStyle.BackgroundRgb);
        }

        private void ApplyIssueStyle(Style style)
        {
            Style resolvedStyle = style ?? Style.Plain;
            SetIssueStyle(resolvedStyle.ToColorScheme(RenderOptions.DefaultScheme), resolvedStyle.Decoration, resolvedStyle.ForegroundRgb, resolvedStyle.BackgroundRgb);
        }

        private void ApplyValidStyle(Style style)
        {
            Style resolvedStyle = style ?? Style.Plain;
            SetValidStyle(resolvedStyle.ToColorScheme(RenderOptions.DefaultScheme), resolvedStyle.Decoration, resolvedStyle.ForegroundRgb, resolvedStyle.BackgroundRgb);
        }

        private void ApplyValidMessageStyle(Style style)
        {
            Style resolvedStyle = style ?? Style.Plain;
            SetValidMessageStyle(resolvedStyle.ToColorScheme(RenderOptions.DefaultScheme), resolvedStyle.Decoration, resolvedStyle.ForegroundRgb, resolvedStyle.BackgroundRgb);
        }

        private void SetInvalidStyle(ColorScheme scheme, Decoration decoration, RgbColor? foregroundRgb, RgbColor? backgroundRgb)
        {
            InvalidScheme = scheme;
            InvalidDecoration = decoration;
            InvalidForegroundRgb = foregroundRgb;
            InvalidBackgroundRgb = backgroundRgb;
        }

        private void SetIssueStyle(ColorScheme scheme, Decoration decoration, RgbColor? foregroundRgb, RgbColor? backgroundRgb)
        {
            IssueScheme = scheme;
            IssueDecoration = decoration;
            IssueForegroundRgb = foregroundRgb;
            IssueBackgroundRgb = backgroundRgb;
        }

        private void SetValidStyle(ColorScheme scheme, Decoration decoration, RgbColor? foregroundRgb, RgbColor? backgroundRgb)
        {
            ValidScheme = scheme;
            ValidDecoration = decoration;
            ValidForegroundRgb = foregroundRgb;
            ValidBackgroundRgb = backgroundRgb;
        }

        private void SetValidMessageStyle(ColorScheme scheme, Decoration decoration, RgbColor? foregroundRgb, RgbColor? backgroundRgb)
        {
            ValidMessageScheme = scheme;
            ValidMessageDecoration = decoration;
            ValidMessageForegroundRgb = foregroundRgb;
            ValidMessageBackgroundRgb = backgroundRgb;
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

    }
}
