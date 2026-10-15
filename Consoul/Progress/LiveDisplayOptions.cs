using System;

namespace ConsoulLibrary
{
    /// <summary>
    /// Defines rendering options for a live display scope.
    /// </summary>
    public class LiveDisplayOptions
    {
        /// <summary>
        /// Gets or sets an optional title rendered before each refresh.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the title foreground color.
        /// </summary>
        public ConsoleColor TitleColor { get; set; } = RenderOptions.PromptColor;

        /// <summary>
        /// Gets or sets optional Spectre-style title foreground/background and decoration metadata.
        /// </summary>
        public Style TitleStyle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the console should be cleared before each refresh.
        /// </summary>
        public bool ClearBeforeRender { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the title should render as a rule.
        /// </summary>
        public bool RenderTitleRule { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the console should be cleared when the scope is disposed.
        /// </summary>
        public bool ClearOnDispose { get; set; }

        /// <summary>
        /// Gets or sets how content taller than the terminal should be handled.
        /// </summary>
        public VerticalOverflow Overflow { get; set; } = VerticalOverflow.Visible;

        /// <summary>
        /// Gets or sets which side of overflowing content should be cropped.
        /// </summary>
        public VerticalOverflowCropping Cropping { get; set; } = VerticalOverflowCropping.Bottom;

        /// <summary>
        /// Gets or sets the color used for successful completion messages.
        /// </summary>
        public ConsoleColor SuccessColor { get; set; } = ConsoleColor.Green;

        /// <summary>
        /// Gets or sets optional Spectre-style success foreground/background and decoration metadata.
        /// </summary>
        public Style SuccessStyle { get; set; }

        /// <summary>
        /// Gets or sets the color used for failure completion messages.
        /// </summary>
        public ConsoleColor FailureColor { get; set; } = RenderOptions.InvalidColor;

        /// <summary>
        /// Gets or sets optional Spectre-style failure foreground/background and decoration metadata.
        /// </summary>
        public Style FailureStyle { get; set; }

        /// <summary>
        /// Creates a copy of the current live display options.
        /// </summary>
        /// <returns>A new <see cref="LiveDisplayOptions"/> instance with the same values.</returns>
        public LiveDisplayOptions Clone()
        {
            return new LiveDisplayOptions
            {
                Title = Title,
                TitleColor = TitleColor,
                TitleStyle = TitleStyle,
                ClearBeforeRender = ClearBeforeRender,
                RenderTitleRule = RenderTitleRule,
                ClearOnDispose = ClearOnDispose,
                Overflow = Overflow,
                Cropping = Cropping,
                SuccessColor = SuccessColor,
                SuccessStyle = SuccessStyle,
                FailureColor = FailureColor,
                FailureStyle = FailureStyle
            };
        }

        internal Style ResolveTitleStyle()
        {
            return TitleStyle ?? new Style(TitleColor);
        }

        internal Style ResolveSuccessStyle()
        {
            return SuccessStyle ?? new Style(SuccessColor);
        }

        internal Style ResolveFailureStyle()
        {
            return FailureStyle ?? new Style(FailureColor);
        }
    }
}
