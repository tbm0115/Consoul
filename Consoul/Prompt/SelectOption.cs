using System;
namespace ConsoulLibrary
{
    /// <summary>
    /// Represents an individual selection option for a prompt, including properties for label, color, index, and selection state.
    /// </summary>
    public class SelectOption
    {
        /// <summary>
        /// Gets or sets the display label for this option.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the display color for this option.
        /// </summary>
        public ConsoleColor Color { get; set; }

        /// <summary>
        /// Gets or sets the zero-based index reference for this option.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this option is the default selection.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this option has been selected.
        /// </summary>
        public bool Selected { get; set; } = false;

        /// <summary>
        /// Gets or sets the rendering style for this option.
        /// </summary>
        public OptionRenderStyle Style { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectOption"/> class with the specified label, color, default flag, and render style.
        /// </summary>
        /// <param name="label">The label for this option.</param>
        /// <param name="color">The color of the option text in the console. Defaults to the standard option color if not provided.</param>
        /// <param name="isDefault">Indicates whether this option is the default selection.</param>
        /// <param name="renderStyle">Specifies the rendering style for this option.</param>
        public SelectOption(string label, ConsoleColor? color = null, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            Label = label;
            Color = color ?? RenderOptions.OptionColor;
            IsDefault = isDefault;
            Style = renderStyle;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectOption"/> class with the specified index, label, color, default flag, and render style.
        /// </summary>
        /// <param name="index">The zero-based index of this option.</param>
        /// <param name="label">The label for this option.</param>
        /// <param name="color">The color of the option text in the console.</param>
        /// <param name="isDefault">Indicates whether this option is the default selection.</param>
        /// <param name="renderStyle">Specifies the rendering style for this option.</param>
        public SelectOption(int index, string label, ConsoleColor color, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable) : this(label, color, isDefault, renderStyle)
        {
            Index = index;
        }

        /// <summary>
        /// Returns a string that represents the current <see cref="SelectOption"/>, formatted based on the rendering style.
        /// </summary>
        /// <returns>A string representation of the current <see cref="SelectOption"/>.</returns>
        public override string ToString()
        {
            string suffix = (IsDefault ? "\t(default) " : string.Empty);
            string formattedLabel = $"{Index + 1}) {Label}{suffix}";
            switch (Style)
            {
                case OptionRenderStyle.Checkbox:
                    return $"[{(Selected ? "x" : " ")}] - {formattedLabel}";
                default:
                    return formattedLabel;
            }
        }
    }

    /// <summary>
    /// Specifies the expected rendering style of each item in a selection list.
    /// </summary>
    public enum OptionRenderStyle
    {
        /// <summary>
        /// Item rendered as an incrementing number.
        /// </summary>
        Indexable = 0,

        /// <summary>
        /// Item rendered as a checkbox, indicating whether it is selected.
        /// </summary>
        Checkbox = 1
    }
}
