using System;
using System.Linq;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides shared-palette color overloads for prompt builders.
    /// </summary>
    public static class PromptPaletteExtensions
    {
        /// <summary>
        /// Sets the color used for the confirmation prompt message.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmPrompt PromptStyle(this ConfirmPrompt prompt, SpectreColor color)
        {
            return Require(prompt).PromptStyle(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for the confirmation prompt message.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmPrompt PromptStyle(this ConfirmPrompt prompt, RgbColor color)
        {
            return Require(prompt).PromptStyle(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for the confirmation prompt message.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmPrompt Style(this ConfirmPrompt prompt, SpectreColor color)
        {
            return Require(prompt).Style(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for the confirmation prompt message.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmPrompt Style(this ConfirmPrompt prompt, RgbColor color)
        {
            return Require(prompt).Style(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for invalid confirmation input messages.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The invalid input message color.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmPrompt InvalidStyle(this ConfirmPrompt prompt, SpectreColor color)
        {
            return Require(prompt).InvalidStyle(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for invalid confirmation input messages.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The invalid input message color.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmPrompt InvalidStyle(this ConfirmPrompt prompt, RgbColor color)
        {
            return Require(prompt).InvalidStyle(ToStyle(color));
        }

        /// <summary>
        /// Sets the style used for non-default yes/no choices.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The choice foreground color.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt ChoicesStyle(this ConfirmationPrompt prompt, SpectreColor color)
        {
            return ConfirmationPromptExtensions.ChoicesStyle(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Sets the style used for non-default yes/no choices.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The choice foreground color.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt ChoicesStyle(this ConfirmationPrompt prompt, RgbColor color)
        {
            return ConfirmationPromptExtensions.ChoicesStyle(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Sets the style used for the default yes/no choice.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The default choice foreground color.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt DefaultValueStyle(this ConfirmationPrompt prompt, SpectreColor color)
        {
            return ConfirmationPromptExtensions.DefaultValueStyle(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Sets the style used for the default yes/no choice.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The default choice foreground color.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt DefaultValueStyle(this ConfirmationPrompt prompt, RgbColor color)
        {
            return ConfirmationPromptExtensions.DefaultValueStyle(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for the confirmation prompt message.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt PromptStyle(this ConfirmationPrompt prompt, SpectreColor color)
        {
            return ConfirmationPromptExtensions.PromptStyle(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for the confirmation prompt message.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt PromptStyle(this ConfirmationPrompt prompt, RgbColor color)
        {
            return ConfirmationPromptExtensions.PromptStyle(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for the confirmation prompt message.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt Style(this ConfirmationPrompt prompt, SpectreColor color)
        {
            return ConfirmationPromptExtensions.Style(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for the confirmation prompt message.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt Style(this ConfirmationPrompt prompt, RgbColor color)
        {
            return ConfirmationPromptExtensions.Style(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for invalid confirmation input messages.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The invalid input message color.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt InvalidStyle(this ConfirmationPrompt prompt, SpectreColor color)
        {
            return ConfirmationPromptExtensions.InvalidStyle(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for invalid confirmation input messages.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The invalid input message color.</param>
        /// <returns>The configured prompt.</returns>
        public static ConfirmationPrompt InvalidStyle(this ConfirmationPrompt prompt, RgbColor color)
        {
            return ConfirmationPromptExtensions.InvalidStyle(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for the text prompt message.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> PromptStyle<T>(this TextPrompt<T> prompt, SpectreColor color)
        {
            return Require(prompt).PromptStyle(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for the text prompt message.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> PromptStyle<T>(this TextPrompt<T> prompt, RgbColor color)
        {
            return Require(prompt).PromptStyle(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for the text prompt message.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> Style<T>(this TextPrompt<T> prompt, SpectreColor color)
        {
            return Require(prompt).Style(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for the text prompt message.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> Style<T>(this TextPrompt<T> prompt, RgbColor color)
        {
            return Require(prompt).Style(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for invalid text input messages.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The invalid input message color.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> InvalidStyle<T>(this TextPrompt<T> prompt, SpectreColor color)
        {
            return Require(prompt).InvalidStyle(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for invalid text input messages.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The invalid input message color.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> InvalidStyle<T>(this TextPrompt<T> prompt, RgbColor color)
        {
            return Require(prompt).InvalidStyle(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for secret mask characters rendered by a text prompt.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The mask character color.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> MaskStyle<T>(this TextPrompt<T> prompt, SpectreColor color)
        {
            return PromptBuilderExtensions.MaskStyle(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for secret mask characters rendered by a text prompt.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The mask character color.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> MaskStyle<T>(this TextPrompt<T> prompt, RgbColor color)
        {
            return PromptBuilderExtensions.MaskStyle(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Sets the style used for accepted value hints rendered by a text prompt.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The hint foreground color.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> ChoicesStyle<T>(this TextPrompt<T> prompt, SpectreColor color)
        {
            return PromptBuilderExtensions.ChoicesStyle(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Sets the style used for accepted value hints rendered by a text prompt.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The hint foreground color.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> ChoicesStyle<T>(this TextPrompt<T> prompt, RgbColor color)
        {
            return PromptBuilderExtensions.ChoicesStyle(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Sets the style used for default value hints rendered by a text prompt.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The default value hint foreground color.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> DefaultValueStyle<T>(this TextPrompt<T> prompt, SpectreColor color)
        {
            return PromptBuilderExtensions.DefaultValueStyle(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Sets the style used for default value hints rendered by a text prompt.
        /// </summary>
        /// <typeparam name="T">The parsed value type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The default value hint foreground color.</param>
        /// <returns>The configured prompt.</returns>
        public static TextPrompt<T> DefaultValueStyle<T>(this TextPrompt<T> prompt, RgbColor color)
        {
            return PromptBuilderExtensions.DefaultValueStyle(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for the secret prompt message.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static SecretPrompt PromptStyle(this SecretPrompt prompt, SpectreColor color)
        {
            return Require(prompt).PromptStyle(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for the secret prompt message.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static SecretPrompt PromptStyle(this SecretPrompt prompt, RgbColor color)
        {
            return Require(prompt).PromptStyle(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for the secret prompt message.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static SecretPrompt Style(this SecretPrompt prompt, SpectreColor color)
        {
            return Require(prompt).Style(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for the secret prompt message.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static SecretPrompt Style(this SecretPrompt prompt, RgbColor color)
        {
            return Require(prompt).Style(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for rendered secret mask characters.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The mask character color.</param>
        /// <returns>The configured prompt.</returns>
        public static SecretPrompt MaskStyle(this SecretPrompt prompt, SpectreColor color)
        {
            return Require(prompt).MaskStyle(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for rendered secret mask characters.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The mask character color.</param>
        /// <returns>The configured prompt.</returns>
        public static SecretPrompt MaskStyle(this SecretPrompt prompt, RgbColor color)
        {
            return Require(prompt).MaskStyle(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for invalid secret input messages.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The invalid input message color.</param>
        /// <returns>The configured prompt.</returns>
        public static SecretPrompt InvalidStyle(this SecretPrompt prompt, SpectreColor color)
        {
            return Require(prompt).InvalidStyle(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for invalid secret input messages.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The invalid input message color.</param>
        /// <returns>The configured prompt.</returns>
        public static SecretPrompt InvalidStyle(this SecretPrompt prompt, RgbColor color)
        {
            return Require(prompt).InvalidStyle(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for the selection prompt message.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt PromptStyle(this SelectionPrompt prompt, SpectreColor color)
        {
            return Require(prompt).PromptStyle(color);
        }

        /// <summary>
        /// Sets the color used for the selection prompt message.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt PromptStyle(this SelectionPrompt prompt, RgbColor color)
        {
            return Require(prompt).PromptStyle(color);
        }

        /// <summary>
        /// Sets the color used for the selection prompt message.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt Style(this SelectionPrompt prompt, SpectreColor color)
        {
            return Require(prompt).Style(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for the selection prompt message.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt Style(this SelectionPrompt prompt, RgbColor color)
        {
            return Require(prompt).Style(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for selection prompt instruction text.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The instruction text color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt InstructionStyle(this SelectionPrompt prompt, SpectreColor color)
        {
            return Require(prompt).InstructionStyle(color);
        }

        /// <summary>
        /// Sets the color used for selection prompt instruction text.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The instruction text color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt InstructionStyle(this SelectionPrompt prompt, RgbColor color)
        {
            return Require(prompt).InstructionStyle(color);
        }

        /// <summary>
        /// Sets the keyboard highlight color used by the selection prompt.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The highlight color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt HighlightColor(this SelectionPrompt prompt, SpectreColor color)
        {
            return Require(prompt).HighlightColor(color);
        }

        /// <summary>
        /// Sets the keyboard highlight color used by the selection prompt.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The highlight color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt HighlightColor(this SelectionPrompt prompt, RgbColor color)
        {
            return Require(prompt).HighlightColor(color);
        }

        /// <summary>
        /// Sets the keyboard highlight color used by the selection prompt.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The highlight color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt HighlightStyle(this SelectionPrompt prompt, SpectreColor color)
        {
            return Require(prompt).HighlightStyle(ToStyle(color));
        }

        /// <summary>
        /// Sets the keyboard highlight color used by the selection prompt.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The highlight color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt HighlightStyle(this SelectionPrompt prompt, RgbColor color)
        {
            return Require(prompt).HighlightStyle(ToStyle(color));
        }

        /// <summary>
        /// Sets the style used for disabled selection options.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The disabled option foreground color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt DisabledStyle(this SelectionPrompt prompt, SpectreColor color)
        {
            return PromptBuilderExtensions.DisabledStyle(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Sets the style used for disabled selection options.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The disabled option foreground color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt DisabledStyle(this SelectionPrompt prompt, RgbColor color)
        {
            return PromptBuilderExtensions.DisabledStyle(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Sets the style used for highlighted search matches.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The search highlight foreground color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt SearchHighlightStyle(this SelectionPrompt prompt, SpectreColor color)
        {
            return PromptBuilderExtensions.SearchHighlightStyle(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Sets the style used for highlighted search matches.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The search highlight foreground color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt SearchHighlightStyle(this SelectionPrompt prompt, RgbColor color)
        {
            return PromptBuilderExtensions.SearchHighlightStyle(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Adds a selectable option with a shared palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="label">The option label.</param>
        /// <param name="color">The option foreground color.</param>
        /// <param name="isDefault">Whether the option is the default selection.</param>
        /// <param name="renderStyle">The option render style.</param>
        /// <returns>The index of the added option.</returns>
        public static int Add(this SelectionPrompt prompt, string label, SpectreColor color, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            prompt = Require(prompt);
            int index = prompt.Add(label, ToConsoleColor(color), isDefault, renderStyle);
            return ApplyOptionColor(prompt, index, color);
        }

        /// <summary>
        /// Adds a selectable option with an RGB palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="label">The option label.</param>
        /// <param name="color">The option foreground color.</param>
        /// <param name="isDefault">Whether the option is the default selection.</param>
        /// <param name="renderStyle">The option render style.</param>
        /// <returns>The index of the added option.</returns>
        public static int Add(this SelectionPrompt prompt, string label, RgbColor color, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            prompt = Require(prompt);
            int index = prompt.Add(label, ToConsoleColor(color), isDefault, renderStyle);
            return ApplyOptionColor(prompt, index, color);
        }

        /// <summary>
        /// Adds a described option with a shared palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="label">The option label.</param>
        /// <param name="description">The option description.</param>
        /// <param name="color">The option foreground color.</param>
        /// <param name="isDefault">Whether the option is the default selection.</param>
        /// <param name="renderStyle">The option render style.</param>
        /// <returns>The index of the added option.</returns>
        public static int AddDescribed(this SelectionPrompt prompt, string label, string description, SpectreColor color, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            prompt = Require(prompt);
            int index = prompt.AddDescribed(label, description, ToConsoleColor(color), isDefault, renderStyle);
            return ApplyOptionColor(prompt, index, color);
        }

        /// <summary>
        /// Adds a described option with an RGB palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="label">The option label.</param>
        /// <param name="description">The option description.</param>
        /// <param name="color">The option foreground color.</param>
        /// <param name="isDefault">Whether the option is the default selection.</param>
        /// <param name="renderStyle">The option render style.</param>
        /// <returns>The index of the added option.</returns>
        public static int AddDescribed(this SelectionPrompt prompt, string label, string description, RgbColor color, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            prompt = Require(prompt);
            int index = prompt.AddDescribed(label, description, ToConsoleColor(color), isDefault, renderStyle);
            return ApplyOptionColor(prompt, index, color);
        }

        /// <summary>
        /// Adds a selectable group option with a shared palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="label">The group label.</param>
        /// <param name="color">The group foreground color.</param>
        /// <param name="isDefault">Whether the group is the default selection.</param>
        /// <returns>The added group option.</returns>
        public static SelectOption AddGroup(this SelectionPrompt prompt, string label, SpectreColor color, bool isDefault = false)
        {
            return ApplyOptionColor(Require(prompt).AddGroup(label, ToConsoleColor(color), isDefault), color);
        }

        /// <summary>
        /// Adds a selectable group option with an RGB palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="label">The group label.</param>
        /// <param name="color">The group foreground color.</param>
        /// <param name="isDefault">Whether the group is the default selection.</param>
        /// <returns>The added group option.</returns>
        public static SelectOption AddGroup(this SelectionPrompt prompt, string label, RgbColor color, bool isDefault = false)
        {
            return ApplyOptionColor(Require(prompt).AddGroup(label, ToConsoleColor(color), isDefault), color);
        }

        /// <summary>
        /// Adds a child option with a shared palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="parent">The parent group option.</param>
        /// <param name="label">The child option label.</param>
        /// <param name="color">The child option foreground color.</param>
        /// <param name="isDefault">Whether the child option is the default selection.</param>
        /// <param name="renderStyle">The child option render style.</param>
        /// <returns>The added child option.</returns>
        public static SelectOption AddChild(this SelectionPrompt prompt, SelectOption parent, string label, SpectreColor color, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            return ApplyOptionColor(Require(prompt).AddChild(parent, label, ToConsoleColor(color), isDefault, renderStyle), color);
        }

        /// <summary>
        /// Adds a child option with an RGB palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="parent">The parent group option.</param>
        /// <param name="label">The child option label.</param>
        /// <param name="color">The child option foreground color.</param>
        /// <param name="isDefault">Whether the child option is the default selection.</param>
        /// <param name="renderStyle">The child option render style.</param>
        /// <returns>The added child option.</returns>
        public static SelectOption AddChild(this SelectionPrompt prompt, SelectOption parent, string label, RgbColor color, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            return ApplyOptionColor(Require(prompt).AddChild(parent, label, ToConsoleColor(color), isDefault, renderStyle), color);
        }

        /// <summary>
        /// Adds a separator with a shared palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="label">Optional separator label.</param>
        /// <param name="color">The separator foreground color.</param>
        /// <returns>The added separator option.</returns>
        public static SelectOption AddSeparator(this SelectionPrompt prompt, string label, SpectreColor color)
        {
            return ApplyOptionColor(Require(prompt).AddSeparator(label, ToConsoleColor(color)), color);
        }

        /// <summary>
        /// Adds a separator with an RGB palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="label">Optional separator label.</param>
        /// <param name="color">The separator foreground color.</param>
        /// <returns>The added separator option.</returns>
        public static SelectOption AddSeparator(this SelectionPrompt prompt, string label, RgbColor color)
        {
            return ApplyOptionColor(Require(prompt).AddSeparator(label, ToConsoleColor(color)), color);
        }

        /// <summary>
        /// Adds a disabled option with a shared palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="label">The option label.</param>
        /// <param name="reason">Optional disabled reason.</param>
        /// <param name="color">The disabled option foreground color.</param>
        /// <param name="renderStyle">The option render style.</param>
        /// <returns>The index of the added option.</returns>
        public static int AddDisabled(this SelectionPrompt prompt, string label, string reason, SpectreColor color, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            prompt = Require(prompt);
            int index = prompt.AddDisabled(label, reason, ToConsoleColor(color), renderStyle);
            return ApplyDisabledOptionColor(prompt, index, color);
        }

        /// <summary>
        /// Adds a disabled option with an RGB palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="label">The option label.</param>
        /// <param name="reason">Optional disabled reason.</param>
        /// <param name="color">The disabled option foreground color.</param>
        /// <param name="renderStyle">The option render style.</param>
        /// <returns>The index of the added option.</returns>
        public static int AddDisabled(this SelectionPrompt prompt, string label, string reason, RgbColor color, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            prompt = Require(prompt);
            int index = prompt.AddDisabled(label, reason, ToConsoleColor(color), renderStyle);
            return ApplyDisabledOptionColor(prompt, index, color);
        }

        /// <summary>
        /// Adds a described disabled option with a shared palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="label">The option label.</param>
        /// <param name="description">The option description.</param>
        /// <param name="reason">Optional disabled reason.</param>
        /// <param name="color">The disabled option foreground color.</param>
        /// <param name="renderStyle">The option render style.</param>
        /// <returns>The index of the added option.</returns>
        public static int AddDisabledDescribed(this SelectionPrompt prompt, string label, string description, string reason, SpectreColor color, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            prompt = Require(prompt);
            int index = prompt.AddDisabledDescribed(label, description, reason, ToConsoleColor(color), renderStyle);
            return ApplyDisabledOptionColor(prompt, index, color);
        }

        /// <summary>
        /// Adds a described disabled option with an RGB palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="label">The option label.</param>
        /// <param name="description">The option description.</param>
        /// <param name="reason">Optional disabled reason.</param>
        /// <param name="color">The disabled option foreground color.</param>
        /// <param name="renderStyle">The option render style.</param>
        /// <returns>The index of the added option.</returns>
        public static int AddDisabledDescribed(this SelectionPrompt prompt, string label, string description, string reason, RgbColor color, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            prompt = Require(prompt);
            int index = prompt.AddDisabledDescribed(label, description, reason, ToConsoleColor(color), renderStyle);
            return ApplyDisabledOptionColor(prompt, index, color);
        }

        /// <summary>
        /// Sets the color used for the typed selection prompt message.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> PromptStyle<T>(this SelectionPrompt<T> prompt, SpectreColor color)
        {
            return Require(prompt).PromptStyle(color);
        }

        /// <summary>
        /// Sets the color used for the typed selection prompt message.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> PromptStyle<T>(this SelectionPrompt<T> prompt, RgbColor color)
        {
            return Require(prompt).PromptStyle(color);
        }

        /// <summary>
        /// Sets the color used for the typed selection prompt message.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> Style<T>(this SelectionPrompt<T> prompt, SpectreColor color)
        {
            return Require(prompt).Style(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for the typed selection prompt message.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> Style<T>(this SelectionPrompt<T> prompt, RgbColor color)
        {
            return Require(prompt).Style(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for typed selection prompt instruction text.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The instruction text color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> InstructionStyle<T>(this SelectionPrompt<T> prompt, SpectreColor color)
        {
            return Require(prompt).InstructionStyle(color);
        }

        /// <summary>
        /// Sets the color used for typed selection prompt instruction text.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The instruction text color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> InstructionStyle<T>(this SelectionPrompt<T> prompt, RgbColor color)
        {
            return Require(prompt).InstructionStyle(color);
        }

        /// <summary>
        /// Sets the keyboard highlight color used by a typed selection prompt.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The highlight color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> HighlightColor<T>(this SelectionPrompt<T> prompt, SpectreColor color)
        {
            return Require(prompt).HighlightColor(color);
        }

        /// <summary>
        /// Sets the keyboard highlight color used by a typed selection prompt.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The highlight color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> HighlightColor<T>(this SelectionPrompt<T> prompt, RgbColor color)
        {
            return Require(prompt).HighlightColor(color);
        }

        /// <summary>
        /// Sets the keyboard highlight color used by a typed selection prompt.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The highlight color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> HighlightStyle<T>(this SelectionPrompt<T> prompt, SpectreColor color)
        {
            return Require(prompt).HighlightStyle(ToStyle(color));
        }

        /// <summary>
        /// Sets the keyboard highlight color used by a typed selection prompt.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The highlight color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> HighlightStyle<T>(this SelectionPrompt<T> prompt, RgbColor color)
        {
            return Require(prompt).HighlightStyle(ToStyle(color));
        }

        /// <summary>
        /// Sets the style used for disabled typed selection options.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The disabled option foreground color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> DisabledStyle<T>(this SelectionPrompt<T> prompt, SpectreColor color)
        {
            return PromptBuilderExtensions.DisabledStyle(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Sets the style used for disabled typed selection options.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The disabled option foreground color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> DisabledStyle<T>(this SelectionPrompt<T> prompt, RgbColor color)
        {
            return PromptBuilderExtensions.DisabledStyle(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Sets the style used for highlighted typed selection search matches.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The search highlight foreground color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> SearchHighlightStyle<T>(this SelectionPrompt<T> prompt, SpectreColor color)
        {
            return PromptBuilderExtensions.SearchHighlightStyle(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Sets the style used for highlighted typed selection search matches.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The search highlight foreground color.</param>
        /// <returns>The configured prompt.</returns>
        public static SelectionPrompt<T> SearchHighlightStyle<T>(this SelectionPrompt<T> prompt, RgbColor color)
        {
            return PromptBuilderExtensions.SearchHighlightStyle(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Adds a typed option with a shared palette color.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="item">The option item.</param>
        /// <param name="color">The option foreground color.</param>
        /// <param name="isDefault">Whether the option is the default selection.</param>
        /// <param name="renderStyle">The option render style.</param>
        /// <returns>The index of the added option.</returns>
        public static int Add<T>(this SelectionPrompt<T> prompt, T item, SpectreColor color, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            prompt = Require(prompt);
            int index = prompt.Add(item, ToConsoleColor(color), isDefault, renderStyle);
            return ApplyOptionColor(prompt, index, color);
        }

        /// <summary>
        /// Adds a typed option with an RGB palette color.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="item">The option item.</param>
        /// <param name="color">The option foreground color.</param>
        /// <param name="isDefault">Whether the option is the default selection.</param>
        /// <param name="renderStyle">The option render style.</param>
        /// <returns>The index of the added option.</returns>
        public static int Add<T>(this SelectionPrompt<T> prompt, T item, RgbColor color, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            prompt = Require(prompt);
            int index = prompt.Add(item, ToConsoleColor(color), isDefault, renderStyle);
            return ApplyOptionColor(prompt, index, color);
        }

        /// <summary>
        /// Adds a typed group option with a shared palette color.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="group">The group item.</param>
        /// <param name="color">The group foreground color.</param>
        /// <param name="isDefault">Whether the group is the default selection.</param>
        /// <returns>The added group option.</returns>
        public static SelectOption AddGroup<T>(this SelectionPrompt<T> prompt, T group, SpectreColor color, bool isDefault = false)
        {
            return ApplyOptionColor(Require(prompt).AddGroup(group, ToConsoleColor(color), isDefault), color);
        }

        /// <summary>
        /// Adds a typed group option with an RGB palette color.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="group">The group item.</param>
        /// <param name="color">The group foreground color.</param>
        /// <param name="isDefault">Whether the group is the default selection.</param>
        /// <returns>The added group option.</returns>
        public static SelectOption AddGroup<T>(this SelectionPrompt<T> prompt, T group, RgbColor color, bool isDefault = false)
        {
            return ApplyOptionColor(Require(prompt).AddGroup(group, ToConsoleColor(color), isDefault), color);
        }

        /// <summary>
        /// Adds a typed child option with a shared palette color.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="parent">The parent group option.</param>
        /// <param name="item">The child item.</param>
        /// <param name="color">The child option foreground color.</param>
        /// <param name="isDefault">Whether the child option is the default selection.</param>
        /// <param name="renderStyle">The child option render style.</param>
        /// <returns>The added child option.</returns>
        public static SelectOption AddChild<T>(this SelectionPrompt<T> prompt, SelectOption parent, T item, SpectreColor color, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            return ApplyOptionColor(Require(prompt).AddChild(parent, item, ToConsoleColor(color), isDefault, renderStyle), color);
        }

        /// <summary>
        /// Adds a typed child option with an RGB palette color.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="parent">The parent group option.</param>
        /// <param name="item">The child item.</param>
        /// <param name="color">The child option foreground color.</param>
        /// <param name="isDefault">Whether the child option is the default selection.</param>
        /// <param name="renderStyle">The child option render style.</param>
        /// <returns>The added child option.</returns>
        public static SelectOption AddChild<T>(this SelectionPrompt<T> prompt, SelectOption parent, T item, RgbColor color, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            return ApplyOptionColor(Require(prompt).AddChild(parent, item, ToConsoleColor(color), isDefault, renderStyle), color);
        }

        /// <summary>
        /// Adds a disabled typed option with a shared palette color.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="item">The option item.</param>
        /// <param name="reason">Optional disabled reason.</param>
        /// <param name="color">The disabled option foreground color.</param>
        /// <param name="renderStyle">The option render style.</param>
        /// <returns>The index of the added option.</returns>
        public static int AddDisabled<T>(this SelectionPrompt<T> prompt, T item, string reason, SpectreColor color, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            prompt = Require(prompt);
            int index = prompt.AddDisabled(item, reason, ToConsoleColor(color), renderStyle);
            return ApplyDisabledOptionColor(prompt, index, color);
        }

        /// <summary>
        /// Adds a disabled typed option with an RGB palette color.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="item">The option item.</param>
        /// <param name="reason">Optional disabled reason.</param>
        /// <param name="color">The disabled option foreground color.</param>
        /// <param name="renderStyle">The option render style.</param>
        /// <returns>The index of the added option.</returns>
        public static int AddDisabled<T>(this SelectionPrompt<T> prompt, T item, string reason, RgbColor color, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            prompt = Require(prompt);
            int index = prompt.AddDisabled(item, reason, ToConsoleColor(color), renderStyle);
            return ApplyDisabledOptionColor(prompt, index, color);
        }

        /// <summary>
        /// Sets the color used for the multi-selection prompt message.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt PromptStyle(this MultiSelectionPrompt prompt, SpectreColor color)
        {
            return Require(prompt).PromptStyle(color);
        }

        /// <summary>
        /// Sets the color used for the multi-selection prompt message.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt PromptStyle(this MultiSelectionPrompt prompt, RgbColor color)
        {
            return Require(prompt).PromptStyle(color);
        }

        /// <summary>
        /// Sets the color used for the multi-selection prompt message.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt Style(this MultiSelectionPrompt prompt, SpectreColor color)
        {
            return Require(prompt).Style(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for the multi-selection prompt message.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt Style(this MultiSelectionPrompt prompt, RgbColor color)
        {
            return Require(prompt).Style(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for multi-selection instruction text.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The instruction text color.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt InstructionStyle(this MultiSelectionPrompt prompt, SpectreColor color)
        {
            return Require(prompt).InstructionStyle(color);
        }

        /// <summary>
        /// Sets the color used for multi-selection instruction text.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The instruction text color.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt InstructionStyle(this MultiSelectionPrompt prompt, RgbColor color)
        {
            return Require(prompt).InstructionStyle(color);
        }

        /// <summary>
        /// Sets the keyboard highlight color used by the multi-selection prompt.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The highlight color.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt HighlightColor(this MultiSelectionPrompt prompt, SpectreColor color)
        {
            return Require(prompt).HighlightColor(color);
        }

        /// <summary>
        /// Sets the keyboard highlight color used by the multi-selection prompt.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The highlight color.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt HighlightColor(this MultiSelectionPrompt prompt, RgbColor color)
        {
            return Require(prompt).HighlightColor(color);
        }

        /// <summary>
        /// Sets the keyboard highlight color used by the multi-selection prompt.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The highlight color.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt HighlightStyle(this MultiSelectionPrompt prompt, SpectreColor color)
        {
            return Require(prompt).HighlightStyle(ToStyle(color));
        }

        /// <summary>
        /// Sets the keyboard highlight color used by the multi-selection prompt.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The highlight color.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt HighlightStyle(this MultiSelectionPrompt prompt, RgbColor color)
        {
            return Require(prompt).HighlightStyle(ToStyle(color));
        }

        /// <summary>
        /// Sets the style used for highlighted multi-selection search matches.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The search highlight foreground color.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt SearchHighlightStyle(this MultiSelectionPrompt prompt, SpectreColor color)
        {
            return PromptBuilderExtensions.SearchHighlightStyle(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Sets the style used for highlighted multi-selection search matches.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The search highlight foreground color.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt SearchHighlightStyle(this MultiSelectionPrompt prompt, RgbColor color)
        {
            return PromptBuilderExtensions.SearchHighlightStyle(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Adds a multi-selection option with a shared palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="label">The option label.</param>
        /// <param name="color">The option foreground color.</param>
        /// <param name="isDefault">Whether the option is selected by default.</param>
        /// <returns>The index of the added option.</returns>
        public static int Add(this MultiSelectionPrompt prompt, string label, SpectreColor color, bool isDefault = false)
        {
            prompt = Require(prompt);
            int index = prompt.Add(label, ToConsoleColor(color), isDefault);
            return ApplyOptionColor(prompt, index, color);
        }

        /// <summary>
        /// Adds a multi-selection option with an RGB palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="label">The option label.</param>
        /// <param name="color">The option foreground color.</param>
        /// <param name="isDefault">Whether the option is selected by default.</param>
        /// <returns>The index of the added option.</returns>
        public static int Add(this MultiSelectionPrompt prompt, string label, RgbColor color, bool isDefault = false)
        {
            prompt = Require(prompt);
            int index = prompt.Add(label, ToConsoleColor(color), isDefault);
            return ApplyOptionColor(prompt, index, color);
        }

        /// <summary>
        /// Adds a described multi-selection option with a shared palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="label">The option label.</param>
        /// <param name="description">The option description.</param>
        /// <param name="color">The option foreground color.</param>
        /// <param name="isDefault">Whether the option is selected by default.</param>
        /// <returns>The index of the added option.</returns>
        public static int AddDescribed(this MultiSelectionPrompt prompt, string label, string description, SpectreColor color, bool isDefault = false)
        {
            prompt = Require(prompt);
            int index = prompt.AddDescribed(label, description, ToConsoleColor(color), isDefault);
            return ApplyOptionColor(prompt, index, color);
        }

        /// <summary>
        /// Adds a described multi-selection option with an RGB palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="label">The option label.</param>
        /// <param name="description">The option description.</param>
        /// <param name="color">The option foreground color.</param>
        /// <param name="isDefault">Whether the option is selected by default.</param>
        /// <returns>The index of the added option.</returns>
        public static int AddDescribed(this MultiSelectionPrompt prompt, string label, string description, RgbColor color, bool isDefault = false)
        {
            prompt = Require(prompt);
            int index = prompt.AddDescribed(label, description, ToConsoleColor(color), isDefault);
            return ApplyOptionColor(prompt, index, color);
        }

        /// <summary>
        /// Adds a multi-selection group option with a shared palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="label">The group label.</param>
        /// <param name="color">The group foreground color.</param>
        /// <param name="isDefault">Whether the group is selected by default.</param>
        /// <returns>The added group option.</returns>
        public static SelectOption AddGroup(this MultiSelectionPrompt prompt, string label, SpectreColor color, bool isDefault = false)
        {
            return ApplyOptionColor(Require(prompt).AddGroup(label, ToConsoleColor(color), isDefault), color);
        }

        /// <summary>
        /// Adds a multi-selection group option with an RGB palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="label">The group label.</param>
        /// <param name="color">The group foreground color.</param>
        /// <param name="isDefault">Whether the group is selected by default.</param>
        /// <returns>The added group option.</returns>
        public static SelectOption AddGroup(this MultiSelectionPrompt prompt, string label, RgbColor color, bool isDefault = false)
        {
            return ApplyOptionColor(Require(prompt).AddGroup(label, ToConsoleColor(color), isDefault), color);
        }

        /// <summary>
        /// Adds a multi-selection child option with a shared palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="parent">The parent group option.</param>
        /// <param name="label">The child option label.</param>
        /// <param name="color">The child option foreground color.</param>
        /// <param name="isDefault">Whether the child option is selected by default.</param>
        /// <param name="renderStyle">The child option render style.</param>
        /// <returns>The added child option.</returns>
        public static SelectOption AddChild(this MultiSelectionPrompt prompt, SelectOption parent, string label, SpectreColor color, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Checkbox)
        {
            return ApplyOptionColor(Require(prompt).AddChild(parent, label, ToConsoleColor(color), isDefault, renderStyle), color);
        }

        /// <summary>
        /// Adds a multi-selection child option with an RGB palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="parent">The parent group option.</param>
        /// <param name="label">The child option label.</param>
        /// <param name="color">The child option foreground color.</param>
        /// <param name="isDefault">Whether the child option is selected by default.</param>
        /// <param name="renderStyle">The child option render style.</param>
        /// <returns>The added child option.</returns>
        public static SelectOption AddChild(this MultiSelectionPrompt prompt, SelectOption parent, string label, RgbColor color, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Checkbox)
        {
            return ApplyOptionColor(Require(prompt).AddChild(parent, label, ToConsoleColor(color), isDefault, renderStyle), color);
        }

        /// <summary>
        /// Adds a multi-selection separator with a shared palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="label">Optional separator label.</param>
        /// <param name="color">The separator foreground color.</param>
        /// <returns>The added separator option.</returns>
        public static SelectOption AddSeparator(this MultiSelectionPrompt prompt, string label, SpectreColor color)
        {
            return ApplyOptionColor(Require(prompt).AddSeparator(label, ToConsoleColor(color)), color);
        }

        /// <summary>
        /// Adds a multi-selection separator with an RGB palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="label">Optional separator label.</param>
        /// <param name="color">The separator foreground color.</param>
        /// <returns>The added separator option.</returns>
        public static SelectOption AddSeparator(this MultiSelectionPrompt prompt, string label, RgbColor color)
        {
            return ApplyOptionColor(Require(prompt).AddSeparator(label, ToConsoleColor(color)), color);
        }

        /// <summary>
        /// Adds a disabled multi-selection option with a shared palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="label">The option label.</param>
        /// <param name="reason">Optional disabled reason.</param>
        /// <param name="color">The disabled option foreground color.</param>
        /// <returns>The index of the added option.</returns>
        public static int AddDisabled(this MultiSelectionPrompt prompt, string label, string reason, SpectreColor color)
        {
            prompt = Require(prompt);
            int index = prompt.AddDisabled(label, reason, ToConsoleColor(color));
            return ApplyDisabledOptionColor(prompt, index, color);
        }

        /// <summary>
        /// Adds a disabled multi-selection option with an RGB palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="label">The option label.</param>
        /// <param name="reason">Optional disabled reason.</param>
        /// <param name="color">The disabled option foreground color.</param>
        /// <returns>The index of the added option.</returns>
        public static int AddDisabled(this MultiSelectionPrompt prompt, string label, string reason, RgbColor color)
        {
            prompt = Require(prompt);
            int index = prompt.AddDisabled(label, reason, ToConsoleColor(color));
            return ApplyDisabledOptionColor(prompt, index, color);
        }

        /// <summary>
        /// Adds a described disabled multi-selection option with a shared palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="label">The option label.</param>
        /// <param name="description">The option description.</param>
        /// <param name="reason">Optional disabled reason.</param>
        /// <param name="color">The disabled option foreground color.</param>
        /// <returns>The index of the added option.</returns>
        public static int AddDisabledDescribed(this MultiSelectionPrompt prompt, string label, string description, string reason, SpectreColor color)
        {
            prompt = Require(prompt);
            int index = prompt.AddDisabledDescribed(label, description, reason, ToConsoleColor(color));
            return ApplyDisabledOptionColor(prompt, index, color);
        }

        /// <summary>
        /// Adds a described disabled multi-selection option with an RGB palette color.
        /// </summary>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="label">The option label.</param>
        /// <param name="description">The option description.</param>
        /// <param name="reason">Optional disabled reason.</param>
        /// <param name="color">The disabled option foreground color.</param>
        /// <returns>The index of the added option.</returns>
        public static int AddDisabledDescribed(this MultiSelectionPrompt prompt, string label, string description, string reason, RgbColor color)
        {
            prompt = Require(prompt);
            int index = prompt.AddDisabledDescribed(label, description, reason, ToConsoleColor(color));
            return ApplyDisabledOptionColor(prompt, index, color);
        }

        /// <summary>
        /// Sets the color used for the typed multi-selection prompt message.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> PromptStyle<T>(this MultiSelectionPrompt<T> prompt, SpectreColor color)
        {
            return Require(prompt).PromptStyle(color);
        }

        /// <summary>
        /// Sets the color used for the typed multi-selection prompt message.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> PromptStyle<T>(this MultiSelectionPrompt<T> prompt, RgbColor color)
        {
            return Require(prompt).PromptStyle(color);
        }

        /// <summary>
        /// Sets the color used for the typed multi-selection prompt message.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> Style<T>(this MultiSelectionPrompt<T> prompt, SpectreColor color)
        {
            return Require(prompt).Style(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for the typed multi-selection prompt message.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The prompt message color.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> Style<T>(this MultiSelectionPrompt<T> prompt, RgbColor color)
        {
            return Require(prompt).Style(ToStyle(color));
        }

        /// <summary>
        /// Sets the color used for typed multi-selection instruction text.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The instruction text color.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> InstructionStyle<T>(this MultiSelectionPrompt<T> prompt, SpectreColor color)
        {
            return Require(prompt).InstructionStyle(color);
        }

        /// <summary>
        /// Sets the color used for typed multi-selection instruction text.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The instruction text color.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> InstructionStyle<T>(this MultiSelectionPrompt<T> prompt, RgbColor color)
        {
            return Require(prompt).InstructionStyle(color);
        }

        /// <summary>
        /// Sets the keyboard highlight color used by a typed multi-selection prompt.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The highlight color.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> HighlightColor<T>(this MultiSelectionPrompt<T> prompt, SpectreColor color)
        {
            return Require(prompt).HighlightColor(color);
        }

        /// <summary>
        /// Sets the keyboard highlight color used by a typed multi-selection prompt.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The highlight color.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> HighlightColor<T>(this MultiSelectionPrompt<T> prompt, RgbColor color)
        {
            return Require(prompt).HighlightColor(color);
        }

        /// <summary>
        /// Sets the keyboard highlight color used by a typed multi-selection prompt.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The highlight color.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> HighlightStyle<T>(this MultiSelectionPrompt<T> prompt, SpectreColor color)
        {
            return Require(prompt).HighlightStyle(ToStyle(color));
        }

        /// <summary>
        /// Sets the keyboard highlight color used by a typed multi-selection prompt.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The highlight color.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> HighlightStyle<T>(this MultiSelectionPrompt<T> prompt, RgbColor color)
        {
            return Require(prompt).HighlightStyle(ToStyle(color));
        }

        /// <summary>
        /// Sets the style used for highlighted typed multi-selection search matches.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The search highlight foreground color.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> SearchHighlightStyle<T>(this MultiSelectionPrompt<T> prompt, SpectreColor color)
        {
            return PromptBuilderExtensions.SearchHighlightStyle(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Sets the style used for highlighted typed multi-selection search matches.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="color">The search highlight foreground color.</param>
        /// <returns>The configured prompt.</returns>
        public static MultiSelectionPrompt<T> SearchHighlightStyle<T>(this MultiSelectionPrompt<T> prompt, RgbColor color)
        {
            return PromptBuilderExtensions.SearchHighlightStyle(Require(prompt), ToStyle(color));
        }

        /// <summary>
        /// Adds a typed multi-selection option with a shared palette color.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="item">The option item.</param>
        /// <param name="color">The option foreground color.</param>
        /// <param name="isDefault">Whether the option is selected by default.</param>
        /// <returns>The index of the added option.</returns>
        public static int Add<T>(this MultiSelectionPrompt<T> prompt, T item, SpectreColor color, bool isDefault = false)
        {
            prompt = Require(prompt);
            int index = prompt.Add(item, ToConsoleColor(color), isDefault);
            return ApplyOptionColor(prompt, index, color);
        }

        /// <summary>
        /// Adds a typed multi-selection option with an RGB palette color.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="item">The option item.</param>
        /// <param name="color">The option foreground color.</param>
        /// <param name="isDefault">Whether the option is selected by default.</param>
        /// <returns>The index of the added option.</returns>
        public static int Add<T>(this MultiSelectionPrompt<T> prompt, T item, RgbColor color, bool isDefault = false)
        {
            prompt = Require(prompt);
            int index = prompt.Add(item, ToConsoleColor(color), isDefault);
            return ApplyOptionColor(prompt, index, color);
        }

        /// <summary>
        /// Adds a typed multi-selection group option with a shared palette color.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="group">The group item.</param>
        /// <param name="color">The group foreground color.</param>
        /// <param name="isDefault">Whether the group is selected by default.</param>
        /// <returns>The added group option.</returns>
        public static SelectOption AddGroup<T>(this MultiSelectionPrompt<T> prompt, T group, SpectreColor color, bool isDefault = false)
        {
            return ApplyOptionColor(Require(prompt).AddGroup(group, ToConsoleColor(color), isDefault), color);
        }

        /// <summary>
        /// Adds a typed multi-selection group option with an RGB palette color.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="group">The group item.</param>
        /// <param name="color">The group foreground color.</param>
        /// <param name="isDefault">Whether the group is selected by default.</param>
        /// <returns>The added group option.</returns>
        public static SelectOption AddGroup<T>(this MultiSelectionPrompt<T> prompt, T group, RgbColor color, bool isDefault = false)
        {
            return ApplyOptionColor(Require(prompt).AddGroup(group, ToConsoleColor(color), isDefault), color);
        }

        /// <summary>
        /// Adds a typed multi-selection child option with a shared palette color.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="parent">The parent group option.</param>
        /// <param name="item">The child item.</param>
        /// <param name="color">The child option foreground color.</param>
        /// <param name="isDefault">Whether the child option is selected by default.</param>
        /// <param name="renderStyle">The child option render style.</param>
        /// <returns>The added child option.</returns>
        public static SelectOption AddChild<T>(this MultiSelectionPrompt<T> prompt, SelectOption parent, T item, SpectreColor color, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Checkbox)
        {
            return ApplyOptionColor(Require(prompt).AddChild(parent, item, ToConsoleColor(color), isDefault, renderStyle), color);
        }

        /// <summary>
        /// Adds a typed multi-selection child option with an RGB palette color.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="parent">The parent group option.</param>
        /// <param name="item">The child item.</param>
        /// <param name="color">The child option foreground color.</param>
        /// <param name="isDefault">Whether the child option is selected by default.</param>
        /// <param name="renderStyle">The child option render style.</param>
        /// <returns>The added child option.</returns>
        public static SelectOption AddChild<T>(this MultiSelectionPrompt<T> prompt, SelectOption parent, T item, RgbColor color, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Checkbox)
        {
            return ApplyOptionColor(Require(prompt).AddChild(parent, item, ToConsoleColor(color), isDefault, renderStyle), color);
        }

        /// <summary>
        /// Adds a disabled typed multi-selection option with a shared palette color.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="item">The option item.</param>
        /// <param name="reason">Optional disabled reason.</param>
        /// <param name="color">The disabled option foreground color.</param>
        /// <returns>The index of the added option.</returns>
        public static int AddDisabled<T>(this MultiSelectionPrompt<T> prompt, T item, string reason, SpectreColor color)
        {
            prompt = Require(prompt);
            int index = prompt.AddDisabled(item, reason, ToConsoleColor(color));
            return ApplyDisabledOptionColor(prompt, index, color);
        }

        /// <summary>
        /// Adds a disabled typed multi-selection option with an RGB palette color.
        /// </summary>
        /// <typeparam name="T">The prompt item type.</typeparam>
        /// <param name="prompt">The prompt to configure.</param>
        /// <param name="item">The option item.</param>
        /// <param name="reason">Optional disabled reason.</param>
        /// <param name="color">The disabled option foreground color.</param>
        /// <returns>The index of the added option.</returns>
        public static int AddDisabled<T>(this MultiSelectionPrompt<T> prompt, T item, string reason, RgbColor color)
        {
            prompt = Require(prompt);
            int index = prompt.AddDisabled(item, reason, ToConsoleColor(color));
            return ApplyDisabledOptionColor(prompt, index, color);
        }

        /// <summary>
        /// Sets the option foreground color.
        /// </summary>
        /// <param name="option">The option to configure.</param>
        /// <param name="color">The option foreground color.</param>
        /// <returns>The configured option.</returns>
        public static SelectOption UseColor(this SelectOption option, SpectreColor color)
        {
            return Require(option).UseColor(color);
        }

        /// <summary>
        /// Sets the option foreground color.
        /// </summary>
        /// <param name="option">The option to configure.</param>
        /// <param name="color">The option foreground color.</param>
        /// <returns>The configured option.</returns>
        public static SelectOption UseColor(this SelectOption option, RgbColor color)
        {
            return Require(option).UseColor(color);
        }

        /// <summary>
        /// Sets the option foreground style.
        /// </summary>
        /// <param name="option">The option to configure.</param>
        /// <param name="color">The option foreground color.</param>
        /// <returns>The configured option.</returns>
        public static SelectOption UseStyle(this SelectOption option, SpectreColor color)
        {
            return Require(option).UseStyle(ToStyle(color));
        }

        /// <summary>
        /// Sets the option foreground style.
        /// </summary>
        /// <param name="option">The option to configure.</param>
        /// <param name="color">The option foreground color.</param>
        /// <returns>The configured option.</returns>
        public static SelectOption UseStyle(this SelectOption option, RgbColor color)
        {
            return Require(option).UseStyle(ToStyle(color));
        }

        /// <summary>
        /// Sets the disabled option foreground style.
        /// </summary>
        /// <param name="option">The option to configure.</param>
        /// <param name="color">The disabled option foreground color.</param>
        /// <returns>The configured option.</returns>
        public static SelectOption UseDisabledStyle(this SelectOption option, SpectreColor color)
        {
            return Require(option).UseDisabledStyle(ToStyle(color));
        }

        /// <summary>
        /// Sets the disabled option foreground style.
        /// </summary>
        /// <param name="option">The option to configure.</param>
        /// <param name="color">The disabled option foreground color.</param>
        /// <returns>The configured option.</returns>
        public static SelectOption UseDisabledStyle(this SelectOption option, RgbColor color)
        {
            return Require(option).UseDisabledStyle(ToStyle(color));
        }

        /// <summary>
        /// Adds a child option with a shared palette color.
        /// </summary>
        /// <param name="option">The parent option.</param>
        /// <param name="label">The child option label.</param>
        /// <param name="color">The child option foreground color.</param>
        /// <param name="isDefault">Whether the child option is the default selection.</param>
        /// <param name="renderStyle">The child option render style.</param>
        /// <returns>The added child option.</returns>
        public static SelectOption AddChild(this SelectOption option, string label, SpectreColor color, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            return ApplyOptionColor(Require(option).AddChild(label, ToConsoleColor(color), isDefault, renderStyle), color);
        }

        /// <summary>
        /// Adds a child option with an RGB palette color.
        /// </summary>
        /// <param name="option">The parent option.</param>
        /// <param name="label">The child option label.</param>
        /// <param name="color">The child option foreground color.</param>
        /// <param name="isDefault">Whether the child option is the default selection.</param>
        /// <param name="renderStyle">The child option render style.</param>
        /// <returns>The added child option.</returns>
        public static SelectOption AddChild(this SelectOption option, string label, RgbColor color, bool isDefault = false, OptionRenderStyle renderStyle = OptionRenderStyle.Indexable)
        {
            return ApplyOptionColor(Require(option).AddChild(label, ToConsoleColor(color), isDefault, renderStyle), color);
        }

        private static int ApplyOptionColor(SelectionPrompt prompt, int index, SpectreColor color)
        {
            ApplyOptionColor(GetOption(prompt, index), color);
            return index;
        }

        private static int ApplyOptionColor(SelectionPrompt prompt, int index, RgbColor color)
        {
            ApplyOptionColor(GetOption(prompt, index), color);
            return index;
        }

        private static int ApplyOptionColor(MultiSelectionPrompt prompt, int index, SpectreColor color)
        {
            ApplyOptionColor(GetOption(prompt, index), color);
            return index;
        }

        private static int ApplyOptionColor(MultiSelectionPrompt prompt, int index, RgbColor color)
        {
            ApplyOptionColor(GetOption(prompt, index), color);
            return index;
        }

        private static SelectOption ApplyOptionColor(SelectOption option, SpectreColor color)
        {
            return Require(option).UseColor(color);
        }

        private static SelectOption ApplyOptionColor(SelectOption option, RgbColor color)
        {
            return Require(option).UseColor(color);
        }

        private static int ApplyDisabledOptionColor(SelectionPrompt prompt, int index, SpectreColor color)
        {
            ApplyDisabledOptionColor(GetOption(prompt, index), color);
            return index;
        }

        private static int ApplyDisabledOptionColor(SelectionPrompt prompt, int index, RgbColor color)
        {
            ApplyDisabledOptionColor(GetOption(prompt, index), color);
            return index;
        }

        private static int ApplyDisabledOptionColor(MultiSelectionPrompt prompt, int index, SpectreColor color)
        {
            ApplyDisabledOptionColor(GetOption(prompt, index), color);
            return index;
        }

        private static int ApplyDisabledOptionColor(MultiSelectionPrompt prompt, int index, RgbColor color)
        {
            ApplyDisabledOptionColor(GetOption(prompt, index), color);
            return index;
        }

        private static SelectOption ApplyDisabledOptionColor(SelectOption option, SpectreColor color)
        {
            return Require(option).UseDisabledColor(color);
        }

        private static SelectOption ApplyDisabledOptionColor(SelectOption option, RgbColor color)
        {
            return Require(option).UseDisabledColor(color);
        }

        private static SelectOption GetOption(SelectionPrompt prompt, int index)
        {
            return Require(prompt).Options.First(option => !option.IsSeparator && option.Index == index);
        }

        private static SelectOption GetOption(MultiSelectionPrompt prompt, int index)
        {
            return Require(prompt).Options.First(option => !option.IsSeparator && option.Index == index);
        }

        private static TPrompt Require<TPrompt>(TPrompt prompt) where TPrompt : class
        {
            if (prompt == null)
            {
                throw new ArgumentNullException(nameof(prompt));
            }

            return prompt;
        }

        private static ConsoleColor ToConsoleColor(SpectreColor color)
        {
            return color.ToConsoleColor();
        }

        private static ConsoleColor ToConsoleColor(RgbColor color)
        {
            return color.ToConsoleColor();
        }

        private static Style ToStyle(SpectreColor color)
        {
            return new Style(color);
        }

        private static Style ToStyle(RgbColor color)
        {
            return new Style(color);
        }
    }
}
