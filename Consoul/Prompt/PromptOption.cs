using System;
namespace ConsoulLibrary 
{
    /// <summary>
    /// Individual selection option for a Prompt
    /// </summary>
    public class PromptOption
    {
        /// <summary>
        /// Display label for this Option
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Display color for this Option
        /// </summary>
        public ConsoleColor Color { get; set; }

        /// <summary>
        /// Index reference for this Option. Zero-based index.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Flags whether this can be considered a Default selection.
        /// </summary>
        public bool IsDefault { get; set; }

        public bool Selected { get; set; } = false;

        public OptionRenderStyle Style { get; set; }


        public PromptOption(string label, ConsoleColor? color = null, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            Label = label;
            Color = color ?? RenderOptions.OptionColor;
            IsDefault = isDefault;
            Style = renderStyle;
        }

        public PromptOption(int index, string label, ConsoleColor color, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable) : this(label, color, isDefault, renderStyle)
        {
            Index = index;
        }

        public override string ToString() {
            string suffix = (IsDefault ? "\t(default) " : string.Empty);
            string formattedLabel = $"{Index + 1}) {Label}{suffix}";
            switch (Style) {
                case OptionRenderStyle.Checkbox:
                    return $"[{(Selected ? "x" : " ")}] - {formattedLabel}";
                default:
                    return formattedLabel;
            }
        }
    }
    /// <summary>
    /// Indicates the expected rendering style of each item in a list
    /// </summary>
    public enum OptionRenderStyle
    {
        /// <summary>
        /// Item rendered as an incrementing number.
        /// </summary>
        Indexable = 0,
        /// <summary>
        /// Item rendered as a checkbox.
        /// </summary>
        Checkbox = 1
    }
}
